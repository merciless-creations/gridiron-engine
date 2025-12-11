using System.Linq;
using Gridiron.Engine.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gridiron.Engine.Simulation.SkillsCheckResults;
using Gridiron.Engine.Tests.Helpers;

namespace Gridiron.Engine.Tests
{
    [TestClass]
    public class SkillsCheckResultsTests
    {
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void FumblePossessionChangeSkillsCheckResultAwayTeamTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(1); // Next(2) will return 1 → Away

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Away, fumbleResult.Possession);
        }

        [TestMethod]
        public void FumblePossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(0); // Next(2) will return 0 → Home (any value != 1 works)

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Home, fumbleResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay!.Possession = Possession.Away;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Home, interceptionResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultAwayTeamTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay!.Possession = Possession.Home;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Away, interceptionResult.Possession);
        }

        // NOTE: Tests commented out - these test the old penalty architecture
        // New penalty tests are in PenaltySkillsChecksTests.cs
        /*
        [TestMethod]
        public void KickoffPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new KickoffPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                PenaltyData.List.Single(p => p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Name);
        }

        [TestMethod]
        public void FieldGoalPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new FieldGoalPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                PenaltyData.List.Single(p => p.Name == PenaltyNames.RoughingtheKicker).Name);
        }

        [TestMethod]
        public void PuntPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new PuntPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                PenaltyData.List.Single(p => p.Name == PenaltyNames.RoughingtheKicker).Name);
        }

        [TestMethod]
        public void RunPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new RunPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                PenaltyData.List.Single(p => p.Name == PenaltyNames.OffensiveHolding).Name);
        }

        [TestMethod]
        public void PassPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new PassPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                PenaltyData.List.Single(p => p.Name == PenaltyNames.OffensiveHolding).Name);
        }
        */

        #region AirYardsSkillsCheckResult Tests

        [TestMethod]
        public void AirYardsSkillsCheckResult_ScreenPass_ReturnsNegativeToShortYards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(1, PassType.Screen); // Target 1 yard screen pass

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Screen, 25);
            airYardsResult.Execute(game);

            // Assert - Target 1 yard, screen min is -3, so we expect ~1
            Assert.AreEqual(1, airYardsResult.Result);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ShortPass_Returns3To11Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(7, PassType.Short); // Target 7 yard short pass

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Short, 25);
            airYardsResult.Execute(game);

            // Assert - Target 7 yards, short min is 1
            Assert.AreEqual(7, airYardsResult.Result);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ForwardPass_Returns8To19Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(14, PassType.Forward); // Target 14 yard medium pass

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Forward, 25);
            airYardsResult.Execute(game);

            // Assert - Target 14 yards, forward min is 5
            Assert.AreEqual(14, airYardsResult.Result);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_DeepPass_Returns18To44Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(30, PassType.Deep); // Target 30 yard deep pass

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Deep, 25);
            airYardsResult.Execute(game);

            // Assert - Target 30 yards, deep min is 15
            Assert.AreEqual(30, airYardsResult.Result);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_DeepPassNearGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At the 90 yard line, only 10 yards to goal
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(25, PassType.Deep); // Target 25 yards, but clamped to 10

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Deep, 90);
            airYardsResult.Execute(game);

            // Assert - Distribution would produce ~25 yards, but clamped to 10 (yards to goal)
            Assert.AreEqual(10, airYardsResult.Result);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ShortPassNearGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At the 95 yard line, only 5 yards to goal
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .AirYardsForTarget(7, PassType.Short); // Target 7 yards, but clamped to 5

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Short, 95);
            airYardsResult.Execute(game);

            // Assert - Distribution would produce ~7 yards, but clamped to 5 (yards to goal)
            Assert.AreEqual(5, airYardsResult.Result);
        }

        #endregion

        #region SackYardsSkillsCheckResult Tests

        [TestMethod]
        public void SackYardsSkillsCheckResult_MidfieldSack_ReturnsNegative2To10Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .SackYards(5);

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 50); // At midfield
            sackResult.Execute(game);

            // Assert - Should return negative yards (loss)
            Assert.IsLessThan(0, sackResult.Result);
            Assert.IsGreaterThanOrEqualTo(-10, sackResult.Result);
            Assert.IsLessThanOrEqualTo(-2, sackResult.Result);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_NearOwnGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At own 5 yard line
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .SackYards(10);

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 5);
            sackResult.Execute(game);

            // Assert - Can't lose more than 5 yards (would be safety)
            Assert.AreEqual(-5, sackResult.Result);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_AtOwnGoalLine_ReturnsZero()
        {
            // Arrange - At own goal line (safety situation)
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .SackYards(7);

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 0);
            sackResult.Execute(game);

            // Assert - Already at goal line, can't lose yards
            Assert.AreEqual(0, sackResult.Result);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_DeepInOwnTerritory_AllowsFullLoss()
        {
            // Arrange - Deep in own territory at 25 yard line
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .SackYards(8);

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 25);
            sackResult.Execute(game);

            // Assert - Full loss allowed (not near goal line)
            Assert.AreEqual(-8, sackResult.Result);
        }

        #endregion

        #region YardsAfterCatchSkillsCheckResult Tests

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_ImmediateTackle_Returns0To2Yards()
        {
            // Arrange - Receiver tackled immediately (YAC check fails)
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 70, Agility = 70, Rushing = 60 };

            // YAC check fails (0.9 > probability), then Next(0, 3) returns 1
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.9)
                .ImmediateTackleYards(1);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - Should be 0-2 yards when tackled immediately
            Assert.IsGreaterThanOrEqualTo(0, yacResult.Result);
            Assert.IsLessThan(3, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_GoodOpportunity_Returns3To14Yards()
        {
            // Arrange - Receiver has YAC opportunity (check succeeds)
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 80, Agility = 75, Rushing = 70 }; // Average 75

            // YAC check succeeds (0.2 < probability)
            // Random factor: 0.5 * 8 - 2 = 2
            // No big play: 0.5 > 0.05
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.5);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - YAC potential = 75, base = 3 + 75/20 = 6.75, randomFactor = 2, total ≈ 9
            Assert.IsGreaterThan(3, yacResult.Result);
            Assert.IsLessThan(15, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_SlowReceiver_LowerYAC()
        {
            // Arrange - Slow receiver with YAC opportunity
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 60, Agility = 60, Rushing = 50 }; // Average 56.67

            // YAC check succeeds, moderate random factor
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.5);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - YAC potential = 56.67, base = 3 + 56.67/20 = 5.83, randomFactor = 2, total ≈ 8
            Assert.IsGreaterThanOrEqualTo(3, yacResult.Result);
            Assert.IsLessThanOrEqualTo(12, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_FastReceiver_HigherYAC()
        {
            // Arrange - Fast receiver with YAC opportunity
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 95, Agility = 90, Rushing = 85 }; // Average 90

            // YAC check succeeds, good random factor, no big play
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.75)
                .BigPlayCheck(0.5);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - YAC potential = 90, base = 3 + 90/20 = 7.5, randomFactor = 4, total ≈ 12
            Assert.IsGreaterThanOrEqualTo(8, yacResult.Result);
            Assert.IsLessThanOrEqualTo(18, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_BigPlay_FastReceiver_AddsExtraYards()
        {
            // Arrange - Fast receiver with big play opportunity (5% chance triggers)
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 90, Agility = 88, Rushing = 80 }; // Average 86

            // YAC check succeeds (0.2 < prob)
            // Random factor: 0.5 * 8 - 2 = 2
            // Big play check: 0.03 < 0.05 AND speed 90 > 85 → triggers
            // Big play yards: Next(10, 30) = 20
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.03)
                .BigPlayBonusYards(20);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - Should have normal YAC + big play yards (>20 total)
            Assert.IsGreaterThanOrEqualTo(20, yacResult.Result);
            Assert.IsLessThan(45, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_SlowReceiver_NoBigPlay()
        {
            // Arrange - Receiver not fast enough for big play (speed < 85)
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 80, Agility = 75, Rushing = 70 }; // Average 75, speed 80 < 85

            // YAC check succeeds, big play roll succeeds BUT speed check fails
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.03);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - Should NOT have big play yards (speed 80 < 85)
            Assert.IsLessThan(20, yacResult.Result);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheckResult_NegativeRandomFactor_MinimumZero()
        {
            // Arrange - Bad random factor could make result negative
            var game = _testGame.GetGame();
            var receiver = new Player { Speed = 60, Agility = 60, Rushing = 50 }; // Low skills

            // YAC check succeeds, very bad random factor: 0.1 * 8 - 2 = -1.2
            var rng = new TestFluentSeedableRandom()
                .YACOpportunityCheck(0.2)
                .YACRandomFactor(0.1)
                .BigPlayCheck(0.5);

            // Act
            var yacResult = new YardsAfterCatchSkillsCheckResult(rng, receiver);
            yacResult.Execute(game);

            // Assert - Should be at least 0 (Math.Max clamps)
            Assert.IsGreaterThanOrEqualTo(0, yacResult.Result);
        }

        #endregion
    }
}