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
using TsFilesTools;
using AldSpecialAlgorithms;
using AaBackPropagationFast;
using TsExtraControls;
using System.Linq;

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrTrainerFrequenciesStatus.xaml
	/// </summary>
	public partial class UsrTrainerFrequenciesStatus : UserControl
	{
        TsFirsStepSolution thesolution;
        TsTrainingSet thetrainingset;

        bool training = false;

        public TsFirsStepSolution TheSolution
        {
            get { return this.thesolution; }
            set
            {
                //infos = new List<Trainers.TsTrainingInfo>();
                if (this.thesolution != null)
                {
                    this.thesolution.TrainerUpdated -= thesolution_TrainerUpdated;
                    this.thesolution.TrainerFrequencies.EpochTrained -= TrainerFrequencies_EpochTrained;
                    //this.thesolution.TrainerTimes.TrainingEnded -= thetrainer_TrainingEnded;
                    //this.thesolution.TrainerTimes.TrainingEpochComplete -= thetrainer_TrainingEpochComplete;
                }
                this.thesolution = value;
                //this.thesolution.TrainerTimes.TrainingEnded += TrainerTimes_TrainingEnded;
                //this.thesolution.TrainerTimes.TrainingEpochComplete += thetrainer_TrainingEpochComplete;
                this.thesolution.TrainerUpdated += thesolution_TrainerUpdated;
                this.thesolution.TrainerFrequencies.EpochTrained += TrainerFrequencies_EpochTrained;
            }
        }

        public TsTrainingSet TheTrainingSet
        {
            get { return this.thetrainingset; }
            set { this.thetrainingset = value; }
        }
        float lasterr = 0;
        void TrainerFrequencies_EpochTrained(object who, int epoch, float err)
        {
            plotter.AddPoint(1, epoch, err);
            if(epoch>1)
                plotter.AddPoint(2, epoch, (err-lasterr)*10);
            lasterr = err;
            this.Dispatcher.Invoke(delegate
            {
                //this.Title = err + "";
            });
        }

        void thesolution_TrainerUpdated(TsFirsStepSolution who, string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork)
        {
            
        }


		public UsrTrainerFrequenciesStatus()
		{
			this.InitializeComponent();
            StartPlot();
		}

        public void LoadTrainer(TsTrainingSet currentTrainingSet, TsFirsStepSolution thesolution )
        {
            //this.thesolution = thesolution;
            //this.thesolution.TrainerFrequencies.EpochTrained += thetrainer_EpochTrained;
            //this.thesolution.TrainerFrequencies.LoadTrainingSet(currentTrainingSet);
            //StartPlot();
            //this.listRanges.ItemsSource = this.thesolution.TrainerFrequencies.ranges;
        }

        

        private void StartPlot()
        {
            plotter.ClearCurves();

            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 1, MinorMarks = 0.1 };

            plotter.AxisX = axisX;
            plotter.AxisY = axisY;
            plotter.Title = "Training";

            plotter.AddCurve("Error over Time", Colors.SkyBlue);

            plotter.AddCurve("dE over Time", Colors.Red);

            //plotter.DisableZoom();

            Extra.AxisConfig axisX2 = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 50, MinorMarks = 10 };
            Extra.AxisConfig axisY2 = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 1, MinorMarks = 0.1 };

            plotterWave.AxisX = axisX2;
            plotterWave.AxisY = axisY2;
            plotterWave.Title = "Waves";
            //plotterWave.DisableZoom();

        }

        private void gridRanges_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridRanges.SelectedIndex < 0) return;
            var res = gridRanges.SelectedValue as Trainer2Data;

            if (res == null) return;

            plotterWave.ClearCurves();
            plotterWave.AddCurve("FFT", res.peaks.allhz, res.peaks.allfft, Colors.Green);

            //plotterWave.AddCurve("Derivative", res.peaks.allhz, NxtUsage(res.peaks.allfft), Colors.Gray);

            plotterWave.AddCurve("Harm", res.peaks.allhz, res.peaks.allharm, Colors.Red);
            plotterWave.AddCurve("Harm Peaks", res.peaks.hzHarm, res.peaks.pksHarm, Colors.Blue);

            //var ydata = trainer.output[res].Select(x=>res.peaks.pksHarm[x]).ToArray();

            plotterWave.AddCurve("Points", res.peaks.hzHarm, this.thesolution.TrainerFrequencies.trainer.output[res], Colors.Black, Extra.AdaptedSymbolType.XCross);

            sliderWave.Minimum = 100;
            sliderWave.Maximum = this.thesolution.TrainerFrequencies.samplerate / 2;

            plotterWave.SetMaxX(this.thesolution.TrainerFrequencies.samplerate / 2 / 4);
            sliderWave.Value = this.thesolution.TrainerFrequencies.samplerate / 2 / 4;
        }

        public float[] HDerivative(float[] fft)
        {
            float[] res = new float[fft.Length];
            for (int i = 1; i < fft.Length; i++)
                res[i] = fft[i] - fft[i - 1];
            res[0] = res[1];
            return res;            
        }

        public float[] HArmonics(float[] fft)
        {
            float[] derivative = HDerivative(fft);
            float[] res = new float[fft.Length];
            for (int i = 0; i < fft.Length; i++)
                for (int j = 1; j<=5 && (i+1) * (j+1) < fft.Length; j++)
                    if (Math.Sign(derivative[i * j]) == Math.Sign(fft[i * (j + 1)]))
                        res[i] -= Math.Abs(fft[i*j] - fft[i * (j+1)]);
            return res;
        }

        public float[] NxtUsage(float[] fft)
        {
            float[] res = new float[fft.Length];
            int sticks = 4;
            float max = fft.Max();

            for (int i = 0; i < res.Length; i++)
                res[i] = 4*max;

            for (int i = 0; i < fft.Length / sticks; i++)
            {
                float total =0;
                for (int j = 1; j <= sticks; j++)
                    total += max-fft[(i + 1) * j - 1];
                res[i] = total;
            }

            return res;
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
            if (this.thesolution == null || this.thesolution.TrainerFrequencies == null)
            {
                MessageBox.Show("No trainer for frequencies is loaded yet...");
                return;
            }

            if (thesolution.TrainerFrequencies.ranges==null || thesolution.TrainerFrequencies.ranges.Count == 0)
            {
                MessageBox.Show("Need almost one range to train.");
                return;
            }
            if (training)
            {
                this.btnStartTraining.Content = "Start Training";
                this.thesolution.TrainerFrequencies.BeginEndTraining();
                training = false;
            }
            else
            {
                StartPlot();
                this.btnStartTraining.Content = "Stop Training";
                this.thesolution.TrainerFrequencies.BeginStartTraining();
                training = true;
            }
            SetStatus(!training);
            //SetStatus(false);
            //this.thesolution.TrainerFrequencies.BeginStartTraining();
            //this.btnStartTraining.Content = "StopTraining";
        }

        private void SetStatus(bool status)
        {
            this.gridRanges.IsEnabled = status;
            this.btnReload.IsEnabled = status;
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            if (this.thetrainingset == null)
            {
                MessageBox.Show("The training set is null.");
                return;
            }
            if (this.thesolution == null || this.thesolution.TrainerFrequencies == null)
            {
                MessageBox.Show("The trainer is not loaded.");
                return;
            }

            this.thesolution.TrainerFrequencies.LoadTrainingSet(this.thetrainingset);
            this.gridRanges.ItemsSource = null;
            this.gridRanges.AutoGenerateColumns = true;
            this.gridRanges.ItemsSource = this.thesolution.TrainerFrequencies.ranges;
        }

        private void gridRanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

	}
}