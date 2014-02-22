using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Livet;
using Newtonsoft.Json;

namespace RemoteControlAdapter.Model
{
    public class Settings : NotificationObject
    {
        private static readonly string SettingsFileName =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

        private Settings()
        {
            this.Users = new ObservableCollection<User>();
        }

        public const string ConsumerKey = "9zIZeOor17G0GiWS8ULMzg";
        public const string ConsumerSecret = "tHEMF1pDiqQWvrof7NtcRIi0yMT3WwpFRpNowtKRs";

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get
            {
                return this.users;
            }
            set
            {
                if (this.users != value)
                {
                    this.users = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFileName));
                    }
                    catch
                    {
                        instance = new Settings();
                    }
                }
                return instance;
            }
        }

        public void Save()
        {
            File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(this));
        }
    }
}
