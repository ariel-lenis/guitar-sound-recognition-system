using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZedGraph;
using D = System.Drawing;

namespace TsExtraControls
{
    /// <summary>
    /// Interaction logic for UsrAldPlotterPoints.xaml
    /// </summary>
    public partial class UsrAldPlotterPoints : UserControl
    {
        public class MarkData
        {
            public int idx;
            public float x;
            public float y;
        }

        public delegate void DOnMarkSetted(object who,int idx,float x,float y);

        public event DOnMarkSetted OnMarkedSetted;

        ZedGraphControl zedGraphControl1;
        double? _minx, _maxx, _miny, _maxy;


        MarkData lastmark;

        public MarkData CurrentMark { get { return this.lastmark; } }

        public UsrAldPlotterPoints()
        {
            InitializeComponent();

            zedGraphControl1 = new ZedGraphControl();

            this.zedGraphControl1.IsShowCursorValues = true;

            theWindowsFormHost.Child = zedGraphControl1;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                theWindowsFormHost.Visibility = System.Windows.Visibility.Hidden;
                //zedGraphControl1.Visible = false;
                //zedGraphControl1.Enabled = false;
            }

            this.zedGraphControl1.MouseClick += zedGraphControl1_MouseClick;
            this.zedGraphControl1.MouseDoubleClick += zedGraphControl1_MouseDoubleClick;

            this.Graphics = this.zedGraphControl1.GraphPane;
            Graphics.AddCurve("X,Y Axis", new double[] { 0, 0 }, new double[] { 10000, -10000 }, D.Color.Black); // Se controla manualmente que el rango del eje X está continuamente
            this.Graphics.YAxis.Scale.Format = "F4";
        }

        void zedGraphControl1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (primary == null) return;

            // Create an instance of Graph Pane
            GraphPane myPane = zedGraphControl1.GraphPane;



            // x & y variables to store the axis values
            double xVal;
            double yVal;

            // Clear the previous values if any
            userClickrList.Clear();

            myPane.Legend.IsVisible = false;

            // Use the current mouse locations to get the corresponding 
            // X & Y CO-Ordinates
            myPane.ReverseTransform(e.Location, out xVal, out yVal);

            CurveItem curveitem = null;
            int idx = 0;
            myPane.FindNearestPoint(e.Location,primary, out curveitem, out idx);

            if (curveitem == null) return;

            PointPair pair = curveitem.Points[idx];

            xVal = pair.X;
            yVal = pair.Y;

            this.lastmark = new MarkData() { idx = idx, x = (float)xVal, y = (float)yVal };

            if (this.OnMarkedSetted != null)
                this.OnMarkedSetted(this, idx, (float)xVal, (float)yVal);

            // Create a list using the above x & y values
            userClickrList.Add(xVal, myPane.YAxis.Scale.Max);
            userClickrList.Add(xVal, 0*myPane.YAxis.Scale.Min);

            // Add a curve
            userClickCurve = myPane.AddCurve(" ", userClickrList, D.Color.Black, SymbolType.None);
            userClickCurve.Line.Width = 2;
            userClickCurve.Line.Style = System.Drawing.Drawing2D.DashStyle.DashDot;
            userClickCurve.Line.StepType = StepType.ForwardStep;
            userClickCurve.Line.DashOn = 1f;
            userClickCurve.Line.DashOff = 1f;

            zedGraphControl1.Refresh();
        }

        PointPairList userClickrList = new PointPairList();
        LineItem userClickCurve = new LineItem("userClickCurve");


        void zedGraphControl1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if (Keyboard.GetKeyStates(Key.LeftCtrl) != KeyStates.Down) return;

        }

        



        GraphPane Graphics;


        public GraphPane TheGraphics 
        {
            get { return Graphics; }
        }


        public void HideAllInfo()
        {
            this.Graphics.YAxis.IsVisible = false;
            this.Graphics.Title.IsVisible = false;
            this.Graphics.Legend.IsVisible = false;
            this.Graphics.XAxis.IsVisible = false;
        }

        public string Title
        {
            get {
                return Graphics.Title.Text;
            }
            set {
                Graphics.Title.Text = value;
            }
        }


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
        public void ClearCurves()
        {
            var aux = Graphics.CurveList.First();
            Graphics.CurveList.Clear();
            Graphics.CurveList.Add(aux);
            zedGraphControl1.Invalidate();
            _minx=_miny=_maxx=_maxy = null;
        }

        LineItem primary = null;

        public void AddCurve(string curvetitle, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None,bool setprimary=false)
        {
            AddCurve(curvetitle, new float[] { }, new float[] { }, curvecolor, symboltype,setprimary);
        }
        public LineItem AddCurve<T>(string curvetitle, IEnumerable<T> pointsX, IEnumerable<T> pointsY, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None,bool setprimary=false) where T : IConvertible
        {
            PointPairList pointList = new PointPairList();
            LineItem curve = this.Graphics.AddCurve(curvetitle, pointList, D.Color.FromArgb(curvecolor.R,curvecolor.G,curvecolor.B), (SymbolType)symboltype);
                        
            
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

            if (setprimary)
                this.primary = curve;

            return curve;
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
        public void AddCurve<T>(string curvetitle, IEnumerable<T> pointsY, Color curvecolor, Extra.AdaptedSymbolType symboltype = Extra.AdaptedSymbolType.None,bool setprimary = false) where T : IConvertible
        {
            AddCurve(curvetitle, null, pointsY, curvecolor, symboltype,setprimary);

        }
        void AldPlotterPoints_BindingContextChanged(object sender, EventArgs e)
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
    }
}
