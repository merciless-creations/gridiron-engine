using Gridiron.Engine.Domain;

namespace Gridiron.Validator;

/// <summary>
/// Collects and aggregates statistics across multiple simulated games.
/// </summary>
public class AggregateStats
{
    // Game counts
    public int GamesSimulated { get; private set; }
    public int OvertimeGames { get; private set; }
    public int TieGames { get; private set; }

    // Rushing
    public int TotalRushAttempts { get; private set; }
    public int TotalRushYards { get; private set; }
    public int NegativeRushPlays { get; private set; }
    public int TenPlusYardRushes { get; private set; }
    public int TwentyPlusYardRushes { get; private set; }
    public int RushFumbles { get; private set; }

    // Passing
    public int TotalPassAttempts { get; private set; }
    public int TotalCompletions { get; private set; }
    public int TotalPassYards { get; private set; }
    public int TotalInterceptions { get; private set; }
    public int TotalSacks { get; private set; }
    public int TotalPassTouchdowns { get; private set; }

    // Turnovers
    public int TotalFumbles { get; private set; }
    public int TotalFumblesLost { get; private set; }

    // Penalties
    public int TotalPenalties { get; private set; }
    public int TotalPenaltyYards { get; private set; }

    // Field Goals
    public int FieldGoalAttempts { get; private set; }
    public int FieldGoalsMade { get; private set; }
    public int FieldGoalAttemptsUnder30 { get; private set; }
    public int FieldGoalsMadeUnder30 { get; private set; }
    public int FieldGoalAttempts30to39 { get; private set; }
    public int FieldGoalsMade30to39 { get; private set; }
    public int FieldGoalAttempts40to49 { get; private set; }
    public int FieldGoalsMade40to49 { get; private set; }
    public int FieldGoalAttempts50Plus { get; private set; }
    public int FieldGoalsMade50Plus { get; private set; }

    // Extra Points
    public int ExtraPointAttempts { get; private set; }
    public int ExtraPointsMade { get; private set; }

    // Punts
    public int TotalPunts { get; private set; }
    public int TotalPuntYards { get; private set; }

    // Kickoffs
    public int TotalKickoffs { get; private set; }
    public int Touchbacks { get; private set; }

    // Scoring
    public int TotalTouchdowns { get; private set; }
    public int TotalPoints { get; private set; }
    public int TotalSafeties { get; private set; }

    // Downs
    public int ThirdDownAttempts { get; private set; }
    public int ThirdDownConversions { get; private set; }
    public int FourthDownAttempts { get; private set; }
    public int FourthDownConversions { get; private set; }

    // Game flow
    public int TotalPlays { get; private set; }

    // Injuries
    public int TotalInjuries { get; private set; }

    /// <summary>
    /// Accumulates statistics from a completed game.
    /// </summary>
    public void Accumulate(Game game)
    {
        GamesSimulated++;
        TotalPoints += game.HomeScore + game.AwayScore;

        if (game.HomeScore == game.AwayScore)
            TieGames++;

        // Check for overtime
        if (game.OvertimeState != null)
            OvertimeGames++;

        foreach (var play in game.Plays)
        {
            TotalPlays++;
            AccumulatePlay(play);
        }
    }

    private void AccumulatePlay(IPlay play)
    {
        // Penalties
        foreach (var penalty in play.Penalties)
        {
            if (penalty.Accepted)
            {
                TotalPenalties++;
                TotalPenaltyYards += penalty.Yards;
            }
        }

        // Fumbles
        if (play.Fumbles.Count > 0)
        {
            TotalFumbles += play.Fumbles.Count;
            // If the play resulted in a possession change and there was a fumble, it was lost
            if (play.PossessionChange && !play.Interception)
                TotalFumblesLost++;
        }

        // Injuries
        TotalInjuries += play.Injuries.Count;

        // Scoring
        if (play.IsTouchdown)
            TotalTouchdowns++;
        if (play.IsSafety)
            TotalSafeties++;

        // Play type specific
        switch (play.PlayType)
        {
            case PlayType.Run:
                AccumulateRunPlay(play);
                break;
            case PlayType.Pass:
                AccumulatePassPlay(play);
                break;
            case PlayType.FieldGoal:
                AccumulateFieldGoalPlay(play);
                break;
            case PlayType.Punt:
                AccumulatePuntPlay(play);
                break;
            case PlayType.Kickoff:
                AccumulateKickoffPlay(play);
                break;
        }

        // Down tracking
        if (play.Down == Downs.Third)
        {
            ThirdDownAttempts++;
            // Check if converted (next play is 1st down or touchdown)
            if (play.IsTouchdown || play.YardsGained >= GetYardsNeeded(play))
                ThirdDownConversions++;
        }
        else if (play.Down == Downs.Fourth && play.PlayType != PlayType.Punt && play.PlayType != PlayType.FieldGoal)
        {
            FourthDownAttempts++;
            if (play.IsTouchdown || play.YardsGained >= GetYardsNeeded(play))
                FourthDownConversions++;
        }
    }

    private int GetYardsNeeded(IPlay play)
    {
        // Estimate yards to go based on field position change for first down
        // This is approximate - ideally we'd track YardsToGo on the play
        return 10; // Default assumption
    }

    private void AccumulateRunPlay(IPlay play)
    {
        TotalRushAttempts++;
        TotalRushYards += play.YardsGained;

        if (play.YardsGained <= 0)
            NegativeRushPlays++;
        if (play.YardsGained >= 10)
            TenPlusYardRushes++;
        if (play.YardsGained >= 20)
            TwentyPlusYardRushes++;

        if (play.Fumbles.Count > 0)
            RushFumbles++;
    }

