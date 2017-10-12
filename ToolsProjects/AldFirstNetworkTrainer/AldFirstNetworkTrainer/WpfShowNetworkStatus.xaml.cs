using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AaBackPropagationFast;

namespace AldFirstNetworkTrainer
{
    /// <summary>
    /// Interaction logic for WpfShowNetworkStatus.xaml
    /// </summary>
    public partial class WpfShowNetworkStatus : Window
    {
        AldNetwork network;
        ScaleTransform scale;
        TranslateTransform translate;

        double prad;
        double rad;

        public WpfShowNetworkStatus()
        {
            InitializeComponent();
            TransformGroup g = new TransformGroup();
            this.theCanvas.LayoutTransform = g;
            scale = new ScaleTransform();
            translate = new TranslateTransform();
            g.Children.Add(scale);
            g.Children.Add(translate);
            Mouse.AddMouseWheelHandler(this, theCanvas_MouseWheel);

        }
        public void LoadData(AldNetwork network)
        {
            this.network=network;
            Generate();
        }
        void Generate()
        {
            prad = 1 / 400.0;
            rad = prad * this.theCanvas.ActualWidth;
            theCanvas.Children.Clear();
            int layers = network.N.Length;
            int wlayers = network.W.Length;

            for (int i = 0; i < wlayers; i++)
                drawWeights(i);
            for (int i = 0; i < layers; i++)
                drawNeurons(i);
            //MessageBox.Show(network.N.Length + " " + network.W.Length);
        }
        private void drawNeurons(int idx)
        {
            int n = this.network.N[idx].Length;

            for (int i = 0; i < n; i++)
            {
                double xfrom, yfrom;
                GetNeuronPosition(idx, i, out xfrom, out yfrom);

                double ww = network.N[idx][i].Bias;
                byte p = WPonderation(ww);

                Ellipse ellipse = new Ellipse();
                //ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
                ellipse.Width = rad * 2;
                ellipse.Height = rad * 2;


                Canvas.SetLeft(ellipse, xfrom-rad);
                Canvas.SetTop(ellipse, yfrom-rad);

                ellipse.Stroke = new SolidColorBrush(Colors.Black);

                if (Math.Abs(ww) < 1e-3)
                    ellipse.Fill = new SolidColorBrush(Colors.Black);
                else if (ww > 0)
                    ellipse.Fill = new SolidColorBrush(Color.FromArgb(p, 0, 0, 255));
                else
                    ellipse.Fill = new SolidColorBrush(Color.FromArgb(p, 255, 0, 0));

                theCanvas.Children.Add(ellipse);
            }
        
        }
        private void drawWeights(int idx)
        {
            int n = this.network.W[idx].Length;
            int nfrom = this.network.N[idx].Length;
            int nto = this.network.N[idx+1].Length;
            for (int i = 0; i < n; i++)
            {
                int fromidx = i / nto;
                int toidx = i % nto;
                double xfrom,yfrom,xto,yto;
                GetNeuronPosition(idx, fromidx,out xfrom,out yfrom);
                GetNeuronPosition(idx+1, toidx, out xto, out yto);
                                
                Line ln = new Line();
                ln.StrokeThickness = 0.5;
                //ln.Fill = new SolidColorBrush(Colors.Black);
                double ww = network.W[idx][i];
                byte p = WPonderation(ww);

                if(Math.Abs(ww)<1e-3)
                    ln.Stroke = new SolidColorBrush(Colors.Transparent);                    
                else if(ww>0)
                    ln.Stroke = new SolidColorBrush(Color.FromArgb((byte)(p/4),0,0,255));
                else
                    ln.Stroke = new SolidColorBrush(Color.FromArgb((byte)(p/4), 255, 0, 0));
                    

                ln.X1 = xfrom; ln.Y1 = yfrom; ln.X2 = xto; ln.Y2 = yto;
                theCanvas.Children.Add(ln);
            }
        }

        private byte WPonderation(double ww)
        {
            double p =  255*Math.Log(Math.Abs(ww));
            if (p > 255) p = 255;
            return (byte)p;
        }

        private void GetNeuronPosition(int idx, int ineuron, out double xfrom, out double yfrom)
        {
            double margin = 0.05;//5%


            double w = this.theCanvas.ActualWidth * (1 - margin*2);
            double h = this.theCanvas.ActualHeight * (1 - margin*2);
            double xini = (this.theCanvas.ActualWidth - w) / 2;
            double yini = (this.theCanvas.ActualHeight - h) / 2;

            yfrom = yini + (h / (network.N[idx].Length + 1)) * (ineuron + 1);

            double d = (w - rad * 2)/(network.N.Length-1);

            xfrom = xini +  rad + d * idx;

        }

        internal void LoadData(Dictionary<string, Networks.IGeneralizedNetwork> dictionary)
        {
            this.cbNetworks.ItemsSource = dictionary;
            this.cbNetworks.DisplayMemberPath = "Key";            
        }

        private void cbNetworks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbNetworks.SelectedValue == null) return;
            LoadData(((KeyValuePair<string,AldNetwork>)cbNetworks.SelectedValue).Value);
        }

        private void theCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            if (e.Delta > 0)
                scale.ScaleX = scale.ScaleY= scale.ScaleY * 1.1*(e.Delta)/120;
            else
                scale.ScaleX = scale.ScaleY = scale.ScaleY / (1.1 * (-e.Delta) / 120);
            this.Title = scale.ScaleX+" "+scale.ScaleY; 
        }
    }
}
