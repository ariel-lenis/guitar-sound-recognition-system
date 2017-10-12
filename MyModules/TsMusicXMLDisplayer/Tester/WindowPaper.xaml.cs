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
using System.Windows.Shapes;
using System.Xml.Serialization;
using TsPentagramEngine;

namespace Tester
{
    /// <summary>
    /// Interaction logic for WindowPaper.xaml
    /// </summary>
    public partial class WindowPaper : Window
    {
        public WindowPaper()
        {
            InitializeComponent();
            this.SourceInitialized += WindowPaper_SourceInitialized;
        }

        void WindowPaper_SourceInitialized(object sender, EventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));

            //FileStream fs = new FileStream(@"C:\Users\Nachobertinho\Desktop\testtesttest.xml", FileMode.Open);

            //FileStream fs = new FileStream(@"C:\Users\Nachobertinho\Desktop\hope alter.xml", FileMode.Open);

            //FileStream fs = new FileStream(@"C:\Users\Nachobertinho\Desktop\2013-12-17 19.15.42.wav.xml", FileMode.Open);

            FileStream fs = new FileStream(@"I:\Tesis P1\Tesis\Tools\ToolsProjects\AldFirstNetworkTrainer\AldFirstNetworkTrainer\bin\Debug\Solutions\2013-12-17 19.15.42.wav.xml", FileMode.Open);



            //FileStream fs = new FileStream(@"C:\Users\Nachobertinho\Desktop\Untitled1.xml", FileMode.Open);

            var res = (MusicXML.scorepartwise)serializer.Deserialize(fs);
            fs.Dispose();

            this.usrDisplayerPaper.LoadMusicXML(res);

        }
    }
}
