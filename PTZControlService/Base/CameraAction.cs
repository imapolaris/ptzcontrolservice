﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public enum CameraAction
    {
        StopPT = 0,
        Up,
        Down,
        Left,
        Right,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        AutoScan,

        AuxOn,
        AuxOff,
        StopZoom,
        ZoomIn,
        ZoomOut,
        StopFocus,
        FocusNear,
        FocusFar,
        StopIris,
        IrisOpen,
        IrisClose,

        SetPreset = 80,
        GoToPreset,
        ClearPreset
    }
}