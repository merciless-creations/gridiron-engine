namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents an interception that occurred during a passing play.
    /// </summary>
    public class Interception
    {
        /// <summary>
        /// Gets or sets the defensive player who intercepted the pass.
        /// </summary>
        public required Player InterceptedBy { get; set; }

        /// <summary>
        /// Gets or sets the quarterback who threw the intercepted pass.
        /// </summary>
        public required Player ThrownBy { get; set; }

        /// <summary>
        /// Gets or sets the yard line (0-100) where the interception occurred.
        /// </summary>
        public int InterceptionYardLine { get; set; }

        /// <summary>
        /// Gets or sets the yards gained on the interception return.
        /// </summary>
        public int ReturnYards { get; set; }

        /// <summary>
        /// Gets or sets whether the intercepting player fumbled during the return.
        /// </summary>
        public bool FumbledDuringReturn { get; set; }

        /// <summary>
        /// Gets or sets the player who recovered the fumble if one occurred during the return.
        /// </summary>
        public Player? RecoveredBy { get; set; }
    }
}