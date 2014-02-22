using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RemoteControlAdapter.Model
{
    [DataContract]
    public class RemoteData:INotifyPropertyChanged
    {

        string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; ModelPropertyChanged("Name"); }
        }

        int _controlData;
        [DataMember]
        public int ControlData
        {
            get { return _controlData; }
            set { _controlData = value; ModelPropertyChanged("ControlData"); }
        }

        public RemoteData()
        {

        }

        



        public event PropertyChangedEventHandler PropertyChanged;
        public void ModelPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,new PropertyChangedEventArgs(name));
        }
    }
}
