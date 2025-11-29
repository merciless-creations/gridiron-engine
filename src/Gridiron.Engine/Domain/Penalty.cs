using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gridiron.Engine.Domain
{
    /// <summary>
    /// Represents a penalty that can occur during a football play.
    /// Contains both the penalty definition (odds, type) and instance data (player, yards).
    /// </summary>
    public class Penalty
    {
        /// <summary>
        /// Gets or sets the name/type of the penalty.
        /// </summary>
        public PenaltyNames Name { get; set; }

        /// <summary>
        /// Gets or sets the probability of this penalty occurring.
        /// </summary>
        public float Odds { get; set; }

        /// <summary>
        /// Gets or sets the probability that this penalty is called on the away team.
        /// </summary>
        public float AwayOdds { get; set; }

        /// <summary>
        /// Gets or sets which team the penalty was called on.
        /// </summary>
        public Possession CalledOn { get; set; }

        /// <summary>
        /// Gets or sets when the penalty occurred (before, during, or after the play).
        /// </summary>
        public PenaltyOccuredWhen OccuredWhen { get; set; }

        /// <summary>
        /// Gets or sets the player associated with this penalty definition.
        /// Null for penalty definitions that don't specify a player.
        /// </summary>
        public Player? Player { get; set; }

        /// <summary>
        /// Gets or sets the player who actually committed this penalty instance.
        /// </summary>
        public Player? CommittedBy { get; set; }

        /// <summary>
        /// Gets or sets the specific type of penalty for this instance.
        /// </summary>
        public PenaltyNames PenaltyType { get; set; }

        /// <summary>
        /// Gets or sets the number of yards assessed for this penalty.
        /// </summary>
        public int Yards { get; set; }

        /// <summary>
        /// Gets or sets whether the penalty was accepted by the opposing team.
        /// </summary>
        public bool Accepted { get; set; }
    }

    /// <summary>
    /// Provides static penalty data including probabilities for each penalty type.
    /// </summary>
    public static class PenaltyData
    {
        /// <summary>
        /// Gets or sets the list of all possible penalties with their occurrence probabilities.
        /// </summary>
        public static List<Penalty> List { get; set; }

        static PenaltyData()
        {
            string penalties =
                @"[{'Name':'OffensiveHolding','Odds':0.01900018,'HomeOdds':0.51,'AwayOdds':0.49},
                    {'Name':'FalseStart','Odds':0.01554800,'HomeOdds':0.53,'AwayOdds':0.47},
                    {'Name':'DefensivePassInterference','Odds':0.00640367,'HomeOdds':0.48,'AwayOdds':0.52},
                    {'Name':'UnnecessaryRoughness','Odds':0.00621920,'HomeOdds':0.54,'AwayOdds':0.46},
                    {'Name':'DefensiveHolding','Odds':0.00600838,'HomeOdds':0.53,'AwayOdds':0.47},
                    {'Name':'DefensiveOffside','Odds':0.00469075,'HomeOdds':0.55,'AwayOdds':0.45},
                    {'Name':'NeutralZoneInfraction','Odds':0.00419005,'HomeOdds':0.33,'AwayOdds':0.67},
                    {'Name':'DelayofGame','Odds':0.00400559,'HomeOdds':0.34,'AwayOdds':0.66},
                    {'Name':'IllegalBlockAbovetheWaist','Odds':0.00339948,'HomeOdds':0.53,'AwayOdds':0.47},
                    {'Name':'IllegalUseofHands','Odds':0.00313595,'HomeOdds':0.57,'AwayOdds':0.43},
                    {'Name':'OffensivePassInterference','Odds':0.00274066,'HomeOdds':0.56,'AwayOdds':0.44},
                    {'Name':'FaceMask15Yards','Odds':0.00266161,'HomeOdds':0.45,'AwayOdds':0.55},
                    {'Name':'RoughingthePasser','Odds':0.00268796,'HomeOdds':0.44,'AwayOdds':0.56},
                    {'Name':'UnsportsmanlikeConduct','Odds':0.00229267,'HomeOdds':0.31,'AwayOdds':0.69},
                    {'Name':'IllegalContact','Odds':0.00163386,'HomeOdds':0.37,'AwayOdds':0.63},
                    {'Name':'IllegalFormation','Odds':0.00163386,'HomeOdds':0.5,'AwayOdds':0.5},
                    {'Name':'Defensive12OnField','Odds':0.00137033,'HomeOdds':0.46,'AwayOdds':0.54},
                    {'Name':'Encroachment','Odds':0.00115951,'HomeOdds':0.36,'AwayOdds':0.64},
                    {'Name':'IntentionalGrounding','Odds':0.00089599,'HomeOdds':0.62,'AwayOdds':0.38},
                    {'Name':'IllegalShift','Odds':0.00084328,'HomeOdds':0.47,'AwayOdds':0.53},
                    {'Name':'Taunting','Odds':0.00050070,'HomeOdds':0.58,'AwayOdds':0.42},
                    {'Name':'IneligibleDownfieldPass','Odds':0.00044799,'HomeOdds':0.41,'AwayOdds':0.59},
                    {'Name':'OffsideonFreeKick','Odds':0.00042164,'HomeOdds':0.56,'AwayOdds':0.44},
                    {'Name':'ChopBlock','Odds':0.00042164,'HomeOdds':0.5,'AwayOdds':0.5},
                    {'Name':'PlayerOutofBoundsonPunt','Odds':0.00034258,'HomeOdds':0.62,'AwayOdds':0.38},
                    {'Name':'RunningIntotheKicker','Odds':0.00034258,'HomeOdds':0.38,'AwayOdds':0.62},
                    {'Name':'HorseCollarTackle','Odds':0.00036894,'HomeOdds':0.36,'AwayOdds':0.64},
                    {'Name':'IllegalMotion','Odds':0.00031623,'HomeOdds':0.42,'AwayOdds':0.58},
                    {'Name':'Tripping','Odds':0.00028988,'HomeOdds':0.45,'AwayOdds':0.55},
                    {'Name':'Offensive12OnField','Odds':0.00018447,'HomeOdds':0.29,'AwayOdds':0.71},
                    {'Name':'IllegalSubstitution','Odds':0.00021082,'HomeOdds':0.25,'AwayOdds':0.75},
                    {'Name':'PersonalFoul','Odds':0.00023717,'HomeOdds':0.89,'AwayOdds':0.11},
                    {'Name':'IneligibleDownfieldKick','Odds':0.00023717,'HomeOdds':0.22,'AwayOdds':0.78},
                    {'Name':'IllegalForwardPass','Odds':0.00023717,'HomeOdds':0.67,'AwayOdds':0.33},
                    {'Name':'Clipping','Odds':0.00021082,'HomeOdds':0.63,'AwayOdds':0.37},
                    {'Name':'IllegalBlindsideBlock','Odds':0.00021082,'HomeOdds':0.63,'AwayOdds':0.37},
                    {'Name':'DefensiveDelayofGame','Odds':0.00015812,'HomeOdds':0.5,'AwayOdds':0.5},
                    {'Name':'IllegalTouchPass','Odds':0.00015812,'HomeOdds':0.33,'AwayOdds':0.67},
                    {'Name':'FairCatchInterference','Odds':0.00015812,'HomeOdds':0.17,'AwayOdds':0.83},
                    {'Name':'OffensiveOffside','Odds':0.00010541,'HomeOdds':0,'AwayOdds':1},
                    {'Name':'IllegalTouchKick','Odds':0.00005271,'HomeOdds':0,'AwayOdds':1},
                    {'Name':'LowBlock','Odds':0.00007906,'HomeOdds':0.67,'AwayOdds':0.33},
                    {'Name':'IllegalPeelback','Odds':0.00005271,'HomeOdds':0,'AwayOdds':1},
                    {'Name':'Leaping','Odds':0.00007906,'HomeOdds':0.67,'AwayOdds':0.33},
                    {'Name':'RoughingtheKicker','Odds':0.00005271,'HomeOdds':0.5,'AwayOdds':0.5},
                    {'Name':'IllegalCrackback','Odds':0.00010541,'HomeOdds':0.75,'AwayOdds':0.25},
                    {'Name':'InvalidFairCatchSignal','Odds':0.00005271,'HomeOdds':0.5,'AwayOdds':0.5},
                    {'Name':'Disqualification','Odds':0.00007906,'HomeOdds':0.33,'AwayOdds':0.67},
                    {'Name':'InterferencewithOpportunitytoCatch','Odds':0.00007906,'HomeOdds':0.67,'AwayOdds':0.33},
                    {'Name':'Leverage','Odds':0.00002635,'HomeOdds':0,'AwayOdds':1},
                    {'Name':'NoPenalty','Odds':0,'HomeOdds':0,'AwayOdds':0}]";

            List = [.. JsonConvert.DeserializeObject<List<Penalty>>(penalties)!];
        }
    }

    /// <summary>
    /// Defines the types of penalties that can be called in a football game.
    /// </summary>
    public enum PenaltyNames
    {
        NoPenalty,
        OffensiveHolding,
        FalseStart,
        DefensivePassInterference,
        UnnecessaryRoughness,
        DefensiveHolding,
        DefensiveOffside,
        NeutralZoneInfraction,
        DelayofGame,
        IllegalBlockAbovetheWaist,
        IllegalUseofHands,
        OffensivePassInterference,
        FaceMask15Yards,
        RoughingthePasser,
        UnsportsmanlikeConduct,
        IllegalContact,
        IllegalFormation,
        Defensive12OnField,
        Encroachment,
        IntentionalGrounding,
        IllegalShift,
        Taunting,
        IneligibleDownfieldPass,
        OffsideonFreeKick,
        ChopBlock,
        PlayerOutofBoundsonPunt,
        RunningIntotheKicker,
        HorseCollarTackle,
        IllegalMotion,
        Tripping,
        Offensive12OnField,
        IllegalSubstitution,
        PersonalFoul,
        IneligibleDownfieldKick,
        IllegalForwardPass,
        Clipping,
        IllegalBlindsideBlock,
        DefensiveDelayofGame,
        IllegalTouchPass,
        FairCatchInterference,
        OffensiveOffside,
        IllegalTouchKick,
        LowBlock,
        IllegalPeelback,
        Leaping,
        RoughingtheKicker,
        IllegalCrackback,
        InvalidFairCatchSignal,
        Disqualification,
        InterferencewithOpportunitytoCatch,
        Leverage
    }

    /// <summary>
    /// Defines when a penalty occurred relative to the play.
    /// </summary>
    public enum PenaltyOccuredWhen
    {
        /// <summary>Penalty occurred before the snap.</summary>
        Before,
        /// <summary>Penalty occurred during the play.</summary>
        During,
        /// <summary>Penalty occurred after the play ended.</summary>
        After
    }
}