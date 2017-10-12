using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class ColorGroup
    {
        D.Color color;
        D.Pen pen;
        D.Brush brush;

        public D.Color Color
        {
            get { return color; }
            set
            {
                ChangeColor(value);
            }
        }
        public D.Pen Pen
        {
            get { return pen; }
        }
        public D.Brush Brush
        {
            get { return brush; }
        }

        private void ChangeColor(D.Color color)
        {
            this.color = color;
            this.pen = new D.Pen(color);
            this.brush = new D.SolidBrush(color);
        }

        public ColorGroup(D.Color color)
        {
            ChangeColor(color);
        }
    }
}
