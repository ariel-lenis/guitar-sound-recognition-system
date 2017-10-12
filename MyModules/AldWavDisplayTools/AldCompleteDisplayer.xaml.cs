using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TsFilesTools;
using System.Linq;
using TsExtraControls;

namespace AldWavDisplayTools
{
	/// <summary>
	/// Interaction logic for AldCompleteDisplayer.xaml
	/// </summary>
	public partial class AldCompleteDisplayer : UserControl
	{
        List<AldWaveDisplayer> waves;
        List<TsWaveData<float>> data;

        public delegate void DDisplayChanged(object who, float start, float length);
        public delegate void DBetweenMarkers(object who, int posa, int posb);

        public delegate void DPlayRequired(object who,int start, int n);
        
        

        public event DDisplayChanged DisplayChanged;
        public event DBetweenMarkers BetweenMarkers;

        public event EventHandler OnWaveSelected;

        public event DPlayRequired PlayRequired;


        
        public List<AldWaveDisplayer> TheWaves { get { return waves; } }
        public List<TsWaveData<float>> TheDataList { get { return data; } }

		public AldCompleteDisplayer()
		{
			this.InitializeComponent();

            this.Focusable = true;
            this.rectSeleccion.Focusable = true;
            this.aldScrollWaves.ScrollChanged += aldScrollWaves_ScrollChanged;

            //Keyboard.AddKeyDownHandler(this, new KeyEventHandler(AldCompleteDisplayer_KeyDown));

            this.KeyDown+=AldCompleteDisplayer_KeyDown;
            
            ClearExisting();
		}


