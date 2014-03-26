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
