using Gridiron.Engine.Domain;
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
        /// Decides what type of scrimmage play to call (Run, Pass, Spike, or Kneel).
        /// Checks for clock management plays (spike/kneel) first, then falls through to run/pass.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>Run, Pass, Spike, or Kneel.</returns>
        public PlayCallDecision DecidePlayType(PlayCallContext context)
        {
            // Check for clock management plays first
            if (ShouldKneel(context))
            {
                return PlayCallDecision.Kneel;
            }

            if (ShouldSpike(context))
            {
                return PlayCallDecision.Spike;
            }

            // Fall through to run/pass decision
            double runProbability = CalculateRunProbability(context);

            var roll = _rng.NextDouble();
            return roll < runProbability
                ? PlayCallDecision.Run
                : PlayCallDecision.Pass;
        }

        /// <summary>
        /// Determines if the team should kneel (victory formation) to run out the clock.
        /// Kneel when: Leading in 4th quarter and can kneel out the remaining time.
        /// The clock runs for approximately 40 seconds per kneel play.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>True if the team should kneel.</returns>
        public bool ShouldKneel(PlayCallContext context)
        {
            // Must be in 4th quarter and leading
            if (!context.IsFourthQuarter || !context.IsLeading)
            {
                return false;
            }

            // Don't kneel on special situations (kickoffs, conversions, etc.)
            if (context.Down == Downs.None)
            {
                return false;
            }

            // Calculate if we can kneel out the clock
            // Each kneel takes approximately 40 seconds (full play clock)
            // We need: (downs_remaining * 40) >= time_remaining
            int kneelTimeAvailable = context.DownsRemaining * GameProbabilities.ClockManagement.KNEEL_ELAPSED_TIME_SECONDS;

            return kneelTimeAvailable >= context.TimeRemainingSeconds;
        }

        /// <summary>
        /// Determines if the team should spike the ball to stop the clock.
        /// Spike when: Trailing late in the game, no timeouts, clock is running, and not 4th down.
        /// A spike takes only 3 seconds but uses a down.
        /// </summary>
        /// <param name="context">The game context for the decision.</param>
        /// <returns>True if the team should spike.</returns>
        public bool ShouldSpike(PlayCallContext context)
        {
            // Must be in the late game (4th quarter, under 2 minutes)
            if (!context.IsLateGame)
            {
                return false;
            }

            // Must be trailing - no point spiking if winning or tied
            if (!context.IsTrailing)
            {
                return false;
            }

            // Must not have timeouts - if you have timeouts, use them instead
            if (context.HasTimeouts)
            {
                return false;
            }

            // Clock must be running - no point spiking if clock is already stopped
            if (!context.IsClockRunning)
            {
                return false;
            }

            // Don't spike on 4th down - you'll turn the ball over
            if (context.Down == Downs.Fourth)
            {
                return false;
            }

            // Don't spike on special situations (kickoffs, conversions, etc.)
            if (context.Down == Downs.None)
            {
                return false;
            }

            return true;
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
