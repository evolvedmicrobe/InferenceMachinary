using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace PopulationSimulator
{
	public class PopulationSimulator
	{
		DiscretizedDFE dfe;
		BeneficialMutationRate mu;
		public int WellsSimulatedCounter = 0;
		public List<int> SimNumbers = new List<int> ();
		public int TotalSimulationsCounter;
		public static object PoissonGeneratorLock = new object ();
		public const double ANCESTRALGROWTHRATE = 0.693147180559945;
		public PopulationSimulator (DiscretizedDFE dfe, BeneficialMutationRate mu)
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
			sr.SampledIsolate = EP.SamplePopulationNotCombiningLowFitness ();
			return sr;
		}

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

		/// <summary>
		/// Poisson sampling at each time, with a max epistsis function, so W12=Max(W1,W2)
		/// </summary>
		public class EvolvingPopulation
		{
			public double[] PopSizes;
			public double[] popRelativeFitnesses;
			public double N0;
			public double NF;
			double LogrelativePopIncrease;
			double InvRelativePopIncrease;
			public double TotalTime = 0;
			ObservedWell currentWell;
			DiscretizedDFE dfe;
			BeneficialMutationRate mu;
			public MutationCounter MutCounter;

			public EvolvingPopulation (DiscretizedDFE dfe, PopulationSize popSize, BeneficialMutationRate mu)
			{
				PopSizes = new double[dfe.NumberOfClassesIncludingNeutral];
				popRelativeFitnesses = new double[dfe.MidPoints.Length];
				dfe.MidPoints.CopyTo (popRelativeFitnesses, 0);
				LogrelativePopIncrease = Math.Log (popSize.NF / popSize.N0);
				InvRelativePopIncrease = popSize.N0 / popSize.NF;
				PopSizes [0] = popSize.N0;
				N0 = popSize.N0;
				NF = popSize.NF;
				this.mu = mu;
				this.dfe = dfe;
				MutCounter = new MutationCounter (dfe);
			}

			public EvolvingPopulation (DiscretizedDFE dfe, ObservedWell well, BeneficialMutationRate mu)
			{
				PopSizes = new double[dfe.NumberOfClassesIncludingNeutral];
				popRelativeFitnesses = new double[dfe.MidPoints.Length];
				dfe.MidPoints.CopyTo (popRelativeFitnesses, 0);
				LogrelativePopIncrease = Math.Log (well.PopSize.NF / well.PopSize.N0);
				InvRelativePopIncrease = well.PopSize.N0 / well.PopSize.NF;
				PopSizes [0] = well.PopSize.N0;
				N0 = well.PopSize.N0;
				NF = well.PopSize.NF;
				currentWell = well;
				this.mu = mu;
				this.dfe = dfe;
				MutCounter = new MutationCounter (dfe);
			}

			/// <summary>
			/// Advance through one transfer
			/// </summary>
			/// <param name="Mutations"></param>
			/// <returns></returns>
			public void GrowOneCycle ()
			{
				double curPopSize = PopSizes.Sum ();
				double[] freqs = PopSizes.ElementDivide (curPopSize);
                
				double meanFitnessStart = freqs.ElementMulitplyAndSum(popRelativeFitnesses);
				double GrowthTime = Math.Log (NF / curPopSize) / (meanFitnessStart);
				//First Handle Mutations
				double[] newMuts = new double[PopSizes.Length];
				double[] FinalPopSize = PopSizes.ElementMultiply (popRelativeFitnesses.ElementMultiply (GrowthTime).Exp ());
				double[] MeanMutations = FinalPopSize.ElementSubtract (PopSizes).ElementMultiply (mu.rate);
                
				double timeForPoisson = FinalPopSize.ElementSubtract (PopSizes).Sum ();
                
				TotalTime += timeForPoisson;
				double MutantGrowth = 0.0;
				for (int i = 0; i < PopSizes.Length; i++) {                    
					//Only work with populations that exist to mutate
					if (PopSizes [i] > 0) {
						//Determine the mean number of mutations
						double meanMutForClass = MeanMutations [i];
						//Sample
                        int mutNumber = RandomVariateGenerator.PoissonSample(meanMutForClass);                           
                        
						//Get the maximum time on the rescaled valued, and subtract one as it ranges from 1 to this high value, so this
						//will be the multiplication factor
						double maxRescaled = Math.Exp (GrowthTime * dfe.MidPoints [i]) - 1.0;
						//Now generate a mutation for each class
						for (int j = 0; j < mutNumber; j++) {
							double uniformTime = 1.0 + RandomVariateGenerator.NextDouble() * maxRescaled;
							//Convert back
                           
							double actualTime = Math.Log (uniformTime) / dfe.MidPoints [i];
							int w = dfe.GetRandomBinAssignment ();
							MutCounter.CountOfEachMutation [w]++;                           
							//If higher, add to population
							if (w > i) {
								double gr = dfe.MidPoints [w];
								//Find out how much the mutant grew, then add it to the new group
								//and subtract it from the old population group
								double GT = Math.Exp (gr * (GrowthTime - actualTime));
								newMuts [w] += GT;
								MutantGrowth += GT;
								if (GT > FinalPopSize [i]) {
									FinalPopSize [i] = 0.0;
									break;
								} else {
									//Note this is approximate
									FinalPopSize [i] = FinalPopSize [i] - GT;
								}                                
							}                            
						}
					}                    
				}
				//DoubleArray Freqs = PopSizes / NF;
				double[] Freqs = PopSizes.ElementDivide (NF);
                
				//Now combine the new mutants with the deterministic growth
				PopSizes = newMuts.ElementAdd (FinalPopSize);
                
				//DoubleArray expectNumber = N0 * (PopSizes / PopSizes.Sum());
				double[] expectNumber = PopSizes.ElementDivide (PopSizes.Sum ()).ElementMultiply (N0);
                
				//Now sample
				double Size = expectNumber.Sum ();
				for (int i = 0; i < PopSizes.Length; i++) {
					if (PopSizes [i] > 0) {//PopSizes[i]=Poisson.Sample(expectNumber[i]);     
						PopSizes [i] = RandomVariateGenerator.PoissonSample((double)expectNumber [i]);
					}
				}                               
			}

			public void GrowOneCycleLabeled ()
			{
				double curPopSize = PopSizes.Sum ();
				double[] freqs = PopSizes.ElementDivide (curPopSize);

				double meanFitnessStart = freqs.ElementMultiply (popRelativeFitnesses).Sum ();
				double GrowthTime = Math.Log (NF / curPopSize) / (meanFitnessStart);
				//First Handle Mutations
				double[] newMuts = new double[PopSizes.Length];
				double[] FinalPopSize = PopSizes.ElementMultiply (popRelativeFitnesses.ElementMultiply (GrowthTime).Exp ());
				double[] MeanMutations = FinalPopSize.ElementSubtract (PopSizes).ElementMultiply (mu.rate);

				//double timeForPoisson = (FinalPopSize - PopSizes).Sum();
				double timeForPoisson = FinalPopSize.ElementSubtractAndSum (PopSizes);

				TotalTime += timeForPoisson;
				double MutantGrowth = 0.0;
				int halfPop = PopSizes.Length / 2;
                
				for (int i = 0; i < PopSizes.Length; i++) {
					//Only work with populations that exist to mutate
					if (PopSizes [i] > 0) {
						//Determine the mean number of mutations
						double meanMutForClass = MeanMutations [i];
						//Sample
						int mutNumber = RandomVariateGenerator.PoissonSample(meanMutForClass);

						//Get the maximum time on the rescaled valued, and subtract one as it ranges from 1 to this high value, so this
						//will be the multiplication factor
						double maxRescaled = Math.Exp (GrowthTime * dfe.MidPoints [i]) - 1.0;
						//Now generate a mutation for each class
						for (int j = 0; j < mutNumber; j++) {
							double uniformTime = 1.0 + RandomVariateGenerator.NextDouble() * maxRescaled;
							//Convert back

							double actualTime = Math.Log (uniformTime) / dfe.MidPoints [i];
							int w = dfe.GetRandomBinAssignment () + (i / halfPop) * halfPop;
							MutCounter.CountOfEachMutation [w]++;
							//If higher, add to population
							if (w > i) {
								double gr = dfe.MidPoints [w];
								//Find out how much the mutant grew, then add it to the new group
								//and subtract it from the old population group
								double GT = Math.Exp (gr * (GrowthTime - actualTime));
								newMuts [w] += GT;
								MutantGrowth += GT;
								if (GT > FinalPopSize [i]) {
									FinalPopSize [i] = 0.0;
									break;
								} else {
									//Note this is approximate
									FinalPopSize [i] = FinalPopSize [i] - GT;
								}
							}
						}
					}
				}
				double[] Freqs = PopSizes.ElementDivide (NF);

				//Now combine the new mutants with the deterministic growth
				//PopSizes = newMuts + FinalPopSize;
				PopSizes = newMuts.ElementAdd (FinalPopSize);

				//double[] expectNumber = PopSizes.ElementDivide (PopSizes.Sum ()).ElementMultiply (N0);
                double[] expectNumber = PopSizes.ElementDivide(PopSizes.Sum()/N0);//.ElementMultiply(N0);

				//Now sample
				double Size = expectNumber.Sum ();
				for (int i = 0; i < PopSizes.Length; i++) {
					if (PopSizes [i] > 0) {//PopSizes[i]=Poisson.Sample(expectNumber[i]);     
						PopSizes [i] = RandomVariateGenerator.PoissonSample(expectNumber [i]);
					}
				}
			}

			public int SamplePopulation ()
			{
				double[] Freqs = PopSizes.ElementDivide (PopSizes.Sum ());
                
				MultinomialSampler m = new MultinomialSampler (Freqs);
				int val = m.GetRandomSample ();
				//if (val == 0)
				//    return 1;
				//else
				//{
				return val;
				//}
			}

			public int SamplePopulationNotCombiningLowFitness ()
			{
				double[] Freqs = PopSizes.ElementDivide (PopSizes.Sum ());
				MultinomialSampler m = new MultinomialSampler (Freqs);
				int val = m.GetRandomSample ();
				return val;
			}
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
