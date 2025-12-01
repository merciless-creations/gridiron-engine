using System;
using System.Collections.Generic;

namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Central registry for overtime rules providers.
    /// Similar to PenaltyRegistry, provides lookup and singleton access to providers.
    /// </summary>
    public static class OvertimeRulesRegistry
    {
        private static readonly NflRegularSeasonOvertimeRulesProvider _nflRegularSeason = new();
        private static readonly NflPlayoffOvertimeRulesProvider _nflPlayoff = new();

        private static readonly Dictionary<string, IOvertimeRulesProvider> _providers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "NFL", _nflRegularSeason },
            { "NFL_REGULAR", _nflRegularSeason },
            { "NFL_REGULAR_SEASON", _nflRegularSeason },
            { "NFL_PLAYOFF", _nflPlayoff },
            { "NFL_PLAYOFFS", _nflPlayoff }
        };

        /// <summary>
        /// Gets the NFL Regular Season overtime rules provider.
        /// </summary>
        public static NflRegularSeasonOvertimeRulesProvider NflRegularSeason => _nflRegularSeason;

        /// <summary>
        /// Gets the NFL Playoff overtime rules provider.
        /// </summary>
        public static NflPlayoffOvertimeRulesProvider NflPlayoff => _nflPlayoff;

        /// <summary>
        /// Gets the default overtime rules provider (NFL Regular Season).
        /// </summary>
        public static IOvertimeRulesProvider Default => _nflRegularSeason;

        /// <summary>
        /// Gets a provider by name.
        /// Supported names: "NFL", "NFL_REGULAR", "NFL_REGULAR_SEASON", "NFL_PLAYOFF", "NFL_PLAYOFFS".
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <returns>The matching provider, or the default (NFL Regular Season) if not found.</returns>
        public static IOvertimeRulesProvider GetByName(string name)
        {
            return _providers.TryGetValue(name, out var provider)
                ? provider
                : _nflRegularSeason;
        }

        /// <summary>
        /// Tries to get a provider by name.
        /// </summary>
        /// <param name="name">The name of the provider (case-insensitive).</param>
        /// <param name="provider">The provider if found, null otherwise.</param>
        /// <returns>True if the provider was found, false otherwise.</returns>
        public static bool TryGetByName(string name, out IOvertimeRulesProvider? provider)
        {
            return _providers.TryGetValue(name, out provider);
        }

        /// <summary>
        /// Gets all registered overtime rules providers.
        /// </summary>
        /// <returns>A read-only list of all providers.</returns>
        public static IReadOnlyList<IOvertimeRulesProvider> GetAll()
        {
            return new List<IOvertimeRulesProvider> { _nflRegularSeason, _nflPlayoff };
        }

        /// <summary>
        /// Registers a custom overtime rules provider.
        /// This allows external providers (like NCAA) to be registered at runtime.
        /// </summary>
        /// <param name="name">The name to register the provider under.</param>
        /// <param name="provider">The provider to register.</param>
        public static void Register(string name, IOvertimeRulesProvider provider)
        {
            _providers[name] = provider;
        }
    }
}
