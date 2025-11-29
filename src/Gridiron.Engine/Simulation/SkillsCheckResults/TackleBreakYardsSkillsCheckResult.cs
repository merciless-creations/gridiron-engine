using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates extra yards gained when a ball carrier breaks a tackle.
    /// Typically adds 3-8 yards to the run.
    /// </summary>
    public class TackleBreakYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="TackleBreakYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining tackle break yardage.</param>
        public TackleBreakYardsSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Executes the calculation to determine extra yardage gained when breaking a tackle.
        /// Adds 3-8 yards to the run when the ball carrier successfully breaks a tackle.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Tackle break adds 3-8 yards
            Result = _rng.Next(3, 9);
        }
    }
}
