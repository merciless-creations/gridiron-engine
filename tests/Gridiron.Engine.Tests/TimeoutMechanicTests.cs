using Gridiron.Engine.Domain;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Simulation.Mechanics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    [TestClass]
    public class TimeoutMechanicTests
    {
        #region Execute Timeout Tests

        [TestMethod]
        public void Execute_WithTimeoutsRemaining_DecrementsCount()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 3;
            var mechanic = new TimeoutMechanic();

            // Act
            var result = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, game.HomeTimeoutsRemaining);
            Assert.AreEqual(2, result.TimeoutsRemainingAfter);
        }

        [TestMethod]
        public void Execute_AwayTeam_DecrementsAwayCount()
        {
            // Arrange
            var game = CreateTestGame();
            game.AwayTimeoutsRemaining = 3;
            var mechanic = new TimeoutMechanic();

            // Act
            var result = mechanic.Execute(game, Possession.Away, TimeoutDecision.IceKicker);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, game.AwayTimeoutsRemaining);
            Assert.AreEqual(Possession.Away, result.Team);
            Assert.AreEqual(TimeoutDecision.IceKicker, result.Reason);
        }

        [TestMethod]
        public void Execute_NoTimeoutsRemaining_ReturnsFailed()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 0;
            var mechanic = new TimeoutMechanic();

            // Act
            var result = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("No timeouts remaining", result.FailureReason);
            Assert.AreEqual(0, game.HomeTimeoutsRemaining); // Should not change
        }

        [TestMethod]
        public void Execute_SetsPlayClockResetValue()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 2;
            var mechanic = new TimeoutMechanic();

            // Act
            var result = mechanic.Execute(game, Possession.Home, TimeoutDecision.AvoidDelayOfGame);

            // Assert
            Assert.AreEqual(GameProbabilities.Timeouts.PLAY_CLOCK_AFTER_TIMEOUT, result.PlayClockResetTo);
            Assert.AreEqual(25, result.PlayClockResetTo);
        }

        [TestMethod]
        public void Execute_MultipleTimeouts_DecrementsCorrectly()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 3;
            var mechanic = new TimeoutMechanic();

            // Act - Call 3 timeouts
            var result1 = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);
            var result2 = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);
            var result3 = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);
            var result4 = mechanic.Execute(game, Possession.Home, TimeoutDecision.StopClock);

            // Assert
            Assert.IsTrue(result1.Success);
            Assert.AreEqual(2, result1.TimeoutsRemainingAfter);

            Assert.IsTrue(result2.Success);
            Assert.AreEqual(1, result2.TimeoutsRemainingAfter);

            Assert.IsTrue(result3.Success);
            Assert.AreEqual(0, result3.TimeoutsRemainingAfter);

            Assert.IsFalse(result4.Success); // No more timeouts
            Assert.AreEqual(0, game.HomeTimeoutsRemaining);
        }

        #endregion

        #region Reset Timeouts Tests

        [TestMethod]
        public void ResetTimeoutsForHalf_ResetsToThree()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 1;
            game.AwayTimeoutsRemaining = 0;
            var mechanic = new TimeoutMechanic();

            // Act
            mechanic.ResetTimeoutsForHalf(game);

            // Assert
            Assert.AreEqual(3, game.HomeTimeoutsRemaining);
            Assert.AreEqual(3, game.AwayTimeoutsRemaining);
        }

        [TestMethod]
        public void SetTimeoutsForOvertime_SetsToTwo()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 3;
            game.AwayTimeoutsRemaining = 3;
            var mechanic = new TimeoutMechanic();

            // Act
            mechanic.SetTimeoutsForOvertime(game);

            // Assert
            Assert.AreEqual(2, game.HomeTimeoutsRemaining);
            Assert.AreEqual(2, game.AwayTimeoutsRemaining);
        }

        #endregion

        #region Game.GetTimeoutsRemaining Tests

        [TestMethod]
        public void Game_GetTimeoutsRemaining_ReturnsCorrectValue()
        {
            // Arrange
            var game = CreateTestGame();
            game.HomeTimeoutsRemaining = 2;
            game.AwayTimeoutsRemaining = 1;

            // Act & Assert
            Assert.AreEqual(2, game.GetTimeoutsRemaining(Possession.Home));
            Assert.AreEqual(1, game.GetTimeoutsRemaining(Possession.Away));
            Assert.AreEqual(0, game.GetTimeoutsRemaining(Possession.None));
        }

        #endregion

        #region Game Initial State Tests

        [TestMethod]
        public void Game_InitialTimeouts_AreThree()
        {
            // Arrange & Act
            var game = new Game();

            // Assert
            Assert.AreEqual(3, game.HomeTimeoutsRemaining);
            Assert.AreEqual(3, game.AwayTimeoutsRemaining);
        }

        #endregion

        #region Helper Methods

        private Game CreateTestGame()
        {
            return new Game
            {
                HomeTeam = new Team { Name = "Home Team", City = "Home" },
                AwayTeam = new Team { Name = "Away Team", City = "Away" }
            };
        }

        #endregion
    }
}
