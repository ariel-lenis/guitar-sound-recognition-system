using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsFilesTools;
using AldSpecialAlgorithms;
using TsFFTFramework;
using D = System.Drawing;
//using AldBackPropagation;
using AaBackPropagationFast;
using System.Diagnostics;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Windows;
using AldFirstNetworkTrainer.Trainers;


namespace AldFirstNetworkTrainer
{
    [Serializable]
    public class TrainerAllTimes
    {
        List<TimeMark> markers;

        Dictionary<string, ITrainer> trainers;
        public Dictionary<string, ITrainer> Trainers { get { return trainers; } }
        public const string Namespace = "TrainerTimes";

        double resumerate;

        int errormax, samplerate;

        public delegate bool DTrainingEpochComplete(TrainerAllTimes all,string name,int idx,float x,float y);
        public event DTrainingEpochComplete TrainingEpochComplete;

        public delegate void DTrainingEnded(TrainerAllTimes all,string name,int idx);
        public event DTrainingEnded TrainingEnded;

        public delegate void DTrainerUpdated(TrainerAllTimes all);

        TsFirsStepSolution solution;
        public TsFirsStepSolution TheSolution { get { return solution; } }


        public int NetworksCount { get { return this.solution.TheDispatcher.CountNetworks(Namespace); } }

        public string TrainerName(int idx)
        {
            return trainers.ElementAt(idx).Key;
        }

        public ITrainer this[int idx]
        {
            get 
            {
                return trainers.ElementAt(idx).Value;
            }
        }
        public ITrainer this[string idx]
        {
            get { return trainers[idx];  }
        }

        public TrainerAllTimes(TsFirsStepSolution solution, int errormax,double resumerate,int samplerate)
        {
            this.errormax = errormax;
            this.resumerate = resumerate;            
            this.samplerate = samplerate;
            trainers = new Dictionary<string, ITrainer>();
            //Networks = new Dictionary<string, Networks.IGeneralizedNetwork>();
            //this.dispatcher = dispatcher;
            this.solution = solution;
        }
        public void SetMarkers(List<TimeMark> markers)
        {
            this.markers = markers;
        }
        /*
        void ChangeStatus(string name,TsTrainingInfo.ETrainerStatus newstatus)
        {
            trainersStatus[name] = newstatus;
            traininginfos[name].Status = newstatus;
        }
         * */
        /*
        public void SetNetwork(string name, Networks.IGeneralizedNetwork network)
        {
            if (Networks.ContainsKey(name))
            {
                if (trainersStatus[name] != TsTrainingInfo.ETrainerStatus.Stopped)
                    throw new Exception("Cant assing this network because is already training.");
                Networks[name].Free();
                trainersStatus[name] = TsTrainingInfo.ETrainerStatus.Stopped;
                Networks[name] = network;
            }
            else
            { 
                Networks.Add(name, network);
                trainersStatus.Add(name, TsTrainingInfo.ETrainerStatus.Stopped);
            }
            if (!stopwatchs.ContainsKey(name))
                stopwatchs.Add(name, new Stopwatch());

            if (!traininginfos.ContainsKey(name))
                traininginfos.Add(name, new TsTrainingInfo() {Network=name,Type=network.GetType().Name,Description=network.ToString()});
            if (this.TrainerUpdated != null)
                this.TrainerUpdated(this);
        }
         * */
        public void SetData1d(string name, float[] data)
        {
            //if (!Networks.ContainsKey(name)) throw new Exception("The network with key " + name + " not exists..");
            if (!this.solution.TheDispatcher.ContainsNetwork(name)) throw new Exception("The network with key " + name + " not exists..");
            TrainerOne trainer = new TrainerOne();
            trainer.Create(errormax, resumerate,0, samplerate, this.solution.TheDispatcher.GetInformation(name).Network);
            trainer.LoadData(data, markers);
            if (trainers.ContainsKey(name))
                trainers[name] = trainer;
            else
                trainers.Add(name, trainer);
            
        }
        public void SetData2d(string name, float[][] spdata, int trainColumns2d, double resumerate2d,float position, bool modify = true, bool negateinputs = false, bool negativenormalization=false)
        {
            //if (!Networks.ContainsKey(name)) throw new Exception("The network with key " + name + " not exists..");

            if (!this.solution.TheDispatcher.ContainsNetwork(name)) throw new Exception("The network with key " + name + " not exists..");

            TrainerOneSpectogram trainer = new TrainerOneSpectogram();
            trainer.Modify = modify;
            trainer.NegateInputs = negateinputs;
            trainer.NegativeNormalization = negativenormalization;
            trainer.Position = position;

            trainer.Create(errormax,resumerate2d, trainColumns2d, samplerate, this.TheSolution.TheDispatcher.GetNetwork(name));
            trainer.LoadData(spdata, markers);

            if (trainers.ContainsKey(name))
                trainers[name] = trainer;
            else
                trainers.Add(name, trainer);
        }
        public void ClearTrainers()
        {
            trainers.Clear();
        }
        public void StartTrainingAll(int bugCasesMultiplication)
        {
            //if (this.IsTraining)
            //    throw new Exception("Already training...");

            foreach (var iname in this.trainers.Keys)
                if (this.TheSolution.TheDispatcher.GetStatus(iname) ==  TsNetworksDispatcher.ETrainerStatus.Stopped)
                    StartTrainingOne(iname, bugCasesMultiplication);
            /*
            Task.Run(delegate
            {
                Parallel.For(0, trainers.Count, delegate(int idx) { TrainOne(idx, bugCasesMultiplication); });
                istraining = false;
                if (TrainingEnded != null)
                    TrainingEnded(this);
            });
            */
        }

