using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Services;
using System;
using System.Linq;

namespace Gridiron.Engine.Simulation.PlayResults
{
    /// <summary>
    /// Processes punt play results including blocked punts, returns, fair catches, touchbacks, and muffed catches.
    /// Handles field position updates, possession changes, and scoring for special teams punt plays.
    /// </summary>
    public class PuntResult : IGameAction
    {
        /// <summary>
        /// Executes the punt play result, updating game state including field position, possession, and score.
        /// Handles various outcomes such as blocked punts, punt returns, fair catches, touchbacks, downed punts, and muffed catches.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        public void Execute(Game game)
        {
            var play = game.CurrentPlay as PuntPlay;

            // Set start field position
            play.StartFieldPosition = game.FieldPosition;

            // Handle safety (bad snap into end zone or tackled in end zone)
            if (play.IsSafety)
            {
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Determine who gets the 2 points
                // If NOT blocked: bad snap, offense tackled in own end zone → defense scores
                // If blocked and possession changed: defense recovered and ran backwards → offense scores
                // If blocked and possession didn't change: offense recovered in end zone → defense scores
                var scoringTeam = (play.Blocked && play.PossessionChange)
                    ? play.Possession  // Defense recovered and ran backwards, original team gets points
                    : (play.Possession == Possession.Home ? Possession.Away : Possession.Home); // Offense tackled in end zone, defense gets points

                game.AddSafety(scoringTeam);

                play.PossessionChange = true;

                // Handle penalties if any occurred
                HandlePenalties(game, play);
                LogPuntSummary(play);
                return; // Safety ends the play immediately
            }

            // Handle different punt outcomes
            if (play.Blocked && play.RecoveredBy != null)
            {
                // Blocked punt was recovered - handle scoring and possession
                HandleBlockedPuntRecovery(game, play);
            }
            else if (play.MuffedCatch && play.RecoveredBy != null)
            {
                // Muffed catch was recovered
                HandleMuffedCatchRecovery(game, play);
            }
            else if (play.Touchback)
            {
                // Touchback - receiving team gets ball at their 20
                play.EndFieldPosition = 80; // From punting team's perspective (100 - 20)
                game.FieldPosition = 80;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else if (play.Downed || play.FairCatch)
            {
                // Punt downed or fair catch - possession changes at spot
                var newFieldPosition = game.FieldPosition + play.YardsGained;
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else if (play.ReturnSegments.Any())
            {
                // Punt return occurred
                HandlePuntReturn(game, play);
            }
            else
            {
                // Default case (shouldn't normally hit this)
                var newFieldPosition = game.FieldPosition + play.YardsGained;
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }

            // Handle penalties if any occurred
            HandlePenalties(game, play);

            // Log final punt summary
            LogPuntSummary(play);

            // Accumulate player stats
            StatsAccumulator.AccumulatePuntStats(play);
        }

        /// <summary>
        /// Handles penalty enforcement for punt plays, including offsetting penalties and automatic first downs.
        /// Adjusts field position and down/distance based on accepted penalties.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The punt play that contains the penalties.</param>
        private void HandlePenalties(Game game, PuntPlay play)
        {
            // Check if there are any penalties on the play
            var hasAcceptedPenalties = play.Penalties != null && play.Penalties.Any();

            if (!hasAcceptedPenalties)
            {
                return; // No penalties, nothing to do
            }

            // If the play already resulted in a TD or safety, and penalties are declined, don't modify
            if ((play.IsTouchdown || play.IsSafety) && play.Penalties.All(p => !p.Accepted))
            {
                return; // Scoring play stands, penalties declined
            }

            // Apply smart acceptance/decline logic to penalties
            var penaltyEnforcement = new PenaltyEnforcement(play.Result);
            ApplyPenaltyAcceptanceLogic(game, play, penaltyEnforcement);

            // Recheck after acceptance logic
            hasAcceptedPenalties = play.Penalties.Any(p => p.Accepted);

            if (!hasAcceptedPenalties)
            {
                return; // All penalties were declined
            }

            // Enforce penalties and get the result
            var enforcementResult = penaltyEnforcement.EnforcePenalties(game, play, play.YardsGained);

            // Determine if this is a return penalty or a punting penalty
            var isReturnPenalty = play.ReturnSegments != null && play.ReturnSegments.Any();
            int finalFieldPosition;

            if (enforcementResult.IsOffsetting)
            {
                // Offsetting penalties - rekick from original spot
                finalFieldPosition = play.StartFieldPosition;
                game.FieldPosition = finalFieldPosition;
                play.EndFieldPosition = finalFieldPosition;
                game.CurrentDown = Downs.Fourth; // Punts are always 4th down
                play.PossessionChange = false; // Punting team rekicks
                play.Result.LogInformation($"Offsetting penalties. Rekick from the {game.FieldPosition}.");
                return;
            }
            else if (isReturnPenalty)
            {
                // Penalty during return - enforce from end of return
                // game.FieldPosition is already set to end of return
                // NetYards is the adjustment (positive helps punting team, negative hurts)
                finalFieldPosition = game.FieldPosition + enforcementResult.NetYards;
            }
            else
            {
                // Penalty during punt (not return) - enforce from line of scrimmage
                // Start from original position and apply penalty
                finalFieldPosition = play.StartFieldPosition + enforcementResult.NetYards;
            }

            // Bounds check
            if (finalFieldPosition >= 100)
            {
                // Penalty pushed returner into end zone for TD
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true;
                return;
            }
            else if (finalFieldPosition <= 0)
            {
                // Penalty pushed team into own end zone for safety
                play.IsSafety = true;
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;
                var scoringTeam = play.Possession;
                game.AddSafety(scoringTeam);
                play.PossessionChange = true;
                return;
            }

            play.EndFieldPosition = finalFieldPosition;
            game.FieldPosition = finalFieldPosition;

            // Apply down and distance from penalty enforcement
            if (enforcementResult.AutomaticFirstDown)
            {
                // Automatic first down from penalty (e.g., defensive penalty on punt)
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = false; // Punting team retains possession
                play.Result.LogInformation($"Automatic first down from penalty. Ball at the {game.FieldPosition} yard line.");
            }
            else
            {
                // Regular punt penalty - possession changes after enforcement
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.PossessionChange = true; // Possession changes on punts (unless automatic first down for punting team)
                play.Result.LogInformation($"Penalty enforced. Ball at the {game.FieldPosition} yard line. 1st and 10.");
            }
        }

        /// <summary>
        /// Applies smart acceptance/decline logic to all penalties on the play.
        /// Determines whether each penalty should be accepted or declined based on game situation and which team benefits.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The punt play that contains the penalties.</param>
        /// <param name="enforcement">The penalty enforcement service that evaluates whether penalties should be accepted.</param>
        private void ApplyPenaltyAcceptanceLogic(Game game, PuntPlay play, PenaltyEnforcement enforcement)
        {
            if (play.Penalties == null || !play.Penalties.Any())
                return;

            // Skip if already explicitly accepted or declined (e.g., in tests or scenarios)
            var hasExplicitAcceptance = play.Penalties.Any(p => p.Accepted);
            if (hasExplicitAcceptance)
            {
                // Don't override explicit acceptance decisions
                return;
            }

            foreach (var penalty in play.Penalties)
            {
                // Determine which team committed the penalty
                var penalizedTeam = penalty.CalledOn;

                // Use smart acceptance logic
                penalty.Accepted = enforcement.ShouldAcceptPenalty(
                    game,
                    penalty,
                    penalizedTeam,
                    play.Possession,
                    play.YardsGained,
                    play.Down,
                    game.YardsToGo);
            }
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

        /// <summary>
        /// Handles the recovery of a blocked punt, determining possession and scoring based on who recovered the ball.
        /// Can result in touchdowns, safeties, or possession changes.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The punt play with a blocked punt recovery.</param>
        private void HandleBlockedPuntRecovery(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check if recovery resulted in touchdown
            if (play.IsTouchdown && (newFieldPosition >= 100 || newFieldPosition <= 0))
            {
                // Set end position based on which end zone
                if (newFieldPosition >= 100)
                {
                    play.EndFieldPosition = 100;
                    game.FieldPosition = 100;
                }
                else // newFieldPosition <= 0 - ball in punting team's end zone
                {
                    play.EndFieldPosition = 0;
                    game.FieldPosition = 0;
                }

                // Determine who scored (defense if they recovered)
                var scoringTeam = play.PossessionChange
                    ? (play.Possession == Possession.Home ? Possession.Away : Possession.Home)
                    : play.Possession;

                game.AddTouchdown(scoringTeam);
                play.PossessionChange = true; // TD always changes possession
            }
            else if (play.PossessionChange)
            {
                // Defense recovered but no TD
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else
            {
                // Offense recovered their own blocked punt
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                // Turnover on downs (4th down punt failed)
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
        }

        /// <summary>
        /// Handles the recovery of a muffed punt catch, determining possession based on who recovered the ball.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The punt play with a muffed catch recovery.</param>
        private void HandleMuffedCatchRecovery(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            if (play.PossessionChange)
            {
                // Receiving team recovered their own muff
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
            else
            {
                // Punting team recovered the muff - they keep possession!
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
                play.Result.LogInformation($"Punting team retains possession after muffed catch recovery!");
            }
        }

        /// <summary>
        /// Handles punt return plays, updating field position and checking for return touchdowns.
        /// </summary>
        /// <param name="game">The game instance containing current game state.</param>
        /// <param name="play">The punt play with a return.</param>
        private void HandlePuntReturn(Game game, PuntPlay play)
        {
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check for touchdown on return
            if (play.IsTouchdown || newFieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                // Receiving team scored
                var scoringTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddTouchdown(scoringTeam);

                play.PossessionChange = true;
            }
            else
            {
                // Normal return, no TD
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;
                play.PossessionChange = true;
                game.CurrentDown = Downs.First;
                game.YardsToGo = 10;
            }
        }

        /// <summary>
        /// Logs a summary of the punt play result including punter, punt distance, and return information.
        /// </summary>
        /// <param name="play">The punt play to summarize.</param>
        private void LogPuntSummary(PuntPlay play)
        {
            if (play.Punter != null)
            {
                var returner = play.InitialReturner;
                var totalReturn = play.TotalReturnYards;

                if (play.Blocked)
                {
                    play.Result.LogInformation($"Blocked punt. Net: {play.YardsGained} yards.");
                }
                else if (play.Touchback)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, touchback.");
                }
                else if (play.FairCatch)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, fair catch.");
                }
                else if (play.Downed)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, downed at {100 - play.DownedAtYardLine}.");
                }
                else if (returner != null)
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt, {totalReturn} yard return by {returner.LastName}. Net: {play.YardsGained} yards.");
                }
                else
                {
                    play.Result.LogInformation($"{play.Punter.LastName}: {play.PuntDistance} yard punt. Net: {play.YardsGained} yards.");
                }
            }
        }
    }
}