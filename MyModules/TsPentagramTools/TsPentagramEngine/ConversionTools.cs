using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine
{
    public class ConversionTools
    {
        float referencevalue;
        NoteTime.EFigures referencessymbol;
        int maxdots;
        RangeInfo[] ranges;

        class RangeInfo 
        {
            public NoteTime.EFigures Figure;
            public float Time;
            public int Dots;
            public NoteTime ToNoteTime() 
            {
                return new NoteTime() { Figure = this.Figure, Puntillos = this.Dots };
            }
        }

        public ConversionTools(float referencevalue, NoteTime.EFigures referencessymbol,int maxdots)
        {
            this.referencevalue = referencevalue;
            this.referencessymbol = referencessymbol;
            this.maxdots = maxdots;
            UpdateParameters();
        }
        void UpdateParameters()
        {
            //ranges = new RangeInfo[7 * (maxdots+1)-maxdots];

            Fraction minimun = new Fraction(1,64);

            var lranges = new List<RangeInfo>();
            
            for (int i = 0; i < 7; i++)
            {
                int startdots = maxdots;
                //Not for semi fusa
                
                for (int idot = startdots; idot >= 0; idot--)
                { 
                    NoteTime.EFigures figure = (NoteTime.EFigures)i;
                    //ranges[i*(maxdots+1)+maxdots-idot] = new RangeInfo(){ Figure = figure, Time = TimeOf(figure,idot) , Dots = idot};

                    Fraction fnote = NoteTime.FiguresFractions[(int)figure];
                    fnote /= (1 << idot);

                    if(fnote>=minimun)
                        lranges.Add(new RangeInfo() { Figure = figure, Time = TimeOf(figure, idot), Dots = idot });
                }
            }
            this.ranges = lranges.ToArray();
        }
        public static float ValueOf(NoteTime.EFigures referencessymbol)
        {
            float[] values = new float[] { 4, 2, 1, 1 / 2f, 1 / 4f, 1 / 8f, 1 / 16f };
            return values[(int)referencessymbol];
        }

        public NoteTime.EFigures SymbolOf(int denominator)
        {
            int[] values = new int[] { 1,2,4,8,16,32,64};
            for (int i = 0; i < values.Length; i++)
                if (values[i] == denominator)
                    return (NoteTime.EFigures)i;
            throw new Exception("Symbol cant be defined.");
        }


        public List<NoteTime> DetermineBestEquivalence(float input)
        {
            float[] fvalues = TranslateFigures(this.referencevalue, this.referencessymbol);
            List<NoteTime> notes = new List<NoteTime>();

            float val = input;

            while(true)
            {
                NoteTime note = MaxCompatible(val);
                if (note==null) break;
                notes.Add(note);
                float ntime = note.Time(fvalues);
                val -= ntime;
                if (val <= 0) break;
            }

            return notes;
        }

        public List<Measure> CreateMeasures(List<WithDuration> notes,int numerator,int denominator)
        {
            Fraction measuresize = new Fraction(numerator, denominator);
            Fraction counter = Fraction.Zero;

            List<Measure> measures = new List<Measure>();
            Measure currentmeasure = new Measure();
            WithDuration newnote;

            for (int i = 0; i < notes.Count; i++)
            {
                var fnote = notes[i].Duration.ToFraction();

                var fsum = counter + fnote;

                if (fsum > measuresize)
                {
                    Fraction needed = measuresize - counter;
                    Fraction rest = fnote - needed;

                    List<NoteTime> lneeded = NoteTime.ListFromNeeded(needed, notes[i] is Note,this.maxdots);
                    List<NoteTime> lrest = NoteTime.ListFromRest(measuresize,rest, notes[i] is Note,this.maxdots);

                    foreach (var inotetime in lneeded)
                    {
                        if (notes[i] is Note)
                            newnote = new Note() { Duration = inotetime, Chords = (notes[i] as Note).Chords };
                        else
                            newnote = new Silence() { Duration = inotetime };                        
                        currentmeasure.Notes.Add(newnote);
                    }

                    if (currentmeasure.SumDurations() != measuresize) throw new Exception("...");
                    measures.Add(currentmeasure);
                    currentmeasure = new Measure();

                    counter = Fraction.Zero;

                    foreach (var inotetime in lrest)
                    {
                        if (notes[i] is Note)
                            newnote = new Note() { Duration = inotetime, Chords = (notes[i] as Note).Chords };
                        else
                            newnote = new Silence() { Duration = inotetime };

                        currentmeasure.Notes.Add(newnote);
                        counter += newnote.Duration.ToFraction();

                        if (counter > measuresize)
                            throw new Exception("...");


                        if (counter == measuresize)
                        {
                            if (currentmeasure.SumDurations() != measuresize) throw new Exception("...");
                            measures.Add(currentmeasure);
                            currentmeasure = new Measure();
                            counter = Fraction.Zero;                        
                        }

                    }
                }
                else
                {
                    currentmeasure.Notes.Add(notes[i]);
                    counter += fnote;

                    if (counter > measuresize) throw new Exception();

                    if(counter==measuresize)
                    {
                        if (currentmeasure.SumDurations() != measuresize) throw new Exception("...");
                        measures.Add(currentmeasure);
                        currentmeasure = new Measure();
                        counter = Fraction.Zero;
                    }
                }
            }

            if (!counter.ItsZero())
            {
                Fraction needed = measuresize - counter;
                List<NoteTime> lneeded = NoteTime.ListFromNeeded(needed, false,this.maxdots);
                foreach (var inotetime in lneeded)
                {
                    newnote = new Silence() { Duration = inotetime };
                    currentmeasure.Notes.Add(newnote);
                }
                if (currentmeasure.SumDurations() != measuresize) throw new Exception("...");
                measures.Add(currentmeasure);
            }

            return measures;
        }

        

        /*
        private NoteTime MaxCompatible(float val)
        {
            int idx = GetLowerIndex(val);

            //Time its soooo lower
            if (idx == -1)
            {
                if (val > ranges.Last().Time / 2) return ranges.Last().ToNoteTime();
                return null;
            }
            
            if(idx==0) return ranges[0].ToNoteTime();

            float timea = ranges[idx].Time;
            float timeb = ranges[idx-1].Time;

            float maxerrora = (timeb - timea) * (timea / timeb);

            if (val - timea < maxerrora) return ranges[idx].ToNoteTime();

            return ranges[idx-1].ToNoteTime();
        }
        */

        private NoteTime MaxCompatible(float val)
        {
            int idx = GetLowerIndex(val);

            //Time its soooo lower
            if (idx == -1)
            {
                if (val > ranges.Last().Time / 2) return ranges.Last().ToNoteTime();
                return null;
            }

            
            double min = double.MaxValue;
            int minidx = -1;
            for (int i = 0; i < this.ranges.Length; i++)
            {
                double dist = Math.Abs(Math.Log(val) - Math.Log(this.ranges[i].Time)) / Math.Log(2);
                if (dist < min)
                {
                    minidx = i;
                    min = dist;
                }
            }

            return this.ranges[minidx].ToNoteTime();
        }


        private int GetLowerIndex(float val)
        {
            for (int i = 0; i < ranges.Length; i++)
                if (ranges[i].Time < val) return i;
            return -1;
        }

        float TimeOf(NoteTime.EFigures figure,int dots)
        {
            float reference = ValueOf(this.referencessymbol);
            float target = ValueOf((NoteTime.EFigures)figure);
            return this.referencevalue * target / reference*NoteTime.PuntillosValues[dots];
        }

        public static float[] TranslateFigures(float referencevalue, NoteTime.EFigures referencesymbol)
        {
            float[] res = new float[7];
            float reference = ValueOf(referencesymbol);

            for (int i = 0; i < 7; i++)
            {
                float target = ValueOf((NoteTime.EFigures)i);
                res[i] = referencevalue * target / reference;
            }
            return res;
        }

    }
}
