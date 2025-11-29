using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    public class InterceptionPossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        public override void Execute(Game game)
        {
            //we know that an interception has occurred - so we change possession
            Possession = game.CurrentPlay.Possession == Possession.Away ? Possession.Home : Possession.Away;
        }
    }
}