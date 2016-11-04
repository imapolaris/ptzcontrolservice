using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PTZControlClient
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

        Pen _linePen = new Pen(Color.Blue, 2);
        Pen _averageLinePen = new Pen(Color.Green, 2){ DashPattern = new float[] { 8, 10 } };
        public bool IsShowAverageLine = true;

        void onPaint(Graphics g, Rectangle clientRect)
        {
            g.FillRectangle(Brushes.LightGray, clientRect);

            Rectangle frameRect = new Rectangle(clientRect.X + 40, clientRect.Y, clientRect.Width - 41, clientRect.Height - 1);
            g.DrawRectangle(Pens.Black, frameRect);
            g.DrawEllipse(new Pen(Color.Green, 20), new RectangleF(-10, -10, 20, 20));
            //g.DrawEllipse(new Pen(Color.Green, 20), new RectangleF(80, 80, 30, 30));
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

                float average = (float)values.Average();
                var aveY = frameRect.Bottom - (float)(average - min) / (max - min) * frameRect.Height;
                g.DrawString(Math.Round(average,1).ToString(), SystemFonts.DefaultFont, Brushes.Green, clientRect.X, aveY);

                PointF[] points = values.Select((value, index) =>
                {
                    float x = (float)frameRect.Width / _maxValueCount * index + frameRect.Left;
                    float y = frameRect.Bottom - (float)(value - min) / (max - min) * frameRect.Height;
                    return new PointF(x, y);
                }).ToArray();
                if (points.Length > 1)
                {
                    g.DrawLines(_linePen, points);
                    foreach (var pt in points)
                        g.DrawEllipse(new Pen(Color.Blue, 3), new RectangleF(pt.X - 1, pt.Y - 1, 3, 3));
                }
                if(IsShowAverageLine)
                {
                    PointF[] pointsAverage = values.Select((value, index) =>
                    {
                        float x = (float)frameRect.Width / _maxValueCount * index + frameRect.Left;
                        float y = aveY;
                        return new PointF(x, aveY);
                    }).ToArray();
                    if (points.Length > 1)
                        g.DrawLines(_averageLinePen, pointsAverage);
                }
            }
        }

        Queue<int> _valueQueue = new Queue<int>();
        const int _maxValueCount = 600;

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
