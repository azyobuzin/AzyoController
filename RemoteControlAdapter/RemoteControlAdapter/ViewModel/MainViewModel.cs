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
        /// <summary>
        /// ソケットのリスナーを開始
        /// これを実行するとWPからの通信待機状態に入る
        /// </summary>
        public RelayCommand<string> BeginListenCommand { get; set; }
        /// <summary>
        /// ソケットのリスナーを終了
        /// </summary>
        public RelayCommand EndListenCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoにTVチャンネル変更信号を送る
        /// </summary>
        public RelayCommand<ControlType> ChangeChannelCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoに音量アップ信号を送る
        /// </summary>
        public RelayCommand VolumeUpCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoに音量ダウン信号を送る
        /// </summary>
        public RelayCommand VolumeDownCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoにTV電源信号を送る
        /// </summary>
        public RelayCommand PowerCommand { get; set; }
        /// <summary>
        /// シリアル通信のためのCOMポートリストを取得する
        /// </summary>
        public RelayCommand ResetPortListCommand { get; set; }
        /// <summary>
        /// Arduinoとシリアル通信を始める
        /// </summary>
        public RelayCommand<string> ConnectArduino { get; set; }
        /// <summary>
        /// Arduinoとのシリアル通信を終了する
        /// </summary>
        public RelayCommand DisConnectArduino { get; set; }
        /// <summary>
        /// リモコンをなくした時のためにWPに向かって音楽を鳴らすように信号を送る(未実装)
        /// </summary>
        public RelayCommand CallMobileCommand { get; set; }
        /// <summary>
        /// WPに推薦番組情報を送信する
        /// </summary>
        public RelayCommand SuggestMobileCommand { get; set; }

        #endregion


        SocketListener _socket;
        /// <summary>
        /// ソケット通信用ソケット
        /// </summary>
        public SocketListener Socket
        {
            get { return _socket; }
            set { _socket = value; RaisePropertyChanged("Socket"); }
        }

        ObservableCollection<SocketClient> _clientList;
        /// <summary>
        /// 現在接続されているソケットリスト
        /// </summary>
        public ObservableCollection<SocketClient> ClientList
        {
            get { return _clientList; }
            set { _clientList = value; }
        }
        /// <summary>
        /// Arduinoとの通信用シリアルポート
        /// </summary>
        SerialPort _serialPort;

        ObservableCollection<string> _portList;
        /// <summary>
        /// 接続可能なCOMポートのリスト
        /// </summary>
        public ObservableCollection<string> PortList
        {
            get { return _portList; }
            set { _portList = value; }
        }

        public MainViewModel()
        {
            
            CommandInitialize();
            
            

            ClientList = new ObservableCollection<SocketClient>();

            PortList = new ObservableCollection<string>();
            
            
        }

        /// <summary>
        /// Commandを初期化する(コンストラクタから呼ばれる)
        /// </summary>
        private void CommandInitialize()
        {
            BeginListenCommand = new RelayCommand<string>(async(ip) =>
            {
                //自身のIPと指定ポート番号でリスナー初期化
                Socket = new SocketListener(ip, 5000);

                //コネクション接続要求がきたら
                Socket.OnAccepted += (socket) =>
                {
                    //新しくソケットを作成
                    var client = new SocketClient(socket);
                    //受信待機(非同期)
                    client.ReceiveTextAsync();
                    //データがきたら
                    client.ReceiveCompleted += async (str) =>
                    {
                        Debug.WriteLine("データ受信" + str);
                        //Remoteデータクラスにデシリアル化
                        var remoteData = await JsonConvert.DeserializeObjectAsync<RemoteData>(str);

                        //どのような信号か判別してコマンド実行
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
                        //再度受信開始
                        client.ReceiveTextAsync();

                    };
                    //接続中ソケットリストに追加
                    ClientList.Add(client);
                    Debug.WriteLine("コネクション確立");
                };
                Debug.WriteLine("接続待機中..");
                //ソケットのリッスン開始
                await Socket.ListenAsync();
            });

            

            EndListenCommand = new RelayCommand(() =>
            {
                //ソケットの切断
                Socket.DisConnect();
            });

            ChangeChannelCommand = new RelayCommand<ControlType>(channel =>
            {
                //Arduinoのシリアルポートに書き込み
                _serialPort.WriteLine(channel.ToString());
            });

            PowerCommand = new RelayCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.Power).ToString());
            });

            ResetPortListCommand = new RelayCommand(() =>
            {
                //接続可能COMポートのリストを初期化
                PortList.Clear();
                //再取得
                foreach (var n in SerialPort.GetPortNames())
                {
                    PortList.Add(n);
                }
                
            });

            ConnectArduino = new RelayCommand<string>((port) =>
            {
                //Arduinoのシリアルポートをオープン
                _serialPort = new SerialPort(port,9600);
                _serialPort.Open();
            });

            DisConnectArduino = new RelayCommand(() =>
            {
                //Arduinoのシリアルポートをクローズ
                _serialPort.Close();
            });

            VolumeUpCommand = new RelayCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.VolueUp).ToString());
            });

            VolumeDownCommand = new RelayCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.VolumeDown).ToString());
            });

            CallMobileCommand = new RelayCommand(async() =>
            {
                //現在ソケット通信でコール情報を送信しているが
                //WP側が取りにこないと受信できないのでプッシュにしたいなぁ
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
                //未実装！！！
            });
            
        }

        private void WriteSerialPort(string str)
        {
            if (_serialPort!=null&&_serialPort.IsOpen)
            {
                _serialPort.WriteLine(str);
            }
            else
            {
                Debug.WriteLine("シリアルポートが開いてません");
            }
        }
    }
}