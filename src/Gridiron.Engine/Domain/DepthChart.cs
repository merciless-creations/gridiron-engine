using System.Collections.Generic;

namespace Gridiron.Engine.Domain
{
    public class DepthChart
    {
        // For each position, a list of players ordered by depth (starter first)
        public Dictionary<Positions, List<Player>> Chart { get; set; } = new();
    }
}
