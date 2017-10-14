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

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrNeuralNetworksControl.xaml
	/// </summary>
	public partial class UsrNeuralNetworksControl : UserControl
	{
        class TrainerItem
        {
            public bool Use { get; set; }
            public string Trainer { get; set; }
            public string Status { get; set; }
            public Brush TheColor { get; set; }
        }

        public delegate void DUsrNeuralNetworksControlAction(UsrNeuralNetworksControl who, string action);
        public event DUsrNeuralNetworksControlAction UsrNeuralNetworksControlAction;

		public UsrNeuralNetworksControl()
		{
			this.InitializeComponent();
		}

        internal void LoadNetworks(TrainerAllTimes trainerAll)
        {
            /*
            gridTrainers.AutoGenerateColumns = false;
            List<TrainerItem> items = new List<TrainerItem>();
            for (int i = 0; i < trainerAll.Networks.Count; i++)
            {
                TrainerItem item = new TrainerItem() { Use = false, Trainer = trainerAll.Networks.ElementAt(i).Key, TheColor = new SolidColorBrush(TsColors.CommonColors[i]), Status = "Loaded" };
                items.Add(item);
            }
            gridTrainers.IsReadOnly = true;
            gridTrainers.ItemsSource = items;
             * */
        }

        private void gridTrainers_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void startTraining_Click(object sender, RoutedEventArgs e)
        {
            if (UsrNeuralNetworksControlAction != null)
                UsrNeuralNetworksControlAction(this, "StartTraining");
        }

        private void stopTraining_Click(object sender, RoutedEventArgs e)
        {
            if (UsrNeuralNetworksControlAction != null)
                UsrNeuralNetworksControlAction(this, "StopTraining");
        }

        private void loadNetworks_Click(object sender, RoutedEventArgs e)
        {
            if (UsrNeuralNetworksControlAction != null)
                UsrNeuralNetworksControlAction(this, "LoadNetworks");
        }

        private void gridTrainers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void resetNetworks_Click(object sender, RoutedEventArgs e)
        {
            if (UsrNeuralNetworksControlAction != null)
                UsrNeuralNetworksControlAction(this, "ResetNetworks");
        }
    }
}