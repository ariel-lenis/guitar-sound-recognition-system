using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using D = System.Drawing;

namespace AldFirstNetworkTrainer
{
    public class TsColors
    {
        private static List<Color> colors;
        private static List<D.Color> dcolors;
        
        public static List<Color> CommonColors
        {
            get
            {
                if (colors == null)
                {
                    var type = typeof(TsColors);
                    //colors = type.GetProperties().Where(x => x.PropertyType == typeof(Color)).Select(y => (Color)y.GetValue(null)).Where(x => x != Colors.Transparent && x!=Colors.White).ToList();
                    colors = CommonDColors.Select(x => Color.FromRgb(x.R, x.G, x.B)).ToList();
                }
                return colors;
            }
        }
        public static List<D.Color> CommonDColors
        {
           
            get
            {
                if (dcolors == null)
                {
                    var type = typeof(D.Color);
                    //dcolors = type.GetProperties().Where(x => x.PropertyType == typeof(D.Color)).Select(y => (D.Color)y.GetValue(null)).Where(x => x != D.Color.Transparent && x != D.Color.White).ToList();
                    dcolors = new List<D.Color>();      


                    dcolors.Add(D.Color.FromName("green"));
                    dcolors.Add(D.Color.FromName("blue"));
                    dcolors.Add(D.Color.FromName("DarkViolet"));
                    dcolors.Add(D.Color.FromName("brown"));
                    dcolors.Add(D.Color.FromName("black"));
                    dcolors.Add(D.Color.FromName("red"));
                    dcolors.Add(D.Color.FromName("gold"));
                    dcolors.Add(D.Color.DarkGoldenrod);
                    dcolors.Add(D.Color.FromName("orange"));
                    dcolors.Add(D.Color.FromName("Violet"));
                }
                return dcolors;
            }
           
        }

    }
}
