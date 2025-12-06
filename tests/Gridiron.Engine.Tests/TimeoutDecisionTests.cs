using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Configuration;
using Gridiron.Engine.Simulation.Decision;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    [TestClass]
    public class TimeoutDecisionTests
    {
        #region Ice the Kicker Tests

        [TestMethod]
        public void IceKicker_LongFieldGoal_DefenseMayCallTimeout()
        {
            // Arrange - Defense facing a 50-yard FG attempt
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Away,
                isOffense: false,
                timeoutsRemaining: 3,
                scoreDifferential: 0,
                timeRemainingInHalfSeconds: 600,
                timeRemainingInGameSeconds: 1800,
                isClockRunning: false,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.FieldGoal,
                fieldGoalDistance: 50
            );

            // Act - Run multiple times to check probability
            int iceCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == TimeoutDecision.IceKicker)
                    iceCount++;
            }

            // Assert - Should ice roughly 30% of the time (with variance)
            Assert.IsTrue(iceCount >= 15 && iceCount <= 50,
                $"Expected ~30% ice the kicker attempts, got {iceCount}%");
        }

        [TestMethod]
        public void IceKicker_ShortFieldGoal_ShouldNotIce()
        {
            // Arrange - Defense facing a 30-yard FG (too short to ice)
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Away,
                isOffense: false,
                timeoutsRemaining: 3,
                scoreDifferential: 0,
                timeRemainingInHalfSeconds: 600,
                timeRemainingInGameSeconds: 1800,
                isClockRunning: false,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.FieldGoal,
                fieldGoalDistance: 30
            );

            // Act
            int iceCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == TimeoutDecision.IceKicker)
                    iceCount++;
            }

            // Assert - Should never ice on a short FG
            Assert.AreEqual(0, iceCount, "Should not ice the kicker on short field goals");
        }

        [TestMethod]
        public void IceKicker_NotFieldGoal_ShouldNotIce()
        {
            // Arrange - Defense facing a run play
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Away,
                isOffense: false,
                timeoutsRemaining: 3,
                scoreDifferential: 0,
                timeRemainingInHalfSeconds: 600,
                timeRemainingInGameSeconds: 1800,
                isClockRunning: false,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.Run,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert
            Assert.AreEqual(TimeoutDecision.None, decision);
        }

        #endregion

        #region Stop the Clock Tests

        [TestMethod]
        public void StopClock_TrailingLateClockRunning_ShouldCallTimeout()
        {
            // Arrange - Trailing by 7, 90 seconds left, clock running
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 2,
                scoreDifferential: -7,
                timeRemainingInHalfSeconds: 90,
                timeRemainingInGameSeconds: 90,
                isClockRunning: true,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PostPlay,
                upcomingPlayType: null,
                fieldGoalDistance: null
            );

            // Act
            int stopClockCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == TimeoutDecision.StopClock)
                    stopClockCount++;
            }

            // Assert - Should stop clock ~85% of the time
            Assert.IsTrue(stopClockCount >= 70,
                $"Expected at least 70% stop clock decisions when trailing late, got {stopClockCount}%");
        }

        [TestMethod]
        public void StopClock_LeadingLate_ShouldNotCallTimeout()
        {
            // Arrange - Leading by 7, 90 seconds left
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 2,
                scoreDifferential: 7, // Leading
                timeRemainingInHalfSeconds: 90,
                timeRemainingInGameSeconds: 90,
                isClockRunning: true,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PostPlay,
                upcomingPlayType: null,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert - Should not stop clock when leading
            Assert.AreEqual(TimeoutDecision.None, decision);
        }

        [TestMethod]
        public void StopClock_ClockAlreadyStopped_ShouldNotCallTimeout()
        {
            // Arrange - Trailing, but clock is already stopped
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 2,
                scoreDifferential: -7,
                timeRemainingInHalfSeconds: 90,
                timeRemainingInGameSeconds: 90,
                isClockRunning: false, // Clock already stopped
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PostPlay,
                upcomingPlayType: null,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert - No point calling timeout if clock is stopped
            Assert.AreEqual(TimeoutDecision.None, decision);
        }

        [TestMethod]
        public void StopClock_PlentyOfTime_ShouldNotCallTimeout()
        {
            // Arrange - Trailing, but plenty of time left
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 2,
                scoreDifferential: -7,
                timeRemainingInHalfSeconds: 600, // 10 minutes left
                timeRemainingInGameSeconds: 1800,
                isClockRunning: true,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PostPlay,
                upcomingPlayType: null,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert - No urgency to stop clock
            Assert.AreEqual(TimeoutDecision.None, decision);
        }

        #endregion

        #region Avoid Delay of Game Tests

        [TestMethod]
        public void AvoidDelay_PlayClockLow_ShouldCallTimeout()
        {
            // Arrange - Play clock at 2 seconds
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 3,
                scoreDifferential: 0,
                timeRemainingInHalfSeconds: 600,
                timeRemainingInGameSeconds: 1800,
                isClockRunning: true,
                playClockSeconds: 2, // About to expire
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.Run,
                fieldGoalDistance: null
            );

            // Act
            int avoidDelayCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var decision = engine.Decide(context);
                if (decision == TimeoutDecision.AvoidDelayOfGame)
                    avoidDelayCount++;
            }

            // Assert - Should call timeout ~90% of the time
            Assert.IsTrue(avoidDelayCount >= 75,
                $"Expected at least 75% avoid delay decisions, got {avoidDelayCount}%");
        }

        [TestMethod]
        public void AvoidDelay_PlayClockFine_ShouldNotCallTimeout()
        {
            // Arrange - Play clock at 15 seconds (plenty of time)
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 3,
                scoreDifferential: 0,
                timeRemainingInHalfSeconds: 600,
                timeRemainingInGameSeconds: 1800,
                isClockRunning: true,
                playClockSeconds: 15,
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.Run,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert
            Assert.AreEqual(TimeoutDecision.None, decision);
        }

        #endregion

        #region No Timeouts Remaining Tests

        [TestMethod]
        public void NoTimeoutsRemaining_CannotCallTimeout()
        {
            // Arrange - No timeouts left, but in a situation where we'd want one
            var rng = new SeedableRandom(42);
            var engine = new TimeoutDecisionEngine(rng);
            var context = new TimeoutContext(
                team: Possession.Home,
                isOffense: true,
                timeoutsRemaining: 0, // No timeouts
                scoreDifferential: -7,
                timeRemainingInHalfSeconds: 90,
                timeRemainingInGameSeconds: 90,
                isClockRunning: true,
                playClockSeconds: 25,
                timingPhase: TimeoutTimingPhase.PostPlay,
                upcomingPlayType: null,
                fieldGoalDistance: null
            );

            // Act
            var decision = engine.Decide(context);

            // Assert
            Assert.AreEqual(TimeoutDecision.None, decision,
                "Should not be able to call timeout with none remaining");
        }

        #endregion

        #region Context Struct Tests

        [TestMethod]
        public void TimeoutContext_StoresValuesCorrectly()
        {
            // Arrange & Act
            var context = new TimeoutContext(
                team: Possession.Away,
                isOffense: false,
                timeoutsRemaining: 2,
                scoreDifferential: -3,
                timeRemainingInHalfSeconds: 120,
                timeRemainingInGameSeconds: 120,
                isClockRunning: true,
                playClockSeconds: 10,
                timingPhase: TimeoutTimingPhase.PrePlay,
                upcomingPlayType: PlayType.FieldGoal,
                fieldGoalDistance: 48
            );

            // Assert
            Assert.AreEqual(Possession.Away, context.Team);
            Assert.IsFalse(context.IsOffense);
            Assert.AreEqual(2, context.TimeoutsRemaining);
            Assert.AreEqual(-3, context.ScoreDifferential);
            Assert.AreEqual(120, context.TimeRemainingInHalfSeconds);
            Assert.AreEqual(120, context.TimeRemainingInGameSeconds);
            Assert.IsTrue(context.IsClockRunning);
            Assert.AreEqual(10, context.PlayClockSeconds);
            Assert.AreEqual(TimeoutTimingPhase.PrePlay, context.TimingPhase);
            Assert.AreEqual(PlayType.FieldGoal, context.UpcomingPlayType);
            Assert.AreEqual(48, context.FieldGoalDistance);
        }

        #endregion
    }
}
