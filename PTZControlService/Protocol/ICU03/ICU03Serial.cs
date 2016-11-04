using PTZControlService.Protocol.ICU03;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    public class ICU03Serial : DeviceBaseSerial
    {
        Buffer _buffer = new ICU03Buffer();
        public ICU03Serial(byte serialId, double zoomMax) : base(serialId, zoomMax)
        {
        }

        #region ToPTZ
        public override void ToPTZ(double pan, double tilt, double zoom)
        {
            ushort uPan = _ptzConverter.ToSerialPan(pan);
            ushort uTilt = _ptzConverter.ToSerialTilt(tilt);
            byte[] data = new byte[10];
            setIcu03(data, uPan, 0);
            setIcu03(data, uTilt, 2);
            ushort uZoom = 0;
            if (zoom != 0)
            {
                uZoom = _ptzConverter.ToSerialZoom(zoom);
                setIcu03(data, uZoom, 4);
            }
            Console.WriteLine("{0},{1},{2}  ==>  {3},{4},{5}", _ptzConverter.CurPan, _ptzConverter.CurTilt, _ptzConverter.CurZoom, uPan, uTilt, uZoom);
            sendCmd(0x22, data);
        }

        #endregion ToPTZ
                        
        protected override void prepareValues()//获取限位等信息
        {
            const int _icu03Magic = 0x55534E54;
            while (!IsInited && !_disposeEvent.WaitOne(1))
            {
                const int readLength = 16;
                for (int i = 0; i < 0x60;)
                {
                    Console.Write("({0})", i);
                    if (readEeprom(i, readLength))
                        i += readLength;
                    if (_disposeEvent.WaitOne(0))
                        return;
                }

                for (int i = 0x100; i < 0x170;)
                {
                    Console.Write("({0})", i);
                    if (readEeprom(i, readLength))
                        i += readLength;
                    if (_disposeEvent.WaitOne(0))
                        return;
                }

                parseEeprom();
                Console.WriteLine("ICU03基本参数获取完毕！");
                if (_magic == _icu03Magic)
                {
                    onStaticValues();
                    IsInited = true;
                }
            }
        }

        protected override void updateCurPTZ()
        {
            while (!_disposeEvent.WaitOne(20))
            {
                byte[] recv = receive();
                if (recv.Length > 0)
                {
                    _buffer.Add(recv);
                    Message msg;
                    while (_buffer.ParseMessage(out msg))
                    {
                        if (msg.Command == 0x23 && msg.Params.Length >= 10)
                        {
                            _ptzConverter.CurPan = getIcu03Short(msg.Params, 0);
                            _ptzConverter.CurTilt = getIcu03Short(msg.Params, 2);
                            _ptzConverter.CurZoom = getIcu03Short(msg.Params, 4);
                            _ptzConverter.CurFocus = getIcu03Short(msg.Params, 6);
                            _ptzConverter.CurIris = getIcu03Short(msg.Params, 8);
                            onPTZEvent(_ptzConverter.UpdatePTZ());
                        }
                    }
                }
            }
        }
        
        #region 云台获取到的PTZ参数

        byte[] _eeprom = new byte[0x170];
        int _magic;
        
        #endregion 云台获取到的PTZ参数

        #region 静态信息

        private void onStaticValues()
        {
            _ptzConverter.UpdateLimit();
        }
        #endregion 静态信息

        #region PTZ数值获取与解析
        
        bool readEeprom(int addr, int length)
        {
            sendCmd(0x11, (byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF), (byte)length);
            return receiveEeprom(addr, length);
        }

        private void parseEeprom()
        {
            _magic = BitConverter.ToInt32(_eeprom, 4);
            _ptzConverter.LeftLimit = getEepromShort(0x08);
            _ptzConverter.RightLimit = getEepromShort(0x0A);
            _ptzConverter.UpLimit = getEepromShort(0x0C);
            _ptzConverter.DownLimit = getEepromShort(0x0E);
            _ptzConverter.MinZoom = getEepromShort(0x10);
            _ptzConverter.MaxZoom = getEepromShort(0x12);
            _ptzConverter.MinFocus = getEepromShort(0x14);
            _ptzConverter.MaxFocus = getEepromShort(0x16);
            _ptzConverter.MinIris = getEepromShort(0x18);
            _ptzConverter.MaxIris = getEepromShort(0x1A);

            _ptzConverter.NorthPan = (ushort)getOldEepromInt(0x150);
            if (_ptzConverter.NorthPan == 0)
                _ptzConverter.NorthPan = getEepromShort(0x20);

            int eastPan = getOldEepromInt(0x154);
            if (eastPan != _ptzConverter.NorthPan && eastPan != 0)
                _ptzConverter.DegreePan = (eastPan > _ptzConverter.NorthPan) ? (eastPan - _ptzConverter.NorthPan) / 90.0 : (_ptzConverter.NorthPan - eastPan) / 270.0;
            if (_ptzConverter.DegreePan <= 0)
                _ptzConverter.DegreePan = getEepromShort(0x22) / 100.0;
            //if (_degreePan <= 0)
            //    _degreePan = 148;
            _ptzConverter.HorizontalTilt = (ushort)getOldEepromInt(0x120);
            if (_ptzConverter.HorizontalTilt == 0)   //
                _ptzConverter.HorizontalTilt = getEepromShort(0x24); //
            int downTilt = getOldEepromInt(0x124);
            if (downTilt != _ptzConverter.HorizontalTilt && downTilt != 0)
                _ptzConverter.DegreeTilt = (downTilt - _ptzConverter.HorizontalTilt) / 90.0;
            if(_ptzConverter.DegreeTilt <= 0)
                _ptzConverter.DegreeTilt = getEepromShort(0x26) / 100.0;
        }

        private ushort getEepromShort(int addr)
        {
            return getIcu03Short(_eeprom, addr);
        }
        private ushort getOldEepromShort(int addr)
        {
            return getIcu02Short(_eeprom, addr);
        }

        private int getOldEepromInt(int addr)
        {
            return getIcu02Int(_eeprom, addr);
        }

        private static ushort getIcu03Short(byte[] data, int offset)
        {
            return (ushort)(((data[offset] << 8) | data[offset + 1])& 0xffff);
        }

        private static int getIcu03Int(byte[] data, int offset)
        {
            return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
        }

        private static void setIcu03(byte[] data, ushort value, int offset)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        private static ushort getIcu02Short(byte[] data, int offset)
        {
            return BitConverter.ToUInt16(data, offset);
        }

        private static int getIcu02Int(byte[] data, int offset)
        {
            return BitConverter.ToInt32(data, offset);
        }

        private bool receiveEeprom(int i, int readLength)
        {
            DateTime expireTime = DateTime.Now + TimeSpan.FromMilliseconds(500);
            while (true)
            {
                int timeout = Math.Max(0, (int)Math.Round((expireTime - DateTime.Now).TotalMilliseconds));
                int wait = WaitHandle.WaitAny(new WaitHandle[] { _disposeEvent, OnDataEvent }, timeout);
                if (wait == 1)
                {
                    byte[] recv = receive();
                    _buffer.Add(recv.Take(recv.Length));
                    Message msg;

                    while (_buffer.ParseMessage(out msg))
                    {
                        if (msg.Command == 0x13 && msg.Params.Length >= 3)
                        {
                            int addr = ((int)msg.Params[0] << 8) + msg.Params[1];
                            int len = msg.Params[2];
                            if (len > 0 && len + 3 <= msg.Params.Length && addr + len <= _eeprom.Length)
                            {
                                Array.Copy(msg.Params, 3, _eeprom, addr, len);
                                if (i == addr && len >= readLength)
                                    return true;
                            }
                        }
                    }
                }
                else
                    break;
            }
            return false;
        }

        #endregion PTZ数值获取与解析

        protected override void updateFeedbackMsg(bool enable)
        {
            sendCmd(0x25, (byte)(enable ? 0x01 : 0x00));
            Thread.Sleep(100);
        }

        void sendCmd(byte cmd, params byte[] pms)
        {
            Console.WriteLine($"0x{cmd.ToString("X2")} - {BitConverter.ToString(pms)}");
            int length = 6 + pms.Length;
            byte[] msg = new byte[length];
            msg[0] = 0xFA;
            msg[1] = (byte)length;
            msg[2] = 0xFF;
            msg[3] = _addressId;
            msg[4] = cmd;
            Array.Copy(pms, 0, msg, 5, pms.Length);
            int checksumIndex = length - 1;
            msg[checksumIndex] = 0;
            for (int i = 1; i < checksumIndex; i++)
                msg[checksumIndex] += msg[i];
            sendBuffer(msg);
        }

        protected override void stopMovePTZ()
        {
            sendCmd(0x43, 0, 0, 0);
        }
    }
}