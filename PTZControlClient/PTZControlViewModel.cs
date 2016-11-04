using AopUtil.WpfBinding;
using CCTVModels;
using PTZControlService;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PTZControlClient
{
    public class PTZControlViewModel : ObservableObject, IDisposable
    {
        public CameraControlViewModel CameraControl { get; private set; }
        public IControl PtzControl { get; private set; } = null;
        ObservableCollection<PTZMode> _ptzModes { get; set; }
        public CollectionViewSource PTZModesSource { get; set; }
        public PTZControlConfig Config { get; set; }
        public ICommand ConnectCommand { get; set; }
        [AutoNotify]
        public PTZLimit PTZLimits { get; set; }
        [AutoNotify]
        public PTZ PTZPosition { get; set; }

        AutoResetEvent _disposeEvent = new AutoResetEvent(false); 

        public PTZControlViewModel()
        {
            initPtzModes();
            loadConfig();
            ConnectCommand = new Common.Command.DelegateCommand(_ => onConnect());
            CameraControl = new CameraControlViewModel();
            CameraControl.CameraControlEvent += onCameraControl;
            initGoToAndSnapshot();
            initTrack();
        }

        #region 定点转动、定点抓拍
        public double ExpPan { get; set; } = 50;
        public double ExpTilt { get; set; } = 10;
        public double ExpZoom { get; set; } = 5;
        public ICommand StartMoveCmd { get; set; }
        void initGoToAndSnapshot()
        {
            StartMoveCmd = new Common.Command.DelegateCommand(_ => startMove());
        }
        
        private void startMove()
        {
            try
            {
                PtzControl.ToPTZ(ExpPan, ExpTilt, ExpZoom);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }
        #endregion 定点转动、定点抓拍

        private void updatePTZs()
        {
            while(!_disposeEvent.WaitOne(1000))
            {
                if(PTZLimits == null)
                    PTZLimits = PtzControl?.PTZLimits;
            }
        }

        private void initPtzModes()
        {
            _ptzModes = new ObservableCollection<PTZMode>();
            foreach (PTZMode mode in Enum.GetValues(typeof(PTZMode)))
            {
                _ptzModes.Add(mode);
            }
            PTZModesSource = new CollectionViewSource();
            PTZModesSource.Source = _ptzModes;
        }

        public void Open()
        {
            save();
            onConnect();
        }
        
        private void onConnect()
        {
            Disconnect();
            _disposeEvent.Reset();
            new Thread(updatePTZs) { IsBackground = true }.Start();
            Console.WriteLine("云镜控制类别：" + Config.Selected);
            PtzControl = Config.GetNewControl();
            PtzControl.PTZEvent += onPtz;
        }

        void onPtz(PTZ ptz)
        {
            PTZPosition = ptz;
        }

        public void Disconnect()
        {
            _disposeEvent.Set();
            if(PtzControl != null)
            {
                PtzControl.PTZEvent -= onPtz;
                PtzControl.Dispose();
                PtzControl = null;
            }
            PTZLimits = null;
            PTZPosition = null;
        }
        
        void onCameraControl(CameraAction action, byte index)
        {
            try
            {
                PtzControl?.CameraControl(action, index);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        
        #region 云台配置文件读写
        private void save()
        {
            try
            {
                ConfigFile<PTZControlConfig>.SaveToFile("manifist.xml", Config);
            }
            catch(Exception ex)
            {
                Common.Log.Logger.Default.Error("配置单写入错误！" + ex.ToString());
            }
        }

        private void loadConfig()
        {
            Config = new PTZControlConfig();
            try
            {
                Config = ConfigFile<PTZControlConfig>.FromFile("manifist.xml");
            }
            catch(Exception ex)
            {
                Common.Log.Logger.Default.Error("配置单读取错误！" + ex.ToString());
            }
        }
        #endregion 云台配置文件读写

        [AutoNotify]
        public TestTargetTracker TargetTracker { get; set; }
        public ICommand TrackTargetCmd { get; set; }
        public ICommand StopTrackCmd { get; set; }
        public ICommand DifFeedbackOpenCmd { get; set; }
        [AutoNotify]
        public CCTVStaticInfo CamaraStaticInfo { get; set; }
        [AutoNotify]
        public DynamicTarget Ship { get; set; }
        void initTrack()
        {
            CamaraStaticInfo = TestCamaraConfig.CamaraStaticInfo;
            Ship = new DynamicTarget("IDString", "0", "船舶名称", 116.363019, 40.006211, 7, 260, 511, 60, 20, 0, 0, DateTime.Now);
            TrackTargetCmd = new Common.Command.DelegateCommand(_ => trackTarget());
            StopTrackCmd = new Common.Command.DelegateCommand(_ => stopTrack());
            DifFeedbackOpenCmd = new Common.Command.DelegateCommand(_ => openDifPtzFeedbackWin());
        }

        private void trackTarget()
        {
            stopTrack();
            Ship.Time = DateTime.Now;
            TargetTracker = new TestTargetTracker(PtzControl as PTZControl, CamaraStaticInfo, Ship);
            TargetTracker.PropertyChanged += TargetTracker_PropertyChanged;
        }

        private void stopTrack()
        {
            if (TargetTracker != null)
            {
                TargetTracker.PropertyChanged -= TargetTracker_PropertyChanged;
                TargetTracker.Dispose();
            }
            TargetTracker = null;
        }

        public void Dispose()
        {
            disposeForm();
            stopTrack();
            Disconnect();
        }


        private void TargetTracker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TargetTracker.ExpPtz):
                    lock (_objLock2)
                    {
                        var ptz = PTZPosition;
                        var exp = TargetTracker?.ExpPtz;
                        if (ptz != null && exp != null && _difptzFeedbackWin != null)
                        {
                            double difPan = ptz.Pan - exp.Pan;
                            if (difPan >= 180)
                                difPan -= 360;
                            else if(difPan < -180)
                                difPan += 360;
                            _difptzFeedbackWin?.UpdatePTZ(new PTZControlService.PTZ(difPan, ptz.Tilt - exp.Tilt, ptz.Zoom - exp.Zoom));
                        }
                    }
                    break;
            }
        }

        PTZFeedbackForm _difptzFeedbackWin = null;
        object _objLock2 = new object();

        private void openDifPtzFeedbackWin()
        {
            lock (_objLock2)
            {
                disposeForm();
                _difptzFeedbackWin = new PTZFeedbackForm();
                _difptzFeedbackWin.Show();
            }
        }

        void disposeForm()
        {
            if (_difptzFeedbackWin != null)
            {
                _difptzFeedbackWin.Close();
                _difptzFeedbackWin = null;
            }
        }
    }
}