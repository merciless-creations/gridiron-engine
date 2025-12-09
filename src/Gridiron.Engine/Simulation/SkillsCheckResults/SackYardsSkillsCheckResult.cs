using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates yards lost when a QB is sacked.
    /// Returns a negative value representing the yardage loss.
    /// </summary>
    public class SackYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private int _fieldPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="SackYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining sack yardage.</param>
        /// <param name="fieldPosition">Current field position to prevent losses beyond the goal line.</param>
        public SackYardsSkillsCheckResult(ISeedableRandom rng, int fieldPosition)
        {
            _rng = rng;
            _fieldPosition = fieldPosition;
        }

        /// <summary>
        /// Executes the calculation to determine sack yardage loss.
        /// Sacks typically result in 2-10 yard losses, clamped to field boundaries.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Calculate sack yardage loss (2-10 yards typically)
            var sackYards = -1 * _rng.Next(
                GameProbabilities.Yardage.SACK_MIN_LOSS, 
                GameProbabilities.Yardage.SACK_MAX_LOSS);

            // Don't go past own goal line (can't lose more yards than field position)
            var maxLoss = -1 * _fieldPosition;
            Result = Math.Max(sackYards, maxLoss);
        }
    }
}
