using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Context for onside kick decisions.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public readonly struct OnsideKickContext
    {
        /// <summary>Score differential from kicking team's perspective (negative = trailing).</summary>
        public int ScoreDifferential { get; }

        /// <summary>Time remaining in the game in seconds.</summary>
        public int TimeRemainingSeconds { get; }

        /// <summary>Current quarter (1-4, or 5+ for overtime).</summary>
        public int Quarter { get; }

        /// <summary>Timeouts remaining for the kicking team.</summary>
        public int TimeoutsRemaining { get; }

        /// <summary>Team performing the kickoff.</summary>
        public Possession KickingTeam { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnsideKickContext"/> struct.
        /// </summary>
        public OnsideKickContext(
            int scoreDifferential,
            int timeRemainingSeconds,
            int quarter,
            int timeoutsRemaining,
            Possession kickingTeam)
        {
            ScoreDifferential = scoreDifferential;
            TimeRemainingSeconds = timeRemainingSeconds;
            Quarter = quarter;
            TimeoutsRemaining = timeoutsRemaining;
            KickingTeam = kickingTeam;
        }

        /// <summary>
        /// Creates a context for an onside kick decision from game state.
        /// </summary>
        public static OnsideKickContext FromGame(Game game, Possession kickingTeam)
        {
            int scoreDiff = kickingTeam == Possession.Home
                ? game.HomeScore - game.AwayScore
                : game.AwayScore - game.HomeScore;

            int timeouts = kickingTeam == Possession.Home
                ? game.HomeTimeoutsRemaining
                : game.AwayTimeoutsRemaining;

            return new OnsideKickContext(
                scoreDifferential: scoreDiff,
                timeRemainingSeconds: game.TimeRemaining,
                quarter: GetCurrentQuarter(game),
                timeoutsRemaining: timeouts,
                kickingTeam: kickingTeam
            );
        }

        private static int GetCurrentQuarter(Game game)
        {
            int quarterNum = 1;
            foreach (var half in game.Halves)
            {
                foreach (var quarter in half.Quarters)
                {
                    if (quarter == game.CurrentQuarter)
                        return quarterNum;
                    quarterNum++;
                }
            }
            return quarterNum;
        }

        #region Derived Properties

        /// <summary>
        /// Whether the kicking team is trailing.
        /// </summary>
        public bool IsTrailing => ScoreDifferential < 0;

        /// <summary>
        /// Whether the kicking team is trailing by more than one score (8+ points).
        /// </summary>
        public bool IsTrailingByMoreThanOneScore => ScoreDifferential <= -9;

        /// <summary>
        /// Whether the kicking team is trailing by exactly one score (1-8 points).
        /// </summary>
        public bool IsTrailingByOneScore => ScoreDifferential >= -8 && ScoreDifferential < 0;

        /// <summary>
        /// Whether this is a late-game situation (4th quarter or overtime).
        /// </summary>
        public bool IsLateGame => Quarter >= 4;

        /// <summary>
        /// Whether time is critically low (less than 5 minutes remaining).
        /// </summary>
        public bool IsCriticalTime => TimeRemainingSeconds <= 300;

        /// <summary>
        /// Whether time is desperately low (less than 2 minutes remaining).
        /// </summary>
        public bool IsDesperateTime => TimeRemainingSeconds <= 120;

        /// <summary>
        /// Whether an onside kick is strategically necessary based on game situation.
        /// True when trailing significantly late in the game.
        /// </summary>
        public bool IsOnsideKickSituation => IsTrailing && IsLateGame && ScoreDifferential <= -7;

        /// <summary>
        /// Whether an onside kick is absolutely desperate (trailing by 2+ scores with little time).
        /// </summary>
        public bool IsDesperateSituation => IsTrailingByMoreThanOneScore && IsDesperateTime;

        #endregion
    }
}
