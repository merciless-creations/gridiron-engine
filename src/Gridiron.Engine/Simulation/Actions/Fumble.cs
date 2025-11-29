using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles fumble outcomes when a player loses control of the ball.
    /// Determines possession changes and records the fumble in the play.
    /// </summary>
    public sealed class Fumble : IGameAction
    {
        private readonly Possession _possession;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fumble"/> class.
        /// </summary>
        /// <param name="possession">The team that recovered the fumble.</param>
        public Fumble(Possession possession)
        {
            _possession = possession;
        }

        /// <summary>
        /// Executes the fumble action, determining if possession changed and logging the result.
        /// </summary>
        /// <param name="game">The game containing the current play with the fumble.</param>
        public void Execute(Game game)
        {
            //first determine if there was a possession change on the play
            game.CurrentPlay.PossessionChange = _possession != game.CurrentPlay.Possession;

            //set the correct possession in the game
            game.CurrentPlay.Possession = _possession;

            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.LogInformation("Fumble on the play");
            if (game.CurrentPlay.PossessionChange)
            {
                game.CurrentPlay.Result.LogInformation("Possession changes hands");
                game.CurrentPlay.Result.LogInformation($"{game.CurrentPlay.Possession} now has possession");
            }
            else
            {
                game.CurrentPlay.Result.LogInformation($"{game.CurrentPlay.Possession} keeps possession");
            }

            //now we know somebody bobbled the ball, and somebody recovered it - add that in the play for the records
            //we'll fill in the players involved in the fumble later
            game.CurrentPlay.Fumbles.Add(new Gridiron.Engine.Domain.Fumble());
        }
    }
}
