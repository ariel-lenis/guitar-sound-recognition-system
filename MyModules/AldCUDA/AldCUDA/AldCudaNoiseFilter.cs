using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        public static float[] NoiseFilter(float[] data,int frequency,int maxFrequency)        
        {
            Complex* fft = (Complex*)Marshal.AllocHGlobal(data.Length*8);
            for(int i=0;i<data.Length;i++)
                fft[i] = new Complex(data[i],0);

            XFFT(fft, data.Length, Direcion.Forward);

            int nlimit = frequency / 2 + 1;

            int maxpos = (int)(((float)maxFrequency / frequency)*nlimit);
            for (int i = maxpos; i < nlimit; i++)
            {
                fft[i] = new Complex();
                fft[nlimit+i] = new Complex();
            }
            XFFT(fft, data.Length, Direcion.Inverse);

            float[] res = new float[data.Length];

            for (int i = 0; i < data.Length; i++)
                res[i] = fft[i].R/data.Length;
            Marshal.FreeHGlobal(new IntPtr((void*)fft));
            return res;
        }
    }
}
