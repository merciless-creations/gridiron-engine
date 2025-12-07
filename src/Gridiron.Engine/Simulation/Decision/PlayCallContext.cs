using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Context for play calling decisions including run/pass selection and conversion attempts.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public readonly struct PlayCallContext
    {
        /// <summary>Current down (First through Fourth, or None for special situations).</summary>
        public Downs Down { get; }

        /// <summary>Yards needed for first down.</summary>
        public int YardsToGo { get; }

        /// <summary>Current field position (0-100, where 100 is opponent's goal line).</summary>
        public int FieldPosition { get; }

        /// <summary>Score differential from offense's perspective (positive = leading).</summary>
        public int ScoreDifferential { get; }

        /// <summary>Time remaining in the game in seconds.</summary>
        public int TimeRemainingSeconds { get; }

        /// <summary>Current quarter (1-4, or 5+ for overtime).</summary>
        public int Quarter { get; }

        /// <summary>Whether this is a two-point conversion attempt.</summary>
        public bool IsTwoPointConversion { get; }

        /// <summary>Team with possession.</summary>
        public Possession Possession { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayCallContext"/> struct.
        /// </summary>
        public PlayCallContext(
            Downs down,
            int yardsToGo,
            int fieldPosition,
            int scoreDifferential,
            int timeRemainingSeconds,
            int quarter,
            bool isTwoPointConversion,
            Possession possession)
        {
            Down = down;
            YardsToGo = yardsToGo;
            FieldPosition = fieldPosition;
            ScoreDifferential = scoreDifferential;
            TimeRemainingSeconds = timeRemainingSeconds;
            Quarter = quarter;
            IsTwoPointConversion = isTwoPointConversion;
            Possession = possession;
        }

        /// <summary>
        /// Creates a context for a normal scrimmage play from game state.
        /// </summary>
        public static PlayCallContext ForScrimmagePlay(Game game, Possession possession)
        {
            int scoreDiff = possession == Possession.Home
                ? game.HomeScore - game.AwayScore
                : game.AwayScore - game.HomeScore;

            return new PlayCallContext(
                down: game.CurrentDown,
                yardsToGo: game.YardsToGo,
                fieldPosition: game.FieldPosition,
                scoreDifferential: scoreDiff,
                timeRemainingSeconds: game.TimeRemaining,
                quarter: GetCurrentQuarter(game),
                isTwoPointConversion: false,
                possession: possession
            );
        }

        /// <summary>
        /// Creates a context for a two-point conversion attempt.
        /// </summary>
        public static PlayCallContext ForTwoPointConversion(Game game, Possession possession)
        {
            int scoreDiff = possession == Possession.Home
                ? game.HomeScore - game.AwayScore
                : game.AwayScore - game.HomeScore;

            return new PlayCallContext(
                down: Downs.None,
                yardsToGo: 2,
                fieldPosition: possession == Possession.Home ? 98 : 2,
                scoreDifferential: scoreDiff,
                timeRemainingSeconds: game.TimeRemaining,
                quarter: GetCurrentQuarter(game),
                isTwoPointConversion: true,
                possession: possession
            );
        }

        /// <summary>
        /// Creates a context for a post-touchdown conversion decision.
        /// </summary>
        public static PlayCallContext ForConversionDecision(Game game, Possession scoringTeam)
        {
            // Score differential BEFORE the touchdown was added
            // (the decision engine needs to know what the score situation is)
            int scoreDiff = scoringTeam == Possession.Home
                ? game.HomeScore - game.AwayScore
                : game.AwayScore - game.HomeScore;

            return new PlayCallContext(
                down: Downs.None,
                yardsToGo: 0,
                fieldPosition: scoringTeam == Possession.Home ? 98 : 2,
                scoreDifferential: scoreDiff,
                timeRemainingSeconds: game.TimeRemaining,
                quarter: GetCurrentQuarter(game),
                isTwoPointConversion: false,
                possession: scoringTeam
            );
        }

        private static int GetCurrentQuarter(Game game)
        {
            // Calculate current quarter number
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
        /// Whether this is a short yardage situation (3 or fewer yards to go).
        /// </summary>
        public bool IsShortYardage => YardsToGo <= 3;

        /// <summary>
        /// Whether this is a long yardage situation (7 or more yards to go).
        /// </summary>
        public bool IsLongYardage => YardsToGo >= 7;

        /// <summary>
        /// Whether the team is in the red zone (inside opponent's 20).
        /// </summary>
        public bool IsRedZone => FieldPosition >= 80;

        /// <summary>
        /// Whether this is a critical game situation (4th quarter, close game).
        /// </summary>
        public bool IsCriticalSituation => Quarter >= 4 && Math.Abs(ScoreDifferential) <= 8;

        /// <summary>
        /// Whether the team is trailing.
        /// </summary>
        public bool IsTrailing => ScoreDifferential < 0;

        /// <summary>
        /// Whether time is running out (less than 2 minutes in the game).
        /// </summary>
        public bool IsTwoMinuteWarning => TimeRemainingSeconds <= 120;

        #endregion
    }
}
