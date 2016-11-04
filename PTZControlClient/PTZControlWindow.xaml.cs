using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PTZControlClient
{
    /// <summary>
    /// PTZControlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PTZControlWindow : Window
    {
        public PTZControlViewModel ViewModel { get { return _model; } }
        PTZControlViewModel _model;
        public PTZControlWindow(PTZControlViewModel model)
        {
            InitializeComponent();
            _model = model;
            this.DataContext = _model;
            if (_model != null)
            {
                _model.PtzControl.PTZEvent += onPTZ;
            }
        }

        PTZControlService.PTZ _ptz;

        private void onPTZ(PTZControlService.PTZ ptz)
        {
            _ptz = ptz;
            lock (_objLock)
            {
                if (ptz != null && _ptzFeedbackForm != null)
                    _ptzFeedbackForm?.UpdatePTZ(ptz);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            (this.DataContext as PTZControlViewModel)?.Dispose();
            base.OnClosed(e);
            if (_ptzFeedbackForm != null)
                _ptzFeedbackForm.Close();
            _ptzFeedbackForm = null;
        }

        PTZFeedbackForm _ptzFeedbackForm = null;
        object _objLock = new object();
        private void PTZReplay_Click(object sender, RoutedEventArgs e)
        {
            lock (_objLock)
            {
                if (_ptzFeedbackForm != null)
                {
                    _ptzFeedbackForm.Close();
                    _ptzFeedbackForm = null;
                }
                _ptzFeedbackForm = new PTZFeedbackForm();
                _ptzFeedbackForm.Show();
            }
        }
    }
}
