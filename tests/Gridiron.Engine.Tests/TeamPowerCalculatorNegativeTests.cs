using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Calculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Negative tests for TeamPowerCalculator to verify null safety and edge case handling.
    /// All tests are deterministic with fixed inputs.
    /// </summary>
    [TestClass]
    public class TeamPowerCalculatorNegativeTests
    {
        private const double DEFAULT_POWER = 50.0;

        #region CalculatePassBlockingPower Null/Empty Tests

        [TestMethod]
        public void CalculatePassBlockingPower_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                TeamPowerCalculator.CalculatePassBlockingPower(null!));
        }

        [TestMethod]
        public void CalculatePassBlockingPower_EmptyList_ReturnsDefaultPower()
        {
            // Arrange
            var emptyList = new List<Player>();

            // Act
            var power = TeamPowerCalculator.CalculatePassBlockingPower(emptyList);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion

        #region CalculatePassRushPower Null/Empty Tests

        [TestMethod]
        public void CalculatePassRushPower_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                TeamPowerCalculator.CalculatePassRushPower(null!));
        }

        [TestMethod]
        public void CalculatePassRushPower_EmptyList_ReturnsDefaultPower()
        {
            // Arrange
            var emptyList = new List<Player>();

            // Act
            var power = TeamPowerCalculator.CalculatePassRushPower(emptyList);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion

        #region CalculateRunBlockingPower Null/Empty Tests

        [TestMethod]
        public void CalculateRunBlockingPower_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                TeamPowerCalculator.CalculateRunBlockingPower(null!));
        }

        [TestMethod]
        public void CalculateRunBlockingPower_EmptyList_ReturnsDefaultPower()
        {
            // Arrange
            var emptyList = new List<Player>();

            // Act
            var power = TeamPowerCalculator.CalculateRunBlockingPower(emptyList);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion

        #region CalculateRunDefensePower Null/Empty Tests

        [TestMethod]
        public void CalculateRunDefensePower_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                TeamPowerCalculator.CalculateRunDefensePower(null!));
        }

        [TestMethod]
        public void CalculateRunDefensePower_EmptyList_ReturnsDefaultPower()
        {
            // Arrange
            var emptyList = new List<Player>();

            // Act
            var power = TeamPowerCalculator.CalculateRunDefensePower(emptyList);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion

        #region CalculateCoveragePower Null/Empty Tests

        [TestMethod]
        public void CalculateCoveragePower_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                TeamPowerCalculator.CalculateCoveragePower(null!));
        }

        [TestMethod]
        public void CalculateCoveragePower_EmptyList_ReturnsDefaultPower()
        {
            // Arrange
            var emptyList = new List<Player>();

            // Act
            var power = TeamPowerCalculator.CalculateCoveragePower(emptyList);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion

        #region Edge Cases - Lists with No Matching Positions

        [TestMethod]
        public void CalculatePassBlockingPower_NoBlockers_ReturnsDefaultPower()
        {
            // Arrange - Only non-blocking positions
            var players = new List<Player>
            {
                new Player { Position = Positions.QB, Blocking = 30 },
                new Player { Position = Positions.WR, Blocking = 40 },
                new Player { Position = Positions.WR, Blocking = 35 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassBlockingPower(players);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        [TestMethod]
        public void CalculatePassRushPower_NoRushers_ReturnsDefaultPower()
        {
            // Arrange - Only coverage positions
            var players = new List<Player>
            {
                new Player { Position = Positions.CB, Tackling = 70, Speed = 90, Strength = 60 },
                new Player { Position = Positions.S, Tackling = 75, Speed = 85, Strength = 65 },
                new Player { Position = Positions.FS, Tackling = 72, Speed = 88, Strength = 62 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassRushPower(players);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        [TestMethod]
        public void CalculateRunDefensePower_NoRunDefenders_ReturnsDefaultPower()
        {
            // Arrange - Only coverage positions
            var players = new List<Player>
            {
                new Player { Position = Positions.CB, Tackling = 70, Strength = 60, Speed = 90 },
                new Player { Position = Positions.FS, Tackling = 72, Strength = 62, Speed = 88 }
            };

            // Act
            var power = TeamPowerCalculator.CalculateRunDefensePower(players);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        [TestMethod]
        public void CalculateCoveragePower_NoCoverageDefenders_ReturnsDefaultPower()
        {
            // Arrange - Only D-Line (no coverage responsibilities)
            var players = new List<Player>
            {
                new Player { Position = Positions.DE, Coverage = 30, Speed = 80, Awareness = 60 },
                new Player { Position = Positions.DT, Coverage = 25, Speed = 65, Awareness = 55 }
            };

            // Act
            var power = TeamPowerCalculator.CalculateCoveragePower(players);

            // Assert
            Assert.AreEqual(DEFAULT_POWER, power);
        }

        #endregion
    }
}
