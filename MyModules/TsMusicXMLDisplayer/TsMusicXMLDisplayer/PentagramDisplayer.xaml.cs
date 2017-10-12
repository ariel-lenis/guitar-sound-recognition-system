using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AldWavDisplayTools;
using TsExtraControls;
using System.Reflection;
using System.ComponentModel;

namespace TsMusicXMLDisplayer
{
	/// <summary>
	/// Interaction logic for PentagramDisplayer.xaml
	/// </summary>
	public partial class PentagramDisplayer : UserControl
	{
        GDI.PentagramGDI pentagramGDI;        

		public PentagramDisplayer()
		{
			this.InitializeComponent();
            this.Background = new SolidColorBrush(Colors.Green);
            if (!DesignerProperties.GetIsInDesignMode(this))
                pentagramGDI = new GDI.PentagramGDI();
		}
        /*
        public void DrawPentagram()
        {
            this.canvasA.Background = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
            this.canvasB.Background = new SolidColorBrush(Color.FromRgb(0xe0, 0xff, 0xff));
            float padding = 10;
            double dh = this.canvasA.ActualHeight/15.0;
            double st = this.canvasA.ActualHeight / 2 - 2.5 * dh;

            for (int i = 0; i < 5; i++)
            {
                Line line = new Line();
                line.X1 = padding;
                line.Y1 = st+i * dh;

                line.X2 = this.canvasA.ActualWidth - padding;
                line.Y2 = st+i * dh;

                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 2;

                this.canvasA.Children.Add(line);
            }
        }
        */
        private void canvasA_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //DrawPentagram();
        }
        AldBitmapSourceCreator source;
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiX = (int)dpiXProperty.GetValue(null, null);
                var dpiY = (int)dpiYProperty.GetValue(null, null);

                source = new AldBitmapSourceCreator((int)(this.gridA.CurrentWidth()), (int)this.gridA.CurrentHeight());
                pentagramGDI.Resize((int)(this.gridA.CurrentWidth()), (int)this.gridA.CurrentHeight());
                source.DrawImage(pentagramGDI.Bitmap);
                this.imageA.Source = source.Bmp;
            }

        }
	}
}