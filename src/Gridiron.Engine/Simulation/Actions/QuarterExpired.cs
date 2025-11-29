using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles the transition between quarters when time expires.
    /// Teams change end zones between quarters.
    /// </summary>
    public class QuarterExpired : IGameAction
    {
        /// <summary>
        /// Executes quarter transition activities.
        /// </summary>
        /// <param name="game">The game transitioning to the next quarter.</param>
        public void Execute(Game game)
        {

        }
    }
}
