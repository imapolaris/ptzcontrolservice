namespace PTZControlService
{
    public interface ICCTVStaticInfo
    {
        double Lon { get; }
        double Lat { get; }
        double Alt { get; }
        double Fov { get; }
    }

    public class CameraStaticInfo: ICCTVStaticInfo
    {
        public double Lon { get; private set; }  //经度
        public double Lat { get; private set; }  //纬度
        public double Alt { get; private set; }  //高度
        public double Fov { get; private set; }  //视场角
        public CameraStaticInfo(double lon, double lat, double alt, double fov)
        {
            Lon = lon;
            Lat = lat;
            Alt = alt;
            Fov = fov;
        }
    }
}