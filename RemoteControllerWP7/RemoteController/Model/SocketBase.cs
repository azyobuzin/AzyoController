using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteControlAdapter.Model
{
    public class SocketBase:INotifyPropertyChanged
    {
        string _IpAddress;

        public string IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; ModelPropertyChanged("IpAddress"); }
        }

        int _port;

        public int Port
        {
            get { return _port; }
            set { _port = value; ModelPropertyChanged("Port"); }
        }

        protected Socket _socket;
        public SocketBase(string ip, int port)
        {
            this.IpAddress = ip;
            this.Port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        protected void ModelPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
