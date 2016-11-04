using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BewatorPTZTest
{
    public class TcpBewatorSerialComm : IDisposable
    {
        string _host;
        int _port;
        TcpClient _client;
        Thread _thread;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public DateTime LatestTime { get; private set; }
        public TcpBewatorSerialComm(string host, int port, byte camId)
        {
            _host = host;
            _port = port;
            _disposeEvent.Reset();
            _thread = new Thread(run);
            _thread.Start();
            LatestTime = DateTime.Now;
        }

        public void Send(byte[] data)
        {
            _client.Client.Send(data);
            Prompt.Instance.OnSend($"Send {data.Length}: {BitConverter.ToString(data)}");
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
                            byte[] recvedBytes = buffer.Take(recv).ToArray();
                            //_icu03.OnReceive(recvedBytes);
                            Prompt.Instance.OnReceived($"Receive {recv}: " + BitConverter.ToString(recvedBytes));
                            LatestTime = DateTime.Now;
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
                Prompt.Instance.OnStatus($"连接 {_host}:{_port}状态：{_client.Connected}");
            }
            catch(Exception ex)
            {
                Prompt.Instance.OnStatus($"连接 {_host}:{_port}错误 {ex.Message}");
                Logger.Default.Error("tcp connect Server error!");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            do
            {
                try
                {
                    if(_client != null)
                    {
                        _client.Close();
                        Prompt.Instance.OnStatus($"连接 {_host}:{_port}已断开！");
                    }
                    _client = null;
                }
                catch
                {
                }
            }
            while (!_thread.Join(1000));
        }
    }
}
