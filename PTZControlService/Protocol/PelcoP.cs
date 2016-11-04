using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    public class PelcoP
    {
        public byte AddrId { get; private set; }
        public bool ReverseZoom { get; private set; }
        
        public PelcoP(byte addrId, bool reverseZoom)
        {
            AddrId = addrId;
            ReverseZoom = reverseZoom;
        }

        /// <param name="action">控制类型</param>
        /// <param name="actIndex">控制值</param>
        public byte[] CameraControl(CameraAction action, int actIndex)
        {
            byte bIndex = (byte)Math.Max(0, Math.Min(0xFF, actIndex));
            byte data1 = 0x00;
            byte data2 = 0x00;
            byte data3 = 0x00;
            byte data4 = 0x00;
            switch (action)
            {
                case CameraAction.AutoScan:
                    data1 = 0x10;
                    data3 = bIndex;
                    break;
                case CameraAction.Up:
                    data2 = 0x08;
                    data4 = bIndex;
                    break;
                case CameraAction.Down:
                    data2 = 0x10;
                    data4 = bIndex;
                    break;
                case CameraAction.Left:
                    data2 = 0x04;
                    data3 = bIndex;
                    break;
                case CameraAction.Right:
                    data2 = 0x02;
                    data3 = bIndex;
                    break;
                case CameraAction.LeftUp:
                    data2 = 0x0C;
                    data3 = bIndex;
                    data4 = bIndex;
                    break;
                case CameraAction.RightUp:
                    data2 = 0x0A;
                    data3 = bIndex;
                    data4 = bIndex;
                    break;
                case CameraAction.LeftDown:
                    data2 = 0x14;
                    data3 = bIndex;
                    data4 = bIndex;
                    break;
                case CameraAction.RightDown:
                    data2 = 0x12;
                    data3 = bIndex;
                    data4 = bIndex;
                    break;
                case CameraAction.ZoomOut:
                    data2 = (byte)(ReverseZoom ? 0x20 : 0x40);
                    break;
                case CameraAction.ZoomIn:
                    data2 = (byte)(ReverseZoom ? 0x40 : 0x20);
                    break;
                case CameraAction.FocusNear:
                    data2 = 0x80;
                    break;
                case CameraAction.FocusFar:
                    data1 = 0x01;
                    break;
                case CameraAction.IrisClose:
                    data1 = 0x02;
                    break;
                case CameraAction.IrisOpen:
                    data1 = 0x04;
                    break;
                case CameraAction.AuxOff:
                    data2 = 0x0b;
                    data4 = bIndex;
                    break;
                case CameraAction.AuxOn:
                    data2 = 0x09;
                    data4 = bIndex;
                    break;
                case CameraAction.GoToPreset:
                    data2 = 0x07;
                    data4 = bIndex;
                    break;
                case CameraAction.SetPreset:
                    data2 = 0x03;
                    data4 = bIndex;
                    break;
                case CameraAction.ClearPreset:
                    data2 = 0x05;
                    data4 = bIndex;
                    break;
                case CameraAction.StopPT:
                case CameraAction.StopZoom:
                case CameraAction.StopFocus:
                case CameraAction.StopIris:
                    break;
                default:
                    return null;
            }
            return getBuffer(AddrId, data1, data2, data3, data4);
        }

        static byte[] getBuffer(byte camId, params byte[] datas)
        {
            byte[] buffer = new byte[3 + datas.Length];
            buffer[0] = 0xA0;
            buffer[1] = camId;
            Array.Copy(datas, 0, buffer, 2, datas.Length);
            buffer[buffer.Length - 1] = 0;
            for (int i = 1; i < buffer.Length - 1; i++)
                buffer[buffer.Length - 1] += buffer[i];
            return buffer;
        }
    }
}
