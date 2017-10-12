using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        public static void Convolution(Complex* a, Complex* b,int n)
        {            
            XFFT(a,n, Direcion.Forward);
            XFFT(b,n, Direcion.Forward);

            for (int i = 0; i < n; i++)            
                a[i] *= b[i];

            XFFT(a, n, Direcion.Inverse);
        }
    }
}
