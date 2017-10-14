using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestMyNetwork
{
    public unsafe class TsCudaANN:IGeneralizedNetwork
    {
        [DllImport("TsCudaANNDll.dll")]
        public static extern IntPtr cudaANNCreateNetwork(int[] layerssize, int layers);

        [DllImport("TsCudaANNDll.dll")]
        public static extern bool cudaANNTrain(IntPtr thedata,float[] inputs,float[] expected,float alpha,float learningrate,out float totalerror);

        [DllImport("TsCudaANNDll.dll")]
        public static extern bool cudaANNForward(IntPtr thedata, float[] inputs, float[] outputs, float alpha);

        [DllImport("TsCudaANNDll.dll")]
        public static extern bool cudaANNFree(IntPtr thedata);


        [DllImport("TsCudaANNDll.dll")]
        public static extern bool cudaANNTrain(IntPtr thedata, float* inputs, float* expected, float alpha, float learningrate, out float totalerror);


        IntPtr thedata;
        int[] layersize;
        int layers;

        public void CreateNetwork(int[] layerssize, int layers)
        {
            this.layersize = layerssize;
            this.layers = layers;
            thedata = cudaANNCreateNetwork(layerssize, layers);
            if(thedata==IntPtr.Zero)
                throw new Exception("Error initializing the cuda network...");

        }

        public float TrainNetwork(float[] inputs, float[] expected, float alpha, float learningrate)
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");
            float totalerror = 0;
            if(!cudaANNTrain(thedata, inputs, expected, alpha, learningrate, out totalerror))
                throw new Exception("Error training the cuda network...");
            return totalerror;
        }

        public void ForwardNetwork(float[] inputs, float[] outputs, float alpha)
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");



            if(!cudaANNForward(thedata, inputs, outputs, alpha))
                throw new Exception("Error with the cuda network...");
            


        }
        public void FreeNetwork()
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");
            if (!cudaANNFree(thedata))
                throw new Exception("Error disposing the cuda network...");            
        }
    }
}
