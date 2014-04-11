using System.Collections.ObjectModel;
using Livet;
using Newtonsoft.Json;

namespace RemoteControlAdapter.Model
{
    public class User : NotificationObject
    {
        public User()
        {
            this.AvailableTimes = new ObservableCollection<UserAvailableTime>(new[]
            {
                new UserAvailableTime()
                {
                    Start = new Time(0, 0, 0),
                    End = new Time(0, 0, 0)
                }
            });
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

        private bool isVoiceSuggest = true;
        public bool IsVoiceSuggest
        {
            get
            {
                return this.isVoiceSuggest;
            }
            set
            {
                if (this.isVoiceSuggest != value)
                {
                    this.isVoiceSuggest = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private Suggestion[] suggestedChannels;
        [JsonIgnore]
        public Suggestion[] SuggestedChannels
        {
            get
            {
                return this.suggestedChannels;
            }
            set
            {
                if (this.suggestedChannels != value)
                {
                    this.suggestedChannels = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }

    public class UserAvailableTime : NotificationObject
    {
        private Time start = new Time();
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

        private Time end = new Time();
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
