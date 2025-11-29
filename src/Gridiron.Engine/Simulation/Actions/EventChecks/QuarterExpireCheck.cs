using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks if a quarter has expired and manages transitions between quarters.
    /// This action updates the game clock, determines if a quarter has ended,
    /// and advances the game to the next quarter when appropriate.
    /// </summary>
    public sealed class QuarterExpireCheck : IGameAction
    {
        /// <summary>
        /// Executes the quarter expiration check, updates game time, and advances to the next quarter if needed.
        /// </summary>
        /// <param name="game">The game instance to check for quarter expiration.</param>
        /// <remarks>
        /// This method first subtracts the elapsed play time from the current quarter's remaining time.
        /// If the quarter time reaches zero, it marks the quarter as expired and transitions to the next quarter:
        /// - First quarter advances to second quarter
        /// - Second quarter advances to third quarter (start of second half)
        /// - Third quarter advances to fourth quarter
        /// - Fourth quarter marks the game as over
        /// - Overtime handling for tied games is planned for future implementation
        /// </remarks>
        public void Execute(Game game)
        {
            //remove the current play elapsed time from the current quarter
            game.CurrentQuarter.TimeRemaining -= (int)game.CurrentPlay.ElapsedTime;

            //see if we need to advance to the next quarter
            if (game.CurrentQuarter.TimeRemaining == 0)
            {
                game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentQuarter.QuarterType} quarter");
                game.CurrentPlay.QuarterExpired = true;

                switch (game.CurrentQuarter.QuarterType)
                {
                    case QuarterType.First:
                        game.CurrentQuarter = game.Halves[0].Quarters[1];
                        break;
                    case QuarterType.Second:
                        game.CurrentQuarter = game.Halves[1].Quarters[0];
                        break;
                    case QuarterType.Third:
                        game.CurrentQuarter = game.Halves[1].Quarters[1];
                        break;
                    case QuarterType.Fourth:
                        game.CurrentQuarter.QuarterType = QuarterType.GameOver;
                        break;
                    case QuarterType.Overtime:
                        //TODO check if tied & move to another OT
                        break;
                }
            }
        }
    }
}