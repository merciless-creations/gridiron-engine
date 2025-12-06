using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Contains all the game state information needed to make a timeout decision.
    /// </summary>
    public readonly struct TimeoutContext
    {
        /// <summary>
        /// The team considering whether to call a timeout.
        /// </summary>
        public Possession Team { get; }

        /// <summary>
        /// Whether the team considering the timeout is on offense.
        /// </summary>
        public bool IsOffense { get; }

        /// <summary>
        /// The number of timeouts remaining for the team.
        /// </summary>
        public int TimeoutsRemaining { get; }

        /// <summary>
        /// The score differential from the perspective of the team considering the timeout.
        /// Positive means leading, negative means trailing.
        /// </summary>
        public int ScoreDifferential { get; }

        /// <summary>
        /// The total time remaining in the half in seconds.
        /// </summary>
        public int TimeRemainingInHalfSeconds { get; }

        /// <summary>
        /// The total time remaining in the game in seconds.
        /// </summary>
        public int TimeRemainingInGameSeconds { get; }

        /// <summary>
        /// Whether the game clock is currently running.
        /// </summary>
        public bool IsClockRunning { get; }

        /// <summary>
        /// The current play clock value in seconds.
        /// </summary>
        public int PlayClockSeconds { get; }

        /// <summary>
        /// The type of play about to be executed (for pre-play decisions).
        /// Null for post-play decisions.
        /// </summary>
        public PlayType? UpcomingPlayType { get; }

        /// <summary>
        /// The field goal distance in yards (if upcoming play is a field goal).
        /// </summary>
        public int? FieldGoalDistance { get; }

        /// <summary>
        /// The timing phase of the decision (pre-play or post-play).
        /// </summary>
        public TimeoutTimingPhase TimingPhase { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutContext"/> struct.
        /// </summary>
        public TimeoutContext(
            Possession team,
            bool isOffense,
            int timeoutsRemaining,
            int scoreDifferential,
            int timeRemainingInHalfSeconds,
            int timeRemainingInGameSeconds,
            bool isClockRunning,
            int playClockSeconds,
            TimeoutTimingPhase timingPhase,
            PlayType? upcomingPlayType = null,
            int? fieldGoalDistance = null)
        {
            Team = team;
            IsOffense = isOffense;
            TimeoutsRemaining = timeoutsRemaining;
            ScoreDifferential = scoreDifferential;
            TimeRemainingInHalfSeconds = timeRemainingInHalfSeconds;
            TimeRemainingInGameSeconds = timeRemainingInGameSeconds;
            IsClockRunning = isClockRunning;
            PlayClockSeconds = playClockSeconds;
            TimingPhase = timingPhase;
            UpcomingPlayType = upcomingPlayType;
            FieldGoalDistance = fieldGoalDistance;
        }
    }

    /// <summary>
    /// Defines when in the play sequence a timeout decision is being considered.
    /// </summary>
    public enum TimeoutTimingPhase
    {
        /// <summary>
        /// Before the play is executed (e.g., ice the kicker, avoid delay of game).
        /// </summary>
        PrePlay,

        /// <summary>
        /// After the play is completed (e.g., stop the clock when trailing).
        /// </summary>
        PostPlay
    }
}
