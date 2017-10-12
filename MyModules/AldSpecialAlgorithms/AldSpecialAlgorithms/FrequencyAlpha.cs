using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public class FrequencyAlpha
    {
        public static float[] AllDistances(int[] locs)
        {
            int kn = locs.Length;
            int n = (kn - 1) * kn / 2;
            float[] res = new float[n];
            int pos=0;

            for (int i = 0; i < kn; i++)
                for (int j = i + 1; j < kn; j++)
                {
                    int diff = locs[j] - locs[i];
                    res[pos++] = diff;                     
                }
            if(pos!=n)
                throw new Exception("rayos");
            //Vectors.Quicksort(res);
            Vectors.BubbleSort(res);
            return res;
        }
        /*
        public static float[] ApplyArmonics(float[] fft, int[] locs)
        {
            float[] res = fft.Clone() as float[];
            int maxpos = locs[locs.Length - 1];
            float max = fft.Max();

            for (int i = 0; i < locs.Length; i++)
            {
                int idx = locs[i];
                int j = idx*2;
                int x = 0;
                while (j <= maxpos)
                {
                    if (fft[j] >= max / 20)
                        res[j] += (fft[j]/max)*fft[idx];
                    j += idx;
                    x++;
                }
            }
            return res;            
        }
         * */
        /*
        public static float[] ApplyArmonics3ssssss(float[] fft, int[] locs)
        {
            float[] res = new float[fft.Length];
            int maxpos = locs[locs.Length - 1];
            float max = fft.Max();

            for (int i = 0; i < locs.Length; i++)
            {
                int idx = locs[i];
                int j = idx * 1;
                int x = 0;
                int div = 2;
                if(fft[idx] >= max / 5)
                    while (j <= maxpos && x<10)
                    {                    
                        float add = fft[j] / div;
                        if(fft[idx]>=add)
                            res[idx] += fft[j]/div;
                        j += idx;
                        x++;
                        div++;
                    }
            }
            res.NormalizeFit();
            //res.Log10Normalization();
            return res;
        }
         * */
        public static float[] ApplyHarmonics3(float[] fft, int[] locs)
        {
            float[] res = new float[fft.Length];

            if (locs.Length == 0) return res;

            int maxpos = locs[locs.Length - 1];

            float sum = fft.Sum();

            int harm = 10;

            int div = 1;

            for (int i = 0; i < fft.Length / harm; i++)
            {                
                for (int j = 1; j < harm; j++)
                {
                    res[i] += fft[i * j]/(div++);
                }
            }

            res.NormalizeFit();

            return res;
        }
        /*
        public static float[] ApplyArmonics3ant(float[] fft, int[] locs)
        {
            float[] res = new float[fft.Length];

            if (locs.Length == 0) return res;

            int maxpos = locs[locs.Length - 1];
            float max = fft.Max();

            int harm = 12;            

            for (int i = 0; i < fft.Length/harm; i++)
            {
                if (fft[i] < max/8) continue;
                int div = 2;
                int cont = 0;
                for (int j = 0; j < harm; j++)
                {
                    var ax =  fft[i * (j + 1)]/(div++);
                    res[i] +=ax;
                    if(ax>=max/10)
                        cont++;
                }
                if (cont >= 3)
                    res[i] = 0;

            }

            res.NormalizeFit();
            //res.Log10Normalization();
            return res;
        }
        */
        /*
        public static float[] ApplyArmonics2(float[] fft, int[] locs)
        {
            float[] res = new float[fft.Length];
            int maxpos = locs[locs.Length - 1];
            float max = fft.Max();


            for (int i = 0; i<locs.Length; i++)
            {
                int idx = locs[i];

                if (idx > maxpos / 2) continue;
                //if (fft[idx] < max / 5) continue;

                int j = idx;
                int x = 0;
                while (j < maxpos)
                {
                    if(fft[j]>=max/20)
                        res[idx] += fft[j];
                    j += idx;
                    x++;
                }
                //if(x>0)
                //    res[idx] /= x;
                //res[idx] = (res[idx] + fft[idx]) / 2;
            }
            
            res.NormalizeFit();
            res.Log10Normalization();
            
            for (int i = 0; i < locs.Length; i++)
            {
                int idx = locs[i];
                res[idx] = (res[idx] * fft[idx] / max);
            }
            
            return res;
        }
         * */
    }
}
