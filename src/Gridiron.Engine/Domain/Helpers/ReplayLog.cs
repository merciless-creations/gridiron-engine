using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Gridiron.Engine.Domain.Helpers
{
    /// <summary>
    /// Represents a log of random values generated during game simulation for replay purposes.
    /// Allows deterministic replay of games by recording and replaying random number sequences.
    /// </summary>
    public class ReplayLog
    {
        /// <summary>
        /// Gets or sets the seed used to initialize the random number generator.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Gets or sets the list of double-precision random values generated.
        /// </summary>
        public List<double> Doubles { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the list of integer random values generated.
        /// </summary>
        public List<int> Ints { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the list of random integer range entries generated.
        /// </summary>
        public List<RandomIntRangeEntry> IntRanges { get; set; } = new List<RandomIntRangeEntry>();

        /// <summary>
        /// Saves the replay log to a JSON file at the specified path.
        /// </summary>
        /// <param name="path">The file path to save the replay log to.</param>
        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads a replay log from a JSON file at the specified path.
        /// </summary>
        /// <param name="path">The file path to load the replay log from.</param>
        /// <returns>The deserialized replay log.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public static ReplayLog Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ReplayLog>(json) ?? throw new InvalidOperationException("Failed to deserialize ReplayLog from the provided JSON.");
        }
    }

    /// <summary>
    /// Represents a single random integer value generated within a specified range.
    /// </summary>
    public class RandomIntRangeEntry
    {
        /// <summary>
        /// Gets or sets the minimum value of the range (inclusive).
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the range (exclusive).
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Gets or sets the generated random value within the range.
        /// </summary>
        public int Value { get; set; }
    }
}