using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PopulationSimulator
{
    public class DataCreator
    {
        public static DiscretizedDFE dfe;
        public static List<ObservedWell> LoadData()
        {
            string direc= @"C:\Users\Clarity\Documents\My Dropbox\EvolutionExperimentDB\Analysis\";
             direc = @"D:\Dropbox\EvolutionExperimentDB\Analysis\";
            string file = direc + "FitnessesFixed2.csv";
            StreamReader SR = new StreamReader(file);
            string line;
            dfe = new DiscretizedDFE(.0625,.25, 7);
            SR.ReadLine();
            PopulationSize curps;
            List<ObservedWell> data = new List<ObservedWell>();
            PopulationSize ps1 = new PopulationSize(3.95e6, 2.53e8);
            PopulationSize ps2 = new PopulationSize(1.03e4, 4.22e7);
            MutationCounter MC1 = new MutationCounter(dfe);
            MutationCounter MC2 = new MutationCounter(dfe);
            int count = 0;
            while ((line = SR.ReadLine()) != null)
            {
                count++;
                if (count == 1)
                {
                    continue;
                }
                string[] sp = line.Split(',');
                int Size=Convert.ToInt32(sp[1]);
                double fitness = Convert.ToDouble(sp[2]);
                if (Size == 1)
                    curps = ps1;
                else
                    curps = ps2;
                int NumrTransfers = Convert.ToInt32(sp[3]);
                ObservedWell ow = new ObservedWell(NumrTransfers, fitness, dfe, curps);
                if (Size == 1)
                { MC1.AddCountToClass(1, ow.binClass); }
                else { MC2.AddCountToClass(1, ow.binClass); ; }
                Console.WriteLine(String.Join("\t", MC1.CountOfEachMutation.Select(x => x.ToString())));
                data.Add(ow);
                Console.WriteLine(sp[4] + " - " + ow.binClass.ToString());
            }
            SR.Close();
           
            Console.WriteLine("Count by population - " +count.ToString());
            Console.WriteLine("Large");
            Console.WriteLine(String.Join("    ", Enumerable.Range(0, 10)));
            Console.WriteLine(String.Join("    ", MC1.CountOfEachMutation.Select(x => x.ToString())));
            Console.WriteLine(String.Join("    ", MC2.CountOfEachMutation.Select(x => x.ToString())));
            return data;

        }
        public static List<ObservedWell> CreateData()
        {
            dfe=new DiscretizedDFE(.3,20);
            List<ObservedWell> data = new List<ObservedWell>();
            PopulationSize ps1 = new PopulationSize(3.95e6, 2.53e8);
            PopulationSize ps2 = new PopulationSize(1.03e4, 4.22e7);
            for (int i = 0; i < 47; i++)
            {
                ObservedWell ow = new ObservedWell(21, .225 + ThreadSafeRandomGenerator.NextDouble() * .01,dfe,ps2);
                data.Add(ow);
            }
            for (int i = 47; i < 96; i++)
            {
                ObservedWell ow = new ObservedWell(21, ThreadSafeRandomGenerator.NextDouble() * .01, dfe, ps2);
                data.Add(ow);
            }
            for (int i = 0; i < 86; i++)
            {
                ObservedWell ow = new ObservedWell(21, .225 + ThreadSafeRandomGenerator.NextDouble() * .01, dfe, ps1);
                data.Add(ow);
            }
            return data;



        }
    }
}
