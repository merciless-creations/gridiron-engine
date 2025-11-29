using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Checks if returner signals and makes a fair catch.
    /// More likely with good hang time (better coverage) or deep in own territory.
    /// </summary>
    public class FairCatchOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly double _hangTime;
        private readonly int _returnSpot;

        /// <summary>
        /// Initializes a new instance of the <see cref="FairCatchOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="hangTime">The hang time of the punt in seconds.</param>
        /// <param name="returnSpot">The yard line where the punt is caught.</param>
        public FairCatchOccurredSkillsCheck(
            ISeedableRandom rng,
            double hangTime,
            int returnSpot)
        {
            _rng = rng;
            _hangTime = hangTime;
            _returnSpot = returnSpot;
        }

        /// <summary>
        /// Executes the fair catch check to determine if the returner signals for a fair catch.
        /// More likely with long hang time (better coverage) or deep in own territory.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Fair catch more likely with:
            // - Long hang time (good coverage breathing down neck)
            // - Deep in own territory (don't risk muff/loss)

            var baseFairCatchChance = GameProbabilities.Punts.PUNT_FAIR_CATCH_BASE;

            // Hang time factor (longer hang time = more pressure)
            if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_HIGH_HANG_THRESHOLD)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_HIGH_HANG_BONUS;
            else if (_hangTime > GameProbabilities.Punts.PUNT_MUFF_MEDIUM_HANG_THRESHOLD)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_MEDIUM_HANG_BONUS;

            // Field position factor (deep in own territory = more conservative)
            var actualFieldPosition = 100 - _returnSpot;
            if (actualFieldPosition < 10)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_10_BONUS;
            else if (actualFieldPosition < 20)
                baseFairCatchChance += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_20_BONUS;

            Occurred = _rng.NextDouble() < baseFairCatchChance;
        }
    }
}
