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
using TsExtraControls;
using System.Linq;

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrTrainersStatus.xaml
	/// </summary>
	public partial class UsrTrainersStatus : UserControl
	{
        AldPlotterPoints plotter;
        TsFirsStepSolution thesolution;
        List<Trainers.TsTrainingInfo> infos;
        bool prepared;

		public UsrTrainersStatus()
		{
			this.InitializeComponent();
            plotter = new AldPlotterPoints();

            Extra.AxisConfig axisX = new Extra.AxisConfig() { AxisTitle = "Epoch's", Mayormarks = 5, MinorMarks = 1 };
            Extra.AxisConfig axisY = new Extra.AxisConfig() { AxisTitle = "Error", Mayormarks = 0.25, MinorMarks = 0.1 };
            plotter.AxisX = axisX;
            plotter.AxisY = axisY;
            plotter.Title = "Learning Curve (Error).";
            plotter.AddCurve("Error", new double[] { 0 }, new double[] { 0 }, System.Drawing.Color.Green, Extra.AdaptedSymbolType.Circle);

            this.windowsHost.Child = plotter;						
		}

        void UsrTrainersStatus_Initialized(object sender, EventArgs e)
        {

        }


        bool thetrainer_TrainingEpochComplete(TrainerAllTimes all, string name, int idx, float x, float y)
        {
            this.Dispatcher.Invoke(delegate
            {
                //this.Title = y.ToString();
                plotter.AddPoint(idx + 1, this.TheSolution.TheDispatcher.GetInformation(name).Information.Epochs /*x*/, y + 0*idx * 0.0125);
                if (dataGridTrainingStatus.ItemsSource==null)
                {
                    infos = all.GetInfos();
                    this.UpdateInfos(infos);
                }
            });
            //if (x == 250) 
            //   return false;
            return true;
        }

        void thetrainer_TrainingEnded(TrainerAllTimes all, string name, int idx)
        {
            if (all.IsTraining) return;

            this.Dispatcher.Invoke(delegate
            {
                //menuStartTraining.Header = "Start Training";
                this.IsEnabled = true;
            });
        }
        public TsFirsStepSolution TheSolution 
        {
            get { return this.thesolution; }
            set 
            {
                infos = new List<Trainers.TsTrainingInfo>();
                if (this.thesolution != null)
                {
                    this.thesolution.TrainerUpdated -= thesolution_TrainerUpdated;
                    this.thesolution.TrainerTimes.TrainingEnded -= thetrainer_TrainingEnded;
                    this.thesolution.TrainerTimes.TrainingEpochComplete -= thetrainer_TrainingEpochComplete;                    
                }
                this.thesolution = value;
                this.thesolution.TrainerTimes.TrainingEnded += thetrainer_TrainingEnded;
                this.thesolution.TrainerTimes.TrainingEpochComplete += thetrainer_TrainingEpochComplete;
                this.thesolution.TrainerUpdated += thesolution_TrainerUpdated;
            }
        }

        void thesolution_TrainerUpdated(TsFirsStepSolution who, string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork)
        {
            //if (type == typeof(TrainerAllTimes))
                ReloadInfos();

        }

        private void ReloadInfos()
        {
            this.plotter.ClearCurves();
            infos = this.thesolution.TrainerTimes.GetInfos();
            this.UpdateInfos(infos);
        }

        //public int BugCasesMultiplication { get; set; }

        private void StartStopTraining(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var info = btn.DataContext as Trainers.TsTrainingInfo;
            int idx = dataGridTrainingStatus.Items.IndexOf(info);

            if (!prepared)
                PrepareStart();
            int bugcasesmultiplication = int.Parse(txtBugCount.Text);
            this.thesolution.TrainerTimes.Swap(info.Network, bugcasesmultiplication);
        }


        internal void UpdateInfos(List< Trainers.TsTrainingInfo> infos)
        {
            this.dataGridTrainingStatus.ItemsSource = infos;
        }

        public void PrepareStart()
        {
            plotter.ClearCurves();
            for (int i = 0; i < this.thesolution.TrainerTimes.Trainers.Count; i++)
            {
                var pair = this.thesolution.TrainerTimes.Trainers.ElementAt(i);
                plotter.AddCurve(pair.Key, TsColors.CommonDColors[i]);
            }
            prepared = true;
        }
    }
}