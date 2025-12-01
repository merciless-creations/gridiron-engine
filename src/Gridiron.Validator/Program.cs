using System.Diagnostics;
using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using Newtonsoft.Json;

namespace Gridiron.Validator;

/// <summary>
/// CLI tool for validating simulation statistics against NFL targets.
/// </summary>
public class Program
{
    private const int DefaultGameCount = 1000;
    private static readonly string TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

    public static int Main(string[] args)
    {
        int gameCount = DefaultGameCount;

        // Parse arguments
        if (args.Length > 0 && int.TryParse(args[0], out var parsedCount))
        {
            gameCount = parsedCount;
        }

        Console.WriteLine();
        Console.WriteLine("Gridiron Statistical Validator");
        Console.WriteLine("==============================");
        Console.WriteLine();

        try
        {
            // Load teams
            Console.Write("Loading teams...");
            var homeTeam = LoadTeam("AtlantaFalcons.json", "Atlanta", "Falcons");
            var awayTeam = LoadTeam("PhiladelphiaEagles.json", "Philadelphia", "Eagles");
            Console.WriteLine(" Done.");

            // Create engine
            var engine = new GameEngine();

            // Run simulations
            Console.WriteLine($"Simulating {gameCount:N0} games...");
            Console.WriteLine();

            var stats = new AggregateStats();
            var stopwatch = Stopwatch.StartNew();
            var lastProgressUpdate = 0;

            for (int i = 0; i < gameCount; i++)
            {
                // Alternate home/away
                var result = i % 2 == 0
                    ? engine.SimulateGame(homeTeam, awayTeam)
                    : engine.SimulateGame(awayTeam, homeTeam);

                stats.Accumulate(result.Game);

                // Progress update every 10%
                int progress = (i + 1) * 100 / gameCount;
                if (progress >= lastProgressUpdate + 10)
                {
                    lastProgressUpdate = progress;
                    Console.Write($"\r  Progress: {progress}% ({i + 1:N0}/{gameCount:N0} games)");
                }
            }

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine();

            // Generate and display report
            var report = ValidationReport.Generate(stats, stopwatch.Elapsed);
            report.PrintToConsole();

            // Return exit code based on pass/fail
            return report.FailedCount == 0 ? 0 : 1;
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Ensure test data files are in the TestData directory:");
            Console.Error.WriteLine($"  {TestDataPath}");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 3;
        }
    }

    private static Team LoadTeam(string fileName, string city, string name)
    {
        string jsonPath = Path.Combine(TestDataPath, fileName);

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException(
                $"Test data not found at: {jsonPath}. " +
                $"Ensure {fileName} exists in the TestData folder."
            );
        }

        string json = File.ReadAllText(jsonPath);
        var players = JsonConvert.DeserializeObject<List<Player>>(json);

        return new Team
        {
            City = city,
            Name = name,
            Players = players ?? new List<Player>()
        };
    }
}
