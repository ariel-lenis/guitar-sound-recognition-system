using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsMessageFont
    {
        TsDrawEngine engine;
        PrivateFontCollection collection;
        public TsMessageFont(TsDrawEngine engine) 
        {
            this.engine = engine;
            collection = new PrivateFontCollection();

            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            collection = new PrivateFontCollection();
            collection.AddFontFile(path + "\\GDI\\fonts\\verdana.ttf");
        }
        public D.Font GetFont(float percent)
        {
            float size = percent * engine.Enviroment.HeightSpace;
            return new D.Font(collection.Families[0], CompleteFontPixels(size,collection.Families[0]), D.GraphicsUnit.Pixel);
        }
        public float CompleteFontPixels(float expectedpx, D.FontFamily fontF)
        {
            float ascent = fontF.GetCellAscent(D.FontStyle.Regular);
            float descent = fontF.GetCellDescent(D.FontStyle.Regular);
            float height = fontF.GetEmHeight(D.FontStyle.Regular);
            float linespacing = fontF.GetLineSpacing(D.FontStyle.Regular);
            float res = height / (ascent + descent) * expectedpx;
            //float res =  height/ (linespacing) * expectedpx;
            return res;
        }
    }
}
