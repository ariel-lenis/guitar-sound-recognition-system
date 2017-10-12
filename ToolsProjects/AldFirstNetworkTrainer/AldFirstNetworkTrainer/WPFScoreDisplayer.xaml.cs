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
using System.Windows.Shapes;
using TsPentagramEngine;

namespace AldFirstNetworkTrainer
{
    /// <summary>
    /// Interaction logic for WPFScoreDisplayer.xaml
    /// </summary>
    public partial class WPFScoreDisplayer : Window
    {
        MusicXML.scorepartwise partwise;
        public WPFScoreDisplayer(MusicXML.scorepartwise partwise)
        {
            this.partwise = partwise;
            InitializeComponent();
            this.SourceInitialized += WPFScoreDisplayer_SourceInitialized;
        }

        void WPFScoreDisplayer_SourceInitialized(object sender, EventArgs e)
        {
            this.usrDisplayer.LoadMusicXML(partwise);
        }


    }
}
 