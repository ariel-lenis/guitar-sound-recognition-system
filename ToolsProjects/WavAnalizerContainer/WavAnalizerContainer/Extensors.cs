using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavAnalizerContainer
{
    public static class Extensors
    {
        public static int MaxPosition(this double[] W)
        {
            int res = 0;
            for (int i = 0; i < 10000; i++)
                if (W[res] > W[i]) res = i;
            return res;
        }
    }
} 
