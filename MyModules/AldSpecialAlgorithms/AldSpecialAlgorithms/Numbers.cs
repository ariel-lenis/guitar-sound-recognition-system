using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms
{
    public static class Numbers
    {
        public static float LeaderNormalization(this float who, int n)
        {
            int aux = (int)(who * n);
            return (float)aux/n;
        }
    }
}
