using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class PTZ
    {
        public double Pan { get; private set; }
        public double Tilt { get; private set; }
        public double Zoom { get; private set; }
        public PTZ(double pan, double tilt, double zoom)
        {
            Pan = pan;
            Tilt = tilt;
            Zoom = zoom;
        }
    }
}
