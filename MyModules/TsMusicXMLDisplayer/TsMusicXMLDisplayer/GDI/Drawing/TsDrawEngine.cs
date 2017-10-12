using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsDrawEngine
    {
        public TsDrawingEnviroment Enviroment { get; set; }
        public TsCursor Cursor { get; set; }
        public MusicalFont FontM { get; set; }
        //public D.Graphics Graphics { get; set; }
        public Elements.TsSymbols Symbols { get; set; }

        private TsMessageFont messageFont;

        public D.Font GetMesageFont(float percent)
        {
            if (messageFont == null)
                messageFont = new TsMessageFont(this);
            return messageFont.GetFont(percent);
        }

        public void Draw(TsGDIPaper paper)
        { 
            //Page Number
            var font = this.GetMesageFont(1.5f);
            string msg = paper.PageNumber+"";
            var size = paper.GetGraphics.MeasureString(msg, font);
            float centerx = Enviroment.MarginLeft + (Enviroment.W-Enviroment.MarginLeft-Enviroment.MarginRight)/2-size.Width/2;
            paper.GetGraphics.DrawString(msg, font, Enviroment.FiguresColor.Brush, centerx , Enviroment.H-Enviroment.MarginBottom);

            if (paper.PageNumber != 1) return;

            //Page Title
            font = this.GetMesageFont(2.5f);
            msg = paper.TheTsPartwise.Title;
            size = paper.GetGraphics.MeasureString(msg, font);
            centerx = Enviroment.MarginLeft + (Enviroment.W - Enviroment.MarginLeft - Enviroment.MarginRight) / 2 - size.Width / 2;
            paper.GetGraphics.DrawString(msg, font, Enviroment.FiguresColor.Brush, centerx, Enviroment.MarginTop);
            float hx = 2.5f*Enviroment.HeightSpace;
            //Page Title
            font = this.GetMesageFont(1.5f);
            msg = paper.TheTsPartwise.Autor;
            size = paper.GetGraphics.MeasureString(msg, font);
            centerx = Enviroment.W - Enviroment.MarginRight - size.Width;
            paper.GetGraphics.DrawString(msg, font, Enviroment.FiguresColor.Brush, centerx, Enviroment.MarginTop+hx);
        }

        public void Draw(TsMeasure measure, ITsDrawable element)
        {
            var cache = element.CacheGraphics;

            D.Graphics g = measure.Line.Paper.GetGraphics;

            if (measure != null)
            {
                ApplyRectangle(g,measure.Line.CacheGraphics[0].Rectangle, measure.Line.CacheGraphics[0].Origin, 1, false);

                //var rr = measure.Line.CacheGraphics[0].Rectangle;
                //g.DrawRectangle(D.Pens.Black, 0, 0, rr.Width, rr.Height);

                ApplyRectangle(g,measure.CacheGraphics[0].Rectangle,measure.CacheGraphics[0].Origin, 1);
            }

            foreach (var icache in cache)
            {
                if(!(element is TsMeasure))
                    ApplyRectangle(g,icache.Rectangle,icache.Origin, 1,element is TsMeasure);

                foreach (var i in icache.Lines)
                {
                    this.Enviroment.FiguresColor.Pen.Width = i.Width;
                    if(element is TsMeasure)
                    {
                        g.DrawLine(D.Pens.Black, i.X1, i.Y1, i.X2, i.Y2);
                        //Console.WriteLine(i.Y1);
                    }
                    else
                        g.DrawLine(this.Enviroment.FiguresColor.Pen, i.X1, i.Y1, i.X2, i.Y2);
                }

                foreach (var iarc in icache.Arcs)
                {
                    this.Enviroment.FiguresColor.Pen.Width = iarc.tickness;

                    for (int i = 0; i < 4; i++)
                    {
                        var rectangle = iarc.GetRectangle(i);
                        if (iarc.Direction == -1)
                            g.DrawArc(this.Enviroment.FiguresColor.Pen, rectangle, 180, 180);
                        else
                        {
                            g.DrawArc(this.Enviroment.FiguresColor.Pen, rectangle, 0, 180);
                        }
                    }
                }

                if (!(element is TsMeasure))
                    ApplyRectangle(g,icache.Rectangle,icache.Origin,  -1,  element is TsMeasure);

                D.Pen pen;

                if(element is TsMeasure)
                    pen = new D.Pen(D.Color.FromArgb(200,0,255,0), 2f);
                else
                    pen = new D.Pen(D.Color.FromArgb(200,255,0,0), 2f);


                pen.DashStyle = D.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 4,4};
                //pen.DashStyle = D.Drawing2D.DashStyle.Dot;                

                if((element is TsMeasure))
                    ApplyRectangle(g,measure.CacheGraphics[0].Rectangle, measure.CacheGraphics[0].Origin, -1);

                //g.DrawRectangle(pen, icache.Rectangle.X,icache.Rectangle.Y,icache.Rectangle.Width+1,icache.Rectangle.Height);

                if ((element is TsMeasure))
                    ApplyRectangle(g,measure.CacheGraphics[0].Rectangle, measure.CacheGraphics[0].Origin, 1);


                ApplyRectangle(g,icache.Rectangle,icache.Origin, 1,false);
                foreach (var i in icache.Characters)
                {
                    g.TextRenderingHint = D.Text.TextRenderingHint.AntiAlias;
                    //this.Graphics.DrawString(i.Symbol.Code + "", this.FontM.CurrentFont, this.Enviroment.FiguresColor.Brush, i.DeltaX, i.DeltaY, D.StringFormat.GenericTypographic);
                    g.DrawString(i.Symbol.Code + "", this.FontM.CurrentFont, this.Enviroment.FiguresColor.Brush, i.FinalX(FontM.FontHeight), i.FinalY(FontM.FontHeight), D.StringFormat.GenericTypographic);
                }
                foreach (var i in icache.Messages)
                {
                    D.RectangleF rect = i.Measure(g, this);
                    g.DrawString(i.Message, this.GetMesageFont(i.FontPercent), this.Enviroment.FiguresColor.Brush, rect.X,rect.Y, D.StringFormat.GenericTypographic);
                    //this.Graphics.DrawRectangle(D.Pens.Green, rect.X, rect.Y, rect.Width, rect.Height);
                }
                ApplyRectangle(g,icache.Rectangle,icache.Origin,  -1, false);                
            }

            if (measure != null)
            {
                ApplyRectangle(g,measure.CacheGraphics[0].Rectangle, measure.CacheGraphics[0].Origin, -1);
                ApplyRectangle(g,measure.Line.CacheGraphics[0].Rectangle, measure.Line.CacheGraphics[0].Origin, -1, false);
            }
        }

        public void ApplyRectangle(D.Graphics g, D.RectangleF rect,D.PointF origin, int multiply,bool center=true)
        {
            float xbase = rect.Left;
            float ybase = rect.Top;
            //float midh = rect.Height / 2.0f*(center?1:0);

            //this.Graphics.TranslateTransform(xbase*multiply, (ybase + midh)*multiply);            
            g.TranslateTransform(xbase * multiply, (ybase + origin.Y) * multiply);            
        }

        public float PositionOfSubLine(int idx)
        { 
            return Enviroment.HeightSpace / 2.0f * (-idx+4);
        }

        public float PositionOfLine(int line)
        {
            return PositionOfSubLine((line - 1) * 2);
        }


        public float SpacesSize(int spaces)
        {
            return Enviroment.HeightSpace * spaces;
        }
    }
}
