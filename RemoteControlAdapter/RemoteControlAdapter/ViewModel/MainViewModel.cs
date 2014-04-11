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
        /// �\�P�b�g�̃��X�i�[���J�n
        /// ��������s�����WP����̒ʐM�ҋ@��Ԃɓ���
        /// </summary>
        public ListenerCommand<string> BeginListenCommand { get; set; }
        /// <summary>
        /// �\�P�b�g�̃��X�i�[���I��
        /// </summary>
        public ViewModelCommand EndListenCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino��TV�`�����l���ύX�M���𑗂�
        /// </summary>
        public ListenerCommand<ControlType> ChangeChannelCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino�ɉ��ʃA�b�v�M���𑗂�
        /// </summary>
        public ViewModelCommand VolumeUpCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino�ɉ��ʃ_�E���M���𑗂�
        /// </summary>
        public ViewModelCommand VolumeDownCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino��TV�d���M���𑗂�
        /// </summary>
        public ViewModelCommand PowerCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̂��߂�COM�|�[�g���X�g���擾����
        /// </summary>
        public ViewModelCommand ResetPortListCommand { get; set; }
        /// <summary>
        /// Arduino�ƃV���A���ʐM���n�߂�
        /// </summary>
        public ListenerCommand<string> ConnectArduino { get; set; }
        /// <summary>
        /// Arduino�Ƃ̃V���A���ʐM���I������
        /// </summary>
        public ViewModelCommand DisConnectArduino { get; set; }
        /// <summary>
        /// �����R�����Ȃ��������̂��߂�WP�Ɍ������ĉ��y��炷�悤�ɐM���𑗂�(������)
        /// </summary>
        public ViewModelCommand CallMobileCommand { get; set; }
        /// <summary>
        /// WP�ɐ��E�ԑg���𑗐M����
        /// </summary>
        public ViewModelCommand SuggestMobileCommand { get; set; }
        /// <summary>
        /// �A�J�E���g�ǉ����J�n����
        /// </summary>
        public ViewModelCommand AddUserCommand { get; set; }
        /// <summary>
        /// �����T�W�F�X�g���e�X�g����
        /// </summary>
        public ViewModelCommand TestSuggestingCommand { get; set; }

        #endregion


        SocketListener _socket;
        /// <summary>
        /// �\�P�b�g�ʐM�p�\�P�b�g
        /// </summary>
        public SocketListener Socket
        {
            get { return _socket; }
            set { _socket = value; RaisePropertyChanged("Socket"); }
        }

        ObservableCollection<SocketClient> _clientList;
        /// <summary>
        /// ���ݐڑ�����Ă���\�P�b�g���X�g
        /// </summary>
        public ObservableCollection<SocketClient> ClientList
        {
            get { return _clientList; }
            set { _clientList = value; }
        }
        /// <summary>
        /// Arduino�Ƃ̒ʐM�p�V���A���|�[�g
        /// </summary>
        SerialPort _serialPort;

        ObservableCollection<string> _portList;
        /// <summary>
        /// �ڑ��\��COM�|�[�g�̃��X�g
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
        /// Command������������(�R���X�g���N�^����Ă΂��)
        /// </summary>
        private void CommandInitialize()
        {
            BeginListenCommand = new ListenerCommand<string>(async ip =>
            {
                //���g��IP�Ǝw��|�[�g�ԍ��Ń��X�i�[������
                Socket = new SocketListener(ip, 5000);

                //�R�l�N�V�����ڑ��v����������
                Socket.OnAccepted += (socket) =>
                {
                    //�V�����\�P�b�g���쐬
                    var client = new SocketClient(socket);
                    //��M�ҋ@(�񓯊�)
                    client.ReceiveTextAsync();
                    //�f�[�^��������
                    client.ReceiveCompleted += str =>
                    {
                        try
                        {
                            Debug.WriteLine("�f�[�^��M" + str);
                            //Remote�f�[�^�N���X�Ƀf�V���A����
                            var remoteData = JsonConvert.DeserializeObject<RemoteData>(str);

                            var user = Settings.Instance.Users.FirstOrDefault(u => u.ScreenName == remoteData.Name);
                            if (user != null && user.AvailableTimes.Any(t => Time.IsInTimeSpan(DateTime.Now, t.Start, t.End)))
                            {
                                //�ǂ̂悤�ȐM�������ʂ��ăR�}���h���s
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

                        //�ēx��M�J�n
                        client.ReceiveTextAsync();

                    };
                    //�ڑ����\�P�b�g���X�g�ɒǉ�
                    ClientList.Add(client);
                    Debug.WriteLine("�R�l�N�V�����m��");
                };
                Debug.WriteLine("�ڑ��ҋ@��..");
                //�\�P�b�g�̃��b�X���J�n
                await Socket.ListenAsync();
            });




            EndListenCommand = new ViewModelCommand(() =>
            {
                //�\�P�b�g�̐ؒf
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

                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)channel).ToString());
            });

            PowerCommand = new ViewModelCommand(() =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.Power).ToString());

                UsualSuggesting.EndWatching();
            });

            ResetPortListCommand = new ViewModelCommand(() =>
            {
                //�ڑ��\COM�|�[�g�̃��X�g��������
                PortList.Clear();
                //�Ď擾
                foreach (var n in SerialPort.GetPortNames())
                {
                    PortList.Add(n);
                }

            });

            ConnectArduino = new ListenerCommand<string>(async port =>
            {
                //Arduino�̃V���A���|�[�g���I�[�v��
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
                    this.Messenger.Raise(new GenericInteractionMessage<string>("�V���A���|�[�g�ڑ����J���܂���ł����B", "ErrorMessage"));
                }
            });

            DisConnectArduino = new ViewModelCommand(() =>
            {
                //Arduino�̃V���A���|�[�g���N���[�Y
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
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.VolueUp).ToString());
            });

            VolumeDownCommand = new ViewModelCommand(() =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.VolumeDown).ToString());
            });

            CallMobileCommand = new ViewModelCommand(() =>
            {
                //���݃\�P�b�g�ʐM�ŃR�[�����𑗐M���Ă��邪
                //WP�������ɂ��Ȃ��Ǝ�M�ł��Ȃ��̂Ńv�b�V���ɂ������Ȃ�
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
                    this.Messenger.Raise(new GenericInteractionMessage<string>("�����R������Ɏ��s���܂����B", "ErrorMessage"));
                }
            }
            else
            {
                Debug.WriteLine("�V���A���|�[�g���J���Ă܂���");
                await this.Messenger.RaiseAsync(new GenericInteractionMessage<string>("IR ���M�@�̐ݒ肪�������Ă��܂���B", "ErrorMessage"));
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
                    : "�f�[�^���Ȃ����߁A�������߂ł��܂���B";
            }
        }
    }
}