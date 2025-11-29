using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Checks if returner muffs (drops) the punt.
    /// Muffed punts can be recovered by either team.
    /// </summary>
    public class MuffedCatchOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _returner;
        private readonly double _hangTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuffedCatchOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="returner">The player attempting to catch the punt.</param>
        /// <param name="hangTime">The hang time of the punt in seconds.</param>
        public MuffedCatchOccurredSkillsCheck(
            ISeedableRandom rng,
            Player returner,
            double hangTime)
        {
            _rng = rng;
            _returner = returner;
            _hangTime = hangTime;
        }

        /// <summary>
        /// Executes the muffed catch check to determine if the returner drops the punt.
        /// Probability decreases with higher catching skill and increases with longer hang time (more pressure).
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Muff probability based on:
            // - Returner's catching skill (lower = more muffs)
            // - Hang time (longer = more pressure)
            // - Random chance

            var catchingFactor = _returner.Catching / GameProbabilities.Punts.PUNT_MUFF_SKILL_DENOMINATOR;
            var baseMuffChance = GameProbabilities.Punts.PUNT_MUFF_BASE
                - (catchingFactor * GameProbabilities.Punts.PUNT_MUFF_SKILL_FACTOR);

            // Hang time pressure (longer hang time = defenders closer = more pressure)
            if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_THRESHOLD)
                baseMuffChance += GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_TIME_BONUS;
            else if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_THRESHOLD)
                baseMuffChance += GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_TIME_BONUS;

            Occurred = _rng.NextDouble() < baseMuffChance;
        }
    }
}
