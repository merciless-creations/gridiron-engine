using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Domain.Time;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Actions.EventChecks;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Mechanics;
using Gridiron.Engine.Simulation.Rules.TwoMinuteWarning;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Handles post-play activities including scoring checks, quarter expiration, and play logging.
    /// This is where injuries are checked, downs are advanced, and possessions may change.
    /// </summary>
    public class PostPlay : IGameAction
    {
        private readonly ISeedableRandom _rng;
        private readonly TimeoutDecisionEngine _timeoutDecisionEngine;
        private readonly TimeoutMechanic _timeoutMechanic;
        private readonly ITwoMinuteWarningRulesProvider _twoMinuteWarningRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostPlay"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for timeout decisions.</param>
        /// <param name="twoMinuteWarningRules">The two-minute warning rules provider.</param>
        public PostPlay(ISeedableRandom rng, ITwoMinuteWarningRulesProvider twoMinuteWarningRules)
        {
            _rng = rng;
            _timeoutDecisionEngine = new TimeoutDecisionEngine(rng);
            _timeoutMechanic = new TimeoutMechanic();
            _twoMinuteWarningRules = twoMinuteWarningRules;
        }

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

            var quarterExpireCheck = new QuarterExpireCheck(_twoMinuteWarningRules);
            quarterExpireCheck.Execute(game);

            var halftimeCheck = new HalfExpireCheck();
            halftimeCheck.Execute(game);

            // Check for stop the clock timeout (offense trailing late in half)
            CheckForPostPlayTimeout(game);

            game.Plays.Add(game.CurrentPlay);
        }

        /// <summary>
        /// Checks if the offense should call a timeout to stop the clock.
        /// </summary>
        private void CheckForPostPlayTimeout(Game game)
        {
            var currentPlay = game.CurrentPlay;
            if (currentPlay == null) return;

            // Skip if a timeout was already called on this play
            if (currentPlay.TimeoutCalledBeforePlay || currentPlay.TimeoutCalledAfterPlay) return;

            // Skip if there was a scoring play or possession change (clock stops anyway)
            if (currentPlay.IsTouchdown || currentPlay.IsSafety || currentPlay.PossessionChange) return;

            // Skip if clock is already stopped
            if (currentPlay.ClockStopped) return;

            // Only offense can call stop the clock timeout
            var offensiveTeam = currentPlay.Possession;
            if (offensiveTeam == Possession.None) return;

            // Calculate score differential from offense's perspective
            int offenseScore = offensiveTeam == Possession.Home ? game.HomeScore : game.AwayScore;
            int defenseScore = offensiveTeam == Possession.Home ? game.AwayScore : game.HomeScore;
            int scoreDifferential = offenseScore - defenseScore;

            // Calculate time remaining in half
            int timeRemainingInHalf = CalculateTimeRemainingInHalf(game);

            var context = new TimeoutContext(
                team: offensiveTeam,
                isOffense: true,
                timeoutsRemaining: game.GetTimeoutsRemaining(offensiveTeam),
                scoreDifferential: scoreDifferential,
                timeRemainingInHalfSeconds: timeRemainingInHalf,
                timeRemainingInGameSeconds: game.TimeRemaining,
                isClockRunning: true, // We already checked ClockStopped above
                playClockSeconds: 0, // Not relevant for post-play
                timingPhase: TimeoutTimingPhase.PostPlay
            );

            var decision = _timeoutDecisionEngine.Decide(context);

            if (decision == TimeoutDecision.StopClock)
            {
                var result = _timeoutMechanic.Execute(game, offensiveTeam, decision);
                if (result.Success)
                {
                    currentPlay.TimeoutCalledAfterPlay = true;
                    currentPlay.TimeoutCalledBy = offensiveTeam;
                    currentPlay.ClockStopped = true; // Clock is now stopped

                    var teamName = offensiveTeam == Possession.Home
                        ? game.HomeTeam.Name
                        : game.AwayTeam.Name;

                    game.Logger.LogInformation($"TIMEOUT: {teamName} calls timeout to stop the clock.");
                    game.Logger.LogInformation($"{teamName} has {result.TimeoutsRemainingAfter} timeout(s) remaining.");
                }
            }
        }

        /// <summary>
        /// Calculates the time remaining in the current half.
        /// </summary>
        private int CalculateTimeRemainingInHalf(Game game)
        {
            var quarterType = game.CurrentQuarter.QuarterType;

            // For Q2 or Q4 (end of half quarters), just use the quarter's remaining time
            if (quarterType == QuarterType.Second || quarterType == QuarterType.Fourth)
            {
                return game.CurrentQuarter.TimeRemaining;
            }

            // For Q1 or Q3 (start of half quarters), include the next quarter's time too
            if (quarterType == QuarterType.First)
            {
                return game.Halves[0].Quarters[0].TimeRemaining + game.Halves[0].Quarters[1].TimeRemaining;
            }

            if (quarterType == QuarterType.Third)
            {
                return game.Halves[1].Quarters[0].TimeRemaining + game.Halves[1].Quarters[1].TimeRemaining;
            }

            // Overtime - use current period's remaining time
            return game.CurrentQuarter.TimeRemaining;
        }
    }
}