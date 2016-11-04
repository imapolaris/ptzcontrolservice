using CCTVModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class TcpSerialControl : IControl, IDisposable
    {
        TcpSerialComm _tcp = null;
        public PTZ PTZPosition { get { return _tcp?.PTZPosition; } }
        public PTZLimit PTZLimits { get { return _tcp?.PTZLimits; } }
        public Action<PTZ> PTZEvent { get; set; }

        public DateTime LatestTime
        {
            get
            {
                return _tcp?.LatestTime ?? DateTime.MinValue;
            }
        }
        
        PelcoD _pelcoD;

        public TcpSerialControl(string ip, int port, SerialType serialType, byte serialId, bool reverseZoom, double zoomMax)
        {
            _pelcoD = new PelcoD(serialId, reverseZoom);
            _tcp = new TcpSerialComm(ip, port, serialId, serialType, zoomMax);
            _tcp.PTZEvent += onPTZ;
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            _tcp?.Send(_pelcoD.CamaraControl(action, actData));
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            _tcp?.ToPTZ(pan, tilt, zoom);
        }

        public bool CaptureJPEGPicture(string fileName, ushort wPicSize = 2)
        {
            //throw new InvalidOperationException("不支持抓拍！");
            return false;
        }

        public void Dispose()
        {
            _tcp?.Dispose();
            _tcp = null;
        }

        public void Send(byte[] buffer)
        {
            _tcp.Send(buffer);
        }
    }
}
