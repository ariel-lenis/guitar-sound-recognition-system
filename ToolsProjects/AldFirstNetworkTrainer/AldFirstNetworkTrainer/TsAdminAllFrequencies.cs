using TsFilesTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AldSpecialAlgorithms;
using AaBackPropagationFast;
using TsExtraControls;
using System.IO;

namespace AldFirstNetworkTrainer
{
    [Serializable]
    public class FrequencySolution
    {
        public List<float> Frequencies;
        public List<float> Amplitudes;
        public List<int> Midies;

        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public float[] NeuralField;

        public float Error;
        public float OctaveError;
    }
    public class TsAdminAllFrequencies
    {
        public const string Namespace = "TrainerFrequencies";
        private TsTrainingSet set;
        public TsTrainerFrequencies trainer;
        public List<Trainer2Data> ranges;
        public int samplerate;

        TsFirsStepSolution solution;

        public TsFirsStepSolution TheSolution { get { return solution; } }        

        public List<Trainer2Data> Ranges { get { return ranges; } }

        public delegate void DEpochTrained(object who, int epoch, float err);

        public delegate void DPeaksCalculated(object who, int idx);

        public event DPeaksCalculated PeaksCalculated;


        public event DEpochTrained EpochTrained;

        public delegate void DTrainerUpdated(TsAdminAllFrequencies admin);
        bool endtraining = false;

        public TsAdminAllFrequencies(TsFirsStepSolution solution)
        {
            this.solution = solution;
        }
        public void LoadTrainingSet(TsTrainingSet currentTrainingSet)
        {
            Networks.IGeneralizedNetwork network = this.TheSolution.TheDispatcher.GetNetwork("F1");
            this.set = currentTrainingSet;
            //this.set.LoadFilesInformation();
            samplerate = currentTrainingSet.WaveDescriptor.SampleRate;            
            ranges = GenerateDivisions(this.set);
            CalculatePeaks();

            trainer = new TsTrainerFrequencies(network.Inputs/5);
            trainer.Network = network;
            this.ranges = trainer.SetData(ranges);
        }



        public FrequencySolution GetSolution(int rangeidx)
        {
            if (rangeidx < 0 || rangeidx >= ranges.Count)
                throw new Exception("Error index out of bounds.");

            FrequencySolution solution = new FrequencySolution();
            solution.NeuralField = ApplyNetwork(ranges[rangeidx].peaks);

            solution.StartTime = TimeSpan.FromSeconds((double)ranges[rangeidx].start / samplerate);
            solution.EndTime = TimeSpan.FromSeconds((double)ranges[rangeidx].end / samplerate);

            //float minfrequency = 60;            

            var peaks = PeakDetection.Detect(solution.NeuralField, sortbypks: true);

            solution.Frequencies = new List<float>();
            solution.Midies = new List<int>();
            solution.Amplitudes = new List<float>();

            /*
            while (ranges[rangeidx].peaks.allfft[peaks.locs[0]] < 0.25f)
            {
                peaks.pks = peaks.pks.Skip(1).ToArray();
                peaks.locs = peaks.locs.Skip(1).ToArray();
            }
             * */

            for (int i = 0; i < peaks.locs.Length; i++)
            {
                int pos = peaks.locs[i];
                float f = ranges[rangeidx].peaks.allhz[pos];

                float amp = 0;

                for (int j = 0; j < 10; j++)
                {
                    float x = ranges[rangeidx].peaks.allfft[pos * (j + 1)];
                    if (x > amp)
                        amp = x;
                }

                int midi = TsMIDITools.FrequencyToMidi(f);


                //if (i == 0 || peaks.pks[i] >= 0.25f)
                //if (i == 0 || Math.Abs(peaks.pks[i - 1] - peaks.pks[i]) < 0.1f)
                if (i == 0 || Math.Abs(peaks.pks[0] - peaks.pks[i]) < 0.4f*0+0.58f)
                //if ( (i == 0) || Math.Abs(ranges[rangeidx].peaks.allfft[peaks.locs[0]] - ranges[rangeidx].peaks.allfft[peaks.locs[i]]) < 0.4f)
                {
                    solution.Midies.Add(midi);
                    solution.Frequencies.Add(f);
                    solution.Amplitudes.Add(amp);
                }
                else
                    break;
            }

            List<int> midismarks = ranges[rangeidx].marks.Select(x => x.AproximateMIDI).Distinct().ToList();
            List<int> midisfounded = solution.Midies.ToList();

            int sum = midismarks.Count + midisfounded.Count;
            int marksn = midismarks.Count;

            var intersect = midismarks.Intersect(midisfounded).ToList();
            /*
            foreach (var i in delete)
            {
                midismarks.Remove(i);
                midisfounded.Remove(i);                
            }
            */
            //solution.Error = (float)(midisfounded.Count+midismarks.Count)/sum;

            solution.Error = (float)(sum-2*intersect.Count)/sum;

            int octaves = 0;

            for (int i = 0; i < midisfounded.Count; i++)
            {
                int midif = midisfounded[i];
                if (intersect.Contains(midisfounded[i])) continue;
                if (midismarks.Contains(midif - 12) || midismarks.Contains(midif + 12))
                    octaves++;
            }

            if (midisfounded.Count == 0)
                solution.OctaveError = 0;
            else
                solution.OctaveError = octaves / midisfounded.Count;

            return solution;
        }

