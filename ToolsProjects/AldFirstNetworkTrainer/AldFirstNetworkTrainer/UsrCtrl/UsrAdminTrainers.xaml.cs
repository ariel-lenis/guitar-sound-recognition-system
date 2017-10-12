using System;
using System.Collections.Generic;
using System.IO;
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
using System.Runtime.Serialization.Formatters.Binary;

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrAdminTrainers.xaml
	/// </summary>
	public partial class UsrAdminTrainers : UserControl
	{
        public class TrainersDirectory
        {
            public string path;
            public string Name { get; set; }
            public int Networks { get; set; }

            public TrainersDirectory(string path,int networks)
            {
                FileInfo info = new FileInfo(path);
                this.path = path;
                this.Name = info.Name;
                this.Networks = networks;
            }
        }



        TsExtraControls.FileFolderDialog ffd = new FileFolderDialog();
        List<Networks.TsNetworkBackup> infos;

        public TsFirsStepSolution TheSolution { get; set; }
        //AldPlotterPoints plotter;
		public UsrAdminTrainers()
		{
			this.InitializeComponent();
            this.Initialized += UsrAdminTrainers_Initialized;
		}

        void UsrAdminTrainers_Initialized(object sender, EventArgs e)
        {
            //this.plotter = new AldPlotterPoints();
            
        }
        public void ChangePath(string path)
        {
            //if (txtPaths.Text == "") txtPaths.Text = path;
            //else txtPaths.Text += ";" + path;
            //txtPaths.CaretIndex = txtPaths.Text.Length;

            this.gridFolders.ItemsSource = null;
            this.gridTrainerNetworks.ItemsSource = null;

            ffd.SelectedPath = path;

            List<TrainersDirectory> td = new List<TrainersDirectory>();

            foreach(var ipath in Directory.GetDirectories(path))
            {
                FileInfo snetwork = Directory.EnumerateDirectories(ipath).Select(x=>new FileInfo(x)).FirstOrDefault(x => x.Name.Equals("Networks", StringComparison.CurrentCultureIgnoreCase));
                if (snetwork == null) continue;
                int count = Directory.EnumerateFiles(snetwork.FullName,"*.ann").Count();
                td.Add(new TrainersDirectory(ipath, count));
            }
            this.gridFolders.ItemsSource = td;

            this.txtPaths.Text = path;
        }

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution==null || this.TheSolution.TrainerTimes == null)
            {
                MessageBox.Show("Error in TrainerAdmin, no trainer is setted.");
                return;
            }

            if (this.TheSolution.IsTraining)
            {
                MessageBox.Show("A network is already training.");
                return;
            }

            var d = DateTime.Now;

            txtNewName.Text = d.Day + "_" + d.Month + "_" + d.Year + " " + d.Hour + "_" + d.Minute + "_" + d.Second;
            /*
            Networks.IGeneralizedNetwork lpcstenetwork = new Networks.TsCudaNetwork();
            lpcstenetwork.Create(new int[] { 128, 256,64, 32, 1 }, 0.025f, 0.05f);
            //this.TheSolution.TrainerTimes.SetNetwork("lpcste", lpcstenetwork);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "lpcste", lpcstenetwork);
            */

            Networks.IGeneralizedNetwork thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 128, 256, 64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "ste", thenetwork);

            thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[]{ 128, 256,64, 32, 1}, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "amd", thenetwork);

            thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 128, 256, 64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "lpcamd", thenetwork);

            thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 128, 256,64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "lpcste", thenetwork);

            Networks.IGeneralizedNetwork ax = new Networks.TsCudaNetwork();
            ax.Create(new int[] { 1024 * 2, 1024,256, 64, 1 }, 0.05f, 0.005f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "spec cuda", ax);


            Networks.IGeneralizedNetwork fnetwork = new Networks.TsFastNetwork();
            fnetwork.Create(new int[] { 50, 100, 20, 1 }, 0.02f, 0.05f);

            //fnetwork.Create(new int[] { 50, 400,200,100, 20, 1 }, 0.02f, 0.05f);

            //this.TheSolution.TrainerFrequencies = new TsAdminAllFrequencies();
            //this.TheSolution.TrainerFrequencies.TheNetwork = fnetwork;

            this.TheSolution.TheDispatcher.SetNetwork(TsAdminAllFrequencies.Namespace, "F1", fnetwork);

            //this.TheTrainer.SetNetwork("spec cuda 2", bx);

            ShowTrainers();
        }

        private void ShowTrainers()
        {
            var infos = this.TheSolution.TheDispatcher.GetInfos();
            this.dataGridTrainingStatus.ItemsSource = infos;
        }

        private void dataGridTrainingStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void dataGridTrainingStatus_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void dataGridTrainingStatus_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell == null) return;
            var parent = VisualTreeHelper.GetParent(targetCell);

            while(parent != null && parent.GetType() != typeof(DataGridRow))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            
            

            DataGridRow row = parent as DataGridRow;

            dataGridTrainingStatus.SelectedIndex = row.GetIndex();
            row.Focus();
            MessageBox.Show("xD");

            dataGridTrainingStatus.SelectedIndex = row.GetIndex();
            row.Focus();

        }

        private void dataGridTrainingStatus_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
            if (this.dataGridTrainingStatus.SelectedIndex < 0) return;
            var item = this.dataGridTrainingStatus.SelectedItem;
            MessageBox.Show(item.ToString());
             * */
            
        }

        private void Menu_Reset_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Trainers.TsTrainingInfo info = item.DataContext as Trainers.TsTrainingInfo;
            string name = info.Network;

            if (this.TheSolution.TheDispatcher.GetStatus(info.Network) != Trainers.TsNetworksDispatcher.ETrainerStatus.Stopped)
            {
                MessageBox.Show("Already training...");
                return;
            }

            Networks.IGeneralizedNetwork currentnetwork = this.TheSolution.TheDispatcher.GetNetwork(info.Network);

            
            Networks.IGeneralizedNetwork newnetwork;

            if(currentnetwork is Networks.TsFastNetwork)
                newnetwork = new Networks.TsFastNetwork();
            else
                newnetwork = new Networks.TsCudaNetwork();    
                    
            newnetwork.Create(currentnetwork.NeuronsPerLayer,currentnetwork.Alpha,currentnetwork.LearningRate);

            this.TheSolution.TheDispatcher.SetNetwork(info.Namespace, info.Network, newnetwork);
            Console.Beep(1000, 200);
        }

        private void Menu_Backup_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Trainers.TsTrainingInfo info = item.DataContext as Trainers.TsTrainingInfo;
            MessageBox.Show(item.Header + "->" + info.Network);
        }

        private void Menu_Restore_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Trainers.TsTrainingInfo info = item.DataContext as Trainers.TsTrainingInfo;
            MessageBox.Show(item.Header + "->" + info.Network);
        }

        private void btnChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ffd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if(!Directory.Exists(ffd.SelectedPath))
            {
                MessageBox.Show("Error, the directory donst exists.");
                return;
            }
            ChangePath(ffd.SelectedPath);
        }

        private void gridFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.gridFolders.SelectedIndex < 0) return;
            LoadFolderFromIndex(this.gridFolders.SelectedIndex);

        }

        private void LoadFolderFromIndex(int idx)
        {
            this.gridTrainerNetworks.ItemsSource = null;

            string path = (this.gridFolders.Items[idx] as TrainersDirectory).path;
            if (!path.EndsWith("\\")) path += "\\";
            path += "Networks\\";

            LoadBackupInfos(path);

        }

        private void LoadBackupInfos(string path)
        {
            infos = new List<Networks.TsNetworkBackup>();

            foreach (var ipath in Directory.EnumerateFiles(path, "*.ann"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(ipath, FileMode.Open);
                Networks.TsNetworkBackup info = (Networks.TsNetworkBackup)bf.Deserialize(fs);
                fs.Dispose();
                infos.Add(info);
            }
            this.gridTrainerNetworks.ItemsSource = infos;
        }

        private void btnSaveCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution.TrainerTimes.IsTraining)
            {
                MessageBox.Show("Cant in the middle of a training.");
                return;
            }

            string path = ffd.SelectedPath;
            if (!path.EndsWith("\\")) path += "\\";

            path += this.txtNewName.Text;

            this.TheSolution.SaveNetworks(ffd.SelectedPath, this.txtNewName.Text);
            ChangePath(ffd.SelectedPath);
        }
        /*
        private void btnLoadSelected_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution == null || this.TheSolution.TrainerTimes == null)
            {
                MessageBox.Show("Error in TrainerAdmin, no trainer is setted.");
                return;
            }

            if (this.TheSolution.IsTraining)
            {
                MessageBox.Show("A network is already training.");
                return;
            }

            Networks.IGeneralizedNetwork thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 256, 512, 64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "ste", thenetwork);

            thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 256, 512, 64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "amd", thenetwork);

            thenetwork = new Networks.TsFastNetwork();
            thenetwork.Create(new int[] { 256, 512, 64, 32, 1 }, 0.025f, 0.05f);
            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "lpcste", thenetwork);


            Networks.IGeneralizedNetwork ax = new Networks.TsCudaNetwork();
            Networks.IGeneralizedNetwork bx = new Networks.TsCudaNetwork();
            ax.Create(new int[] { 1024 * 2, 1024, 256, 64, 1 }, 0.05f, 0.01f);
            //bx.Create(new int[] { 1024 * 2, 1024, 256, 64, 1 }, 0.05f, 0.1f);

            this.TheSolution.TheDispatcher.SetNetwork(TrainerAllTimes.Namespace, "spec cuda", ax);
            //this.TheTrainer.SetNetwork("spec cuda 2", bx);

            ShowTrainers();
        }
        */
        private void btnLoadNetworksInGrid_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            TrainersDirectory directory = btn.DataContext as TrainersDirectory;

            this.txtNewName.Text = directory.Name;

            DependencyObject cell = VisualTreeHelper.GetParent(btn);
            
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell == null) return;
            var parent = VisualTreeHelper.GetParent(targetCell);

            while (parent != null && parent.GetType() != typeof(DataGridRow))
                parent = VisualTreeHelper.GetParent(parent);
            
            DataGridRow row = parent as DataGridRow;
            
            LoadFolderFromIndex(row.GetIndex());

            foreach (var ibackup in infos)
                LoadBackup(ibackup);
        }

        private void btnLoadJustThisInGrid_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Networks.TsNetworkBackup backup = btn.DataContext as Networks.TsNetworkBackup;
            LoadBackup(backup);
        }

        private void LoadBackup(Networks.TsNetworkBackup backup)
        {
            Networks.IGeneralizedNetwork network;
            if (backup.NetworkType == typeof(Networks.TsCudaNetwork))
                network = new Networks.TsCudaNetwork();
            else
                network = new Networks.TsFastNetwork();
            network.Restore(backup);
            
            this.TheSolution.TheDispatcher.SetNetwork(backup.Namespace,backup.Key, network);

            ShowTrainers();
        }

        private void btnSaveCurrentAs_Click(object sender, RoutedEventArgs e)
        {
            if (this.TheSolution.TrainerTimes.IsTraining)
            {
                MessageBox.Show("Cant in the middle of a training.");
                return;
            }

            if (ffd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string path = ffd.SelectedPath;
            if (!path.EndsWith("\\")) path += "\\";

            path += this.txtNewName.Text;

            if (Directory.Exists(path))
            {
                if (MessageBox.Show("Rewrite the current files???", "Question", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }

            //this.TheSolution.TrainerTimes.SaveNetworks(ffd.SelectedPath, this.txtNewName.Text);

            this.TheSolution.SaveNetworks(ffd.SelectedPath, this.txtNewName.Text);

            ChangePath(ffd.SelectedPath);
        }

	}
}