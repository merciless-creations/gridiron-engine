using Gridiron.Engine.Domain.Time;

namespace Gridiron.Engine.Simulation.Rules.TwoMinuteWarning
{
    /// <summary>
    /// Implements NFL two-minute warning rules.
    /// The two-minute warning occurs when the game clock crosses the 2:00 mark
    /// in the 2nd and 4th quarters. It is an automatic timeout that stops the clock.
    /// </summary>
    public class NflTwoMinuteWarningRulesProvider : ITwoMinuteWarningRulesProvider
    {
        /// <inheritdoc/>
        public string Name => "NFL";

        /// <inheritdoc/>
        public bool ShouldCallTwoMinuteWarning(
            QuarterType quarterType,
            int timeBeforePlay,
            int timeAfterPlay,
            bool alreadyCalled)
        {
            // Only in 2nd and 4th quarters
            if (quarterType != QuarterType.Second && quarterType != QuarterType.Fourth)
            {
                return false;
            }

            // Already called this quarter - don't call again
            if (alreadyCalled)
            {
                return false;
            }

            // Check if we just crossed the 2:00 (120 seconds) threshold
            // timeBeforePlay was > 120, timeAfterPlay is <= 120
            return timeBeforePlay > 120 && timeAfterPlay <= 120;
        }
    }
}
