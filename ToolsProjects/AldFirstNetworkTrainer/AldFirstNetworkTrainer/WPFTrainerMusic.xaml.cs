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

namespace AldFirstNetworkTrainer
{
    /// <summary>
    /// Interaction logic for WPFTrainerMusic.xaml
    /// </summary>
    public partial class WPFTrainerMusic : Window
    {
        BitmapImage imgkey;
        public WPFTrainerMusic()
        {
            InitializeComponent();
            imgkey = new BitmapImage(new Uri(@"img\\MusicKey.png", UriKind.Relative));
            //Console.WriteLine(imgkey.Width);
            this.SourceInitialized += WPFTrainerMusic_SourceInitialized;
            this.SizeChanged += canvasAll_SizeChanged;
        }

        void canvasAll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasAll.Children.Clear();
            DrawCanvas();
        }

        void WPFTrainerMusic_SourceInitialized(object sender, EventArgs e)
        {
//            DrawCanvas();
        }
        public void DrawCanvas()
        {
            Image key = new Image();
           
            key.Source = imgkey;           
            key.Width = 25;
            key.Height = 1.0f*key.Width*imgkey.Height/imgkey.Width;
            
            Canvas.SetLeft(key, 10);
            Canvas.SetTop(key, 49);

            

            for (int i = 0; i < 5; i++)
            {
                Line line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.X1 = 0;
                line.X2 = canvasAll.ActualWidth;
                line.Y1 = line.Y2 = 50+16*i;
                line.StrokeThickness = 2;
                canvasAll.Children.Add(line);
            }
            canvasAll.Children.Add(key);
        }
    }
}
