using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public class PreEmphasisFilter
    {
        public static float[] PreEmphasis(float[] data,float a=0.9f)
        { 
            float[] res = new float[data.Length];
            res[0] = data[0];
            for (int i = 1; i < data.Length; i++)
                res[i] = data[i] - data[i - 1] * a;
            return res;
        }

    }
}
