using System;
using System.Collections.Generic;

namespace Gridiron.Engine.Simulation.Rules.TwoMinuteWarning
{
    /// <summary>
    /// Central registry for two-minute warning rules providers.
    /// Provides lookup and singleton access to rule implementations for different leagues.
    /// </summary>
    public static class TwoMinuteWarningRulesRegistry
    {
        private static readonly NflTwoMinuteWarningRulesProvider _nfl = new();
        private static readonly NcaaTwoMinuteWarningRulesProvider _ncaa = new();

        private static readonly Dictionary<string, ITwoMinuteWarningRulesProvider> _providers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "NFL", _nfl },
                { "NCAA", _ncaa }
            };

        /// <summary>
        /// Gets the NFL two-minute warning rules provider.
        /// </summary>
        public static NflTwoMinuteWarningRulesProvider Nfl => _nfl;

        /// <summary>
        /// Gets the NCAA two-minute warning rules provider.
        /// </summary>
        public static NcaaTwoMinuteWarningRulesProvider Ncaa => _ncaa;

        /// <summary>
        /// Gets the default two-minute warning rules provider (NFL).
        /// </summary>
        public static ITwoMinuteWarningRulesProvider Default => _nfl;

        /// <summary>
        /// Gets a provider by name.
        /// Supported names: "NFL", "NCAA".
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <returns>The matching provider, or the default (NFL) if not found.</returns>
        public static ITwoMinuteWarningRulesProvider GetByName(string name)
        {
            return _providers.TryGetValue(name, out var provider) ? provider : _nfl;
        }

        /// <summary>
        /// Tries to get a provider by name.
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <param name="provider">The provider if found, null otherwise.</param>
        /// <returns>True if the provider was found, false otherwise.</returns>
        public static bool TryGetByName(string name, out ITwoMinuteWarningRulesProvider? provider)
        {
            return _providers.TryGetValue(name, out provider);
        }

        /// <summary>
        /// Gets all registered two-minute warning rules providers.
        /// </summary>
        /// <returns>A read-only list of all providers.</returns>
        public static IReadOnlyList<ITwoMinuteWarningRulesProvider> GetAll()
        {
            return new List<ITwoMinuteWarningRulesProvider> { _nfl, _ncaa };
        }

        /// <summary>
        /// Registers a custom two-minute warning rules provider.
        /// This allows external providers (like XFL, CFL) to be registered at runtime.
        /// </summary>
        /// <param name="name">The name to register the provider under.</param>
        /// <param name="provider">The provider to register.</param>
        public static void Register(string name, ITwoMinuteWarningRulesProvider provider)
        {
            _providers[name] = provider;
        }
    }
}
