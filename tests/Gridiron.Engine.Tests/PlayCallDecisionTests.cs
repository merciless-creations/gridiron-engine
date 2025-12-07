using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Actions;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Simulation.Plays;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Comprehensive tests for the PlayCallDecisionEngine and related types.
    /// Tests the Context → Decision → Mechanic pattern for play calling.
    /// </summary>
    [TestClass]
    public class PlayCallDecisionTests
    {
        #region DecideConversion Tests

        [TestMethod]
        public void DecideConversion_ReturnsExtraPointMostOfTheTime()
        {
            // Arrange - 10% 2-pt conversion rate means ~90% extra point
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateConversionContext(scoreDifferential: 0);

            // Act - Run multiple times to verify probability
            int extraPointCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.DecideConversion(context);
                if (decision == ConversionDecision.ExtraPoint)
                    extraPointCount++;
            }

            // Assert - Should be ~90% extra point (allow 85-95% range for variance)
            double percentage = extraPointCount / 10.0;
            Assert.IsTrue(percentage >= 85 && percentage <= 95,
                $"Expected ~90% extra point decisions, got {percentage}%");
        }

        [TestMethod]
        public void DecideConversion_ReturnsTwoPointConversionSometimes()
        {
            // Arrange - 10% 2-pt conversion rate
            var rng = new SeedableRandom(12345);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateConversionContext(scoreDifferential: 0);

            // Act
            int twoPointCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.DecideConversion(context);
                if (decision == ConversionDecision.TwoPointConversion)
                    twoPointCount++;
            }

            // Assert - Should be ~10% two-point (allow 5-15% range for variance)
            double percentage = twoPointCount / 10.0;
            Assert.IsTrue(percentage >= 5 && percentage <= 15,
                $"Expected ~10% two-point decisions, got {percentage}%");
        }

        [TestMethod]
        public void DecideConversion_DifferentSeedsProduceDifferentResults()
        {
            // Arrange
            var context = CreateConversionContext(scoreDifferential: 0);

            var rng1 = new SeedableRandom(1);
            var rng2 = new SeedableRandom(999999);
            var engine1 = new PlayCallDecisionEngine(rng1);
            var engine2 = new PlayCallDecisionEngine(rng2);

            // Act - Get first 10 decisions from each
            var decisions1 = new List<ConversionDecision>();
            var decisions2 = new List<ConversionDecision>();
            for (int i = 0; i < 10; i++)
            {
                decisions1.Add(engine1.DecideConversion(context));
                decisions2.Add(engine2.DecideConversion(context));
            }

            // Assert - At least some decisions should differ
            bool anyDifferent = false;
            for (int i = 0; i < 10; i++)
            {
                if (decisions1[i] != decisions2[i])
                {
                    anyDifferent = true;
                    break;
                }
            }
            Assert.IsTrue(anyDifferent, "Different seeds should produce different decision sequences");
        }

        [TestMethod]
        public void DecideConversion_UsesConfiguredProbability()
        {
            // This test validates we're using the GameProbabilities constant
            // TWO_POINT_CONVERSION_ATTEMPT = 0.10
            var expectedProbability = GameProbabilities.GameDecisions.TWO_POINT_CONVERSION_ATTEMPT;
            Assert.AreEqual(0.10, expectedProbability,
                "GameProbabilities.GameDecisions.TWO_POINT_CONVERSION_ATTEMPT should be 0.10");
        }

        #endregion

        #region DecidePlayType Tests - Normal Scrimmage

        [TestMethod]
        public void DecidePlayType_NormalPlay_Returns50PercentRun()
        {
            // Arrange - 50/50 run/pass for normal plays
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateScrimmageContext(down: Downs.First, yardsToGo: 10);

            // Act
            int runCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.DecidePlayType(context);
                if (decision == PlayCallDecision.Run)
                    runCount++;
            }

            // Assert - Should be ~50% run (allow 45-55% range)
            double percentage = runCount / 10.0;
            Assert.IsTrue(percentage >= 45 && percentage <= 55,
                $"Expected ~50% run decisions, got {percentage}%");
        }

        [TestMethod]
        public void DecidePlayType_NormalPlay_Returns50PercentPass()
        {
            // Arrange
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateScrimmageContext(down: Downs.Second, yardsToGo: 7);

            // Act
            int passCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.DecidePlayType(context);
                if (decision == PlayCallDecision.Pass)
                    passCount++;
            }

            // Assert - Should be ~50% pass (allow 45-55% range)
            double percentage = passCount / 10.0;
            Assert.IsTrue(percentage >= 45 && percentage <= 55,
                $"Expected ~50% pass decisions, got {percentage}%");
        }

        [TestMethod]
        public void DecidePlayType_AllDowns_MaintainsProbability()
        {
            // Arrange - Test across all downs
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);

            foreach (var down in new[] { Downs.First, Downs.Second, Downs.Third, Downs.Fourth })
            {
                var context = CreateScrimmageContext(down: down, yardsToGo: 5);

                // Act
                int runCount = 0;
                for (int i = 0; i < 200; i++)
                {
                    var decision = engine.DecidePlayType(context);
                    if (decision == PlayCallDecision.Run)
                        runCount++;
                }

                // Assert - Should be ~50% run (allow 40-60% for smaller sample)
                double percentage = runCount / 2.0;
                Assert.IsTrue(percentage >= 40 && percentage <= 60,
                    $"Expected ~50% run on {down}, got {percentage}%");
            }
        }

        #endregion

        #region DecidePlayType Tests - Two-Point Conversion

        [TestMethod]
        public void DecidePlayType_TwoPointConversion_Returns50PercentRun()
        {
            // Arrange - Uses TWO_POINT_RUN_PROBABILITY = 0.50
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateTwoPointContext();

            // Act
            int runCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.DecidePlayType(context);
                if (decision == PlayCallDecision.Run)
                    runCount++;
            }

            // Assert - Should be ~50% run
            double percentage = runCount / 10.0;
            Assert.IsTrue(percentage >= 45 && percentage <= 55,
                $"Expected ~50% run on 2-pt conversion, got {percentage}%");
        }

        [TestMethod]
        public void DecidePlayType_TwoPointConversion_UsesConfiguredProbability()
        {
            // Validate we're using the correct constant
            var expectedProbability = GameProbabilities.GameDecisions.TWO_POINT_RUN_PROBABILITY;
            Assert.AreEqual(0.50, expectedProbability,
                "GameProbabilities.GameDecisions.TWO_POINT_RUN_PROBABILITY should be 0.50");
        }

        [TestMethod]
        public void DecidePlayType_TwoPointConversion_ReturnsOnlyRunOrPass()
        {
            // Arrange
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = CreateTwoPointContext();

            // Act & Assert - All decisions should be Run or Pass
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.DecidePlayType(context);
                Assert.IsTrue(
                    decision == PlayCallDecision.Run || decision == PlayCallDecision.Pass,
                    $"Decision should be Run or Pass, got {decision}");
            }
        }

        #endregion

        #region PlayCallContext Tests - Struct Values

        [TestMethod]
        public void PlayCallContext_Constructor_StoresAllValues()
        {
            // Arrange & Act
            var context = new PlayCallContext(
                down: Downs.Third,
                yardsToGo: 7,
                fieldPosition: 65,
                scoreDifferential: -7,
                timeRemainingSeconds: 300,
                quarter: 4,
                isTwoPointConversion: false,
                possession: Possession.Home
            );

            // Assert
            Assert.AreEqual(Downs.Third, context.Down);
            Assert.AreEqual(7, context.YardsToGo);
            Assert.AreEqual(65, context.FieldPosition);
            Assert.AreEqual(-7, context.ScoreDifferential);
            Assert.AreEqual(300, context.TimeRemainingSeconds);
            Assert.AreEqual(4, context.Quarter);
            Assert.IsFalse(context.IsTwoPointConversion);
            Assert.AreEqual(Possession.Home, context.Possession);
        }

        [TestMethod]
        public void PlayCallContext_TwoPointConversion_HasCorrectValues()
        {
            // Arrange & Act
            var context = new PlayCallContext(
                down: Downs.None,
                yardsToGo: 2,
                fieldPosition: 98,
                scoreDifferential: -2,
                timeRemainingSeconds: 60,
                quarter: 4,
                isTwoPointConversion: true,
                possession: Possession.Away
            );

            // Assert
            Assert.IsTrue(context.IsTwoPointConversion);
            Assert.AreEqual(Downs.None, context.Down);
            Assert.AreEqual(2, context.YardsToGo);
        }

        #endregion

        #region PlayCallContext Tests - Derived Properties

        [TestMethod]
        public void PlayCallContext_IsShortYardage_TrueWhen3OrLess()
        {
            // Arrange & Act
            var context1 = CreateScrimmageContext(down: Downs.Third, yardsToGo: 1);
            var context2 = CreateScrimmageContext(down: Downs.Third, yardsToGo: 2);
            var context3 = CreateScrimmageContext(down: Downs.Third, yardsToGo: 3);
            var context4 = CreateScrimmageContext(down: Downs.Third, yardsToGo: 4);

            // Assert
            Assert.IsTrue(context1.IsShortYardage, "1 yard should be short yardage");
            Assert.IsTrue(context2.IsShortYardage, "2 yards should be short yardage");
            Assert.IsTrue(context3.IsShortYardage, "3 yards should be short yardage");
            Assert.IsFalse(context4.IsShortYardage, "4 yards should NOT be short yardage");
        }

        [TestMethod]
        public void PlayCallContext_IsLongYardage_TrueWhen7OrMore()
        {
            // Arrange & Act
            var context6 = CreateScrimmageContext(down: Downs.Second, yardsToGo: 6);
            var context7 = CreateScrimmageContext(down: Downs.Second, yardsToGo: 7);
            var context10 = CreateScrimmageContext(down: Downs.Second, yardsToGo: 10);
            var context15 = CreateScrimmageContext(down: Downs.Second, yardsToGo: 15);

            // Assert
            Assert.IsFalse(context6.IsLongYardage, "6 yards should NOT be long yardage");
            Assert.IsTrue(context7.IsLongYardage, "7 yards should be long yardage");
            Assert.IsTrue(context10.IsLongYardage, "10 yards should be long yardage");
            Assert.IsTrue(context15.IsLongYardage, "15 yards should be long yardage");
        }

        [TestMethod]
        public void PlayCallContext_IsRedZone_TrueWhenFieldPositionAtLeast80()
        {
            // Arrange & Act
            var context79 = CreateScrimmageContextWithFieldPosition(79);
            var context80 = CreateScrimmageContextWithFieldPosition(80);
            var context95 = CreateScrimmageContextWithFieldPosition(95);
            var context99 = CreateScrimmageContextWithFieldPosition(99);

            // Assert
            Assert.IsFalse(context79.IsRedZone, "Position 79 should NOT be red zone");
            Assert.IsTrue(context80.IsRedZone, "Position 80 should be red zone");
            Assert.IsTrue(context95.IsRedZone, "Position 95 should be red zone");
            Assert.IsTrue(context99.IsRedZone, "Position 99 should be red zone");
        }

        [TestMethod]
        public void PlayCallContext_IsCriticalSituation_TrueWhenQ4AndCloseGame()
        {
            // Arrange & Act - Q4, close game (within 8 points)
            var critical1 = CreateContextWithQuarterAndScore(quarter: 4, scoreDiff: 0);
            var critical2 = CreateContextWithQuarterAndScore(quarter: 4, scoreDiff: -7);
            var critical3 = CreateContextWithQuarterAndScore(quarter: 4, scoreDiff: 8);
            var critical4 = CreateContextWithQuarterAndScore(quarter: 5, scoreDiff: -3); // OT

            // Not critical - Q4 but blowout
            var notCritical1 = CreateContextWithQuarterAndScore(quarter: 4, scoreDiff: 21);
            var notCritical2 = CreateContextWithQuarterAndScore(quarter: 4, scoreDiff: -14);

            // Not critical - early game
            var notCritical3 = CreateContextWithQuarterAndScore(quarter: 1, scoreDiff: 0);
            var notCritical4 = CreateContextWithQuarterAndScore(quarter: 3, scoreDiff: -7);

            // Assert
            Assert.IsTrue(critical1.IsCriticalSituation, "Q4 tied should be critical");
            Assert.IsTrue(critical2.IsCriticalSituation, "Q4 down 7 should be critical");
            Assert.IsTrue(critical3.IsCriticalSituation, "Q4 up 8 should be critical");
            Assert.IsTrue(critical4.IsCriticalSituation, "OT down 3 should be critical");

            Assert.IsFalse(notCritical1.IsCriticalSituation, "Q4 up 21 should NOT be critical");
            Assert.IsFalse(notCritical2.IsCriticalSituation, "Q4 down 14 should NOT be critical");
            Assert.IsFalse(notCritical3.IsCriticalSituation, "Q1 tied should NOT be critical");
            Assert.IsFalse(notCritical4.IsCriticalSituation, "Q3 down 7 should NOT be critical");
        }

        [TestMethod]
        public void PlayCallContext_IsTrailing_TrueWhenScoreDifferentialNegative()
        {
            // Arrange & Act
            var trailing1 = CreateContextWithQuarterAndScore(quarter: 2, scoreDiff: -1);
            var trailing2 = CreateContextWithQuarterAndScore(quarter: 2, scoreDiff: -14);
            var tied = CreateContextWithQuarterAndScore(quarter: 2, scoreDiff: 0);
            var leading = CreateContextWithQuarterAndScore(quarter: 2, scoreDiff: 7);

            // Assert
            Assert.IsTrue(trailing1.IsTrailing, "Down 1 should be trailing");
            Assert.IsTrue(trailing2.IsTrailing, "Down 14 should be trailing");
            Assert.IsFalse(tied.IsTrailing, "Tied should NOT be trailing");
            Assert.IsFalse(leading.IsTrailing, "Leading should NOT be trailing");
        }

        [TestMethod]
        public void PlayCallContext_IsTwoMinuteWarning_TrueWhen120SecondsOrLess()
        {
            // Arrange & Act
            var context121 = CreateContextWithTimeRemaining(121);
            var context120 = CreateContextWithTimeRemaining(120);
            var context60 = CreateContextWithTimeRemaining(60);
            var context1 = CreateContextWithTimeRemaining(1);

            // Assert
            Assert.IsFalse(context121.IsTwoMinuteWarning, "121 seconds should NOT be 2-minute warning");
            Assert.IsTrue(context120.IsTwoMinuteWarning, "120 seconds should be 2-minute warning");
            Assert.IsTrue(context60.IsTwoMinuteWarning, "60 seconds should be 2-minute warning");
            Assert.IsTrue(context1.IsTwoMinuteWarning, "1 second should be 2-minute warning");
        }

        #endregion

        #region PlayCallContext Factory Method Tests

        [TestMethod]
        public void ForScrimmagePlay_CreatesCorrectContext()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.CurrentDown = Downs.Second;
            game.YardsToGo = 8;
            game.FieldPosition = 45;
            game.HomeScore = 14;
            game.AwayScore = 10;
            // TimeRemaining is calculated from all quarters, so capture initial value
            var initialTimeRemaining = game.TimeRemaining;

            // Act
            var context = PlayCallContext.ForScrimmagePlay(game, Possession.Home);

            // Assert
            Assert.AreEqual(Downs.Second, context.Down);
            Assert.AreEqual(8, context.YardsToGo);
            Assert.AreEqual(45, context.FieldPosition);
            Assert.AreEqual(4, context.ScoreDifferential); // Home leading by 4
            Assert.AreEqual(initialTimeRemaining, context.TimeRemainingSeconds);
            Assert.IsFalse(context.IsTwoPointConversion);
            Assert.AreEqual(Possession.Home, context.Possession);
        }

        [TestMethod]
        public void ForScrimmagePlay_CalculatesScoreDiffFromAwayPerspective()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 21;
            game.AwayScore = 14;

            // Act
            var context = PlayCallContext.ForScrimmagePlay(game, Possession.Away);

            // Assert - Away is trailing by 7
            Assert.AreEqual(-7, context.ScoreDifferential);
            Assert.AreEqual(Possession.Away, context.Possession);
        }

        [TestMethod]
        public void ForTwoPointConversion_CreatesCorrectContext()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 6;
            game.AwayScore = 7;
            var initialTimeRemaining = game.TimeRemaining;

            // Act
            var context = PlayCallContext.ForTwoPointConversion(game, Possession.Home);

            // Assert
            Assert.AreEqual(Downs.None, context.Down);
            Assert.AreEqual(2, context.YardsToGo);
            Assert.AreEqual(98, context.FieldPosition); // Home at opponent's 2
            Assert.AreEqual(-1, context.ScoreDifferential); // Home trailing by 1
            Assert.AreEqual(initialTimeRemaining, context.TimeRemainingSeconds);
            Assert.IsTrue(context.IsTwoPointConversion);
            Assert.AreEqual(Possession.Home, context.Possession);
        }

        [TestMethod]
        public void ForTwoPointConversion_AwayTeam_CorrectFieldPosition()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 12;

            // Act
            var context = PlayCallContext.ForTwoPointConversion(game, Possession.Away);

            // Assert - Away team 2-pt from position 2 (Home's 2-yard line)
            Assert.AreEqual(2, context.FieldPosition);
            Assert.AreEqual(-2, context.ScoreDifferential); // Away trailing by 2
            Assert.IsTrue(context.IsTwoPointConversion);
        }

        [TestMethod]
        public void ForConversionDecision_CreatesCorrectContext()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 13; // Just scored TD (was 7, now 13)
            game.AwayScore = 14;
            var initialTimeRemaining = game.TimeRemaining;

            // Act
            var context = PlayCallContext.ForConversionDecision(game, Possession.Home);

            // Assert
            Assert.AreEqual(Downs.None, context.Down);
            Assert.AreEqual(0, context.YardsToGo); // No yards to go for decision
            Assert.AreEqual(98, context.FieldPosition); // Home at opponent's 2
            Assert.AreEqual(-1, context.ScoreDifferential); // Home trailing by 1
            Assert.AreEqual(initialTimeRemaining, context.TimeRemainingSeconds);
            Assert.IsFalse(context.IsTwoPointConversion); // This is the decision context
            Assert.AreEqual(Possession.Home, context.Possession);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void DecidePlayType_GoalLine_MaintainsProbability()
        {
            // Arrange - At the goal line, should still work
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = new PlayCallContext(
                down: Downs.First,
                yardsToGo: 1,
                fieldPosition: 99, // 1-yard line
                scoreDifferential: 0,
                timeRemainingSeconds: 600,
                quarter: 2,
                isTwoPointConversion: false,
                possession: Possession.Home
            );

            // Act
            int runCount = 0;
            for (int i = 0; i < 200; i++)
            {
                var decision = engine.DecidePlayType(context);
                if (decision == PlayCallDecision.Run)
                    runCount++;
            }

            // Assert - Should still be ~50%
            double percentage = runCount / 2.0;
            Assert.IsTrue(percentage >= 40 && percentage <= 60,
                $"Goal line should still be ~50% run, got {percentage}%");
        }

        [TestMethod]
        public void DecidePlayType_Overtime_WorksCorrectly()
        {
            // Arrange - Overtime (quarter 5+)
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = new PlayCallContext(
                down: Downs.First,
                yardsToGo: 10,
                fieldPosition: 75,
                scoreDifferential: 0,
                timeRemainingSeconds: 600,
                quarter: 5, // OT
                isTwoPointConversion: false,
                possession: Possession.Away
            );

            // Act & Assert - Should not throw, should return valid decision
            var decision = engine.DecidePlayType(context);
            Assert.IsTrue(decision == PlayCallDecision.Run || decision == PlayCallDecision.Pass);
        }

        [TestMethod]
        public void DecideConversion_ZeroTimeRemaining_StillWorks()
        {
            // Arrange - Game clock expired but still need to convert
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = new PlayCallContext(
                down: Downs.None,
                yardsToGo: 0,
                fieldPosition: 98,
                scoreDifferential: -1,
                timeRemainingSeconds: 0,
                quarter: 4,
                isTwoPointConversion: false,
                possession: Possession.Home
            );

            // Act & Assert - Should not throw
            var decision = engine.DecideConversion(context);
            Assert.IsTrue(
                decision == ConversionDecision.ExtraPoint ||
                decision == ConversionDecision.TwoPointConversion);
        }

        [TestMethod]
        public void DecidePlayType_ExtremeScoreDifferential_StillWorks()
        {
            // Arrange - Blowout game
            var rng = new SeedableRandom(42);
            var engine = new PlayCallDecisionEngine(rng);
            var context = new PlayCallContext(
                down: Downs.Second,
                yardsToGo: 5,
                fieldPosition: 50,
                scoreDifferential: -42, // Down by 42
                timeRemainingSeconds: 1800,
                quarter: 2,
                isTwoPointConversion: false,
                possession: Possession.Away
            );

            // Act & Assert
            var decision = engine.DecidePlayType(context);
            Assert.IsTrue(decision == PlayCallDecision.Run || decision == PlayCallDecision.Pass);
        }

        #endregion

        #region Integration with PrePlay

        [TestMethod]
        public void PrePlay_UsesPlayCallDecisionEngine_ForScrimmagePlay()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            var rng = new SeedableRandom(42);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Add a previous play so we're not at kickoff
            game.Plays.Add(new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                PossessionChange = true,
                Result = logger
            });

            game.CurrentDown = Downs.First;
            game.YardsToGo = 10;
            game.FieldPosition = 25;

            // Act
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert - Should have determined a run or pass play
            Assert.IsNotNull(game.CurrentPlay);
            Assert.IsTrue(
                game.CurrentPlay.PlayType == PlayType.Run ||
                game.CurrentPlay.PlayType == PlayType.Pass,
                "Should be a scrimmage play");
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Possession should have changed after kickoff");
        }

        [TestMethod]
        public void PrePlay_UsesPlayCallDecisionEngine_ForConversionChoice()
        {
            // Arrange - After a touchdown, should use decision engine for conversion choice
            var testGame = new TestGame();
            var game = testGame.GetGame();
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Add touchdown play
            game.Plays.Add(new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Third,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            });

            game.HomeScore = 6;
            game.AwayScore = 0;

            // Run 100 times with different seeds to verify distribution
            int extraPointCount = 0;
            int twoPointCount = 0;

            for (int seed = 1; seed <= 100; seed++)
            {
                // Reset game state
                game.Plays.Clear();
                game.Plays.Add(new PassPlay
                {
                    Possession = Possession.Home,
                    Down = Downs.Third,
                    IsTouchdown = true,
                    PossessionChange = true,
                    Result = logger
                });

                var rng = new SeedableRandom(seed);
                var prePlay = new PrePlay(rng);
                prePlay.Execute(game);

                if (game.CurrentPlay.PlayType == PlayType.FieldGoal)
                    extraPointCount++;
                else if (game.CurrentPlay.PlayType == PlayType.Run ||
                         game.CurrentPlay.PlayType == PlayType.Pass)
                    twoPointCount++;
            }

            // Assert - Should see ~90% extra point, ~10% 2-pt
            Assert.IsTrue(extraPointCount >= 80,
                $"Expected at least 80% extra point, got {extraPointCount}%");
            Assert.IsTrue(twoPointCount >= 5,
                $"Expected at least 5% two-point, got {twoPointCount}%");
        }

        #endregion

        #region Helper Methods

        private PlayCallContext CreateConversionContext(int scoreDifferential)
        {
            return new PlayCallContext(
                down: Downs.None,
                yardsToGo: 0,
                fieldPosition: 98,
                scoreDifferential: scoreDifferential,
                timeRemainingSeconds: 600,
                quarter: 2,
                isTwoPointConversion: false,
                possession: Possession.Home
            );
        }

        private PlayCallContext CreateScrimmageContext(Downs down, int yardsToGo)
        {
            return new PlayCallContext(
                down: down,
                yardsToGo: yardsToGo,
                fieldPosition: 50,
                scoreDifferential: 0,
                timeRemainingSeconds: 900,
                quarter: 2,
                isTwoPointConversion: false,
                possession: Possession.Home
            );
        }

        private PlayCallContext CreateScrimmageContextWithFieldPosition(int fieldPosition)
        {
            return new PlayCallContext(
                down: Downs.First,
                yardsToGo: 10,
                fieldPosition: fieldPosition,
                scoreDifferential: 0,
                timeRemainingSeconds: 900,
                quarter: 2,
                isTwoPointConversion: false,
                possession: Possession.Home
            );
        }

        private PlayCallContext CreateTwoPointContext()
        {
            return new PlayCallContext(
                down: Downs.None,
                yardsToGo: 2,
                fieldPosition: 98,
                scoreDifferential: 0,
                timeRemainingSeconds: 600,
                quarter: 3,
                isTwoPointConversion: true,
                possession: Possession.Home
            );
        }

        private PlayCallContext CreateContextWithQuarterAndScore(int quarter, int scoreDiff)
        {
            return new PlayCallContext(
                down: Downs.First,
                yardsToGo: 10,
                fieldPosition: 50,
                scoreDifferential: scoreDiff,
                timeRemainingSeconds: 300,
                quarter: quarter,
                isTwoPointConversion: false,
                possession: Possession.Home
            );
        }

        private PlayCallContext CreateContextWithTimeRemaining(int seconds)
        {
            return new PlayCallContext(
                down: Downs.First,
                yardsToGo: 10,
                fieldPosition: 50,
                scoreDifferential: 0,
                timeRemainingSeconds: seconds,
                quarter: 4,
                isTwoPointConversion: false,
                possession: Possession.Home
            );
        }

        #endregion
    }
}
