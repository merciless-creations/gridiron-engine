using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Determines possession change when an interception occurs.
    /// Always changes possession to the opposing team.
    /// </summary>
    public class InterceptionPossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        /// <summary>
        /// Executes the possession change, switching possession from the current offensive team
        /// to the defensive team that intercepted the pass.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            //we know that an interception has occurred - so we change possession
            Possession = game.CurrentPlay.Possession == Possession.Away ? Possession.Home : Possession.Away;
        }
    }
}