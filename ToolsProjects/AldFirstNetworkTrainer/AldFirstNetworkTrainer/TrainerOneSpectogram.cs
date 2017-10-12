using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AaBackPropagationFast;
using TsFilesTools;
using AldSpecialAlgorithms;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using TsFFTFramework;
 
namespace AldFirstNetworkTrainer
{
    [Serializable]
    public class TrainerOneSpectogram: ITrainer
    {
        int samplerate;
        int errorMax;
        Networks.IGeneralizedNetwork network;
        int frameNetwork;
        float[] bufferNetwork;
        float[][] resume;

        float[] outputField;
        double resumerate;
        int[] resumeMarks;
        Range[] zeroes;
        float[] errorWindow;

        public Networks.IGeneralizedNetwork TheNetwork { get { return network; } }
        public float[] OutputField { get { return outputField; } }
        public float[] ResumeData { get { return null; } }
        public object GetData { get { return resume; } }

        public bool Modify { get; set; }
        public bool NegateInputs { get; set; }
        public bool NegativeNormalization { get; set; }
        public float Position { get; set; }

        

        int idxZeroes;
        int idxSubZeroes;
        int trainColumns2D;

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
        public float TrainOneTime(int pos, int bugCasesMultiplication)
        {
            float error = 0;
            pos = resumeMarks[pos]-(trainColumns2D-1*0)/2;
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
            pos -= (trainColumns2D-1*0)/2;
            FillBuffer(pos);
            return bufferNetwork;
        }
        public float[] GetNetworkSolution()
        {
            float[] res = new float[resume[0].Length];
            float output;

            for (int i = 0; i < res.Length - trainColumns2D + 1; i++)
            {
                FillBuffer(i);
                output = network.Forward(bufferNetwork)[0];
                res[i + (trainColumns2D - 1*0) / 2] = -output;
            }            
            return res;
        }
        void FillBuffer(int pos)
        {
            int rows = frameNetwork / trainColumns2D;
            int totalrows = resume.Length;
            int start = (totalrows - rows)/2;

            if (!Modify)
                start = 0;
            else
            {
                start = (int)(totalrows * Position);// (int)(totalrows * Position - rows / 2);
            }

            int cont = 0;

            for (int irow = 0; irow < rows; irow++)
                for (int icol = 0; icol < trainColumns2D; icol++)
                {
                    int col = pos + icol;
                    float prop = (float)irow / rows;
                    int row = start + irow; //(int)(prop * totalrows);
                    //bufferNetwork[irow * trainColumns2D + icol] = resume[row][col] / 255f;

                    if (Modify)
                    {
                        int aux;
                        if (NegateInputs)
                            aux = (int)((resume[row][col]) * 255);
                        else
                            aux = (int)((1 - resume[row][col]) * 255);

                        bufferNetwork[cont++] = (float)aux / 255f;
                    }
                    else
                        bufferNetwork[cont++] = resume[row][col];
                }

            if (Modify)
            {
                if (!NegativeNormalization)
                    bufferNetwork.AldFitValuesMinToMax(0);
                else
                    bufferNetwork.StandarDeviation();
            }
                    
        }
        private float TrainNetworkUnit(int pos)
        {
            float[] output = new float[network.Outputs];

            FillBuffer(pos);

            if (bufferNetwork.Count(x => /*x < 0 ||*/ x > 1 || float.IsNaN(x)) > 0) throw new Exception("¬¬ buffer");



            output[0] = outputField[pos + (trainColumns2D-1*0)/2];


            if (bufferNetwork.Count(x => /* x < 0 || */ x > 1 || float.IsNaN(x)) > 0) throw new Exception("¬¬ buffer ");
            if (output.Count(x => /* x < 0 ||  */ x > 1 || float.IsNaN(x)) > 0) throw new Exception("¬¬ output");

            return network.Train(bufferNetwork, output);

            //return  network.TotalError(output);
        }
        public void Create(int errorMax,double resumerate, int trainColumns2D, int samplerate, Networks.IGeneralizedNetwork network)
        {
            this.errorMax = errorMax;
            this.network = network;
            this.samplerate = samplerate;
            this.frameNetwork = network.Inputs;
            bufferNetwork = new float[this.frameNetwork];
            this.errorWindow = Windows.Hanning(1+2*errorMax);
            this.resumerate = resumerate;

            if (trainColumns2D == 0 || frameNetwork % trainColumns2D != 0) throw new Exception("TrainColumns2D must be a divider of FrameNetwork...");
            this.trainColumns2D = trainColumns2D;
        }
        public void LoadData(object data,List<TimeMark> markers) 
        {
            if (resume == (float[][])data) return;
            resume = (float[][])data;
            ResumeMarks(markers);
            CreateZeroes();
            CreateOutputField();
            idxZeroes = 0;
            idxSubZeroes=0;
        }

        private void CreateOutputField()
        {
            outputField = new float[resume[0].Length];
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
                int end = (i==resumeMarks.Length-1)?(resume[0].Length-1):(resumeMarks[i + 1] - errorMax - 1);
                if (start <= end-trainColumns2D)
                    aux.Add(new Range(start, end - start + 1-trainColumns2D));
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
                //var posible = markers[i].MarkPosition / (resume.Length*2);// for the fft
                var posible = (int)(markers[i].MarkPosition / resumerate);// for the fft
                if (i == 0 || posible >= last + errorMax)
                    aux.Add(posible);
                last = posible;
            }
            if(aux.Count>0 && aux[0]==0)
                resumeMarks = aux.Skip(1).ToArray();
            else
                resumeMarks = aux.ToArray();
        }

        private void SaveTrain(float[] bufferNetwork, int pos)
        {
            string path = "E:\\train\\" + pos + ".bmp";
            if (File.Exists(path)) return;
            int rows = frameNetwork / trainColumns2D;
            Bitmap bmp = new Bitmap(trainColumns2D, rows, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var p = bmp.Palette;
            for (int i = 0; i < 256; i++)
                p.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = p;
            Rectangle rect = new Rectangle(0, 0, trainColumns2D, rows);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int stride = data.Stride;
            byte[] bytes = new byte[stride * rows];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < trainColumns2D; j++)
                    bytes[i * stride + j] = (byte)(255 * bufferNetwork[i * trainColumns2D + j]);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            bmp.UnlockBits(data);

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            bmp.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);

            bmp.Dispose();
            fs.Dispose();
        }
    }    
}