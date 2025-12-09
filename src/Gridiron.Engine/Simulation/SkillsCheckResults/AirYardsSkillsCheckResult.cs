using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates air yards for a pass based on pass type and field position.
    /// Air yards represent the distance the ball travels in the air before being caught.
    /// </summary>
    public class AirYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private PassType _passType;
        private int _fieldPosition;

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
        /// Air yards are clamped to ensure the ball cannot be thrown past the end zone.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            var yardsToGoal = 100 - _fieldPosition;

            var airYards = _passType switch
            {
                PassType.Screen => _rng.Next(
                    GameProbabilities.Yardage.SCREEN_MIN_YARDS, 
                    GameProbabilities.Yardage.SCREEN_MAX_YARDS),
                PassType.Short => _rng.Next(
                    GameProbabilities.Yardage.SHORT_MIN_YARDS, 
                    Math.Max(4, Math.Min(GameProbabilities.Yardage.SHORT_MAX_YARDS, yardsToGoal))),
                PassType.Forward => _rng.Next(
                    GameProbabilities.Yardage.FORWARD_MIN_YARDS, 
                    Math.Max(9, Math.Min(GameProbabilities.Yardage.FORWARD_MAX_YARDS, yardsToGoal))),
                PassType.Deep => _rng.Next(
                    GameProbabilities.Yardage.DEEP_MIN_YARDS, 
                    Math.Max(19, Math.Min(GameProbabilities.Yardage.DEEP_MAX_YARDS, yardsToGoal))),
                _ => _rng.Next(
                    GameProbabilities.Yardage.DEFAULT_PASS_MIN_YARDS, 
                    Math.Max(6, Math.Min(GameProbabilities.Yardage.DEFAULT_PASS_MAX_YARDS, yardsToGoal)))
            };

            // Clamp result to available field (can't throw past end zone)
            Result = Math.Min(airYards, yardsToGoal);
        }
    }
}
