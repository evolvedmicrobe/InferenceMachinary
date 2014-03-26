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

        public double priorAlpha = 1.0;
        public double priorBeta = 1.0;

        /// <summary>
        /// The rates in each class
        /// </summary>
        protected double[] _pMutationRates;
        /// <summary>
        /// The rate at which mutations appear per unit time per individual.
        /// This array does NOT have the neutral class.
        /// </summary>
        public double[] MutationRates { get { return _pMutationRates; } }

        /// <summary>
        /// The rate obtained by summing the individual rates.
        /// </summary>
        public double BeneficialMutationRate { get { return _pMutationRates.Sum(); } }
        
        /// <summary>
		/// 1 to N
		/// </summary>
        
		public double[] ClassProbabilities 
        {
            get{return _pClassProbabilities;}
            set{
                _pClassProbabilities=value;
                if (_pClassProbabilities == null || cumProbs==null || _pClassProbabilities.Length != value.Length)
                {
                    cumProbs = new double[_pClassProbabilities.Length];
                }
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
            _pMutationRates = new double[NumberBins];
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
            _pMutationRates = new double[NumberBins];
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
        /// <summary>
        /// Assign a fitness value 
        /// </summary>
        /// <param name="W">1+s, to be multiplied by ancestral growth rate</param>
        /// <returns></returns>
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
            ///Sample the rate for each class, then update the cumulative probabilities
            double rateTotal=0;
            var totalTime=AugmentedData.Sum(x=>x.AmountOfTimeLastRun);
            //Sample a new mu
            //Gamma is parameterized with shape and rate (rate = 1/scale)
            for (int i = 0; i < _pMutationRates.Length; i++)
            {
                int totalMutations = AugmentedData.Select(x => x.MutCounter.CountOfEachMutation[i - 1]).Sum();
                double newRate=RandomVariateGenerator.GammaSample((priorAlpha + totalMutations), (priorBeta + totalTime));
                rateTotal+=newRate;
                _pMutationRates[i]=newRate;  
            }
            ClassProbabilities = _pMutationRates.ElementDivide(rateTotal);
			CreateCumulativeProbs ();
			PointMass = false;
		}

        /// <summary>
        /// Tries to guess good initial parameters based on some observed values.
        /// </summary>
        /// <param name="Data"></param>
        public void InitializeWithObservedData(List<ObservedWell> Data)
        {
            //A mutation escapes loss at probability ~2s, and it rises to high frequency with probability 
            //Ne = N0*growthrate*t or N0*ln(2)*(Gen. Between Transfers).
            //time to hit X% frequency is: T= ln((X/10)*(Ne-1))/s

            //so here is the strategy, ignoring clonal interferance, the expected number of each class present is equal to the 
            //number that escape drift each generation times the frequency they achieve after X amount of time, so will set these two equal

            //number that appear each generation = Ne*mu*2s
            //end frequency = 1/ (1+exp(-s*t)*(Ne-1)
            //so expectated frequency = Sum_(over generations) Ne*mu*2s*(1/(1+exp(-s*t)*(Ne-1))
            //and this can be solved for the initial rate.
            var totTransfers = Data.Select(x => x.TotalTransfers).Distinct();
            if (totTransfers.Count() > 1)
            {
                throw new Exception("Code needs to be changed to handle different number of transfers.");
            }
            //otherwise just go by variables
            var popSizes = Data.GroupBy(y => y.PopSize).Select(x => new { PopSize = x.Key, Obs = x.ToList() }).ToList();
            for(int i=1;i<NumberOfClassesIncludingNeutral;i++)
            {
                double s = (MidPoints[i] / MidPoints[0]) - 1;
                //estimate rate for each pop size
                double muEst = 0;// new double[popSizes.Count];
                List<double> estimates = new List<double>();
                foreach(var curPop in popSizes)
                {
                    var ps=curPop.PopSize;
                    var Ne = ps.Ne;
                    var totalGens = curPop.Obs[0].TotalGenerations;
                    var ne2s = Ne * 2 * s;
                    double[] expectations = new double[(int)totalGens];
                    var expectedFreqSansMu=ne2s*Enumerable.Range(0,(int)totalGens).Select(t=>  1 / (1+Math.Exp(-s*(totalGens-t))*(Ne-1))).Sum();
                    var obsFreq=curPop.Obs.Where(x=>x.BinClass==i).Count()/(double)curPop.Obs.Count;
                    var curEst = obsFreq / expectedFreqSansMu;
                    estimates.Add(curEst);
                    muEst += curEst / popSizes.Count;//get average by summing x*(1/N)
                }
                estimates.Add(1e-20);
                _pMutationRates[i - 1] = estimates.Max();
            }
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
