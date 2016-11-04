using CCTVModels;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.UnControl
{
    public class UnControlSnapshotMgr : ISnapshotMgr, IDisposable
    {
        UnControlSnapshotTracksMgr _tracks;
        PTZControlService.Hikvision.NetDvr.Serial _serial;
        //DataBusDataReceiver _databus;
        int _channel;
        public UnControlSnapshotMgr(CCTVStaticInfo staticInfo, CCTVControlConfig config, string path, string activeMqUri, string topic, string ruleType)
        {
            _serial = new PTZControlService.Hikvision.NetDvr.Serial(config.Ip, config.Port, config.UserName, config.Password);
            _channel = config.Channel;
            //_snapshoter = new CCTVSnapshoter(staticInfo, null, path);
            _tracks = new UnControlSnapshotTracksMgr(activeMqUri, topic, ruleType);
            _tracks.SnapshotEvent += snapshot;
            //_databus = new DataBusDataReceiver(databusEndpoint, dataBusTopic);
            //_databus.DynamicEvent += databusDynamic;
        }

        //private void databusDynamic(DynamicTarget target)
        //{
        //    _tracks.UpdateTarget(target);
        //}
        DateTime _lastSnapshotTime = DateTime.MinValue;
        DynamicTarget _lastSnapshotTarget = null;
        void snapshot(DynamicTarget target)
        {
            bool isStopSnapshot;
            if (isRepeatTarget(target,out isStopSnapshot))
            {
                Common.Log.Logger.Default.Trace($"目标融合：{target.Id} - {_lastSnapshotTarget.Id}");
                Base.SnapshotHistoryJson.Instance.Mux(target.Id, _lastSnapshotTarget.Id);
            }
            if (isStopSnapshot)
            {
                Common.Log.Logger.Default.Trace($"名称：{target.Name}, Id:{target.Id}，抓图频繁，取消当前抓图项");
                return;
            }
            DateTime snapshotTime = DateTime.Now;
            string fileName = SnapshotNameGenerate.GetFullName(target, snapshotTime);
            bool isSucceeded = false;
            try
            {
                isSucceeded = _serial.CaptureJPEGPicture(_channel, fileName, 2);
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error($"名称：{target.Name}, Id:{target.Id}， 文件存储路径:{fileName}\r\n" +ex.ToString());
            }
            //添加抓拍纪录文档
            Base.SnapshotHistoryJson.Instance.Add(new Base.SnapshotInfo() { Ship = target, FileName = fileName, IsSucceeded = isSucceeded, SnapshotTime = snapshotTime });
            _lastSnapshotTarget = target;
            _lastSnapshotTime = snapshotTime;
        }

        private bool isRepeatTarget(DynamicTarget target, out bool isStopSnapshot)
        {
            isStopSnapshot = false;
            var lastTarget = _lastSnapshotTarget;
            if (lastTarget == null)
                return false;
            if (target.Id == lastTarget.Id)
            {
                if (DateTime.Now - _lastSnapshotTime < TimeSpan.FromSeconds(3))
                    isStopSnapshot = true;
                return false;
            }
            else
            {
                if(DateTime.Now - _lastSnapshotTime < TimeSpan.FromSeconds(4) && target.CrossDirection == lastTarget.CrossDirection)
                {
                    isStopSnapshot = true;
                    if(target.Length > 0 && lastTarget.Length == 0 || target.Length == 0 && lastTarget.Length > 0)
                    {
                        double length = Math.Max(target.Length, lastTarget.Length);
                        double dist = Util.Calculator.CalcDis(target.Lon, target.Lat, lastTarget.Lon, lastTarget.Lat);
                        if(dist < length)
                            return true;
                    }
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            if(_tracks != null)
                _tracks.Dispose();
            _tracks = null;
            _serial.Dispose();
            //_databus?.Dispose();
        }
    }
}
