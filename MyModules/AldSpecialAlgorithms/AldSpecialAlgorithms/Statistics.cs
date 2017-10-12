using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class Statistics
    {
        public static float Mean(this float[][] where)
        {
            double sum = 0;
            int c = 0;
            for (int i = 0; i < where.Length; i++)
                for (int j = 0; j < where[i].Length; j++)
                {
                    sum += where[i][j];
                    c++;
                }
            return (float)(sum / c);
        }
        public static float Mean(this float[] where)
        {
            double sum = 0;
            for (int i = 0; i < where.Length; i++)
                sum += where[i];
            return (float)(sum / where.Length);
        }
        public static float StandarDeviation(this float[][] where, float mean = float.NegativeInfinity)
        {
            double sum = 0;
            if (float.IsNegativeInfinity(mean))
                mean = where.Mean();
            int c = 0;
            for (int i = 0; i < where.Length; i++)
                for (int j = 0; j < where[i].Length; j++)
                {
                    float delta = where[i][j] - mean;
                    sum += delta * delta;
                    c++;
                }
            return (float)Math.Sqrt(sum / c);
        }
        public static float StandarDeviation(this float[] where,float mean=float.NegativeInfinity)
        {
            double sum = 0;
            if(float.IsNegativeInfinity(mean))
                mean = where.Mean();
            for (int i = 0; i < where.Length; i++)
            {
                float delta = where[i] - mean;
                sum += delta*delta;
            }
            return (float)Math.Sqrt(sum/where.Length);
        }
        public static void StandarNormalizationBlock(this float[] where,int div)
        {
            float mean = where.Mean();
            float deviation = StandarDeviation(where, mean);

            float[] ax = new float[where.Length / div];
            int subn = where.Length/div;
            for (int i = 0; i < div; i++)
            {
                for (int j = 0; j < subn; j++)
                    ax[j] = where[i * subn + j];
                ax.StandarNormalization();
                for (int j = 0; j < subn; j++)
                    where[i * subn + j]=ax[j];
            }
        }
        public static void MeanCenterBlock(this float[] where, int div)
        {
            float mean = where.Mean();
            float[] ax = new float[where.Length / div];
            int subn = where.Length / div;
            for (int i = 0; i < div; i++)
            {
                for (int j = 0; j < subn; j++)
                    ax[j] = where[i * subn + j];
                ax.MeanCenter();
                for (int j = 0; j < subn; j++)
                    where[i * subn + j] = ax[j];
            }
        }
        public static void MeanCenter(this float[] where)
        {
            float mean = where.Mean();
            for (int i = 0; i < where.Length; i++)
                where[i] = where[i] - mean;
        }

        public static void StandarNormalization(this float[] where)
        {
            float mean = where.Mean();
            float deviation = StandarDeviation(where, mean);

            if (deviation == 0) return;

            for (int i = 0; i < where.Length; i++)
                where[i] = (where[i] - mean) / deviation;
        }
        public static void ChebNormalization(this float[] where,float percent)
        {
            float mean = where.Mean();
            float deviation = StandarDeviation(where, mean);

            float m = (float)Math.Sqrt(1.0f / (1 - percent));

            float div = m * deviation / 2;

            for (int i = 0; i < where.Length; i++)
                where[i] = (where[i] - mean) / div;
        }
    }
}
