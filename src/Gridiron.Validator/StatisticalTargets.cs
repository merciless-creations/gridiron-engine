namespace Gridiron.Validator;

/// <summary>
/// Defines a statistical target with acceptable range.
/// </summary>
public class StatisticalTarget
{
    public string Category { get; init; } = "";
    public string Metric { get; init; } = "";
    public double MinTarget { get; init; }
    public double MaxTarget { get; init; }
    public double Tolerance { get; init; } = 0.05; // 5% default
    public Func<AggregateStats, double> GetActualValue { get; init; } = _ => 0;

    public double Target => (MinTarget + MaxTarget) / 2;

    public bool IsWithinRange(double actual)
    {
        return actual >= MinTarget * (1 - Tolerance) && actual <= MaxTarget * (1 + Tolerance);
    }
}

/// <summary>
/// All statistical targets from statistical-targets.md
/// </summary>
public static class StatisticalTargets
{
    public static readonly List<StatisticalTarget> All = new()
    {
        // ===== RUSHING =====
        new StatisticalTarget
        {
            Category = "Rushing",
            Metric = "Yards per carry",
            MinTarget = 4.0,
            MaxTarget = 4.6,
            GetActualValue = s => s.YardsPerCarry
        },
        new StatisticalTarget
        {
            Category = "Rushing",
            Metric = "Negative plays %",
            MinTarget = 13,
            MaxTarget = 17,
            GetActualValue = s => s.NegativePlayRate
        },
        new StatisticalTarget
        {
            Category = "Rushing",
            Metric = "10+ yard runs %",
            MinTarget = 10,
            MaxTarget = 14,
            GetActualValue = s => s.TenPlusYardRate
        },
        new StatisticalTarget
        {
            Category = "Rushing",
            Metric = "20+ yard runs %",
            MinTarget = 2,
            MaxTarget = 4,
            GetActualValue = s => s.TwentyPlusYardRate
        },
        new StatisticalTarget
        {
            Category = "Rushing",
            Metric = "Fumble rate %",
            MinTarget = 0.5,
            MaxTarget = 1.1,
            GetActualValue = s => s.RushFumbleRate
        },

        // ===== PASSING =====
        new StatisticalTarget
        {
            Category = "Passing",
            Metric = "Completion %",
            MinTarget = 64,
            MaxTarget = 67,
            GetActualValue = s => s.CompletionPercentage
        },
        new StatisticalTarget
        {
            Category = "Passing",
            Metric = "Yards per attempt",
            MinTarget = 7.0,
            MaxTarget = 7.5,
            GetActualValue = s => s.YardsPerAttempt
        },
        new StatisticalTarget
        {
            Category = "Passing",
            Metric = "Interception rate %",
            MinTarget = 2.0,
            MaxTarget = 2.5,
            GetActualValue = s => s.InterceptionRate
        },
        new StatisticalTarget
        {
            Category = "Passing",
            Metric = "TD rate %",
            MinTarget = 4.5,
            MaxTarget = 5.0,
            GetActualValue = s => s.PassTouchdownRate
        },

        // ===== TURNOVERS =====
        new StatisticalTarget
        {
            Category = "Turnovers",
            Metric = "Per game (team)",
            MinTarget = 1.2,
            MaxTarget = 1.4,
            GetActualValue = s => s.TurnoversPerGame
        },

        // ===== PENALTIES =====
        new StatisticalTarget
        {
            Category = "Penalties",
            Metric = "Per game (team)",
            MinTarget = 5.5,
            MaxTarget = 6.5,
            GetActualValue = s => s.PenaltiesPerGame / 2 // Divide by 2 for per-team
        },
        new StatisticalTarget
        {
            Category = "Penalties",
            Metric = "Yards per game",
            MinTarget = 48,
            MaxTarget = 55,
            GetActualValue = s => s.PenaltyYardsPerGame / 2 // Divide by 2 for per-team
        },

        // ===== THIRD DOWN =====
        new StatisticalTarget
        {
            Category = "Third Down",
            Metric = "Conversion % (overall)",
            MinTarget = 38,
            MaxTarget = 42,
            GetActualValue = s => s.ThirdDownConversionRate
        },

        // ===== FOURTH DOWN =====
        new StatisticalTarget
        {
            Category = "Fourth Down",
            Metric = "Conversion %",
            MinTarget = 52,
            MaxTarget = 58,
            GetActualValue = s => s.FourthDownConversionRate
        },

        // ===== GAME FLOW =====
        new StatisticalTarget
        {
            Category = "Game Flow",
            Metric = "Plays per game",
            MinTarget = 130,
            MaxTarget = 140,
            GetActualValue = s => s.PlaysPerGame
        },
        new StatisticalTarget
        {
            Category = "Game Flow",
            Metric = "Points per game (team)",
            MinTarget = 21,
            MaxTarget = 24,
            GetActualValue = s => s.PointsPerGame
        },

        // ===== PUNTING =====
        new StatisticalTarget
        {
            Category = "Punting",
            Metric = "Gross yards per punt",
            MinTarget = 45,
            MaxTarget = 47,
            GetActualValue = s => s.GrossYardsPerPunt
        },

        // ===== KICKOFFS =====
        new StatisticalTarget
        {
            Category = "Kickoffs",
            Metric = "Touchback %",
            MinTarget = 55,
            MaxTarget = 65,
            GetActualValue = s => s.TouchbackRate
        },

        // ===== OVERTIME =====
        new StatisticalTarget
        {
            Category = "Overtime",
            Metric = "Overtime rate %",
            MinTarget = 5,
            MaxTarget = 7,
            GetActualValue = s => s.OvertimeRate
        },

        // ===== INJURIES =====
        new StatisticalTarget
        {
            Category = "Injuries",
            Metric = "Per game (both teams)",
            MinTarget = 0.6,
            MaxTarget = 1.5,
            GetActualValue = s => s.InjuriesPerGame
        }
    };
}
