using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Base class containing common properties for all play types in the simulation.
    /// </summary>
    public class Play
    {
        /// <summary>
        /// Gets or sets the logger for play-by-play output.
        /// Defaults to NullLogger if not configured.
        /// </summary>
        public ILogger Result { get; set; } = NullLogger.Instance;

        /// <summary>
        /// Gets or sets the down on which this play occurred.
        /// </summary>
        public Downs Down { get; set; }

        /// <summary>
        /// Gets or sets whether the snap was executed cleanly.
        /// </summary>
        public bool GoodSnap { get; set; }

        /// <summary>
        /// Gets or sets whether possession changed during this play.
        /// </summary>
        public bool PossessionChange { get; set; } = false;

        /// <summary>
        /// Gets or sets which team has possession at the start of the play.
        /// </summary>
        public Possession Possession { get; set; } = Possession.None;

        /// <summary>
        /// Gets or sets the game clock time when the play started (in seconds).
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Gets or sets the game clock time when the play ended (in seconds).
        /// </summary>
        public int StopTime { get; set; }

        /// <summary>
        /// Gets or sets the type of play executed.
        /// </summary>
        public PlayType PlayType { get; set; }

        /// <summary>
        /// Gets or sets the list of penalties that occurred during this play.
        /// </summary>
        public List<Penalty> Penalties { get; set; } = new List<Penalty>();

        /// <summary>
        /// Gets or sets the list of fumbles that occurred during this play.
        /// </summary>
        public List<Fumble> Fumbles { get; set; } = new List<Fumble>();

        /// <summary>
        /// Gets or sets the list of injuries that occurred during this play.
        /// </summary>
        public List<Injury> Injuries { get; set; } = new List<Injury>();

        /// <summary>
        /// Gets or sets whether an interception occurred on this play.
        /// </summary>
        public bool Interception { get; set; } = false;

        /// <summary>
        /// Gets or sets the elapsed time during this play in seconds.
        /// </summary>
        public Double ElapsedTime { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets whether this play ended the current quarter.
        /// </summary>
        public bool QuarterExpired { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this play ended the current half.
        /// </summary>
        public bool HalfExpired { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this play ended the game.
        /// </summary>
        public bool GameExpired { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the clock was stopped at the end of this play.
        /// </summary>
        public bool ClockStopped { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of offensive players on the field for this play.
        /// </summary>
        public List<Player> OffensePlayersOnField { get; set; } = new List<Player>();

        /// <summary>
        /// Gets or sets the list of defensive players on the field for this play.
        /// </summary>
        public List<Player> DefensePlayersOnField { get; set; } = new List<Player>();

        /// <summary>
        /// Gets or sets the field position (0-100) where the play started.
        /// </summary>
        public int StartFieldPosition { get; set; } = 0;

        /// <summary>
        /// Gets or sets the field position (0-100) where the play ended.
        /// </summary>
        public int EndFieldPosition { get; set; } = 0;

        /// <summary>
        /// Gets or sets the net yards gained or lost on this play.
        /// </summary>
        public int YardsGained { get; set; } = 0;

        /// <summary>
        /// Gets or sets the yards needed for a first down at the start of this play.
        /// </summary>
        public int YardsToGo { get; set; } = 10;

        /// <summary>
        /// Gets or sets whether this play resulted in a first down.
        /// </summary>
        public bool IsFirstDown { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this play resulted in a touchdown.
        /// </summary>
        public bool IsTouchdown { get; set; } = false;

        /// <summary>
        /// Gets or sets whether a timeout was called before this play (e.g., ice the kicker).
        /// </summary>
        public bool TimeoutCalledBeforePlay { get; set; } = false;

        /// <summary>
        /// Gets or sets whether a timeout was called after this play (e.g., stop the clock).
        /// </summary>
        public bool TimeoutCalledAfterPlay { get; set; } = false;

        /// <summary>
        /// Gets or sets which team called the timeout (if any).
        /// </summary>
        public Possession? TimeoutCalledBy { get; set; } = null;
    }
}
