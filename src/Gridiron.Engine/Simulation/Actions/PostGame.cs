using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles post-game activities after the final whistle.
    /// Currently a placeholder for future implementation of statistics finalization and handshakes.
    /// </summary>
    public class PostGame : IGameAction
    {
        /// <summary>
        /// Executes post-game activities.
        /// </summary>
        /// <param name="game">The completed game.</param>
        public void Execute(Game game)
        {

        }
    }
}
