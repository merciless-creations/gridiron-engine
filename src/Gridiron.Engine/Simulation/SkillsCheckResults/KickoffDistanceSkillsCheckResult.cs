using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates kickoff distance based on kicker skill.
    /// Base distance ranges from 40-70 yards with variance, clamped to 30-80 yards.
    /// </summary>
    public class KickoffDistanceSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _kicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="KickoffDistanceSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining distance variance.</param>
        /// <param name="kicker">The kicker performing the kickoff.</param>
        public KickoffDistanceSkillsCheckResult(ISeedableRandom rng, Player kicker)
        {
            _rng = rng;
            _kicker = kicker;
        }

        /// <summary>
        /// Executes the calculation to determine kickoff distance.
        /// Kicker's skill (0-100) affects base distance, with random variance applied.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Base kickoff distance: 50-70 yards for average kicker
            // Kicker skill range: 0-100 (typically 40-90 for specialists)

            var kickerSkill = _kicker.Kicking;

            // Base distance centered around 60 yards
            var baseDistance = 40.0 + (kickerSkill / 100.0) * 30.0;  // 40-70 yards range

            // Random variance ±10 yards
            var randomFactor = (_rng.NextDouble() * 20.0) - 10.0;

            var totalDistance = baseDistance + randomFactor;

            // Clamp to realistic range (30-80 yards)
            Result = Math.Max(30.0, Math.Min(80.0, totalDistance));
        }
    }
}
