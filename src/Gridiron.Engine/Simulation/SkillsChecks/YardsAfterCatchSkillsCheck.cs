using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if receiver breaks tackles for extra yards after catch
    /// </summary>
    public class YardsAfterCatchSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _receiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="YardsAfterCatchSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="receiver">The receiver who caught the ball.</param>
        public YardsAfterCatchSkillsCheck(ISeedableRandom rng, Player receiver)
        {
            _rng = rng;
            _receiver = receiver;
        }

        /// <summary>
        /// Executes the yards after catch check to determine if the receiver gains extra yards after the catch.
        /// Probability is based on receiver's speed, agility, and rushing ability.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Calculate receiver's YAC potential based on speed, agility, and elusiveness
            var yacPotential = (_receiver.Speed + _receiver.Agility + _receiver.Rushing) / 3.0;

            // Base chance for good YAC opportunity, increased by receiver skills above threshold
            var yacBonus = (yacPotential - GameProbabilities.Passing.YAC_SKILL_THRESHOLD)
                / GameProbabilities.Passing.YAC_SKILL_DENOMINATOR;
            var yacProbability = GameProbabilities.Passing.YAC_OPPORTUNITY_BASE_PROBABILITY + yacBonus;

            // Clamp to reasonable bounds
            yacProbability = Math.Max(
                GameProbabilities.Passing.YAC_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.YAC_MAX_CLAMP, yacProbability));

            Occurred = _rng.NextDouble() < yacProbability;
        }
    }
}
