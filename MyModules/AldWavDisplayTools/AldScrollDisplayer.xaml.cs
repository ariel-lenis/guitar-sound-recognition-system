using TsFilesTools;
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
using TsExtraControls;

namespace AldWavDisplayTools
{
	/// <summary>
	/// Interaction logic for AldScrollDisplayer.xaml
	/// </summary>
	public partial class AldScrollDisplayer : UserControl
	{
        double inix;

        double currrentPercent;
        double currentSizePercent;

        public double CurrentValuePercent
        {
            get { return currrentPercent; }
            set {
                if (value < 0) value = 0;
                else if (value + currentSizePercent > 1) value = 1 - currentSizePercent;
                this.currrentPercent = value;
                AdaptScroll();
            }
        }
        public double CurrentSizePercent 
        {
            get { return currentSizePercent; }
            set {
                if (value<0) value=0;
                else if (value + currrentPercent > 1) value = 1 - currrentPercent;
                this.currentSizePercent = value;
                AdaptScroll();
            }
        }

		public AldScrollDisplayer()
		{
			this.InitializeComponent();
            currrentPercent = 0;
            currentSizePercent = 1;
		}

        public delegate void DScrollChanged(AldScrollDisplayer who);
        public event DScrollChanged ScrollChanged;
        List<AldWaveDisplayer> waves;

        public void LoadSamples(List<TsWaveData<float>> data)
        {
            waves =  new List<AldWaveDisplayer>();
            foreach (var i in data)
            {
                var currentwave = new AldWaveDisplayer();
                panelWaves.Children.Add(currentwave);
                waves.Add(currentwave);
                currentwave.Width = this.panelWaves.CurrentWidth();
                currentwave.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                currentwave.Height = this.panelWaves.CurrentHeight()/data.Count;
                currentwave.Add(i, Color.FromRgb(0, 0, 255),Colors.Black);
            }
            //tinyWaveA.DisplayValues(data[0]);
            //tinyWaveB.DisplayValues(data[1]);
        }

        public void AdaptScroll()
        {            
            double w = this.CurrentWidth() * CurrentSizePercent;
            double x = this.CurrentWidth() * CurrentValuePercent;

            if(w>=0 && w<5) w=5;
            this.scroll.Width = w;
            this.scroll.SetX(x);
            scroll.Height = canvasx.CurrentHeight();
        }

        private void scroll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            inix = e.GetPosition(this).X;
            scroll.CaptureMouse();
        }

        private void scroll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (scroll.IsMouseCaptured)
                scroll.ReleaseMouseCapture();
        }

        private void scroll_MouseMove(object sender, MouseEventArgs e)
        {
            if (scroll.IsMouseCaptured)
            {
                double newx = e.GetPosition(this).X;
                double delta = newx - inix;

                CurrentValuePercent += delta/canvasx.CurrentWidth();

                if (ScrollChanged != null) ScrollChanged(this);
                inix = newx;
            }
        }

        internal void Resize()
        {
            foreach (var i in waves)
            {
                i.Width = this.panelWaves.CurrentWidth(); ;
                i.Resize();
            }
        }

        internal void Clear()
        {
            if (waves != null)
            {
                foreach (var i in waves)
                {
                    this.panelWaves.Children.Remove(i);
                    i.Dispose();
                }
                waves.Clear();
                waves = null;
            }
        }


    }
}