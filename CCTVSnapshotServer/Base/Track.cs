using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer
{
    public class EventData
    {
        public ShipData Ship { get; set; }
        public string RuleType { get; set; }
        public int CrossDirection { get; set; }//过线方向, In：1 正向, Out: 2 反向, All：3 双向
    }

    public class ShipData
    {
        public TrackData Track { get; set; }
    }

    public class TrackData
    {
        public string Id { get; set; }
        //public string FID { get; set; }
        public string Geometry { get; set; }
        public DateTime Time { get; set; }
        public double Sog { get; set; }
        public double Cog { get; set; }
        public int Heading { get; set; }
        public string MMSI { get; set; }
        public string Name { get; set; }
        //public string[] Origins { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int MeasureA { get; set; }
        public int MeasureC { get; set; }
        public string[] Origins { get; set; }
    }
}
