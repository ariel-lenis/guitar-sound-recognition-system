using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine
{
    public class Partwise
    {
        public List<Measure> Measures { get; set; }

        public Partwise()
        {
            this.Measures = new List<Measure>();
        }
    }
    public class Measure 
    {
        public List<WithDuration> Notes { get; set; }
        public Measure()
        {
            this.Notes = new List<WithDuration>();
        }

        public Fraction SumDurations()
        {
            Fraction f = Fraction.Zero;
            for (int i = 0; i < this.Notes.Count; i++)
                f += this.Notes[i].Duration.ToFraction();
            return f;
        }
    }
    public abstract class WithDuration
    {
        public NoteTime Duration { get; set; }
    }
    public class Silence : WithDuration
    {
        public override string ToString()
        {
            return "Silence:\t" + this.Duration;
        }
    }
    public class Note : WithDuration
    {
        public List<NoteData> Chords { get; set; }
        public Note()
        {
            this.Chords = new List<NoteData>();
        }

        public string ChordsToString()
        {
            string res = "";
            foreach (var i in Chords)
                res += i.ToString() + " ";
            return res;
        }

        public override string ToString()
        {
            return "Note:\t" + this.Duration+"{"+this.ChordsToString()+"}";
        }
    }
    public class NoteData
    {
        public enum ENote { C, D, E, F, G, A, B };
        public enum EAlters { Normal=0, Sharp=1, Bemol=-1 };

        public ENote Note { get; set; }
        public EAlters Alter { get; set; }
        public int Octave { get; set; }

        public override string ToString()
        {
            string salter = "";
            if (this.Alter != EAlters.Normal)
                salter = (this.Alter == EAlters.Sharp) ? "#" : "b";
            return Note + salter + Octave;
        }

        public static NoteData CreateFromString(string cad,int octave)
        {
            string modifier="";
            string note = cad[0].ToString();
            if(cad.Length>1) 
                modifier = cad[1].ToString();
            string[] notes = {"C","D","E","F","G","A","B"};

            NoteData res = new NoteData();
            res.Octave = octave;

            for(int i=0;i<notes.Length;i++)
                if(notes[i]==note)
                    res.Note = (ENote)i;
            if(modifier!="") 
                res.Alter=(modifier=="#")?EAlters.Sharp: EAlters.Bemol;
            return res;
        }
    }
    public class NoteTime
    {
        public enum EFigures { Redonda = 0, Blanca, Negra, Corchea, SemiCorchea, Fusa, SemiFusa,Garrapatea };
        //public enum EGroup { None, Start, End };

        public static float[] FiguresValues = new float[] { 4, 2, 1, 1 / 2f, 1 / 4f, 1 / 8f, 1 / 16f, 1 / 32f };
        public static float[] PuntillosValues = new float[] { 1,1+ 1 / 2f,1+ 1 / 2f + 1 / 4f,1+ 1 / 2 + 1 / 4f + 1 / 8f };

        public static Fraction[] FiguresFractions = new Fraction[] 
        {
            new Fraction(1),
            new Fraction(1,2),
            new Fraction(1,4),
            new Fraction(1,8),
            new Fraction(1,16),
            new Fraction(1,32),
            new Fraction(1,64)
        };

        //public EGroup Tie;


        public bool IsStartTie;
        public bool IsEndTie;

        public EFigures Figure;
        public int Puntillos;

        public Fraction Fraction
        {
            get { return this.ToFraction(); }
        }


        /*
        public static NoteTime FromFraction(Fraction fraction)
        {
            return null;
        }
        */

        public static List<NoteTime> ListFromNeeded(Fraction fraction,bool tied,int maxpuntillos)
        {
            List<Fraction> fractions = new List<Fraction>();
            List<NoteTime> notes = new List<NoteTime>();

            Fraction dfraction = fraction;
            for (int i = 0; i < FiguresFractions.Length; i++)
            {
                while (dfraction >= FiguresFractions[i])
                {
                    fractions.Add(FiguresFractions[i]);
                    notes.Add(new NoteTime() { Figure = (EFigures)i });
                    dfraction -= FiguresFractions[i];
                }
            }

            if(tied)
            { 
                for(int i=0;i<notes.Count;i++)
                {
                    notes[i].IsStartTie = true;
                    if(i<notes.Count-1)
                        notes[i+1].IsEndTie = true;
                }
            }

            if (dfraction != Fraction.Zero)
                throw new Exception("...");
            return ResumeNotes(notes,maxpuntillos);
        }

        private static List<NoteTime> ResumeNotes(List<NoteTime> notes,int maxpuntillos,int start=0)
        {
            //return notes;
            List<NoteTime> result = new List<NoteTime>();

            int puntilloscount = 1;
            int lastnote = -1;

            for (int i = 0; i < start; i++)
                result.Add(notes[i]);

            for (int i = start; i < notes.Count; i++)
            {
                //if (lastnote > -1)
                //    Console.WriteLine(result[lastnote].ToFraction() + "->" + result[lastnote].FractionOnDot(puntilloscount));
                    //Console.WriteLine(notes[i].ToFraction() + "->" + result[lastnote].FractionOnDot(puntilloscount));

                if (i == start || puntilloscount > maxpuntillos || notes[i].ToFraction() != result[lastnote].FractionOnDot(puntilloscount))
                {
                    lastnote = result.Count;
                    puntilloscount = 1;
                    result.Add(notes[i]);
                }
                else if (notes[i].ToFraction() == result[lastnote].FractionOnDot(puntilloscount))
                {
                    puntilloscount++;
                    result[lastnote].Puntillos++;
                }
                else
                {
                    throw new Exception("xD");
                }
            }

            return result;

            
        }

        public static List<NoteTime> ListFromRest(Fraction measuresize, Fraction rest,bool tied,int maxpuntillos)
        {
            if (rest.Numerator == 27 && rest.Denominator == 16)
            { 
            
            }
            measuresize.Simplify();

            List<Fraction> fractions = new List<Fraction>();
            List<NoteTime> notes = new List<NoteTime>();

            Fraction unity = measuresize;
            if (measuresize.Numerator != 1) unity = new Fraction(1, measuresize.Denominator);
            Fraction dfraction = rest;

            //If the rest its so much long... we need to fill it with unities of the time

            int skip = 0;
            int idx = -1;
            if(rest>measuresize)
            { 
                for (int i = 0; i < FiguresFractions.Length; i++)
                    if (FiguresFractions[i] == unity)
                    {
                        idx = i;
                        break;
                    }
                if(idx==-1) throw new Exception("...");
                while (dfraction >= unity)
                {
                    skip++;
                    fractions.Add(unity);
                    notes.Add(new NoteTime() { Figure = (EFigures)idx });
                    dfraction -= unity;                    
                }
            }

            
            for (int i = 0; i < FiguresFractions.Length; i++)
            {
                while (dfraction >= FiguresFractions[i])
                {
                    fractions.Add(FiguresFractions[i]);
                    notes.Add(new NoteTime() { Figure = (EFigures)i });
                    dfraction -= FiguresFractions[i];
                }
            }

            if (tied)
            {
                for (int i = 0; i < notes.Count-1; i++)
                {
                    if (i ==0)
                        notes[i].IsEndTie = true;                    

                    notes[i].IsStartTie = true;                    
                    notes[i + 1].IsEndTie = true;
                }
            }
            

            if (dfraction != Fraction.Zero)
                throw new Exception("...");

            return ResumeNotes(notes,maxpuntillos,skip);
        }

        public Fraction FractionOnDot(int puntillo)
        {
            var f = this.ToBaseFraction();
            var ff = this.ToBaseFraction() / (1 << puntillo);

            if (puntillo == 2)
            { 
            
            }

            return ff;
        }

        public Fraction ToBaseFraction()
        {
            int val = (int)Figure;
            Fraction res = new Fraction(1);
            res /= (1 << val);

            return res;
        }



        public Fraction ToFraction()
        {
            int val = (int)Figure;
            Fraction res = new Fraction(1);
            res /= (1 << val);

            Fraction div = res;
            for (int i = 0; i < this.Puntillos; i++)
            {
                div /= 2;
                res += div;
            }

            return res;
        }

        public float Time(float[] fvalues)
        {
            return fvalues[(int)this.Figure] * PuntillosValues[this.Puntillos];
        }

        public float Value 
        {
            get 
            {
                return NoteTime.FiguresValues[(int)this.Figure] * PuntillosValues[Puntillos];
            }
        }


        public override string ToString()
        {
            return Figure + " " + Puntillos;
        }
    }

}
