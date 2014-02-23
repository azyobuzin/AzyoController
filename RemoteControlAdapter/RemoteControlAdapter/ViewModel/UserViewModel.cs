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

            this.RemoveCommand = new ViewModelCommand(() => Settings.Instance.Users.Remove(this.Model));
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

        public ViewModelCommand RemoveCommand { get; private set; }
    }
}
