using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Decision engine for play calling including run/pass selection and conversion decisions.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public class PlayCallDecisionEngine
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayCallDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for probabilistic decisions.</param>
        public PlayCallDecisionEngine(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Decides whether to attempt an extra point or two-point conversion after a touchdown.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>ExtraPoint or TwoPointConversion.</returns>
        public ConversionDecision DecideConversion(PlayCallContext context)
        {
            // Calculate two-point probability based on game situation
            double twoPointProbability = CalculateTwoPointProbability(context);

            var roll = _rng.NextDouble();
            return roll < twoPointProbability
                ? ConversionDecision.TwoPointConversion
                : ConversionDecision.ExtraPoint;
        }

        /// <summary>
        /// Decides whether to run or pass on a scrimmage play.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>Run or Pass.</returns>
        public PlayCallDecision DecidePlayType(PlayCallContext context)
        {
            // Calculate run probability based on game situation
            double runProbability = CalculateRunProbability(context);

            var roll = _rng.NextDouble();
            return roll < runProbability
                ? PlayCallDecision.Run
                : PlayCallDecision.Pass;
        }

        /// <summary>
        /// Calculates the probability of attempting a two-point conversion.
        /// Currently uses base probability, but can be extended for situational logic.
        /// </summary>
        private double CalculateTwoPointProbability(PlayCallContext context)
        {
            // Base probability from game settings
            double baseProbability = GameProbabilities.GameDecisions.TWO_POINT_CONVERSION_ATTEMPT;

            // Future: Adjust based on game situation
            // - Late in game with specific score differentials
            // - Coaching tendencies
            // - Analytics-based decisions

            // Example situational adjustments (currently commented out for behavioral parity):
            // if (context.IsCriticalSituation)
            // {
            //     // More likely to go for 2 when game is on the line
            //     // Specific score scenarios where 2-pt makes strategic sense
            //     if (context.ScoreDifferential == -2 || context.ScoreDifferential == -5)
            //     {
            //         baseProbability = 0.75; // Strongly favor going for 2
            //     }
            // }

            return baseProbability;
        }

        /// <summary>
        /// Calculates the probability of calling a run play.
        /// Currently uses 50/50 split, but can be extended for situational logic.
        /// </summary>
        private double CalculateRunProbability(PlayCallContext context)
        {
            // Two-point conversion has its own probability
            if (context.IsTwoPointConversion)
            {
                return GameProbabilities.GameDecisions.TWO_POINT_RUN_PROBABILITY;
            }

            // Base probability for normal scrimmage plays
            // Currently 50/50 as per existing behavior
            double baseProbability = 0.50;

            // Future: Adjust based on game situation
            // - Short yardage = more runs
            // - Long yardage = more passes
            // - Trailing late = more passes
            // - Leading late = more runs (clock management)
            // - Down and distance tendencies
            // - Coaching philosophy

            // Example situational adjustments (currently commented out for behavioral parity):
            // if (context.IsShortYardage)
            // {
            //     baseProbability = 0.65; // Favor runs in short yardage
            // }
            // else if (context.IsLongYardage)
            // {
            //     baseProbability = 0.35; // Favor passes in long yardage
            // }
            //
            // if (context.IsTwoMinuteWarning && context.IsTrailing)
            // {
            //     baseProbability = 0.20; // Pass-heavy when trailing late
            // }

            return baseProbability;
        }
    }
}
