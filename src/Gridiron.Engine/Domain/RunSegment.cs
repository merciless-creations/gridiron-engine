namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a segment of a run play, tracking ball carrier and yardage.
    /// </summary>
    public class RunSegment : IPlaySegment
    {
        /// <summary>
        /// Gets or sets the player carrying the ball during this segment.
        /// </summary>
        public Player BallCarrier { get; set; } = null!;

        /// <summary>
        /// Gets or sets the yards gained or lost during this segment.
        /// </summary>
        public int YardsGained { get; set; }

        /// <summary>
        /// Gets or sets whether the segment ended in a fumble.
        /// </summary>
        public bool EndedInFumble { get; set; }

        /// <summary>
        /// Gets or sets the player who fumbled, if applicable.
        /// </summary>
        public Player? FumbledBy { get; set; }

        /// <summary>
        /// Gets or sets the player who recovered the fumble, if applicable.
        /// </summary>
        public Player? RecoveredBy { get; set; }

        /// <summary>
        /// Gets or sets the direction of the run.
        /// </summary>
        public RunDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets whether the play segment ended out of bounds.
        /// </summary>
        public bool IsOutOfBounds { get; set; }
    }

    /// <summary>
    /// Defines the direction of a rushing play.
    /// </summary>
    public enum RunDirection
    {
        /// <summary>Run to the left side of the field.</summary>
        Left,
        /// <summary>Run to the right side of the field.</summary>
        Right,
        /// <summary>Run up the middle of the field.</summary>
        Middle,
        /// <summary>Run toward the middle-left.</summary>
        MiddleLeft,
        /// <summary>Run toward the middle-right.</summary>
        MiddleRight,
        /// <summary>Run directly up the middle through the center.</summary>
        UpTheMiddle,
        /// <summary>Run off the left tackle position.</summary>
        OffLeftTackle,
        /// <summary>Run off the right tackle position.</summary>
        OffRightTackle,
        /// <summary>End-around or sweep play to the outside.</summary>
        Sweep
    }
}
