using Gridiron.Engine.Domain.Helpers;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Encapsulates fourth down decision-making logic.
    /// Determines whether to go for it, punt, or attempt a field goal based on game situation.
    ///
    /// <para><b>DECISION ALGORITHM OVERVIEW</b></para>
    ///
    /// <para>The engine uses a probability-based approach with three phases:</para>
    ///
    /// <para><b>Phase 1: Forced Decisions (Desperation Mode)</b></para>
    /// <para>
    /// Certain game situations mandate going for it regardless of distance:
    /// <list type="bullet">
    ///   <item>Trailing by 9+ points with under 2 minutes → Always go for it</item>
    ///   <item>Trailing by any amount with under 30 seconds → Always go for it</item>
    ///   <item>Trailing in opponent territory, under 5 minutes, 4th and short → Always go for it</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>Phase 2: Probability Calculation</b></para>
    /// <para>
    /// If not in desperation mode, calculate go-for-it probability:
    /// <code>
    ///   FinalProbability = BaseProbability + FieldPositionMod + ScoreMod + TimeMod + FGRangeMod
    /// </code>
    /// Then roll against this probability to decide whether to go for it.
    /// </para>
    ///
    /// <para><b>Phase 3: Punt vs Field Goal Selection</b></para>
    /// <para>
    /// If not going for it, choose between punt and field goal based on:
    /// <list type="bullet">
    ///   <item>Field goal distance (chip shot → normal → long → out of range)</item>
    ///   <item>Score differential (need points → attempt longer FGs)</item>
    ///   <item>No-punt zone (inside opponent's 35 → must kick FG or go for it)</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>BASE PROBABILITIES BY DISTANCE</b></para>
    /// <para>
    /// <code>
    /// | Yards to Go | Base Go-For-It % |
    /// |-------------|------------------|
    /// | 1           | 65%              |
    /// | 2           | 35%              |
    /// | 3           | 20%              |
    /// | 4-5         | 8%               |
    /// | 6-10        | 3%               |
    /// | 11+         | 1%               |
    /// </code>
    /// </para>
    ///
    /// <para><b>SITUATIONAL MODIFIERS</b></para>
    /// <para>
    /// <code>
    /// Field Position:
    ///   Red zone (≤20 yards to goal)        → +15%
    ///   Opponent territory (≤50 yards)      → +8%
    ///   Own territory (≥70 yards to goal)   → -15%
    ///
    /// Score Differential:
    ///   Trailing by 8+ points               → +20%
    ///   Trailing by 1-7 points              → +10%
    ///   Leading by 15+ points               → -15%
    ///
    /// Time Remaining (under 5 minutes):
    ///   Trailing                            → +15%
    ///   Leading                             → -10%
    ///
    /// Field Goal Availability:
    ///   Chip shot (≤35 yards) available     → -25% (unless 4th and ≤2)
    /// </code>
    /// </para>
    ///
    /// <para><b>EXAMPLE CALCULATIONS</b></para>
    /// <para>
    /// <i>Example 1: 4th and 1 at opponent's 40, trailing by 3, 8 minutes left</i>
    /// <code>
    ///   Base:           65%
    ///   Opponent terr:  +8%
    ///   Trailing small: +10%
    ///   Total:          83% chance to go for it
    /// </code>
    /// </para>
    /// <para>
    /// <i>Example 2: 4th and 5 at own 30, tied, 10 minutes left</i>
    /// <code>
    ///   Base:           8%
    ///   Own territory:  -15%
    ///   Total:          0% (clamped) → Will punt
    /// </code>
    /// </para>
    /// <para>
    /// <i>Example 3: 4th and 3 at opponent's 20, down 10, 1:30 left</i>
    /// <code>
    ///   Desperation mode triggered → 100% go for it
    /// </code>
    /// </para>
    /// </summary>
    public class FourthDownDecisionEngine
    {
        private readonly ISeedableRandom _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="FourthDownDecisionEngine"/> class.
        /// </summary>
        /// <param name="rng">The random number generator for probabilistic decisions.</param>
        public FourthDownDecisionEngine(ISeedableRandom rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Determines the best fourth down decision based on game situation.
        /// </summary>
        /// <param name="context">The current game context for the decision.</param>
        /// <returns>The recommended fourth down decision.</returns>
        public FourthDownDecision Decide(FourthDownContext context)
        {
            // ══════════════════════════════════════════════════════════════════
            // STEP 1: Calculate key derived values
            // ══════════════════════════════════════════════════════════════════

            // Field goal distance = yards to goal + 17 (10 yard end zone + 7 yard snap/hold)
            int fieldGoalDistance = CalculateFieldGoalDistance(context.FieldPosition, context.IsHome);

            // Is a field goal attempt realistic? (Max ~60 yards in NFL)
            bool fieldGoalInRange = fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_MAX_RANGE;

            // Are we too close to punt effectively? (Inside opponent's 35)
            // A punt from the 35 would go into or through the end zone
            bool tooCloseForPunt = GetYardsToOpponentGoal(context.FieldPosition, context.IsHome)
                                   <= FourthDownConstants.NO_PUNT_ZONE_YARDS;

            // ══════════════════════════════════════════════════════════════════
            // STEP 2: Check for forced "go for it" situations (desperation mode)
            // ══════════════════════════════════════════════════════════════════

            if (ShouldAlwaysGoForIt(context))
            {
                return FourthDownDecision.GoForIt;
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 3: Calculate go-for-it probability
            // ══════════════════════════════════════════════════════════════════

            // Start with base probability based on yards needed
            double goForItProbability = GetBaseGoForItProbability(context.YardsToGo);

            // Apply situational modifiers (field position, score, time, FG availability)
            goForItProbability = ApplySituationalModifiers(goForItProbability, context, fieldGoalInRange);

            // ══════════════════════════════════════════════════════════════════
            // STEP 4: Roll against probability to decide go-for-it
            // ══════════════════════════════════════════════════════════════════

            if (_rng.NextDouble() < goForItProbability)
            {
                return FourthDownDecision.GoForIt;
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 5: Not going for it - choose between punt and field goal
            // ══════════════════════════════════════════════════════════════════

            // If FG is in range and we're not too close to punt, evaluate both options
            if (fieldGoalInRange && !tooCloseForPunt)
            {
                return ChooseBetweenPuntAndFieldGoal(context, fieldGoalDistance);
            }

            // If too close to punt (no-punt zone), must attempt field goal
            if (tooCloseForPunt)
            {
                return FourthDownDecision.AttemptFieldGoal;
            }

            // Default: punt for field position
            return FourthDownDecision.Punt;
        }

        /// <summary>
        /// Determines if the situation mandates going for it regardless of normal probabilities.
        ///
        /// <para>Three desperation scenarios trigger automatic go-for-it:</para>
        /// <list type="number">
        ///   <item><b>Desperation Mode:</b> Trailing by 9+ with under 2 minutes.
        ///         You need two scores and can't afford to give the ball away.</item>
        ///   <item><b>Last Chance:</b> Trailing by any amount with under 30 seconds.
        ///         There's no time for another possession after a punt.</item>
        ///   <item><b>Aggressive Mode:</b> Trailing + in opponent territory + under 5 min + short yardage.
        ///         High reward, acceptable risk situation.</item>
        /// </list>
        /// </summary>
        private bool ShouldAlwaysGoForIt(FourthDownContext context)
        {
            // Scenario 1: Trailing by more than one possession with less than 2 minutes
            // You need multiple scores - can't afford to punt
            bool desperationMode = context.ScoreDifferential < -8 &&
                                   context.TimeRemainingSeconds < FourthDownConstants.DESPERATION_TIME_SECONDS;

            // Scenario 2: Trailing by any amount with less than 30 seconds
            // This is likely your last possession - punt is pointless
            bool lastChance = context.ScoreDifferential < 0 &&
                              context.TimeRemainingSeconds < FourthDownConstants.LAST_CHANCE_TIME_SECONDS;

            // Scenario 3: Trailing, in opponent territory, short yardage, under 5 minutes
            // High-value situation where going for it is strategically sound
            bool aggressiveMode = context.ScoreDifferential < 0 &&
                                  GetYardsToOpponentGoal(context.FieldPosition, context.IsHome) <= 50 &&
                                  context.TimeRemainingSeconds < FourthDownConstants.AGGRESSIVE_TIME_SECONDS &&
                                  context.YardsToGo <= FourthDownConstants.AGGRESSIVE_MAX_YARDS_TO_GO;

            return desperationMode || lastChance || aggressiveMode;
        }

        /// <summary>
        /// Gets the base probability of going for it based solely on yards to go.
        ///
        /// <para>These probabilities represent typical NFL coaching tendencies:</para>
        /// <list type="bullet">
        ///   <item>4th and 1: High conversion rate (~70%), so 65% go for it</item>
        ///   <item>4th and 2: Still reasonable (~55%), so 35% go for it</item>
        ///   <item>4th and 3+: Drops off rapidly as success rates decline</item>
        /// </list>
        ///
        /// <para>These are base rates that get modified by game situation.</para>
        /// </summary>
        private double GetBaseGoForItProbability(int yardsToGo)
        {
            if (yardsToGo <= 1)
                return FourthDownConstants.GO_FOR_IT_PROB_1_YARD;      // 65%
            if (yardsToGo <= 2)
                return FourthDownConstants.GO_FOR_IT_PROB_2_YARDS;     // 35%
            if (yardsToGo <= 3)
                return FourthDownConstants.GO_FOR_IT_PROB_3_YARDS;     // 20%
            if (yardsToGo <= 5)
                return FourthDownConstants.GO_FOR_IT_PROB_4_5_YARDS;   // 8%
            if (yardsToGo <= 10)
                return FourthDownConstants.GO_FOR_IT_PROB_6_10_YARDS;  // 3%

            return FourthDownConstants.GO_FOR_IT_PROB_LONG;            // 1%
        }

        /// <summary>
        /// Applies situational modifiers to the base go-for-it probability.
        ///
        /// <para>Modifiers are additive and can push probability up or down:</para>
        ///
        /// <para><b>Field Position Modifiers:</b></para>
        /// <list type="bullet">
        ///   <item>Red Zone (≤20 to goal): +15% - touchdown or FG likely either way</item>
        ///   <item>Opponent Territory (≤50): +8% - failure still leaves decent field position for defense</item>
        ///   <item>Own Territory (≥70 to goal): -15% - failure gives opponent short field</item>
        /// </list>
        ///
        /// <para><b>Score Differential Modifiers:</b></para>
        /// <list type="bullet">
        ///   <item>Trailing 8+: +20% - need to be aggressive to catch up</item>
        ///   <item>Trailing 1-7: +10% - need points, can take more risk</item>
        ///   <item>Leading 15+: -15% - protect the lead, no need for risk</item>
        /// </list>
        ///
        /// <para><b>Time Modifiers (under 5 minutes):</b></para>
        /// <list type="bullet">
        ///   <item>Trailing: +15% - urgency increases</item>
        ///   <item>Leading: -10% - run out clock, avoid turnovers</item>
        /// </list>
        ///
        /// <para><b>Field Goal Availability:</b></para>
        /// <list type="bullet">
        ///   <item>Chip shot (≤35 yards): -25% if 4th and 3+ - take the points</item>
        /// </list>
        /// </summary>
        private double ApplySituationalModifiers(double baseProbability, FourthDownContext context, bool fieldGoalInRange)
        {
            double probability = baseProbability;
            int yardsToGoal = GetYardsToOpponentGoal(context.FieldPosition, context.IsHome);

            // ─────────────────────────────────────────────────────────────────
            // FIELD POSITION MODIFIERS
            // ─────────────────────────────────────────────────────────────────

            if (yardsToGoal <= FourthDownConstants.RED_ZONE_YARDS)
            {
                // Red zone: More aggressive - even if we fail, opponent is pinned deep
                // and we likely have a FG option anyway
                probability += FourthDownConstants.RED_ZONE_GO_BONUS;
            }
            else if (yardsToGoal <= FourthDownConstants.OPPONENT_TERRITORY_YARDS)
            {
                // Opponent territory: Slightly more aggressive
                // Failure still leaves defense in okay position
                probability += FourthDownConstants.OPPONENT_TERRITORY_GO_BONUS;
            }
            else if (yardsToGoal >= FourthDownConstants.OWN_TERRITORY_CONSERVATIVE_YARDS)
            {
                // Deep in own territory: Be conservative
                // Failure gives opponent a short field for easy points
                probability -= FourthDownConstants.OWN_TERRITORY_GO_PENALTY;
            }

            // ─────────────────────────────────────────────────────────────────
            // SCORE DIFFERENTIAL MODIFIERS
            // ─────────────────────────────────────────────────────────────────

            if (context.ScoreDifferential < -7)
            {
                // Trailing by more than a TD: Must be aggressive to catch up
                probability += FourthDownConstants.TRAILING_BIG_GO_BONUS;
            }
            else if (context.ScoreDifferential < 0)
            {
                // Trailing by a small margin: Slightly more aggressive
                probability += FourthDownConstants.TRAILING_SMALL_GO_BONUS;
            }
            else if (context.ScoreDifferential > 14)
            {
                // Leading big: Protect the lead, no unnecessary risks
                probability -= FourthDownConstants.LEADING_BIG_GO_PENALTY;
            }

            // ─────────────────────────────────────────────────────────────────
            // TIME REMAINING MODIFIERS (late game adjustments)
            // ─────────────────────────────────────────────────────────────────

            if (context.TimeRemainingSeconds < FourthDownConstants.LATE_GAME_TIME_SECONDS)
            {
                if (context.ScoreDifferential < 0)
                {
                    // Trailing late: Urgency - each possession is precious
                    probability += FourthDownConstants.TRAILING_LATE_GO_BONUS;
                }
                else if (context.ScoreDifferential > 0)
                {
                    // Leading late: Milk the clock, avoid risky plays
                    probability -= FourthDownConstants.LEADING_LATE_GO_PENALTY;
                }
            }

            // ─────────────────────────────────────────────────────────────────
            // FIELD GOAL AVAILABILITY MODIFIER
            // ─────────────────────────────────────────────────────────────────

            if (fieldGoalInRange)
            {
                int fgDistance = CalculateFieldGoalDistance(context.FieldPosition, context.IsHome);
                if (fgDistance <= FourthDownConstants.FIELD_GOAL_CHIP_SHOT_YARDS)
                {
                    // Chip shot available: Strongly prefer taking the points
                    // Exception: 4th and 1-2 still worth going for (high success rate)
                    if (context.YardsToGo > 2)
                    {
                        probability -= FourthDownConstants.CHIP_SHOT_FG_GO_PENALTY;
                    }
                }
            }

            // Clamp final probability to valid range [0, 1]
            return Math.Clamp(probability, 0.0, 1.0);
        }

        /// <summary>
        /// Chooses between punt and field goal when both are viable options.
        ///
        /// <para>Decision is based on field goal distance and game situation:</para>
        ///
        /// <para><b>Chip Shot (≤35 yards):</b> Always attempt FG - near automatic</para>
        ///
        /// <para><b>Normal Range (36-45 yards):</b> 80% FG, 20% punt
        /// May punt if pinning opponent deep is more valuable</para>
        ///
        /// <para><b>Long Range (46-55 yards):</b> Situational
        /// <list type="bullet">
        ///   <item>If trailing by 3+: Attempt FG (need points)</item>
        ///   <item>Otherwise: 50/50 between FG and punt</item>
        /// </list>
        /// </para>
        ///
        /// <para><b>Very Long (56-60 yards):</b> Usually punt for field position</para>
        /// </summary>
        private FourthDownDecision ChooseBetweenPuntAndFieldGoal(FourthDownContext context, int fieldGoalDistance)
        {
            // Chip shot range (≤35 yards): ~95%+ make rate, always kick
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_CHIP_SHOT_YARDS)
            {
                return FourthDownDecision.AttemptFieldGoal;
            }

            // Normal range (36-45 yards): ~85% make rate, usually kick
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_NORMAL_RANGE)
            {
                // 80% field goal, 20% punt (punt if pinning them deep matters more)
                if (_rng.NextDouble() < 0.80)
                {
                    return FourthDownDecision.AttemptFieldGoal;
                }
            }

            // Long range (46-55 yards): ~65% make rate, more situational
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_LONG_RANGE)
            {
                // Score differential matters more for long kicks
                if (context.ScoreDifferential <= -3)
                {
                    // Need points - worth the risk
                    return FourthDownDecision.AttemptFieldGoal;
                }
                // Otherwise, coin flip between FG and punt
                if (_rng.NextDouble() < 0.50)
                {
                    return FourthDownDecision.AttemptFieldGoal;
                }
            }

            // Beyond comfortable range (56-60 yards): Usually punt
            // Miss gives opponent good field position, punt is safer
            return FourthDownDecision.Punt;
        }

        /// <summary>
        /// Calculates the field goal distance from the current field position.
        /// FG distance = yards to goal + 17 (10 yard end zone + 7 yard snap/hold)
        /// </summary>
        private int CalculateFieldGoalDistance(int fieldPosition, bool isHome)
        {
            int yardsToGoal = GetYardsToOpponentGoal(fieldPosition, isHome);
            return yardsToGoal + 17;
        }

        /// <summary>
        /// Gets the yards remaining to the opponent's goal line.
        /// Home team attacks position 100, Away team attacks position 0.
        /// </summary>
        private int GetYardsToOpponentGoal(int fieldPosition, bool isHome)
        {
            if (isHome)
            {
                return 100 - fieldPosition;
            }
            else
            {
                return fieldPosition;
            }
        }
    }
}
