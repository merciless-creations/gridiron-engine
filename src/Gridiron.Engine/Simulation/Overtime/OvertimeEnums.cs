namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Types of scores that can occur in overtime.
    /// </summary>
    public enum OvertimeScoreType
    {
        /// <summary>Touchdown scored by offense (6 points).</summary>
        Touchdown,

        /// <summary>Field goal scored by offense (3 points).</summary>
        FieldGoal,

        /// <summary>Safety scored by defense (2 points).</summary>
        Safety,

        /// <summary>Defensive touchdown (interception/fumble return for TD).</summary>
        DefensiveTouchdown
    }

    /// <summary>
    /// Reasons a possession can end without scoring.
    /// </summary>
    public enum PossessionEndReason
    {
        /// <summary>Team punted the ball.</summary>
        Punt,

        /// <summary>Team failed to convert on 4th down.</summary>
        TurnoverOnDowns,

        /// <summary>Pass was intercepted.</summary>
        Interception,

        /// <summary>Ball was fumbled and recovered by defense.</summary>
        Fumble,

        /// <summary>Field goal attempt was missed.</summary>
        MissedFieldGoal,

        /// <summary>Overtime period clock expired.</summary>
        TimeExpired
    }

    /// <summary>
    /// Result of checking if the overtime game should end after a score.
    /// </summary>
    public enum OvertimeGameEndResult
    {
        /// <summary>Game continues - possession changes or play continues.</summary>
        Continue,

        /// <summary>Game is over - there is a winner.</summary>
        GameOver,

        /// <summary>Current overtime period ends, may start new one.</summary>
        PeriodOver,

        /// <summary>Game ends in a tie (NFL regular season only after one OT).</summary>
        TieGame
    }

    /// <summary>
    /// Result of a possession ending in overtime.
    /// </summary>
    public enum OvertimePossessionResult
    {
        /// <summary>Normal possession change - other team gets the ball.</summary>
        OtherTeamGetsBall,

        /// <summary>Switch to sudden death mode - next score wins.</summary>
        SuddenDeath,

        /// <summary>Start a new overtime period.</summary>
        NewPeriod,

        /// <summary>Game is over (tie or winner determined).</summary>
        GameOver
    }
}
