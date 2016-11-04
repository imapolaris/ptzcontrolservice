using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class PTZLimit
    {
        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Up { get; private set; }
        public double Down { get; private set; }
        public double ZoomMax { get; private set; }
        public PTZLimit(double left, double right, double up, double down, double zoomMax)
        {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
            ZoomMax = zoomMax;
        }
    }
}
