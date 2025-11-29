using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;

namespace Gridiron.Engine.Api
{
    /// <summary>
    /// Public API for simulating football games.
    /// This is the primary entry point for consuming the Gridiron Engine.
    /// </summary>
    public interface IGameEngine
    {
        /// <summary>
        /// Simulates a complete game between two teams.
        /// </summary>
        /// <param name="homeTeam">The home team with full roster and depth charts</param>
        /// <param name="awayTeam">The away team with full roster and depth charts</param>
        /// <param name="options">Optional simulation configuration</param>
        /// <returns>The result of the simulated game</returns>
        GameResult SimulateGame(Team homeTeam, Team awayTeam, SimulationOptions? options = null);
    }

    /// <summary>
    /// Configuration options for game simulation.
    /// </summary>
    public class SimulationOptions
    {
        /// <summary>
        /// Random seed for reproducible simulations. If null, uses system random.
        /// </summary>
        public int? RandomSeed { get; set; }

        /// <summary>
        /// Logger for play-by-play output. If null, no logging is performed.
        /// </summary>
        public ILogger? Logger { get; set; }
    }

    /// <summary>
    /// Result of a simulated game containing the final game state and statistics.
    /// </summary>
    public class GameResult
    {
        /// <summary>
        /// The complete game object with final scores, plays, and statistics.
        /// </summary>
        public required Game Game { get; init; }

        /// <summary>
        /// Final score for the home team.
        /// </summary>
        public int HomeScore => Game.HomeScore;

        /// <summary>
        /// Final score for the away team.
        /// </summary>
        public int AwayScore => Game.AwayScore;

        /// <summary>
        /// The home team with updated player statistics.
        /// </summary>
        public Team HomeTeam => Game.HomeTeam;

        /// <summary>
        /// The away team with updated player statistics.
        /// </summary>
        public Team AwayTeam => Game.AwayTeam;

        /// <summary>
        /// All plays executed during the game.
        /// </summary>
        public IReadOnlyList<IPlay> Plays => Game.Plays;

        /// <summary>
        /// Total number of plays in the game.
        /// </summary>
        public int TotalPlays => Game.Plays.Count;

        /// <summary>
        /// Whether the game ended in a tie (overtime not yet implemented).
        /// </summary>
        public bool IsTie => HomeScore == AwayScore;

        /// <summary>
        /// The winning team, or null if tied.
        /// </summary>
        public Team? Winner => IsTie ? null : (HomeScore > AwayScore ? HomeTeam : AwayTeam);

        /// <summary>
        /// The random seed used for this simulation (for replay purposes).
        /// </summary>
        public int? RandomSeed => Game.RandomSeed;
    }
}
