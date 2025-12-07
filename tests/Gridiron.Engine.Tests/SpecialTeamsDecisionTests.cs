using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Comprehensive tests for the Special Teams decision engines:
    /// - OnsideKickDecisionEngine
    /// - FairCatchDecisionEngine
    /// Tests the Context → Decision → Mechanic pattern for special teams play calling.
    /// </summary>
    [TestClass]
    public class SpecialTeamsDecisionTests
    {
        #region OnsideKickDecisionEngine Tests

        [TestMethod]
        public void OnsideKickDecision_NotTrailing_ReturnsNormalKickoff()
        {
            // Arrange - Not trailing, should never attempt onside
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: 0,  // Tied
                timeRemainingSeconds: 300,
                quarter: 4,
                timeoutsRemaining: 3,
                kickingTeam: Possession.Home
            );

            // Act - Run multiple times
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                // Assert - Should always be normal kickoff
                Assert.AreEqual(OnsideKickDecision.NormalKickoff, decision,
                    "Should never attempt onside when not trailing");
            }
        }

        [TestMethod]
        public void OnsideKickDecision_Leading_ReturnsNormalKickoff()
        {
            // Arrange - Leading, should never attempt onside
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: 14,  // Up by 14
                timeRemainingSeconds: 60,
                quarter: 4,
                timeoutsRemaining: 1,
                kickingTeam: Possession.Away
            );

            // Act
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                Assert.AreEqual(OnsideKickDecision.NormalKickoff, decision);
            }
        }

        [TestMethod]
        public void OnsideKickDecision_TrailingBy6_ReturnsNormalKickoff()
        {
            // Arrange - Trailing by 6 (less than 7), should not trigger
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: -6,
                timeRemainingSeconds: 300,
                quarter: 4,
                timeoutsRemaining: 2,
                kickingTeam: Possession.Home
            );

            // Act
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                Assert.AreEqual(OnsideKickDecision.NormalKickoff, decision,
                    "Trailing by 6 should not trigger onside kick");
            }
        }

        [TestMethod]
        public void OnsideKickDecision_TrailingBy7_HasOnsideProbability()
        {
            // Arrange - Trailing by exactly 7
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: -7,
                timeRemainingSeconds: 300,
                quarter: 4,
                timeoutsRemaining: 2,
                kickingTeam: Possession.Home
            );

            // Act - Run many times to verify ~5% onside rate
            int onsideCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == OnsideKickDecision.OnsideKick)
                    onsideCount++;
            }

            // Assert - Should be ~5% (allow 2-8% for variance)
            double percentage = onsideCount / 10.0;
            Assert.IsTrue(percentage >= 2 && percentage <= 8,
                $"Expected ~5% onside kick attempts when trailing by 7, got {percentage}%");
        }

        [TestMethod]
        public void OnsideKickDecision_TrailingByMoreThan7_HasOnsideProbability()
        {
            // Arrange - Trailing by 14 (more than 7)
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: -14,
                timeRemainingSeconds: 300,
                quarter: 4,
                timeoutsRemaining: 2,
                kickingTeam: Possession.Away
            );

            // Act
            int onsideCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == OnsideKickDecision.OnsideKick)
                    onsideCount++;
            }

            // Assert - Should be ~5%
            double percentage = onsideCount / 10.0;
            Assert.IsTrue(percentage >= 2 && percentage <= 8,
                $"Expected ~5% onside kick attempts when trailing by 14, got {percentage}%");
        }

        [TestMethod]
        public void OnsideKickDecision_UsesConfiguredProbability()
        {
            // Validate we're using the GameProbabilities constant
            var expectedProbability = GameProbabilities.Kickoffs.ONSIDE_ATTEMPT_PROBABILITY;
            Assert.AreEqual(0.05, expectedProbability,
                "GameProbabilities.Kickoffs.ONSIDE_ATTEMPT_PROBABILITY should be 0.05");
        }

        [TestMethod]
        public void OnsideKickDecision_DifferentSeedsProduceDifferentResults()
        {
            // Arrange
            var context = new OnsideKickContext(
                scoreDifferential: -14,
                timeRemainingSeconds: 120,
                quarter: 4,
                timeoutsRemaining: 1,
                kickingTeam: Possession.Home
            );

            // Run with many different seeds and count onside decisions
            int onsideCount = 0;
            for (int seed = 0; seed < 1000; seed++)
            {
                var rng = new SeedableRandom(seed);
                var engine = new OnsideKickDecisionEngine(rng);
                if (engine.Decide(context) == OnsideKickDecision.OnsideKick)
                    onsideCount++;
            }

            // Should see variety in results
            Assert.IsTrue(onsideCount > 0, "At least some seeds should produce onside kicks");
            Assert.IsTrue(onsideCount < 1000, "Not all seeds should produce onside kicks");
        }

        [TestMethod]
        public void OnsideKickDecision_DoesNotConsumeRngWhenNotTrailing()
        {
            // Arrange - Use the fluent RNG to track calls
            // Add two values - if leading, only the first should remain unconsumed
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.5)  // First value - should NOT be consumed when leading
                .NextDouble(0.9); // Extra safety value

            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(
                scoreDifferential: 7,  // Leading
                timeRemainingSeconds: 300,
                quarter: 4,
                timeoutsRemaining: 2,
                kickingTeam: Possession.Home
            );

            // Act
            var decision = engine.Decide(context);

            // Assert - Decision should be normal kickoff (not trailing)
            Assert.AreEqual(OnsideKickDecision.NormalKickoff, decision);

            // The RNG value should still be available (first value not consumed)
            // If it was consumed, we'd get 0.9 instead of 0.5
            var nextValue = ((ISeedableRandom)rng).NextDouble();
            Assert.AreEqual(0.5, nextValue, "RNG should not be consumed when not trailing");
        }

        #endregion

        #region OnsideKickContext Tests

        [TestMethod]
        public void OnsideKickContext_Constructor_StoresAllValues()
        {
            // Arrange & Act
            var context = new OnsideKickContext(
                scoreDifferential: -10,
                timeRemainingSeconds: 180,
                quarter: 4,
                timeoutsRemaining: 2,
                kickingTeam: Possession.Away
            );

            // Assert
            Assert.AreEqual(-10, context.ScoreDifferential);
            Assert.AreEqual(180, context.TimeRemainingSeconds);
            Assert.AreEqual(4, context.Quarter);
            Assert.AreEqual(2, context.TimeoutsRemaining);
            Assert.AreEqual(Possession.Away, context.KickingTeam);
        }

        [TestMethod]
        public void OnsideKickContext_FromGame_CalculatesScoreDifferentialForHome()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 21;

            // Act
            var context = OnsideKickContext.FromGame(game, Possession.Home);

            // Assert - Home is trailing by 7
            Assert.AreEqual(-7, context.ScoreDifferential);
            Assert.AreEqual(Possession.Home, context.KickingTeam);
        }

        [TestMethod]
        public void OnsideKickContext_FromGame_CalculatesScoreDifferentialForAway()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeScore = 28;
            game.AwayScore = 21;

            // Act
            var context = OnsideKickContext.FromGame(game, Possession.Away);

            // Assert - Away is trailing by 7
            Assert.AreEqual(-7, context.ScoreDifferential);
            Assert.AreEqual(Possession.Away, context.KickingTeam);
        }

        [TestMethod]
        public void OnsideKickContext_DerivedProperties_Trailing()
        {
            // Arrange
            var context = new OnsideKickContext(-7, 300, 4, 2, Possession.Home);

            // Assert
            Assert.IsTrue(context.IsTrailing);
            Assert.IsFalse(context.IsTrailingByMoreThanOneScore); // -7 is not more than -8
            Assert.IsTrue(context.IsTrailingByOneScore); // -7 is within [-8, 0)
        }

        [TestMethod]
        public void OnsideKickContext_DerivedProperties_TrailingByTwoScores()
        {
            // Arrange
            var context = new OnsideKickContext(-17, 120, 4, 0, Possession.Home);

            // Assert
            Assert.IsTrue(context.IsTrailing);
            Assert.IsTrue(context.IsTrailingByMoreThanOneScore); // -17 <= -9
            Assert.IsFalse(context.IsTrailingByOneScore);
        }

        [TestMethod]
        public void OnsideKickContext_DerivedProperties_LateGame()
        {
            // Arrange
            var q3Context = new OnsideKickContext(-7, 600, 3, 2, Possession.Home);
            var q4Context = new OnsideKickContext(-7, 600, 4, 2, Possession.Home);
            var otContext = new OnsideKickContext(-7, 600, 5, 2, Possession.Home);

            // Assert
            Assert.IsFalse(q3Context.IsLateGame);
            Assert.IsTrue(q4Context.IsLateGame);
            Assert.IsTrue(otContext.IsLateGame);
        }

        [TestMethod]
        public void OnsideKickContext_DerivedProperties_CriticalTime()
        {
            // Arrange
            var notCritical = new OnsideKickContext(-7, 301, 4, 2, Possession.Home);
            var critical = new OnsideKickContext(-7, 300, 4, 2, Possession.Home);
            var desperate = new OnsideKickContext(-7, 120, 4, 2, Possession.Home);

            // Assert
            Assert.IsFalse(notCritical.IsCriticalTime);
            Assert.IsTrue(critical.IsCriticalTime);
            Assert.IsTrue(desperate.IsCriticalTime);
            Assert.IsFalse(notCritical.IsDesperateTime);
            Assert.IsFalse(critical.IsDesperateTime);
            Assert.IsTrue(desperate.IsDesperateTime);
        }

        #endregion

        #region FairCatchDecisionEngine Tests

        [TestMethod]
        public void FairCatchDecision_BaseProbabilityApplied()
        {
            // Arrange - Standard situation should have ~25% base fair catch rate
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 3.5, // Below medium threshold
                landingSpot: 50, // Midfield
                kickType: PlayType.Punt
            );

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~25% (allow 20-30% for variance)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 20 && percentage <= 30,
                $"Expected ~25% fair catch at midfield, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_HighHangTimeIncreasesProbability()
        {
            // Arrange - High hang time (>4.5s) should add 15%
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 5.0, // High hang time
                landingSpot: 50, // Midfield
                kickType: PlayType.Punt
            );

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~40% (25% base + 15% high hang)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 35 && percentage <= 45,
                $"Expected ~40% fair catch with high hang time, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_MediumHangTimeIncreasesProbability()
        {
            // Arrange - Medium hang time (>4.0s but <4.5s) should add 10%
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 4.2,
                landingSpot: 50,
                kickType: PlayType.Punt
            );

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~35% (25% base + 10% medium hang)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 30 && percentage <= 40,
                $"Expected ~35% fair catch with medium hang time, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_DeepInOwnTerritoryIncreasesProbability()
        {
            // Arrange - Inside own 10-yard line should add 20%
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 3.5,
                landingSpot: 95, // Receiving team at their own 5
                kickType: PlayType.Punt
            );

            // Assert - ReceivingTeamFieldPosition should be 5
            Assert.AreEqual(5, context.ReceivingTeamFieldPosition);

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~45% (25% base + 20% own 10)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 40 && percentage <= 50,
                $"Expected ~45% fair catch deep in own territory, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_InsideOwn20IncreasesProbability()
        {
            // Arrange - Inside own 20 but outside 10 should add 10%
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 3.5,
                landingSpot: 85, // Receiving team at their own 15
                kickType: PlayType.Punt
            );

            // Assert - ReceivingTeamFieldPosition should be 15
            Assert.AreEqual(15, context.ReceivingTeamFieldPosition);

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~35% (25% base + 10% own 20)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 30 && percentage <= 40,
                $"Expected ~35% fair catch inside own 20, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_CombinedFactors_HighProbability()
        {
            // Arrange - High hang time + deep in own territory = very high fair catch
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(
                hangTime: 5.0, // High hang = +15%
                landingSpot: 95, // Own 5 = +20%
                kickType: PlayType.Punt
            );

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~60% (25% + 15% + 20%)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 55 && percentage <= 65,
                $"Expected ~60% fair catch with combined factors, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_KickoffSlightlyHigherProbability()
        {
            // Arrange - Kickoffs have +5% fair catch probability
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var contextKickoff = new FairCatchContext(
                hangTime: 3.5,
                landingSpot: 50,
                kickType: PlayType.Kickoff
            );

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(contextKickoff);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be ~30% (25% base + 5% kickoff)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 25 && percentage <= 35,
                $"Expected ~30% fair catch on kickoff, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_ReturnsOnlyValidDecisions()
        {
            // Arrange
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = FairCatchContext.ForPuntReturn(4.0, 60, null);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                Assert.IsTrue(
                    decision == FairCatchDecision.FairCatch ||
                    decision == FairCatchDecision.AttemptReturn,
                    $"Decision should be FairCatch or AttemptReturn, got {decision}");
            }
        }

        #endregion

        #region FairCatchContext Tests

        [TestMethod]
        public void FairCatchContext_Constructor_StoresAllValues()
        {
            // Arrange & Act
            var context = new FairCatchContext(
                hangTime: 4.5,
                landingSpot: 75,
                kickType: PlayType.Punt,
                returnerCatching: 85,
                returnerAwareness: 80
            );

            // Assert
            Assert.AreEqual(4.5, context.HangTime);
            Assert.AreEqual(75, context.LandingSpot);
            Assert.AreEqual(25, context.ReceivingTeamFieldPosition); // 100 - 75
            Assert.AreEqual(PlayType.Punt, context.KickType);
            Assert.AreEqual(85, context.ReturnerCatching);
            Assert.AreEqual(80, context.ReturnerAwareness);
        }

        [TestMethod]
        public void FairCatchContext_ForPuntReturn_CreatesCorrectContext()
        {
            // Arrange
            var returner = new Player { Catching = 75, Awareness = 70 };

            // Act
            var context = FairCatchContext.ForPuntReturn(4.2, 65, returner);

            // Assert
            Assert.AreEqual(4.2, context.HangTime);
            Assert.AreEqual(65, context.LandingSpot);
            Assert.AreEqual(35, context.ReceivingTeamFieldPosition);
            Assert.AreEqual(PlayType.Punt, context.KickType);
            Assert.AreEqual(75, context.ReturnerCatching);
            Assert.AreEqual(70, context.ReturnerAwareness);
        }

        [TestMethod]
        public void FairCatchContext_ForKickoffReturn_CreatesCorrectContext()
        {
            // Arrange
            var returner = new Player { Catching = 80, Awareness = 85 };

            // Act
            var context = FairCatchContext.ForKickoffReturn(3.8, 70, returner);

            // Assert
            Assert.AreEqual(3.8, context.HangTime);
            Assert.AreEqual(70, context.LandingSpot);
            Assert.AreEqual(30, context.ReceivingTeamFieldPosition);
            Assert.AreEqual(PlayType.Kickoff, context.KickType);
            Assert.AreEqual(80, context.ReturnerCatching);
            Assert.AreEqual(85, context.ReturnerAwareness);
        }

        [TestMethod]
        public void FairCatchContext_DerivedProperties_HangTime()
        {
            // Arrange
            var lowHang = new FairCatchContext(3.5, 50, PlayType.Punt);
            var mediumHang = new FairCatchContext(4.2, 50, PlayType.Punt);
            var highHang = new FairCatchContext(5.0, 50, PlayType.Punt);

            // Assert
            Assert.IsFalse(lowHang.IsHighHangTime);
            Assert.IsFalse(lowHang.IsMediumHangTime);

            Assert.IsFalse(mediumHang.IsHighHangTime);
            Assert.IsTrue(mediumHang.IsMediumHangTime);

            Assert.IsTrue(highHang.IsHighHangTime);
            Assert.IsTrue(highHang.IsMediumHangTime); // High also passes medium threshold
        }

        [TestMethod]
        public void FairCatchContext_DerivedProperties_FieldPosition()
        {
            // Arrange
            var midfield = new FairCatchContext(4.0, 50, PlayType.Punt); // Own 50
            var inside20 = new FairCatchContext(4.0, 85, PlayType.Punt); // Own 15
            var inside10 = new FairCatchContext(4.0, 95, PlayType.Punt); // Own 5

            // Assert
            Assert.IsFalse(midfield.IsDeepInOwnTerritory);
            Assert.IsFalse(midfield.IsInDangerZone);

            Assert.IsFalse(inside20.IsDeepInOwnTerritory);
            Assert.IsTrue(inside20.IsInDangerZone);

            Assert.IsTrue(inside10.IsDeepInOwnTerritory);
            Assert.IsTrue(inside10.IsInDangerZone);
        }

        [TestMethod]
        public void FairCatchContext_DerivedProperties_HasGoodCoverage()
        {
            // Arrange
            var shortHang = new FairCatchContext(3.5, 50, PlayType.Punt);
            var mediumHang = new FairCatchContext(4.2, 50, PlayType.Punt);
            var highHang = new FairCatchContext(5.0, 50, PlayType.Punt);

            // Assert
            Assert.IsFalse(shortHang.HasGoodCoverage);
            Assert.IsTrue(mediumHang.HasGoodCoverage);
            Assert.IsTrue(highHang.HasGoodCoverage);
        }

        [TestMethod]
        public void FairCatchContext_DerivedProperties_ReturnerSkill()
        {
            // Arrange
            var avgReturner = new FairCatchContext(4.0, 50, PlayType.Punt, 60, 60);
            var goodCatcher = new FairCatchContext(4.0, 50, PlayType.Punt, 75, 60);
            var aware = new FairCatchContext(4.0, 50, PlayType.Punt, 60, 75);
            var eliteReturner = new FairCatchContext(4.0, 50, PlayType.Punt, 85, 90);

            // Assert
            Assert.IsFalse(avgReturner.IsGoodCatcher);
            Assert.IsFalse(avgReturner.HasHighAwareness);

            Assert.IsTrue(goodCatcher.IsGoodCatcher);
            Assert.IsFalse(goodCatcher.HasHighAwareness);

            Assert.IsFalse(aware.IsGoodCatcher);
            Assert.IsTrue(aware.HasHighAwareness);

            Assert.IsTrue(eliteReturner.IsGoodCatcher);
            Assert.IsTrue(eliteReturner.HasHighAwareness);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void OnsideKickDecision_ExactlyMinus7_HasProbability()
        {
            // Arrange - Edge case: exactly -7
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(-7, 300, 4, 2, Possession.Home);

            // Act
            int onsideCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == OnsideKickDecision.OnsideKick)
                    onsideCount++;
            }

            // Assert - Should have some onside attempts
            Assert.IsTrue(onsideCount > 0, "Should have some onside kick attempts at -7");
        }

        [TestMethod]
        public void OnsideKickDecision_FirstQuarter_StillHasProbabilityIfTrailing()
        {
            // Arrange - Q1 but trailing by 7+
            // Original behavior: any quarter, just needs to be trailing by 7+
            var rng = new SeedableRandom(42);
            var engine = new OnsideKickDecisionEngine(rng);
            var context = new OnsideKickContext(-14, 900, 1, 3, Possession.Home);

            // Act
            int onsideCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == OnsideKickDecision.OnsideKick)
                    onsideCount++;
            }

            // Assert - Should have ~5% onside attempts (original behavior)
            double percentage = onsideCount / 10.0;
            Assert.IsTrue(percentage >= 2 && percentage <= 8,
                $"Expected ~5% onside in Q1 when trailing by 14, got {percentage}%");
        }

        [TestMethod]
        public void FairCatchDecision_ZeroHangTime_StillWorks()
        {
            // Arrange - Edge case: very short hang time
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(0.5, 50, PlayType.Kickoff);

            // Act & Assert - Should not throw
            var decision = engine.Decide(context);
            Assert.IsTrue(
                decision == FairCatchDecision.FairCatch ||
                decision == FairCatchDecision.AttemptReturn);
        }

        [TestMethod]
        public void FairCatchDecision_AtGoalLine_VeryHighProbability()
        {
            // Arrange - At the goal line with high hang time
            var rng = new SeedableRandom(42);
            var engine = new FairCatchDecisionEngine(rng);
            var context = new FairCatchContext(5.0, 99, PlayType.Punt); // Own 1-yard line

            // Act
            int fairCatchCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var decision = engine.Decide(context);
                if (decision == FairCatchDecision.FairCatch)
                    fairCatchCount++;
            }

            // Assert - Should be very high (25% + 15% + 20% = 60%)
            double percentage = fairCatchCount / 10.0;
            Assert.IsTrue(percentage >= 55,
                $"Expected high fair catch rate at goal line, got {percentage}%");
        }

        #endregion

        #region Integration with Game State

        [TestMethod]
        public void OnsideKickContext_FromGame_CapturesTimeoutsRemaining()
        {
            // Arrange
            var testGame = new TestGame();
            var game = testGame.GetGame();
            game.HomeTimeoutsRemaining = 1;
            game.AwayTimeoutsRemaining = 3;

            // Act
            var homeContext = OnsideKickContext.FromGame(game, Possession.Home);
            var awayContext = OnsideKickContext.FromGame(game, Possession.Away);

            // Assert
            Assert.AreEqual(1, homeContext.TimeoutsRemaining);
            Assert.AreEqual(3, awayContext.TimeoutsRemaining);
        }

        [TestMethod]
        public void FairCatchContext_WithNullReturner_UsesDefaults()
        {
            // Arrange & Act
            var context = FairCatchContext.ForPuntReturn(4.0, 60, null);

            // Assert - Should use default values (50)
            Assert.AreEqual(50, context.ReturnerCatching);
            Assert.AreEqual(50, context.ReturnerAwareness);
        }

        #endregion
    }
}
