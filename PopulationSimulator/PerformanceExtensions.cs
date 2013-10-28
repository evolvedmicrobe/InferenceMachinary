using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PopulationSimulator
{
    /// <summary>
    /// Extensions methods to replace arrays in the ShoNS, which I gather 
    /// are probably pretty fast for large arrays, but are about 19X slower than array math
    /// </summary>
    public static class PerformanceExtensions
    {
        public static double[] ElementMultiply(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply Vectors of Unequal Length"); }
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] * y[i];
            }
            return toR;
        }
        public static double[] ElementMultiply(this double[] x, double y)
        {
           double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] * y;
            }
            return toR;
        }
        public static double[] ElementSubtract(this double[] x, double y)
        {
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] - y;
            }
            return toR;
        }
        public static double[] ElementSubtract(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply Vectors of Unequal Length"); }
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] - y[i];
            }
            return toR;
        }
        public static double[] ElementAdd(this double[] x, double y)
        {
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] + y;
            }
            return toR;
        }
        public static double[] ElementAdd(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply Vectors of Unequal Length"); }
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] + y[i];
            }
            return toR;
        }
        public static double[] ElementDivide(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply Vectors of Unequal Length"); }
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i] /y[i];
            }
            return toR;
        }
        public static double[] ElementDivide(this double[] x, double y)
        {
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i]/y;
            }
            return toR;
        }
        public static double[] Exp(this double[] x)
        {
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = Math.Exp(x[i]);
            }
            return toR;
        }

        public static double[] CumSum(this double[] x)
        {
      
            double[] toR = new double[x.Length];
            toR[0] = x[0];
            for (int i = 1; i < x.Length; i++)
            {
                toR[i] = x[i] + toR[i-1];
            }
            return toR;
        
        
        }
        public static double[] CloneDouble(this double[] x)
        {
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = x[i];
            }
            return toR;
        }

    }
}
