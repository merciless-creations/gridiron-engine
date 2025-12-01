using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Implements NFL Playoff overtime rules.
    ///
    /// Key differences from regular season:
    /// - Unlimited overtime periods until a winner is determined
    /// - No ties allowed
    /// - Each period is 10 minutes
    /// </summary>
    public class NflPlayoffOvertimeRulesProvider : NflOvertimeRulesProviderBase
    {
        /// <inheritdoc/>
        public override string Name => "NFL Playoff";

        /// <inheritdoc/>
        public override string Description => "NFL playoff overtime rules - play until there is a winner";

        /// <inheritdoc/>
        public override bool AllowsTies => false;

        /// <inheritdoc/>
        public override int MaxOvertimePeriods => 0; // Unlimited

        /// <inheritdoc/>
        protected override OvertimePossessionResult HandlePeriodEnd(OvertimeState state)
        {
            // Playoff - always start a new period until there's a winner
            return OvertimePossessionResult.NewPeriod;
        }

        /// <inheritdoc/>
        public override bool ShouldStartNewPeriod(OvertimeState state)
        {
            // Playoffs: always start a new period if still tied
            return true;
        }
    }
}
