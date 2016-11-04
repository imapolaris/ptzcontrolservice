using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public static class SnapshotStaticInfo
    {
        public static double DistanceMax = 3000;   //最大抓拍距离
        public static double DistanceMin = 1;       //最小抓拍距离
        public static double DefaultLength = 30;    //默认船长
        public static double TimeoutSeconds = 20;   //抓拍超时秒数
    }
}
