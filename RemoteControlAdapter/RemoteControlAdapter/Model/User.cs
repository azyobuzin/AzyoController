using System.Collections.ObjectModel;
using Livet;
using Newtonsoft.Json;

namespace RemoteControlAdapter.Model
{
    public class User : NotificationObject
    {
        public User()
        {
            this.AvailableTimes = new ObservableCollection<UserAvailableTime>();
        }

        private string oauthToken;
        public string OAuthToken
        {
            get
            {
                return this.oauthToken;
            }
            set
            {
                if (this.oauthToken != value)
                {
                    this.oauthToken = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string oauthTokenSecret;
        public string OAuthTokenSecret
        {
            get
            {
                return this.oauthTokenSecret;
            }
            set
            {
                if (this.oauthTokenSecret != value)
                {
                    this.oauthTokenSecret = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private long userId;
        public long UserId
        {
            get
            {
                return this.userId;
            }
            set
            {
                if (this.userId != value)
                {
                    this.userId = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string screenName;
        public string ScreenName
        {
            get
            {
                return this.screenName;
            }
            set
            {
                if (this.screenName != value)
                {
                    this.screenName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string profileImage;
        public string ProfileImage
        {
            get
            {
                return this.profileImage;
            }
            set
            {
                if (this.profileImage != value)
                {
                    this.profileImage = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<UserAvailableTime> AvailableTimes { get; private set; }

        private SocketClient client;
        [JsonIgnore]
        public SocketClient Client
        {
            get
            {
                return this.client;
            }
            set
            {
                if (this.client != value)
                {
                    this.client = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }

    public class UserAvailableTime : NotificationObject
    {
        private Time start = Time.Zero;
        public Time Start
        {
            get
            {
                return this.start;
            }
            set
            {
                if (this.start != value)
                {
                    this.start = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private Time end = Time.Zero;
        public Time End
        {
            get
            {
                return this.end;
            }
            set
            {
                if (this.end != value)
                {
                    this.end = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}
