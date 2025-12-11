namespace Gridiron.Engine.Simulation.Configuration
{
    /// <summary>
    /// Central configuration for all game probability values.
    /// Modify these values to tune game balance and simulation realism.
    /// All probability values are organized by game domain for easy discovery and maintenance.
    /// </summary>
    public static class GameProbabilities
    {
        /// <summary>
        /// Passing game probabilities including completion rates, interceptions, pressure, and yards after catch.
        /// </summary>
        public static class Passing
        {
            // Pass Type Distribution
            public const double SCREEN_PASS_THRESHOLD = 0.15;        // 15% of passes are screens
            public const double SHORT_PASS_THRESHOLD = 0.50;         // Next 35% are short passes (cumulative 50%)
            public const double FORWARD_PASS_THRESHOLD = 0.85;       // Next 35% are forward passes (cumulative 85%)
                                                                      // Remaining 15% are deep passes

            // Pass Completion
            public const double COMPLETION_BASE_PROBABILITY = 0.60;  // Base 60% completion rate
            public const double COMPLETION_PRESSURE_PENALTY = 0.20;  // -20% when QB is under pressure
            public const double COMPLETION_MIN_CLAMP = 0.25;         // Minimum 25% completion rate
            public const double COMPLETION_MAX_CLAMP = 0.85;         // Maximum 85% completion rate
            public const double COMPLETION_SKILL_DENOMINATOR = 250.0; // Skill differential divisor for completion adjustment

            // Interceptions
            public const double INTERCEPTION_BASE_PROBABILITY = 0.035; // 3.5% interception rate on incomplete passes
            public const double INTERCEPTION_PRESSURE_BONUS = 0.02;    // +2% interception chance when QB pressured
            public const double INTERCEPTION_MIN_CLAMP = 0.01;         // Minimum 1% interception rate
            public const double INTERCEPTION_MAX_CLAMP = 0.15;         // Maximum 15% interception rate
            /// <summary>
            /// Scale factor for logarithmic skill modifier on interception probability.
            /// Set to 0.5 to keep interception probability in realistic range (typically 1%-5%).
            /// Full modifier (1.0) would swing probability too dramatically for rare events like INTs.
            /// Example: With 0.5 scale, a +40 skill differential adds ~12% instead of ~24%.
            /// </summary>
            public const double INTERCEPTION_SKILL_MODIFIER_SCALE = 0.5;

            // QB Pressure
            public const double QB_PRESSURE_BASE_PROBABILITY = 0.30;   // 30% base pressure rate
            public const double QB_PRESSURE_MIN_CLAMP = 0.10;          // Minimum 10% pressure rate
            public const double QB_PRESSURE_MAX_CLAMP = 0.60;          // Maximum 60% pressure rate
            public const double QB_PRESSURE_SKILL_DENOMINATOR = 250.0; // Skill differential divisor

            // Pass Protection (Sacks)
            public const double PASS_PROTECTION_BASE_PROBABILITY = 0.75; // 75% protection success rate
            public const double PASS_PROTECTION_MIN_CLAMP = 0.40;        // Minimum 40% protection success
            public const double PASS_PROTECTION_MAX_CLAMP = 0.95;        // Maximum 95% protection success (sacks are relatively rare)
            public const double PASS_PROTECTION_SKILL_DENOMINATOR = 200.0; // Skill differential divisor

            // Yards After Catch (YAC)
            public const double YAC_OPPORTUNITY_BASE_PROBABILITY = 0.35; // 35% chance for YAC opportunity
            public const double YAC_SKILL_THRESHOLD = 70.0;              // Receiver skill threshold for YAC bonus
            public const double YAC_SKILL_DENOMINATOR = 400.0;           // Skill bonus divisor
            public const double YAC_MIN_CLAMP = 0.15;                    // Minimum 15% YAC opportunity
            public const double YAC_MAX_CLAMP = 0.55;                    // Maximum 55% YAC opportunity

            // Big Play After Catch
            public const double BIG_PLAY_YAC_PROBABILITY = 0.05;         // 5% chance for big play after catch
            public const double BIG_PLAY_YAC_SPEED_THRESHOLD = 85.0;     // Requires 85+ speed rating
            public const int BIG_PLAY_YAC_MIN_BONUS = 10;                // Minimum bonus yards on big play
            public const int BIG_PLAY_YAC_MAX_BONUS = 30;                // Maximum bonus yards on big play
        }

        /// <summary>
        /// Rushing game probabilities including scrambles, tackle breaks, big runs, and blocking.
        /// </summary>
        public static class Rushing
        {
            // QB Scramble
            public const double QB_SCRAMBLE_PROBABILITY = 0.10;          // 10% chance QB keeps ball for scramble/option

            // Tackle Break
            public const double TACKLE_BREAK_BASE_PROBABILITY = 0.25;    // 25% base tackle break rate (for elite backs)
            public const double TACKLE_BREAK_MIN_CLAMP = 0.05;           // Minimum 5% tackle break rate
            public const double TACKLE_BREAK_MAX_CLAMP = 0.50;           // Maximum 50% tackle break rate
            public const double TACKLE_BREAK_SKILL_DENOMINATOR = 250.0;  // Skill differential divisor

            // Big Run Breakaway
            public const double BIG_RUN_BASE_PROBABILITY = 0.08;         // 8% base big run probability
            public const double BIG_RUN_SPEED_THRESHOLD = 70.0;          // Speed threshold for bonus
            public const double BIG_RUN_SPEED_DENOMINATOR = 500.0;       // Speed bonus divisor
            public const double BIG_RUN_MIN_CLAMP = 0.03;                // Minimum 3% big run rate
            public const double BIG_RUN_MAX_CLAMP = 0.15;                // Maximum 15% big run rate

            // Blocking Success
            public const double BLOCKING_SUCCESS_BASE_PROBABILITY = 0.50; // 50% base blocking success
            public const double BLOCKING_SUCCESS_MIN_CLAMP = 0.20;        // Minimum 20% blocking success
            public const double BLOCKING_SUCCESS_MAX_CLAMP = 0.80;        // Maximum 80% blocking success
            public const double BLOCKING_SUCCESS_SKILL_DENOMINATOR = 200.0; // Skill differential divisor
        }

        /// <summary>
        /// Turnover probabilities including fumbles and fumble recoveries.
        /// </summary>
        public static class Turnovers
        {
            // Fumble Probability by Play Type
            public const double FUMBLE_QB_SACK_PROBABILITY = 0.12;       // 12% strip sack fumble rate
            public const double FUMBLE_RETURN_PROBABILITY = 0.025;       // 2.5% fumble rate on kickoff/punt returns
            public const double FUMBLE_NORMAL_PROBABILITY = 0.015;       // 1.5% fumble rate on normal plays
            public const double FUMBLE_MIN_CLAMP = 0.003;                // Minimum 0.3% fumble rate
            public const double FUMBLE_MAX_CLAMP = 0.25;                 // Maximum 25% fumble rate

            // Fumble Multipliers
            public const double FUMBLE_GANG_TACKLE_MULTIPLIER = 1.3;     // 1.3x fumble chance with 3+ defenders
            public const double FUMBLE_TWO_DEFENDERS_MULTIPLIER = 1.15;  // 1.15x fumble chance with 2 defenders

            // Fumble Recovery
            public const double FUMBLE_OUT_OF_BOUNDS_PROBABILITY = 0.12; // 12% chance fumble goes out of bounds
            public const double FUMBLE_RECOVERY_BACKWARD_BASE = 0.50;    // 50% offense recovery on backward bounce
            public const double FUMBLE_RECOVERY_FORWARD_BASE = 0.70;     // 70% offense recovery on forward bounce
            public const double FUMBLE_RECOVERY_SIDEWAYS_BASE = 0.60;    // 60% offense recovery on sideways bounce
            public const double FUMBLE_RECOVERY_BACKWARD_THRESHOLD = 0.4; // Random threshold for backward bounce
            public const double FUMBLE_RECOVERY_FORWARD_THRESHOLD = 0.7;  // Random threshold for forward bounce
            public const double FUMBLE_RECOVERY_AWARENESS_FACTOR = 0.15; // ±15% adjustment for awareness differential
            public const double FUMBLE_RECOVERY_MIN_CLAMP = 0.3;         // Minimum 30% recovery chance
            public const double FUMBLE_RECOVERY_MAX_CLAMP = 0.8;         // Maximum 80% recovery chance
        }

        /// <summary>
        /// Field goal probabilities including make rates by distance, blocks, and recoveries.
        /// </summary>
        public static class FieldGoals
        {
            // Make Probability by Distance
            public const double FG_MAKE_VERY_SHORT = 0.98;               // 98% make rate ≤30 yards (extra points)
            public const double FG_MAKE_SHORT_BASE = 0.90;               // 90% make rate at 30 yards
            public const double FG_MAKE_MEDIUM_BASE = 0.80;              // 80% make rate at 40 yards
            public const double FG_MAKE_LONG_BASE = 0.65;                // 65% make rate at 50 yards
            public const double FG_MAKE_VERY_LONG_BASE = 0.40;           // 40% make rate at 60 yards

            // Make Probability Decay Rates (per yard)
            public const double FG_MAKE_SHORT_DECAY = 0.01;              // 1% decay per yard (30-40 yards)
            public const double FG_MAKE_MEDIUM_DECAY = 0.015;            // 1.5% decay per yard (40-50 yards)
            public const double FG_MAKE_LONG_DECAY = 0.025;              // 2.5% decay per yard (50-60 yards)
            public const double FG_MAKE_VERY_LONG_DECAY = 0.03;          // 3% decay per yard (60+ yards)

            // Distance Thresholds
            public const int FG_DISTANCE_SHORT = 30;                     // Short field goal threshold
            public const int FG_DISTANCE_MEDIUM = 40;                    // Medium field goal threshold
            public const int FG_DISTANCE_LONG = 50;                      // Long field goal threshold
            public const int FG_DISTANCE_VERY_LONG = 60;                 // Very long field goal threshold

            // Make Probability Adjustments
            public const double FG_MAKE_SKILL_DENOMINATOR = 200.0;       // Kicker skill adjustment divisor
            public const double FG_MAKE_MIN_CLAMP = 0.05;                // Minimum 5% make rate
            public const double FG_MAKE_MAX_CLAMP = 0.99;                // Maximum 99% make rate

            // Block Probability
            public const double FG_BLOCK_VERY_SHORT = 0.015;             // 1.5% block rate ≤30 yards
            public const double FG_BLOCK_SHORT = 0.025;                  // 2.5% block rate 30-45 yards
            public const double FG_BLOCK_MEDIUM = 0.040;                 // 4% block rate 45-55 yards
            public const double FG_BLOCK_LONG = 0.065;                   // 6.5% block rate 55+ yards
            public const double FG_BLOCK_BAD_SNAP_MULTIPLIER = 10.0;     // 10x block chance on bad snap
            public const double FG_BLOCK_KICKER_SKILL_DENOMINATOR = 300.0; // Kicker skill factor divisor
            public const double FG_BLOCK_DEFENDER_SKILL_FACTOR = 0.003;  // Adjustment per 10 skill points differential
            public const double FG_BLOCK_MIN_CLAMP = 0.005;              // Minimum 0.5% block rate
            public const double FG_BLOCK_MAX_CLAMP = 0.25;               // Maximum 25% block rate

            // Block Distance Thresholds
            public const int FG_BLOCK_DISTANCE_SHORT = 30;               // Short kick threshold for blocking
            public const int FG_BLOCK_DISTANCE_MEDIUM = 45;              // Medium kick threshold for blocking
            public const int FG_BLOCK_DISTANCE_LONG = 55;                // Long kick threshold for blocking

            // Blocked FG Recovery
            public const double BLOCKED_FG_DEFENSE_RECOVERY = 0.5;       // 50% defense recovery on blocked kick

            // Field Goal Miss Direction
            public const double FG_MISS_WIDE_RIGHT_THRESHOLD = 0.4;      // 40% of misses go wide right
            public const double FG_MISS_WIDE_LEFT_THRESHOLD = 0.8;       // 40% of misses go wide left (cumulative 80%)
                                                                          // Remaining 20% of misses are short
        }

        /// <summary>
        /// Kickoff probabilities including onside kicks, muffed catches, and out of bounds.
        /// </summary>
        public static class Kickoffs
        {
            // Onside Kick
            public const double ONSIDE_ATTEMPT_PROBABILITY = 0.05;       // 5% chance to attempt onside when trailing by 7+
            public const double ONSIDE_RECOVERY_BASE_PROBABILITY = 0.20; // 20% base recovery rate
            public const double ONSIDE_RECOVERY_SKILL_BONUS = 0.10;      // Up to +10% from kicker skill
            public const double ONSIDE_RECOVERY_SKILL_DENOMINATOR = 100.0; // Kicker skill divisor

            // Out of Bounds
            public const double KICKOFF_OOB_NORMAL = 0.03;               // 3% out of bounds on normal kicks
            public const double KICKOFF_OOB_DANGER_ZONE = 0.10;          // 10% out of bounds in danger zone
            public const int KICKOFF_OOB_DANGER_MIN = 65;                // Danger zone minimum yardage
            public const int KICKOFF_OOB_DANGER_MAX = 95;                // Danger zone maximum yardage

            // Muffed Catch
            public const double KICKOFF_MUFF_BASE = 0.015;               // 1.5% base muff rate
            public const double KICKOFF_MUFF_SHORT_KICK = 0.04;          // 4% muff rate on short kicks
            public const int KICKOFF_MUFF_SHORT_THRESHOLD = 50;          // Short kick distance threshold
            public const double KICKOFF_MUFF_SKILL_DENOMINATOR = 150.0;  // Returner skill adjustment divisor

            // Muff Recovery
            public const double KICKOFF_MUFF_RECEIVING_TEAM_RECOVERY = 0.6; // 60% receiving team recovers muff
        }

        /// <summary>
        /// Punt probabilities including bad snaps, blocks, muffs, fair catches, downed punts, and out of bounds.
        /// </summary>
        public static class Punts
        {
            // Bad Snap
            public const double PUNT_BAD_SNAP_BASE = 0.05;               // 5% worst case bad snap rate
            public const double PUNT_BAD_SNAP_SKILL_FACTOR = 0.04;       // Reduction factor based on long snapper skill
            public const double PUNT_BAD_SNAP_SKILL_DENOMINATOR = 100.0; // Long snapper skill divisor

            // Block Probability
            public const double PUNT_BLOCK_GOOD_SNAP = 0.01;             // 1% block rate on good snap
            public const double PUNT_BLOCK_BAD_SNAP = 0.20;              // 20% block rate on bad snap
            public const double PUNT_BLOCK_PUNTER_SKILL_DENOMINATOR = 200.0; // Punter skill factor divisor
            public const double PUNT_BLOCK_DEFENDER_SKILL_FACTOR = 0.005; // Adjustment per 10 skill points differential
            public const double PUNT_BLOCK_MIN_CLAMP = 0.002;            // Minimum 0.2% block rate
            public const double PUNT_BLOCK_MAX_CLAMP = 0.30;             // Maximum 30% block rate

            // Muffed Catch
            public const double PUNT_MUFF_BASE = 0.05;                   // 5% worst case muff rate
            public const double PUNT_MUFF_SKILL_FACTOR = 0.04;           // Reduction factor based on returner catching
            public const double PUNT_MUFF_SKILL_DENOMINATOR = 100.0;     // Returner skill divisor
            public const double PUNT_MUFF_HIGH_HANG_TIME_BONUS = 0.02;   // +2% muff chance over 4.5 seconds hang time
            public const double PUNT_MUFF_MEDIUM_HANG_TIME_BONUS = 0.01; // +1% muff chance over 4.0 seconds hang time
            public const double PUNT_MUFF_HIGH_HANG_THRESHOLD = 4.5;     // High hang time threshold (seconds)
            public const double PUNT_MUFF_MEDIUM_HANG_THRESHOLD = 4.0;   // Medium hang time threshold (seconds)

            // Fair Catch
            public const double PUNT_FAIR_CATCH_BASE = 0.25;             // 25% base fair catch rate
            public const double PUNT_FAIR_CATCH_HIGH_HANG_BONUS = 0.15;  // +15% for high hang time (>4.5s)
            public const double PUNT_FAIR_CATCH_MEDIUM_HANG_BONUS = 0.10; // +10% for medium hang time (>4.0s)
            public const double PUNT_FAIR_CATCH_OWN_10_BONUS = 0.20;     // +20% inside own 10 yard line
            public const double PUNT_FAIR_CATCH_OWN_20_BONUS = 0.10;     // +10% inside own 20 yard line

            // Out of Bounds
            public const double PUNT_OOB_BASE = 0.12;                    // 12% base out of bounds rate
            public const double PUNT_OOB_INSIDE_10_BONUS = 0.08;         // +8% inside opponent's 10 yard line
            public const double PUNT_OOB_INSIDE_15_BONUS = 0.05;         // +5% inside opponent's 15 yard line
            public const int PUNT_OOB_INSIDE_10_THRESHOLD = 90;          // Inside 10 field position threshold
            public const int PUNT_OOB_INSIDE_15_THRESHOLD = 85;          // Inside 15 field position threshold

            // Downed
            public const double PUNT_DOWNED_BASE = 0.15;                 // 15% base downed rate
            public const double PUNT_DOWNED_INSIDE_5_BONUS = 0.40;       // +40% inside opponent's 5 yard line
            public const double PUNT_DOWNED_INSIDE_10_BONUS = 0.25;      // +25% inside opponent's 10 yard line
            public const double PUNT_DOWNED_INSIDE_15_BONUS = 0.15;      // +15% inside opponent's 15 yard line
            public const double PUNT_DOWNED_HIGH_HANG_BONUS = 0.10;      // +10% for high hang time (>4.5s)
            public const double PUNT_DOWNED_MEDIUM_HANG_BONUS = 0.05;    // +5% for medium hang time (>4.0s)
            public const int PUNT_DOWNED_INSIDE_5_THRESHOLD = 95;        // Inside 5 field position threshold
            public const int PUNT_DOWNED_INSIDE_10_THRESHOLD = 90;       // Inside 10 field position threshold
            public const int PUNT_DOWNED_INSIDE_15_THRESHOLD = 85;       // Inside 15 field position threshold
        }

        /// <summary>
        /// Game decision probabilities including extra point vs 2-point conversion attempts and play selection.
        /// </summary>
        public static class GameDecisions
        {
            // Extra Point vs 2-Point Conversion
            public const double TWO_POINT_CONVERSION_ATTEMPT = 0.10;     // 10% go for 2-point conversion

            // 2-Point Play Selection
            public const double TWO_POINT_RUN_PROBABILITY = 0.5;         // 50% run on 2-point conversion
            public const double TWO_POINT_PASS_PROBABILITY = 0.5;        // 50% pass on 2-point conversion
        }

        /// <summary>
        /// Fourth down decision probabilities and thresholds.
        /// Controls when teams go for it, punt, or attempt field goals on fourth down.
        /// </summary>
        public static class FourthDown
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

        /// <summary>
        /// Timeout decision thresholds and mechanics.
        /// </summary>
        public static class Timeouts
        {
            // ========================================
            // TIMEOUT ALLOCATION
            // ========================================

            /// <summary>Number of timeouts each team receives per half.</summary>
            public const int TIMEOUTS_PER_HALF = 3;

            /// <summary>Number of timeouts each team receives in overtime.</summary>
            public const int TIMEOUTS_PER_OVERTIME = 2;

            // ========================================
            // CLOCK RESET VALUES
            // ========================================

            /// <summary>Play clock value after a timeout is called (seconds).</summary>
            public const int PLAY_CLOCK_AFTER_TIMEOUT = 25;

            // ========================================
            // ICE THE KICKER THRESHOLDS
            // ========================================

            /// <summary>Probability of attempting to ice the kicker when conditions are met.</summary>
            public const double ICE_KICKER_PROBABILITY = 0.30;

            /// <summary>Minimum field goal distance (yards) to consider icing the kicker.</summary>
            public const int ICE_KICKER_MIN_DISTANCE = 45;

            // ========================================
            // STOP THE CLOCK THRESHOLDS
            // ========================================

            /// <summary>Time remaining in half (seconds) to consider stopping the clock.</summary>
            public const int STOP_CLOCK_TIME_THRESHOLD = 120;

            /// <summary>Probability of stopping clock when trailing with time running out.</summary>
            public const double STOP_CLOCK_PROBABILITY = 0.85;

            // ========================================
            // AVOID DELAY OF GAME THRESHOLDS
            // ========================================

            /// <summary>Play clock value (seconds) at which to consider calling timeout to avoid delay.</summary>
            public const int AVOID_DELAY_PLAY_CLOCK_THRESHOLD = 3;

            /// <summary>Probability of calling timeout to avoid delay of game.</summary>
            public const double AVOID_DELAY_PROBABILITY = 0.90;
        }

        /// <summary>
        /// Clock management play constants for spike and kneel plays.
        /// </summary>
        public static class ClockManagement
        {
            // ========================================
            // KNEEL (VICTORY FORMATION) CONSTANTS
            // ========================================

            /// <summary>Elapsed time for a kneel play (uses full play clock).</summary>
            public const int KNEEL_ELAPSED_TIME_SECONDS = 40;

            /// <summary>Yards lost on a kneel play (always -1).</summary>
            public const int KNEEL_YARDS_LOST = 1;

            // ========================================
            // SPIKE CONSTANTS
            // ========================================

            /// <summary>Elapsed time for a spike play (very quick).</summary>
            public const int SPIKE_ELAPSED_TIME_SECONDS = 3;

            /// <summary>Yards gained on a spike play (always 0).</summary>
            public const int SPIKE_YARDS_GAINED = 0;
        }

        /// <summary>
        /// Yardage calculation constants for runs, passes, sacks, and yards after catch.
        /// These values tune the base yardage formulas and variance ranges.
        /// </summary>
        public static class Yardage
        {
            // ========================================
            // RUN YARDS
            // ========================================

            /// <summary>Base yards for a run play before skill/randomness applied.</summary>
            public const double RUN_BASE_YARDS = 3.0;

            /// <summary>Skill differential divisor for run yardage (affects impact of blocking vs tackling).</summary>
            public const double RUN_SKILL_DENOMINATOR = 20.0;

            /// <summary>Random variance range for run plays (multiplied by NextDouble).</summary>
            public const double RUN_RANDOM_RANGE = 25.0;

            /// <summary>Random variance offset for run plays (supports losses up to -15 yards).</summary>
            public const double RUN_RANDOM_OFFSET = -15.0;

            // ========================================
            // SACK YARDS
            // ========================================

            /// <summary>Minimum sack loss in yards.</summary>
            public const int SACK_MIN_LOSS = 2;

            /// <summary>Maximum sack loss in yards (exclusive upper bound for Next()).</summary>
            public const int SACK_MAX_LOSS = 11; // Next(2, 11) gives 2-10 yards

            // ========================================
            // AIR YARDS BY PASS TYPE
            // ========================================

            /// <summary>Screen pass: minimum air yards (behind LOS).</summary>
            public const int SCREEN_MIN_YARDS = -3;

            /// <summary>Screen pass: maximum air yards (exclusive upper bound).</summary>
            public const int SCREEN_MAX_YARDS = 3;

            /// <summary>Short pass: minimum air yards.</summary>
            public const int SHORT_MIN_YARDS = 3;

            /// <summary>Short pass: maximum air yards (before clamping).</summary>
            public const int SHORT_MAX_YARDS = 12;

            /// <summary>Forward pass: minimum air yards.</summary>
            public const int FORWARD_MIN_YARDS = 8;

            /// <summary>Forward pass: maximum air yards (before clamping).</summary>
            public const int FORWARD_MAX_YARDS = 20;

            /// <summary>Deep pass: minimum air yards.</summary>
            public const int DEEP_MIN_YARDS = 18;

            /// <summary>Deep pass: maximum air yards (before clamping).</summary>
            public const int DEEP_MAX_YARDS = 45;

            /// <summary>Default pass (fallback): minimum air yards.</summary>
            public const int DEFAULT_PASS_MIN_YARDS = 5;

            /// <summary>Default pass (fallback): maximum air yards (before clamping).</summary>
            public const int DEFAULT_PASS_MAX_YARDS = 15;

            // ========================================
            // YARDS AFTER CATCH (YAC)
            // ========================================

            /// <summary>Base YAC before skill adjustments.</summary>
            public const double YAC_BASE_YARDS = 3.0;

            /// <summary>Skill denominator for YAC calculation.</summary>
            public const double YAC_SKILL_DENOMINATOR = 20.0;

            /// <summary>Random variance range for YAC.</summary>
            public const double YAC_RANDOM_RANGE = 8.0;

            /// <summary>Random variance offset for YAC.</summary>
            public const double YAC_RANDOM_OFFSET = -2.0;

            /// <summary>Yards when receiver is tackled immediately (no YAC opportunity).</summary>
            public const int YAC_IMMEDIATE_TACKLE_MAX = 3; // Next(0, 3) gives 0-2 yards

            // ========================================
            // KICKING DISTANCE
            // ========================================

            /// <summary>Kickoff: base distance before skill adjustment.</summary>
            public const double KICKOFF_BASE_DISTANCE = 40.0;

            /// <summary>Kickoff: skill multiplier range (0-100 skill × 30.0 = 0-30 yards bonus).</summary>
            public const double KICKOFF_SKILL_RANGE = 30.0;

            /// <summary>Kickoff: random variance range.</summary>
            public const double KICKOFF_RANDOM_RANGE = 20.0;

            /// <summary>Kickoff: random variance offset.</summary>
            public const double KICKOFF_RANDOM_OFFSET = -10.0;

            /// <summary>Kickoff: minimum realistic distance.</summary>
            public const double KICKOFF_MIN_DISTANCE = 30.0;

            /// <summary>Kickoff: maximum realistic distance.</summary>
            public const double KICKOFF_MAX_DISTANCE = 80.0;

            /// <summary>Punt: base distance before skill adjustment.</summary>
            public const double PUNT_BASE_DISTANCE = 30.0;

            /// <summary>Punt: skill multiplier range.</summary>
            public const double PUNT_SKILL_RANGE = 25.0;

            /// <summary>Punt: random variance range.</summary>
            public const double PUNT_RANDOM_RANGE = 25.0;

            /// <summary>Punt: random variance offset.</summary>
            public const double PUNT_RANDOM_OFFSET = -10.0;

            /// <summary>Punt: minimum distance (shanked punt).</summary>
            public const double PUNT_MIN_DISTANCE = 10.0;

            /// <summary>Punt: maximum field boundary (110 yards - field position).</summary>
            public const int PUNT_FIELD_BOUNDARY = 110;
        }

        /// <summary>
        /// Play timing constants for elapsed time during play execution.
        /// These values simulate how long plays take in real time (for game clock).
        /// </summary>
        public static class Timing
        {
            // ========================================
            // PASS PLAY EXECUTION TIMES
            // ========================================

            /// <summary>Pass play (completion): base execution time.</summary>
            public const double PASS_PLAY_BASE_TIME = 4.0;

            /// <summary>Pass play (completion): execution time variance.</summary>
            public const double PASS_PLAY_VARIANCE = 3.0;

            /// <summary>Pass play (sack): base execution time.</summary>
            public const double PASS_PLAY_SACK_BASE_TIME = 4.0;

            /// <summary>Pass play (sack): execution time variance.</summary>
            public const double PASS_PLAY_SACK_VARIANCE = 4.0;

            // ========================================
            // RUN PLAY EXECUTION TIMES
            // ========================================

            /// <summary>Run play: base execution time.</summary>
            public const double RUN_PLAY_BASE_TIME = 5.0;

            /// <summary>Run play: execution time variance.</summary>
            public const double RUN_PLAY_VARIANCE = 3.0;

            /// <summary>Run play (breakaway): execution time variance.</summary>
            public const double RUN_PLAY_BREAKAWAY_VARIANCE = 4.0;

            // ========================================
            // FIELD GOAL EXECUTION TIMES
            // ========================================

            /// <summary>Field goal attempt: base time.</summary>
            public const double FIELD_GOAL_BASE_TIME = 4.0;

            /// <summary>Field goal attempt: variance.</summary>
            public const double FIELD_GOAL_VARIANCE = 3.0;

            /// <summary>Field goal (blocked): base time.</summary>
            public const double FIELD_GOAL_BLOCKED_BASE_TIME = 3.0;

            /// <summary>Field goal (blocked): variance.</summary>
            public const double FIELD_GOAL_BLOCKED_VARIANCE = 3.0;

            /// <summary>Field goal (blocked, recovery): fixed time.</summary>
            public const double FIELD_GOAL_BLOCKED_RECOVERY_TIME = 2.0;

            /// <summary>Field goal (bad snap): base time.</summary>
            public const double FIELD_GOAL_BAD_SNAP_BASE_TIME = 2.0;

            /// <summary>Field goal (bad snap): variance.</summary>
            public const double FIELD_GOAL_BAD_SNAP_VARIANCE = 1.0;

            // ========================================
            // KICKOFF EXECUTION TIMES
            // ========================================

            /// <summary>Kickoff (touchback): fixed time.</summary>
            public const double KICKOFF_TOUCHBACK_TIME = 3.0;

            /// <summary>Kickoff (return): base time.</summary>
            public const double KICKOFF_RETURN_BASE_TIME = 4.0;

            /// <summary>Kickoff (return): variance.</summary>
            public const double KICKOFF_RETURN_VARIANCE = 2.0;

            /// <summary>Kickoff (out of bounds): fixed time.</summary>
            public const double KICKOFF_OOB_TIME = 3.0;

            /// <summary>Kickoff (onside, recovered by kicking team): fixed time.</summary>
            public const double KICKOFF_ONSIDE_RECOVERED_TIME = 5.0;

            /// <summary>Kickoff (hang time estimation buffer).</summary>
            public const double KICKOFF_HANG_TIME_BUFFER = 0.5;

            /// <summary>Kickoff (long return): base time.</summary>
            public const double KICKOFF_LONG_RETURN_BASE_TIME = 5.0;

            /// <summary>Kickoff (long return): variance.</summary>
            public const double KICKOFF_LONG_RETURN_VARIANCE = 2.0;

            /// <summary>Kickoff (TD return): base time.</summary>
            public const double KICKOFF_TD_RETURN_BASE_TIME = 6.0;

            /// <summary>Kickoff (TD return): variance.</summary>
            public const double KICKOFF_TD_RETURN_VARIANCE = 2.0;

            /// <summary>Kickoff (fair catch): base time.</summary>
            public const double KICKOFF_FAIR_CATCH_BASE_TIME = 5.0;

            /// <summary>Kickoff (fair catch): variance.</summary>
            public const double KICKOFF_FAIR_CATCH_VARIANCE = 3.0;

            /// <summary>Kickoff (muff, offense recovers): base time.</summary>
            public const double KICKOFF_MUFF_OFFENSE_BASE_TIME = 5.0;

            /// <summary>Kickoff (muff, offense recovers): variance.</summary>
            public const double KICKOFF_MUFF_OFFENSE_VARIANCE = 3.0;

            /// <summary>Kickoff (muff, defense recovers): base time.</summary>
            public const double KICKOFF_MUFF_DEFENSE_BASE_TIME = 5.0;

            /// <summary>Kickoff (muff, defense recovers): variance.</summary>
            public const double KICKOFF_MUFF_DEFENSE_VARIANCE = 4.0;

            /// <summary>Kickoff (muff, touchback): base time.</summary>
            public const double KICKOFF_MUFF_TOUCHBACK_BASE_TIME = 5.0;

            /// <summary>Kickoff (muff, touchback): variance.</summary>
            public const double KICKOFF_MUFF_TOUCHBACK_VARIANCE = 3.0;

            /// <summary>Kickoff (muff, out of bounds): base time.</summary>
            public const double KICKOFF_MUFF_OOB_BASE_TIME = 5.0;

            /// <summary>Kickoff (muff, out of bounds): variance.</summary>
            public const double KICKOFF_MUFF_OOB_VARIANCE = 2.0;

            // ========================================
            // PUNT EXECUTION TIMES
            // ========================================

            /// <summary>Punt (normal): base time.</summary>
            public const double PUNT_BASE_TIME = 4.0;

            /// <summary>Punt (normal): variance.</summary>
            public const double PUNT_VARIANCE = 4.0;

            /// <summary>Punt (blocked): base time.</summary>
            public const double PUNT_BLOCKED_BASE_TIME = 3.0;

            /// <summary>Punt (blocked): variance.</summary>
            public const double PUNT_BLOCKED_VARIANCE = 3.0;

            /// <summary>Punt (downed/OOB/fair catch): hang time buffer.</summary>
            public const double PUNT_HANG_TIME_BUFFER = 0.5;

            /// <summary>Punt (touchback): hang time buffer.</summary>
            public const double PUNT_TOUCHBACK_HANG_TIME_BUFFER = 1.0;

            /// <summary>Punt (return): base time (added to hang time).</summary>
            public const double PUNT_RETURN_BASE_TIME = 2.0;

            /// <summary>Punt (return): variance.</summary>
            public const double PUNT_RETURN_VARIANCE = 2.0;

            /// <summary>Punt (TD return): base time (added to hang time).</summary>
            public const double PUNT_TD_RETURN_BASE_TIME = 2.0;

            /// <summary>Punt (TD return): variance.</summary>
            public const double PUNT_TD_RETURN_VARIANCE = 4.0;
        }

        /// <summary>
        /// Statistical distribution parameters for realistic yardage generation.
        /// These values control the shape of log-normal (runs) and normal (passes) distributions.
        ///
        /// NFL Target Statistics:
        /// - Run plays: Mean ~4.3 yards, Median ~3 yards, right-skewed
        /// - Breakaway runs (15+): ~5% of runs
        /// - Negative runs (TFL): ~15% of runs
        /// - Pass completions: Mean ~11.5 yards (varies by pass type)
        /// </summary>
        public static class YardageDistributions
        {
            // ========================================
            // RUN YARDAGE (LOG-NORMAL DISTRIBUTION)
            // ========================================
            // Log-normal produces right-skewed distribution:
            // - Most runs cluster around 2-4 yards
            // - Occasional breakaway runs (15+ yards)
            // - Some negative runs (TFL)

            /// <summary>Log-normal location parameter (mu) for run yards.</summary>
            /// <remarks>
            /// mu=1.5 gives median of exp(1.5)=4.48 yards before shift.
            /// Combined with shift=2.2, produces median ~2.3 yards.
            /// </remarks>
            public const double RUN_MU = 1.5;

            /// <summary>Log-normal scale parameter (sigma) for run yards.</summary>
            /// <remarks>
            /// sigma=0.7 controls spread of distribution.
            /// Higher sigma = more variance, more breakaways.
            /// </remarks>
            public const double RUN_SIGMA = 0.7;

            /// <summary>Shift value to allow negative yards (subtracted from log-normal output).</summary>
            /// <remarks>
            /// shift=2.8 produces ~15% negative runs (TFL) after rounding.
            /// Higher shift needed because rounding converts -0.5 to 0.49 to 0.
            /// </remarks>
            public const double RUN_SHIFT = 2.8;

            /// <summary>Skill modifier multiplier for run yards.</summary>
            public const double RUN_SKILL_MULTIPLIER = 2.0;

            /// <summary>
            /// Normalizer for skill differential calculation.
            /// Power values typically range 40-80, so differential is roughly -40 to +40.
            /// Dividing by this value normalizes to roughly -1 to +1 range.
            /// </summary>
            public const double SKILL_DIFFERENTIAL_NORMALIZER = 40.0;

            // ========================================
            // PASS YARDAGE (NORMAL DISTRIBUTION BY TYPE)
            // ========================================

            /// <summary>Screen pass: mean yardage.</summary>
            public const double PASS_SCREEN_MEAN = 4.0;

            /// <summary>Screen pass: standard deviation.</summary>
            public const double PASS_SCREEN_STDDEV = 3.0;

            /// <summary>Short pass: mean yardage.</summary>
            public const double PASS_SHORT_MEAN = 7.0;

            /// <summary>Short pass: standard deviation.</summary>
            public const double PASS_SHORT_STDDEV = 3.5;

            /// <summary>Medium pass: mean yardage.</summary>
            public const double PASS_MEDIUM_MEAN = 14.0;

            /// <summary>Medium pass: standard deviation.</summary>
            public const double PASS_MEDIUM_STDDEV = 5.0;

            /// <summary>Deep pass: mean yardage.</summary>
            public const double PASS_DEEP_MEAN = 30.0;

            /// <summary>Deep pass: standard deviation.</summary>
            public const double PASS_DEEP_STDDEV = 10.0;

            /// <summary>Skill modifier multiplier for pass yards.</summary>
            public const double PASS_SKILL_MULTIPLIER = 3.0;

            // ========================================
            // SACK YARDAGE (NORMAL DISTRIBUTION)
            // ========================================

            /// <summary>Sack: mean loss in yards.</summary>
            public const double SACK_MEAN = 7.0;

            /// <summary>Sack: standard deviation.</summary>
            public const double SACK_STDDEV = 2.0;

            /// <summary>Sack: minimum loss (floor).</summary>
            public const double SACK_MIN_LOSS = 1.0;

            /// <summary>Sack: maximum loss (ceiling).</summary>
            public const double SACK_MAX_LOSS = 15.0;

            // ========================================
            // TACKLE FOR LOSS (NORMAL DISTRIBUTION)
            // ========================================

            /// <summary>Tackle for loss: mean loss in yards.</summary>
            public const double TFL_MEAN = 2.0;

            /// <summary>Tackle for loss: standard deviation.</summary>
            public const double TFL_STDDEV = 1.0;

            /// <summary>Tackle for loss: minimum loss (floor).</summary>
            public const double TFL_MIN_LOSS = 1.0;

            /// <summary>Tackle for loss: maximum loss (ceiling).</summary>
            public const double TFL_MAX_LOSS = 5.0;
        }
    }
}
