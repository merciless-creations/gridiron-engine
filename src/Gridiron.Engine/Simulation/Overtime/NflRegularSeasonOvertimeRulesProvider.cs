using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Implements NFL Regular Season overtime rules.
    ///
    /// Key differences from playoff:
    /// - Only one 10-minute overtime period
    /// - Can end in a tie if still tied after one period
    /// </summary>
    public class NflRegularSeasonOvertimeRulesProvider : NflOvertimeRulesProviderBase
    {
        /// <inheritdoc/>
        public override string Name => "NFL Regular Season";

        /// <inheritdoc/>
        public override string Description => "NFL regular season overtime rules - one 10-minute period, can end in tie";

        /// <inheritdoc/>
        public override bool AllowsTies => true;

        /// <inheritdoc/>
        public override int MaxOvertimePeriods => 1;

        /// <inheritdoc/>
        protected override OvertimePossessionResult HandlePeriodEnd(OvertimeState state)
        {
            // Regular season - one period only, can end in tie
            if (state.CurrentPeriod >= MaxOvertimePeriods)
            {
                return OvertimePossessionResult.GameOver; // Will be a tie
            }

            return OvertimePossessionResult.NewPeriod;
        }

        /// <inheritdoc/>
        public override bool ShouldStartNewPeriod(OvertimeState state)
        {
            // Regular season: only one OT period
            return false;
        }
    }
}
