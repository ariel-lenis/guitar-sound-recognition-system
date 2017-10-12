using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsClef:TsElement
    {
        public enum EClef { F, G, C };
        public EClef Clef { get; set; }
        public int Line { get; set; }
        public int Octave { get; set; }



        public TsNoteData NoteData
        {
            get 
            {
                return new TsNoteData() { Alter= TsNoteData.Ealter.Normal, Note=this.Note, Octave = this.Octave };
            }
        }

        public TsClef(TsMeasure measure)
            : base(measure)
        { 
        
        }



        public Elements.TsNoteData.Enote Note
        {
            get
            {
                switch (Clef)
                {
                    case EClef.C:
                        return GDI.Elements.TsNoteData.Enote.C;
                    case EClef.F:
                        return GDI.Elements.TsNoteData.Enote.F;
                    case EClef.G:
                        return GDI.Elements.TsNoteData.Enote.G;
                }
                return GDI.Elements.TsNoteData.Enote.F;
            }
        }

        public bool Innerit { get; set; }

        public override void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            //param.Graphics.DrawString(Elements.TsSymbols.ClaveSol.Code+"", param.FontM.CurrentFont, param.Enviroment.FiguresColor.Brush, param.Cursor.X+ 0, 0);
            param.Draw(measure, this);
            
        }


        public override void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();

            Elements.TsSymbols.TsSymbolProperties symbol=null;

            

            switch(this.Clef)
            {
                case EClef.G:
                    symbol = param.Symbols[Elements.TsSymbols.ESymbols.ClaveSol];
                    cache.SubLine = 2;
                    cache.Rectangle = new D.RectangleF()
                    {
                        Height = (symbol.BPercent-symbol.TPercent)*param.FontM.FontHeight+2*param.Enviroment.HeightSpace,
                        Y = param.PositionOfSubLine(2) - symbol.TopToCenter * param.FontM.FontHeight - param.Enviroment.HeightSpace,
                        Width = (symbol.RPercent-symbol.LPercent)*param.FontM.FontHeight
                    };
                    break;
                default:
                    throw new Exception("Clef " + this.Clef + " is not supported yet.");
            }
            int plusoctaves = this.Octave-4;

            /*

            if (plusoctaves != 0)
            {
                D.RectangleF rect = cache.Rectangle;
                rect.Height += param.Enviroment.HeightSpace;
                rect.Y-=(plusoctaves>4)?
            }
            */
            cache.Characters.Add(new Drawing.TsCacheCharacter()
            {
                 Symbol = symbol,
                 DeltaY = param.PositionOfSubLine(2)-cache.Rectangle.Y

            });

            

            string message = "";

            if (plusoctaves != 0)
            { 
                message = (Math.Abs(plusoctaves) == 1) ? "8" : "15";

                cache.Messages.Add(new Drawing.TsMessage()
                {
                     FontPercent = 1,
                     ReferenceX=(symbol.Rectangle.Width*param.FontM.FontHeight)/2,
                     ReferenceY = (plusoctaves > 0) ? 0 : cache.Rectangle.Height,
                     HorizontalAlignment = Drawing.TsMessage.EHorizontalAlignment.Center,
                     VerticalAlignment = (plusoctaves>0)?Drawing.TsMessage.EVerticalAlignment.Top: Drawing.TsMessage.EVerticalAlignment.Bottom,
                     Message=message
                });
            }

            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }


        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

        internal TsClef CreateInnerit(TsMeasure tsMeasure)
        {
            TsClef clef = new TsClef(tsMeasure);
            clef.Clef = this.Clef;
            clef.Line = this.Line;
            clef.Octave = this.Octave;
            clef.Innerit = true;
            return clef;
        }
    }
}
