using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.SkillsChecks;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates yards after catch based on receiver's ability to break tackles and elude defenders.
    /// Integrates with YardsAfterCatchSkillsCheck to determine if receiver has opportunity for YAC.
    /// </summary>
    public class YardsAfterCatchSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private Player _receiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="YardsAfterCatchSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining YAC variance.</param>
        /// <param name="receiver">The receiver who caught the pass.</param>
        public YardsAfterCatchSkillsCheckResult(ISeedableRandom rng, Player receiver)
        {
            _rng = rng;
            _receiver = receiver;
        }

        /// <summary>
        /// Executes the calculation to determine yards after catch.
        /// First checks if the receiver has a YAC opportunity, then calculates yardage
        /// based on the receiver's speed, agility, and rushing ability.
        /// May result in big play bonus for fast receivers.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Check for YAC opportunity
            var yacCheck = new YardsAfterCatchSkillsCheck(_rng, _receiver);
            yacCheck.Execute(game);

            if (!yacCheck.Occurred)
            {
                // Tackled immediately (0-2 yards)
                Result = _rng.Next(0, GameProbabilities.Yardage.YAC_IMMEDIATE_TACKLE_MAX);
                return;
            }

            // Good YAC opportunity - receiver breaks tackles
            var yacPotential = (_receiver.Speed + _receiver.Agility + _receiver.Rushing) / 3.0;
            var baseYAC = GameProbabilities.Yardage.YAC_BASE_YARDS + 
                         (yacPotential / GameProbabilities.Yardage.YAC_SKILL_DENOMINATOR);

            // Add randomness (-2 to +6 yards)
            var randomFactor = (_rng.NextDouble() * GameProbabilities.Yardage.YAC_RANDOM_RANGE) + 
                               GameProbabilities.Yardage.YAC_RANDOM_OFFSET;
            var totalYAC = Math.Max(0, (int)Math.Round(baseYAC + randomFactor));

            // Chance for big play after catch if receiver is fast
            if (_rng.NextDouble() < GameProbabilities.Passing.BIG_PLAY_YAC_PROBABILITY
                && _receiver.Speed > GameProbabilities.Passing.BIG_PLAY_YAC_SPEED_THRESHOLD)
            {
                totalYAC += _rng.Next(
                    GameProbabilities.Passing.BIG_PLAY_YAC_MIN_BONUS,
                    GameProbabilities.Passing.BIG_PLAY_YAC_MAX_BONUS);
                game.CurrentPlay.Result.LogInformation($"{_receiver.LastName} breaks free! Great run after catch!");
            }

            Result = totalYAC;
        }
    }
}
