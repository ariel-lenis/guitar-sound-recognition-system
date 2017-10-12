using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsSilence:TsWithDuration
    {
        /*
        static TsSymbols.TsSymbolProperties[] Symbols = 
        {            
            TsSymbols.Silence001, 
            TsSymbols.Silence002, 
            TsSymbols.Silence004, 
            TsSymbols.Silence008, 
            TsSymbols.Silence016, 
            TsSymbols.Silence032, 
            TsSymbols.Silence064, 
            TsSymbols.Silence128 
        };
        */
        public TsSilence(TsMeasure measure)
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

            Tools.TsDurationEngine.CompleteDuration duration = Tools.TsDurationEngine.CalculateDuration(this.Measure, this);
            Tools.TsDurationEngine.DurationParameters dparameters = Tools.TsDurationEngine.DurationParameters.GetParameters(duration.Duration);

            var note = param.Symbols[dparameters.NoteBase];

            D.RectangleF rcontainer = new D.RectangleF();

            TsSymbols.TsSymbolProperties silence = param.Symbols[dparameters.SilenceCode];

            int subline = 4;

            switch (dparameters.SilenceCode)
            {
                case TsSymbols.ESymbols.Silence008:
                    subline = 5;
                    break;
                case TsSymbols.ESymbols.Silence016:
                    subline = 3;
                    break;
                case TsSymbols.ESymbols.Silence032:
                    subline = 3;
                    break;
                case TsSymbols.ESymbols.Silence064:
                    subline = 1;
                    break;
                case TsSymbols.ESymbols.Silence128:
                    subline = 1;
                    break;
            }

            rcontainer.Y = param.PositionOfSubLine(subline)-silence.TopToCenter*param.FontM.FontHeight;
            rcontainer.Height = (silence.BPercent - silence.TPercent) * param.FontM.FontHeight;


            cache.Characters.Add(new Drawing.TsCacheCharacter()
            {
                Symbol = silence,
                DeltaX = 0,
                DeltaY = param.PositionOfSubLine(subline) - rcontainer.Y
            });

            CachePuntillos(cache, param, rcontainer, duration, silence);

            //rcontainer.Width = silence.Rectangle.Width * param.FontM.FontHeight;

            var frect = cache.CharactersRectangle(param.FontM.FontHeight);

            cache.MoveAll(-frect.X, 0);

            rcontainer.Width = frect.Width;

            cache.Rectangle = rcontainer;
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }

        private void CachePuntillos(Drawing.TsCacheGraphics cache, Drawing.TsDrawEngine param, D.RectangleF rcontainer, Tools.TsDurationEngine.CompleteDuration duration, TsSymbols.TsSymbolProperties note)
        {
            if (duration.Puntillos > 0)
            {
                int puntillossubline = 5;
                TsSymbols.TsSymbolProperties puntillo = param.Symbols[TsSymbols.ESymbols.Dot];
                for (int i = 0; i < duration.Puntillos; i++)
                {
                    cache.Characters.Add(new Drawing.TsCacheCharacter()
                    {
                        Symbol = puntillo,
                        DeltaY = param.PositionOfSubLine(puntillossubline) - rcontainer.Y,
                        DeltaX = note.Rectangle.Width * 1.1f * param.FontM.FontHeight + puntillo.Rectangle.Width * param.FontM.FontHeight * 1.1f * i
                    });
                }
            }
        }

        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

    }
}
