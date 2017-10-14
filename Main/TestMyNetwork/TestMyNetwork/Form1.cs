using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AaBackPropagationFast;
//using AForge.Neuro;
//using AForge.Neuro.Learning;
using System.Diagnostics;

namespace TestMyNetwork
{
    public partial class Form1 : Form
    {
        bool tang = false;
        int n = 10000;
        int w = 300;

        IGeneralizedNetwork[] networks;

        //AldNetwork network;
        //ActivationNetwork anetwork;
        //BackPropagationLearning ateacher;
        //AldNetwork network = new AldNetwork(new int[] { 2, 16,4, 1 }, AldActivationFunctions.TangentHiperbolic, AldActivationFunctions.dTangentHiperbolic, 0.01f, 0.01f);
        Displayer displayer;
        public Form1()
        {
            int[] layersize = { 2, 128, 64, 1 };

            networks = new IGeneralizedNetwork[2];

            networks[0] = new TsFastANN();
            networks[1] = new TsCudaANN();

            networks[0].CreateNetwork(layersize, layersize.Length);
            networks[1].CreateNetwork(layersize, layersize.Length);

            //network = new AldNetwork(new int[] { 2, 16,8, 1 }, tang?AldActivationFunctions.TangentHiperbolic:AldActivationFunctions.Sigmoidal, tang?AldActivationFunctions.dTangentHiperbolic:AldActivationFunctions.dSigmoidal, 0.5f, 2f);
            //anetwork = new ActivationNetwork(new BipolarSigmoidFunction (2), 2, 2, 16, 1);
            //ateacher = new BackPropagationLearning(anetwork);
            //ateacher.LearningRate = 0.1;
            //ateacher.Momentum = 0;
            InitializeComponent();
            displayer = new Displayer();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(Math.Atan2(163.675, -40.5) * 180 / Math.PI + "");

            //MessageBox.Show(Math.Atan(163.675/-40.5) * 180 / Math.PI + "");
        }

