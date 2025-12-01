using Gridiron.Engine.Domain;
using Gridiron.Engine.Domain.Helpers;
using Gridiron.Engine.Simulation.Overtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gridiron.Engine.Tests
{
    [TestClass]
    public class OvertimeTests
    {
        #region NFL Regular Season Provider Tests

        [TestMethod]
        public void NflRegularSeason_FirstPossessionTouchdown_EndsGame()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = false
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.Touchdown, Possession.Home);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.GameOver, result);
        }

        [TestMethod]
        public void NflRegularSeason_FirstPossessionFieldGoal_GameContinues()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = false
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.FieldGoal, Possession.Home);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.Continue, result);
        }

        [TestMethod]
        public void NflRegularSeason_SecondPossessionTouchdownAfterFieldGoal_SecondTeamWins()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = true,
                SecondPossessionComplete = false,
                FirstTeamPeriodScore = 3 // First team kicked FG
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.Touchdown, Possession.Away);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.GameOver, result);
        }

        [TestMethod]
        public void NflRegularSeason_SecondPossessionFieldGoalAfterFieldGoal_GameContinues()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = true,
                SecondPossessionComplete = false,
                FirstTeamPeriodScore = 3 // First team kicked FG
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.FieldGoal, Possession.Away);

            // Assert - game continues to sudden death
            Assert.AreEqual(OvertimeGameEndResult.Continue, result);
        }

        [TestMethod]
        public void NflRegularSeason_SuddenDeath_AnyScoreWins()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = true,
                SecondPossessionComplete = true,
                IsSuddenDeath = true
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.FieldGoal, Possession.Home);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.GameOver, result);
        }

        [TestMethod]
        public void NflRegularSeason_DefensiveTouchdown_AlwaysEndsGame()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = false
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.DefensiveTouchdown, Possession.Away);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.GameOver, result);
        }

        [TestMethod]
        public void NflRegularSeason_AllowsTies()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();

            // Assert
            Assert.IsTrue(rules.AllowsTies);
        }

        [TestMethod]
        public void NflRegularSeason_MaxOnePeriod()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();

            // Assert
            Assert.AreEqual(1, rules.MaxOvertimePeriods);
        }

        [TestMethod]
        public void NflRegularSeason_TenMinutePeriod()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();

            // Assert
            Assert.AreEqual(600, rules.OvertimePeriodDuration);
        }

        [TestMethod]
        public void NflRegularSeason_TwoTimeouts()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();

            // Assert
            Assert.AreEqual(2, rules.TimeoutsPerTeam);
        }

        [TestMethod]
        public void NflRegularSeason_UsesKickoff()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState { CurrentPeriod = 1 };

            // Assert
            Assert.IsTrue(rules.UsesKickoff(state));
        }

        #endregion

        #region NFL Playoff Provider Tests

        [TestMethod]
        public void NflPlayoff_DoesNotAllowTies()
        {
            // Arrange
            var rules = new NflPlayoffOvertimeRulesProvider();

            // Assert
            Assert.IsFalse(rules.AllowsTies);
        }

        [TestMethod]
        public void NflPlayoff_UnlimitedPeriods()
        {
            // Arrange
            var rules = new NflPlayoffOvertimeRulesProvider();

            // Assert
            Assert.AreEqual(0, rules.MaxOvertimePeriods);
        }

        [TestMethod]
        public void NflPlayoff_ShouldStartNewPeriod()
        {
            // Arrange
            var rules = new NflPlayoffOvertimeRulesProvider();
            var state = new OvertimeState { CurrentPeriod = 1 };

            // Assert
            Assert.IsTrue(rules.ShouldStartNewPeriod(state));
        }

        [TestMethod]
        public void NflPlayoff_FirstPossessionTouchdown_EndsGame()
        {
            // Arrange
            var rules = new NflPlayoffOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = false
            };

            // Act
            var result = rules.ShouldGameEnd(state, OvertimeScoreType.Touchdown, Possession.Home);

            // Assert
            Assert.AreEqual(OvertimeGameEndResult.GameOver, result);
        }

        #endregion

        #region Registry Tests

        [TestMethod]
        public void OvertimeRulesRegistry_GetByName_NFL_ReturnsRegularSeason()
        {
            // Act
            var provider = OvertimeRulesRegistry.GetByName("NFL");

            // Assert
            Assert.IsInstanceOfType(provider, typeof(NflRegularSeasonOvertimeRulesProvider));
        }

        [TestMethod]
        public void OvertimeRulesRegistry_GetByName_NFL_PLAYOFF_ReturnsPlayoff()
        {
            // Act
            var provider = OvertimeRulesRegistry.GetByName("NFL_PLAYOFF");

            // Assert
            Assert.IsInstanceOfType(provider, typeof(NflPlayoffOvertimeRulesProvider));
        }

        [TestMethod]
        public void OvertimeRulesRegistry_GetByName_CaseInsensitive()
        {
            // Act
            var provider1 = OvertimeRulesRegistry.GetByName("nfl");
            var provider2 = OvertimeRulesRegistry.GetByName("NFL");
            var provider3 = OvertimeRulesRegistry.GetByName("Nfl");

            // Assert
            Assert.IsNotNull(provider1);
            Assert.IsNotNull(provider2);
            Assert.IsNotNull(provider3);
        }

        [TestMethod]
        public void OvertimeRulesRegistry_Default_ReturnsNflRegularSeason()
        {
            // Act
            var provider = OvertimeRulesRegistry.Default;

            // Assert
            Assert.IsInstanceOfType(provider, typeof(NflRegularSeasonOvertimeRulesProvider));
        }

        [TestMethod]
        public void OvertimeRulesRegistry_GetAll_ReturnsBothProviders()
        {
            // Act
            var providers = OvertimeRulesRegistry.GetAll();

            // Assert
            Assert.AreEqual(2, providers.Count);
        }

        #endregion

        #region OvertimeState Tests

        [TestMethod]
        public void OvertimeState_SecondPossessionTeam_ReturnsOpposite()
        {
            // Arrange
            var state = new OvertimeState
            {
                FirstPossessionTeam = Possession.Home
            };

            // Assert
            Assert.AreEqual(Possession.Away, state.SecondPossessionTeam);
        }

        [TestMethod]
        public void OvertimeState_StartNewPeriod_IncrementsAndResets()
        {
            // Arrange
            var state = new OvertimeState
            {
                CurrentPeriod = 1,
                FirstPossessionTeam = Possession.Home,
                FirstPossessionComplete = true,
                SecondPossessionComplete = true,
                FirstTeamPeriodScore = 3,
                SecondTeamPeriodScore = 3,
                PossessionsInCurrentPeriod = 4
            };

            // Act
            state.StartNewPeriod();

            // Assert
            Assert.AreEqual(2, state.CurrentPeriod);
            Assert.IsFalse(state.FirstPossessionComplete);
            Assert.IsFalse(state.SecondPossessionComplete);
            Assert.AreEqual(0, state.FirstTeamPeriodScore);
            Assert.AreEqual(0, state.SecondTeamPeriodScore);
            Assert.AreEqual(0, state.PossessionsInCurrentPeriod);
            // First possession team should alternate
            Assert.AreEqual(Possession.Away, state.FirstPossessionTeam);
        }

        #endregion

        #region Possession End Reason Tests

        [TestMethod]
        public void NflRegularSeason_FirstPossessionPunt_OtherTeamGetsBall()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = false
            };

            // Act
            var result = rules.GetNextPossessionAction(state, PossessionEndReason.Punt);

            // Assert
            Assert.AreEqual(OvertimePossessionResult.OtherTeamGetsBall, result);
        }

        [TestMethod]
        public void NflRegularSeason_SecondPossessionFailsToMatch_GameOver()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = true,
                SecondPossessionComplete = false,
                FirstTeamPeriodScore = 3, // First team kicked FG
                SecondTeamPeriodScore = 0 // Second team failed to score
            };

            // Act
            var result = rules.GetNextPossessionAction(state, PossessionEndReason.TurnoverOnDowns);

            // Assert
            Assert.AreEqual(OvertimePossessionResult.GameOver, result);
        }

        [TestMethod]
        public void NflRegularSeason_SecondPossessionMatchesScore_SuddenDeath()
        {
            // Arrange
            var rules = new NflRegularSeasonOvertimeRulesProvider();
            var state = new OvertimeState
            {
                IsInOvertime = true,
                CurrentPeriod = 1,
                FirstPossessionComplete = true,
                SecondPossessionComplete = false,
                FirstTeamPeriodScore = 3, // First team kicked FG
                SecondTeamPeriodScore = 3 // Second team also kicked FG
            };

            // Act - possession ends without scoring more
            var result = rules.GetNextPossessionAction(state, PossessionEndReason.TurnoverOnDowns);

            // Assert
            Assert.AreEqual(OvertimePossessionResult.SuddenDeath, result);
        }

        #endregion

        #region Quarter Duration Tests

        [TestMethod]
        public void Quarter_OvertimeDuration_CanBeSetTo600Seconds()
        {
            // Arrange & Act
            var quarter = new Gridiron.Engine.Domain.Time.Quarter(
                Gridiron.Engine.Domain.Time.QuarterType.Overtime,
                600);

            // Assert
            Assert.AreEqual(600, quarter.TimeRemaining);
            Assert.AreEqual(600, quarter.MaxDuration);
        }

        [TestMethod]
        public void Quarter_RegularDuration_Is900Seconds()
        {
            // Arrange & Act
            var quarter = new Gridiron.Engine.Domain.Time.Quarter(
                Gridiron.Engine.Domain.Time.QuarterType.First);

            // Assert
            Assert.AreEqual(900, quarter.TimeRemaining);
            Assert.AreEqual(900, quarter.MaxDuration);
        }

        #endregion

        #region Game Properties Tests

        [TestMethod]
        public void Game_IsInOvertime_FalseWhenNoOvertimeState()
        {
            // Arrange
            var game = new Game();

            // Assert
            Assert.IsFalse(game.IsInOvertime);
        }

        [TestMethod]
        public void Game_IsInOvertime_TrueWhenOvertimeStateSet()
        {
            // Arrange
            var game = new Game
            {
                OvertimeState = new OvertimeState { IsInOvertime = true }
            };

            // Assert
            Assert.IsTrue(game.IsInOvertime);
        }

        [TestMethod]
        public void Game_IsTie_TrueWhenScoresEqual()
        {
            // Arrange
            var game = new Game
            {
                HomeScore = 21,
                AwayScore = 21
            };

            // Assert
            Assert.IsTrue(game.IsTie);
        }

        [TestMethod]
        public void Game_Winner_NullWhenTied()
        {
            // Arrange
            var game = new Game
            {
                HomeScore = 21,
                AwayScore = 21
            };

            // Assert
            Assert.IsNull(game.Winner);
        }

        [TestMethod]
        public void Game_Winner_ReturnsHomeTeamWhenHomeWins()
        {
            // Arrange
            var homeTeam = new Team { Name = "Home" };
            var awayTeam = new Team { Name = "Away" };
            var game = new Game
            {
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                HomeScore = 28,
                AwayScore = 21
            };

            // Assert
            Assert.AreEqual(homeTeam, game.Winner);
        }

        #endregion
    }
}
