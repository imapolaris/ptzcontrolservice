using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class PTZObtainer
    {
        double _lon;
        double _lat;
        double _alt;
        double _fov;
        double _correctLength = 1;
        double _flRatio;
        /// <summary>
        /// 摄像机位置及视场角
        /// </summary>
        /// <param name="lon">经度:°</param>
        /// <param name="lat">纬度:°</param>
        /// <param name="alt">高度:m </param>
        /// <param name="fov">水平视场角:°</param>
        public PTZObtainer(double lon, double lat, double alt, double fov)
        {
            _lon = lon;
            _lat = lat;
            _alt = alt;
            _fov = fov;
            load();
        }

        private void load()
        {
            _correctLength = Math.Cos(_lat * Math.PI / 180);
            _flRatio = Math.Tan(_fov / 2 * Math.PI / 180);
            if (_correctLength <= 0)
                throw new InvalidSettingException("无效的摄像机纬度.");
            if (_flRatio <= 0)
                throw new InvalidSettingException("无效的摄像机视场角.");
        }


        /// <summary>正北0，顺时针（0~360）</summary>
        /// <param name="x">转化后的经度方向的距离（m）</param>
        /// <param name="y">转化后的纬度方向的距离（m）</param>
        public static double GetPan(double x, double y)
        {
            double pan = Math.Round(Math.Atan2(x, y) * 180 / Math.PI, 1);//正北0，顺时针（-180~180）
            if (pan < 0)
                pan += 360;
            return pan;
        }

        public static double GetTilt(double h, double hordist)
        {
            return  Math.Round(Math.Tan(h / hordist) * 180 / Math.PI, 1);
        }

        public double GetZoom(double length, double distace)
        {
            if (length <= 0.01)
                return 100;
            return Math.Round(_flRatio * distace * 2 / length, 1);
        }

        /// <summary>Point3d X：lon; Y: lat, Z: alt, 单位：米</summary>
        public Point3d GetPoint(double lon, double lat, double alt)
        {
            double y = (lat - _lat) * (40007862.917 / 360.0);//正北为正方向
            double x = (lon - _lon) * (40075015.68 / 360.0) * _correctLength;
            double z = _alt - alt;
            return new Point3d(x, y, z);
        }

        public PTZ GetPTZ(double lon, double lat, double alt, double length)
        {
            Point3d pt = GetPoint(lon, lat, alt);
            double hordist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            double dist = Math.Sqrt(hordist * hordist + pt.Z * pt.Z);
            double pan = GetPan(pt.X, pt.Y);
            double tilt = GetTilt(pt.Z, hordist);
            double zoom = GetZoom(length, dist);
            return new PTZ(Math.Round(pan, 1), Math.Round(tilt, 1), Math.Round(zoom, 1));
        }
    }
}
