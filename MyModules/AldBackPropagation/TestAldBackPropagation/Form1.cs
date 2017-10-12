using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BP = AldBackPropagation;

namespace TestAldBackPropagation
{
    public partial class Form1 : Form
    {
        BP.AldNetwork network;

        List<double> errors;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            network = new BP.AldNetwork(new int[]{2,1}, BP.AldActivationFunctions.Sigmoidal, BP.AldActivationFunctions.dSigmoidal,1);
            errors = new List<double>();

            Task.Run(() => { Training(); });
        }
        public void Training()
        {
            double error=0;
            for (int i = 0; i < 4*100; i++)
            {
                float a = (i % 4)/2;
                float b = i % 2;

                float c = a + b;

                if (c > 1) c = 1;

                network.Train(new float[] { a, b }, new float[] { c }, 10);                
                error+= network.TotalError(new float[]{c});
                if (i % 4 == 0 && i > 0)
                {
                    errors.Add(error / 4);
                    error = 0;
                }
            }

            this.Invoke(new Action(() => {
                /*
                var linkA = network[1][0].Inputs[0];
                var linkB = network[1][0].Inputs[1];
                var linkC = network[1][0].Inputs[2];
                Console.WriteLine(linkA.Weight + " " + linkB.Weight + " " + linkC.Weight+" "+(linkA == linkB));*/
                aldPlotterPoints1.ClearCurves();
                aldPlotterPoints1.AddCurve("avgError", errors, Color.Blue, AldExtraControls.AldPlotterPoints.AdaptedSymbolType.None);
                this.lblActualError.Text = "E:" + errors.Last();
            }));
        }

        private bool isPrime(ushort x)
        {
            int limit = (int)Math.Sqrt(x);
            for (int i = 2; i < x; i++)
                if (x % i == 0) return false;
            return true;
        }
        private byte CountUnos(byte i)
        {
            byte res = 0;
            while (i > 0)
            {
                if ((i & 1) == 1) res++;
                i = (byte)(i >> 1);
            }
            return res;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.propertyGrid1.SelectedObject = aldPlotterPoints1;
        }


        private float[] GetBits(ushort p)
        {
            float[] res = new float[16];
            for (int i = 0; i < 16; i++)
            {
                res[i] = p % 2;
                p /= 2;
            }
            return res;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {/*
            timer2.Stop();

            if (t != null)
            {
                terminate = true;
                while (t.IsAlive) 
                    Thread.Sleep(10);
            }
            if (network == null) return;

            BinaryFormatter f=new BinaryFormatter();
            var filestream = new FileStream("d:\\serializado.red", FileMode.OpenOrCreate);

            f.Serialize(filestream, network);

            filestream.Dispose();
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            /*
            t.Suspend();
            if (errors.Count > 0)
            {
                aldPlotterPoints1.ClearCurves();
                aldPlotterPoints1.AddCurve("avgError", errors, Color.Blue, AldExtraControls.AldPlotterPoints.AdaptedSymbolType.None);
                this.lblActualError.Text = "E:" + errors.Last();
            }
            t.Resume();*/
        }

        private void aldPlotterPoints1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            BinaryFormatter f = new BinaryFormatter();            
            var filestream = new FileStream("d:\\serializado.red", FileMode.OpenOrCreate);
            network = (BP.AldNetwork)f.Deserialize(filestream);
            filestream.Dispose();

            var output = network.Forward(GetBits(7921));
            string res = "";
            foreach (var i in output)
                res += i + " ";
            MessageBox.Show(res);
            
        }


    }
}
