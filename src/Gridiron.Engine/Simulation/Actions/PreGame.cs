using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles pre-game activities before the coin toss.
    /// Currently a placeholder for future implementation of warmups and introductions.
    /// </summary>
    public sealed class PreGame : IGameAction
    {
        /// <summary>
        /// Executes pre-game activities.
        /// </summary>
        /// <param name="game">The game to prepare.</param>
        public void Execute(Game game)
        {

        }
    }
}
