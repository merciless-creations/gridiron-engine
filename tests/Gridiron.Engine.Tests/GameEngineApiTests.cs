using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Tests for the public IGameEngine API
    /// </summary>
    [TestClass]
    public class GameEngineApiTests
    {
        [TestMethod]
        public void SimulateGame_WithValidTeams_ReturnsGameResult()
        {
            // Arrange
            var engine = new GameEngine();
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();

            // Act
            var result = engine.SimulateGame(homeTeam, awayTeam);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Game);
            Assert.IsTrue(result.TotalPlays > 0);
        }

        [TestMethod]
        public void SimulateGame_WithSeed_ProducesDeterministicResults()
        {
            // Arrange
            var engine = new GameEngine();
            var options = new SimulationOptions { RandomSeed = 12345 };

            // Act - run twice with same seed
            var homeTeam1 = TestTeams.LoadAtlantaFalcons();
            var awayTeam1 = TestTeams.LoadPhiladelphiaEagles();
            var result1 = engine.SimulateGame(homeTeam1, awayTeam1, options);

            var homeTeam2 = TestTeams.LoadAtlantaFalcons();
            var awayTeam2 = TestTeams.LoadPhiladelphiaEagles();
            var result2 = engine.SimulateGame(homeTeam2, awayTeam2, options);

            // Assert - same seed = same results
            Assert.AreEqual(result1.HomeScore, result2.HomeScore);
            Assert.AreEqual(result1.AwayScore, result2.AwayScore);
            Assert.AreEqual(result1.TotalPlays, result2.TotalPlays);
        }

        [TestMethod]
        public void SimulateGame_WithDifferentSeeds_ProducesDifferentResults()
        {
            // Arrange
            var engine = new GameEngine();

            // Act
            var homeTeam1 = TestTeams.LoadAtlantaFalcons();
            var awayTeam1 = TestTeams.LoadPhiladelphiaEagles();
            var result1 = engine.SimulateGame(homeTeam1, awayTeam1, new SimulationOptions { RandomSeed = 11111 });

            var homeTeam2 = TestTeams.LoadAtlantaFalcons();
            var awayTeam2 = TestTeams.LoadPhiladelphiaEagles();
            var result2 = engine.SimulateGame(homeTeam2, awayTeam2, new SimulationOptions { RandomSeed = 99999 });

            // Assert - different seeds should produce different results (statistically almost certain)
            var sameScore = result1.HomeScore == result2.HomeScore && result1.AwayScore == result2.AwayScore;
            var samePlays = result1.TotalPlays == result2.TotalPlays;
            Assert.IsFalse(sameScore && samePlays, "Different seeds should produce different game results");
        }

        [TestMethod]
        public void SimulateGame_ReturnsValidScores()
        {
            // Arrange
            var engine = new GameEngine();
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();

            // Act
            var result = engine.SimulateGame(homeTeam, awayTeam, new SimulationOptions { RandomSeed = 42 });

            // Assert
            Assert.IsTrue(result.HomeScore >= 0);
            Assert.IsTrue(result.AwayScore >= 0);
            Assert.IsTrue(result.HomeScore + result.AwayScore > 0, "At least one team should score");
        }

        [TestMethod]
        public void SimulateGame_SetsRandomSeedOnResult()
        {
            // Arrange
            var engine = new GameEngine();
            var options = new SimulationOptions { RandomSeed = 54321 };
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();

            // Act
            var result = engine.SimulateGame(homeTeam, awayTeam, options);

            // Assert
            Assert.AreEqual(54321, result.RandomSeed);
        }

        [TestMethod]
        public void SimulateGame_Winner_ReturnsCorrectTeam()
        {
            // Arrange
            var engine = new GameEngine();
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();

            // Act - use seed that produces a clear winner
            var result = engine.SimulateGame(homeTeam, awayTeam, new SimulationOptions { RandomSeed = 12345 });

            // Assert
            if (!result.IsTie)
            {
                Assert.IsNotNull(result.Winner);
                if (result.HomeScore > result.AwayScore)
                {
                    Assert.AreEqual(result.HomeTeam, result.Winner);
                }
                else
                {
                    Assert.AreEqual(result.AwayTeam, result.Winner);
                }
            }
        }

        [TestMethod]
        public void SimulateGame_WithNullHomeTeam_ThrowsException()
        {
            // Arrange
            var engine = new GameEngine();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();
            var exceptionThrown = false;

            // Act
            try
            {
                engine.SimulateGame(null!, awayTeam);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsTrue(exceptionThrown, "Expected ArgumentNullException for null home team");
        }

        [TestMethod]
        public void SimulateGame_WithNullAwayTeam_ThrowsException()
        {
            // Arrange
            var engine = new GameEngine();
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var exceptionThrown = false;

            // Act
            try
            {
                engine.SimulateGame(homeTeam, null!);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsTrue(exceptionThrown, "Expected ArgumentNullException for null away team");
        }

        [TestMethod]
        public void SimulateGame_PlaysArePopulated()
        {
            // Arrange
            var engine = new GameEngine();
            var homeTeam = TestTeams.LoadAtlantaFalcons();
            var awayTeam = TestTeams.LoadPhiladelphiaEagles();

            // Act
            var result = engine.SimulateGame(homeTeam, awayTeam, new SimulationOptions { RandomSeed = 99999 });

            // Assert
            Assert.IsTrue(result.Plays.Count > 50, "A full game should have many plays");
            Assert.AreEqual(result.Plays.Count, result.TotalPlays);
        }
    }
}
