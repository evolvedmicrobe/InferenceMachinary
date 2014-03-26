using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PopulationSimulator.Data
{
    /// <summary>
    /// A collection of observed wells binned in to groups.  This is part of
    /// code changes on 3/25/2014 that allowed the code to stop using different
    /// different simulations for each individual well.
    /// 
    /// Basically, simulations happen and when a simulation matches a well that has not been assigned yet, we grab an
    /// unassigned well and sets its missing data equal to that simulations values.
    /// </summary>
    public class ObservedWellCollection
    {
        /// <summary>
        /// Count of total wells observed for each bin class
        /// </summary>
        int[] allCounts;
        int allTotal;
        /// <summary>
        /// Mutable count of total wells observed for each bin class, goes to zero as simulations complete
        /// </summary>
        int[] curSimCounts;
        int curTotal;
        const int UPDATE_CHECK_FREQUENCY = 5;
        int totalTransfers;
        List<ObservedWell>[] binnedWells;
        PopulationSize popSize;
        DiscretizedDFE dfe;
        int TotalSimsCounter = 0;

        public ObservedWellCollection(List<ObservedWell> wells,DiscretizedDFE dfe)
        {
            this.dfe = dfe;
            var popSizes=wells.Select(x=>x.PopSize).Distinct().ToList();
            if(popSizes.Count!=1) {
                throw new Exception("Too many/few population sizes for one well.");
            }
            popSize = popSizes[0];

            var totalTransfersL = wells.Select(x => x.TotalTransfers).Distinct().ToList();
            if(totalTransfersL.Count!=1)
            {
                throw new Exception("Too many/few total transfers for one group.");
            }
            this.totalTransfers = (int)totalTransfersL[0];
            binnedWells = new List<ObservedWell>[dfe.NumberOfClassesIncludingNeutral];
            allCounts = new int[dfe.NumberOfClassesIncludingNeutral];
            for(int i=0;i<dfe.NumberOfClassesIncludingNeutral;i++)
            {
                binnedWells[i] = wells.Where(x => x.BinClass == i).ToList();
                allCounts[i] = binnedWells[i].Count;
            }
            allTotal = allCounts.Sum();
        }

        public void Simulate(int numberOfParallelTasks)
        {           
            //Copy all counts
            curSimCounts=allCounts.ToArray();
            curTotal = allTotal;
            Task[] runIt = new Task[numberOfParallelTasks];
            for (int i = 0; i < numberOfParallelTasks; i++)
            {
                var t=new Task(() => simRunner());
                runIt[i] = t;
                t.Start();
            }
            Task.WaitAll(runIt);
        }
		void simRunner ()
		{  
#if DEBUG
            int[] winners=new int[dfe.NumberOfClassesIncludingNeutral];
#endif
			int Winner = -1;
			int NumSims = 0;
            int lTotalTransfers=this.totalTransfers;
			double gensBetweenTransfers = popSize.GenerationsInBetweenTransfers;
			EvolvingPopulation EP;
            do
            {

                EP = new EvolvingPopulation(dfe, popSize);
                for (int i = 0; i < lTotalTransfers; i++)
                {
                    EP.GrowOneCycle();
                }
                Winner = EP.SamplePopulation();
#if DEBUG
                winners[Winner]++;
#endif		
                //If there is still a simulation for this mutation class unaccounted for
                if (curSimCounts[Winner] > 0)
                {
                    lock (curSimCounts)
                    {
                        int index = -1;
                        //figure out who to update and lower total counts
                        index = curSimCounts[Winner] - 1;
                        if (index >= 0)
                        {
                            curSimCounts[Winner] = index;
                            Interlocked.Decrement(ref curTotal);
                            var toUpdate = binnedWells[Winner][index];
                            toUpdate.AmountOfTimeLastRun = EP.TotalTime;
                            toUpdate.MutCounter = EP.MutCounter;
                            toUpdate.NumberOfSimulationsLastRun = NumSims;
                        }
                    }
                }
                Interlocked.Increment(ref TotalSimsCounter);

                //Goofy instruction, meant to be safe on locking, if the value is 0, then set it to zero and return the original value, meant to read
                //the value in a thread safe way and ensure the compiler doesn't rmeove it.
            } while (Interlocked.CompareExchange(ref curTotal, 0, 0) > 0);				

			
		}

    }
}
