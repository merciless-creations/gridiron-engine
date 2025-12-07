using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Contains all context needed to make a penalty accept/decline decision.
    /// </summary>
    public readonly struct PenaltyDecisionContext
    {
        // ========================================
        // PENALTY INFORMATION
        // ========================================

        /// <summary>
        /// The penalty being evaluated.
        /// </summary>
        public PenaltyNames PenaltyName { get; }

        /// <summary>
        /// Yards assessed for this penalty.
        /// </summary>
        public int PenaltyYards { get; }

        /// <summary>
        /// Which team committed the penalty.
        /// </summary>
        public Possession PenalizedTeam { get; }

        /// <summary>
        /// When the penalty occurred (before, during, after play).
        /// </summary>
        public PenaltyOccuredWhen OccurredWhen { get; }

        // ========================================
        // PLAY RESULT
        // ========================================

        /// <summary>
        /// Which team had possession (offense) on the play.
        /// </summary>
        public Possession Offense { get; }

        /// <summary>
        /// Yards gained (or lost if negative) on the play.
        /// </summary>
        public int YardsGainedOnPlay { get; }

        /// <summary>
        /// Whether the play resulted in a first down (before penalty consideration).
        /// </summary>
        public bool PlayResultedInFirstDown { get; }

        /// <summary>
        /// Whether the play resulted in a turnover.
        /// </summary>
        public bool PlayResultedInTurnover { get; }

        /// <summary>
        /// Whether the play resulted in a touchdown.
        /// </summary>
        public bool PlayResultedInTouchdown { get; }

        // ========================================
        // GAME SITUATION (before the play)
        // ========================================

        /// <summary>
        /// Down before the play.
        /// </summary>
        public Downs CurrentDown { get; }

        /// <summary>
        /// Yards to go before the play.
        /// </summary>
        public int YardsToGo { get; }

        /// <summary>
        /// Field position before the play (0-100, 0 = own goal line).
        /// </summary>
        public int FieldPosition { get; }

        /// <summary>
        /// Score differential from the deciding team's perspective.
        /// Positive = leading, Negative = trailing.
        /// </summary>
        public int ScoreDifferential { get; }

        /// <summary>
        /// Time remaining in the game (seconds).
        /// </summary>
        public int TimeRemainingSeconds { get; }

        // ========================================
        // DERIVED PROPERTIES
        // ========================================

        /// <summary>
        /// Whether this is an offensive penalty (committed by team with ball).
        /// </summary>
        public bool IsOffensivePenalty => PenalizedTeam == Offense;

        /// <summary>
        /// Whether this is a defensive penalty (committed by team without ball).
        /// </summary>
        public bool IsDefensivePenalty => !IsOffensivePenalty;

        /// <summary>
        /// The team that gets to decide whether to accept or decline.
        /// (The team that was fouled - opposite of penalized team)
        /// </summary>
        public Possession DecidingTeam => PenalizedTeam == Possession.Home
            ? Possession.Away
            : Possession.Home;

        /// <summary>
        /// Initializes a new instance of the <see cref="PenaltyDecisionContext"/> struct.
        /// </summary>
        public PenaltyDecisionContext(
            PenaltyNames penaltyName,
            int penaltyYards,
            Possession penalizedTeam,
            PenaltyOccuredWhen occurredWhen,
            Possession offense,
            int yardsGainedOnPlay,
            bool playResultedInFirstDown,
            bool playResultedInTurnover,
            bool playResultedInTouchdown,
            Downs currentDown,
            int yardsToGo,
            int fieldPosition,
            int scoreDifferential,
            int timeRemainingSeconds)
        {
            PenaltyName = penaltyName;
            PenaltyYards = penaltyYards;
            PenalizedTeam = penalizedTeam;
            OccurredWhen = occurredWhen;
            Offense = offense;
            YardsGainedOnPlay = yardsGainedOnPlay;
            PlayResultedInFirstDown = playResultedInFirstDown;
            PlayResultedInTurnover = playResultedInTurnover;
            PlayResultedInTouchdown = playResultedInTouchdown;
            CurrentDown = currentDown;
            YardsToGo = yardsToGo;
            FieldPosition = fieldPosition;
            ScoreDifferential = scoreDifferential;
            TimeRemainingSeconds = timeRemainingSeconds;
        }

        /// <summary>
        /// Creates a context from game state and penalty information.
        /// </summary>
        public static PenaltyDecisionContext FromGameState(
            Game game,
            IPlay play,
            Penalty penalty)
        {
            // Calculate score differential from deciding team's perspective
            var decidingTeam = penalty.CalledOn == Possession.Home
                ? Possession.Away
                : Possession.Home;

            var decidingTeamScore = decidingTeam == Possession.Home
                ? game.HomeScore
                : game.AwayScore;
            var opposingScore = decidingTeam == Possession.Home
                ? game.AwayScore
                : game.HomeScore;

            return new PenaltyDecisionContext(
                penaltyName: penalty.Name,
                penaltyYards: penalty.Yards,
                penalizedTeam: penalty.CalledOn,
                occurredWhen: penalty.OccuredWhen,
                offense: play.Possession,
                yardsGainedOnPlay: play.YardsGained,
                playResultedInFirstDown: play.YardsGained >= game.YardsToGo,
                playResultedInTurnover: play.PossessionChange,
                playResultedInTouchdown: play.IsTouchdown,
                currentDown: play.Down,
                yardsToGo: game.YardsToGo,
                fieldPosition: game.FieldPosition,
                scoreDifferential: decidingTeamScore - opposingScore,
                timeRemainingSeconds: game.TimeRemaining
            );
        }
    }
}
