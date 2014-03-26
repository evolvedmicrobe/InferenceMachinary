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
       

        /// <summary>
        /// Returns A*exp(r*t) as an array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[] Element_A_Times_E_ToThe_RT(double[] A, double[] r,double t)
        {
            if (A.Length != r.Length)
            { throw new Exception("Cannot Multiply vectors of Unequal Length"); }
            double[] toR = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                toR[i] = A[i] * Math.Exp(r[i] * t) ;
            }
            return toR;
        }

        public static double ElementMulitplyAndSum(this double[] x, double[] y)
        {
            double toR = 0.0;
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply And Sum Vectors of Unequal Length"); }
            for (int i = 0; i < x.Length; i++)
            {
                toR += x[i] * y[i];
            }
            return toR;
        }
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

        public static double Sum(this double[] x)
        {
            double toR = 0;
            for(int i=0;i<x.Length;i++)
            {
                toR += x[i];
            }
            return toR;
        }

        /// <summary>
        ///  Returns array of constant * (x-y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="constant"></param>
        /// <returns></returns>
        public static double[] ElementSubtractAndMultiplyByConstant(this double[] x, double[] y,double constant)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Multiply Vectors of Unequal Length"); }
            double[] toR = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                toR[i] = constant*(x[i] - y[i]);
            }
            return toR;
        }
        public static double SummedDifference(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Subtract and Sum Vectors of Unequal Length"); }
            double toR = 0;
            for (int i = 0; i < x.Length; i++)
            {
                toR += x[i] - y[i];
            }
            return toR;
        }
        public static double ElementSubtractAndSum(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Subtract and Sum Vectors of Unequal Length"); }
            double toR =0;
            for (int i = 0; i < x.Length; i++)
            {
                toR -= x[i] - y[i];
            }
            return toR;
        }
        /// <summary>
        /// Equivalent to x+=y for each element
        /// </summary>
        /// <param name="?"></param>
        public static void ElementAddInPlace(this double[] x, double[] y)
        {
            if (x.Length != y.Length)
            { throw new Exception("Cannot Subtract and Sum Vectors of Unequal Length"); }
            for (int i = 0; i < x.Length; i++)
            {
                x[i] += y[i];
            }
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
            { throw new Exception("Cannot add vectors of Unequal Length"); }
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
