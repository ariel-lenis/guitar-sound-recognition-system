using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using TsFFTFramework;
using TsFilesTools;
using AldSpecialAlgorithms;
using System.IO;

namespace TsRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TsWavRecorder recorder;
        Thread thread;
        bool close;
        short[] rawdata;

        Stopwatch sw;

        string folder;

        string savedpath;

        bool nocapture;
        bool threadended;
        bool shutdown;

        public string SavedPath { get { return savedpath; } }

		public MainWindow()
		{
			InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;

            //this.folder = Directory;
            
            this.plotter.HideAllInfo();
            recorder = new TsWavRecorder();
            gridRecording.Visibility = System.Windows.Visibility.Hidden;
            thread = new Thread(Run);
            thread.Start();

            this.wavDisplayer.DisplayChanged += wavDisplayer_DisplayChanged;

            sw = new Stopwatch();
		}

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            bool res = TsFFTFramework.TsFFTLink.LoadModule(TsFFTLink.Modules.CudaFFT);

            //return;

            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length != 2)
            {
                MessageBox.Show("Wrong number of parameters.");
                Application.Current.Shutdown(1);
                return;
            }
            string path = arguments[1].Replace("\"", "");
            //MessageBox.Show(path);

            if (File.Exists(path))
            {
                MessageBox.Show("File already exists!!!");
                Application.Current.Shutdown(1);
                return;
            }

            

            this.savedpath = path;
        }
        
        void wavDisplayer_DisplayChanged(object who, float start, float length)
        {
            //spectrogram1.Display(start, length);
        }

        private void btnGrabar_Click(object sender, RoutedEventArgs e)
        {
            nocapture = false;
            gridRecording.Visibility = System.Windows.Visibility.Visible;
            recorder.Start();
            sw.Reset();
            sw.Start();
        }

        void Run()
        {
            while (!close)
            {
                if (recorder.Recording && !nocapture)
                {

                    short[] data = recorder.GetSubRecord(50);

                    float[] aux = new float[data.Length];

                    for (int i = 0; i < data.Length; i++)
                        aux[i] = (float)data[i] / short.MaxValue;

                    float[] fft = AldCudaAlgorithms.XFFTReal(aux, AldCudaAlgorithms.Direcion.Forward).Select(x => 2 * x / aux.Length).Take(aux.Length / 8).ToArray();

                    fft.Log10Normalization();

                    try
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {                            
                            plotter.ClearCurves();
                            lblTimeRecording.Content = sw.Elapsed;                            
                            plotter.AddCurve("FFT", fft, Colors.SkyBlue);

                        }));
                    }
                    catch
                    {
                        Debug.WriteLine("Problem");
                    }
                }
                Thread.Sleep(50);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (recorder.Recording)
            {
                MessageBox.Show("Is recording...");
                e.Cancel = true;
                return;
            }
            if (!shutdown)
            {
                Application.Current.Shutdown(1);
                return;
            }
            recorder.Dispose();
            close = true;
        }

        void ShowData(short[] rawdata)
        {
            //if (rawdata.Length == 0) return;

            float[] data = rawdata.Select(x => (float)x / short.MaxValue).ToArray();
            //data.ChebNormalization(0.999f);
            
            /*
            float[] amps;
            float mean, std;

            float[][] spec = null;
            
            this.Owner.Dispatcher.Invoke(new Action(delegate
            {
                spec = AldCudaAlgorithms.AldGenerateSpectogram2(data, 1024, data.Length / 1024, false, out amps, out mean, out std);
            }));
            */
            /* 
            spectrogram1.GenerateSpectogram(spec);
            spectrogram1.Resize();
            spectrogram1.Display(0, 1);
            */
            wavDisplayer.ClearExisting();
            wavDisplayer.AddSamples(data, 0, Colors.Red, 44100, null);
            wavDisplayer.Resize();
            wavDisplayer.RenderAll();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            nocapture = true;

            recorder.Stop();

            Thread.Sleep(500);

            gridRecording.Visibility = System.Windows.Visibility.Hidden;

            rawdata = recorder.GetRecord();

            MessageBox.Show("Saved...");

            ShowData(rawdata);
            sw.Stop();

            this.lblTimeRecorded.Content = sw.Elapsed;
        }

        private void btnRecortar_Click(object sender, RoutedEventArgs e)
        {
            if (rawdata==null || rawdata.Length == 0) return;
            int start,length;
            float density;
            wavDisplayer.GetSelectionRange(out start, out length, out density);

            if (length > rawdata.Length - 512)
            {
                MessageBox.Show("Is to Much short.");
                return;
            }

            short[] cuttedrawdata = new short[rawdata.Length - length];
            int cont = 0;
            for (int i = 0; i < rawdata.Length; i++)            
                if (!(i >= start && i <= start + length))
                    cuttedrawdata[cont++] = rawdata[i];

            rawdata = cuttedrawdata;
            ShowData(rawdata);
        }

        private void btnUsar_Click(object sender, RoutedEventArgs e)
        {
            WavDescriptor wav = new WavDescriptor();
            wav.Samples = new List<short[]>() { rawdata };

            //string name = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;

            //this.savedpath = folder + "\\" + name + ".wav";

            wav.SaveData(savedpath);

            //recorder.Dispose();

            //this.DialogResult = true;

            shutdown = true;
            this.Close();
        }
    }
}
