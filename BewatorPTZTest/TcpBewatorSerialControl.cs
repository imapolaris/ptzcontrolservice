using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BewatorPTZTest
{
    public class TcpBewatorSerialControl
    {
        TcpBewatorSerialComm _tcp = null;
        PelcoD _pelcoD;
        public TcpBewatorSerialControl(string ip, int port, byte camId, bool reverseZoom)
        {
            _pelcoD = new PelcoD(camId, reverseZoom);
            _tcp = new TcpBewatorSerialComm(ip, port, camId);
        }

        public void Send(byte[] buffer)
        {
            _tcp.Send(buffer);
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            _tcp?.Send(_pelcoD.CamaraControl(action, actData));
        }

        public DateTime LatestTime
        {
            get
            {
                if (_tcp != null)
                    return _tcp.LatestTime;
                else
                    return DateTime.MinValue;
            }
        }

        public void Dispose()
        {
            _tcp?.Dispose();
            _tcp = null;
        }
    }
}
