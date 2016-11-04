using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public static class PTZManager
    {
        public static double GetNeedWaitSeconds(double curZoom, double nextZoom, double minSeconds = 0.8)
        {
            double waitSec = 0.5;
            if (nextZoom >= 10)
                waitSec = Math.Round(Math.Min(3, Math.Max(minSeconds, (nextZoom / curZoom - 1) * 2)),1);
            return waitSec;
        }
    }
}
