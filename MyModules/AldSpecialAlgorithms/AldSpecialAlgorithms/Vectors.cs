using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class Vectors
    {

        public static float[] CopyRange(this float[] who,int from,int length)
        {
            float[] res = new float[length];
            for (int i = 0; i < length; i++)
                res[i] = who[from + i];
            return res;
        }
        public static void TsMultiply(this float[] a,float[] b)
        {
            for (int i = 0; i < a.Length; i++)
                a[i] *= b[i];            
        }

        public static void TsReduceMargin(this float[] a,float min)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i] <= min)
                    a[i] = 0;
        }

        public static void Zero(this float[] where)
        {
            Array.Clear(where,0,where.Length);
        }
        public static void RepairBadPeaks(this float[] where)
        { 
            int size = where.Length;            

            int cont = 0;

            for (int i = 0; i < size-1; i++)
            {
                if (cont == 0)
                {
                    if (where[i] < where[i + 1])
                        cont = 1;
                }
                else
                {
                    if (where[i] == where[i + 1])
                        cont++;
                    else
                    {
                        if (where[i] < where[i + 1])
                            cont = 1;
                        else
                        {
                            if (cont > 0)
                            {
                                //Debug.WriteLine("Repair : " + cont);
                                where[i - cont / 2] *=1.25f;// float.Epsilon;
                            }
                            cont = 0;
                        }
                    }
                }
            }
        }
        public static float[] AdaptVector(this float[] where, int n)
        {
            if(where.Length==n) return where;

            float[] res = new float[n];            
            
            float k;

            if (n > where.Length)
            {
                //k = (float)where.Length / n;
                //for (int i = 0; i < n; i++)
                //    res[i] = where[(int)(i * k)];

                for (int i = 0; i < where.Length - 1; i++)
                {
                    k = (float)n / where.Length;
                    int a = (int)(k * i);
                    int b = (int)(k * (i+1));
                    int ab = b-a+1;
                    for (int j = 0; j < ab; j++)
                        res[a + j] = where[i] + (where[i+1] - where[i]) * j / (ab - 1);
                }
            }
            else
            {
                k = (float)n/where.Length;
                for (int i = 0; i < where.Length; i++)
                {
                    int pos = (int)(i * k);
                    if (where[i] < res[pos])
                        res[pos] = where[i];
                }
            }
            return res;
        }
        public static float[] LinSpace(int n,float val=0)
        {
            float[] res = new float[n];
            for (int i = 0; i < n; i++)
                res[i] = val;
            return res;
        }
        public static int MaxPosition(this float[] where)
        {
            int pos = 0;
            for (int i = 0; i < where.Length; i++)
                if (where[i] > where[pos])
                    pos = i; ;            
            return pos;
        }
        public static void BubbleSort<T>(this T[] elements) where T : IComparable
        {
            bool sorted;
            for (int i = 0; i < elements.Length; i++)
            {
                sorted = false;
                for (int j = 0; j < elements.Length-1; j++)
                    if (elements[j].CompareTo(elements[j+1]) > 0)
                    {
                        sorted = true;
                        T aux = elements[j];
                        elements[j] = elements[j + 1];
                        elements[j + 1] = aux;                        
                    }
                if (!sorted) return;
            }
        }
        public static void Quicksort<T>(this T[] elements) where T : IComparable
        {
            Quicksort(elements, 0, elements.Length-1);
        }
        private static void Quicksort<T>(T[] elements, int left, int right) where T : IComparable
        {
            int i = left, j = right;
            IComparable pivot = elements[(left + right) / 2];

            while (i <= j)
            {
                while (elements[i].CompareTo(pivot) < 0)
                    i++;
                while (elements[j].CompareTo(pivot) > 0)
                    j--;
                if (i <= j)
                {
                    T tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;

                    i++;
                    j--;
                }
            }
            if (left < j)
                Quicksort(elements, left, j);            

            if (i < right)            
                Quicksort(elements, i, right);            
        }
 
    }
}
