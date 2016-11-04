using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PTZControlService.Hikvision
{
    public class NetDvr : IDisposable
    {
        private class Engine
        {
            protected Engine()
            {
                NetDvrDll.NET_DVR_Init();
                UsntDvrDll.USNTDVR_Init();
            }

            ~Engine()
            {
                NetDvrDll.NET_DVR_Cleanup();
                UsntDvrDll.USNTDVR_Cleanup();
            }

            public void Init()
            {
            }

            public static readonly Engine Instance = new Engine();
        }

        public int Handle = -1;
        public bool IsHik = true;
        internal NetDvrDll.NET_DVR_DEVICEINFO_V30 DeviceInfo = new NetDvrDll.NET_DVR_DEVICEINFO_V30();

        public NetDvr()
        {
            Engine.Instance.Init();
        }

        public bool Login(string ip, int port, string user, string pass)
        {
            Logout();

            Handle = NetDvrDll.NET_DVR_Login_V30(ip, (ushort)port, user, pass, out DeviceInfo);
            if (Handle >= 0)
            {
                IsHik = true;
                Console.WriteLine("Handle : " + Handle);
                return true;
            }
            else
            {
                Handle = UsntDvrDll.USNTDVR_Login_V30(ip, (ushort)port, user, pass, out DeviceInfo);
                if (Handle >= 0)
                {
                    IsHik = false;
                    return true;
                }
            }

            return false;
        }

        public void Logout()
        {
            if (Handle >= 0)
            {
                if (IsHik)
                    NetDvrDll.NET_DVR_Logout_V30(Handle);
                else
                    UsntDvrDll.USNTDVR_Logout_V30(Handle);
            }
            Handle = -1;
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Logout();
        }

        #endregion

        public class Serial : IDisposable
        {
            public NetDvr Dvr;
            /// <param name="channel"> 通道号，不同的命令对应不同的取值，如果该参数无效则置为0xFFFFFFFF即可</param>
            public Serial(string hikHost, int port = 8000, string user = "admin", string pass = "12345")
            {
                var dvr = new NetDvr();
                if (dvr.Login(hikHost, port, user, pass))
                {
                    Dvr = dvr;
                }
                else
                    throw new CanNotLoginException("设备登录失败！");
            }

            /// <summary>获取快照信息</summary>
            public bool CaptureJPEGPicture(int channel, string fileName, ushort wPicSize)
            {
                new System.IO.FileInfo(fileName).Directory.Create();
                int value = 0;
                if (Dvr.Handle >= 0)
                {
                    if (Dvr.IsHik)
                        value = NetDvrDll.NET_DVR_CaptureJPEGPicture(Dvr.Handle, channel, fileName, wPicSize);
                    else
                        value = UsntDvrDll.NET_DVR_CaptureJPEGPicture(Dvr.Handle, channel, fileName, wPicSize);
                }
                return checkAndBack(value);
            }

            protected bool checkAndBack(int value)
            {
                if (value == 0)
                    error();
                return value != 0;
            }

            protected void error()
            {
                if (Dvr.Handle < 0)
                    throw new CanNotLoginException("设备登陆失败！");
                if(Dvr.IsHik)
                {
                    throw new CanNotControlExpection("Hik Error NO." + NetDvrDll.NET_DVR_GetLastError());
                }
                else
                {
                    throw new CanNotControlExpection("Usnt Error NO." + UsntDvrDll.USNTDVR_GetLastError());
                }
            }

            public virtual void Dispose()
            {
                Dvr.Dispose();
            }
        }
    }
}
