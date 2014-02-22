using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlAdapter.Model
{
    /// <summary>
    /// ソケット通信で非同期データリッスンを行う
    /// </summary>
    public class SocketListener : SocketBase
    {


        int _maxConnectionCount;

        public int MaxConnectionCount
        {
            get { return _maxConnectionCount; }
            set { _maxConnectionCount = value; ModelPropertyChanged("MaxConnectionCount"); }
        }

        public event Action<Socket> OnAccepted;

        public event Action OnDisConnection;

        private bool _connectionEndFlag;

        public SocketListener(string ip, int port)
            : base(ip, port)
        {

            this.MaxConnectionCount = 10;

            OnAccepted += (socket) => { };


            OnDisConnection += () => { };

            _connectionEndFlag = false;
        }

        public async Task ListenAsync()
        {
            await Task.Run(() =>
            {
                try{
                while (true)
                {
                    if (_connectionEndFlag == true)
                    {
                        _connectionEndFlag = false;
                        OnDisConnection();
                        break;
                    }
                    _socket.Listen(MaxConnectionCount);
                    var connection = _socket.Accept();
                    OnAccepted(connection);
                }
                }
                catch (Exception e)
                {
                    OnDisConnection();
                }
            });
        }

        public void DisConnect()
        {
            _connectionEndFlag = true;
        }
    }
}
