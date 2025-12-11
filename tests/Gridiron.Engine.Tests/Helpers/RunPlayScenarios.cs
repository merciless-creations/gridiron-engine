using System;

namespace Gridiron.Engine.Tests.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for run play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for run plays so that when new random checks are added
    /// (like pre-snap penalties), we only need to update these factory methods instead of hundreds of tests.
    ///
    /// USAGE: Instead of manually building random sequences in each test, call a factory method:
    ///   var rng = RunPlayScenarios.SimpleGain(yards: 5);
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    /// 1. QB check (NextDouble) - determines if QB scrambles or RB gets ball
    /// 2. Direction (NextInt 0-4) - run direction
    /// 3. Blocking check (NextDouble) - offensive line blocking success
    /// 4. Blocking penalty check (NextDouble) - offensive/defensive holding
    /// 5. Base yards random factor (NextDouble) - variance in yards calculation
    /// 6. Tackle break check (NextDouble) - ball carrier breaks tackle
    /// 7. [Conditional] Tackle break yards (NextInt 3-8) - only if tackle break succeeds
    /// 8. Breakaway check (NextDouble) - long run opportunity
    /// 9. [Conditional] Breakaway yards (NextInt 15-44) - only if breakaway succeeds
    /// 10. Tackle penalty check (NextDouble) - unnecessary roughness, etc.
    /// 11. Fumble check (NextDouble) - ball carrier fumbles
    /// 12. Elapsed time random factor (NextDouble) - play duration
    ///
    /// NOTE: When Phase 3 adds pre-snap penalties, we'll insert those checks at position 1,
    /// and all tests using these factories will automatically work.
    /// </summary>
    public static class RunPlayScenarios
    {
        #region Basic Scenarios

        /// <summary>
        /// Simple gain scenario - RB gets ball, moderate blocking, no special plays.
        /// Most common scenario for testing basic run play mechanics.
        ///
        /// Random sequence:
        /// 1. QB check (0.15) - RB gets ball (> 0.10 threshold)
        /// 2. Direction (2) - Middle
        /// 3. Blocking check (0.5) - Moderate success
        /// 4. Blocking penalty (0.99) - No penalty
        /// 5. Base yards factor - Calculated to achieve target yards
        /// 6. Tackle break (0.9) - No tackle break
        /// 7. Breakaway (0.9) - No breakaway
        /// 8. Tackle penalty (0.99) - No penalty
        /// 9. Injury checks (3x)
        /// 10. Fumble (0.99) - No fumble
        /// 11. Out of bounds (0.99) - No OOB
        /// 12. Elapsed time (0.5) - ~6.5 seconds
        /// 13. Runoff time (0.5) - ~30 seconds
        /// </summary>
        /// <param name="yards">Target yards to gain (will calculate appropriate random factor)</param>
        /// <param name="direction">Run direction (0-4), default 2 (Middle)</param>
        /// <returns>Configured TestFluentSeedableRandom for this scenario</returns>
        public static TestFluentSeedableRandom SimpleGain(int yards, int direction = 2)
        {
            // SimpleGain uses blocking check 0.4 which typically succeeds (1.2x modifier)
            // Calculate base yards needed: baseYards * 1.2 >= yards
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(direction)
                .RunBlockingCheck(0.4)  // Lower value = likely to succeed
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks (ball carrier + 2 tacklers)
                .InjuryOccurredCheck(0.99)      // Ball carrier no injury
                .TacklerInjuryGateCheck(0.9)    // Tackler 1 skip
                .TacklerInjuryGateCheck(0.9)    // Tackler 2 skip
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        /// <summary>
        /// QB scramble scenario - QB keeps ball and runs.
        /// Used when QB check falls below 0.10 threshold.
        ///
        /// Random sequence: Same as SimpleGain but with QB check = 0.05
        /// </summary>
        /// <param name="yards">Target yards to gain</param>
        /// <param name="direction">Run direction, default 2 (Middle)</param>
        public static TestFluentSeedableRandom QBScramble(int yards, int direction = 2)
        {
            // QBScramble uses blocking check 0.4 which typically succeeds (1.2x modifier)
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.05)
                .RunDirection(direction)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Tackle for loss scenario - defense stuffs the run for negative yards.
        /// Uses bad blocking and low base yards factor.
        ///
        /// Random sequence:
        /// - Bad blocking (0.7-0.8)
        /// - Low base yards factor (0.05-0.2) produces negative yards
        /// </summary>
        /// <param name="lossYards">Yards lost (positive number, will result in negative gain)</param>
        public static TestFluentSeedableRandom TackleForLoss(int lossYards = 2)
        {
            // TFL uses blocking check 0.8 which typically fails (0.8x modifier)
            // For negative yards, we need base yards that result in negative after modifier
            int baseYards = (int)Math.Ceiling(-lossYards / 0.8);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.8)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        #endregion

        #region Blocking Variance Scenarios

        /// <summary>
        /// Good blocking scenario - offensive line dominates, creates running lanes.
        /// Blocking check succeeds (< 0.5 threshold).
        /// </summary>
        /// <param name="yards">Target yards to gain</param>
        public static TestFluentSeedableRandom GoodBlocking(int yards)
        {
            // Good blocking (0.4) succeeds = 1.2x modifier
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Bad blocking scenario - offensive line fails, RB hit in backfield.
        /// Blocking check fails (>= 0.5 threshold).
        /// </summary>
        /// <param name="yards">Target yards to gain</param>
        /// <param name="yardsRandomOverride">Ignored - kept for API compatibility</param>
        public static TestFluentSeedableRandom BadBlocking(int yards, double? yardsRandomOverride = null)
        {
            // Bad blocking (0.6) fails = 0.8x modifier
            int baseYards = (int)Math.Ceiling(yards / 0.8);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.6)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        #endregion

        #region Special Play Scenarios

        /// <summary>
        /// Tackle break scenario - ball carrier breaks initial tackle for extra yards.
        ///
        /// Random sequence includes tackle break yards (NextInt 3-8).
        /// NOTE: This adds an extra random value compared to simple scenarios.
        /// </summary>
        /// <param name="baseYards">Target base yards before tackle break</param>
        /// <param name="tackleBreakYards">Additional yards from breaking tackle (3-8)</param>
        /// <param name="blockingValue">Blocking check value (default 0.4 = good blocking)</param>
        /// <param name="baseYardsFactor">Ignored - kept for API compatibility</param>
        public static TestFluentSeedableRandom TackleBreak(int baseYards, int tackleBreakYards = 5,
            double blockingValue = 0.4, double baseYardsFactor = 0.68)
        {
            if (tackleBreakYards < 3 || tackleBreakYards > 8)
                throw new ArgumentOutOfRangeException(nameof(tackleBreakYards),
                    "Tackle break yards must be 3-8 per TackleBreakYardsSkillsCheckResult");

            // Blocking modifier: 0.4 succeeds (1.2x), 0.6+ fails (0.8x)
            double blockingModifier = blockingValue < 0.5 ? 1.2 : 0.8;
            int runBaseYards = (int)Math.Ceiling(baseYards / blockingModifier);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(blockingValue)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(runBaseYards)
                .TackleBreakCheck(0.1)
                .TackleBreakYards(tackleBreakYards)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Breakaway run scenario - ball carrier breaks free for significant yardage.
        ///
        /// Random sequence includes breakaway yards (NextInt 15-44).
        /// NOTE: This adds an extra random value compared to simple scenarios.
        /// </summary>
        /// <param name="baseYards">Yards before breakaway</param>
        /// <param name="breakawayYards">Additional yards from breakaway (15-44)</param>
        public static TestFluentSeedableRandom Breakaway(int baseYards, int breakawayYards = 30)
        {
            if (breakawayYards < 15 || breakawayYards > 44)
                throw new ArgumentOutOfRangeException(nameof(breakawayYards),
                    "Breakaway yards must be 15-44 per BreakawayYardsSkillsCheckResult");

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.5)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget((int)Math.Ceiling(baseYards / 1.2))  // Good blocking (1.2x)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.05)
                .BreakawayYards(breakawayYards)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Maximum yardage scenario - everything goes right: good blocking, tackle break, AND breakaway.
        ///
        /// Random sequence includes BOTH tackle break yards AND breakaway yards.
        /// NOTE: This adds TWO extra random values.
        /// </summary>
        /// <param name="tackleBreakYards">Tackle break bonus (3-8)</param>
        /// <param name="breakawayYards">Breakaway bonus (15-44)</param>
        public static TestFluentSeedableRandom MaximumYardage(int tackleBreakYards = 8, int breakawayYards = 40)
        {
            if (tackleBreakYards < 3 || tackleBreakYards > 8)
                throw new ArgumentOutOfRangeException(nameof(tackleBreakYards));
            if (breakawayYards < 15 || breakawayYards > 44)
                throw new ArgumentOutOfRangeException(nameof(breakawayYards));

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.3)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(5)  // Base 5 yards + tackle break + breakaway
                .TackleBreakCheck(0.05)
                .TackleBreakYards(tackleBreakYards)
                .BreakawayCheck(0.02)
                .BreakawayYards(breakawayYards)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)  // Add missing 2nd tackler
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5)
                .RunoffTimeRandomFactor(0.5);
        }

        #endregion

        #region Fumble Scenarios

        /// <summary>
        /// Fumble scenario - ball carrier fumbles after gaining yards.
        ///
        /// Random sequence: Same as SimpleGain but fumble check = 0.01 (occurs).
        /// NOTE: No runoff time when fumble occurs (clock handling is different)
        /// </summary>
        /// <param name="yardsBeforeFumble">Yards gained before fumble</param>
        public static TestFluentSeedableRandom Fumble(int yardsBeforeFumble)
        {
            // Blocking check 0.4 succeeds = 1.2x modifier
            int baseYards = (int)Math.Ceiling(yardsBeforeFumble / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .FumbleCheck(0.01)
                .ElapsedTimeRandomFactor(0.5);
            // NOTE: No RunoffTimeRandomFactor - fumble handling doesn't consume it
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Blocking penalty scenario - offensive holding during run.
        ///
        /// Random sequence: Same as SimpleGain but blocking penalty = 0.01 (occurs) + penalty effect random values.
        /// This triggers BlockingPenaltyOccurredSkillsCheck to detect holding.
        /// NOTE: No runoff time when penalty occurs
        /// </summary>
        /// <param name="yards">Yards that would have been gained</param>
        public static TestFluentSeedableRandom WithBlockingPenalty(int yards)
        {
            // Blocking check 0.4 succeeds = 1.2x modifier
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.01)
                .NextDouble(0.5)    // Penalty effect: team selection
                .NextInt(5)     // Penalty effect: player selection
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
                // NOTE: No RunoffTimeRandomFactor - penalty stops clock
        }

        /// <summary>
        /// Tackle penalty scenario - unnecessary roughness on tackle.
        ///
        /// Random sequence: Same as SimpleGain but tackle penalty = 0.01 (occurs) + penalty effect random values.
        /// This triggers TacklePenaltyOccurredSkillsCheck with BallCarrier context.
        /// NOTE: No runoff time when penalty occurs
        /// </summary>
        /// <param name="yards">Yards gained before penalty</param>
        public static TestFluentSeedableRandom WithTacklePenalty(int yards)
        {
            // Blocking check 0.4 succeeds = 1.2x modifier
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.99)
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.01)
                .NextDouble(0.5)              // Penalty effect: team selection
                .NextInt(5)            // Penalty effect: player selection
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
                // NOTE: No RunoffTimeRandomFactor - penalty stops clock
        }

        /// <summary>
        /// Run with BOTH blocking penalty AND tackle penalty.
        /// Tests that multiple penalties can be detected on the same play.
        ///
        /// Random sequence: QB check → Direction → Blocking → BLOCKING PENALTY → Penalty effect →
        /// Base yards → Tackle break → Breakaway → TACKLE PENALTY → Penalty effect → Fumble → Time
        /// NOTE: No runoff time when penalties occur
        /// </summary>
        public static TestFluentSeedableRandom WithBlockingAndTacklePenalty(int yards)
        {
            // Blocking check 0.4 succeeds = 1.2x modifier
            int baseYards = (int)Math.Ceiling(yards / 1.2);

            return new TestFluentSeedableRandom()
                .QBCheck(0.15)
                .RunDirection(2)
                .RunBlockingCheck(0.4)
                .BlockingPenaltyCheck(0.01)
                .NextDouble(0.5)         // Penalty effect: team selection
                .NextInt(5)         // Penalty effect: player selection
                .RunYardsForTarget(baseYards)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .TacklePenaltyCheck(0.01)
                .NextDouble(0.5)         // Penalty effect: team selection
                .NextInt(5)        // Penalty effect: player selection
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .FumbleCheck(0.99)
                .OutOfBoundsCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
                // NOTE: No RunoffTimeRandomFactor - penalties stop clock
        }

        #endregion

        #region Custom Builders

        /// <summary>
        /// Custom scenario builder for complete control over all random values.
        /// Use this for edge cases or when testing specific combinations not covered by other scenarios.
        ///
        /// IMPORTANT: This method is useful when you need precise control, but prefer using
        /// the named scenario methods above for maintainability. When random sequences change
        /// (like adding pre-snap penalties), custom builders will need manual updates.
        /// </summary>
        public static TestFluentSeedableRandom Custom(
            bool qbScrambles = false,
            int direction = 2,
            double blockingCheckValue = 0.5,
            bool blockingPenalty = false,
            double baseYardsFactor = 0.6,
            bool tackleBreak = false,
            int tackleBreakYards = 5,
            bool breakaway = false,
            int breakawayYards = 30,
            bool tacklePenalty = false,
            bool fumble = false,
            double elapsedTimeFactor = 0.5)
        {
            var rng = new TestFluentSeedableRandom()
                .QBCheck(qbScrambles ? 0.05 : 0.15)
                .RunDirection(direction)
                .RunBlockingCheck(blockingCheckValue)
                .BlockingPenaltyCheck(blockingPenalty ? 0.01 : 0.99);

            if (blockingPenalty)
            {
                rng.NextDouble(0.5).NextInt(5);  // Penalty effect randoms
            }

            // Custom scenario - use blocking modifier to calculate base yards
            double blockingModifier = blockingCheckValue < 0.5 ? 1.2 : 0.8;
            // Note: baseYardsFactor is ignored with log-normal; we need to calculate differently
            // Assuming baseYardsFactor maps roughly to ~5 yards average, use that as target
            int targetYards = 5;
            int runBaseYards = (int)Math.Ceiling(targetYards / blockingModifier);

            rng.RunYardsForTarget(runBaseYards)
                .TackleBreakCheck(tackleBreak ? 0.1 : 0.9);

            if (tackleBreak)
                rng.TackleBreakYards(tackleBreakYards);

            rng.BreakawayCheck(breakaway ? 0.05 : 0.9);

            if (breakaway)
                rng.BreakawayYards(breakawayYards);

            rng.TacklePenaltyCheck(tacklePenalty ? 0.01 : 0.99);

            if (tacklePenalty)
            {
                rng.NextDouble(0.5).NextInt(5);  // Penalty effect randoms
            }

            // Injury checks (always present now)
            rng.InjuryOccurredCheck(0.99)        // Ball carrier no injury
                .TacklerInjuryGateCheck(0.9)      // Tackler 1 skip
                .TacklerInjuryGateCheck(0.9);     // Tackler 2 skip

            rng.FumbleCheck(fumble ? 0.01 : 0.99)
                .OutOfBoundsCheck(0.99)                 // Out of bounds check
                .ElapsedTimeRandomFactor(elapsedTimeFactor);

            // Only add runoff time if clock keeps running (no penalty, no fumble)
            if (!blockingPenalty && !tacklePenalty && !fumble)
            {
                rng.RunoffTimeRandomFactor(0.5);
            }

            return rng;
        }

        #endregion
    }
}
