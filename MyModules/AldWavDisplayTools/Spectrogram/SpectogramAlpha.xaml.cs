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
using TsExtraControls;

namespace AldWavDisplayTools
{
	public partial class SpectogramAlpha : UserControl
	{
        float[] data;
        AldSpectrogramGDI spectrogramGDI;
        float startY = 0;
        float startpercent = 0;
        float lengthpercent = 1;
		public SpectogramAlpha()
		{
			this.InitializeComponent();
            spectrogramGDI = new AldSpectrogramGDI(this.imgCanvas);
		}
        public void GenerateSpectogram(float[][] data)
        {            
            this.spectrogramGDI.GenerateSpectogram(data);

        }
        public void Resize()
        {
            this.spectrogramGDI.Resize((int)this.CurrentWidth(),(int)this.CurrentHeight());
        }
        public void Display(float startpercent, float lengthpercent)
        {
            this.startpercent = startpercent;
            this.lengthpercent = lengthpercent;
            this.spectrogramGDI.Display(startpercent, lengthpercent,startY);
        }
        Point start;
        private void imgCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(this);
            this.CaptureMouse();            
        }

        private void imgCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            { 
                FrameworkElement ini = (FrameworkElement)this.Parent;
                while (!ini.GetType().IsSubclassOf(typeof(Window)))
                    ini = (FrameworkElement)ini.Parent;
                

                Point now = e.GetPosition(this);

                //double dx = now.X - start.X;
                double dy = (now.Y - start.Y)/this.CurrentHeight();
                double py = 1 - startY;

                py = py - dy * py;

                if (py > 1) py = 1;
                if (py < 0) py = 0;

                this.startY = (float)(1-py);


                (ini as Window).Title = dy/this.CurrentHeight()+"";
                this.Display(this.startpercent, this.lengthpercent);
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
        }
    }
}