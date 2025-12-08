using Gridiron.Engine.Domain.Time;

namespace Gridiron.Engine.Simulation.Rules.TwoMinuteWarning
{
    /// <summary>
    /// Implements NCAA two-minute warning rules.
    /// NCAA does not have a two-minute warning - this provider always returns false.
    /// </summary>
    public class NcaaTwoMinuteWarningRulesProvider : ITwoMinuteWarningRulesProvider
    {
        /// <inheritdoc/>
        public string Name => "NCAA";

        /// <inheritdoc/>
        public bool ShouldCallTwoMinuteWarning(
            QuarterType quarterType,
            int timeBeforePlay,
            int timeAfterPlay,
            bool alreadyCalled)
        {
            // NCAA has no two-minute warning
            return false;
        }
    }
}
