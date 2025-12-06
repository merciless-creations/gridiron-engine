namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Represents the outcome of a timeout decision.
    /// </summary>
    public enum TimeoutDecision
    {
        /// <summary>
        /// No timeout should be called.
        /// </summary>
        None,

        /// <summary>
        /// Call timeout to stop the game clock (offensive strategy when trailing late).
        /// </summary>
        StopClock,

        /// <summary>
        /// Call timeout to "ice" the kicker before a field goal attempt (defensive strategy).
        /// </summary>
        IceKicker,

        /// <summary>
        /// Call timeout to avoid a delay of game penalty (offensive strategy).
        /// </summary>
        AvoidDelayOfGame
    }
}
