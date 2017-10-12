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
using TsExtraControls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using System.Diagnostics;

namespace AldFirstNetworkTrainer
{
	public partial class UsrResults : UserControl
	{
        FileFolderDialog ffd = new FileFolderDialog();
        TsFirsStepSolution solution;

        List<TsPentagramEngine.InputFormat> thenotes;

        float center;

        List<float> distances;

        AldMidiPlayer midiplayer;

        string[] tempos = new string[] { "2/4", "3/4", "4/4", "5/4", "6/4", "3/8", "6/8", "9/8", "12/8" };
        string[] octavechanges = new string[] {"One octave down","No change","One octave up", "Two octaves up"};

        public AldMidiPlayer MidiPlayer 
        {
            get { return this.midiplayer; }
            set 
            {
                this.midiplayer = value;
                this.cbMidiInstrument.ItemsSource = midiplayer.TheInstrumentsManager.Keys;
                this.cbMidiInstrument.SelectedValue = "AcousticGuitarNylon";
            }
        }


        public TsFirsStepSolution TheSolution 
        {
            get { return solution; }
            set { this.solution = value; }
        }

		public UsrResults()
		{
			this.InitializeComponent();

            var menu = new ContextMenu();
            gridFiles.ContextMenu = menu;
            var menuitem = new MenuItem() { Header = "Open Location" };
            menuitem.Click += openLocation_Click;
            menu.Items.Add(menuitem);

            this.cbTempo.ItemsSource = tempos.ToList();
            this.cbOctaveChange.ItemsSource = octavechanges.ToList();

            cbTempo.SelectedIndex = 2;
            cbOctaveChange.SelectedIndex = 0;
            
		}

        void openLocation_Click(object sender, RoutedEventArgs e)
        {
            if (gridFiles.SelectedIndex < 0) return;
            var path = (this.gridFiles.SelectedValue as FileInfo).FullName;
            Process.Start("explorer", "/select, \"" + path + "\"");
        }

        private void btnResults_Click(object sender, RoutedEventArgs e)
        {
            //BinaryFormatter bf = new BinaryFormatter();
            //string path = "bksolution.solution";
            //FileStream fs=null;
            
            //if(File.Exists(path))
            //    fs= new FileStream(path, FileMode.OpenOrCreate);

            if (this.TheSolution.TheSolution == null)
            {
                //if(fs!=null)
                //    this.TheSolution.TheSolution = (List<FrequencySolution>)bf.Deserialize(fs);    
                //else
                //{ 
                    MessageBox.Show("Solution is null...");
                    return;
                //}
            }
            //else
            //{
            //    if(fs==null)
            //        fs = new FileStream(path, FileMode.OpenOrCreate);
            //    bf.Serialize(fs, this.TheSolution.TheSolution);
            //}

            //if(fs!=null)
            //    fs.Dispose();

            this.thenotes = Convert(this.TheSolution.TheSolution);
            usrMIDIVisual1.Notes = thenotes;

            distances = new List<float>();

            for (int i = 0; i < thenotes.Count; i++)
                for (int j = i + 1; j < i + 2 && j < thenotes.Count; j++)
                    distances.Add((float)(thenotes[j].StartNote.TotalSeconds - thenotes[i].StartNote.TotalSeconds));

            distances = distances.Where(x => x > 0).ToList();
            distances.Sort();

            this.sliderPercent.Value = 0.04;
        }

        public List<TsPentagramEngine.InputFormat> Convert(List<FrequencySolution> result)
        {
            return result.Select(x => new TsPentagramEngine.InputFormat(x.Midies, x.StartTime, x.EndTime)).ToList();
        }

        private void sliderPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (distances==null|| distances.Count == 0) return;            

            float val = (float)this.sliderPercent.Value;

            //for (int i = 0; i < this.distances.Count; i++)
            //    distances[i] = (float)(Math.Log(distances[i]) / Math.Log(2));

            var histogram = AldSpecialAlgorithms.AldAlgorithms.CreateOverlappedHistogram(this.distances.ToArray(), 100, val /*0.008f*/);

            this.plotter.ClearCurves();
            this.plotter.AddCurve("Histogram", histogram.c, histogram.h, Colors.Green, TsExtraControls.Extra.AdaptedSymbolType.Star);

