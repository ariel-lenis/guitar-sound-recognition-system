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
using System.Diagnostics;

namespace borrarCompareANN
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int layers = 5;
            int[] layerssize = new int[layers];
            layerssize[0] = 10;
            layerssize[1] = 5;
            layerssize[2] = 30;
            layerssize[3] = 20;
            layerssize[4] = 10;

            AldNetwork network = new AldNetwork(layerssize, AldActivationFunctions.Sigmoidal, AldActivationFunctions.dSigmoidal, 0.05f, 0.05f);
            float[] inputs = new float[10];
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = i / 1000;
            var results = network.Forward(inputs);

            for (int i = 0; i < results.Length; i++)
                Debug.WriteLine(results[i]);


        }
    }
}
