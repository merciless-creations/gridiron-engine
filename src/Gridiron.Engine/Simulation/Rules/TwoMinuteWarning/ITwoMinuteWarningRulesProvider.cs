using Gridiron.Engine.Domain.Time;

namespace Gridiron.Engine.Simulation.Rules.TwoMinuteWarning
{
    /// <summary>
    /// Defines the contract for two-minute warning rules implementations.
    /// Different leagues have different rules:
    /// - NFL: Two-minute warning at 2:00 remaining in 2nd and 4th quarters
    /// - NCAA: No two-minute warning
    /// </summary>
    public interface ITwoMinuteWarningRulesProvider
    {
        /// <summary>
        /// Gets the name of the ruleset (e.g., "NFL", "NCAA").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines if a two-minute warning should be called.
        /// Called after the game clock has been decremented for a play.
        /// </summary>
        /// <param name="quarterType">The current quarter (First, Second, Third, Fourth, Overtime).</param>
        /// <param name="timeBeforePlay">Time remaining in seconds before the play was executed.</param>
        /// <param name="timeAfterPlay">Time remaining in seconds after the play was executed.</param>
        /// <param name="alreadyCalled">Whether the two-minute warning has already been called this quarter.</param>
        /// <returns>True if the two-minute warning should be called now, false otherwise.</returns>
        bool ShouldCallTwoMinuteWarning(
            QuarterType quarterType,
            int timeBeforePlay,
            int timeAfterPlay,
            bool alreadyCalled);
    }
}
