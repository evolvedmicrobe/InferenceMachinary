using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace PopulationSimulator
{
    public class GibbsSampler
    {
        StreamWriter SW;
        List<OutputColumn> OutputColumns;
        public BeneficialMutationRate mu;
        public DiscretizedDFE dfe;
        List<ObservedWell> ObsData;
        const string SEPERATOR = "\t";
        public static ThreadSafeRandomGeneratorInstance rnd = new ThreadSafeRandomGeneratorInstance();
        PopulationSimulator ps;
        int curRep;
        void Initialize()
        {
            //ObsData = DataCreator.CreateData();
            ObsData = DataCreator.LoadData();
            dfe = DataCreator.dfe;
            mu = new BeneficialMutationRate();   
        }
        double GetClassProbability(int i)
        {
            return dfe.ClassProbabilities[i];
        }
        public void InitializeOutput(string FileName)
        {
            SW=new StreamWriter(FileName);
            OutputColumns = new List<OutputColumn>();
            OutputColumns.Add(new OutputColumn("State",()=>this.curRep));
            OutputColumns.Add(new OutputColumn("TotalSimulations", () => this.ps.TotalSimulationsCounter));
            OutputColumns.Add(new OutputColumn("Rate", () => this.mu.rate));
            for (int i = 0; i < dfe.ClassProbabilities.Length; i++)
            {
                int j = i;
                OutputColumns.Add(new OutputColumn("Bin-" + i.ToString(), () => GetClassProbability(j)));
            }

            var PopSizes = (from x in ObsData select x.PopSize).Distinct();
            var MutationTypes = (from x in ObsData select x.binClass).Distinct();
            List<OutputColumn> MutsCount = new List<OutputColumn>();
            foreach (var pop in PopSizes)
            {
                foreach (var mut in MutationTypes)
                {
                    var data = (from x in ObsData where x.PopSize == pop & x.binClass == mut select x).ToList();
                    if (data.Count > 0)
                    {
                        CollectionOfData cdata = new CollectionOfData(data, "Pop-" + pop.TotalGrowth.ToString("E2") + "-" + mut.ToString());
                        MutsCount.Add(new OutputColumn(cdata.Name+"-AvgMutsPerSim", () => cdata.AverageMutations()));
                        
                        OutputColumns.Add(new OutputColumn(cdata.Name + "-AvgSims", () => cdata.AverageSimulations()));
                    }
                }
            }
            OutputColumns.AddRange(MutsCount);
            SW.WriteLine(string.Join(SEPERATOR,OutputColumns.Select(x=>x.Name)));

        }
        void OutputState()
        {
            SW.WriteLine(string.Join(SEPERATOR,OutputColumns.Select(x=>x.outFunc().ToString())));
            SW.Flush();
        }
        private double initRate = 7.5e-8;
        public GibbsSampler(string outputName="Results.Log", double StartRate=7.5e-8)
        {
            Initialize();
            initRate = StartRate;
            InitializeOutput(outputName);
            ps=new PopulationSimulator(dfe,mu);
        }
        public void Run()
        {   
            int reps = 10000000;
            ObsData[0].MutCounter.AddCountToClass(30, 1);
            ObsData[0].MutCounter.AddCountToClass(30, 2);
            dfe.UpdateWithNewSamples(ObsData);
           
            mu.rate = initRate;
            DateTime dt = DateTime.Now;
            for (curRep = 0; curRep< reps; curRep++)
            {
                if (curRep > 0)
                {
                    mu.SampleRate(ObsData);
                    dfe.UpdateWithNewSamples(ObsData);
                }
                //CAN'T RUN IN PARALLEL AS POISSON IS NOT THREAD SAFE
                //Think I made it thread safe with new sampler
                ObsData.ForEach(x=>ps.SimulateWell(x));
                if (curRep % 5 == 0)
                { OutputState(); }
                if(curRep%5==0)
                {
                    TimeSpan ts=DateTime.Now.Subtract(dt);
                    Console.WriteLine(ts.TotalMinutes+" Minutes per 5 states");
                    dt = DateTime.Now;
                }
                else if (curRep % 5 == 0)
                {
                    Console.WriteLine(mu.rate.ToString());
                }
            }
            SW.Close();
        }
        public void SampleFreqs()
        {
            
        }
        public delegate double ValueGetter();
        public sealed class OutputColumn
        {
            public readonly string Name;
            public readonly ValueGetter outFunc;
            public OutputColumn(string Name, ValueGetter OutputFunction)
            {
                this.Name = Name;
                this.outFunc = OutputFunction;
            }
        }
    }
}
