using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class Normalization
    {
        public static void NormalizeCentral(this float[] where)
        {
            double max=0, min=0;
            double prom = 0;
            for (int i = 0; i < where.Length; i++)
            {
                if (i == 0 || where[i] > max)
                    max = where[i];
                if (i == 0 || where[i] < min)
                    min = where[i];
                prom += where[i];
            }
            prom /= where.Length;
            if(Math.Abs(min)>Math.Abs(max))
                max = -min+prom;
            else 
                max = max-prom;
            for (int i = 0; i < where.Length; i++)
                where[i] = (float)((where[i] - prom) / max);
        }
        public static void NormalizeFit(this float[] where)
        {
            double max = 0, min = 0;
            for (int i = 0; i < where.Length; i++)
            {
                if (i == 0 || where[i] > max)
                    max = where[i];
                if (i == 0 || where[i] < min)
                    min = where[i];
            }
            double ratio = max - min;

            if (ratio == 0) return;

            for (int i = 0; i < where.Length; i++)
                where[i] = (float)((where[i] - min) / ratio);
        }
        public static void Log10Normalization(this float[] where)
        {
            where.NormalizeFit();
            for (int i = 0; i < where.Length; i++)
                where[i] = (float)Math.Log10(1 + where[i] * 10);
        }
        public static void Log(this float[] where,float plus=0)
        {
            for (int i = 0; i < where.Length; i++)
                where[i] = (float)Math.Log(where[i]+plus);
        }
    }
}
