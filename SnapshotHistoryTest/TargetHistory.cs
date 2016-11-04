using SnapshotHistoryTest.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapshotHistoryTest
{
    public class TargetHistory
    {
        SnapshotHistoryInfo _history = new SnapshotHistoryInfo();
        public string Id { get; set; }
        public string 名称 { get; set; }
        public int Count { get { return _history.History.Count; } }
        public DateTime BeginTime { get { return _history.History.First().SnapshotTime; } }
        public DateTime EndTime { get { return _history.History.Last().SnapshotTime; } }
        public string 方向 { get { return _history.History.Count(_=>_.Ship.CrossDirection == 1) > _history.History.Count / 2 ? "正向" : "反向"; } }
        public string TrackLine()
        {
            return getLineString(_history.History.ToArray());
        }

        public SnapshotInfo[] SnapshotInfos()
        {
            return _history.History.ToArray();
        }
        public TargetHistory(SnapshotInfo info)
        {
            Id = info.Ship.Id;
            if (info.Ship.Name != null)
                名称 = info.Ship.Name;
            _history.History.Add(info);
        }

        public void Add(SnapshotInfo info)
        {
            if(info != null && info.Ship.Id == Id)
            {
                if (info.Ship.Name != null)
                    名称 = info.Ship.Name;
                _history.History.Add(info);
                _history.History = _history.History.OrderBy(_ => _.SnapshotTime).ToList();
            }
        }

        private string getLineString(SnapshotInfo[] infos)
        {
            string str = "line";
            foreach (var info in infos)
                str += $",{info.Ship.Lon},{info.Ship.Lat}";
            return str;
        }
    }
}