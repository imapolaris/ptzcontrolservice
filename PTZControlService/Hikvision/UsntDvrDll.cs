using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTZControlService.Hikvision
{
    static class UsntDvrDll
    {
        static bool _isX64 = IntPtr.Size == 8;

        public static int USNTDVR_Init()
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_Init();
            else
                return USNTDvrDll32.USNTDVR_Init();
        }

        public static int USNTDVR_Cleanup()
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_Cleanup();
            else
                return USNTDvrDll32.USNTDVR_Cleanup();
        }

        public static int USNTDVR_Login_V30(string sDVRIP, ushort wDVRPort, string sUserName, string sPassword, out NetDvrDll.NET_DVR_DEVICEINFO_V30 lpDeviceInfo)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_Login_V30(sDVRIP, wDVRPort, sUserName, sPassword, out lpDeviceInfo);
            else
                return USNTDvrDll32.USNTDVR_Login_V30(sDVRIP, wDVRPort, sUserName, sPassword, out lpDeviceInfo);
        }

        public static int USNTDVR_Logout_V30(int lUserID)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_Logout_V30(lUserID);
            else
                return USNTDvrDll32.USNTDVR_Logout_V30(lUserID);
        }

        public static int USNTDVR_SerialStart(int lUserID, int lSerialPort, NetDvrDll.SerialDataCallBack fSerialDataCallBack, IntPtr dwUser)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_SerialStart(lUserID, lSerialPort, fSerialDataCallBack, dwUser);
            else
                return USNTDvrDll32.USNTDVR_SerialStart(lUserID, lSerialPort, fSerialDataCallBack, dwUser);
        }

        public static int USNTDVR_SerialSend(int lSerialHandle, int lChannel, IntPtr pSendBuf, uint dwBufSize)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_SerialSend(lSerialHandle, lChannel, pSendBuf, dwBufSize);
            else
                return USNTDvrDll32.USNTDVR_SerialSend(lSerialHandle, lChannel, pSendBuf, dwBufSize);
        }

        public static int USNTDVR_SerialStop(int lSerialHandle)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_SerialStop(lSerialHandle);
            else
                return USNTDvrDll32.USNTDVR_SerialStop(lSerialHandle);
        }

        public static int USNTDVR_PTZControlWithSpeed_Other(int userId, int channel, uint dwPTZCommand, uint dwStop, uint dwSpeed)
        {
            if (_isX64)
                return  USNTDvrDll64.USNTDVR_PTZControlWithSpeed_Other(userId, channel, dwPTZCommand, dwStop, dwSpeed);
            else
                return USNTDvrDll32.USNTDVR_PTZControlWithSpeed_Other(userId, channel, dwPTZCommand, dwStop, dwSpeed);
        }

        public static int USNTDVR_PTZPreset_Other(int userId, int channel, uint dwPtzPresetCmd, uint dwPresetIndex)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_PTZPreset_Other(userId, channel, dwPtzPresetCmd, dwPresetIndex);
            else
                return USNTDvrDll32.USNTDVR_PTZPreset_Other(userId, channel, dwPtzPresetCmd, dwPresetIndex);
        }

        public static int USNTDVR_TransPTZ_Other(int handle, int channel, IntPtr ptr, uint bufSize)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_TransPTZ_Other(handle, channel, ptr, bufSize);
            else 
                return USNTDvrDll32.USNTDVR_TransPTZ_Other(handle, channel, ptr, bufSize);
        }

        public static int NET_DVR_CaptureJPEGPicture(int userId, int channel, string fileName, ushort wPicSize)
        {
            if (_isX64)
            {
                USNTDvrDll64.USNTDVR_JPEGPARA para = new USNTDvrDll64.USNTDVR_JPEGPARA()
                {
                    wPicSize = wPicSize,
                    wPicQuality = 0
                };
                return USNTDvrDll64.USNTDVR_CaptureJPEGPicture(userId, channel, ref para, fileName);
            }
            else
            {
                USNTDvrDll32.USNTDVR_JPEGPARA para = new USNTDvrDll32.USNTDVR_JPEGPARA()
                {
                    wPicSize = wPicSize,
                    wPicQuality = 0
                };
                return USNTDvrDll32.USNTDVR_CaptureJPEGPicture(userId, channel, ref para, fileName);
            }
        }

        public static int USNTDVR_GetLastError()
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_GetLastError();
            else
                return USNTDvrDll32.USNTDVR_GetLastError();
        }

        public static int USNTDVR_GetDVRConfig(int userId, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, out uint lpBytesReturned)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_GetDVRConfig(userId, dwCommand, lChannel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned);
            else
                return USNTDvrDll32.USNTDVR_GetDVRConfig(userId, dwCommand, lChannel, lpOutBuffer, dwOutBufferSize, out lpBytesReturned);
        }

        public static int USNTDVR_SetDVRConfig(int userId, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize)
        {
            if (_isX64)
                return USNTDvrDll64.USNTDVR_SetDVRConfig(userId, dwCommand, lChannel, lpInBuffer, dwInBufferSize);
            else
                return USNTDvrDll32.USNTDVR_SetDVRConfig(userId, dwCommand, lChannel, lpInBuffer, dwInBufferSize);
        }
    }
}
