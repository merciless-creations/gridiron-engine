using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Decision engine for onside kick attempts.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public class OnsideKickDecisionEngine
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnsideKickDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for probabilistic decisions.</param>
        public OnsideKickDecisionEngine(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Decides whether to attempt an onside kick or normal kickoff.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>OnsideKick or NormalKickoff.</returns>
        public OnsideKickDecision Decide(OnsideKickContext context)
        {
            double onsideProbability = CalculateOnsideProbability(context);

            // Only consume RNG if there's a non-zero probability of onside kick
            // This maintains behavioral parity with the original short-circuit logic
            if (onsideProbability <= 0.0)
            {
                return OnsideKickDecision.NormalKickoff;
            }

            var roll = _rng.NextDouble();
            return roll < onsideProbability
                ? OnsideKickDecision.OnsideKick
                : OnsideKickDecision.NormalKickoff;
        }

        /// <summary>
        /// Calculates the probability of attempting an onside kick based on game situation.
        /// </summary>
        private double CalculateOnsideProbability(OnsideKickContext context)
        {
            // Original behavior: trailing by 7+ points triggers onside kick probability
            // This maintains behavioral parity with the original ShouldAttemptOnsideKick()
            if (context.ScoreDifferential <= -7)
            {
                return GameProbabilities.Kickoffs.ONSIDE_ATTEMPT_PROBABILITY;
            }

            // Not trailing by enough = no onside attempt
            return 0.0;

            // Future enhancements (commented out for behavioral parity):
            // - Late game adjustments (higher probability in Q4)
            // - Desperate situations (trailing by 2+ scores with < 2 minutes)
            // - Coaching tendencies (aggressive vs conservative)
            // - Kicker's onside kick skill
            // - Opponent's hands team effectiveness
        }
    }
}
