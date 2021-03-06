using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using RemoteControlAdapter.Model;

namespace RemoteControlAdapter.ViewModel
{

    public class MainViewModel : Livet.ViewModel
    {

        #region Command
        /// <summary>
        /// ソケットのリスナーを開始
        /// これを実行するとWPからの通信待機状態に入る
        /// </summary>
        public ListenerCommand<string> BeginListenCommand { get; set; }
        /// <summary>
        /// ソケットのリスナーを終了
        /// </summary>
        public ViewModelCommand EndListenCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoにTVチャンネル変更信号を送る
        /// </summary>
        public ListenerCommand<ControlType> ChangeChannelCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoに音量アップ信号を送る
        /// </summary>
        public ViewModelCommand VolumeUpCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoに音量ダウン信号を送る
        /// </summary>
        public ViewModelCommand VolumeDownCommand { get; set; }
        /// <summary>
        /// シリアル通信のコネクションされたArduinoにTV電源信号を送る
        /// </summary>
        public ViewModelCommand PowerCommand { get; set; }
        /// <summary>
        /// シリアル通信のためのCOMポートリストを取得する
        /// </summary>
        public ViewModelCommand ResetPortListCommand { get; set; }
        /// <summary>
        /// Arduinoとシリアル通信を始める
        /// </summary>
        public ListenerCommand<string> ConnectArduino { get; set; }
        /// <summary>
        /// Arduinoとのシリアル通信を終了する
        /// </summary>
        public ViewModelCommand DisConnectArduino { get; set; }
        /// <summary>
        /// リモコンをなくした時のためにWPに向かって音楽を鳴らすように信号を送る(未実装)
        /// </summary>
        public ViewModelCommand CallMobileCommand { get; set; }
        /// <summary>
        /// WPに推薦番組情報を送信する
        /// </summary>
        public ViewModelCommand SuggestMobileCommand { get; set; }
        /// <summary>
        /// アカウント追加を開始する
        /// </summary>
        public ViewModelCommand AddUserCommand { get; set; }
        /// <summary>
        /// 音声サジェストをテストする
        /// </summary>
        public ViewModelCommand TestSuggestingCommand { get; set; }

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

        public event Action<bool> OnSuggest;

        public MainViewModel()
        {

            CommandInitialize();



            ClientList = new ObservableCollection<SocketClient>();

            PortList = new ObservableCollection<string>();
            OnSuggest += force => { };

            ChannelSuggesting.RequestedVoiceSuggest += (sender, e) => OnSuggest(false);

            this.CompositeDisposable.Add(new PropertyChangedEventListener(Settings.Instance, (sender, e) =>
            {
                if (e.PropertyName == "Users")
                    this.Users = ViewModelHelper.CreateReadOnlyDispatcherCollection(Settings.Instance.Users,
                        u => new UserViewModel(u), DispatcherHelper.UIDispatcher);
            }));

            this.Users = ViewModelHelper.CreateReadOnlyDispatcherCollection(Settings.Instance.Users, u => new UserViewModel(u), DispatcherHelper.UIDispatcher);

            UsualSuggesting.SuggestedChannelChanged += (sender, e) => this.RaisePropertyChanged(() => this.UsualChannel);
        }

        /// <summary>
        /// Commandを初期化する(コンストラクタから呼ばれる)
        /// </summary>
        private void CommandInitialize()
        {
            BeginListenCommand = new ListenerCommand<string>(async ip =>
            {
                Settings.Instance.IpAddress = ip;
                Settings.Instance.Save();

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
                    client.ReceiveCompleted += str =>
                    {
                        try
                        {
                            Debug.WriteLine("データ受信" + str);
                            //Remoteデータクラスにデシリアル化
                            var remoteData = JsonConvert.DeserializeObject<RemoteData>(str);

                            var user = Settings.Instance.Users.FirstOrDefault(u => u.ScreenName == remoteData.Name);
                            if (user != null && user.AvailableTimes.Any(t => Time.IsInTimeSpan(DateTime.Now, t.Start, t.End)))
                            {
                                //どのような信号か判別してコマンド実行
                                switch ((ControlType)remoteData.ControlData)
                                {
                                    case ControlType.Power:
                                        PowerCommand.Execute();
                                        break;
                                    case ControlType.VolueUp:
                                        VolumeUpCommand.Execute();
                                        break;
                                    case ControlType.VolumeDown:
                                        VolumeDownCommand.Execute();
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
                            }
                            else
                            {
                                MobileRemoteData data = new MobileRemoteData()
                                {
                                    ControlType = MobileControlType.Reject
                                };
                                client.SendTextAsync(JsonConvert.SerializeObject(data));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed reading data from socket: " + ex.ToString());
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




            EndListenCommand = new ViewModelCommand(() =>
            {
                //ソケットの切断
                Socket.DisConnect();
            });

            ChangeChannelCommand = new ListenerCommand<ControlType>(channel =>
            {
                UsualSuggesting.SetWatchingChannel(
                    channel == ControlType.Chanel1
                        ? 1
                        : channel == ControlType.Chanel4
                            ? 4
                            : channel == ControlType.Chanel5
                                ? 5
                                : channel == ControlType.Chanel6
                                    ? 6
                                    : channel == ControlType.Chanel8
                                        ? 8
                                        : 10
                );

                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)channel).ToString());
            });

            PowerCommand = new ViewModelCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.Power).ToString());

                UsualSuggesting.EndWatching();
            });

