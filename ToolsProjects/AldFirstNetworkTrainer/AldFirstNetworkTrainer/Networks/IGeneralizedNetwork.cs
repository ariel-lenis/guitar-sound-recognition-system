using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AldFirstNetworkTrainer.Networks
{
    public interface IGeneralizedNetwork
    {
        float Alpha { get; set; }
        float LearningRate { get; set; }

        int Inputs { get; }
        int Outputs { get; }

        int[] NeuronsPerLayer { get; }

        void Create(int[] NeuronsPerLayer,float alpha, float learningrate);
        float Train(float[] inputs,float[] expected);
        float[] Forward(float[] inputs);
        void Free();

        TsNetworkBackup Backup(string thenamespace,string thekey,string description="");
        void Restore(TsNetworkBackup data);
    }
}

