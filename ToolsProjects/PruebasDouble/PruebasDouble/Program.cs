using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebasDouble
{
    class Program
    {
        static void Main(string[] args)
        {
            int size = short.MinValue;
            size = size * (-2);
            float []values=new float[size];
            int limit = short.MinValue;limit=-limit;
            int pos=0;
            for (int i = short.MinValue; i <= short.MaxValue; i++)
                values[pos++] = (float)i / limit;
            int cont = 0;
            for (int i = 1; i < values.Length; i++)
            {
                Console.WriteLine(values[i]);
                if (values[i].ToString().Equals(values[i - 1].ToString()))
                    cont++;
                    
            }
            Console.WriteLine("Ok : {0}% ={1} elements", (double)(cont + 1) / size,cont);
            Console.ReadKey(false);
        }
    }
}
