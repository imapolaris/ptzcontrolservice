using CCTVModels;
using PTZControlService.TargetTrack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PTZControlService
{
    public class PTZControl : IControl, IDisposable
    {
        IControl _control = null;
        public PTZLimit PTZLimits { get { return _control?.PTZLimits; } }
        public PTZ PTZPosition { get { return _control?.PTZPosition; } }
        public Action<PTZ> PTZEvent { get; set; }

        public CCTVControlConfig PtzConfig { get; private set; }
        public DateTime LatestTime { get { return _control.LatestTime; } }
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public static TimeSpan TimeOutSpan { get; set; } = TimeSpan.FromSeconds(3);
        object _objLock = new object();
        int _zoomMax = 37;
        public PTZControl(CCTVControlConfig config, int zoomMax = 37)
        {
            PtzConfig = config;
            _zoomMax = zoomMax;
            _control = newControl(PtzConfig, zoomMax);
            _control.PTZEvent += onPTZ;
            _disposeEvent.Reset();
            new Thread(run) { IsBackground = true }.Start();
        }

        private void run()
        {
            while (!_disposeEvent.WaitOne(TimeSpan.FromSeconds(1)))
            {
                try
                {
                    lock (_objLock)
                    {
                        if (LatestTime.Add(TimeOutSpan) < DateTime.Now)
                            reConnect();
                        moveToTrackTarget();
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.Logger.Default.Error(ex);
                    _disposeEvent.WaitOne(TimeSpan.FromSeconds(10));
                }
            }
        }

        void reConnect()
        {
            disposeConnect();
            _control = newControl(PtzConfig, _zoomMax);
            _control.PTZEvent += onPTZ;
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            lock (_objLock)
            {
                _control?.CameraControl(action, actData);
            }
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            lock (_objLock)
            {
                _control?.ToPTZ(pan, tilt, zoom);
            }
        }

        public bool CaptureJPEGPicture(string fileName, ushort wPicSize)
        {
            if (_control != null)
                return _control.CaptureJPEGPicture(fileName, wPicSize);
            return false;
        }

        private IControl newControl(CCTVControlConfig config, double zoomMax = 37)
        {
            if (config == null)
                throw new CanNotControlExpection("未配置控制参数");
            if (config.Type == CCTVControlType.DVR)
                return new DvrSerialControl(config.Ip, config.Port, config.UserName, config.Password, config.Channel);
            else if (config.Type == CCTVControlType.TCP)
                return new TcpSerialControl(config.Ip, config.Port, config.SerialType, (byte)config.SerialPort, config.ReverseZoom, zoomMax);
            else if (config.Type == CCTVControlType.TransDVR)
                return new TransSerialControl(config.Ip, config.Port, config.UserName, config.Password, config.SerialPort, config.CameraId, config.Channel, config.ReverseZoom, zoomMax);
            else if (config.Type == CCTVControlType.UnControl)
                throw new CanNotControlExpection("云镜不可控！");
            else
                throw new CanNotControlExpection("不支持的控制类型！");
        }

        private void disposeConnect()
        {
            if (_control != null)
                _control.Dispose();
            _control = null;
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            Thread.Sleep(1000);
            disposeTrack();
            disposeConnect();
        }

        #region 目标跟踪

        PositionSimulation _pSimulation;

        public void InitTargetTrack(CCTVStaticInfo staticInfo)
        {
            lock(_objLock)
            {
                _pSimulation = new PositionSimulation(staticInfo);
            }
        }

        public void TrackToDynamicTarget(DynamicTarget target)
        {
            lock (_objLock)
            {
                if (_pSimulation == null)
                    throw new CanNotControlExpection("未设置云台静态信息。");
                _pSimulation?.UpdateTarget(target);
            }
        }

        public void StopTrack()
        {
            lock (_objLock)
            {
                _pSimulation?.UpdateTarget(null);
            }
        }

        private void moveToTrackTarget()
        {
            if (_pSimulation != null)
            {
                PTZ ptz = _pSimulation.GetPTZ(DateTime.Now);
                if (ptz == null)
                    return;
                if (!PTZConverter.IsValidAngle(ptz.Pan, PTZLimits.Left, PTZLimits.Right))
                {
                    Console.WriteLine("目标已离开监控区域！");
                    _pSimulation.UpdateTarget(null);
                }
                Console.WriteLine($"{ptz.Pan}, {ptz.Tilt}, {ptz.Zoom}");

                if (ptz.Zoom != 0)
                {
                    if (Math.Abs(ptz.Zoom - PTZPosition.Zoom) < 0.3 || Math.Abs(ptz.Zoom - PTZPosition.Zoom) / PTZPosition.Zoom < 0.2)
                        ptz = new PTZ(ptz.Pan, ptz.Tilt, 0);
                }
                _control?.ToPTZ(ptz.Pan, ptz.Tilt, ptz.Zoom);
            }
        }

        private void disposeTrack()
        {
            if (_pSimulation != null)
                _pSimulation.Dispose();
            _pSimulation = null;
        }
        #endregion 目标跟踪
    }
}