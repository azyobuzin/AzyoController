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
        /// �\�P�b�g�̃��X�i�[���J�n
        /// ��������s�����WP����̒ʐM�ҋ@��Ԃɓ���
        /// </summary>
        public RelayCommand<string> BeginListenCommand { get; set; }
        /// <summary>
        /// �\�P�b�g�̃��X�i�[���I��
        /// </summary>
        public RelayCommand EndListenCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino��TV�`�����l���ύX�M���𑗂�
        /// </summary>
        public RelayCommand<ControlType> ChangeChannelCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino�ɉ��ʃA�b�v�M���𑗂�
        /// </summary>
        public RelayCommand VolumeUpCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino�ɉ��ʃ_�E���M���𑗂�
        /// </summary>
        public RelayCommand VolumeDownCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̃R�l�N�V�������ꂽArduino��TV�d���M���𑗂�
        /// </summary>
        public RelayCommand PowerCommand { get; set; }
        /// <summary>
        /// �V���A���ʐM�̂��߂�COM�|�[�g���X�g���擾����
        /// </summary>
        public RelayCommand ResetPortListCommand { get; set; }
        /// <summary>
        /// Arduino�ƃV���A���ʐM���n�߂�
        /// </summary>
        public RelayCommand<string> ConnectArduino { get; set; }
        /// <summary>
        /// Arduino�Ƃ̃V���A���ʐM���I������
        /// </summary>
        public RelayCommand DisConnectArduino { get; set; }
        /// <summary>
        /// �����R�����Ȃ��������̂��߂�WP�Ɍ������ĉ��y��炷�悤�ɐM���𑗂�(������)
        /// </summary>
        public RelayCommand CallMobileCommand { get; set; }
        /// <summary>
        /// WP�ɐ��E�ԑg���𑗐M����
        /// </summary>
        public RelayCommand SuggestMobileCommand { get; set; }

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

        public MainViewModel()
        {
            
            CommandInitialize();
            
            

            ClientList = new ObservableCollection<SocketClient>();

            PortList = new ObservableCollection<string>();
            
            
        }

        /// <summary>
        /// Command������������(�R���X�g���N�^����Ă΂��)
        /// </summary>
        private void CommandInitialize()
        {
            BeginListenCommand = new RelayCommand<string>(async(ip) =>
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
                    client.ReceiveCompleted += async (str) =>
                    {
                        Debug.WriteLine("�f�[�^��M" + str);
                        //Remote�f�[�^�N���X�Ƀf�V���A����
                        var remoteData = await JsonConvert.DeserializeObjectAsync<RemoteData>(str);

                        //�ǂ̂悤�ȐM�������ʂ��ăR�}���h���s
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

            

            EndListenCommand = new RelayCommand(() =>
            {
                //�\�P�b�g�̐ؒf
                Socket.DisConnect();
            });

            ChangeChannelCommand = new RelayCommand<ControlType>(channel =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                _serialPort.WriteLine(channel.ToString());
            });

            PowerCommand = new RelayCommand(() =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.Power).ToString());
            });

            ResetPortListCommand = new RelayCommand(() =>
            {
                //�ڑ��\COM�|�[�g�̃��X�g��������
                PortList.Clear();
                //�Ď擾
                foreach (var n in SerialPort.GetPortNames())
                {
                    PortList.Add(n);
                }
                
            });

            ConnectArduino = new RelayCommand<string>((port) =>
            {
                //Arduino�̃V���A���|�[�g���I�[�v��
                _serialPort = new SerialPort(port,9600);
                _serialPort.Open();
            });

            DisConnectArduino = new RelayCommand(() =>
            {
                //Arduino�̃V���A���|�[�g���N���[�Y
                _serialPort.Close();
            });

            VolumeUpCommand = new RelayCommand(() =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.VolueUp).ToString());
            });

            VolumeDownCommand = new RelayCommand(() =>
            {
                //Arduino�̃V���A���|�[�g�ɏ�������
                WriteSerialPort(((int)ControlType.VolumeDown).ToString());
            });

            CallMobileCommand = new RelayCommand(async() =>
            {
                //���݃\�P�b�g�ʐM�ŃR�[�����𑗐M���Ă��邪
                //WP�������ɂ��Ȃ��Ǝ�M�ł��Ȃ��̂Ńv�b�V���ɂ������Ȃ�
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
                //�������I�I�I
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
                Debug.WriteLine("�V���A���|�[�g���J���Ă܂���");
            }
        }
    }
}