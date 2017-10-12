using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMyNetwork
{
    public interface IGeneralizedNetwork
    {
        void CreateNetwork(int[] layerssize, int layers);
        float TrainNetwork(float[] inputs, float[] expected, float alpha, float learningrate);
        void ForwardNetwork(float[] inputs, float[] outputs, float alpha);
        void FreeNetwork();
    }
}
