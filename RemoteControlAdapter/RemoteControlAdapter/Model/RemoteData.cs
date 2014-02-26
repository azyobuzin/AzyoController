using Livet;

namespace RemoteControlAdapter.Model
{
    /// <summary>
    /// WPからWPF側に来るデータ
    /// </summary>
    public class RemoteData : NotificationObject
    {

        string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        int _controlData;
        public int ControlData
        {
            get { return _controlData; }
            set { _controlData = value; RaisePropertyChanged(); }
        }

        public RemoteData()
        {

        }
    }
}
