using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsTime:TsElement
    {
        public int Beats { get; set; }
        public int BeatType { get; set; }
        public int Divisions { get; set; }

        public TsTime(TsMeasure mesasure)
            : base(mesasure)
        { 
        
        }

        public override void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            param.Draw(measure, this);
        }


        public override void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();

            Elements.TsSymbols.ESymbols snumerator = (TsSymbols.ESymbols)('0' + this.Beats);
            Elements.TsSymbols.ESymbols sdenominator = (TsSymbols.ESymbols)('0' + this.BeatType);

            Elements.TsSymbols.TsSymbolProperties numerator = param.Symbols[snumerator];
            Elements.TsSymbols.TsSymbolProperties denominator = param.Symbols[sdenominator];

            D.RectangleF rcontainer = new D.RectangleF();

            rcontainer.Height = param.Enviroment.HeightSpace*4;
            rcontainer.Y = param.PositionOfSubLine(8) - 0 * param.FontM.FontHeight * numerator.CenterY;

            cache.Characters.Add(new Drawing.TsCacheCharacter() 
            {
                Symbol = denominator, 
                DeltaY= -0*param.Enviroment.HeightSpace+param.PositionOfSubLine(2)-rcontainer.Y
            });

            cache.Characters.Add(new Drawing.TsCacheCharacter() 
            {
                Symbol = numerator,
                DeltaY = 0*param.Enviroment.HeightSpace + param.PositionOfSubLine(6)-rcontainer.Y
            });
            /*
            if (numerator.Rectangle.Width > denominator.Rectangle.Width)
                rcontainer.Width = numerator.Rectangle.Width * param.FontM.FontHeight;
            else
                rcontainer.Width = denominator.Rectangle.Width * param.FontM.FontHeight;
            */
            rcontainer.Width = cache.CharactersRectangle(param.FontM.FontHeight).Width;
            cache.Rectangle = rcontainer;
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }

        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

        internal TsTime CreateInnerit(TsMeasure tsMeasure)
        {
            TsTime time = new TsTime(tsMeasure);
            time.Beats = this.Beats;
            time.BeatType = this.BeatType;
            time.Divisions = this.Divisions;
            time.Innerit = true;
            return time;
        }
    }
}
