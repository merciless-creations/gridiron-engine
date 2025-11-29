using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks if a half has expired and handles transitioning between halves.
    /// This action monitors quarter expiration and advances the game to the next half
    /// or marks the game as over when appropriate.
    /// </summary>
    public sealed class HalfExpireCheck : IGameAction
    {
        /// <summary>
        /// Executes the half expiration check and advances the game state when a half ends.
        /// </summary>
        /// <param name="game">The game instance to check for half expiration.</param>
        /// <remarks>
        /// When the third quarter expires, this method transitions the game to the second half.
        /// When the game is over, it sets the half type to GameOver. Future enhancements will
        /// include handling tied games and overtime transitions.
        /// </remarks>
        public void Execute(Game game)
        {
            if (game.CurrentPlay.QuarterExpired)
            {
                switch (game.CurrentQuarter.QuarterType)
                {
                    case QuarterType.Third:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf = game.Halves[1];
                        break;
                    case QuarterType.GameOver:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf.HalfType = HalfType.GameOver;
                        break;
                }
            }

            //TODO check if tied & move to OT
            //TODO check if tied & move to another OT
        }
    }
}
