using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteControlAdapter.Model
{
    public class SocketClient:SocketBase
    {

        public event Action OnConnected;

        public event Action SendCompleted;

        public event Action<string> ReceiveCompleted;

        public event Action<string> OnError;
        public SocketClient(string ip,int port)
            :base(ip,port)
        {
            OnConnected+=()=>{};
            SendCompleted += () => { };
            ReceiveCompleted += (s) => { };

            OnError += (s) => { };
        }

        public bool ConnectAsync()
        {
            var endPoint = new DnsEndPoint(IpAddress,Port);
            var args = new SocketAsyncEventArgs()
            {
                RemoteEndPoint=endPoint,
                UserToken=_socket
            };
            args.Completed += (s,e) =>
            {
                if (e.SocketError == SocketError.Success)
                {
                    OnConnected();
                }
                else 
                {
                    OnError(e.SocketError.ToString());
                };
            };
            return _socket.ConnectAsync(args);
        }

        public bool SendTextAsync(string text)
        {
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = _socket.RemoteEndPoint;
            args.UserToken = null;
            byte[] data = Encoding.UTF8.GetBytes(text);
            args.SetBuffer(data,0,data.Length);
            args.Completed += (s, e) =>
            {
                if (e.SocketError == SocketError.Success)
                {
                    SendCompleted();
                }
                else
                {
                    OnError(e.SocketError.ToString());
                };
                
            };
            return _socket.SendAsync(args);
        }

        public void ReceiveTextAsync()
        {
            byte[] receiveData = new byte[1000];
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = _socket.RemoteEndPoint;
            args.UserToken = null;
            args.Completed += (s, e) =>
            {
                if (e.SocketError == SocketError.Success)
                {
                    string str = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    ReceiveCompleted(str);
                }
                else
                {
                    OnError(e.SocketError.ToString());
                };
                
            };
            args.SetBuffer(receiveData,0,receiveData.Length);
            _socket.ReceiveAsync(args);
        }
    }
}
