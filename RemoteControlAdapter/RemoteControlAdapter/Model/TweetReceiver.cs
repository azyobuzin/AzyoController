using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LinqToTwitter;

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
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var user = users.OrderBy(_ => Guid.NewGuid()).First();
                    IsRunning = true;
                    await new TwitterContext(new SingleUserAuthorizer()
                    {
                        CredentialStore = new InMemoryCredentialStore()
                        {
                            ConsumerKey = Settings.ConsumerKey,
                            ConsumerSecret = Settings.ConsumerSecret,
                            OAuthToken = user.OAuthToken,
                            OAuthTokenSecret = user.OAuthTokenSecret
                        }
                    }).Streaming
                    .Where(s => s.Type == StreamingType.Filter && s.Track == string.Join(",", SearchHashtags))
                    .StartAsync(s => Task.Run(() =>
                    {
                        var j = LitJson.JsonMapper.ToObject(s.Content);
                        LitJson.JsonData _;
                        if (j.TryGetValue("text", out _))
                        {
                            var status = new Status(j);
                            ReceivedTweets.Add(status);
                            System.Diagnostics.Debug.WriteLine("@{0}: {1}", status.User.ScreenNameResponse, status.Text);
                        }
                    }));
                }
                catch { }
                IsRunning = false;
                StartFilterStream();
            }, TaskCreationOptions.LongRunning);
        }
    }
}
