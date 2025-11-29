using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Contains enumeration types for tracking various statistics in the football simulation.
    /// </summary>
    public class StatTypes
    {
        /// <summary>
        /// Defines the types of statistics tracked for individual players.
        /// </summary>
        public enum PlayerStatType
        {
            PassingYards,
            PassingAttempts,
            PassingCompletions,
            PassingTouchdowns,
            InterceptionsThrown,
            RushingYards,
            RushingAttempts,
            RushingTouchdowns,
            Fumbles,
            Receptions,
            ReceivingTargets,
            ReceivingYards,
            ReceivingTouchdowns,
            Tackles,
            Sacks,
            InterceptionsCaught,
            ForcedFumbles,
            FumbleRecoveries,
            FieldGoalsMade,
            FieldGoalsAttempted,
            ExtraPointsMade,
            ExtraPointsAttempted,
            Punts,
            PuntYards,
            KickoffReturns,
            KickoffReturnYards,
            PuntReturns,
            PuntReturnYards,
            GamesPlayed,
            GamesStarted,
            InterceptionReturnYards,
            FumblesLost,
            PuntsInside20
        }

        /// <summary>
        /// Defines the types of statistics tracked for teams.
        /// </summary>
        public enum TeamStatType
        {
            PointsScored,
            PointsAllowed,
            TotalYards,
            PassingYards,
            RushingYards,
            TurnoversCommitted,
            TurnoversForced,
            Penalties,
            PenaltyYards,
            ThirdDownConversions,
            ThirdDownAttempts,
            RedZoneAttempts,
            RedZoneTouchdowns,
            TimeOfPossessionSeconds,
            Wins,
            Losses,
            Ties
        }

        /// <summary>
        /// Defines the types of statistics tracked for coaches.
        /// </summary>
        public enum CoachStatType
        {
            Wins,
            Losses,
            Ties,
            PlayoffAppearances,
            Championships,
            GamesCoached
        }

        /// <summary>
        /// Defines the types of statistics tracked for trainers and medical staff.
        /// </summary>
        public enum TrainerStatType
        {
            InjuriesTreated,
            AverageRecoveryTimeDays,
            SuccessfulRehabs,
            SeasonEndingInjuries,
            PlayerReturnRate
        }

        /// <summary>
        /// Defines the types of statistics tracked for scouts.
        /// </summary>
        public enum ScoutStatType
        {
            PlayersScouted,
            DraftHits,
            DraftMisses,
            ProPlayersRecommended,
            CollegePlayersRecommended,
            ScoutingAccuracy
        }
    }
}
