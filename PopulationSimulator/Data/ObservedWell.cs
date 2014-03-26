using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopulationSimulator
{
    
    public class ObservedWell
    {
        public PopulationSize PopSize;
        public double TotalTransfers;
        public double ObservedFitness;
        public double AmountOfTimeLastRun;
        public int NumberOfSimulationsLastRun = 0;
        /// <summary>
        /// A class that keeps track of all the mutations that have been observed in this well from each bin.
        /// Used to update the prior.
        /// </summary>
        public MutationCounter MutCounter;
        /// <summary>
        /// 0 is the neutral class.
        /// </summary>
        public readonly int BinClass;
        public double TotalGenerations
        {
            get { return this.TotalTransfers * this.PopSize.GenerationsInBetweenTransfers; }
        }
        public ObservedWell(double Transfers, double W, DiscretizedDFE dfe,PopulationSize size)
        {
            this.TotalTransfers = Transfers;
            this.ObservedFitness = W;
            BinClass= dfe.AssignFitnessToBin(W);
            MutCounter = new MutationCounter(dfe);
            if (BinClass != 1)
            { MutCounter.AddCountToClass(1, BinClass); }
            PopSize = size;
        }
    }
}
