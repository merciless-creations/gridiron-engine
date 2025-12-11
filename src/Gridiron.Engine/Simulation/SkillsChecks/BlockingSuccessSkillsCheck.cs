using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Utilities;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if the offensive line successfully creates a running lane
    /// </summary>
    public class BlockingSuccessSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingSuccessSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        public BlockingSuccessSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the blocking success check to determine if the offensive line creates a running lane.
        /// Compares offensive blocking power against defensive line power to calculate success probability.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate offensive blocking power
            var blockers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            var offensiveBlockingPower = blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : 50;

            // Calculate defensive line power
            var defenders = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB).ToList();

            var defensivePower = defenders.Any()
                ? defenders.Average(d => (d.Tackling + d.Strength) / 2.0)
                : 50;

            // Calculate success probability (base rate adjusted by skill differential)
            // Uses logarithmic curve for diminishing returns at skill extremes
            var skillDifferential = offensiveBlockingPower - defensivePower;
            var successProbability = GameProbabilities.Rushing.BLOCKING_SUCCESS_BASE_PROBABILITY
                + AttributeModifier.FromDifferential(skillDifferential);

            // Clamp to reasonable bounds
            successProbability = Math.Max(
                GameProbabilities.Rushing.BLOCKING_SUCCESS_MIN_CLAMP,
                Math.Min(GameProbabilities.Rushing.BLOCKING_SUCCESS_MAX_CLAMP, successProbability));

            Occurred = _rng.NextDouble() < successProbability;
        }
    }
}
