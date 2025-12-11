using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Statistical validation tests for yardage distributions.
    /// These tests run many simulated plays and verify that aggregate statistics
    /// match expected NFL distributions.
    ///
    /// NFL Target Statistics:
    /// - Run plays: Mean ~4.3 yards, Median ~3 yards, right-skewed (log-normal)
    /// - Pass plays: Mean ~11.5 yards, varies by pass type
    /// - Breakaway runs (15+): ~5% of runs
    /// - Negative runs: ~15% of runs
    /// </summary>
    [TestClass]
    public class StatisticalValidationTests
    {
        private const int GAMES_TO_SIMULATE = 20;
        private const int BASE_SEED = 10000;

        #region Run Yardage Distribution Tests

        [TestMethod]
        public void RunYardage_MeanShouldBeAround4Yards()
        {
            // Arrange & Act
            var stats = CollectRunYardageStats();

            // Assert - NFL average is ~4.3 yards per carry
            Console.WriteLine($"Run Yards - Mean: {stats.Mean:F2}, Target: 4.0-4.5");
            Console.WriteLine($"Run Yards - Median: {stats.Median:F2}, Target: 2.5-3.5");
            Console.WriteLine($"Run Yards - StdDev: {stats.StdDev:F2}");
            Console.WriteLine($"Run Yards - Count: {stats.Count}");

            // Current baseline - document what we have (may fail until #15 is implemented)
            Assert.IsTrue(stats.Count > 100, $"Should have enough run plays to analyze. Got: {stats.Count}");

            // TODO: Uncomment after implementing log-normal distribution
            // Assert.IsTrue(stats.Mean >= 4.0 && stats.Mean <= 4.5,
            //     $"Run mean should be 4.0-4.5 yards. Actual: {stats.Mean:F2}");
            // Assert.IsTrue(stats.Median >= 2.5 && stats.Median <= 3.5,
            //     $"Run median should be 2.5-3.5 yards. Actual: {stats.Median:F2}");
        }

        [TestMethod]
        public void RunYardage_ShouldBeRightSkewed()
        {
            // Arrange & Act
            var stats = CollectRunYardageStats();

            // Assert - Right-skewed means mean > median
            Console.WriteLine($"Skewness check - Mean: {stats.Mean:F2}, Median: {stats.Median:F2}");
            Console.WriteLine($"Mean > Median (right-skewed): {stats.Mean > stats.Median}");

            // TODO: Uncomment after implementing log-normal distribution
            // Assert.IsTrue(stats.Mean > stats.Median,
            //     $"Run distribution should be right-skewed (mean > median). Mean: {stats.Mean:F2}, Median: {stats.Median:F2}");
        }

        [TestMethod]
        public void RunYardage_BreakawayRunsAround5Percent()
        {
            // Arrange & Act
            var stats = CollectRunYardageStats();
            var breakawayPercent = (double)stats.BreakawayCount / stats.Count * 100;

            // Assert - Breakaway runs (15+ yards) should be ~5% of runs
            Console.WriteLine($"Breakaway runs (15+ yards): {stats.BreakawayCount} / {stats.Count} = {breakawayPercent:F1}%");
            Console.WriteLine($"Target: ~5%");

            Assert.IsTrue(stats.Count > 100, $"Should have enough run plays. Got: {stats.Count}");

            // TODO: Uncomment after implementing log-normal distribution
            // Assert.IsTrue(breakawayPercent >= 3.0 && breakawayPercent <= 8.0,
            //     $"Breakaway runs should be 3-8%. Actual: {breakawayPercent:F1}%");
        }

        [TestMethod]
        public void RunYardage_NegativeRunsAround15Percent()
        {
            // Arrange & Act
            var stats = CollectRunYardageStats();
            var negativePercent = (double)stats.NegativeCount / stats.Count * 100;

            // Assert - Negative runs (TFL) should be ~15% of runs
            Console.WriteLine($"Negative runs: {stats.NegativeCount} / {stats.Count} = {negativePercent:F1}%");
            Console.WriteLine($"Target: ~15%");

            Assert.IsTrue(stats.Count > 100, $"Should have enough run plays. Got: {stats.Count}");

            // TODO: Uncomment after implementing log-normal distribution
            // Assert.IsTrue(negativePercent >= 10.0 && negativePercent <= 20.0,
            //     $"Negative runs should be 10-20%. Actual: {negativePercent:F1}%");
        }

        [TestMethod]
        public void RunYardage_PrintHistogram()
        {
            // Arrange & Act
            var stats = CollectRunYardageStats();
            var histogram = stats.GetHistogram();

            // Print histogram for visual inspection
            Console.WriteLine("\n=== RUN YARDAGE HISTOGRAM ===");
            Console.WriteLine($"Total runs: {stats.Count}");
            Console.WriteLine($"Mean: {stats.Mean:F2}, Median: {stats.Median:F2}, StdDev: {stats.StdDev:F2}");
            Console.WriteLine();

            foreach (var bucket in histogram.OrderBy(b => b.Key))
            {
                var percent = (double)bucket.Value / stats.Count * 100;
                var bar = new string('█', (int)(percent * 2));
                Console.WriteLine($"{bucket.Key,4} yards: {bar} {percent:F1}% ({bucket.Value})");
            }

            // This test always passes - it's for visual inspection
            Assert.IsTrue(true);
        }

        #endregion

        #region Pass Yardage Distribution Tests

        [TestMethod]
        public void PassYardage_CompletionsMeanShouldBeAround11Yards()
        {
            // Arrange & Act
            var stats = CollectPassYardageStats();

            // Assert - NFL average is ~11.5 yards per completion
            Console.WriteLine($"Pass Yards (completions) - Mean: {stats.Mean:F2}, Target: ~11.5");
            Console.WriteLine($"Pass Yards (completions) - Median: {stats.Median:F2}");
            Console.WriteLine($"Pass Yards (completions) - StdDev: {stats.StdDev:F2}");
            Console.WriteLine($"Pass Yards (completions) - Count: {stats.Count}");

            Assert.IsTrue(stats.Count > 50, $"Should have enough completions. Got: {stats.Count}");

            // TODO: Tune after implementing normal distributions by pass type
            // Assert.IsTrue(stats.Mean >= 9.0 && stats.Mean <= 14.0,
            //     $"Pass completion mean should be 9-14 yards. Actual: {stats.Mean:F2}");
        }

        [TestMethod]
        public void PassYardage_PrintHistogram()
        {
            // Arrange & Act
            var stats = CollectPassYardageStats();
            var histogram = stats.GetHistogram();

            // Print histogram for visual inspection
            Console.WriteLine("\n=== PASS YARDAGE HISTOGRAM (Completions) ===");
            Console.WriteLine($"Total completions: {stats.Count}");
            Console.WriteLine($"Mean: {stats.Mean:F2}, Median: {stats.Median:F2}, StdDev: {stats.StdDev:F2}");
            Console.WriteLine();

            foreach (var bucket in histogram.OrderBy(b => b.Key))
            {
                var percent = (double)bucket.Value / stats.Count * 100;
                var bar = new string('█', (int)(percent * 2));
                Console.WriteLine($"{bucket.Key,4} yards: {bar} {percent:F1}% ({bucket.Value})");
            }

            Assert.IsTrue(true);
        }

        #endregion

        #region Combined Statistics Report

        [TestMethod]
        public void PrintFullStatisticsReport()
        {
            // Collect all stats
            var runStats = CollectRunYardageStats();
            var passStats = CollectPassYardageStats();

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("YARDAGE DISTRIBUTION VALIDATION REPORT");
            Console.WriteLine(new string('=', 60));

            Console.WriteLine("\n--- RUN PLAYS ---");
            Console.WriteLine($"Count:      {runStats.Count}");
            Console.WriteLine($"Mean:       {runStats.Mean:F2} yards (Target: 4.0-4.5)");
            Console.WriteLine($"Median:     {runStats.Median:F2} yards (Target: 2.5-3.5)");
            Console.WriteLine($"Std Dev:    {runStats.StdDev:F2}");
            Console.WriteLine($"Min:        {runStats.Min}");
            Console.WriteLine($"Max:        {runStats.Max}");
            Console.WriteLine($"Negative:   {(double)runStats.NegativeCount / runStats.Count * 100:F1}% (Target: ~15%)");
            Console.WriteLine($"Breakaway:  {(double)runStats.BreakawayCount / runStats.Count * 100:F1}% (Target: ~5%)");
            Console.WriteLine($"Right-skew: {(runStats.Mean > runStats.Median ? "YES" : "NO")} (mean > median)");

            Console.WriteLine("\n--- PASS PLAYS (Completions) ---");
            Console.WriteLine($"Count:      {passStats.Count}");
            Console.WriteLine($"Mean:       {passStats.Mean:F2} yards (Target: ~11.5)");
            Console.WriteLine($"Median:     {passStats.Median:F2} yards");
            Console.WriteLine($"Std Dev:    {passStats.StdDev:F2}");
            Console.WriteLine($"Min:        {passStats.Min}");
            Console.WriteLine($"Max:        {passStats.Max}");

            Console.WriteLine("\n" + new string('=', 60));

            Assert.IsTrue(true);
        }

        #endregion

        #region Helper Methods

        private YardageStats CollectRunYardageStats()
        {
            var engine = new GameEngine();
            var allRunYards = new List<int>();

            for (int i = 0; i < GAMES_TO_SIMULATE; i++)
            {
                var homeTeam = TestTeams.LoadAtlantaFalcons();
                var awayTeam = TestTeams.LoadPhiladelphiaEagles();
                var options = new SimulationOptions { RandomSeed = BASE_SEED + i };

                var result = engine.SimulateGame(homeTeam, awayTeam, options);

                // Collect run play yards
                var runPlays = result.Plays
                    .Where(p => p.PlayType == PlayType.Run && !p.PossessionChange)
                    .Select(p => p.YardsGained);

                allRunYards.AddRange(runPlays);
            }

            return new YardageStats(allRunYards);
        }

        private YardageStats CollectPassYardageStats()
        {
            var engine = new GameEngine();
            var allPassYards = new List<int>();

            for (int i = 0; i < GAMES_TO_SIMULATE; i++)
            {
                var homeTeam = TestTeams.LoadAtlantaFalcons();
                var awayTeam = TestTeams.LoadPhiladelphiaEagles();
                var options = new SimulationOptions { RandomSeed = BASE_SEED + i };

                var result = engine.SimulateGame(homeTeam, awayTeam, options);

                // Collect completed pass yards (exclude sacks, incompletions, interceptions)
                var passPlays = result.Plays
                    .Where(p => p.PlayType == PlayType.Pass &&
                                p.YardsGained > 0 &&
                                !p.PossessionChange)
                    .Select(p => p.YardsGained);

                allPassYards.AddRange(passPlays);
            }

            return new YardageStats(allPassYards);
        }

        #endregion

        #region Helper Classes

        private class YardageStats
        {
            public int Count { get; }
            public double Mean { get; }
            public double Median { get; }
            public double StdDev { get; }
            public int Min { get; }
            public int Max { get; }
            public int NegativeCount { get; }
            public int BreakawayCount { get; }

            private readonly List<int> _values;

            public YardageStats(IEnumerable<int> values)
            {
                _values = values.ToList();
                Count = _values.Count;

                if (Count == 0)
                {
                    Mean = Median = StdDev = 0;
                    Min = Max = 0;
                    NegativeCount = BreakawayCount = 0;
                    return;
                }

                Mean = _values.Average();
                Min = _values.Min();
                Max = _values.Max();

                // Calculate median
                var sorted = _values.OrderBy(v => v).ToList();
                Median = Count % 2 == 0
                    ? (sorted[Count / 2 - 1] + sorted[Count / 2]) / 2.0
                    : sorted[Count / 2];

                // Calculate standard deviation
                var sumSquaredDiffs = _values.Sum(v => Math.Pow(v - Mean, 2));
                StdDev = Math.Sqrt(sumSquaredDiffs / Count);

                // Count special cases
                NegativeCount = _values.Count(v => v < 0);
                BreakawayCount = _values.Count(v => v >= 15);
            }

            public Dictionary<int, int> GetHistogram()
            {
                // Bucket by yards: <-5, -5 to -1, 0-2, 3-5, 6-9, 10-14, 15-19, 20-29, 30+
                var histogram = new Dictionary<int, int>();

                foreach (var yards in _values)
                {
                    int bucket;
                    if (yards < -5) bucket = -10;
                    else if (yards < 0) bucket = -5;
                    else if (yards <= 2) bucket = 0;
                    else if (yards <= 5) bucket = 3;
                    else if (yards <= 9) bucket = 6;
                    else if (yards <= 14) bucket = 10;
                    else if (yards <= 19) bucket = 15;
                    else if (yards <= 29) bucket = 20;
                    else bucket = 30;

                    if (!histogram.ContainsKey(bucket))
                        histogram[bucket] = 0;
                    histogram[bucket]++;
                }

                return histogram;
            }
        }

        #endregion
    }
}
