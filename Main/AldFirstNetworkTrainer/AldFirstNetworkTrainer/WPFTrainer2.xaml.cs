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
using TsFilesTools;
using AldSpecialAlgorithms;
using AaBackPropagationFast;
using TsExtraControls;

namespace AldFirstNetworkTrainer
{
    /// <summary>
    /// Interaction logic for WPFTrainer2.xaml
    /// </summary>
    public partial class WPFTrainer2 : Window
    {
        TsFirsStepSolution thesolution;

        public WPFTrainer2(TsTrainingSet currentTrainingSet, TsFirsStepSolution thesolution )
        {
            InitializeComponent();

            this.thesolution = thesolution;

            this.thesolution.TrainerFrequencies.EpochTrained += thetrainer_EpochTrained;

            this.thesolution.TrainerFrequencies.LoadTrainingSet(currentTrainingSet);

            StartPlot();

            this.listRanges.ItemsSource = this.thesolution.TrainerFrequencies.ranges;
        }

        void thetrainer_EpochTrained(object who, int epoch, float err)
        {
            plotter.AddPoint(1, epoch, err);
            this.Dispatcher.Invoke(delegate
            {
                this.Title = err + "";
            });
        }
        private void StartPlot()
        {
            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 1, MinorMarks = 0.1 };

            plotter.AxisX = axisX;
            plotter.AxisY = axisY;
            plotter.Title = "Training";

            plotter.AddCurve("Error over Time", Colors.SkyBlue);

            //plotter.DisableZoom();

            Extra.AxisConfig axisX2 = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY2 = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 1, MinorMarks = 0.1 };

            plotterWave.AxisX = axisX2;
            plotterWave.AxisY = axisY2;
            plotterWave.Title = "Waves";
            //plotterWave.DisableZoom();

        }

        private void listRanges_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listRanges.SelectedIndex < 0) return;
            var res = listRanges.SelectedValue as Trainer2Data;

            plotterWave.ClearCurves();
            plotterWave.AddCurve("FFT", res.peaks.allhz, res.peaks.allfft, Colors.Green);
            plotterWave.AddCurve("Harm", res.peaks.allhz, res.peaks.allharm, Colors.Red);
            plotterWave.AddCurve("Harm Peaks", res.peaks.hzHarm, res.peaks.pksHarm, Colors.Blue);



            //var ydata = trainer.output[res].Select(x=>res.peaks.pksHarm[x]).ToArray();

            plotterWave.AddCurve("Points", res.peaks.hzHarm, this.thesolution.TrainerFrequencies.trainer.output[res], Colors.Black, Extra.AdaptedSymbolType.XCross);


            sliderWave.Minimum = 100;
            sliderWave.Maximum = this.thesolution.TrainerFrequencies.samplerate / 2;

            plotterWave.SetMaxX(this.thesolution.TrainerFrequencies.samplerate / 2 / 4);
            sliderWave.Value = this.thesolution.TrainerFrequencies.samplerate / 2 / 4;

        }

        private void sliderWave_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            plotterWave.SetMaxX(e.NewValue);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.thesolution.TrainerFrequencies.BeginEndTraining();
        }

        private void btnStartTraining_Click(object sender, RoutedEventArgs e)
        {
            this.thesolution.TrainerFrequencies.BeginStartTraining();
        }
    }
}
