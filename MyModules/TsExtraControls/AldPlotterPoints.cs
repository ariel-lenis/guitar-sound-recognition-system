using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;

namespace TsExtraControls
{
    public partial class AldPlotterPoints : UserControl
    {
        GraphPane Graphics;

        public string Title
        {
            get {
                return Graphics.Title.Text;
            }
            set {
                Graphics.Title.Text = value;
            }
        }

        double? _minx, _maxx, _miny, _maxy;

        Extra.AxisConfig axisX;
        Extra.AxisConfig axisY;

        public Extra.AxisConfig AxisX
        {
            get 
            {
                return axisX;
            }
            set 
            {
                Graphics.XAxis.Title.Text=value.AxisTitle;
                Graphics.XAxis.Scale.MinorStep=value.MinorMarks;
                Graphics.XAxis.Scale.MajorStep=value.Mayormarks;
                this.zedGraphControl1.Invalidate();
                axisX = value;
            }
        }
        public Extra.AxisConfig AxisY
        {
            get
            {
                return axisY;
            }
            set
            {
                Graphics.YAxis.Title.Text = value.AxisTitle;
                Graphics.YAxis.Scale.MinorStep = value.MinorMarks;
                Graphics.YAxis.Scale.MajorStep = value.Mayormarks;
                this.zedGraphControl1.Invalidate();
                axisY = value;
            }
        }
        public AldPlotterPoints()
        {
            InitializeComponent();
            this.Graphics = this.zedGraphControl1.GraphPane;
            this.BindingContextChanged += AldPlotterPoints_BindingContextChanged;
            Graphics.AddCurve("X,Y Axis", new double[] { 0, 0 }, new double[] { 10000, -10000 }, Color.Black); // Se controla manualmente que el rango del eje X está continuamente

            this.Graphics.YAxis.Scale.Format = "F4";
        }

        public void ClearCurves()
        {
            var aux = Graphics.CurveList.First();
            Graphics.CurveList.Clear();
            Graphics.CurveList.Add(aux);
            zedGraphControl1.Invalidate();
            _minx=_miny=_maxx=_maxy = null;
        }

        public void AddCurve(string curvetitle, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None)
        {
            AddCurve(curvetitle, new float[] { }, new float[] { }, curvecolor, symboltype);
        }
        public void AddCurve<T>(string curvetitle, IEnumerable<T> pointsX, IEnumerable<T> pointsY, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None) where T : IConvertible
        {
            /*
            PointPairList pointList = new PointPairList();
            LineItem curve = this.Graphics.AddCurve(curvetitle, pointList, curvecolor, (SymbolType)symboltype);

            for (int i = 0; i < pointsY.Count(); i++)
            {
                double x = i;
                if (pointsX != null)
                    x = pointsX.ElementAt(i).ToDouble(CultureInfo.CurrentUICulture);
                pointList.Add(x, pointsY.ElementAt(i).ToDouble(CultureInfo.CurrentUICulture));
            }

            ReajustAxis(pointList);

            this.zedGraphControl1.AxisChange(); 
            this.zedGraphControl1.Invalidate();
             * */
            AddCurveM(curvetitle, pointsX, pointsY, curvecolor, symboltype);
        }
        public struct Pair<T, U>
        {
            public T a;
            public U b;
        }
        public void AddCurveP<T, U>(string curvetitle, IEnumerable<Pair<T,U>> points,Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None)
            where T : IConvertible
            where U : IConvertible
        {
            PointPairList pointList = new PointPairList();
            LineItem curve = this.Graphics.AddCurve(curvetitle, pointList, curvecolor, (SymbolType)symboltype);

            for (int i = 0; i < points.Count(); i++)
            {
                double x = i;
                if (points != null)
                    x = points.ElementAt(i).a.ToDouble(CultureInfo.CurrentUICulture);
                pointList.Add(x, points.ElementAt(i).b.ToDouble(CultureInfo.CurrentUICulture));
            }

            ReajustAxis(pointList);

            this.zedGraphControl1.AxisChange();
            this.zedGraphControl1.Invalidate();
        }
        public void AddCurveM<T,U>(string curvetitle, IEnumerable<T> pointsX, IEnumerable<U> pointsY, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None) where T : IConvertible where U : IConvertible
        {
            PointPairList pointList = new PointPairList();
            LineItem curve = this.Graphics.AddCurve(curvetitle, pointList, curvecolor, (SymbolType)symboltype);

            for (int i = 0; i < pointsY.Count(); i++)
            {
                double x = i;
                if (pointsX != null)
                    x = pointsX.ElementAt(i).ToDouble(CultureInfo.CurrentUICulture);
                pointList.Add(x, pointsY.ElementAt(i).ToDouble(CultureInfo.CurrentUICulture));
            }

            ReajustAxis(pointList);

            this.zedGraphControl1.AxisChange();
            this.zedGraphControl1.Invalidate();
        }

