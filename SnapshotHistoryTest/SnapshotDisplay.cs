using SnapshotHistoryTest.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotHistoryTest
{
    public class SnapshotDisplay
    {
        SnapshotInfo _info;
        public string ID{ get { return _info.Ship.Id; } }
        public string MMSI { get { return _info.Ship.MMSI; } }
        public DateTime Time { get { return _info.SnapshotTime; } }
        public string Name { get { return _info.Ship.Name; } }
        public double Lon { get { return Math.Round(_info.Ship.Lon,6); } }
        public double Lat { get { return Math.Round(_info.Ship.Lat, 6); } }
        public double Sog { get { return Math.Round(_info.Ship.Sog, 1); } }
        public double Cog { get { return Math.Round(_info.Ship.Cog); } }
        public double Length { get { return _info.Ship.Length; } }
        public double Width { get { return _info.Ship.Width; } }
        public int Direction { get { return _info.Ship.CrossDirection; } }
        public SnapshotDisplay(SnapshotInfo info)
        {
            _info = info;
        }
    }
}
