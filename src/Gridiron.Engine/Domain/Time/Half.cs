using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain.Time
{
    /// <summary>
    /// Represents a half of a football game, containing two quarters.
    /// </summary>
    public abstract class Half
    {
        /// <summary>
        /// Gets the list of quarters in this half (always two quarters).
        /// </summary>
        public List<Quarter> Quarters { get; private set; }

        /// <summary>
        /// Gets the total time remaining in this half in seconds.
        /// Calculated by summing the time remaining in both quarters.
        /// </summary>
        public int TimeRemaining => Quarters[0].TimeRemaining + Quarters[1].TimeRemaining;

        /// <summary>
        /// Gets or sets the type of half (First or Second).
        /// </summary>
        public HalfType HalfType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Half"/> class with the specified half type.
        /// Creates two quarters appropriate for the half type.
        /// </summary>
        /// <param name="type">The type of half to create.</param>
        protected Half(HalfType type)
        {
            HalfType = type;
            Quarters = new List<Quarter>
            {
                new Quarter(type == HalfType.First ? QuarterType.First : QuarterType.Third),
                new Quarter(type == HalfType.First ? QuarterType.Second : QuarterType.Fourth)
            };
        }
    }

    /// <summary>
    /// Defines the type of half in a football game.
    /// </summary>
    public enum HalfType
    {
        /// <summary>The first half of the game (quarters 1 and 2).</summary>
        First,
        /// <summary>The second half of the game (quarters 3 and 4).</summary>
        Second,
        /// <summary>The game has ended.</summary>
        GameOver
    }
}
