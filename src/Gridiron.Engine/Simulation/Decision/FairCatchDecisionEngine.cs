using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Decision engine for fair catch decisions on punts and kickoffs.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public class FairCatchDecisionEngine
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="FairCatchDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for probabilistic decisions.</param>
        public FairCatchDecisionEngine(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Decides whether the returner should signal for a fair catch or attempt a return.
        /// </summary>
        /// <param name="context">The context for the fair catch decision.</param>
        /// <returns>FairCatch or AttemptReturn.</returns>
        public FairCatchDecision Decide(FairCatchContext context)
        {
            double fairCatchProbability = CalculateFairCatchProbability(context);

            var roll = _rng.NextDouble();
            return roll < fairCatchProbability
                ? FairCatchDecision.FairCatch
                : FairCatchDecision.AttemptReturn;
        }

        /// <summary>
        /// Calculates the probability of signaling for a fair catch based on situation.
        /// Factors in hang time (coverage pressure) and field position (risk).
        /// </summary>
        private double CalculateFairCatchProbability(FairCatchContext context)
        {
            // Base fair catch probability
            double baseProbability = GameProbabilities.Punts.PUNT_FAIR_CATCH_BASE;

            // Hang time adjustments (higher hang time = more coverage = more fair catches)
            if (context.IsHighHangTime)
            {
                baseProbability += GameProbabilities.Punts.PUNT_FAIR_CATCH_HIGH_HANG_BONUS;
            }
            else if (context.IsMediumHangTime)
            {
                baseProbability += GameProbabilities.Punts.PUNT_FAIR_CATCH_MEDIUM_HANG_BONUS;
            }

            // Field position adjustments (deep in own territory = more conservative)
            if (context.IsDeepInOwnTerritory)
            {
                baseProbability += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_10_BONUS;
            }
            else if (context.IsInDangerZone)
            {
                baseProbability += GameProbabilities.Punts.PUNT_FAIR_CATCH_OWN_20_BONUS;
            }

            // Returner skill adjustments (future enhancement)
            // Good catchers and high awareness players might be more willing to return
            // if (context.IsGoodCatcher && context.HasHighAwareness)
            // {
            //     baseProbability -= 0.05; // Slightly more aggressive
            // }

            // Kickoff vs Punt adjustments
            // Kickoffs typically have more momentum and coverage, so slightly higher fair catch
            if (context.KickType == PlayType.Kickoff)
            {
                baseProbability += 0.05;
            }

            // Clamp to valid probability range
            return Math.Clamp(baseProbability, 0.0, 1.0);
        }
    }
}
