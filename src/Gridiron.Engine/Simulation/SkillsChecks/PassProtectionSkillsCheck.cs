using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if the offensive line successfully protects the QB from being sacked
    /// </summary>
    public class PassProtectionSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassProtectionSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        public PassProtectionSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the pass protection check to determine if the offensive line protects the QB from being sacked.
        /// Compares offensive line blocking against defensive pass rush to calculate protection success.
        /// Sets the Margin property to indicate how decisively protection held or failed.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            var play = game.CurrentPlay;

            // Calculate offensive line pass blocking power
            var blockers = play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.RB ||  // RB can help in pass protection
                p.Position == Positions.FB).ToList();

            var offensiveProtection = blockers.Any()
                ? blockers.Average(b => b.Blocking)
                : 50;

            // Calculate pass rush power
            var rushers = play.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            var passRushPower = rushers.Any()
                ? rushers.Average(r => (r.Tackling + r.Speed + r.Strength) / 3.0)
                : 50;

            // Calculate protection success probability (base rate adjusted by skill differential)
            var skillDifferential = offensiveProtection - passRushPower;
            var protectionProbability = GameProbabilities.Passing.PASS_PROTECTION_BASE_PROBABILITY
                + (skillDifferential / GameProbabilities.Passing.PASS_PROTECTION_SKILL_DENOMINATOR);

            // Clamp to reasonable bounds (sacks are relatively rare)
            protectionProbability = Math.Max(
                GameProbabilities.Passing.PASS_PROTECTION_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.PASS_PROTECTION_MAX_CLAMP, protectionProbability));

            // Roll for success
            var roll = _rng.NextDouble();
            Occurred = roll < protectionProbability;

            // Calculate margin for narrative purposes
            // Positive margin = protection held decisively
            // Negative margin = sack occurred decisively
            // Range: approximately -100 to +100
            Margin = (protectionProbability - roll) * 100.0;
        }
    }
}
