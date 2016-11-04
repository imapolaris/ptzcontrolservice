using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public static class PTZCommand
    {
         public static object Parse(CameraAction action, int data)
        {
            PTZCmdTable command = 0;
            bool isStop = false;
            uint speed = 0;
            PTZCmdTable presetCommand = 0;
            uint spd = (uint)Math.Min(Math.Max(1, data * 7 / 255), 7);
            if (!checkAuxValid(action, ref data))
                return null;
            switch(action)
            {
                case CameraAction.StopPT:
                    command = PTZCmdTable.UP_LEFT;
                    isStop = true;
                    break;
                case CameraAction.Up:
                    command = PTZCmdTable.TILT_UP;
                    speed = spd;
                    break;
                case CameraAction.Down:
                    command = PTZCmdTable.TILT_DOWN;
                    speed = spd;
                    break;
                case CameraAction.Left:
                    command = PTZCmdTable.PAN_LEFT;
                    speed = spd;
                    break;
                case CameraAction.Right:
                    command = PTZCmdTable.PAN_RIGHT;
                    speed = spd;
                    break;
                case CameraAction.LeftUp:
                    command = PTZCmdTable.UP_LEFT;
                    speed = spd;
                    break;
                case CameraAction.RightUp:
                    command = PTZCmdTable.UP_RIGHT;
                    speed = spd;
                    break;
                case CameraAction.LeftDown:
                    command = PTZCmdTable.DOWN_LEFT;
                    speed = spd;
                    break;
                case CameraAction.RightDown:
                    command = PTZCmdTable.DOWN_RIGHT;
                    speed = spd;
                    break;
                case CameraAction.AutoScan:
                    command = PTZCmdTable.PAN_AUTO;
                    isStop = data != 0;
                    break;
                case CameraAction.StopZoom:
                    command = PTZCmdTable.ZOOM_IN;
                    isStop = true;
                    break;
                case CameraAction.ZoomIn:
                    command = PTZCmdTable.ZOOM_IN;
                    break;
                case CameraAction.ZoomOut:
                    command = PTZCmdTable.ZOOM_OUT;
                    break;
                case CameraAction.StopIris:
                    command = PTZCmdTable.IRIS_OPEN;
                    isStop = true;
                    break;
                case CameraAction.IrisOpen:
                    command = PTZCmdTable.IRIS_OPEN;
                    break;
                case CameraAction.IrisClose:
                    command = PTZCmdTable.IRIS_CLOSE;
                    break;
                case CameraAction.StopFocus:
                    command = PTZCmdTable.FOCUS_NEAR;
                    isStop = true;
                    break;
                case CameraAction.FocusNear:
                    command = PTZCmdTable.FOCUS_NEAR;
                    break;
                case CameraAction.FocusFar:
                    command = PTZCmdTable.FOCUS_FAR;
                    break;
                case CameraAction.AuxOn:
                    command = (PTZCmdTable)data;
                    break;
                case CameraAction.AuxOff:
                    command = (PTZCmdTable)data;
                    isStop = true;
                    break;
                case CameraAction.SetPreset:
                    presetCommand = PTZCmdTable.SET_PRESET;
                    break;
                case CameraAction.GoToPreset:
                    presetCommand = PTZCmdTable.GOTO_PRESET;
                    break;
                case CameraAction.ClearPreset:
                    presetCommand = PTZCmdTable.CLE_PRESET;
                    break;
                default:
                    return null;
            }
            if (command != 0)
                return new PTZControlWithSpeedConfig()
                {
                    PTZCommand = (uint)(int)command,
                    Stop = (uint)(isStop? 0x01:0x00),
                    Speed = speed,
                };
            if (presetCommand != 0)
                return new PTZPresetConfig()
                {
                    PTZPresetCmd = (uint)(int)presetCommand,
                    PresetIndex = (uint)data,
                };
            return null;
        }

        private static bool checkAuxValid(CameraAction action, ref int data)
        {
            if (action == CameraAction.AuxOff || action == CameraAction.AuxOn)
            {
                switch((AuxPowers)data)
                {
                    case AuxPowers.Wiper:
                        data = (int)PTZCmdTable.WIPER_PWRON;
                        break;
                    case AuxPowers.Light:
                        data = (int)PTZCmdTable.LIGHT_PWRON;
                        break;
                    case AuxPowers.Heater:
                        data = (int)PTZCmdTable.HEATER_PWRON;
                        break;
                    case AuxPowers.Fan:
                        data = (int)PTZCmdTable.FAN_PWRON;
                        break;
                    case AuxPowers.Aux1:
                        data = (int)PTZCmdTable.AUX_PWRON1;
                        break;
                    case AuxPowers.Aux2:
                        data = (int)PTZCmdTable.AUX_PWRON2;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            return true;
        }
    }

    public class PTZControlWithSpeedConfig
    {
        public uint PTZCommand; //云台控制指令
        public uint Stop;       //云台停止动作或开始动作：0－开始；1－停止 
        public uint Speed;      //云台控制的速度，用户按不同解码器的速度控制值设置。取值范围[1,7] 
    }

    public class PTZPresetConfig
    {
        public uint PTZPresetCmd;
        public uint PresetIndex;
    }

    public enum PTZCmdTable
    {
        LIGHT_PWRON = 2, // 接通灯光电源
        WIPER_PWRON = 3,// 接通雨刷开关
        FAN_PWRON = 4,//接通风扇开关
        HEATER_PWRON = 5,//接通加热器开关
        AUX_PWRON1 = 6,//接通辅助设备开关
        AUX_PWRON2 = 7,// 接通辅助设备开关
        ZOOM_IN = 11,//焦距变大(倍率变大) 
        ZOOM_OUT = 12,//焦距变小(倍率变小) 
        FOCUS_NEAR = 13,// 焦点前调
        FOCUS_FAR = 14,//焦点后调
        IRIS_OPEN = 15,//光圈扩大
        IRIS_CLOSE = 16,//光圈缩小
        TILT_UP = 21,//云台上仰
        TILT_DOWN = 22,//云台下俯
        PAN_LEFT = 23,//云台左转
        PAN_RIGHT = 24,//云台右转
        UP_LEFT = 25,//云台上仰和左转
        UP_RIGHT = 26,//云台上仰和右转
        DOWN_LEFT = 27,//云台下俯和左转
        DOWN_RIGHT = 28,//云台下俯和右转
        PAN_AUTO = 29,//云台左右自动扫描
        TILT_DOWN_ZOOM_IN = 58,//云台下俯和焦距变大(倍率变大) 
        TILT_DOWN_ZOOM_OUT = 59,//云台下俯和焦距变小(倍率变小) 
        PAN_LEFT_ZOOM_IN = 60,//云台左转和焦距变大(倍率变大) 
        PAN_LEFT_ZOOM_OUT = 61,//云台左转和焦距变小(倍率变小) 
        PAN_RIGHT_ZOOM_IN = 62,//云台右转和焦距变大(倍率变大) 
        PAN_RIGHT_ZOOM_OUT = 63,//云台右转和焦距变小(倍率变小) 
        UP_LEFT_ZOOM_IN = 64,//云台上仰和左转和焦距变大(倍率变大) 
        UP_LEFT_ZOOM_OUT = 65,//云台上仰和左转和焦距变小(倍率变小) 
        UP_RIGHT_ZOOM_IN = 66,//云台上仰和右转和焦距变大(倍率变大) 
        UP_RIGHT_ZOOM_OUT = 67,//云台上仰和右转和焦距变小(倍率变小) 
        DOWN_LEFT_ZOOM_IN = 68,//云台下俯和左转和焦距变大(倍率变大) 
        DOWN_LEFT_ZOOM_OUT = 69,//云台下俯和左转和焦距变小(倍率变小) 
        DOWN_RIGHT_ZOOM_IN = 70,//云台下俯和右转和焦距变大(倍率变大) 
        DOWN_RIGHT_ZOOM_OUT = 71,//云台下俯和右转和焦距变小(倍率变小) 
        TILT_UP_ZOOM_IN = 72,//云台上仰和焦距变大(倍率变大) 
        TILT_UP_ZOOM_OUT = 73,// 云台上仰和焦距变小(倍率变小) 


        SET_PRESET = 8,// 设置预置点
        CLE_PRESET = 9,//清除预置点
        GOTO_PRESET = 39,//转到预置点
    }
}
