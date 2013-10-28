using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShoNS.Array;
using ShoNS.MathFunc;

namespace PopulationSimulator
{
    public class MultinomialSampler
    {
        DoubleArray probs;
        //double[] probs;
        public MultinomialSampler(IEnumerable<double> Frequencies)
        {
            //probs = Frequencies.ToArray();
            //probs = probs.CumSum();
           
            probs = DoubleArray.From(Frequencies);
            probs=probs.CumSum(DimOp.OverCol);
        }
        public int GetRandomSample()
        {
            double d = ThreadSafeRandomGenerator.NextDouble();
            return (from x in Enumerable.Range(0, probs.Length) where d < probs[x] select x).First();  
        }


    }
}
