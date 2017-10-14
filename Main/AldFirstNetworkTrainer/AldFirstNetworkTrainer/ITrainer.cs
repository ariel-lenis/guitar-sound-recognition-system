//using AldBackPropagation;
using AaBackPropagationFast;

using TsFilesTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldFirstNetworkTrainer
{
    public interface ITrainer
    {
        object GetData{get;}
        float[] ResumeData{get;}
        Networks.IGeneralizedNetwork TheNetwork { get;}
        float[] OutputField { get;}

        

        int CalculateRecomendedIterations();
        float TrainOneTime(int pos, int bugCasesMultiplication);
        float[] GetTrainingSample(int pos);
        float[] GetNetworkSolution();

        void Create(int errorMax,double resumeRate, int trainColumns2D, int sampleRate, Networks.IGeneralizedNetwork network);
        void LoadData(object data, List<TimeMark> markers); 
    }
}
