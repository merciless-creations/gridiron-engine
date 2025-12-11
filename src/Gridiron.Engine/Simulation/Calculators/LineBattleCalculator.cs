using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Simulation.Calculators
{
    /// <summary>
    /// Stateless utility class for calculating the defensive pressure factor.
    /// This foundational calculation affects all downstream play mechanics.
    /// </summary>
    public static class LineBattleCalculator
    {
        private const double BASE_PRESSURE = 1.0;
        private const double MIN_PRESSURE = 0.0;
        private const double MAX_PRESSURE = 2.5;
        private const int STANDARD_RUSH_COUNT = 4;

        /// <summary>
        /// Calculates the defensive pressure factor for pass plays.
        /// Returns a multiplier where:
        /// - 0.0-0.5: Soft rush (prevent defense, 3-man rush) - Offense advantage
        /// - 1.0: Standard pressure (4-man rush, even matchup)
        /// - 1.5-2.0+: Heavy blitz (5+ rushers) - Defense advantage
        /// </summary>
        /// <param name="offensivePlayers">All offensive players on field</param>
        /// <param name="defensivePlayers">All defensive players on field</param>
        /// <param name="isPassPlay">True for pass protection, false for run blocking</param>
        /// <returns>Defensive pressure factor (0.0 to 2.5+)</returns>
        /// <exception cref="ArgumentNullException">Thrown when offensivePlayers or defensivePlayers is null</exception>
        public static double CalculateDPressureFactor(
            List<Player> offensivePlayers,
            List<Player> defensivePlayers,
            bool isPassPlay = true)
        {
            if (offensivePlayers == null)
                throw new ArgumentNullException(nameof(offensivePlayers));
            if (defensivePlayers == null)
                throw new ArgumentNullException(nameof(defensivePlayers));

            // Calculate base power ratings
            double offensivePower = isPassPlay
                ? TeamPowerCalculator.CalculatePassBlockingPower(offensivePlayers)
                : TeamPowerCalculator.CalculateRunBlockingPower(offensivePlayers);

            double defensivePower = isPassPlay
                ? TeamPowerCalculator.CalculatePassRushPower(defensivePlayers)
                : TeamPowerCalculator.CalculateRunDefensePower(defensivePlayers);

            // Count how many defenders are rushing/involved
            var rushers = CountRushers(defensivePlayers, isPassPlay);

            // Calculate skill differential impact
            // Uses logarithmic curve for diminishing returns at skill extremes
            var skillDifferential = defensivePower - offensivePower;
            var skillImpact = AttributeModifier.FromDifferential(skillDifferential);

            // Calculate rusher count impact
            var rusherDifferential = rushers - STANDARD_RUSH_COUNT;
            var rusherImpact = rusherDifferential * 0.15; // Each extra rusher adds ~15% pressure

            // Combine factors
            var pressureFactor = BASE_PRESSURE + skillImpact + rusherImpact;

            // Clamp to reasonable bounds
            return System.Math.Max(MIN_PRESSURE, System.Math.Min(MAX_PRESSURE, pressureFactor));
        }

        /// <summary>
        /// Counts how many defensive players are involved in the rush/line battle.
        /// </summary>
        /// <param name="defensivePlayers">All defensive players on the field</param>
        /// <param name="isPassPlay">True for pass rush count, false for run defense count</param>
        /// <returns>Number of defenders involved in the line battle</returns>
        private static int CountRushers(List<Player> defensivePlayers, bool isPassPlay)
        {
            if (isPassPlay)
            {
                // Pass rush: DL + blitzing LBs
                return defensivePlayers.Count(p =>
                    p.Position == Positions.DT ||
                    p.Position == Positions.DE ||
                    p.Position == Positions.LB ||
                    p.Position == Positions.OLB);
            }
            else
            {
                // Run defense: DL + LBs (all involved in run stop)
                return defensivePlayers.Count(p =>
                    p.Position == Positions.DT ||
                    p.Position == Positions.DE ||
                    p.Position == Positions.LB ||
                    p.Position == Positions.OLB);
            }
        }
    }
}
