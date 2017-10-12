using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI
{
    public class MusicalFont
    {
        //35 98
        PrivateFontCollection privatefont;
        Font currentfont;
        public Font CurrentFont { get { return currentfont; } }
        float size;
        //float prop = 11f;

        float prop = 12.5f;

        float NegraPercent = 0.0888f;

        public float FontCenter { get { return size * prop*0.52f; } }
        public float FontBottom { get { return size * prop; } }
        public float LineSpace { get { return size; } }


        public float FontHeight 
        {
            get 
            {
                return size / NegraPercent;
            }
        }

        public MusicalFont()
        {            
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            privatefont = new PrivateFontCollection();
            privatefont.AddFontFile(path + "\\GDI\\fonts\\Musical.ttf");
        }
        public void SetSize(float size)
        {
            this.currentfont = GetFont(size);
            this.size = size;
        }
        private Font GetFontOld(D.Graphics g, float size)
        {
            size *= prop;
            //we need a font that fits the required size
            float b = 50;

            var tfont = new Font(privatefont.Families[0], b);
            
            SizeF sizef = g.MeasureString("ï", tfont);
            float scale = (size) / sizef.Height;

            //scale *= 12;

            tfont.Dispose();
            tfont = new Font(privatefont.Families[0], b * scale);
            SizeF sizef2 = g.MeasureString("ï", tfont);
            return tfont;
        }
        private Font GetFont( float size)
        {
            //size *= prop;
            //we need a font that fits the required size
            float b = size/NegraPercent;
            var tfont = new Font(privatefont.Families[0], CompleteFontPixels(b,privatefont.Families[0])  ,GraphicsUnit.Pixel);

            //SizeF sizef = g.MeasureString("ï", tfont);
            //float scale = (size) / sizef.Height;

            //scale *= 12;

            //tfont.Dispose();
            //tfont = new Font(privatefont.Families[0], b * scale);
            //SizeF sizef2 = g.MeasureString("ï", tfont);
            return tfont;
        }

        public float CompleteFontPixels(float expectedpx, FontFamily fontF)
        {
            float ascent = fontF.GetCellAscent(FontStyle.Regular);
            float descent = fontF.GetCellDescent(FontStyle.Regular);
            float height = fontF.GetEmHeight(FontStyle.Regular);
            float linespacing = fontF.GetLineSpacing(FontStyle.Regular);
            float res = height / (ascent + descent) * expectedpx;
            //float res =  height/ (linespacing) * expectedpx;
            return res;
        }

        public char GetAscii(Symbols symbol)
        {
            return (char)symbol;
        }

        public enum Symbols
        { 
            Sharp = 35,
            Bemol = 98        
        }
    }
}
