using System;

namespace Gridiron.Engine.Simulation.Utilities
{
    /// <summary>
    /// Provides logarithmic attribute modifier calculations for game balance.
    ///
    /// The logarithmic curve produces diminishing returns at skill extremes:
    /// - Rating 30: -10.4% modifier
    /// - Rating 50: 0% modifier (baseline)
    /// - Rating 70: +10.4% modifier
    /// - Rating 90: +15.9% modifier
    /// - Rating 99: +17.6% modifier
    ///
    /// This prevents elite players from being unstoppable while still making
    /// skill differences meaningful. The jump from 90->99 (+1.7%) matters less
    /// than 50->60 (+6.1%), enabling "any given Sunday" upset potential.
    /// </summary>
    public static class AttributeModifier
    {
        /// <summary>
        /// Base multiplier controlling the magnitude of modifiers.
        /// 0.15 produces approximately ±17% maximum modifier for extreme ratings.
        /// </summary>
        private const double BASE_MULTIPLIER = 0.15;

        /// <summary>
        /// Scaling factor controlling how quickly diminishing returns kick in.
        /// 10.0 means every 10 points of differential approaches the next log step.
        /// </summary>
        private const double SCALE_FACTOR = 10.0;

        /// <summary>
        /// Calculates a logarithmic modifier from a player rating.
        /// </summary>
        /// <param name="rating">The player's attribute rating (typically 0-99).</param>
        /// <param name="baseline">The baseline rating representing no modifier (default 50).</param>
        /// <returns>
        /// A modifier value, typically in the range -0.18 to +0.18.
        /// Positive values indicate above-baseline performance.
        /// </returns>
        /// <example>
        /// // Elite QB vs average coverage
        /// var modifier = AttributeModifier.Calculate(90); // Returns ~0.159 (+15.9%)
        /// completionProbability += modifier;
        /// </example>
        public static double Calculate(double rating, double baseline = 50.0)
        {
            var diff = rating - baseline;
            return FromDifferential(diff);
        }

        /// <summary>
        /// Calculates a logarithmic modifier from a pre-computed skill differential.
        /// Use this when you've already calculated the difference between offensive
        /// and defensive powers.
        /// </summary>
        /// <param name="skillDifferential">
        /// The difference between offensive and defensive skill powers.
        /// Positive values favor offense, negative values favor defense.
        /// </param>
        /// <returns>
        /// A modifier value with diminishing returns at extremes.
        /// For example:
        /// - Differential +10: +0.104 modifier
        /// - Differential +20: +0.165 modifier
        /// - Differential +40: +0.241 modifier
        /// </returns>
        /// <example>
        /// // Calculate completion modifier from skill matchup
        /// var offensivePower = (qb.Passing + receiver.Catching) / 2.0;
        /// var defensivePower = coverage.Average(d => d.Coverage);
        /// var skillDiff = offensivePower - defensivePower;
        /// var modifier = AttributeModifier.FromDifferential(skillDiff);
        /// completionProbability += modifier;
        /// </example>
        public static double FromDifferential(double skillDifferential)
        {
            if (Math.Abs(skillDifferential) < 0.001)
            {
                return 0.0;
            }

            return Math.Sign(skillDifferential)
                * Math.Log(1 + Math.Abs(skillDifferential) / SCALE_FACTOR)
                * BASE_MULTIPLIER;
        }
    }
}
