namespace Gridiron.Validator;

/// <summary>
/// Result of validating a single statistical target.
/// </summary>
public class ValidationResult
{
    public required string Category { get; init; }
    public required string Metric { get; init; }
    public required double Target { get; init; }
    public required double MinTarget { get; init; }
    public required double MaxTarget { get; init; }
    public required double Actual { get; init; }
    public required double Tolerance { get; init; }
    public required bool Passed { get; init; }

    public double Deviation => Target != 0 ? (Actual - Target) / Target * 100 : 0;
    public double MinWithTolerance => MinTarget * (1 - Tolerance);
    public double MaxWithTolerance => MaxTarget * (1 + Tolerance);
}

/// <summary>
/// Complete validation report comparing simulation results against targets.
/// </summary>
public class ValidationReport
{
    public int GamesSimulated { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public TimeSpan SimulationDuration { get; init; }
    public List<ValidationResult> Results { get; init; } = new();

    public int PassedCount => Results.Count(r => r.Passed);
    public int FailedCount => Results.Count(r => !r.Passed);
    public double PassRate => Results.Count > 0 ? (double)PassedCount / Results.Count * 100 : 0;

    public IEnumerable<ValidationResult> FailedResults => Results.Where(r => !r.Passed);
    public IEnumerable<ValidationResult> PassedResults => Results.Where(r => r.Passed);

    /// <summary>
    /// Generate a validation report from aggregate stats.
    /// </summary>
    public static ValidationReport Generate(AggregateStats stats, TimeSpan duration)
    {
        var results = new List<ValidationResult>();

        foreach (var target in StatisticalTargets.All)
        {
            var actual = target.GetActualValue(stats);
            results.Add(new ValidationResult
            {
                Category = target.Category,
                Metric = target.Metric,
                Target = target.Target,
                MinTarget = target.MinTarget,
                MaxTarget = target.MaxTarget,
                Actual = actual,
                Tolerance = target.Tolerance,
                Passed = target.IsWithinRange(actual)
            });
        }

        return new ValidationReport
        {
            GamesSimulated = stats.GamesSimulated,
            SimulationDuration = duration,
            Results = results
        };
    }

    /// <summary>
    /// Print the report to console.
    /// </summary>
    public void PrintToConsole()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    GRIDIRON STATISTICAL VALIDATION REPORT                    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"  Games Simulated: {GamesSimulated:N0}");
        Console.WriteLine($"  Duration: {SimulationDuration.TotalSeconds:F1} seconds ({SimulationDuration.TotalMilliseconds / GamesSimulated:F1} ms/game)");
        Console.WriteLine($"  Generated: {GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();
        Console.WriteLine($"  Overall: {PassedCount}/{Results.Count} passed ({PassRate:F1}%)");
        Console.WriteLine();

        // Group by category
        var categories = Results.GroupBy(r => r.Category).OrderBy(g => g.Key);

        foreach (var category in categories)
        {
            Console.WriteLine($"  ┌─ {category.Key} ─────────────────────────────────────────────────────────────");
            Console.WriteLine("  │");

            foreach (var result in category)
            {
                var status = result.Passed ? "✓" : "✗";
                var color = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;
                var deviation = result.Deviation >= 0 ? $"+{result.Deviation:F1}%" : $"{result.Deviation:F1}%";

                Console.Write("  │ ");
                Console.ForegroundColor = color;
                Console.Write($"[{status}] ");
                Console.ResetColor();
                Console.Write($"{result.Metric,-30}");
                Console.Write($"Target: {result.MinTarget:F1}-{result.MaxTarget:F1}  ");
                Console.Write($"Actual: ");
                Console.ForegroundColor = color;
                Console.Write($"{result.Actual:F2}");
                Console.ResetColor();
                Console.WriteLine($"  ({deviation})");
            }

            Console.WriteLine("  │");
        }

        Console.WriteLine("  └──────────────────────────────────────────────────────────────────────────────");
        Console.WriteLine();

        if (FailedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠ Some targets were not met. Review the simulation parameters.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ All statistical targets met!");
            Console.ResetColor();
        }

        Console.WriteLine();
    }
}
