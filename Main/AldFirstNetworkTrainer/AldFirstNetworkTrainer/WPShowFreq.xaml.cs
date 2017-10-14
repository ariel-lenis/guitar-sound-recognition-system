using AaBackPropagationFast;
using TsFFTFramework;
using TsExtraControls;
using TsFilesTools;
using AldSpecialAlgorithms;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AldFirstNetworkTrainer
{
    public partial class WPShowFreq : Window
    {
        private float[] p;
        private int posa;
        private int posb;
        private int samplerate;
        List<TimeMark> theMarks;
        List<TimeMark> allMarks;

        List<int> themidinotes;
        List<float> thefrequencies;


        AldMidiPlayer midiplayer;

        //TsAdminAllFrequencies thetrainer;
        TsFirsStepSolution thesolution;

        public WPShowFreq(float[] p, int posa, int posb, int samplerate, TsFirsStepSolution thesolution,AldMidiPlayer midiplayer)
        {
            this.midiplayer = midiplayer;
            this.thesolution = thesolution;

            this.allMarks = thesolution.CurrentTrainingSet.Markers;
            this.samplerate = samplerate;
            this.p = p;
            this.posa = posa;
            this.posb = posb;

            InitializeComponent();
            this.SourceInitialized += WPShowFreq_SourceInitialized;

            ContextMenu menu = new System.Windows.Controls.ContextMenu();

            MenuItem itemAddCurrent = new MenuItem() { Header = "Add Current" };
            itemAddCurrent.Click += itemAddCurrent_Click;
            menu.Items.Add(itemAddCurrent);

            MenuItem itemSetCurrent = new MenuItem() { Header = "Set Current" };
            itemSetCurrent.Click += itemSetCurrent_Click;
            menu.Items.Add(itemSetCurrent);

            MenuItem itemRemoveThis = new MenuItem() { Header = "Remove This" };
            itemRemoveThis.Click += itemRemoveThis_Click;
            itemRemoveThis.IsEnabled = false;
            menu.Items.Add(itemRemoveThis);

            gridMarkers.ContextMenu = menu;

            this.plotter.OnMarkedSetted += plotter_OnMarkedSetted;



        }

        void itemRemoveThis_Click(object sender, RoutedEventArgs e)
        {
            if (this.gridMarkers.SelectedIndex < 0)
            {
                Console.Beep(200, 500);
                return;
            }
            TimeMark mark = this.gridMarkers.SelectedValue as TimeMark;
            this.allMarks.Remove(mark);
            ReloadMarkers();
        }

        void itemSetCurrent_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentValue();
        }

        private void SetCurrentValue()
        {
            if (this.gridMarkers.SelectedIndex < 0 || plotter.CurrentMark == null)
            {
                Console.Beep(200, 500);
                return;
            }
            TimeMark mark = this.gridMarkers.SelectedValue as TimeMark;
            mark.Frequency = plotter.CurrentMark.x;
            mark.RelatedMark.Frequency = plotter.CurrentMark.x;
            ReloadMarkers();
        }

        void itemAddCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (plotter.CurrentMark == null)
            {
                Console.Beep(200, 500);
                return;
            }

            TimeMark markA = new TimeMark(PointToTime(posa), "NoteOn", 80, this.thesolution.CurrentTrainingSet.SampleRate, plotter.CurrentMark.x);
            TimeMark markB = new TimeMark(PointToTime(posb), "NoteOff", 0, this.thesolution.CurrentTrainingSet.SampleRate, plotter.CurrentMark.x);


            markA.RelatedMark = markB;

            int idxA = 0;
            int idxB = 0;
            for (int i = 0; i < this.allMarks.Count; i++)
            {
                var mark = this.allMarks[i];
                if (mark.MarkTime.TotalSeconds <= markA.MarkTime.TotalSeconds)
                    idxA = i;
            }

            this.allMarks.Insert(idxA + 1, markA);


            for (int i = 0; i < this.allMarks.Count; i++)
            {
                var mark = this.allMarks[i];
                if (mark.MarkTime.TotalSeconds <= markB.MarkTime.TotalSeconds)
                    idxB = i;
            }


            this.allMarks.Insert(idxB + 1, markB);

            ReloadMarkers();  
        }


        TimeSpan PointToTime(int pos)
        {
            return TimeSpan.FromSeconds((double)pos / this.thesolution.CurrentTrainingSet.SampleRate);
        }

        void plotter_OnMarkedSetted(object who, int idx, float x, float y)
        {
            this.lblFrequency.Content = "F:" + x + " Hz"; 
        }

        void WPShowFreq_SourceInitialized(object sender, EventArgs e)
        {
            ReloadMarkers();
        }

        public void ReloadMarkers()
        {
            List<TimeMark> marks = this.thesolution.CurrentTrainingSet.Markers.MarkersBetween(posa, posb);
            this.theMarks = marks;

            this.Title = "" + (posb - posa + 1) + "  " + (float)(samplerate) / (posb - posa + 1);
            this.gridMarkers.ItemsSource = this.theMarks.Where(x => x.IsNoteOn || x.IsNoteOff).ToList();

            StartPlot();
            ShowFFT();
        }

        int FrequencyToMidi(float f)
        {
            double fb = 8.1757989156f;
            double midi = Math.Log(f / fb) * 12 / Math.Log(2);

            int min = (int)midi;
            int max = min + 1;

            if (Math.Abs(midi - min) < Math.Abs(midi - max))
                return min;

            return max;
        }
        private void ShowFFT()
        {
            float minfrequency = 60;
            var res = FrequenciesPeaks.CalculatePeaks(p, posa, posb, samplerate, 5, 0.05f,minfrequency);

            plotter.ClearCurves();

            if (this.thesolution != null && this.thesolution.TrainerFrequencies!=null)
            {

                float[] neural = this.thesolution.TrainerFrequencies.ApplyNetwork(res);

                var peaks = PeakDetection.Detect(neural, sortbypks: true);

                themidinotes = new List<int>();
                thefrequencies = new List<float>();

                //int pos = peaks.locs[peaks.pks.MaxPosition()];

                for (int i = 0; i < peaks.locs.Length; i++)
                {
                    int pos = peaks.locs[i];
                    float f = res.allhz[pos];
                    int midi = FrequencyToMidi(f);

                    if (i == 0 || Math.Abs(peaks.pks[i - 1] - peaks.pks[i]) < 0.1f)
                    {
                        themidinotes.Add(midi);
                        thefrequencies.Add(f);
                    }
                    else
                        break;
                }

                TimeMark mark = new TimeMark(TimeSpan.Zero, "NoteOn", /*themidinotes[0],*/ 127, samplerate, thefrequencies[0]);
                this.Title = mark.Note + " " + mark.Octave + " " + mark.Frequency + "~" + thefrequencies[0];


                plotter.AddCurve<float>("Neural", res.allhz, neural, Colors.Black);
                plotter.AddCurve<float>("Neural pks", peaks.locs.Select(x => res.allhz[x]).ToArray(), peaks.pks, Colors.Brown, Extra.AdaptedSymbolType.Star);

            }

            var smooth = AldCudaAlgorithms.WaveSmooth(res.allfft, res.allfft.Length / 10, 1+res.allfft.Length / 1000);
            smooth.NormalizeFit();

            res.allfft.NormalizeFit();

            plotter.AddCurve<float>("FFT", res.allhz, res.allfft, Colors.Green ,setprimary:true);

      

            plotter.AddCurve<float>("Harmonics Sum", res.allhz, res.allharm , Colors.Red);

            plotter2.ClearCurves();

            var cepsx = Vectors.LinSpace(res.cepstrum.Length);

            plotter2.AddCurve("Cepstrum", cepsx, res.cepstrum, Colors.Gray, Extra.AdaptedSymbolType.None);
            plotter2.AddCurve("AutoCorrelation", cepsx, res.autocorr, Colors.Black, Extra.AdaptedSymbolType.None);

            plotter.AddCurve("Ceps To Freq", res.allhz, res.allcepstofreq, Colors.DarkOrange, Extra.AdaptedSymbolType.None);
            plotter.AddCurve("Autocorr To Freq", res.allhz, res.allacorrtofreq, Colors.Blue, Extra.AdaptedSymbolType.None);


            //plotter2.AddCurve("Smooth", cepsx, res.smoothcepstrum, Colors.Blue, Extra.AdaptedSymbolType.None);

/*
            float[] points = new float[cepstrum[0].Length];            
            foreach(var i in theMarks)
                points[(int)(samplerate / i.Frequency)] = cepstrum[1][(int)(samplerate / i.Frequency)];
            plotter2.AddCurve("Marks", cepstrum[0], points, Colors.Blue);
            */
            int mincepstrum = (int)(samplerate / 60.0);
            int maxcepstrum = (int)(samplerate / 1500);

            float[] points2 = new float[res.cepstrum.Length];
            points2[mincepstrum] = 10000;
            points2[maxcepstrum] = 10000;

            plotter2.AddCurve("Marks", cepsx, points2, Colors.Red);

            

            sliderAxisX.Minimum = 100;
            sliderAxisX.Maximum = samplerate / 2;

            plotter.SetMaxX(samplerate / 2 / 4);
            sliderAxisX.Value = samplerate / 2 / 4/4;
        }

        private void StartPlot()
        {
            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Hz", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "db", Mayormarks = 1, MinorMarks = 0.1 };

            plotter.AxisX = axisX;
            plotter.AxisY = axisY;

            plotter.Title = "Fast Fourier Transform";

            Extra.AxisConfig axisX2 = new Extra.AxisConfig() { AxisTitle = "Hz", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY2 = new Extra.AxisConfig() { AxisTitle = "db", Mayormarks = 1, MinorMarks = 0.1 };

            plotter2.AxisX = axisX2;
            plotter2.AxisY = axisY2;

        }

        private void sliderAxisX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            plotter.SetMaxX(e.NewValue); 
        }
        
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            short[] data;

            data = new short[posb - posa + 1];
            for (int i = 0; i < data.Length; i++)
                if (posa + i<p.Length)
                    data[i] = (short)(p[posa + i] * short.MaxValue/2);
            IntPtr ptr = data.ToIntPtr(data.Length);

            TsWavPlayer.WriteBytes(samplerate,16,ptr, data.Length * sizeof(short),true);   
         
        }

        private void btnPlayMIDI_Click(object sender, RoutedEventArgs e)
        {
            if (themidinotes == null) return;

            //midiplayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, midiplayer.TheInstrumentsManager.GetInstrumentKey("Piano")));
            //midiplayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, midiplayer.TheInstrumentsManager.GetInstrumentKey("AcousticGuitarNylon")));
            
            //midiplayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, midiplayer.TheInstrumentsManager.GetInstrumentKey("Trumpet")));

            //var message = new SysExMessage(new byte[] { 0xB0, 0x07, 0x7F });

            //midiplayer.SendMessage(message);

            //new ChannelMessage(ChannelCommand.Controller,1,

            Task.Run(delegate
            {
                for (int i = 0; i < themidinotes.Count;i++ )
                    midiplayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOn, 1, themidinotes[i], 0x7f));

                
               
                Thread.Sleep(1000*(posb-posa+1)/samplerate);

                for (int i = 0; i < themidinotes.Count; i++)
                    midiplayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOff, 1, themidinotes[i], 0x7f));
            });
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //midiplayer.Dispose();
        }

        private void btnPlayMarkerMIDI_Click(object sender, RoutedEventArgs e)
        {
            Button item = sender as Button;
            TimeMark mark = item.DataContext as TimeMark;

            //var message = new SysExMessage(new byte[] { 0xB0, 0x07, 0x7F });

            //midiplayer.SendMessage(message);

                        
            Task.Run(delegate
            {
                for (int i = 0; i < themidinotes.Count; i++)
                    midiplayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOn, 1, mark.AproximateMIDI, 0x7f));

                Thread.Sleep(1000 * (posb - posa + 1) / samplerate);

                for (int i = 0; i < themidinotes.Count; i++)
                    midiplayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOff, 1, mark.AproximateMIDI, 0x7f));
            });
        }

        private void gridMarkers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.SetCurrentValue();
        }

    }
}
