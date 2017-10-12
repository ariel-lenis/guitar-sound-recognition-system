using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldBackPropagation
{    
    [Serializable]
    public class AldActivationFunctions
    {
        //public static Func<float, float, float> Sigmoidal = (x, temperature) => (float)((1.0 / (1 + Math.Exp(-temperature * x))));
        public static Func<float, float, float> Sigmoidal = (x, temperature) =>
        {
            if(float.IsNaN(x)) throw new Exception("wtf!!");
            double expp = Math.Exp(-temperature * x);
            if(double.IsNaN(expp)) throw new Exception("wtf!!");
            var res =  (float)((1.0 / (1 + expp)));
            if (res < -0.95) return -1;
            if (res >  0.95) return  1;

            if (float.IsNaN(res)) throw new Exception("wft!!!");
            return res;
        };

        public static Func<float, float, float> dSigmoidal = (x, temperature) =>
        {
            if (float.IsNaN(x)) throw new Exception("wtf!!");
            if (float.IsInfinity(x)) return 0;
            float res = (float)(Math.Exp(-temperature * x) / Math.Pow(1 + Math.Exp(-temperature * x), 2));
            if (float.IsNaN(res)) return 0;
            return res;
        };

        public static Func<float, float, float> TangentHiperbolic = (x, temperature) => (float)(Math.Tanh(x));
        public static Func<float, float, float> dTangentHiperbolic = (x, temperature) => (float)(1-Math.Pow(Math.Tanh(x),2));
        
    }
}
