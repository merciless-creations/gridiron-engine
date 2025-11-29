using System;
using System.Collections.Generic;
using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a football player in the simulation with attributes, skills, and statistics.
    /// </summary>
    public class Player : Person
    {
        /// <summary>
        /// Gets or sets the player's primary position on the field.
        /// </summary>
        public Positions Position { get; set; }

        /// <summary>
        /// Gets or sets the player's jersey number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the player's height (e.g., "6'2\"").
        /// </summary>
        public string? Height { get; set; }

        /// <summary>
        /// Gets or sets the player's weight in pounds.
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets the player's age in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the player's years of professional experience.
        /// </summary>
        public int Exp { get; set; }

        /// <summary>
        /// Gets or sets the college the player attended.
        /// </summary>
        public string? College { get; set; }

        /// <summary>
        /// Gets or sets the player's current game statistics.
        /// </summary>
        public Dictionary<PlayerStatType, int> Stats { get; set; } = new();

        /// <summary>
        /// Gets or sets the player's accumulated season statistics.
        /// </summary>
        public Dictionary<PlayerStatType, int> SeasonStats { get; set; } = new();

        /// <summary>
        /// Gets or sets the player's speed rating (0-100).
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// Gets or sets the player's strength rating (0-100).
        /// </summary>
        public int Strength { get; set; }

        /// <summary>
        /// Gets or sets the player's agility rating (0-100).
        /// </summary>
        public int Agility { get; set; }

        /// <summary>
        /// Gets or sets the player's awareness rating (0-100).
        /// </summary>
        public int Awareness { get; set; }

        /// <summary>
        /// Gets or sets the player's injury proneness rating (0-100).
        /// Higher values indicate the player is more susceptible to injuries.
        /// </summary>
        public int Fragility { get; set; } = 50;

        /// <summary>
        /// Gets or sets the player's morale level (0-100).
        /// </summary>
        public int Morale { get; set; }

        /// <summary>
        /// Gets or sets the player's discipline rating (0-100).
        /// Higher values result in fewer penalties committed.
        /// </summary>
        public int Discipline { get; set; }

        /// <summary>
        /// Gets or sets the player's passing skill rating (0-100).
        /// Primarily relevant for quarterbacks.
        /// </summary>
        public int Passing { get; set; }

        /// <summary>
        /// Gets or sets the player's catching skill rating (0-100).
        /// Relevant for wide receivers, tight ends, and running backs.
        /// </summary>
        public int Catching { get; set; }

        /// <summary>
        /// Gets or sets the player's rushing skill rating (0-100).
        /// Relevant for running backs and quarterbacks.
        /// </summary>
        public int Rushing { get; set; }

        /// <summary>
        /// Gets or sets the player's blocking skill rating (0-100).
        /// Relevant for offensive linemen, tight ends, and fullbacks.
        /// </summary>
        public int Blocking { get; set; }

        /// <summary>
        /// Gets or sets the player's tackling skill rating (0-100).
        /// Relevant for defensive linemen, linebackers, safeties, and cornerbacks.
        /// </summary>
        public int Tackling { get; set; }

        /// <summary>
        /// Gets or sets the player's coverage skill rating (0-100).
        /// Relevant for cornerbacks, safeties, and linebackers.
        /// </summary>
        public int Coverage { get; set; }

        /// <summary>
        /// Gets or sets the player's kicking skill rating (0-100).
        /// Relevant for kickers and punters.
        /// </summary>
        public int Kicking { get; set; }

        /// <summary>
        /// Gets or sets the player's career statistics across all seasons.
        /// </summary>
        public Dictionary<PlayerStatType, int> CareerStats { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the player has retired from professional football.
        /// </summary>
        public bool IsRetired { get; set; }

        /// <summary>
        /// Gets or sets the number of years remaining on the player's contract.
        /// </summary>
        public int ContractYears { get; set; }

        /// <summary>
        /// Gets or sets the player's annual salary.
        /// </summary>
        public int Salary { get; set; }

        /// <summary>
        /// Gets or sets the player's potential ceiling rating (0-100).
        /// </summary>
        public int Potential { get; set; }

        /// <summary>
        /// Gets or sets the player's progression rate for skill development (0-100).
        /// </summary>
        public int Progression { get; set; }

        /// <summary>
        /// Gets or sets the player's current health level (0-100).
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Gets or sets the current active injury for this player.
        /// Null if the player is not injured.
        /// </summary>
        public Injury? CurrentInjury { get; set; }

        /// <summary>
        /// Gets a value indicating whether the player is currently injured and unavailable.
        /// </summary>
        public bool IsInjured => CurrentInjury != null;
    }
}