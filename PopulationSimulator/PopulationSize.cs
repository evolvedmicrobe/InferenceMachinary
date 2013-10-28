using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopulationSimulator
{
    public class PopulationSize
    {
        public double N0;
        public double NF;
        public double GenerationsInBetweenTransfers;
        //Gets the total number of divisions in this well for one round
        public double TotalGrowth
        {
            get
            {
                return (NF - N0);
            }
        }
        public PopulationSize(double n0, double nf)
        {
            this.N0 = n0;
            this.NF = nf;
            this.GenerationsInBetweenTransfers = Math.Log(nf / n0, 2.0);

        }
    }
}
