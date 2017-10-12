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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AldWavDisplayTools;
using TsExtraControls;
using D = System.Drawing;
using System.ComponentModel;
using System.Reflection;

namespace TsMusicXMLDisplayer
{    
    public partial class DisplayerTheDocument : UserControl
    {
        GDI.Drawing.TsGDITheDocument theDocument;

        AldBitmapSourceCreator source;
        Point startdown;

        List<DisplayerThePaper> thepapers = new List<DisplayerThePaper>();

        public DisplayerTheDocument()
        {
            InitializeComponent();
            //Initialize(5);
        }

        double startmargin = 0;

        void Initialize(int n)
        {
            double w = 400;
            double h = 500;
            double margin = 20;
            startmargin = margin;
            double currentY = margin;
            for (int i = 0; i < n; i++)
            {
                DisplayerThePaper paper = new DisplayerThePaper();
                paper.MinWidth =0;
                paper.MinHeight = 0;
                this.canvasPaper.Children.Add(paper);
                this.thepapers.Add(paper);
            }
            ReAllocatePapers(w, h, 0, 0, margin);
        }

        public void ReAllocatePapers(double w, double h, double sx, double sy, double margin)
        {
            double currentY = sy;
            for (int i = 0; i < this.thepapers.Count; i++)
            {
                DisplayerThePaper paper = this.thepapers[i];
                paper.Width = w;
                paper.Height = h;

                paper.SetX(sx);
                paper.SetY(currentY);
                currentY += h + margin;
            }
        }

        public void LoadMusicXML(MusicXML.scorepartwise partwise)
        {
            var enviroment = new GDI.Drawing.TsDrawingEnviroment();
            enviroment.FiguresColor = new GDI.Drawing.ColorGroup(D.Color.Black);
            enviroment.HighlightedColor = new GDI.Drawing.ColorGroup(D.Color.Blue);
            enviroment.LinesColor = new GDI.Drawing.ColorGroup(D.Color.Gray);

            int w = (int)(this.CurrentWidth());
            int h = (int)(w * 10 / 8.5);

            enviroment.W = 850*2;
            enviroment.H = 1100*2;

            enviroment.MarginLeft = enviroment.W / 20;
            enviroment.MarginRight = enviroment.W / 20;
            enviroment.MarginTop = enviroment.W / 20;
            enviroment.MarginBottom = enviroment.W / 20;

            enviroment.HeightSpace = enviroment.W / 75;

            //if (gpaper == null) gpaper = new GDI.Drawing.TsGDIPaper(enviroment, tspartwise);
            //else gpaper.Enviroment = enviroment;

            //gpaper.ReloadSize();
            GDI.Parser.ParserMusicXML parser = new GDI.Parser.ParserMusicXML(partwise);
            GDI.TsPartwise tspartwise = parser.Parse();
            this.theDocument = tspartwise.CreateDocument(enviroment);
            
            tspartwise.CalculateCachePosition(tspartwise.TheEngine,null);

            this.theDocument.CalculateCachePosition(tspartwise.TheEngine, null);

            this.theDocument.Draw(null, tspartwise.TheEngine);

            foreach (var ipaper in tspartwise.Papers)
                AddPaper(ipaper);

            

                /*
            Draw(w, h);


            //////
            
            this.theDocument = new GDI.Drawing.TsGDITheDocument(engine);

            GDI.Parser.ParserMusicXML parser = new GDI.Parser.ParserMusicXML(partwise);
            this.theDocument.LoadMusicXML(parser);
            this.OnResize(500);
             * 
             * */


            Mouse.AddMouseWheelHandler(this, new MouseWheelEventHandler(this.OnMouseWheel));          
            
            this.canvasPaper.MouseDown += DisplayerPaper_MouseDown;
            this.canvasPaper.MouseUp += DisplayerPaper_MouseUp;
            this.canvasPaper.MouseMove += canvasPaper_MouseMove;
            //this.imgPaper.SetX(0);
            //this.imgPaper.SetY(0);
             
        }

        private void AddPaper(GDI.Drawing.TsGDIPaper ipaper)
        {
            var enviroment = this.theDocument.Partwise.TheEngine.Enviroment;
            DisplayerThePaper paper = new DisplayerThePaper();
            paper.LoadPaper(ipaper);
            paper.MinWidth = 0;
            paper.MinHeight = 0;
            this.canvasPaper.Children.Add(paper);
            this.thepapers.Add(paper);
            if (this.startmargin == 0) this.startmargin = enviroment.W / 20.0;
            ReAllocatePapers(enviroment.W, enviroment.H, 0, 0, this.startmargin);
        }
        