        private void ReajustAxis(PointPairList last)
        {
            double xmin=0, xmax=0, ymin=0, ymax=0;

            for (int i = 0; i < last.Count; i++)
            {
                if (i == 1 || last[i].X < xmin) xmin = last[i].X;
                if (i == 1 || last[i].X > xmax) xmax = last[i].X;

                if (i == 1 || last[i].Y < ymin) ymin = last[i].Y;
                if (i == 1 || last[i].Y > ymax) ymax = last[i].Y;
            }

            if (_minx == null || xmin < _minx.Value) _minx = xmin;
            if (_maxx == null || xmax > _maxx.Value) _maxx = xmax;

            if (_miny == null || ymin < _miny.Value) _miny = ymin;
            if (_maxy == null || ymax > _maxy.Value) _maxy = ymax;


            Scale xScale = this.zedGraphControl1.GraphPane.XAxis.Scale;

            xScale.Min = _minx.Value;
            xScale.Max = _maxx.Value*1.1;

            Scale yScale = this.zedGraphControl1.GraphPane.YAxis.Scale;
            yScale.Min = _miny.Value;
            yScale.Max = _maxy.Value * 1.1;

            double deltaY = yScale.Max;
            yScale.MinorStep = deltaY / 10;
            yScale.MajorStep = deltaY / 5;

        }
        public void AddCurve<T>(string curvetitle, IEnumerable<T> pointsY, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None) where T : IConvertible
        {
            AddCurve(curvetitle, null, pointsY, curvecolor, symboltype);

        }
        void AldPlotterPoints_BindingContextChanged(object sender, EventArgs e)
        {
            
        }
        private void zedGraphControl1_Load(object sender, EventArgs e)
        {
            
        }

        public void DisableZoom()
        {
            zedGraphControl1.IsEnableZoom = false;
        }

        public void SetMaxX(double p)
        {
            Scale xScale = this.zedGraphControl1.GraphPane.XAxis.Scale;
            xScale.Max = p;
            zedGraphControl1.Invalidate();
        }

        public void AddPoint(int curve, double x, double error)
        {
            PointPairList pointList = this.Graphics.CurveList[curve].Points as PointPairList;
            pointList.Add(x, error);

            Scale xScale = this.zedGraphControl1.GraphPane.XAxis.Scale;
            xScale.Min = 0;
            xScale.Max = pointList.Count;

            Scale yScale = this.zedGraphControl1.GraphPane.YAxis.Scale;
            yScale.Min = 0;
            yScale.Max = pointList.Max(u=>u.Y) * 1.1;

            ReajustAxis(pointList);

            this.zedGraphControl1.AxisChange(); 
            this.zedGraphControl1.Invalidate();
        }

        public void AddLine(string curvetitle,Color curvecolor, float x1, float y1, float x2, float y2)
        {
            PointPairList pointList = new PointPairList();
            LineItem curve = this.Graphics.AddCurve(curvetitle, pointList, curvecolor, SymbolType.None);

            pointList.Add(x1, y1);
            pointList.Add(x2, y2);

            ReajustAxis(pointList);

            this.zedGraphControl1.AxisChange();
            this.zedGraphControl1.Invalidate();
        }
    }
}
