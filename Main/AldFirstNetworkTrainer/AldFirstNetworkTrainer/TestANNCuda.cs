using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AldFirstNetworkTrainer
{
    public class TestANNCuda
    {
        /*
        [DllImport(@"C:\Users\WOS.1\Documents\Visual Studio 2012\Projects\TsCudaANN\Debug\TsCudaANN.dll")]
        public static extern IntPtr SetData(float** data, int rows, int cols);
        [DllImport(@"C:\Users\WOS.1\Documents\Visual Studio 2012\Projects\TsCudaANN\Debug\TsCudaANN.dll")]
        public static extern void MyGetLastError(out string where);
         * */

        [DllImport(@"H:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern int SetWave(float[] wave,  int n);
        [DllImport(@"H:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern int SetWave(float[] wave,float[] rwave, int n);
        [DllImport(@"H:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern int Spectrogram(int fftsize, int samplesrequired, float[] window, float[] output);
    }
}
