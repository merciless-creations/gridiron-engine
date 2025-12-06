using Gridiron.Engine.Domain.Helpers;

namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Encapsulates fourth down decision-making logic.
    /// Determines whether to go for it, punt, or attempt a field goal based on game situation.
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
            // Calculate field goal distance if attempted
            int fieldGoalDistance = CalculateFieldGoalDistance(context.FieldPosition, context.IsHome);

            // Check if field goal is in range (realistic max ~60 yards)
            bool fieldGoalInRange = fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_MAX_RANGE;

            // Check if too close to opponent's goal to punt effectively
            bool tooCloseForPunt = GetYardsToOpponentGoal(context.FieldPosition, context.IsHome) <= FourthDownConstants.NO_PUNT_ZONE_YARDS;

            // Get base go-for-it probability based on distance
            double goForItProbability = GetBaseGoForItProbability(context.YardsToGo);

            // Apply situational modifiers
            goForItProbability = ApplySituationalModifiers(goForItProbability, context, fieldGoalInRange);

            // Decision logic
            if (ShouldAlwaysGoForIt(context))
            {
                return FourthDownDecision.GoForIt;
            }

            // Roll for go-for-it based on calculated probability
            if (_rng.NextDouble() < goForItProbability)
            {
                return FourthDownDecision.GoForIt;
            }

            // If not going for it, choose between punt and field goal
            if (fieldGoalInRange && !tooCloseForPunt)
            {
                // Prefer field goal if in range and reasonable distance
                return ChooseBetweenPuntAndFieldGoal(context, fieldGoalDistance);
            }

            if (tooCloseForPunt)
            {
                // Must attempt field goal if too close to punt
                return FourthDownDecision.AttemptFieldGoal;
            }

            // Default to punt
            return FourthDownDecision.Punt;
        }

        /// <summary>
        /// Determines if the situation mandates going for it regardless of normal probabilities.
        /// </summary>
        private bool ShouldAlwaysGoForIt(FourthDownContext context)
        {
            // Trailing by more than one possession with less than 2 minutes
            bool desperationMode = context.ScoreDifferential < -8 &&
                                   context.TimeRemainingSeconds < FourthDownConstants.DESPERATION_TIME_SECONDS;

            // Trailing by any amount with less than 30 seconds (last chance)
            bool lastChance = context.ScoreDifferential < 0 &&
                              context.TimeRemainingSeconds < FourthDownConstants.LAST_CHANCE_TIME_SECONDS;

            // In opponent territory, trailing, under 5 minutes
            bool aggressiveMode = context.ScoreDifferential < 0 &&
                                  GetYardsToOpponentGoal(context.FieldPosition, context.IsHome) <= 50 &&
                                  context.TimeRemainingSeconds < FourthDownConstants.AGGRESSIVE_TIME_SECONDS &&
                                  context.YardsToGo <= FourthDownConstants.AGGRESSIVE_MAX_YARDS_TO_GO;

            return desperationMode || lastChance || aggressiveMode;
        }

        /// <summary>
        /// Gets the base probability of going for it based on yards to go.
        /// </summary>
        private double GetBaseGoForItProbability(int yardsToGo)
        {
            if (yardsToGo <= 1)
                return FourthDownConstants.GO_FOR_IT_PROB_1_YARD;
            if (yardsToGo <= 2)
                return FourthDownConstants.GO_FOR_IT_PROB_2_YARDS;
            if (yardsToGo <= 3)
                return FourthDownConstants.GO_FOR_IT_PROB_3_YARDS;
            if (yardsToGo <= 5)
                return FourthDownConstants.GO_FOR_IT_PROB_4_5_YARDS;
            if (yardsToGo <= 10)
                return FourthDownConstants.GO_FOR_IT_PROB_6_10_YARDS;

            return FourthDownConstants.GO_FOR_IT_PROB_LONG;
        }

        /// <summary>
        /// Applies situational modifiers to the base go-for-it probability.
        /// </summary>
        private double ApplySituationalModifiers(double baseProbability, FourthDownContext context, bool fieldGoalInRange)
        {
            double probability = baseProbability;
            int yardsToGoal = GetYardsToOpponentGoal(context.FieldPosition, context.IsHome);

            // Field position modifiers
            if (yardsToGoal <= FourthDownConstants.RED_ZONE_YARDS)
            {
                // More aggressive in red zone
                probability += FourthDownConstants.RED_ZONE_GO_BONUS;
            }
            else if (yardsToGoal <= FourthDownConstants.OPPONENT_TERRITORY_YARDS)
            {
                // Slightly more aggressive in opponent territory
                probability += FourthDownConstants.OPPONENT_TERRITORY_GO_BONUS;
            }
            else if (yardsToGoal >= FourthDownConstants.OWN_TERRITORY_CONSERVATIVE_YARDS)
            {
                // Conservative in own territory
                probability -= FourthDownConstants.OWN_TERRITORY_GO_PENALTY;
            }

            // Score differential modifiers
            if (context.ScoreDifferential < -7)
            {
                // Trailing by more than a TD - more aggressive
                probability += FourthDownConstants.TRAILING_BIG_GO_BONUS;
            }
            else if (context.ScoreDifferential < 0)
            {
                // Trailing by small margin
                probability += FourthDownConstants.TRAILING_SMALL_GO_BONUS;
            }
            else if (context.ScoreDifferential > 14)
            {
                // Leading big - conservative
                probability -= FourthDownConstants.LEADING_BIG_GO_PENALTY;
            }

            // Time remaining modifiers
            if (context.TimeRemainingSeconds < FourthDownConstants.LATE_GAME_TIME_SECONDS)
            {
                if (context.ScoreDifferential < 0)
                {
                    // Trailing late - more aggressive
                    probability += FourthDownConstants.TRAILING_LATE_GO_BONUS;
                }
                else if (context.ScoreDifferential > 0)
                {
                    // Leading late - try to run out clock, less aggressive
                    probability -= FourthDownConstants.LEADING_LATE_GO_PENALTY;
                }
            }

            // If field goal is in good range, reduce go-for-it probability
            if (fieldGoalInRange)
            {
                int fgDistance = CalculateFieldGoalDistance(context.FieldPosition, context.IsHome);
                if (fgDistance <= FourthDownConstants.FIELD_GOAL_CHIP_SHOT_YARDS)
                {
                    // Chip shot range - strongly prefer FG unless short yardage
                    if (context.YardsToGo > 2)
                    {
                        probability -= FourthDownConstants.CHIP_SHOT_FG_GO_PENALTY;
                    }
                }
            }

            // Clamp probability between 0 and 1
            return Math.Clamp(probability, 0.0, 1.0);
        }

        /// <summary>
        /// Chooses between punt and field goal when both are viable options.
        /// </summary>
        private FourthDownDecision ChooseBetweenPuntAndFieldGoal(FourthDownContext context, int fieldGoalDistance)
        {
            // Strong preference for field goal in chip shot range
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_CHIP_SHOT_YARDS)
            {
                return FourthDownDecision.AttemptFieldGoal;
            }

            // Moderate preference for field goal in normal range
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_NORMAL_RANGE)
            {
                // 80% field goal, 20% punt (punt if pinning them deep matters more)
                if (_rng.NextDouble() < 0.80)
                {
                    return FourthDownDecision.AttemptFieldGoal;
                }
            }

            // Long field goal range - more situational
            if (fieldGoalDistance <= FourthDownConstants.FIELD_GOAL_LONG_RANGE)
            {
                // Score differential matters more for long kicks
                if (context.ScoreDifferential <= -3)
                {
                    // Need points - attempt the long field goal
                    return FourthDownDecision.AttemptFieldGoal;
                }
                // Otherwise, consider punting for better field position
                if (_rng.NextDouble() < 0.50)
                {
                    return FourthDownDecision.AttemptFieldGoal;
                }
            }

            // Beyond comfortable range - usually punt
            return FourthDownDecision.Punt;
        }

        /// <summary>
        /// Calculates the field goal distance from the current field position.
        /// </summary>
        private int CalculateFieldGoalDistance(int fieldPosition, bool isHome)
        {
            // Field goal distance = yards to goal + 17 (10 yards end zone + 7 yard snap)
            int yardsToGoal = GetYardsToOpponentGoal(fieldPosition, isHome);
            return yardsToGoal + 17;
        }

        /// <summary>
        /// Gets the yards remaining to the opponent's goal line.
        /// </summary>
        private int GetYardsToOpponentGoal(int fieldPosition, bool isHome)
        {
            // Home team attacks position 100, Away team attacks position 0
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
