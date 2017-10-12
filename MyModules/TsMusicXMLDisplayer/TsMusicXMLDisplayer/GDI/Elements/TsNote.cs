using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsNote : TsWithDuration
    {
        public class ChordInfo
        {
            public int SubLine;
            public float CenterX;
            public float CenterY;
        }

        public TsChords Chords { get; set; }
        public enum EDirection { Up, Down };
        public int Corcheas { get; set; }
        public EDirection Direction { get; set; }

        public List<ChordInfo> ChordInfos { get; set; }

        TsChords.DirectionResult direcction;

        public TsChords.DirectionResult Direcction { get { return direcction; } }

        private int NoteDeltaPosition(TsNoteData chord)
        {
            //TsNote clefnote = new TsNote();
            var clefnote = this.Measure.Clef.NoteData;

            int clefline = this.Measure.Clef.Line;
            int lineposition = (clefline - 1) * 2;

            int clefrelative = clefnote.RelativePosition;

            int delta = lineposition + chord.RelativePosition - clefnote.RelativePosition;
            return delta;
        }

        //public int AbsolutePosition { get { return this.Octave * 7 + (int)this.Note; } }
        public TsNote(TsMeasure measure)
            : base(measure)
        {
            Chords = new TsChords();
        }


        public override void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            param.Draw(measure, this);
        }

        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

        public override List<TsNoteData> RequiredNoteData
        {
            get
            {
                return this.Chords;
            }
        }

        public override void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();

            Tools.TsDurationEngine.CompleteDuration duration = Tools.TsDurationEngine.CalculateDuration(this.Measure, this);
            Tools.TsDurationEngine.DurationParameters dparameters = Tools.TsDurationEngine.DurationParameters.GetParameters(duration.Duration);

            direcction = this.Chords.DetermineDirection(this.Measure,dparameters.Corcheas);            

            var note = param.Symbols[dparameters.NoteBase];

            D.RectangleF rcontainer = new D.RectangleF();
            if (direcction.Direcction == 1)
            {
                rcontainer.Y = param.PositionOfSubLine(direcction.DestinySubLine);
                rcontainer.Height = param.PositionOfSubLine(direcction.OpositeSubLine-1)-rcontainer.Y;
            }
            else
            { 
                rcontainer.Y = param.PositionOfSubLine(direcction.OpositeSubLine+1);
                rcontainer.Height = param.PositionOfSubLine(direcction.DestinySubLine) - rcontainer.Y;
            }

            CacheChords(cache,param,rcontainer,note,duration,direcction);

            float xplica = ((direcction.Direcction == 1) ? (note.RPercent - 0*0.01f) : (note.LPercent + 0*0.001f)) * param.FontM.FontHeight;
            TsSymbols.ESymbols scorchea = (direcction.Direcction==1)?TsSymbols.ESymbols.CorcheaUp:TsSymbols.ESymbols.CorcheaDown;
            TsSymbols.TsSymbolProperties corchea = param.Symbols[scorchea];

            for (int i = 0; i < dparameters.Corcheas; i++)
            {
                cache.Characters.Add(new Drawing.TsCacheCharacter()
                {
                     Symbol = corchea,
                     DeltaX = xplica - 0*corchea.LPercent * param.FontM.FontHeight,
                     DeltaY = param.PositionOfSubLine(direcction.DestinySubLine-2*i*direcction.Direcction)-rcontainer.Y 
                });
            }
            
            cache.Lines.Add(new Drawing.TsCacheLine()
            {
                Width=2.5f,
                X1= xplica,
                Y1= param.PositionOfSubLine(direcction.OpositeSubLine)-rcontainer.Y,
                X2=xplica,
                Y2 = param.PositionOfSubLine(direcction.DestinySubLine)-rcontainer.Y
            });

            /*
            if (this.Groups!=null && this.Groups.Count > 0)
            {
                cache.Lines.Add(new Drawing.TsCacheLine()
                {
                    Width = 1f,
                    X1 = xplica,
                    Y1 = param.PositionOfSubLine(12) - rcontainer.Y,
                    X2 = xplica,
                    Y2 = param.PositionOfSubLine(-4) - rcontainer.Y
                });
                
            }
            */

            var frect = cache.CharactersRectangle(param.FontM.FontHeight);

            cache.MoveAll(-frect.X, 0);
            
            rcontainer.Width = frect.Width;

            cache.Rectangle = rcontainer;
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }

        public float ConvertToLineX(float measurex)
        {
            var rect = this.CacheGraphics[0].Rectangle;
            return  this.Measure.CacheGraphics[0].Rectangle.X+ rect.X + measurex;
        }

        public float ConvertToLineY(float measurey)
        {
            var cache = this.CacheGraphics[0];
            var rect = this.CacheGraphics[0].Rectangle;

            return rect.Y + cache.Origin.Y + measurey;
        }

        private void CacheChords(Drawing.TsCacheGraphics cache, Drawing.TsDrawEngine param, D.RectangleF rcontainer, TsSymbols.TsSymbolProperties note, Tools.TsDurationEngine.CompleteDuration duration, TsChords.DirectionResult direcction)
        {
            int subline;
            this.Chords.SortNotes();

            int lastsubline=0;
            bool lastinversed=false;
            bool invert = false;

            this.ChordInfos = new List<ChordInfo>();

            //foreach (var ichord in this.Chords)
            for (int i = 0; i < this.Chords.Count;i++ )
            {
                TsNoteData ichord;

                if (direcction.Direcction == 1) ichord = this.Chords[i];
                else ichord = this.Chords[this.Chords.Count - 1 - i];

                subline = ichord.PentagramSubLine(this.Measure.Clef);

                if (i > 0 && Math.Abs(subline - lastsubline) == 1 && lastinversed == false)
                    invert = true;
                else
                    invert = false;

                ChordInfo chordinfo = new ChordInfo();
                chordinfo.SubLine = subline;
                chordinfo.CenterY = param.PositionOfSubLine(subline) - 0 * param.FontM.FontHeight * note.CenterY - rcontainer.Y;
                chordinfo.CenterX = (invert ? 1 : 0) * direcction.Direcction * note.Rectangle.Width * param.FontM.FontHeight;
                this.ChordInfos.Add(chordinfo);

                cache.Characters.Add(new Drawing.TsCacheCharacter()
                {
                    Symbol = note,
                    DeltaY = chordinfo.CenterY,
                    DeltaX = chordinfo.CenterX 
                });                

                lastsubline = subline;
                lastinversed = invert;

                CachePuntillos(cache, param, rcontainer, subline, duration, note);
                AuxiliarLines(rcontainer, ichord, param, cache, note,(invert?1:0)*direcction.Direcction);

                chordinfo.CenterX += (note.Rectangle.Width * param.FontM.FontHeight) / 2;
            }
        }

        private void CachePuntillos(Drawing.TsCacheGraphics cache, Drawing.TsDrawEngine param, D.RectangleF rcontainer, int subline, Tools.TsDurationEngine.CompleteDuration duration, TsSymbols.TsSymbolProperties note)
        {
            if (duration.Puntillos > 0)
            {
                int puntillosline = (subline % 2 == 0) ? (subline + 1) : subline;
                TsSymbols.TsSymbolProperties puntillo = param.Symbols[TsSymbols.ESymbols.Dot];
                for (int i = 0; i < duration.Puntillos; i++)
                {
                    cache.Characters.Add(new Drawing.TsCacheCharacter()
                    {
                        Symbol = puntillo,
                        DeltaY = param.PositionOfSubLine(puntillosline) - rcontainer.Y,
                        DeltaX = note.Rectangle.Width * 1.1f * param.FontM.FontHeight + puntillo.Rectangle.Width * param.FontM.FontHeight * 1.1f * i
                    });
                }
            }     
        }

        void AuxiliarLines(D.RectangleF rcontainer, TsNoteData note,Drawing.TsDrawEngine param,Drawing.TsCacheGraphics cache,TsSymbols.TsSymbolProperties symbol,int xdirection)
        {
            int delta = this.NoteDeltaPosition(note);
            for (int i = delta; !(i >= 0 && i <= 8); i += (delta < 0) ? 1 : -1)
            {
                //Just Even positions have a line
                if (i % 2 == 0)
                {
                    float yline = param.PositionOfSubLine(i);
                    cache.Lines.Add(new Drawing.TsCacheLine()
                    {
                        X1 = (symbol.Rectangle.Width*xdirection+  symbol.LPercent - symbol.Rectangle.Width * 0.25f) * param.FontM.FontHeight,
                        Y1 = yline - rcontainer.Y,
                        X2 = (symbol.Rectangle.Width * xdirection + symbol.RPercent + symbol.Rectangle.Width * 0.25f) * param.FontM.FontHeight,
                        Y2 = yline - rcontainer.Y
                    });
                }
            }
        
        }


    }
}
