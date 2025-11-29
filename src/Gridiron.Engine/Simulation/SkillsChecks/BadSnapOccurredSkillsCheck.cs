using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Checks if a bad snap occurs on a punt.
    /// Bad snaps are rare but can be catastrophic.
    /// </summary>
    public class BadSnapOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _longSnapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadSnapOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="longSnapper">The long snapper whose skill affects bad snap probability.</param>
        public BadSnapOccurredSkillsCheck(ISeedableRandom rng, Player longSnapper)
        {
            _rng = rng;
            _longSnapper = longSnapper;
        }

        /// <summary>
        /// Executes the bad snap check to determine if a bad snap occurs.
        /// Bad snap probability is based on the long snapper's blocking skill.
        /// Average LS (50 skill): ~2% chance, Good LS (70+ skill): ~0.5% chance, Poor LS (30 skill): ~5% chance.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Bad snap probability based on long snapper's skill
            // Average LS (50 skill): ~2% chance
            // Good LS (70+ skill): ~0.5% chance
            // Poor LS (30 skill): ~5% chance

            var skillFactor = _longSnapper.Blocking / GameProbabilities.Punts.PUNT_BAD_SNAP_SKILL_DENOMINATOR;
            var badSnapChance = GameProbabilities.Punts.PUNT_BAD_SNAP_BASE
                - (skillFactor * GameProbabilities.Punts.PUNT_BAD_SNAP_SKILL_FACTOR);

            Occurred = _rng.NextDouble() < badSnapChance;
        }
    }
}
