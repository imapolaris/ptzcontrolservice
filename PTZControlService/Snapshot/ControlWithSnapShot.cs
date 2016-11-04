using Common.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class ControlWithSnapShot: IDisposable
    {
        public string Id { get; private set; }
        IControl _control;
        PTZLimit _limits;
        PTZ[] _expPtzs;
        int _expIndex = 0;
        DateTime _startTime = DateTime.MinValue;
        DateTime _arrivalTime = DateTime.MaxValue;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        string _fileName;
        string _path;
        TimeSpan _timeoutSpan;
        object _obj = new object();
        double _waitSeconds = 0;
        double _panSpeed;
        public double DefaultFocusSeconds { get; set; } = 2;
        public bool Snapshotting { get { return _expPtzs != null; } }

        public Action<TargetSnapshotInfo> SnapshotEvent;
        public ControlWithSnapShot(IControl control, string path)
        {
            _timeoutSpan = TimeSpan.FromSeconds(SnapshotStaticInfo.TimeoutSeconds);
            _control = control;
            _path = path;
            _disposeEvent.Reset();
            new Thread(run) { IsBackground = true }.Start();
        }

        private void run()
        {
            while (!_disposeEvent.WaitOne(50))
            {
                lock(_obj)
                {
                    if (_expPtzs != null && _expPtzs.Length > _expIndex)
                    {
                        if (_startTime.Add(_timeoutSpan) >= DateTime.Now)
                            checkAndSnapshot();
                        else//超时
                        {
                            Common.Log.Logger.Default.Warn("抓拍目标超时，停止抓拍!");
                            _expPtzs = null;
                            onSnapshot(Path.Combine(_path, _fileName), DateTime.Now, false, "抓拍目标超时，停止抓拍!");
                        }
                    }
                }
            }
        }

        private void checkAndSnapshot()
        {
            try
            {
                if (_arrivalTime != DateTime.MaxValue)
                {
                    if (isArrivedPTZ())
                    {
                        if ((DateTime.Now - _arrivalTime).TotalSeconds > _waitSeconds)
                        {
                            DateTime time = DateTime.Now;
                            string fileName = Path.Combine(_path, SnapshotNameGenerate.GetJpgName(_fileName, time));
                            bool isSecceeded = Snapshot(fileName);
                            onSnapshot(fileName, time, isSecceeded);
                            tryMoveToNext();
                        }
                    }
                    else
                        _arrivalTime = DateTime.MaxValue;
                }
                else if (isArrivedPTZ())
                    _arrivalTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Default.Error(ex);
                _expPtzs = null;
                onSnapshot(Path.Combine(_path, _fileName), DateTime.Now, false, "错误：" + ex.Message);
            }
        }

        private void tryMoveToNext()
        {
            _expIndex++;
            if (_expIndex >= _expPtzs.Length)
            {
                _expPtzs = null;
                _expIndex = 0;
            }
            else
                moveToIndex();
        }

        private void moveToIndex()
        {
            try
            {
                if (_expPtzs != null && _expIndex < _expPtzs.Length)
                {
                    updateExpPTZ();
                    PTZ ptz = _expPtzs[_expIndex];
                    checkPTZ(ptz);
                    _waitSeconds = PTZManager.GetNeedWaitSeconds(_control.PTZPosition.Zoom, ptz.Zoom, DefaultFocusSeconds);
                    _control.ToPTZ(ptz.Pan, ptz.Tilt, ptz.Zoom);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Error("抓拍失败，尝试抓拍下一张！\n" + ex.ToString());
                onSnapshot(Path.Combine(_path, _fileName), DateTime.Now, false, "抓拍失败:" + ex.Message);
                tryMoveToNext();
            }
        }
        
        private void updateExpPTZ()
        {
            if (_panSpeed != 0)
            {
                PTZ ptz = _expPtzs[_expIndex];
                double secondOut = Math.Round((DateTime.Now - _startTime).TotalSeconds, 1);
                double secondMove = RotationTimeConsuming.GetConsumingSeconds(_control.PTZPosition, ptz);
                double secondFocus = PTZManager.GetNeedWaitSeconds(_control.PTZPosition.Zoom, ptz.Zoom, DefaultFocusSeconds);
                double panChanged = Math.Round(_panSpeed * (secondOut + secondMove + secondFocus), 1);
                double pan = PTZConverter.ToValidAngle(ptz.Pan + panChanged, _limits.Left, _limits.Right);
                Logger.Default.Info($"开始第 {_expIndex + 1} 抓拍 预估耗时：{secondOut} + {secondMove} + {secondFocus}\t预估转角：{panChanged} PTZ ({_control.PTZPosition.Pan},{_control.PTZPosition.Tilt},{_control.PTZPosition.Zoom})--> ({pan},{ptz.Tilt},{ptz.Zoom})");
                _expPtzs[_expIndex] = new PTZ(pan, ptz.Tilt, ptz.Zoom);
            }
        }

        bool isArrivedPTZ()
        {
            return neerPTZ(_expPtzs[_expIndex], _control.PTZPosition);
        }

        bool neerPTZ(PTZ ptz1, PTZ ptz2)
        {
            return Math.Abs(ptz1.Pan - ptz2.Pan) < 0.2 &&
                   Math.Abs(ptz1.Tilt - ptz2.Tilt) < 0.2 &&
                   Math.Abs(ptz1.Zoom - ptz2.Zoom) < 0.3;
        }

        bool Snapshot(string fileName)
        {
            return _control.CaptureJPEGPicture(fileName);
        }

        public void Snapshot(string id, string fileName, PTZ[] ptzs, double panSpeed, DateTime startTime)
        {
            if (DateTime.Now > startTime.Add(_timeoutSpan))
            {
                Logger.Default.Error("目标时间超时或错误，不进行抓拍!{0}", startTime);
                return;
            }
            checkBase();
            lock (_obj)
            {
                _panSpeed = panSpeed;
                if(Id == null || !Id.Equals(id) || (_expPtzs != null && _expPtzs.Length != ptzs.Length))
                    loadPTZ(ptzs, startTime);
                else if(_expPtzs != null)//此时 _expPtzs.Length == ptzs.Length
                    updatePTZ(ptzs, startTime);
                Id = id;
                _fileName = fileName;
            }
        }

        void checkBase()
        {
            _limits = _control?.PTZLimits;
            if (_limits == null)
                throw new CanNotControlExpection("获取限位值失败，无法获取快照！");
            if (_control?.PTZPosition == null)
                throw new CanNotControlExpection("获取云镜位置信息失败，无法获取快照！");
        }

        void checkPTZ(PTZ ptz)
        {
            if (!PTZConverter.IsValidAngle(ptz.Pan, _limits.Left, _limits.Right))
                throw new CanNotControlExpection($"Pan值不在有效区间内,无法获取快照! {ptz.Pan} ~ [{_limits.Left}, {_limits.Right}]");
            if (!PTZConverter.IsValid(ptz.Tilt, _limits.Up, _limits.Down))
                throw new CanNotControlExpection($"Tilt值不在有效区间内,无法获取快照! {ptz.Tilt} ~ [{_limits.Up}, {_limits.Down}]");
            if (!PTZConverter.IsValid(ptz.Zoom, 1, _limits.ZoomMax))
                throw new CanNotControlExpection($"Zoom值不在有效区间内,无法获取快照! {ptz.Zoom} ~ [1, {_limits.ZoomMax}]");
        }

        public void MoveTo(double pan, double tilt, double zoom)
        {
            _control.ToPTZ(pan, tilt, zoom);
        }

        private void loadPTZ(PTZ[] ptzs, DateTime startTime)
        {
            _startTime = startTime;
            _arrivalTime = DateTime.MaxValue;
            _expPtzs = ptzs;
            _expIndex = 0;
            moveToIndex();
        }

        /// <summary>更新抓拍点位置</summary>
        private void updatePTZ(PTZ[] ptzs, DateTime startTime)
        {
            _startTime = startTime;
            PTZ cur = _expPtzs[_expIndex];
            _expPtzs = ptzs;
            if (isArrivedPTZ())//若已转到，不更新当前抓拍点
                _expPtzs[_expIndex] = cur;
            moveToIndex();
        }

        private void onSnapshot(string fileName, DateTime snapshotTime, bool isSecceeded, string prompt = null)
        {
            var handle = SnapshotEvent;
            if (handle != null)
                handle(new TargetSnapshotInfo() { Id = this.Id, IsSecceeded = isSecceeded, FileName = fileName, Time = snapshotTime, Prompt = prompt });
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            _control = null;
        }

        public class TargetSnapshotInfo
        {
            public string Id { get; set; }
            public bool IsSecceeded { get; set; }
            public string FileName { get; set; }
            public string Prompt { get; set; }
            public DateTime Time { get; set; }
        }
    }
}