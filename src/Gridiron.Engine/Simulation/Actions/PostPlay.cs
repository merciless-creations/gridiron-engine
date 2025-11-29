using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Actions.EventChecks;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles post-play activities including scoring checks, quarter expiration, and play logging.
    /// This is where injuries are checked, downs are advanced, and possessions may change.
    /// </summary>
    public class PostPlay : IGameAction
    {
        /// <summary>
        /// Executes post-play activities and adds the current play to the game's play list.
        /// </summary>
        /// <param name="game">The game containing the completed play.</param>
        public void Execute(Game game)
        {
            //inside here we will do things like check for injuries, advance the down, change possession
            //determine if it's a hurry up offense or if they are trying to
            //kill the clock and add time appropriately...
            //add the current play to the plays list

            // Visual marker for end of play
            game.CurrentPlay.Result.LogInformation("---");

            var scoreCheck = new ScoreCheck();
            scoreCheck.Execute(game);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            var halftimeCheck = new HalfExpireCheck();
            halftimeCheck.Execute(game);

            game.Plays.Add(game.CurrentPlay);
        }
    }
}