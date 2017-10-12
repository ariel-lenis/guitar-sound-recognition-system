using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TsPentagramToolsTest
{
    public partial class WinBubble : Form
    {
        List<float> distances;
        List<PointF> points;
        float radio = 4f;
        public WinBubble(List<float> distances)
        {
            InitializeComponent();
            this.distances = distances;
            PreparePoints();
        }

        private void PreparePoints()
        {
            this.distances.Sort();
            points = new List<PointF>();
            
            for (int i = 0; i < distances.Count; i++)
                points.Add(SearchBest(distances[i]*500));        
        }

        private PointF SearchBest(float x)
        {
            float y = 4*radio;
            
            for (int i = 0; i < points.Count /*&& x<points[i].X+2*radio*/; i++)
            {
                if (Math.Abs(points[i].X - x) <= radio)
                {
                    float u = (points[i].X - x) * (points[i].X - x)+4*radio*radio;


                    y = points[i].Y + (float)Math.Sqrt(u);
                }
            }
             
            return new PointF(x, y);
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (this.distances == null) return;
            Draw(e.Graphics);
        }

        private void Draw(Graphics g)
        {
            g.Clear(Color.White);

            float h = this.pictureBox1.Height;

            foreach (var ipoint in points)
                g.DrawEllipse(Pens.Red, ipoint.X - radio,h- (ipoint.Y - radio), 2 * radio, 2 * radio);


        }
    }
}
