using Gridiron.Engine.Domain;
using Gridiron.Engine.Simulation.Interfaces;

namespace Gridiron.Engine.Simulation.BaseClasses
{
    /// <summary>
    /// Base class for skills checks that determine whether a specific action occurred.
    /// Use this when a skills check needs to determine IF something happened (e.g., fumble occurred, interception occurred).
    /// </summary>
    public abstract class ActionOccurredSkillsCheck : IGameAction
    {
        /// <summary>
        /// Gets a value indicating whether the action occurred as a result of the skills check.
        /// </summary>
        public bool Occurred { get; private protected set; } = false;

        /// <summary>
        /// Represents the margin of success or failure for narrative purposes.
        /// Positive values indicate decisive success, negative values indicate decisive failure.
        /// Zero indicates an even matchup or that margin was not calculated.
        /// </summary>
        public double Margin { get; protected set; } = 0.0;

        /// <summary>
        /// Executes the action against the specified game state.
        /// </summary>
        /// <param name="game">The game state to execute the action against.</param>
        public abstract void Execute(Game game);
    }
}
