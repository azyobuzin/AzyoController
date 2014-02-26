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
                if (e.PropertyName == "ScreenName")
                    this.RaisePropertyChanged(() => this.ScreenName);
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

        public ViewModelCommand RemoveCommand { get; private set; }

        public ViewModelCommand AddTimeCommand { get; private set; }
    }
}
