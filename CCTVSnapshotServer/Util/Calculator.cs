using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.Util
{
    public static class Calculator
    {
        public static double CalcDis(double lon1, double lat1, double lon2, double lat2)
        {
            double x1 = Math.Cos(lat1 * Math.PI / 180) * Math.Sin(lon1 * Math.PI / 180);
            double y1 = Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lon1 * Math.PI / 180);
            double z1 = Math.Sin(lat1 * Math.PI / 180);
            double x2 = Math.Cos(lat2 * Math.PI / 180) * Math.Sin(lon2 * Math.PI / 180);
            double y2 = Math.Cos(lat2 * Math.PI / 180) * Math.Cos(lon2 * Math.PI / 180);
            double z2 = Math.Sin(lat2 * Math.PI / 180);
            double d = x1 * x2 + y1 * y2 + z1 * z2;
            if (d > 1)
                d = 1;
            return Math.Acos(d) * 180 / Math.PI * 60;
        }
    }
}
