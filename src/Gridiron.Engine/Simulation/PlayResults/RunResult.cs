using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Services;
using System;
using System.Linq;

namespace Gridiron.Engine.Simulation.PlayResults
{
    /// <summary>
    /// Processes running play results including yards gained, fumbles, touchdowns, and penalties.
    /// Handles field position updates, down progression, and scoring for rushing plays.
    /// </summary>
    public class RunResult : IGameAction
    {
        /// <summary>
        /// Executes the running play result, updating game state including field position, score, down and distance.
        /// Handles various outcomes such as gains, losses, fumbles, touchdowns, safeties, and penalties.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        public void Execute(Game game)
        {
            var play = (RunPlay)game.CurrentPlay;

            // Set start field position
            play.StartFieldPosition = game.FieldPosition;

            // Check for fumble with safety (handle early)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Determine who scores the safety based on who recovered
                // If defense recovered in offense's end zone, they score the safety
                // If offense recovered in their own end zone, defense scores
                Possession scoringTeam;
                if (play.Fumbles.Count > 0 && play.PossessionChange)
                {
                    // Defense recovered in offense's end zone - defense scores
                    scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                }
                else
                {
                    // Offense recovered in own end zone or normal safety - defense scores
                    scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                }

                game.AddSafety(scoringTeam);
                play.PossessionChange = true;
                return;
            }

            // Check for fumble with defensive TD
            if (play.IsTouchdown && play.Fumbles.Count > 0 && play.PossessionChange)
            {
                // Defensive TD on fumble recovery
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(defendingTeam);
                play.PossessionChange = true;
                return;
            }

            // Calculate new field position
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check for touchdown or two-point conversion
            if (newFieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                // Update score using centralized method
                if (play.IsTwoPointConversion)
                {
                    game.AddTwoPointConversion(play.Possession);
                }
                else
                {
                    game.AddTouchdown(play.Possession);
                }

                // After touchdown/2pt conversion, possession changes (kickoff will follow)
                play.PossessionChange = true;
            }
            else if (newFieldPosition <= 0)
            {
                // Safety - ball carrier tackled in own end zone
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Award 2 points to defense using centralized method
                var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddSafety(defendingTeam);

                play.PossessionChange = true;
            }
            else
            {
                // Normal play result - check for penalties
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;

                // Check if there are any accepted penalties
                var hasAcceptedPenalties = play.Penalties != null && play.Penalties.Any();

                if (hasAcceptedPenalties)
                {
                    // Apply smart acceptance/decline logic to penalties using decision engine
                    ApplyPenaltyAcceptanceLogic(game, play);

                    // Recheck after acceptance logic
                    hasAcceptedPenalties = play.Penalties.Any(p => p.Accepted);
                }

                // Two-point conversion failed - possession changes
                if (play.IsTwoPointConversion)
                {
                    play.PossessionChange = true;
                    play.Result.LogInformation($"Two-point conversion FAILED. No points scored.");
                }
                else if (hasAcceptedPenalties)
                {
                    // Enforce penalties and adjust field position / down / distance
                    var penaltyEnforcement = new PenaltyEnforcement(play.Result);
                    var enforcementResult = penaltyEnforcement.EnforcePenalties(game, play, play.YardsGained);

                    // Update field position based on net yards (play result + penalties)
                    var finalFieldPosition = game.FieldPosition + enforcementResult.NetYards - play.YardsGained;

                    // Bounds check
                    if (finalFieldPosition >= 100)
                    {
                        // Penalty pushed offense into end zone for TD
                        play.IsTouchdown = true;
                        play.EndFieldPosition = 100;
                        game.FieldPosition = 100;
                        game.AddTouchdown(play.Possession);
                        play.PossessionChange = true;
                        return;
                    }
                    else if (finalFieldPosition <= 0)
                    {
                        // Penalty pushed offense into own end zone for safety
                        play.IsSafety = true;
                        play.EndFieldPosition = 0;
                        game.FieldPosition = 0;
                        var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                        game.AddSafety(defendingTeam);
                        play.PossessionChange = true;
                        return;
                    }

                    play.EndFieldPosition = finalFieldPosition;
                    game.FieldPosition = finalFieldPosition;

                    // Apply down and distance from penalty enforcement
                    if (enforcementResult.IsOffsetting)
                    {
                        // Offsetting penalties - replay the down
                        play.Result.LogInformation($"Offsetting penalties. {FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FormatFieldPosition(play.Possession)}.");
                    }
                    else if (enforcementResult.AutomaticFirstDown)
                    {
                        // Automatic first down from penalty
                        game.CurrentDown = Downs.First;
                        game.YardsToGo = 10;
                        play.Result.LogInformation($"First down! Ball at the {game.FormatFieldPositionWithYardLine(play.Possession)}.");
                    }
                    else if (enforcementResult.NewDown == Downs.First && enforcementResult.NewYardsToGo == 10)
                    {
                        // First down achieved
                        game.CurrentDown = Downs.First;
                        game.YardsToGo = 10;
                        play.Result.LogInformation($"First down! Ball at the {game.FormatFieldPositionWithYardLine(play.Possession)}.");
                    }
                    else if (enforcementResult.NewDown == Downs.None)
                    {
                        // Turnover on downs
                        play.PossessionChange = true;
                        game.CurrentDown = Downs.First;
                        game.YardsToGo = 10;
                        play.Result.LogInformation($"Turnover on downs! Ball at the {game.FormatFieldPositionWithYardLine(play.Possession)}.");
                    }
                    else
                    {
                        // Update down and distance from enforcement result
                        game.CurrentDown = enforcementResult.NewDown;
                        game.YardsToGo = Math.Max(1, enforcementResult.NewYardsToGo);
                        play.Result.LogInformation($"Runner is down. {FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FormatFieldPosition(play.Possession)}.");
                    }
                }
                else
                {
                    // No penalties - use normal down progression
                    // Check for first down
                    if (play.YardsGained >= game.YardsToGo)
                    {
                        // First down!
                        game.CurrentDown = Downs.First;
                        game.YardsToGo = 10;
                        play.Result.LogInformation($"First down! Ball at the {game.FormatFieldPositionWithYardLine(play.Possession)}.");
                    }
                    else
                    {
                        // Advance the down
                        var nextDown = AdvanceDown(game.CurrentDown);
                        game.YardsToGo -= play.YardsGained;

                        if (nextDown == Downs.None)
                        {
                            // Turnover on downs - other team gets 1st and 10
                            play.PossessionChange = true;
                            game.CurrentDown = Downs.First;
                            game.YardsToGo = 10;
                            play.Result.LogInformation($"Turnover on downs! Ball at the {game.FormatFieldPositionWithYardLine(play.Possession)}.");
                        }
                        else
                        {
                            game.CurrentDown = nextDown;
                            play.Result.LogInformation($"Runner is down. {FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FormatFieldPosition(play.Possession)}.");
                        }
                    }
                }
            }

