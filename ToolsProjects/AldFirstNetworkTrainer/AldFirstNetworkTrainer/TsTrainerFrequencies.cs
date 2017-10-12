using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AaBackPropagationFast;
using AldSpecialAlgorithms;
using TsFilesTools;
namespace AldFirstNetworkTrainer
{
    public class Trainer2Data
    {
        public List<TimeMark> marks;
        public int start;
        public int end;
        public FrequenciesPeaks.FPResult peaks;
        public int Start { get { return start; } }
        public int End { get { return end; } }
        public int Size { get { return this.end - this.start + 1; } }

        public int HarmSize { get { return this.peaks.hzHarm.Length; } }

        public override string ToString()
        {
            return start + "->" + end + " " + peaks.hzHarm.Length;
        }
    }
    public class TsTrainerFrequencies
    {
        Networks.IGeneralizedNetwork network;
        public Dictionary<Trainer2Data, float[]> output;        

        public Networks.IGeneralizedNetwork Network 
        {
            get {return network;}
            set
            {
                if (value.Inputs != harmonics * 5) throw new Exception("The number of input neurons must be 4*harmonics.");
                this.network = value;
            }
        }

        List<Trainer2Data> data;
        public List<Trainer2Data> FinalData { get { return data; } }
        public List<Trainer2Data> SetData(List<Trainer2Data> data)
        {
            this.data = data;
            GenerateOutput();
            return this.data;
        }

        private void GenerateOutput()
        {
            output = new Dictionary<Trainer2Data, float[]>();
            List<Trainer2Data> datax = new List<Trainer2Data>();
            foreach (var i in this.data)
            {
                if (!NoHaveArmonicInterference(i.marks) ||i.peaks.idxHarm.Length==0) continue;
                float[] outnetwork = new float[i.peaks.idxHarm.Length];
                if (i.marks.Count == 0) throw new Exception("¬¬");
                foreach (var j in i.marks)
                    MarkPoints(outnetwork,i,j);
                output.Add(i, outnetwork);
                datax.Add(i);
            }
            this.data = datax;
        }

        private bool NoHaveArmonicInterference(List<TimeMark> list)
        {
            foreach (var i in list)
                foreach (var j in list)
                {
                    if (i.Frequency == j.Frequency) continue;
                    double ddiv = i.Frequency / j.Frequency;
                    int div = (int)ddiv;

                    if (ddiv - div < 1e-4)
                        return false;
                }            
            return true;            
        }

        private void MarkPoints(float[] outnetwork, Trainer2Data trainer, TimeMark mark)
        {
            float frequency = (float)mark.Frequency;
            float pos = frequency / trainer.peaks.allhz[1];//position 1 have 1*sr/n;
            int idx = MoreClose(trainer, pos);
            outnetwork[idx] = 1;
        }

        private int MoreClose(Trainer2Data trainer, float pos)
        {
            int idx = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < trainer.peaks.idxHarm.Length; i++)
            {
                float delta = Math.Abs(pos - trainer.peaks.idxHarm[i]);
                if (delta < distance)
                {
                    distance = delta;
                    idx = i;
                }
            }
            return idx;
        }

        int harmonics;
        public TsTrainerFrequencies(int harmonics)
        {
            this.harmonics = harmonics;
        }
        public int CalculateIterations()
        {
            return data.Sum(x => x.peaks.pksHarm.Length);
        }
        public float Iterate(int idx)
        { 
            int trainerpos = 0;
            int trainersubpos = 0;
            GetTrainerPos(idx, out trainerpos, out trainersubpos);
            float[] inputs = GetNetworkInput(trainerpos, trainersubpos);
            float[] result = new float[]{ GetNetworkResult(trainerpos, trainersubpos)};
            return network.Train(inputs, result);
            //return network.TotalError(result);
        }

        private float GetNetworkResult(int trainerpos, int trainersubpos)
        {
            return output[data[trainerpos]][trainersubpos];            
        }
        /*
        private float[] GetNetworkInput(int trainerpos, int trainersubpos)
        {
            var trainer = data[trainerpos];
            float[] inputs = new float[network.Inputs];

            int pos = trainer.peaks.idxHarm[trainersubpos];
            int div = inputs.Length / harmonics;

            for (int i = 0; i < harmonics; i++)
            {
                inputs[harmonics * 0 + i] = ComputeMax(trainer.peaks.allharm, (i + 1) * pos, 1);
                inputs[harmonics * 1 + i] = ComputeMax(trainer.peaks.allfft, (i + 1) * pos, 1);
                inputs[harmonics * 2 + i] = ComputeMax(trainer.peaks.allcepstofreq, (i + 1) * pos, 1);
                inputs[harmonics * 3 + i] = ComputeMax(trainer.peaks.allacorrtofreq, (i + 1) * pos, 1);

                int subpos = pos / 2;
                int subi = i * 2 + 1;

                inputs[harmonics * 4 + i] = (0 + ComputeMax(trainer.peaks.allharm, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allfft, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allcepstofreq, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allacorrtofreq, (subi + 1) * subpos, 1));
            }
            inputs.MeanCenterBlock(div);
            return inputs;
        }
        */
        
        private float[] GetNetworkInput(int trainerpos, int trainersubpos)
        {
            var trainer = data[trainerpos];
            float[] inputs = new float[network.Inputs];
            
            int pos = trainer.peaks.idxHarm[trainersubpos];
            int div = inputs.Length / harmonics;

            for (int i = 0; i < harmonics; i++)
            {
                inputs[div * i + 0] = ComputeMax(trainer.peaks.allharm, (i + 1) * pos, 1);
                inputs[div * i + 1] = ComputeMax(trainer.peaks.allfft, (i + 1) * pos, 1);                
                inputs[div * i + 2] = ComputeMax(trainer.peaks.allcepstofreq, (i + 1) * pos, 1);
                inputs[div * i + 3] = ComputeMax(trainer.peaks.allacorrtofreq, (i + 1) * pos, 1);

                int subpos = pos / 2;
                int subi = i * 2 + 1;

                inputs[div * i + 4] = (0 + ComputeMax(trainer.peaks.allharm, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allfft, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allcepstofreq, (subi + 1) * subpos, 1)) *
                                      (0 + ComputeMax(trainer.peaks.allacorrtofreq, (subi + 1) * subpos, 1));
            }
            inputs.MeanCenterBlock(div);
            return inputs;
        }
        

        private float ComputeMax(float[] p, int mid,int lados)
        {
            float max = float.MinValue;
            for (int i = mid - lados; i <= mid + lados; i++)            
                if(i>=0 && i<p.Length)
                    if (p[i] > max)
                        max = p[i];
            return max;
        }
        private float Max(float a, float b)
        {
            if (a > b) return a;
            return b;
        }
        private void GetTrainerPos(int idx, out int trainerpos, out int trainersubpos)
        {
            int acum = 0;
            trainerpos = 0;
            for (int i = 0; i < data.Count; i++)
            {
                acum += data[i].peaks.pksHarm.Length;
                if (acum > idx)
                {
                    acum -= data[i].peaks.pksHarm.Length;
                    trainerpos = i;
                    break;
                }
            }
            trainersubpos = idx - acum;
        }
    }
}
