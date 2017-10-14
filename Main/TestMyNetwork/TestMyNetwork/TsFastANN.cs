using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AaBackPropagationFast;

namespace TestMyNetwork
{
    class TsFastANN:IGeneralizedNetwork
    {
        AldNetwork network;

        public void CreateNetwork(int[] layerssize, int layers)
        {
            network = new AldNetwork(layerssize, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, 1, 1);            
        }

        public float TrainNetwork(float[] inputs, float[] expected, float alpha, float learningrate)
        {
            if (network == null)
                throw new Exception("The network is not initialized...");
            
            network.Alpha = alpha;
            network.Temperature = learningrate;

            return network.Train(inputs,expected);

            //return network.tota(expected);
        }

        public void ForwardNetwork(float[] inputs, float[] outputs, float alpha)
        {
            if (network == null)
                throw new Exception("The network is not initialized...");

            network.Alpha = alpha;

            network.Forward(inputs);
        }

        public void FreeNetwork()
        {
            
        }
    }
}
