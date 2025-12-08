namespace Gridiron.Engine.Simulation.Rules.EndOfHalf
{
    /// <summary>
    /// Defines the contract for end-of-half rules implementations.
    /// Different leagues have different rules about whether a half can end on a penalty:
    /// - NFL: Half cannot end on an accepted defensive penalty (offense gets untimed down)
    /// - NCAA: Similar to NFL (half cannot end on accepted defensive penalty)
    /// </summary>
    public interface IEndOfHalfRulesProvider
    {
        /// <summary>
        /// Gets the name of the ruleset (e.g., "NFL", "NCAA").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines if the half can end when there is an accepted penalty on the defense
        /// with no time remaining.
        /// 
        /// NFL/NCAA Rule: If time expires and there is an accepted penalty by the defense,
        /// the period is extended for one untimed down.
        /// </summary>
        /// <returns>
        /// False if half CANNOT end on defensive penalty (offense gets untimed down).
        /// True if half CAN end on defensive penalty (no extension).
        /// </returns>
        bool AllowsHalfToEndOnDefensivePenalty { get; }

        /// <summary>
        /// Determines if the half can end when there is an accepted penalty on the offense
        /// with no time remaining.
        /// 
        /// NFL/NCAA Rule: If time expires and there is an accepted penalty by the offense,
        /// the half ends (no untimed down).
        /// </summary>
        /// <returns>
        /// True if half CAN end on offensive penalty (standard rule).
        /// False if half CANNOT end on offensive penalty (extension - rare).
        /// </returns>
        bool AllowsHalfToEndOnOffensivePenalty { get; }
    }
}
