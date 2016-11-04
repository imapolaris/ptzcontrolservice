using AopUtil.WpfBinding;
using CCTVModels;
using Common.Log;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BewatorPTZTest.ScBewator
{
    public class TcpScBewatorSerial: ObservableObject
    {
        string _host;
        int _port;
        TcpClient _client;
        Thread _thread;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        [AutoNotify]
        public ScBewatorSerial Serial { get; set; }
        public TcpScBewatorSerial(string host, int port = 4001, byte serialId = 1, SerialType serialType = SerialType.Bewator, double zoomMax = 37)
        {
            _host = host;
            _port = port;
            _disposeEvent.Reset();
            _thread = new Thread(run);
            _thread.Start();
            Thread.Sleep(50);
            initAndStartSerial(serialId, serialType, zoomMax);
        }
        
        private void initAndStartSerial(byte serialId, SerialType serialType, double zoomMax)
        {
            if (serialType == SerialType.Bewator)
                Serial = new ScBewatorSerial(serialId, zoomMax);
            else
                throw new InvalidSettingException("无效的控制类型: " + serialType);
            Serial.SendBufferEvent += onSendBuffer;
            Serial.Start();
            //while (Serial != null && !Serial.IsInited)
            //    Thread.Sleep(10);
        }
        
        public void ToPTZ(double pan, double tilt, double zoom)
        {
            Serial.ToPTZ(pan, tilt, zoom);
        }

        public void Send(byte[] data)
        {
            try
            {
                Prompt.Instance.OnSend($"Send {BitConverter.ToString(data)}\t{DateTime.Now.TimeOfDay}");
                _client.Client.Send(data);
                //Console.WriteLine(DateTime.Now.TimeOfDay + " Send: " + BitConverter.ToString(data)+ " - "+ data.Length);
            }
            catch(Exception ex)
            {
                Prompt.Instance.OnStatus($"Send Error {BitConverter.ToString(data)}\n {ex}");
            }
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            Serial.CameraControl(action, actData);
        }

        private void onSendBuffer(byte[] buffer)
        {
            Send(buffer);
        }

        void run()
        {
            byte[] buffer = new byte[1024000];
            do
            {
                if (connectServer())
                {
                    while (!_disposeEvent.WaitOne(0))
                    {
                        int recv = 0;
                        try
                        {
                            recv = _client.Client.Receive(buffer);
                        }
                        catch(Exception ex)
                        {
                            if(!_disposeEvent.WaitOne(0))
                                Prompt.Instance.OnStatus($"接收数据错误\n {ex}");
                        }

                        if (recv > 0)
                        {
                            IEnumerable<byte> recvedBytes = buffer.Take(recv);
                            Serial?.OnReceive(recvedBytes.ToArray());
                        }
                        else
                            break;
                    }
                }
            }
            while (!_disposeEvent.WaitOne(10000));
        }

        private bool connectServer()
        {
            _client = new TcpClient();
            try
            {
                _client.Connect(_host, _port);
                Prompt.Instance.OnStatus($"TCP已连接: {_host}:{_port}");
            }
            catch(Exception ex)
            {
                Logger.Default.Error("tcp connect Server error!");
                Prompt.Instance.OnStatus($"连接 {_host}:{_port}错误 {ex.Message}");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            Serial?.Dispose();
            Serial = null;
            do
            {
                try
                {
                    _client.Close();
                    Prompt.Instance.OnStatus($"连接 {_host}:{_port}已断开！");
                }
                catch
                {
                }
            }
            while (!_thread.Join(1000));
        }
    }
}
