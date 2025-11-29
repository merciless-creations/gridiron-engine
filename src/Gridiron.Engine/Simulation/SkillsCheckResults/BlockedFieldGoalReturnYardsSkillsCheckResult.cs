using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates return yardage for blocked field goal recoveries.
    /// Returns can range from -5 yards (tackled behind recovery spot) to 100 yards (full-field touchdown).
    /// </summary>
    public class BlockedFieldGoalReturnYardsSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockedFieldGoalReturnYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining return variance.</param>
        /// <param name="returner">The player recovering and returning the blocked field goal.</param>
        public BlockedFieldGoalReturnYardsSkillsCheckResult(ISeedableRandom rng, Player returner)
        {
            _rng = rng;
            _returner = returner;
        }

        /// <summary>
        /// Executes the calculation to determine blocked field goal return yardage.
        /// Returner's speed and agility affect the base return, with significant random variance.
        /// Most returns fall in the 10-30 yard range, but touchdowns are possible.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Average blocked FG return: 15-25 yards
            // Elite returns can go the distance (rare but possible)

            var returnerSkill = (_returner.Speed + _returner.Agility) / 2.0;

            // Base return: 10-25 yards based on returner skill
            var baseReturn = 5.0 + (returnerSkill / 100.0) * 20.0;

            // Random factor: ±50 yards
            // This allows for:
            // - Negative returns (tackled behind recovery spot)
            // - Big returns (40-60 yards)
            // - Full-field TD returns (rare but possible)
            var randomFactor = (_rng.NextDouble() * 100.0) - 50.0;

            var totalReturn = baseReturn + randomFactor;

            // Clamp to realistic range (-5 to 100 yards)
            // Negative = tackled behind recovery spot
            // 100 yards = full-field TD return (very rare)
            // Most returns will be in the 10-30 yard range
            Result = Math.Max(-5.0, Math.Min(100.0, totalReturn));
        }
    }
}
