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
using Sanford.Multimedia.Midi;
using TsPentagramEngine;
using AldSpecialAlgorithms;
using System.Diagnostics;
using TsFilesTools;
using System.Xml.Serialization;
using System.IO;
using TsExtraControls;

namespace TsPentagramToolsTest
{
    public partial class MainWindow : Window
    {
        List<float> distances = new List<float>();
        List<InputFormat> data;
        float center;
        FileFolderDialog ffdialog = new FileFolderDialog();
        System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog() { Filter = "MusicXML Files|.xml" };

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            //ffdialog.
            //string path = ffdialog.SelectedPath;
            //LoadDirectory(path);
        }

        private void btnAplicar_Click(object sender, RoutedEventArgs e)
        {      
            Sequence sequence = new Sequence(@"C:\Users\Nachobertinho\Desktop\2013-12-17 19.15.42.wav.mid");
            //Sequence sequence = new Sequence(@"C:\Users\Nachobertinho\Desktop\Aguado_Op8_No3_Vals.mid");
            //Sequence sequence = new Sequence(@"C:\Users\Nachobertinho\Desktop\Weiss_Suite_No19_Prelude.mid");       

            data = MidiToInputFormat.Parse(sequence);
            visualizador.Notes = data;

            DisplayData(data);
            
        }
        
