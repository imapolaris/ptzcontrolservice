using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PTZControlService.Hikvision
{
    static class NetDvrDll
    {
        static bool _isX64 = IntPtr.Size == 8;

        public static int NET_DVR_Init()
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_Init();
            else
                return NetDvrDll32.NET_DVR_Init();
        }

        public static int NET_DVR_Cleanup()
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_Cleanup();
            else
                return NetDvrDll32.NET_DVR_Cleanup();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_DEVICEINFO_V30
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NetDvrDll32.SERIALNO_LEN)]
            public string sSerialNumber;  //序列号
            public byte byAlarmInPortNum;		//报警输入个数
            public byte byAlarmOutPortNum;		//报警输出个数
            public byte byDiskNum;				//硬盘个数
            public byte byDVRType;				//设备类型, 1:DVR 2:ATM DVR 3:DVS ......
            public byte byChanNum;				//模拟通道个数
            public byte byStartChan;			//起始通道号,例如DVS-1,DVR - 1
            public byte byAudioChanNum;         //语音通道数
            public byte byIPChanNum;					//最大数字通道个数  
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] byRes1;					//保留
        };

        public static int NET_DVR_Login_V30(string sDVRIP, ushort wDVRPort, string sUserName, string sPassword, out NET_DVR_DEVICEINFO_V30 lpDeviceInfo)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_Login_V30(sDVRIP, wDVRPort, sUserName, sPassword, out lpDeviceInfo);
            else
                return NetDvrDll32.NET_DVR_Login_V30(sDVRIP, wDVRPort, sUserName, sPassword, out lpDeviceInfo);
        }

        public static int NET_DVR_Logout_V30(int lUserID)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_Logout_V30(lUserID);
            else
                return NetDvrDll32.NET_DVR_Logout_V30(lUserID);
        }

        public delegate void SerialDataCallBack(int lSerialHandle, IntPtr pRecvDataBuffer, uint dwBufSize, IntPtr dwUser);

        public static int NET_DVR_SerialStart(int lUserID, int lSerialPort, SerialDataCallBack fSerialDataCallBack, IntPtr dwUser)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_SerialStart(lUserID, lSerialPort, fSerialDataCallBack, dwUser);
            else
                return NetDvrDll32.NET_DVR_SerialStart(lUserID, lSerialPort, fSerialDataCallBack, dwUser);
        }

        public static int NET_DVR_SerialSend(int lSerialHandle, int lChannel, IntPtr pSendBuf, uint dwBufSize)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_SerialSend(lSerialHandle, lChannel, pSendBuf, dwBufSize);
            else
                return NetDvrDll32.NET_DVR_SerialSend(lSerialHandle, lChannel, pSendBuf, dwBufSize);
        }

        public static int NET_DVR_SerialStop(int lSerialHandle)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_SerialStop(lSerialHandle);
            else
                return NetDvrDll32.NET_DVR_SerialStop(lSerialHandle);
        }

        public static int NET_DVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, out uint lpBytesReturned)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_GetDVRConfig(lUserID, dwCommand, lChannel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned);
            else
                return NetDvrDll32.NET_DVR_GetDVRConfig(lUserID, dwCommand, lChannel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned);
        }

        public static int NET_DVR_SetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_SetDVRConfig(lUserID, dwCommand, lChannel, lpInBuffer, dwInBufferSize);
            else
                return NetDvrDll32.NET_DVR_SetDVRConfig(lUserID, dwCommand, lChannel, lpInBuffer, dwInBufferSize);
        }

        public static int NET_DVR_PTZControlWithSpeed_Other(int userId, int channel, uint dwPTZCommand, uint dwStop, uint dwSpeed)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_PTZControlWithSpeed_Other(userId, channel, dwPTZCommand, dwStop, dwSpeed);
            else
                return NetDvrDll32.NET_DVR_PTZControlWithSpeed_Other(userId, channel, dwPTZCommand, dwStop, dwSpeed);
        }

        public static int NET_DVR_PTZPreset_Other(int userId, int channel, uint dwPtzPresetCmd, uint dwPresetIndex)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_PTZPreset_Other(userId, channel, dwPtzPresetCmd, dwPresetIndex);
            else
                return NetDvrDll32.NET_DVR_PTZPreset_Other(userId, channel, dwPtzPresetCmd, dwPresetIndex);
        }

        public static int NET_DVR_CaptureJPEGPicture(int userId, int channel, string fileName, ushort wPicSize)
        {
            if (_isX64)
            {
                NetDvrDll64.NET_DVR_JPEGPARA para = new NetDvrDll64.NET_DVR_JPEGPARA()
                {
                    wPicSize = wPicSize,
                    wPicQuality = 0
                };
                return NetDvrDll64.NET_DVR_CaptureJPEGPicture(userId, channel, ref para, fileName);
            }
            else
            {
                NetDvrDll32.NET_DVR_JPEGPARA para = new NetDvrDll32.NET_DVR_JPEGPARA()
                {
                    wPicSize = wPicSize,
                    wPicQuality = 0
                };
                return NetDvrDll32.NET_DVR_CaptureJPEGPicture(userId, channel, ref para, fileName);
            }
        }
        
        public static int NET_DVR_TransPTZ_Other(int userId, int channel, IntPtr ptzCodePtr, uint bufSize)
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_TransPTZ_Other(userId, channel, ptzCodePtr, bufSize);
            else
                return NetDvrDll32.NET_DVR_TransPTZ_Other(userId, channel, ptzCodePtr, bufSize);
        }

        public static int NET_DVR_GetLastError()
        {
            if (_isX64)
                return NetDvrDll64.NET_DVR_GetLastError();
            else
                return NetDvrDll32.NET_DVR_GetLastError();
        }
    }
}
