using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class ErrorTools
    {
        /*
        public static double CompareAndGetError(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("Error the size of a and b is not the same.");

            float[] nxa = a.TsMultiply(1.0f);
            float[] nxb = b.TsMultiply(1.0f);
            
            double error = 0;

            for (int i = 0; i < nxa.Length; i++)
                error += Math.Abs(nxa[i] - nxb[i]);

            return error/nxa.Length;
        }
         * */

        public static AldAlgorithms.Histogram lastHA;
        public static AldAlgorithms.Histogram lastHB;

        public static double CompareAndGetError(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("Error the size of a and b is not the same.");

            float[] nxa = a.TsAbsolutize();
            float[] nxb = b.TsAbsolutize();


            //float ma = AldAlgorithms.CreateHistogramT(nxa, 1000).MaxValue();
            //float mb = AldAlgorithms.CreateHistogramT(nxb, 1000).MaxValue();

            var ha = AldAlgorithms.CreateOverlappedHistogram(nxa, 1000, 0.01f);
            var hb = AldAlgorithms.CreateOverlappedHistogram(nxb, 1000, 0.01f);

            lastHA = ha;
            lastHB = hb;

            float ma = ha.MaxValue();
            float mb = hb.MaxValue();
            
            Console.WriteLine("Ma:" + ma);
            Console.WriteLine("Mb:" + mb);
            
            //nxa.StandarNormalization();
            //nxb.StandarNormalization();

            double error = 0;
            int c = 0;
            for (int i = 0; i < nxa.Length; i++)
            {
                if (nxa[i] > ma || nxb[i] > mb)
                {
                    error += Math.Abs(nxa[i] - nxb[i]);
                    c++;
                }
            }

            double res = error / c;
            //if (double.IsNaN(res))
            //    throw new Exception("xD");
            return res;
        }


        public static float CompareMarkers(float[] toriginal, float[] tmarkers,float maxseconds)
        {
            toriginal = PrepareData(toriginal);

            bool[] maskoriginal = new bool[toriginal.Length];
            bool[] maskmarkers = new bool[tmarkers.Length];            

            for (int i = 0; i < toriginal.Length; i++)
            {
                float mindist = float.MaxValue;
                int idxmin = -1;

                for (int j = 0; j < tmarkers.Length; j++)
                { 
                    if(maskmarkers[j]) continue;
                    float dist = Math.Abs(toriginal[i]-tmarkers[j]);
                    if (dist < mindist)
                    {
                        idxmin = j;
                        mindist = dist;
                    }
                }

                if (idxmin>-1 && mindist <= maxseconds)
                {
                    maskoriginal[i] = true;
                    maskmarkers[idxmin] = true;
                }
            }

            int countoriginal=0;
            int countmarkers = 0;

            for (int i = 0; i < maskoriginal.Length; i++)
                if (!maskoriginal[i]) countoriginal++;

            for (int i = 0; i < maskmarkers.Length; i++)
                if (!maskmarkers[i]) countmarkers++;

            float errororiginal = (float)countoriginal / toriginal.Length;
            float errormarkers = (float)countmarkers / tmarkers.Length;

            return (errororiginal + errormarkers) / 2.0f;
        }

        private static float[] PrepareData(float[] toriginal)
        {
            List<float> res = new List<float>();

            for (int i = 1; i < toriginal.Length; i++)
                if (Math.Abs(toriginal[i - 1] - toriginal[i]) > 0.005)
                    res.Add(toriginal[i]);

            return res.ToArray();
        }

        
    }
}
