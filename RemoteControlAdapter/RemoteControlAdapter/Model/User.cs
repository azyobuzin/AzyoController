using Livet;
using Newtonsoft.Json;

namespace RemoteControlAdapter.Model
{
    public class User : NotificationObject
    {
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

        private Time startTime;
        public Time StartTime
        {
            get
            {
                return this.startTime;
            }
            set
            {
                if (this.startTime != value)
                {
                    this.startTime = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private Time endTime;
        public Time EndTime
        {
            get
            {
                return this.endTime;
            }
            set
            {
                if (this.endTime != value)
                {
                    this.endTime = value;
                    this.RaisePropertyChanged();
                }
            }
        }

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
}
