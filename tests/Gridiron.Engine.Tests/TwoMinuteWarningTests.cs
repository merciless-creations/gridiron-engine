using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Rules.TwoMinuteWarning;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests;

/// <summary>
/// Tests for two-minute warning functionality.
/// Verifies that NFL two-minute warning is called at 2:00 in Q2 and Q4,
/// and that NCAA games have no two-minute warning.
/// </summary>
[TestClass]
public class TwoMinuteWarningTests
{
    #region NFL Two-Minute Warning Tests

    [TestMethod]
    public void TwoMinuteWarning_NFL_CalledInSecondQuarter()
    {
        // Arrange - simulate full game with NFL rules
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 12345,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Nfl
        };

        // Act
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Check Q2 quarter state
        var q2 = game.Halves[0].Quarters[1]; // Second quarter
        Assert.IsTrue(q2.TwoMinuteWarningCalled, "Two-minute warning should be called in Q2");
    }

    [TestMethod]
    public void TwoMinuteWarning_NFL_CalledInFourthQuarter()
    {
        // Arrange
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 67890,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Nfl
        };

        // Act
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Check Q4 quarter state
        var q4 = game.Halves[1].Quarters[1]; // Fourth quarter
        Assert.IsTrue(q4.TwoMinuteWarningCalled, "Two-minute warning should be called in Q4");
    }

    [TestMethod]
    public void TwoMinuteWarning_NFL_NotCalledInFirstQuarter()
    {
        // Arrange
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 11111,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Nfl
        };

        // Act
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Check Q1 quarter state
        var q1 = game.Halves[0].Quarters[0]; // First quarter
        Assert.IsFalse(q1.TwoMinuteWarningCalled, "Two-minute warning should NOT be called in Q1");
    }

    [TestMethod]
    public void TwoMinuteWarning_NFL_NotCalledInThirdQuarter()
    {
        // Arrange
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 22222,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Nfl
        };

        // Act
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Check Q3 quarter state
        var q3 = game.Halves[1].Quarters[0]; // Third quarter
        Assert.IsFalse(q3.TwoMinuteWarningCalled, "Two-minute warning should NOT be called in Q3");
    }

    #endregion

    #region NCAA No Two-Minute Warning Tests

    [TestMethod]
    public void TwoMinuteWarning_NCAA_NeverCalled()
    {
        // Arrange
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 99999,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Ncaa
        };

        // Act
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Check all quarters
        Assert.IsFalse(game.Halves[0].Quarters[0].TwoMinuteWarningCalled, "Q1 should not have two-minute warning (NCAA)");
        Assert.IsFalse(game.Halves[0].Quarters[1].TwoMinuteWarningCalled, "Q2 should not have two-minute warning (NCAA)");
        Assert.IsFalse(game.Halves[1].Quarters[0].TwoMinuteWarningCalled, "Q3 should not have two-minute warning (NCAA)");
        Assert.IsFalse(game.Halves[1].Quarters[1].TwoMinuteWarningCalled, "Q4 should not have two-minute warning (NCAA)");
    }

    #endregion

    #region Provider Unit Tests

    [TestMethod]
    public void NflTwoMinuteWarningProvider_ShouldCallInSecondQuarter()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - simulate crossing 2:00 threshold (before: 125, after: 115)
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second,
            timeBeforePlay: 125,
            timeAfterPlay: 115,
            alreadyCalled: false);

        // Assert
        Assert.IsTrue(result, "NFL should call two-minute warning in Q2");
    }

    [TestMethod]
    public void NflTwoMinuteWarningProvider_ShouldCallInFourthQuarter()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - simulate crossing 2:00 threshold
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Fourth,
            timeBeforePlay: 125,
            timeAfterPlay: 115,
            alreadyCalled: false);

        // Assert
        Assert.IsTrue(result, "NFL should call two-minute warning in Q4");
    }

    [TestMethod]
    public void NflTwoMinuteWarningProvider_ShouldNotCallInFirstQuarter()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.First,
            timeBeforePlay: 125,
            timeAfterPlay: 115,
            alreadyCalled: false);

        // Assert
        Assert.IsFalse(result, "NFL should NOT call two-minute warning in Q1");
    }

    [TestMethod]
    public void NflTwoMinuteWarningProvider_ShouldNotCallIfAlreadyCalled()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - simulate second attempt to call warning
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second,
            timeBeforePlay: 125,
            timeAfterPlay: 115,
            alreadyCalled: true);

        // Assert
        Assert.IsFalse(result, "NFL should NOT call two-minute warning twice in same quarter");
    }

    [TestMethod]
    public void NflTwoMinuteWarningProvider_ShouldNotCallIfNotCrossing120()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - time went from 130 to 125 (didn't cross 120)
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second,
            timeBeforePlay: 130,
            timeAfterPlay: 125,
            alreadyCalled: false);

        // Assert
        Assert.IsFalse(result, "Should NOT call if didn't cross 120-second threshold");
    }

    [TestMethod]
    public void NcaaTwoMinuteWarningProvider_NeverCalls()
    {
        // Arrange
        var provider = new NcaaTwoMinuteWarningRulesProvider();

        // Act - try all scenarios
        var q2Result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second, 125, 115, false);
        var q4Result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Fourth, 125, 115, false);

        // Assert
        Assert.IsFalse(q2Result, "NCAA should never call two-minute warning (Q2)");
        Assert.IsFalse(q4Result, "NCAA should never call two-minute warning (Q4)");
    }

    [TestMethod]
    public void TwoMinuteWarningRegistry_DefaultIsNfl()
    {
        // Assert
        Assert.IsInstanceOfType(TwoMinuteWarningRulesRegistry.Default, typeof(NflTwoMinuteWarningRulesProvider));
    }

    [TestMethod]
    public void TwoMinuteWarningRegistry_GetByName_ReturnsCorrectProvider()
    {
        // Act
        var nflProvider = TwoMinuteWarningRulesRegistry.GetByName("NFL");
        var ncaaProvider = TwoMinuteWarningRulesRegistry.GetByName("NCAA");

        // Assert
        Assert.IsInstanceOfType(nflProvider, typeof(NflTwoMinuteWarningRulesProvider));
        Assert.IsInstanceOfType(ncaaProvider, typeof(NcaaTwoMinuteWarningRulesProvider));
    }

    #endregion
}
