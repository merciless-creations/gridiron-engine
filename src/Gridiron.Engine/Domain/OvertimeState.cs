using Gridiron.Engine.Simulation.Overtime;
using System.Collections.Generic;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Tracks the state of overtime play including possession history,
    /// scoring events, and period information.
    /// </summary>
    public class OvertimeState
    {
        /// <summary>
        /// Gets or sets whether the game is currently in overtime.
        /// </summary>
        public bool IsInOvertime { get; set; }

        /// <summary>
        /// Gets or sets the current overtime period number (1-based).
        /// </summary>
        public int CurrentPeriod { get; set; } = 1;

        /// <summary>
        /// Gets or sets the team that won the overtime coin toss.
        /// </summary>
        public Possession CoinTossWinner { get; set; }

        /// <summary>
        /// Gets or sets the team that received first in overtime.
        /// </summary>
        public Possession FirstPossessionTeam { get; set; }

        /// <summary>
        /// Gets or sets whether the first team has completed their initial possession.
        /// </summary>
        public bool FirstPossessionComplete { get; set; }

        /// <summary>
        /// Gets or sets whether the second team has completed their initial possession.
        /// </summary>
        public bool SecondPossessionComplete { get; set; }

        /// <summary>
        /// Gets or sets whether the game is in sudden death mode (next score wins).
        /// </summary>
        public bool IsSuddenDeath { get; set; }

        /// <summary>
        /// Gets or sets the score by the first possession team in the current period.
        /// </summary>
        public int FirstTeamPeriodScore { get; set; }

        /// <summary>
        /// Gets or sets the score by the second possession team in the current period.
        /// </summary>
        public int SecondTeamPeriodScore { get; set; }

        /// <summary>
        /// Gets or sets the list of all overtime possessions for tracking.
        /// </summary>
        public List<OvertimePossession> Possessions { get; set; } = new();

        /// <summary>
        /// Gets or sets the home team timeouts remaining in overtime.
        /// </summary>
        public int HomeTimeoutsRemaining { get; set; }

        /// <summary>
        /// Gets or sets the away team timeouts remaining in overtime.
        /// </summary>
        public int AwayTimeoutsRemaining { get; set; }

        /// <summary>
        /// Gets or sets the team currently on offense in overtime.
        /// </summary>
        public Possession CurrentPossessionTeam { get; set; }

        /// <summary>
        /// Gets or sets the number of possessions completed in the current period.
        /// </summary>
        public int PossessionsInCurrentPeriod { get; set; }

        /// <summary>
        /// Gets the second possession team (the team that didn't receive first).
        /// </summary>
        public Possession SecondPossessionTeam =>
            FirstPossessionTeam == Possession.Home ? Possession.Away : Possession.Home;

        /// <summary>
        /// Resets the state for a new overtime period.
        /// </summary>
        public void StartNewPeriod()
        {
            CurrentPeriod++;
            FirstPossessionComplete = false;
            SecondPossessionComplete = false;
            FirstTeamPeriodScore = 0;
            SecondTeamPeriodScore = 0;
            PossessionsInCurrentPeriod = 0;
            // In NFL playoffs, teams alternate who gets first possession each period
            // Swap first possession team for new period
            FirstPossessionTeam = FirstPossessionTeam == Possession.Home ? Possession.Away : Possession.Home;
            CurrentPossessionTeam = FirstPossessionTeam;
        }
    }

    /// <summary>
    /// Represents a single possession during overtime.
    /// </summary>
    public class OvertimePossession
    {
        /// <summary>
        /// Gets or sets the overtime period number.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Gets or sets the team that had possession.
        /// </summary>
        public Possession Team { get; set; }

        /// <summary>
        /// Gets or sets the starting field position.
        /// </summary>
        public int StartingFieldPosition { get; set; }

        /// <summary>
        /// Gets or sets the reason the possession ended (null if still active or scored).
        /// </summary>
        public PossessionEndReason? EndReason { get; set; }

        /// <summary>
        /// Gets or sets the type of score if the possession resulted in points.
        /// </summary>
        public OvertimeScoreType? ScoreType { get; set; }

        /// <summary>
        /// Gets or sets the points scored during this possession.
        /// </summary>
        public int PointsScored { get; set; }

        /// <summary>
        /// Gets or sets the list of plays executed during this possession.
        /// </summary>
        public List<IPlay> Plays { get; set; } = new();
    }
}
