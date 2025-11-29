using Gridiron.Engine.Domain;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.BaseClasses
{
    /// <summary>
    /// Base class for skills check results that return a typed value.
    /// Use this when a skills check determines not just IF something happened,
    /// but also WHAT the outcome was (e.g., yards gained, player selected).
    /// </summary>
    /// <typeparam name="T">The type of result this skills check returns</typeparam>
    public abstract class SkillsCheckResult<T> : IGameAction
    {
        /// <summary>
        /// The result of the skills check calculation.
        /// </summary>
        public T Result { get; protected set; }

        /// <summary>
        /// Executes the action against the specified game state.
        /// </summary>
        /// <param name="game">The game state to execute the action against.</param>
        public abstract void Execute(Game game);
    }
}
