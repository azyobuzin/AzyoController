using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using RemoteControlAdapter.Model;
using Newtonsoft.Json;


namespace RemoteController
{
    public class MainViewModel : INotifyPropertyChanged
    {
        

        SocketClient _client;

        public SocketClient Client
        {
            get { return _client; }
            set { this._client = value; }
        }

        string _connectStatus;

        public string ConnectStatus
        {
            get { return this._connectStatus; }
            set { this._connectStatus = value; NotifyPropertyChanged("ConnectStatus"); }
        }

        public ObservableCollection<ControlType> CommandList { get; set; }
        public MainViewModel()
        {
            CommandList = new ObservableCollection<ControlType>()
            {
                ControlType.Power,
                ControlType.VolueUp,
                ControlType.VolumeDown,
                ControlType.Chanel1,
                ControlType.Chanel4,
                ControlType.Chanel5,
                ControlType.Chanel6,
                ControlType.Chanel8,
                ControlType.Chanel10,
            };

            ConnectStatus = "NotConnected";
        }

        public void Connect(string ip,int port)
        {

            _client = new SocketClient(ip, port);
            _client.ConnectAsync();
        }

        public void SendData(RemoteData data)
        {
            string str = JsonConvert.SerializeObject(data);
            _client.SendTextAsync(str);
        }

        public void BeginReceiveData()
        {
            _client.ReceiveTextAsync();
        }
        

        /// <summary>
        /// ItemViewModel オブジェクトのコレクションです。
        /// </summary>
       

        /// <summary>
        /// いくつかの ItemViewModel オブジェクトを作成し、Items コレクションに追加します。
        /// </summary>
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}