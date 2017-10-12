using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public struct Complex
    {
        public float R;
        public float I;

        public Complex(float R, float I)
        {
            this.R = R;
            this.I = I;
        }
        public static Complex FromEuler(float X,float Y)
        {
            float eX = (float)Math.Exp(X);
            float R = eX*(float)Math.Cos(Y);
            float I = eX*(float)Math.Sin(Y);
            return new Complex(R, I);
        }
        public float Module()
        {
            return (float)Math.Sqrt(R * R + I * I);
        }

        public Complex Conjugate()
        {
            return new Complex(R, -I);
        }

        public static Complex operator +(Complex A,Complex B)
        {
            return new Complex(A.R + B.R, A.I + B.I);
        }
        public static Complex operator -(Complex A, Complex B)
        {
            return new Complex(A.R - B.R, A.I - B.I);
        }
        public static Complex operator *(Complex A, Complex B)
        {
            return new Complex((A.R * B.R - A.I * B.I), (A.R * B.I + A.I * B.R));
        }
        public static Complex operator /(Complex A, Complex B)
        {
            float den = B.R * B.R + A.I + B.I;
            return new Complex((A.R * B.R + A.I * B.I)/den, (-A.R * B.I + A.I * B.R)/den);
        }
        public static Complex operator *(float k, Complex A)
        {
            return new Complex(k*A.R,k*A.I);
        }
        public static Complex operator *(Complex A,float k)
        {
            return new Complex(k * A.R, k * A.I);
        }

        public static Complex operator /(float k, Complex A)
        {
            return new Complex(A.R/k, A.I/k);
        }
        public static Complex operator /(Complex A, float k)
        {
            return new Complex(A.R / k, A.I / k);
        }


        public override string ToString()
        {
            if (I < 0)
                return R + "" + I + "i";
            return R + "+" + I + "i";
        }
    }
}
