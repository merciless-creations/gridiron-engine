using Gridiron.Engine.Domain.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Gridiron.Engine.Domain.Helpers;
using Half = Gridiron.Engine.Domain.Time.Half;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a football game between two teams, including all game state and scoring.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Gets or sets the home team.
        /// </summary>
        public Team HomeTeam { get; set; } = null!;

        /// <summary>
        /// Gets or sets the away team.
        /// </summary>
        public Team AwayTeam { get; set; } = null!;

        /// <summary>
        /// Gets or sets the random seed for reproducible game simulation.
        /// When set, the game will produce deterministic results.
        /// </summary>
        public int? RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets the current play being executed.
        /// This is runtime state only and may be null between plays.
        /// </summary>
        public IPlay? CurrentPlay { get; set; }

        /// <summary>
        /// Gets or sets which team won the coin toss.
        /// </summary>
        public Possession WonCoinToss { get; set; }

        /// <summary>
        /// Gets or sets whether the team that won the coin toss deferred their choice to the second half.
        /// </summary>
        public bool DeferredPossession { get; set; }

        /// <summary>
        /// Gets or sets the list of all plays executed in the game.
        /// </summary>
        public List<IPlay> Plays { get; set; } = new List<IPlay>();

        /// <summary>
        /// Gets or sets the logger for game events and play-by-play output.
        /// Defaults to NullLogger so tests don't need to configure it.
        /// </summary>
        public ILogger Logger { get; set; } = NullLogger.Instance;

        /// <summary>
        /// Gets or sets the current field position (0-100).
        /// This is an absolute position that does not flip on possession changes.
        /// Use <see cref="FormatFieldPosition(Possession)"/> for NFL notation display.
        /// </summary>
        public int FieldPosition { get; set; } = 0;

        /// <summary>
        /// Gets or sets the yards needed for a first down.
        /// </summary>
        public int YardsToGo { get; set; } = 10;

        /// <summary>
        /// Gets or sets the current down (First, Second, Third, Fourth, or None for kickoffs).
        /// </summary>
        public Downs CurrentDown { get; set; } = Downs.First;

        /// <summary>
        /// Gets or sets the home team's current score.
        /// </summary>
        public int HomeScore { get; set; } = 0;

        /// <summary>
        /// Gets or sets the away team's current score.
        /// </summary>
        public int AwayScore { get; set; } = 0;

        /// <summary>
        /// Gets the total time remaining in the game in seconds.
        /// Calculated by summing remaining time across all quarters.
        /// </summary>
        public int TimeRemaining =>
            Halves[0].Quarters[0].TimeRemaining +
            Halves[0].Quarters[1].TimeRemaining +
            Halves[1].Quarters[0].TimeRemaining +
            Halves[1].Quarters[1].TimeRemaining;

        /// <summary>
        /// Gets the list of halves in the game, each containing two quarters.
        /// </summary>
        public List<Half> Halves { get; } = new List<Half>() {
            new FirstHalf(),
            new SecondHalf()
        };

        /// <summary>
        /// Gets or sets the current quarter being played.
        /// </summary>
        public Quarter CurrentQuarter { get; set; }

        /// <summary>
        /// Gets or sets the current half being played.
        /// </summary>
        public Half CurrentHalf { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// Creates a game with 3600 seconds (60 minutes) remaining, starting in the first quarter.
        /// </summary>
        public Game()
        {
            CurrentQuarter = Halves[0].Quarters[0];
            CurrentHalf = Halves[0];
        }

        // ========================================
        // FIELD POSITION HELPERS
        // ========================================

        /// <summary>
        /// Gets the offensive team (team with current possession)
        /// </summary>
        public Team GetOffensiveTeam(Possession possession)
        {
            return possession == Possession.Home ? HomeTeam : AwayTeam;
        }

        /// <summary>
        /// Gets the defensive team (team without possession)
        /// </summary>
        public Team GetDefensiveTeam(Possession possession)
        {
            return possession == Possession.Home ? AwayTeam : HomeTeam;
        }

        /// <summary>
        /// Formats current field position in NFL notation
        /// </summary>
        public string FormatFieldPosition(Possession possession)
        {
            return FieldPositionHelper.FormatFieldPosition(FieldPosition, HomeTeam, AwayTeam);
        }

        /// <summary>
        /// Formats a specific field position in NFL notation
        /// </summary>
        public string FormatFieldPosition(int fieldPosition, Possession possession)
        {
            return FieldPositionHelper.FormatFieldPosition(fieldPosition, HomeTeam, AwayTeam);
        }

        /// <summary>
        /// Formats current field position with "yard line" suffix
        /// </summary>
        public string FormatFieldPositionWithYardLine(Possession possession)
        {
            return FieldPositionHelper.FormatFieldPositionWithYardLine(FieldPosition, HomeTeam, AwayTeam);
        }

        // ========================================
        // SCORING METHODS
        // ========================================

        /// <summary>
        /// Adds a touchdown (6 points) to the scoring team
        /// </summary>
        /// <param name="scoringTeam">The team that scored the touchdown</param>
        public void AddTouchdown(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 6;
                Logger.LogInformation($"TOUCHDOWN! Home team scores! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 6;
                Logger.LogInformation($"TOUCHDOWN! Away team scores! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a field goal (3 points) to the scoring team
        /// </summary>
        /// <param name="scoringTeam">The team that kicked the field goal</param>
        public void AddFieldGoal(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 3;
                Logger.LogInformation($"FIELD GOAL is good! Home team scores! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 3;
                Logger.LogInformation($"FIELD GOAL is good! Away team scores! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a safety (2 points) to the defending team.
        /// NOTE: The defending team (the team that got the safety) receives the points.
        /// </summary>
        /// <param name="defendingTeam">The team that forced the safety (receives the 2 points)</param>
        public void AddSafety(Possession defendingTeam)
        {
            if (defendingTeam == Possession.Home)
            {
                HomeScore += 2;
                Logger.LogInformation($"SAFETY! Home team gets 2 points! Home {HomeScore}, Away {AwayScore}");
            }
            else if (defendingTeam == Possession.Away)
            {
                AwayScore += 2;
                Logger.LogInformation($"SAFETY! Away team gets 2 points! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds an extra point (1 point) to the scoring team after a touchdown
        /// </summary>
        /// <param name="scoringTeam">The team that kicked the extra point</param>
        public void AddExtraPoint(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 1;
                Logger.LogInformation($"Extra point is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 1;
                Logger.LogInformation($"Extra point is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a two-point conversion (2 points) to the scoring team after a touchdown
        /// </summary>
        /// <param name="scoringTeam">The team that converted the two-point attempt</param>
        public void AddTwoPointConversion(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 2;
                Logger.LogInformation($"Two-point conversion is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 2;
                Logger.LogInformation($"Two-point conversion is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
        }
    }

    /// <summary>
    /// Defines the player positions in American football.
    /// </summary>
    public enum Positions
    {
        /// <summary>Quarterback - leads the offense and throws passes.</summary>
        QB,
        /// <summary>Center - snaps the ball to the quarterback.</summary>
        C,
        /// <summary>Guard - offensive lineman who blocks for runs and passes.</summary>
        G,
        /// <summary>Tackle - offensive lineman on the outside of the line.</summary>
        T,
        /// <summary>Tight End - hybrid blocker and receiver.</summary>
        TE,
        /// <summary>Wide Receiver - primary pass catcher.</summary>
        WR,
        /// <summary>Running Back - carries the ball on rushing plays.</summary>
        RB,
        /// <summary>Defensive Tackle - interior defensive lineman.</summary>
        DT,
        /// <summary>Defensive End - edge defender and pass rusher.</summary>
        DE,
        /// <summary>Linebacker - defensive player behind the line.</summary>
        LB,
        /// <summary>Outside Linebacker - linebacker on the edge of the defense.</summary>
        OLB,
        /// <summary>Cornerback - covers wide receivers.</summary>
        CB,
        /// <summary>Safety - deep defensive back.</summary>
        S,
        /// <summary>Kicker - kicks field goals and extra points.</summary>
        K,
        /// <summary>Punter - punts the ball on fourth down.</summary>
        P,
        /// <summary>Fullback - blocking back who also carries the ball.</summary>
        FB,
        /// <summary>Free Safety - deep coverage safety.</summary>
        FS,
        /// <summary>Long Snapper - specialist who snaps for punts and field goals.</summary>
        LS,
        /// <summary>Holder - holds the ball for field goals and extra points.</summary>
        H,
    }

    /// <summary>
    /// Defines the down number in a series of plays.
    /// </summary>
    public enum Downs
    {
        /// <summary>First down - first of four attempts to gain 10 yards.</summary>
        First,
        /// <summary>Second down - second attempt.</summary>
        Second,
        /// <summary>Third down - third attempt.</summary>
        Third,
        /// <summary>Fourth down - final attempt before turnover on downs.</summary>
        Fourth,
        /// <summary>No down - used for kickoffs and other non-down plays.</summary>
        None
    }

    /// <summary>
    /// Defines which team has possession of the ball.
    /// </summary>
    public enum Possession
    {
        /// <summary>No team has possession (e.g., during a kickoff in flight).</summary>
        None,
        /// <summary>The home team has possession.</summary>
        Home,
        /// <summary>The away team has possession.</summary>
        Away
    }

    /// <summary>
    /// Defines the types of plays that can be executed.
    /// </summary>
    public enum PlayType
    {
        /// <summary>A kickoff to start the half or after a score.</summary>
        Kickoff,
        /// <summary>A field goal attempt.</summary>
        FieldGoal,
        /// <summary>A punt to change field position.</summary>
        Punt,
        /// <summary>A passing play.</summary>
        Pass,
        /// <summary>A rushing play.</summary>
        Run
    }
}
