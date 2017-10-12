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
        /*
        [DllImport(@"Dll\TsCudaDll.dll")]
        public static extern void FFT(Complex[] input, Complex* output, int n, int direccion);
        [DllImport(@"Dll\TsCudaDll.dll")]
        public static extern void FFT(Complex* input, Complex* output, int n, int direccion);
        [DllImport(@"Dll\TsCudaDll.dll")]
        public static extern void MultipleFFT(Complex* input, Complex* output, int n, int batch, int direccion);
        [DllImport(@"Dll\TsCudaDll.dll")]
        public static extern bool HaveCuda();
        [DllImport(@"Dll\TsCudaDll.dll")]
        public static extern bool SetCuda(bool status);
        */

        public enum Direcion{Forward,Inverse};

        public static void XFFT(Complex* data,int n, Direcion direccion)
        { 
            int dir = (direccion==Direcion.Forward)?1:-1;
            TsFFTLink.FFT(data, data, n, dir);
        }
        public static void XFFT(Complex* input,Complex* output,int n, Direcion direccion)
        {
            int dir = (direccion == Direcion.Forward) ? 1 : -1;
            TsFFTLink.FFT(input, output, n, dir);
        }
        public static Complex[] XFFTRtoC(float[] vals, Direcion direccion)
        {
            Complex* data = (Complex*)Marshal.AllocHGlobal(vals.Length * 8);
            int dir = (direccion == Direcion.Forward) ? 1 : -1;

            for (int i = 0; i < vals.Length; i++)
                data[i] = new Complex(vals[i], 0);

            TsFFTLink.FFT(data, data, vals.Length, dir);

            Complex[] res = new Complex[vals.Length];

            for (int i = 0; i < vals.Length; i++)
                res[i] = data[i];
            Marshal.FreeHGlobal(new IntPtr((void*)data));
            return res;
        }
        public static Complex[] XFFTCtoC(Complex[] vals, Direcion direccion)
        {
            Complex* data = (Complex*)Marshal.AllocHGlobal(vals.Length * 8);
            int dir = (direccion == Direcion.Forward) ? 1 : -1;

            for (int i = 0; i < vals.Length; i++)
                data[i] = vals[i];

            TsFFTLink.FFT(data, data, vals.Length, dir);

            Complex[] res = new Complex[vals.Length];

            for (int i = 0; i < vals.Length; i++)
                res[i] = data[i];
            Marshal.FreeHGlobal(new IntPtr((void*)data));
            return res;
        }

        public static float[] XFFTBlocks(float[] vals,int start,int n,int blocksize,Direcion direccion)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Complex* data = (Complex*)Marshal.AllocHGlobal(n * 8);
            int dir = (direccion == Direcion.Forward) ? 1 : -1;

            float[] hanning = Windows.Hamming(blocksize);

            for (int i = 0; i < n/blocksize; i++)
                for (int j = 0; j < blocksize; j++)
                    data[i * blocksize + j] = new Complex(vals[start + i * blocksize + j] * hanning[j], 0);

            TsFFTLink.MultipleFFT(data, data, blocksize, n / blocksize, dir);
            GC.Collect();
            float[] res = new float[n];

            for (int i = 0; i < n; i++)
                res[i] = data[i].Module();

            Marshal.FreeHGlobal(new IntPtr((void*)data));
            Console.WriteLine("Batch FFT:" + sw.ElapsedMilliseconds);
            return res;
        }



        public static float[] XFFTReal(float[] vals, Direcion direccion)
        {
            Complex* data = (Complex*)Marshal.AllocHGlobal(vals.Length * 8);
            int dir = (direccion == Direcion.Forward) ? 1 : -1;

            for (int i = 0; i < vals.Length; i++)
                data[i] = new Complex(vals[i], 0);

            TsFFTLink.FFT(data, data, vals.Length, dir);

            float[] res = new float[vals.Length];

            for (int i = 0; i < vals.Length; i++)
                res[i] = data[i].Module();
            Marshal.FreeHGlobal(new IntPtr((void*)data));
            return res;
        }
    }

}
