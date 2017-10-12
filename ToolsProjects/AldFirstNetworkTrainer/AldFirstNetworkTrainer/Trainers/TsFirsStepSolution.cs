using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldFirstNetworkTrainer
{
    public class TsFirsStepSolution
    {
        public delegate void DTrainerUpdated(TsFirsStepSolution who, string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork);
        public event DTrainerUpdated TrainerUpdated;

        TsAdminAllFrequencies trainerFrequencies;
        TrainerAllTimes trainerTimes;
        Trainers.TsNetworksDispatcher dispatcher;

        public Trainers.TsNetworksDispatcher TheDispatcher { get { return dispatcher; } }

        public List<FrequencySolution> TheSolution { get; set; }

        public bool IsTraining
        {
            get { return trainerTimes.IsTraining; }
        }

        public TsAdminAllFrequencies TrainerFrequencies 
        {
            get { return trainerFrequencies; }
            set 
            {
                //if (this.trainerFrequencies != null)
                //    this.trainerFrequencies.TrainerUpdated -= trainerFrequencies_TrainerUpdated;
                this.trainerFrequencies = value; 
                //this.trainerFrequencies.TrainerUpdated+=trainerFrequencies_TrainerUpdated;                
            }
        }

        void trainerFrequencies_TrainerUpdated(TsAdminAllFrequencies admin)
        {
            //if (TrainerUpdated != null)
            //    TrainerUpdated(this, typeof(TsAdminAllFrequencies));            
        }
        public TrainerAllTimes TrainerTimes 
        {
            get { return trainerTimes; }
            set 
            {

                //if(this.trainerTimes!=null)
                //    this.trainerTimes.TrainerUpdated -= trainerTimes_TrainerUpdated;
                this.trainerTimes = value; 
                //this.trainerTimes.TrainerUpdated += trainerTimes_TrainerUpdated;                
            }
        }
        /*
        void trainerTimes_TrainerUpdated(TrainerAllTimes all)
        {
            if (TrainerUpdated != null)
                TrainerUpdated(this, typeof(TrainerAllTimes));
        }
        */
        public TsFirsStepSolution()
        {
            this.dispatcher = new Trainers.TsNetworksDispatcher();
            this.dispatcher.NetworkUpdated += dispatcher_NetworkUpdated;
        }

        void dispatcher_NetworkUpdated(Trainers.TsNetworksDispatcher who, string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork)
        {
            if (this.TrainerUpdated != null)
                this.TrainerUpdated(this, thenamespace, thename, thenetwork);
        }


        public void SaveNetworks(string path, string folder)
        {
            //this.TrainerTimes.SaveNetworks(path, folder);
            //this.trainerFrequencies.SaveNetworks(path, folder);

            this.dispatcher.SaveNetworks(path, folder);

        }

        public TsFilesTools.TsTrainingSet CurrentTrainingSet { get; set; }
    }
}
