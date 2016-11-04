using PTZControlService.Protocol.ScBewator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    /// <summary>
    /// Bewator TRX15D解码板解析（视酷定制）
    /// </summary>
    public class ScBewatorSerial : DeviceBaseSerial
    {
        PelcoD _pelcoD;
        ScBewatorBuffer _buffer = new ScBewatorBuffer();
        ManualResetEvent _feedbackControl = new ManualResetEvent(false);
        ManualResetEvent _recvEvent = new ManualResetEvent(false);

        public ScBewatorSerial(byte serialId, double zoomMax) :
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
                if (recv.Length > 0)
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
            ScBewatorMessage scMsg = msg as ScBewatorMessage;
            if (scMsg.SynchByte == 0xFF)//Pelco-D Expand
                decodeExpand(scMsg);
            else if (scMsg.SynchByte == 0xEE)//私有协议
                decodeProprietary(scMsg);
            if (!IsInited && _ptzConverter.IsInited)
                IsInited = _ptzConverter.IsInited;
        }

        void decodeExpand(ScBewatorMessage msg)
        {
            ushort data1 = getShort(msg.Params, 0);
            switch (msg.Command)
            {
                case 0x000F:
                    Console.WriteLine("开始自检！");
                    break;
                case 0x0059:
                    _ptzConverter.CurPan = data1;
                    break;
                case 0x005B:
                    _ptzConverter.CurTilt = data1;
                    break;
                case 0x005D:
                    _ptzConverter.CurZoom = data1;
                    break;
                case 0x005F:
                    _ptzConverter.CurFocus = data1;
                    break;
                case 0x0079:
                    _ptzConverter.NorthPan = data1;
                    Console.WriteLine("NorthPan {0}", data1);
                    break;
                case 0x007B:
                    _ptzConverter.HorizontalTilt = data1;
                    Console.WriteLine("HorizontalTilt {0}", data1);
                    break;
                case 0x007D:
                    _ptzConverter.DegreePan = (double)data1 / 100;
                    Console.WriteLine("_ptzConverter.DegreePan {0}", _ptzConverter.DegreePan);
                    break;
                case 0x007F:
                    _ptzConverter.DegreeTilt = (double)data1 / 100;
                    Console.WriteLine("_ptzConverter.DegreeTilt {0}", _ptzConverter.DegreeTilt);
                    break;
                case 0x0097:// 回传水平，垂直功能
                case 0x0099://回传水平，垂直，变倍，聚焦功能
                    Console.WriteLine($"回传时间间隔 {msg.Command} {data1}");
                    break;
            }
        }

        void decodeProprietary(ScBewatorMessage msg)
        {
            ushort data1 = getShort(msg.Params, 0);
            ushort data2 = getShort(msg.Params, 2);
            switch (msg.Command)
            {
                case 0x0000:
                    //msg.Params = 0x00000001 云台自检
                    break;
                case 0x0300://云台位置连续回传
                    _ptzConverter.CurPan = data1;
                    _ptzConverter.CurTilt = data2;
                    break;
                case 0x0500://镜头位置连续回传
                    _ptzConverter.CurZoom = data1;
                    _ptzConverter.CurFocus = data2;
                    if(IsInited)
                        onPTZEvent(_ptzConverter.UpdatePTZ());
                    break;
                case 0x0700://云台水平限位角度回传
                    _ptzConverter.LeftLimit = data1;
                    _ptzConverter.RightLimit = data2;

                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0900://云台垂直限位角度回传
                    _ptzConverter.UpLimit = data1;
                    _ptzConverter.DownLimit = data2;
                    _ptzConverter.TiltReverse = data1 > data2;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0B00://镜头变倍限位值回传
                    _ptzConverter.MinZoom = data1;
                    _ptzConverter.MaxZoom = data2;
                    _ptzConverter.UpdateLimit();
                    break;
                case 0x0D00://镜头聚焦限位值回传
                    _ptzConverter.MinFocus = data1;
                    _ptzConverter.MaxFocus = data2;
                    _ptzConverter.UpdateLimit();
                    break;
            }
        }

        #endregion 接收并解析反馈指令

        #region ToPTZ

        public override void ToPTZ(double pan, double tilt, double zoom)
        {
            ushort uPan = _ptzConverter.ToSerialPan(pan);
            ptzControl(0x0059, uPan);
            ushort uTilt = _ptzConverter.ToSerialTilt(tilt);
            ptzControl(0x005B, uTilt);
            if (zoom != 0)
            {
                ushort uZoom = _ptzConverter.ToSerialZoom(zoom);
                Console.WriteLine($"Zoom {zoom} => {uZoom}");
                ptzControl(0x005D, uZoom);
            }
        }

        void ptzControl(ushort command, ushort data)
        {
            byte[] buffer = _pelcoD.GetBuffer((byte)(command >> 8), (byte)command, (byte)(data >> 8), (byte)data);
            sendBuffer(buffer);
        }

        #endregion ToPTZ

        private static ushort getShort(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        protected override void prepareValues()//获取限位等信息
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            DateTime begin = DateTime.Now;
            while (!_ptzConverter.IsInitPTZLimit && !_disposeEvent.WaitOne(50))
            {
                if (DateTime.Now - begin > timeout)
                    throw new InvalidOperationException("获取参数超时！");
                if (_ptzConverter.NorthPan == -1)
                    ptzControl(0x0071, 0x0000);//查询水平零点值协议
                else if (_ptzConverter.HorizontalTilt == -1)
                    ptzControl(0x0073, 0x0000);//查询垂直零点值协议 
                else if (_ptzConverter.DegreePan == 0)
                    ptzControl(0x0075, 0x0000);//查询水平校准值协议 
                else if (_ptzConverter.DegreeTilt == 0)
                    ptzControl(0x0077, 0x0000);//查询垂直校准值协议
                else if (_ptzConverter.LeftLimit == -1 || _ptzConverter.RightLimit == -1)
                    ptzControl(0x0061, 0x0000);//发送云台水平限位位置返回请求
                else if (_ptzConverter.UpLimit == -1 || _ptzConverter.DownLimit == -1)
                    ptzControl(0x0063, 0x0000);//发送云台垂直限位位置返回请求
                else if (_ptzConverter.MinZoom == -1 || _ptzConverter.MaxZoom == -1)
                    ptzControl(0x0065, 0x0000);//发送镜头变倍限位位置返回请求
                else if (_ptzConverter.MaxFocus == -1 || _ptzConverter.MinFocus == -1)
                    ptzControl(0x0067, 0x0000);//发送镜头聚焦限位位置返回请求
                else
                {
                    _ptzConverter.UpdateLimit();
                    if (PtzLimits != null)
                        IsInited = true;
                    Console.WriteLine("IsInited = {0}", IsInited);
                }
            }
        }

        protected override void updateFeedbackMsg(bool enable)
        {
            ptzControl(0x0099, 0x0000);//结束反馈
            Thread.Sleep(50);
            if (enable)
                ptzControl(0x0099, 0x0050);//反馈水平，垂直，变倍，聚焦功能,间隔时间，40毫秒
            else
                ptzControl(0x0099, 0x0000);//结束反馈
            Thread.Sleep(50);
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
