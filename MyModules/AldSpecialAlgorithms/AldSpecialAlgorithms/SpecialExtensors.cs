using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class SpecialExtensors
    {
        //fit the positive limit to 1
        public static void AldFitPositiveLimitMax(this float[] where)
        {
            float max = where.Max();
            if (max == 0) return;
            for (int i = 0; i < where.Length; i++)
                where[i] /= max;
        }
        public static void AldWaveFitToOne(this float[] wave)
        { 
            float max = wave.Max();
            float min = wave.Min();
            if (min < 0) min = -min;
            if (min > max) max = min;
            
            if(max==0) return;

            for (int i = 0; i < wave.Length; i++)
                wave[i] /= max;
        }
        public static void AldFitValuesMinToMax(this float[] A,float minmin)
        {
            float max = A.Max();
            float min = A.Min();

            if (min < minmin && max < minmin)
            {
                for (int i = 0; i < A.Length; i++)
                    A[i] = 0;
            }

            max -= min;
            if (max == 0) return;
            for (int i = 0; i < A.Length; i++)
                A[i] = (A[i] - min) / max;
        }
        public static void AldNormalizePositive(this float[] A,float peak)
        {
            float val = A.Sum()/A.Length;
            float max = 0;
            for (int i = 0; i < A.Length; i++)
            {
                A[i] -= val;
                if (A[i] < 0) A[i] = 0;
                if (A[i] > max) max = A[i];
            }
            if(max==0) return;
            for (int i = 0; i < A.Length; i++)
                A[i] *= (peak / max);
        }        
        public static float[] AldAbsSignDistance(this float[] A, float[] B)
        {
            if (A.Length != B.Length) throw new Exception("The sizes of A and B are not equals...");
            float[] res = new float[A.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = Math.Abs(Math.Sign(A[i]) - Math.Sign(B[i]));
            return res;
        }
        public static float[] AldTofloat(this int[] A)
        {
            return A.Select(x => (float)x).ToArray();
        }
        public static float[] AldPow2(this float[] A)
        {
            float[] res = new float[A.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = A[i] * A[i];
            return res;
        }
        public static void AldSquare(this float[] A)
        {
            for (int i = 0; i < A.Length; i++)
                A[i] = A[i] * A[i];
        } 
        public static void AldSquare(this Complex[] A)
        {
            for (int i = 0; i < A.Length; i++)
                A[i] = A[i] * A[i];
        }
        public static float[] TsAbsolutize(this float[] A)
        {
            float[] res = new float[A.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = Math.Abs(A[i]);
            return res;        
        
        }
        public static float[] TsMultiply(this float[] A, float k)
        {
            float[] res = new float[A.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = A[i] * k;
            return res;        
        }
        public static void AldOperateIn(this float[] A, Func<float,float> fx)
        {
            for (int i = 0; i < A.Length; i++)
                A[i] = fx(A[i]);
        }
        public static float[] AldMultiply(this float[] A, float[] B)
        {
            if (A.Length != B.Length) throw new Exception("The sizes of A and B are not equals...");
            float[] res = new float[A.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = A[i] * B[i];
            return res;
        }

        public static void AldMultiply(this Complex[] A, Complex[] B)
        {
            if (A.Length != B.Length) throw new Exception("The sizes of A and B are not equals...");
            for (int i = 0; i < A.Length; i++)
                A[i]*= B[i];
        }
        public static float[] AldExtend(this float[] A,int newsize)
        {
            if (A.Length > newsize) throw new Exception("The newsize is less than the existing size.");
            float[] res = new float[newsize];
            Array.Copy(A, res, A.Length);
            return res;
        }

        public static float[] AldExtractModules(this Complex[] Who)
        {
            return Who.Select(x => x.Module()).ToArray();
        }
        public static Complex[] AldToComplex(this float[] Who)
        {
            return Who.Select(x => new Complex(x, 0)).ToArray();
        }
    }
}
