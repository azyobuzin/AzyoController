using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlAdapter.Model
{
    
    
    /// <summary>
    /// TVの制御機能列挙体
    /// 数字はArduinoのシリアルポートに書き込むときの信号
    /// </summary>
    public enum ControlType
    {
        Power=0,VolueUp=1,VolumeDown=2,Chanel1=4,Chanel4=5,Chanel5=6,Chanel6=7,Chanel8=8,Chanel10=9
    }

    
}
