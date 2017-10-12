using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public partial class AldAlgorithms
    {
        public static FrequenciesTable CreateHistogramT(float[] data, int divisions, float max = float.NaN, float min = float.NaN)
        {
            max = float.MinValue;
            min = float.MaxValue;
            int n = data.Length;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max) max = data[i];
                if (data[i] < min) min = data[i];
            }

            float delta = (max - min) / divisions;

            max -= min;

            int[] histogram = new int[divisions];
            float[] centers = new float[divisions];
            float inversemean = (float)(min + 0.5 * delta * (divisions + 3));

            int c = 0;
            for (int i = 0; i < data.Length; i++)
            {
                double pos = 0;
                if (min != 0 || max != 0)
                    pos = ((data[i] - min) / max) * (divisions - 1);
                if(!double.IsNaN(pos))
                    histogram[(int)pos]++;
            }
            for (int i = 0; i < divisions; i++)
                centers[i] = min + delta / 2 + delta * i;
            return new FrequenciesTable(centers, histogram, n, inversemean);
        }

        public struct Histogram
        {
            public float[] c;
            public float[] h;

            public float window;

            public int MaxIndex() 
            {
                int im = -1;
                for (int i = 0; i < c.Length; i++)
                    if (i == 0 || this.h[i] > this.h[im])
                        im = i;
                return im;
            }

            public double SumTo(int idx)
            {
                double res = 0;
                for (int i = 0; i <= idx; i++)
                    res += this.h[i];
                return res;
            }

            public int IndexOfPercent(float percent)
            {
                double target = this.h.Sum() * percent;
                double acum = 0;
                int i = 0;
                for (i = 0; acum < target; i++)
                    acum += this.h[i];
                return i;
            }

            public Histogram(int n, float window)
            {
                this.c = new float[n];
                this.h = new float[n];
                this.window = window;
            }

            public float MaxValue()
            {
                int idx = this.MaxIndex();
                return this.c[idx];
            }
        }
        public static Histogram CreateOverlappedHistogram(float[] data,int np,float windowpercent,float max=float.NaN,float min=float.NaN)
        {
            if (float.IsNaN(max)) max = data.Max();
            if (float.IsNaN(min)) min = data.Min();

            float average = data.Average();
            average = data.StandarDeviation();

            float d = (max - min)/np;
            float window = average * windowpercent;

            //int[] res = new int[np];

            Histogram res = new Histogram(np,window);

            for (int i = 0; i < data.Length; i++)
            {                
                float val = data[i];
                for (int j = 0; j < np; j++)
                    if(Math.Abs(j*d-val)<=window)
                        res.h[j]++;                    
            }

            for (int i = 0; i < np;i++ )
                res.c[i] = min + i * d; 

            return res;
        }

        public static Histogram CreateOverlappedPlus(float[] data, int np, float windowpercent, float max = float.NaN, float min = float.NaN)
        {
            if (float.IsNaN(max)) max = data.Max();
            if (float.IsNaN(min)) min = data.Min();

            float average = data.Average();
            average = data.StandarDeviation();

            float[] props = new float[] { 4, 2, 1, 1.0f / 2, 1.0f / 4/*, 1.0f / 8, 1.0f / 16 */};

            float d = (max - min) / np;
            float window = average * windowpercent;

            //int[] res = new int[np];

            Histogram res = new Histogram(np, window);

            for (int i = 0; i < data.Length; i++)
            {
                //float val = data[i];
                for (int h = 0; h < props.Length; h++)
                {
                    float val = data[i] * props[h];
                    for (int j = 0; j < np; j++)
                    {
                        if (Math.Abs(j * d - val) <= window)
                            res.h[j]++;
                    }
                }
            }

            for (int i = 0; i < np; i++)
                res.c[i] = min + i * d;

            return res;
        }

        public static int[] CreateHistogram(float[] data, int divisions, float max = float.NaN, float min = float.NaN)
        {
            if (float.IsNaN(max)) max = data.Max();
            if (float.IsNaN(min)) min = data.Min();

            max -= min;

            int[] histogram = new int[divisions];

            for (int i = 0; i < data.Length; i++)
            {
                double pos = ((data[i] - min) / max) * (divisions - 1);
                if (double.IsNaN(pos)) pos = 0;
                histogram[(int)pos]++;
            }
            return histogram;
        }
        public static void AdjustWave(float[] data, float minpercent, int divisions = 100, float max = float.NaN, float min = float.NaN)
        {
            if (float.IsNaN(max)) max = data.Max();
            if (float.IsNaN(min)) min = data.Min();
            int[] histogram = CreateHistogram(data, divisions, max, min);
            float hmax = histogram.Max();
            float[] fhistogram = histogram.Select(x => x / hmax).ToArray();
            float sum = fhistogram.Sum();

            float leftsum = 0;
            int leftpos = 0;
            while (leftsum < minpercent * sum / 2)
                leftsum += fhistogram[leftpos++];

            float righttsum = 0;
            int rightpos = fhistogram.Length - 1;
            while (righttsum < minpercent * sum / 2)
                righttsum += fhistogram[rightpos--];

            int finalpos = (leftpos + (fhistogram.Length - 1 - rightpos)) / 2;

            float proportion = (float)finalpos / (fhistogram.Length - 1);
            proportion = min + proportion * (max - min);
            proportion = Math.Abs(proportion);

            /*
            if (Math.Abs(max) > Math.Abs(min))
                proportion /= Math.Abs(max);
            else
                proportion /= Math.Abs(min);
            */

            for (int i = 0; i < data.Length; i++)
                data[i] /= proportion;
        }
        public struct FrequenciesTable
        {
            public float[] centers;
            public int[] frequencies;
            public int n;
            public float inversemean;
            public FrequenciesTable(float[] centers, int[] frequencies,int n,float inversemean)
            {
                this.centers = centers;
                this.frequencies = frequencies;
                this.n=n;
                this.inversemean = inversemean;
            }
            public int MaxIndex()
            {
                int max = 0;
                for (int i = 0; i < frequencies.Length; i++)
                    if (frequencies[i] > frequencies[max])
                        max = i;
                return max;
            }
            public float MaxValue()
            { 
                return  this.centers[this.MaxIndex()];
            }
        }
        public static FrequenciesTable CreateHistogram(float[][] data, int divisions, float max = float.NaN, float min = float.NaN)
        {
            max = float.MinValue;
            min = float.MaxValue;
            int n = 0;
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[i].Length; j++)
                {
                    if (data[i][j] > max) max = data[i][j];
                    if (data[i][j] < min) min = data[i][j];
                }
                n += data[i].Length;
            }

            float delta = (max - min) / divisions;

            max -= min;

            int[] histogram = new int[divisions];
            float[] centers = new float[divisions];
            float inversemean = (float)(min + 0.5 * delta * (divisions + 3));

            int c = 0;
            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < data[i].Length; j++)
                {
                    double pos = ((data[i][j] - min) / max) * (divisions - 1);
                    if(!double.IsNaN(pos))
                        histogram[(int)pos]++;
                    //centers[c] = min + delta / 2 + delta * c;
                    //c++;
                }
            for (int i = 0; i < divisions;i++ )
                centers[i] = min + delta / 2 + delta * i;
            return new FrequenciesTable(centers, histogram,n,inversemean);
        }
        public static float GroupIntervalMean(FrequenciesTable histogram)
        { 
            double sum=0;
            for (int i = 0; i < histogram.centers.Length; i++)
                sum += histogram.centers[i]*histogram.frequencies[i];
            return (float)(sum / histogram.n);
        }

        public static float GroupIntervalStandarDeviation(FrequenciesTable histogram,float mean)
        {
            double sum = 0;
            for (int i = 0; i < histogram.centers.Length; i++)
            {
                float d = histogram.centers[i] - mean;
                sum += d*d * histogram.frequencies[i];
            }
            return (float)Math.Sqrt(sum / (histogram.n-1));
        }
    }
}
