using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if the ball carrier breaks a tackle attempt
    /// </summary>
    public class TackleBreakSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _ballCarrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="TackleBreakSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="ballCarrier">The ball carrier attempting to break the tackle.</param>
        public TackleBreakSkillsCheck(ISeedableRandom rng, Player ballCarrier)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
        }

        /// <summary>
        /// Executes the tackle break check to determine if the ball carrier breaks a tackle attempt.
        /// Combines ball carrier's rushing, strength, and agility against tackler's skills.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate ball carrier's evasion power
            var ballCarrierPower = (_ballCarrier.Rushing + _ballCarrier.Strength + _ballCarrier.Agility) / 3.0;

            // Calculate tackler power (get primary tackler - closest defender)
            var tacklers = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.LB ||
                p.Position == Positions.DE ||
                p.Position == Positions.DT ||
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS).ToList();

            var tacklerPower = tacklers.Any()
                ? tacklers.Average(t => (t.Tackling + t.Strength + t.Speed) / 3.0)
                : 50;

            // Calculate break tackle probability (base rate for elite backs)
            var skillDifferential = ballCarrierPower - tacklerPower;
            var breakProbability = GameProbabilities.Rushing.TACKLE_BREAK_BASE_PROBABILITY
                + (skillDifferential / GameProbabilities.Rushing.TACKLE_BREAK_SKILL_DENOMINATOR);

            // Clamp to reasonable bounds
            breakProbability = Math.Max(
                GameProbabilities.Rushing.TACKLE_BREAK_MIN_CLAMP,
                Math.Min(GameProbabilities.Rushing.TACKLE_BREAK_MAX_CLAMP, breakProbability));

            Occurred = _rng.NextDouble() < breakProbability;
        }
    }
}
