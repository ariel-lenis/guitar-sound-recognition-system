using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsCacheGraphics
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int SubLine { get; set; }
        public D.RectangleF Rectangle { get; set; }

        public List<TsCacheCharacter> Characters{get;set;}
        public List<TsCacheLine> Lines{get;set;}
        public List<TsMessage> Messages { get; set; }

        public List<TsCacheArc> Arcs { get; set; }

        public D.PointF Origin { get; set; }

        public TsCacheGraphics()
        {
            Characters = new List<TsCacheCharacter>();
            Lines = new List<TsCacheLine>();
            Messages = new List<TsMessage>();
            Arcs = new List<TsCacheArc>();
        }

        public D.RectangleF CharactersRectangle(float h)
        {
            Tools.TsSuperRectangle super = new Tools.TsSuperRectangle();

            for (int i = 0; i < this.Characters.Count;i++ )
            {
                TsCacheCharacter ichar = this.Characters[i];
                D.RectangleF irect = ichar.FinalRectangle(h);
                super.AddRectangle(irect);
            }            

            return super.Rectangle;
        }

        public void MoveAll(float dx, float dy)
        {
            foreach (var i in this.Characters)
            { 
                i.DeltaX += dx;
                i.DeltaY += dy;
            }
            foreach (var i in this.Lines)
            {
                i.X1 += dx;
                i.Y1 += dy;
                i.X2 += dx;
                i.Y2 += dy;
            }                
        }

    }

    public class TsMessage 
    {
        public string Message { get; set; }
        public float ReferenceX { get; set; }

        public float ReferenceY { get; set; }
        public enum EHorizontalAlignment { Left, Center, Right };
        public enum EVerticalAlignment { Top, Center, Bottom };
        public EHorizontalAlignment HorizontalAlignment { get; set; }
        public EVerticalAlignment VerticalAlignment { get; set; }
        public float FontPercent { get; set; }

        public D.RectangleF Measure(D.Graphics g,Drawing.TsDrawEngine param)
        {
            D.Font font = param.GetMesageFont(this.FontPercent);
            D.SizeF size = g.MeasureString(Message,font,new D.PointF(),D.StringFormat.GenericTypographic);
            float x=0, y=0;
            
            switch(this.HorizontalAlignment)
            {
                case EHorizontalAlignment.Left:
                    x = ReferenceX;
                    break;
                case EHorizontalAlignment.Center:
                    x = ReferenceX - size.Width / 2;
                    break;
                case EHorizontalAlignment.Right:
                    x = ReferenceX - size.Width;
                    break;
            }

            switch (this.VerticalAlignment)
            { 
                case EVerticalAlignment.Top:
                    y = ReferenceY;
                    break;
                case EVerticalAlignment.Center:
                    y = ReferenceY - size.Height / 2;
                    break;
                case EVerticalAlignment.Bottom:
                    y = ReferenceY - size.Height;
                    break;
            }

            return new D.RectangleF(x, y, size.Width, size.Height);
        }

    }

    public class TsCacheCharacter
    {
        public Elements.TsSymbols.TsSymbolProperties Symbol { get; set; }
        public float DeltaX { get; set; }
        public float DeltaY { get; set; }

        public float FinalX (float h)
        {
            return DeltaX - (0*Symbol.LPercent+Symbol.CenterX) * h;
        }
        public float FinalY(float h)
        {
            return DeltaY - (0*Symbol.TPercent + Symbol.CenterY) * h;
        }

        public float FinalW(float h)
        {
            return (this.Symbol.RPercent - this.Symbol.LPercent) * h + (0 * Symbol.LPercent + Symbol.CenterX) * h;
        }

        public float FinalH(float h)
        {
            return (this.Symbol.BPercent - this.Symbol.TPercent) * h;
        }

        public D.RectangleF FinalRectangle(float h)
        {
            return new D.RectangleF() { 
                X=this.FinalX(h),
                Y=this.FinalY(h),
                Width=this.FinalW(h),
                Height=this.FinalH(h)
            };
        }
    }
    public class TsCacheLine 
    {
        public TsCacheLine()
        {
            this.Width = 1;
        }
        public float Width { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2{ get; set; }
    }

    public class TsCacheArc
    {
        public float H;
        public float tickness;

        public int Direction;
        public TsCacheArc()
        {
            this.tickness = 1;
        }

        public D.RectangleF GetRectangle(float hpadding = 0)
        {
            float hh = this.H + hpadding;
            return new D.RectangleF(this.X1, this.Y1 - hh, this.X2-this.X1, this.Y2-this.Y1 + 2*hh);
        }

        public float X1;
        public float Y1;

        public float X2;
        public float Y2;
    }
}
