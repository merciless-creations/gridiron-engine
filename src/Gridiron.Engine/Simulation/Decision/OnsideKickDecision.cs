namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Represents the decision for what type of kickoff to attempt.
    /// </summary>
    public enum OnsideKickDecision
    {
        /// <summary>Execute a normal kickoff for distance.</summary>
        NormalKickoff,
        /// <summary>Attempt an onside kick to recover possession.</summary>
        OnsideKick
    }
}
