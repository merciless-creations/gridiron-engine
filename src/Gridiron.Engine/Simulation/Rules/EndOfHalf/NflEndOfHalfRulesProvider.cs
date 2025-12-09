namespace Gridiron.Engine.Simulation.Rules.EndOfHalf
{
    /// <summary>
    /// Implements NFL end-of-half penalty rules.
    /// 
    /// NFL Rule: A half cannot end on an accepted defensive penalty.
    /// If time expires (0:00) and there is a defensive penalty that is accepted,
    /// the offense is granted one untimed down to continue the drive.
    /// 
    /// However, if the penalty is on the offense and accepted, the half ends.
    /// </summary>
    public class NflEndOfHalfRulesProvider : IEndOfHalfRulesProvider
    {
        /// <inheritdoc/>
        public string Name => "NFL";

        /// <inheritdoc/>
        public bool AllowsHalfToEndOnDefensivePenalty => false; // Half CANNOT end - offense gets untimed down

        /// <inheritdoc/>
        public bool AllowsHalfToEndOnOffensivePenalty => true; // Half CAN end - no untimed down
    }
}
