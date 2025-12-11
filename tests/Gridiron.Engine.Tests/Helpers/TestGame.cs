using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Actions;

namespace Gridiron.Engine.Tests.Helpers
{
    public class TestGame
    {
        /// <summary>
        /// Gets a game object set to the first play (kickoff).
        /// Uses a fixed seed for deterministic test results.
        /// </summary>
        /// <param name="seed">RNG seed for deterministic behavior (default: 12345)</param>
        /// <returns>A game object ready for testing</returns>
        public Game GetGame(int seed = 12345)
        {
            var rng = new SeedableRandom(seed);
            var teams = TestTeams.CreateTestTeams();
            var game = GameHelper.GetNewGame(teams.HomeTeam, teams.VisitorTeam);
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);
            return game;
        }
    }
}