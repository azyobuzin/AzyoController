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
        public const string DatabaseName = "Data Source=database.db";

        public static readonly ReadOnlyCollection<Channel> Channels = new ReadOnlyCollection<Channel>(new[] {
            new Channel(1, "NHK総合", "#nhk"),
            new Channel(4, "MBS毎日放送", "#mbs"),
            new Channel(5, "KBS京都", "#kbs"),
            new Channel(6, "ABC朝日放送", "#abc"),
            new Channel(8, "KTV関西テレビ放送", "#ktv"),
            new Channel(10, "ytv読売テレビ", "#ytv")
        });

        public ObservableCollection<User> Users { get; private set; }

        public string IpAddress { get; set; }

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
