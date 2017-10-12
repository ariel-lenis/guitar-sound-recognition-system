using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public abstract class TsWithDuration:TsElement
    {
        public class ClassDuration
        {
            public enum EFigures {Blanca=0,Redonda,Negra,Corchea,SemiCorchea,Fusa,SemiFusa,Garrapatea,SemiGarrapatea};
            public EFigures Figure{get;set;}
            public int Puntillos{get;set;}
        }
        public int Duration { get; set; }

        public TsWithDuration(TsMeasure measure)
            : base(measure)
        { 
        
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
        public ClassDuration CalculateDuration()
        {
            int beattype = this.Measure.Time.BeatType;

            int num = this.Duration;
            int den = beattype * (int)this.Measure.Time.Divisions;
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

            return new ClassDuration() { Figure = (ClassDuration.EFigures)power, Puntillos = puntillos };      
        }

    }
}
