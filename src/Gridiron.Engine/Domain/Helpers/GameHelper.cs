namespace Gridiron.Engine.Domain.Helpers
{
    /// <summary>
    /// Static helper class for creating and initializing game instances.
    /// </summary>
    public static class GameHelper
    {
        /// <summary>
        /// Creates a new game with the provided teams
        /// </summary>
        public static Game GetNewGame(Team homeTeam, Team awayTeam)
        {
            return new Game()
            {
                HomeTeam = homeTeam,
                AwayTeam = awayTeam
            };
        }
    }
}