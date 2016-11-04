using CCTVInfoHub;
using CCTVInfoHub.Entity;
using CCTVModels;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVPTZControlService
{
    public class CCTVStaticInfoManager
    {
        public static CCTVStaticInfoManager Instance { get; private set; } = new CCTVStaticInfoManager();

        Dictionary<string, CCTVStaticInfo> _dictStaticInfo;
        Dictionary<string, CCTVControlConfig> _dictControl;
        CCTVStaticInfoManager()
        {
            //Common.Log.Logger.Default.Trace(CCTVConfig.WebApiBaseUri);
            _dictStaticInfo = new Dictionary<string, CCTVStaticInfo>();
            _dictControl = new Dictionary<string, CCTVControlConfig>();

            _dictStaticInfo.Add("CCTV1_50BAD15900010301", new CCTVStaticInfo()
            {
                VideoId = "CCTV1_50BAD15900010301",
                Name = "155球机控制",
                Longitude = 116.35832,
                Latitude = 40.006411,
                Altitude = 2,
                ViewPort = 58,
            });
            _dictControl.Add("CCTV1_50BAD15900010301", new CCTVControlConfig()
            {
                VideoId = "CCTV1_50BAD15900010301",
                Type = CCTVControlType.DVR,
                Ip = "192.168.9.155",
                Port = 8000,
                UserName = "admin",
                Password = "admin12345",
                Channel = 1,
            });

            //_dictStaticInfo.Add("video2",  new CCTVStaticInfo()
            //{
            //    Name = "天津现场",
            //    VideoId = "video2",
            //});
            //_dictControl.Add("video2", new CCTVControlConfig()
            //{
            //    VideoId = "video2",
            //    Type = CCTVControlType.TransDVR,
            //    Ip = "192.168.9.251",
            //    Port = 8000,
            //    UserName = "admin",
            //    Password = "admin12345",
            //    Channel = 0,
            //    CameraId = 5,
            //    SerialPort = 1,
            //    ReverseZoom = false,
            //});

            _dictStaticInfo.Add("CCTV1_50BAD15900010302", new CCTVStaticInfo()
            {
                VideoId = "CCTV1_50BAD15900010302",
                Name = "56ICU03",
            });
            _dictControl.Add("CCTV1_50BAD15900010302", new CCTVControlConfig()
            {
                VideoId = "CCTV1_50BAD15900010302",
                Type = CCTVControlType.TCP,
                Ip = "192.168.9.56",
                Port = 4001,
                CameraId = 1,
                ReverseZoom = false,
            });

            _dictStaticInfo.Add("video4", new CCTVStaticInfo()
            {
                VideoId = "video4",
                Name = "706研发大厅",
            });
            _dictControl.Add("video4", new CCTVControlConfig()
            {
                VideoId = "video4",
                Type = CCTVControlType.UnControl,
                Ip = "192.168.9.97",
                Port = 8000,
                UserName = "admin",
                Password = "12345",
                Channel = 1,
            });
        }

        //public CCTVStaticInfo GetStaticInfo(string videoId)
        //{
        //    if (!string.IsNullOrWhiteSpace(videoId) && _dictStaticInfo.ContainsKey(videoId))
        //        return _dictStaticInfo[videoId];
        //    return null;
        //}

        //public CCTVStaticInfo[] GetAllStaticInfo()
        //{
        //    return _dictStaticInfo.Values.ToArray();
        //}

        //public CCTVControlConfig GetControlConfig(string videoId)
        //{
        //    if (!string.IsNullOrWhiteSpace(videoId) && _dictControl.ContainsKey(videoId))
        //        return _dictControl[videoId];
        //    return null;
        //}

        public CCTVDefaultInfoSync ClientHub { get; private set; }
        public void Init(string webApiBaseUri)
        {
            ClientHub = new CCTVDefaultInfoSync(webApiBaseUri);
            ClientHub.RegisterDefaultWithoutUpdate(CCTVInfoType.GlobalInfo);
            ClientHub.RegisterDefault(CCTVInfoType.HierarchyInfo, TimeSpan.FromSeconds(5));
            ClientHub.RegisterDefault(CCTVInfoType.StaticInfo, TimeSpan.FromSeconds(5));
            ClientHub.RegisterDefault(CCTVInfoType.DynamicInfo, TimeSpan.FromSeconds(5));
            ClientHub.RegisterDefault(CCTVInfoType.ControlConfig, TimeSpan.FromSeconds(10));
        }

        public CCTVStaticInfo[] GetAllStaticInfo()
        {
            return ClientHub.GetAllStaticInfo();
        }

        public CCTVStaticInfo GetStaticInfo(string videoId)
        {
            return ClientHub.GetStaticInfo(videoId);
        }

        public CCTVControlConfig GetControlConfig(string videoId)
        {
            return ClientHub.GetControlConfig(videoId);
        }

        public CCTVControlConfig[] GetAllControlConfig()
        {
            return ClientHub.GetAllControlConfig();
        }

        public CCTVOnlineStatus GetOnlineStatus(string videoId)
        {
            return ClientHub.GetOnlineStatus(videoId);
        }

        public void PutCameraLimitsInfo(CCTVCameraLimits limit)
        {
            ClientHub.PutCameraLimitsInfo(limit, false);
        }
    }
}