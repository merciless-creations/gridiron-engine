using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Plays;
using Gridiron.Engine.Simulation.PlayResults;
using Gridiron.Engine.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests;

/// <summary>
/// Tests for Spike and Kneel play types.
/// Spike is a PassPlay with IsSpike=true.
/// Kneel is a RunPlay with IsKneel=true.
/// These plays are used for clock management in late-game situations.
/// </summary>
[TestClass]
public class SpikeAndKneelPlayTests
{
    private TestGame? _testGame;

    [TestInitialize]
    public void Setup()
    {
        _testGame = new TestGame();
    }

    #region Spike Play Tests

    [TestMethod]
    public void SpikePlay_HasCorrectPlayType()
    {
        var play = new PassPlay { IsSpike = true };
        Assert.AreEqual(PlayType.Spike, play.PlayType);
    }

    [TestMethod]
    public void RegularPassPlay_HasPassPlayType()
    {
        var play = new PassPlay { IsSpike = false };
        Assert.AreEqual(PlayType.Pass, play.PlayType);
    }

    [TestMethod]
    public void Pass_Execute_Spike_SetsYardsGainedToZero()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        var play = (PassPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var pass = new Pass(rng);

        // Act
        pass.Execute(game);

        // Assert
        Assert.AreEqual(0, play.YardsGained);
    }

    [TestMethod]
    public void Pass_Execute_Spike_StopsClock()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        var play = (PassPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var pass = new Pass(rng);

        // Act
        pass.Execute(game);

        // Assert
        Assert.IsTrue(play.ClockStopped);
    }

    [TestMethod]
    public void Pass_Execute_Spike_Sets3SecondElapsedTime()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        var play = (PassPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var pass = new Pass(rng);

        // Act
        pass.Execute(game);

        // Assert
        Assert.AreEqual(3.0, play.ElapsedTime);
    }

    [TestMethod]
    public void Pass_Execute_Spike_EndFieldPositionEqualToStart()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        game.FieldPosition = 50;
        var play = (PassPlay)game.CurrentPlay!;
        play.StartFieldPosition = 50;

        var rng = new TestFluentSeedableRandom();
        var pass = new Pass(rng);

        // Act
        pass.Execute(game);