        public void ClearEpochCounters()
        {
            if (IsTraining) throw new Exception("Cant clear counters in middle of the training.");
            foreach (var iinfo in this.trainers.Keys)
                this.TheSolution.TheDispatcher.GetInformation(iinfo).Information.Epochs = 0;
        }
        /*
        public TsTrainingInfo.ETrainerStatus StatusOf(string name)
        {
            return trainersStatus[name];
        }
        */
        public void Swap(string name,int bugCasesMutiplication)
        {
            if (this.TheSolution.TheDispatcher.GetStatus(name) == TsNetworksDispatcher.ETrainerStatus.Training)
                InitializeStopTraining(name);
            if (this.TheSolution.TheDispatcher.GetStatus(name) == TsNetworksDispatcher.ETrainerStatus.Stopped)
                StartTrainingOne(name, bugCasesMutiplication);

        }


        public void StartTrainingOne(string name, int bugCasesMutiplication)
        {
            if (!this.TheSolution.TheDispatcher.ContainsNetwork(name))
                throw new Exception("Error cant start this trainer because the name dont exists.");
            if (this.TheSolution.TheDispatcher.GetStatus(name) != TsNetworksDispatcher.ETrainerStatus.Stopped)
                throw new Exception("Error cant start this trainer because is already training.");

            //ChangeStatus(name, TsTrainingInfo.ETrainerStatus.Training);
            this.TheSolution.TheDispatcher.ChangeStatus(name, TsNetworksDispatcher.ETrainerStatus.Training);
            //ChangeStatus(name, TsTrainingInfo.ETrainerStatus.Training);
            //trainersStatus[name] = ETrainerStatus.Training;
            Task.Run(delegate() { TrainOne(name, bugCasesMutiplication); });
        }

        public void InitializeStopTraining(string name)
        {
            if (!this.TheSolution.TheDispatcher.ContainsNetwork(name))
                throw new Exception("The key dont exists.");
            if (this.TheSolution.TheDispatcher.GetStatus(name) != TsNetworksDispatcher.ETrainerStatus.Training)
                throw new Exception("This trainer is not training.");

            //ChangeStatus(name, TsTrainingInfo.ETrainerStatus.Stopping);
            this.TheSolution.TheDispatcher.ChangeStatus(name, TsNetworksDispatcher.ETrainerStatus.Stopping);
            //trainersStatus[name] = ETrainerStatus.Stopping;
        }

        public void InitializeStopAll()
        {
            foreach (var iname in this.trainers.Keys)
                if (this.TheSolution.TheDispatcher.GetStatus(iname) == TsNetworksDispatcher.ETrainerStatus.Training)
                    InitializeStopTraining(iname);
        }

        public bool IsTraining
        {
            get
            {
                return this.TheSolution.TheDispatcher.NamespaceIsTraining(Namespace);
            }
        }

        public int IndexOfTrainer(string name)
        {
            return this.trainers.Keys.ToList().IndexOf(name);            
        }

        void TrainOne(string name,int bugCount)
        {
            var thetrainer = this.trainers[name]; //trainers.ElementAt(idx);
            int idx = IndexOfTrainer(name);

            //string name = thetrainer.Key;

            var data = this.TheSolution.TheDispatcher.GetInformation(name);

            Stopwatch sw = data.Timer;
            TsTrainingInfo info = data.Information;


            int n = thetrainer.CalculateRecomendedIterations();
            double sum = 0;

            for (int j = 0; this.TheSolution.TheDispatcher.GetStatus(name) == TsNetworksDispatcher.ETrainerStatus.Training; j++)
            {
                sw.Start();

                float errorp = 0;
                
                for (int i = 0; i < n; i++)
                {
                    errorp += thetrainer.TrainOneTime(i, bugCount);                
                }

                float x = j;
                float y = errorp / n;

                sum += sw.ElapsedMilliseconds / 1000.0;

                sw.Reset();

                info.AverageTime = (float)(sum / (j+1));
                info.TheColor = TsColors.CommonColors[idx];
                info.Epochs++; /*=j + 1*/;
                info.TrainsPerEpoch = n*(1 + bugCount);

                if (TrainingEpochComplete != null)
                    if (!TrainingEpochComplete(this, name, idx, x, y))
                        return;
            }
            this.TheSolution.TheDispatcher.ChangeStatus(name, TsNetworksDispatcher.ETrainerStatus.Stopped);
            //trainersStatus[name] = ETrainerStatus.Stopped;
            if (this.TrainingEnded != null)
                this.TrainingEnded(this, name, idx);
        }
        
        public List<TsTrainingInfo> GetInfos()
        {
            return this.TheSolution.TheDispatcher.GetInfos(Namespace);
        }
        



    }
}
