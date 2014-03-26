using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopulationSimulator;
using System.IO;
namespace PopulationSimulator.DataSimulation
{
    public class BasicSimulator
    {
        public static void SimulateData()
        {
            DiscretizedDFE dfe = BasicSimulator.CreateDBFEFromTruncExponetialArray();
            BeneficialMutationRate be = new BeneficialMutationRate();
            be.rate = 9e-6;
            //now to simulate
            List<EvolvingPopulation> eps = new List<EvolvingPopulation>();
            ///Hacky at the moment, going to discretize
            for (int i = 0; i < 48; i++)
            {
                eps.Add(new EvolvingPopulation(dfe, PopulationSize.LargePopFromExperiment, be));
                eps.Add(new EvolvingPopulation(dfe, PopulationSize.SmallPopFromExperiment, be));
            }
            for(int i=0;i<PopulationSize.ExperimentTransferNumber;i++)
            {
                eps.ForEach(x => x.GrowOneCycle());
            }
            //now output
            StreamWriter sw = new StreamWriter("SimulationResults.csv");
            sw.WriteLine("Num,Size,W,Name");
            foreach(var s in eps)
            {
                int i = s.SamplePopulation();
                double w;
                if (i == 0)
                    w = 1;
                else
                    w=dfe.GetGrowthRateForBin(i-1)/GibbsPopulationSimulator.ANCESTRALGROWTHRATE;
                
                string pop ="2";
                if(s.N0==PopulationSize.LargePopFromExperiment.N0) {pop="1";}
                sw.WriteLine("1," + pop + "," + w.ToString() + ",Simulation");
            }
            sw.Close();
        }
        public static DiscretizedDFE CreateDBFEFromTruncExponetialArray()
        {
            TruncatedExponentialDBFE te = new TruncatedExponentialDBFE(.04, .2);
            DiscretizedDFE dfe = new DiscretizedDFE(.2, 20);
            //Simulate several draws from this distribution, then load it in to the DBFE
            var simRes = Enumerable.Range(0, 10000).Select(x => dfe.AssignFitnessToBin(1 + te.Sample())).GroupBy(x => x).Select(z => new { Index = z.Key, Count = (double)z.Count() }).ToDictionary(b => b.Index, q => q.Count);
            double[] probs = new double[dfe.NumberOfClassesIncludingNeutral - 1];
            for (int i = 0; i < probs.Length; i++)
            {
                if (simRes.ContainsKey(i)) { probs[i] = simRes[i]; }
                else { probs[i] = 0; }
            }
            probs = probs.ElementDivide(probs.Sum());
            dfe.SetProb(probs);
            return dfe;
        }

    }
}
