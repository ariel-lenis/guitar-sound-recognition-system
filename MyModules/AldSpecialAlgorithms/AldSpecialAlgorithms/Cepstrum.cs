using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsFFTFramework;

namespace AldSpecialAlgorithms
{
    public class Cepstrum
    {
        //public static float[] Poten
        public static float[][] CalculateCepstrum2(float[] data, int posa, int posb, int samplerate)
        {
            int n = posb - posa + 1;
            float[] cps = new float[n];
            float[] xs = new float[n];
            for (int i = posa; i <= posb; i++)
                if(i<data.Length)
                    cps[i - posa] = data[i];

            for (int i = 0; i < n; i++)
                xs[i] = i;// ((float)i / n) * samplerate;


            //Windows.ApplyHamming(cps, 1);

            var res = TsFFTFramework.AldCudaAlgorithms.XFFTReal(cps, TsFFTFramework.AldCudaAlgorithms.Direcion.Forward);
            // = res.AldMultiply(2.0f/ res.Length);

            //res.NormalizeFit();
            //res=res.AldMultiply((float)Math.E);
            //res.AldSquare();
            //res.Log10Normalization();
            
            
            res.Log(float.Epsilon);
            //res.AldSquare();


            res = TsFFTFramework.AldCudaAlgorithms.XFFTReal(res, TsFFTFramework.AldCudaAlgorithms.Direcion.Inverse);
            
            return new float[][] { xs, res };
        }
        public static float[][] AutoCorrelation(float[] data, int posa, int posb, int samplerate)
        {
            int n = posb - posa + 1;
            float[] cps = new float[n];
            float[] xs = new float[n];
            for (int i = posa; i <= posb; i++)
                if(i<data.Length)
                    cps[i - posa] = data[i];

            for (int i = 0; i < n; i++)
                xs[i] = i;// ((float)i / n) * samplerate;


            //Windows.ApplyHamming(cps, 1);

            var res = TsFFTFramework.AldCudaAlgorithms.XFFTReal(cps, TsFFTFramework.AldCudaAlgorithms.Direcion.Forward);
            res.AldSquare();
            res = TsFFTFramework.AldCudaAlgorithms.XFFTReal(res, TsFFTFramework.AldCudaAlgorithms.Direcion.Inverse);
            return new float[][] { xs, res };
        }

        public static float[] CepstrumToFrequency(float[] cepstrum,int samplerate,int maxfreq)
        {
            float[] freq = new float[cepstrum.Length / 2+1];
            float deltafreq = (float)samplerate / cepstrum.Length;

            int n = (int)(maxfreq / deltafreq);

            for (int i = 0; i < n; i++)
            {
                float pos = samplerate / (deltafreq * i);
                if (pos >= freq.Length) continue;

                int ixa = (int)pos;
                float mid = pos - ixa;

                freq[i] = cepstrum[ixa] + (cepstrum[ixa + 1] - cepstrum[ixa]) * mid;                
            }
            freq.NormalizeFit();
            return freq;
        }
        /*
        public static float[][] CalculateCepstrum(float[] data, int posa, int posb,int samplerate)
        {
            int n = posb - posa + 1;
            float[] cps = new float[n];
            float[] xs = new float[n];
            for(int i=posa;i<=posb;i++)
                cps[i-posa] = data[i];

            for (int i = 0; i < n; i++)            
                xs[i] = ((float)i / n) * samplerate;
            
            

            Windows.ApplyHamming(cps, 1);

            var res = AldCUDA.AldCudaAlgorithms.XFFTReal(cps, AldCUDA.AldCudaAlgorithms.Direcion.Forward);
            res.Log10Normalization();
            res = AldCUDA.AldCudaAlgorithms.XFFTReal(res, AldCUDA.AldCudaAlgorithms.Direcion.Forward);

            return new float[][] { xs, res };
        }
        */
    }
}
