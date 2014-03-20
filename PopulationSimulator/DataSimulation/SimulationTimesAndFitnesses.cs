using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public SimulationTimesAndFitnesses (DiscretizedDFE dfe)
		{
			this.dfe = dfe;
		}
	
		public List<TimeFitnessClass> SimulateMutations (double rate, ObservedWell well)
		{
			double lambda=rate * well.TotalTransfers * well.PopSize.GenerationsInBetweenTransfers;
			int mutNumber = RandomVariateGenerator.PoissonSample(lambda);
			List<TimeFitnessClass> classes = new List<TimeFitnessClass> ();
			for (int i = 0; i < classes.Count; i++) {
				double time = RandomVariateGenerator.NextDouble () * well.TotalGenerations;
				int w = dfe.GetRandomBinAssignment ();
				TimeFitnessClass tfc = new TimeFitnessClass (){ time = time, Class = w };
				classes.Add (tfc);
			}
			classes.Sort ((x, y) => x.time.CompareTo (y.time));
			return classes;
		}
	}
}
