using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Interfaces
{
    /// <summary>
    /// Represents an action that can be executed within the context of a game.
    /// This interface follows the Command pattern, allowing actions to be encapsulated
    /// and executed on a game instance.
    /// </summary>
    public interface IGameAction
    {
        /// <summary>
        /// Executes the action on the specified game.
        /// </summary>
        /// <param name="game">The game instance on which to execute the action.</param>
        void Execute(Game game);
    }
}
