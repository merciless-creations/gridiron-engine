using System;

namespace Gridiron.Engine.Domain.Helpers
{
    /// <summary>
    /// Interface for a seedable random number generator, allowing deterministic random sequences.
    /// Provides the same API as cryptographic random sources for compatibility.
    /// </summary>
    public interface ISeedableRandom : IDisposable
    {
        /// <summary>
        /// Fills the specified buffer with random bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill with random bytes.</param>
        void GetBytes(byte[] buffer);

        /// <summary>
        /// Returns a random double-precision floating point number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random double between 0.0 (inclusive) and 1.0 (exclusive).</returns>
        double NextDouble();

        /// <summary>
        /// Returns a random integer within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound.</param>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer within the specified range.</returns>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>A non-negative random integer.</returns>
        int Next();

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A non-negative random integer less than maxValue.</returns>
        int Next(int maxValue);

        /// <summary>
        /// Fills the specified buffer with non-zero random bytes.
        /// </summary>
        /// <param name="data">The buffer to fill with non-zero random bytes.</param>
        void GetNonZeroBytes(byte[] data);
    }

    /// <summary>
    /// A seedable random number generator for deterministic and testable simulation.
    /// Wraps the standard .NET Random class with a consistent seed-based initialization.
    /// </summary>
    public class SeedableRandom : ISeedableRandom
    {
        private readonly Random _internal;

        /// <summary>
        /// Initializes a new instance with the specified integer seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        public SeedableRandom(int seed) => _internal = new Random(seed);

        /// <summary>
        /// Initializes a new instance using a GUID as the seed source.
        /// </summary>
        /// <param name="guidSeed">The GUID to use as the seed source.</param>
        public SeedableRandom(Guid guidSeed) => _internal = new Random(guidSeed.GetHashCode());

        /// <summary>
        /// Initializes a new instance with a time-based seed.
        /// </summary>
        public SeedableRandom() => _internal = new Random();

        /// <inheritdoc/>
        public double NextDouble() => _internal.NextDouble();

        /// <inheritdoc/>
        public int Next(int minValue, int maxValue) => _internal.Next(minValue, maxValue);

        /// <inheritdoc/>
        public int Next() => _internal.Next();

        /// <inheritdoc/>
        public int Next(int maxValue) => _internal.Next(maxValue);

        /// <inheritdoc/>
        public void GetBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)_internal.Next(0, 256);
        }

        /// <inheritdoc/>
        public void GetNonZeroBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // Generate random non-zero byte
                byte b = 0;
                while (b == 0)
                    b = (byte)_internal.Next(1, 256);
                data[i] = b;
            }
        }

        /// <inheritdoc/>
        public void Dispose() { /* nothing to dispose */ }
    }
}