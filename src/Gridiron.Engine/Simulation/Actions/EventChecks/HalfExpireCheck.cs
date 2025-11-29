using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    public sealed class HalfExpireCheck : IGameAction
    {
        public void Execute(Game game)
        {
            if (game.CurrentPlay.QuarterExpired)
            {
                switch (game.CurrentQuarter.QuarterType)
                {
                    case QuarterType.Third:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf = game.Halves[1];
                        break;
                    case QuarterType.GameOver:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf.HalfType = HalfType.GameOver;
                        break;
                }
            }

            //TODO check if tied & move to OT
            //TODO check if tied & move to another OT
        }
    }
}
