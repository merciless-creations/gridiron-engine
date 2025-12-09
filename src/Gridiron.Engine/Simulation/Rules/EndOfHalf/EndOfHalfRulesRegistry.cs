using System;
using System.Collections.Generic;

namespace Gridiron.Engine.Simulation.Rules.EndOfHalf
{
    /// <summary>
    /// Central registry for end-of-half rules providers.
    /// Provides lookup and singleton access to rule implementations for different leagues.
    /// </summary>
    public static class EndOfHalfRulesRegistry
    {
        private static readonly NflEndOfHalfRulesProvider _nfl = new();
        private static readonly NcaaEndOfHalfRulesProvider _ncaa = new();

        private static readonly Dictionary<string, IEndOfHalfRulesProvider> _providers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "NFL", _nfl },
                { "NCAA", _ncaa }
            };

        /// <summary>
        /// Gets the NFL end-of-half rules provider.
        /// </summary>
        public static NflEndOfHalfRulesProvider Nfl => _nfl;

        /// <summary>
        /// Gets the NCAA end-of-half rules provider.
        /// </summary>
        public static NcaaEndOfHalfRulesProvider Ncaa => _ncaa;

        /// <summary>
        /// Gets the default end-of-half rules provider (NFL).
        /// </summary>
        public static IEndOfHalfRulesProvider Default => _nfl;

        /// <summary>
        /// Gets a provider by name.
        /// Supported names: "NFL", "NCAA".
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <returns>The matching provider, or the default (NFL) if not found.</returns>
        public static IEndOfHalfRulesProvider GetByName(string name)
        {
            return _providers.TryGetValue(name, out var provider) ? provider : _nfl;
        }

        /// <summary>
        /// Tries to get a provider by name.
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <param name="provider">The provider if found, null otherwise.</param>
        /// <returns>True if the provider was found, false otherwise.</returns>
        public static bool TryGetByName(string name, out IEndOfHalfRulesProvider? provider)
        {
            return _providers.TryGetValue(name, out provider);
        }

        /// <summary>
        /// Gets all registered end-of-half rules providers.
        /// </summary>
        /// <returns>A read-only list of all providers.</returns>
        public static IReadOnlyList<IEndOfHalfRulesProvider> GetAll()
        {
            return new List<IEndOfHalfRulesProvider> { _nfl, _ncaa };
        }

        /// <summary>
        /// Registers a custom end-of-half rules provider.
        /// This allows external providers (like XFL, CFL) to be registered at runtime.
        /// </summary>
        /// <param name="name">The name to register the provider under.</param>
        /// <param name="provider">The provider to register.</param>
        public static void Register(string name, IEndOfHalfRulesProvider provider)
        {
            _providers[name] = provider;
        }
    }
}
