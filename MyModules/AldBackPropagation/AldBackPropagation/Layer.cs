using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldBackPropagation
{
    [Serializable]
    public class Layer : List<Neuron>
    {
        public enum ELayerType { Input, Hidden, Output };

        private AldNetwork theNetwork;
        private ELayerType layerType;

        public ELayerType LayerType { get { return layerType; } }
        public AldNetwork TheNetwork { get { return theNetwork; } }

        public Layer(ELayerType layerType, int Neurons, Layer previous, AldNetwork theNetwork)
        {
            if (Neurons == 0) throw new Exception("The neurons number for a la layer must be almost 1.");

            this.layerType = layerType;
            this.theNetwork = theNetwork;

            for (int i = 0; i < Neurons; i++)
            {
                Neuron theNeuron = null;

                switch (LayerType)
                {
                    case ELayerType.Input:
                        theNeuron = new NeuronInput(theNetwork);
                        break;
                    case ELayerType.Hidden:
                        theNeuron = new NeuronHidden(theNetwork);
                        break;
                    case ELayerType.Output:
                        theNeuron = new NeuronOutput(theNetwork);
                        break;
                }

                this.Add(theNeuron);
                if (previous != null)
                    previous.ConnectAllWith(theNeuron);
            }
        }
        private void ConnectAllWith(Neuron theNeuron)
        {
            foreach (var i in this)
                new Link(i, theNeuron, theNetwork.Random());
        }
        public void UpdateForward()
        {
            foreach (var i in this)
                i.ActivationOutput();
        }
        public float[] GetLayerValues()
        {
            return this.Select(x => x.LastActivationOutput).ToArray();
        }

        public void CalculateErrorGradientLocal()
        {
            if (layerType == ELayerType.Input) throw new Exception("The Error Gradient is not for input layers.");
            foreach (var i in this)
                i.LocalGradient();
        }

        public void AdjustWeights(float temperature)
        {
            if (layerType == ELayerType.Input) throw new Exception("The adjust weights is not for input layers.");
            foreach (var i in this)
                (i as NeuronHidden).AdjustWeights(temperature);
        }
    }
}
