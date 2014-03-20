using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace PopulationSimulator
{
	public class GibbsPopulationSimulator
	{
		DiscretizedDFE dfe;
		BeneficialMutationRate mu;
		public int WellsSimulatedCounter = 0;
		public List<int> SimNumbers = new List<int> ();
		public int TotalSimulationsCounter;
		public const double ANCESTRALGROWTHRATE = 0.693147180559945;
		public GibbsPopulationSimulator (DiscretizedDFE dfe, BeneficialMutationRate mu)
		{
			this.dfe = dfe;
			this.mu = mu;
		}
		//For simpler simulations
		public SimulationResult SimulatePopulation (PopulationSize pop, int NumberOfTransfers)
		{
			SimulationResult sr = new SimulationResult (NumberOfTransfers);
			double gensBetweenTransfers = pop.GenerationsInBetweenTransfers;
			EvolvingPopulation EP;
			EP = new EvolvingPopulation (dfe, pop, mu);
			for (int i = 0; i < NumberOfTransfers; i++) {
				EP.GrowOneCycle ();
				sr.FrequenciesAtTime.Add (EP.PopSizes.ToArray ());
			}
			sr.SampledIsolate = EP.SamplePopulation ();
			return sr;
		}

        /// <summary>
        /// Simulate a particular well outcome until you get the correct number of results.
        /// </summary>
        /// <param name="well"></param>
		public void SimulateWell (ObservedWell well)
		{  
			int Winner = -1;
			int NumSims = 0;
			double gensBetweenTransfers = well.PopSize.GenerationsInBetweenTransfers;
			EvolvingPopulation EP;
			do {
				EP = new EvolvingPopulation (dfe, well, mu);
				for (int i = 0; i < well.TotalTransfers; i++) {
					EP.GrowOneCycle ();
				}
				Winner = EP.SamplePopulation ();
				Interlocked.Increment (ref TotalSimulationsCounter);
				NumSims += 1;
			} while (Winner != well.binClass);
			//well.CurrentMissingData = EP.Mutations;
			well.AmountOfTimeLastRun = EP.TotalTime;
			well.MutCounter = EP.MutCounter;
			well.NumberOfSimulationsLastRun = NumSims;
			//Console.WriteLine("Class :" + well.binClass.ToString() + " took " + NumSims.ToString() + " and has " + EP.MutCounter.CountOfEachMutation.Sum() + " Mutations");
			Interlocked.Increment (ref WellsSimulatedCounter);
			SimNumbers.Add (NumSims);
		}
	}

	public class SimulationResult
	{
		public List<double[]> FrequenciesAtTime;
		public int SampledIsolate;
		public SimulationResult (int NumberOfTransfers)
		{
			FrequenciesAtTime = new List<double[]> (NumberOfTransfers);
		}
	}
}
