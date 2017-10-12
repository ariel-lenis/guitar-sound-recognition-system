using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsGraphicsAdmin
    {
        public class GraphicsImage
        {
            public D.Bitmap Bmp;
            public D.Graphics Graphics;
        }
        Dictionary<int, GraphicsImage> data;
        Drawing.TsDrawEngine engine;

        public Drawing.TsDrawEngine Engine { get { return engine; } set { this.engine = value; } }

        public TsGraphicsAdmin(Drawing.TsDrawEngine engine)
        {
            this.engine = engine;
            this.data = new Dictionary<int, GraphicsImage>();
        }

        public GraphicsImage this[int idx]
        { 
            get
            {
                if (!data.ContainsKey(idx))
                { 
                    GraphicsImage newg = CreateGraphicsImage();
                    data.Add(idx, newg);
                }
                return this.data[idx];
            }
        }

        public void RenegerateAll()
        {
            foreach (var i in data)
                Renegerate(i.Value);
        }

        private void Renegerate(GraphicsImage graphicsImage)
        {
            graphicsImage = CreateGraphicsImage();
        }

        private GraphicsImage CreateGraphicsImage()
        {
            D.Bitmap bmp = new D.Bitmap(engine.Enviroment.W, engine.Enviroment.H);
            D.Graphics gbmp =  D.Graphics.FromImage(bmp);
            return new GraphicsImage() { Bmp = bmp, Graphics = gbmp };
        }
    }
}
