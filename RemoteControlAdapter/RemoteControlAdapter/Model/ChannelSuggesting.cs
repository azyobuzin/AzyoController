using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CoreTweet;
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

                    Tuple<string, int>[][] analyzed;
                    lock (TweetReceiver.FilteredTweets.SyncRoot)
                        analyzed = TweetReceiver.FilteredTweets.Select(TweetAnalyzer.Analyze).ToArray();

                    foreach (var user in Settings.Instance.Users)
                    {
                        var rank = new Dictionary<Channel, List<long>>();
                        foreach (var channel in Settings.Channels) rank.Add(channel, new List<long>());
                        var wordList = ReceivedUserTweets.GetWordList(user.UserId);

                        foreach (var filtered in analyzed.Select(f => f.Select(x => x.Item1).ToArray()))
                        {
                            var point = filtered.Aggregate(0L, (i, str) =>
                            {
                                var word = wordList.FirstOrDefault(w => w.Word == str);
                                return word != null ? i + word.Count : i;
                            });

                            foreach (var channel in Settings.Channels.Where(ch => filtered.Contains(ch.Hashtag)))
                                rank[channel].Add(point);
                        }

                        if (rank.Values.All(v => v.Count == 0))
                            user.SuggestedChannel = null;
                        else
                        {
                            var top = rank
                                .Where(kvp => kvp.Value.Count != 0)
                                .Select(kvp => Tuple.Create(kvp.Key, (double)kvp.Value.Sum() / kvp.Value.Count))
                                .OrderByDescending(t => t.Item2)
                                .First();
                            user.SuggestedChannel = top.Item1;
                            Debug.WriteLine("Suggest {0} for @{1}, rank {2}", top.Item1.Name, user.ScreenName, top.Item2);
                        }
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
