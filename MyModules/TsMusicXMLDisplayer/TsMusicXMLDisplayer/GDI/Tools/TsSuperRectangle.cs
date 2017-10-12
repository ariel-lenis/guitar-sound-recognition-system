using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Tools
{
    public class TsSuperRectangle
    {
        float x1, y1, x2, y2;
        bool first;
        public TsSuperRectangle()
        {
            first = true;
        }

        public float OriginY { get { return Math.Abs(y1); } }
       
        public void AddRectangle(D.RectangleF r)
        {
            if (first)
            {
                this.x1 = r.X;
                this.y1 = r.Y;
                this.x2 = r.Right;
                this.y2 = r.Bottom;
                first = false;
            }
            else
            {
                if (r.X < this.x1) this.x1 = r.X;
                if (r.Y < this.y1) this.y1 = r.Y;
                if (r.Right > this.x2) this.x2 = r.Right;
                if (r.Bottom > this.y2) this.y2 = r.Bottom;
            }
        }

       

        public D.RectangleF Rectangle 
        {
            get 
            {
                return new D.RectangleF(x1, y1, x2 - x1, y2 - y1);
            }
        }



        internal void AddRectangle(D.RectangleF r, D.PointF c)
        {
            r.X -= c.X;
            r.Y -= c.Y;
            this.AddRectangle(r);
        }
    }
}
