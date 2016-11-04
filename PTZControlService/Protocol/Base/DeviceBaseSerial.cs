using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    public abstract class DeviceBaseSerial : IDisposable
    {
        protected byte _addressId { get; private set; }
        public bool IsInited { get; protected set; }
        protected ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public Action<PTZ> PTZFeedbackEvent;
        public Action<byte[]> SendBufferEvent;

        public PTZLimit PtzLimits { get { return _ptzConverter.UpdateLimit(); } }
        public PTZ PTZPosition { get; private set; }
        public DateTime LatestTime { get; private set; }

        protected Queue<byte[]> _received = new Queue<byte[]>();
        protected AutoResetEvent OnDataEvent = new AutoResetEvent(false);
        protected PTZFeedbackConverter _ptzConverter;

        public DeviceBaseSerial(byte addressId, double zoomMax = 37)
        {
            _addressId = addressId;
            _ptzConverter = new PTZFeedbackConverter(zoomMax);
            LatestTime = DateTime.Now;
        }

        public void Start()
        {
            _disposeEvent.Reset();
            new Thread(runStaticInfo).Start();
        }

        public void OnReceive(byte[] buffer)
        {
            lock (_received)
                _received.Enqueue(buffer);
            OnDataEvent.Set();
        }
        
        private void runStaticInfo()
        {
            updateFeedbackMsg(false);
            prepareValues();
            runCurrentValues();
        }

        public abstract void ToPTZ(double pan, double tilt, double zoom);
        protected abstract void updateFeedbackMsg(bool enable);
        protected abstract void prepareValues();
        protected abstract void stopMovePTZ();
        protected virtual void updateCurPTZ()
        { }

        private void runCurrentValues()
        {
            updateFeedbackMsg(true);
            updateCurPTZ();
        }
        
        protected void onPTZEvent(PTZ ptz)
        {
            if (ptz == null)
                return;
            PTZPosition = ptz;
            var handler = PTZFeedbackEvent;
            if (handler != null)
                handler(ptz);
        }

        protected void sendBuffer(byte[] buffer)
        {
            Console.WriteLine($"Send Buffer {BitConverter.ToString(buffer)}\t{DateTime.Now.TimeOfDay}");
            var handler = SendBufferEvent;
            if (handler != null)
                handler(buffer);
        }
        
        protected byte[] receive()
        {
            byte[][] packets;
            lock (_received)
            {
                packets = _received.ToArray();
                _received.Clear();
            }
            if (packets != null && packets.Length > 0)
                LatestTime = DateTime.Now;

            using (MemoryStream ms = new MemoryStream())
            {
                foreach (byte[] packet in packets)
                    ms.Write(packet, 0, packet.Length);
                return ms.ToArray();
            }
        }

        public virtual void Dispose()
        {
            _disposeEvent.Set();
            updateFeedbackMsg(false);
            Thread.Sleep(100);
            stopMovePTZ();
            Thread.Sleep(100);
            stopMovePTZ();
        }
    }
}
