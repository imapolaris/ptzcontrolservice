using PTZControlService.Hikvision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class DvrSerialControl : IControl, IDisposable
    {
        HikSerial _hikSerial = null;

        public DvrSerialControl(string ip, int port, string userName, string password, int channel)
        {
            _hikSerial = new HikSerial(ip, port, userName, password, channel);
            _hikSerial.PTZEvent += onPTZ;
        }

        public DateTime LatestTime
        {
            get
            {
                if(_hikSerial != null)
                    return _hikSerial.LatestTime;
                return DateTime.MinValue;
            }
        }

        public PTZLimit PTZLimits { get { return _hikSerial?.PTZLimits; } }

        public PTZ PTZPosition { get { return _hikSerial?.PTZPosition; } }
        public Action<PTZ> PTZEvent { get; set; }

        public void CameraControl(CameraAction action, byte actData)
        {
            var cmd = PTZCommand.Parse(action, actData);
            if (cmd == null)
                throw new CanNotControlExpection("无效的控制指令。");
            if (_hikSerial == null)
                throw new CanNotControlExpection("未建立通信，云镜控制失败。");
            var cwsConfig = cmd as PTZControlWithSpeedConfig;
            if (cwsConfig != null)
            {
                if (!_hikSerial.PTZControlWithSpeed(cwsConfig.PTZCommand, cwsConfig.Stop, cwsConfig.Speed))
                    throw new CanNotControlExpection();
                return;
            }
            var preset = cmd as PTZPresetConfig;
            if (preset != null)
            {
                if (!_hikSerial.PTZPreset(preset.PTZPresetCmd, preset.PresetIndex))
                    throw new CanNotControlExpection();
                return;
            }
        }

        public bool CaptureJPEGPicture(string fileName, ushort wPicSize)
        {
            return _hikSerial.CaptureJPEGPicture(fileName, wPicSize);
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(PTZPosition);
        }
        
        public void Dispose()
        {
            _hikSerial?.Dispose();
            _hikSerial = null;
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            _hikSerial.ToPTZ(pan, tilt, zoom);
        }
    }
}
