using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Decision;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    [TestClass]
    public class FourthDownDecisionTests
    {
        #region Basic Decision Tests

        [TestMethod]
        public void FourthAndOne_MidField_HighChanceToGoForIt()
        {
            // Arrange - 4th and 1 at midfield, tied game, plenty of time
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 50,
                yardsToGo: 1,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act - Run multiple times to verify probability
            int goForItCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.GoForIt)
                    goForItCount++;
            }

            // Assert - Should go for it at least 50% of the time on 4th and 1
            Assert.IsTrue(goForItCount >= 50, $"Expected at least 50% go-for-it on 4th and 1, got {goForItCount}%");
        }

        [TestMethod]
        public void FourthAndLong_OwnTerritory_ShouldPunt()
        {
            // Arrange - 4th and 10 at own 25, tied game
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 25, // Home's 25 yard line
                yardsToGo: 10,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act
            int puntCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.Punt)
                    puntCount++;
            }

            // Assert - Should punt at least 80% of the time
            Assert.IsTrue(puntCount >= 80, $"Expected at least 80% punt on 4th and 10 in own territory, got {puntCount}%");
        }

        [TestMethod]
        public void FourthDown_InFieldGoalRange_ShouldAttemptFieldGoal()
        {
            // Arrange - 4th and 5 at opponent's 25 (42-yard FG)
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 75, // Opponent's 25 for home team
                yardsToGo: 5,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act
            int fieldGoalCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.AttemptFieldGoal)
                    fieldGoalCount++;
            }

            // Assert - Should attempt field goal at least 60% of the time
            Assert.IsTrue(fieldGoalCount >= 60, $"Expected at least 60% field goal attempts in FG range, got {fieldGoalCount}%");
        }

        #endregion

        #region Desperation Mode Tests

        [TestMethod]
        public void TrailingBig_LateGame_AlwaysGoForIt()
        {
            // Arrange - Down by 10, under 2 minutes, must go for it
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 50,
                yardsToGo: 5,
                scoreDifferential: -10,
                timeRemainingSeconds: 90, // Under 2 minutes
                isHome: true
            );

            // Act
            int goForItCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.GoForIt)
                    goForItCount++;
            }

            // Assert - Should always go for it in desperation
            Assert.AreEqual(100, goForItCount, "Should always go for it when trailing big in late game");
        }

        [TestMethod]
        public void Trailing_Under30Seconds_AlwaysGoForIt()
        {
            // Arrange - Down by 3, under 30 seconds
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 60,
                yardsToGo: 10,
                scoreDifferential: -3,
                timeRemainingSeconds: 25,
                isHome: true
            );

            // Act
            int goForItCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.GoForIt)
                    goForItCount++;
            }

            // Assert - Should always go for it with no time left
            Assert.AreEqual(100, goForItCount, "Should always go for it when trailing with under 30 seconds");
        }

        #endregion

        #region Field Position Tests

        [TestMethod]
        public void RedZone_FourthAndShort_MoreAggressiveDecision()
        {
            // Arrange - 4th and 2 at opponent's 5 yard line
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 95, // Home at opponent's 5
                yardsToGo: 2,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act
            int goForItCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.GoForIt)
                    goForItCount++;
            }

            // Assert - Should be more aggressive in red zone
            Assert.IsTrue(goForItCount >= 40, $"Expected at least 40% go-for-it in red zone on 4th and 2, got {goForItCount}%");
        }

        [TestMethod]
        public void NoPuntZone_MustAttemptFieldGoalOrGoForIt()
        {
            // Arrange - 4th and 10 at opponent's 30 (too close to punt effectively)
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 70, // Home at opponent's 30
                yardsToGo: 10,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act
            int puntCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.Punt)
                    puntCount++;
            }

            // Assert - Should never punt in the no-punt zone
            Assert.AreEqual(0, puntCount, "Should never punt when too close to opponent's goal");
        }

        #endregion

        #region Score Differential Tests

        [TestMethod]
        public void LeadingBig_MoreConservative()
        {
            // Arrange - Up by 17, 4th and 3 at midfield
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 50,
                yardsToGo: 3,
                scoreDifferential: 17,
                timeRemainingSeconds: 1800,
                isHome: true
            );

            // Act
            int puntOrFgCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision != FourthDownDecision.GoForIt)
                    puntOrFgCount++;
            }

            // Assert - Should be conservative when leading big
            Assert.IsTrue(puntOrFgCount >= 70, $"Expected at least 70% conservative decisions when leading big, got {puntOrFgCount}%");
        }

        [TestMethod]
        public void TrailingSmall_MoreAggressive()
        {
            // Arrange - Down by 7, 4th and 2 at midfield
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 50,
                yardsToGo: 2,
                scoreDifferential: -7,
                timeRemainingSeconds: 900, // Mid-game
                isHome: true
            );

            // Act
            int goForItCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.GoForIt)
                    goForItCount++;
            }

            // Assert - Should be more aggressive when trailing
            Assert.IsTrue(goForItCount >= 50, $"Expected at least 50% go-for-it when trailing by a TD on 4th and 2, got {goForItCount}%");
        }

        #endregion

        #region Context Struct Tests

        [TestMethod]
        public void FourthDownContext_StoresValuesCorrectly()
        {
            // Arrange & Act
            var context = new FourthDownContext(
                fieldPosition: 65,
                yardsToGo: 3,
                scoreDifferential: -7,
                timeRemainingSeconds: 450,
                isHome: false
            );

            // Assert
            Assert.AreEqual(65, context.FieldPosition);
            Assert.AreEqual(3, context.YardsToGo);
            Assert.AreEqual(-7, context.ScoreDifferential);
            Assert.AreEqual(450, context.TimeRemainingSeconds);
            Assert.IsFalse(context.IsHome);
        }

        #endregion

        #region Away Team Tests

        [TestMethod]
        public void AwayTeam_FieldGoalDistance_CalculatedCorrectly()
        {
            // Arrange - Away team at their own 40 (position 40), attacking position 0
            // FG distance should be 40 + 17 = 57 yards
            var rng = new SeedableRandom(42);
            var engine = new FourthDownDecisionEngine(rng);
            var context = new FourthDownContext(
                fieldPosition: 40,
                yardsToGo: 3,
                scoreDifferential: 0,
                timeRemainingSeconds: 1800,
                isHome: false // Away team
            );

            // Act - At 57 yards, should mostly punt (outside comfortable FG range)
            int puntCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FourthDownDecision.Punt)
                    puntCount++;
            }

            // Assert - Should punt more often than attempt 57-yard FG
            Assert.IsTrue(puntCount >= 30, $"Expected at least 30% punts at 57-yard FG distance, got {puntCount}%");
        }

        #endregion
    }
}
