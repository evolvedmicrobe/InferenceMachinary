using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using ShoNS.Array;

//using ShoNS.MathFunc;

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

        protected double[] _pClassProbabilities;
        /// <summary>
		/// 1 to N
		/// </summary>
        
		public double[] ClassProbabilities 
        {
            get{return _pClassProbabilities;}
            set{
                _pClassProbabilities=value;
                cumProbs = new double[_pClassProbabilities.Length];
                CreateCumulativeProbs();
            }
        }
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
			this.max = (1 + max) * GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
			this.MidPoints = new double[NumberBins + 1];
			_pClassProbabilities = new double[NumberBins];
			cumProbs = new double[NumberBins];
			double interval = (max - min) / NumberBins;
			MidPoints [1] = min + interval / 2.0;
			//Make a list of relative fitness improvements
			for (int i = 2; i < NumberBins + 1; i++) {
				MidPoints [i] = MidPoints [i - 1] + interval;
			}
			//Now just get the actual growth rates
			for (int i = 0; i < NumberBins + 1; i++) {
				MidPoints [i] = GibbsPopulationSimulator.ANCESTRALGROWTHRATE + MidPoints [i] * GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
			}       
		}

		public DiscretizedDFE (double max, int NumberBins)
		{
			//this.min = min;
			this.max = (1 + max) * GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
			this.MidPoints = new double[NumberBins + 1];
			ClassProbabilities = new double[NumberBins];
			double interval = max / NumberBins;
			MidPoints [1] = interval / 2.0;
			//Make a list of relative fitness improvements
			for (int i = 2; i < NumberBins + 1; i++) {
				MidPoints [i] = MidPoints [i - 1] + interval;
			}
			//Now just get the actual growth rates
			for (int i = 0; i < NumberBins + 1; i++) {
				MidPoints [i] = GibbsPopulationSimulator.ANCESTRALGROWTHRATE + MidPoints [i] * GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
			}       
		}

		/// <summary>
		/// Returns a random bin for a mutation based on the
		/// current parameter values, BIN NUMBER IS in 0 to 1 scheme
		/// </summary>
		/// <returns></returns>
		public int GetRandomBinAssignment ()
		{

			double d = RandomVariateGenerator.NextDouble ();
			for (int i = 0; i < cumProbs.Length; i++) {
				if (d < cumProbs [i])
					return i + 1;
			}            
			throw new Exception ("Problem with population cumulative ability array:" +d.ToString());
		}

		protected void CreateCumulativeProbs ()
		{
            cumProbs[0] = _pClassProbabilities[0];
            for(int i=1;i<_pClassProbabilities.Length;i++)
            {
                cumProbs[i] = _pClassProbabilities[i] + cumProbs[i - 1];
            }
		}
        /// <summary>
        /// Overwrite the values in the existing array instead of copying and allocating.
        /// </summary>
        /// <param name="probs"></param>
        public void SetProb(double[] probs)
        {
            if (probs.Length != _pClassProbabilities.Length)
            {
                throw new Exception("Cannot set an unequal array!");
            }
            for (int i = 0; i < probs.Length; i++)
                _pClassProbabilities[i] = probs[i];
            CreateCumulativeProbs();
        }
		//Fitness is s, where s is (r0+s)/r0;
		public int AssignFitnessToBin (double W)
		{
			if (W < min) {
				throw new Exception ("Can't assign below the min!");
			}
			W = W * GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
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
            ClassProbabilities = RandomVariateGenerator.DirichletSample(counts);
			CreateCumulativeProbs ();
			PointMass = false;
		}

		public void SetDFEasPointMass (int IndexToSet)
		{
			_pClassProbabilities = new double[ClassProbabilities.Length];
			_pClassProbabilities [IndexToSet] = 1.0;
			CreateCumulativeProbs ();
			PointMass = true;
			PointMassGroup = IndexToSet + 1;
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


		
     
	}
}
