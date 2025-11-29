using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles the coin toss at the start of the game to determine initial possession.
    /// </summary>
    public class CoinToss : IGameAction
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoinToss"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for the coin flip.</param>
        public CoinToss(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the coin toss, determining which team wins and whether they defer.
        /// </summary>
        /// <param name="game">The game to execute the coin toss for.</param>
        public void Execute(Game game)
        {
            var toss = _rng.Next(2);
            game.WonCoinToss = toss == 1 ? Possession.Away : Possession.Home;

            var deferred = _rng.Next(2);
            game.DeferredPossession = deferred == 1;
        }
    }
}
