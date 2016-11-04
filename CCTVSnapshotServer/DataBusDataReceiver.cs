using Adapter.Proto;
using PTZControlService;
using Seecool.DataBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CCTVSnapshotServer
{
    //DataBus接收数据，用于联动跟踪抓拍的实时性控制
    class DataBusDataReceiver:IDisposable
    {
        private Consumer _consumer;
        private string _endpoint;
        private string[] _topics;
        private CancellationTokenSource _cts;
        public Action<DynamicTarget> DynamicEvent;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public DataBusDataReceiver(string endpoint="tcp://127.0.0.1:62626",string topics="ScUnion")
        {
            _endpoint = endpoint;
            _topics = topics.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            _disposeEvent.Reset();
            startup();
        }
        private void startup()
        {
            _cts = new CancellationTokenSource();
            _consumer = new Consumer(_endpoint, _topics);
            //Task.Factory.StartNew(onReceive, _cts.Token);
            new Thread(onReceive) { IsBackground = true }.Start();
        }

        private void onReceive()
        {
            //while (!_cts.IsCancellationRequested)
            while(!_disposeEvent.WaitOne(0))
            {
                try
                {
                    Seecool.DataBus.Message msg;
                    _consumer.TryReceiveMessage(out msg);
                    handleData(msg);
                }
                catch (ZmqException err)
                {
                    Console.WriteLine(err.ToString());
                    Thread.Sleep(10 * 1000);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        private void handleData(Seecool.DataBus.Message msg)
        {
            if (msg.Data == null || msg.Data.Length == 0)
                return;
            using (MemoryStream ms = new MemoryStream(msg.Data))
            {
                ScUnion union = ProtoBuf.Serializer.Deserialize<ScUnion>(ms);
                onData(union);
            }
        }

        private void onData(ScUnion union)
        {
            string id = "SCUNION.." + union.ID;
            DynamicTarget target = new DynamicTarget(id, union.MMSI.ToString(), union.Name, union.Longitude, union.Latitude, union.SOG, union.COG, (int)union.TrueHeading, union.Length, union.Width, union.MeasureA, union.MeasureC, new DateTime(union.DynamicTime).ToLocalTime(), 0);
            onDynamic(target);
        }

        void onDynamic(DynamicTarget target)
        {
            Console.WriteLine($"DataBus {target.Id} Length: {target.Length} Width:{target.Width}");
            var handler = DynamicEvent;
            if (handler != null)
                handler(target);
        }

        private void shutdown()
        {
            _cts?.Cancel();
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            shutdown();
        }
    }
}