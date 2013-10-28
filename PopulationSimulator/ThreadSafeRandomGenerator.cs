using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PopulationSimulator
{
    //TODO: COULD BE PERFORMANCE BOTTLENECK, NEED TO TEST
    public static class ThreadSafeRandomGenerator 
    {
        //const int NUMBERSINBAG=2000;
        static Random rgenerator = new Random();
        //static ConcurrentBag<double> randomNumbers=new ConcurrentBag<double>();
        //In case I want to change it later, this keeps a list of numbers and then passes them out
        public static double NextDouble()
        {
            double toReturn;
            lock (rgenerator)
            {
                toReturn = rgenerator.NextDouble();
            }
            return toReturn;

            //double toReturn;
            //bool stillValues=randomNumbers.TryTake(out toReturn);
            //if(stillValues)
            //{
            //    return toReturn;
            //}
            //else
            //{
            //    lock(rgenerator)
            //    {
            //        Enumerable.Range(0,NUMBERSINBAG).ToList().ForEach(x=>randomNumbers.Add(rgenerator.NextDouble()));
            //        toReturn=rgenerator.NextDouble();
            //    }
            //    return toReturn;
            //}
        }

        
    }
    public class ThreadSafeRandomGeneratorInstance : Random
    {
        const int NUMBERSINBAG = 5000;
        //Random rgenerator = new Random();
       //ConcurrentBag<double> randomNumbers = new ConcurrentBag<double>();
        public ThreadSafeRandomGeneratorInstance()
        {
            //Fill up collection
            this.NextDouble();

        }
        //In case I want to change it later, this keeps a list of numbers and then passes them out
        public override double NextDouble()
{
    return ThreadSafeRandomGenerator.NextDouble();
            //double toReturn;
            //bool stillValues = randomNumbers.TryTake(out toReturn);
            //if (stillValues)
            //{
            //    return toReturn;
            //}
            //else
            //{
            //    lock (rgenerator)
            //    {
            //        Enumerable.Range(0, NUMBERSINBAG).ToList().ForEach(x => randomNumbers.Add(rgenerator.NextDouble()));
            //        toReturn = rgenerator.NextDouble();
            //    }
            //    return toReturn;
            //}
        }
        //public override int Next(int maxValue)
        //{
        //    throw new Exception("Not implemented");
        //}
        //protected override double Sample()
        //{
        //    throw new Exception("Not implemented");
        //}
        //public override void NextBytes(byte[] buffer)
        //{
        //    throw new Exception("Not implemented");
        //}
        //public override int Next()
        //{
        //    throw new Exception("Not implemented");
        
        //}
        //public override int Next(int minValue, int maxValue)
        //{
        //    throw new Exception("Not implemented");
        //}


    }
    
}
