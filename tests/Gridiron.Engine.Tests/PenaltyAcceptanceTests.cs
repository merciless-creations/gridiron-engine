using Gridiron.Engine.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gridiron.Engine.Simulation.Decision;
using Gridiron.Engine.Tests.Helpers;

namespace Gridiron.Engine.Tests
{
    /// <summary>
    /// Tests for smart penalty acceptance logic.
    /// Verifies that PenaltyDecisionEngine.Decide() makes correct decisions
    /// based on game situation, penalty type, and play outcome.
    /// </summary>
    [TestClass]
    public class PenaltyAcceptanceTests
    {
        private readonly TestGame _testGame = new TestGame();
        private PenaltyDecisionEngine? _decisionEngine;

        [TestInitialize]
        public void Setup()
        {
            _decisionEngine = new PenaltyDecisionEngine();
        }

        /// <summary>
        /// Helper method to create a PenaltyDecisionContext for testing.
        /// </summary>
        private PenaltyDecisionContext CreateContext(
            PenaltyNames penaltyName,
            int penaltyYards,
            Possession penalizedTeam,
            Possession offense,
            int yardsGainedOnPlay,
            Downs currentDown,
            int yardsToGo,
            int fieldPosition = 50,
            bool playResultedInTouchdown = false,
            bool playResultedInTurnover = false)
        {
            return new PenaltyDecisionContext(
                penaltyName: penaltyName,
                penaltyYards: penaltyYards,
                penalizedTeam: penalizedTeam,
                occurredWhen: PenaltyOccuredWhen.During,
                offense: offense,
                yardsGainedOnPlay: yardsGainedOnPlay,
                playResultedInFirstDown: yardsGainedOnPlay >= yardsToGo,
                playResultedInTurnover: playResultedInTurnover,
                playResultedInTouchdown: playResultedInTouchdown,
                currentDown: currentDown,
                yardsToGo: yardsToGo,
                fieldPosition: fieldPosition,
                scoreDifferential: 0,
                timeRemainingSeconds: 3600
            );
        }

        #region Defensive Penalties - Automatic First Down

        [TestMethod]
        public void DefensivePenalty_AutomaticFirstDown_AlwaysAccepted()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensiveHolding,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 8, // Would be 2nd and 2
                currentDown: Downs.First,
                yardsToGo: 10);

