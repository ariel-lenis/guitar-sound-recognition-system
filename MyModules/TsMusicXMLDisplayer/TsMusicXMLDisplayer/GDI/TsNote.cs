using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TsMusicXMLDisplayer.GDI
{
    public class DurationParameters
    {
        public TsNote.EDurations Duration;
        public char SilenceCode;
        public char NoteCode;
        public char NoteInvertedCode;
        public char NoteBase;
        public bool Plica;
        public int Corcheas;
       

        private DurationParameters(TsNote.EDurations duration, char silencecode, char notecode, char noteinvertedcode,char notebase,bool plica,int corcheas)
        {
            this.Duration = duration;
            this.SilenceCode = silencecode;
            this.NoteCode = notecode;
            this.NoteInvertedCode = noteinvertedcode;
            this.NoteBase = notebase;
            this.Plica = plica;
            this.Corcheas = corcheas;
        }

        public static DurationParameters GetParameters(TsNote.EDurations who)
        {
            char[] notes = new char[] { (char)119, (char)104, (char)113, (char)101, (char)120, (char)114, (char)198, (char)141 };
            char[] notesinverted = new char[] { (char)119, (char)72, (char)81, (char)69, (char)88, (char)82, (char)239, (char)61570 };
            char[] notesbase = new char[] { (char)61559, (char)61559, (char)61647, (char)61647, (char)61647, (char)61647, (char)61647, (char)61647 };
            
            //dot 61486

            char[] silences = new char[] { (char)183, (char)238, (char)206, (char)228, (char)197, (char)168, (char)244, (char)229 };
            bool[] plicas = new bool[] { false, true, true, true, true, true, true, true };
            int[] corcheas = new int[] { 0, 0, 0, 1, 2, 3, 4, 5 };

            int pos = (int)who;
            return new DurationParameters(who,silences[pos],notes[pos],notesinverted[pos],notesbase[pos], plicas[pos],corcheas[pos]);

        }
    }
    public struct CompleteDuration
    {
        public TsNote.EDurations Duration;
        public int Puntillos;
    }
    public class TsNote
    {
        public enum Enote { C=0,D,E,F,G,A,B};
        public enum Ealter { Normal = 0, Sharp=1, Bemol=-1 };
        public Enote Note;
        public Ealter Alter;
        public int Octave;
        public int Duration;

        public int AbsolutePosition { get { return this.Octave * 7 + (int)this.Note; } }

        public List<TsNote> Chords;

        //public enum EDurations { Redonda, Blanca, Negra, Corchea, SemiCorchea, Fusa, SemiFusa, Garrapatea };

        public enum EDurations { T001=0, T002, T004, T008, T016, T032, T064, T128 };
        public enum EType { Note=0, Silence,Chord };

        public EType Type;

        public TsNote MinimunChord()
        {
            int imin = 0;
            int min=0;
            for (int i = 0; i < Chords.Count; i++)
            {
                int bmin = Chords[i].Octave * 7 + (int)Chords[i].Note;
                if (i == 0 || bmin < min)
                {
                    imin = i;
                    min = bmin;
                }
            }
            return Chords[imin];
        }

        public void OrderChords()
        {
            bool order = true; ;
            for (int i = 0; order && i < Chords.Count; i++)
            {
                order = false;
                for (int j = 0; j < Chords.Count - 1; j++)
                {
                    if (Chords[j].AbsolutePosition > Chords[j + 1].AbsolutePosition)
                    {
                        var aux = Chords[j];
                        Chords[j] = Chords[j + 1];
                        Chords[j + 1] = aux;
                        order = true;
                    }
                }
            }
        }

        public TsNote MaximunChord()
        {
            int imax = 0;
            int max = 0;
            for (int i = 0; i < Chords.Count; i++)
            {
                int bmax = Chords[i].Octave * 7 + (int)Chords[i].Note;
                if (i == 0 || bmax > max)
                {
                    imax = i;
                    max = bmax;
                }
            }
            return Chords[imax];
        }

        private int MCD(int a,int b)
        {
            if (a == 0 && b == 0) return 0;
            if (b == 0)           return a;
            return MCD(b, a % b);
        }

        private int Power2(int x)
        {
            int pow = 1;
            int cont = 0;
            while (pow < x)
            {
                pow *= 2;
                cont++;
            }
            if (pow == x) return cont;
            return -1;
        }
        
        public CompleteDuration CalculateDuration(int beat, int beattype, decimal divisions)
        {
            int num = this.Duration;
            int den = beattype*(int)divisions;
            int mcd = MCD(num, den);            

            if (mcd > 0)
            {
                num /= mcd;
                den /= mcd;
            }

            int puntillos = 0;

            if (num != 1)
            {
                if (num == 3)
                    puntillos = 1;
                else if (num == 7)
                    puntillos = 2;
                else if (num == 15)
                    puntillos = 3;
                else
                    throw new Exception("So much Puntillos.");
                for (int i = 0; i < puntillos; i++)
                    den /= 2;
            }

            int power = Power2(den);
            if (power == -1 || power > 8) throw new Exception("Error with the duration.");

            return new CompleteDuration(){ Duration=  (EDurations)power, Puntillos=puntillos};
        }
        

        public int RelativePosition
        {
            get
            {
                return Octave * 7 + (int)Note;
            }
        }

        public void LoadFromNote(string note)
        {
            this.Note = (Enote)NoteToNumber(note[0]+"");
            this.Octave = int.Parse(note.Substring(1));
        }

        public void LoadFromXMLClef(MusicXML.clef clef)
        {
            int octave = 0;
            if (clef.clefoctavechange == null)
                octave = 4;
            else
                octave = 4 + int.Parse(clef.clefoctavechange);

            string note = "C";
            if (clef.sign == MusicXML.clefsign.F) note = "F";
            else if (clef.sign == MusicXML.clefsign.G) note = "G";
            this.LoadFromNote(note + octave);
        }

        public void LoadFromXMLNote(MusicXML.note note)
        {
            MusicXML.pitch pitch=null;
            int duration = 0;
            bool silence = false;
           
            for(int i=0;i<note.ItemsElementName.Length;i++)
            {
                switch (note.ItemsElementName[i])
                { 
                    case MusicXML.ItemsChoiceType1.pitch:
                        pitch = note.Items[i] as MusicXML.pitch;
                        break;
                    case MusicXML.ItemsChoiceType1.duration:
                        duration = int.Parse(note.Items[i].ToString());
                        break;
                    case MusicXML.ItemsChoiceType1.chord:
                        this.Type = EType.Chord;
                        break;
                    case MusicXML.ItemsChoiceType1.rest:
                        silence = true;
                        break;
                }
            }
            if (silence)
                this.Type = EType.Silence;
            else if (pitch != null)
            {
                this.Octave = int.Parse(pitch.octave);
                this.Note = (Enote)NoteToNumber(pitch.step);
                if (Math.Abs(pitch.alter) > 1) throw new Exception("Bad alter!!!");
                this.Alter = (Ealter)pitch.alter;
            }
            this.Duration = duration;
            this.Chords = new List<TsNote>() { this };
        }

        public static int NoteToNumber(MusicXML.step note)
        {
            MusicXML.step[] steps = new MusicXML.step[] { MusicXML.step.C, MusicXML.step.D, MusicXML.step.E, MusicXML.step.F, MusicXML.step.G, MusicXML.step.A, MusicXML.step.B };
            for (int i = 0; i < steps.Length; i++)
                if (steps[i] == note)
                    return i;
            throw new Exception("Bad note!!!");
        }

        public static int NoteToNumber(string note)
        {
            //  C   D   E   F   G   A   B
            //  0   1   2   3   4   5   6
            string[] notes = new string[] { "C", "D", "E", "F", "G", "A", "B" };
            for (int i = 0; i < notes.Length; i++)
                if (notes[i]==note)
                    return i;
            throw new Exception("Bad note!!!");
        }
    }

}
