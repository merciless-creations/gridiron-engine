using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Interfaces;
using Gridiron.Engine.Simulation.Overtime;
using Microsoft.Extensions.Logging;

namespace Gridiron.Engine.Simulation.Actions
{
    /// <summary>
    /// Sets up overtime: initializes state, performs coin toss, sets timeouts.
    /// </summary>
    public class OvertimeSetup : IGameAction
    {
        private readonly ISeedableRandom _rng;
        private readonly IOvertimeRulesProvider _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="OvertimeSetup"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for coin toss.</param>
        /// <param name="rules">The overtime rules provider.</param>
        public OvertimeSetup(ISeedableRandom rng, IOvertimeRulesProvider rules)
        {
            _rng = rng;
            _rules = rules;
        }

        /// <summary>
        /// Executes the overtime setup, including coin toss and state initialization.
        /// </summary>
        /// <param name="game">The game to set up overtime for.</param>
        public void Execute(Game game)
        {
            // Initialize or update overtime state
            if (game.OvertimeState == null)
            {
                game.OvertimeState = new OvertimeState
                {
                    IsInOvertime = true,
                    CurrentPeriod = 1,
                    HomeTimeoutsRemaining = _rules.TimeoutsPerTeam,
                    AwayTimeoutsRemaining = _rules.TimeoutsPerTeam
                };

                // Perform coin toss for overtime
                if (_rules.HasOvertimeCoinToss)
                {
                    var toss = _rng.Next(2);
                    game.OvertimeState.CoinTossWinner = toss == 1 ? Possession.Away : Possession.Home;

                    // In overtime, winner typically receives (rarely defers)
                    game.OvertimeState.FirstPossessionTeam = game.OvertimeState.CoinTossWinner;
                }
                else
                {
                    // No coin toss - use existing possession logic
                    game.OvertimeState.FirstPossessionTeam = Possession.Home;
                }

                game.Logger.LogInformation($"=== OVERTIME: {_rules.Name} ===");
                game.Logger.LogInformation($"Coin toss won by {game.OvertimeState.CoinTossWinner}");
            }
            else
            {
                // Starting a new overtime period
                game.OvertimeState.StartNewPeriod();
                game.OvertimeState.HomeTimeoutsRemaining = _rules.TimeoutsPerTeam;
                game.OvertimeState.AwayTimeoutsRemaining = _rules.TimeoutsPerTeam;

                game.Logger.LogInformation($"=== OVERTIME PERIOD {game.OvertimeState.CurrentPeriod} ===");
            }

            game.OvertimeState.CurrentPossessionTeam = game.OvertimeState.FirstPossessionTeam;

            // Set up overtime quarter with appropriate duration
            game.CurrentQuarter = new Quarter(QuarterType.Overtime, _rules.OvertimePeriodDuration);

            // Set field position based on rules
            if (_rules.UsesKickoff(game.OvertimeState))
            {
                // NFL: Set up for kickoff
                // The kickoff team is opposite of receiving team
                var receivingTeam = game.OvertimeState.FirstPossessionTeam;
                var kickingTeam = receivingTeam == Possession.Home ? Possession.Away : Possession.Home;

                // Set up for kickoff play
                game.CurrentDown = Downs.None;
                game.YardsToGo = 0;

                // Kicking team kicks from their 35-yard line
                game.FieldPosition = kickingTeam == Possession.Home ? 35 : 65;
            }
            else
            {
                // NCAA-style: start at fixed position
                var fieldPos = _rules.FixedStartingFieldPosition ?? 75;
                game.FieldPosition = game.OvertimeState.FirstPossessionTeam == Possession.Home
                    ? 100 - fieldPos  // Convert to absolute field position
                    : fieldPos;

                var (down, yardsToGo) = _rules.GetStartingDownAndDistance(game.OvertimeState);
                game.CurrentDown = down;
                game.YardsToGo = yardsToGo;
            }

            // Create initial possession record
            game.OvertimeState.Possessions.Add(new OvertimePossession
            {
                Period = game.OvertimeState.CurrentPeriod,
                Team = game.OvertimeState.FirstPossessionTeam,
                StartingFieldPosition = game.FieldPosition
            });

            game.Logger.LogInformation(
                $"{game.OvertimeState.FirstPossessionTeam} will receive. " +
                $"Period duration: {_rules.OvertimePeriodDuration / 60} minutes. " +
                $"Ties allowed: {_rules.AllowsTies}");
        }
    }
}
