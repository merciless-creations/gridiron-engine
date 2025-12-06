using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Encapsulates timeout decision-making logic.
    /// Determines whether a team should call a timeout based on game situation.
    ///
    /// <para><b>This is a DECISION SYSTEM - it answers "should we call a timeout?"</b></para>
    /// <para>It does NOT execute the timeout. Use <see cref="TimeoutMechanic"/> to apply timeout effects.</para>
    ///
    /// <para><b>DECISION SCENARIOS</b></para>
    ///
    /// <para><b>Pre-Play Decisions:</b></para>
    /// <list type="bullet">
    ///   <item><b>Ice the Kicker:</b> Defense calls timeout before a long FG attempt to disrupt the kicker</item>
    ///   <item><b>Avoid Delay of Game:</b> Offense calls timeout when play clock is about to expire</item>
    /// </list>
    ///
    /// <para><b>Post-Play Decisions:</b></para>
    /// <list type="bullet">
    ///   <item><b>Stop the Clock:</b> Offense calls timeout when trailing late to preserve time</item>
    /// </list>
    /// </summary>
    public class TimeoutDecisionEngine
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for probabilistic decisions.</param>
        public TimeoutDecisionEngine(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Determines whether a timeout should be called based on game situation.
        /// </summary>
        /// <param name="context">The current game context for the decision.</param>
        /// <returns>The timeout decision (None if no timeout should be called).</returns>
        public TimeoutDecision Decide(TimeoutContext context)
        {
            // No timeouts remaining - cannot call one
            if (context.TimeoutsRemaining <= 0)
            {
                return TimeoutDecision.None;
            }

            // Route to appropriate decision logic based on timing phase
            return context.TimingPhase switch
            {
                TimeoutTimingPhase.PrePlay => DecidePrePlay(context),
                TimeoutTimingPhase.PostPlay => DecidePostPlay(context),
                _ => TimeoutDecision.None
            };
        }

        /// <summary>
        /// Decides whether to call a timeout before a play.
        /// </summary>
        private TimeoutDecision DecidePrePlay(TimeoutContext context)
        {
            // Check for avoid delay of game (offense only)
            if (context.IsOffense && ShouldAvoidDelayOfGame(context))
            {
                return TimeoutDecision.AvoidDelayOfGame;
            }

            // Check for ice the kicker (defense only)
            if (!context.IsOffense && ShouldIceKicker(context))
            {
                return TimeoutDecision.IceKicker;
            }

            return TimeoutDecision.None;
        }

        /// <summary>
        /// Decides whether to call a timeout after a play.
        /// </summary>
        private TimeoutDecision DecidePostPlay(TimeoutContext context)
        {
            // Check for stop the clock (offense only, when trailing)
            if (context.IsOffense && ShouldStopClock(context))
            {
                return TimeoutDecision.StopClock;
            }

            return TimeoutDecision.None;
        }

        /// <summary>
        /// Determines if the offense should call timeout to avoid delay of game.
        ///
        /// <para>Conditions:</para>
        /// <list type="bullet">
        ///   <item>Play clock is at or below threshold (default 3 seconds)</item>
        ///   <item>Random roll passes probability check</item>
        /// </list>
        /// </summary>
        private bool ShouldAvoidDelayOfGame(TimeoutContext context)
        {
            if (context.PlayClockSeconds > GameProbabilities.Timeouts.AVOID_DELAY_PLAY_CLOCK_THRESHOLD)
            {
                return false;
            }

            return _rng.NextDouble() < GameProbabilities.Timeouts.AVOID_DELAY_PROBABILITY;
        }

        /// <summary>
        /// Determines if the defense should call timeout to ice the kicker.
        ///
        /// <para>Conditions:</para>
        /// <list type="bullet">
        ///   <item>Upcoming play is a field goal attempt</item>
        ///   <item>Field goal distance is at or above threshold (default 45 yards)</item>
        ///   <item>Random roll passes probability check (default 30%)</item>
        /// </list>
        /// </summary>
        private bool ShouldIceKicker(TimeoutContext context)
        {
            // Must be a field goal attempt
            if (context.UpcomingPlayType != PlayType.FieldGoal)
            {
                return false;
            }

            // Must be a long enough kick to warrant icing
            if (!context.FieldGoalDistance.HasValue ||
                context.FieldGoalDistance.Value < GameProbabilities.Timeouts.ICE_KICKER_MIN_DISTANCE)
            {
                return false;
            }

            // Probabilistic decision
            return _rng.NextDouble() < GameProbabilities.Timeouts.ICE_KICKER_PROBABILITY;
        }

        /// <summary>
        /// Determines if the offense should call timeout to stop the clock.
        ///
        /// <para>Conditions:</para>
        /// <list type="bullet">
        ///   <item>Team is trailing (negative score differential)</item>
        ///   <item>Under 2 minutes remaining in the half</item>
        ///   <item>Clock is currently running</item>
        ///   <item>Random roll passes probability check (default 85%)</item>
        /// </list>
        /// </summary>
        private bool ShouldStopClock(TimeoutContext context)
        {
            // Must be trailing
            if (context.ScoreDifferential >= 0)
            {
                return false;
            }

            // Must be late in the half
            if (context.TimeRemainingInHalfSeconds > GameProbabilities.Timeouts.STOP_CLOCK_TIME_THRESHOLD)
            {
                return false;
            }

            // Clock must be running (no point calling timeout if clock is stopped)
            if (!context.IsClockRunning)
            {
                return false;
            }

            // Probabilistic decision (high probability - usually want to stop clock)
            return _rng.NextDouble() < GameProbabilities.Timeouts.STOP_CLOCK_PROBABILITY;
        }
    }
}
