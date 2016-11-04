using PTZControlService.Protocol.Bewator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    public class BewatorSerial: DeviceBaseSerial
    {
        PelcoD _pelcoD;
        BewatorBuffer _buffer = new BewatorBuffer();
        ManualResetEvent _feedbackControl = new ManualResetEvent(false);
        bool _feedbackStatus = false;
        ManualResetEvent _recvEvent = new ManualResetEvent(false);
        
        public BewatorSerial(byte serialId, double zoomMax = 37) :
            base(serialId, zoomMax)
        {
            _pelcoD = new PelcoD(serialId, false);
            _recvEvent.Reset();
            new Thread(recvRun) { IsBackground = true }.Start();
        }

        #region 接收并解析反馈指令
        private void recvRun()
        {
            while (!_recvEvent.WaitOne(1))
            {
                byte[] recv = receive();
                if(recv.Length > 0)
                {
                    _buffer.Add(recv);
                    Message msg;
                    while (_buffer.ParseMessage(out msg))
                    {
                        updateMsg(msg);
                    }
                }
            }
        }

        private void updateMsg(Message msg)
        {
            ushort data1 = getShort(msg.Params, 0);
            ushort data2 = msg.Params.Length >= 4? getShort(msg.Params, 2): (ushort)0xffff;
            switch (msg.Command)
            {
                case 0x0081:
                    _ptzConverter.CurPan = data1;
                    break;
                case 0x0181:
                    _ptzConverter.LeftLimit = data1;
                    break;
                case 0x0281:
                    _ptzConverter.RightLimit = data1;
                    break;
                case 0x0381:
                    _ptzConverter.LeftLimit = data1;
                    _ptzConverter.RightLimit = data2;
                    break;
                case 0x0083:
                    _ptzConverter.CurTilt = data1;
                    break;
                case 0x0183:
                    _ptzConverter.UpLimit = data1;
                    break;
                case 0x0283:
                    _ptzConverter.DownLimit = data1;
                    break;
                case 0x0383:
                    _ptzConverter.UpLimit = data1;
                    _ptzConverter.DownLimit = data1;
                    break;
                case 0x0085:
                    _ptzConverter.CurZoom = data1;
                    onPTZEvent(_ptzConverter.UpdatePTZ());
                    break;
                case 0x0185:
                    _ptzConverter.MaxZoom = data1;
                    break;
                case 0x0285:
                    _ptzConverter.MinZoom = data1;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0385:
                    _ptzConverter.MaxZoom = data1;
                    _ptzConverter.MinZoom = data2;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0087:
                    _ptzConverter.CurFocus = data1;
                    break;
                case 0x0187:
                    _ptzConverter.MaxFocus = data1;
                    break;
                case 0x0287:
                    _ptzConverter.MinFocus = data1;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0387:
                    _ptzConverter.MaxFocus = data1;
                    _ptzConverter.MinFocus = data2;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0089:
                    _ptzConverter.CurIris = data1;
                    break;
                case 0x0189:
                    _ptzConverter.MaxIris = data1;
                    break;
                case 0x0289:
                    _ptzConverter.MinIris = data1;
                    break;
                case 0x0389:
                    _ptzConverter.MaxIris = data1;
                    _ptzConverter.MinIris = data2;
                    break;
            }
            if (!IsInited && _ptzConverter.IsInited)
                IsInited = _ptzConverter.IsInited;
        }
        #endregion 接收并解析反馈指令

        #region ToPTZ

        public override void ToPTZ(double pan, double tilt, double zoom)
        {
            ushort uPan = _ptzConverter.ToSerialPan(pan);
            ptzControl(0x0081, uPan);
            ushort uTilt = _ptzConverter.ToSerialTilt(tilt);
            ptzControl(0x0083, uTilt);
            if (zoom != 0)
            {
                ushort uZoom = _ptzConverter.ToSerialZoom(zoom);
                Console.WriteLine($"Zoom {zoom} => {uZoom}");
                ptzControl(0x0085, uZoom);
            }
        }

        void ptzControl(ushort command, ushort data)
        {
            byte[] buffer = _pelcoD.GetBuffer((byte)(command >> 8), (byte)command, (byte)(data >> 8), (byte)data);
            sendBuffer(buffer);
            //Console.WriteLine($"Send Buffer {BitConverter.ToString(buffer)}");
        }

        #endregion ToPTZ

        private static ushort getShort(byte[] data, int offset)
        {
            return (ushort)(((data[offset] << 8) | data[offset + 1]) & 0xffff);
        }

        protected override void prepareValues()//获取限位等信息
        {
            //预留数据，暂无接口
            _ptzConverter.DegreePan = 17.8;
            _ptzConverter.DegreeTilt = 17.8;
            _ptzConverter.NorthPan = 4867;
            _ptzConverter.HorizontalTilt = 3811;

            while (!_ptzConverter.IsInitPTZLimit && !_disposeEvent.WaitOne(1))
            {
                ptzControl(0x0003, 0x00FA);//发送云台镜头限位位置返回请求
                Thread.Sleep(500);
                if (PtzLimits != null)
                    IsInited = true;
            }
        }

        protected override void updateFeedbackMsg(bool enable)
        {
            _feedbackStatus = enable;
        }

        protected override void updateCurPTZ()
        {
            //if(_feedbackStatus)
            //{
            //    ptzControl(0x0003,0x00fd);
            //    Thread.Sleep(30);
            //    ptzControl(0x0003,0x00fc);
            //}
        }

        protected override void stopMovePTZ()
        {
            ptzControl(0x0000, 0x0000);//停命令
        }

        public override void Dispose()
        {
            _feedbackControl.Set();
            _recvEvent.Set();
            base.Dispose();
        }
    }
}
