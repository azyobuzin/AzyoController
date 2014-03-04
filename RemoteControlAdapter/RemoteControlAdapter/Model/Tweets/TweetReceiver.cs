using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using Newtonsoft.Json.Linq;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class TweetReceiver
    {
        private static Timer userTimelineTimer;

        public static ConcurrentBag<Status> FilteredTweets { get; private set; }

        static TweetReceiver()
        {
            FilteredTweets = new ConcurrentBag<Status>();
        }
        
        public static void Initialize()
        {
            Settings.Instance.Users.CollectionChanged += users_CollectionChanged;
            StartFilterStream();
            
            userTimelineTimer = new Timer(2 * 60 * 1000);
            userTimelineTimer.Elapsed += (sender, e) => GetUserTimelines();
            userTimelineTimer.Start();
            GetUserTimelines();

            Search();
        }
        
        public static bool IsRunning { get; private set; }

        private static void users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            StartFilterStream();
            GetUserTimelines();
        }

        private static void StartFilterStream()
        {
            if (IsRunning || Settings.Instance.Users.Count == 0) return;
            var user = Settings.Instance.Users.OrderBy(_ => Guid.NewGuid()).First();
            Debug.WriteLine("Connecting filter stream with @" + user.ScreenName);
            IsRunning = true;
            Tokens.Create(Settings.ConsumerKey, Settings.ConsumerSecret, user.OAuthToken, user.OAuthTokenSecret).Streaming
                .StartObservableStream(StreamingType.Filter,
                    new StreamingParameters(track => string.Join(",", Settings.Channels.Select(c => c.Hashtag))))
                .OfType<StatusMessage>()
                .Subscribe(
                    m =>
                    {
                        var status = m.Status;
                        Debug.WriteLine("@{0}: {1}", status.User.ScreenName, status.Text);
                        FilteredTweets.Add(status);
                    },
                    ex =>
                    {
                        Debug.WriteLine("Filter srteam error: " + ex.ToString());
                        IsRunning = false;
                        StartFilterStream();
                    },
                    () =>
                    {
                        Debug.WriteLine("Filter stream completed");
                        IsRunning = false;
                        StartFilterStream();
                    }
                );
        }

        private static async void GetUserTimelines()
        {
            foreach (var user in Settings.Instance.Users)
            {
                Debug.WriteLine("Getting {0}'s timeline", new[] { user.ScreenName });
                try
                {
                    var timeline = await Task.Run(() =>
                        Tokens.Create(Settings.ConsumerKey, Settings.ConsumerSecret, user.OAuthToken, user.OAuthTokenSecret)
                            .Statuses.UserTimeline(id => user.UserId, count => 200)
                            .Where(s => !ReceivedUserTweets.ContainsTweet(s.ID))
                            .ToArray()
                    );
                    foreach (var status in timeline)
                    {
                        await ReceivedUserTweets.AddTweet(status);
                        foreach (var t in await TweetAnalyzer.Analyze(status))
                            await ReceivedUserTweets.IncrementWordCount(user.UserId, t.Item1, t.Item2);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(user.ScreenName + "'s timeline error: " + ex.ToString());
                }
            }
        }

        private static async void Search()
        {
            Debug.WriteLine("Searching");
            try
            {
                using (var client = OAuth2.CreateOAuth2Client(await OAuth2.GetBearerToken()))
                {
                    var json = await client.GetStringAsync("https://api.twitter.com/1.1/search/tweets.json?result_type=recent&count=100&lang=ja&q="
                        + Uri.EscapeDataString(string.Join(" OR ", Settings.Channels.Select(c => c.Hashtag))));
                    await Task.Run(() =>
                    {
                        var statuses = JObject.Parse(json)["statuses"]
                            .Select(j => j.ToObject<Status>())
                            .Where(s => FilteredTweets.All(x => s.ID != x.ID));
                        foreach (var status in statuses)
                        {
                            FilteredTweets.Add(status);
                            Debug.WriteLine("@{0}: {1}", status.User.ScreenName, status.Text);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Serach error: " + ex.ToString());
            }
        }
    }
}
