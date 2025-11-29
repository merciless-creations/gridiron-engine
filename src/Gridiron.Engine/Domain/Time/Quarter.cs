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
        /// Gets or sets the time remaining in the quarter in seconds.
        /// Value is clamped between 0 and 900 (15 minutes).
        /// </summary>
        [RangeAttribute(0, 900, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int TimeRemaining
        {
            get => timeRemaining;
            set
            {
                if (value >= 900)
                {
                    timeRemaining = 900;
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
            TimeRemaining = 900;
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
