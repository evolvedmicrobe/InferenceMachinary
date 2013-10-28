using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopulationSimulator
{
    public class MutationCounter
    {
        public int[] CountOfEachMutation;
        public MutationCounter(DiscretizedDFE dfe)
        {
            CountOfEachMutation = new int[dfe.NumberOfClassesIncludingNeutral];
        }
        public void AddMutations(List<TimeFitnessClass> Mutations)
        {
            foreach (TimeFitnessClass tfc in Mutations)
            {
                CountOfEachMutation[tfc.Class] += 1;
            }
        }
        public void AddCountToClass(int numberOfMutants, int MutantClass)
        {
            CountOfEachMutation[MutantClass] += numberOfMutants;
        }
        public void AddMutationCounter(MutationCounter MC)
        {
            if (MC.CountOfEachMutation.Length != this.CountOfEachMutation.Length)
            { throw new Exception("Can't add mismatched mutant counters"); }
            this.CountOfEachMutation=CountOfEachMutation.Zip(MC.CountOfEachMutation,(x,y)=>x+y).ToArray();
        }
    }
}
