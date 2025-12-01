using Gridiron.Engine.Domain;

namespace Gridiron.Engine.Simulation.Overtime
{
    /// <summary>
    /// Base class for NFL overtime rules providers.
    /// Contains shared logic for NFL 2024 overtime rules.
    ///
    /// Key rules:
    /// - 10-minute period (not 15)
    /// - 2 timeouts per team
    /// - Coin toss determines first possession
    /// - First possession: TD = win, FG = other team gets ball
    /// - After both possess: sudden death
    /// </summary>
    public abstract class NflOvertimeRulesProviderBase : IOvertimeRulesProvider
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }

        /// <inheritdoc/>
        public int OvertimePeriodDuration => 600; // 10 minutes

        /// <inheritdoc/>
        public int TimeoutsPerTeam => 2;

        /// <inheritdoc/>
        public int? FixedStartingFieldPosition => null; // Uses kickoff

        /// <inheritdoc/>
        public bool HasOvertimeCoinToss => true;

        /// <inheritdoc/>
        public abstract bool AllowsTies { get; }

        /// <inheritdoc/>
        public abstract int MaxOvertimePeriods { get; }

        /// <inheritdoc/>
        public virtual OvertimeGameEndResult ShouldGameEnd(OvertimeState state, OvertimeScoreType scoreType, Possession scoringTeam)
        {
            // Defensive touchdown always ends the game immediately
            if (scoreType == OvertimeScoreType.DefensiveTouchdown)
            {
                return OvertimeGameEndResult.GameOver;
            }

            // Sudden death - any score wins
            if (state.IsSuddenDeath)
            {
                return OvertimeGameEndResult.GameOver;
            }

            // First possession
            if (!state.FirstPossessionComplete)
            {
                if (scoreType == OvertimeScoreType.Touchdown)
                {
                    // TD on first possession wins immediately
                    return OvertimeGameEndResult.GameOver;
                }
                // FG, safety - game continues, other team gets possession
                return OvertimeGameEndResult.Continue;
            }

            // Second possession
            if (!state.SecondPossessionComplete)
            {
                int firstTeamScore = state.FirstTeamPeriodScore;
                int secondTeamScore = state.SecondTeamPeriodScore + GetPointsForScore(scoreType);

                if (secondTeamScore > firstTeamScore)
                {
                    // Second team scored more - they win
                    return OvertimeGameEndResult.GameOver;
                }

                // Tied or behind - continue to sudden death
                return OvertimeGameEndResult.Continue;
            }

            // Both teams have possessed - we're in sudden death
            return OvertimeGameEndResult.GameOver;
        }

        /// <inheritdoc/>
        public virtual OvertimePossessionResult GetNextPossessionAction(OvertimeState state, PossessionEndReason reason)
        {
            if (!state.FirstPossessionComplete)
            {
                // First team's possession ended without winning TD - other team gets ball
                return OvertimePossessionResult.OtherTeamGetsBall;
            }

            if (!state.SecondPossessionComplete)
            {
                // Second team's possession ended
                if (state.SecondTeamPeriodScore < state.FirstTeamPeriodScore)
                {
                    // Second team failed to match - first team wins
                    return OvertimePossessionResult.GameOver;
                }

                // Scores are equal - go to sudden death
                return OvertimePossessionResult.SuddenDeath;
            }

            // Already in sudden death
            if (reason == PossessionEndReason.TimeExpired)
            {
                // Period ended - check if we should start new period or end in tie
                return HandlePeriodEnd(state);
            }

            // Possession changed in sudden death - other team gets ball
            return OvertimePossessionResult.OtherTeamGetsBall;
        }

        /// <summary>
        /// Handles the end of an overtime period.
        /// </summary>
        protected abstract OvertimePossessionResult HandlePeriodEnd(OvertimeState state);

        /// <inheritdoc/>
        public abstract bool ShouldStartNewPeriod(OvertimeState state);

        /// <inheritdoc/>
        public int GetStartingFieldPosition(OvertimeState state, Possession possession)
        {
            // NFL uses normal kickoff - ball starts at 35-yard line for kickoff
            // After touchback, ball is placed at 25-yard line (field position 25 for home, 75 for away)
            return possession == Possession.Home ? 25 : 75;
        }

        /// <inheritdoc/>
        public (Downs down, int yardsToGo) GetStartingDownAndDistance(OvertimeState state)
        {
            return (Downs.First, 10);
        }

        /// <inheritdoc/>
        public bool IsTwoPointConversionRequired(OvertimeState state) => false;

        /// <inheritdoc/>
        public bool IsTwoPointPlayOnly(OvertimeState state) => false;

        /// <inheritdoc/>
        public bool UsesKickoff(OvertimeState state) => true;

        /// <summary>
        /// Gets the point value for a score type.
        /// </summary>
        protected static int GetPointsForScore(OvertimeScoreType scoreType)
        {
            return scoreType switch
            {
                OvertimeScoreType.Touchdown => 6,
                OvertimeScoreType.FieldGoal => 3,
                OvertimeScoreType.Safety => 2,
                OvertimeScoreType.DefensiveTouchdown => 6,
                _ => 0
            };
        }
    }
}
