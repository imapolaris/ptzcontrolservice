using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BewatorPTZTest
{
    public partial class LineChartControl : UserControl
    {
        public LineChartControl()
        {
            InitializeComponent();
        }
        
        private void LineChartControl_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (DesignMode)
                base.OnPaintBackground(e);
        }

        private void LineChartControl_Paint(object sender, PaintEventArgs e)
        {
            Rectangle clientRect = this.ClientRectangle;
            using (Bitmap bitmap = new Bitmap(clientRect.Width, clientRect.Height, e.Graphics))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                    onPaint(g, clientRect);
                e.Graphics.DrawImage(bitmap, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
        }

        Pen _linePen = new Pen(Color.Blue, 3);

        void onPaint(Graphics g, Rectangle clientRect)
        {
            g.FillRectangle(Brushes.LightGray, clientRect);

            Rectangle frameRect = new Rectangle(clientRect.X + 40, clientRect.Y, clientRect.Width - 41, clientRect.Height - 1);
            g.DrawRectangle(Pens.Black, frameRect);

            int[] values = _valueQueue.ToArray();
            if (values.Length > 0)
            {
                int maxValue = values.Max();
                int minValue = values.Min();
                int min = minValue;
                int max = maxValue;
                if (max == min)
                    max++;
                g.DrawString(max.ToString(), SystemFonts.DefaultFont, Brushes.Black, clientRect.X, clientRect.Y);
                g.DrawString(min.ToString(), SystemFonts.DefaultFont, Brushes.Black, clientRect.X, clientRect.Bottom - 20);

                PointF[] points = values.Select((value, index) =>
                {
                    float x = (float)frameRect.Width / _maxValueCount * index + frameRect.Left;
                    float y = frameRect.Bottom - (float)(value - min) / (max - min) * frameRect.Height;
                    return new PointF(x, y);
                }).ToArray();
                if (points.Length > 1)
                    g.DrawLines(_linePen, points);
            }
        }

        Queue<int> _valueQueue = new Queue<int>();
        const int _maxValueCount = 1000;

        public void AddValue(int value)
        {
            _valueQueue.Enqueue(value);
            if (_valueQueue.Count > _maxValueCount)
                _valueQueue.Dequeue();

            Invalidate();
        }

        public void Clear()
        {
            _valueQueue.Clear();
        }
    }
}
