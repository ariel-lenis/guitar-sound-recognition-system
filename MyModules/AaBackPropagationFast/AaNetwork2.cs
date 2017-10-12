using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AaBackPropagationFast
{
    [Serializable]
    public class AldNetwork2
    {
        public Func<float, float, float> Activation;
        public Func<float, float, float> dActivation;
        Random rnd;
        public float temperature;
        public float Temperature { get { return temperature; } set { temperature = value; } }


        public float Alpha { get; set; }

        float[][] W;

        bool[][] Mask;


        AaNeuron[][] N;

        float[] R;
        float[] E;

        public int Inputs { get { return N[0].Length; } }
        public int Outputs { get { return N[N.Length - 1].Length; } }
        float randomamplitude;

        int divisions;

        public AldNetwork2(int[] NeuronsPerLayer, Func<float, float, float> Activation, Func<float, float, float> dActivation, float temperature, float alpha, float randomamplitude = 1,int divisions=0)
        {
            this.randomamplitude = randomamplitude;
            if (NeuronsPerLayer == null || NeuronsPerLayer.Length < 2) throw new Exception("The neurons per layer has a minimun of 2.");
            if (temperature <= 0) throw new Exception("The sigmoidal function must be greater than 0.");

            Alpha = alpha;

            rnd = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);

            this.Activation = Activation;
            this.dActivation = dActivation;
            this.temperature = temperature;

            W = new float[NeuronsPerLayer.Length - 1][];
            Mask = new bool[NeuronsPerLayer.Length - 1][];

            N = new AaNeuron[NeuronsPerLayer.Length][];

            for (int i = 0; i < NeuronsPerLayer.Length; i++)
            {
                if (i < NeuronsPerLayer.Length - 1)
                {
                    W[i] = new float[NeuronsPerLayer[i] * NeuronsPerLayer[i + 1]];
                    Mask[i] = new bool[NeuronsPerLayer[i] * NeuronsPerLayer[i + 1]];
                    for (int j = 0; j < W[i].Length; j++)
                    {
                        W[i][j] = (float)Random();
                        if(divisions==0)
                            Mask[i][j] = true;
                    }
                }
                N[i] = new AaNeuron[NeuronsPerLayer[i]];
                for (int j = 0; j < N[i].Length; j++)
                {
                    if (i > 0) N[i][j].Bias = (float)Random();//hidden or output
                    else N[i][j].Bias = 1;//input
                    N[i][j].LastInputsSum = 0;
                    N[i][j].LastActivationOutput = 0;
                    N[i][j].LastLocalGradient = 0;
                }
            }

            this.divisions = divisions;

            if (divisions > 0)
                CreateDivisions();

            R = new float[N[N.Length - 1].Length];
            E = new float[N[N.Length - 1].Length];
        }

        private void CreateDivisions()
        {
            for (int i = 0; i < N.Length - 2; i++)            
                for (int j = 0; j < N[i].Length; j++)
                {
                    int jdiv = j/(N[i].Length / divisions);
                    for (int k = 0; k < N[i + 1].Length; k++)
                    {
                        int kdiv = k / (N[i+1].Length / divisions);
                        if (kdiv != jdiv) continue;
                        Mask[i][j * N[i + 1].Length + k] = true;
                    }
                }
            for (int i = 0; i < Mask[Mask.Length - 1].Length; i++)
            {
                Mask[Mask.Length - 1][i] = true;
            }
        }
        public float[] Forward(float[] inputValues)
        {
            if (inputValues.Length != N[0].Length) throw new Exception("The size of inputs values is not the same.");

            //Start InputLayer
            for (int i = 0; i < inputValues.Length; i++)
                N[0][i].LastActivationOutput = inputValues[i];

            //Update LayerByLayer
            for (int i = 1; i < N.Length; i++)
                UpdateForward(i, temperature);

            //Compute the output
            for (int i = 0; i < R.Length; i++)
                R[i] = N[N.Length - 1][i].LastActivationOutput;
            return R;
        }
        private void UpdateForward(int idx, float temperature)
        {
            for (int i = 0; i < N[idx].Length; i++)
            {
                float sum = SumInputs(idx, i);
                if (float.IsNaN(sum)) throw new Exception("wtf");
                N[idx][i].LastInputsSum = sum;

                float lao = this.Activation(sum, Alpha);
                if (float.IsNaN(lao)) throw new Exception("wtf");
                N[idx][i].LastActivationOutput = lao;
            }
        }

        private float SumInputs(int layer, int neuron)
        {
            if (layer == 0) throw new Exception("wtf!!!");
            float res = 0;
            int nx = N[layer].Length;

            int from = 0;
            int size = N[layer - 1].Length;
            /*
            if (divisions>0 && layer<N.Length-1)
            {
                int idx = neuron / (N[layer].Length / divisions);
                int prediv = N[layer - 1].Length / divisions;
                from = idx * prediv;
                size = prediv;
            }
            */
            for (int i = from; i < from+size; i++)
                if (Mask[layer - 1][i * nx + neuron])
                    res += W[layer - 1][i * nx + neuron] * N[layer - 1][i].LastActivationOutput;            

            return res + N[layer][neuron].Bias;
        }
        public void Train(float[] inputValues, float[] expectedOutputValues)
        {
            var outputlayer = N[N.Length - 1];
            if (expectedOutputValues.Length != outputlayer.Length) throw new Exception("The size of outputs values is not the same.");

            //First compute the outputs
            Forward(inputValues);

            //Next we need to calculate the error for every neuron:

            //Start setting the expected outputs on the last layer/output layer and calculating the total error
            var totalerror = TotalError(expectedOutputValues);

            //All except input layer
            for (int i = N.Length - 1; i >= 1; i--)
                CalculateErrorGradientLocal(i);

            //Next is the time to adjust the weights in the network, all except input layer
            for (int i = 1; i < N.Length; i++)
                AdjustWeights(i);
        }
        private void AdjustWeights(int layer)
        {
            if (layer == 0) throw new Exception("The input layer cant be used.");
            var theLayer = N[layer];
            var preLayer = N[layer - 1];

            float delta;

            for (int ito = 0; ito < theLayer.Length; ito++)
            {
                for (int ifrom = 0; ifrom < preLayer.Length; ifrom++)
                {
                    delta = temperature * theLayer[ito].LastLocalGradient * preLayer[ifrom].LastActivationOutput;
                    if (float.IsNaN(delta)) throw new Exception("wft2");

                    if (Mask[layer - 1][ifrom * theLayer.Length + ito])
                        W[layer - 1][ifrom * theLayer.Length + ito] += delta;
                }
                delta = temperature * theLayer[ito].LastLocalGradient * 1;//bias
                if (float.IsNaN(delta)) throw new Exception("wft2");
                theLayer[ito].Bias += delta;
            }
        }
        private void CalculateErrorGradientLocal(int layer)
        {
            if (layer == 0) throw new Exception("The input layer cant be used.");
            var theLayer = N[layer];
            for (int i = 0; i < theLayer.Length; i++)
            {
                if (layer < N.Length - 1)
                {
                    float dF = dActivation(theLayer[i].LastInputsSum, Alpha);
                    float sum = SumOutputsGradient(layer, i);
                    float llg = dF * sum;
                    if (double.IsNaN(llg))
                        throw new Exception("wtf3");
                    theLayer[i].LastLocalGradient = llg;
                }
                else
                    theLayer[i].LastLocalGradient = E[i] * dActivation(theLayer[i].LastInputsSum, Alpha);
            }
        }
        private float SumOutputsGradient(int layer, int neuron)
        {
            if (layer == N.Length - 1) throw new Exception("The last layer havent a output.");
            float res = 0;

            var theLayer = N[layer + 1];
            int ny = theLayer.Length;

            int from = neuron * ny;
            int size = (neuron + 1) * ny;

            /*
            if(divisions>0)
            {
                int currentidx = neuron / (N[layer].Length / divisions);
                int nextdiv = theLayer.Length / divisions;

                from += nextdiv * currentidx;
                size = nextdiv;
            }
            */

            for (int i = from; i < size; i++)
                if (Mask[layer][i])
                    res += W[layer][i] * theLayer[i % ny].LastLocalGradient;
            return res;
        }
        public float TotalError(float[] expected)
        {
            var outputlayer = N[N.Length - 1];
            if (expected.Length != outputlayer.Length) throw new Exception("The size of the output and the desired vector is not the same.");

            float totalerror = 0;

            for (int i = 0; i < outputlayer.Length; i++)
            {
                float e = expected[i] - outputlayer[i].LastActivationOutput;
                E[i] = e;
                totalerror += e * e;
            }
            return 0.5f * totalerror;
        }
        public float Random()
        {

            float a = (float)(rnd.NextDouble() - 0.25);

            if (a < 0) a -= 0.25f;
            else a += 0.25f;

            return a * randomamplitude;

            //return 0.5f;
            //return (float)(rnd.NextDouble() - 0 * 0.5);
        }
        public AldNetwork GetCopy
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formater = new BinaryFormatter();
                formater.Serialize(ms, this);

                ms.Position = 0;
                ms.Seek(0, SeekOrigin.Begin);

                var res = (AldNetwork)formater.Deserialize(ms);
                ms.Dispose();
                return res;
            }
        }
    }
}
