﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        public static float[] AverageTransform(float[] Data, float windowsAmp, int windowsLength)
        {
            int n = Data.Length + (windowsLength/2)*2;
            Complex* win = (Complex*)Marshal.AllocHGlobal(n * 8);
            Complex* x2 = (Complex*)Marshal.AllocHGlobal(n * 8);

            for (int i = 0; i < n; i++)
            {
                if (i < Data.Length)
                    x2[i] = new Complex(Math.Abs(Data[i]), 0);
                else
                    x2[i] = new Complex();
                win[i] = new Complex();
            }

            var hamming = Windows.Hamming(windowsLength);
            for (int i = 0; i < hamming.Length; i++)
            {
                win[i].R = windowsAmp * hamming[i];
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
    }
}
