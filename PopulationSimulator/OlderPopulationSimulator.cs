//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ShoNS.Array;
//using ShoNS.MathFunc;
//using MicrosoftResearch.Infer.Distributions;
//namespace PopulationSimulator
//{
//    /// <summary>
//    /// Simulates wells
//    /// </summary>
//    public class OlderPopulationSimulator
//    {
//        DiscretizedDFE dfe;
//        BeneficialMutationRate mu;
//        public OlderPopulationSimulator(DiscretizedDFE dfe, BeneficialMutationRate mu)
//        {
//            this.dfe = dfe;
//            this.mu = mu;
//        }       
//        List<TimeFitnessClass> SimulateMutationsBasic(ObservedWell well)
//        {
//            //Simulate a set of mutations, ensuring that they at least have one of the
//            //mutation of interest
//            List<TimeFitnessClass> muts;
//            int count = 0;
//            do
//            {
//                muts = taf.SimulateMutations(mu.rate, well);
//                count = (from x in muts select x.Class).Count(x => x == well.binClass);
//            }
//            while (count == 0);
//            return muts;
//        }
//        public const double ANCESTRALGROWTHRATE = 0.693147180559945;// Math.Log(2.0);
//        SimulationTimesAndFitnesses taf;
//        //Simulate growth in a well
//        public List<TimeFitnessClass> SimulateWell(ObservedWell well)
//        {  
//            int Winner=-1;
//            int NumSims=0;
//            double gensBetweenTransfers = well.PopSize.GenerationsInBetweenTransfers;
//            List<TimeFitnessClass> muts;
//            do
//            {   
//                //Population mean fitness at start and end
//                CompletelyStochasticallyEvolvingPopulation simmedWell = new CompletelyStochasticallyEvolvingPopulation(dfe, well);
//                //Initialize the doubling time to the ancestral value
//                double TransferTime = gensBetweenTransfers;
//                int curMutIndex = 0;
//                for (int i = 0; i < well.TotalTransfers; i++)
//                {
//                    //Find all mutations that occur in this interval
//                    double startTime = i;
//                    double endTime = i * gensBetweenTransfers;
//                    List<TimeFitnessClass> newMuts = new List<TimeFitnessClass>();
//                    double timeRescale=TransferTime/gensBetweenTransfers;
//                    while (curMutIndex < muts.Count)
//                    {
//                        TimeFitnessClass tfc = muts[curMutIndex];
//                        if (tfc.time > endTime)
//                            break;
//                        else
//                        {
//                            tfc.time *= timeRescale;
//                            newMuts.Add(tfc);
//                        }
//                        curMutIndex++;
//                    }

//                    //Advance the simulation, and record how much time they will grow for 
//                    //next time interval so things can be rescaled
//                    TransferTime= simmedWell.GrowOneCycle(newMuts);
//                }
//                //Now sample a mutant
//               Winner = simmedWell.SamplePopulation();
//               NumSims += 1;
//            }
//            while (Winner != well.binClass);
//            return muts;
//        }       
//        /// <summary>
//        /// Poisson sampling at each time, with a max epistsis function, so W12=Max(W1,W2)
//        /// </summary>
//        public class CompletelyStochasticallyEvolvingPopulation
//        {
//            public DoubleArray PopSizes;
//            public DoubleArray popRelativeFitnesses;
//            public double meanFitnessStart;
//            public double meanFitnessEnd;
//            public double midPointFitnessLastTransfer;
//            public double N0;
//            public double NF;
//            double LogrelativePopIncrease;
//            double InvRelativePopIncrease;
//            int currentTransferNumber = 0;
//            public CompletelyStochasticallyEvolvingPopulation(DiscretizedDFE dfe, ObservedWell well)
//            {
//                PopSizes = DoubleArray.Zeros(dfe.NumberOfClassesIncludingNeutral);
//                popRelativeFitnesses = DoubleArray.From(dfe.MidPoints);
//                meanFitnessStart = 0;
//                LogrelativePopIncrease = Math.Log(well.PopSize.NF / well.PopSize.N0);
//                InvRelativePopIncrease = well.PopSize.N0 / well.PopSize.NF;
//                PopSizes[0] = well.PopSize.N0;
//                N0 = well.PopSize.N0;
//                NF = well.PopSize.NF;
//            }
//            /// <summary>
//            /// Advance through one transfer, return the expected time for the next transfer.
//            /// </summary>
//            /// <param name="Mutations"></param>
//            /// <returns></returns>
//            public double GrowOneCycle(IEnumerable<TimeFitnessClass> Mutations)
//            {
//                double curPopSize = PopSizes.Sum();
//                double GrowthTime = Math.Log(NF/curPopSize) / (ANCESTRALGROWTHRATE + meanFitnessStart);
//                PopSizes = PopSizes.ElementMultiply(ArrayMath.Exp(GrowthTime * popRelativeFitnesses));
//                DoubleArray Freqs = PopSizes / NF;
//                meanFitnessEnd = (Freqs.ElementMultiply(popRelativeFitnesses)).Sum();
//                midPointFitnessLastTransfer = .5 * meanFitnessEnd + .5 * meanFitnessStart;
//                meanFitnessStart = meanFitnessEnd;
//                //handle mutations
//                MultinomialSampler m = new MultinomialSampler(Freqs);
//                foreach (TimeFitnessClass tfc in Mutations)
//                {
//                    double w = popRelativeFitnesses[tfc.Class];
//                    if (w < meanFitnessEnd)
//                        continue;
//                    //pick a fitness to grab it from
//                    int group = m.GetRandomSample();
//                    if (popRelativeFitnesses[group] >= w)
//                        continue;
//                    else
//                    {
//                        double Addition = Math.Exp((GrowthTime - tfc.time) * (w - midPointFitnessLastTransfer));
//                        //Add the end mutant population to the start, and approximate the loss in the original
//                        //population by subtracting the end growth from the start (should be only very slight difference)
//                        PopSizes[group] -= Addition;
//                        //This should basically never happen
//                        if (PopSizes[group] < 0)
//                        { PopSizes[group] = 0; }
//                        PopSizes[tfc.Class] += Addition;
//                    }
//                }
//                //Now get expected values and shrink population
//                PopSizes.DivEquals(InvRelativePopIncrease);
//                for(int i=0;i<PopSizes.Length;i++)
//                {
//                    double exp=PopSizes[i];
//                    if(exp>0)
//                        PopSizes[i]=Poisson.Sample(exp);
//                }                //note that I could update the mean population fitness here, assume it should be the same though
//                return Math.Log(NF / curPopSize) / (ANCESTRALGROWTHRATE + meanFitnessEnd);
                
//            }
//            public int SamplePopulation()
//            {
//                DoubleArray Freqs = PopSizes / PopSizes.Sum();
//                MultinomialSampler m = new MultinomialSampler(Freqs);
//                return m.GetRandomSample();
//            }
//        }
//    }

//}
