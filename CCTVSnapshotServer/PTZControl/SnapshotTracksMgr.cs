using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer
{
    public class SnapshotTracksMgr: IDisposable
    {
        Queue<DynamicTarget> _queue = new Queue<DynamicTarget>();
        ActiveMQDataProvider _mqdataProvider;

        public SnapshotTracksMgr(string activeMqUri, string topic, string ruleType)
        {
            _mqdataProvider = new ActiveMQDataProvider(activeMqUri, topic, ruleType);
            _mqdataProvider.DynamicEvent += onDynamic;
        }

        public DynamicTarget Dequeue()
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
            }
            return null;
        }

        public void UpdateTarget(DynamicTarget target)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    DynamicTarget dt = _queue.FirstOrDefault(_ => _.Id.ToUpper() == target.Id.ToUpper());
                    if (dt != null)
                    {
                        dt.Update(target);
                        Console.WriteLine("Id: " + target.Id + "  Lon: " + target.Lon + " Lat: " + target.Lat + "  Time: " + target.Time);
                    }
                }
            }
        }

        public SnapshotModel GetModel()
        {
            SnapshotModel quality = SnapshotModel.NormalQuality;
            if (_queue.Count < 3)
                quality = SnapshotModel.HighQuality;
            else if (_queue.Count > 6)
                quality = SnapshotModel.LowQuality;
            return quality;
        }

        private void onDynamic(DynamicTarget target)
        {
            lock (_queue)
            {
                if (!_queue.Any(_ => _.Id == target.Id))
                    _queue.Enqueue(target);
            }
        }

        public void Dispose()
        {
            if(_mqdataProvider != null)
                _mqdataProvider.Dispose();
            _mqdataProvider = null;
        }
    }
}
