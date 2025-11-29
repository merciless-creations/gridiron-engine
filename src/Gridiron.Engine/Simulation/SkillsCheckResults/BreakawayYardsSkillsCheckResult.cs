using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates extra yards gained when a ball carrier breaks into the open field.
    /// Typically adds 15-44 yards on big run breakaways.
    /// </summary>
    public class BreakawayYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakawayYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining breakaway yardage.</param>
        public BreakawayYardsSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the calculation to determine extra yardage gained on a breakaway run.
        /// Adds 15-44 yards to represent the ball carrier breaking into open field.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Breakaway run adds 15-44 yards
            Result = _rng.Next(15, 45);
        }
    }
}
