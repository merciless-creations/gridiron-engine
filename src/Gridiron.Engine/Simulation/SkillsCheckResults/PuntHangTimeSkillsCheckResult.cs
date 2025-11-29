using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates hang time (time ball is in air) based on punt distance.
    /// Longer punts generally have more hang time.
    /// Returns hang time as a double (in seconds).
    /// </summary>
    public class PuntHangTimeSkillsCheckResult : SkillsCheckResult<double>
    {
        private readonly ISeedableRandom _rng;
        private readonly int _puntDistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PuntHangTimeSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining time variance.</param>
        /// <param name="puntDistance">The distance the punt traveled in yards.</param>
        public PuntHangTimeSkillsCheckResult(
            ISeedableRandom rng,
            int puntDistance)
        {
            _rng = rng;
            _puntDistance = puntDistance;
        }

        /// <summary>
        /// Executes the calculation to determine punt hang time.
        /// Formula is approximately 0.08-0.10 seconds per yard with variance.
        /// Minimum hang time is 2.0 seconds.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Hang time formula: roughly 0.08-0.10 seconds per yard
            // 40-yard punt: ~3.2-4.0 seconds
            // 50-yard punt: ~4.0-5.0 seconds

            var baseHangTime = _puntDistance * 0.08;

            // Add randomness (±0.5 seconds)
            var randomFactor = (_rng.NextDouble() - 0.5);
            var totalHangTime = baseHangTime + randomFactor;

            // Ensure minimum hang time
            totalHangTime = Math.Max(2.0, totalHangTime);

            Result = Math.Round(totalHangTime, 1);
        }
    }
}
