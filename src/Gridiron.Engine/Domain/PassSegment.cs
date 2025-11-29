namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a segment of a pass play, including forward passes and laterals.
    /// </summary>
    public class PassSegment : IPlaySegment
    {
        /// <summary>
        /// Gets or sets the player who threw the pass.
        /// </summary>
        public Player Passer { get; set; } = null!;

        /// <summary>
        /// Gets or sets the intended or actual receiver of the pass.
        /// </summary>
        public Player Receiver { get; set; } = null!;

        /// <summary>
        /// Gets the ball carrier for this segment (the receiver on a completed pass).
        /// </summary>
        public Player BallCarrier => Receiver;

        /// <summary>
        /// Gets the total yards gained on this segment.
        /// Returns 0 if the pass was incomplete.
        /// </summary>
        public int YardsGained => IsComplete ? AirYards + YardsAfterCatch : 0;

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
        /// Gets or sets whether the pass was completed.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the type of pass thrown.
        /// </summary>
        public PassType Type { get; set; }

        /// <summary>
        /// Gets or sets the distance the ball traveled in the air.
        /// </summary>
        public int AirYards { get; set; }

        /// <summary>
        /// Gets or sets the yards gained after the catch.
        /// </summary>
        public int YardsAfterCatch { get; set; }

        /// <summary>
        /// Gets or sets whether the play segment ended out of bounds.
        /// </summary>
        public bool IsOutOfBounds { get; set; }
    }

    /// <summary>
    /// Defines the types of passes that can be thrown.
    /// </summary>
    public enum PassType
    {
        /// <summary>Legal forward pass (only one allowed per play).</summary>
        Forward,
        /// <summary>Lateral or backward pass (unlimited per play).</summary>
        Lateral,
        /// <summary>Intentional backward pass.</summary>
        Backward,
        /// <summary>Short forward pass.</summary>
        Short,
        /// <summary>Deep forward pass.</summary>
        Deep,
        /// <summary>Screen pass behind the line of scrimmage.</summary>
        Screen
    }
}
