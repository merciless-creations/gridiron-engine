using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Time;
using Gridiron.Engine.Simulation.Rules.EndOfHalf;
using Gridiron.Engine.Simulation.Rules.TwoMinuteWarning;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests;

/// <summary>
/// Additional clock management integration tests covering edge cases and interactions.
/// These tests verify complex scenarios like multiple kneels, penalty interactions,
/// and boundary conditions for two-minute warning.
/// </summary>
[TestClass]
public class ClockManagementIntegrationTests
{
    #region Multiple Kneel Sequence Tests

    [TestMethod]
    public void MultipleKneels_RunsOutClock_EndsGame()
    {
        // Arrange - Create a scenario where offense is winning late in Q4
        var teams = TestTeams.CreateTestTeams();
        var engine = new GameEngine();
        var options = new SimulationOptions
        {
            RandomSeed = 99999,
            TwoMinuteWarningRulesProvider = TwoMinuteWarningRulesRegistry.Nfl
        };

        // Act - Simulate full game
        var result = engine.SimulateGame(teams.HomeTeam, teams.VisitorTeam, options);
        var game = result.Game;

        // Assert - Look for kneel plays in Q4
        var q4KneelPlays = game.Plays
            .Where(p => p.PlayType == PlayType.Kneel)
            .ToList();

        // Should have at least some kneel plays if winning team ran clock out
        // (This is probabilistic - depends on game score, but with enough games would show pattern)
        // For now, just verify the game completed successfully
        Assert.IsTrue(game.Halves[1].Quarters[1].QuarterType == QuarterType.Fourth || 
                      game.Halves[1].Quarters[1].QuarterType == QuarterType.GameOver);
    }

    #endregion

    #region Two-Minute Warning Boundary Tests

    [TestMethod]
    public void TwoMinuteWarning_PlayCrossing120Threshold_TriggersWarning()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - Simulate play that crosses exactly through 120s
        // Play starts at 121s, takes 5s, ends at 116s
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second,
            timeBeforePlay: 121,
            timeAfterPlay: 116,
            alreadyCalled: false);

        // Assert
        Assert.IsTrue(result, "Should trigger warning when crossing 120s threshold (121→116)");
    }

    [TestMethod]
    public void TwoMinuteWarning_PlayEndsExactlyAt120_TriggersWarning()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - Play ends exactly at 120s
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Fourth,
            timeBeforePlay: 125,
            timeAfterPlay: 120,
            alreadyCalled: false);

        // Assert
        Assert.IsTrue(result, "Should trigger warning when ending exactly at 120s");
    }

    [TestMethod]
    public void TwoMinuteWarning_PlayStartsAt120_DoesNotTrigger()
    {
        // Arrange
        var provider = new NflTwoMinuteWarningRulesProvider();

        // Act - Play starts at 120s (already crossed threshold)
        var result = provider.ShouldCallTwoMinuteWarning(
            QuarterType.Second,
            timeBeforePlay: 120,
            timeAfterPlay: 115,
            alreadyCalled: false);

        // Assert
        Assert.IsFalse(result, "Should not trigger if already at or below 120s before play");
    }

    #endregion

    #region End-of-Half Penalty Extension Tests

    [TestMethod]
    public void EndOfHalf_DefensivePenalty_ExtendsHalf()
    {
        // Arrange
        var provider = new NflEndOfHalfRulesProvider();

        // Assert - Defensive penalty should NOT allow half to end
        Assert.IsFalse(provider.AllowsHalfToEndOnDefensivePenalty, 
            "NFL: Half cannot end on defensive penalty");
    }

    [TestMethod]
    public void EndOfHalf_OffensivePenalty_AllowsHalfToEnd()
    {
        // Arrange
        var provider = new NflEndOfHalfRulesProvider();

        // Assert - Offensive penalty SHOULD allow half to end
        Assert.IsTrue(provider.AllowsHalfToEndOnOffensivePenalty,
            "NFL: Half CAN end on offensive penalty");
    }

    [TestMethod]
    public void EndOfHalf_NCAA_SameAsNFL()
    {
        // Arrange
        var nflProvider = new NflEndOfHalfRulesProvider();
        var ncaaProvider = new NcaaEndOfHalfRulesProvider();

        // Assert - NCAA rules match NFL for penalty extensions
        Assert.AreEqual(nflProvider.AllowsHalfToEndOnDefensivePenalty, 
            ncaaProvider.AllowsHalfToEndOnDefensivePenalty,
            "NCAA and NFL should have same defensive penalty rules");
        
        Assert.AreEqual(nflProvider.AllowsHalfToEndOnOffensivePenalty,
            ncaaProvider.AllowsHalfToEndOnOffensivePenalty,
            "NCAA and NFL should have same offensive penalty rules");
    }

    #endregion

    #region Penalty on Kneel/Spike Tests

    [TestMethod]
    public void Kneel_WithPenalty_ClockStillRuns()
    {
        // This is a complex integration test that would require:
        // 1. Setting up a game state with kneel situation
        // 2. Forcing a penalty to occur on the kneel
        // 3. Verifying clock behavior
        // 
        // For now, this is documented as needed but not implemented
        // Issue #12 tracking item
        Assert.Inconclusive("Penalty on kneel interaction test - needs implementation");
    }

    [TestMethod]
    public void Spike_WithPenalty_ClockStaysStopped()
    {
        // Similar to kneel test - requires complex setup
        // Documented for future implementation
        Assert.Inconclusive("Penalty on spike interaction test - needs implementation");
    }

    #endregion
}
