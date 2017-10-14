using System;
using System.Collections.Generic;
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
using W = System.Windows.Forms;
using System.IO;
using TsExtraControls;
using AaBackPropagationFast;
using System.Diagnostics;
using TsFilesTools;
using AldSpecialAlgorithms;
using System.Threading;
using TsFFTFramework;
using AldSpecialAlgorithms.LPC;
using Sanford.Multimedia.Midi;
using AldFirstNetworkTrainer.Properties;
using System.Runtime.InteropServices;

namespace AldFirstNetworkTrainer
{
    public partial class MainWindow : Window
    {

        FileFolderDialog browser = new FileFolderDialog();
        List<TsTrainingSet> trainingFiles = new List<TsTrainingSet>();
        System.Windows.Forms.SaveFileDialog save = new W.SaveFileDialog();

        AldPlotterPoints plotter;
        TsTrainingSet currentTrainingSet;

        TsFirsStepSolution thesolution;

        List<TimeMark> editingMarks;
        bool editinglabels;
        float[][] spdata;

        PeakSolution? markdetection;
        int markdetectionsamples;

        string currentfolder;

        AldMidiPlayer midiplayer = new AldMidiPlayer();

        AldNetwork hopenetwork;

        float[] trainoutputfield;
        List<float[]> trainsolutions=new List<float[]>();

        List<FrequencySolution> solution;

        UsrBlock block;

        public MainWindow()
        {
            InitializeComponent();

            bool res = TsFFTFramework.TsFFTLink.LoadModule(TsFFTLink.Modules.CudaFFT);
            this.SourceInitialized += MainWindow_SourceInitialized;

            block = new UsrBlock();

            thesolution = new TsFirsStepSolution();
            thesolution.TrainerTimes = new TrainerAllTimes(thesolution,2,256,44100);
            thesolution.TrainerFrequencies = new TsAdminAllFrequencies(thesolution);
            thesolution.TrainerFrequencies.PeaksCalculated += TrainerFrequencies_PeaksCalculated;

            usrTrainerStatus.TheSolution = thesolution;
            this.usrTrainerAdmins.TheSolution = thesolution;

            this.usrTrainerFrequenciesStatus.TheSolution = thesolution;

            this.usrTheResult.TheSolution = thesolution;

            this.cbInstrument.ItemsSource = midiplayer.TheInstrumentsManager.Keys;

            this.usrTheResult.MidiPlayer = midiplayer;
 
            var menu = new ContextMenu();
            listFiles.ContextMenu = menu;
            var menuitem = new MenuItem() { Header = "Open Location" };
            menuitem.Click += abrirUbicacion_Click;
            menu.Items.Add(menuitem);

            usrTrainerAdmins.ChangePath(System.IO.Directory.GetCurrentDirectory()+"\\Trainers\\");
            usrTheResult.ChangePath(System.IO.Directory.GetCurrentDirectory() + "\\Solutions\\");

            

        }


        void abrirUbicacion_Click(object sender, RoutedEventArgs e)
        {
            if(listFiles.SelectedIndex<0) return;
            var path = new FileInfo((listFiles.SelectedValue as TsTrainingSet).WaveFile).Directory.FullName;
            path = (listFiles.SelectedValue as TsTrainingSet).WaveFile;
            Process.Start("explorer", "/select, \""+path+"\"");
        }


        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            plotter = new AldPlotterPoints();
            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 5, MinorMarks = 1 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 0.25, MinorMarks = 0.1 };
            plotter.AxisX = axisX;
            plotter.AxisY = axisY;
            plotter.Title = "Learning Curve (Error).";
            plotter.AddCurve("Error", new double[] { 0 }, new double[] { 0 }, System.Drawing.Color.Green, Extra.AdaptedSymbolType.Circle);

            this.plotterHost.Child = plotter;
            //windowsHost.Child = plotter;
            waveDisplayer.OnWaveSelected += waveDisplayer_OnWaveSelected;

            waveDisplayer.DisplayChanged += waveDisplayer_DisplayChanged;
            waveDisplayer.BetweenMarkers += waveDisplayer_BetweenMarkers;
            waveDisplayer.PlayRequired += waveDisplayer_PlayRequired;

            displayerB.DisplayChanged += displayerB_DisplayChanged;

            if (File.Exists(LastNetworkPath("trainer.netHOPE")))
                hopenetwork = RawData.ReadObject<AldNetwork>(LastNetworkPath("trainer.netHOPE"));
            else
                hopenetwork =  new AldNetwork(new int[] { 7,14, 28, 14, 7, 1 }, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, 0.02f, 0.05f);

            //for (int i = 0; i < 25;i++ )
            //    AddConsoleText("Title"+i+":", i+"", Colors.Red, Colors.Black);