        void AldCompleteDisplayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this.IsMouseOver) return;
            if (e.Key == Key.Space)
            {
                int start, length;
                float density;

                GetSelectionRange(out start, out length, out density);

                if (this.PlayRequired != null)
                    this.PlayRequired(this, start, length);
            }
        }

        public void ClearExisting()
        {
            if (waves != null)
            {
                foreach (var i in waves)
                {
                    i.MouseWheel -= wavesDisplayer_MouseWheel;
                    this.panelWaves.Children.Remove(i);
                    i.Dispose();
                }
                waves.Clear();
                waves = null;                
            }
            if (data != null)
            {
                foreach (var i in data)
                    i.Dispose();
                data = null;
            }

            this.aldScrollWaves.Clear();
            data = new List<TsWaveData<float>>();
            waves = new List<AldWaveDisplayer>();
            GC.Collect();
        }
        public void AddSamples(float[] sample, int pos = 0, Color? wavecolor = null, int samplerate = 44100, List<TimeMark> markers = null)            
        {
            if (wavecolor == null) wavecolor = Colors.SlateGray;

            data.Add(new TsWaveData<float>(markers,samplerate, sample, 256));
            if (pos == waves.Count)
            {
                waves.Add(new AldWaveDisplayer());
                panelWaves.Children.Add(waves.Last());
            }
            else if (pos > waves.Count) throw new Exception("Error with the position.");

            waves[pos].Width = panelWaves.CurrentWidth();

            for (int i = 0; i < waves.Count;i++ )
                waves[i].Height = panelWaves.CurrentHeight() / waves.Count;

            
            waves[pos].MouseWheel+=wavesDisplayer_MouseWheel;
            waves[pos].DrawMarkers = true;

            waves[pos].Add(data.Last(),wavecolor.Value,Colors.Red);

            if(data.Count==1)
                this.aldScrollWaves.LoadSamples(data);            
        }

        void aldScrollWaves_ScrollChanged(AldScrollDisplayer who)
        {
            foreach(var i in waves)
                i.SetParams((float)who.CurrentValuePercent,(float)who.CurrentSizePercent);
            if (DisplayChanged != null)
                DisplayChanged(this, (float)who.CurrentValuePercent, (float)who.CurrentSizePercent);
        }

        void wavesDisplayer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double prop = Math.Abs((float)e.Delta / System.Windows.Input.Mouse.MouseWheelDeltaForOneLine);

            double zoom = 1.1 * prop;

            if (e.Delta > 0)
                zoom = 1 / zoom;

            //Console.WriteLine("Delta:" + e.Delta + "  " + System.Windows.Input.Mouse.MouseWheelDeltaForOneLine);

            this.aldScrollWaves.CurrentSizePercent *= zoom;


            

            foreach(var i in waves)
                i.SetParams((float)aldScrollWaves.CurrentValuePercent, (float)aldScrollWaves.CurrentSizePercent);

            //Console.WriteLine("Density:"+waves[0].AldAdapter.AldTransform.Density());

            if (DisplayChanged != null)
                DisplayChanged(this, (float)aldScrollWaves.CurrentValuePercent, (float)aldScrollWaves.CurrentSizePercent);

        }

        public void Resize()
        {
            if (waves != null && waves.Count>0 && waves[0] != null && waves[0].TheData != null)
            {
                this.aldScrollWaves.CurrentSizePercent = waves[0].PointsPercentContained();

                var height = gridPanel.CurrentHeight() / waves.Count;

                //Console.WriteLine(" LH:" + gridPanel.CurrentHeight() + " " + panelWaves.VerticalAlignment);

                for (int i = 0; i < waves.Count; i++)
                {
                    waves[i].Height = height;
                    waves[i].Width = panelWaves.CurrentWidth();
                }

                foreach (var i in waves)
                    i.Resize();

                this.aldScrollWaves.Resize();
            }
        }
        public TimeSpan GetVisibleRange(out int start, out int length, out float density)
        {
            waves[0].AldAdapter.AldTransform.CalculateMargins(out start, out length);
            density = waves[0].AldAdapter.AldTransform.Density();
            return TimeSpan.FromSeconds((float)length / waves[0].TheData[0].samplerate);     
        }
        public TimeSpan GetSelectionRange(out int start, out int length,out float density)
        {
            float pstart = (float)(rectSeleccion.Margin.Left / this.gridPanel.CurrentWidth());
            float plength = (float)(rectSeleccion.Width / this.gridPanel.CurrentWidth());
            waves[0].AldAdapter.AldTransform.GetSelectionRange(pstart, plength,out start,out length);
            density = waves[0].AldAdapter.AldTransform.Density();
            return TimeSpan.FromSeconds((float)length / waves[0].TheData[0].samplerate);            
        }
        Point startsel;
        private void gridPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.gridPanel.CaptureMouse();
                startsel = Mouse.GetPosition(gridPanel);
                var margin = rectSeleccion.Margin;
                margin.Left = startsel.X;
                rectSeleccion.Margin = margin;
                rectSeleccion.Width = 0;
            }
            else if(e.RightButton== MouseButtonState.Pressed)
                SelectMarker((float)e.GetPosition(gridPanel).X);            
        }

        private void SelectMarker(float x)
        {
            float kx = (float)(x / this.gridPanel.CurrentWidth());
            int position = waves[0].AldAdapter.AldTransform.PositionFromPercent(kx);
            int a, b;
            waves[0].TheData[0].Marks.MarkersAround(position, out a, out b);
            if (a == -1 || b == -1) return;

            TimeMark ma = waves[0].TheData[0].Marks[a];
            TimeMark mb = waves[0].TheData[0].Marks[b];

            float pa = waves[0].AldAdapter.AldTransform.PercentFromPosition(ma.MarkPosition);
            float pb = waves[0].AldAdapter.AldTransform.PercentFromPosition(mb.MarkPosition);

            var margin = rectSeleccion.Margin;
            margin.Left = pa * gridPanel.CurrentWidth();
            rectSeleccion.Margin = margin;

            rectSeleccion.Width = (pb-pa)* gridPanel.CurrentWidth();
            if (BetweenMarkers != null)
                BetweenMarkers(this, a, b);
        }
        private void gridPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Point endsel;
            if (this.gridPanel.IsMouseCaptured)
            {
                endsel = Mouse.GetPosition(gridPanel);
                if (endsel.X > gridPanel.CurrentWidth()) endsel.X = gridPanel.CurrentWidth();

                if (endsel.X < 0) endsel.X = 0;

                double delta = endsel.X - startsel.X;

                if (delta < 0)
                {
                    var margin = rectSeleccion.Margin;
                    margin.Left = endsel.X;
                    rectSeleccion.Margin = margin;

                    rectSeleccion.Width = -delta;
                }
                else
                {
                    var margin = rectSeleccion.Margin;
                    margin.Left = startsel.X;
                    rectSeleccion.Margin = margin;
                    rectSeleccion.Width = delta;
                }
            }
        }

        private void gridPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.gridPanel.IsMouseCaptured)
            {
                this.gridPanel.ReleaseMouseCapture();
                if (OnWaveSelected != null && data.Count>0)
                    OnWaveSelected(this,EventArgs.Empty);
                this.Focus();
            }
        }

        public void RenderAll()
        {
            if(waves!=null)
                foreach (var i in waves)
                    i.AldAdapter.Render();
        }

        public void DisplayRange(float start, float length)
        {
            this.aldScrollWaves.CurrentValuePercent = start;
            this.aldScrollWaves.CurrentSizePercent = length;

            foreach (var i in waves)
                i.SetParams(start, length);

            if (DisplayChanged != null)
                DisplayChanged(this, start, length);

        }
    }
}