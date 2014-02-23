using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Timers;
using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using System.Threading.Tasks;
using RemoteControlAdapter.Model.Databases;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class TweetReceiver
    {
        public static readonly string[] SearchHashtags = new[] { "#nhk", "#mbs", "#kbs", "#abc", "#ktv", "#ytv" };

        private static Timer userTimelineTimer;

        public static ObservableCollection<Status> FilteredTweets { get; private set; }

        static TweetReceiver()
        {
            FilteredTweets = new ObservableCollection<Status>();
        }
        
        public static void Initialize()
        {
            Settings.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Users")
                {
                    if (users != null)
                        users.CollectionChanged -= users_CollectionChanged;
                    users = Settings.Instance.Users;
                    users.CollectionChanged += users_CollectionChanged;
                    StartFilterStream();
                }
            };
            users = Settings.Instance.Users;
            users.CollectionChanged += users_CollectionChanged;
            StartFilterStream();
            userTimelineTimer = new Timer(2 * 60 * 1000);
            userTimelineTimer.Elapsed += (sender, e) => GetUserTimelines();
            userTimelineTimer.Start();
            GetUserTimelines();
        }

        private static ObservableCollection<User> users;

        public static bool IsRunning { get; private set; }

        private static void users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            StartFilterStream();
            GetUserTimelines();
        }

        private static void StartFilterStream()
        {
            if (IsRunning || users.Count == 0) return;
            var user = users.OrderBy(_ => Guid.NewGuid()).First();
            Debug.WriteLine("Connecting filter stream with @" + user.ScreenName);
            IsRunning = true;
            Tokens.Create(Settings.ConsumerKey, Settings.ConsumerSecret, user.OAuthToken, user.OAuthTokenSecret).Streaming
                .StartObservableStream(StreamingType.Filter, new StreamingParameters(track => string.Join(",", SearchHashtags)))
                .OfType<StatusMessage>()
                .Subscribe(
                    m =>
                    {
                        var status = m.Status;
                        Debug.WriteLine("@{0}: {1}", status.User.ScreenName, status.Text);
                        FilteredTweets.Add(status);
                        //TODO: マッチング
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
                    );
                    using (var exec = await Database.Connect())
                    {
                        foreach (var status in timeline)
                        {
                            if (await Task.Run(() => !exec.ExecuteReader("select * from MyTweets where Id = @ID", status).Any()))
                            {
                                await Task.Run(() => exec.Insert("MyTweets", Tweet.FromStatus(status)));
                                foreach (var t in await TweetAnalyzer.Analyze(status))
                                {
                                    await Task.Run(() =>
                                    {
                                        var word = exec.Select<WordCount>(
                                            "select * from Words where UserId = @Item1 and Word = @Item2",
                                            Tuple.Create(user.UserId, t.Item2)
                                        ).FirstOrDefault();
                                        if (word == null)
                                            exec.Insert("Words", new WordCount()
                                            {
                                                UserId = user.UserId,
                                                Word = t.Item1,
                                                Count = t.Item2
                                            });
                                        else
                                            exec.Update("Words",
                                                new { Count = word.Count + t.Item2 },
                                                new { UserId = word.UserId, Word = word.Word }
                                            );
                                    });
                                }
                            }
                        }
                        await Task.Run(() => exec.TransactionComplete());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(user.ScreenName + "'s timeline error: " + ex.ToString());
                }
            }
        }
    }
}
