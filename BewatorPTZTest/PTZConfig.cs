namespace BewatorPTZTest
{
    public class PTZConfig
    {
        public string Ip { get; set; } = "192.168.9.56";
        public int Port { get; set; } = 4001;
        public byte CameraId { get; set; } = 1;
        public bool ReverseZoom { get; set; } = false;
        public int Channel { get; set; }
    }
}