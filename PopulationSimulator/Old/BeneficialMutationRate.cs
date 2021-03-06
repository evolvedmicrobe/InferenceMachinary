﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopulationSimulator.Old
{
	public class BeneficialMutationRate
	{
		public  double priorAlpha = 1.0;
		public double priorBeta = 1.0;
		public double rate;

		public void SampleRate (List<ObservedWell> ObsData)
		{
			int totalMutations = (from x in ObsData
			                      select x.MutCounter.CountOfEachMutation.Sum ()).Sum ();
			double totalTime = (from x in ObsData
			                    select x.PopSize.TotalGrowth * x.TotalTransfers).Sum ();
			double totTime2 = (from x in ObsData
			                   select x.AmountOfTimeLastRun).Sum ();
			double dif = totalTime - totTime2;
			double percDif = dif / totalTime;
			//Sample a new mu
			//Gamma is parameterized with shape and rate (rate = 1/scale)
            rate = RandomVariateGenerator.GammaSample(priorAlpha + totalMutations, (priorBeta + totTime2));
            //Former code from Infer.Net, took shape and scale.            
			//rate = Gamma.Sample (priorAlpha + totalMutations, 1.0 / (priorBeta + totTime2));
		}
	}
}
