using Gridiron.Engine.Domain;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Decision;
using Microsoft.Extensions.Logging;

namespace Gridiron.Engine.Simulation.Mechanics
{
    /// <summary>
    /// Executes the game mechanics when a timeout is called.
    ///
    /// <para><b>This is a GAME MECHANIC - it answers "here's what happens when a timeout is called"</b></para>
    /// <para>It does NOT decide whether to call a timeout. Use <see cref="TimeoutDecisionEngine"/> for that.</para>
    ///
    /// <para><b>TIMEOUT EFFECTS</b></para>
    /// <list type="bullet">
    ///   <item>Game clock stops</item>
    ///   <item>Play clock resets to 25 seconds</item>
    ///   <item>Team's timeout count decrements by 1</item>
    ///   <item>Timeout is recorded in game state</item>
    /// </list>
    /// </summary>
    public class TimeoutMechanic
    {
        /// <summary>
        /// Executes a timeout for the specified team.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="team">The team calling the timeout.</param>
        /// <param name="reason">The reason for the timeout (for logging/tracking).</param>
        /// <returns>The result of the timeout execution.</returns>
        public TimeoutResult Execute(Game game, Possession team, TimeoutDecision reason)
        {
            // Validate: team must have timeouts remaining
            int timeoutsRemaining = GetTimeoutsRemaining(game, team);
            if (timeoutsRemaining <= 0)
            {
                return new TimeoutResult
                {
                    Success = false,
                    Team = team,
                    Reason = reason,
                    FailureReason = "No timeouts remaining"
                };
            }

            // Decrement timeout count
            DecrementTimeouts(game, team);

            // Stop the game clock (clock management is tracked elsewhere, but we record the effect)
            // Note: Actual clock stopping is handled by the game flow

            // Record the timeout
            var result = new TimeoutResult
            {
                Success = true,
                Team = team,
                Reason = reason,
                TimeoutsRemainingAfter = GetTimeoutsRemaining(game, team),
                PlayClockResetTo = GameProbabilities.Timeouts.PLAY_CLOCK_AFTER_TIMEOUT
            };

            // Log the timeout
            LogTimeout(game, result);

            return result;
        }

        /// <summary>
        /// Resets timeouts for both teams at halftime.
        /// </summary>
        /// <param name="game">The current game state.</param>
        public void ResetTimeoutsForHalf(Game game)
        {
            game.HomeTimeoutsRemaining = GameProbabilities.Timeouts.TIMEOUTS_PER_HALF;
            game.AwayTimeoutsRemaining = GameProbabilities.Timeouts.TIMEOUTS_PER_HALF;

            game.Logger.LogInformation("Timeouts reset for second half. Each team has 3 timeouts.");
        }

        /// <summary>
        /// Sets timeouts for overtime.
        /// </summary>
        /// <param name="game">The current game state.</param>
        public void SetTimeoutsForOvertime(Game game)
        {
            game.HomeTimeoutsRemaining = GameProbabilities.Timeouts.TIMEOUTS_PER_OVERTIME;
            game.AwayTimeoutsRemaining = GameProbabilities.Timeouts.TIMEOUTS_PER_OVERTIME;

            game.Logger.LogInformation("Overtime starting. Each team has 2 timeouts.");
        }

        /// <summary>
        /// Gets the number of timeouts remaining for a team.
        /// </summary>
        private int GetTimeoutsRemaining(Game game, Possession team)
        {
            return team switch
            {
                Possession.Home => game.HomeTimeoutsRemaining,
                Possession.Away => game.AwayTimeoutsRemaining,
                _ => 0
            };
        }

        /// <summary>
        /// Decrements the timeout count for a team.
        /// </summary>
        private void DecrementTimeouts(Game game, Possession team)
        {
            if (team == Possession.Home)
            {
                game.HomeTimeoutsRemaining--;
            }
            else if (team == Possession.Away)
            {
                game.AwayTimeoutsRemaining--;
            }
        }

        /// <summary>
        /// Logs the timeout to the game logger.
        /// </summary>
        private void LogTimeout(Game game, TimeoutResult result)
        {
            string teamName = result.Team == Possession.Home ? "Home" : "Away";
            string reasonText = result.Reason switch
            {
                TimeoutDecision.StopClock => "to stop the clock",
                TimeoutDecision.IceKicker => "to ice the kicker",
                TimeoutDecision.AvoidDelayOfGame => "to avoid delay of game",
                _ => ""
            };

            game.Logger.LogInformation(
                $"TIMEOUT called by {teamName} {reasonText}. " +
                $"{result.TimeoutsRemainingAfter} timeout(s) remaining.");
        }
    }

    /// <summary>
    /// Represents the result of executing a timeout.
    /// </summary>
    public class TimeoutResult
    {
        /// <summary>
        /// Whether the timeout was successfully executed.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// The team that called the timeout.
        /// </summary>
        public Possession Team { get; init; }

        /// <summary>
        /// The reason for the timeout.
        /// </summary>
        public TimeoutDecision Reason { get; init; }

        /// <summary>
        /// The number of timeouts remaining after this timeout.
        /// </summary>
        public int TimeoutsRemainingAfter { get; init; }

        /// <summary>
        /// The play clock value after the timeout (typically 25 seconds).
        /// </summary>
        public int PlayClockResetTo { get; init; }

        /// <summary>
        /// If the timeout failed, the reason why.
        /// </summary>
        public string? FailureReason { get; init; }
    }
}
