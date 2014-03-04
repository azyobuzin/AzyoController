using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using RemoteControlAdapter.Model.Tweets;
using CoreTweet;

namespace RemoteControlAdapter.Model
{
    public static class ChannelSuggesting
    {
        public static void Initialize()
        {
            timer = new Timer(60 * 1000);
            timer.Elapsed += (sender, e) =>
            {
                Suggest();
                if (new[] { 10, 40 }.Contains(e.SignalTime.Minute))
                    SuggestWithVoice();
            };
            timer.Start();
        }

        private static Timer timer;

        public static void Suggest()
        {
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
                        var rank = new Dictionary<Channel, long>();
                        foreach (var channel in Settings.Channels) rank.Add(channel, 0);
                        var wordList = ReceivedUserTweets.GetWordList(user.UserId);

                        foreach (var filtered in analyzed.Select(f => f.Select(x => x.Item1).ToArray()))
                        {
                            var point = filtered.Aggregate(0L, (i, str) =>
                            {
                                var word = wordList.FirstOrDefault(w => w.Word == str);
                                return word != null ? i + word.Count : i;
                            });

                            foreach (var channel in Settings.Channels.Where(ch => filtered.Contains(ch.Hashtag)))
                                rank[channel] += point;
                        }

                        if (rank.Values.All(v => v == 0))
                            user.SuggestedChannel = null;
                        else
                            user.SuggestedChannel = rank.OrderByDescending(kvp => kvp.Value).First().Key;
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
