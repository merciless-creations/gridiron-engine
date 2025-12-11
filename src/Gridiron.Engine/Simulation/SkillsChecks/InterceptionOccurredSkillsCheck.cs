using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Utilities;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if an incomplete pass is intercepted by the defense
    /// Only called when a pass is incomplete
    /// </summary>
    public class InterceptionOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _qb;
        private Player _receiver;
        private bool _underPressure;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="qb">The quarterback throwing the pass.</param>
        /// <param name="receiver">The intended receiver.</param>
        /// <param name="underPressure">Whether the quarterback is under pressure.</param>
        public InterceptionOccurredSkillsCheck(ISeedableRandom rng, Player qb, Player receiver, bool underPressure)
        {
            _rng = rng;
            _qb = qb;
            _receiver = receiver;
            _underPressure = underPressure;
        }

        /// <summary>
        /// Executes the interception check for incomplete passes to determine if the defense intercepts.
        /// Probability increases with better coverage, worse QB skill, and pressure on the QB.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate QB passing skill (lower is worse, increases INT chance)
            var qbPassing = _qb.Passing;
            var qbAwareness = _qb.Awareness;
            var qbSkill = (qbPassing * 2 + qbAwareness) / 3.0;

            // Calculate coverage effectiveness
            var defenders = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.CB ||
                p.Position == Positions.S ||
                p.Position == Positions.FS ||
                p.Position == Positions.LB).ToList();

            var coverageSkill = defenders.Any()
                ? defenders.Average(d => (d.Coverage * 2 + d.Awareness + d.Agility) / 4.0)
                : 50;

            // Base interception probability on incomplete passes
            var interceptionProbability = GameProbabilities.Passing.INTERCEPTION_BASE_PROBABILITY;

            // Adjust based on skill differential
            // Uses logarithmic curve for diminishing returns at skill extremes
            var skillDiff = coverageSkill - qbSkill;
            interceptionProbability += AttributeModifier.FromDifferential(skillDiff) * GameProbabilities.Passing.INTERCEPTION_SKILL_MODIFIER_SCALE;

            // Pressure increases interception chance (bad throws)
            if (_underPressure)
            {
                interceptionProbability += GameProbabilities.Passing.INTERCEPTION_PRESSURE_BONUS;
            }

            // Clamp to reasonable bounds
            interceptionProbability = Math.Max(
                GameProbabilities.Passing.INTERCEPTION_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.INTERCEPTION_MAX_CLAMP, interceptionProbability));

            Occurred = _rng.NextDouble() < interceptionProbability;
        }
    }
}
