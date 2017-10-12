using AldWavDisplayTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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

namespace TsMusicXMLDisplayer
{
	/// <summary>
	/// Interaction logic for DsiplayerThePaper.xaml
	/// </summary>
	public partial class DisplayerThePaper : UserControl
	{
        GDI.Drawing.TsGDIPaper thepaper;
        AldBitmapSourceCreator source;
		public DisplayerThePaper()
		{
			this.InitializeComponent();
		}

        internal void LoadPaper(GDI.Drawing.TsGDIPaper ipaper)
        {
            this.thepaper = ipaper;
            var enviroment = thepaper.TheTsPartwise.TheEngine.Enviroment;
            Draw(enviroment.W, enviroment.H);
        }
        private void Draw(int w, int h)
        {            
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiX = (int)dpiXProperty.GetValue(null, null);
                var dpiY = (int)dpiYProperty.GetValue(null, null);

                source = new AldBitmapSourceCreator(w, h);
                //pentagramGDI.Resize((int)(this.CurrentWidth()), (int)this.CurrentHeight());
                source.DrawImage(this.thepaper.GetBitmap);

                this.imagePaper.Source = source.Bmp;
            }             
        }
    }
}