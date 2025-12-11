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
    /// Determines if a field goal attempt is blocked by the defense.
    /// Block probability increases with kick distance and decreases with kicker skill.
    /// </summary>
    public class FieldGoalBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _kicker;
        private readonly int _kickDistance;
        private readonly List<Player> _offensiveLine;
        private readonly List<Player> _defensiveRushers;
        private readonly bool _goodSnap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldGoalBlockOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="kicker">The kicker attempting the field goal.</param>
        /// <param name="kickDistance">The distance of the field goal attempt in yards.</param>
        /// <param name="offensiveLine">The offensive line protecting the kick.</param>
        /// <param name="defensiveRushers">The defensive players rushing the kick.</param>
        /// <param name="goodSnap">Whether the snap was good.</param>
        public FieldGoalBlockOccurredSkillsCheck(
            ISeedableRandom rng,
            Player kicker,
            int kickDistance,
            List<Player> offensiveLine,
            List<Player> defensiveRushers,
            bool goodSnap)
        {
            _rng = rng;
            _kicker = kicker;
            _kickDistance = kickDistance;
            _offensiveLine = offensiveLine;
            _defensiveRushers = defensiveRushers;
            _goodSnap = goodSnap;
        }

        /// <summary>
        /// Executes the field goal block check to determine if the kick is blocked.
        /// Considers kick distance, kicker skill, snap quality, and defensive pressure.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Base probability by distance (longer kicks = higher trajectory = easier to block)
            double blockProbability;
            if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_SHORT)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_VERY_SHORT;
            else if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_MEDIUM)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_SHORT;
            else if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_LONG)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_MEDIUM;
            else
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_LONG;

            // Bad snap multiplier - MUCH easier to block
            if (!_goodSnap)
                blockProbability *= GameProbabilities.FieldGoals.FG_BLOCK_BAD_SNAP_MULTIPLIER;

            // Factor 1: Kicker skill (better kicker = faster operation)
            var kickerSkill = _kicker.Kicking;
            var kickerFactor = 1.0 - (kickerSkill / GameProbabilities.FieldGoals.FG_BLOCK_KICKER_SKILL_DENOMINATOR);
            blockProbability *= kickerFactor;

            // Factor 2: Defensive pressure (best rusher vs avg blocker)
            var bestRusher = _defensiveRushers
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (_offensiveLine.Count > 0 && bestRusher != null)
            {
                // Uses logarithmic curve for diminishing returns at skill extremes
                var avgBlocker = _offensiveLine.Average(p => p.Strength + p.Awareness);
                var rusherSkill = (bestRusher.Strength + bestRusher.Speed) / 2.0;
                var skillDifferential = rusherSkill - (avgBlocker / 2.0);

                blockProbability += AttributeModifier.FromDifferential(skillDifferential);
            }

            // Clamp to reasonable range
            blockProbability = Math.Max(
                GameProbabilities.FieldGoals.FG_BLOCK_MIN_CLAMP,
                Math.Min(GameProbabilities.FieldGoals.FG_BLOCK_MAX_CLAMP, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}