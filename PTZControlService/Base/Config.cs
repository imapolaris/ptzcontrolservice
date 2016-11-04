using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    //public class PTZConfig: IPTZControlConfig
    //{
    //    public string Ip { get; set; } = "192.168.9.155";
    //    public int Port { get; set; } = 8000;
    //    public string UserName { get; set; } = "admin";
    //    public string Password { get; set; }= "admin12345";
    //    public int Channel { get; set; } = 1;
    //}

    //public class TransPTZConfig : IPTZControlConfig
    //{
    //    public string Ip { get; set; } = "192.168.9.251";
    //    public int Port { get; set; } = 8000;
    //    public string UserName { get; set; } = "admin";
    //    public string Password { get; set; } = "admin12345";
    //    public int Channel { get; set; } = 0;
    //    public byte CamId { get; set; } = 1;
    //    public int SerialPort { get; set; } = 1;//串口号：1－232串口；2－485串口
    //    public bool ReverseZoom { get; set; } = false;
    //}

    //public class TCPPTZConfig : IPTZControlConfig
    //{
    //    public string Ip { get; set; } = "192.168.9.56";
    //    public int Port { get; set; } = 4001;
    //    public byte CamId { get; set; } = 1;
    //    public bool ReverseZoom { get; set; } = false;
    //}

    public class WebApiConfig: IPTZControlConfig
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8888;
        public string VideoId { get; set; }
    }
}