        // Assert
        Assert.AreEqual(50, play.EndFieldPosition);
    }

    [TestMethod]
    public void Pass_Execute_Spike_CreatesIncompletePassSegment()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        var play = (PassPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var pass = new Pass(rng);

        // Act
        pass.Execute(game);

        // Assert
        Assert.AreEqual(1, play.PassSegments.Count);
        Assert.IsFalse(play.PassSegments[0].IsComplete);
    }

    [TestMethod]
    public void PassResult_Execute_Spike_AdvancesDown()
    {
        // Arrange
        var game = CreateGameWithSpikePlay();
        game.CurrentDown = Downs.Second;
        game.FieldPosition = 50;
        game.YardsToGo = 10;
        var play = (PassPlay)game.CurrentPlay!;
        play.YardsGained = 0;

        var passResult = new PassResult();

        // Act
        passResult.Execute(game);

        // Assert
        Assert.AreEqual(Downs.Third, game.CurrentDown);
    }

    #endregion

    #region Kneel Play Tests

    [TestMethod]
    public void KneelPlay_HasCorrectPlayType()
    {
        var play = new RunPlay { IsKneel = true };
        Assert.AreEqual(PlayType.Kneel, play.PlayType);
    }

    [TestMethod]
    public void RegularRunPlay_HasRunPlayType()
    {
        var play = new RunPlay { IsKneel = false };
        Assert.AreEqual(PlayType.Run, play.PlayType);
    }

    [TestMethod]
    public void Run_Execute_Kneel_SetsYardsGainedToMinusOne()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        var play = (RunPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.AreEqual(-1, play.YardsGained);
    }

    [TestMethod]
    public void Run_Execute_Kneel_ClockKeepsRunning()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        var play = (RunPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.IsFalse(play.ClockStopped);
    }

    [TestMethod]
    public void Run_Execute_Kneel_Sets40SecondElapsedTime()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        var play = (RunPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.AreEqual(40.0, play.ElapsedTime);
    }

    [TestMethod]
    public void Run_Execute_Kneel_EndFieldPositionIsStartMinusOne()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        game.FieldPosition = 50;
        var play = (RunPlay)game.CurrentPlay!;
        play.StartFieldPosition = 50;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.AreEqual(49, play.EndFieldPosition);
    }

    [TestMethod]
    public void Run_Execute_Kneel_CreatesRunSegment()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        var play = (RunPlay)game.CurrentPlay!;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.AreEqual(1, play.RunSegments.Count);
        Assert.AreEqual(-1, play.RunSegments[0].YardsGained);
    }

    [TestMethod]
    public void Run_Execute_Kneel_AtOneYardLine_TriggersSafety()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        game.FieldPosition = 1;
        var play = (RunPlay)game.CurrentPlay!;
        play.StartFieldPosition = 1;

        var rng = new TestFluentSeedableRandom();
        var run = new Run(rng);

        // Act
        run.Execute(game);

        // Assert
        Assert.IsTrue(play.IsSafety);
    }

    [TestMethod]
    public void RunResult_Execute_Kneel_AdvancesDown()
    {
        // Arrange
        var game = CreateGameWithKneelPlay();
        game.CurrentDown = Downs.First;
        game.FieldPosition = 50;
        game.YardsToGo = 10;
        var play = (RunPlay)game.CurrentPlay!;
        play.YardsGained = -1;

        var runResult = new RunResult();

        // Act
        runResult.Execute(game);

        // Assert
        Assert.AreEqual(Downs.Second, game.CurrentDown);
    }

    #endregion

    #region PlayCallDecision Enum Tests

    [TestMethod]
    public void PlayCallDecision_ContainsSpike()
    {
        var decision = Simulation.Decision.PlayCallDecision.Spike;
        Assert.AreEqual(Simulation.Decision.PlayCallDecision.Spike, decision);
    }

    [TestMethod]
    public void PlayCallDecision_ContainsKneel()
    {
        var decision = Simulation.Decision.PlayCallDecision.Kneel;
        Assert.AreEqual(Simulation.Decision.PlayCallDecision.Kneel, decision);
    }

    #endregion

    #region Helper Methods

    private Game CreateGameWithSpikePlay()
    {
        var game = _testGame!.GetGame();

        var spikePlay = new PassPlay
        {
            IsSpike = true,
            Possession = Possession.Home,
            Down = Downs.Second,
            StartFieldPosition = 50,
            Result = NullLogger.Instance,
            OffensePlayersOnField = new List<Player>(),
            DefensePlayersOnField = new List<Player>()
        };

        // Add QB for spike
        spikePlay.OffensePlayersOnField.Add(new Player
        {
            Position = Positions.QB,
            LastName = "Quarterback",
            Speed = 60,
            Strength = 50
        });

        game.CurrentPlay = spikePlay;
        game.FieldPosition = 50;
        game.YardsToGo = 10;
        game.CurrentDown = Downs.Second;

        return game;
    }

    private Game CreateGameWithKneelPlay()
    {
        var game = _testGame!.GetGame();

        var kneelPlay = new RunPlay
        {
            IsKneel = true,
            Possession = Possession.Home,
            Down = Downs.First,
            StartFieldPosition = 50,
            Result = NullLogger.Instance,
            OffensePlayersOnField = new List<Player>(),
            DefensePlayersOnField = new List<Player>()
        };

        // Add QB for kneel
        kneelPlay.OffensePlayersOnField.Add(new Player
        {
            Position = Positions.QB,
            LastName = "Quarterback",
            Speed = 60,
            Strength = 50,
            Agility = 55
        });

        game.CurrentPlay = kneelPlay;
        game.FieldPosition = 50;
        game.YardsToGo = 10;
        game.CurrentDown = Downs.First;

        return game;
    }

    #endregion
}
