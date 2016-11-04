using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCTVPTZSnapshot
{
    //public class ControlWithSnapShot: IDisposable
    //{
    //    IControl _control;
    //    PTZLimit _limits;
    //    PTZ[] _expPtzs;
    //    int _expIndex = 0;
    //    DateTime _startTime = DateTime.MinValue;
    //    DateTime _arrivalTime = DateTime.MaxValue;
    //    ManualResetEvent _disposeEvent = new ManualResetEvent(false);
    //    string _fileName;
    //    string _path;
    //    static TimeSpan _timeoutSpan = TimeSpan.FromSeconds(15);
    //    object _obj = new object();
    //    double _waitSeconds = 0;
    //    public ControlWithSnapShot(IControl control, string path)
    //    {
    //        _control = control;
    //        _path = path;
    //        _disposeEvent.Reset();
    //        new System.IO.DirectoryInfo(path).Create();
    //        new Thread(run) { IsBackground = true }.Start();
    //    }

    //    private void run()
    //    {
    //        while (!_disposeEvent.WaitOne(50))
    //        {
    //            lock(_obj)
    //            {
    //                if (_expPtzs != null && _expPtzs.Length > _expIndex)
    //                {
    //                    if (_startTime.Add(_timeoutSpan) >= DateTime.Now)
    //                        checkAndSnapshot();
    //                    else
    //                    {
    //                        Console.WriteLine("超时！{0}", DateTime.Now.TimeOfDay);
    //                        _expPtzs = null;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private void checkAndSnapshot()
    //    {
    //        try
    //        {
    //            if (_arrivalTime != DateTime.MaxValue)
    //            {
    //                if (isArrivedPTZ())
    //                {
    //                    if ((DateTime.Now - _arrivalTime).TotalSeconds > _waitSeconds)
    //                    {
    //                        string fileName = System.IO.Path.Combine(_path, string.Format("{0}{1}.jpg", _fileName, DateTime.Now.ToString("yyyyMMddHHmmssfff")));
    //                        Snapshot(fileName);
    //                        finishCurExp();
    //                    }
    //                }
    //                else
    //                    _arrivalTime = DateTime.MaxValue;
    //            }
    //            else if (isArrivedPTZ())
    //                _arrivalTime = DateTime.Now;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //            _expPtzs = null;
    //        }
    //    }

    //    private void finishCurExp()
    //    {
    //        _expIndex++;
    //        if (_expIndex >= _expPtzs.Length)
    //        {
    //            _expPtzs = null;
    //            _expIndex = 0;
    //            Console.WriteLine("完成！{0}, 用时： {1}", DateTime.Now.TimeOfDay, DateTime.Now - _startTime);
    //        }
    //        else
    //            moveToIndex();
    //    }

    //    bool isArrivedPTZ()
    //    {
    //        return Math.Abs(_expPtzs[_expIndex].Pan - _control.PTZPosition.Pan) < 0.1 &&
    //            Math.Abs(_expPtzs[_expIndex].Tilt - _control.PTZPosition.Tilt) < 0.1 &&
    //            Math.Abs(_expPtzs[_expIndex].Zoom - _control.PTZPosition.Zoom) < 0.1;
    //    }

    //    public void Snapshot(string fileName)
    //    {
    //        _control.CaptureJPEGPicture(fileName);
    //    }

    //    public void Snapshot(string fileName, double pan, double tilt, double zoom)
    //    {
    //        _limits = _control?.PTZLimits;
    //        if (_limits == null)
    //            throw new FailControlExpection("获取限位值失败，无法获取快照！");
    //        if (_control?.PTZPosition == null)
    //            throw new FailControlExpection("获取云镜位置信息失败，无法获取快照！");
    //        pan = Math.Round(pan, 1);
    //        tilt = Math.Round(tilt, 1);
    //        zoom = Math.Round(zoom, 1);
    //        if (!PTZConverter.IsValidAngle(pan, _limits.Left, _limits.Right))
    //            throw new FailControlExpection("Pan值不在有效区间内,无法获取快照!");
    //        if (!PTZConverter.IsValid(tilt, _limits.Up, _limits.Down))
    //            throw new FailControlExpection("Tilt值不在有效区间内,无法获取快照!");
    //        if(!PTZConverter.IsValid(zoom, 1, _limits.ZoomMax))
    //            throw new FailControlExpection("Zoom值不在有效区间内,无法获取快照!");
    //        Snapshot(fileName, new PTZ[] { new PTZ(pan, tilt, zoom) });
    //    }

    //    public void Snapshot(string fileName, PTZ[] ptzs)
    //    {
    //        lock (_obj)
    //        {
    //            _expPtzs = ptzs;
    //            _expIndex = 0;
    //            _fileName = fileName;
    //            _startTime = DateTime.Now;
    //            _arrivalTime = DateTime.MaxValue;
    //            moveToIndex();
    //        }
    //    }

    //    private void moveToIndex()
    //    {
    //        if (_expPtzs != null && _expIndex < _expPtzs.Length)
    //        {
    //            PTZ ptz = _expPtzs[_expIndex];
    //            updateWaitTime(ptz.Zoom);
    //            _control.ToPTZ(ptz.Pan, ptz.Tilt, ptz.Zoom);
    //            Console.WriteLine("时间！{0} - {1} - {2}", DateTime.Now.TimeOfDay, _fileName, _expIndex);
    //        }
    //    }

    //    void updateWaitTime(double zoom)
    //    {
    //        if (zoom < 10 || zoom <= _control.PTZPosition.Zoom)
    //            _waitSeconds = 0.5;
    //        else
    //            _waitSeconds = Math.Min(3, Math.Max(0.5, (zoom - _control.PTZPosition.Zoom) / 5));
    //    }

    //    public void Dispose()
    //    {
    //        _disposeEvent.Set();
    //        _control = null;
    //    }
    //}
}
