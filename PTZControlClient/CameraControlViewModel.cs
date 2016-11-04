using AopUtil.WpfBinding;
using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PTZControlClient
{
    public class CameraControlViewModel: ObservableObject
    {
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
        public byte Speed { get; set; } = 0xff;
        public byte AuxIndex { get; set; }
        public byte PresetIndex { get; set; }
        public Action<CameraAction, byte> CameraControlEvent;
        public CameraControlViewModel()
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

        private ICommand newCommand(CameraAction action, byte index = 0)
        {
            return new Common.Command.DelegateCommand(_ => control(action, index));
        }

        private ICommand newCommand(CameraAction action, ControlType type)
        {
            return new Common.Command.DelegateCommand(_ => control(action, type));
        }

        private void control(CameraAction action, ControlType type)
        {
            control(action, getDataFromType(type));
        }

        private void control(CameraAction action, byte index)
        {
            var handler = CameraControlEvent;
            if (handler != null)
                handler(action, index);
        }

        byte getDataFromType(ControlType type)
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
