using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public enum AuxPowers
    {
        Light = 1,    //接通灯光电源
        Wiper,        //接通雨刷开关
        Fan,          //接通风扇开关
        Heater,       //接通加热器开关
        Aux1,         //接通辅助设备开关
        Aux2,         //接通辅助设备开关
    }
}
