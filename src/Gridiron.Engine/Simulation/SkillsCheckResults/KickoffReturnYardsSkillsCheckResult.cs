using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates kickoff return yardage based on returner skill.
    /// Average return is 20-25 yards, with potential for long returns and touchdowns.
    /// </summary>
    public class KickoffReturnYardsSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;

        /// <summary>
        /// Initializes a new instance of the <see cref="KickoffReturnYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining return variance.</param>
        /// <param name="returner">The player returning the kickoff.</param>
        public KickoffReturnYardsSkillsCheckResult(ISeedableRandom rng, Player returner)
        {
            _rng = rng;
            _returner = returner;
        }

        /// <summary>
        /// Executes the calculation to determine kickoff return yardage.
        /// Returner's speed and agility affect base return (15-30 yards), with significant
        /// random variance allowing for tackles at spot (-5 yards) or touchdown returns (85 yards).
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Average kickoff return: 20-25 yards in NFL
            // Returner skill factors: Speed and Agility

            var returnerSkill = (_returner.Speed + _returner.Agility) / 2.0;

            // Base return: 15-30 yards
            var baseReturn = 10.0 + (returnerSkill / 100.0) * 20.0;

            // Random factor: ±60 yards (allows for both tackles at spot and breakaway TDs)
            var randomFactor = (_rng.NextDouble() * 120.0) - 60.0;

            var totalReturn = baseReturn + randomFactor;

            // Clamp to realistic range (-5 to 85 yards)
            // Negative returns represent tackles behind catch point
            // Upper range allows for long return TDs
            Result = Math.Max(-5.0, Math.Min(85.0, totalReturn));
        }
    }
}
