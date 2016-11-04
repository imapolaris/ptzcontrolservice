using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVPTZSnapshot
{
    //public class RotationTimeConsuming
    //{
    //    public static double GetConsumingSeconds(PTZ cur, PTZ exp)
    //    {
    //        double panSec = (Math.Abs(cur.Pan - exp.Pan) - 9) / 70 + 0.45;
    //        double tileSec = (Math.Abs(cur.Tilt - exp.Tilt) - 9) / 45 + 0.45;

    //        double zoomSec = Math.Abs(cur.Zoom - exp.Zoom) / 7 + 0.35;
    //        if (cur.Zoom < 5 || exp.Zoom < 5)
    //            zoomSec += (Math.Min(5, Math.Max(cur.Zoom, exp.Zoom)) - Math.Min(cur.Zoom, exp.Zoom)) / 2;

    //        return Math.Max(zoomSec,Math.Max(panSec, tileSec));
    //    }
    //}
}
