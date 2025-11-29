namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a segment of a kick return (punt or kickoff return).
    /// </summary>
    public class ReturnSegment : IPlaySegment
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
        /// Gets or sets whether the play segment ended out of bounds.
        /// </summary>
        public bool IsOutOfBounds { get; set; }
    }
}
