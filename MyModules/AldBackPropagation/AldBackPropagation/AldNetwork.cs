using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AldBackPropagation
{
    [Serializable]
    public class AldNetwork: List<Layer>
    {
        public Func<float, float, float> Activation;
        public Func<float, float, float> dActivation;
        public NeuronBias bias;
        Random rnd;
        public float temperature;
        public float Temperature { get { return temperature; } set { temperature = value; } }


        public int Inputs { get { return this[0].Count; } }

        public AldNetwork(int[] NeuronsPerLayer, Func<float, float, float> Activation, Func<float, float, float> dActivation, float temperature)
        {
            if (NeuronsPerLayer == null || NeuronsPerLayer.Length < 2) throw new Exception("The neurons per layer has a minimun of 2.");
            if (temperature <= 0) throw new Exception("The sigmoidal function must be greater than 0.");

            rnd = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);
            bias = new NeuronBias(this);

            this.Activation = Activation;
            this.dActivation  =dActivation;
            this.temperature = temperature;

            Layer previous = null;
            for (int i = 0; i < NeuronsPerLayer.Length; i++)
            {
                Layer.ELayerType type = Layer.ELayerType.Hidden;
                if (i == 0) type = Layer.ELayerType.Input;
                if (i == NeuronsPerLayer.Length - 1) type = Layer.ELayerType.Output;
                previous = new Layer(type, NeuronsPerLayer[i], previous,this);
                this.Add(previous);
            }
        }
        public float[] Forward(float[] inputValues)
        {
            if (inputValues.Length != this[0].Count) throw new Exception("The size of inputs values is not the same.");

            //Start InputLayer
            for (int i = 0; i < inputValues.Length; i++)
                this[0][i].LastActivationOutput = inputValues[i];
            
            //Update LayerByLayer
            for (int i = 1; i < this.Count; i++)            
                this[i].UpdateForward();

            //Compute the error
            return this.Last().GetLayerValues();
        }
        public void Train(float[] inputValues, float[] expectedOutputValues/*,float temperature*/)
        {
            //float temperature = 1;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var outputlayer = this.Last();
            if (expectedOutputValues.Length != outputlayer.Count) throw new Exception("The size of outputs values is not the same.");

            //First compute the outputs
            Forward(inputValues);
            
            //Next we need to calculate the error for every neuron:

            //Start setting the expected outputs on the last layer/output layer and calculating the total error
            var totalerror = TotalError(expectedOutputValues);
            
            //All except input layer
            for (int i = this.Count - 1; i >= 1; i--)
                this[i].CalculateErrorGradientLocal();

            //Next is the time to adjust the weights in the network, all except input layer
            for (int i = 1; i <this.Count; i++)
                this[i].AdjustWeights(temperature);
            /*
            Console.WriteLine("Time : \t" + sw.Elapsed.TotalMilliseconds);
            Console.WriteLine("Old :");
            Console.WriteLine("\tSum:\t" + this.Last().First().LastInputsSum);
            Console.WriteLine("\tOut:\t" + this.Last().First().LastActivationOutput);
            Console.WriteLine("\tGrad:\t" + this.Last().First().LastLocalGradient);
            Console.WriteLine("\tBias:\t" + this.Last().First().Inputs.First(x=>x.From is NeuronBias).Weight);
            */
        }
        public float TotalError(float[] expected)
        {
            var outputlayer = this.Last();
            if (expected.Length != outputlayer.Count) throw new Exception("The size of the output and the desired vector is not the same.");

            float totalerror = 0;
            float e;
            for (int i = 0; i < outputlayer.Count; i++)
            {
                e = (outputlayer[i] as NeuronOutput).GetError(expected[i]);
                totalerror += e*e;
            }
            return 0.5f * totalerror;            
        }
        public float Random()
        {
            //return 0.5f;
            return  (float)(rnd.NextDouble() - 0 * 0.5);
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

                AldNetwork res = (AldNetwork)formater.Deserialize(ms);
                ms.Dispose();
                return res;
            }            
        }
    }
}
