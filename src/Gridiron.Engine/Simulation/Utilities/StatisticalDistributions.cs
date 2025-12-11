using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;
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
            // Use centralized configuration from GameProbabilities
            var mu = GameProbabilities.YardageDistributions.RUN_MU;
            var sigma = GameProbabilities.YardageDistributions.RUN_SIGMA;
            var shift = GameProbabilities.YardageDistributions.RUN_SHIFT;
            var skillMultiplier = GameProbabilities.YardageDistributions.RUN_SKILL_MULTIPLIER;

            // Generate base yards from log-normal (always positive)
            var baseYards = LogNormal(rng, mu, sigma);

            // Shift down to allow negatives (TFL)
            var shiftedYards = baseYards - shift;

            // Apply skill modifier (better skills = more yards)
            var finalYards = shiftedYards + (skillModifier * skillMultiplier);

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
        /// Generates pass air yards using normal distribution based on pass type.
        /// Uses centralized configuration from GameProbabilities.YardageDistributions.
        ///
        /// NFL averages by pass type:
        /// - Screen: ~4 yards (high completion %, low variance)
        /// - Short: ~7 yards (slants, curls, out routes)
        /// - Forward/Medium: ~14 yards (crossing routes, intermediate)
        /// - Deep: ~30 yards (go routes, posts) - lower completion %
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="passType">Type of pass attempt (domain PassType)</param>
        /// <param name="skillModifier">Skill differential modifier (-1.0 to +1.0 typical)</param>
        /// <returns>Pass air yards (minimum 1 for screen, varies by type)</returns>
        public static int PassYards(ISeedableRandom rng, PassType passType, double skillModifier = 0.0)
        {
            double mean, stdDev;
            int minYards;

            switch (passType)
            {
                case PassType.Screen:
                    mean = GameProbabilities.YardageDistributions.PASS_SCREEN_MEAN;
                    stdDev = GameProbabilities.YardageDistributions.PASS_SCREEN_STDDEV;
                    minYards = -3; // Screens can be behind LOS
                    break;
                case PassType.Short:
                    mean = GameProbabilities.YardageDistributions.PASS_SHORT_MEAN;
                    stdDev = GameProbabilities.YardageDistributions.PASS_SHORT_STDDEV;
                    minYards = 1;
                    break;
                case PassType.Forward: // "Forward" in domain = "Medium" in NFL terminology
                    mean = GameProbabilities.YardageDistributions.PASS_MEDIUM_MEAN;
                    stdDev = GameProbabilities.YardageDistributions.PASS_MEDIUM_STDDEV;
                    minYards = 5;
                    break;
                case PassType.Deep:
                    mean = GameProbabilities.YardageDistributions.PASS_DEEP_MEAN;
                    stdDev = GameProbabilities.YardageDistributions.PASS_DEEP_STDDEV;
                    minYards = 15;
                    break;
                default:
                    // Lateral, Backward, or unknown - use short pass defaults
                    mean = GameProbabilities.YardageDistributions.PASS_SHORT_MEAN;
                    stdDev = GameProbabilities.YardageDistributions.PASS_SHORT_STDDEV;
                    minYards = 1;
                    break;
            }

            // Apply skill modifier to mean
            var adjustedMean = mean + (skillModifier * GameProbabilities.YardageDistributions.PASS_SKILL_MULTIPLIER);

            var yards = Normal(rng, adjustedMean, stdDev);

            // Floor at minimum yards for pass type (screens can be negative)
            return Math.Max(minYards, (int)Math.Round(yards));
        }

        /// <summary>
        /// Generates pass yardage with explicit parameters for testing/tuning.
        /// </summary>
        public static int PassYards(ISeedableRandom rng, double mean, double stdDev, double skillModifier)
        {
            var skillMultiplier = GameProbabilities.YardageDistributions.PASS_SKILL_MULTIPLIER;
            var adjustedMean = mean + (skillModifier * skillMultiplier);
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
