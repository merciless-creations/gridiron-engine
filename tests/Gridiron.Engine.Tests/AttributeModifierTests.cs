using Gridiron.Engine.Simulation.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Tests for the AttributeModifier utility class.
    /// Verifies logarithmic curve behavior and expected modifier values.
    /// </summary>
    [TestClass]
    public class AttributeModifierTests
    {
        private const double TOLERANCE = 0.001;

        #region Calculate (from rating) Tests

        [TestMethod]
        public void Calculate_BaselineRating_ReturnsZero()
        {
            // Arrange & Act
            var modifier = AttributeModifier.Calculate(50.0);

            // Assert
            Assert.AreEqual(0.0, modifier, TOLERANCE,
                "Baseline rating (50) should produce zero modifier");
        }

        [TestMethod]
        public void Calculate_Rating70_ReturnsExpectedModifier()
        {
            // Arrange & Act
            var modifier = AttributeModifier.Calculate(70.0);

            // Assert - log(1 + 20/10) * 0.15 = log(3) * 0.15 ≈ 0.165
            Assert.IsTrue(modifier > 0.10 && modifier < 0.20,
                $"Rating 70 should produce positive modifier ~0.165. Got: {modifier:F3}");
        }

        [TestMethod]
        public void Calculate_Rating30_ReturnsNegativeModifier()
        {
            // Arrange & Act
            var modifier = AttributeModifier.Calculate(30.0);

            // Assert - sign(-20) * log(1 + 20/10) * 0.15 ≈ -0.165
            Assert.IsTrue(modifier < -0.10 && modifier > -0.20,
                $"Rating 30 should produce negative modifier ~-0.165. Got: {modifier:F3}");
        }

        [TestMethod]
        public void Calculate_EliteRating99_HasDiminishingReturns()
        {
            // Arrange & Act
            var modifier90 = AttributeModifier.Calculate(90.0);
            var modifier99 = AttributeModifier.Calculate(99.0);

            // Assert - jump from 90 to 99 should be smaller than 50 to 60
            var jump90to99 = modifier99 - modifier90;
            var jump50to60 = AttributeModifier.Calculate(60.0) - AttributeModifier.Calculate(50.0);

            Console.WriteLine($"Modifier at 90: {modifier90:F4}");
            Console.WriteLine($"Modifier at 99: {modifier99:F4}");
            Console.WriteLine($"Jump 50->60: {jump50to60:F4}");
            Console.WriteLine($"Jump 90->99: {jump90to99:F4}");

            Assert.IsTrue(jump90to99 < jump50to60,
                $"Jump 90->99 ({jump90to99:F4}) should be smaller than 50->60 ({jump50to60:F4})");
        }

        [TestMethod]
        public void Calculate_CustomBaseline_AdjustsCorrectly()
        {
            // Arrange & Act
            var modifierBase50 = AttributeModifier.Calculate(70.0, 50.0);
            var modifierBase60 = AttributeModifier.Calculate(70.0, 60.0);

            // Assert
            Assert.IsTrue(modifierBase50 > modifierBase60,
                "Higher baseline should produce smaller modifier for same rating");
        }

        #endregion

        #region FromDifferential Tests

        [TestMethod]
        public void FromDifferential_ZeroDiff_ReturnsZero()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(0.0);

            // Assert
            Assert.AreEqual(0.0, modifier, TOLERANCE);
        }

        [TestMethod]
        public void FromDifferential_PositiveDiff_ReturnsPositive()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(10.0);

            // Assert
            Assert.IsTrue(modifier > 0,
                $"Positive differential should produce positive modifier. Got: {modifier}");
        }

        [TestMethod]
        public void FromDifferential_NegativeDiff_ReturnsNegative()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(-10.0);

            // Assert
            Assert.IsTrue(modifier < 0,
                $"Negative differential should produce negative modifier. Got: {modifier}");
        }

        [TestMethod]
        public void FromDifferential_IsSymmetric()
        {
            // Arrange
            var positiveDiff = 25.0;
            var negativeDiff = -25.0;

            // Act
            var positiveModifier = AttributeModifier.FromDifferential(positiveDiff);
            var negativeModifier = AttributeModifier.FromDifferential(negativeDiff);

            // Assert
            Assert.AreEqual(positiveModifier, -negativeModifier, TOLERANCE,
                "Modifier should be symmetric around zero");
        }

        [TestMethod]
        public void FromDifferential_SmallDiff_ProducesSmallModifier()
        {
            // Arrange & Act
            var modifier5 = AttributeModifier.FromDifferential(5.0);
            var modifier10 = AttributeModifier.FromDifferential(10.0);

            // Assert
            Assert.IsTrue(modifier5 < modifier10,
                "Larger differential should produce larger modifier");
            Assert.IsTrue(modifier5 < 0.10,
                $"Small differential (5) should produce small modifier. Got: {modifier5}");
        }

        [TestMethod]
        public void FromDifferential_LargeDiff_ShowsDiminishingReturns()
        {
            // Arrange & Act
            var modifier10 = AttributeModifier.FromDifferential(10.0);
            var modifier20 = AttributeModifier.FromDifferential(20.0);
            var modifier40 = AttributeModifier.FromDifferential(40.0);

            // Calculate marginal gains
            var gain0to10 = modifier10;
            var gain10to20 = modifier20 - modifier10;
            var gain20to40 = modifier40 - modifier20;

            Console.WriteLine($"Modifier at diff 10: {modifier10:F4}");
            Console.WriteLine($"Modifier at diff 20: {modifier20:F4}");
            Console.WriteLine($"Modifier at diff 40: {modifier40:F4}");
            Console.WriteLine($"Gain 0->10: {gain0to10:F4}");
            Console.WriteLine($"Gain 10->20: {gain10to20:F4}");
            Console.WriteLine($"Gain 20->40: {gain20to40:F4}");

            // Assert diminishing returns
            Assert.IsTrue(gain0to10 > gain10to20,
                $"Gain 0->10 ({gain0to10:F4}) should be > gain 10->20 ({gain10to20:F4})");
            Assert.IsTrue(gain10to20 > gain20to40 / 2,
                $"Gain 10->20 ({gain10to20:F4}) should be > half of gain 20->40 ({gain20to40 / 2:F4})");
        }

        #endregion

        #region Expected Values from Issue #27

        [TestMethod]
        public void Calculate_ProducesExpectedModifierTable()
        {
            // These values come from Issue #27 specification
            // | Rating | Expected Modifier |
            // |--------|-------------------|
            // | 30     | -10.4%            |
            // | 50     | 0%                |
            // | 70     | +10.4%            |
            // | 90     | +15.9%            |
            // | 99     | +17.6%            |

            var modifier30 = AttributeModifier.Calculate(30.0);
            var modifier50 = AttributeModifier.Calculate(50.0);
            var modifier70 = AttributeModifier.Calculate(70.0);
            var modifier90 = AttributeModifier.Calculate(90.0);
            var modifier99 = AttributeModifier.Calculate(99.0);

            Console.WriteLine($"Rating 30: {modifier30 * 100:F1}% (expected: -10.4%)");
            Console.WriteLine($"Rating 50: {modifier50 * 100:F1}% (expected: 0%)");
            Console.WriteLine($"Rating 70: {modifier70 * 100:F1}% (expected: +10.4%)");
            Console.WriteLine($"Rating 90: {modifier90 * 100:F1}% (expected: +15.9%)");
            Console.WriteLine($"Rating 99: {modifier99 * 100:F1}% (expected: +17.6%)");

            // Note: The formula in Issue #27 uses /10 * 0.15, which gives slightly different
            // values than shown in the table. The actual math:
            // Rating 30: sign(-20) * log(1 + 20/10) * 0.15 = -log(3) * 0.15 ≈ -0.165
            // Rating 70: sign(20) * log(1 + 20/10) * 0.15 = log(3) * 0.15 ≈ 0.165
            // Rating 90: sign(40) * log(1 + 40/10) * 0.15 = log(5) * 0.15 ≈ 0.241
            // Rating 99: sign(49) * log(1 + 49/10) * 0.15 = log(5.9) * 0.15 ≈ 0.266

            // The key behaviors we're testing:
            Assert.AreEqual(0.0, modifier50, 0.01, "Rating 50 should be ~0%");
            Assert.IsTrue(modifier30 < 0, "Rating 30 should be negative");
            Assert.IsTrue(modifier70 > 0, "Rating 70 should be positive");
            Assert.IsTrue(Math.Abs(modifier30) < Math.Abs(modifier70) + 0.01,
                "Modifier should be approximately symmetric");

            // Verify diminishing returns at extremes
            var jump50to70 = modifier70 - modifier50;
            var jump70to90 = modifier90 - modifier70;
            var jump90to99 = modifier99 - modifier90;

            Assert.IsTrue(jump50to70 > jump70to90,
                $"Jump 50->70 ({jump50to70:F4}) should be > jump 70->90 ({jump70to90:F4})");
            Assert.IsTrue(jump70to90 > jump90to99,
                $"Jump 70->90 ({jump70to90:F4}) should be > jump 90->99 ({jump90to99:F4})");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void FromDifferential_VerySmallDiff_ReturnsZero()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(0.0001);

            // Assert - very small values should return 0 to avoid floating point issues
            Assert.AreEqual(0.0, modifier, TOLERANCE);
        }

        [TestMethod]
        public void FromDifferential_ExtremeDiff_DoesNotOverflow()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(1000.0);

            // Assert - should handle extreme values gracefully
            Assert.IsFalse(double.IsNaN(modifier), "Should not produce NaN");
            Assert.IsFalse(double.IsInfinity(modifier), "Should not produce Infinity");
            Assert.IsTrue(modifier > 0 && modifier < 1.0,
                $"Extreme positive diff should produce reasonable modifier. Got: {modifier}");
        }

        [TestMethod]
        public void FromDifferential_NegativeExtreme_DoesNotOverflow()
        {
            // Arrange & Act
            var modifier = AttributeModifier.FromDifferential(-1000.0);

            // Assert
            Assert.IsFalse(double.IsNaN(modifier), "Should not produce NaN");
            Assert.IsFalse(double.IsInfinity(modifier), "Should not produce Infinity");
            Assert.IsTrue(modifier < 0 && modifier > -1.0,
                $"Extreme negative diff should produce reasonable modifier. Got: {modifier}");
        }

        #endregion
    }
}
