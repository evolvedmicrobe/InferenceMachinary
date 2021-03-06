﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using System.Threading;
using MathNet.Numerics.Distributions;
namespace PopulationSimulator
{
    public class RandomVariateGenerator
    {
        //public static MersenneTwister MT = new MersenneTwister(Guid.NewGuid().GetHashCode(), true);
        //see https://github.com/mathnet/mathnet-numerics/issues/200
        /// <summary>
        /// Thread save random variable generator.
        /// </summary>
        public static ThreadLocal<MersenneTwister> MT = new ThreadLocal<MersenneTwister>(() => new MersenneTwister(false));
        public static double NextDouble() {
            return MT.Value.NextDouble();
        }

        public static int PoissonSample(double lambda)
        {
            return Poisson.Sample(MT.Value, lambda);
        }
        public static double GammaSample(double shape, double invScale)
        {
            return Gamma.Sample(MT.Value, shape,invScale);
        }
        public static double[] DirichletSample(double[] alphas)
        {
            return Dirichlet.Sample(MT.Value, alphas);
        }

        public static int GetMultinomialSample(double[] probabilities)
        {
            double d = RandomVariateGenerator.NextDouble();
            double cumSum = probabilities[0];
            int i = 0;
            do
            {
                if (d < cumSum) { return i; }
                i++;
                cumSum += probabilities[i];

            }
            while (i < probabilities.Length);
            throw new Exception("Probability in multinomial sampler was goofed!");
            
        }
        public static double ExponentialSample(double mean)
        {
            return Exponential.Sample(MT.Value,1/mean);
        }
    }
}