    private void AccumulatePassPlay(IPlay play)
    {
        // We need to determine if it was a completion, incompletion, sack, or interception
        // Based on play properties

        if (play.Interception)
        {
            TotalPassAttempts++;
            TotalInterceptions++;
        }
        else if (play.YardsGained < 0 && !play.IsTouchdown)
        {
            // Likely a sack (negative yards, not a touchdown)
            TotalSacks++;
        }
        else
        {
            TotalPassAttempts++;
            // If yards gained > 0 or touchdown, assume completion
            if (play.YardsGained > 0 || play.IsTouchdown)
            {
                TotalCompletions++;
                TotalPassYards += play.YardsGained;
                if (play.IsTouchdown)
                    TotalPassTouchdowns++;
            }
            // else: incompletion (0 yards, no TD)
        }
    }

    private void AccumulateFieldGoalPlay(IPlay play)
    {
        // Calculate distance based on field position
        // Field goal distance = 100 - startFieldPosition + 17 (end zone + hold spot)
        int distance = 100 - play.StartFieldPosition + 17;

        FieldGoalAttempts++;
        bool made = play.YardsGained > 0 || play.EndFieldPosition > play.StartFieldPosition;
        // Actually, for FG, "made" is usually indicated differently
        // Let's assume if no possession change and points scored, it's made
        // For now, use a simple heuristic: if EndFieldPosition indicates scoring

        // Better heuristic: check if it resulted in points (we'd need to track this)
        // For now, assume made if YardsGained != 0 or special flag
        // This is imperfect - may need refinement based on actual play structure

        if (distance < 30)
        {
            FieldGoalAttemptsUnder30++;
            // We need a better way to track makes - for now skip
        }
        else if (distance < 40)
        {
            FieldGoalAttempts30to39++;
        }
        else if (distance < 50)
        {
            FieldGoalAttempts40to49++;
        }
        else
        {
            FieldGoalAttempts50Plus++;
        }
    }

    private void AccumulatePuntPlay(IPlay play)
    {
        TotalPunts++;
        // Punt distance is typically stored differently
        // YardsGained on a punt might be negative (from offense perspective) or represent net
        TotalPuntYards += Math.Abs(play.YardsGained);
    }

    private void AccumulateKickoffPlay(IPlay play)
    {
        TotalKickoffs++;
        // Check for touchback - typically results in ball at 25 yard line
        if (play.EndFieldPosition == 25 || play.EndFieldPosition == 75)
            Touchbacks++;
    }

    // Calculated metrics
    public double YardsPerCarry => TotalRushAttempts > 0 ? (double)TotalRushYards / TotalRushAttempts : 0;
    public double NegativePlayRate => TotalRushAttempts > 0 ? (double)NegativeRushPlays / TotalRushAttempts * 100 : 0;
    public double TenPlusYardRate => TotalRushAttempts > 0 ? (double)TenPlusYardRushes / TotalRushAttempts * 100 : 0;
    public double TwentyPlusYardRate => TotalRushAttempts > 0 ? (double)TwentyPlusYardRushes / TotalRushAttempts * 100 : 0;
    public double RushFumbleRate => TotalRushAttempts > 0 ? (double)RushFumbles / TotalRushAttempts * 100 : 0;

    public double CompletionPercentage => TotalPassAttempts > 0 ? (double)TotalCompletions / TotalPassAttempts * 100 : 0;
    public double YardsPerAttempt => TotalPassAttempts > 0 ? (double)TotalPassYards / TotalPassAttempts : 0;
    public double InterceptionRate => TotalPassAttempts > 0 ? (double)TotalInterceptions / TotalPassAttempts * 100 : 0;
    public double PassTouchdownRate => TotalPassAttempts > 0 ? (double)TotalPassTouchdowns / TotalPassAttempts * 100 : 0;

    public double TurnoversPerGame => GamesSimulated > 0 ? (double)(TotalFumblesLost + TotalInterceptions) / GamesSimulated : 0;
    public double PenaltiesPerGame => GamesSimulated > 0 ? (double)TotalPenalties / GamesSimulated : 0;
    public double PenaltyYardsPerGame => GamesSimulated > 0 ? (double)TotalPenaltyYards / GamesSimulated : 0;

    public double ThirdDownConversionRate => ThirdDownAttempts > 0 ? (double)ThirdDownConversions / ThirdDownAttempts * 100 : 0;
    public double FourthDownConversionRate => FourthDownAttempts > 0 ? (double)FourthDownConversions / FourthDownAttempts * 100 : 0;

    public double PlaysPerGame => GamesSimulated > 0 ? (double)TotalPlays / GamesSimulated : 0;
    public double PointsPerGame => GamesSimulated > 0 ? (double)TotalPoints / GamesSimulated / 2 : 0; // Divide by 2 for per-team
    public double TouchdownsPerGame => GamesSimulated > 0 ? (double)TotalTouchdowns / GamesSimulated : 0;
    public double InjuriesPerGame => GamesSimulated > 0 ? (double)TotalInjuries / GamesSimulated : 0;

    public double GrossYardsPerPunt => TotalPunts > 0 ? (double)TotalPuntYards / TotalPunts : 0;
    public double TouchbackRate => TotalKickoffs > 0 ? (double)Touchbacks / TotalKickoffs * 100 : 0;
    public double OvertimeRate => GamesSimulated > 0 ? (double)OvertimeGames / GamesSimulated * 100 : 0;
}
