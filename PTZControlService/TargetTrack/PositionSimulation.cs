using CCTVModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.TargetTrack
{
    public class PositionSimulation
    {
        CCTVStaticInfo _info;
        //string _targetID;
        //DateTime _beginTime = DateTime.MinValue;
        //List<TargetInfo> _targetInfos = new List<TargetInfo>();
        public TimeSpan TimeOutSup { get; set; } = TimeSpan.FromMinutes(5);//5分钟，与GIS保持一致
        public bool IsControlZoomAll { get; set; } = true;  //True时表示Zoom值自动，false表示仅首次调用自动，其它时刻非自动
        public double DistanceSup { get; set; } = 1852.0;//最大跟踪距离1海里，与GIS保持一致
        bool _isFirstControlZoom = false;
        DynamicTarget _target = null;
        TargetInfo _targetBase = null;
        PTZObtainer _ptzObtainer;
        public double Altitude { get; set; } = 0;
        object _objLock = new object();
        public PositionSimulation(CCTVStaticInfo info, bool isControlZoomAll = true)
        {
            _info = info;
            _ptzObtainer = new PTZObtainer(_info.Longitude, _info.Latitude, _info.Altitude, _info.ViewPort);
            IsControlZoomAll = isControlZoomAll;
        }

        public void UpdateTarget(DynamicTarget target)
        {
            lock(_objLock)
            {
                if (target == null)
                    removeTrack();
                else
                {
                    if (_target == null || _target.Id != target.Id)
                        initTarget();
                    updateTarget(target);
                }
            }
        }
         
        public PTZ GetPTZ(DateTime time)
        {
            lock(_objLock)
            {
                if (_target == null || time < _target.Time)
                    return null;
                if (!handleTimeOut(_target.Time, time))//超时，移除跟踪目标
                    return null;
                double secondSpan = (time - _target.Time).TotalSeconds;
                double x = _targetBase.X + _targetBase.SpeedX * secondSpan;
                double y = _targetBase.Y + _targetBase.SpeedY * secondSpan;
                double z = _info.Altitude - Altitude;
                double horizonDis = Math.Sqrt(x * x + y * y);
                if (!handleDistanceSup(horizonDis))//目标移动到监控范围之外
                    return null;
                double pan = PTZObtainer.GetPan(x, y);
                double tilt = PTZObtainer.GetTilt(z, horizonDis);
                double zoom = getZoom(x, y, z);
                return new PTZ(pan, tilt, zoom);
            }
        }
        
        private void initTarget()
        {
            _isFirstControlZoom = true;
        }

        private double getZoom(double x, double y, double z)
        {
            double zoom = 0;
            if (IsControlZoomAll || _isFirstControlZoom)
            {
                int length = Math.Max(_target.Length, _target.Width);
                if (length <= 10 || length > 350)
                    length = 100;
                zoom = _ptzObtainer.GetZoom(length + 40, Math.Sqrt(x * x + y * y + z * z));
            }
            _isFirstControlZoom = false;
            return zoom;
        }

        private void updateTarget(DynamicTarget target)
        {
            _target = target;
            _targetBase = new TargetInfo();
            Point3d pt = _ptzObtainer.GetPoint(_target.Lon, _target.Lat, Altitude);
            _targetBase.X = pt.X;
            _targetBase.Y = pt.Y;
            if (_target.Sog > 1)
            {
                _targetBase.SpeedX = _target.Sog * 1852.0 / 3600 * Math.Sin(_target.Cog * Math.PI / 180);
                _targetBase.SpeedY = _target.Sog * 1852.0 / 3600 * Math.Cos(_target.Cog * Math.PI / 180);
            }
            double horizonDis = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            handleDistanceSup(Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y));
        }

        private void removeTrack()
        {
            _target = null;
            _targetBase = null;
        }
        
        public void Dispose()
        {
            lock(_objLock)
            {
                removeTrack();
            }
        }

        /// <summary>跟踪目标是否在摄像机有效监控范围内判断，若不在，移除跟踪目标</summary>
        /// <param name="dis">目标距离摄像机的水平距离</param>
        /// <returns>true表示目标在有效范围内，</returns>
        bool handleDistanceSup(double dis)
        {
            if (dis> DistanceSup)//目标移动到监控范围之外
            {
                removeTrack();
                Console.WriteLine("Distance Sup!");
                return false;
            }
            return true;
        }

        /// <summary>跟踪目标是否超时，若超时，移除跟踪目标</summary>
        bool handleTimeOut(DateTime beginTime, DateTime curTime)
        {
            if (curTime > beginTime.Add(TimeOutSup))
            {
                removeTrack();
                Console.WriteLine("Time Out!");
                return false;
            }
            return true;
        }

        class TargetInfo
        {
            public double X;
            public double Y;
            public double SpeedX;
            public double SpeedY;
        }
    }
}