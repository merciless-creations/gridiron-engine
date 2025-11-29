using Gridiron.Engine.Domain;
using System.Reflection;
using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Simulation.Services
{
    /// <summary>
    /// Provides static methods for accumulating player statistics during play execution.
    /// Tracks offensive, defensive, and special teams statistics for all players involved in plays.
    /// </summary>
    public static class StatsAccumulator
    {
        private static Random _random = new Random();

        /// <summary>
        /// Accumulates passing and receiving statistics from a completed pass play.
        /// Tracks passing yards, touchdowns, interceptions, completions, attempts, and receiving stats.
        /// </summary>
        /// <param name="play">The pass play to accumulate statistics from.</param>
        public static void AccumulatePassStats(PassPlay play)
        {
            // Check for sack (negative yards on pass play)
            bool isSack = play.YardsGained < 0;

            if (isSack)
            {
                // Sack stats handled in AccumulateDefensiveStats
            }
            else
            {
                if (play.PrimaryPasser != null)
                {
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingYards, play.YardsGained);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingTouchdowns, play.IsTouchdown ? 1 : 0);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.InterceptionsThrown, play.Interception ? 1 : 0);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingAttempts, 1);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingCompletions, play.IsComplete ? 1 : 0);
                }

                foreach (var segment in play.PassSegments)
                {
                    UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingTargets, 1);
                    if (segment.IsComplete)
                    {
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.Receptions, 1);
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingYards, segment.YardsGained);
                        // PassSegment doesn't have Touchdown property directly, infer from play result if this is the final segment
                        // Or check if segment ended in TD? PassSegment doesn't have IsTouchdown.
                        // We can check if the play was a TD and this is the final receiver.
                        bool isTouchdown = play.IsTouchdown && segment == play.PassSegments.Last();
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingTouchdowns, isTouchdown ? 1 : 0);
                    }
                }
            }
        }

        /// <summary>
        /// Accumulates rushing statistics from a completed run play.
        /// Tracks rushing yards, touchdowns, and attempts for each ball carrier in the play.
        /// </summary>
        /// <param name="play">The run play to accumulate statistics from.</param>
        public static void AccumulateRunStats(RunPlay play)
        {
            foreach (var segment in play.RunSegments)
            {
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingYards, segment.YardsGained);
                // RunSegment likely doesn't have Touchdown property either.
                bool isTouchdown = play.IsTouchdown && segment == play.RunSegments.Last();
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingTouchdowns, isTouchdown ? 1 : 0);
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingAttempts, 1);
            }
        }

        /// <summary>
        /// Accumulates field goal kicking statistics from a field goal attempt.
        /// Tracks field goals made and attempted for the kicker.
        /// </summary>
        /// <param name="play">The field goal play to accumulate statistics from.</param>
        /// <exception cref="InvalidOperationException">Thrown when the play is missing a kicker.</exception>
        public static void AccumulateFieldGoalStats(FieldGoalPlay play)
        {
            // Defensive check - should never happen, but catches bugs early
            if (play.Kicker == null)
            {
                // Log or throw - this indicates a bug in play execution
                throw new InvalidOperationException("Field goal play missing kicker");
            }

            if (play.IsGood)
            {
                UpdatePlayerStat(play.Kicker, PlayerStatType.FieldGoalsMade, 1);
            }
            UpdatePlayerStat(play.Kicker, PlayerStatType.FieldGoalsAttempted, 1);
        }

        /// <summary>
        /// Accumulates punting and punt return statistics from a punt play.
        /// Tracks punts, punt yards, punts inside the 20, punt returns, and punt return yards.
        /// </summary>
        /// <param name="play">The punt play to accumulate statistics from.</param>
        /// <exception cref="InvalidOperationException">Thrown when the play is missing a punter.</exception>
        public static void AccumulatePuntStats(PuntPlay play)
        {
            // Defensive check - should never happen, but catches bugs early
            if (play.Punter == null)
            {
                throw new InvalidOperationException("Punt play missing kicker");
            }

            UpdatePlayerStat(play.Punter, PlayerStatType.Punts, 1);
            UpdatePlayerStat(play.Punter, PlayerStatType.PuntYards, play.PuntDistance);

            // Return stats
            if (play.InitialReturner != null)
            {
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.PuntReturns, 1);
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.PuntReturnYards, play.TotalReturnYards);
            }

            // Punts inside 20
            // If punt ends inside the 20 yard line (field position <= 20)
            // Note: Field position is relative to the offense. 
            // If kicking from own 20, end position 80 is opponent's 20.
            // Standard convention: 0-100 scale where 100 is opponent end zone.
            // So inside 20 means EndFieldPosition >= 80.
            if (play.EndFieldPosition >= 80)
            {
                UpdatePlayerStat(play.Punter, PlayerStatType.PuntsInside20, 1);
            }
        }

        /// <summary>
        /// Accumulates kickoff return statistics from a kickoff play.
        /// Tracks kickoff returns and kickoff return yards for the returner.
        /// </summary>
        /// <param name="play">The kickoff play to accumulate statistics from.</param>
        public static void AccumulateKickoffStats(KickoffPlay play)
        {
            if (play.InitialReturner != null)
            {
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.KickoffReturns, 1);
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.KickoffReturnYards, play.TotalReturnYards);
            }
        }

        /// <summary>
        /// Accumulates fumble statistics from any play that contains fumbles.
        /// Tracks fumbles and fumble recoveries for all players involved.
        /// </summary>
        /// <param name="play">The play to accumulate fumble statistics from.</param>
        public static void AccumulateFumbleStats(IPlay play)
        {
            ProcessFumbles(play.Fumbles);
        }

        /// <summary>
        /// Processes a list of fumbles and updates player statistics.
        /// Records fumbles for players who fumbled and fumble recoveries for players who recovered.
        /// </summary>
        /// <param name="fumbles">The list of fumbles to process.</param>
        private static void ProcessFumbles(List<Fumble> fumbles)
        {
            if (fumbles == null) return;

            foreach (var fumble in fumbles)
            {
                UpdatePlayerStat(fumble.FumbledBy, PlayerStatType.Fumbles, 1);
                if (fumble.RecoveredBy != null)
                {
                    UpdatePlayerStat(fumble.RecoveredBy, PlayerStatType.FumbleRecoveries, 1);
                }
            }
        }

        /// <summary>
        /// Accumulates defensive statistics from any play.
        /// Tracks sacks, tackles, and interceptions for defensive players.
        /// Randomly assigns credit for tackles and sacks to defensive players on the field.
        /// </summary>
        /// <param name="play">The play to accumulate defensive statistics from.</param>
        public static void AccumulateDefensiveStats(IPlay play)
        {
            var defensePlayers = play.DefensePlayersOnField;
            if (defensePlayers == null || !defensePlayers.Any()) return;

            var defenders = defensePlayers.Where(p => IsDefender(p)).ToList();
            if (!defenders.Any()) return;

            bool isSack = false;

            // Sacks
            if (play is PassPlay passPlay && passPlay.YardsGained < 0 && !passPlay.IsComplete)
            {
                isSack = true;
                var sacker = defenders[_random.Next(defenders.Count)];
                UpdatePlayerStat(sacker, PlayerStatType.Sacks, 1);
                UpdatePlayerStat(sacker, PlayerStatType.Tackles, 1); // Credit tackle too
            }

            // Generic Tackles
            // Only award generic tackle if it wasn't a sack AND play ended in a tackle (not TD, not incomplete)
            bool genericTackleOccurred = !play.IsTouchdown;

            if (play is PassPlay pp)
            {
                // If incomplete and not a sack, no tackle occurred
                if (!pp.IsComplete && !isSack)
                {
                    genericTackleOccurred = false;
                }
            }

            if (genericTackleOccurred && !isSack)
            {
                var tackler = defenders[_random.Next(defenders.Count)];
                UpdatePlayerStat(tackler, PlayerStatType.Tackles, 1);
            }

            // Interceptions
            if (play is PassPlay interceptionPlay && interceptionPlay.Interception && interceptionPlay.InterceptionDetails != null)
            {
                UpdatePlayerStat(interceptionPlay.InterceptionDetails.InterceptedBy, PlayerStatType.InterceptionsCaught, 1);
            }
        }

        /// <summary>
        /// Updates a specific statistic for a player by adding the specified value.
        /// Creates the stat entry if it doesn't exist for the player.
        /// </summary>
        /// <param name="player">The player to update statistics for.</param>
        /// <param name="statType">The type of statistic to update.</param>
        /// <param name="value">The value to add to the statistic.</param>
        private static void UpdatePlayerStat(Player player, PlayerStatType statType, int value)
        {
            if (player == null) return;

            if (!player.Stats.ContainsKey(statType))
            {
                player.Stats[statType] = 0;
            }
            player.Stats[statType] += value;
        }

        /// <summary>
        /// Determines whether a player is in a defensive position.
        /// Checks if the player's position is one of the standard defensive positions.
        /// </summary>
        /// <param name="p">The player to check.</param>
        /// <returns>True if the player is a defender, false otherwise.</returns>
        private static bool IsDefender(Player p)
        {
            return p.Position == Positions.DT ||
                   p.Position == Positions.DE ||
                   p.Position == Positions.LB ||
                   p.Position == Positions.CB ||
                   p.Position == Positions.S ||
                   p.Position == Positions.FS;
        }
    }
}
