using TsFFTFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms.LPC
{
    public class AldLPCTransform
    {
        public static float[] LPCTransform(float[] data, int sampleRate, float frameTime, int orderP)
        {
            float[] res = new float[data.Length];
            int samples = (int)(sampleRate * frameTime);
            int frames = (data.Length - orderP) / samples;
            for (int i = 0; i < frames; i++)
                FillData(orderP + i * samples, samples, data, res, orderP);

            return PreEmphasisFilter.PreEmphasis(res);
        }
        private static void FillData(int start, int n, float[] data, float[] res, int orderP)
        {
            float[] buff = new float[n + orderP];
            for (int i = 0; i < n + orderP; i++)
                buff[i] = data[start + i - orderP];

            buff = PreEmphasisFilter.PreEmphasis(buff);
            //Windows.ApplyHamming(buff, 1);

            var lpcres = AldLPC.LPC(buff, orderP);

            for (int i = orderP; i < buff.Length; i++)
            {
                float sum = 0;
                for (int j = 1; j < lpcres.Alphas.Length; j++)
                    sum -= lpcres.Alphas[j] * buff[i - j];
                //if (!float.IsNaN(lpcres.Error))
                res[start + i - orderP] = buff[i] - sum;
                //else
                //    res[start + i - orderP] = 0;
                //res[start + i-orderP] =  Math.Abs(buff[i] - sum);
                //res[start + i] = lpcres.Error;
            }
        }
    }

    /*
    public class AldLPCTransform
    {
        public static float[] LPCTransform(float[] data, int sampleRate, float frameTime, int orderP)
        {
            float[] res = new float[data.Length];
            int samples = (int)(sampleRate * frameTime);
            int frames = (data.Length - samples) / samples;
            for (int i = 0; i < frames; i++)
                FillData(samples / 2 + i * samples, samples, data, res, orderP);

            return PreEmphasisFilter.PreEmphasis( res);
        }
        private static void FillData(int start, int n, float[] data, float[] res, int orderP)
        {
            float[] buff = new float[n + n];
            for (int i = 0; i < n + n; i++)
                buff[i] = data[start + i - n / 2];

            buff = PreEmphasisFilter.PreEmphasis(buff);

            //float[] hamming = Windows.Hamming(buff.Length, 1);
            //Windows.ApplyHamming(buffe, 1);

            var lpcres = AldLPC.LPC(buff, orderP);

            for (int i = 0; i < n; i++)
            {
                float sum = 0;
                for (int j = 1; j < lpcres.Alphas.Length; j++)
                    sum -= lpcres.Alphas[j] * buff[n / 2 + i - j];

               
                if (!float.IsNaN(lpcres.Error))
                    res[start + i - orderP] = buff[n / 2 + i] - sum;
                else                
                    res[start + i - orderP] = 0;


            }
        }
    }*/

}
