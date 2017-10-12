using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TsFilesTools;

namespace AldFirstNetworkTrainer.Networks
{
    public class TsCudaNetwork:IGeneralizedNetwork
    {
        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern IntPtr cudaANNCreateNetwork(int[] layerssize, int layers);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern bool cudaANNTrain(IntPtr thedata, float[] inputs, float[] expected, float alpha, float learningrate, out float totalerror);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern bool cudaANNForward(IntPtr thedata, float[] inputs, float[] outputs, float alpha);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern bool cudaANNFree(IntPtr thedata);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern bool cudaANNBackup(IntPtr thedata, out IntPtr ptr, out int size);

        [DllImport(@"I:\Tesis P1\Tesis\Tools\MyModules\TsCudaANNDll\Debug\TsCudaANNDll.dll")]
        public static extern bool cudaANNRestore(IntPtr thedata, IntPtr weights, IntPtr bias);

        float _alpha;
        float _learningrate;
        public float Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value;
            }
        }

        public float LearningRate
        {
            get
            {
                return _learningrate;
            }
            set
            {
                _learningrate = value;
            }
        }
        IntPtr thedata;
        int[] layersize;
        int layers;
        bool created = false;

        public void Create(int[] NeuronsPerLayer, float alpha, float learningrate)
        {            
            this._alpha = alpha;
            this._learningrate = learningrate;

            this.layersize = NeuronsPerLayer;
            this.layers = NeuronsPerLayer.Length;
            thedata = cudaANNCreateNetwork(layersize, layers);
            if (thedata == IntPtr.Zero)
                throw new Exception("Error initializing the cuda network...");
            created = true;
        }

        public float Train(float[] inputs, float[] expected)
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");
            float totalerror = 0;
            if (!cudaANNTrain(thedata, inputs, expected, _alpha, _learningrate, out totalerror))
                throw new Exception("Error training the cuda network...");
            return totalerror;
        }

        public float[] Forward(float[] inputs)
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");
            float[] outputs = new float[layersize[layers - 1]];
            if (!cudaANNForward(thedata, inputs, outputs, _alpha))
                throw new Exception("Error with the cuda network...");
            return outputs;
        }

        public void Free()
        {
            if (thedata == IntPtr.Zero)
                throw new Exception("Error the cuda network is null...");
            if (!cudaANNFree(thedata))
                throw new Exception("Error disposing the cuda network...");     
        }



        public int Inputs
        {
            get { return layersize[0]; }
        }

        public int Outputs
        {
            get { return layersize[layers-1]; }
        }


        public TsNetworkBackup Backup(string thenamespace,string thekey,string description="")
        {
            TsNetworkBackup backup = new TsNetworkBackup(thenamespace,thekey);
            backup.Date = DateTime.Now;
            backup.Alpha = this.Alpha;
            backup.Description = description;
            backup.LearningRate = this.LearningRate;
            backup.NetworkType = typeof(TsCudaNetwork);

            IntPtr ptr;
            int size;
            if (!cudaANNBackup(this.thedata, out ptr, out size))
                throw new Exception("Error obtaining the backup of the device.");
            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);
            backup.Data = buffer;

            return backup;
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < this.layers; i++)
            {
                res += "{";
                res += this.layersize[i];
                res += "} ";
            }
            return res;
        }

        public void Restore(TsNetworkBackup data)
        {
            this.Alpha = data.Alpha;
            this.LearningRate = data.LearningRate;

            MemoryStream ms = new MemoryStream();

            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;
            ms.WriteBytes(data.Data);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;

            int layers = ms.ReadInt();
            int[] sizes = new int[layers];

            this.layersize = sizes;

            for (int i = 0; i < layers; i++)
                sizes[i] = ms.ReadInt();

            if (!created)
                this.Create(sizes, this.Alpha, this.LearningRate);
            else
            {
                this.Alpha = data.Alpha;
                this.LearningRate = data.LearningRate;
            }

            int totaln = ms.ReadInt();

            IntPtr bias = ms.ReadBytesIntPtr(totaln * sizeof(float));

            int totalw = ms.ReadInt();

            IntPtr weights = ms.ReadBytesIntPtr(totalw * sizeof(float));

            if (!cudaANNRestore(this.thedata, weights, bias))
                throw new Exception("Error restoring the data into the device.");

            Marshal.FreeHGlobal(bias);
            Marshal.FreeHGlobal(weights);
            ms.Dispose();
        }
        /*
        public bool SaveToStream(System.IO.Stream where)
        {
            IntPtr ptr;
            int size;
            if (!cudaANNBackup(this.thedata, out ptr, out size))
                throw new Exception("Error obtaining the backup of the device.");
            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);
            where.Write(buffer, 0, buffer.Length);
            return true;
        }

        public bool LoadFromStream(System.IO.Stream where)
        {
            int layers = where.ReadInt();
            int[] sizes = new int[layers];

            for (int i = 0; i < layers; i++)
                sizes[i] = where.ReadInt();

            if (!created)
                this.Create(sizes, 0.1f, 0.1f);
            
            int totaln = where.ReadInt();

            IntPtr bias = where.ReadBytesIntPtr(totaln * sizeof(float));

            int totalw = where.ReadInt();

            IntPtr weights = where.ReadBytesIntPtr(totalw * sizeof(float));

            if (!cudaANNRestore(this.thedata, weights, bias))
                throw new Exception("Error restoring the data into the device.");

            Marshal.FreeHGlobal(bias);
            Marshal.FreeHGlobal(weights);      
            return true;
        }
         * */


        public int[] NeuronsPerLayer
        {
            get { return this.layersize; }
        }
    }
}
