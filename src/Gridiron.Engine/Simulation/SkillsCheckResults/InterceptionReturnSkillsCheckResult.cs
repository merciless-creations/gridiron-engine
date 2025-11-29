using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates interception return yardage with potential for "pick-six" touchdowns.
    /// Base return is 8-15 yards, with skill differential and random variance affecting the outcome.
    /// </summary>
    public class InterceptionReturnSkillsCheckResult : SkillsCheckResult<InterceptionReturnResult>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _interceptor;
        private readonly List<Player> _offensePlayers;
        private readonly int _interceptionSpot;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionReturnSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining return variance.</param>
        /// <param name="interceptor">The defensive player who intercepted the pass.</param>
        /// <param name="offensePlayers">Offensive players on the field for pursuit calculation.</param>
        /// <param name="interceptionSpot">Field position where the interception occurred.</param>
        public InterceptionReturnSkillsCheckResult(
            ISeedableRandom rng,
            Player interceptor,
            List<Player> offensePlayers,
            int interceptionSpot)
        {
            _rng = rng;
            _interceptor = interceptor;
            _offensePlayers = offensePlayers;
            _interceptionSpot = interceptionSpot;
        }

        /// <summary>
        /// Executes the interception return calculation based on interceptor skill,
        /// offensive pursuit ability, and random variance.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Calculate returner's skill
            var returnerSpeed = _interceptor.Speed;
            var returnerAgility = _interceptor.Agility;
            var returnerSkill = (returnerSpeed + returnerAgility) / 2.0;

            // Calculate offensive pursuit (average of relevant offensive players)
            var pursuitSkill = _offensePlayers.Any()
                ? _offensePlayers.Average(p => p.Speed)
                : 50;

            // Skill differential affects return yardage
            var skillDiff = returnerSkill - pursuitSkill;

            // Base return: 8-15 yards
            var baseReturn = 8.0 + (_rng.NextDouble() * 7.0);

            // Skill adjustment: -15 to +35 yards based on skill differential
            var skillAdjustment = skillDiff / 2.0; // +/- 0.5 yards per skill point difference

            // Random variance: -5 to +25 yards
            var randomVariance = (_rng.NextDouble() * 30.0) - 5.0;

            // Total return yardage
            var returnYards = (int)(baseReturn + skillAdjustment + randomVariance);

            // Ensure reasonable bounds (minimum 0, maximum to end zone)
            returnYards = Math.Max(0, returnYards);

            // Calculate return distance from interception spot (moving toward offense's end zone)
            // Field position scale: 0 = offense's goal line, 100 = defense's goal line
            // Interception return moves toward offense's 0 (defense going the opposite direction)
            var maxReturnToEndZone = _interceptionSpot; // Can't return beyond 0 yard line
            returnYards = Math.Min(returnYards, maxReturnToEndZone);

            Result = new InterceptionReturnResult
            {
                Interceptor = _interceptor,
                InterceptionSpot = _interceptionSpot,
                ReturnYards = returnYards
            };
        }
    }

    /// <summary>
    /// Result of interception return calculation containing interceptor and yardage details.
    /// </summary>
    public class InterceptionReturnResult
    {
        /// <summary>
        /// Gets or sets the player who intercepted the pass.
        /// </summary>
        public Player Interceptor { get; set; }

        /// <summary>
        /// Gets or sets the field position where the interception occurred.
        /// </summary>
        public int InterceptionSpot { get; set; }

        /// <summary>
        /// Gets or sets the yards gained on the interception return.
        /// </summary>
        public int ReturnYards { get; set; }
    }
}
