using Gridiron.Engine.Domain.Helpers;
using System;

namespace Gridiron.Engine.Simulation.Utilities
{
    /// <summary>
    /// Utility class for generating statistically realistic random values.
    /// Provides normal and log-normal distributions for yardage calculations.
    ///
    /// NFL Statistical Reality:
    /// - Run plays: Right-skewed (log-normal), mean ~4.3 yards, median ~3 yards
    /// - Pass plays: Normal distribution varies by pass type
    /// </summary>
    public static class StatisticalDistributions
    {
        #region Core Distribution Methods

        /// <summary>
        /// Generates a normally distributed random value using the Box-Muller transform.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="mean">Mean of the distribution</param>
        /// <param name="stdDev">Standard deviation of the distribution</param>
        /// <returns>A normally distributed random value</returns>
        public static double Normal(ISeedableRandom rng, double mean, double stdDev)
        {
            // Box-Muller transform: converts two uniform random numbers to normal distribution
            var u1 = rng.NextDouble();
            var u2 = rng.NextDouble();

            // Avoid log(0)
            while (u1 == 0) u1 = rng.NextDouble();

            var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return mean + stdDev * z;
        }

        /// <summary>
        /// Generates a log-normally distributed random value.
        /// Log-normal is right-skewed: most values are small, with occasional large values.
        /// Perfect for run yardage where most runs are 0-4 yards with occasional breakaways.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="mu">Location parameter (not the mean of the output)</param>
        /// <param name="sigma">Scale parameter (not the stddev of the output)</param>
        /// <returns>A log-normally distributed random value (always positive)</returns>
        public static double LogNormal(ISeedableRandom rng, double mu, double sigma)
        {
            var normalValue = Normal(rng, mu, sigma);
            return Math.Exp(normalValue);
        }

        #endregion

        #region Run Yardage Distribution

        /// <summary>
        /// Generates run yardage using a log-normal distribution.
        ///
        /// Target statistics:
        /// - Mean: ~4.3 yards
        /// - Median: ~3 yards
        /// - Negative runs: ~15%
        /// - Breakaway (15+): ~5%
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="skillModifier">Skill differential modifier (-1.0 to +1.0 typical)</param>
        /// <returns>Run yardage (can be negative for TFL)</returns>
        public static int RunYards(ISeedableRandom rng, double skillModifier = 0.0)
        {
            // Log-normal parameters tuned for NFL run distribution
            // mu=1.1, sigma=0.7 produces mean ~3.7, with right skew
            const double mu = 1.1;
            const double sigma = 0.7;

            // Generate base yards from log-normal (always positive)
            var baseYards = LogNormal(rng, mu, sigma);

            // Shift down to allow negatives (TFL)
            // Subtract ~1 so that low rolls can go negative
            var shiftedYards = baseYards - 1.0;

            // Apply skill modifier (better skills = more yards)
            var finalYards = shiftedYards + (skillModifier * 2.0);

            return (int)Math.Round(finalYards);
        }

        /// <summary>
        /// Generates run yardage with explicit parameters for testing/tuning.
        /// </summary>
        public static int RunYards(ISeedableRandom rng, double mu, double sigma, double shift, double skillModifier)
        {
            var baseYards = LogNormal(rng, mu, sigma);
            var shiftedYards = baseYards - shift;
            var finalYards = shiftedYards + (skillModifier * 2.0);
            return (int)Math.Round(finalYards);
        }

        #endregion

        #region Pass Yardage Distribution

        /// <summary>
        /// Pass type enumeration for distribution selection.
        /// </summary>
        public enum PassType
        {
            Screen,
            Short,
            Medium,
            Deep
        }

        /// <summary>
        /// Generates pass yardage using normal distribution based on pass type.
        ///
        /// NFL averages by pass type:
        /// - Screen: ~4 yards (high completion %, low variance)
        /// - Short: ~7 yards (slants, curls, out routes)
        /// - Medium: ~14 yards (crossing routes, intermediate)
        /// - Deep: ~30 yards (go routes, posts) - lower completion %
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="passType">Type of pass attempt</param>
        /// <param name="skillModifier">Skill differential modifier</param>
        /// <returns>Pass yardage (for completions)</returns>
        public static int PassYards(ISeedableRandom rng, PassType passType, double skillModifier = 0.0)
        {
            double mean, stdDev;

            switch (passType)
            {
                case PassType.Screen:
                    mean = 4.0;
                    stdDev = 3.0;
                    break;
                case PassType.Short:
                    mean = 7.0;
                    stdDev = 3.5;
                    break;
                case PassType.Medium:
                    mean = 14.0;
                    stdDev = 5.0;
                    break;
                case PassType.Deep:
                    mean = 30.0;
                    stdDev = 10.0;
                    break;
                default:
                    mean = 10.0;
                    stdDev = 5.0;
                    break;
            }

            // Apply skill modifier to mean
            var adjustedMean = mean + (skillModifier * 3.0);

            var yards = Normal(rng, adjustedMean, stdDev);

            // Floor at 1 yard for completions (can't have 0 or negative completion yards)
            return Math.Max(1, (int)Math.Round(yards));
        }

        /// <summary>
        /// Generates pass yardage with explicit parameters for testing/tuning.
        /// </summary>
        public static int PassYards(ISeedableRandom rng, double mean, double stdDev, double skillModifier)
        {
            var adjustedMean = mean + (skillModifier * 3.0);
            var yards = Normal(rng, adjustedMean, stdDev);
            return Math.Max(1, (int)Math.Round(yards));
        }

        #endregion

        #region Sack Yardage Distribution

        /// <summary>
        /// Generates sack yardage using normal distribution.
        /// Sacks typically result in 5-10 yard losses.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <returns>Sack yardage (always negative)</returns>
        public static int SackYards(ISeedableRandom rng)
        {
            // Mean loss of 7 yards, stddev of 2
            var loss = Normal(rng, 7.0, 2.0);
            // Ensure at least 1 yard loss, cap at reasonable max
            var clampedLoss = Math.Max(1.0, Math.Min(15.0, loss));
            return -(int)Math.Round(clampedLoss);
        }

        #endregion

        #region Tackle For Loss Distribution

        /// <summary>
        /// Generates tackle for loss yardage.
        /// TFLs are typically 1-3 yard losses.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <returns>TFL yardage (always negative)</returns>
        public static int TackleForLossYards(ISeedableRandom rng)
        {
            // Mean loss of 2 yards, stddev of 1
            var loss = Normal(rng, 2.0, 1.0);
            var clampedLoss = Math.Max(1.0, Math.Min(5.0, loss));
            return -(int)Math.Round(clampedLoss);
        }

        #endregion
    }
}
