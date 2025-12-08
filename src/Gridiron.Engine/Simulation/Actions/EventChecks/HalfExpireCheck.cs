using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Rules.EndOfHalf;
using System.Linq;

namespace Gridiron.Engine.Simulation.Actions.EventChecks
{
    /// <summary>
    /// Checks if a half has expired and handles transitioning between halves.
    /// This action monitors quarter expiration and advances the game to the next half
    /// or marks the game as over when appropriate.
    /// Also enforces end-of-half penalty extension rules.
    /// </summary>
    public sealed class HalfExpireCheck : IGameAction
    {
        private readonly IEndOfHalfRulesProvider _endOfHalfRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="HalfExpireCheck"/> class.
        /// </summary>
        /// <param name="endOfHalfRules">The end-of-half rules provider.</param>
        public HalfExpireCheck(IEndOfHalfRulesProvider endOfHalfRules)
        {
            _endOfHalfRules = endOfHalfRules;
        }
        /// <summary>
        /// Executes the half expiration check and advances the game state when a half ends.
        /// </summary>
        /// <param name="game">The game instance to check for half expiration.</param>
        /// <remarks>
        /// When the third quarter expires, this method transitions the game to the second half.
        /// When the game is over, it sets the half type to GameOver. Future enhancements will
        /// include handling tied games and overtime transitions.
        /// </remarks>
        public void Execute(Game game)
        {
            if (game.CurrentPlay.QuarterExpired)
            {
                // Check for end-of-half penalty extension
                bool shouldExtendHalf = ShouldExtendHalfForPenalty(game);

                if (shouldExtendHalf)
                {
                    // Untimed down - don't end the half, reset quarter expired flag
                    game.CurrentPlay.QuarterExpired = false;
                    game.CurrentPlay.Result.LogInformation("Penalty extends the half. Untimed down.");
                    game.CurrentQuarter.TimeRemaining = 0; // Keep time at 0
                    return; // Don't advance half
                }

                switch (game.CurrentQuarter.QuarterType)
                {
                    case QuarterType.Third:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf = game.Halves[1];
                        break;
                    case QuarterType.GameOver:
                        game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf.HalfType = HalfType.GameOver;
                        break;
                }
            }

            //TODO check if tied & move to OT
            //TODO check if tied & move to another OT
        }

        /// <summary>
        /// Determines if the half should be extended for one untimed down due to a penalty.
        /// NFL/NCAA Rule: Half cannot end on an accepted defensive penalty.
        /// </summary>
        private bool ShouldExtendHalfForPenalty(Game game)
        {
            var play = game.CurrentPlay;

            // Only check on plays that would end a half (Q2 or Q4/OT end)
            if (game.CurrentQuarter.QuarterType != QuarterType.Second &&
                game.CurrentQuarter.QuarterType != QuarterType.Fourth &&
                game.CurrentQuarter.QuarterType != QuarterType.Overtime)
            {
                return false;
            }

            // Must have at least one penalty
            if (play.Penalties == null || !play.Penalties.Any())
            {
                return false;
            }

            // Check if there's an accepted penalty
            var acceptedPenalties = play.Penalties.Where(p => p.Accepted).ToList();
            if (!acceptedPenalties.Any())
            {
                return false;
            }

            // Determine offense for this play
            var offensivePossession = play.Possession;

            // Check each accepted penalty
            foreach (var penalty in acceptedPenalties)
            {
                // Is this a defensive penalty?
                bool isDefensivePenalty = penalty.CalledOn != offensivePossession;

                if (isDefensivePenalty)
                {
                    // Defensive penalty - check if rules allow half to end
                    if (!_endOfHalfRules.AllowsHalfToEndOnDefensivePenalty)
                    {
                        // Half CANNOT end - extend for untimed down
                        return true;
                    }
                }
                else
                {
                    // Offensive penalty - check if rules allow half to end
                    if (!_endOfHalfRules.AllowsHalfToEndOnOffensivePenalty)
                    {
                        // Half CANNOT end (rare) - extend for untimed down
                        return true;
                    }
                }
            }

            // No penalties require half extension
            return false;
        }
    }
}
