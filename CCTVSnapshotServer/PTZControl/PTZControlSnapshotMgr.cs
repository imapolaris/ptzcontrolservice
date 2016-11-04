using CCTVModels;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CCTVSnapshotServer
{
    public class PTZControlSnapshotMgr: ISnapshotMgr, IDisposable
    {
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        PTZControl _control;
        CCTVControlConfig _config;
        CCTVSnapshoter _snapshoter;
        SnapshotTracksMgr _tracks;
        DataBusDataReceiver _databus;
        CCTVStaticInfo _staticInfo;
        string _path;
        public PTZControlSnapshotMgr(CCTVStaticInfo staticInfo, CCTVControlConfig config, string path, string activeMqUri, string topic, string ruleType, string databusEndpoint, string dataBusTopic)
        {
            _disposeEvent.Reset();
            _config = config;
            _staticInfo = staticInfo;
            _path = path;
            _tracks = new SnapshotTracksMgr(activeMqUri, topic, ruleType);
            new Thread(run) { IsBackground = true }.Start();
            _databus = new DataBusDataReceiver(databusEndpoint, dataBusTopic);
            _databus.DynamicEvent += databusDynamic;
        }

        private void run()
        {
            while (!_disposeEvent.WaitOne(100))
            {
                try
                {
                    if (_snapshoter == null || !_snapshoter.Snapshotting)
                    {
                        DynamicTarget target = _tracks.Dequeue();
                        if (target != null)
                        {
                            initControl();      //抓取新目标时尝试创建云台控制
                            snapshot(target);
                        }
                        else
                            disposeControl();   //抓取完成后释放云台控制
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.Logger.Default.Error(ex);
                }
            }
        }

        private void databusDynamic(DynamicTarget target)
        {
            if (_snapshoter != null && _snapshoter.LastDynamicTarget != null && _snapshoter.Snapshotting && _snapshoter.LastDynamicTarget.Id.ToUpper() == target.Id.ToUpper())
            {
                snapshot(target);
                return;
            }
            _tracks.UpdateTarget(target);
        }

        void snapshot(DynamicTarget target)
        {
            updateQuality();
            _snapshoter.Snapshot(target);
        }

        private void updateQuality()
        {
            SnapshotModel quality = _tracks.GetModel();
            if (quality != _snapshoter.QualityModel)
            {
                Common.Log.Logger.Default.Info("抓拍模式变更， {0}  --> {1}", _snapshoter.QualityModel, quality);
                _snapshoter.QualityModel = quality;
            }
        }

        TimeSpan ConnectTimeOut = TimeSpan.FromSeconds(30);
        private void initControl()
        {
            if (_control != null)
                return;
            _control = new PTZControl(_config);
            _snapshoter = new CCTVSnapshoter(_staticInfo, _control, _path);
            _snapshoter.SnapshotEvent += onSnapshot;
            DateTime start = DateTime.Now;
            while (!_disposeEvent.WaitOne(10))//确保有数据反馈后再进行云镜控制
            {
                if (_control.PTZLimits != null && _control.PTZPosition != null)
                    break;
                if (DateTime.Now > start.Add(ConnectTimeOut))
                    throw new CanNotControlExpection("获取云台反馈信息超时（30s）");
            }
            Common.Log.Logger.Default.Info("----------------开始抓拍-----------------");
        }

        private void onSnapshot(DynamicTarget target, string fileName, DateTime snapshotTime, bool isSecceeded, string prompt)
        {
            Base.SnapshotHistoryJson.Instance.Add(new Base.SnapshotInfo()
            {
                FileName = fileName,
                Ship = target,
                IsSucceeded = isSecceeded,
                Prompt = prompt,
                SnapshotTime = snapshotTime
            });
        }

        private void disposeControl()
        {
            if(_snapshoter != null)
            {
                _snapshoter.SnapshotEvent -= onSnapshot;
                _snapshoter.Dispose();
            }
            _snapshoter = null;
            if(_control != null)
                _control.Dispose();
            _control = null;
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            _tracks?.Dispose();
            disposeControl();
            _databus.Dispose();
        }
    }
}