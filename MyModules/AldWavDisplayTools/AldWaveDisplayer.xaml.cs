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
using System.Linq;
using TsFilesTools;
using TsExtraControls;

namespace AldWavDisplayTools
{
	/// <summary>
	/// Interaction logic for AldWaveDisplayer.xaml
	/// </summary>
	public partial class AldWaveDisplayer : UserControl,IDisposable
	{
        List<TsWaveData<float>> Data;
        AldGdiWavesMotor<float> adapter;


        public AldGdiWavesMotor<float> AldAdapter { get { return adapter; } }

        public List<TsWaveData<float>> TheData { get { return Data; } }


        public bool DrawMarkers { get { return adapter.DrawMarks; } set { adapter.DrawMarks = value; } }

		public AldWaveDisplayer()
		{
			this.InitializeComponent();
            this.adapter = new AldGdiWavesMotor<float>(CanvasWaves);
            Data = new List<TsWaveData<float>>();
            //AldBitmapSourceCreator bmp = new AldBitmapSourceCreator(100,100);
		}
        
        public void Add(TsWaveData<float> Data, Color waveColor,Color markColor)
        {            
            adapter.RecreateImage((int)LayoutRoot.CurrentWidth(), (int)LayoutRoot.CurrentHeight());
            this.Data.Add(Data);

            var penWave = new System.Drawing.Pen(MultiverseColor(waveColor));
            var penMark = new System.Drawing.Pen(MultiverseColor(markColor));

            adapter.AddData(Data,penWave ,penMark);
            adapter.AdjustMatrix();
            adapter.Render();
        }
        public System.Drawing.Color MultiverseColor(Color who)
        {
            return System.Drawing.Color.FromArgb(who.A, who.R, who.G, who.B);
        }
        public float PointsPercentContained()
        {
            return adapter.PointsPercentContained();
        }
        public void SetParams(float percentX, float percentSize)
        {
            adapter.SetParams(percentX, percentSize);

        }

        public void Resize()
        {
            Console.WriteLine("W:"+this.LayoutRoot.Width+" H:"+this.Height);
            if (this.Data != null && this.CurrentWidth() != 0 && this.CurrentHeight() != 0)
            {
                Console.WriteLine(this.CurrentHeight() + " " + LayoutRoot.CurrentHeight());
                adapter.RecreateImage((int)this.CurrentWidth(), (int)this.CurrentHeight());
                adapter.AdjustMatrix();
                adapter.Render();
            }
        }

        public void Dispose()
        {
            this.Data = null;
            this.adapter.Dispose();
        }
 
    }
}