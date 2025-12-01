using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain.Time
{
    /// <summary>
    /// Represents a quarter of a football game with time tracking.
    /// </summary>
    public class Quarter
    {
        private int timeRemaining;

        /// <summary>
        /// Gets the maximum duration for this quarter in seconds.
        /// Default is 900 (15 minutes), but overtime periods may have different durations.
        /// </summary>
        public int MaxDuration { get; private set; } = 900;

        /// <summary>
        /// Gets or sets the time remaining in the quarter in seconds.
        /// Value is clamped between 0 and MaxDuration.
        /// </summary>
        public int TimeRemaining
        {
            get => timeRemaining;
            set
            {
                if (value >= MaxDuration)
                {
                    timeRemaining = MaxDuration;
                    return;
                }

                if (value <= 0)
                {
                    timeRemaining = 0;
                    return;
                }

                timeRemaining = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of quarter.
        /// </summary>
        public QuarterType QuarterType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quarter"/> class.
        /// Quarter starts with 900 seconds (15 minutes) remaining.
        /// </summary>
        /// <param name="type">The type of quarter.</param>
        public Quarter(QuarterType type)
        {
            QuarterType = type;
            MaxDuration = 900;
            TimeRemaining = 900;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quarter"/> class with a custom duration.
        /// Used for overtime periods which may have different durations (e.g., NFL OT is 10 minutes).
        /// </summary>
        /// <param name="type">The type of quarter.</param>
        /// <param name="durationSeconds">The duration of the quarter in seconds.</param>
        public Quarter(QuarterType type, int durationSeconds)
        {
            QuarterType = type;
            MaxDuration = durationSeconds;
            TimeRemaining = durationSeconds;
        }
    }

    /// <summary>
    /// Defines the type of quarter in a football game.
    /// </summary>
    public enum QuarterType
    {
        /// <summary>First quarter of the game.</summary>
        First,
        /// <summary>Second quarter of the game.</summary>
        Second,
        /// <summary>Third quarter of the game.</summary>
        Third,
        /// <summary>Fourth quarter of the game.</summary>
        Fourth,
        /// <summary>Overtime period.</summary>
        Overtime,
        /// <summary>The game has ended.</summary>
        GameOver
    }
}
