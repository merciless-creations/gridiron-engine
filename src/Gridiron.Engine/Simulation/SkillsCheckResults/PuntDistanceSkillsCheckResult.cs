using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates the distance a punt travels based on punter's kicking skill
    /// and field position constraints.
    /// </summary>
    public class PuntDistanceSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _punter;
        private readonly int _fieldPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="PuntDistanceSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining distance variance.</param>
        /// <param name="punter">The punter kicking the ball.</param>
        /// <param name="fieldPosition">Current field position to determine maximum punt distance.</param>
        public PuntDistanceSkillsCheckResult(
            ISeedableRandom rng,
            Player punter,
            int fieldPosition)
        {
            _rng = rng;
            _punter = punter;
            _fieldPosition = fieldPosition;
        }

        /// <summary>
        /// Executes the calculation to determine punt distance.
        /// Punter's kicking skill affects base distance (30-55 yards), with variance and
        /// field boundary constraints applied. Minimum punt is 10 yards (shanked punt).
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Base punt distance: 35-50 yards for average punter (kicking skill 50)
            // Better punters (70+ kicking): 45-60 yards
            // Weaker punters (30 kicking): 25-40 yards

            var skillFactor = _punter.Kicking / 100.0;
            var baseDistance = 30.0 + (skillFactor * 25.0); // 30-55 yard base

            // Add randomness (-10 to +15 yard variance for realistic variance)
            var randomFactor = (_rng.NextDouble() * 25.0) - 10.0;
            var totalDistance = baseDistance + randomFactor;

            // Ensure minimum punt distance (shanked punt: 10 yards)
            totalDistance = Math.Max(10.0, totalDistance);

            // Clamp to field boundaries
            // Can't punt beyond opponent's end zone (110 yards - field position gives max distance to back of end zone)
            var maxDistance = 110 - _fieldPosition;
            totalDistance = Math.Min(totalDistance, maxDistance);

            Result = (int)Math.Round(totalDistance);
        }
    }
}
