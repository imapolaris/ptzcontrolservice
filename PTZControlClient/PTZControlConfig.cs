using AopUtil.WpfBinding;
using CCTVModels;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlClient
{
    public class PTZControlConfig: ObservableObject
    {
        public CCTVControlConfig PtzConfig   {get; set;}
        public CCTVControlConfig TransPtzConfig { get; set; }
        public CCTVControlConfig TcpPtzConfig { get; set; }
        public WebApiConfig WebConfig { get; set; }
        [AutoNotify]
        public PTZMode Selected { get; set; }
        public PTZControlConfig()
        {
            PtzConfig = new CCTVControlConfig()
            {
                Type = CCTVControlType.DVR
            };
            TransPtzConfig = new CCTVControlConfig()
            {
                Type = CCTVControlType.TransDVR
            } ;
            TcpPtzConfig = new CCTVControlConfig()
            {
                Type = CCTVControlType.TCP
            };
            WebConfig = new WebApiConfig();
            Selected = PTZMode.普通云台;
        }

        public IControl GetNewControl()
        {
            switch(Selected)
            {
                case PTZMode.WebApi:
                    return new WebApiControl(WebConfig.Ip, WebConfig.Port, WebConfig.VideoId);
                case PTZMode.普通云台:
                    return new PTZControl(PtzConfig);
                case PTZMode.透明通道:
                    return new PTZControl(TransPtzConfig);
                case PTZMode.串口服务器:
                    return new PTZControl(TcpPtzConfig, 32);
                default:
                throw new CanNotControlExpection("未找到对应的控制类型");
            }
        }
    }
}
