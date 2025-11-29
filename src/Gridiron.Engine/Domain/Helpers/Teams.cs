using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Domain.Helpers
{
    /// <summary>
    /// Container for home and visitor teams with automatic depth chart building.
    /// Initializes both teams' depth charts when created.
    /// </summary>
    public class Teams
    {
        /// <summary>
        /// Gets or sets the home team.
        /// </summary>
        public Team HomeTeam { get; set; }

        /// <summary>
        /// Gets or sets the visiting team.
        /// </summary>
        public Team VisitorTeam { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Teams"/> class with the specified teams.
        /// Automatically builds depth charts for both teams using <see cref="DepthChartBuilder"/>.
        /// </summary>
        /// <param name="homeTeam">The home team.</param>
        /// <param name="awayTeam">The away/visiting team.</param>
        public Teams(Team homeTeam, Team awayTeam)
        {
            HomeTeam = homeTeam;
            VisitorTeam = awayTeam;

            // Build depth charts for both teams using centralized builder
            DepthChartBuilder.AssignAllDepthCharts(HomeTeam);
            DepthChartBuilder.AssignAllDepthCharts(VisitorTeam);
        }
    }
}
