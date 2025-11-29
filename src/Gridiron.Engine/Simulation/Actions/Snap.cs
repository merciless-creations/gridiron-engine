using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles the snap of the ball from center to quarterback.
    /// Determines whether the snap is clean or muffed.
    /// </summary>
    public class Snap : IGameAction
    {
        private ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="Snap"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for snap quality determination.</param>
        public Snap(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the snap, determining if it was good or muffed.
        /// A muffed snap has approximately 1% probability (except on kickoffs which cannot be muffed).
        /// </summary>
        /// <param name="game">The game containing the current play.</param>
        public void Execute(Game game)
        {
            var didItHappen = _rng.NextDouble();

            //we can't have a muffed snap on a kick off - so don't even check
            game.CurrentPlay.GoodSnap = true;

            if (game.CurrentPlay.PlayType != PlayType.Kickoff)
            {
                game.CurrentPlay.GoodSnap = !(didItHappen <= .01);
            }

            game.CurrentPlay.ElapsedTime += game.CurrentPlay.GoodSnap ? 0.2 : 0.5;

            game.CurrentPlay.Result.LogInformation(game.CurrentPlay.GoodSnap
                ? "Good snap..."
                : "Oh no!  The snap is muffed - players are scrambling for the ball...");

            //TODO: Handle a muffed snap in every playtype except a kickoff
        }
    }
}
