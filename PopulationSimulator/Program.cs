using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace PopulationSimulator
{
    class Program
    {
        /// <summary>
        /// Main program which loads and runs the code.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
           // PosteriorSimulations2();
            //RunSimulations();
            GibbsSampler gs;
            if (args.Length > 0)
            {
                gs = new GibbsSampler(args[0], Convert.ToDouble(args[1]));
            }
            else
            {
                gs = new GibbsSampler();
            }
            gs.Run();
        }
        //How long until the population is over 90% mutant
        static void PosteriorSimulations2()
        {
            int numberOfTransfers = 21;
            //StreamWriter SW = new StreamWriter(@"D:\Dropbox\EvolutionExperimentDB\Analysis\TimeFor90PercFixation.csv");
            StreamWriter SW = new StreamWriter(@"C:\Users\Nigel\Documents\My Dropbox\EvolutionExperimentDB\Analysis\TimeFor90PercFixation.csv");
            PopulationSize ps = new PopulationSize(1.03e4, 4.22e7);
            BeneficialMutationRate mu = new BeneficialMutationRate();
            DiscretizedDFE dfe = new DiscretizedDFE(.0625, .25, 7);
            double[] posteriorModeFreqs = new double[] { 2.661e-3, 5.757e-2, 6.036e-2, .533, .291, 5.373e-2, 1.726e-3 };
            posteriorModeFreqs = posteriorModeFreqs.Select(x => x / posteriorModeFreqs.Sum()).ToArray();
            double rate = 4.08e-7;
            mu.rate = rate;
            dfe.ClassProbabilities = posteriorModeFreqs;
            PopulationSize pop = ps;  
            SW.WriteLine("Time");
            for (int reps = 0; reps < 10000; reps++)
            {
                PopulationSimulator.EvolvingPopulation EP;
                EP = new PopulationSimulator.EvolvingPopulation(dfe, pop, mu);
                int i = 0;
                while (true)
                {
                    EP.GrowOneCycle();
                    i++;
                    double tot = EP.PopSizes.Sum();
                    if (((tot - EP.PopSizes[0]) / tot) > .9)
                    {
                        SW.WriteLine(i);
                        break;
                    }

                }
            }

            SW.Close();
        }
        static void PosteriorSimulations()
        {
            
            int numberOfTransfers = 21;
            StreamWriter SW = new StreamWriter(@"C:\Users\Clarity\Documents\My Dropbox\EvolutionExperimentDB\Analysis\PosteriorSimulationOutputSmallPop.csv");
            PopulationSize ps1 = new PopulationSize(3.95e6, 2.53e8);
            PopulationSize ps2 = new PopulationSize(1.03e4, 4.22e7);
            PopulationSize[] sizes = new PopulationSize[] { ps1, ps2 };
            BeneficialMutationRate mu = new BeneficialMutationRate();
            DiscretizedLabelledDFE dfe=new DiscretizedLabelledDFE(.0625,.25,7);
            double[] posteriorModeFreqs=new double[]{2.661e-3,5.757e-2,6.036e-2,.533,.291,5.373e-2,1.726e-3};
            posteriorModeFreqs = posteriorModeFreqs.Select(x => x / posteriorModeFreqs.Sum()).ToArray();
            double rate=4.08e-7;
            mu.rate = rate;
            dfe.SetProb(posteriorModeFreqs);
            PopulationSize pop=ps2;
            string header = string.Join(",", Enumerable.Range(0, numberOfTransfers).Select(x => "Fitness" + Convert.ToString(x)));
            string header2 = string.Join(",", Enumerable.Range(0, numberOfTransfers).Select(x => "Venus" + Convert.ToString(x)));
                
            SW.WriteLine("SIZE," + header2 +","+ header + ",SampleW");
                
            for (int reps = 0; reps < 10000; reps++)
            {
                PopulationSimulator.EvolvingPopulation EP;
                EP = new PopulationSimulator.EvolvingPopulation(dfe, pop, mu);
                int newStart = dfe.MidPoints.Length / 2;
                double HalfPop = EP.PopSizes[0] / 2;
                EP.PopSizes[0] = HalfPop;
                EP.PopSizes[newStart] = HalfPop;
                double[] Res = new double[numberOfTransfers * 2];
                SW.Write(pop.N0.ToString() + ",");
                for (int i = 0; i < numberOfTransfers; i++)
                {
                    EP.GrowOneCycleLabeled();
                    double size = EP.PopSizes.Sum();
                    double VenusSize = EP.PopSizes.Take(EP.PopSizes.Length / 2).Sum();
                    double venusFreq = VenusSize / size;
                    Res[i] = venusFreq;
                    double curPopSize = EP.PopSizes.Sum();
                    double[] freqs = EP.PopSizes.ElementDivide(curPopSize);
                    double meanFitness = freqs.ElementMultiply(EP.popRelativeFitnesses).Sum();
                    Res[i + numberOfTransfers] = meanFitness;
                }
                double size2 = EP.PopSizes.Sum();
                double[] freqs2 = EP.PopSizes.ElementDivide(size2);
                MultinomialSampler ms = new MultinomialSampler(freqs2);
                int sample = ms.GetRandomSample();
                double EndW = EP.popRelativeFitnesses[sample] / Math.Log(2) ;

                SW.Write(string.Join(",", Res.Select(x => x.ToString())) +","+ EndW.ToString()+"\n");
            }
            
            SW.Close();
        }
        static void RunSimulations()
        {
            //PopulationSize ps1 = new PopulationSize(3.95e6, 2.53e8);
            //PopulationSize ps2 = new PopulationSize(1.03e4, 4.22e7);
            //DiscretizedDFE dfe = new DiscretizedDFE(.3, 10);
       
            //PopulationSize[] sizes=new PopulationSize[] {ps1,ps2};
            //dfe = new DiscretizedDFE(.25, 20);
            //BeneficialMutationRate mu = new BeneficialMutationRate();
            double[] logRates = new double[] { -12, -11.5,-11,-10.5, -10,-9.5, -9,-8.5, -8,-7.5, -7, -6.5, -6, -5.5, -5,-4.5 };//{-6,-5.7,-5.4,-5};//
            double[] rates = logRates.Select(x => Math.Pow(10, x)).ToArray();
            int dfeClasses = 15;
            List<double[]> results = new List<double[]>(logRates.Length * dfeClasses);
            int numSims = 5000;
            int numberOfTransfers=21;
            StreamWriter SW = new StreamWriter(@"C:\Users\Clarity\Documents\My Dropbox\EvolutionExperimentDB\Analysis\SimulationOutputFull2.csv");
            
            Parallel.ForEach(rates, rate =>
            {
                PopulationSize ps1 = new PopulationSize(3.95e6, 2.53e8);
                PopulationSize ps2 = new PopulationSize(1.03e4, 4.22e7);
                DiscretizedDFE dfe = new DiscretizedDFE(.3,dfeClasses);

                PopulationSize[] sizes = new PopulationSize[] { ps1, ps2 };
                dfe = new DiscretizedDFE(.25, 20);
                BeneficialMutationRate mu = new BeneficialMutationRate();
                //double rate=1e-6;
                mu.rate = rate;
                Console.WriteLine(rate.ToString());
                for (int i = 1; i < dfe.NumberOfClassesIncludingNeutral; i++)
                {
                //    int i = 10;
                    double curW = dfe.MidPoints[i] / Math.Log(2);
                    PopulationSimulator psim = new PopulationSimulator(dfe, mu);
                    dfe.SetDFEasPointMass(i - 1);
                    foreach (PopulationSize ps in sizes)
                    {
                        //StreamWriter sw2 = new StreamWriter(@"C:\Users\Nigel\Documents\My Dropbox\EvolutionExperimentDB\Analysis\\" + ps.N0.ToString() + ".csv");

                        Console.WriteLine(ps.NF.ToString());
                        int numSuccesses = 0;
                        for (int j = 0; j < numSims; j++)
                        {
                            //if (ps == ps2)
                            //    numberOfTransfers = 11;
                            //else
                            //    numberOfTransfers = 22;
                            var res = psim.SimulatePopulation(ps, numberOfTransfers);
                            if (res.SampledIsolate == i)
                            {
                                numSuccesses++;
                            }
                            //   foreach (var q in res.FrequenciesAtTime)
                            //   {
                            //       sw2.WriteLine(String.Join(",",q.Select(x => Convert.ToString(x))));
                            //   }
                            //   sw2.Close();
                        }
                        double SuccessP = (double)numSuccesses / (double)numSims;
                        lock (results)
                        {
                            results.Add(new double[] { rate, curW, ps.N0, SuccessP });
                        }
                        }
                }
            });
           SW.WriteLine("Rate,Fitness,PopSize,ProbOfSuccess");
            string sep = ", ";
            foreach (var res in results)
            {
                string val = String.Join(sep,res.Select(x => Convert.ToString(x)));
                SW.WriteLine(val);
            }
            SW.Close();
        }
        
    }
}
