using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if the QB is under pressure (affects pass accuracy)
    /// </summary>
    public class QBPressureSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="QBPressureSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        public QBPressureSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the QB pressure check to determine if the quarterback is under pressure.
        /// Pressure affects pass accuracy even if the QB is not sacked.
        /// Compares defensive pass rush against offensive line blocking.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate pass rush effectiveness (even if not sacked, QB can be pressured)
            var rushers = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            var passRushPower = rushers.Any()
                ? rushers.Average(r => (r.Speed + r.Strength) / 2.0)
                : 50;

            var blockers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T).ToList();

            var protectionPower = blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : 50;

            // Calculate pressure probability (base rate adjusted by rush vs protection)
            var skillDifferential = passRushPower - protectionPower;
            var pressureProbability = GameProbabilities.Passing.QB_PRESSURE_BASE_PROBABILITY
                + (skillDifferential / GameProbabilities.Passing.QB_PRESSURE_SKILL_DENOMINATOR);

            // Clamp to reasonable bounds
            pressureProbability = Math.Max(
                GameProbabilities.Passing.QB_PRESSURE_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.QB_PRESSURE_MAX_CLAMP, pressureProbability));

            Occurred = _rng.NextDouble() < pressureProbability;
        }
    }
}