            ResetPortListCommand = new ViewModelCommand(() =>
            {
                //接続可能COMポートのリストを初期化
                PortList.Clear();
                //再取得
                foreach (var n in SerialPort.GetPortNames())
                {
                    PortList.Add(n);
                }

            });

            ConnectArduino = new ListenerCommand<string>(async port =>
            {
                //Arduinoのシリアルポートをオープン
                try
                {
                    await Task.Run(() =>
                    {
                        if (_serialPort != null)
                        {
                            try
                            {
                                _serialPort.Close();
                                _serialPort = null;
                            }
                            catch { }
                        }
                        _serialPort = new SerialPort(port, 9600);
                        _serialPort.Open();
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Couldn't open serial port: " + ex.ToString());
                    this.Messenger.Raise(new GenericInteractionMessage<string>("シリアルポート接続を開けませんでした。", "ErrorMessage"));
                }
            });

            DisConnectArduino = new ViewModelCommand(() =>
            {
                //Arduinoのシリアルポートをクローズ
                Task.Run(() =>
                {
                    try
                    {
                        _serialPort.Close();
                    }
                    catch { }
                });
            });

            VolumeUpCommand = new ViewModelCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.VolueUp).ToString());
            });

            VolumeDownCommand = new ViewModelCommand(() =>
            {
                //Arduinoのシリアルポートに書き込み
                WriteSerialPort(((int)ControlType.VolumeDown).ToString());
            });

            CallMobileCommand = new ViewModelCommand(() =>
            {
                //現在ソケット通信でコール情報を送信しているが
                //WP側が取りにこないと受信できないのでプッシュにしたいなぁ
                MobileRemoteData data = new MobileRemoteData()
                {
                    ControlType = MobileControlType.Call
                };
                string str = JsonConvert.SerializeObject(data);
                foreach (var client in ClientList)
                {
                    client.SendTextAsync(str);
                }
            });
            
            AddUserCommand = new ViewModelCommand(async () =>
            {
                var authorizer = new Authorizer();
                var progress = await this.Messenger
                    .GetResponse(new ResponsiveInteractionMessage<Task<ProgressDialogController>>("WaitingForGettingTokens"))
                    .Response;
                Uri uri;
                try
                {
                    uri = await authorizer.GetRequestTokenAsync();
                }
                catch
                {
                    progress.CloseAsync()
                        .ContinueWith(t => this.Messenger.Raise(new InteractionMessage("AuthorizationError")));
                    return;
                }
                await progress.CloseAsync();

                var pin = await this.Messenger
                    .GetResponse(new GenericResponsiveInteractionMessage<Uri, Task<string>>(uri, "InputPin"))
                    .Response;
                if (!string.IsNullOrWhiteSpace(pin))
                {
                    progress = await this.Messenger
                        .GetResponse(new ResponsiveInteractionMessage<Task<ProgressDialogController>>("WaitingForGettingTokens"))
                        .Response;
                    try
                    {
                        await authorizer.GetAccessTokenAsync(pin.Trim());
                    }
                    catch
                    {
                        progress.CloseAsync()
                            .ContinueWith(t => this.Messenger.Raise(new InteractionMessage("AuthorizationError")));
                        return;
                    }
                    progress.CloseAsync();
                }
            });

            TestSuggestingCommand = new ViewModelCommand(() =>
            {
                Task.Run(() => OnSuggest(true));
            });

        }

        private async void WriteSerialPort(string str)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    await Task.Run(() => _serialPort.WriteLine(str));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed writing to serial port: " + ex.ToString());
                    this.Messenger.Raise(new GenericInteractionMessage<string>("リモコン操作に失敗しました。", "ErrorMessage"));
                }
            }
            else
            {
                Debug.WriteLine("シリアルポートが開いてません");
                await this.Messenger.RaiseAsync(new GenericInteractionMessage<string>("IR 送信機の設定が完了していません。", "ErrorMessage"));
            }
        }

        private ReadOnlyDispatcherCollection<UserViewModel> users;
        public ReadOnlyDispatcherCollection<UserViewModel> Users
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
                }
            }
        }

        public string UsualChannel
        {
            get
            {
                return UsualSuggesting.SuggestedChannel != null
                    ? string.Format("{0} {1}", UsualSuggesting.SuggestedChannel.Number, UsualSuggesting.SuggestedChannel.Name)
                    : "データがないため、おすすめできません。";
            }
        }
    }
}