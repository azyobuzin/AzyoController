using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using RemoteControlAdapter.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace RemoteControlAdapter.ViewModel
{

    public class MainViewModel : ViewModelBase
    {

        #region Command
        public RelayCommand BeginListenCommand { get; set; }

        public RelayCommand EndListenCommand { get; set; }

        public RelayCommand<int> ChangeChannelCommand { get; set; }

        public RelayCommand VolumeUpCommand { get; set; }

        public RelayCommand VolumeDownCommand { get; set; }

        public RelayCommand PowerCommand { get; set; }

        public RelayCommand ResetPortListCommand { get; set; }

        public RelayCommand<string> ConnectArduino { get; set; }

        public RelayCommand DisConnectArduino { get; set; }

        public RelayCommand CallMobileCommand { get; set; }

        public RelayCommand SuggestMobileCommand { get; set; }

        #endregion


        SocketListener _socket;

        public SocketListener Socket
        {
            get { return _socket; }
            set { _socket = value; RaisePropertyChanged("Socket"); }
        }

        ObservableCollection<SocketClient> _clientList;

        public ObservableCollection<SocketClient> ClientList
        {
            get { return _clientList; }
            set { _clientList = value; }
        }

        SerialPort _serialPort;

        ObservableCollection<string> _portList;

        public ObservableCollection<string> PortList
        {
            get { return _portList; }
            set { _portList = value; }
        }

        public MainViewModel()
        {
            
            CommandInitialize();
            Socket = new SocketListener("192.168.100.104",5000);
            Socket.OnAccepted += (socket) =>
            {
                
                var client = new SocketClient(socket);
                client.ReceiveTextAsync();
                client.ReceiveCompleted += async (str) =>
                {
                    Debug.WriteLine("データ受信" + str);

                    var remoteData = await JsonConvert.DeserializeObjectAsync<RemoteData>(str);
                    switch ((ControlType)remoteData.ControlData)
                    {
                        case ControlType.Power:
                            PowerCommand.Execute(null);
                            break;
                        case ControlType.VolueUp:
                            VolumeUpCommand.Execute(null);
                            break;
                        case ControlType.VolumeDown:
                            VolumeDownCommand.Execute(null);
                            break;

                        case ControlType.Chanel1:
                            ChangeChannelCommand.Execute(ControlType.Chanel1);
                            break;
                        case ControlType.Chanel4:
                            ChangeChannelCommand.Execute(ControlType.Chanel4);
                            break;
                        case ControlType.Chanel5:
                            ChangeChannelCommand.Execute(ControlType.Chanel5);
                            break;
                        case ControlType.Chanel6:
                            ChangeChannelCommand.Execute(ControlType.Chanel6);
                            break;
                        case ControlType.Chanel8:
                            ChangeChannelCommand.Execute(ControlType.Chanel8);
                            break;
                        case ControlType.Chanel10:
                            ChangeChannelCommand.Execute(ControlType.Chanel10);
                            break;
                    }
                    client.ReceiveTextAsync();

                };
                ClientList.Add(client);
                Debug.WriteLine("コネクション確立");
            };
            

            ClientList = new ObservableCollection<SocketClient>();

            PortList = new ObservableCollection<string>();
            
            
        }

        private void CommandInitialize()
        {
            BeginListenCommand = new RelayCommand(async() =>
            {
                Debug.WriteLine("接続待機中..");
                await Socket.ListenAsync();
            });

            EndListenCommand = new RelayCommand(() =>
            {
                Socket.DisConnect();
            });

            ChangeChannelCommand = new RelayCommand<int>(channel =>
            {
                _serialPort.WriteLine(channel.ToString());
            });

            PowerCommand = new RelayCommand(() =>
            {
                _serialPort.WriteLine(((int)ControlType.Power).ToString());
            });

            ResetPortListCommand = new RelayCommand(() =>
            {
                PortList.Clear();
                foreach (var n in SerialPort.GetPortNames())
                {
                    PortList.Add(n);
                }
                
            });

            ConnectArduino = new RelayCommand<string>((port) =>
            {
                _serialPort = new SerialPort(port,9600);
                _serialPort.Open();
            });

            DisConnectArduino = new RelayCommand(() =>
            {
                _serialPort.Close();
            });

            VolumeUpCommand = new RelayCommand(() =>
            {
                _serialPort.WriteLine(((int)ControlType.VolueUp).ToString());
            });

            VolumeDownCommand = new RelayCommand(() =>
            {
                _serialPort.WriteLine(((int)ControlType.VolumeDown).ToString());
            });

            CallMobileCommand = new RelayCommand(async() =>
            {
                MobileRemoteData data = new MobileRemoteData()
                {
                    ControlType=MobileControlType.Call
                };
                string str = await JsonConvert.SerializeObjectAsync(data);
                foreach (var client in ClientList)
                {
                    client.SendTextAsync(str);
                }
            });

            SuggestMobileCommand = new RelayCommand(()=>
            {

            });
            
        }
    }
}