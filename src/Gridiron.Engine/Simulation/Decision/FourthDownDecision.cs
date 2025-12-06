namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Represents the possible decisions on fourth down.
    /// </summary>
    public enum FourthDownDecision
    {
        /// <summary>Attempt to convert the fourth down with a run or pass play.</summary>
        GoForIt,

        /// <summary>Punt the ball to change field position.</summary>
        Punt,

        /// <summary>Attempt a field goal.</summary>
        AttemptFieldGoal
    }
}
