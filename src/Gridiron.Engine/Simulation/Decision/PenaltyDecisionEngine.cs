using System;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Decision engine that determines whether to accept or decline a penalty.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public class PenaltyDecisionEngine
    {
        private readonly ISeedableRandom? _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="PenaltyDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">Optional random number generator for probabilistic decisions.
        /// Currently not used as penalty decisions are deterministic based on game situation.</param>
        public PenaltyDecisionEngine(ISeedableRandom? rng = null)
        {
            _rng = rng;
        }

        /// <summary>
        /// Determines whether to accept or decline a penalty based on game situation.
        /// </summary>
        /// <param name="context">The penalty decision context.</param>
        /// <returns>Accept or Decline decision.</returns>
        public PenaltyDecision Decide(PenaltyDecisionContext context)
        {
            // If play resulted in touchdown for the deciding team, always decline
            // (can't get better than a touchdown)
            if (context.PlayResultedInTouchdown && !context.IsOffensivePenalty)
            {
                return PenaltyDecision.Decline;
            }

            if (context.IsDefensivePenalty)
            {
                return DecideOnDefensivePenalty(context);
            }
            else
            {
                return DecideOnOffensivePenalty(context);
            }
        }

        /// <summary>
        /// Decides whether to accept a defensive penalty (offense decides).
        /// </summary>
        private PenaltyDecision DecideOnDefensivePenalty(PenaltyDecisionContext context)
        {
            // Defensive penalty - the offense (fouled team) decides

            // Always accept if it gives automatic first down and we didn't already get one
            if (IsAutomaticFirstDown(context.PenaltyName) && !context.PlayResultedInFirstDown)
            {
                return PenaltyDecision.Accept;
            }

            // If play resulted in turnover, accept to negate the turnover
            if (context.PlayResultedInTurnover)
            {
                return PenaltyDecision.Accept;
            }

            // Compare outcomes: what do we get with penalty vs without?
            var penaltyYardsGain = context.PenaltyYards;
            var playYardsGain = context.YardsGainedOnPlay;

            // Accept if penalty gives more yards than the play result
            if (penaltyYardsGain > playYardsGain)
            {
                return PenaltyDecision.Accept;
            }

            // Accept if penalty gives automatic first down even with fewer yards
            if (IsAutomaticFirstDown(context.PenaltyName))
            {
                // We get an automatic first down - usually worth it
                // Exception: if play result was significantly better
                if (playYardsGain > penaltyYardsGain + 10)
                {
                    // Play gave us 10+ more yards than penalty would
                    // AND we already got first down, so decline
                    if (context.PlayResultedInFirstDown)
                    {
                        return PenaltyDecision.Decline;
                    }
                }
                return PenaltyDecision.Accept;
            }

            // Play result was better - decline the penalty
            return PenaltyDecision.Decline;
        }

        /// <summary>
        /// Decides whether to accept an offensive penalty (defense decides).
        /// </summary>
        private PenaltyDecision DecideOnOffensivePenalty(PenaltyDecisionContext context)
        {
            // Offensive penalty - the defense (fouled team) decides

            // If play resulted in turnover (good for defense), usually decline
            // to keep the turnover
            if (context.PlayResultedInTurnover)
            {
                return PenaltyDecision.Decline;
            }

            // Calculate what each outcome gives the defense
            var playResultDown = CalculateResultingDown(context);

            // If play result is turnover on downs (4th down didn't convert),
            // decline to keep the turnover
            if (playResultDown == Downs.None)
            {
                return PenaltyDecision.Decline;
            }

            // Loss of down penalties are usually good to accept
            if (IsLossOfDown(context.PenaltyName))
            {
                return PenaltyDecision.Accept;
            }

            // Calculate yardage impact
            var penaltyPushback = context.PenaltyYards;
            var playYardsGained = context.YardsGainedOnPlay;

            // If play lost yards anyway, accepting penalty might not help much
            if (playYardsGained < 0 && penaltyPushback <= Math.Abs(playYardsGained))
            {
                // Play result was worse than penalty would impose
                return PenaltyDecision.Decline;
            }

            // Default: accept offensive penalties (they push offense back)
            return PenaltyDecision.Accept;
        }

        /// <summary>
        /// Calculates what down it would be if the play result stands.
        /// </summary>
        private Downs CalculateResultingDown(PenaltyDecisionContext context)
        {
            // If play gained enough for first down
            if (context.YardsGainedOnPlay >= context.YardsToGo)
            {
                return Downs.First;
            }

            // Otherwise advance the down
            return context.CurrentDown switch
            {
                Downs.First => Downs.Second,
                Downs.Second => Downs.Third,
                Downs.Third => Downs.Fourth,
                Downs.Fourth => Downs.None, // Turnover on downs
                _ => Downs.None
            };
        }

        /// <summary>
        /// Determines if a penalty gives an automatic first down.
        /// Per NFL rules, most defensive penalties give automatic first down.
        /// </summary>
        private bool IsAutomaticFirstDown(PenaltyNames penalty)
        {
            // Defensive penalties that do NOT give automatic first down
            var noAutomaticFirstDown = new[]
            {
                PenaltyNames.DefensiveOffside,
                PenaltyNames.Encroachment,
                PenaltyNames.NeutralZoneInfraction,
                PenaltyNames.DefensiveDelayofGame,
                PenaltyNames.IllegalSubstitution,
                PenaltyNames.Defensive12OnField,
                PenaltyNames.RunningIntotheKicker
            };

            return !noAutomaticFirstDown.Contains(penalty);
        }

        /// <summary>
        /// Determines if a penalty causes loss of down.
        /// </summary>
        private bool IsLossOfDown(PenaltyNames penalty)
        {
            var lossOfDownPenalties = new[]
            {
                PenaltyNames.IntentionalGrounding,
                PenaltyNames.IllegalForwardPass
            };

            return lossOfDownPenalties.Contains(penalty);
        }
    }
}
