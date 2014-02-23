using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;

namespace RemoteControlAdapter.Model
{
    public static class TweetReceiver
    {
        public static readonly string[] SearchHashtags = new[] { "#ntv", "#tbs", "#tvasahi", "#fujitv" }; //TODO
        public static ObservableCollection<Status> ReceivedTweets { get; private set; }

        static TweetReceiver()
        {
            ReceivedTweets = new ObservableCollection<Status>();
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
        }

        private static ObservableCollection<User> users;

        public static bool IsRunning { get; private set; }

        private static void users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            StartFilterStream();
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
                        //TODO
                    },
                    ex =>
                    {
                        Debug.WriteLine("Filter srteam: " + ex.ToString());
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
    }
}
