using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks and validates scoring events during game simulation.
    /// This action is responsible for monitoring and processing score changes
    /// that occur during gameplay.
    /// </summary>
    public sealed class ScoreCheck : IGameAction
    {
        /// <summary>
        /// Executes the score check to validate and process scoring events.
        /// </summary>
        /// <param name="game">The game instance to check for scoring events.</param>
        /// <remarks>
        /// This method will be implemented to handle score validation and updates
        /// during game simulation.
        /// </remarks>
        public void Execute(Game game)
        {

        }
    }
}
