using CCTVModels;
using Common.Log;
using PTZControlService.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService
{
    internal class TcpSerialComm: IDisposable
    {
        string _host;
        int _port;
        DeviceBaseSerial _serial;
        public PTZ PTZPosition { get { return _serial?.PTZPosition; } }
        public PTZLimit PTZLimits { get { return _serial?.PtzLimits; } }
        public DateTime LatestTime { get { return _serial?.LatestTime ?? DateTime.MinValue; } }

        public Action<PTZ> PTZEvent { get; set; }

        TcpClient _client;
        Thread _thread;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        #region Init
        public TcpSerialComm(string host, int port, byte serialId, SerialType serialType, double zoomMax)
        {
            _host = host;
            _port = port;
            _disposeEvent.Reset();
            _thread = new Thread(run);
            _thread.Start();
            initAndStartSerial(serialId, serialType, zoomMax);
        }

        private void initAndStartSerial(byte serialId, SerialType serialType, double zoomMax)
        {
            initSerial(serialId, serialType, zoomMax);
            checkValidInited();
        }

        private void initSerial(byte serialId, SerialType serialType, double zoomMax)
        {
            if (serialType == SerialType.ICU03)
                _serial = new ICU03Serial(serialId, zoomMax);
            else if (serialType == SerialType.Bewator)
                _serial = new ScBewatorSerial(serialId, zoomMax);
            else
                throw new InvalidSettingException("无效的控制类型: " + serialType);
            _serial.SendBufferEvent += onSendBuffer;
            _serial.PTZFeedbackEvent += onPTZ;
            _serial.Start();
        }

        private void checkValidInited()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            DateTime start = DateTime.Now;
            while (_serial != null && !_serial.IsInited)
            {
                if (DateTime.Now - start > timeout)
                {
                    Dispose();
                    throw new InvalidOperationException("获取云镜信息超时！");
                }
                Thread.Sleep(1000);
            }
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
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
                        catch
                        {
                        }

                        if (recv > 0)
                        {
                            IEnumerable<byte> recvedBytes = buffer.Take(recv);
                            _serial.OnReceive(recvedBytes.ToArray());
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
            var client = new TcpClient();
            try
            {
                client.Connect(_host, _port);
                client.Client.ReceiveTimeout = 1000;
                client.Client.SendTimeout = 3000;
                _client = client;
            }
            catch
            {
                Logger.Default.Error("tcp connect Server error!");
                return false;
            }
            return true;
        }

        #endregion Init

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            _serial.ToPTZ(pan, tilt, zoom);
        }

        public void Send(byte[] data)
        {
            try
            {
                _client.Client.Send(data);
                //Console.WriteLine(DateTime.Now.TimeOfDay + " Send: " + BitConverter.ToString(data)+ " - "+ data.Length);
            }
            catch(Exception ex)
            {
                Logger.Default.Error($"send buffer {BitConverter.ToString(data)} error!",ex);
            }
        }
        
        public void Dispose()
        {
            _disposeEvent.Set();
            _serial?.Dispose();
            _serial = null;
            do
            {
                try
                {
                    _client.Close();
                }
                catch
                {
                }
            }
            while (!_thread.Join(1000));
        }
    }
}