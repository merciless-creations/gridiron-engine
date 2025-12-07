namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Represents the decision for what type of scrimmage play to call.
    /// </summary>
    public enum PlayCallDecision
    {
        /// <summary>Run the ball.</summary>
        Run,
        /// <summary>Pass the ball.</summary>
        Pass,
        /// <summary>Spike the ball to stop the clock.</summary>
        Spike,
        /// <summary>Kneel down (victory formation) to run out the clock.</summary>
        Kneel
    }

    /// <summary>
    /// Represents the decision for post-touchdown conversion attempt.
    /// </summary>
    public enum ConversionDecision
    {
        /// <summary>Kick an extra point (1 point).</summary>
        ExtraPoint,
        /// <summary>Attempt a two-point conversion (2 points).</summary>
        TwoPointConversion
    }
}
