using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using AldBackPropagation;
using AaBackPropagationFast;

using TsFilesTools;
using AldSpecialAlgorithms;
using System.Diagnostics;
using TsFFTFramework;
 
namespace AldFirstNetworkTrainer
{
    [Serializable]
    public struct Range
    {
        public int Start,N;
        public Range(int start,int n)
        {
            this.Start = start;
            this.N = n;
        }
    }
    [Serializable]
    public class TrainerOne : ITrainer
    {
        double resumeRate;
        int sampleRate;
        int frameNetwork;
        int errorMax;
        Networks.IGeneralizedNetwork network;

        float[] thedata;
        float[] bufferNetwork;
        float[] resume;

        float[] outputField;

        int[] resumeMarks;

        //regions that have 0 marks
        Range[] zeroes;

        float[] errorWindow;

        public Networks.IGeneralizedNetwork TheNetwork { get { return network; } }
        public float[] OutputField { get { return outputField; } }

        public object GetData { get { return thedata; } }
        public float[] ResumeData { get { return resume; } }

        int idxZeroes;
        int idxSubZeroes;

        int NextZero()
        {
            int res= zeroes[idxZeroes].Start + idxSubZeroes;
            idxSubZeroes++;
            if (idxSubZeroes >= zeroes[idxZeroes].N)
            {
                idxSubZeroes = 0;
                idxZeroes++;
                if (idxZeroes >= zeroes.Length)
                    idxZeroes = 0;
            }
            return res;
        }
        public int CalculateRecomendedIterations()
        {
            return resumeMarks.Length;
        }
        public float TrainOneTime(int pos,int bugCasesMultiplication)
        {
            float error = 0;
            pos = resumeMarks[pos]-(frameNetwork-1)/2;

            //if (pos < 0) return 0;

            int cont = 0;
            for (int i = pos - errorMax; i <= pos + errorMax; i++)
            {
                error+=TrainNetworkUnit(i);
                cont++;
                for (int j = 0; j < bugCasesMultiplication; j++)
                {
                    error += TrainNetworkUnit(NextZero());
                    cont++;
                }
            }
            return error / cont;//((2 * errorMax + 1) * 2);
        }
        public float[] GetTrainingSample(int pos)
        {
            pos = resumeMarks[pos];
            pos -= (frameNetwork-1)/2;
            for (int i = 0; i < frameNetwork; i++)
                bufferNetwork[i] = resume[pos + i];
            return bufferNetwork;

        }
        public float[] GetNetworkSolution()
        {
            float[] res = new float[resume.Length];
            for (int i = 0; i < res.Length - frameNetwork + 1; i++)
            {
                for (int j = 0; j < frameNetwork; j++)
                    bufferNetwork[j] = resume[i + j];

                //bufferNetwork.StandarNormalization();

                //bufferNetwork.MeanCenter();

                float output = network.Forward(bufferNetwork)[0];
                res[i+(frameNetwork-1)/2] = -output;
            }
            return res;
        }
        private float TrainNetworkUnit(int pos)
        {
            float[] output = new float[1];

            for (int i = 0; i < frameNetwork; i++)
            {
                //bufferNetwork[i]= outputField[pos + (frameNetwork - 1) / 2];
                if (pos + i>=0 && pos+i<resume.Length)
                    bufferNetwork[i] = resume[pos + i];
                else
                    bufferNetwork[i] = 0;
            }


            //bufferNetwork.MeanCenter();

            //bufferNetwork.AldFitValuesMinToMax();

            bufferNetwork.StandarNormalization();



            output[0] = outputField[pos + (frameNetwork-1)/2];

            return network.Train(bufferNetwork, output);
            //return network.TotalError(output);
        }
        public void Create(int errorMax,double resumeRate,int train2dColumns,int sampleRate, Networks.IGeneralizedNetwork network)
        {
            this.errorMax = errorMax;
            this.resumeRate = resumeRate;
            this.network = network;
            this.sampleRate = sampleRate;
            this.frameNetwork = network.Inputs;
            bufferNetwork = new float[this.frameNetwork];
            this.errorWindow = Windows.Hanning(1+2*errorMax);
        }
        public void LoadData(object odata,List<TimeMark> markers) 
        {
            float[] data = (float[])odata;
            if (data == thedata) return;
            this.thedata = data;
            Resume(data);
            ResumeMarks(markers);
            CreateZeroes();
            CreateOutputField();
            idxZeroes = 0;
            idxSubZeroes=0;
        }

        private void CreateOutputField()
        {
            outputField = new float[resume.Length];
            foreach (var i in resumeMarks)
                for (int j = i - errorMax; j <= i + errorMax; j++)
                {
                    //outputField[j] = (outputField[j] + errorWindow[j - (i - errorMax)]) / 2;
                    outputField[j] += errorWindow[j - (i - errorMax)];
                    if (outputField[j] > 1) outputField[j] = 1;
                }
        }

        private void CreateZeroes()
        {
            if (resumeMarks.Length == 0) return;
            idxSubZeroes = idxZeroes = 0;
            var aux = new List<Range>();
            if (resumeMarks[0] - errorMax - 1>0)
                aux.Add(new Range(0, resumeMarks[0] - errorMax - 1));
            for (int i = 0; i < resumeMarks.Length; i++)
            {
                int start = resumeMarks[i] + errorMax + 1;
                int end = (i==resumeMarks.Length-1)?(resume.Length-1):(resumeMarks[i + 1] - errorMax - 1);
                if (start <= end-frameNetwork)
                    aux.Add(new Range(start, end - start + 1-frameNetwork));
            }
            zeroes = aux.ToArray();
        }
        private void ResumeMarks(List<TimeMark> markers)
        {
            List<int> aux = new List<int>();
            int last = 0;
            for (int i = 0; i < markers.Count; i++)
            {
                if (!markers[i].IsNoteOn) continue;
                var posible = (int)(markers[i].MarkPosition / resumeRate);
                if(i==0 || posible>=last+errorMax)
                    aux.Add(posible);
                last = posible;
            }
            if (aux.Count > 0 && aux[0] == 0)
                resumeMarks = aux.Skip(1).ToArray();
            else
                resumeMarks = aux.ToArray();            
        }
        void Resume(float[] data)
        {
            int n =(int)(data.Length / resumeRate);
            resume = new float[n];
            float max=0, min=0,aux;

            for (int i = 0; i < n; i++)
            {
                aux = MaximunRange(data, (int)(i * resumeRate), (int)(resumeRate));
                if(i==0||aux>max) max=aux;
                if(i==0||aux<min) min=aux;
                resume[i] = aux;
            }            

            float ratio = 1/(max-min);
            for (int i = 0; i < n; i++)
                resume[i] = (resume[i] - min) * ratio;
        }
        private float MaximunRange(float[] data,int start, int n)
        {
            float max = 0;
            for (int i = start; i < start + n; i++)
                if (i == start || data[i] > max)
                    max = data[i];
            return max;
        }
    }    
}