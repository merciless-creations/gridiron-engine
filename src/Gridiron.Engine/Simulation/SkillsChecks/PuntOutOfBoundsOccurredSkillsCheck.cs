using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Checks if punt goes out of bounds (no return possible).
    /// Punters may intentionally punt out of bounds in certain situations.
    /// </summary>
    public class PuntOutOfBoundsOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly int _puntLandingSpot;

        /// <summary>
        /// Initializes a new instance of the <see cref="PuntOutOfBoundsOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="puntLandingSpot">The yard line where the punt lands.</param>
        public PuntOutOfBoundsOccurredSkillsCheck(
            ISeedableRandom rng,
            int puntLandingSpot)
        {
            _rng = rng;
            _puntLandingSpot = puntLandingSpot;
        }

        /// <summary>
        /// Executes the punt out of bounds check to determine if the punt goes out of bounds.
        /// Probability increases when punting deep in opponent territory (directional punting).
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Punt out of bounds probability:
            // - Generally around 10-15%
            // - Slightly higher deep in opponent territory (directional punting)

            var baseOutOfBoundsChance = GameProbabilities.Punts.PUNT_OOB_BASE;

            // Field position factor (directional punting near goal line)
            if (_puntLandingSpot > GameProbabilities.Punts.PUNT_OOB_INSIDE_10_THRESHOLD)
                baseOutOfBoundsChance += GameProbabilities.Punts.PUNT_OOB_INSIDE_10_BONUS;
            else if (_puntLandingSpot > GameProbabilities.Punts.PUNT_OOB_INSIDE_15_THRESHOLD)
                baseOutOfBoundsChance += GameProbabilities.Punts.PUNT_OOB_INSIDE_15_BONUS;

            Occurred = _rng.NextDouble() < baseOutOfBoundsChance;
        }
    }
}
