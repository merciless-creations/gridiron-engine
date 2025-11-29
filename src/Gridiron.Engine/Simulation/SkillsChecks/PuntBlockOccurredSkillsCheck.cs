using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsChecks
{
    /// <summary>
    /// Determines if a punt is blocked by the defense.
    /// Block probability increases significantly with bad snaps and decreases with punter skill.
    /// </summary>
    public class PuntBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _punter;
        private readonly List<Player> _offensiveLine;
        private readonly List<Player> _defensiveRushers;
        private readonly bool _goodSnap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PuntBlockOccurredSkillsCheck"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for determining outcomes.</param>
        /// <param name="punter">The punter kicking the ball.</param>
        /// <param name="offensiveLine">The offensive line protecting the punt.</param>
        /// <param name="defensiveRushers">The defensive players rushing the punt.</param>
        /// <param name="goodSnap">Whether the snap was good.</param>
        public PuntBlockOccurredSkillsCheck(
            ISeedableRandom rng,
            Player punter,
            List<Player> offensiveLine,
            List<Player> defensiveRushers,
            bool goodSnap)
        {
            _rng = rng;
            _punter = punter;
            _offensiveLine = offensiveLine;
            _defensiveRushers = defensiveRushers;
            _goodSnap = goodSnap;
        }

        /// <summary>
        /// Executes the punt block check to determine if the punt is blocked.
        /// Considers punter skill, snap quality, and defensive pressure from best rusher vs average blocker.
        /// </summary>
        /// <param name="game">The current game instance.</param>
        public override void Execute(Game game)
        {
            // Base block probability
            var blockProbability = _goodSnap
                ? GameProbabilities.Punts.PUNT_BLOCK_GOOD_SNAP
                : GameProbabilities.Punts.PUNT_BLOCK_BAD_SNAP;

            // Factor 1: Punter skill (better punter = faster release)
            var punterSkill = _punter.Kicking;
            var punterFactor = 1.0 - (punterSkill / GameProbabilities.Punts.PUNT_BLOCK_PUNTER_SKILL_DENOMINATOR);
            blockProbability *= punterFactor;

            // Factor 2: Best rusher vs average blocker
            var bestRusher = _defensiveRushers
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (_offensiveLine.Count > 0 && bestRusher != null)
            {
                var avgBlocker = _offensiveLine.Average(p => p.Strength + p.Awareness);
                var rusherSkill = (bestRusher.Strength + bestRusher.Speed) / 2.0;
                var skillDifferential = rusherSkill - (avgBlocker / 2.0);

                blockProbability += (skillDifferential / 10.0) * GameProbabilities.Punts.PUNT_BLOCK_DEFENDER_SKILL_FACTOR;
            }

            // Clamp to reasonable range
            blockProbability = Math.Max(
                GameProbabilities.Punts.PUNT_BLOCK_MIN_CLAMP,
                Math.Min(GameProbabilities.Punts.PUNT_BLOCK_MAX_CLAMP, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}