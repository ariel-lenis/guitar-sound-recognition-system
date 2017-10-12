using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF = System.Windows;
using D = System.Drawing;
using S = System.Windows.Shapes;
using I = System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Globalization;
using TsFilesTools;


namespace AldWavDisplayTools
{
    public class AldTransformPoints<T>:IDisposable where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public struct Point
        {
            public int X;
            public T Y;
            public Point(int x, T y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        public float ScaleY,TranslationY;
        public float ContainerW, ContainerH;

        public List<TsWaveData<T>> Data;

        float percentX, percentSize;

        public float PercentX { get{return percentX;}}
        public float PercentSize { get { return percentSize; } }

        public AldTransformPoints()
        {
            ScaleY = 1;
            TranslationY = 0;
            percentX = 0;
            percentSize = 1;
            Data = new List<TsWaveData<T>>();
        }
        public void CalculateMargins(out int cstart, out int cn)
        {
            cstart = (int)(Data[0].Points.Length * percentX);
            cn = (int)(Data[0].Points.Length * percentSize);            
        }
        public float Density()
        {
            int cstart, cn;
            CalculateMargins(out cstart, out cn);
            return cn/ContainerW;
        }
        public List<float> MarksPositions(TsWaveData<T> data)
        {    
            int n = data.Points.Length;
            int cstart, cn;
            CalculateMargins(out cstart, out cn);
            float pixelspersample = ContainerW / cn;

            List<float> res = new List<float>();

            foreach (var i in data.Marks)
                res.Add((i.MarkPosition - cstart) * pixelspersample);

            return res;

        }
        /*
        public float MarkPosition(TimeMark i)
        {

        }
        */
        public List<Point[]> TransformPoints()
        {
            List<Point[]> res = new List<Point[]>();
            foreach (var i in Data)
                res.Add(TransformOneData(i));
            return res;
        }

        private Point[] TransformOneData(TsWaveData<T> iData)
        {
            int n = iData.Points.Length;
            int cstart, cn;

            CalculateMargins(out cstart, out cn);

            Point[] res = ResumePoints(iData, cstart, cn);
            cn = res.Length;

            float propX = ContainerW / cn;

            for (int i = 0; i < cn; i++)
            {
                res[i].X = (int)(i * propX);
                float y = -res[i].Y.ToSingle(CultureInfo.CurrentUICulture) * ScaleY + TranslationY;
                res[i].Y = (T)Convert.ChangeType(y, typeof(T));
            }

            return res;
        }

        private Point[] ResumePoints(TsWaveData<T> iData, int cstart, int cn)
        {
            var Points = iData.Points;
            int pointsperpixel = (int)(cn/ContainerW);
            if (pointsperpixel == 0)
            {
                Point[] resv = new Point[cn];
                
                for(int i=0;i<cn;i++)
                {
                    var aux = cstart + i;
                    resv[i].X =  aux;
                    resv[i].Y = Points[cstart + i];
                }
                return resv;
            }
            int n = cn / pointsperpixel;
            if (Points.Length % pointsperpixel != 0) n++;
            Point[] res = new Point[n * 2];
            for (int i = 0; i < n; i++)
                ProcessRange(i,iData, res, cstart+ i * pointsperpixel, pointsperpixel);
            return res;
        }

        private void ProcessRange(int idx, TsWaveData<T> iData, Point[] res, int p, int n)
        {
            var points = iData.Points;
            if (p + n >= points.Length) n = points.Length - p;
            T min = default(T), max = default(T);
            iData.Magic.Query(p, n, ref min, ref max);
            res[idx*2]=new Point(p, min);
            res[idx*2+1]=new Point(p , max);
        }
        public void AdjustMatrix()
        {
            float max = 1;//Data.Magic.Maximun.Tofloat(CultureInfo.CurrentUICulture);
            float min = -1;//Data.Magic.Minimun.Tofloat(CultureInfo.CurrentUICulture);
            float deltaM =(float)( max - min);
            percentX = 0;
            percentSize = 1;
            ScaleY = ContainerH / deltaM;
            TranslationY = -min*ScaleY;
        }

        public void SetParams(float percentX, float percentSize)
        {
            this.percentX = percentX;
            this.percentSize = percentSize;
        }
        public float PointsPercentContained()
        {
            int cstart, cn;
            CalculateMargins(out cstart, out cn);
            return (float)cn / Data[0].Points.Length;
        }
        public int PositionFromPercent(float x)
        {
            int cstart, cn;
            CalculateMargins(out cstart, out cn);
            return (int)(cstart + cn * x);
        }

        public void GetSelectionRange(float pstart, float plength, out int start, out int length)
        {
            int cstart, cn;
            CalculateMargins(out cstart, out cn);

            start = (int)(cstart + cn * pstart);
            length = (int)(cn * plength);
        }


        public void AddData(TsWaveData<T> newData)
        {
            if (this.Data.Count > 0 && this.Data[0].Points.Length != newData.Points.Length) throw new Exception("The size must be the same...");
            this.Data.Add(newData);
        }

        public void Dispose()
        {
            Data = null;
        }

        public float PercentFromPosition(int p)
        {
            int cstart, cn;
            CalculateMargins(out cstart, out cn);
            return (float)(p - cstart) / cn;
        }
    }
}
