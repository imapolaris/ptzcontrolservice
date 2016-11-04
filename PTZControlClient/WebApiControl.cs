using Common.Util.Serialization;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace PTZControlClient
{
    public class WebApiControl : IControl, IDisposable
    {
        string _api = null;
        string _videoId;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public WebApiControl(string ip, int port, string videoId)
        {
            _videoId = videoId;
            _api = $"http://{ip}:{port}/api";
            _disposeEvent.Reset();
            new Thread(run) { IsBackground = true }.Start();
        }

        private void run()
        {
            while (!_disposeEvent.WaitOne(1000))
            {
                {
                    string limitsUri = _api + @"/GetCCTVStaticInfos";
                    dooGet(limitsUri, "CCTVStaticInfo");
                }

                if (PTZLimits == null)
                {
                    string limitsUri = _api + @"/ptzlimits/" + _videoId;
                    dooGet(limitsUri, "PTZLimit");
                }
                string posUri = _api + @"/ptz/" + _videoId;
                dooGet(posUri, "PTZ");
            }
        }

        public PTZLimit PTZLimits { get; private set; }

        public PTZ PTZPosition { get; private set; }
        public Action<PTZ> PTZEvent { get; set; }

        public DateTime LatestTime { get { return DateTime.Now; } }

        public void CameraControl(CameraAction action, byte actData)
        {
            string uri = _api + @"/ptzcontrol/control?videoid=" + _videoId + "&action=" + (int)action + "&data=" + actData;
            dooGet(uri);
        }

        public bool CaptureJPEGPicture(string fileName, ushort wPicSize = 2)
        {
            string uri = _api + @"/snapshot/" + _videoId;
            dooGet(uri);
            return false;
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            string uri = _api + @"/ptzcontrol/toptz?videoid=" + _videoId + "&pan=" + pan + "&tilt=" + tilt +"&zoom=" + zoom;
            dooGet(uri);
        }

        public void ToGeometry(double lon, double lat, double alt, double width)
        {
            string uri = _api + @"/ptzcontrol/togeometry?" +"videoid="+ _videoId
                + "&lon=" + lon + "&lat=" + lat + "&alt=" + alt + "width=" + width;
        }

        public void Dispose()
        {
            _disposeEvent.Set();
        }

        private void dooGet(string url, string valueType = null)
        {
            try
            {
                //创建HttpClient（注意传入HttpClientHandler）
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                    //await异步等待回应
                    var response = http.GetAsync(url).Result;
                    //确保HTTP成功状态值
                    response.EnsureSuccessStatusCode();
                    updateJsonStream(response.Content.ReadAsStreamAsync(), valueType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void updateJsonStream(Task<Stream> task,string valueType)
        {
            if (valueType == null)
                return;
            Stream stream = task.Result;
            JsonSerializer js = new JsonSerializer();
            switch (valueType)
            {
                case nameof(PTZLimit):
                    PTZLimits = js.Deserialize<PTZLimit>(stream);
                    break;
                case nameof(PTZ):
                    onPTZ(js.Deserialize<PTZ>(stream));
                    break;
                default:
                    stream.Position = 0;
                    Console.WriteLine(new StreamReader(stream).ReadToEnd());
                    break;
            }
        }

        private void onPTZ(PTZ ptz)
        {
            PTZPosition = ptz;
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }
    }
}
