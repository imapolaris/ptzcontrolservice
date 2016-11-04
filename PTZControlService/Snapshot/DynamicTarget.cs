using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class DynamicTarget
    {
        public string Id { get;  set; }
        public string MMSI { get; set; }
        public string Name { get; set; }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public double Sog { get; set; }
        public double Cog { get; set; }
        public int Heading { get; set; }            //船首向
        public int Length { get; set; }
        public int Width { get;  set; }
        public int MeasureA { get; set; }    //AIS到船首的距离
        public int MeasureC { get; set; }    //AIS到左侧的距离
        public DateTime Time { get; set; }

        public int CrossDirection { get; set; }

        public DynamicTarget()
        {
        }

        public DynamicTarget(string id, string mmsi, string name, double lon, double lat, double sog, double cog, int heading, int length, int width, int measureA, int measureC, DateTime time,int crossDirection = 0)
        {
            Id = id;
            MMSI = mmsi;
            Name = name;
            Lon = lon;
            Lat = lat;
            Sog = sog;
            Cog = cog;
            Heading = heading;
            Length = length;
            Width = width;
            MeasureA = measureA;
            MeasureC = measureC;
            Time = time;
            CrossDirection = crossDirection;
        }

        public void Update(DynamicTarget target)
        {
            if (Id?.ToLower() != target.Id?.ToLower())
                return;
            Name = target.Name;
            Lon = target.Lon;
            Lat = target.Lat;
            Sog = target.Sog;
            Cog = target.Cog;
            Heading = target.Heading;
            Length = target.Length;
            Width = target.Width;
            MeasureA = target.MeasureA;
            MeasureC = target.MeasureC;
            Time = target.Time;
            CrossDirection = target.CrossDirection;
        }
    }
}