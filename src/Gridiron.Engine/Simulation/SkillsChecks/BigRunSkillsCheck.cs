using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Utilities;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if the ball carrier breaks into the secondary for a big run
    /// </summary>
    public class BigRunSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _ballCarrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRunSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="ballCarrier">The ball carrier whose speed affects big run probability.</param>
        public BigRunSkillsCheck(ISeedableRandom rng, Player ballCarrier)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
        }

        /// <summary>
        /// Executes the big run check to determine if the ball carrier breaks into the secondary.
        /// Probability increases with the ball carrier's speed above the threshold.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Base chance for a big run, increased by speed above threshold
            // Uses logarithmic curve for diminishing returns at skill extremes
            var speedModifier = AttributeModifier.Calculate(
                _ballCarrier.Speed,
                GameProbabilities.Rushing.BIG_RUN_SPEED_THRESHOLD);
            var bigRunProbability = GameProbabilities.Rushing.BIG_RUN_BASE_PROBABILITY + speedModifier;

            // Clamp to reasonable bounds
            bigRunProbability = Math.Max(
                GameProbabilities.Rushing.BIG_RUN_MIN_CLAMP,
                Math.Min(GameProbabilities.Rushing.BIG_RUN_MAX_CLAMP, bigRunProbability));

            Occurred = _rng.NextDouble() < bigRunProbability;
        }
    }
}
