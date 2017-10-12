using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public class Windows
    {

        public static void ApplyHanning(float[] data,float amp=1)
        {
            int n = data.Length;
            for (int i = 0; i < n; i++)
                data[i] *= amp * (0.5f - 0.5f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1)));
        }

        public static float[] Morlet(int n, float amplitude = 1)
        {
            float[] res = new float[n];
            for (int i = 0; i < n; i++)
            {
                float range = 4;
                float x=-range+2.0f*range*i/n;
                res[i] = (float)(amplitude * (Math.Exp(-x*x/2)*Math.Cos(5*x)));
            }
            return res;
        }

        public static float[] Hamming(int n,float amplitude=1)
        {
            float[] res = new float[n];
            for (int i = 0; i < n; i++)
                res[i] = amplitude*( 0.54f - 0.46f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1)));
            return res;
        }

        public static Complex[] ComplexHamming(int n,float amplitude=1)
        {
            Complex[] res = new Complex[n];
            for (int i = 0; i < n; i++)
                res[i] = new Complex(amplitude*(0.54f - 0.46f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1))), 0);
            return res;
        }
        public static Complex[] ComplexRectangular(int n, float amplitude = 1)
        {
            Complex[] res = new Complex[n];
            for (int i = 0; i < n; i++)
                res[i] =  new Complex(amplitude,0);
            return res;
        }
        //
        public static Complex[] ComplexTriangular(int N, float amplitude = 1)
        {
            Complex[] res = new Complex[N];
            for (int n = 0; n < N; n++)
            {
                double val = amplitude*(1 - Math.Abs((n - (N - 1) / 2) / ((N + 1) / 2)));
                res[n] = new Complex((float)val, 0);
            }
            return res;
        }

        public static void ApplyHamming(float[] Y,float amplitude)
        {
            int n = Y.Length;
            for (int i = 0; i < n; i++)
                Y[i] *= amplitude * (0.54f - 0.46f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1)));
        }
        public static void ApplyHalfHamming(float[] Y, float amplitude)
        {
            float[] dhamming = Hamming(Y.Length*2, amplitude);

            int n = Y.Length;
            for (int i = 0; i < n; i++)
                Y[i] *= Y[i] * dhamming[Y.Length / 2 + i];
        }

        public static float[] Hanning(int n,float amplitude=1)
        {
            float[] res = new float[n];
            for (int i = 0; i < n; i++)
                res[i] = amplitude * (0.5f - 0.5f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1)));
            return res;
        }

        public static float[] HalfHanning(int n, float amplitude = 1)
        {
            float[] res = new float[n];
            for (int i = 0; i < n; i++)
                if(i>=n/2)
                    res[i] = amplitude * (0.5f - 0.5f * (float)Math.Cos(2 * (float)Math.PI * i / (n - 1)));
            return res;
        }

    }
}
