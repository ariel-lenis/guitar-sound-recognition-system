using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsCursor
    {
        public int Row { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        TsDrawingEnviroment enviroment;
        public TsCursor(TsDrawingEnviroment enviroment)
        {
            this.enviroment = enviroment;
        }

    }
}
