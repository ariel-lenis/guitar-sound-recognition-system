using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsChords : List<TsNoteData>
    {
        public TsNoteData MaxChord()
        {
            int pos = -1;
            for(int i=0;i<this.Count;i++)
                if (i == 0 || this[i].RelativePosition > this[pos].RelativePosition)
                    pos = i;
            if (pos == -1) return null;
            return this[pos];
        }
        public TsNoteData MinChord()
        {
            int pos = -1;
            for (int i = 0; i < this.Count; i++)
                if (i == 0 || this[i].RelativePosition < this[pos].RelativePosition)
                    pos = i;
            if (pos == -1) return null;
            return this[pos];
        }

        public void SortNotes()
        {
            this.Sort((x,y)=>x.RelativePosition-y.RelativePosition);
        }

        public DirectionResult DetermineDirection(TsMeasure measure,int corcheas)
        {
            TsNoteData maxnote = this.MaxChord();
            TsNoteData minnote = this.MinChord();

            int[] lplicas = new int[] { 7, 7, 7, 8, 10, 10, 10 };


            //int line = measure.Clef.Line;
            //int subline = (line - 1) * 2;
            //TsNoteData clefnote = measure.Clef.NoteData;

            int midsubline = 4;

            //int maxdelta = subline + maxnote.RelativePosition - clefnote.RelativePosition;
            //int mindelta = subline + minnote.RelativePosition - clefnote.RelativePosition;

            int maxdelta = maxnote.PentagramSubLine(measure.Clef);
            int mindelta = minnote.PentagramSubLine(measure.Clef);

            DirectionResult res = new DirectionResult();
            res.MinNote = minnote;
            res.MaxNote = maxnote;                   

            if (midsubline - mindelta > maxdelta - midsubline)
            {
                res.Direcction = 1;
                res.DeltaNote = maxnote;
                res.OpositeSubLine = mindelta;
                if (Math.Abs(maxdelta - midsubline) < 10) res.DestinySubLine = maxdelta + lplicas[corcheas] + 0 * 10;
                else                            res.DestinySubLine = midsubline;
            }
            else
            {
                res.Direcction = -1;
                res.DeltaNote = minnote;
                res.OpositeSubLine = maxdelta;
                if ( Math.Abs( midsubline - mindelta) < 10) res.DestinySubLine = mindelta -lplicas[corcheas]+ 0*10;
                else                            res.DestinySubLine = midsubline;
            }

            return res;
        }

        public class DirectionResult
        {
            public TsNoteData MaxNote { get; set; }
            public TsNoteData MinNote { get; set; }
            public TsNoteData DeltaNote { get; set; }
            public int Direcction { get; set; }
            public int OpositeSubLine { get; set; }
            public int DestinySubLine { get; set; }
        }

    }
}
