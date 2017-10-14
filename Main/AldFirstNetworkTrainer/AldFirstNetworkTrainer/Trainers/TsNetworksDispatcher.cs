using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsFilesTools;

namespace AldFirstNetworkTrainer.Trainers
{
    public class TsNetworksDispatcher
    {
        public class DispatcherInformation
        {
            public string Name;
            public string Namespace;
            public ETrainerStatus Status;
            public Stopwatch Timer;
            public TsTrainingInfo Information;
            public Networks.IGeneralizedNetwork Network;
        }
        public enum ETrainerStatus { Stopped, Training, Stopping };

        public delegate void DNetworkUpdated(TsNetworksDispatcher who, string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork);
        public event DNetworkUpdated NetworkUpdated;

        Dictionary<string, Networks.IGeneralizedNetwork> thenetworks;
        Dictionary<string,string> thenamespaces;
        Dictionary<string, ETrainerStatus> thestatuses;
        Dictionary<string, Stopwatch> thestopwatches;
        Dictionary<string, TsTrainingInfo> thetraininginfos;

        public TsNetworksDispatcher()
        {
            thenetworks = new Dictionary<string, Networks.IGeneralizedNetwork>();
            thenamespaces = new Dictionary<string, string>();
            thestatuses = new Dictionary<string, ETrainerStatus>();
            thestopwatches = new Dictionary<string, Stopwatch>();
            thetraininginfos = new Dictionary<string, TsTrainingInfo>();
        }

        public bool ContainsNetwork(string name)
        {
            return thenetworks.ContainsKey(name);
        }
        
        public DispatcherInformation GetInformation(string name)
        {
            if (!this.ContainsNetwork(name)) return null;
            DispatcherInformation res = new DispatcherInformation() { 
                Information = thetraininginfos.ContainsKey(name)?thetraininginfos[name]:null,
                Name=name,
                Namespace =  thenamespaces.ContainsKey(name)?thenamespaces[name]:null,
                Status = thestatuses.ContainsKey(name)?thestatuses[name]: ETrainerStatus.Stopped,
                Timer = thestopwatches.ContainsKey(name)?thestopwatches[name]:null,
                Network=thenetworks.ContainsKey(name)?thenetworks[name]:null
            };
            return res;
        }


        public int CountNetworks(string thenamespace)
        {
            return this.thenamespaces.Where(x => x.Value == thenamespace && thenetworks.ContainsKey(x.Key)).Count();
        }

        public ETrainerStatus GetStatus(string name)
        {
            if (!thestatuses.ContainsKey(name)) throw new Exception("The networks dont exists.");
            return this.thestatuses[name];
        }

        public Networks.IGeneralizedNetwork GetNetwork(string name)
        {
            return thenetworks[name];
        }

        public void ChangeStatus(string name, ETrainerStatus newstatus)
        {
            this.thestatuses[name] = newstatus;
            this.thetraininginfos[name].Status = newstatus;
        }

        public void SetNetwork(string thenamespace, string thename, Networks.IGeneralizedNetwork thenetwork)
        {
            if (this.thenetworks.ContainsKey(thename))
            {
                if (this.thestatuses[thename] != ETrainerStatus.Stopped)
                    throw new Exception("Cant assing this network because is already training.");
                if(this.thenetworks[thename]!=thenetwork)
                    this.thenetworks[thename].Free();
                this.thestatuses[thename] = ETrainerStatus.Stopped;
                this.thenetworks[thename] = thenetwork;
            }
            else
            {
                this.thenetworks.Add(thename, thenetwork);
                this.thestatuses.Add(thename, ETrainerStatus.Stopped);
            }
            if (!this.thestopwatches.ContainsKey(thename))
                this.thestopwatches.Add(thename, new Stopwatch());

            if (!this.thetraininginfos.ContainsKey(thename))
                this.thetraininginfos.Add(thename, new TsTrainingInfo() {Namespace=thenamespace, Network = thename, Type = thenetwork.GetType().Name, Description = thenetwork.ToString() });

            if (this.thenamespaces.ContainsKey(thename))
                this.thenamespaces[thename] = thenamespace;
            else
                this.thenamespaces.Add(thename, thenamespace);

            if (this.NetworkUpdated != null)
                this.NetworkUpdated(this,thenamespace,thename,thenetwork);

        }

        public bool NamespaceIsTraining(string Namespace)
        {
            return  this.thenamespaces.Where(x => x.Value==Namespace && this.thenetworks.ContainsKey(x.Key)  && this.thestatuses[x.Key] != ETrainerStatus.Stopped ).Count() > 0;
        }

        public List<TsTrainingInfo> GetInfos(string Namespace)
        {
            var result = this.thetraininginfos.Where(x => this.thenamespaces.ContainsKey(x.Key) && this.thenamespaces[x.Key] == Namespace).Select(x => x.Value).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                var iresult = result[i];
                iresult.TheColor = TsColors.CommonColors[i];
                iresult.Status = this.thestatuses[iresult.Network];
            }
            return result;
        }

        public List<TsTrainingInfo> GetInfos()
        {
            var result = thetraininginfos.Values.ToList();
            for (int i = 0; i < result.Count; i++)
            {
                var iresult = result[i];
                iresult.TheColor = TsColors.CommonColors[i];
                iresult.Status = this.thestatuses[iresult.Network];
            }
            return result;
        }

        public void SaveNetworks(string path, string name = "")
        {
            
            if(!path.EndsWith("\\")) path+="\\";
            if(name=="")
                name = DateTime.Now.ToString();
            
            name = name.PreparePath("_");
            string directory = path+name;
            string networksdirectory = directory + "\\Networks";

            if(!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if(!Directory.Exists(networksdirectory))
                Directory.CreateDirectory(networksdirectory);
            
            //this.SaveObject(directory + "\\thetrainer.trainer");
            
            foreach(var inetwork in this.thenetworks)
            {
                string npath = networksdirectory + "\\" + inetwork.Key.PreparePath("_")+".ann";
                inetwork.Value.Backup(this.thenamespaces[inetwork.Key], inetwork.Key).SaveObject(npath);
            }

        }

        public void LoadNetworks(string path)
        {
            if (!path.EndsWith("\\")) path += "\\";
            var files = Directory.GetFiles(path + "\\Networks", "*.ann");
            foreach (var ifile in files)
            {
                FileInfo info = new FileInfo(ifile);
                string name = info.Name;

                int idx = name.LastIndexOf(info.Extension);
                if (info.Extension.Length > 0 && idx != -1)
                    name = name.Substring(0, idx);

                var backup = RawData.ReadObject<Networks.TsNetworkBackup>(ifile);
                Networks.IGeneralizedNetwork network;
                if (backup.NetworkType == typeof(Networks.TsCudaNetwork))
                    network = new Networks.TsCudaNetwork();
                else
                    network = new Networks.TsFastNetwork();
                network.Restore(backup);
                this.SetNetwork(backup.Namespace, backup.Key, network);
            }
        }


        public List<string> GetNamespaces()
        {
            return this.thenamespaces.Values.ToList();
        }
    }
}
