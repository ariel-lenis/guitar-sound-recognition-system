using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AldFirstNetworkTrainer.Trainers
{
    [Serializable]
    public class TsTrainingInfo : INotifyPropertyChanged
    {
        //public enum ETrainerStatus { Stopped, Training, Stopping };

        Color _thecolor;
        string _namespace;
        string _network;
        float _averagetime;
        int _epochs;
        int _trainsperepoch;
        TsNetworksDispatcher.ETrainerStatus _status;

        public Color TheColor { get { return _thecolor; } set { this._thecolor = value; /*this._thebrush = new SolidColorBrush(value);*/ Changed("TheColor"); } }
        public SolidColorBrush TheBrush { get { return new SolidColorBrush(_thecolor); } }

        public string Namespace { get { return _namespace; } set { this._namespace = value; Changed("Namespace"); } }

        public string Network { get { return _network; } set { this._network = value; Changed("Network"); } }
        public float AverageTime { get { return _averagetime; } set { this._averagetime = value; Changed("AverageTime"); } }
        public int Epochs { get { return _epochs; } set { this._epochs = value; Changed("Epochs"); } }
        public int TrainsPerEpoch { get { return _trainsperepoch; } set { this._trainsperepoch = value; Changed("TrainsPerEpoch"); } }

        public TsNetworksDispatcher.ETrainerStatus Status { get { return _status; } set { this._status = value; Changed("Status"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Type { get; set; }

        public string Description { get; set; }

        public void Changed(string property)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }
}
