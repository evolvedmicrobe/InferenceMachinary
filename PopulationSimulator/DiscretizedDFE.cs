using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ShoNS.Array;

using ShoNS.MathFunc;
using MicrosoftResearch.Infer.Distributions;
using MicrosoftResearch.Infer.Maths;
using System.Runtime.InteropServices;

namespace PopulationSimulator
{
	public class DiscretizedDFE
	{
		protected double max, min, step;
		/// <summary>
		/// 0 to N, GROWTH RATES! Not fitness per say
		/// </summary>
		public double[] MidPoints;
		/// <summary>
		/// 1 to N
		/// </summary>
		public double[] ClassProbabilities;
		/// <summary>
		/// 1 to N
		/// </summary>
		protected double[] cumProbs;
		private bool PointMass = false;
		private int PointMassGroup = 0;

		public int NumberOfClassesIncludingNeutral {
			get { return this.MidPoints.Length; }
		}

		public double GetGrowthRateForBin (int binNumber)
		{
			return MidPoints [binNumber + 1];
		}

		public DiscretizedDFE (double min, double max, int NumberBins)
		{
			this.min = min;
			this.max = (1 + max) * PopulationSimulator.ANCESTRALGROWTHRATE;
			this.MidPoints = new double[NumberBins + 1];
			ClassProbabilities = new double[NumberBins];
			cumProbs = new double[NumberBins];
			double interval = (max - min) / NumberBins;
			MidPoints [1] = min + interval / 2.0;
			//Make a list of relative fitness improvements
			for (int i = 2; i < NumberBins + 1; i++) {
				MidPoints [i] = MidPoints [i - 1] + interval;
			}
			//Now just get the actual growth rates
			for (int i = 0; i < NumberBins + 1; i++) {
				MidPoints [i] = PopulationSimulator.ANCESTRALGROWTHRATE + MidPoints [i] * PopulationSimulator.ANCESTRALGROWTHRATE;
			}       
		}

		public DiscretizedDFE (double max, int NumberBins)
		{
			//this.min = min;
			this.max = (1 + max) * PopulationSimulator.ANCESTRALGROWTHRATE;
			this.MidPoints = new double[NumberBins + 1];
			ClassProbabilities = new double[NumberBins];
			cumProbs = new double[NumberBins];
			// CreateCumProbArray(NumberBins);
			double interval = max / NumberBins;
			MidPoints [1] = interval / 2.0;
			//Make a list of relative fitness improvements
			for (int i = 2; i < NumberBins + 1; i++) {
				MidPoints [i] = MidPoints [i - 1] + interval;
			}
			//Now just get the actual growth rates
			for (int i = 0; i < NumberBins + 1; i++) {
				MidPoints [i] = PopulationSimulator.ANCESTRALGROWTHRATE + MidPoints [i] * PopulationSimulator.ANCESTRALGROWTHRATE;
			}       
		}

		/// <summary>
		/// Returns a random bin for a mutation based on the
		/// current parameter values, BIN NUMBER IS in 0 to 1 scheme
		/// </summary>
		/// <returns></returns>
		public int GetRandomBinAssignment ()
		{
			double d = ThreadSafeRandomGenerator.NextDouble ();
			for (int i = 0; i < cumProbs.Length; i++) {
				if (d < cumProbs [i])
					return i + 1;
			}
			//int length = cumProbs.Length;
			//for (int count = 0; count < length; count++)
			//{
			//    if (d < *(prtCumProbArrayStart + count))
			//    {
			//        return count + 1;
			//    }
			//}
            
			//int toReturn=(from x in Enumerable.Range(0,cumProbs.Length) where d < cumProbs[x] select x).First()+1;
			//return toReturn;
			throw new Exception ("Problem with population cumulative ability array");
		}

		public void CreateCumulativeProbs ()
		{

			DoubleArray da = DoubleArray.From (ClassProbabilities);
			cumProbs = da.CumSum (DimOp.OverCol).ToArray ();

			//for (int i = 0; i < cumProbs.Length; i++)
			//{
			//    *(prtCumProbArrayStart + i) = cumProbs[i];
			//}

			//Much better results with the ShoNS for this
			//cumProbs = ClassProbabilities.CumSum();
		}
		//Fitness is s, where s is (r0+s)/r0;
		public int AssignFitnessToBin (double W)
		{
			if (W < min) {
				throw new Exception ("Can't assign below the min!");
			}
			W = W * PopulationSimulator.ANCESTRALGROWTHRATE;
			var b = (from x in MidPoints
			                  select Math.Abs (W - x)).ToList ();
			var m = b.Min ();
			int v = b.IndexOf (m);
			//if (v == 0)
			//    return 1;
			//else
			return v;
		}