            LoadLastFolder();
        }

        void waveDisplayer_PlayRequired(object who, int start, int n)
        {
            short[] data;

            data = new short[n];
            for (int i = 0; i < data.Length; i++)
                data[i] = (short)(this.currentTrainingSet.Wave[start+i] * short.MaxValue / 2);

            IntPtr ptr = data.ToIntPtr(data.Length);

            TsWavPlayer.WriteBytes(this.currentTrainingSet.SampleRate, 16, ptr, data.Length * sizeof(short), true);  
        }

        void displayerB_DisplayChanged(object who, float start, float length)
        {
            waveDisplayer.DisplayRange(start, length);
        }

        void waveDisplayer_BetweenMarkers(object who, int posa, int posb)
        {
            var data = waveDisplayer.TheWaves[0].TheData[0];

            int markposa = data.Marks[posa].MarkPosition;
            int markposb = data.Marks[posb].MarkPosition-1; 

            //List<TimeMark> themarks =  data.Marks.MarkersBetween(markposa,markposb);

            WPShowFreq win = new WPShowFreq(data.Points,markposa,markposb,data.samplerate,/*themarks,*/thesolution,midiplayer);
            win.ShowDialog();
        }

        void waveDisplayer_DisplayChanged(object who, float start, float length)
        {
            spectrogram1.Display((float)start, (float)length);
            spectrogram2.Display((float)start, (float)length);
        }

        void waveDisplayer_OnWaveSelected(object sender, EventArgs e)
        {
            int start=0,length=0;
            float density;

            if (waveDisplayer.TheWaves == null) return;

            var time = waveDisplayer.GetSelectionRange(out start,out length,out density);

            TimeSpan startTime = TimeSpan.FromSeconds((float)start / currentTrainingSet.WaveDescriptor.SampleRate );

            if (editinglabels)
            {
                TimeMark delete = FindMinimunMark(startTime,0.1f);
                if (delete != null)
                {
                    editingMarks.Remove(delete);
                    Console.Beep(500, 100);
                }
                else
                {
                    ////editingMarks.Add(new TimeMark(startTime, "NoteOn", 0, 0, currentTrainingSet.WaveDescriptor.SampleRate));
                    editingMarks.Add(new TimeMark(0, 0, currentTrainingSet.WaveDescriptor.SampleRate, startTime, "NoteOn"));
                    Console.Beep(1000, 100);
                }
                waveDisplayer.RenderAll();
                
            }
            lblSelect.Content = "TimeStart:" + startTime + "L:" + length + "\tT:" + time.TotalSeconds+"\tD:"+density+"\tPuntos:"+(length/density);
        }

        private TimeMark FindMinimunMark(TimeSpan startTime, float minseconds)
        {
            foreach (var i in editingMarks)
                if (Math.Abs(i.MarkTime.TotalMilliseconds - startTime.TotalMilliseconds) <= minseconds * 1000f)
                    return i;
            return null;
        }

        private void LoadTrainingSet()
        {
            solution = null;
            ChangeEditMode(false);
            currentTrainingSet.LoadFilesInformation();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            waveDisplayer.ClearExisting();
            waveDisplayer.AddSamples(currentTrainingSet.Wave,0,Colors.SlateGray,currentTrainingSet.WaveDescriptor.SampleRate, currentTrainingSet.Markers);
            waveDisplayer.Resize();   
            Spectrogram();

            this.thesolution.TrainerTimes.ClearTrainers();
            this.thesolution.TrainerTimes.SetMarkers(currentTrainingSet.Markers);

            this.usrTrainerFrequenciesStatus.TheTrainingSet = currentTrainingSet;

            Console.WriteLine("Loading in : " + sw.ElapsedMilliseconds);

            markdetection = null;
        }

        void Spectrogram()
        {
            int start, n;
            float density;

            this.waveDisplayer.GetVisibleRange(out start, out n, out density);
            if (n <= 1) return;
            
            var data = this.waveDisplayer.TheWaves[0].TheData[0].Points;

            this.spectrogram1.Resize();
            this.spectrogram2.Resize();

            Stopwatch sw = new Stopwatch();

            sw.Start();

            float[] amps=null;
            float mean, sdev;

            spdata = AldCudaAlgorithms.AldGenerateSpectogram2(data, 1024,data.Length/512, false, out amps,out mean,out sdev);

         /*
            float nmean = spdata.Mean();
            var histogram = AldSpecialAlgorithms.AldAlgorithms.CreateHistogram(spdata, 1000);
            mean = AldSpecialAlgorithms.AldAlgorithms.GroupIntervalMean(histogram);
            sdev = AldSpecialAlgorithms.AldAlgorithms.GroupIntervalStandarDeviation(histogram, mean);
            float nsdev = spdata.StandarDeviation(nmean);

            
            this.plotter.ClearCurves();

            this.plotter.AddCurveM("Histogram", histogram.centers, histogram.frequencies, System.Drawing.Color.Red);
            //this.plotter.AxisX = new Extra.AxisConfig() { AxisTitle = this.plotter.AxisX.AxisTitle, MinorMarks = 0.01,Mayormarks=0.1 };

            this.plotter.AddLine("Mean",System.Drawing.Color.Blue, mean, 0f, mean, 100000f);
            this.plotter.AddLine("SD 1", System.Drawing.Color.Orange, mean-sdev, 0f, mean-sdev, 100000f);
            this.plotter.AddLine("SD 2", System.Drawing.Color.Orange, mean + sdev, 0f, mean + sdev, 100000f);

            this.plotter.AddLine("N Mean", System.Drawing.Color.Green,  nmean, 0f, nmean, 100000f);
            this.plotter.AddLine("NSD 1", System.Drawing.Color.Gray, nmean - nsdev, 0f, nmean - nsdev, 100000f);
            this.plotter.AddLine("NSD 2", System.Drawing.Color.Gray, nmean + nsdev, 0f, nmean + nsdev, 100000f);
           */ 

            Console.WriteLine("spectrogram " + sw.ElapsedMilliseconds);

            
            this.spectrogram1.GenerateSpectogram(spdata);
            this.spectrogram1.Display((float)start / data.Length, (float)n / data.Length);

            waveDisplayer.Resize();
        }

        private void btnCopyLastRows_Click(object sender, RoutedEventArgs e)
        {
            Table tab = txtConsole.Document.Blocks.FirstOrDefault(x => x is Table) as Table;

            if (tab == null)
            {
                var gridLenghtConvertor = new GridLengthConverter();
                tab = new Table();
                tab.Columns.Add(new TableColumn() { Name = "Title", Width = (GridLength)gridLenghtConvertor.ConvertFromString("*") });
                tab.Columns.Add(new TableColumn { Name = "Message", Width = (GridLength)gridLenghtConvertor.ConvertFromString("4*") });
                tab.RowGroups.Add(new TableRowGroup());
            }

            string[] lines = txtConsole.Selection.Text.Replace("\r","").Split('\n').Where(x=>x.Length>0).ToArray();
            string srow = currentTrainingSet.WaveName+"\t";

            for (int i = 0; i < lines.Length; i++)
            {
                string part = lines[i];
                int idx = part.IndexOf('\t');
                part = part.Substring(idx + 1);
                srow += part;
                if (i < lines.Length-1)
                    srow += '\t';
            }


            txt1.Text = srow;
            ///Clipboard.SetText(srow);
            Console.Beep(700, 100);

        }


        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Document.Blocks.Clear();
        }


        void AddConsoleText(string title, string value, Color colortitle, Color colorvalue)
        {
            Table tab = txtConsole.Document.Blocks.FirstOrDefault(x => x is Table) as Table;

            if (tab == null)
            {
                var gridLenghtConvertor = new GridLengthConverter();
                tab = new Table();
                tab.Columns.Add(new TableColumn() { Name = "Title", Width = (GridLength)gridLenghtConvertor.ConvertFromString("*") });
                tab.Columns.Add(new TableColumn { Name = "Message", Width = (GridLength)gridLenghtConvertor.ConvertFromString("4*") });
                tab.RowGroups.Add(new TableRowGroup());
                //txtConsole.Document.Blocks.Add(tab);

            }            

            
            tab.RowGroups[0].Rows.Add(new TableRow());
            var tabRow = tab.RowGroups[0].Rows[tab.RowGroups[0].Rows.Count-1];

            tabRow.Cells.Add(new TableCell(new Paragraph(new Run(title) { Foreground = new SolidColorBrush(colortitle) })) { TextAlignment = TextAlignment.Left });
            tabRow.Cells.Add(new TableCell(new Paragraph(new Run(value) { Foreground = new SolidColorBrush(colorvalue) })) { TextAlignment = TextAlignment.Left });


            txtConsole.Document.Blocks.Clear();
            txtConsole.Document.Blocks.Add(tab);
            txtConsole.ScrollToEnd();


            /*
            return;
            Paragraph res = new Paragraph();
            res.LineHeight = 0.1;
            res.BreakColumnBefore = true;
            

            Run ptitle = new Run();
            ptitle.TextDecorations.Add(TextDecorations.Underline);
            ptitle.Text=title;
            ptitle.Foreground = new SolidColorBrush(colortitle);            


            Run pvalue = new Run();
            pvalue.Text=value;
            pvalue.Foreground = new SolidColorBrush(colorvalue);


            res.Inlines.Add(ptitle);
            res.Inlines.Add(new Run("\t"));
            res.Inlines.Add(pvalue);

            txtConsole.Document.Blocks.Add(res);

            res = new Paragraph();
            res.LineHeight = 0.1;
            txtConsole.Document.Blocks.Add(res);
            */
        }
        private void listFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = listFiles.SelectedIndex;
            if (idx < 0) return;
            if (currentTrainingSet != null) currentTrainingSet.UnloadFilesInformation();
            currentTrainingSet = listFiles.SelectedValue as TsTrainingSet;
            this.thesolution.CurrentTrainingSet = currentTrainingSet;
            LoadTrainingSet();
            Settings.Default.LastFile = currentTrainingSet.WaveFile;
            Settings.Default.Save();
        }

        void LoadCustomFile(string path)
        {
            if (currentTrainingSet != null) currentTrainingSet.UnloadFilesInformation();
            var trainingset = new TsTrainingSet(path);
            currentTrainingSet = trainingset;
            LoadTrainingSet();

        }

        public string LastNetworkPath(string file)
        {
            string currentPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            return currentPath + "\\"+file;;
        }     
   
        private float MaxPeak(float[] solution, int j)
        {
            return Math.Abs(solution[j]);

            float a,c;
            float b = Math.Abs(solution[j]);
            //return b;
            if (j == 0) a = b;
            else a = Math.Abs(solution[j - 1]);

            if (j >= solution.Length-1) c = b;
            else c = Math.Abs(solution[j + 1]);

            if (a > b && a > c)
                return a;
            if (b > c)
                return b;
            return c;
        }


        private void ChangeEditMode(bool p)
        {
            editinglabels = p;
            if (p)
            {
                waveDisplayer.Cursor = Cursors.Cross;
                editingMarks = new List<TimeMark>();
                waveDisplayer.TheDataList[0].Marks = editingMarks;                
            }
            else
                waveDisplayer.Cursor = Cursors.Arrow;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (trainsolutions.Count == 0)
            {
                MessageBox.Show("Error no solutions.");
                return;
            }

            plotter.ClearCurves();
            plotter.AddCurve("Hope", System.Drawing.Color.Blue);

            float[] inputs = new float[hopenetwork.Inputs];
            Task.Run(new Action(delegate {

                for (int i = 0; i < 500; i++)
                {
                    float avg = 0;
                    for (int j = 0; j < trainoutputfield.Length; j++)
                    {
                        var output = new float[] { Math.Abs(trainoutputfield[j]) };
                        for (int k = 0; k < trainsolutions.Count; k++)
                            inputs[k] = Math.Abs(trainsolutions[k][j]);
                        //inputs.StandarNormalization();

                        float error = hopenetwork.Train(inputs, output);
                        avg += error;
                        //avg+=hopenetwork.TotalError(output);                        
                    }
                    avg /= trainoutputfield.Length;
                    plotter.AddPoint(1, i, avg);
                }

                RawData.WriteObject(LastNetworkPath("trainer.netHOPE"), this.hopenetwork);

                MessageBox.Show("OK");

            }));
            
        }

        private void SaveSolution(List<FrequencySolution> solution, string path)
        {
            Sanford.Multimedia.Midi.Track track = new Track();

            InstrumentsManager manager = new InstrumentsManager();



            track.Insert(0, new ChannelMessage(ChannelCommand.ProgramChange, 1, manager.GetInstrumentKey("DistortionGuitar")));

            //track.Insert(0,new Sanford.Multimedia.Midi.MetaMessage(MetaType.Tempo,

            foreach (var i in solution)
            {
                foreach (var midinote in i.Midies)
                {
                    track.Insert(SecondsToPosition(i.StartTime.TotalSeconds, 128), new Sanford.Multimedia.Midi.ChannelMessage(ChannelCommand.NoteOn, 1, midinote, 0x7f));
                    track.Insert(SecondsToPosition(i.EndTime.TotalSeconds, 128), new Sanford.Multimedia.Midi.ChannelMessage(ChannelCommand.NoteOff, 1, midinote, 0x7f));
                }
            }

            Sanford.Multimedia.Midi.Sequence sequence = new Sequence(128);

            sequence.Add(track);

            sequence.Save(path);
        }
        public void PlaySounds(List<FrequencySolution> solution)
        {
            int wait = (int)solution.First().StartTime.TotalMilliseconds;

            Thread.Sleep(wait);

            foreach (var isolution in solution)
            {
                for (int i = 0; i < isolution.Midies.Count; i++)
                {
                    var note = isolution.Midies[i];
                    float amp = isolution.Amplitudes[i];

                    midiplayer.SendMessage(new Sanford.Multimedia.Midi.ChannelMessage(Sanford.Multimedia.Midi.ChannelCommand.NoteOn, 1, (byte)note,(byte)(0x7f)));
                }

                TimeSpan delta = isolution.EndTime.Subtract(isolution.StartTime);
                Thread.Sleep((int)delta.TotalMilliseconds);

                foreach (var note in isolution.Midies)
                    midiplayer.SendMessage(new Sanford.Multimedia.Midi.ChannelMessage(Sanford.Multimedia.Midi.ChannelCommand.NoteOff, 1, (byte)note, 0x1f));
            }   
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            midiplayer.Dispose();
        }

        public int SecondsToPosition(double seconds, int tpqn, int bpm = 120)
        {
            double ticktempo = 60.0 / (tpqn * bpm);
            return (int)(seconds / ticktempo);
        }

        private void cbInstrument_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string instrument = cbInstrument.SelectedValue.ToString();

            midiplayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, midiplayer.TheInstrumentsManager.GetInstrumentKey(instrument)));

        }

        private void listFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void menuApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainingSet == null)
            {
                MessageBox.Show("No training set loaded.");
                return;
            }
            if (currentTrainingSet.Wave == null)
            {
                MessageBox.Show("No wave selected.");
                return;
            }

            if (this.thesolution.TrainerTimes.NetworksCount == 0)
            {
                MessageBox.Show("No networks loaded!!!");
                return;
            }

            int windowsLength = 1001;
            float windowsAmp = 1.0f / windowsLength;

            var wave = currentTrainingSet.Wave;
            
            var amdwave = TsFFTFramework.AldCudaAlgorithms.AverageTransform(wave, windowsAmp, windowsLength);
            amdwave.AldFitPositiveLimitMax();
            amdwave.Log10Normalization();
            waveDisplayer.AddSamples(amdwave, 0, TsColors.CommonColors[0]);
            /*
            var zcrwave = TsFFTFramework.AldCudaAlgorithms.ZeroCrossingRate(wave, windowsAmp, windowsLength);
            zcrwave.AldFitPositiveLimitMax();
            waveDisplayer.AddSamples(zcrwave, 0, TsColors.CommonColors[0]);  
            */
            var stewave = TsFFTFramework.AldCudaAlgorithms.ShortTimeEnergy(wave, windowsAmp, windowsLength);
            stewave.AldFitPositiveLimitMax();
            stewave.Log10Normalization();
            waveDisplayer.AddSamples(stewave, 0, TsColors.CommonColors[1]);
            
            var lpc = AldLPCTransform.LPCTransform(wave, currentTrainingSet.WaveDescriptor.SampleRate, 0.03f, 12);
            
            var lpcamd = TsFFTFramework.AldCudaAlgorithms.AverageTransform(lpc, windowsAmp, windowsLength);
            lpcamd.AldFitPositiveLimitMax();
            lpcamd.Log10Normalization();
            waveDisplayer.AddSamples(lpcamd, 0, TsColors.CommonColors[2]);
           
            var lpcste = TsFFTFramework.AldCudaAlgorithms.ShortTimeEnergy(lpc, windowsAmp, windowsLength);
            lpcste.AldFitPositiveLimitMax();
            lpcste.Log10Normalization();
            waveDisplayer.AddSamples(lpcste, 0, TsColors.CommonColors[3]);

            double resumerate = (double)currentTrainingSet.WaveDescriptor.Samples[0].Length / spdata[0].Length;

            this.thesolution.TrainerTimes.SetData1d("amd", amdwave);
            this.thesolution.TrainerTimes.SetData1d("ste", stewave);

            this.thesolution.TrainerTimes.SetData1d("lpcamd", lpcamd);
            this.thesolution.TrainerTimes.SetData1d("lpcste", lpcste);

            this.thesolution.TrainerTimes.SetData2d("spec cuda", spdata, 8, resumerate, 0f, negativenormalization: false);

            /*
            lpcste = TsFFTFramework.AldCudaAlgorithms.ShortTimeEnergy(lpc, windowsAmp, windowsLength / 8);
            lpcste.AldFitPositiveLimitMax();
            lpcste.Log10Normalization();
            waveDisplayer.AddSamples(lpcste.TsMultiply(-1), 0, TsColors.CommonColors[2]);
            */

            stackLegend.Children.Clear();

            for (int i = 0; i < this.thesolution.TrainerTimes.Trainers.Count; i++)
                stackLegend.Children.Add(new Label() { Content = this.thesolution.TrainerTimes.Trainers.ElementAt(i).Key, Foreground = new SolidColorBrush(TsColors.CommonColors[i]), Background = Brushes.White, Padding = new Thickness(1) });

            waveDisplayer.RenderAll();
        }
        private void menuTestSolution_Click(object sender, RoutedEventArgs e)
        {
            if (this.thesolution.TrainerTimes.Trainers.Count == 0) { MessageBox.Show("No trainer is created.."); return; }

            displayerB.ClearExisting();
            displayerB.AddSamples(this.thesolution.TrainerTimes[0].OutputField, 0, Color.FromArgb(200, 255, 0, 0));
            trainoutputfield = this.thesolution.TrainerTimes[0].OutputField;

            List<float[]> solutions = new List<float[]>();
            List<string> names = new List<string>();

            for (int i = 0; i < this.thesolution.TrainerTimes.Trainers.Count; i++)
            {
                solutions.Add(null);
                names.Add(null);
            }

            Parallel.For(0, this.thesolution.TrainerTimes.Trainers.Count, delegate(int i)
            {
                solutions[i] = this.thesolution.TrainerTimes[i].GetNetworkSolution();
                names[i] = this.thesolution.TrainerTimes.TrainerName(i);
            });


            for (int i = 0; i < solutions.Count; i++)
            {
                AddConsoleText(names[i], solutions[i].CompareAndGetError(this.thesolution.TrainerTimes[i].OutputField) + "", TsColors.CommonColors[i], Colors.Gray);

                if (i == 0)
                {
                    this.plotter.ClearCurves();
                    this.plotter.AddCurve("Histogram A", ErrorTools.lastHA.c,ErrorTools.lastHA.h, System.Drawing.Color.Red);

                    this.plotter.AddCurve("Histogram B", ErrorTools.lastHB.c, ErrorTools.lastHB.h, System.Drawing.Color.Green);

                }
                
            }
            int specindex = 0;
            
            for (int i = 0; i < solutions.Count; i++)
            {
                if (i>0 && solutions[i].Length != solutions[0].Length)
                    solutions[i] = solutions[i].AdaptVector(solutions[0].Length);
                solutions[i] = solutions[i].TsAbsolutize();

                if (this.thesolution.TrainerTimes[i] is TrainerOneSpectogram)
                    specindex = i;
            }




            var spechistogram = AldAlgorithms.CreateOverlappedHistogram(solutions[specindex], 512, 0.01f);
            int idx = spechistogram.MaxIndex();

            double total = spechistogram.h.Sum();
            double totalto = spechistogram.SumTo(idx);


            idx = spechistogram.IndexOfPercent(/*0.85f*/0.85f);

            //MessageBox.Show(totalto / total+"");

            float peakerror = spechistogram.c[idx] + spechistogram.window / 2;
            solutions[specindex].TsReduceMargin(peakerror);

            //this.plotter.ClearCurves();
            //this.plotter.AddCurve("Histogram", spechistogram.c, spechistogram.h, System.Drawing.Color.Red, Extra.AdaptedSymbolType.Star);

            //spechistogram = AldAlgorithms.CreateOverlappedHistogram(solutions[specindex], 512, 0.01f);
            //this.plotter.AddCurve("Histogram After", spechistogram.c, spechistogram.h, System.Drawing.Color.Green, Extra.AdaptedSymbolType.Star);

            for (int i = 0; i < this.thesolution.TrainerTimes.Trainers.Count; i++)
            {
                displayerB.AddSamples(solutions[i].TsMultiply(-1), 0, TsColors.CommonColors[i]);
                trainsolutions.Add(solutions[i]);
            }

            float[] sum = new float[solutions[0].Length];
            float[] sumsum = new float[solutions[0].Length];

            for (int j = 0; j < sum.Length; j++) 
                sum[j] = 1;

            float[] spec = new float[solutions[0].Length];

            for (int i = 0; i < solutions.Count; i++)
                for (int j = 0; j < sum.Length; j++)
                {
                    if (!(this.thesolution.TrainerTimes[i] is TrainerOneSpectogram))
                    {
                        sum[j] *= (1f + MaxPeak(solutions[i], j) / 40);
                        sumsum[j] = MaxPeak(solutions[i], j);
                    }
                    else
                    {
                        spec[j] += MaxPeak(solutions[i], j);
                        if (spec[j] > 1) spec[j] = 1;
                    }
                    
                }
            

            spec.NormalizeFit();

            float max = sum.Max();
            //float max = 1;

            
            for (int j = 0; j < sum.Length; j++)
            {
                float xt = MaxPeak(spec, j);
                if (xt == 0.25f && sumsum[j]>1e-2)
                    sum[j] = max;
                else if (( Math.Abs( (sum[j] - 1) / (max - 1)) > 0.01 && xt > 0.02f))
                    sum[j] *= (1f + xt);
                    //sum[j] = xt;
                else
                    sum[j] = 0;
                    //sum[j] = 1;
            }
            
            /*
            for (int j = 0; j < sum.Length; j++)
            {
                float xt = MaxPeak(spec, j);
                if (xt == 0.25f && sumsum[j] > 1e-2)
                    sum[j] = max;
                //else if ((Math.Abs((sum[j] - 1) / (max - 1)) > 0.05 && xt > 0.01f))
                //    sum[j] *= (1f + xt);
                else if ((Math.Abs((sum[j] - 1) / (max - 1)) > 0.01 && xt > 0.15f))
                    sum[j] *= (1f + xt);

                    //sum[j] = xt;
                else
                    sum[j] = 0;
                //sum[j] = 1;
            }
            */
            sum.NormalizeFit();

            AddConsoleText("Error Total:", sum.CompareAndGetError(this.thesolution.TrainerTimes[0].OutputField) + "", Colors.Red, Colors.Gray);

            sum.RepairBadPeaks();


            trainsolutions.Clear();


            displayerB.AddSamples(sum.TsMultiply(-1), 0, Colors.DarkCyan);

            var markdetection = PeakDetection.Detect(sum, Pd: 10, Th: 0.02f);
            markdetectionsamples = sum.Length;


            float[] peaks = new float[sum.Length];
            for (int i = 0; i < markdetection.locs.Length; i++)
                peaks[markdetection.locs[i]] = markdetection.pks[i];

            displayerB.AddSamples(peaks, 0, Colors.Black);

            waveDisplayer.AddSamples(peaks.AdaptVector(currentTrainingSet.WaveDescriptor.Samples[0].Length).TsMultiply(-1), 0, Colors.Chocolate);




            this.markdetection = markdetection;

            displayerB.Resize();
            displayerB.RenderAll();
        }

        private void menuApplyMarkers_Click(object sender, RoutedEventArgs e)
        {
            if (markdetection == null)
            {
                MessageBox.Show("No Detection.");
                return;
            }


            List<TimeMark> marks = new List<TimeMark>();

            TimeMark lastmark = null;
            int samplerate = currentTrainingSet.WaveDescriptor.SampleRate;

            for (int i = 0; i < markdetection.Value.locs.Length; i++)
            {
                int pos = markdetection.Value.locs[i];
                if (markdetection.Value.pks[i] < 0.05) continue;

                double proportion = (double)pos / markdetectionsamples * currentTrainingSet.WaveDescriptor.Samples[0].Length;
                double seconds = proportion / samplerate;
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                TimeMark mark = new TimeMark(time, "NoteOn", /*0,*/ 0, samplerate, 0);

                if (i > 0 && marks.Count > 0)
                {
                    var noteoff = mark.ToNoteOFF();
                    marks.Last().RelatedMark = noteoff;
                    marks.Add(noteoff);
                }

                marks.Add(mark);
                lastmark = mark;
            }

            if (marks.Count > 0)
            {
                var noteoff = new TimeMark(currentTrainingSet.WaveDescriptor.Duration, "NoteOff", /*0,*/ 0, samplerate, 0);
                marks.Last().RelatedMark = noteoff;
                marks.Add(noteoff);
            }

            waveDisplayer.TheDataList[0].Marks = marks;
            currentTrainingSet.Markers = marks;
            this.thesolution.TrainerTimes.SetMarkers(marks);

            float error = currentTrainingSet.MarksError(0.025f);

            AddConsoleText("E.M.:", error.ToString(), Colors.Violet, Colors.Black);

            waveDisplayer.RenderAll();
        }

        private void menuStartTraining2_Click(object sender, RoutedEventArgs e)
        {
            if (thesolution == null || thesolution.TrainerFrequencies == null)
            {
                MessageBox.Show("No trainer frequencies is loaded.");
                return;
            }

            if (currentTrainingSet == null || currentTrainingSet.Markers == null || currentTrainingSet.Markers.Count == 0)
            {
                MessageBox.Show("No data of segment.");
                return;
            }

            WPFTrainer2 win = new WPFTrainer2(currentTrainingSet, this.thesolution);
            win.ShowDialog();
        }


        private void menuTunner_Click(object sender, RoutedEventArgs e)
        {
            WpfTunner win = new WpfTunner();
            win.ShowDialog();
            win.Close();
        }

        private void menuRecord_Click(object sender, RoutedEventArgs e)
        {

            if (!Directory.Exists(currentfolder))
            {
                MessageBox.Show("No folder selected.");
                return;
            }

            string name = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;
            string path = currentfolder + "\\" + name + ".wav";

            Process p = new Process();
            p.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\TsRecorder.exe";
            p.StartInfo.Arguments = "\"" + path + "\"";
            p.Start();

            p.WaitForExit();


            if (p.ExitCode != 0)
            {
                MessageBox.Show("Error recording.");
                return;
            }
            else
            {
                loadPath(currentfolder);
                if (currentTrainingSet != null) currentTrainingSet.UnloadFilesInformation();
                currentTrainingSet = trainingFiles.First(x => x.WaveFile == path);
                
                LoadTrainingSet();
                this.thesolution.CurrentTrainingSet = currentTrainingSet;
            }


           /*
            WpfRecordAndEdit win = new WpfRecordAndEdit(currentfolder);
            win.Owner = this;

            if (win.ShowDialog() == true)
            {
                loadPath(currentfolder);
                if (currentTrainingSet != null) currentTrainingSet.UnloadFilesInformation();
                currentTrainingSet = trainingFiles.First(x => x.WaveFile == win.SavedPath);
                LoadTrainingSet();
            }*/


        }

        private void menuSelPeaks_Click(object sender, RoutedEventArgs e)
        {
            if (thesolution == null || thesolution.TrainerFrequencies == null)
            {
                MessageBox.Show("No trainer frequencies is loaded.");
                return;
            }

            int start, length;
            float density;

            if (waveDisplayer.TheWaves == null) return;

            var time = waveDisplayer.GetSelectionRange(out start, out length, out density);

            var data = waveDisplayer.TheWaves[0].TheData[0];

            List<TimeMark> themarks = new List<TimeMark>();

            int from = start;
            int to = start + length - 1;
            if (length == 0)
            {
                MessageBox.Show("Error no data selected.");
                return;
            }

            if (this.thesolution == null || this.thesolution.TrainerFrequencies == null || this.thesolution.TrainerFrequencies.trainer == null || this.thesolution.TrainerFrequencies.trainer.Network == null)
            {
                MessageBox.Show("Error, no data loaded.");
                return;                
            }

            WPShowFreq win = new WPShowFreq(data.Points, from, to, data.samplerate, /*themarks,*/ this.thesolution, midiplayer);

            win.ShowDialog();
        }
        private void menuSaveMarkersChanges_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainingSet == null)
            {
                MessageBox.Show("No training set.");
                return;
            }
            if (this.editinglabels)
            {
                MessageBox.Show("Cant save the data while editing labels.");
                return;
            }

            string midifile = currentTrainingSet.WaveFile.Replace(".wav", ".aldmidi");
            string[] lines = this.currentTrainingSet.Markers.Select(x=>x.ToString()).ToArray();
            File.WriteAllLines(midifile, lines);
            this.currentTrainingSet.Update();
            LoadTrainingSet();
            MessageBox.Show("Saved and reloaded...");
        }

        private void menuStartMarker_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainingSet == null)
            {
                MessageBox.Show("No training set.");
                return;
            }

            string midifile = currentTrainingSet.WaveFile.Replace(".wav", ".aldmidi");
            if (editinglabels)
            {
                menuStartMarker.Header = "Start Manual Marking";
                
                ChangeEditMode(false);
                editingMarks = editingMarks.OrderBy(x => x.MarkPosition).ToList();


                List<TimeMark> marks = new List<TimeMark>();

                int samplerate = currentTrainingSet.WaveDescriptor.SampleRate;

                TimeMark endmark = null;

                for (int i = 0; i < editingMarks.Count - 1; i++)
                {
                    marks.Add(editingMarks[i]);
                    endmark = new TimeMark(editingMarks[i + 1].MarkTime, "NoteOff", /*0,*/ 0, samplerate, editingMarks[i].Frequency);
                    marks.Add(endmark);
                }
                if (editingMarks.Count > 0)
                {
                    marks.Add(editingMarks.Last());
                    endmark = new TimeMark(currentTrainingSet.WaveDescriptor.Duration, "NoteOff", /*0,*/ 0, samplerate, editingMarks.Last().Frequency);
                    marks.Add(endmark);
                }


                string[] lines = marks.Select(x => x + "").ToArray();
                File.WriteAllLines(midifile, lines);

                LoadTrainingSet();

                MessageBox.Show("Saved and reloaded...");
            }
            else
            {
                menuStartMarker.Header = "Save Edited Marks";
                if (File.Exists(midifile))
                {
                    if (MessageBox.Show("The label file already exists, do you want to replace it???", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    File.Delete(midifile);
                }
                ChangeEditMode(true);
            }
        }

        void loadPath(string path)
        {
            currentfolder = path;

            Settings.Default.LastFolder = path;
            Settings.Default.Save();

            trainingFiles.Clear();
            var files = Directory.EnumerateFiles(path, "*.wav");
            foreach (var i in files)
                //if (File.Exists(i.Replace(".wav", ".aldmidi")))                
                trainingFiles.Add(new TsTrainingSet(i));

            //btnSelectBase.Content = "[" + trainingFiles.Count + " Files]" + path;
            listFiles.ItemsSource = null;
            listFiles.ItemsSource = trainingFiles;
            listFiles.DisplayMemberPath = "WaveName";

            //currentTrainingSet = trainingFiles[0];

            waveDisplayer.RenderAll();
            waveDisplayer.Resize();        
        }

        private void menuChangeWorkingFolder_Click(object sender, RoutedEventArgs e)
        {
            if (browser.ShowDialog() != W.DialogResult.OK) return;
            string path = browser.SelectedPath;
            loadPath(path);
        }


        private void menuLastFolder_Click(object sender, RoutedEventArgs e)
        {
            LoadLastFolder();
        }

        void LoadLastFolder()
        {
            string path = Settings.Default.LastFolder;

            if (Directory.Exists(path))
                loadPath(path);
            expanderFiles.IsExpanded = true;
        }

        private void menuApplySounds_Click(object sender, RoutedEventArgs e)
        {
            
            if (currentTrainingSet == null)
            {
                MessageBox.Show("No training set.");
                return;
            }
            if (currentTrainingSet.Markers == null || currentTrainingSet.Markers.Count == 0)
            {
                MessageBox.Show("No markers data.");
                return;
            }
            if (this.thesolution == null|| this.thesolution.TrainerFrequencies==null)
            {
                //trainerFrequencies = new TsAdminAllFrequencies(LoadNetworks2());
                MessageBox.Show("No frequencies network is loaded!!!");
                return;
            }
            this.block.Block(this.mainGrid, "Analyzing Ranges.");
            Task.Run(new Action(FrequenciesSolution));
        }

        void TrainerFrequencies_PeaksCalculated(object who, int idx)
        {
            block.SetProgress(idx, this.thesolution.TrainerFrequencies.ranges.Count);
        }

        void FrequenciesSolution()
        {
            this.thesolution.TrainerFrequencies.LoadTrainingSet(currentTrainingSet);
            block.SetProgress(0, this.thesolution.TrainerFrequencies.ranges.Count);

            solution = new List<FrequencySolution>();

            float promerror = 0;
            float promoctaveerror = 0;            

            for (int i = 0; i < this.thesolution.TrainerFrequencies.ranges.Count; i++)
            {
                var sol = this.thesolution.TrainerFrequencies.GetSolution(i);
                promerror += sol.Error;
                promoctaveerror += sol.OctaveError;
                solution.Add(sol);
                //block.SetProgress(this.thesolution.TrainerFrequencies.ranges.Count+i, this.thesolution.TrainerFrequencies.ranges.Count);
            }

            promerror /= this.thesolution.TrainerFrequencies.ranges.Count;
            promoctaveerror /= this.thesolution.TrainerFrequencies.ranges.Count;

            //txtConsole.Text += "\nFrequencies Error:\t\t" + promerror;

            this.Dispatcher.Invoke(new Action(delegate() {
                AddConsoleText("Frequencies Error:", promerror + "", Colors.Red, Colors.Gray);
                AddConsoleText("Octaves Error:", promoctaveerror + "", Colors.Green, Colors.Gray);            
            }));


            this.thesolution.TheSolution = solution;

            this.Dispatcher.Invoke(new Action(delegate() {
                this.block.UnBlock(this.mainGrid);
            }));

            if (MessageBox.Show("Do you want to play the result??", "Solution Generated!!!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Task.Run(new Action(delegate
                {
                    PlaySounds(solution);
                }));

            }
        }

        private void menuGenerateMIDI_Click(object sender, RoutedEventArgs e)
        {
            if (solution == null)
            {
                MessageBox.Show("No solution is loaded.");
                return;
            }
            save.Filter = "MIDI Files|*.mid";
            save.FileName = currentTrainingSet.WaveName + ".mid";
            if (save.ShowDialog() == W.DialogResult.OK)
            {
                
                SaveSolution(solution, save.FileName);
                MessageBox.Show("Saved in " + save.FileName);
            }
        }

        float[] GetHaar(float[] data)
        {
            float[] haar = WaveletHaar.FWT(data);
            float[] mhaar = new float[haar.Length / 2];

            for (int i = 0; i < mhaar.Length; i++)
                mhaar[i] = haar[mhaar.Length*0 + i];
           
            //return mhaar.AdaptVector(mhaar.Length);
            return mhaar;
        }

        private void menuApplyHaar_Click(object sender, RoutedEventArgs e)
        {
            float[] haar = WaveletHaar.NxtHaar(currentTrainingSet.Wave, 8); //GetHaar(currentTrainingSet.Wave);

            for (int i = (haar.Length>>8); i < haar.Length; i++)
                if (Math.Abs(haar[i]) <2.5f)
                    haar[i] = 0;            

            haar = WaveletHaar.InverseNxtHaar(haar, 8);


                //for (int i = 0; i < 4; i++)
                //    haar = GetHaar(haar);
            //haar = haar.AdaptVector(currentTrainingSet.Wave.Length);
            //haar.NormalizeCentral();
            //haar.ChebNormalization(0.98f);

            //haar = TsFFTFramework.AldCudaAlgorithms.AverageTransform(haar, 10, 1000);
            haar.AldFitPositiveLimitMax();

            waveDisplayer.AddSamples(sample: haar, wavecolor: Colors.Blue);
        }

        private void menuLastFile_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Settings.Default.LastFile))
                LoadCustomFile(Settings.Default.LastFile);
            else
                MessageBox.Show("The file ins't exists anymore");
        }

        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void menuFile_MouseEnter(object sender, MouseEventArgs e)
        {
            menuFile.IsSubmenuOpen = true;
        }

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern IntPtr HostPlanScalogram(IntPtr deviceData, int n);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern int HostIterateWindow(int wn, IntPtr plan, float[] hostresult);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern void HostDestroyScalogram(IntPtr data);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaDll\Release\TsCudaDll.dll")]
        public static extern IntPtr HostCloneWaveToDevice(float[] wave, int n);


        

        private void menuApplyMorlet_Click(object sender, RoutedEventArgs e)
        {
            int windowsLength = 1001;
            float windowsAmp = 1.0f / windowsLength;

            var wave = currentTrainingSet.Wave;

            IntPtr deviceWave = HostCloneWaveToDevice(wave, wave.Length);

            IntPtr plan = HostPlanScalogram(deviceWave, wave.Length);

            //MessageBox.Show(plan + ""); 

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int n = 32;
            float[][] data = new float[n][];
            float wn = 82;
            for (int i = 0; i < n; i++)
            {
                
                float[] result = new float[wave.Length+ (4-wave.Length%4)];
                if (wn > result.Length) Console.WriteLine("mmm : " + wn + "  " + result.Length);
                int riterate;
                lock (result)
                {
                    riterate = HostIterateWindow((int)wn, plan, result);
                }
                if (riterate == 0)
                    MessageBox.Show(":p");
                result.NormalizeFit();
                
                wn *= (float)Math.Pow(2,1.0/12);
                //wn += 10;
                data[i] = result;
            }
            /*
            float[] amps = new float[wave.Length];
            float mean;
            float sdev;
            spdata = AldCudaAlgorithms.AldGenerateSpectogram2(wave, 1024, wave.Length / 512, false, out amps, out mean, out sdev);
            */
            DisplayScalogram(data);

            /*
            Console.WriteLine(sw.ElapsedMilliseconds);

            morlet = Windows.Morlet(1001);
            float[] result2 = new float[wave.Length];
            riterate = HostIterateWindow(morlet, morlet.Length, plan, result2);


            morlet = Windows.Morlet(2001);
            float[] result3 = new float[wave.Length];
            riterate = HostIterateWindow(morlet, morlet.Length, plan, result3);


            if (plan == IntPtr.Zero || riterate == 0) 
            {
                MessageBox.Show("Morlet Error!!!");
                return;
            }

            result.NormalizeFit();
            result2.NormalizeFit();
            result3.NormalizeFit();

            waveDisplayer.AddSamples(result, 0, TsColors.CommonColors[2]);
            waveDisplayer.AddSamples(result2, 0, TsColors.CommonColors[3]);
            waveDisplayer.AddSamples(result3, 0, TsColors.CommonColors[4]);
            */
            HostDestroyScalogram(plan);

            /*
            var morlet = TsFFTFramework.AldCudaAlgorithms.CudaMorletConvolution(wave, windowsAmp, windowsLength);
            morlet.AldNormalizePositive(1);
            waveDisplayer.AddSamples(morlet, 0, TsColors.CommonColors[4]);


            windowsLength = 10001;
            windowsAmp = 1.0f / windowsLength;

            morlet = TsFFTFramework.AldCudaAlgorithms.CudaMorletConvolution(wave, windowsAmp, windowsLength);
            morlet.AldNormalizePositive(1);

            waveDisplayer.AddSamples(morlet.TsMultiply(-1), 0, TsColors.CommonColors[5]);
            */
        }

        private void DisplayScalogram(float[][] scdata)
        {
            int start, n;
            float density;

            this.waveDisplayer.GetVisibleRange(out start, out n, out density);
            if (n <= 1) return;

            var data = this.waveDisplayer.TheWaves[0].TheData[0].Points;

            this.spectrogram1.Resize();
            this.spectrogram2.Resize();


            this.spectrogram1.GenerateSpectogram(scdata);
            this.spectrogram1.Display((float)start / data.Length, (float)n / data.Length);

            //this.spectrogram1.Resize();

            waveDisplayer.Resize();
        }

        private void menuResults_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuAdjustMarkers_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure of auto adjust the markers??", "Question...", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            this.currentTrainingSet.AutoAdjust();
            this.currentTrainingSet.SaveAdjust();
            Console.Beep(700, 100);

        }

        private void usrTheResult_Loaded(object sender, RoutedEventArgs e)
        {

        }





    }
}
 
