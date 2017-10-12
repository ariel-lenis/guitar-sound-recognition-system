using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AldSpecialAlgorithms.LPC;

namespace AldLPCProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float[] data = new float[1024];
            int p = 12;
            Random rnd= new Random();

            for (int i = 0; i < data.Length/3; i++)
                data[i] = (float)(2f*Math.Sin(2 * Math.PI * i / 64) + (float)Math.Sin(2*2 * Math.PI * i / 64)+(float)(0*rnd.NextDouble()));

            for (int i = data.Length/3; i < data.Length*2/3; i++)
                data[i] = (float)(2f*Math.Sin(3.5 * Math.PI * i / 64) + (float)Math.Sin(0.5 * 2 * Math.PI * i / 64) + (float)(0 * rnd.NextDouble()));
           
            for (int i = data.Length*2 / 3; i < data.Length ; i++)
                data[i] = (float)(2f * Math.Sin(2.5 * Math.PI * i / 64) + (float)Math.Sin(0.1 * 2 * Math.PI * i / 64) + (float)(0 * rnd.NextDouble()));


            float[] emph = new float[data.Length];
            for (int i = 1; i < emph.Length; i++)
                emph[i] = data[i - 1] - data[i] * 0.95f;


            AldLPC.LPCResult res;
           
            /*
            res = AldLPC.LPCAutocorrelation(data, p);
            for (int i = 0; i < res.Alphas.Length; i++)
                Console.WriteLine("Alpha[{0}]={1}",i,res.Alphas[i]);
            Console.WriteLine("Error={0}", res.Error);
            Console.WriteLine();
            
            
            res = AldLPC.LPCSolve(data, p);
            for (int i = 0; i < res.Alphas.Length; i++)
                Console.WriteLine("Alpha[{0}]={1}", i, res.Alphas[i]);
            Console.WriteLine("Error={0}", res.Error);
            Console.WriteLine();
            


            res = AldLPC.LPC(data, p);
            for (int i = 0; i < res.Alphas.Length; i++)
                Console.WriteLine("Alpha[{0}]={1}", i, res.Alphas[i]);
            Console.WriteLine("Error={0}", res.Error);

            Console.WriteLine();
            */

            res = AldLPC.LPCMatlab(emph, p);
            for (int i = 0; i < res.Alphas.Length; i++)
                Console.WriteLine("Alpha[{0}]={1}", i, res.Alphas[i]);
            Console.WriteLine("Error={0}", res.Error);


            var waveLPC = new float[emph.Length];

            for (int i = res.Alphas.Length; i < emph.Length; i++)
            {
                float sum = 0;
                for (int j = 1; j < res.Alphas.Length; j++)
                    sum += res.Alphas[j] * emph[i - j];
                waveLPC[i] = -sum;
            }

            var x = new float[emph.Length];
            for (int i = 0; i < x.Length; i++)
                x[i] = i;

            float[] error = new float[emph.Length];
            for (int i = 0; i < emph.Length; i++)
                error[i] = emph[i] - waveLPC[i];


            // aldPlotterPoints1.AddCurve("LPC",x, waveLPC, Color.Blue);
            //aldPlotterPoints1.AddCurve("Onda",x, data, Color.Red);
            aldPlotterPoints1.AddCurve("Error", x, error, Color.DarkGreen);
            //aldPlotterPoints1.AddCurve("Emph", x, emph, Color.Gold);

        }
    }
}
