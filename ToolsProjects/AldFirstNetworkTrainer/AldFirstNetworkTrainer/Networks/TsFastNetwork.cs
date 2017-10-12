using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AaBackPropagationFast;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace AldFirstNetworkTrainer.Networks
{
    public class TsFastNetwork:IGeneralizedNetwork
    {
        float _alpha;
        float _learningrate;
        public float Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value;
            }
        }

        public float LearningRate
        {
            get
            {
                return _learningrate;
            }
            set
            {
                _learningrate = value;
            }
        }


        public int[] NeuronsPerLayer
        {
            get { return this.network.NeuronsPerLayer; }
        }

        AldNetwork network;
        public void Create(int[] NeuronsPerLayer, float alpha, float learningrate)
        {
            this._alpha = alpha;
            this._learningrate = learningrate;
            network = new AldNetwork(NeuronsPerLayer, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, learningrate, alpha);            
        }

        public float Train(float[] inputs, float[] expected)
        {
            return network.Train(inputs, expected);
        }

        public float[] Forward(float[] inputs)
        {
            return network.Forward(inputs);
        }

        public void Free()
        {
            GC.Collect();
        }


        public int Inputs
        {
            get { return network.Inputs; }
        }

        public int Outputs
        {
            get { return network.Outputs; }
        }
        public override string ToString()
        {
            string res = "";
            for(int i=0;i<this.network.N.Length;i++)
            {
                res += "{";
                res += this.network.N[i].Length;
                res += "} ";
            }
            return res;
        }

        public TsNetworkBackup Backup(string thenamespace,string thekey,string description = "")
        {
            TsNetworkBackup backup = new TsNetworkBackup(thenamespace,thekey);

            backup.Alpha = this.Alpha;
            backup.LearningRate = this.LearningRate;
            backup.Description = description;

            backup.Date = DateTime.Now;
            backup.NetworkType = typeof(TsFastNetwork);

            backup.Data = this.network.SerializeToBytes();
            return backup;
        }

        public void Restore(TsNetworkBackup data)
        {
            this.Alpha = data.Alpha;
            this.LearningRate = data.LearningRate;

            MemoryStream ms = new MemoryStream();
            ms.Write(data.Data, 0, data.Data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;

            BinaryFormatter bf = new BinaryFormatter();
            AldNetwork network =  bf.Deserialize(ms) as AldNetwork;
            this.network = network;
            ms.Dispose();
        }


    }
}
