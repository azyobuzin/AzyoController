using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CoreTweet;
using RemoteControlAdapter.Model.Databases;
using RemoteControlAdapter.Model.Tweets;

namespace RemoteControlAdapter.Model
{
    public static class ChannelSuggesting
    {
        public static void Initialize()
        {
            timer = new Timer(120 * 1000);
            timer.Elapsed += (sender, e) =>
            {
                Suggest();
                if (new[] { 10, 11, 40, 41 }.Contains(e.SignalTime.Minute))
                    SuggestWithVoice();
            };
            timer.Start();
        }

        private static Timer timer;

        public static void Suggest()
        {
            if (!Settings.Instance.Users.Any()) return;

            Task.Run(() =>
            {
                try
                {
                    Status[] toRemove;
                    lock (TweetReceiver.FilteredTweets.SyncRoot)
                        toRemove = TweetReceiver.FilteredTweets
                            .Where(s => s.CreatedAt < DateTimeOffset.Now - TimeSpan.FromMinutes(10))
                            .ToArray();

                    foreach (var s in toRemove) TweetReceiver.FilteredTweets.Remove(s);

                    Status[] filteredTweets;
                    lock (TweetReceiver.FilteredTweets.SyncRoot)
                        filteredTweets = TweetReceiver.FilteredTweets.ToArray();
                    var analyzed = filteredTweets.Select(TweetAnalyzer.Analyze).ToArray();

                    foreach (var user in Settings.Instance.Users)
                    {
                        var rank = new Dictionary<Channel, List<WordCount>>();
                        foreach (var channel in Settings.Channels) rank.Add(channel, new List<WordCount>());
                        var wordList = ReceivedUserTweets.GetWordList(user.UserId);

                        foreach (var filtered in analyzed.Select(f => f.Select(x => x.Item1).ToArray()))
                        {
                            var points = filtered.Select(str => wordList.FirstOrDefault(w => w.Word == str))
                                .Where(w => w != null)
                                .ToArray();

                            foreach (var channel in Settings.Channels.Where(ch => filtered.Contains(ch.Hashtag)))
                                rank[channel].AddRange(points);
                        }

                        user.SuggestedChannels = rank
                            .Where(kvp => kvp.Value.Any())
                            .Select(kvp => Tuple.Create(
                                kvp.Key,
                                kvp.Value.GroupBy(w => w.Word)
                                    .Select(g => Tuple.Create(g.Key, g.Select(w => w.Count).Sum()))
                                    .ToArray()
                            ))
                            .OrderByDescending(x => x.Item2.Select(y => y.Item2).Sum())
                            .Select(x => new Suggestion() {
                                Channel = x.Item1,
                                Words = x.Item2.OrderByDescending(y => y.Item2)
                                    .Select(y => y.Item1)
                                    .Take(3)
                                    .ToArray()
                            })
                            .Take(2)
                            .ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Suggest error: " + ex.ToString());
                }
            });

        }

        public static void SuggestWithVoice()
        {
            if (RequestedVoiceSuggest != null)
                RequestedVoiceSuggest(null, EventArgs.Empty);
        }

        public static event EventHandler RequestedVoiceSuggest;
    }
}
