using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a football team with players, coaches, and staff.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Gets or sets the team's name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the city where the team is based.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the list of players on the team's roster.
        /// </summary>
        public List<Player> Players { get; set; } = new();

        /// <summary>
        /// Gets or sets the team's salary cap budget.
        /// </summary>
        public int Budget { get; set; }

        /// <summary>
        /// Gets or sets the number of championships the team has won.
        /// </summary>
        public int Championships { get; set; }

        /// <summary>
        /// Gets or sets the team's total wins.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Gets or sets the team's total losses.
        /// </summary>
        public int Losses { get; set; }

        /// <summary>
        /// Gets or sets the team's total ties.
        /// </summary>
        public int Ties { get; set; }

        /// <summary>
        /// Gets or sets the team's fan support level (0-100).
        /// </summary>
        public int FanSupport { get; set; }

        /// <summary>
        /// Gets or sets the team's chemistry level (0-100).
        /// </summary>
        public int Chemistry { get; set; }

        /// <summary>
        /// Gets or sets the team's statistics.
        /// </summary>
        public Dictionary<TeamStatType, int> Stats { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for the offensive unit.
        /// </summary>
        public DepthChart OffenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for the defensive unit.
        /// </summary>
        public DepthChart DefenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for field goal offensive personnel.
        /// </summary>
        public DepthChart FieldGoalOffenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for field goal defensive personnel.
        /// </summary>
        public DepthChart FieldGoalDefenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for kickoff coverage personnel.
        /// </summary>
        public DepthChart KickoffOffenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for kickoff return personnel.
        /// </summary>
        public DepthChart KickoffDefenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for punt coverage personnel.
        /// </summary>
        public DepthChart PuntOffenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the depth chart for punt return personnel.
        /// </summary>
        public DepthChart PuntDefenseDepthChart { get; set; } = new();

        /// <summary>
        /// Gets or sets the head coach. May be null if the position is unfilled.
        /// </summary>
        public Coach? HeadCoach { get; set; }

        /// <summary>
        /// Gets or sets the offensive coordinator. May be null if the position is unfilled.
        /// </summary>
        public Coach? OffensiveCoordinator { get; set; }

        /// <summary>
        /// Gets or sets the defensive coordinator. May be null if the position is unfilled.
        /// </summary>
        public Coach? DefensiveCoordinator { get; set; }

        /// <summary>
        /// Gets or sets the special teams coordinator. May be null if the position is unfilled.
        /// </summary>
        public Coach? SpecialTeamsCoordinator { get; set; }

        /// <summary>
        /// Gets or sets the list of assistant coaches.
        /// </summary>
        public List<Coach> AssistantCoaches { get; set; } = new();

        /// <summary>
        /// Gets or sets the head athletic trainer. May be null if the position is unfilled.
        /// </summary>
        public Trainer? HeadAthleticTrainer { get; set; }

        /// <summary>
        /// Gets or sets the team doctor. May be null if the position is unfilled.
        /// </summary>
        public Trainer? TeamDoctor { get; set; }

        /// <summary>
        /// Gets or sets the director of scouting. May be null if the position is unfilled.
        /// </summary>
        public Scout? DirectorOfScouting { get; set; }

        /// <summary>
        /// Gets or sets the list of college scouts.
        /// </summary>
        public List<Scout> CollegeScouts { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of pro scouts.
        /// </summary>
        public List<Scout> ProScouts { get; set; } = new();

        /// <summary>
        /// Gets or sets additional team statistics by name.
        /// </summary>
        public Dictionary<string, int> TeamStats { get; set; } = new();
    }
}
