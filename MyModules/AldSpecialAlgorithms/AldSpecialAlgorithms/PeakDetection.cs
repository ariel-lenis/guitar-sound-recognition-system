using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public struct PeakSolution
    {
        public float[] pks;
        public int[] locs;
    }
    public class PeakDetection
    {
        public static PeakSolution Detect(float[] X, float Ph=float.MinValue,int Pd=0,float? Th=null,bool sortbypks=false)
        {
            if (X.Length < 3) throw new Exception("Error the length must be almost 3.");

            for (int i = 0; i < X.Length; i++)
                if (float.IsInfinity(X[i]))
                    X[i] = float.IsPositiveInfinity(X[i]) ? float.MaxValue : float.MinValue;
                else if (float.IsNaN(X[i]))
                    throw new Exception("NaN values is not supported.");

            if (Pd > X.Length) throw new Exception("Pd must be less than X length.");

            List<float> pks = new List<float>();
            List<int> locs = new List<int>();

            getPeaksAboveMinPeakHeight(X, Ph,pks,locs);
            if(Th.HasValue)
                removePeaksBelowThreshold(X, pks, locs, Th.Value);
            removePeaksSeparatedByLessThanMinPeakDistance(X, pks, locs, Pd);

            if(sortbypks)
                Sort(pks, locs, false);
            else
                Sort(pks, locs, true);

            PeakSolution solution = new PeakSolution();
            solution.pks = pks.ToArray();
            solution.locs = locs.ToArray();

            return solution;
        }
        public static void getPeaksAboveMinPeakHeight(float[] X, float Ph,List<float> pks,List<int> locs)
        {
            int[] trend = SignOnDiff(X);
            int[] idx = FindValue(trend,0);
            int N = trend.Length;

            for (int i = idx.Length - 1; i >= 0; i--)
            {
                int a = idx[i];
                int m = min(a + 1, N - 1);
                if (trend[m] >= 0)
                    trend[idx[i]] = 1;
                else
                    trend[idx[i]] = -1;
            }

            trend = FindValue(Diff(trend),-2);

            for (int i = 0; i < trend.Length; i++)
                if (X[trend[i] + 1] > Ph)
                    locs.Add(trend[i] + 1);
            for (int i = 0; i < locs.Count; i++)
                pks.Add(X[locs[i]]);
        }
        public static void removePeaksBelowThreshold(float[] X, List<float> pks, List<int> locs, float Th)
        {
            List<int> delete = new List<int>();

            for (int i = 0; i < pks.Count; i++)
            {
                float delta= min(pks[i] - X[locs[i] - 1], pks[i] - X[locs[i] + 1]);
                if (delta < Th)
                    delete.Add(i);
            }

            for (int i = 0; i < delete.Count; i++)
            {
                pks.RemoveAt(delete[i]-i);
                locs.RemoveAt(delete[i] - i);
            }
        }
        public static void removePeaksSeparatedByLessThanMinPeakDistance(float[] X, List<float> pks, List<int> locs, int Pd)
        {
            if (pks.Count == 0 || Pd==1) return;
            Sort(pks, locs);

            bool[] delete = new bool[locs.Count];

            for(int i=0;i<delete.Length;i++)
            {
                if(delete[i]) continue;

                for(int j=0;j<locs.Count;j++)                
                    delete[j]|=(locs[j]>=locs[i]-Pd) && (locs[j]<=locs[i]+Pd);

                delete[i] = false;
            }

            int delcount = 0;
            for (int i = 0; i < delete.Length; i++)
                if (delete[i])
                {
                    locs.RemoveAt(i-delcount);
                    pks.RemoveAt(i - delcount);
                    delcount++;
                }            
        }
        private static void Sort(List<float> pks, List<int> locs,bool bylocks=false)
        {
            int n = pks.Count;
            if (n != locs.Count) throw new Exception("pks and locs havent the same size.");
            bool sort;
            for (int i = 0; i < n; i++)
            { 
                sort = false;
                for (int j = 0; j < n - 1; j++)
                    if ((bylocks && locs[j] > locs[j + 1]) || (!bylocks && pks[j] < pks[j + 1]))                    
                    {
                        sort = true;
                        
                        float ax = pks[j];
                        pks[j] = pks[j + 1];
                        pks[j + 1] = ax;

                        int bx = locs[j];
                        locs[j] = locs[j + 1];
                        locs[j + 1] = bx;
                    }                
                if (!sort) return;
            }
        }

        static int min(int a, int b)
        {
            if (a < b) return a;
            return b;
        }
        static float min(float a, float b)
        {
            if (a > b) return a;
            return b;
        }
        private static int[] FindValue(int[] trend, float value)
        {
            int[] res = new int[trend.Length];
            int idx = 0;
            for (int i = 0; i < trend.Length; i++)
                if (trend[i] == value) res[idx++] = i;
            int[] rres = new int[idx];
            Array.Copy(res, rres, idx);
            return rres;
        }
        public static int[] SignOnDiff(float[] X)
        {
            int[] res = new int[X.Length - 1];            
            for (int i = 0; i < res.Length; i++)
                res[i] = Math.Sign(X[i + 1] - X[i]);
            return res;
        }
        public static int[] Diff(int[] X)
        {
            int[] res = new int[X.Length - 1];
            for (int i = 0; i < res.Length; i++)
                res[i] = X[i + 1] - X[i];
            return res;
        }  
    }
}
