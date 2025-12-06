using Gridiron.Engine.Domain;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Mechanics;
using Microsoft.Extensions.Logging;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles halftime activities between the second and third quarters.
    /// Includes resetting timeouts for both teams.
    /// </summary>
    public class Halftime : IGameAction
    {
        private readonly TimeoutMechanic _timeoutMechanic;

        /// <summary>
        /// Initializes a new instance of the <see cref="Halftime"/> class.
        /// </summary>
        public Halftime()
        {
            _timeoutMechanic = new TimeoutMechanic();
        }

        /// <summary>
        /// Executes halftime activities including resetting timeouts.
        /// </summary>
        /// <param name="game">The game at halftime.</param>
        public void Execute(Game game)
        {
            // Reset timeouts for both teams
            _timeoutMechanic.ResetTimeoutsForHalf(game);
        }
    }
}
