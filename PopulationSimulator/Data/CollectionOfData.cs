using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationSimulator
{
    public sealed class CollectionOfData
    {
        List<ObservedWell> data;
        double Count;
        public string Name;
        public CollectionOfData(List<ObservedWell> data, string Name)
        {
            this.Count = (double)data.Count;
            this.data = data;
            this.Name = Name;
        }
        public double AverageSimulations()
        {
            int totalSims = data.Sum(x => x.NumberOfSimulationsLastRun);
            double averageSims = (double)totalSims / Count;
            return averageSims;
        }
        public double AverageMutations()
        {
            int totalMutations = data.Sum(x => x.MutCounter.CountOfEachMutation.Sum());
            double averageMuts = (double)totalMutations / Count;
            return averageMuts;

        }
        public void OutputGroup()
        {
           
           // Console.WriteLine(Name + " had and average of " + averageSims.ToString() + " sims and " + averageMuts.ToString() + " mutations.");
        }
    }
}
