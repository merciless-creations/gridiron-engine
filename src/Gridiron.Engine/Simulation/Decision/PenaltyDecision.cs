namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Represents the decision to accept or decline a penalty.
    /// </summary>
    public enum PenaltyDecision
    {
        /// <summary>
        /// Accept the penalty - enforce the yardage and down/distance changes.
        /// </summary>
        Accept,

        /// <summary>
        /// Decline the penalty - keep the result of the play as-is.
        /// </summary>
        Decline
    }
}
