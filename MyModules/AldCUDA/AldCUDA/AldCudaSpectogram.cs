using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFFTFramework
{
    public unsafe partial class AldCudaAlgorithms
    {
        /*
         * Old Grade 1
        public static float[][] AldGenerateSpectogramRaw(float[] data, int fftsize)
        { 
            int n = data.Length;
            n = (n / fftsize) * fftsize;
            int columns = n / fftsize;
            int rows = fftsize / 2;
            float[][] res = new float[rows][];         


            for (int i = 0; i < rows; i++)
                res[i] = new float[columns];

            data = XFFTBlocks(data, 0, n, fftsize, Direcion.Forward);

            for (int i = 0; i < data.Length; i++)
            {
                int irow = i%columns;
                int icol = i / columns;
                res[irow][icol] = data[i];
            }
            return res;
        }
         * */
        /*
         * Old Grade 2
        public static float[][] AldGenerateSpectogram2(float[] data, int fftsize,bool xflag,out float[] amps)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int overlapp = fftsize / 4;
            int n = (data.Length - overlapp) / (fftsize - overlapp);

            float[] res = new float[n * fftsize];

            float[] window = Windows.Hamming(fftsize,1);

            for (int i = 0; i < n; i++)
                for (int j = 0; j < fftsize; j++)
                    //if(xflag)
                        res[i * fftsize + j] = data[i * (fftsize - overlapp) + j]*window[j];            
                    //else
                    //    res[i * fftsize + j] = data[i * (fftsize - overlapp) + j];

            Debug.WriteLine("Spec time : " + sw.ElapsedMilliseconds);

            return AldGenerateSpectogram2d(res, fftsize,xflag,out amps);
        }
        */
        /*
        public static float[][] AldGenerateSpectogram2(float[] data, int fftsize,int samplesrequired, bool xflag, out float[] amps)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Nl = data.Length;
            int Hn = fftsize;

            int r = (Nl - samplesrequired) % (samplesrequired - 1);//Nl must be multiple of (samplesrequired-1)
            if (r > 0) Nl = Nl + (samplesrequired - 1 - r);//round to multiple of (samplesrequired-1)
            int distance = (Nl - Hn) / (samplesrequired - 1);

            int loststart = (Hn / 2) / distance;
            //if(loststart<)
            int lpad = loststart * Hn;

            float[] res = new float[lpad+Hn*samplesrequired];
            float[] window = Windows.Hanning(Hn, 1);

            for (int i = 0; i < samplesrequired; i++)
                for (int j = 0; j < Hn; j++)
                {
                    int idx = i * distance + j;
                    if (idx < data.Length)
                    {
                        var val = data[idx] *(0+1* window[j]);
                        res[lpad+i * Hn + j] = val;
                    }
                }

            Debug.WriteLine("Spec time : " + sw.ElapsedMilliseconds);

            var result =  AldGenerateSpectogram2d(res, fftsize, xflag, out amps);
            return result;

        }
         * */
        public static float[][] AldGenerateSpectogram2BK(float[] data, int fftsize, int samplesrequired, bool xflag, out float[] amps)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Nl = data.Length;
            int Hn = fftsize;
            int Np = samplesrequired;

            double p = (double)Nl / (Np - 1);

            float[] res = new float[samplesrequired * fftsize];
            float[] window = Windows.Hanning(Hn);
            //int c=0;
            int padd = fftsize/2;

            Parallel.For(0, samplesrequired, new Action<int>(delegate(int i)

            //for (int i = 0; i < samplesrequired; i++)
            {
                int j;
                int start = (int)(i * p) - padd;
                if (start < 0) j = -start;
                else j = 0;
                int size = fftsize;
                if (start + size > data.Length)
                    size = data.Length - start;
                for (; j < size; j++)
                    res[(i*Hn+j)] = data[start + j] * window[j];
            }
            
            ));

            Debug.WriteLine("Spec time : " + sw.ElapsedMilliseconds);

            var result = AldGenerateSpectogram2d(res, fftsize, xflag, out amps);
            return result;
        }

        [DllImport("TsCudaDll.dll")]
        public static extern IntPtr HostCloneWaveToDevice(float[] wave, int n);
        [DllImport("TsCudaDll.dll")]
        public static extern int Spectrogram(IntPtr data,int n, int fftsize, int samplesrequired, float[] window, float[] output,out float mean,out float std);
        [DllImport("TsCudaDll.dll")]
        public static extern void HostFastFourierTransform(IntPtr deviceData, int n, int direction);



        public static float[][] AldGenerateSpectogram2(float[] data, int fftsize, int samplesrequired, bool xflag, out float[] amps,out float mean,out float std)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IntPtr cudaHandle = HostCloneWaveToDevice(data, data.Length);
            Debug.WriteLine("SetWave:" + cudaHandle);

            float[] window = Windows.Hanning(fftsize);

            int _fftsize = (fftsize + 1) / 2;
            float[] output = new float[samplesrequired * _fftsize];
            
            lock (window)
            {
                lock (output)
                {
                    int bresult = Spectrogram(cudaHandle,data.Length, fftsize, samplesrequired, window, output,out mean,out std);
                    Debug.WriteLine("cudaSpectrogram:" + bresult + "\nMean:" + mean + "\nStd:" + std);
                }
            }
            
            Debug.WriteLine("Spec time : " + sw.ElapsedMilliseconds);
            var result = AldGenerateSpectogram2d(output, fftsize, xflag, out amps);
            return result;

        }
        public static float[][] AldGenerateSpectogram2d(float[] data, int fftsize,bool xflag,out float[] amps)
        {
            int n = data.Length;
            fftsize = (fftsize + 1) / 2;
            int columns = n / fftsize;
            int rows = fftsize;
            float[][] res = new float[rows][];

            for (int i = 0; i < rows; i++)
                res[i] = new float[columns];

            //data = XFFTBlocks(data, 0, n, fftsize, Direcion.Forward);            

            //float min = data.Min();
            //float val = data.Max()-min;

            //if (!xflag)
            //    min = 0;
            Stopwatch sw = new Stopwatch();

            sw.Start();

            amps = new float[columns];           

            for (int i = 0; i < columns; i++)
            {
                float max = 0;
                for (int j = 0; j < rows; j++)
                {
                    //double customr = Math.Exp(j * Math.Log(1 + rows) / rows) - 1;
                    //double customr = PowH(j * LogH(1 + rows) / rows) - 1;

                    ///
                    //if (data[i * fftsize + j] > max) max = data[i * fftsize + j];
                    ///

                    //if (customr < 0) customr = -customr;
                    int pos = j;//+0*(int)customr;                    

                    double part = (data[i*fftsize+pos]/*-min*/);

                    double prop;

                    //if (!xflag)
                    //    prop = Math.Log(1 + part) / Math.Log(1 + val);
                    //else
                    //    prop = Math.Log10(1 + part/val*9);



                    prop = part;

                    ///
                    //prop = Math.Log(1 + part);
                    ///


                    //double prop = Math.Log(1+part)/Math.Log(1+val);
                    //int lrow = (int)(rows * Math.Log10(1 + j / div) / Math.Log10(1 + rows / div));
                    res[j][i] = (float)prop;
                }
                amps[i] = max;
            }

            Console.WriteLine("Elapsed blocks : " + sw.ElapsedMilliseconds);

            return res;
        }

        public static float[][] AldGenerateSpectogram2d222(float[] data, int fftsize, bool xflag, out float[] amps)
        {
            int n = data.Length;
            n = (n / fftsize) * fftsize;
            int columns = n / fftsize;
            int rows = fftsize / 2;
            float[][] res = new float[rows][];

            for (int i = 0; i < rows; i++)
                res[i] = new float[columns];

            //data = XFFTBlocks(data, 0, n, fftsize, Direcion.Forward);            

            float min = data.Min();
            float val = data.Max() - min;

            if (!xflag)
                min = 0;
            Stopwatch sw = new Stopwatch();

            sw.Start();

            amps = new float[columns];

            for (int i = 0; i < columns; i++)
            {
                float max = 0;
                for (int j = 0; j < rows; j++)
                {
                    //double customr = Math.Exp(j * Math.Log(1 + rows) / rows) - 1;
                    double customr = PowH(j * LogH(1 + rows) / rows) - 1;
                    if (data[i * fftsize + j] > max) max = data[i * fftsize + j];
                    if (customr < 0) customr = -customr;
                    int pos = j + 0 * (int)customr;

                    double part = (data[i * fftsize + pos] - min);

                    double prop;

                    //if (!xflag)
                    //    prop = Math.Log(1 + part) / Math.Log(1 + val);
                    //else
                    //    prop = Math.Log10(1 + part/val*9);



                    prop = part;

                    //double prop = Math.Log(1+part)/Math.Log(1+val);
                    //int lrow = (int)(rows * Math.Log10(1 + j / div) / Math.Log10(1 + rows / div));
                    res[j][i] = (float)prop;
                }
                amps[i] = max;
            }

            Console.WriteLine("Elapsed blocks : " + sw.ElapsedMilliseconds);

            return res;
        }

        static double LogH(double val)
        {
            double b = Math.Pow(2, 1);
            return Math.Log(val)/Math.Log(b);
        }
        static double PowH(double val)
        {
            return Math.Pow(2, val);
        }
    }
}
