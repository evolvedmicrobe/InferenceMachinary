using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
namespace PopulationSimulator
{
    public class RandomVariateGenerator
    {
        public static MersenneTwister MT = new MersenneTwister(Guid.NewGuid().GetHashCode(), true);
        public static double NextDouble() {
            return MT.NextDouble();
        }

        public static int PoissonSample(double lambda)
        {
            return Poisson.Sample(MT, lambda);
        }
        public static double GammaSample(double shape, double invScale)
        {
            return Gamma.Sample(MT, shape,invScale);
        }
        public static double[] DirichletSample(double[] alphas)
        {
            return Dirichlet.Sample(MT, alphas);
        }
    }
}