            int maxindex = histogram.MaxIndex();
            center = histogram.c[maxindex];
            //center = (float)Math.Pow(2, histogram.c[maxindex]);
        }

        private void btnGenerateMIDI_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution == null||this.TheSolution.TheSolution==null)
            {
                MessageBox.Show("No solution is loaded.");
                return;
            }

            string path = this.ffd.SelectedPath;

            if (!path.EndsWith("\\")) path += "\\";

            //if(this.TheSolution.CurrentTrainingSet!=null)
                path += this.TheSolution.CurrentTrainingSet.WaveName + ".mid";
            //else
            //    path += "2013-12-17 19.15.42.wav.mid";

            if (File.Exists(path))
                File.Delete(path);

            //Thread.Sleep(1000);

            int instrument = midiplayer.TheInstrumentsManager.GetInstrumentKey(cbMidiInstrument.SelectedValue.ToString());

            Midi.TsMIDICreator.SaveSolution(instrument,this.TheSolution.TheSolution, path);
            MessageBox.Show("Saved in " + path);
            ChangePath(ffd.SelectedPath);
        }



        public void ChangePath(string path)
        {
            this.gridFiles.ItemsSource = null;

            ffd.SelectedPath = path;
            this.txtPath.Text = path;

            List<FileInfo> files = new List<FileInfo>();



            files.AddRange(Directory.EnumerateFiles(ffd.SelectedPath, "*.xml").Select(x => new FileInfo(x)));
            files.AddRange(Directory.EnumerateFiles(ffd.SelectedPath, "*.mid").Select(x => new FileInfo(x)));

            this.gridFiles.ItemsSource = null;
            this.gridFiles.ItemsSource = files;
        }

        private void btnGenerateMusicXML_Click(object sender, RoutedEventArgs e)
        {
            string path = this.ffd.SelectedPath;

            if (!path.EndsWith("\\")) path += "\\";

            //if (this.TheSolution.CurrentTrainingSet != null) 
                path += this.TheSolution.CurrentTrainingSet.WaveName + ".xml";
            //else 
            //    path += "2013-12-17 19.15.42.wav.xml";

            SaveMusicXML(path);
            ChangePath(ffd.SelectedPath);
            Console.Beep(1000, 500);
        }

        private void SaveMusicXML( string path)
        {
            if (this.TheSolution == null || this.TheSolution.TheSolution == null)
            {
                MessageBox.Show("No solution is loaded.");
                return;
            }

            string title = "";

            //if (this.TheSolution.CurrentTrainingSet != null) 
                title = this.TheSolution.CurrentTrainingSet.WaveName;
            //else 
            //    title = "2013-12-17 19.15.42";

            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }catch{
                    MessageBox.Show("ERROR Cant delete the file.");
                    return;
                }
            }

            int instrument = midiplayer.TheInstrumentsManager.GetInstrumentKey(cbMidiInstrument.SelectedValue.ToString());

            TsPentagramEngine.MXMLC.MusicXMLMaker maker = new TsPentagramEngine.MXMLC.MusicXMLMaker(this.thenotes);
            maker.Autor = "Recognition System";
            maker.Title = title;
            maker.Copyright = "Undefined Copyright";

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            string tempo = cbTempo.Text;
            this.rtbResult.Selection.Text = maker.Parse(fs, center,tempo[0]-'0',tempo[2]-'0',cbOctaveChange.SelectedIndex-1);
            fs.Dispose();
        }

        private void btnDisplayMusicXML_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution == null || this.TheSolution.CurrentTrainingSet==null || this.TheSolution.TrainerFrequencies == null)
            {
                MessageBox.Show("No solution is loaded.");
                return;
            }
            string path = this.ffd.SelectedPath;

            if (!path.EndsWith("\\")) path += "\\";

            //if (this.TheSolution.CurrentTrainingSet != null) 
                path += this.TheSolution.CurrentTrainingSet.WaveName + ".xml";
            //else 
            //    path += "2013-12-17 19.15.42.wav.xml";

            SaveMusicXML(path);

            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));
            FileStream fs = new FileStream(path, FileMode.Open);
            var partwise = (MusicXML.scorepartwise)serializer.Deserialize(fs);

            WPFScoreDisplayer wsd = new WPFScoreDisplayer(partwise);
            wsd.Show();

            fs.Dispose();
        }
    }
}