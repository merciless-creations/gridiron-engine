using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Context for fair catch decisions on punts and kickoffs.
    /// Follows the Context → Decision → Mechanic pattern.
    /// </summary>
    public readonly struct FairCatchContext
    {
        /// <summary>Hang time of the kick in seconds.</summary>
        public double HangTime { get; }

        /// <summary>
        /// The yard line where the kick will land (from kicking team's perspective).
        /// For punts: 0-100 where 100 is opponent's goal line.
        /// </summary>
        public int LandingSpot { get; }

        /// <summary>
        /// Field position from receiving team's perspective (0-100 where 100 is their goal).
        /// Calculated as 100 - LandingSpot.
        /// </summary>
        public int ReceivingTeamFieldPosition { get; }

        /// <summary>Type of kick being returned (Punt or Kickoff).</summary>
        public PlayType KickType { get; }

        /// <summary>The returner's catching ability (0-100).</summary>
        public int ReturnerCatching { get; }

        /// <summary>The returner's awareness (0-100).</summary>
        public int ReturnerAwareness { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FairCatchContext"/> struct.
        /// </summary>
        public FairCatchContext(
            double hangTime,
            int landingSpot,
            PlayType kickType,
            int returnerCatching = 50,
            int returnerAwareness = 50)
        {
            HangTime = hangTime;
            LandingSpot = landingSpot;
            ReceivingTeamFieldPosition = 100 - landingSpot;
            KickType = kickType;
            ReturnerCatching = returnerCatching;
            ReturnerAwareness = returnerAwareness;
        }

        /// <summary>
        /// Creates a fair catch context for a punt return.
        /// </summary>
        public static FairCatchContext ForPuntReturn(double hangTime, int landingSpot, Player? returner = null)
        {
            return new FairCatchContext(
                hangTime: hangTime,
                landingSpot: landingSpot,
                kickType: PlayType.Punt,
                returnerCatching: returner?.Catching ?? 50,
                returnerAwareness: returner?.Awareness ?? 50
            );
        }

        /// <summary>
        /// Creates a fair catch context for a kickoff return.
        /// </summary>
        public static FairCatchContext ForKickoffReturn(double hangTime, int landingSpot, Player? returner = null)
        {
            return new FairCatchContext(
                hangTime: hangTime,
                landingSpot: landingSpot,
                kickType: PlayType.Kickoff,
                returnerCatching: returner?.Catching ?? 50,
                returnerAwareness: returner?.Awareness ?? 50
            );
        }

        #region Derived Properties

        /// <summary>
        /// Whether this is a high hang time kick (> 4.5 seconds).
        /// High hang time = more coverage pressure = more likely fair catch.
        /// </summary>
        public bool IsHighHangTime => HangTime > 4.5;

        /// <summary>
        /// Whether this is a medium hang time kick (> 4.0 seconds).
        /// </summary>
        public bool IsMediumHangTime => HangTime > 4.0;

        /// <summary>
        /// Whether the returner is deep in their own territory (inside own 10).
        /// </summary>
        public bool IsDeepInOwnTerritory => ReceivingTeamFieldPosition < 10;

        /// <summary>
        /// Whether the returner is in the danger zone (inside own 20).
        /// </summary>
        public bool IsInDangerZone => ReceivingTeamFieldPosition < 20;

        /// <summary>
        /// Whether coverage is likely to be good based on hang time.
        /// </summary>
        public bool HasGoodCoverage => IsHighHangTime || IsMediumHangTime;

        /// <summary>
        /// Whether the returner is skilled at catching (above average).
        /// </summary>
        public bool IsGoodCatcher => ReturnerCatching >= 70;

        /// <summary>
        /// Whether the returner has high awareness (above average).
        /// </summary>
        public bool HasHighAwareness => ReturnerAwareness >= 70;

        #endregion
    }
}
