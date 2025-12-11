using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Calculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Negative tests for LineBattleCalculator to verify null safety and edge case handling.
    /// All tests are deterministic with fixed inputs.
    /// </summary>
    [TestClass]
    public class LineBattleCalculatorNegativeTests
    {
        #region Null Input Tests

        [TestMethod]
        public void CalculateDPressureFactor_NullOffensivePlayers_ThrowsArgumentNullException()
        {
            // Arrange
            var defensivePlayers = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                LineBattleCalculator.CalculateDPressureFactor(null!, defensivePlayers, isPassPlay: true));
        }

        [TestMethod]
        public void CalculateDPressureFactor_NullDefensivePlayers_ThrowsArgumentNullException()
        {
            // Arrange
            var offensivePlayers = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 }
            };

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                LineBattleCalculator.CalculateDPressureFactor(offensivePlayers, null!, isPassPlay: true));
        }

        [TestMethod]
        public void CalculateDPressureFactor_BothNull_ThrowsArgumentNullException()
        {
            // Act & Assert - Should throw for the first null parameter (offensivePlayers)
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                LineBattleCalculator.CalculateDPressureFactor(null!, null!, isPassPlay: true));
        }

        [TestMethod]
        public void CalculateDPressureFactor_RunPlay_NullOffensivePlayers_ThrowsArgumentNullException()
        {
            // Arrange
            var defensivePlayers = new List<Player>
            {
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                LineBattleCalculator.CalculateDPressureFactor(null!, defensivePlayers, isPassPlay: false));
        }

        [TestMethod]
        public void CalculateDPressureFactor_RunPlay_NullDefensivePlayers_ThrowsArgumentNullException()
        {
            // Arrange
            var offensivePlayers = new List<Player>
            {
                new Player { Position = Positions.G, Blocking = 70 }
            };

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                LineBattleCalculator.CalculateDPressureFactor(offensivePlayers, null!, isPassPlay: false));
        }

        #endregion

        #region Empty List Tests

        [TestMethod]
        public void CalculateDPressureFactor_BothEmptyLists_ReturnsBasePressure()
        {
            // Arrange
            var emptyOffense = new List<Player>();
            var emptyDefense = new List<Player>();

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(emptyOffense, emptyDefense, isPassPlay: true);

            // Assert - With empty lists, both teams get default power (50), no rushers (0 vs 4 standard = -4 * 0.15 = -0.6)
            // Base 1.0 + skill diff (0) + rusher impact (-0.6) = 0.4
            Assert.AreEqual(0.4, pressure, 0.001, "Empty lists: 1.0 + 0 + (-0.6) = 0.4");
        }

        [TestMethod]
        public void CalculateDPressureFactor_EmptyOffense_ValidDefense_ReturnsValidPressure()
        {
            // Arrange
            var emptyOffense = new List<Player>();
            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(emptyOffense, defense, isPassPlay: true);

            // Assert - Defense has power 70, offense gets default 50, skill diff +20
            // Logarithmic: sign(20) * log(1 + 20/10) * 0.15 ≈ 0.165
            // 4 rushers vs 4 standard = 0 rusher impact
            // Base 1.0 + skill impact (~0.165) + rusher impact (0) ≈ 1.165
            Assert.AreEqual(1.165, pressure, 0.01, "Empty offense vs 70-power defense: 1.0 + ~0.165 + 0 ≈ 1.165");
        }

        [TestMethod]
        public void CalculateDPressureFactor_ValidOffense_EmptyDefense_ReturnsValidPressure()
        {
            // Arrange
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 }
            };
            var emptyDefense = new List<Player>();

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, emptyDefense, isPassPlay: true);

            // Assert - Offense has power 70, defense gets default 50, skill diff -20
            // Logarithmic: sign(-20) * log(1 + 20/10) * 0.15 ≈ -0.165
            // Plus 0 rushers vs 4 standard = -0.6 rusher impact
            // Base 1.0 + skill impact (~-0.165) + rusher impact (-0.6) ≈ 0.235
            Assert.AreEqual(0.235, pressure, 0.01, "70-power offense vs empty defense: 1.0 - ~0.165 - 0.6 ≈ 0.235");
        }

        [TestMethod]
        public void CalculateDPressureFactor_RunPlay_EmptyLists_ReturnsValidPressure()
        {
            // Arrange
            var emptyOffense = new List<Player>();
            var emptyDefense = new List<Player>();

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(emptyOffense, emptyDefense, isPassPlay: false);

            // Assert - Same formula as pass play with empty lists
            // Base 1.0 + skill diff (0) + rusher impact (-0.6) = 0.4
            Assert.AreEqual(0.4, pressure, 0.001, "Empty lists (run): 1.0 + 0 + (-0.6) = 0.4");
        }

        #endregion

        #region Edge Cases - No Relevant Position Players

        [TestMethod]
        public void CalculateDPressureFactor_OffenseHasNoBlockers_ReturnsValidPressure()
        {
            // Arrange - Offense has only QB and WRs (no blockers)
            var offense = new List<Player>
            {
                new Player { Position = Positions.QB, Blocking = 20 },
                new Player { Position = Positions.WR, Blocking = 30 },
                new Player { Position = Positions.WR, Blocking = 35 }
            };
            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - QB/WR are not blockers, so offense gets default power (50)
            // Defense has power 70 (average of (70+70+70)/3 = 70), 4 rushers = standard
            // Logarithmic: sign(20) * log(1 + 20/10) * 0.15 ≈ 0.165
            // Base 1.0 + skill impact (~0.165) + rusher impact (0) ≈ 1.165
            Assert.AreEqual(1.165, pressure, 0.01, "No blockers vs 70-power defense: 1.0 + ~0.165 + 0 ≈ 1.165");
        }

        [TestMethod]
        public void CalculateDPressureFactor_DefenseHasNoRushers_ReturnsValidPressure()
        {
            // Arrange - Defense has only DBs (no rushers)
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 }
            };
            var defense = new List<Player>
            {
                new Player { Position = Positions.CB, Tackling = 70, Speed = 90, Strength = 60 },
                new Player { Position = Positions.CB, Tackling = 70, Speed = 90, Strength = 60 },
                new Player { Position = Positions.S, Tackling = 75, Speed = 85, Strength = 65 },
                new Player { Position = Positions.FS, Tackling = 72, Speed = 88, Strength = 62 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - DBs are not rushers, so defense gets default power (50)
            // Offense has power 70, 0 rushers vs 4 standard = -0.6
            // Logarithmic: sign(-20) * log(1 + 20/10) * 0.15 ≈ -0.165
            // Base 1.0 + skill impact (~-0.165) + rusher impact (-0.6) ≈ 0.235
            Assert.AreEqual(0.235, pressure, 0.01, "70-power offense vs no rushers: 1.0 - ~0.165 - 0.6 ≈ 0.235");
        }

        #endregion
    }
}
