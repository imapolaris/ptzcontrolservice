using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTZControlApi.Models
{
    public class CameraControlAction
    {
        public int Index;
        public string Action;
        public CameraControlAction(string action, int index)
        {
            Action = action;
            Index = index;
        }
    }

    public class CameraControlActionManager
    {
        public static CameraControlActionManager Instance { get; private set; } = new CameraControlActionManager();
        CameraControlActionManager()
        {
            initAction();
        }

        private void initAction()
        {
            foreach(CameraAction action in Enum.GetValues(typeof(CameraAction)))
            {
                _actions.Add(new CameraControlAction(action.ToString(), (int)action));
            }
        }

        List<CameraControlAction> _actions = new List<CameraControlAction>();
        public CameraControlAction[] Actions { get { return _actions.ToArray(); } }
    }
}