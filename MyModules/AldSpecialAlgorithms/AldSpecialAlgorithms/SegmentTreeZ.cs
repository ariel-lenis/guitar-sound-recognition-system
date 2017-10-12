using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{

    public class SegmentTreeNodeZ<T> :IDisposable where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public SegmentTreeNodeZ<T>[] Nodes;
        public T Minimun,Maximun;
        public int IndexStart,Count;
        int mymaxelements;
        T[] Data;


        public void Dispose()
        {
            Nodes = null;
            Data = null;
        }

        public override string ToString()
        {            
            return "[" + IndexStart + "," + Count + "] Min=" + Minimun + " Max=" + Maximun;
        }

        public static SegmentTreeNodeZ<T> CreateAll(int maxelements,T[] data)
        {
            SegmentTreeNodeZ<T>[] row = CreateFirst(maxelements, data);

            while (row.Length > 1)
                row = NextRow(row, maxelements, data);
            return row[0];
        }

        public bool Query(int thestart, int thecount, ref T resminimun, ref T resmaximun)
        {            
            T auxmin = default(T);
            T auxmax = default(T);
            
            if (!((thestart>=this.IndexStart && thestart<=this.IndexStart+this.Count) || (this.IndexStart>=thestart&&this.IndexStart<=thestart+thecount))) return false;

            if (thestart < this.IndexStart)
            {
                thecount = thecount - (this.IndexStart - thestart);
                thestart = this.IndexStart;
            }
            if (thestart + thecount > this.IndexStart + this.Count)
                thecount = this.IndexStart + this.Count - thestart;

            if (thestart == this.IndexStart && thecount == this.Count)
            {
                resminimun = this.Minimun;
                resmaximun = this.Maximun;
                return true;
            }

            if (Nodes != null ) 
            {
                int ini = (thestart-Nodes[0].IndexStart) / Nodes[0].Count;

                for (int i = ini; i < Nodes.Length; i++)
                    if (Nodes[i].Query(thestart, thecount, ref auxmin, ref auxmax))
                    {
                        if (i == ini || Comparer<T>.Default.Compare(auxmin,resminimun)<0) resminimun = auxmin;
                        if (i == ini || Comparer<T>.Default.Compare(auxmax,resmaximun)>0) resmaximun = auxmax;                       

                        //if (i==ini || auxmin < resminimun) resminimun = auxmin;
                        //if (i==ini || auxmax > resmaximun) resmaximun = auxmax;                                               
                    }
                    else break;
            }
                
            else
            {
                MinMaxBetween(Data, thestart, thecount, out auxmin, out auxmax);
                resminimun = auxmin;
                resmaximun = auxmax;    
            }
            return true;
        }


        private static SegmentTreeNodeZ<T>[] NextRow(SegmentTreeNodeZ<T>[] row, int maxelements, T[] data)
        {
            SegmentTreeNodeZ<T>[] res;
            int n = row.Length / maxelements;
            int residue = row.Length % maxelements;
            if (residue > 0) n++;

            res = new SegmentTreeNodeZ<T>[n];

            T min, max;
            for (int i = 0; i < n; i++)
            {
                SegmentTreeNodeZ<T> node = new SegmentTreeNodeZ<T>();
                var idx = i * maxelements;                
                int plus = maxelements;
                if (idx + plus > row.Length) plus = residue;

                var ini = row[idx];
                var end = row[idx + plus-1];

                node.IndexStart = ini.IndexStart;
                node.Count = end.IndexStart+end.Count-ini.IndexStart;

                node.Data = data;


                MinMaxBetweenNodes(row, idx, plus, out min, out max);
                
                node.Minimun = min;
                node.Maximun = max;

                node.mymaxelements = maxelements;

                node.Nodes = SubArray(row,idx,plus);

                res[i]=node;
            }
            return res;
        }

        private static SegmentTreeNodeZ<T>[] SubArray(SegmentTreeNodeZ<T>[] row, int idx, int plus)
        {
            SegmentTreeNodeZ<T>[] res = new SegmentTreeNodeZ<T>[plus];
            Array.Copy(row, idx, res, 0, plus);
            return res;
        }
        static void MinMaxBetweenNodes(SegmentTreeNodeZ<T>[] row, int ini, int n, out T min, out T max)
        {
            min = row[ini].Minimun;
            max = row[ini].Maximun;

            for (int i = 1; i < n; i++)
            {
                var v = row[ini+i];
                if (Comparer<T>.Default.Compare(v.Minimun, min) < 0) min = v.Minimun;
                if (Comparer<T>.Default.Compare(v.Maximun, max) > 0) max = v.Maximun;
                //if (v.Minimun < min) min = v.Minimun;
                //if (v.Maximun > max) max = v.Maximun;
            }
        }
        private static SegmentTreeNodeZ<T>[] CreateFirst(int maxelements, T[] data)
        {
            SegmentTreeNodeZ<T>[] res;
            int n = data.Length / maxelements;
            int residue = data.Length % maxelements;
            if (residue > 0) n++;

            res = new SegmentTreeNodeZ<T>[n];

            T min, max;
            for (int i = 0; i < n; i++)
            {
                SegmentTreeNodeZ<T> node = new SegmentTreeNodeZ<T>();
                node.IndexStart = i * maxelements;
                node.Count = maxelements;
                if (node.IndexStart + node.Count > data.Length) node.Count = residue;
                node.Data = data;                
                MinMaxBetween(data, node.IndexStart, node.Count, out min, out max);
                node.Minimun = min;
                node.Maximun = max;
                node.Nodes = null;
                node.mymaxelements = maxelements;
                res[i]=node;
            }
            return res;
        }

        static void MinMaxBetween(T[] data, int ini, int n, out T min, out T max)
        {
            min = max = data[ini];
            for (int i = 1; i < n; i++)
            {
                T v = data[ini+i];
                if (Comparer<T>.Default.Compare(v, min) < 0) min = v;
                if (Comparer<T>.Default.Compare(v, max) > 0) max = v;
                //if(v<min) min = v;
                //if(v>max) max = v;
            }
        }


    }
}
