using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.UnControl
{
    public class UnControlSnapshotTracksMgr : IDisposable
    {
        ActiveMQDataProvider _mqdataProvider;
        //List<DynamicTarget> _tracks = new List<DynamicTarget>();
        //ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public Action<DynamicTarget> SnapshotEvent;

        public UnControlSnapshotTracksMgr(string activeMqUri, string topic, string ruleType)
        {
            _mqdataProvider = new ActiveMQDataProvider(activeMqUri, topic, ruleType);
            _mqdataProvider.DynamicEvent += onSnapshot;//直接抓拍
            //_mqdataProvider.DynamicEvent += onDynamic;
            //_disposeEvent.Reset();
            //new Thread(run).Start();
        }

        //public void UpdateTarget(DynamicTarget target)
        //{
        //    lock (_tracks)
        //    {
        //        if (_tracks.Count > 0)
        //        {
        //            DynamicTarget dt = _tracks.FirstOrDefault(_ => _.Id.ToUpper() == target.Id.ToUpper());
        //            if (dt != null)
        //            {
        //                dt.Update(target);
        //                Console.WriteLine("Id: " + target.Id + "  Lon: " + target.Lon + " Lat: " + target.Lat + "  Time: " + target.Time);
        //            }
        //        }
        //    }
        //}

        //private void run()
        //{
        //    while (!_disposeEvent.WaitOne(1))
        //    {
        //        lock(_tracks)
        //            snapshotTracks();
        //    }
        //}

        //private void snapshotTracks()
        //{//TODO:未完成
        //    List<DynamicTarget> removed = new List<DynamicTarget>();
        //    foreach (var track in _tracks)
        //    {
        //        removed.Add(track);
        //        onSnapshot(track);
        //    }
        //    foreach(var del in removed)
        //        _tracks.Remove(del);
        //}

        //private void onDynamic(DynamicTarget target)
        //{
        //    lock (_tracks)
        //    {
        //        if (!_tracks.Any(_ => _.Id == target.Id))
        //            _tracks.Add(target);
        //    }
        //}

        private void onSnapshot(DynamicTarget target)
        {
            Common.Log.Logger.Default.Trace($"Id:{target.Id} {target.Name} {Math.Round(target.Lon,6)} {Math.Round(target.Lat, 6)} {target.Length} {target.Width} {target.Heading} {Math.Round(target.Sog,2)} {Math.Round(target.Cog,0)} {target.CrossDirection}");
            var handler = SnapshotEvent;
            if (handler != null)
                handler(target);
        }

        public void Dispose()
        {
            //_disposeEvent.Set();
            if (_mqdataProvider != null)
                _mqdataProvider.Dispose();
            _mqdataProvider = null;
            //lock (_tracks)
            //    _tracks.Clear();
        }
    }
}
