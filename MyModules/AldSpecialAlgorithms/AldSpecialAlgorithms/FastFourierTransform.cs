using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public class FastFourierTransform
    {
        public static int InvertBits(int n, int exponent)
        {
            int res = 0;
            for (int i = 0; i < exponent; i++)
            {
                res = (res << 1) | (n & 1);
                n = (n >> 1);
            }
            return res;
        }
        public static void NextPower2(int n, out int value, out int exponent)
        {
            value = 1;
            for (exponent = 0; value < n; exponent++)
                value = (value << 1);
        }
        public static void XFFT(Complex[] res,bool isInverse,int exponent=-1)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int newN, newR;
            NextPower2(res.Length, out newN, out newR);
            int q = 1;
            int sgn = isInverse ? -1 : 1;
            
            for (int m = 1; m <= newR / 2; m++, q *= 2)
            {
                //for (int k = 0; k < q; k++)
                Parallel.For(0, q, (k) =>
                {
                    Complex u = Omega(-k, 2 * q, sgn);
                    for (int n = 0; n < newN; n += 2 * q)
                    {
                        res[n + k + q] = res[n + k] - u * res[n + k + q];
                        res[n + k] = res[n + k] * 2 - res[n + k + q];
                    }
                }
                );
            }
            for (int m = newR / 2 + 1; m <= newR; m++, q *= 2)
            {   
                //for (int n = 0; n < newN; n += 2 * q)
                Parallel.For(0, newN / (2 * q), (nx) =>
                {
                    int n = nx * 2 * q;
                    for (int k = 0; k < q; k++)
                    {
                        Complex u = Omega(-k, 2 * q, sgn);
                        res[n + k + q] = res[n + k] - u * res[n + k + q];
                        res[n + k] = res[n + k] * 2 - res[n + k + q];
                    }

                });
            }      
            Console.WriteLine("Fourier Time ("+ res.Length +"): " + sw.ElapsedMilliseconds);
        }

        static Complex[] InitialValues(Complex[] values,int exponent=-1)
        {
            int newN, newR;
            if (exponent == -1)
                NextPower2(values.Length, out newN, out newR);
            else
            {
                newR = exponent;
                newN = (1 << exponent);
            }

            Complex[] res = new Complex[newN];


            //Is not necesary to go to newN because the empty positions is by default 0+0i
            for (int i = 0; i < values.Length; i++)
                res[InvertBits(i, newR)] = values[i];
            return res;
        }
        static void InitialValuesOn(Complex[] values)
        {
            int newN, newR;
            NextPower2(values.Length, out newN, out newR);
            if (newN != values.Length) throw new Exception("Error is not a x^2. length");
            //Is not necesary to go to newN because the empty positions is by default 0+0i            
            for (int i = 0; i < values.Length; i++)
            {
                int inverti = InvertBits(i, newR);
                if (inverti > i)
                {
                    var aux = values[inverti];
                    values[inverti] = values[i];
                    values[i] = aux;
                }                
            }
        }

        public static Complex[] FFT(Complex[] values,bool scale=false)
        {
            Complex[] res = InitialValues(values);
            XFFT(res, false);
            if (scale) Scale(res);
            return res;
        }
        public static Complex[] FFT(Complex[] values,int exponent, bool scale = false)
        {
            Complex[] res = InitialValues(values,exponent);
            XFFT(res, false);
            if (scale) Scale(res);
            return res;
        }

        private static void Scale(Complex[] res)
        {
            float x = res.Length;
            for (int i = 0; i < res.Length; i++)
                res[i] /= x;
        }
        public static Complex[] IFFT(Complex[] values,bool scale=true)
        {
            Complex[] res = InitialValues(values);
            XFFT(values, true);
            if (scale) Scale(res);
            return res;
        }
        public static void IFFTOn(Complex[] values, bool scale = true)
        {
            InitialValuesOn(values);
            XFFT(values, true);
            if (scale) Scale(values);
        }
        private static Complex Omega(int exponent, int index,int sgn)
        {
            float ime = (float)(sgn*(2*Math.PI*exponent)/index);
            //w(N)=exp(j2π/N)
            return Complex.FromEuler(0, ime);
        }
        public static float[] FFT(float[] values)
        {
            Complex[] v = values.Select(x => new Complex(x, 0)).ToArray();
            return FFT(v).Select(x => x.Module()).ToArray();
        }
        public static float[] IFFT(float[] values)
        {
            Complex[] v = values.Select(x => new Complex(x, 0)).ToArray();
            return IFFT(v).Select(x => x.Module()).ToArray();
        }
    }

}
