using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using System.Linq;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Calculates base yards gained on a run play based on player skills and matchups.
    /// Considers offensive power (ball carrier + blockers) vs defensive power,
    /// then adds randomness to create realistic variance in run outcomes.
    /// </summary>
    public class RunYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _ballCarrier;
        private readonly List<Player> _offensivePlayers;
        private readonly List<Player> _defensivePlayers;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunYardsSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining yardage variance.</param>
        /// <param name="ballCarrier">The player carrying the ball.</param>
        /// <param name="offensivePlayers">Offensive players on the field (blockers).</param>
        /// <param name="defensivePlayers">Defensive players on the field (tacklers).</param>
        public RunYardsSkillsCheckResult(
            ISeedableRandom rng,
            Player ballCarrier,
            List<Player> offensivePlayers,
            List<Player> defensivePlayers)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
            _offensivePlayers = offensivePlayers;
            _defensivePlayers = defensivePlayers;
        }

        /// <summary>
        /// Executes the calculation to determine base run yardage.
        /// Compares offensive power (blockers + ball carrier) vs defensive power (tacklers).
        /// Average run is 3-5 yards with variance from -15 to +10 yards.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
        {
            // Calculate offensive power (ball carrier + blockers)
            var offensivePower = CalculateOffensivePower();

            // Calculate defensive power
            var defensivePower = CalculateDefensivePower();

            // Calculate base yardage (with randomness)
            var skillDifferential = offensivePower - defensivePower;
            var baseYards = 3.0 + (skillDifferential / 20.0); // Average around 3-5 yards

            // Add randomness (-15 to +10 yard variance)
            // This supports realistic big losses (sacks, backfield tackles, muffed snaps)
            // while keeping big gains reasonable (tackle breaks/breakaways handle explosive plays)
            var randomFactor = (_rng.NextDouble() * 25) - 15;
            var totalYards = baseYards + randomFactor;

            Result = (int)Math.Round(totalYards);
        }

        /// <summary>
        /// Calculates offensive power based on blockers' blocking skill and ball carrier's rushing ability.
        /// </summary>
        /// <returns>Offensive power value used in yardage calculation.</returns>
        private double CalculateOffensivePower()
        {
            var blockers = _offensivePlayers.Where(p =>
                p.Position == Positions.C ||
                p.Position == Positions.G ||
                p.Position == Positions.T ||
                p.Position == Positions.TE ||
                p.Position == Positions.FB).ToList();

            var blockingPower = blockers.Any() ? blockers.Average(b => b.Blocking) : 50;
            var ballCarrierPower = (_ballCarrier.Rushing * 2 + _ballCarrier.Speed + _ballCarrier.Agility) / 4.0;

            return (blockingPower + ballCarrierPower) / 2.0;
        }

        /// <summary>
        /// Calculates defensive power based on defenders' tackling, strength, and speed.
        /// </summary>
        /// <returns>Defensive power value used in yardage calculation.</returns>
        private double CalculateDefensivePower()
        {
            var defenders = _defensivePlayers.Where(p =>
                p.Position == Positions.DT ||
                p.Position == Positions.DE ||
                p.Position == Positions.LB ||
                p.Position == Positions.OLB).ToList();

            return defenders.Any() ? defenders.Average(d => (d.Tackling + d.Strength + d.Speed) / 3.0) : 50;
        }
    }
}
