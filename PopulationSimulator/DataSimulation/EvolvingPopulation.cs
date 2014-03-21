using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationSimulator
{

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
        DiscretizedDFE dfe;
        BeneficialMutationRate mu;
        public MutationCounter MutCounter;

        public EvolvingPopulation(DiscretizedDFE dfe, PopulationSize popSize, BeneficialMutationRate mu)
        {
            PopSizes = new double[dfe.NumberOfClassesIncludingNeutral];
            popRelativeFitnesses = new double[dfe.MidPoints.Length];
            dfe.MidPoints.CopyTo(popRelativeFitnesses, 0);
            LogrelativePopIncrease = Math.Log(popSize.NF / popSize.N0);
            InvRelativePopIncrease = popSize.N0 / popSize.NF;
            PopSizes[0] = popSize.N0;
            N0 = popSize.N0;
            NF = popSize.NF;
            this.mu = mu;
            this.dfe = dfe;
            MutCounter = new MutationCounter(dfe);
        }

        public EvolvingPopulation(DiscretizedDFE dfe, ObservedWell well, BeneficialMutationRate mu) :this(dfe,well.PopSize,mu)
        {}

        /// <summary>
        /// Advance through one transfer
        /// </summary>
        /// <param name="Mutations"></param>
        /// <returns></returns>
        public void GrowOneCycle()
        {
            double curPopSize = PopSizes.Sum();
            double[] freqs = PopSizes.ElementDivide(curPopSize);
            //sum Freq*Fitness
            double meanFitnessStart = freqs.ElementMulitplyAndSum(popRelativeFitnesses);
            //how much growth occurs
            double GrowthTime = Math.Log(NF / curPopSize) / (meanFitnessStart);
            //First Handle Mutations
            double[] newMuts = new double[PopSizes.Length];
            
            //double[] FinalPopSize = PopSizes.ElementMultiply(popRelativeFitnesses.ElementMultiply(GrowthTime).Exp());
            double[] FinalPopSize = PerformanceExtensions.Element_A_Times_E_ToThe_RT(PopSizes,popRelativeFitnesses,GrowthTime);
            
            //double[] MeanMutations = FinalPopSize.ElementSubtract(PopSizes).ElementMultiply(mu.rate);
            double[] MeanMutations = FinalPopSize.ElementSubtractAndMultiplyByConstant(PopSizes,mu.rate);
            
            //Sum of N_f- N_0
            double timeForPoisson = FinalPopSize.SummedDifference(PopSizes);

            TotalTime += timeForPoisson;
            double MutantGrowth = 0.0;
            for (int i = 0; i < PopSizes.Length; i++)
            {
                //Only work with populations that exist to mutate
                if (PopSizes[i] > 0)
                {
                    //Determine the mean number of mutations
                    double meanMutForClass = MeanMutations[i];
                    //Sample
                    int mutNumber = RandomVariateGenerator.PoissonSample(meanMutForClass);
                    //Get the maximum time on the rescaled valued, and subtract one as it ranges from 1 to this high value, so this
                    //will be the multiplication factor
                    double maxRescaled = Math.Exp(GrowthTime * dfe.MidPoints[i]) - 1.0;
                    //Now generate a mutation for each class
                    for (int j = 0; j < mutNumber; j++)
                    {
                        double uniformTime = 1.0 + RandomVariateGenerator.NextDouble() * maxRescaled;
                        //Convert back
                        double actualTime = Math.Log(uniformTime) / dfe.MidPoints[i];
                        int w = dfe.GetRandomBinAssignment();
                        MutCounter.CountOfEachMutation[w]++;
                        //If higher, add to population
                        if (w > i)
                        {
                            double gr = dfe.MidPoints[w];
                            //Find out how much the mutant grew, then add it to the new group
                            //and subtract it from the old population group
                            double GT = Math.Exp(gr * (GrowthTime - actualTime));
                            newMuts[w] += GT;
                            MutantGrowth += GT;
                            if (GT > FinalPopSize[i])
                            {
                                FinalPopSize[i] = 0.0;
                                break;
                            }
                            else
                            {
                                //Note this is approximate
                                FinalPopSize[i] = FinalPopSize[i] - GT;
                            }
                        }
                    }
                }
            }
            //DoubleArray Freqs = PopSizes / NF;
            double[] Freqs = PopSizes.ElementDivide(NF);

            //Now combine the new mutants with the deterministic growth
            PopSizes = newMuts.ElementAdd(FinalPopSize);

            //DoubleArray expectNumber = N0 * (PopSizes / PopSizes.Sum());
            //double[] expectNumber = PopSizes.ElementDivide(PopSizes.Sum()).ElementMultiply(N0);
            double[] expectNumber = PopSizes.ElementDivide(PopSizes.Sum()/N0);//

            //Now sample
            for (int i = 0; i < PopSizes.Length; i++)
            {
                if (PopSizes[i] > 0)
                {
                    PopSizes[i] = RandomVariateGenerator.PoissonSample((double)expectNumber[i]);
                }
            }
        }

        public void GrowOneCycleLabeled()
        {
            double curPopSize = PopSizes.Sum();
            double[] freqs = PopSizes.ElementDivide(curPopSize);

            double meanFitnessStart = freqs.ElementMultiply(popRelativeFitnesses).Sum();
            double GrowthTime = Math.Log(NF / curPopSize) / (meanFitnessStart);
            //First Handle Mutations
            double[] newMuts = new double[PopSizes.Length];
            double[] FinalPopSize = PopSizes.ElementMultiply(popRelativeFitnesses.ElementMultiply(GrowthTime).Exp());
            double[] MeanMutations = FinalPopSize.ElementSubtract(PopSizes).ElementMultiply(mu.rate);

            //double timeForPoisson = (FinalPopSize - PopSizes).Sum();
            double timeForPoisson = FinalPopSize.ElementSubtractAndSum(PopSizes);

            TotalTime += timeForPoisson;
            double MutantGrowth = 0.0;
            int halfPop = PopSizes.Length / 2;

            for (int i = 0; i < PopSizes.Length; i++)
            {
                //Only work with populations that exist to mutate
                if (PopSizes[i] > 0)
                {
                    //Determine the mean number of mutations
                    double meanMutForClass = MeanMutations[i];
                    //Sample
                    int mutNumber = RandomVariateGenerator.PoissonSample(meanMutForClass);

                    //Get the maximum time on the rescaled valued, and subtract one as it ranges from 1 to this high value, so this
                    //will be the multiplication factor
                    double maxRescaled = Math.Exp(GrowthTime * dfe.MidPoints[i]) - 1.0;
                    //Now generate a mutation for each class
                    for (int j = 0; j < mutNumber; j++)
                    {
                        double uniformTime = 1.0 + RandomVariateGenerator.NextDouble() * maxRescaled;
                        //Convert back

                        double actualTime = Math.Log(uniformTime) / dfe.MidPoints[i];
                        int w = dfe.GetRandomBinAssignment() + (i / halfPop) * halfPop;
                        MutCounter.CountOfEachMutation[w]++;
                        //If higher, add to population
                        if (w > i)
                        {
                            double gr = dfe.MidPoints[w];
                            //Find out how much the mutant grew, then add it to the new group
                            //and subtract it from the old population group
                            double GT = Math.Exp(gr * (GrowthTime - actualTime));
                            newMuts[w] += GT;
                            MutantGrowth += GT;
                            if (GT > FinalPopSize[i])
                            {
                                FinalPopSize[i] = 0.0;
                                break;
                            }
                            else
                            {
                                //Note this is approximate
                                FinalPopSize[i] = FinalPopSize[i] - GT;
                            }
                        }
                    }
                }
            }

            double[] Freqs = PopSizes.ElementDivide(NF);

            //Now combine the new mutants with the deterministic growth
            //PopSizes = newMuts + FinalPopSize;
            PopSizes = newMuts.ElementAdd(FinalPopSize);

            //double[] expectNumber = PopSizes.ElementDivide (PopSizes.Sum ()).ElementMultiply (N0);
            double[] expectNumber = PopSizes.ElementDivide(PopSizes.Sum() / N0);//.ElementMultiply(N0);

            //Now sample
            double Size = expectNumber.Sum();
            for (int i = 0; i < PopSizes.Length; i++)
            {
                if (PopSizes[i] > 0)
                {     
                    PopSizes[i] = RandomVariateGenerator.PoissonSample(expectNumber[i]);
                }
            }
        }

        public int SamplePopulation()
        {
            double[] Freqs = PopSizes.ElementDivide(PopSizes.Sum());
            int val = RandomVariateGenerator.GetMultinomialSample(Freqs);
            return val;
          
        }
    }
}
