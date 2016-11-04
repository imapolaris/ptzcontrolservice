using CCTVModels;
using CCTVSnapshotServer.UnControl;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CCTVSnapshotServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ISnapshotMgr _snapshot;
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Common.Log.Logger.Default.Trace("-------------------------启动抓拍任务-----------------------------");
            string ip = appSettings("Ip");
            int port = int.Parse(appSettings("Port"));
            string userName = appSettings("UserName");
            string password = appSettings("Password");
            int channel = int.Parse(appSettings("Channel"));
            CCTVControlType cameraType = (CCTVControlType)Enum.Parse(typeof(CCTVControlType), appSettings("CameraType"));
            CCTVControlConfig controlConfig = new CCTVControlConfig()
            {
                Type = cameraType,
                Ip = ip,
                Port = port,
                UserName = userName,
                Password = password,
                Channel = channel,
            };

            string path = appSettings("SavePath");

            double longitude = double.Parse(appSettings("Longitude"));
            double latitude = double.Parse(appSettings("Latitude"));
            double altitude = double.Parse(appSettings("Altitude"));
            double fov = double.Parse(appSettings("FOV"));

            string activeMqUri = appSettings("EventActiveMQUri");
            string topic = appSettings("EventActiveMQTopic", "RITSV2.Event");
            string ruleType = appSettings("RuleType", "Enter_Region");
            
            SnapshotStaticInfo.DistanceMax = double.Parse(appSettings("DistanceMax", "3000"));
            SnapshotStaticInfo.DistanceMin = double.Parse(appSettings("DistanceMin", "10"));
            SnapshotStaticInfo.DefaultLength = double.Parse(appSettings("DefaultLength", "30"));
            SnapshotStaticInfo.TimeoutSeconds = double.Parse(appSettings("TimeoutSeconds", "20"));
            CCTVStaticInfo staticInfo = new CCTVStaticInfo()
            {
                Longitude = longitude,
                Latitude = latitude,
                Altitude = altitude,
                ViewPort = fov,
            };
            CameraStaticInfo camera = new CameraStaticInfo(longitude, latitude, altitude, fov);
            
            if (controlConfig.Type == CCTVControlType.DVR)
            {
                string databusEndpoint = appSettings("DataBusEndpoint");
                string dataBusTopic = appSettings("DataBusTopic", "ScUnion");
                _snapshot = new PTZControlSnapshotMgr(staticInfo, controlConfig, path, activeMqUri, topic, ruleType, databusEndpoint, dataBusTopic);
            }
            else if (controlConfig.Type == CCTVControlType.UnControl)
                _snapshot = new UnControlSnapshotMgr(staticInfo, controlConfig, path, activeMqUri, topic, ruleType);
            else
                throw new InvalidOperationException("不支持的抓拍类型：" + controlConfig.Type);
        }

        string appSettings(string setting)
        {
            try
            {
                return ConfigurationManager.AppSettings[setting];
            }
            catch
            {
                throw new InvalidOperationException($"App.xaml读取参数“{setting}”出错！");
            }
        }
        
        string appSettings(string setting, string defaultString)
        {
            string app = defaultString;
            try
            {
                app = appSettings(setting);
            }
            catch(InvalidOperationException ex)
            {
                Common.Log.Logger.Default.Warn(ex.Message + "取值为默认参数：" + defaultString);
            }
            return app;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _snapshot?.Dispose();
            Common.Log.Logger.Default.Trace("-------------------------退出抓拍任务-----------------------------");
        }
    }
}
