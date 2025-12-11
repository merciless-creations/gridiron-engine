using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Utilities;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates air yards for a pass based on pass type and field position.
    /// Uses normal distribution for realistic NFL pass patterns.
    /// Air yards represent the distance the ball travels in the air before being caught.
    /// </summary>
    public class AirYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly PassType _passType;
        private readonly int _fieldPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining yardage variance.</param>
        /// <param name="passType">The type of pass being thrown (Screen, Short, Forward, Deep).</param>
        /// <param name="fieldPosition">Current field position to determine maximum possible air yards.</param>
        public AirYardsSkillsCheckResult(ISeedableRandom rng, PassType passType, int fieldPosition)
        {
            _rng = rng;
            _passType = passType;
            _fieldPosition = fieldPosition;
        }

        /// <summary>
        /// Executes the calculation to determine air yards based on pass type and field position.
        /// Uses normal distribution for each pass type (screen, short, forward/medium, deep).
        /// Air yards are clamped to ensure the ball cannot be thrown past the end zone.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            var yardsToGoal = 100 - _fieldPosition;

            // Use normal distribution for realistic pass yardage
            // skillModifier = 0 for now (could be enhanced to consider QB/WR skills)
            var airYards = StatisticalDistributions.PassYards(_rng, _passType, skillModifier: 0.0);

            // Clamp result to available field (can't throw past end zone)
            Result = Math.Min(airYards, yardsToGoal);
        }
    }
}
