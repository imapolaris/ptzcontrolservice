using AopUtil.WpfBinding;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BewatorPTZTest.ScBewator
{
    public class ScBewatorSerial : ObservableObject, IDisposable
    {
        PelcoD _pelcoD;
        ScBewatorBuffer _buffer = new ScBewatorBuffer();
        ManualResetEvent _feedbackControl = new ManualResetEvent(false);
        ManualResetEvent _recvEvent = new ManualResetEvent(false);
        protected byte _addressId { get; private set; }
        public bool IsInited { get; protected set; }
        protected ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public Action<PTZ> PTZFeedbackEvent;
        public Action<byte[]> SendBufferEvent;

        public PTZ PTZPosition { get; private set; }
        public DateTime LatestTime { get; private set; }

        protected Queue<byte[]> _received = new Queue<byte[]>();
        [AutoNotify]
        public PTZFeedbackConverter PtzConverter { get; set; }
        public ScBewatorSerial(byte serialId, double zoomMax)
        {
            _pelcoD = new PelcoD(serialId, false, 0x40);
            _recvEvent.Reset();
            new Thread(recvRun) { IsBackground = true }.Start();
            _addressId = serialId;
            PtzConverter = new PTZFeedbackConverter(zoomMax);
            LatestTime = DateTime.Now;
            initCmd();
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
        }

        public void CameraControl(CameraAction action, byte actData)
        {
            sendBuffer(_pelcoD.CamaraControl(action, actData));
        }

        protected void sendBuffer(byte[] buffer)
        {
            Console.WriteLine($"Send Buffer： {BitConverter.ToString(buffer)}\t{DateTime.Now.TimeOfDay}");
            var handler = SendBufferEvent;
            if (handler != null)
                handler(buffer);
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
                    ScBewatorMessage msg;
                    while (_buffer.ParseMessage(out msg))
                    {
                        updateMsg(msg);
                    }
                }
            }
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

        private void updateMsg(ScBewatorMessage msg)
        {
            ScBewatorMessage scMsg = msg as ScBewatorMessage;
            if (scMsg.SynchByte == 0xFF)//Pelco-D Expand
                decodeExpand(scMsg);
            else if (scMsg.SynchByte == 0xEE)//私有协议
                decodeProprietary(scMsg);
            if (!IsInited && PtzConverter.IsInited)
                IsInited = PtzConverter.IsInited;
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
                    PtzConverter.CurPan = data1;
                    break;
                case 0x005B:
                    PtzConverter.CurTilt = data1;
                    break;
                case 0x005D:
                    PtzConverter.CurZoom = data1;
                    break;
                case 0x005F:
                    PtzConverter.CurFocus = data1;
                    break;
                case 0x0079:
                    PtzConverter.NorthPan = data1;
                    Console.WriteLine("NorthPan {0}", data1);
                    PtzConverter.UpdateLimit();
                    break;
                case 0x007B:
                    PtzConverter.HorizontalTilt = data1;
                    Console.WriteLine("HorizontalTilt {0}", data1);
                    PtzConverter.UpdateLimit();
                    break;
                case 0x007D:
                    DegreePan = data1;
                    PtzConverter.DegreePan = (double)data1 / 100;
                    Console.WriteLine("_ptzConverter.DegreePan {0}", PtzConverter.DegreePan);
                    PtzConverter.UpdateLimit();
                    break;
                case 0x007F:
                    DegreeTilt = data1;
                    PtzConverter.DegreeTilt = (double)data1 / 100;
                    Console.WriteLine("_ptzConverter.DegreeTilt {0}", PtzConverter.DegreeTilt);
                    PtzConverter.UpdateLimit();
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
                    PtzConverter.CurPan = data1;
                    PtzConverter.CurTilt = data2;
                    break;
                case 0x0500://镜头位置连续回传
                    PtzConverter.CurZoom = data1;
                    PtzConverter.CurFocus = data2;
                    if (IsInited)
                        onPTZEvent(PtzConverter.UpdatePTZ());
                    break;
                case 0x0700://云台水平限位角度回传
                    PtzConverter.LeftLimit = data1;
                    PtzConverter.RightLimit = data2;
                    break;
                case 0x0900://云台垂直限位角度回传
                    PtzConverter.UpLimit = data1;
                    PtzConverter.DownLimit = data2;
                    PtzConverter.TiltReverse = data1 > data2;
                    break;
                case 0x0B00://镜头变倍限位值回传
                    PtzConverter.MinZoom = data1;
                    PtzConverter.MaxZoom = data2;
                    PtzConverter.UpdateLimit();
                    break;
                case 0x0D00://镜头聚焦限位值回传
                    PtzConverter.MinFocus = data1;
                    PtzConverter.MaxFocus = data2;
                    PtzConverter.UpdateLimit();
                    break;
            }
        }

        #endregion 接收并解析反馈指令

        #region ToPTZ

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            ushort uPan = PtzConverter.ToSerialPan(pan);
            ptzControl(0x0059, uPan);
            ushort uTilt = PtzConverter.ToSerialTilt(tilt);
            ptzControl(0x005B, uTilt);
            if (zoom != 0)
            {
                ushort uZoom = PtzConverter.ToSerialZoom(zoom);
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

        protected void onPTZEvent(PTZ ptz)
        {
            if (ptz == null)
                return;
            PTZPosition = ptz;
            var handler = PTZFeedbackEvent;
            if (handler != null)
                handler(ptz);
            lock (_objLock)
            {
                if (ptz != null && _ptzFeedbackForm != null)
                    _ptzFeedbackForm?.UpdatePTZ(ptz);
            }
        }

        private void runStaticInfo()
        {
            updateFeedbackMsg(false);
            prepareValues();
            updateFeedbackMsg(true);
        }

        protected void prepareValues()//获取限位等信息
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            DateTime begin = DateTime.Now;
            while (!PtzConverter.IsInitPTZLimit && !_disposeEvent.WaitOne(50))
            {
                if (DateTime.Now - begin > timeout)
                {
                    //throw new InvalidOperationException("获取参数超时！");
                    Prompt.Instance.OnStatus("获取参数超时！");
                    Thread.Sleep(10000);
                }
                if (PtzConverter.NorthPan == -1)
                    ptzControl(0x0071, 0x0000);//查询水平零点值协议
                else if (PtzConverter.HorizontalTilt == -1)
                    ptzControl(0x0073, 0x0000);//查询垂直零点值协议 
                else if (PtzConverter.DegreePan == 0)
                    ptzControl(0x0075, 0x0000);//查询水平校准值协议 
                else if (PtzConverter.DegreeTilt == 0)
                    ptzControl(0x0077, 0x0000);//查询垂直校准值协议
                else if (PtzConverter.LeftLimit == -1 || PtzConverter.RightLimit == -1)
                    ptzControl(0x0061, 0x0000);//发送云台水平限位位置返回请求
                else if (PtzConverter.UpLimit == -1 || PtzConverter.DownLimit == -1)
                    ptzControl(0x0063, 0x0000);//发送云台垂直限位位置返回请求
                else if (PtzConverter.MinZoom == -1 || PtzConverter.MaxZoom == -1)
                    ptzControl(0x0065, 0x0000);//发送镜头变倍限位位置返回请求
                else if (PtzConverter.MaxFocus == -1 || PtzConverter.MinFocus == -1)
                    ptzControl(0x0067, 0x0000);//发送镜头聚焦限位位置返回请求
                else
                {
                    PtzConverter.UpdateLimit();
                    if (PtzConverter.PtzLimits != null)
                        IsInited = true;
                    Console.WriteLine("IsInited = {0}", IsInited);
                }
            }
        }
        
        protected void updateFeedbackMsg(bool enable)
        {
            ptzControl(0x0099, 0x0000);//结束反馈
            Thread.Sleep(50);
            if (enable)
                ptzControl(0x0099, 0x0050);//反馈水平，垂直，变倍，聚焦功能,间隔时间，80毫秒
            else
                ptzControl(0x0099, 0x0000);//结束反馈
            Thread.Sleep(50);
        }

        protected void stopMovePTZ()
        {
            ptzControl(0x0000, 0x0000);//停命令
        }

        public void Dispose()
        {
            _feedbackControl.Set();
            _recvEvent.Set();
            _disposeEvent.Set();
            //updateFeedbackMsg(false);
            stopMovePTZ();
            Thread.Sleep(100);
            stopMovePTZ();
            Thread.Sleep(100);
        }

        #region ViewModel
        public ICommand SelfScanCmd { get; set; }
        public ICommand PTZStartFeedBackCmd { get; set; }
        public ICommand PTZStopFeedBackCmd { get; set; }
        public ICommand ReplayPTZFeedBackCmd { get; set; }
        public ICommand DemandPanCmd { get; set; }
        public ICommand DemandTiltCmd { get; set; }
        public ICommand DemandZoomCmd { get; set; }
        public ICommand DemandFocusCmd { get; set; }
        public ICommand DemandPanLimitCmd { get; set; }
        public ICommand DemandTiltLimitCmd { get; set; }
        public ICommand DemandZoomLimitCmd { get; set; }
        public ICommand DemandFocusLimitCmd { get; set; }
        public ICommand DemandNorthPanCmd { get; set; }
        public ICommand SetNorthPanCmd { get; set; }
        public ICommand DemandHorizontalTiltCmd { get; set; }
        public ICommand SetHorizontalTiltCmd { get; set; }
        public ICommand DemandDegreePanCmd { get; set; }
        public ICommand SetDegreePanCmd { get; set; }
        public ICommand DemandDegreeTiltCmd { get; set; }
        public ICommand SetDegreeTiltCmd { get; set; }
        public ICommand GoToPanCmd { get; set; }
        public ICommand GoToTiltCmd { get; set; }
        public ICommand GoToZoomCmd { get; set; }
        public ICommand GoToFocusCmd { get; set; }
        [AutoNotify]
        public ushort DegreePan { get; set; }
        [AutoNotify]
        public ushort DegreeTilt { get; set; }
        [AutoNotify]
        public ushort NextNorthPan { get; set; }
        [AutoNotify]
        public ushort NextHorizontalTilt { get; set; }
        [AutoNotify]
        public ushort NextDegreePan { get; set; }
        [AutoNotify]
        public ushort NextDegreeTilt { get; set; }
        [AutoNotify]
        public ushort NextPan { get; set; }
        [AutoNotify]
        public ushort NextTilt { get; set; }
        [AutoNotify]
        public ushort NextZoom { get; set; }
        [AutoNotify]
        public ushort NextFocus { get; set; }

        void initCmd()
        {
            SelfScanCmd = newCommand(0x000F, 0x0000);
            PTZStartFeedBackCmd = newCommand(0x0099, 0x0050);
            PTZStopFeedBackCmd = newCommand(0x0099, 0x0000);
            ReplayPTZFeedBackCmd = new DelegateCommand(_ => replayPTZFeedback());
            DemandPanCmd = newCommand(0x0051);
            DemandTiltCmd = newCommand(0x0053);
            DemandZoomCmd = newCommand(0x0055);
            DemandFocusCmd = newCommand(0x0057);
            DemandPanLimitCmd = newCommand(0x0061);
            DemandTiltLimitCmd = newCommand(0x0063);
            DemandZoomLimitCmd = newCommand(0x0065);
            DemandFocusLimitCmd = newCommand(0x0067);

            DemandNorthPanCmd = newCommand(0x0071, 0x0000);
            SetNorthPanCmd = newCommand(0x0079, SetDataType.NorthPan);
            DemandHorizontalTiltCmd = newCommand(0x0073, 0x0000);
            SetHorizontalTiltCmd = newCommand(0x007b, SetDataType.HorizontalTilt);
            DemandDegreePanCmd = newCommand(0x0075, 0x0000);
            SetDegreePanCmd = newCommand(0x007D, SetDataType.DegreePan);
            DemandDegreeTiltCmd = newCommand(0x0077);
            SetDegreeTiltCmd = newCommand(0x007F, SetDataType.DegreeTilt);
            GoToPanCmd = newCommand(0x0059, SetDataType.Pan);
            GoToTiltCmd = newCommand(0x005B, SetDataType.Tilt);
            GoToZoomCmd = newCommand(0x005D, SetDataType.Zoom);
            GoToFocusCmd = newCommand(0x005F, SetDataType.Focus);
        }

        PTZFeedbackForm _ptzFeedbackForm = null;
        object _objLock = new object();

        private void replayPTZFeedback()
        {
            lock (_objLock)
            {
                if (_ptzFeedbackForm != null)
                {
                    _ptzFeedbackForm.Close();
                    _ptzFeedbackForm = null;
                }
                if (_ptzFeedbackForm == null)
                {
                    _ptzFeedbackForm = new PTZFeedbackForm();
                    _ptzFeedbackForm.Show();
                }
            }
        }

        private DelegateCommand newCommand(int cmd, int data = 0)
        {
            return new DelegateCommand(_ => sendCommand(cmd, data));
        }

        private DelegateCommand newCommand(int cmd, SetDataType type)
        {
            return new DelegateCommand(_ => sendCommand(cmd, type));
        }

        private void sendCommand(int cmd, int data = 0)
        {
            byte[] buffer = new byte[7];
            buffer[0] = 0xFF;
            buffer[1] = _pelcoD.AddressId;
            buffer[2] = (byte)((cmd >> 8) & 0xFF);
            buffer[3] = (byte)(cmd & 0xFF);
            buffer[4] = (byte)((data >> 8) & 0xFF);
            buffer[5] = (byte)(data & 0xFF);
            checkDigit(buffer);
            sendBuffer(buffer);
        }

        private void sendCommand(int cmd, SetDataType type)
        {
            sendCommand(cmd, getData(type));
        }

        void checkDigit(byte[] buffer)
        {
            byte sum = 0;
            for (int i = 1; i < buffer.Length - 1; i++)
                sum += buffer[i];
            buffer[buffer.Length - 1] = sum;
        }

        ushort getData(SetDataType type)
        {
            switch(type)
            {
                case SetDataType.None:
                    return 0x00000;
                case SetDataType.NorthPan:
                    return NextNorthPan;
                case SetDataType.HorizontalTilt:
                    return NextHorizontalTilt;
                case SetDataType.DegreePan:
                    return NextDegreePan;
                case SetDataType.DegreeTilt:
                    return NextDegreeTilt;
                case SetDataType.Pan:
                    return NextPan;
                case SetDataType.Tilt:
                    return NextTilt;
                case SetDataType.Zoom:
                    return NextZoom;
                case SetDataType.Focus:
                    return NextFocus;
                default:
                    throw new InvalidDataException("无效的控制类型！");
            }
        }

        #endregion

        enum SetDataType
        {
            None,
            NorthPan,
            HorizontalTilt,
            DegreePan,
            DegreeTilt,
            Pan,
            Tilt,
            Zoom,
            Focus,
        }
    }
}
