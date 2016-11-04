using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCTVModels;
using PTZControlService;

namespace PTZControlClient
{
    public static class TestCamaraConfig
    {
        public readonly static CCTVControlConfig Serial_56Control = new CCTVControlConfig()
        {
            VideoId = Guid.NewGuid().ToString(),
            Ip = "192.168.9.56",
            Port = 4001,
            Type = CCTVControlType.TCP,
            CameraId = 1,
            SerialPort = 1,
            SerialType = SerialType.Bewator,
        };

        public readonly static CCTVControlConfig Dvr_155Control = new CCTVControlConfig()
        {
            VideoId = Guid.NewGuid().ToString(),
            Ip = "192.168.9.155",
            Port = 8000,
            UserName = "admin",
            Password = "admin12345",
            Type = CCTVControlType.DVR,
            Channel = 1,
        };

        public static CCTVStaticInfo CamaraStaticInfo = new CCTVStaticInfo()
        {
            VideoId = "test",
            Name = "test",
            Longitude = 116.35832,
            Latitude = 40.004411,
            Altitude = 40,
            ViewPort = 35.483333,
        };

        //double lon = 116.354954;//向西约286米
        //double lat = 40;  //向南约490米
        //double sog = 7;   //速度7节
        //double cog = 90;  //目标向东行驶
        //int length = 30;
        public readonly static DynamicTarget SimulationTargetBegin = new DynamicTarget("","0", "test", 116.354954, 40, 7, 90, 0, 30, 10, 0,0, DateTime.Now);
    }
}
