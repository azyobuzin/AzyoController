using System.Linq;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using RemoteControlAdapter.Model;

namespace RemoteControlAdapter.ViewModel
{
    public class UserViewModel : Livet.ViewModel
    {
        public UserViewModel(User model)
        {
            this.Model = model;

            this.CompositeDisposable.Add(new PropertyChangedEventListener(this.Model, (sender, e) =>
            {
                if (new[] { "ScreenName", "ProfileImage", "IsVoiceSuggest" }.Contains(e.PropertyName))
                    this.RaisePropertyChanged(e.PropertyName);
                switch (e.PropertyName)
                {
                    case "ScreenName":
                    case "ProfileImage":
                    case "IsVoiceSuggest":
                        this.RaisePropertyChanged(e.PropertyName);
                        break;
                    case "SuggestedChannels":
                        this.RaisePropertyChanged(() => this.SuggestedChannels);
                        break;
                }
            }));

            this.AvailableTimes = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                this.Model.AvailableTimes, _ => _, DispatcherHelper.UIDispatcher);

            this.RemoveCommand = new ViewModelCommand(() => Settings.Instance.Users.Remove(this.Model));
            this.AddTimeCommand = new ViewModelCommand(() => this.Model.AvailableTimes.Add(new UserAvailableTime()));
        }

        public User Model { get; private set; }

        public string ScreenName
        {
            get
            {
                return this.Model.ScreenName;
            }
        }

        public string ProfileImage
        {
            get
            {
                return this.Model.ProfileImage;
            }
        }

        public ReadOnlyDispatcherCollection<UserAvailableTime> AvailableTimes { get; private set; }

        public string SuggestedChannels
        {
            get
            {
                return this.Model.SuggestedChannels != null && this.Model.SuggestedChannels.Any()
                    ? string.Join("\n", this.Model.SuggestedChannels
                        .Select(s => string.Format("{0} {1}: {2}", s.Channel.Number, s.Channel.Name, string.Join(", ", s.Words))))
                    : "おすすめを特定できませんでした。おすすめは時間帯やTwitterの利用状況に左右されます。";
            }
        }

        public bool IsVoiceSuggest
        {
            get
            {
                return this.Model.IsVoiceSuggest;
            }
            set
            {
                this.Model.IsVoiceSuggest = value;
            }
        }

        public ViewModelCommand RemoveCommand { get; private set; }

        public ViewModelCommand AddTimeCommand { get; private set; }
    }
}
