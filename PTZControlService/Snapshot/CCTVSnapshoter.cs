using CCTVModels;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class CCTVSnapshoter: IDisposable
    {
        ControlWithSnapShot _snapshot;
        PTZObtainer _ptzObtainer;
        IControl _control;
        public bool Snapshotting { get { return _snapshot != null && _snapshot.Snapshotting; } }
        public DynamicTarget LastDynamicTarget { get; private set; }
        bool _lastFromMinZoom = false;
        bool _isUpdatedTarget = false;
        SnapshotModel _qualityModel;
        public SnapshotModel QualityModel
        {
            get { return _qualityModel; }
            set
            {
                _qualityModel = value;
                if (_qualityModel == SnapshotModel.HighQuality)
                    _snapshot.DefaultFocusSeconds = 2;
                else
                    _snapshot.DefaultFocusSeconds = 1;
            }
        }

        public CCTVSnapshoter(CCTVStaticInfo staticInfo, IControl control, string savePath)
        {
            _control = control;
            _snapshot = new ControlWithSnapShot(control, savePath);
            _snapshot.SnapshotEvent += onSnapshot;
            QualityModel = SnapshotModel.HighQuality;
            _ptzObtainer = new PTZObtainer(staticInfo.Longitude, staticInfo.Latitude, staticInfo.Altitude, staticInfo.ViewPort);
        }

        /// <summary>单目标抓拍模式,返回估计抓拍耗时（秒）</summary>
        public void Snapshot(string id, string shipName, double lon, double lat, double alt, double sog, double cog, double width, DateTime startTime)
        {
            snapshot(id, shipName, lon, lat, startTime, alt, sog, cog, width);
        }
        
        public void Snapshot(string id, string name, PTZ[] ptzs, double panSpeed, DateTime startTime)
        {
            _snapshot.Snapshot(id, name, ptzs, panSpeed, startTime);
        }

        public void Snapshot(string shipName, params PTZ[] ptzs)
        {
            Snapshot(shipName, shipName, ptzs, 0, DateTime.Now);
        }

        public void ToGeometry(double longitude, double latitude, double altitude, double width)
        {
            Point3d pt = _ptzObtainer.GetPoint(longitude, latitude, altitude);
            double leveldist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            double dist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y + pt.Z * pt.Z);
            double pan = adjustPan(PTZObtainer.GetPan(pt.X, pt.Y));
            double tilt = adjustTilt(PTZObtainer.GetTilt(pt.Z, leveldist));
            double zoom = adjustZoom(_ptzObtainer.GetZoom(width, dist));
            _snapshot.MoveTo(pan, tilt, zoom);
        }

        public void Snapshot(DynamicTarget target)
        {
            _isUpdatedTarget = (LastDynamicTarget != null && LastDynamicTarget.Id.Equals(target.Id));//判断是否为更新抓拍目标
            LastDynamicTarget = target;
            string name = SnapshotNameGenerate.GetShortName(target);
            snapshot(target.Id, name, target.Lon, target.Lat, target.Time, 0, target.Sog, target.Cog, target.Length, target.Width, target.Heading, target.MeasureA);
        }

        /// <summary>算法以正对船身为模板进行抓拍</summary>
        /// <param name="pt">相对摄像机坐标点（X：Lon方向；Y：Lat方向；Z：Tilt方向）</param>
        /// <param name="speed">移动速度米/秒</param>
        /// <param name="radian">移动方向/弧度，正北为0</param>
        /// <param name="width">目标宽度</param>
        /// <param name="seconds">输出估算转动耗时</param>
        /// <returns>采样点列</returns>
        private PTZ[] getPTZsFromExp(Point3d pt, double speed, double radian, double width, ref double panTurnByMoving)
        {
            double leveldist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            if (isInvalidDistance(leveldist))
            {
                Logger.Default.Warn("不支持该距离抓拍！ {0}m", leveldist);
                return null;
            }
            double dist = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y + pt.Z * pt.Z);
            panTurnByMoving = Math.Atan((speed * Math.Sin(radian) * pt.Y - speed * Math.Cos(radian) * pt.X) / leveldist / dist) * 180 / Math.PI;   //目标移动速度转为摄像机水平旋转速度
            double panTurnByWidth = (panTurnByMoving >= 0? 1: -1) * Math.Atan(width / 3 / dist) * 180 / Math.PI;    //目标宽度的一半对应的水平角度范围

            double pan = adjustPan(PTZObtainer.GetPan(pt.X, pt.Y));
            double tilt = adjustTilt(PTZObtainer.GetTilt(pt.Z, leveldist));
            double minZoom = adjustZoom(_ptzObtainer.GetZoom(width * 2 / 3 + panTurnByMoving * 2, dist));
            double maxZoom = adjustZoom(_ptzObtainer.GetZoom(width * 1.5 + panTurnByMoving * 3, dist));
            Logger.Default.Info("Dis:{0}, Pan: {1}, Width:{2} Zoom:{3}", Math.Round(dist),pan, Math.Round(width), _ptzObtainer.GetZoom(width, dist));

            List<PTZ> ptzs = new List<PTZ>();
            if (QualityModel != SnapshotModel.LowQuality)
            {
                if (speed != 0)
                    ptzs.Add(new PTZ(adjustPan(pan + panTurnByWidth * 2), tilt, minZoom));//前端
                if (minZoom != maxZoom && minZoom != 1)
                {
                    ptzs.Add(new PTZ(adjustPan(pan + panTurnByWidth), tilt, minZoom));//前侧
                    ptzs.Add(new PTZ(adjustPan(pan - panTurnByWidth), tilt, minZoom));//后侧
                }
            }
            addPTZ(ptzs, new PTZ(pan, tilt, maxZoom), (minZoom + maxZoom) / 2);//整船
            return ptzs.ToArray();
        }

        private bool isInvalidDistance(double dist)
        {
            return (dist > SnapshotStaticInfo.DistanceMax || dist < SnapshotStaticInfo.DistanceMin);
        }

        private void addPTZ(List<PTZ> ptzs, PTZ ptz, double centerZoom)//整船
        {
            _lastFromMinZoom = (_isUpdatedTarget && _lastFromMinZoom) || (!_isUpdatedTarget && (_control.PTZPosition.Zoom >= centerZoom));
            if (_lastFromMinZoom || ptzs.Count == 0)
                ptzs.Add(ptz);
            else
                ptzs.Insert(0, ptz);
        }

        void snapshot(string id, string fileName, double lon, double lat, DateTime time, double alt, double sog = 0, double cog = 0, double length = 0, double width = 0, int heading = 511, int measureA = 0)
        {
            Point3d pt = _ptzObtainer.GetPoint(lon, lat, alt);
            double size = getSizeFromCamera(pt, heading, length, width, measureA);
            double panSpeed = 0;
            PTZ[] ptzs = getPTZsFromExp(pt, sog * 1852.0 / 3600, cog * Math.PI / 180, size, ref panSpeed);
            if(ptzs != null)
                Snapshot(id, fileName, ptzs, panSpeed, time);
        }

        private double getSizeFromCamera(Point3d pt, int heading, double length, double width, int measureA)
        {
            double size = length;
            if (heading < 360 && heading >= 0 && length > 0)
            {
                double jiajiao = Math.Atan(pt.Y / pt.X) - heading * Math.PI / 180 + Math.PI / 2;
                size = length * Math.Abs(Math.Cos(jiajiao)) + width * Math.Abs(Math.Sin(jiajiao));
                if (measureA > 0)
                {
                    pt.X += Math.Sin(heading * Math.PI / 180) * (measureA - length / 2);
                    pt.Y += Math.Cos(heading * Math.PI / 180) * (measureA - length / 2);
                }
            }
            if (size <= 0)
                size = SnapshotStaticInfo.DefaultLength;
            return size;
        }
                
        private double adjustZoom(double zoom)
        {
            zoom = Math.Round(zoom);
            if (zoom > 10)
                zoom = Math.Round(zoom / 5) * 5;
            return PTZConverter.ToValid(zoom, 1, _control.PTZLimits.ZoomMax);
        }

        private double adjustPan(double pan)
        {
            pan = PTZConverter.ToPanAngle(Math.Round(pan, 1));
            return PTZConverter.ToValidAngle(pan, _control.PTZLimits.Left, _control.PTZLimits.Right);
        }

        private double adjustTilt(double tilt)
        {
            tilt = Math.Round(tilt, 1);
            return PTZConverter.ToValid(tilt, _control.PTZLimits.Up, _control.PTZLimits.Down);
        }
                
        public void Dispose()
        {
            Logger.Default.Info("----------------结束抓拍-----------------");
            if(_snapshot != null)
            {
                _snapshot.SnapshotEvent -= onSnapshot;
                _snapshot?.Dispose();
            }
            _snapshot = null;
            _control = null;
            _ptzObtainer = null;
        }

        /// <summary>更新快照记录</summary>
        private void onSnapshot(ControlWithSnapShot.TargetSnapshotInfo obj)
        {
            if (LastDynamicTarget != null && LastDynamicTarget.Id == obj?.Id)
            {
                var handle = SnapshotEvent;
                if (handle != null)
                    handle(LastDynamicTarget, obj.FileName, obj.Time, obj.IsSecceeded, obj.Prompt);
            }
        }

        public OnSnapShot SnapshotEvent;
        public delegate void OnSnapShot(DynamicTarget target, string fileName, DateTime snapshotTime, bool isSecceeded, string prompt = null);
    }
}