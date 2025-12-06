namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Contains all the game state information needed to make a fourth down decision.
    /// </summary>
    public readonly struct FourthDownContext
    {
        /// <summary>
        /// The current field position (0-100 absolute scale).
        /// </summary>
        public int FieldPosition { get; }

        /// <summary>
        /// The yards needed for a first down.
        /// </summary>
        public int YardsToGo { get; }

        /// <summary>
        /// The score differential from the perspective of the team with possession.
        /// Positive means leading, negative means trailing.
        /// </summary>
        public int ScoreDifferential { get; }

        /// <summary>
        /// The total time remaining in the game in seconds.
        /// </summary>
        public int TimeRemainingSeconds { get; }

        /// <summary>
        /// Whether the home team has possession.
        /// </summary>
        public bool IsHome { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FourthDownContext"/> struct.
        /// </summary>
        /// <param name="fieldPosition">The current field position (0-100).</param>
        /// <param name="yardsToGo">The yards needed for a first down.</param>
        /// <param name="scoreDifferential">The score differential (positive = leading).</param>
        /// <param name="timeRemainingSeconds">Time remaining in seconds.</param>
        /// <param name="isHome">Whether home team has possession.</param>
        public FourthDownContext(
            int fieldPosition,
            int yardsToGo,
            int scoreDifferential,
            int timeRemainingSeconds,
            bool isHome)
        {
            FieldPosition = fieldPosition;
            YardsToGo = yardsToGo;
            ScoreDifferential = scoreDifferential;
            TimeRemainingSeconds = timeRemainingSeconds;
            IsHome = isHome;
        }
    }
}
