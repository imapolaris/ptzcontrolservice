using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class RotationTimeConsuming
    {
        public static double GetConsumingSeconds(PTZ cur, PTZ exp)
        {
            double panSec = (Math.Abs(cur.Pan - exp.Pan) - 9) / 70 + 0.5;
            double tileSec = (Math.Abs(cur.Tilt - exp.Tilt) - 9) / 45 + 0.5;
            double zoomSec = getConsumingSeconds(cur.Zoom, exp.Zoom);

            double seconds = Math.Round(Math.Max(zoomSec,Math.Max(panSec, tileSec)),1);
            return seconds;
        }

        static double getConsumingSeconds(double curZoom, double expZoom)
        {
            double zoomSec = 0;
            if (curZoom != expZoom)
            {
                double dif = Math.Abs(curZoom - expZoom);
                if (dif < 5)
                    zoomSec = dif / 3;
                else
                    zoomSec = (dif - 5) / 7 + 5.0 / 3;
                if (curZoom < 5 || expZoom < 5)
                    zoomSec += (Math.Min(5, Math.Max(curZoom, expZoom)) - Math.Min(curZoom, expZoom)) * 0.4;
            }
            return zoomSec;
        }
    }
}
