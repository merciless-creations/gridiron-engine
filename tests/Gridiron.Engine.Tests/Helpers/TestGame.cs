using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Actions;

namespace Gridiron.Engine.Tests.Helpers
{
    public class TestGame
    {
        /// <summary>
        /// Gets a game object set to the first play (kickoff)
        /// </summary>
        /// <returns></returns>
        public Game GetGame()
        {
            var rng = new SeedableRandom();
            var teams = TestTeams.CreateTestTeams();
            var game = GameHelper.GetNewGame(teams.HomeTeam, teams.VisitorTeam);
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);
            return game;
        }
    }
}