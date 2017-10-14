using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using AldWavDisplayTools;
using System.Runtime.InteropServices;
using TsExtraControls;
using TsFilesTools;
using AldSpecialAlgorithms;
using TsFFTFramework;

namespace WavAnalizerContainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        ResizingWatchDog resizingwd;
        System.Windows.Forms.PropertyGrid propertyGrid;
        FileFolderDialog fileBrowser;
        AldPlotterPoints plotter;
        List<TsTrainingSet> wavesInfos;
        TsTrainingSet currentTrainingSet;

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;

            if (!TsFFTLink.LoadModule(TsFFTLink.Modules.CudaFFT))
            {
                MessageBox.Show("Cannot load the FFT libraries.");
                Application.Current.Shutdown(1);
            }
        }

        void resizingwd_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.aldCompleteDisplayer1.Resize();
            }));
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            fileBrowser = new FileFolderDialog();
            propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.windowsFormsContainer.Child = propertyGrid;

            resizingwd = new ResizingWatchDog(this);
            resizingwd.SizeChanged += resizingwd_SizeChanged;

            StartPlot();
        }

        private void StartPlot()
        {
            plotter = new AldPlotterPoints();
            hostPlot.Child = plotter;
            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Hz", Mayormarks = 10, MinorMarks = 10 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "db", Mayormarks = 1, MinorMarks = 0.1};

            plotter.AxisX = axisX;
            plotter.AxisY = axisY;
            plotter.Title = "Fast Fourier Transform";
            plotter.DisableZoom();
        }

        void OpenFile(TsTrainingSet who)
        {
            if (currentTrainingSet != null) currentTrainingSet.UnloadFilesInformation();
            currentTrainingSet = who;

            currentTrainingSet.LoadFilesInformation();
            sliderAdjust.Value = 0;
            propertyGrid.SelectedObject = currentTrainingSet.WaveDescriptor;

            if (currentTrainingSet.WaveDescriptor.AudioFormat != 1)
            {
                MessageBox.Show("No se permite compresion :p");
                return;
            }

            float[] samples=currentTrainingSet.Wave;
            this.aldCompleteDisplayer1.ClearExisting();
            this.aldCompleteDisplayer1.AddSamples(samples,0,Colors.Gray,currentTrainingSet.WaveDescriptor.SampleRate, currentTrainingSet.Markers);
            //this.aldCompleteDisplayer1.LoadSamples(samples, currentTrainingSet.WaveDescriptor.SampleRate, currentTrainingSet.Markers);
            aldCompleteDisplayer1.Resize();

            ReloadGrid();
        }
       
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (this.fileBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            this.gridFiles.ItemsSource = null;

            wavesInfos = new List<TsTrainingSet >();
            foreach (var i in Directory.EnumerateFiles(this.fileBrowser.SelectedPath, "*.wav"))
                wavesInfos.Add(new TsTrainingSet(i));
            this.gridFiles.ItemsSource = wavesInfos;
        }
        private void btnFFT_Click(object sender, RoutedEventArgs e)
        {
            int start,n;
            float density;
            aldCompleteDisplayer1.GetSelectionRange(out start, out n,out density);
            if (n == 0) return;

            int samplerate = currentTrainingSet.WaveDescriptor.SampleRate;

            var res = FrequenciesPeaks.CalculatePeaks(currentTrainingSet.Wave, start, start+n, samplerate, 5, 0.05f,82);

            plotter.ClearCurves();


            plotter.AddCurve<float>("FFT", res.allhz, res.allfft, System.Drawing.Color.Green);

            plotter.AddCurve<float>("Pks", res.hzHarm, res.pksHarm, System.Drawing.Color.Blue, Extra.AdaptedSymbolType.Triangle);

            plotter.AddCurve<float>("Harm", res.allhz, res.allharm, System.Drawing.Color.Red);


            sliderAxisX.Minimum = 100;
            sliderAxisX.Maximum = samplerate / 2;

            plotter.SetMaxX(samplerate / 2 / 4);
            sliderAxisX.Value = samplerate / 2 / 4;
        }


        private void sliderAdjust_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentTrainingSet.Markers != null)
            {
                double maximun = 1;
                currentTrainingSet.Adjust = TimeSpan.FromSeconds(maximun * e.NewValue);
                lblAdjust.Content = (maximun * e.NewValue * 1000) + "ms.";
                aldCompleteDisplayer1.RenderAll();


            }
        }

        private void ReloadGrid()
        {
            int pos = gridFiles.SelectedIndex;
            gridFiles.ItemsSource = null;
            gridFiles.ItemsSource = wavesInfos;
            gridFiles.SelectedIndex = pos;
        }

        private void sliderAxisX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            plotter.SetMaxX(e.NewValue);            
        }

        private void btnSaveMarkers_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainingSet == null || currentTrainingSet.Markers==null) { MessageBox.Show("I have no markers..."); return; }
            currentTrainingSet.SaveAdjust();
            MessageBox.Show("Markers Saved xD");
        }

        private void gridFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridFiles.SelectedIndex < 0) return;
            OpenFile(gridFiles.SelectedValue as TsTrainingSet);
        }

        private void sliderAdjust_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void sliderAdjust_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void sliderAdjust_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReloadGrid();
        }

        private void btnAdjustTimes_Click(object sender, RoutedEventArgs e)
        {
            TsToolsMarkers.Adjust(currentTrainingSet);
            aldCompleteDisplayer1.RenderAll();
            ReloadGrid();
            //var time = TsToolsMarkers.GetFirstNoteTime(currentTrainingSet);
            //MessageBox.Show(time.ToString());
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in wavesInfos)
                if (i.Adjust.TotalSeconds != 0) i.SaveAdjust();                
        }

        private void btnAdjustAll_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Task.Run(new Action(delegate {
                for (int i = 0; i < wavesInfos.Count; i++)
                {
                    if (wavesInfos[i] != currentTrainingSet)
                        wavesInfos[i].LoadFilesInformation();

                    TsToolsMarkers.Adjust(wavesInfos[i]);

                    wavesInfos[i].SaveAdjust(); 

                    if (wavesInfos[i] != currentTrainingSet)
                        wavesInfos[i].UnloadFilesInformation();
                    this.Dispatcher.Invoke(new Action(delegate { 
                        progressChange.Value = (i + 1.0) / wavesInfos.Count;
                    }));
                }

                this.Dispatcher.Invoke(new Action(delegate { 
                    ReloadGrid();
                    aldCompleteDisplayer1.RenderAll();
                    this.IsEnabled = true;
                }));
            }));
        }

        private void btnOpen1_Click(object sender, RoutedEventArgs e)
        {
            if(currentTrainingSet==null) return;

            Process.Start("explorer.exe","/select, \""+currentTrainingSet.WaveFile+"\"");// ;            
        }

        private void gridFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
