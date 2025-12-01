using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Defines the contract for overtime rules implementations.
    /// Different leagues (NFL, NCAA, etc.) have distinct overtime rules
    /// that can be swapped via this provider interface.
    /// </summary>
    public interface IOvertimeRulesProvider
    {
        // ==================== Identity ====================

        /// <summary>
        /// Gets the name of the ruleset (e.g., "NFL Regular Season", "NFL Playoff", "NCAA").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a description of the overtime format.
        /// </summary>
        string Description { get; }

        // ==================== Configuration ====================

        /// <summary>
        /// Gets the duration of each overtime period in seconds.
        /// NFL: 600 (10 minutes), NCAA: 0 (untimed possessions).
        /// </summary>
        int OvertimePeriodDuration { get; }

        /// <summary>
        /// Gets the number of timeouts per team in overtime.
        /// NFL: 2, NCAA: 1.
        /// </summary>
        int TimeoutsPerTeam { get; }

        /// <summary>
        /// Gets the fixed starting field position for possessions.
        /// Returns null if kickoff is used (NFL), or field position value (NCAA: 75 = 25-yard line).
        /// </summary>
        int? FixedStartingFieldPosition { get; }

        /// <summary>
        /// Gets whether a coin toss occurs at the start of overtime.
        /// </summary>
        bool HasOvertimeCoinToss { get; }

        /// <summary>
        /// Gets whether ties are allowed.
        /// NFL regular season: true (after one OT), NFL playoffs: false, NCAA: false.
        /// </summary>
        bool AllowsTies { get; }

        /// <summary>
        /// Gets the maximum number of overtime periods.
        /// 0 = unlimited (NFL playoffs), 1 = NFL regular season.
        /// </summary>
        int MaxOvertimePeriods { get; }

        // ==================== Game State Queries ====================

        /// <summary>
        /// Determines if the game should end immediately after a score.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <param name="scoreType">Type of score that occurred.</param>
        /// <param name="scoringTeam">Team that scored.</param>
        /// <returns>Result indicating whether game ends, continues, or ties.</returns>
        OvertimeGameEndResult ShouldGameEnd(OvertimeState state, OvertimeScoreType scoreType, Possession scoringTeam);

        /// <summary>
        /// Determines what happens after a possession ends without scoring.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <param name="reason">Reason the possession ended.</param>
        /// <returns>Result indicating next action (possession change, sudden death, etc.).</returns>
        OvertimePossessionResult GetNextPossessionAction(OvertimeState state, PossessionEndReason reason);

        /// <summary>
        /// Determines if a new overtime period should start.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <returns>True if a new period should start.</returns>
        bool ShouldStartNewPeriod(OvertimeState state);

        /// <summary>
        /// Gets the starting field position for the next possession.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <param name="possession">Team receiving possession.</param>
        /// <returns>Field position (0-100 scale).</returns>
        int GetStartingFieldPosition(OvertimeState state, Possession possession);

        /// <summary>
        /// Gets the down and distance for starting a possession.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <returns>Tuple of (down, yardsToGo).</returns>
        (Downs down, int yardsToGo) GetStartingDownAndDistance(OvertimeState state);

        // ==================== Special Rules ====================

        /// <summary>
        /// Determines if a two-point conversion is required after a touchdown.
        /// NCAA: Required starting 3rd OT.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <returns>True if two-point conversion is required.</returns>
        bool IsTwoPointConversionRequired(OvertimeState state);

        /// <summary>
        /// Determines if only two-point conversion plays are allowed (no regular plays).
        /// NCAA: Starting 4th OT.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <returns>True if only two-point plays are allowed.</returns>
        bool IsTwoPointPlayOnly(OvertimeState state);

        /// <summary>
        /// Determines if kickoff is used or possession starts at a fixed position.
        /// </summary>
        /// <param name="state">Current overtime state.</param>
        /// <returns>True if kickoff is used, false if possession starts at fixed position.</returns>
        bool UsesKickoff(OvertimeState state);
    }
}
