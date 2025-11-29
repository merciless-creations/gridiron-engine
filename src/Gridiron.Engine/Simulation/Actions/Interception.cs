using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles interception outcomes when a defensive player catches a pass intended for the offense.
    /// Always results in a possession change.
    /// </summary>
    public sealed class Interception : IGameAction
    {
        private readonly Possession _possession;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interception"/> class.
        /// </summary>
        /// <param name="possession">The team that intercepted the ball.</param>
        public Interception(Possession possession)
        {
            _possession = possession;
        }

        /// <summary>
        /// Executes the interception action, recording the turnover and possession change.
        /// </summary>
        /// <param name="game">The game containing the current play with the interception.</param>
        public void Execute(Game game)
        {
            //there was a possession change on the play
            game.CurrentPlay.PossessionChange = true;

            //set the correct possession in the game
            game.CurrentPlay.Possession = _possession;
            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.LogInformation("Interception!!");
            game.CurrentPlay.Result.LogInformation("Possession changes hands");
            game.CurrentPlay.Result.LogInformation($"{game.CurrentPlay.Possession} now has possession");

            //now we know somebody bobbled the ball, and somebody recovered it - add that in the play for the records
            game.CurrentPlay.Interception = true;
        }
    }
}
