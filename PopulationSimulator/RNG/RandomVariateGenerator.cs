using System;
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
    }
}
