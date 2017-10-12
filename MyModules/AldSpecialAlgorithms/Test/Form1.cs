using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AldFastFourierTransform;
using ZedGraph;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double Fs = 1000;             // Sampling frequency
            double T = 1/Fs;                     // Sample time
            int L = 1000;                     // Length of signal

            double[] x = new double[L];
            double[] y = new double[L];

            Random rnd = new Random();

            for(int i=0;i<L;i++)
            {
                double t = i*T;                // Time vector
                // Sum of a 50 Hz sinusoid and a 120 Hz sinusoid
                double yv = 20*Math.Sin(2*Math.PI*100*t) + 35*Math.Sin(2*Math.PI*50*t) + Math.Sin(2*Math.PI*120*t)+Math.Sin(2*Math.PI*200*t)+rnd.NextDouble()-0.5; 
                //y = x + 2*randn(size(t));     // Sinusoids plus noise
                x[i] = t;
                y[i] = yv;
            }

            Plot(plotData, x, y);

            y = FastFourierTransform.FFT(y).Select(u=>2*u/L).Take(y.Length/2).ToArray();
            x = new double[y.Length];

            for (int i = 0; i < x.Length; i++)
                x[i] = i*(1.0 / x.Length) * Fs / 2;
            //x = x.Select(u => u*Fs/2).Take(x.Length/2).ToArray();


            Plot(plotFFT, x, y);

        }

        private void Plot(ZedGraphControl where, double[] x, double[] y)
        {

            LineItem curvaGrafico = where.GraphPane.CurveList[0] as LineItem;
            IPointListEdit lista = curvaGrafico.Points as IPointListEdit;

            for (int i = 0; i < x.Length; i++)
                lista.Add(x[i], y[i]);

            Scale xScale = where.GraphPane.XAxis.Scale;
            xScale.Min = x.Min();
            xScale.Max = x.Max();

            Scale yScale = where.GraphPane.YAxis.Scale;
            yScale.Min = y.Min();
            yScale.Max = y.Max();

            where.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GraphPane Grafico = this.plotData.GraphPane; //Títulos de los gráficos
            
            RollingPointPairList Lista = new RollingPointPairList(1200000);
            LineItem curva = Grafico.AddCurve("F(x)", Lista, Color.Red, SymbolType.None); // Se controla manualmente que el rango del eje X está continuamente

            Grafico.XAxis.Scale.MinorStep = 0.01;
            Grafico.XAxis.Scale.MajorStep = 0.1; //Escalar los ejes

            Grafico.AddCurve("Ejes X,Y", new double[] { 0, 0 }, new double[] { 1000, -1000 }, Color.Black); // Se controla manualmente que el rango del eje X está continuamente        


            Grafico = this.plotFFT.GraphPane; //Títulos de los gráficos

            Lista = new RollingPointPairList(1200000);
            curva = Grafico.AddCurve("FFT", Lista, Color.Green, SymbolType.None); // Se controla manualmente que el rango del eje X está continuamente

            Grafico.XAxis.Scale.MinorStep = 1;
            Grafico.XAxis.Scale.MajorStep = 1; //Escalar los ejes

            Grafico.AddCurve("Ejes X,Y", new double[] { 0, 0 }, new double[] { 1000, -1000 }, Color.Black); // Se controla manualmente que el rango del eje X está continuamente        


        }
    }
}
