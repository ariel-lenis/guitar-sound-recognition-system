using System;
using System.Collections.Generic;
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
using System.Xml.Serialization;

namespace Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.PropertyGrid properties;
        public MainWindow()
        {
            InitializeComponent();
            properties = new System.Windows.Forms.PropertyGrid();
            //host.Child = properties;
            this.SourceInitialized += MainWindow_SourceInitialized;
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MusicXML.dynamics c = new MusicXML.dynamics();
            MusicXML.link l = new MusicXML.link();
            MusicXML.root r = new MusicXML.root();
            MusicXML.sound s = new MusicXML.sound();
            MusicXML.@string ss = new MusicXML.@string();

            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));

            FileStream fs = new FileStream(@"C:\Users\nxt\Desktop\test.xml",FileMode.Open);

            //FileStream fs = new FileStream(@"C:\Users\nxt\Desktop\simpleplan.xml", FileMode.Open);
            
            FileStream fs2 = new FileStream(@"C:\Users\nxt\Desktop\test2.xml",FileMode.OpenOrCreate);

            MusicXML.scorepartwise res = (MusicXML.scorepartwise)serializer.Deserialize(fs);

            properties.SelectedObject = res;

            res.part[0].id = "P1111";

            var measure = res.part[0].measure[0];

            //properties.SelectedObject = measure;

            MusicXML.note note = new MusicXML.note();
            MusicXML.pitch pitch = new MusicXML.pitch() { octave="1", step= MusicXML.step.A };
            note.Items = new object[] { pitch, (decimal)1 };
            note.ItemsElementName = new MusicXML.ItemsChoiceType1[] { MusicXML.ItemsChoiceType1.pitch, MusicXML.ItemsChoiceType1.duration };
            note.voice = "1";
            note.type = new MusicXML.notetype() { Value = MusicXML.notetypevalue.quarter };
            note.stem = new MusicXML.stem() { Value = MusicXML.stemvalue.down };

            MusicXML.notations notation = new MusicXML.notations();
            MusicXML.technical tnotation = new MusicXML.technical();

            tnotation.Items = new object[] { new MusicXML.@string(){ Value="2"} ,  new MusicXML.fret(){ Value="1"} };
            tnotation.ItemsElementName = new MusicXML.ItemsChoiceType3[] { MusicXML.ItemsChoiceType3.@string , MusicXML.ItemsChoiceType3.fret };


            notation.Items = new object[] { tnotation };


            note.notations = new MusicXML.notations[] { notation };

            measure.Items = AddToObject(measure.Items, note);

            //note.

            
            //note.
            //res.part[0].measure[0].Items
            

            serializer.Serialize(fs2, res);

            fs.Close();
            fs2.Close();

        }
        public object[] AddToObject(object[] from, object element)
        {
            object[] res = new object[from.Length+1];
            for (int i = 0; i < from.Length; i++)
                res[i] = from[i];
            res[from.Length] = element;
            return res;
        }
    }
}
