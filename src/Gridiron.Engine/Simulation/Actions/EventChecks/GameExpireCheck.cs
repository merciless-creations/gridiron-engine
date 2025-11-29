using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks if the game has expired and handles end-of-game conditions.
    /// This action is responsible for determining if the game is over and managing
    /// overtime scenarios when the game ends in a tie.
    /// </summary>
    public sealed class GameExpireCheck : IGameAction
    {
        /// <summary>
        /// Executes the game expiration check to determine if the game has ended.
        /// </summary>
        /// <param name="game">The game instance to check for expiration.</param>
        /// <remarks>
        /// This method checks if the game is over and not tied. If the game is tied,
        /// it sets up overtime quarters and extends the game accordingly.
        /// </remarks>
        public void Execute(Game game)
        {
            //in here is where we need to check to see if the game is over and not tied...
            //if it is tied this is where we would need to set up overtime quarters
            //and extend the game etc
        }
    }
}
