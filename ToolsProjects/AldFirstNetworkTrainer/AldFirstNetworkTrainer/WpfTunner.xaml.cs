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
using System.Windows.Shapes;
using TsFilesTools;
using System.Threading.Tasks;
using System.Threading;
using TsFFTFramework;
using System.Linq;
using AldSpecialAlgorithms;
using System.Diagnostics;

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for WpfTunner.xaml
	/// </summary>
	public partial class WpfTunner : Window
	{
        TsWavRecorder recorder;
        Thread thread;

        UsrStringTunning[] tunners;
        int[] midis = new int[] {64,59,55,50,45,40};
        float[] frequencies = new float[6];
        string[] names = new string[6];
        int[] octaves = new int[6];

        bool endthread;

        bool threadended;

		public WpfTunner()
		{
			this.InitializeComponent();

            tunners = new UsrStringTunning[] { tunnerstring1, tunnerstring2, tunnerstring3, tunnerstring4, tunnerstring5,tunnerstring6 };

            for (int i = 0; i < tunners.Length; i++)
            {
                frequencies[i] = (float)TsMIDITools.FrequencyFor(midis[i]);
                names[i] = TsMIDITools.NoteFor(midis[i]);
                octaves[i] = TsMIDITools.OctaveFor(midis[i]);
                tunners[i].Information = names[i]+":"+frequencies[i].ToString("000.000")+"hz";
            }

            recorder = new TsWavRecorder(64);

            thread = new Thread(Magic);
            thread.Start();

            //Task.Run(new Action(Magic));

            spectrumplot.HideAllInfo();

		}
        void Magic()
        {
            recorder.Start();

            while (true)
            {                
                short[] data = recorder.GetRecord();
                float[] aux = new float[data.Length];

                for (int i = 0; i < data.Length; i++)
                    aux[i] = (float)data[i] / short.MaxValue;
                                
                
                if (aux.Length>0)
                {
                    float[] fft = AldCudaAlgorithms.XFFTReal(aux, AldCudaAlgorithms.Direcion.Forward).Select(x => 2 * x / aux.Length).Take(aux.Length / 8).ToArray();

                    var ceps = Cepstrum.CalculateCepstrum2(aux, 0, aux.Length - 1, 44100);
                    var cepstofreq = Cepstrum.CepstrumToFrequency(ceps[1], 44100, 500);

                    //fft.Multiply(cepstofreq);

                    int max = cepstofreq.MaxPosition();

                    float fqc = (float)max * 44100 / aux.Length;

                    

                    //float fqc = (float)fft.MaxPosition() * 44100 / aux.Length;                        


                    ProcessFrequency(fqc);

                    fft.Log10Normalization();

                    try
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {

                            spectrumplot.ClearCurves();
                            spectrumplot.AddCurve("FFT", fft, Colors.SkyBlue);

                        }));
                    }
                    catch
                    {
                        Debug.WriteLine("Problem");
                    }

                }

                
                Thread.Sleep(50);

                if (endthread)
                {
                    Debug.WriteLine("Ending");
                    threadended = true;
                    break; 
                }
            }
        }

        private void ProcessFrequency(float fqc)
        {
            int minpos=0;
            float mindis=float.MaxValue;

            

            for (int i = 0; i < frequencies.Length; i++)
            {
                float mx = TsMIDITools.DistanceBetweenNotes(frequencies[i], fqc);
                if (Math.Abs(mx) < Math.Abs(mindis))
                {
                    minpos = i;
                    mindis = mx;
                }
            }
            this.Dispatcher.Invoke(new Action(delegate
            {
                for (int i = 0; i < frequencies.Length; i++)
                {
                    if (i != minpos) tunners[i].Activate = false;
                    else tunners[i].Activate = true;
                }
                float cents = TsMIDITools.FrequencyToCents(frequencies[minpos], fqc);
                tunners[minpos].Frequency = fqc;
                tunners[minpos].Distance = mindis;
                tunners[minpos].Cents = cents;

            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            endthread = true;

            Task.Run(new Action(delegate { 
                while (!threadended) 
                    Thread.Sleep(10);
                Debug.WriteLine("Ended");

                recorder.Stop();
                recorder.Dispose();

            }));

            //e.Cancel = true;
        }
	}
}