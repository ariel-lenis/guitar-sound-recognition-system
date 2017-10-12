using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldBackPropagation
{
    [Serializable]
    public class Link
    {
        public Neuron From, To;
        public float Weight;

        public Link(Neuron from, Neuron to, float weight)
        {
            this.Weight = weight;
            this.From = from;
            this.To = to;

            from.Outputs.Add(this);
            to.Inputs.Add(this);
        }

        public float GetInputValue()
        {
            var res =  this.Weight * From.LastActivationOutput;
            if (float.IsNaN(res)) throw new Exception("wtf");
            return res;
        }

        public override string ToString()
        {
            return "Link,W:" + this.Weight;
        }

        public void AdjustWeigth(float temperature)
        {
            float delta = temperature * To.LastLocalGradient * From.LastActivationOutput;
            if (float.IsNaN(delta)) throw new Exception("wft2");
            this.Weight += delta;
        }
    }
}
