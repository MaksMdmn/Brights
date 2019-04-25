using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingAvgCalculation2
{
    class Program
    {
        static void Main(string[] args)
        {
            int period = 2;
            double[] testValues = { 3, 2, 6, 6, 2, 1, 3, 2 };
            double[] result = MovingAvgCalc(testValues, period);

            foreach (double elem in result)
            {
                Console.WriteLine(elem);
            }

            Console.ReadKey();
        }

        static double[] MovingAvgCalc(double[] values, int period)
        {
            List<double> result = new List<double>();
            LinkedList<double> cacheList = new LinkedList<double>();

            for (int i = 0; i < values.Length; i++)
            {
                cacheList.AddLast(values[i]);
                if (i < (period - 1))
                {
                    result.Add(0);
                }
                else
                {
                    result.Add(cacheList.Average());
                    cacheList.RemoveFirst();
                }
            }

            return result.ToArray();

        }
    }
}
