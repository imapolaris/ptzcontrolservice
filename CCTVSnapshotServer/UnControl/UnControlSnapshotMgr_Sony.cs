using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCTVSnapshotServer.Util;
using PTZControlService;

namespace CCTVSnapshotServer.UnControl
{
    public class UnControlSnapshotMgr_Sony : ISnapshotMgr, IDisposable
    {
        CameraConnector _cameraConnector = null;
        UnControlSnapshotTracksMgr _tracks = null;
        string _path = null;

        public UnControlSnapshotMgr_Sony(string path, string activeMqUri, string topic, string ruleType)
        {
            _cameraConnector = new CameraConnector();

            _tracks = new UnControlSnapshotTracksMgr(activeMqUri, topic, ruleType);
            _tracks.SnapshotEvent += snapshot;

            _path = path;
        }

        DateTime _lastSnapshotTime = DateTime.MinValue;
        DynamicTarget _lastSnapshotTarget = null;
        void snapshot(DynamicTarget target)
        {
            bool isStopSnapshot;
            if (isRepeatTarget(target, out isStopSnapshot))
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
                isSucceeded = _cameraConnector.Shoot(fileName);
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error($"名称：{target.Name}, Id:{target.Id}， 文件存储路径:{fileName}\r\n" + ex.ToString());
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
                if (DateTime.Now - _lastSnapshotTime < TimeSpan.FromSeconds(4) && target.CrossDirection == lastTarget.CrossDirection)
                {
                    isStopSnapshot = true;
                    if (target.Length > 0 && lastTarget.Length == 0 || target.Length == 0 && lastTarget.Length > 0)
                    {
                        double length = Math.Max(target.Length, lastTarget.Length);
                        double dist = Util.Calculator.CalcDis(target.Lon, target.Lat, lastTarget.Lon, lastTarget.Lat);
                        if (dist < length)
                            return true;
                    }
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            _cameraConnector.Dispose();
        }
    }
}
