using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Simulation.Calculators
{
    /// <summary>
    /// Stateless utility class for calculating team power ratings.
    /// Used by skills checks to determine matchup advantages.
    /// </summary>
    public static class TeamPowerCalculator
    {
        private const double DEFAULT_POWER = 50.0;

        /// <summary>
        /// Calculates offensive pass blocking power based on O-Line, TEs, RBs, and FBs.
        /// </summary>
        /// <param name="offensivePlayers">List of offensive players on the field</param>
        /// <returns>Average blocking rating of all pass blockers, or default power if no blockers found</returns>
        /// <exception cref="ArgumentNullException">Thrown when offensivePlayers is null</exception>
        public static double CalculatePassBlockingPower(List<Player> offensivePlayers)
        {
            if (offensivePlayers == null)
                throw new ArgumentNullException(nameof(offensivePlayers));

            var blockers = offensivePlayers.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.RB ||
                p.Position == Positions.FB).ToList();

            return blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : DEFAULT_POWER;
        }

        /// <summary>
        /// Calculates defensive pass rush power based on DL and LBs.
        /// </summary>
        /// <param name="defensivePlayers">List of defensive players on the field</param>
        /// <returns>Average rush power based on tackling, speed, and strength attributes, or default power if no rushers found</returns>
        /// <exception cref="ArgumentNullException">Thrown when defensivePlayers is null</exception>
        public static double CalculatePassRushPower(List<Player> defensivePlayers)
        {
            if (defensivePlayers == null)
                throw new ArgumentNullException(nameof(defensivePlayers));

            var rushers = defensivePlayers.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            return rushers.Any()
                ? rushers.Average(r => (r.Tackling + r.Speed + r.Strength) / 3.0)
                : DEFAULT_POWER;
        }

        /// <summary>
        /// Calculates offensive run blocking power based on O-Line, TEs, and FBs.
        /// </summary>
        /// <param name="offensivePlayers">List of offensive players on the field</param>
        /// <returns>Average blocking rating of all run blockers, or default power if no blockers found</returns>
        /// <exception cref="ArgumentNullException">Thrown when offensivePlayers is null</exception>
        public static double CalculateRunBlockingPower(List<Player> offensivePlayers)
        {
            if (offensivePlayers == null)
                throw new ArgumentNullException(nameof(offensivePlayers));

            var blockers = offensivePlayers.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            return blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : DEFAULT_POWER;
        }

        /// <summary>
        /// Calculates defensive run stopping power based on DL and LBs.
        /// </summary>
        /// <param name="defensivePlayers">List of defensive players on the field</param>
        /// <returns>Average run defense power based on tackling, strength, and speed attributes, or default power if no defenders found</returns>
        /// <exception cref="ArgumentNullException">Thrown when defensivePlayers is null</exception>
        public static double CalculateRunDefensePower(List<Player> defensivePlayers)
        {
            if (defensivePlayers == null)
                throw new ArgumentNullException(nameof(defensivePlayers));

            var defenders = defensivePlayers.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            return defenders.Any()
                ? defenders.Average(d => (d.Tackling + d.Strength + d.Speed) / 3.0)
                : DEFAULT_POWER;
        }

        /// <summary>
        /// Calculates defensive coverage power based on DBs and LBs.
        /// </summary>
        /// <param name="defensivePlayers">List of defensive players on the field</param>
        /// <returns>Average coverage power based on coverage, speed, and awareness attributes, or default power if no defenders found</returns>
        /// <exception cref="ArgumentNullException">Thrown when defensivePlayers is null</exception>
        public static double CalculateCoveragePower(List<Player> defensivePlayers)
        {
            if (defensivePlayers == null)
                throw new ArgumentNullException(nameof(defensivePlayers));

            var defenders = defensivePlayers.Where(p =>
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS ||
                p.Position == Positions.LB).ToList();

            return defenders.Any()
                ? defenders.Average(d => (d.Coverage + d.Speed + d.Awareness) / 3.0)
                : DEFAULT_POWER;
        }
    }
}
