using PTZControlService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BewatorPTZTest
{
    public partial class PTZFeedbackForm : Form
    {
        public PTZFeedbackForm()
        {
            InitializeComponent();
        }

        public void UpdatePTZ(PTZ ptz)
        {
            if (ptz == null)
                return;
            lineChartControlPan.AddValue((int)(ptz.Pan * 10));
            lineChartControlTilt.AddValue((int)(ptz.Tilt * 10));
            lineChartControlZoom.AddValue((int)(ptz.Zoom * 10));
        }

        private void lineChartControlPan_Load(object sender, EventArgs e)
        {

        }
    }
}
