using PTZControlService.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.Hikvision
{
    internal class HikTransSerial: NetDvr.Serial, IDisposable
    {
        ICU03Serial _icu03;
        public int SerialHandle { get; private set; }
        public PTZ PTZPosition { get { return _icu03?.PTZPosition; } }

        public PTZLimit PTZLimits { get { return _icu03?.PtzLimits; } }
        public Action<PTZ> PTZEvent { get; set; }

        public DateTime LatestTime { get; private set; } = DateTime.Now;

        NetDvrDll.SerialDataCallBack _serialDataCallBack;
        public int Channel { get; private set; }

        /// <summary>建立透明通道。</summary>
        /// <param name="serialPort">串口号：1－232串口；2－485串口 </param>
        public HikTransSerial(string hikHost, int port, string user, string pass,int serialPort, byte camId, int channel, double zoomMax)
            :base(hikHost, port, user, pass)
        {
            Channel = channel;
            _serialDataCallBack = onSerialDataCallBack;
            SerialHandle = serialStart(Dvr.Handle, serialPort, _serialDataCallBack, new IntPtr());
            if (SerialHandle < 0)
                throw new CanNotOpenSerialException("建立透明通道失败！");
            _icu03 = new ICU03Serial(camId, zoomMax);
            _icu03.SendBufferEvent += onSendBuffer;
            _icu03.PTZFeedbackEvent += onPTZ;
            _icu03.Start();
            while (_icu03 != null && !_icu03.IsInited)
                System.Threading.Thread.Sleep(10);
            LatestTime = DateTime.Now;
        }

        private void onPTZ(PTZ ptz)
        {
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            _icu03.ToPTZ(pan, tilt, zoom);
        }

        private void onSendBuffer(byte[] buffer)
        {
            SerialSend(buffer);
        }

        #region 透明通道

        /// <summary>
        /// 通过透明通道向设备串口发送数据。
        /// </summary>
        /// <param name="channel"> 使用485串口时有效，从1开始；232串口作为透明通道时该值设置为0 </param>
        /// <param name="buffer">发送数据的缓冲区</param>
        /// <returns>TRUE表示成功，FALSE表示失败。</returns>
        public bool SerialSend(byte[] buffer)
        {
            bool values = false;
            int bufSize = buffer.Length;
            IntPtr ptr = Marshal.AllocHGlobal(bufSize);
            Marshal.Copy(buffer, 0, ptr, bufSize);
            if (Dvr.IsHik)
                values = NetDvrDll.NET_DVR_SerialSend(SerialHandle, Channel, ptr, (uint)(buffer.Length)) != 0;
            else
                values = UsntDvrDll.USNTDVR_SerialSend(SerialHandle, Channel, ptr, (uint)(buffer.Length)) != 0;
            Marshal.FreeHGlobal(ptr);
            if (!values)
                error();
            return values;
        }
        
        private int serialStart(int userId, int serialPort, NetDvrDll.SerialDataCallBack serialDataCallBack, IntPtr dwUser)
        {
            if (Dvr.IsHik)
                return NetDvrDll.NET_DVR_SerialStart(userId, serialPort, serialDataCallBack, dwUser);
            else
                return UsntDvrDll.USNTDVR_SerialStart(userId, serialPort, serialDataCallBack, dwUser);
        }

        private void serialStop()
        {
            if (Dvr.IsHik)
                NetDvrDll.NET_DVR_SerialStop(SerialHandle);
            else
                UsntDvrDll.USNTDVR_SerialStop(SerialHandle);
            SerialHandle = -1;
        }

        private void onSerialDataCallBack(int lSerialHandle, IntPtr pRecvDataBuffer, uint dwBufSize, IntPtr dwUser)
        {
            byte[] buffer = new byte[dwBufSize];
            Marshal.Copy(pRecvDataBuffer, buffer, 0, buffer.Length);
            if (lSerialHandle == SerialHandle)
                _icu03?.OnReceive(buffer);
        }
        #endregion 透明通道

        public override void Dispose()
        {
            _icu03.Dispose();
            _icu03 = null;
            serialStop();
            base.Dispose();
        }
    }
}