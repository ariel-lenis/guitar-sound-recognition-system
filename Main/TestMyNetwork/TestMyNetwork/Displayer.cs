using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestMyNetwork
{
    public partial class Displayer : Form
    {
        PointF[] points;
        bool[] selectors;
        Bitmap bmp;
        Graphics gbmp;
        int w, h;
        public Displayer()
        {
            InitializeComponent();
        }
        public void  SetData(PointF[] points, bool[] selectors)
        {
            this.points = points;
            this.selectors = selectors;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Displayer_Load(object sender, EventArgs e)
        {
            regenBmp();
        }

        private void regenBmp()
        {
            //if (w == 0) return;
            w = pictureBox1.Width;
            h = pictureBox1.Height;
            if (w == 0) w = 1;
            if (h == 0) h = 1;
            bmp = new Bitmap(w, h);
            gbmp = Graphics.FromImage(bmp);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            drawAll(e.Graphics);
        }

        private void drawAll(Graphics graphics)
        {
            //if (gbmp == null) return;
            gbmp.FillRectangle(Brushes.Black,0,0,w,h);
            
            for (int i = 0;points!=null && i < points.Length; i++)
            {
                Color color = selectors[i] ? Color.Gold : Color.SlateGray;
                gbmp.FillEllipse(new SolidBrush(color), points[i].X-1, points[i].Y-1, 2, 2);
            }
            graphics.DrawImage(bmp,0,0);
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            regenBmp();
        }
    }
}
