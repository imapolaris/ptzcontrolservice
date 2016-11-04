using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public static class PTZConverter
    {
        public static double ToPanAngle(double angle)
        {
            if (angle >= 0 && angle < 360)
                return angle;
            double circ = angle / 360;
            double circ0 = circ - (int)circ;
            if (circ0 < 0)
                circ0 += 1;
            return circ0 * 360;
        }

        public static bool IsValidAngle(double angle, double beginAngle, double endAngle)
        {
            if (beginAngle < endAngle)
                return angle >= beginAngle && angle < endAngle;
            return (angle >= beginAngle || angle < endAngle);
        }
        
        public static double ToValidAngle(double angle, double beginAngle, double endAngle)
        {
            if (IsValidAngle(angle, beginAngle, endAngle))
                return angle;
            double toBegin = Math.Abs(angle - beginAngle);
            if (toBegin > 180)
                toBegin = 360 - toBegin;
            double toEnd = Math.Abs(angle - endAngle);
            if (toEnd > 180)
                toEnd = 360 - toEnd;
            return toBegin < toEnd ? beginAngle : endAngle;
        }

        public static bool IsValid(double data, double inf, double sup)
        {
            return data >= inf && data <= sup;
        }

        public static double ToValid(double data, double inf, double sup)
        {
            if(inf < sup)
                return Math.Max(inf, Math.Min(sup, data));
            else
                return Math.Max(sup, Math.Min(inf, data));
        }
    }
}
