using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF=System.Windows;
using D = System.Drawing;
using S = System.Windows.Shapes;
using I=System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using TsFilesTools;


namespace AldWavDisplayTools
{
    public class AldGdiWavesMotor<T>:IDisposable where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        WPF.Controls.Image canvas;
        D.Bitmap bmp;
        D.Graphics gbmp;
        int w, h;

        List<TsWaveData<T>> Data;

        Dictionary<TsWaveData<T>, System.Drawing.Pen> WavePens;
        Dictionary<TsWaveData<T>, System.Drawing.Pen> MarkPens;

        AldTransformPoints<T> aldTransform;

        public AldTransformPoints<T> AldTransform { get { return aldTransform; } }

        public int W { get { return w; } }
        public int H { get { return h; } }

        AldBitmapSourceCreator aldsource;

        public bool DrawMarks { get; set; }
        

        public AldGdiWavesMotor(WPF.Controls.Image canvas)
        {
            this.canvas = canvas;
            aldTransform = new AldTransformPoints<T>();
            Data = new List<TsWaveData<T>>();
            WavePens = new Dictionary<TsWaveData<T>, D.Pen>();
            MarkPens = new Dictionary<TsWaveData<T>, D.Pen>();
        }

        public void SetParams(float percentX,float percentSize)
        {
            aldTransform.SetParams(percentX,percentSize);
            Render();
        }

        public void RecreateImage(int w,int h)
        {
            if (bmp != null) { bmp.Dispose(); gbmp.Dispose(); }
            
            if (w == 0) w = 1;
            if (h == 0) h = 1;
            
            this.w = w;
            this.h = h;            
            
            bmp = new D.Bitmap(w,h);
            gbmp = D.Graphics.FromImage(bmp);

            aldTransform.ContainerW = w;
            aldTransform.ContainerH = h;

            aldsource = new AldBitmapSourceCreator(w, h);
            canvas.Source = aldsource.Bmp;
        }
        public void Render()
        {
            gbmp.FillRectangle(D.Brushes.White,new D.Rectangle(0,0,w,h));

            DrawPolilyne();
            if(DrawMarks)
                DrawMarkers();
            
            aldsource.DrawImage(bmp);
        }

        private void DrawMarkers()
        {
            foreach (var idata in Data)
            {
                if (idata.Marks == null) continue;
                var positions = aldTransform.MarksPositions(idata);

                for (int i = 0; i < idata.Marks.Count; i++)
                {
                    //if (idata.Marks[i].Data2 != 0)
                    if (!float.IsInfinity(positions[i]) && !float.IsNaN(positions[i]) && positions[i] < 1e10)
                    { 
                        if(idata.Marks[i].ExtraData=="NoteOn")
                            gbmp.DrawLine(MarkPens[idata], (float)positions[i], 0, (float)positions[i], h);
                        else if(idata.Marks[i].ExtraData=="NoteOff")                        
                            gbmp.DrawLine(new Pen(Color.Blue,2), (float)positions[i], 0, (float)positions[i], h);
                    }
                    //else
                        // gbmp.DrawLine(new D.Pen(D.Color.Blue, 3), (float)positions[i], 0, (float)positions[i], h);
                }
            }
           
        }
        private void DrawPolilyne()
        {
            //aldTransform.Data = Data;
            var alldraws = aldTransform.TransformPoints();
            for (int i = 0; i < Data.Count;i++ )
            {
                var tpoint = alldraws[i].Select(x => new D.PointF((float)x.X, x.Y.ToSingle(CultureInfo.CurrentUICulture))).ToArray();
                if (tpoint.Length < 2)
                {
                    Console.WriteLine("Empty");
                    return;
                }
                gbmp.DrawLines(WavePens[Data[i]], tpoint);
            }
        }

           
        public void AdjustMatrix()
        {
            //aldTransform.Data = Data;
            aldTransform.AdjustMatrix();      
        }

        public void AddData(TsWaveData<T> Data,Pen wavePen,Pen markPen)
        {
            if (this.Data.Contains(Data)) throw new Exception("¬¬");
            this.Data.Add(Data);
            this.aldTransform.AddData(Data);
            this.WavePens.Add(Data, wavePen);
            this.MarkPens.Add(Data, markPen);
        }
        public void AdoptMarks()
        { 
        }

        public float PointsPercentContained()
        {
            return aldTransform.PointsPercentContained();
        }



        public void Dispose()
        {
            this.canvas = null;
            this.Data = null;
            this.aldTransform.Dispose();
            bmp.Dispose();
            gbmp.Dispose();

        }

    }
}
