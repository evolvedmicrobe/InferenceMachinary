using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicrosoftResearch.Infer.Distributions;

namespace PopulationSimulator
{
    
    public struct TimeFitnessClass
    {
        public double time;
        public int Class;
    }    
    public class SimulationTimesAndFitnesses
    {
        DiscretizedDFE dfe;
       public SimulationTimesAndFitnesses(DiscretizedDFE dfe)
        {
            this.dfe = dfe;
        }
       //Don't recreate, possibly should upgrade to sho? This is not mersenne, but based on something
        public Random r = new Random();
        public List<TimeFitnessClass> SimulateMutations(double rate, ObservedWell well)
        {
            Poisson p = new Poisson(rate * well.TotalTransfers*well.PopSize.GenerationsInBetweenTransfers);
            int mutNumber = p.Sample();
            List<TimeFitnessClass> classes = new List<TimeFitnessClass>();
            for (int i = 0; i < classes.Count; i++)
            {
                double time = r.NextDouble() * well.TotalGenerations;
                int w = dfe.GetRandomBinAssignment();
                TimeFitnessClass tfc=new TimeFitnessClass(){time=time,Class=w};
                classes.Add(tfc);
            }
            classes.Sort((x, y) => x.time.CompareTo(y.time));
            return classes;
        }

    }
}
