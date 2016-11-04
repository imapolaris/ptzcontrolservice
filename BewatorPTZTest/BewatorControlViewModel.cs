using AopUtil.WpfBinding;
using BewatorPTZTest.ScBewator;
using CCTVModels;
using PTZControlService;
using PTZControlService.Hikvision;
using PTZControlService.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BewatorPTZTest
{
    public class BewatorControlViewModel : ObservableObject, IDisposable
    {
        [AutoNotify]
        public TcpScBewatorSerial Serial { get; set; }
        [AutoNotify]
        public PTZConfig PtzConfig { get; set; }
        [AutoNotify]
        public PTZLimit PTZLimits { get; set; }
        [AutoNotify]
        public PTZ PTZPosition { get; set; }

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public BewatorControlViewModel()
        {
            PtzConfig = new PTZConfig();
            ConnectCommand = new DelegateCommand(_ => command());
            DisconnectCommand = new DelegateCommand(_ => discommand());
            SendCommand = new DelegateCommand(_ => send());
            initCameraControlCmd();
            initPromptCmd();
        }

        #region 连接
        private void command()
        {
            discommand();
            try
            {
                Serial = new ScBewator.TcpScBewatorSerial(PtzConfig.Ip, PtzConfig.Port, PtzConfig.CameraId, SerialType.Bewator, 37);
                //_ptzControl = new TcpBewatorSerialControl(PtzConfig.Ip, PtzConfig.Port, PtzConfig.CameraId, PtzConfig.ReverseZoom);
            }
            catch (CanNotLoginException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (CanNotOpenSerialException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (CanNotControlExpection ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void discommand()
        {
            if (Serial != null)
                Serial.Dispose();
            Serial = null;
            //if (_ptzControl != null)
            //    _ptzControl.Dispose();
            //_ptzControl = null;
            PTZLimits = null;
            PTZPosition = null;
        }
        #endregion 连接

        #region Send
        public ICommand SendCommand { get; set; }
        public string SendString { get; set; } = "FF-01-00-00-00-00-00";

        void send()
        {
            try
            {
                byte[] buffer = str2bytes(SendString);
                if (buffer.Length == 4)
                {
                    byte[] comp = new byte[7];
                    comp[0] = 0xFF;
                    comp[1] = PtzConfig.CameraId;
                    Array.Copy(buffer, 0, comp, 2, 4);
                    buffer = comp;
                }
                send(buffer);
                //_ptzControl.Send(buffer);
            }
            catch (Exception ex)
            {
                Prompt.Instance.OnStatus($"Send Error: {SendString}\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        void send(byte[] buffer)
        {
            checkDigit(buffer);
            Serial.Send(buffer);
        }

        void checkDigit(byte[] buffer)
        {
            byte sum = 0;
            for (int i = 1; i < buffer.Length - 1; i++)
                sum += buffer[i];
            buffer[buffer.Length - 1] = sum;
        }

        byte[] str2bytes(string str)
        {
            List<byte> list = new List<byte>();
            char[] separator = new char[] { ' ', '-' };
            string[] strList = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strList.Length; i++)
            {
                string curStr = strList[i];
                for (int j = 0; j < curStr.Length; j += 2)
                {
                    byte b = toByteFromChar16(curStr[j]);
                    if (j + 1 < curStr.Length)
                    {
                        b = (byte)(b * 16 + toByteFromChar16(curStr[j + 1]));
                    }
                    list.Add(b);
                }
            }
            return list.ToArray();
        }

        private byte toByteFromChar16(char c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');
            else if (c >= 'a' && c <= 'f')
                return (byte)(c - 'a' + 10);
            else if (c >= 'A' && c <= 'F')
                return (byte)(c - 'A' + 10);
            throw new InvalidOperationException("指令格式错误");
        }

        #endregion Send

        #region 云镜控制
        public ICommand LeftUpCommand { get; set; }
        public ICommand UpCommand { get; set; }
        public ICommand RightUpCommand { get; set; }
        public ICommand LeftCommand { get; set; }
        public ICommand RightCommand { get; set; }
        public ICommand LeftDownCommand { get; set; }
        public ICommand DownCommand { get; set; }
        public ICommand RightDownCommand { get; set; }
        public ICommand StopMoveCommand { get; set; }

        public ICommand StopZoomCommand { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand StopFocusCommand { get; set; }
        public ICommand FocusNearRCommand { get; set; }
        public ICommand FocusFarCommand { get; set; }
        public ICommand StopIrisCommand { get; set; }
        public ICommand IrisOpenCommand { get; set; }
        public ICommand IrisCloseCommand { get; set; }
        public ICommand AuxOnCommand { get; set; }
        public ICommand AuxOffCommand { get; set; }
        public ICommand SetPresetCommand { get; set; }
        public ICommand GoToPresetCommand { get; set; }
        public ICommand ClearPresetCommand { get; set; }

        [AutoNotify]
        public byte Speed { get; set; } = 0xff;
        public byte AuxIndex { get; set; }
        public byte PresetIndex { get; set; }

        private void initCameraControlCmd()
        {
            StopMoveCommand = newCommand(CameraAction.StopPT);
            LeftUpCommand = newCommand(CameraAction.LeftUp, ControlType.PTZ);
            UpCommand = newCommand(CameraAction.Up, ControlType.PTZ);
            RightUpCommand = newCommand(CameraAction.RightUp, ControlType.PTZ);
            LeftCommand = newCommand(CameraAction.Left, ControlType.PTZ);
            RightCommand = newCommand(CameraAction.Right, ControlType.PTZ);
            LeftDownCommand = newCommand(CameraAction.LeftDown, ControlType.PTZ);
            DownCommand = newCommand(CameraAction.Down, ControlType.PTZ);
            RightDownCommand = newCommand(CameraAction.RightDown, ControlType.PTZ);

            StopZoomCommand = newCommand(CameraAction.StopZoom);
            ZoomInCommand = newCommand(CameraAction.ZoomIn);
            ZoomOutCommand = newCommand(CameraAction.ZoomOut);
            StopFocusCommand = newCommand(CameraAction.StopFocus);
            FocusNearRCommand = newCommand(CameraAction.FocusNear);
            FocusFarCommand = newCommand(CameraAction.FocusFar);
            StopIrisCommand = newCommand(CameraAction.StopIris);
            IrisOpenCommand = newCommand(CameraAction.IrisOpen);
            IrisCloseCommand = newCommand(CameraAction.IrisClose);
            AuxOnCommand = newCommand(CameraAction.AuxOn, ControlType.AuxIndex);
            AuxOffCommand = newCommand(CameraAction.AuxOff, ControlType.AuxIndex);
            SetPresetCommand = newCommand(CameraAction.SetPreset, ControlType.PresetIndex);
            GoToPresetCommand = newCommand(CameraAction.GoToPreset, ControlType.PresetIndex);
            ClearPresetCommand = newCommand(CameraAction.ClearPreset, ControlType.PresetIndex);

        }
        #endregion 云镜控制

        #region 操作提示
        [AutoNotify]
        public string SendPrompt { get; set; }
        [AutoNotify]
        public string ReceivedPrompt { get; set; }
        [AutoNotify]
        public string AllPrompt { get; set; }
        [AutoNotify]
        public string StatusPrompt { get; set; }
        [AutoNotify]
        public bool IsAddRecrivePromptCmd { get; set; } = true;

        public ICommand ClearPromptCmd { get; set; }
        private void initPromptCmd()
        {
            ClearPromptCmd = new DelegateCommand(_ => clearPrompt());
            Prompt.Instance.SendEvent += onSend;
            Prompt.Instance.ReceivedEvent += onReceived;
            Prompt.Instance.StatusEvent += onStatus;
        }

        private void onStatus(string obj)
        {
            StatusPrompt += obj + "\n";
            AllPrompt += obj + "\n";
        }

        private void onSend(string obj)
        {
            SendPrompt += obj + "\n";
            AllPrompt += obj + "\n";
        }

        private void onReceived(string obj)
        {
            if (IsAddRecrivePromptCmd)
            {
                ReceivedPrompt += obj + "\n";
                AllPrompt += obj + "\n";
            }
        }

        private void clearPrompt()
        {
            AllPrompt = null;
            ReceivedPrompt = null;
            SendPrompt = null;
            StatusPrompt = null;
        }
        #endregion 操作提示

        private DelegateCommand newCommand(CameraAction action, byte index = 0)
        {
            return new DelegateCommand(_ => control(action, index));
        }

        private DelegateCommand newCommand(CameraAction action, ControlType type)
        {
            return new DelegateCommand(_ => control(action, type));
        }

        private void control(CameraAction action, ControlType type)
        {
            control(action, getData(type));
        }

        private void control(CameraAction action, byte index)
        {
            try
            {
                Serial.CameraControl(action, index);
            }
            catch (Exception ex)
            {
                Prompt.Instance.OnStatus(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }

        public void Dispose()
        {
            discommand();
        }

        byte getData(ControlType type)
        {
            switch (type)
            {
                case ControlType.AuxIndex:
                    return AuxIndex;
                case ControlType.PresetIndex:
                    return PresetIndex;
                case ControlType.PTZ:
                    return Speed;
                default:
                    return 0;
            }
        }

        enum ControlType
        {
            PTZ,
            PresetIndex,
            AuxIndex,
        }
    }
}
