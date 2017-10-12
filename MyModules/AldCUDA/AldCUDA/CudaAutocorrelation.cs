using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        public static float[] AutoCorrelation(float[] Data,int lag, float windowsAmp, int windowsLength)
        {
            int n = Data.Length + windowsLength - 1;

            Complex* win = (Complex*)Marshal.AllocHGlobal(n * 8);
            Complex* x2 = (Complex*)Marshal.AllocHGlobal(n * 8);

            for (int i = 0; i < n; i++)
            {
                if (i + lag < Data.Length)
                {
                    float avg = 0;
                    for (int j = 0; j < lag; j++)
                        avg += Math.Abs(Data[i + lag])/lag;
                    
                    x2[i] = new Complex(avg, 0);
                }
                else
                    x2[i] = new Complex();
                win[i] = new Complex();
            }

            var hamming = Windows.Hamming(windowsLength);
            for (int i = 0; i < hamming.Length-lag; i++)
            {
                win[i].R = windowsAmp * hamming[i] * windowsAmp * hamming[i+lag];                
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            AldCudaAlgorithms.Convolution(x2, win, n);

            var dres = new float[Data.Length];//size of the convolution        

            Marshal.FreeHGlobal(new IntPtr((void*)win));

            int start = (windowsLength - 1) / 2;

            for (int i = 0; i < Data.Length; i++)
                dres[i] = x2[start + i].Module();

            Marshal.FreeHGlobal(new IntPtr((void*)x2));

            Debug.WriteLine("STE time : " + sw.ElapsedMilliseconds);

            sw.Stop();

            return dres;
        }
    }
}
