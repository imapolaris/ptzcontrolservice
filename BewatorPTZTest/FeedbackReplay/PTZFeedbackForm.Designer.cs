namespace BewatorPTZTest
{
    partial class PTZFeedbackForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lineChartControlPan = new BewatorPTZTest.LineChartControl();
            this.lineChartControlTilt = new BewatorPTZTest.LineChartControl();
            this.lineChartControlZoom = new BewatorPTZTest.LineChartControl();
            this.SuspendLayout();
            // 
            // lineChartControlPan
            // 
            this.lineChartControlPan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lineChartControlPan.Location = new System.Drawing.Point(0, 0);
            this.lineChartControlPan.Name = "lineChartControlPan";
            this.lineChartControlPan.Size = new System.Drawing.Size(918, 251);
            this.lineChartControlPan.TabIndex = 0;
            // 
            // lineChartControlTilt
            // 
            this.lineChartControlTilt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lineChartControlTilt.Location = new System.Drawing.Point(0, 251);
            this.lineChartControlTilt.Name = "lineChartControlTilt";
            this.lineChartControlTilt.Size = new System.Drawing.Size(918, 251);
            this.lineChartControlTilt.TabIndex = 3;
            // 
            // lineChartControlZoom
            // 
            this.lineChartControlZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lineChartControlZoom.Location = new System.Drawing.Point(0, 498);
            this.lineChartControlZoom.Name = "lineChartControlZoom";
            this.lineChartControlZoom.Size = new System.Drawing.Size(918, 251);
            this.lineChartControlZoom.TabIndex = 4;
            // 
            // PTZFeedbackForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(919, 752);
            this.Controls.Add(this.lineChartControlZoom);
            this.Controls.Add(this.lineChartControlTilt);
            this.Controls.Add(this.lineChartControlPan);
            this.Name = "PTZFeedbackForm";
            this.Text = "PTZFeedbackForm";
            this.ResumeLayout(false);

        }

        #endregion

        private LineChartControl lineChartControlTilt;
        private LineChartControl lineChartControlZoom;
        private LineChartControl lineChartControlPan;
    }
}