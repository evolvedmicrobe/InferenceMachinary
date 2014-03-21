using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationSimulator.DataSimulation
{
    public class TruncatedExponentialDBFE
    {
        double mean;
        double max;
        public TruncatedExponentialDBFE(double mean, double max)
        {
            this.mean = mean; this.max = max;
        }
        public double Sample()
        {
            for (; ; )
            {
                double next = RandomVariateGenerator.ExponentialSample(mean);
                if (next < max) return next;
            }
        }
    }
}
