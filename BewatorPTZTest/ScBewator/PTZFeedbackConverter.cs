using AopUtil.WpfBinding;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BewatorPTZTest.ScBewator
{
    public class PTZFeedbackConverter: ObservableObject
    {
        [AutoNotify]
        public int LeftLimit { get; set; } = -1;
        [AutoNotify]
        public int RightLimit { get; set; } = -1;
        [AutoNotify]
        public int UpLimit { get; set; } = -1;
        [AutoNotify]
        public int DownLimit { get; set; } = -1;
        [AutoNotify]
        public int MinZoom { get; set; } = -1;
        [AutoNotify]
        public int MaxZoom { get; set; } = -1;
        [AutoNotify]
        public int MinFocus { get; set; } = -1;
        [AutoNotify]
        public int MaxFocus { get; set; } = -1;
        [AutoNotify]
        public int MinIris { get; set; }
        [AutoNotify]
        public int MaxIris { get; set; }
        [AutoNotify]
        public int NorthPan { get; set; } = -1;
        [AutoNotify]
        public double DegreePan { get; set; } = 0;
        [AutoNotify]
        public int HorizontalTilt { get; set; } = -1;
        [AutoNotify]
        public double DegreeTilt { get; set; } = 0;
        [AutoNotify]
        public int CurPan { get; set; } = -1;
        [AutoNotify]
        public int CurTilt { get; set; } = -1;
        [AutoNotify]
        public int CurZoom { get; set; } = -1;
        [AutoNotify]
        public int CurFocus { get; set; } = -1;
        [AutoNotify]
        public int CurIris { get; set; } = -1;
        
        /// <summary>解码器中PTZ对应的值与实际角度或变倍值的转换</summary>
        /// <param name="zoomMax">镜头最大变倍</param>
        public PTZFeedbackConverter(double zoomMax)
        {
            _zoomMax = zoomMax;
        }

        public bool IsInited
        {
            get
            {
                return IsInitPTZLimit
                  && NorthPan != -1 && DegreePan != 0 && HorizontalTilt != -1 && DegreeTilt != 0
                  && IsInitPTZ;
            }
        }

        public bool IsInitPTZLimit
        {
            get
            {
                return LeftLimit != -1 && RightLimit != -1 && UpLimit != -1 && DownLimit != -1
                  && MinZoom != -1 && MaxZoom != -1;
            }
        }

        public bool IsInitPTZ
        {
            get
            {
                return CurPan != -1 && CurTilt != -1 && CurZoom != -1;
            }
        }

        double _zoomMax = 30;
        [AutoNotify]
        public PTZLimit PtzLimits { get; private set; }
        [AutoNotify]
        public PTZ PtzPosition { get; set; }

        public void UpdateZoomMax(double zoomMax)
        {
            _zoomMax = zoomMax;
            if (PtzLimits != null)
                UpdateLimit();
        }

        public ushort ToSerialPan(double anglePan)
        {
            int iPan = (int)Math.Round(anglePan * DegreePan + NorthPan);
            while (iPan > DegreePan * 360 + LeftLimit)
                iPan -= (int)Math.Round(DegreePan * 360);
            return (ushort)iPan;
        }

        public ushort ToSerialTilt(double tilt)
        {
            int iTilt = (int)Math.Round(tilt * DegreeTilt * getSign(TiltReverse) + HorizontalTilt);
            while (iTilt < 0)
                iTilt += (int)Math.Round(DegreeTilt * 360);
            return (ushort)iTilt;
        }

        double _zoomScale = 98.5;
        double _zoomDif = 320;
        double _zoomInf = -0.625;

        public ushort ToSerialZoom(double zoom)
        {
            zoom = PTZConverter.ToValid(zoom, 1, PtzLimits.ZoomMax);
            double dZoom = Math.Log(zoom + _zoomInf, 2) * _zoomScale + _zoomDif;
            ushort uZoom = (ushort)Math.Round(PTZConverter.ToValid(dZoom, MinZoom, MaxZoom));
            return uZoom;
        }

        public double ToRealPan(int pan)
        {
            double angle = (pan - NorthPan) / DegreePan;
            if (angle < 0)
                angle += 360;
            return Math.Round(angle, 1);
        }

        public double ToRealTilt(int tilt)
        {
            double angle = (tilt - HorizontalTilt) / DegreeTilt * getSign(TiltReverse);
            return Math.Round(angle, 1);
        }

        public double ToRealZoom(int zoom)
        {
            double zoomIndex = Math.Pow(2, (zoom - _zoomDif) / _zoomScale) - _zoomInf;
            zoomIndex = Math.Max(1, Math.Min(PtzLimits.ZoomMax, zoomIndex));
            if (zoomIndex > PtzLimits.ZoomMax - 0.5)
                zoomIndex = PtzLimits.ZoomMax;
            return Math.Round(zoomIndex, 1);
        }

        public PTZLimit UpdateLimit()
        {
            if (!IsInitPTZLimit)
                return null;
            double leftLimit = ToRealPan(LeftLimit);
            double rightLimit = ToRealPan(RightLimit);
            double upLimit = ToRealTilt(UpLimit);
            double downLimit = ToRealTilt(DownLimit);
            PtzLimits = new PTZLimit(leftLimit, rightLimit, upLimit, downLimit, _zoomMax);
            _zoomScale = (MaxZoom - MinZoom) / (Math.Log(_zoomMax + _zoomInf, 2) - Math.Log(1.0 + _zoomInf, 2));
            _zoomDif = MinZoom - Math.Log(1.0 + _zoomInf, 2) * _zoomScale;
            Console.WriteLine($"PTZLimit Data L:{LeftLimit}, R:{RightLimit}, U:{UpLimit},D:{DownLimit},Zx:{MaxZoom},Zi:{MinZoom},Fx:{MaxFocus}, Fi:{MinFocus}");
            Console.WriteLine($"PTZLimit Angle L:{PtzLimits.Left}, R:{PtzLimits.Right}, U:{PtzLimits.Up},D:{PtzLimits.Down},Z:{PtzLimits.ZoomMax}");
            return PtzLimits;
        }

        public PTZ UpdatePTZ()
        {
            if (!IsInited)
                return null;
            double pan = ToRealPan(CurPan);
            double tilt = ToRealTilt(CurTilt);
            double zoom = ToRealZoom(CurZoom);
            //Console.WriteLine($"Pan {pan}, Tilt: {tilt}, Zoom: {zoom} [{CurPan},{CurTilt},{CurZoom},{CurFocus}]\t{DateTime.Now.TimeOfDay}");
            PtzPosition = new PTZ(pan, tilt, zoom);
            return PtzPosition;
        }

        public bool TiltReverse = false;

        int getSign(bool isReverse)
        {
            return isReverse ? -1 : 1;
        }
    }
}
