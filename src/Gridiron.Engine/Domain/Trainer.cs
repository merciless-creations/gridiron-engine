using System.Collections.Generic;
using static Gridiron.Engine.Domain.StatTypes;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a trainer or medical staff member in the football simulation.
    /// Trainers include athletic trainers, team doctors, and physical therapists.
    /// </summary>
    public class Trainer : Person
    {
        /// <summary>
        /// Gets or sets the trainer's role (e.g., Head Athletic Trainer, Assistant Trainer, Team Doctor, Physical Therapist).
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the trainer's age in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the number of years of professional experience.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the trainer's medical diagnosis and treatment skill rating (0-100).
        /// </summary>
        public int MedicalSkill { get; set; }

        /// <summary>
        /// Gets or sets the trainer's rehabilitation and recovery skill rating (0-100).
        /// </summary>
        public int RehabSkill { get; set; }

        /// <summary>
        /// Gets or sets the trainer's injury prevention skill rating (0-100).
        /// </summary>
        public int PreventionSkill { get; set; }

        /// <summary>
        /// Gets or sets the trainer's nutrition and conditioning knowledge rating (0-100).
        /// </summary>
        public int NutritionSkill { get; set; }

        /// <summary>
        /// Gets or sets the trainer's reputation in the league (0-100).
        /// </summary>
        public int Reputation { get; set; }

        /// <summary>
        /// Gets or sets the number of years remaining on the trainer's contract.
        /// </summary>
        public int ContractYears { get; set; }

        /// <summary>
        /// Gets or sets the trainer's annual salary.
        /// </summary>
        public int Salary { get; set; }

        /// <summary>
        /// Gets or sets the trainer's career statistics.
        /// </summary>
        public Dictionary<TrainerStatType, int> Stats { get; set; } = new();
    }
}