        private void btnDo_Click(object sender, EventArgs e)
        {

            PointF[] data;
            bool[] selectors;

            generateData(out data, out selectors, n, w);
            
            Displayer dsp = new Displayer();
            dsp.SetData(data, selectors);
            dsp.Show();
        }
        public void generateData(out PointF[] data,out bool[] selectors,int n, int w)
        { 
            data = new PointF[n];
            selectors = new bool[n];
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < n; i++)
            {
                data[i].X = (float)rnd.NextDouble() * w;
                data[i].Y = (float)rnd.NextDouble() * w;
                selectors[i] = selector(data[i], w);
            }
        }
        public bool selector(PointF point,int w)
        {
            //return point.X > point.Y;


           // return ((int)(point.X/w * 4) + (int)(point.Y/w * 4)) % 2 == 0;

            //return point.X < 100 && point.Y < 100;

            //return point.X > w-point.Y;

            //return point.X > w*0.25 && point.X < w*0.75;

            float dx = w / 2-point.X;
            float dy = w / 2-point.Y;

            float r = (float)Math.Sqrt(dx * dx + dy * dy);

            return (r < 80 && r > 50) || (r < 30 && r > 10);

        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            PointF[] data;
            bool[] selectors;

            generateData(out data, out selectors, n, w);

            aldPlotterPoints1.AddCurve("Error Fast", Color.Red);
            aldPlotterPoints1.AddCurve("Error Cuda", Color.Green);
            
            displayer.Show();

            Task.Run(new Action(delegate {
                Parallel.For(0, networks.Length, new Action<int>(delegate(int idx) {
                    for (int i = 0; i < 1; i++)
                    {
                        float error = trainEpoch(networks[idx], data, selectors);
                        aldPlotterPoints1.AddPoint(1+idx, i, error);
                    }                                
                }));
            }));

        }
        public float trainEpoch(IGeneralizedNetwork network, PointF[] data,bool[] selectors)
        {
            float epocherror = 0;
            for (int i = 0; i < n; i++)
            {
                float val;
                if (tang)
                    val = selectors[i] ? 1 : -1;
                else
                    val = selectors[i] ? 1 : 0;

                epocherror+=network.TrainNetwork(new float[] { data[i].X / w, data[i].Y / w }, new float[] { val }, 0.5f, 0.5f);

                //epocherror += (float)ateacher.Run(new double[] { data[i].X/w, data[i].Y/w }, new double[] { val });
            }
            bool[] res = selectorsFor(network, data);
            if(network is TsCudaANN)
            this.Invoke(new Action(delegate
            {
                displayer.SetData(data, res);
                displayer.Invalidate();
            }));
            return epocherror/n;
        }
        public bool[] selectorsFor(IGeneralizedNetwork network, PointF[] data)
        {
            bool[] res = new bool[data.Length];
            float[] outputs = new float[1];
            for (int i = 0; i < res.Length; i++)
            {
                network.ForwardNetwork(new float[] { data[i].X / w, data[i].Y / w }, outputs, 0.5f);
                float val = outputs[0];
                //if (tang)
                //    res[i] = val >= 0;
                //else
                    res[i] = val > 0.5;
            }
            return res;
        }
        /*
        public bool[] selectorsFor(Network network, PointF[] data)
        {
            bool[] res = new bool[data.Length];
            for (int i = 0; i < res.Length; i++)
            {
                float val = (float)network.Compute(new double[] { data[i].X / w, data[i].Y / w })[0];
                if (tang)
                    res[i] = val >= 0;
                else
                    res[i] = val > 0.5;
            }
            return res;
        }
         * */
        private void btnTest_Click(object sender, EventArgs e)
        {
            /*
            PointF[] data;
            bool[] selectors;
            bool[] selectorsnetwork=new bool[n];

            generateData(out data, out selectors, n, w);

            float[] res = new float[n];
            float[] outputs = new float[1];

            for (int i = 0; i < n; i++)
            {
                res[i] = 
                res[i] = network.Forward(new float[] { data[i].X/w, data[i].Y/w })[0];
                //res[i] = (float)anetwork.Compute(new double[] { data[i].X/w, data[i].Y/w })[0];
                //selectorsnetwork[i] = res[i]>0.5;

                if (tang)
                    selectorsnetwork[i] = res[i] >= 0;
                else
                    selectorsnetwork[i] = res[i] > 0.5;
            }
            
            var hist = AldSpecialAlgorithms.AldAlgorithms.CreateHistogramT(res, 100);

            //aldPlotterPoints2.AddCurve<float>("Histogram", res, Color.Blue);
            aldPlotterPoints2.AddCurve<float>("Histogram", hist.centers,hist.frequencies.Select(x=>(float)x), Color.Blue);

            //MessageBox.Show(ca + " " + cb);

            Displayer dsp = new Displayer();
            dsp.SetData(data, selectorsnetwork);
            dsp.Show();
             * */
        }

        private void aldPlotterPoints1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //AaBackPropagationFast.AldNetwork network = new AldNetwork(new int[] { 1024,2048,32,1 }, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, 0.5f, 0.5f);

            AaBackPropagationFast.AldNetwork network = new AldNetwork(new int[] { 2048*0+1024*0+2, 2048*2*0+512,256, 1 }, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, 0.5f, 0.5f);

            float[] expected=new float[1];

            //expected = network.Forward(new float[] { 1f, 0f });
            
            /*
            network.Train(new float[] { 1f, 0f }, new float[] { 0f });
            network.Train(new float[] { 0f, 1f }, new float[] { 1f });
            network.Train(new float[] { 1f, 1f }, new float[] { 1f });
            */

            float[] inputs = new float[network.N[0].Length];
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 100000; i++)
            {
                int x = i % 4;
                bool res = (x == 1 || x == 2);

                

                network.Train(new float[] { x/2, x%2 }, new float[] { res?1:0 });
                //network.Train(inputs, new float[] { res ? 1 : 0 });
            }

            sw.Stop();

            MessageBox.Show(sw.ElapsedMilliseconds+"");
            //return;

            //MessageBox.Show(network.WeightsToString(0) + "\n" + network.WeightsToString(1) + "\n" + network.WeightsToString(2));

            string sres = "";

            for (int i = 0; i < 4; i++)
            {
                //bool res = (i == 1 || i == 2);
                sres += "\n" + (i / 2) + ";" + (i % 2) + "=" + network.Forward(new float[] { i / 2, i % 2 })[0];
            }

            MessageBox.Show(sres);
            //MessageBox.Show(network.Forward(new float[] { 1f, 0f })[0] + "");

            //MessageBox.Show(network.Errors());

        }
    }
}
