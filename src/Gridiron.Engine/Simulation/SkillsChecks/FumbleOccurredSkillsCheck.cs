using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if a fumble occurs during a play.
    /// Fumble probability is affected by ball carrier security, defender pressure, and play type.
    /// </summary>
    public class FumbleOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _ballCarrier;
        private readonly List<Player> _defenders;
        private readonly PlayType _playType;
        private readonly bool _isQBSack;

        /// <summary>
        /// Initializes a new instance of the <see cref="FumbleOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="ballCarrier">The player carrying the ball.</param>
        /// <param name="defenders">The defenders involved in the tackle.</param>
        /// <param name="playType">The type of play.</param>
        /// <param name="isQBSack">Whether this is a quarterback sack.</param>
        public FumbleOccurredSkillsCheck(
            ISeedableRandom rng,
            Player ballCarrier,
            List<Player> defenders,
            PlayType playType,
            bool isQBSack = false)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
            _defenders = defenders;
            _playType = playType;
            _isQBSack = isQBSack;
        }

        /// <summary>
        /// Executes the fumble check to determine if a fumble occurs.
        /// Considers ball carrier awareness (security), defender pressure, and number of tacklers.
        /// Gang tackles increase fumble probability.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            double fumbleProbability;

            // Base probability by play type
            if (_isQBSack)
            {
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_QB_SACK_PROBABILITY;
            }
            else if (_playType == PlayType.Kickoff || _playType == PlayType.Punt)
            {
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_RETURN_PROBABILITY;
            }
            else
            {
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_NORMAL_PROBABILITY;
            }

            // Ball carrier security factor
            // Use Awareness as proxy for ball security (higher = better)
            // Uses logarithmic curve for diminishing returns at skill extremes
            var carrierSecurity = _ballCarrier.Awareness; // 0-100
            // Negative modifier reduces fumble chance (good security), positive increases it
            var securityModifier = -AttributeModifier.Calculate(carrierSecurity);
            fumbleProbability *= (1.0 + securityModifier);

            // Defensive pressure factor
            // Find best defender (highest strength + speed)
            var bestDefender = _defenders
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (bestDefender != null)
            {
                // Uses logarithmic curve for diminishing returns at skill extremes
                var defenderPressure = (bestDefender.Strength + bestDefender.Speed) / 2.0; // 0-100
                // Positive modifier increases fumble chance (good defender)
                var pressureModifier = AttributeModifier.Calculate(defenderPressure);
                fumbleProbability *= (1.0 + pressureModifier);
            }

            // Number of defenders (gang tackles increase fumbles)
            if (_defenders.Count >= 3)
                fumbleProbability *= GameProbabilities.Turnovers.FUMBLE_GANG_TACKLE_MULTIPLIER;
            else if (_defenders.Count >= 2)
                fumbleProbability *= GameProbabilities.Turnovers.FUMBLE_TWO_DEFENDERS_MULTIPLIER;

            // Clamp to reasonable range
            fumbleProbability = Math.Max(
                GameProbabilities.Turnovers.FUMBLE_MIN_CLAMP,
                Math.Min(GameProbabilities.Turnovers.FUMBLE_MAX_CLAMP, fumbleProbability));

            Occurred = _rng.NextDouble() < fumbleProbability;
        }
    }
}