using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PTZControlService.Hikvision
{
    static class USNTDvrDll64
    {
        const string _dllPath = @"x64\USNT_SDK.dll";

        [DllImport(_dllPath)]
        public static extern int USNTDVR_Init();
        [DllImport(_dllPath)]
        public static extern int USNTDVR_Cleanup();

        [DllImport(_dllPath)]
        public static extern int USNTDVR_Login_V30(string sDVRIP, ushort wDVRPort, string sUserName, string sPassword, out NetDvrDll.NET_DVR_DEVICEINFO_V30 lpDeviceInfo);
        [DllImport(_dllPath)]
        public static extern int USNTDVR_Logout_V30(int lUserID);

        [DllImport(_dllPath)]
        public static extern int USNTDVR_SerialStart(int lUserID, int lSerialPort, NetDvrDll.SerialDataCallBack fSerialDataCallBack, IntPtr dwUser);
        [DllImport(_dllPath)]
        public static extern int USNTDVR_SerialSend(int lSerialHandle, int lChannel, IntPtr pSendBuf, uint dwBufSize);
        [DllImport(_dllPath)]
        public static extern int USNTDVR_SerialStop(int lSerialHandle);
        [DllImport(_dllPath)]
        public static extern int USNTDVR_PTZControlWithSpeed_Other(int lUserID, int lChannel, uint dwPTZCommand, uint dwStop, uint dwSpeed);
        [DllImport(_dllPath)]
        public static extern int USNTDVR_PTZPreset_Other(int userId, int channel, uint dwPtzPresetCmd, uint dwPresetIndex);

        [DllImport(_dllPath)]
        public static extern int USNTDVR_TransPTZ_Other(int handle, int channel, IntPtr ptr, uint bufSize);

        //图片质量
        [StructLayout(LayoutKind.Sequential)]
        public struct USNTDVR_JPEGPARA
        {
            //注意：当图像压缩分辨率为VGA时，支持0=CIF, 1=QCIF, 2=D1抓图，
            //当分辨率为3=UXGA(1600x1200), 4=SVGA(800x600), 5=HD720p(1280x720),6=VGA,7=XVGA, 8=HD900p
            //仅支持当前分辨率的抓图
            public ushort wPicSize;				// 0=CIF, 1=QCIF, 2=D1 3=UXGA(1600x1200), 4=SVGA(800x600), 5=HD720p(1280x720),6=VGA
            public ushort wPicQuality;			// 图片质量系数 0-最好 1-较好 2-一般
        }

        //2016-06-12
        [DllImport(_dllPath)]
        public static extern int USNTDVR_CaptureJPEGPicture(int lUserID, int lChannel, ref USNTDVR_JPEGPARA lpJpegPara, string sPicFileName);

        //2016-06-12
        [DllImport(_dllPath)]
        public static extern int USNTDVR_GetLastError();

        //2016.06.13
        [DllImport(_dllPath)]
        public static extern int USNTDVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, out uint lpBytesReturned);

        //2016.06.14
        [DllImport(_dllPath)]
        public static extern int USNTDVR_SetDVRConfig(int userId, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize);
    }
}
