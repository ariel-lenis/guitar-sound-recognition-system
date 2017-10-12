using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsFFTFramework;

namespace AldSpecialAlgorithms
{
    public class FrequenciesPeaks
    {        
        public class FPResult
        {
            public float[] allfft;
            public float[] allharm;
            public float[] allcepstofreq;
            public float[] allacorrtofreq;

            public float[] cepstrum;
            public float[] smoothcepstrum;

            public float[] autocorr;
            public float[] smoothautocorr;

            public float[] allhz;

            
            public float[] pksFFT;
            public float[] pksHarm;

            public float[] hzFFT;
            public float[] hzHarm;

            public int[] idxFFT;
            public int[] idxHarm;

        }                
        public static FPResult CalculatePeaks(float[] all,int start,int end,int samplerate,int pkdis,float pkthr,float minfrequency)
        {
            int size = end - start + 1;



            float[] fft = new float[size];
            

            for (int i = start; i <= end; i++)
                if(i<all.Length)
                    fft[i - start] = all[i];

            Windows.ApplyHamming(fft, 1);
            fft = TsFFTFramework.AldCudaAlgorithms.XFFTReal(fft, TsFFTFramework.AldCudaAlgorithms.Direcion.Forward).Select(u => 2 * u / fft.Length).Take(fft.Length / 2 + 1).ToArray();
            fft.Log10Normalization();

            //
            var spechistogram = AldAlgorithms.CreateOverlappedHistogram(fft, 512, 0.05f);
            int idx = spechistogram.IndexOfPercent(0.95f);

            float peakerror = spechistogram.c[idx] + spechistogram.window / 2;
            fft.TsReduceMargin(peakerror);
            //

            float[] X = new float[fft.Length];
            for (int i = 0; i < X.Length; i++)
            {
                X[i] = (float)i * (1.0f / size) * samplerate;
                if (X[i] < minfrequency) fft[i] = 0;
            }

            var peaks = PeakDetection.Detect(fft, Pd: pkdis, Th: pkthr);

            var harmonics = FrequencyAlpha.ApplyHarmonics3(fft, peaks.locs);
            harmonics.NormalizeFit();

            for (int i = 0; i < X.Length; i++)
            {
                var ax = (float)i * (1.0f / size) * samplerate;
                if (ax < minfrequency) harmonics[i] = 0;
            }

            var peaks2 = PeakDetection.Detect(harmonics, Pd: pkdis, Th: pkthr);            

            FPResult res = new FPResult();

            res.allfft = fft;
            res.allharm = harmonics;
            res.allhz = X;

            res.pksFFT = peaks.pks;
            res.idxFFT = peaks.locs;

            res.pksHarm = peaks2.pks;
            res.idxHarm = peaks2.locs;

            res.hzFFT = new float[peaks.locs.Length];
            for (int i = 0; i < peaks.locs.Length; i++)            
                res.hzFFT[i] = (float)(peaks.locs[i] * (1.0f / size) * samplerate);            

            res.hzHarm = new float[peaks2.locs.Length];
            for (int i = 0; i < peaks2.locs.Length; i++)
                res.hzHarm[i] = (float)(peaks2.locs[i] * (1.0f / size) * samplerate);


            res.cepstrum = Cepstrum.CalculateCepstrum2(all, start, end, samplerate)[1];
            res.autocorr = Cepstrum.AutoCorrelation(all, start, end, samplerate)[1];

            res.smoothcepstrum = AldCudaAlgorithms.WaveSmooth(res.cepstrum, 1/10.0f, 10);
            res.smoothautocorr = res.autocorr;

            res.smoothcepstrum = res.smoothcepstrum.TsMultiply(res.cepstrum.Max() / res.smoothcepstrum.Max());
            res.smoothautocorr = res.smoothautocorr.TsMultiply(res.autocorr.Max() / res.smoothautocorr.Max());

            res.allcepstofreq = Cepstrum.CepstrumToFrequency(res.smoothcepstrum, samplerate, 2000);
            res.allacorrtofreq = Cepstrum.CepstrumToFrequency(res.smoothautocorr, samplerate, 2000);

            for (int i = 0; i < res.allcepstofreq.Length; i++)
            {
                var ax = (float)i * (1.0f / size) * samplerate;
                if (ax < minfrequency)
                {
                    res.allcepstofreq[i] = 0;
                    res.allacorrtofreq[i] = 0;
                }
            }
            
            return res;
        }
        
    }
}
