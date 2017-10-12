using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public unsafe class TsFFTLink
    {
        public delegate void DFFT(Complex* input, Complex* output, int n, int direccion);
        public delegate void DMultipleFFT(Complex* input, Complex* output, int n, int batch, int direccion);
        public delegate bool DTest();

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibraryA(string library);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hlibrary, string library);

        public static DFFT FFT;
        public static DMultipleFFT MultipleFFT;
        public static DTest Test;

        public enum Modules { CudaFFT, FFTW };

        
        public static bool LoadModule(Modules emodule)
        {
            string module="";
            if (emodule == Modules.CudaFFT)
                module = "TsCudaDll.dll";
            else
                module = "TsFFTWDll.dll";

            IntPtr hlib = LoadLibraryA(module);
            if (hlib == IntPtr.Zero)
            {
                //System.Windows.Forms.MessageBox.Show("Loading");
                return false;
            }
            IntPtr ptrF = GetProcAddress(hlib, "Test");
            if (ptrF == IntPtr.Zero)
            {
                //System.Windows.Forms.MessageBox.Show("Test");
                return false;
            }
            Test = (DTest)Marshal.GetDelegateForFunctionPointer(ptrF, typeof(DTest));

            ptrF = GetProcAddress(hlib, "FFT");
            if (ptrF == IntPtr.Zero)
            {
                //System.Windows.Forms.MessageBox.Show("FFT");
                return false;
            }
            FFT = (DFFT)Marshal.GetDelegateForFunctionPointer(ptrF, typeof(DFFT));

            ptrF = GetProcAddress(hlib, "MultipleFFT");
            if (ptrF == IntPtr.Zero)
            {
                //System.Windows.Forms.MessageBox.Show("FFT");
                return false;
            }
            MultipleFFT = (DMultipleFFT)Marshal.GetDelegateForFunctionPointer(ptrF, typeof(DMultipleFFT));
            //System.Windows.Forms.MessageBox.Show("Test???");
            return Test();
        }
    }
}