        private void DisplayData(List<InputFormat> data)
        {            
            List<float> altures = new List<float>();

            distances.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = i+1; j<i+2 &&  j < data.Count; j++)
                {
                    altures.Add(1);
                    distances.Add((float)(data[j].StartNote.TotalSeconds - data[i].StartNote.TotalSeconds));
                }
            }

            distances = distances.Where(x => x > 0).ToList();
            distances.Sort();

            WinBubble win = new WinBubble(distances);
            win.Show();
            
            float[] y = Vectors.LinSpace(distances.Count, 5);

            plotterPoints.AddCurve("Distances", distances,y, Colors.Blue, TsExtraControls.Extra.AdaptedSymbolType.Circle);

            this.sliderPercent.Value = 0.04;

        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (distances.Count == 0) return;            

            float val = (float)this.sliderPercent.Value;

            this.Title = val.ToString();

            var histogram = AldSpecialAlgorithms.AldAlgorithms.CreateOverlappedHistogram(distances.ToArray(), 100, val /*0.008f*/);

            plotterPoints2.ClearCurves();
            plotterPoints2.AddCurve("Histogram", histogram.c, histogram.h, Colors.Green, TsExtraControls.Extra.AdaptedSymbolType.Star);

            int maxindex = histogram.MaxIndex();
            center = histogram.c[maxindex];
        }
        /*
        private void btnRegresion_Copy_Click(object sender, RoutedEventArgs e)
        {
            MusicXML.scorepartwise partwise = new MusicXML.scorepartwise();
            MusicXML.scorepartwisePart part = new MusicXML.scorepartwisePart();
            partwise.part = new MusicXML.scorepartwisePart[] { part };
            part.id="P0";

            List<MusicXML.scorepartwisePartMeasure> measures = new List<MusicXML.scorepartwisePartMeasure>();

            MusicXML.time time = new MusicXML.time();
            time.Items = new object[]{"4","4"};
            time.ItemsElementName = new MusicXML.ItemsChoiceType9[] { MusicXML.ItemsChoiceType9.beats, MusicXML.ItemsChoiceType9.beattype };
            

            MusicXML.clef clef = new MusicXML.clef();
            clef.line = "2";
            clef.sign = MusicXML.clefsign.G;
            clef.clefoctavechange = "0";

            List<WithDuration> elements = new List<WithDuration>();

            foreach (var idata in data)
            {
                var lres = ConversionTools.DetermineBestEquivalence(center, NoteTime.EFigures.Negra, (float)idata.Duration.TotalSeconds);

                if (lres.Count == 0)
                {
                    Console.WriteLine("0-> In Conversion...");
                    continue;
                }

                Note note = new Note();
                note.Duration = lres[0];

                foreach (var im in idata.MidiNotes)
                {
                    int octave = TsMIDITools.OctaveBase4(im);
                    string snote = TsMIDITools.NoteFor(im);
                    note.Chords.Add(NoteData.CreateFromString(snote,octave));
                }

                elements.Add(note);

                for (int i = 1; i < lres.Count; i++)
                {
                    Silence silence = new Silence();
                    silence.Duration = lres[i];
                    elements.Add(silence);
                }                
            }

            //MessageBox.Show(elements.Count+"");

            foreach (var i in elements)
                Console.WriteLine(i);

            MusicXML.scorepartwisePartMeasure measure=null;

            double currentacum = 0;

            int timetime = 128;

            List<object> notes=null;

            bool first = true;

            MusicXML.direction direcction = new MusicXML.direction();
            MusicXML.directiontype direcctiontype = new MusicXML.directiontype();
            MusicXML.metronome metronome = new MusicXML.metronome();

            direcctiontype.Items = new object[] { metronome };
            direcctiontype.ItemsElementName = new MusicXML.ItemsChoiceType7[] { MusicXML.ItemsChoiceType7.metronome };

            MusicXML.perminute ppm = new MusicXML.perminute();
            ppm.Value = (int)(60.0f/center)+"";
            metronome.Items = new object[] { MusicXML.notetypevalue.quarter, ppm };
            direcction.directiontype = new MusicXML.directiontype[] { direcctiontype };

            for (int i = 0; i < elements.Count; i++)
            {
                if(measure==null)
                {
                    notes = new List<object>();
                    measure = new MusicXML.scorepartwisePartMeasure();
                    measure.number = measures.Count+"";
                    if(first)
                    { 
                        MusicXML.attributes attributes = new MusicXML.attributes();
                        attributes.clef = new MusicXML.clef[] { clef };
                        attributes.time = new MusicXML.time[] { time };
                        attributes.divisions = timetime;
                        attributes.divisionsSpecified = true;
                        notes.Add(attributes);
                        notes.Add(direcction);
                        first = false;
                    }
                    //measure.Items = new object[] { attributes };
                }

                var element = elements[i];
                float value = element.Duration.Value;

                
                Dictionary<MusicXML.ItemsChoiceType1,object> dnote;
                if(element is Note)
                {
                    Note enote = element as Note;
                    for(int j=0;j<enote.Chords.Count;j++)
                    {
                        dnote = new Dictionary<MusicXML.ItemsChoiceType1,object>();

                        if (j > 0)
                            dnote.Add(MusicXML.ItemsChoiceType1.chord, new MusicXML.empty());

                        MusicXML.pitch pitch = new MusicXML.pitch();
                        dnote.Add(MusicXML.ItemsChoiceType1.pitch, pitch);

                        MusicXML.note note = new MusicXML.note();

                        dnote.Add(MusicXML.ItemsChoiceType1.duration, (decimal)(value * timetime));
                        if (enote.Chords[j].Alter != NoteData.EAlters.Normal)
                            pitch.alter = (int)enote.Chords[j].Alter;

                        pitch.octave = enote.Chords[j].Octave+"";
                        pitch.step = FindStep(enote.Chords[j].Note);
                        
                        

                        note.Items = dnote.Values.Select(x => (object)x).ToArray();
                        note.ItemsElementName = dnote.Keys.ToArray();
                        notes.Add(note);
                    }                    
                }
                else
                {
                    
                    dnote = new Dictionary<MusicXML.ItemsChoiceType1, object>();
                    MusicXML.note note = new MusicXML.note();

                    dnote.Add(MusicXML.ItemsChoiceType1.rest, new MusicXML.displaystepoctave() { displayoctave = "4", displaystep = MusicXML.step.B });
                    dnote.Add(MusicXML.ItemsChoiceType1.duration, (decimal)(value * timetime));
                    //dnote.Add(MusicXML.ItemsChoiceType1., new MusicXML.empty());

                    //MusicXML.

                    note.Items = dnote.Values.Select(x => (object)x).ToArray();
                    note.ItemsElementName = dnote.Keys.ToArray();
                    notes.Add(note);
                    
                }

                currentacum += value;
                

                if (currentacum >= 4)
                {
                    Console.WriteLine("Correct:" + (currentacum-4));
                    measure.Items = notes.Select(x=>(object)x).ToArray();
                    measures.Add(measure);
                    currentacum = 0;
                    measure = null;
                }
            }

            part.measure = measures.ToArray();

            //partwise.partlist.
            MusicXML.scorepart spart = new MusicXML.scorepart();
            spart.id = "P0";
            spart.partname= new MusicXML.partname(){ Value="Hope PartName"};
            spart.partabbreviation = new MusicXML.partname(){ Value="H.A."};
            //spart.scoreinstrument = new MusicXML.scoreinstrument(){ id=""};

            MusicXML.partlist plist = new MusicXML.partlist();
            plist.Items = new object[] { spart };



            partwise.partlist = plist;
            
            string path = @"C:\Users\Nachobertinho\Desktop\hope.xml";
            
            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));
            FileStream fs = new FileStream(path, FileMode.Truncate);

            XmlSerializerNamespaces nms = new XmlSerializerNamespaces();
            nms.Add("", "");

            serializer.Serialize(fs,partwise,nms);
            fs.Close();

            string doctype = "<!DOCTYPE score-partwise PUBLIC \"-//Recordare//DTD MusicXML 2.0 Partwise//EN\" \"http://www.musicxml.org/dtds/partwise.dtd\">";


            var lines = File.ReadAllLines(path).ToList();
            lines.Insert(1, doctype);
            File.WriteAllLines(path, lines.ToArray());


            MessageBox.Show(measures.Count + "");
        }
        
        MusicXML.step FindStep(NoteData.ENote note)
        {
            switch (note)
            { 
                case NoteData.ENote.C:
                    return MusicXML.step.C;
                case NoteData.ENote.D:
                    return MusicXML.step.D;
                case NoteData.ENote.E:
                    return MusicXML.step.E;
                case NoteData.ENote.F:
                    return MusicXML.step.F;
                case NoteData.ENote.G:
                    return MusicXML.step.G;
                case NoteData.ENote.A:
                    return MusicXML.step.A;
                default:
                    return MusicXML.step.B;
            }
        }
        */
        List<FileInfo> files = new List<FileInfo>();
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (ffdialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            string path = ffdialog.SelectedPath;
            LoadDirectory(path);
        }

        private void LoadDirectory(string path)
        {
            files = Directory.EnumerateFiles(path, "*.mid*").Select(x => new FileInfo(x)).ToList();
            listFiles.ItemsSource = files;
            listFiles.DisplayMemberPath = "Name";
        }

        private void listFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listFiles.SelectedIndex < 0) return;

            string path = (listFiles.SelectedValue as FileInfo).FullName;

            if(e.ChangedButton== MouseButton.Right)
            {
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            }
            else
            { 
                Sequence sequence = new Sequence(path);
                data = MidiToInputFormat.Parse(sequence);
                visualizador.Notes = data;
                DisplayData(data);
            }
        }

        private void btnGenerateMusicXML_Click(object sender, RoutedEventArgs e)
        {
            TsPentagramEngine.MXMLC.MusicXMLMaker maker = new TsPentagramEngine.MXMLC.MusicXMLMaker(data);
            if(sfd.ShowDialog()!= System.Windows.Forms.DialogResult.OK)
                return;

            FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
            maker.Parse(fs, center);

            fs.Dispose();

            Console.Beep(1000, 500);
        }
    }
}
