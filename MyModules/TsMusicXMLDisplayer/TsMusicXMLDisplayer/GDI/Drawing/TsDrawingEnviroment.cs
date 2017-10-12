using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsDrawingEnviroment
    {
        public int W { get; set; }
        public int H { get; set; }

        public float HeightSpace { get; set; }
        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }

        public ColorGroup LinesColor { get; set; }
        public ColorGroup FiguresColor { get; set; }
        public ColorGroup HighlightedColor { get; set; }

    }
}
