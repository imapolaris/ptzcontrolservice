using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CCTVModels;
using PTZControlService;
using AopUtil.WpfBinding;

namespace PTZControlClient
{
    public class TestTargetTracker: ObservableObject, IDisposable
    {
        PTZControl _tracker;
        Thread _thread;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        string _shipId = Guid.NewGuid().ToString();
        DynamicTarget _dynamicTarget;
        PTZObtainer _ptzObtainer;
        [AutoNotify]
        public PTZ ExpPtz { get; set; }

        public TestTargetTracker(PTZControl control, CCTVStaticInfo staticInfo, DynamicTarget dynamicTarget)
        {
            if (control == null)
                throw new InvalidOperationException("无效的控制类型");
            _ptzObtainer = new PTZObtainer(staticInfo.Longitude, staticInfo.Latitude, staticInfo.Altitude, staticInfo.ViewPort);
            _dynamicTarget = dynamicTarget;
            _shipId = dynamicTarget.Id;
            _tracker = control;
            _tracker.InitTargetTrack(staticInfo);
            //_tracker = new TargetTracker(, staticInfo);
            _disposeEvent.Reset();
            _thread = new Thread(run) { IsBackground = true };
            _thread.Start();
        }

        private void run()
        {
            double curLon = _dynamicTarget.Lon;
            double curLat = _dynamicTarget.Lat;
            DateTime start = DateTime.Now;
            do
            {
                double dis = _dynamicTarget.Sog / 60 / 3600 * (DateTime.Now - start).TotalSeconds;
                curLon = _dynamicTarget.Lon + dis * Math.Sin(_dynamicTarget.Cog * Math.PI / 180) / Math.Cos(_dynamicTarget.Lat * Math.PI / 180);
                curLat = _dynamicTarget.Lat + dis * Math.Cos(_dynamicTarget.Cog * Math.PI / 180);
                DynamicTarget target = new DynamicTarget(_shipId, "0", "联动跟踪船舶", curLon, curLat, _dynamicTarget.Sog, _dynamicTarget.Cog, _dynamicTarget.Heading, _dynamicTarget.Length, _dynamicTarget.Width, _dynamicTarget.MeasureA, _dynamicTarget.MeasureC, DateTime.Now);
                _tracker.TrackToDynamicTarget(target);
                ExpPtz = _ptzObtainer.GetPTZ(target.Lon, target.Lat, 0, target.Length + 40);
            }
            while (!_disposeEvent.WaitOne(TimeSpan.FromSeconds(0.1)));
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            _thread.Join();
            _tracker.StopTrack();
            //_tracker?.Dispose();
            _tracker = null;
        }
    }
}
