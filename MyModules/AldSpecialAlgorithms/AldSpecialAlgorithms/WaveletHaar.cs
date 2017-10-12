using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public class WaveletHaar
    {
        public static float[] Haar1D(float[] data)
        {
            int n = data.Length;
            int i = 0;
            int w = n;
            float[] vecp = new float[n];
            float[] vec = new float[n];

            for (i = 0; i < n; i++)
            {
                vecp[i] = 0;
                vec[i] = data[i];
            }
            while (w > 1)
            {
                w /= 2;
                for (i = 0; i < w; i++)
                {
                    vecp[i] = (vec[2 * i] + vec[2 * i + 1]) / (float)Math.Sqrt(2.0);
                    vecp[i + w] = (vec[2 * i] - vec[2 * i + 1]) / (float)Math.Sqrt(2.0);
                }

                for (i = 0; i < (w * 2); i++)
                    vec[i] = vecp[i];
            }
            return vec;
        }

        private const float root2 = 1.414213562373095f;
        private const float w0 = root2;
        private const float w1 = -root2;
        private const float s0 = root2;
        private const float s1 = root2;

        public static float[] FWT(float[] d)
        {
            float[] data = new float[d.Length];
            for (int i = 0; i < d.Length; i++)
                data[i] = d[i];

            float[] temp = new float[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[i] = data[k] * s0 + data[k + 1] * s1;
                temp[i + h] = data[k] * w0 + data[k + 1] * w1;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
            return data;
        }

        public static float[] NxtHaar(float[] d,int levels)
        {
            int n2 = 1;
            while (n2 < d.Length)
                n2 *= 2;
            n2 = d.Length;
            float[] data = new float[n2];
            for(int i=0;i<d.Length;i++)
                data[i]=d[i];

            int n = data.Length;
            for (int i = 0; i < levels;i++ )
            {
                ApplyNxtHaar(data, n);
                n /= 2;
            }
            return data;
        }

        private static void ApplyNxtHaar(float[] data, int n)
        {
            float[] trend = new float[n];

            for (int i = 0; i < n / 2;i++ )
            {
                trend[i] = (data[2 * i] + data[2 * i + 1]) / (float)Math.Sqrt(2);
                trend[i+n/2] = (data[2 * i] - data[2 * i + 1]) / (float)Math.Sqrt(2);
            }
            for (int i = 0; i < n; i++)
                data[i] = trend[i];
        }


        public static float[] InverseNxtHaar(float[] haar, int levels)
        {
            float[] data = new float[haar.Length];
            for (int i = 0; i < haar.Length; i++)
                data[i] = haar[i];

            int nx = haar.Length;
            for (int i = 1; i < levels; i++)
                nx /= 2;
            
            for (int i = 0; i < levels; i++)
            {
                ApplyInverseNxtHaar(data, nx);
                nx *= 2;
            }
            return data;
        }

        private static void ApplyInverseNxtHaar(float[] data, int nx)
        {
            float[] trend = new float[nx];

            for (int i = 0; i < nx / 2; i++)
            {
                trend[2*i] = (data[i] + data[nx/2+i]) / (float)Math.Sqrt(2);
                trend[2 * i + 1] = (data[i] - data[nx / 2 + i]) / (float)Math.Sqrt(2);
            }
            for (int i = 0; i < nx; i++)
                data[i] = trend[i];
        }
    }
}