        void canvasPaper_MouseMove(object sender, MouseEventArgs e)
        {
            if(canvasPaper.IsMouseCaptured)
            { 
                var pos = e.GetPosition(this.canvasPaper);
                foreach(var ipaper in this.thepapers)
                {
                    ipaper.PlusX(pos.X - startdown.X);
                    ipaper.PlusY(pos.Y - startdown.Y);
                }
                startdown = pos;
            }
        }

        void DisplayerPaper_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            this.Cursor = Cursors.Arrow;
            if (canvasPaper.IsMouseCaptured)
                this.canvasPaper.ReleaseMouseCapture();
             
        }

        void DisplayerPaper_MouseDown(object sender, MouseButtonEventArgs e)
        {
          
            this.Cursor = Cursors.SizeAll;
            startdown = e.GetPosition(canvasPaper);
            canvasPaper.CaptureMouse();
            
        }

        double GetCenter(double f, double k, double p)
        {
            return f * (1 - k) + k * p;
        }

        public void OnMouseWheel(object who, MouseWheelEventArgs args)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                ScrollPages(who, args);
            else
                ResizePages(who, args);
            
        }

        private void ResizePages(object who, MouseWheelEventArgs args)
        {
            if (this.thepapers.Count == 0) return;
            var pos = args.MouseDevice.GetPosition(this.thepapers[0]);
            pos.X /= canvasPaper.CurrentWidth();
            pos.Y /= canvasPaper.CurrentHeight();

            double prop = 1;

            if (args.Delta < 0)
                prop = 0.8f;
            else
                prop = 1.2f;

            double newW = thepapers[0].CurrentWidth() * prop;
            double newH = thepapers[0].CurrentHeight() * prop;
            /*
            foreach(var ipaper in this.thepapers)
            { 
                ipaper.Width = ipaper.CurrentWidth() * prop;
                ipaper.Height = ipaper.CurrentHeight() * prop;
            }
            */

            this.startmargin *= prop;

            var final = args.MouseDevice.GetPosition(canvasPaper);

            double currentx = this.thepapers[0].GetX();
            double currenty = this.thepapers[0].GetY();

            var paper = new Point(currentx, currenty);

            double _x = GetCenter(final.X, prop, paper.X);
            double _y = GetCenter(final.Y, prop, paper.Y);

            if (double.IsNaN(_x) || double.IsNaN(_y))
                throw new Exception("xD");

            ReAllocatePapers(newW, newH, _x, _y, startmargin);

            /*
            double deltax = _x-currentx;
            double deltay = _y - currenty;


            foreach(var ipaper in this.thepapers)
            {
                ipaper.PlusX(deltax);
                ipaper.PlusY(deltay);
            }
            */

            //OnResize((int)(this.imgPaper.Width ));
        }

        private void ScrollPages(object who, MouseWheelEventArgs args)
        {
            double prop = 1;

            if (args.Delta < 0)
                prop = -1;
            else
                prop = 1;

            double _x = thepapers[0].GetX();
            double _y = thepapers[0].GetY();

            double currentW = thepapers[0].CurrentWidth();
            double currentH = thepapers[0].CurrentHeight();

            ReAllocatePapers(currentW, currentH, _x, _y+currentH/20*prop, startmargin);

        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (tspartwise == null) return;

            //this.OnResize(500);            
        }

        private void OnResize(int w)
        {
            /*
            enviroment = new GDI.Drawing.TsDrawingEnviroment();
            enviroment.FiguresColor = new GDI.Drawing.ColorGroup(D.Color.Black);
            enviroment.HighlightedColor = new GDI.Drawing.ColorGroup(D.Color.Blue);
            enviroment.LinesColor = new GDI.Drawing.ColorGroup(D.Color.Gray);

            w = (int)(this.CurrentWidth());
            int h = (int)(w*10/8.5);

            enviroment.W = w;
            enviroment.H = h;

            enviroment.MarginLeft = enviroment.W / 20;
            enviroment.MarginRight = enviroment.W / 20;
            enviroment.MarginTop = enviroment.W / 20;
            enviroment.MarginBottom = enviroment.W / 20;

            enviroment.HeightSpace = enviroment.W / 75;

            if (gpaper == null) gpaper = new GDI.Drawing.TsGDIPaper(enviroment,tspartwise);
            else gpaper.Enviroment = enviroment;

            gpaper.ReloadSize();

            Draw(w,h);
             * */
        }

        private void Draw(int w,int h)
        {
            /*
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
                var dpiX = (int)dpiXProperty.GetValue(null, null);
                var dpiY = (int)dpiYProperty.GetValue(null, null);

                source = new AldBitmapSourceCreator(w, h);
                //pentagramGDI.Resize((int)(this.CurrentWidth()), (int)this.CurrentHeight());
                source.DrawImage(gpaper.GetBitmap);

                this.imgPaper.Source = source.Bmp;
            }
             * */
        }

    }
}
