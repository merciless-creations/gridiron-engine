using System.Collections.Generic;
using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a scout in the football simulation responsible for evaluating players.
    /// Scouts can be college scouts, pro scouts, or directors of scouting.
    /// </summary>
    public class Scout : Person
    {
        /// <summary>
        /// Gets or sets the scouting role (e.g., Director of Scouting, College Scout, Pro Scout).
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the scout's age in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the number of years of scouting experience.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the scout's ability to evaluate player talent rating (0-100).
        /// </summary>
        public int EvaluationSkill { get; set; }

        /// <summary>
        /// Gets or sets the scout's ability to negotiate with players and agents rating (0-100).
        /// </summary>
        public int NegotiationSkill { get; set; }

        /// <summary>
        /// Gets or sets the scout's professional network and connections rating (0-100).
        /// </summary>
        public int NetworkingSkill { get; set; }

        /// <summary>
        /// Gets or sets the scout's ability to identify player potential rating (0-100).
        /// </summary>
        public int PotentialRecognition { get; set; }

        /// <summary>
        /// Gets or sets the scout's reputation in the league (0-100).
        /// </summary>
        public int Reputation { get; set; }

        /// <summary>
        /// Gets or sets the number of years remaining on the scout's contract.
        /// </summary>
        public int ContractYears { get; set; }

        /// <summary>
        /// Gets or sets the scout's annual salary.
        /// </summary>
        public int Salary { get; set; }

        /// <summary>
        /// Gets or sets the scout's career statistics.
        /// </summary>
        public Dictionary<ScoutStatType, int> Stats { get; set; } = new();
    }
}