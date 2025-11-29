using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Interfaces
{
    public interface IGameAction
    {
        void Execute(Game game);
    }
}
