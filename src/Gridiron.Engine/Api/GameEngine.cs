using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation;
using Gridiron.Engine.Simulation.Overtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gridiron.Engine.Api
{
    /// <summary>
    /// Default implementation of IGameEngine.
    /// Wraps the internal GameFlow state machine with a clean, simple API.
    /// </summary>
    public class GameEngine : IGameEngine
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Creates a new GameEngine with default settings (no logging).
        /// </summary>
        public GameEngine() : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Creates a new GameEngine with the specified logger factory.
        /// </summary>
        /// <param name="loggerFactory">Factory for creating loggers</param>
        public GameEngine(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        /// <inheritdoc />
        public GameResult SimulateGame(Team homeTeam, Team awayTeam, SimulationOptions? options = null)
        {
            if (homeTeam == null) throw new ArgumentNullException(nameof(homeTeam));
            if (awayTeam == null) throw new ArgumentNullException(nameof(awayTeam));

            options ??= new SimulationOptions();

            // Create the game object
            var game = new Game
            {
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                RandomSeed = options.RandomSeed
            };

            // Create RNG (seeded or random)
            ISeedableRandom rng = options.RandomSeed.HasValue
                ? new SeedableRandom(options.RandomSeed.Value)
                : new SeedableRandom();

            // Get or create logger
            var logger = options.Logger as ILogger<GameFlow>
                ?? _loggerFactory.CreateLogger<GameFlow>();

            // Build depth charts if not already set
            DepthChartBuilder.AssignAllDepthCharts(homeTeam);
            DepthChartBuilder.AssignAllDepthCharts(awayTeam);

            // Get overtime rules provider (default to NFL Regular Season)
            var overtimeRules = options.OvertimeRulesProvider ?? OvertimeRulesRegistry.Default;

            // Run the simulation
            var gameFlow = new GameFlow(game, rng, logger, overtimeRules);
            gameFlow.Execute();

            return new GameResult { Game = game };
        }
    }
}