		public void UpdateWithNewSamples (List<ObservedWell> AugmentedData)
		{
			//First to count up all the types.
			MutationCounter MC = new MutationCounter (this);
			foreach (ObservedWell ow in AugmentedData) {
				MC.AddMutationCounter (ow.MutCounter);
			}
			double[] counts = new double[ClassProbabilities.Length];
			for (int i = 0; i < counts.Length; i++) {
				counts [i] = (double)MC.CountOfEachMutation [i + 1] + .1;
			}
			Dirichlet d = new Dirichlet (counts);
			Vector v = d.Sample ();
			ClassProbabilities = v.ToArray ();
			CreateCumulativeProbs ();
			PointMass = false;
		}

		public void SetDFEasPointMass (int IndexToSet)
		{
			ClassProbabilities = new double[ClassProbabilities.Length];
			ClassProbabilities [IndexToSet] = 1.0;
			CreateCumulativeProbs ();
			PointMass = true;
			PointMassGroup = IndexToSet + 1;
		}

		public void UpdateWithNewSamplesOLD (List<ObservedWell> AugmentedData)
		{
			//First to count up all the types.
			//DoubleArray countstmp=DoubleArray.Ones(NumberOfClassesIncludingNeutral-1);
			//double[] counts=countstmp.ToArray();
			//foreach(ObservedWell ow in AugmentedData)
			//{
			//    foreach(TimeFitnessClass tfc in ow.CurrentMissingData)
			//    {
			//        counts[tfc.Class-1]+=1.0;
			//    }
			//}
			//Dirichlet d = new Dirichlet(counts);
			//Vector v=d.Sample();
			//ClassProbabilities = v.ToArray();
			//CreateCumulativeProbs();
		}
	}

	public class DiscretizedLabelledDFE :DiscretizedDFE
	{
		/// <summary>
		/// 0 to N, GROWTH RATES! Not fitness per say
		/// </summary>
       
		public int NumberOfClassesIncludingNeutral {
			get { return this.MidPoints.Length / 2; }
		}

		public DiscretizedLabelledDFE (double min, double max, int NumberBins) : base (min, max, NumberBins)
		{
			int oldLength = MidPoints.Length;
			double[] old = MidPoints;
			MidPoints = new double[oldLength * 2];
			for (int i = 0; i < oldLength; i++) {
				MidPoints [i] = old [i];
				MidPoints [i + oldLength] = old [i];
			}
		}

		/// <summary>
		/// Returns a random bin for a mutation based on the
		/// current parameter values, BIN NUMBER IS in 0 to 1 scheme
		/// </summary>
		/// <returns></returns>
		public int GetRandomBinAssignment ()
		{
			double d = ThreadSafeRandomGenerator.NextDouble ();
        
			for (int i = 0; i < cumProbs.Length; i++) {
				if (d < cumProbs [i])
					return i + 1;
			}
			throw new Exception ("Problem with population cumulative ability array");
		}

		public void SetProb (double[] probs)
		{
			for (int i = 0; i < probs.Length; i++)
				ClassProbabilities [i] = probs [i];
			CreateCumulativeProbs ();
		}

		public void CreateCumulativeProbs ()
		{

			DoubleArray da = DoubleArray.From (ClassProbabilities);
			cumProbs = da.CumSum (DimOp.OverCol).ToArray ();

		}
		//Fitness is s, where s is (r0+s)/r0;
		public int AssignFitnessToBin (double W)
		{
			if (W < min) {
				throw new Exception ("Can't assign below the min!");
			}
			W = W * PopulationSimulator.ANCESTRALGROWTHRATE;
			var b = (from x in MidPoints
			                  select Math.Abs (W - x)).ToList ();
			var m = b.Min ();
			int v = b.IndexOf (m);
			//if (v == 0)
			//    return 1;
			//else
			return v;
		}
	}
}
