namespace Gridiron.Engine.Simulation.Decision
{
    /// <summary>
    /// Configuration constants for fourth down decision-making.
    /// Adjust these values to tune coaching aggressiveness.
    /// </summary>
    public static class FourthDownConstants
    {
        // ========================================
        // BASE GO-FOR-IT PROBABILITIES BY DISTANCE
        // ========================================

        /// <summary>Base probability of going for it on 4th and 1.</summary>
        public const double GO_FOR_IT_PROB_1_YARD = 0.65;

        /// <summary>Base probability of going for it on 4th and 2.</summary>
        public const double GO_FOR_IT_PROB_2_YARDS = 0.35;

        /// <summary>Base probability of going for it on 4th and 3.</summary>
        public const double GO_FOR_IT_PROB_3_YARDS = 0.20;

        /// <summary>Base probability of going for it on 4th and 4-5.</summary>
        public const double GO_FOR_IT_PROB_4_5_YARDS = 0.08;

        /// <summary>Base probability of going for it on 4th and 6-10.</summary>
        public const double GO_FOR_IT_PROB_6_10_YARDS = 0.03;

        /// <summary>Base probability of going for it on 4th and long (11+).</summary>
        public const double GO_FOR_IT_PROB_LONG = 0.01;

        // ========================================
        // FIELD POSITION THRESHOLDS
        // ========================================

        /// <summary>Red zone threshold (yards to goal).</summary>
        public const int RED_ZONE_YARDS = 20;

        /// <summary>Opponent territory threshold (yards to goal).</summary>
        public const int OPPONENT_TERRITORY_YARDS = 50;

        /// <summary>Own territory conservative threshold (yards to goal).</summary>
        public const int OWN_TERRITORY_CONSERVATIVE_YARDS = 70;

        /// <summary>Yards to goal where punting becomes ineffective.</summary>
        public const int NO_PUNT_ZONE_YARDS = 35;

        // ========================================
        // FIELD GOAL THRESHOLDS
        // ========================================

        /// <summary>Maximum realistic field goal range in yards.</summary>
        public const int FIELD_GOAL_MAX_RANGE = 60;

        /// <summary>Chip shot field goal distance (near automatic).</summary>
        public const int FIELD_GOAL_CHIP_SHOT_YARDS = 35;

        /// <summary>Normal field goal range.</summary>
        public const int FIELD_GOAL_NORMAL_RANGE = 45;

        /// <summary>Long field goal range.</summary>
        public const int FIELD_GOAL_LONG_RANGE = 55;

        // ========================================
        // TIME THRESHOLDS (SECONDS)
        // ========================================

        /// <summary>Desperation mode time threshold (2 minutes).</summary>
        public const int DESPERATION_TIME_SECONDS = 120;

        /// <summary>Last chance time threshold (30 seconds).</summary>
        public const int LAST_CHANCE_TIME_SECONDS = 30;

        /// <summary>Aggressive mode time threshold (5 minutes).</summary>
        public const int AGGRESSIVE_TIME_SECONDS = 300;

        /// <summary>Late game time threshold (5 minutes).</summary>
        public const int LATE_GAME_TIME_SECONDS = 300;

        /// <summary>Maximum yards to go for aggressive mode.</summary>
        public const int AGGRESSIVE_MAX_YARDS_TO_GO = 3;

        // ========================================
        // SITUATIONAL MODIFIERS
        // ========================================

        /// <summary>Go-for-it bonus when in red zone.</summary>
        public const double RED_ZONE_GO_BONUS = 0.15;

        /// <summary>Go-for-it bonus when in opponent territory.</summary>
        public const double OPPONENT_TERRITORY_GO_BONUS = 0.08;

        /// <summary>Go-for-it penalty when deep in own territory.</summary>
        public const double OWN_TERRITORY_GO_PENALTY = 0.15;

        /// <summary>Go-for-it bonus when trailing by more than 7.</summary>
        public const double TRAILING_BIG_GO_BONUS = 0.20;

        /// <summary>Go-for-it bonus when trailing by 1-7.</summary>
        public const double TRAILING_SMALL_GO_BONUS = 0.10;

        /// <summary>Go-for-it penalty when leading by more than 14.</summary>
        public const double LEADING_BIG_GO_PENALTY = 0.15;

        /// <summary>Go-for-it bonus when trailing in late game.</summary>
        public const double TRAILING_LATE_GO_BONUS = 0.15;

        /// <summary>Go-for-it penalty when leading in late game.</summary>
        public const double LEADING_LATE_GO_PENALTY = 0.10;

        /// <summary>Go-for-it penalty when chip shot field goal is available.</summary>
        public const double CHIP_SHOT_FG_GO_PENALTY = 0.25;
    }
}
