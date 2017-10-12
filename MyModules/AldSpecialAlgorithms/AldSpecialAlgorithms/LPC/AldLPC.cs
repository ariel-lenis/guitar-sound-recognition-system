using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldSpecialAlgorithms.LPC
{
    public class AldLPC
    {
        public struct LPCResult
        {
            public float[] Alphas;
            public float Error;
            public LPCResult(float[] alphas, float error)
            {
                this.Alphas = alphas;
                this.Error = error;
            }
        }
        public static LPCResult LPC(float[] s, int p)
        {
            float[] R = new float[p + 1];
            for (int k = 0; k <= p; k++)
                R[k] = CalculateR(s, k);

            return LevinsonDurbin(R, p);
        }
        private static LPCResult LevinsonDurbin2(float[] R, int m)
        { 
	         // INITIALIZE Ak 
            float []Ak = new float[m+1];
            Ak[ 0 ] = 1.0f; 
	 
	        // INITIALIZE Ek 
	        float Ek = R[ 0 ]; 
 
	        // LEVINSON-DURBIN RECURSION 
	        for (int k = 0; k < m; k++ ) 
	        { 
		        // COMPUTE LAMBDA 
		        float lambda = 0.0f; 
		        for (int j = 0; j <= k; j++ ) 		        
			        lambda -= Ak[ j ] * R[ k + 1 - j ];
		        
		        lambda /= Ek; 

		        // UPDATE Ak 
		        for (int n = 0; n <= ( k + 1 ) / 2; n++ ) 
		        { 
			        float temp = Ak[ k + 1 - n ] + lambda * Ak[ n ]; 
			        Ak[ n ] = Ak[ n ] + lambda * Ak[ k + 1 - n ]; 
			        Ak[ k + 1 - n ] = temp; 
		        } 
		        // UPDATE Ek 
		        Ek *= 1.0f - lambda * lambda; 
	        }

            return new LPCResult(Ak, Ek);
        }
        private static LPCResult LevinsonDurbin(float[] _R, int p)
        {
            float[] r = new float[_R.Length+1];

            for(int i=0;i<_R.Length;i++)
                r[i+1]=_R[i];

            float[] ap = new float[p+1+1];
            float[] aj = new float[p+1+1];
            float[] aj1 = new float[p+1+1];

            float ej = r[1];  
            for(int j=1;j<=p;j++)
            {
                for(int i=0;i<aj1.Length;i++)
                    aj1[i] = 0;
                aj1[1] = 1;
                float gammaj = r[j+1];
        
                for(int i=2;i<=j;i++)
                    gammaj = gammaj + aj[i]*r[j-i+2];
        
                float lambdaj1 = -gammaj/ej;
                for(int i=2;i<=j;i++)
                    aj1[i] = aj[i]+lambdaj1*aj[j-i+2];

                aj1[j+1] = lambdaj1;
                ej = ej*(1-Math.Abs(lambdaj1*lambdaj1));
                
                for (int i = 0; i < aj1.Length;i++ )
                    aj[i] = aj1[i];
            }
            for (int i = 0; i < aj.Length; i++)
            {
                if (float.IsNaN(aj[i]))
                    aj[i] = 0;
            }
            if (float.IsNaN(ej))
            {
                return new LPCResult(new float[aj.Length], 0);
                //ej = 0;
            }
            return new LPCResult(aj.Skip(1).ToArray(), (float)Math.Sqrt(ej));
        }
        private static float CalculateR(float[] s, int k)
        {
            int N=s.Length;
            float res = 0;

            for (int m = 0; m < N - k; m++)
                res += s[m] * s[m + k];

            return res;
        }
    }
}