            // Act - Even with a long gain on the play, should accept for automatic 1st
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Should always accept automatic first down penalties");
        }

        [TestMethod]
        public void DefensivePenalty_RoughingThePasser_AlwaysAccepted()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.RoughingthePasser,
                penaltyYards: 15,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 20, // 20-yard completion
                currentDown: Downs.First,
                yardsToGo: 10);

            // Act - Even on a completed pass, roughing the passer is automatic 1st
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Roughing the passer gives automatic first down");
        }

        [TestMethod]
        public void DefensivePenalty_PassInterference_AlwaysAccepted()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensivePassInterference,
                penaltyYards: 25, // Spot foul
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 0, // Incomplete pass
                currentDown: Downs.Third,
                yardsToGo: 10);

            // Act
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "DPI gives automatic first down and spot foul yards");
        }

        #endregion

        #region Defensive Penalties - No Automatic First Down

        [TestMethod]
        public void DefensivePenalty_Offsides_AcceptedWhenBetterThanPlayResult()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensiveOffside,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 2,
                currentDown: Downs.Second,
                yardsToGo: 8);

            // Act - Play gained 2 yards, penalty gives 5 yards
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Should accept offsides when penalty yards > play yards");
        }

        [TestMethod]
        public void DefensivePenalty_Offsides_DeclinedWhenPlayBetter()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensiveOffside,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 20, // Big gain
                currentDown: Downs.First,
                yardsToGo: 10);

            // Act - Play gained 20 yards (first down), penalty only gives 5 yards
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Decline, decision, "Should decline offsides when play result was better");
        }

        [TestMethod]
        public void DefensivePenalty_Encroachment_DeclinedOnTouchdown()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.Encroachment,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 50, // Touchdown
                currentDown: Downs.First,
                yardsToGo: 10,
                playResultedInTouchdown: true);

            // Act - Offense scored TD, penalty only gives 5 yards
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Decline, decision, "Should decline when offense scored touchdown");
        }

        #endregion

        #region Offensive Penalties - Defense Decides

        [TestMethod]
        public void OffensivePenalty_Holding_AcceptedOnShortGain()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 3,
                currentDown: Downs.Third,
                yardsToGo: 10);

            // Act - Offense gained 3 yards on 3rd and 10
            // Accept: 3rd and 20 (gives offense another chance)
            // Decline: 4th and 7 (forces punt)
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Defense should accept holding to push offense back");
        }

        [TestMethod]
        public void OffensivePenalty_Holding_AcceptedOn3rdDown()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 2,
                currentDown: Downs.Third,
                yardsToGo: 10);

            // Act - Offense gained 2 yards on 3rd and 10
            // Accept: 3rd and 20 (makes it harder for offense, keeps them on 3rd)
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Defense should accept to push offense back on 3rd down");
        }

        [TestMethod]
        public void OffensivePenalty_FalseStart_AcceptedOnFirstDown()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.FalseStart,
                penaltyYards: 5,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 0, // No play occurred
                currentDown: Downs.First,
                yardsToGo: 10);

            // Act - 1st and 10 becomes 1st and 15
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Defense should accept to make it harder for offense");
        }

        [TestMethod]
        public void OffensivePenalty_IntentionalGrounding_AcceptedWithLossOfDown()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.IntentionalGrounding,
                penaltyYards: 10, // From spot of foul
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 0,
                currentDown: Downs.Second,
                yardsToGo: 10);

            // Act - 2nd and 10, intentional grounding
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Intentional grounding includes loss of down");
        }

        #endregion

        #region Edge Cases and Situational Logic

        [TestMethod]
        public void DefensivePenalty_ThirdDownShortYardage_AcceptForFirstDown()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensiveHolding,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 0, // Incomplete
                currentDown: Downs.Third,
                yardsToGo: 2);

            // Act - 3rd and 2, incomplete pass, holding penalty gives automatic 1st
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Should accept for automatic first down");
        }

        [TestMethod]
        public void OffensivePenalty_FourthDownSack_DeclineToGetBall()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: -3, // Sack
                currentDown: Downs.Fourth,
                yardsToGo: 5);

            // Act - 4th down sack with holding
            // Decline: Get ball on turnover on downs
            // Accept: Gives offense another 4th down try
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Decline, decision, "Defense should decline to get turnover on downs");
        }

        [TestMethod]
        public void DefensivePenalty_RunningIntoKicker_NoAutomaticFirstDown_CompareYards()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.RunningIntotheKicker,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 40, // Good punt
                currentDown: Downs.Fourth,
                yardsToGo: 5);

            // Act - Punting team (offense for penalty purposes), 4th down punt
            // Running into the kicker does NOT give automatic first down
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Decline, decision, "Punt was successful, better than 5-yard penalty");
        }

        [TestMethod]
        public void DefensivePenalty_RoughingTheKicker_AutomaticFirstDown_AlwaysAccept()
        {
            // Arrange
            // Note: For punts, playResultedInFirstDown should be false since punts don't give first downs.
            // We manually create context to set this correctly.
            var context = new PenaltyDecisionContext(
                penaltyName: PenaltyNames.RoughingtheKicker,
                penaltyYards: 15,
                penalizedTeam: Possession.Away,
                occurredWhen: PenaltyOccuredWhen.During,
                offense: Possession.Home,
                yardsGainedOnPlay: 40, // Even with good punt
                playResultedInFirstDown: false, // Punts don't give first downs
                playResultedInTurnover: false,
                playResultedInTouchdown: false,
                currentDown: Downs.Fourth,
                yardsToGo: 5,
                fieldPosition: 50,
                scoreDifferential: 0,
                timeRemainingSeconds: 3600
            );

            // Act - Roughing the kicker DOES give automatic first down
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Roughing gives automatic first down - keep possession");
        }

        [TestMethod]
        public void OffensivePenalty_OnBigGain_StillAccepted()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 30,
                currentDown: Downs.Second,
                yardsToGo: 10);

            // Act - 30-yard gain on 2nd and 10 with holding
            // Accept: 2nd and 20 (still 2nd down)
            // Decline: 1st and 10 (first down conversion)
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Defense should accept to negate the big gain");
        }

        [TestMethod]
        public void OffensivePenalty_NegativeYards_StillAccepted()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: -5,
                currentDown: Downs.First,
                yardsToGo: 10);

            // Act - Sack for -5 yards with holding
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Defense should accept to push offense further back");
        }

        #endregion

        #region Multiple Down and Distance Scenarios

        [TestMethod]
        public void AcceptanceLogic_SecondAndLong_OffensiveHolding()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.OffensiveHolding,
                penaltyYards: 10,
                penalizedTeam: Possession.Home,
                offense: Possession.Home,
                yardsGainedOnPlay: 0,
                currentDown: Downs.Second,
                yardsToGo: 15);

            // Act - 2nd and 15, incomplete pass with holding
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Makes it 2nd and 25");
        }

        [TestMethod]
        public void AcceptanceLogic_ThirdAndGoal_DefensiveHolding()
        {
            // Arrange
            var context = CreateContext(
                penaltyName: PenaltyNames.DefensiveHolding,
                penaltyYards: 5,
                penalizedTeam: Possession.Away,
                offense: Possession.Home,
                yardsGainedOnPlay: 0,
                currentDown: Downs.Third,
                yardsToGo: 5,
                fieldPosition: 95); // 5 yards from goal

            // Act - 3rd and goal from 5, incomplete pass
            var decision = _decisionEngine!.Decide(context);

            // Assert
            Assert.AreEqual(PenaltyDecision.Accept, decision, "Automatic first down at the 2-3 yard line (half distance)");
        }

        #endregion
    }
}
