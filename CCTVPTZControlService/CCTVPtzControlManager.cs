using Common.Log;
using PTZControlService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCTVPTZControlService
{
    public class CCTVPtzControlManager
    {
        public static CCTVPtzControlManager Instance { get; private set; } = new CCTVPtzControlManager();
        ConcurrentDictionary<string, PTZControl> _controls = new ConcurrentDictionary<string, PTZControlService.PTZControl>();
        
        public void PTZControl(string videoId, CameraAction action, byte actData)
        {
            var control = GetOrAddPtzControl(videoId);
            if (control != null)
                control.CameraControl(action, actData);
            else
                throw new CanNotControlExpection("控制失败！");
        }
        
        public PTZControl GetOrAddPtzControl(string videoId)
        {
            lock(_controls)
            {
                var control = getControl(videoId);
                if (control == null)
                {
                    var config = getControlInfo(videoId);
                    _controls[videoId] = new PTZControl(config);//TODO: 根据静态信息调整
                    updateLimit(videoId);
                }
                return _controls[videoId];
            }
        }

        private void updateLimit(string videoId)
        {
            var control = _controls[videoId];
            var ptzLimit = control.PTZLimits;
            if (ptzLimit != null)
            {
                CCTVModels.CCTVCameraLimits limit = new CCTVModels.CCTVCameraLimits();
                limit.LeftLimit = ptzLimit.Left;
                CCTVStaticInfoManager.Instance.PutCameraLimitsInfo(limit);
            }
        }

        public void CaptureJPEGPicture(string videoId, string fileName)
        {
            lock (_controls)
            {
                IControl control = getControl(videoId);
                if (control != null)
                    control.CaptureJPEGPicture(fileName, 2);
                else
                {
                    var config = getControlInfo(videoId);
                    using (var serial = new PTZControlService.Hikvision.HikSerial(config.Ip, config.Port, config.UserName, config.Password, config.Channel))
                    {
                        serial.CaptureJPEGPicture(fileName, 2);
                    }
                }
            }
        }

        CCTVModels.CCTVControlConfig getControlInfo(string videoId)
        {
            var video = CCTVStaticInfoManager.Instance.GetStaticInfo(videoId);
            if (video == null)
                throw new CanNotControlExpection("未找到该设备。");
            if (video.Platform != CCTVModels.CCTVPlatformType.CCTV2)
                throw new CanNotControlExpection("不支持的设备类型。");
            var config = CCTVStaticInfoManager.Instance.GetControlConfig(videoId);
            if (config == null)
                throw new CanNotControlExpection("未找到该设备控制信息。");
            return config;
        }

        private PTZControl getControl(string videoId)
        {
            if (_controls.ContainsKey(videoId))
                return _controls[videoId];
            return null;
        }
    }
}
