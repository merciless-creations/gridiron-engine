using System;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.BaseClasses;
using Gridiron.Engine.Simulation.Configuration;

namespace Gridiron.Engine.Simulation.SkillsCheckResults
{
    /// <summary>
    /// Represents the complete details of an injury including type, severity, and removal requirement.
    /// </summary>
    public class InjuryDetails
    {
        /// <summary>
        /// Gets or sets the type of injury (Ankle, Knee, Shoulder, Concussion, Hamstring).
        /// </summary>
        public InjuryType InjuryType { get; set; }

        /// <summary>
        /// Gets or sets the severity of the injury (Minor, Moderate, GameEnding).
        /// </summary>
        public InjurySeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player must be removed from the game immediately.
        /// </summary>
        public bool RequiresImmediateRemoval { get; set; }
    }

    /// <summary>
    /// Determines the details of an injury: type, severity, and whether player needs immediate removal.
    /// Injury type distribution varies by position, reflecting realistic injury patterns.
    /// </summary>
    public class InjuryEffectSkillsCheckResult : SkillsCheckResult<InjuryDetails>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _injuredPlayer;
        private readonly PlayType _playType;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjuryEffectSkillsCheckResult"/> class.
        /// </summary>
        /// <param name="rng">Random number generator for determining injury characteristics.</param>
        /// <param name="injuredPlayer">The player who was injured.</param>
        /// <param name="playType">The type of play when the injury occurred.</param>
        /// <exception cref="ArgumentNullException">Thrown when rng or injuredPlayer is null.</exception>
        public InjuryEffectSkillsCheckResult(ISeedableRandom rng, Player injuredPlayer, PlayType playType)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _injuredPlayer = injuredPlayer ?? throw new ArgumentNullException(nameof(injuredPlayer));
            _playType = playType;
        }

        /// <summary>
        /// Executes the injury calculation to determine severity, type, and immediate removal requirement.
        /// Severity is determined first (60% minor, 30% moderate, 10% game-ending), then injury type
        /// is selected based on the player's position.
        /// </summary>
        /// <param name="game">The current game context.</param>
        public override void Execute(Game game)
   {
    Result = new InjuryDetails();
    
        // Determine severity (minor, moderate, game-ending) - FIRST
   Result.Severity = DetermineSeverity();

        // Determine injury type based on position and play type - SECOND
      Result.InjuryType = DetermineInjuryType();

    // Determine if player must leave immediately
  // Game-ending and moderate injuries always require immediate removal
    Result.RequiresImmediateRemoval = Result.Severity == InjurySeverity.GameEnding || 
   Result.Severity == InjurySeverity.Moderate;
    }

        /// <summary>
        /// Determines injury type based on position and play type.
        /// Different positions have different injury profiles.
        /// </summary>
        /// <returns>The type of injury sustained.</returns>
        private InjuryType DetermineInjuryType()
{
          // Get injury type distribution for player's position
     var (ankle, knee, shoulder, concussion, hamstring) = GetInjuryDistribution(_injuredPlayer.Position);

        // Roll for injury type
       double roll = _rng.NextDouble();
double cumulative = 0.0;

  if ((cumulative += ankle) > roll) return InjuryType.Ankle;
     if ((cumulative += knee) > roll) return InjuryType.Knee;
   if ((cumulative += shoulder) > roll) return InjuryType.Shoulder;
if ((cumulative += concussion) > roll) return InjuryType.Concussion;
      return InjuryType.Hamstring;
 }

        /// <summary>
        /// Returns injury type distribution for a given position.
        /// Returns tuple: (ankle, knee, shoulder, concussion, hamstring) probabilities.
        /// </summary>
        /// <param name="position">The player's position.</param>
        /// <returns>A tuple containing probability weights for each injury type.</returns>
        private (double ankle, double knee, double shoulder, double concussion, double hamstring) GetInjuryDistribution(Positions position)
    {
    return position switch
   {
      // RB/WR: High ankle/knee/hamstring risk from cutting and speed
      Positions.RB or Positions.WR => (0.40, 0.25, 0.10, 0.05, 0.20),

  // QB: High shoulder risk from throwing, concussion from hits
           Positions.QB => (0.15, 0.25, 0.35, 0.20, 0.05),

      // OL/DL: High knee/ankle risk from weight and leverage
     Positions.C or Positions.G or Positions.T or Positions.DE or Positions.DT =>
   (0.40, 0.40, 0.10, 0.05, 0.05),

        // LB/CB/S: Balanced risk, high hamstring from pursuit
    Positions.LB or Positions.OLB or Positions.CB or Positions.S or Positions.FS =>
  (0.25, 0.20, 0.15, 0.05, 0.35),

    // TE: Mix of skill position and blocker
       Positions.TE or Positions.FB => (0.30, 0.25, 0.20, 0.05, 0.20),

     // K/P: High ankle risk from kicking motion
     Positions.K or Positions.P => (0.50, 0.20, 0.10, 0.05, 0.15),

    // Default fallback
      _ => (0.30, 0.25, 0.20, 0.10, 0.15)
   };
        }

        /// <summary>
        /// Determines severity of injury: Minor (60%), Moderate (30%), GameEnding (10%).
        /// </summary>
        /// <returns>The severity level of the injury.</returns>
        private InjurySeverity DetermineSeverity()
   {
     double roll = _rng.NextDouble();

   if (roll < InjuryProbabilities.MINOR_INJURY_PROBABILITY)
    {
 return InjurySeverity.Minor;
       }
else if (roll < InjuryProbabilities.MINOR_INJURY_PROBABILITY + InjuryProbabilities.MODERATE_INJURY_PROBABILITY)
  {
return InjurySeverity.Moderate;
      }
     else
     {
        return InjurySeverity.GameEnding;
}
    }
 }
}
