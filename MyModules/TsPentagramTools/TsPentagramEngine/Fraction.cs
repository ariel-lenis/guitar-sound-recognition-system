using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine
{
    public struct Fraction
    {
        public int Numerator;
        public int Denominator;

        public static Fraction Zero = new Fraction(0, 1);

        public override string ToString()
        {
            return this.Numerator + "/" + this.Denominator;
        }

        public Fraction(int numerator=0, int denominator=1)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public bool ItsZero()
        {
            return this.Numerator == 0 && this.Denominator != 0;
        }

        public bool ItsInfinity()
        {
            return this.Numerator != 0 && this.Denominator == 0;
        }

        public bool ItsNaN()
        {
            return this.Numerator == 0 && this.Denominator == 0;
        }

        public static bool operator >(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a > b;
        }

        public static bool operator >=(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a >= b;
        }

        public static bool operator <=(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a <= b;
        }
        
        public static bool operator <(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a < b;
        }

        public static bool operator ==(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a == b;
        }

        public override bool Equals(object obj)
        {
            return this==(Fraction)obj;
        }

        public static bool operator !=(Fraction c1, Fraction c2)
        {
            int mcm = MCM(c1.Denominator, c2.Denominator);

            int a = mcm / c1.Denominator * c1.Numerator;
            int b = mcm / c2.Denominator * c2.Numerator;
            return a != b;
        }  

        public static Fraction operator +(Fraction c1, Fraction c2)
        {
            int denominator = MCM(c1.Denominator, c2.Denominator);
            int numerator = denominator / c1.Denominator * c1.Numerator + denominator / c2.Denominator * c2.Numerator;
            Fraction res = new Fraction(numerator, denominator);
            res.Simplify();
            return res;
        }
        public static Fraction operator -(Fraction c1, Fraction c2)
        {
            int denominator = MCM(c1.Denominator, c2.Denominator);
            int numerator = denominator / c1.Denominator * c1.Numerator - denominator / c2.Denominator * c2.Numerator;
            Fraction res = new Fraction(numerator, denominator);
            res.Simplify();

            if (res.Denominator < 0) throw new Exception();

            return res;
        }

        public static Fraction operator *(Fraction c1, int val)
        {
            Fraction res = new Fraction(c1.Numerator * val, c1.Denominator);
            res.Simplify();
            return res;
        }
        public static Fraction operator *(int val, Fraction c1)
        {
            return c1 * val;
        }
        public static Fraction operator /(Fraction c1, int val)
        {
            Fraction res = new Fraction(c1.Numerator, c1.Denominator * val);
            res.Simplify();
            return res;
        }
        public static Fraction operator /(int val, Fraction c1)
        {
            return val * c1.Invert();
        }

        public Fraction Invert()
        {
            return new Fraction(this.Denominator, this.Numerator);
        }

        public void Simplify()
        {
            int mcd = MCD(this.Numerator, this.Denominator);
            this.Numerator /= mcd;
            this.Denominator /= mcd;
        }

        private static int MCM(int a, int b)
        {
            int mcd = MCD(a, b);
            return a * b / mcd;
        }

        private static int MCD(int a, int b)
        {
            if (a == 0 && b == 0) return 1;
            if (b == 0) return a;
            return MCD(b, a % b);
        }
    }
}