        public float[] ApplyNetwork(FrequenciesPeaks.FPResult peaks)
        {
            Networks.IGeneralizedNetwork network = this.TheSolution.TheDispatcher.GetNetwork("F1");

            float[] res = new float[peaks.allfft.Length];
            int harmonics = network.Inputs / 5;

            float[] inputs = new float[network.Inputs];

            int div = inputs.Length / harmonics;

            for (int idx = 0; idx < peaks.pksFFT.Length; idx++)
            {
                int i = peaks.idxFFT[idx];

                if (i >= res.Length / harmonics) continue;

                for (int j = 0; j < harmonics; j++)
                {
                    inputs[div * j + 0] = ComputeMax(peaks.allharm, (j + 1) * i, 1);
                    inputs[div * j + 1] = ComputeMax(peaks.allfft, (j + 1) * i, 1);
                    inputs[div * j + 2] = ComputeMax(peaks.allcepstofreq, (j + 1) * i, 1);
                    inputs[div * j + 3] = ComputeMax(peaks.allacorrtofreq, (j + 1) * i, 1);

                    int subpos = i / 2;
                    int subi = j * 2 + 1;


                    inputs[div * j + 4] = (0 + ComputeMax(peaks.allharm, (subi + 1) * subpos, 1)) *
                                            (0 + ComputeMax(peaks.allfft, (subi + 1) * subpos, 1)) *
                                            (0 + ComputeMax(peaks.allcepstofreq, (subi + 1) * subpos, 1)) *
                                            (0 + ComputeMax(peaks.allacorrtofreq, (subi + 1) * subpos, 1));


                }
                inputs.MeanCenterBlock(div);
                res[i] = network.Forward(inputs)[0];
            }
            res.NormalizeFit();
            return res;
        }

        private float ComputeMax(float[] p, int mid, int lados)
        {
            float max = float.MinValue;
            for (int i = mid - lados; i <= mid + lados; i++)
                if (i >= 0 && i < p.Length)
                    if (p[i] > max)
                        max = p[i];
            return max;
        }

        private void CalculatePeaks()
        {
            for (int i = 0; i < ranges.Count; i++)
            {                
                FillPeaksFor(ranges[i]);
                if (PeaksCalculated != null)
                    PeaksCalculated(this, i);
            }
        }

        private void FillPeaksFor(Trainer2Data trainer)
        {
            var data = set.Wave;
            float minfrequency = 60;


            trainer.peaks = FrequenciesPeaks.CalculatePeaks(data, trainer.start, trainer.end, samplerate, 5, 0.05f, minfrequency);
            trainer.peaks.allfft.NormalizeFit();
            trainer.peaks.allharm.NormalizeFit();
            trainer.peaks.pksFFT.NormalizeFit();
            trainer.peaks.pksHarm.NormalizeFit();
        }
        private List<Trainer2Data> GenerateDivisions(TsTrainingSet set)
        {            
            List<Trainer2Data> res = new List<Trainer2Data>();
            Trainer2Data last = null;
            TimeMark lastmark=null;

            for (int i = 0; i < set.Markers.Count;i++)
            {
                if (!set.Markers[i].IsNoteOn) continue;
                if (lastmark == null || set.Markers[i].MarkTime.Subtract(lastmark.MarkTime).TotalMilliseconds > 10)
                {
                    Trainer2Data trainer = new Trainer2Data();
                    trainer.start = set.Markers[i].MarkPosition;

                    if (last != null)
                        last.end = trainer.start;

                    last = trainer;
                    res.Add(trainer);
                    lastmark = set.Markers[i];
                }
            }

            if (res.Count > 0 && res.Last().end == 0)//last have no end
            {
                TimeSpan duration = set.Duration;
                var mlast = res.Last();
                TimeSpan lstart = TimeSpan.FromSeconds((float)mlast.start / set.SampleRate);
                if (duration.Subtract(lstart).TotalMilliseconds > 10)
                    res.Last().end = set.Wave.Length-1;
                else
                    res.RemoveAt(res.Count - 1);
            }

            foreach (var i in res)
            {
                i.marks = set.Markers.MarkersBetween(i.start, i.end); 
                if (i.marks.Count == 0) throw new Exception("¬¬");
            }
            
            return res;
        }

        public void BeginStartTraining()
        {
            endtraining = false;
            Task.Run(new Action(Train));        
        }
        void Train()
        {
            int n = trainer.CalculateIterations();

            int epoca=0;
            while (!endtraining)
            {                 
                float error = 0;
                for (int i = 0; i < n; i++)
                    error += trainer.Iterate(i);
                error = error / n;

                if (EpochTrained != null)
                    EpochTrained(this, epoca++, error);
            }

        }
        public void BeginEndTraining()
        {
            endtraining = true;        
        }

        /*
        public List<Trainers.TsTrainingInfo> GetInfos()
        {
            var res = new Trainers.TsTrainingInfo()
            {
                Description = this.network.ToString(),
                 Network = "Frequencies.N1",
                 Status=  Trainers.TsNetworksDispatcher.ETrainerStatus.Stopped,
                 TheColor = TsColors.CommonColors.Last(),
                 Type= this.network.GetType().Name
            };

            return new List<Trainers.TsTrainingInfo>() { res };
            
        }
        */
    }
}
