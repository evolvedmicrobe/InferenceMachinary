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
        public double TotalTime=0;
        DiscretizedDFE dfe;
        public MutationCounter MutCounter;

        public EvolvingPopulation(DiscretizedDFE dfe, PopulationSize popSize)
        {
            PopSizes = new double[dfe.NumberOfClassesIncludingNeutral];
            popRelativeFitnesses = new double[dfe.MidPoints.Length];
            dfe.MidPoints.CopyTo(popRelativeFitnesses, 0);
            LogrelativePopIncrease = Math.Log(popSize.NF / popSize.N0);
            InvRelativePopIncrease = popSize.N0 / popSize.NF;
            PopSizes[0] = popSize.N0;
            N0 = popSize.N0;
            NF = popSize.NF;
            this.dfe = dfe;
            MutCounter = new MutationCounter(dfe);
        }

        public EvolvingPopulation(DiscretizedDFE dfe, ObservedWell well) :this(dfe,well.PopSize)
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
            //how much growth occurs, approximate by how long the mean fitness would take to grow
            double GrowthTime = Math.Log(NF / curPopSize) / (meanFitnessStart);
            //First Handle Mutations
            double[] newMuts = new double[PopSizes.Length];            
            double[] FinalPopSize = PerformanceExtensions.Element_A_Times_E_ToThe_RT(PopSizes,popRelativeFitnesses,GrowthTime);
            double[] NFminusN0akaTimeForPoisson = FinalPopSize.ElementSubtract(PopSizes);//also the time for poisson.
            //Sum of N_f- N_0, note approximate as if a mutation occurs that one guy that mutates will no longer contribute to this time
            TotalTime += NFminusN0akaTimeForPoisson.Sum();
            var mutRates=dfe.MutationRates;
            for (int i = 0; i < PopSizes.Length; i++)
            {
                //Only work with populations that exist to mutate
                var popSize=PopSizes[i];
                if (popSize > 0)
                {                 
                    //New algorithm, for each class we will go through and mutate
                    //the expected number of each fitness class
                    //TODO: Decide later if in the case of very few classes (mean muts =0) we should not do one poisson for all classes and 
                    //assign after the fact. Note small poissons are ~3X more expensive than single unifrom with the MersenneTwister RNG
                    for(int j=0;j<mutRates.Length;j++) {
                        //Determine the mean number of mutations
                        double meanMutForClass = mutRates[j] * popSize;                    
                        //Sample
                        int mutNumber = RandomVariateGenerator.PoissonSample(meanMutForClass);
                        int w = j + 1;                        
                        //Get the maximum time on the rescaled valued, and subtract one as it ranges from 1 to this high value, so this
                        //will be the multiplication factor
                        MutCounter.CountOfEachMutation[w]+=mutNumber;
                        //If higher, add to population
                        if (w > i)//Not the *i* index includes the zero class, while the *j* index does not, so corresponds to i+1
                        {
                            //TODO: Why is 1 here?  Did I do that to account for the need of one division to happen?
                            //time seems to be on scale [1,GrowthTime]
                            //double maxRescaled = Math.Exp(GrowthTime * dfe.MidPoints[i]) - 1.0;                        
                            double maxRescaled = Math.Exp(GrowthTime * dfe.MidPoints[i]);                        
                            
                            //Now generate a mutation for each class
                            for (int k = 0; k < mutNumber; k++)
                            {
                                //double uniformTime = 1.0 + RandomVariateGenerator.NextDouble() * maxRescaled;
                                double uniformTime = RandomVariateGenerator.NextDouble() * maxRescaled;
                                //Convert back
                                double actualTime = Math.Log(uniformTime) / dfe.MidPoints[i];
                                //Find out how much the mutant grew, then add it to the new group
                                //and subtract from the old population group the growth it would have contributed there
                                //had it not mutated
                                double gr = dfe.MidPoints[w];
                                double GT = Math.Exp(gr * (GrowthTime - actualTime));
                                newMuts[w] += GT;
                                double old_gr = dfe.MidPoints[i];
                                double oldGT = Math.Exp(old_gr * (GrowthTime - actualTime));
                                if (oldGT > FinalPopSize[i]) //if the growth is higher due to the approximation, subtract it all, should rarely happen.
                                {
                                    FinalPopSize[i] = 0.0; }
                                else {
                                    FinalPopSize[i] = FinalPopSize[i] - oldGT;
                                }}}}}}
            //Now combine the new mutants with the deterministic growth
            newMuts.ElementAddInPlace(FinalPopSize); //do as two steps to avoid array allocation
            PopSizes = newMuts;
            //Now sample in to the next generation after the transfer.
            double expectationFactor = N0 / PopSizes.Sum();
            for (int i = 0; i < PopSizes.Length; i++) {
                var cPopSize=PopSizes[i];
                if (cPopSize> 0)
                {
                    double expectation = cPopSize * expectationFactor;
                    PopSizes[i] = RandomVariateGenerator.PoissonSample(cPopSize);
                }}
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
            double[] MeanMutations = FinalPopSize.ElementSubtract(PopSizes).ElementMultiply(dfe.MutationRates);

            //double timeForPoisson = (FinalPopSize - PopSizes).Sum();
            double[] timeForPoisson = FinalPopSize.ElementSubtract(PopSizes);
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
