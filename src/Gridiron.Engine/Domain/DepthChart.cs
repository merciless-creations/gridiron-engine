using System.Collections.Generic;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a team's depth chart, organizing players by position and priority order.
    /// </summary>
    public class DepthChart
    {
        /// <summary>
        /// Gets or sets the depth chart mapping positions to lists of players.
        /// For each position, players are ordered by depth (starter first, then backups).
        /// </summary>
        public Dictionary<Positions, List<Player>> Chart { get; set; } = new();
    }
}
