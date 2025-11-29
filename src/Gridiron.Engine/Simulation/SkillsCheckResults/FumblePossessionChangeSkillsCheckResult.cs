using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Determines which team recovers a fumble through a 50/50 coin flip.
    /// </summary>
    public class FumblePossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="FumblePossessionChangeSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining fumble recovery.</param>
        public FumblePossessionChangeSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes a coin flip to determine which team recovers the fumble.
        /// Each team has an equal 50% chance of recovery.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            //there was a fumble - who got it?
            var toss = _rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}