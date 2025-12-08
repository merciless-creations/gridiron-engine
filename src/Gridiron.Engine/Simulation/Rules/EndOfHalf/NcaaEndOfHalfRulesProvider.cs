namespace Gridiron.Engine.Simulation.Rules.EndOfHalf
{
    /// <summary>
    /// Implements NCAA end-of-half penalty rules.
    /// 
    /// NCAA Rule: Similar to NFL - a period cannot end on an accepted defensive penalty.
    /// If time expires and there is a live-ball foul by the defense that is accepted,
    /// the period is extended for one untimed down.
    /// 
    /// If the foul is by the offense and accepted, the period ends.
    /// </summary>
    public class NcaaEndOfHalfRulesProvider : IEndOfHalfRulesProvider
    {
        /// <inheritdoc/>
        public string Name => "NCAA";

        /// <inheritdoc/>
        public bool AllowsHalfToEndOnDefensivePenalty => false; // Period CANNOT end - offense gets untimed down

        /// <inheritdoc/>
        public bool AllowsHalfToEndOnOffensivePenalty => true; // Period CAN end - no untimed down
    }
}
