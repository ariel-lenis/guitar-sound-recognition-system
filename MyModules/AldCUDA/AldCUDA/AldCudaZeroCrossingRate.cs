using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        public static float[] ZeroCrossingRate(float[] Data, float windowsAmp, int windowsLength)
        {
            int n = Data.Length + windowsLength - 1;

            Complex* win = (Complex*)Marshal.AllocHGlobal(n * 8);
            Complex* x2 = (Complex*)Marshal.AllocHGlobal(n * 8);

            for (int i = 0; i < n; i++)
            {
                if (i>0 && i < Data.Length)
                {
                    if (Sgn(Data[i]) != Sgn(Data[i - 1]))
                        x2[i] = new Complex(2, 0);
                    else
                        x2[i] = new Complex(0, 0);
                }
                else
                    x2[i] = new Complex();
                win[i] = new Complex();
            }

            var hamming = Windows.Hamming(windowsLength);
            for (int i = 0; i < hamming.Length; i++)
            {
                win[i+(n-hamming.Length)/2].R = windowsAmp * hamming[i];
            }

            AldCudaAlgorithms.Convolution(x2, win, n);


            var dres = new float[Data.Length];//size of the convolution            

            Marshal.FreeHGlobal(new IntPtr((void*)win));

            int start = (windowsLength - 1) / 2;

            for (int i = 0; i < Data.Length; i++)
                dres[i] = x2[start + i].Module();

            Marshal.FreeHGlobal(new IntPtr((void*)x2));

            return dres;
        }
        public static int Sgn(float x)
        {
            if (x < 0) return -1;
            return 1;
        }
    }
}
