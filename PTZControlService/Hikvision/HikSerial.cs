using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Hikvision
{
    public class HikSerial : NetDvr.Serial, IDisposable
    {
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public PTZLimit PTZLimits { get; private set; }
        public PTZ PTZPosition { get; private set; }
        public Action<PTZ> PTZEvent { get; set; }
        public DateTime LatestTime { get; private set; }

        public int Channel { get; private set; } = 1;
        public HikSerial(string hikHost, int port, string user, string pass, int channel)
            :base(hikHost, port, user, pass)
        {
            Channel = channel;
            loadLimits();
            _disposeEvent.Reset();
            LatestTime = DateTime.Now;
            new Thread(updatePTZ) { IsBackground = true }.Start();
        }

        public void ToPTZ(double pan, double tilt, double zoom)
        {
            if (PTZLimits == null)
                return;
            ///判断是否控制Zoom.
            ushort action = 1;
            if (zoom == 0)
                action = 5;
            pan = PTZConverter.ToPanAngle(pan);
            double toPan = PTZConverter.ToValidAngle(toHikPan(pan), PTZLimits.Left, PTZLimits.Right);
            double toTilt = PTZConverter.ToValid(tilt, PTZLimits.Up, PTZLimits.Down); 
            double toZoom = PTZConverter.ToValid(zoom, 1,PTZLimits.ZoomMax);
            setPTZ(action, new PTZ(toPan, toTilt, toZoom));
        }

        /// <summary>
        /// 带速度的云台控制操作
        /// </summary>
        /// <param name="ptzCommand">云台控制命令</param>
        /// <param name="isStop">云台停止动作或开始动作：false－开始；true－停止</param>
        /// <param name="speed">云台控制的速度，用户按不同解码器的速度控制值设置。取值范围[1,7]</param>
        /// <returns>TRUE表示成功，FALSE表示失败。</returns>
        public bool PTZControlWithSpeed(uint dwPTZCommand, uint dwStop, uint dwSpeed)
        {
            int value = 0;
            if (Dvr.Handle >= 0)
            {
                if (Dvr.IsHik)
                    value = NetDvrDll.NET_DVR_PTZControlWithSpeed_Other(Dvr.Handle, Channel, dwPTZCommand, dwStop, dwSpeed);
                else
                    value = UsntDvrDll.USNTDVR_PTZControlWithSpeed_Other(Dvr.Handle, Channel, dwPTZCommand, dwStop, dwSpeed);
            }
            return checkAndBack(value);
        }

        /// <summary>
        /// 云台预置点操作。
        /// </summary>
        /// <param name="ptzPresetCmd"> 操作云台预置点命令</param>
        /// <param name="presetIndex">预置点的序号（从1开始），最多支持300个预置点 </param>
        /// <returns>TRUE表示成功，FALSE表示失败。</returns>
        public bool PTZPreset(uint ptzPresetCmd, uint presetIndex)
        {
            int value = 0;
            if (Dvr.Handle >= 0)
            {
                if (Dvr.IsHik)
                    value = NetDvrDll.NET_DVR_PTZPreset_Other(Dvr.Handle, Channel, ptzPresetCmd, presetIndex);
                else
                    value = UsntDvrDll.USNTDVR_PTZPreset_Other(Dvr.Handle, Channel, ptzPresetCmd, presetIndex);
            }
            return checkAndBack(value);
        }

        /// <summary>
        /// 获取设备的配置信息
        /// </summary>
        /// <param name="dwCommand">设备配置命令</param>
        /// <param name="lpOutBuffer">接收数据的缓冲指针 </param>
        /// <param name="dwOutBufferSize">接收数据的缓冲长度(以字节为单位)，不能为0</param>
        /// <param name="lpBytesReturned">实际收到的数据长度指针，不能为NULL </param>
        /// <returns>TRUE表示成功，FALSE表示失败</returns>
        public bool GetDVRConfig(uint dwCommand, IntPtr lpOutBuffer, uint dwOutBufferSize, out uint lpBytesReturned)
        {
            bool values = false;
            lpBytesReturned = 0;
            if (Dvr.Handle >= 0)
            {
                if (Dvr.IsHik)
                    values = NetDvrDll.NET_DVR_GetDVRConfig(Dvr.Handle, dwCommand, Channel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned) != 0;
                else
                    values = UsntDvrDll.USNTDVR_GetDVRConfig(Dvr.Handle, dwCommand, Channel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned) != 0;
            }
            if (!values)
                error();
            return values;
        }

        public bool SetDVRConfig(uint dwCommand, IntPtr lpInBuffer, uint dwInBufferSize)
        {
            bool values = false;
            if (Dvr.Handle >= 0)
            {
                if (Dvr.IsHik)
                    values = NetDvrDll.NET_DVR_SetDVRConfig(Dvr.Handle, dwCommand, Channel, lpInBuffer, dwInBufferSize) != 0;
                else
                    values = UsntDvrDll.USNTDVR_SetDVRConfig(Dvr.Handle, dwCommand, Channel, lpInBuffer, dwInBufferSize) != 0;
            }
            if (!values)
                error();
            return values;
        }

        /// <summary>获取快照信息</summary>
        public bool CaptureJPEGPicture(string fileName, ushort wPicSize)
        {
            return CaptureJPEGPicture(Channel, fileName, wPicSize);
        }

        private void loadLimits()
        {
            int length = Marshal.SizeOf(new NetDvrDll32.NET_DVR_PTZSCOPE());
            IntPtr lpOutBuffer = Marshal.AllocHGlobal(length);
            uint dwReceived = 0;
            if (GetDVRConfig(NetDvrDll32.NET_DVR_GET_PTZSCOPE, lpOutBuffer, (uint)length, out dwReceived))
            {
                NetDvrDll32.NET_DVR_PTZSCOPE scope = (NetDvrDll32.NET_DVR_PTZSCOPE)Marshal.PtrToStructure(lpOutBuffer, typeof(NetDvrDll32.NET_DVR_PTZSCOPE));
                double top = convertHikTilt(HexToDec(scope.wTiltPosMin));
                double bottom = convertHikTilt(HexToDec(scope.wTiltPosMax));
                double left = HexToDec(scope.wPanPosMin);
                double right = HexToDec(scope.wPanPosMax);
                double maxZoom = HexToDec(scope.wZoomPosMax);
                if (right < left + 360)//转为实际角度范围
                {
                    left = toRealPan(left);
                    right = toRealPan(right);
                }
                PTZLimits = new PTZLimit(left, right, top, bottom, maxZoom);
            }
            Marshal.FreeHGlobal(lpOutBuffer);
        }

        private void updatePTZ()
        {
            while (!_disposeEvent.WaitOne(10))
            {
                getPTZ();
            }
        }

        private void getPTZ()
        {
            uint dwReceived = 0;
            int length = Marshal.SizeOf(new NetDvrDll32.NET_DVR_PTZPOS());
            IntPtr lpOutBuffer = Marshal.AllocHGlobal(length);
            if (GetDVRConfig(NetDvrDll32.NET_DVR_GET_PTZPOS, lpOutBuffer, (uint)length, out dwReceived))
            {
                NetDvrDll32.NET_DVR_PTZPOS pos = (NetDvrDll32.NET_DVR_PTZPOS)Marshal.PtrToStructure(lpOutBuffer, typeof(NetDvrDll32.NET_DVR_PTZPOS));
                double pan = toRealPan(HexToDec(pos.wPanPos));
                double tilt = convertHikTilt(HexToDec(pos.wTiltPos));
                double zoom = HexToDec(pos.wZoomPos);
                onPTZ(new PTZ(pan, tilt, zoom));
                LatestTime = DateTime.Now;
            }
            Marshal.FreeHGlobal(lpOutBuffer);
        }


        private void onPTZ(PTZ ptz)
        {
            PTZPosition = ptz;
            var handler = PTZEvent;
            if (handler != null)
                handler(ptz);
        }

        /// <summary>
        /// 设置IP快球PTZ
        /// </summary>
        /// <param name="wAction">操作类型，仅在设置时有效。1-定位PTZ参数，2-定位P参数，3-定位T参数，4-定位Z参数，5-定位PT参数 </param>
        /// <param name="hikPTZ">PTZ信息</param>
        private void setPTZ(ushort wAction, PTZ hikPTZ)
        {
            NetDvrDll32.NET_DVR_PTZPOS pos = new NetDvrDll32.NET_DVR_PTZPOS();
            pos.wAction = wAction;
            pos.wPanPos = DecToHex(hikPTZ.Pan);
            pos.wTiltPos = DecToHex(convertHikTiltBack(hikPTZ.Tilt));
            pos.wZoomPos = DecToHex(hikPTZ.Zoom);
            int posLength = Marshal.SizeOf(pos);
            IntPtr lpInBuffer = Marshal.AllocHGlobal(posLength);
            Marshal.StructureToPtr(pos, lpInBuffer, true);
            SetDVRConfig(NetDvrDll32.NET_DVR_SET_PTZPOS, lpInBuffer, (uint)posLength);
            //pos.wAction = wAction;
            //Marshal.StructureToPtr(pos, lpInBuffer, true);
            //SetDVRConfig(NetDvrDll32.NET_DVR_SET_PTZPOS, lpInBuffer, (uint)posLength);
            Marshal.FreeHGlobal(lpInBuffer);
        }

        #region 云镜数据格式转换
        ushort DecToHex(double dDec)
        {
            int dec = (int)(dDec * 10 + 0.5);
            int hex = 0;
            int scale = 1;
            while (dec != 0)
            {
                int data = dec % 10;
                hex += data * scale;
                dec /= 10;
                scale *= 16;
            }
            return (ushort)hex;
        }

        double HexToDec(ushort hex)
        {
            int dec = 0;
            int scale = 1;
            while (hex != 0)
            {
                int data = Math.Min(9, hex % 16);
                dec += data * scale;
                hex /= 16;
                scale *= 10;
            }
            return dec / 10.0;
        }

        double convertHikTilt(double dTilt)
        {
            if (dTilt > 180)
                dTilt -= 360;
            return dTilt;
        }

        double convertHikTiltBack(double nTilt)
        {
            if (nTilt < 0)
                nTilt += 360;
            return nTilt;
        }

        double toRealPan(double hikPan)
        {
            return PTZConverter.ToPanAngle(hikPan);
        }

        double toHikPan(double realPan)
        {
            return PTZConverter.ToPanAngle(realPan);
        }
        
        #endregion 云镜数据格式转换

        public override void Dispose()
        {
            _disposeEvent.Set();
            base.Dispose();
        }
    }
}