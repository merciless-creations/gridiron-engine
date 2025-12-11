using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Unit tests for StatisticalDistributions utility class.
    /// Tests verify that distribution functions produce expected statistical properties.
    /// </summary>
    [TestClass]
    public class StatisticalDistributionsTests
    {
        private const int SAMPLE_SIZE = 10000;
        private const int SEED = 42;

        #region Normal Distribution Tests

        [TestMethod]
        public void Normal_MeanIsCorrect()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<double>();
            const double expectedMean = 10.0;
            const double stdDev = 2.0;

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.Normal(rng, expectedMean, stdDev));
            }

            var actualMean = samples.Average();

            // Assert - mean should be within 1% of expected
            Console.WriteLine($"Normal distribution - Expected mean: {expectedMean}, Actual: {actualMean:F3}");
            Assert.IsTrue(Math.Abs(actualMean - expectedMean) < 0.1,
                $"Mean should be close to {expectedMean}. Actual: {actualMean:F3}");
        }

        [TestMethod]
        public void Normal_StdDevIsCorrect()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<double>();
            const double mean = 0.0;
            const double expectedStdDev = 5.0;

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.Normal(rng, mean, expectedStdDev));
            }

            var actualMean = samples.Average();
            var variance = samples.Sum(x => Math.Pow(x - actualMean, 2)) / samples.Count;
            var actualStdDev = Math.Sqrt(variance);

            // Assert - stddev should be within 5% of expected
            Console.WriteLine($"Normal distribution - Expected stddev: {expectedStdDev}, Actual: {actualStdDev:F3}");
            Assert.IsTrue(Math.Abs(actualStdDev - expectedStdDev) < 0.3,
                $"StdDev should be close to {expectedStdDev}. Actual: {actualStdDev:F3}");
        }

        [TestMethod]
        public void Normal_IsDeterministicWithSameSeed()
        {
            // Arrange
            var rng1 = new SeedableRandom(12345);
            var rng2 = new SeedableRandom(12345);

            // Act
            var value1 = StatisticalDistributions.Normal(rng1, 10.0, 2.0);
            var value2 = StatisticalDistributions.Normal(rng2, 10.0, 2.0);

            // Assert
            Assert.AreEqual(value1, value2, "Same seed should produce same value");
        }

        #endregion

        #region Log-Normal Distribution Tests

        [TestMethod]
        public void LogNormal_AlwaysPositive()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);

            // Act & Assert
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                var value = StatisticalDistributions.LogNormal(rng, 1.0, 0.5);
                Assert.IsTrue(value > 0, $"Log-normal should always be positive. Got: {value}");
            }
        }

        [TestMethod]
        public void LogNormal_IsRightSkewed()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<double>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.LogNormal(rng, 1.0, 0.5));
            }

            var mean = samples.Average();
            var sorted = samples.OrderBy(x => x).ToList();
            var median = sorted[SAMPLE_SIZE / 2];

            // Assert - right-skewed means mean > median
            Console.WriteLine($"Log-normal - Mean: {mean:F3}, Median: {median:F3}");
            Assert.IsTrue(mean > median, $"Log-normal should be right-skewed (mean > median). Mean: {mean:F3}, Median: {median:F3}");
        }

        #endregion

        #region Run Yards Distribution Tests

        [TestMethod]
        public void RunYards_ProducesRealisticDistribution()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.RunYards(rng, 0.0));
            }

            var mean = samples.Average();
            var sorted = samples.OrderBy(x => x).ToList();
            var median = sorted[SAMPLE_SIZE / 2];
            var negativePercent = (double)samples.Count(x => x < 0) / SAMPLE_SIZE * 100;
            var breakawayPercent = (double)samples.Count(x => x >= 15) / SAMPLE_SIZE * 100;

            // Print stats
            Console.WriteLine($"\n=== RUN YARDS DISTRIBUTION (StatisticalDistributions) ===");
            Console.WriteLine($"Mean:       {mean:F2} (Target: 4.0-4.5)");
            Console.WriteLine($"Median:     {median} (Target: 2.5-3.5)");
            Console.WriteLine($"Negative:   {negativePercent:F1}% (Target: ~15%)");
            Console.WriteLine($"Breakaway:  {breakawayPercent:F1}% (Target: ~5%)");
            Console.WriteLine($"Min:        {samples.Min()}");
            Console.WriteLine($"Max:        {samples.Max()}");

            // Assert basic sanity
            Assert.IsTrue(mean > 2.0 && mean < 7.0, $"Mean should be realistic. Got: {mean:F2}");
            Assert.IsTrue(samples.Min() < 0, "Should have some negative runs");
            Assert.IsTrue(samples.Max() > 15, "Should have some breakaway runs");
        }

        [TestMethod]
        public void RunYards_SkillModifierAffectsYards()
        {
            // Arrange
            var rng1 = new SeedableRandom(SEED);
            var rng2 = new SeedableRandom(SEED);
            var lowSkillSamples = new List<int>();
            var highSkillSamples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                lowSkillSamples.Add(StatisticalDistributions.RunYards(rng1, -0.5));
                highSkillSamples.Add(StatisticalDistributions.RunYards(rng2, 0.5));
            }

            var lowMean = lowSkillSamples.Average();
            var highMean = highSkillSamples.Average();

            // Assert
            Console.WriteLine($"Low skill mean: {lowMean:F2}, High skill mean: {highMean:F2}");
            Assert.IsTrue(highMean > lowMean, $"High skill should produce more yards. Low: {lowMean:F2}, High: {highMean:F2}");
        }

        [TestMethod]
        public void RunYards_PrintHistogram()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.RunYards(rng, 0.0));
            }

            // Create histogram
            var histogram = new Dictionary<int, int>();
            foreach (var yards in samples)
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

            // Print
            Console.WriteLine("\n=== RUN YARDS HISTOGRAM (New Distribution) ===");
            foreach (var bucket in histogram.OrderBy(b => b.Key))
            {
                var percent = (double)bucket.Value / SAMPLE_SIZE * 100;
                var bar = new string('█', (int)(percent * 2));
                var label = bucket.Key switch
                {
                    -10 => "< -5",
                    -5 => "-5 to -1",
                    0 => " 0 to  2",
                    3 => " 3 to  5",
                    6 => " 6 to  9",
                    10 => "10 to 14",
                    15 => "15 to 19",
                    20 => "20 to 29",
                    30 => "30+",
                    _ => bucket.Key.ToString()
                };
                Console.WriteLine($"{label,10}: {bar} {percent:F1}%");
            }

            Assert.IsTrue(true);
        }

        #endregion

        #region Pass Yards Distribution Tests

        [TestMethod]
        public void PassYards_ScreenPass_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.PassYards(rng, StatisticalDistributions.PassType.Screen, 0.0));
            }

            var mean = samples.Average();

            // Assert
            Console.WriteLine($"Screen pass mean: {mean:F2} (Target: ~4)");
            Assert.IsTrue(mean > 2.0 && mean < 6.0, $"Screen pass mean should be ~4. Got: {mean:F2}");
        }

        [TestMethod]
        public void PassYards_ShortPass_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.PassYards(rng, StatisticalDistributions.PassType.Short, 0.0));
            }

            var mean = samples.Average();

            // Assert
            Console.WriteLine($"Short pass mean: {mean:F2} (Target: ~7)");
            Assert.IsTrue(mean > 5.0 && mean < 9.0, $"Short pass mean should be ~7. Got: {mean:F2}");
        }

        [TestMethod]
        public void PassYards_MediumPass_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.PassYards(rng, StatisticalDistributions.PassType.Medium, 0.0));
            }

            var mean = samples.Average();

            // Assert
            Console.WriteLine($"Medium pass mean: {mean:F2} (Target: ~14)");
            Assert.IsTrue(mean > 11.0 && mean < 17.0, $"Medium pass mean should be ~14. Got: {mean:F2}");
        }

        [TestMethod]
        public void PassYards_DeepPass_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.PassYards(rng, StatisticalDistributions.PassType.Deep, 0.0));
            }

            var mean = samples.Average();

            // Assert
            Console.WriteLine($"Deep pass mean: {mean:F2} (Target: ~30)");
            Assert.IsTrue(mean > 25.0 && mean < 35.0, $"Deep pass mean should be ~30. Got: {mean:F2}");
        }

        [TestMethod]
        public void PassYards_AlwaysAtLeastOneYard()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);

            // Act & Assert
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                var yards = StatisticalDistributions.PassYards(rng, StatisticalDistributions.PassType.Screen, 0.0);
                Assert.IsTrue(yards >= 1, $"Pass completions should be at least 1 yard. Got: {yards}");
            }
        }

        #endregion

        #region Sack Yards Tests

        [TestMethod]
        public void SackYards_AlwaysNegative()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);

            // Act & Assert
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                var yards = StatisticalDistributions.SackYards(rng);
                Assert.IsTrue(yards < 0, $"Sack yards should always be negative. Got: {yards}");
            }
        }

        [TestMethod]
        public void SackYards_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.SackYards(rng));
            }

            var mean = samples.Average();

            // Assert - mean should be around -7 yards
            Console.WriteLine($"Sack yards mean: {mean:F2} (Target: ~-7)");
            Assert.IsTrue(mean > -9.0 && mean < -5.0, $"Sack mean should be ~-7. Got: {mean:F2}");
        }

        #endregion

        #region TFL Yards Tests

        [TestMethod]
        public void TackleForLossYards_AlwaysNegative()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);

            // Act & Assert
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                var yards = StatisticalDistributions.TackleForLossYards(rng);
                Assert.IsTrue(yards < 0, $"TFL yards should always be negative. Got: {yards}");
            }
        }

        [TestMethod]
        public void TackleForLossYards_MeanAroundTarget()
        {
            // Arrange
            var rng = new SeedableRandom(SEED);
            var samples = new List<int>();

            // Act
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(StatisticalDistributions.TackleForLossYards(rng));
            }

            var mean = samples.Average();

            // Assert - mean should be around -2 yards
            Console.WriteLine($"TFL yards mean: {mean:F2} (Target: ~-2)");
            Assert.IsTrue(mean > -4.0 && mean < -1.0, $"TFL mean should be ~-2. Got: {mean:F2}");
        }

        #endregion
    }
}
