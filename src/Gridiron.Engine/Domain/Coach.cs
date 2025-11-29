using System.Collections.Generic;
using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a coach in the football simulation, including head coaches and coordinators.
    /// </summary>
    public class Coach : Person
    {
        /// <summary>
        /// Gets or sets the coaching role (e.g., Head Coach, Offensive Coordinator, Defensive Coordinator, Special Teams Coordinator).
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the coach's age in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the number of years of coaching experience.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the coach's leadership ability rating (0-100).
        /// </summary>
        public int Leadership { get; set; }

        /// <summary>
        /// Gets or sets the coach's strategic planning ability rating (0-100).
        /// </summary>
        public int Strategy { get; set; }

        /// <summary>
        /// Gets or sets the coach's ability to motivate players rating (0-100).
        /// </summary>
        public int Motivation { get; set; }

        /// <summary>
        /// Gets or sets the coach's ability to adapt to situations rating (0-100).
        /// </summary>
        public int Adaptability { get; set; }

        /// <summary>
        /// Gets or sets the coach's career statistics.
        /// </summary>
        public Dictionary<CoachStatType, int> Stats { get; set; } = new();

        /// <summary>
        /// Gets or sets the coach's offensive play-calling and scheme design skill (0-100).
        /// Primarily relevant for offensive coordinators and head coaches with offensive backgrounds.
        /// </summary>
        public int OffensiveSkill { get; set; }

        /// <summary>
        /// Gets or sets the coach's defensive scheme and play-calling skill (0-100).
        /// Primarily relevant for defensive coordinators and head coaches with defensive backgrounds.
        /// </summary>
        public int DefensiveSkill { get; set; }

        /// <summary>
        /// Gets or sets the coach's special teams management skill (0-100).
        /// Primarily relevant for special teams coordinators.
        /// </summary>
        public int SpecialTeamsSkill { get; set; }

        /// <summary>
        /// Gets or sets the coach's reputation in the league (0-100).
        /// </summary>
        public int Reputation { get; set; }

        /// <summary>
        /// Gets or sets the number of years remaining on the coach's contract.
        /// </summary>
        public int ContractYears { get; set; }

        /// <summary>
        /// Gets or sets the coach's annual salary.
        /// </summary>
        public int Salary { get; set; }
    }
}