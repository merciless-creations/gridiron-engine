using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Rules.TwoMinuteWarning;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks if a quarter has expired and manages transitions between quarters.
    /// This action updates the game clock, determines if a quarter has ended,
    /// and advances the game to the next quarter when appropriate.
    /// Also handles two-minute warning detection.
    /// </summary>
    public sealed class QuarterExpireCheck : IGameAction
    {
        private readonly ITwoMinuteWarningRulesProvider _twoMinuteWarningRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuarterExpireCheck"/> class.
        /// </summary>
        /// <param name="twoMinuteWarningRules">The two-minute warning rules provider.</param>
        public QuarterExpireCheck(ITwoMinuteWarningRulesProvider twoMinuteWarningRules)
        {
            _twoMinuteWarningRules = twoMinuteWarningRules;
        }
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
            // Calculate time before play (for two-minute warning detection)
            int timeBeforePlay = game.CurrentQuarter.TimeRemaining + (int)game.CurrentPlay.ElapsedTime;

            //remove the current play elapsed time from the current quarter
            game.CurrentQuarter.TimeRemaining -= (int)game.CurrentPlay.ElapsedTime;

            // Check for two-minute warning (must happen after clock is decremented)
            if (_twoMinuteWarningRules.ShouldCallTwoMinuteWarning(
                game.CurrentQuarter.QuarterType,
                timeBeforePlay,
                game.CurrentQuarter.TimeRemaining,
                game.CurrentQuarter.TwoMinuteWarningCalled))
            {
                game.CurrentPlay.ClockStopped = true;
                game.CurrentPlay.Result.LogInformation("⏱️  Two-minute warning");
                game.CurrentQuarter.TwoMinuteWarningCalled = true;
            }

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
                        // Check if game is tied - if so, go to overtime
                        if (game.HomeScore == game.AwayScore)
                        {
                            game.CurrentQuarter.QuarterType = QuarterType.Overtime;
                            game.CurrentPlay.Result.LogInformation("Game is tied! Going to overtime...");
                        }
                        else
                        {
                            game.CurrentQuarter.QuarterType = QuarterType.GameOver;
                        }
                        break;
                    case QuarterType.Overtime:
                        // Overtime period ended - check if game should continue
                        // The OvertimeState and rules provider will determine the outcome
                        // For now, just check if still tied and rules allow ties
                        if (game.OvertimeState != null)
                        {
                            if (game.HomeScore == game.AwayScore)
                            {
                                // Still tied - GameFlow will determine if we continue or end
                                // QuarterType stays as Overtime to trigger another period or tie
                            }
                            else
                            {
                                // Game decided - there's a winner
                                game.CurrentQuarter.QuarterType = QuarterType.GameOver;
                            }
                        }
                        else
                        {
                            game.CurrentQuarter.QuarterType = QuarterType.GameOver;
                        }
                        break;
                }
            }
        }
    }
}