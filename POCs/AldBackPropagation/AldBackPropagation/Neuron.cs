using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldBackPropagation
{
    [Serializable]
    public abstract class Neuron
    {
        public List<Link> Inputs;
        public List<Link> Outputs;

        public abstract float ActivationOutput();
        public abstract float LocalGradient();

        public float LastInputsSum;
        public float LastActivationOutput;
        public float LastLocalGradient;

        public AldNetwork TheNetwork;

        public Neuron(AldNetwork TheNetwork)
        {
            this.TheNetwork = TheNetwork;
        }
        public override string ToString()
        {
            int inputs = 0, outputs = 0;

            if (Inputs != null) inputs = Inputs.Count;
            if (Inputs != null) outputs = Outputs.Count;

            return this.GetType().Name + ",I:" + inputs + ",O=" + outputs;
        }
    }
    [Serializable]
    public class NeuronInput : Neuron
    {
        public NeuronInput(AldNetwork TheNetwork)
            : base(TheNetwork)
        {
            Outputs = new List<Link>();
        }
        public override float ActivationOutput()
        {
            return LastActivationOutput;
        }
        public override float LocalGradient()
        {
            return LastLocalGradient = 0;
        }
    }
    [Serializable]
    public class NeuronBias : Neuron
    {
        public NeuronBias(AldNetwork TheNetwork)
            : base(TheNetwork)
        {
            LastActivationOutput = 1;
            Outputs = new List<Link>();
        }
        public override float ActivationOutput()
        {
            LastActivationOutput = 1;
            return 1;
        }

        public override float LocalGradient()
        {
            return LastLocalGradient = 0;
        }
    }
    [Serializable]
    public class NeuronHidden : Neuron
    {
        public NeuronHidden(AldNetwork TheNetwork)
            : base(TheNetwork)
        {
            Inputs = new List<Link>();
            new Link(TheNetwork.bias, this, TheNetwork.Random());
            Outputs = new List<Link>();
        }
        public override float ActivationOutput()
        {
            float sum = (float)Inputs.Sum(x => x.GetInputValue());
            LastInputsSum = sum;
            if (float.IsNaN(sum)) throw new Exception("wtf");
            LastActivationOutput = this.TheNetwork.Activation(sum, TheNetwork.temperature);
            if (float.IsNaN(LastActivationOutput)) throw new Exception("wtf");
            return LastActivationOutput;
        }
        public override float LocalGradient()
        {
            float dF = TheNetwork.dActivation(LastInputsSum, TheNetwork.temperature);
            float sum = 0;
            foreach (var i in Outputs)
                sum += i.Weight * i.To.LastLocalGradient;
            LastLocalGradient = dF * sum;
            if (double.IsNaN(LastLocalGradient))
                throw new Exception("wtf3");
            return LastLocalGradient;
        }
        public void AdjustWeights(float temperature)
        {
            foreach (var i in Inputs)
                i.AdjustWeigth(temperature);
        }
    }
    [Serializable]
    public class NeuronOutput : NeuronHidden
    {
        public float LastError;
        public NeuronOutput(AldNetwork TheNetwork)
            : base(TheNetwork)
        {

        }
        public override float LocalGradient()
        {
            return LastLocalGradient = LastError * TheNetwork.dActivation(LastInputsSum, TheNetwork.temperature);
        }
        public float GetError(float d)
        {
            return LastError = d - LastActivationOutput;
        }
    }
}
