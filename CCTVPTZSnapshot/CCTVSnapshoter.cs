using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVPTZSnapshot
{
    //public class CCTVSnapshoter
    //{
    //    ControlWithSnapShot _snapshot;
    //    PTZObtainer _ptzObtainer;
    //    IControl _control;
    //    public CCTVSnapshoter(IControl control, ICCTVStaticInfo staticInfo)
    //    {
    //        _control = control;
    //        _snapshot = new ControlWithSnapShot(control, ConfigSettings.ShotshopPath);
    //        _ptzObtainer = new PTZObtainer(staticInfo.Lon, staticInfo.Lat, staticInfo.Alt, staticInfo.Fov);
    //    }

    //    public double Snapshot(string shipName, double lon, double lat, double alt, double sog, double cog, double width)
    //    {
    //        Point3d pt = _ptzObtainer.GetPoint(lon, lat, alt);
    //        PTZ ptz = getPTZ(pt, width);
    //        double seconds = RotationTimeConsuming.GetConsumingSeconds(_control.PTZPosition, ptz);
    //        PTZ[] ptzs = getPTZsFromExp(pt, sog, cog, width, seconds);
    //        _snapshot.Snapshot(shipName, ptzs);
    //        return seconds + ptzs.Length;
    //    }

    //    private PTZ[] getPTZsFromExp(Point3d pt, double sog, double cog, double width, double seconds)
    //    {
    //        List<PTZ> ptzs = new List<PTZ>();
    //        double speed = sog * 1852.0 / 3600;
    //        double sin = Math.Sin(cog * Math.PI / 180);
    //        double cos = Math.Cos(cog * Math.PI / 180);
    //        double speedX = speed * sin;
    //        double speedY = speed * cos;
    //        Point3d ptDef = new Point3d(pt.X + speedX * seconds, pt.Y + speedY * seconds, pt.Z);
    //        //船首
    //        Point3d ptBow = new Point3d()
    //        {
    //            X = ptDef.X + width / 2 * sin,
    //            Y = ptDef.Y + width / 2 * cos,
    //            Z = ptDef.Z,
    //        };
    //        ptzs.Add(getPTZ(ptBow, width * 2 / 3));
    //        //船尾
    //        Point3d ptPoop = new Point3d()
    //        {
    //            X = ptDef.X - width / 4 * sin + speedX,
    //            Y = ptDef.Y - width / 4 * cos + speedY,
    //            Z = ptDef.Z,
    //        };
    //        ptzs.Add(getPTZ(ptPoop, width * 2 / 3));
    //        //船身
    //        Point3d ptHull = new Point3d(ptDef.X + speedX * 2, ptDef.Y + speedY * 2, ptDef.Z);
    //        ptzs.Add(getPTZ(ptHull, width * 2));
    //        return ptzs.ToArray();
    //    }

    //    PTZ getPTZ(Point3d pt, double width)
    //    {
    //        PTZ ptz = _ptzObtainer.GetPTZFromXY(pt.X, pt.Y, pt.Z, width);
    //        ptz = updatePTZFromLimit(ptz);
    //        return ptz;
    //    }

    //    private PTZ updatePTZFromLimit(PTZ ptz)
    //    {
    //        PTZLimit limits = _control.PTZLimits;
    //        double pan = ptz.Pan;
    //        double tilt = ptz.Tilt;
    //        double zoom = PTZConverter.ToValid(ptz.Zoom, 1, limits.ZoomMax);
    //        PTZConverter.ToValidAngle(pan, limits.Left, limits.Right);
    //        return ptz;
    //    }
    //}
}
