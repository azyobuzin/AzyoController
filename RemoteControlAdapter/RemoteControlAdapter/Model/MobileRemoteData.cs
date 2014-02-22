using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlAdapter.Model
{
    /// <summary>
    /// WPFからWPに送信するデータ
    /// </summary>
    public class MobileRemoteData
    {
        public MobileControlType ControlType { get; set; }

        public MobileRemoteData()
        {

        }
    }
}
