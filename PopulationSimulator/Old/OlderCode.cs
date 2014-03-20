using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using ShoNS.Array;
//using ShoNS.MathFunc;
//using MicrosoftResearch.Infer.Distributions;
//namespace PopulationSimulator
//{
//    class OlderCode
//    {
//        //NOT WORKING YET!!!
//        public class DeterministicallyEvolvingPopulation
//        {
//            public DoubleArray PopSizes;
//            public DoubleArray popRelativeFitnesses;
//            public double meanFitnessStart;
//            public double meanFitnessEnd;
//            public double midPointFitnessLastTransfer;
//            public double N0;
//            double LogrelativePopIncrease;
//            double InvRelativePopIncrease;
//            int currentTransferNumber = 0;
//            public DeterministicallyEvolvingPopulation(DiscretizedDFE dfe, ObservedWell well)
//            {
//                PopSizes = DoubleArray.Zeros(dfe.NumberOfClassesIncludingNeutral);
//                popRelativeFitnesses = DoubleArray.From(dfe.MidPoints);
//                meanFitnessStart = 0;
//                LogrelativePopIncrease = Math.Log(well.PopSize.NF / well.PopSize.N0);
//                InvRelativePopIncrease = well.PopSize.N0 / well.PopSize.NF;
//                PopSizes[0] = well.PopSize.N0;
//                N0 = well.PopSize.N0;
//            }
//            public void GrowOneCycle()
//            {
//                //Correct growth rate for fitness increase in populations
//                double GrowthTime = LogrelativePopIncrease / (ANCESTRALGROWTHRATE + meanFitnessStart);
//                PopSizes = PopSizes.ElementMultiply(ArrayMath.Exp(GrowthTime * popRelativeFitnesses));
//                //Shrink population
//                PopSizes.DivEquals(InvRelativePopIncrease);
//                DoubleArray Freqs = PopSizes / N0;
//                meanFitnessEnd = (Freqs.ElementMultiply(popRelativeFitnesses)).Sum();
//                currentTransferNumber++;
//                midPointFitnessLastTransfer = .5 * meanFitnessEnd + .5 * meanFitnessStart;
//                meanFitnessStart = meanFitnessEnd;
//            }
//        }
//        //NOT WORKING YET!!!
//        struct StochasticAdditionToDeterministicPopulations
//        {
//            public int PopIndex;
//            public double PopSize;
//        }
//        /// <summary>
//        /// WARNING NOT ACTUALLY WORKING!!!!
//        /// </summary>
//        public class StochasticallyEvolvingSubPopulations
//        {
//            DoubleArray CurrentMembersInEachClass;
//            public DoubleArray popRelativeFitnesses;
//            public double meanFitnessStart;
//            public double meanFitnessEnd;
//            public double midPointFitnessLastTransfer;
//            public double N0;
//            double LogrelativePopIncrease;
//            double InvRelativePopIncrease;
//            int currentTransferNumber = 0;
//            public StochasticallyEvolvingSubPopulations(DiscretizedDFE dfe, ObservedWell well)
//            {
//                CurrentMembersInEachClass = DoubleArray.Zeros(dfe.NumberOfClassesIncludingNeutral);
//                popRelativeFitnesses = DoubleArray.From(dfe.MidPoints);
//                meanFitnessStart = 0;
//                LogrelativePopIncrease = Math.Log(well.PopSize.NF / well.PopSize.N0);
//                InvRelativePopIncrease = well.PopSize.N0 / well.PopSize.NF;
//                N0 = well.PopSize.N0;
//            }
//            public IEnumerable<StochasticAdditionToDeterministicPopulations> StochasticUpdate(double popMeanS, IEnumerable<TimeFitnessClass> newMutations)
//            {
//                //First to deterministically update the guys that made it, note using the mean pop instead of the start pop might
//                //bias, but they should be exactly the same
//                double GrowthTime = LogrelativePopIncrease / (ANCESTRALGROWTHRATE + popMeanS);
//                //Grow any leftovers from last time
//                CurrentMembersInEachClass = CurrentMembersInEachClass.ElementMultiply(ArrayMath.Exp(GrowthTime * popRelativeFitnesses));
//                //Now add all the mutants in
//                foreach (TimeFitnessClass tfc in newMutations)
//                {
//                    double w = popRelativeFitnesses[tfc.Class];
//                    //Skip mutations that already have fitness values
//                    if (w < popMeanS)
//                        continue;
//                    CurrentMembersInEachClass += Math.Exp((GrowthTime - tfc.time) * (popRelativeFitnesses[tfc.Class] - popMeanS));
//                }
//                //Now to poisson sample timepoints
//                for (int i = 0; i < popRelativeFitnesses.Length; i++)
//                {
//                    double w = popRelativeFitnesses[i];
//                    //Skip mutations that already have fitness values
//                    if (w < popMeanS)
//                        continue;
//                }
//                return null;
//            }
//        }
//    }
//}