            // Log final ball carrier and stats
            var finalCarrier = play.FinalBallCarrier;
            if (finalCarrier != null)
            {
                play.Result.LogInformation($"{finalCarrier.LastName}: {play.RunSegments.Count} carry for {play.YardsGained} yards.");
            }

            // Accumulate stats
            StatsAccumulator.AccumulateRunStats(play);
            StatsAccumulator.AccumulateDefensiveStats(play);
            StatsAccumulator.AccumulateFumbleStats(play);
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// Uses PenaltyDecisionEngine to determine whether each penalty should be accepted or declined.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The run play that contains the penalties.</param>
        private void ApplyPenaltyAcceptanceLogic(Game game, RunPlay play)
        {
            if (play.Penalties == null || !play.Penalties.Any())
                return;

            var decisionEngine = new PenaltyDecisionEngine();

            foreach (var penalty in play.Penalties)
            {
                var context = PenaltyDecisionContext.FromGameState(game, play, penalty);
                var decision = decisionEngine.Decide(context);
                penalty.Accepted = decision == PenaltyDecision.Accept;
            }
        }

        /// <summary>
        /// Advances the current down to the next down.
        /// </summary>
        /// <param name="currentDown">The current down.</param>
        /// <returns>The next down, or Downs.None if turnover on downs.</returns>
        private Downs AdvanceDown(Downs currentDown)
        {
            return currentDown switch
            {
                Downs.First => Downs.Second,
                Downs.Second => Downs.Third,
                Downs.Third => Downs.Fourth,
                Downs.Fourth => Downs.None, // Turnover on downs
                _ => Downs.None
            };
        }

        /// <summary>
        /// Formats a down enumeration value to its display string representation (1st, 2nd, 3rd, 4th).
        /// </summary>
        /// <param name="down">The down to format.</param>
        /// <returns>The formatted down string.</returns>
        private string FormatDown(Downs down)
        {
            return down switch
            {
                Downs.First => "1st",
                Downs.Second => "2nd",
                Downs.Third => "3rd",
                Downs.Fourth => "4th",
                _ => "?"
            };
        }
    }
}