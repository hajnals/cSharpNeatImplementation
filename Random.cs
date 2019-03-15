using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {
    static class MyRand {
        private static Random rnd = new Random(1929);

        /// <summary>
        /// Generates a double uniform random number between the min and the max value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double UniformRnd(double min, double max) {
            double range = min - max;
            return (rnd.NextDouble() * Math.Abs(range) + min);
        }

        // Box-Muller transform to generate normal random distribution
        public static double NormalDist(double mean, double stdDev) {
            double u1 = 1.0 - rnd.NextDouble();    //uniform(0,1] random doubles
            double u2 = 1.0 - rnd.NextDouble();
            double randStdNormal =   Math.Sqrt(-2.0 * Math.Log(u1))
                                   * Math.Sin(2.0 * Math.PI * u2);      //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal;          //random normal(mean,stdDev^2)

            return randNormal;
        }

        public static int Next() {
            return rnd.Next();
        }

        public static int Next(int minValue, int maxValue) {
            return rnd.Next(minValue, maxValue);
        }

        public static int Next(int maxValue) {
            return rnd.Next(maxValue);
        }
    }
}
