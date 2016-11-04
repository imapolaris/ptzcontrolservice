using PTZControlService.Hikvision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class TransSerialControl : IControl, IDisposable
    {
        HikTransSerial _transSerial = null;

        public PTZ PTZPosition { get { return _transSerial?.PTZPosition; } }
        public PTZLimit PTZLimits { get { return _transSerial?.PTZLimits; } }
        public Action<PTZ> PTZEvent { get; set; }
        public DateTime LatestTime
        {
            get
            {
                if (_transSerial != null)
                    return _transSerial.LatestTime;
                else
                    return DateTime.MinValue;
            }
        }
        
        PelcoD _pelcoD;
        
        public TransSerialControl(string ip, int port, string userName, string password, int serialPort, byte camId, int channel, bool reverseZoom, double zoomMax)
        {
            _pelcoD = new PelcoD(camId, reverseZoom);
             _transSerial = new HikTransSerial(ip, port, userName, password, serialPort, camId, channel, zoomMax);
            _transSerial.PTZEvent += onPTZ;
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            if (_transSerial == null)
                throw new CanNotControlExpection("未建立通信，云镜控制失败。");
            if (!_transSerial.SerialSend(_pelcoD.CamaraControl(action, actData)))
                throw new CanNotControlExpection();
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            if(_transSerial != null)
                _transSerial.ToPTZ(pan, tilt, zoom);
        }

        public bool CaptureJPEGPicture(string fileName, ushort wPicSize = 2)
        {
            if(_transSerial != null)
                return _transSerial.CaptureJPEGPicture(_transSerial.Channel, fileName, wPicSize);
            return false;
        }

        public void Dispose()
        {
            _transSerial?.Dispose();
            _transSerial = null;
        }
    }
}
