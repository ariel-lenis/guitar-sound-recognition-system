using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Tools
{
    public class TsDurationEngine
    {
        public enum EDurations { T001 = 0, T002, T004, T008, T016, T032, T064, T128 };

        public struct CompleteDuration
        {
            public EDurations Duration;
            public int Puntillos;
        }
        public class DurationParameters
        {
            public EDurations Duration;
            public Elements.TsSymbols.ESymbols SilenceCode;
            public Elements.TsSymbols.ESymbols NoteBase;
            public bool Plica;
            public int Corcheas;


            private DurationParameters(EDurations duration, Elements.TsSymbols.ESymbols silencecode, Elements.TsSymbols.ESymbols notebase, bool plica, int corcheas)
            {
                this.Duration = duration;
                this.SilenceCode = silencecode;
                this.NoteBase = notebase;
                this.Plica = plica;
                this.Corcheas = corcheas;
            }

            public static DurationParameters GetParameters(EDurations who)
            {
                //char[] notes = new char[] { (char)119, (char)104, (char)113, (char)101, (char)120, (char)114, (char)198, (char)141 };
                //char[] notesinverted = new char[] { (char)119, (char)72, (char)81, (char)69, (char)88, (char)82, (char)239, (char)61570 };
                //char[] notesbase = new char[] { (char)61559, (char)61559, (char)61647, (char)61647, (char)61647, (char)61647, (char)61647, (char)61647 };
                Elements.TsSymbols.ESymbols[] notesbase = new Elements.TsSymbols.ESymbols[] { Elements.TsSymbols.ESymbols.Redonda, Elements.TsSymbols.ESymbols.Redonda, Elements.TsSymbols.ESymbols.RedondaNegra, Elements.TsSymbols.ESymbols.RedondaNegra, Elements.TsSymbols.ESymbols.RedondaNegra, Elements.TsSymbols.ESymbols.RedondaNegra, Elements.TsSymbols.ESymbols.RedondaNegra, Elements.TsSymbols.ESymbols.RedondaNegra };
                //dot 61486

                Elements.TsSymbols.ESymbols[] silences = new Elements.TsSymbols.ESymbols[] { Elements.TsSymbols.ESymbols.Silence001, Elements.TsSymbols.ESymbols.Silence002, Elements.TsSymbols.ESymbols.Silence004, Elements.TsSymbols.ESymbols.Silence008, Elements.TsSymbols.ESymbols.Silence016, Elements.TsSymbols.ESymbols.Silence032, Elements.TsSymbols.ESymbols.Silence064, Elements.TsSymbols.ESymbols.Silence128 };
                bool[] plicas = new bool[] { false, true, true, true, true, true, true, true };
                int[] corcheas = new int[] { 0, 0, 0, 1, 2, 3, 4, 5 };

                int pos = (int)who;
                return new DurationParameters(who, silences[pos], notesbase[pos], plicas[pos], corcheas[pos]);

            }
        }
        private static int MCD(int a, int b)
        {
            if (a == 0 && b == 0) return 0;
            if (b == 0) return a;
            return MCD(b, a % b);
        }

        private static int Power2(int x)
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

        public static CompleteDuration CalculateDuration(TsMeasure measure,Elements.TsWithDuration note)
        {
            int beat =  measure.Time.Beats;
            int beattype = measure.Time.BeatType;
            int divisions = measure.Time.Divisions;

            int num = note.Duration;
            int den = beattype * (int)divisions;
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

            return new CompleteDuration() { Duration = (EDurations)power, Puntillos = puntillos };
        }
    }
}
