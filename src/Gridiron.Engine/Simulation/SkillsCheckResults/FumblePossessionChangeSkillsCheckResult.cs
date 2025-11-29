using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    public class FumblePossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        private ISeedableRandom _rng;
        public FumblePossessionChangeSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //there was a fumble - who got it?
            var toss = _rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}