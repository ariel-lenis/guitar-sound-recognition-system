using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsAlter : TsElement
    {
        public TsNoteData Note { get; set; }

        public TsAlter(TsMeasure measure)
            : base(measure)
        {

        }

        public override void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            param.Draw(measure, this);
        }

        public override void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();

            TsSymbols.ESymbols symbol;

            switch (Note.Alter)
            { 
                case TsNoteData.Ealter.Bemol:
                    symbol = TsSymbols.ESymbols.Bemol;
                    break;
                case TsNoteData.Ealter.Sharp:
                    symbol = TsSymbols.ESymbols.Sharp;
                    break;
                default:
                    symbol = TsSymbols.ESymbols.Becuadro;
                    break;
            }

            var alter = param.Symbols[symbol];

            D.RectangleF rcontainer = new D.RectangleF();

            int subline = this.Note.PentagramSubLine(this.Measure.Clef);

            rcontainer.Y = param.PositionOfSubLine(subline) - alter.TopToCenter * param.FontM.FontHeight;
            rcontainer.Height = alter.Rectangle.Height * param.FontM.FontHeight;

            cache.Characters.Add(new Drawing.TsCacheCharacter()
            {
                Symbol = alter,
                DeltaX = 0,
                DeltaY = param.PositionOfSubLine(subline) - rcontainer.Y
            });

            rcontainer.Width = alter.Rectangle.Width * param.FontM.FontHeight;

            cache.Rectangle = rcontainer;
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }


        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

    }
}
