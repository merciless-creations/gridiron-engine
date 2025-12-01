# Rule Providers

Pluggable rule systems for game mechanics that differ between league types, eras, or custom configurations.

## The Problem

Football rules aren't universal. Key differences exist between:
- **NFL vs NCAA** — Overtime format, clock rules, pass interference enforcement, catch rules
- **Regular Season vs Playoffs** — Overtime can/cannot end in tie
- **Era variations** — Kickoff touchback rules have changed multiple times
- **Custom leagues** — Users may want house rules

Hardcoding these differences throughout the simulation creates maintenance nightmares. Instead, we use the **provider pattern**.

## The Pattern

A rule provider encapsulates a set of related rules behind an interface. The simulation queries the provider at decision points rather than containing hardcoded logic.

```
┌─────────────────────────────────────────────────────────┐
│                    SIMULATION ENGINE                     │
│                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │ Play        │    │ Clock       │    │ Scoring     │  │
│  │ Resolution  │    │ Management  │    │ Logic       │  │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘  │
│         │                  │                  │         │
│         └────────┬─────────┴─────────┬────────┘         │
│                  │                   │                  │
│                  ▼                   ▼                  │
│         ┌───────────────────────────────────┐           │
│         │        RULE PROVIDER INTERFACE     │           │
│         └───────────────────────────────────┘           │
└─────────────────────────────────────────────────────────┘
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
   │ NFL Regular │  │ NFL Playoff │  │    NCAA     │
   │   Season    │  │             │  │   College   │
   └─────────────┘  └─────────────┘  └─────────────┘
```

**Benefits:**
- Add new rule sets without modifying core simulation
- Test rule variations independently
- Support custom league configurations
- Clear separation of "what happened" vs "what rules apply"

---

# Where Rules Diverge

## Currently Implemented: Overtime

See detailed implementation below.

## Future: Kickoff Rules

| Rule | NFL (Pre-2024) | NFL (2024+) | NCAA |
|------|----------------|-------------|------|
| Kickoff from | 35-yard line | 35-yard line | 35-yard line |
| Touchback to | 25-yard line | **30-yard line** | 25-yard line |
| Fair catch touchback | 25-yard line | 25-yard line | N/A |
| Onside kick rules | Anytime | Restricted in 4th | Anytime |

## Future: Clock Rules

| Rule | NFL | NCAA |
|------|-----|------|
| Clock on first down | Runs (except <2 min) | **Stops briefly** |
| Clock after out of bounds | Stops, restarts on ready | Stops until snap (varies) |
| Play clock | 40 seconds | 40 seconds |
| Two-minute warning | **Yes** | No |

## Future: Penalty Enforcement

| Rule | NFL | NCAA |
|------|-----|------|
| Pass interference | Spot foul | **15 yards max** |
| Targeting | 15 yards | 15 yards + **ejection** |
| Defensive holding | 5 yards + auto 1st | 10 yards |

## Future: Catch Rules

| Rule | NFL | NCAA |
|------|-----|------|
| Feet in bounds | Two feet | **One foot** |
| Going to ground | Must survive | Must survive |

## Future: Conversion Rules

| Rule | NFL | NCAA |
|------|-----|------|
| PAT distance | 15-yard line (33-yard kick) | 3-yard line (20-yard kick) |
| 2-point from | 2-yard line | 3-yard line |
| Defensive return | 2 points | 2 points |

---

# Current Implementation: Overtime Rules

The first rule provider implemented is for overtime mechanics.

## Interface: `IOvertimeRulesProvider`

**File:** `Gridiron.Engine/Simulation/Overtime/IOvertimeRulesProvider.cs`

```csharp
public interface IOvertimeRulesProvider
{
    // Identity
    string Name { get; }
    string Description { get; }

    // Configuration
    int OvertimePeriodDuration { get; }      // Seconds (600 = 10 min)
    int TimeoutsPerTeam { get; }
    int? FixedStartingFieldPosition { get; } // null = use kickoff
    bool HasOvertimeCoinToss { get; }
    bool AllowsTies { get; }
    int MaxOvertimePeriods { get; }          // 0 = unlimited

    // Game State Queries
    OvertimeGameEndResult ShouldGameEnd(OvertimeState state, OvertimeScoreType scoreType, Possession scoringTeam);
    OvertimePossessionResult GetNextPossessionAction(OvertimeState state, PossessionEndReason reason);
    bool ShouldStartNewPeriod(OvertimeState state);
    int GetStartingFieldPosition(OvertimeState state, Possession possession);
    (Downs down, int yardsToGo) GetStartingDownAndDistance(OvertimeState state);

    // Special Rules
    bool IsTwoPointConversionRequired(OvertimeState state);
    bool IsTwoPointPlayOnly(OvertimeState state);
    bool UsesKickoff(OvertimeState state);
}
```

## Supporting Enums

**File:** `Gridiron.Engine/Simulation/Overtime/OvertimeEnums.cs`

```csharp
public enum OvertimeScoreType
{
    Touchdown,          // 6 points
    FieldGoal,          // 3 points
    Safety,             // 2 points
    DefensiveTouchdown  // 6 points (pick-six, fumble return)
}

public enum PossessionEndReason
{
    Punt, TurnoverOnDowns, Interception, Fumble, MissedFieldGoal, TimeExpired
}

public enum OvertimeGameEndResult
{
    Continue,    // Game continues
    GameOver,    // Winner determined
    PeriodOver,  // Period ends, may start new one
    TieGame      // NFL regular season only
}

public enum OvertimePossessionResult
{
    OtherTeamGetsBall,  // Normal possession change
    SuddenDeath,        // Next score wins
    NewPeriod,          // Start new OT period
    GameOver            // Tie or winner determined
}
```

## Implemented Providers

### NflRegularSeasonOvertimeRulesProvider

**File:** `Gridiron.Engine/Simulation/Overtime/NflRegularSeasonOvertimeRulesProvider.cs`

Default for Gridiron leagues during regular season.

| Configuration | Value |
|---------------|-------|
| OvertimePeriodDuration | 600 seconds (10 minutes) |
| TimeoutsPerTeam | 2 |
| FixedStartingFieldPosition | null (uses kickoff) |
| HasOvertimeCoinToss | true |
| AllowsTies | **true** |
| MaxOvertimePeriods | **1** |

**Overtime Rules:**
- 10-minute single overtime period
- Coin toss determines first possession
- First TD wins immediately (no response possession)
- If first possession ends in FG, other team gets possession
- After both possess, sudden death (any score wins)
- Game ends in tie if still tied after 10 minutes

### NflPlayoffOvertimeRulesProvider

**File:** `Gridiron.Engine/Simulation/Overtime/NflPlayoffOvertimeRulesProvider.cs`

Used for postseason games. Cannot end in tie.

| Configuration | Value |
|---------------|-------|
| OvertimePeriodDuration | 600 seconds (10 minutes) |
| TimeoutsPerTeam | 2 |
| FixedStartingFieldPosition | null (uses kickoff) |
| HasOvertimeCoinToss | true |
| AllowsTies | **false** |
| MaxOvertimePeriods | **0** (unlimited) |

**Overtime Rules:**
- 10-minute periods, unlimited until winner
- First TD wins immediately (same as regular season currently)
- After both possess, sudden death applies
- New periods start if still tied when time expires

## Registry Access

**File:** `Gridiron.Engine/Simulation/Overtime/OvertimeRulesRegistry.cs`

```csharp
// Get providers
OvertimeRulesRegistry.Default           // NFL Regular Season
OvertimeRulesRegistry.NflRegularSeason  // NFL Regular Season
OvertimeRulesRegistry.NflPlayoff        // NFL Playoff

// Lookup by name (case-insensitive)
OvertimeRulesRegistry.GetByName("NFL")              // Regular Season
OvertimeRulesRegistry.GetByName("NFL_PLAYOFF")      // Playoff
OvertimeRulesRegistry.GetByName("NFL_PLAYOFFS")     // Playoff

// Register custom provider
OvertimeRulesRegistry.Register("NCAA", new NcaaOvertimeRulesProvider());
```

## Base Class: `NflOvertimeRulesProviderBase`

Both NFL providers inherit from this base class which implements the shared NFL 2024 overtime logic:

- Defensive TD always ends game immediately
- First possession TD wins immediately
- FG/Safety on first possession → other team gets ball
- Second team scores more → they win
- Scores tied after both possess → sudden death

---

# Not Yet Implemented

## 2025 NFL Rule Change

> **Status:** Not implemented. See comment in documentation below.

As of 2025, NFL regular season overtime requires both teams to possess even if the first team scores a TD. Current implementation still uses the pre-2025 "first TD wins" behavior.

To implement, the base class `ShouldGameEnd()` method would need modification:
```csharp
// Current behavior (pre-2025):
if (scoreType == OvertimeScoreType.Touchdown)
{
    return OvertimeGameEndResult.GameOver; // First TD wins
}

// 2025 behavior (not yet implemented):
// First TD should allow response possession
```

## NCAACollegeRules

> **Status:** Not implemented. Interface supports it but no provider exists.

Target design for potential college mode:

**Overtime:**
- No game clock in overtime (untimed possessions)
- Each team gets one possession per period
- Possessions start at opponent's 25-yard line
- One timeout per team per overtime period
- 2nd overtime: Must attempt 2-point conversion after TD
- 3rd+ overtime: Alternating 2-point conversion attempts only (no drives)

```csharp
// Target implementation
public class NcaaOvertimeRulesProvider : IOvertimeRulesProvider
{
    public int OvertimePeriodDuration => 0;  // No game clock
    public int? FixedStartingFieldPosition => 75;  // 25-yard line
    public int TimeoutsPerTeam => 1;
    public bool AllowsTies => false;
    public int MaxOvertimePeriods => 0;  // Unlimited

    public bool IsTwoPointConversionRequired(OvertimeState state)
        => state.CurrentPeriod >= 2;

    public bool IsTwoPointPlayOnly(OvertimeState state)
        => state.CurrentPeriod >= 3;

    public bool UsesKickoff(OvertimeState state) => false;
}
```

**Other NCAA Rule Differences (not modeled):**
- Clock stops after every first down
- Pass interference: 15 yards max (not spot foul)
- One foot in bounds for catch (vs. two in NFL)
- Targeting rules

---

# Target Design: Comprehensive Rule Provider

The current implementation only covers overtime. The target is a comprehensive rule provider that handles all divergent rules.

## Proposed Architecture

```csharp
public interface IRuleProvider
{
    // Identity
    string Name { get; }              // "NFL 2024", "NCAA 2024", "NFL 2020"
    string Description { get; }

    // Sub-providers (composition)
    IOvertimeRulesProvider Overtime { get; }
    IKickoffRulesProvider Kickoff { get; }
    IClockRulesProvider Clock { get; }
    IPenaltyRulesProvider Penalties { get; }
    IScoringRulesProvider Scoring { get; }
    ICatchRulesProvider Catching { get; }
}
```

## Sub-Provider Interfaces (Target)

### IKickoffRulesProvider

```csharp
public interface IKickoffRulesProvider
{
    int KickoffYardLine { get; }           // 35
    int TouchbackYardLine { get; }         // 25 or 30
    int FairCatchTouchbackYardLine { get; }
    bool IsOnsideKickRestricted(GameState state);
    int SafetyKickYardLine { get; }        // 20
}
```

### IClockRulesProvider

```csharp
public interface IClockRulesProvider
{
    int QuarterLengthSeconds { get; }      // 900 (15 min)
    int PlayClockSeconds { get; }          // 40
    bool HasTwoMinuteWarning { get; }      // NFL: true, NCAA: false
    bool ClockStopsOnFirstDown(GameState state);
    bool ClockRestartsOnReady(PlayEndReason reason);
}
```

### IPenaltyRulesProvider

```csharp
public interface IPenaltyRulesProvider
{
    int GetPassInterferenceYardage(int spotYards);  // NFL: spot, NCAA: min(15, spot)
    bool IsTargetingAnEjection { get; }
    int DefensiveHoldingYardage { get; }   // NFL: 5, NCAA: 10
    bool DefensiveHoldingAutoFirstDown { get; }
}
```

### IScoringRulesProvider

```csharp
public interface IScoringRulesProvider
{
    int PatSnapYardLine { get; }           // NFL: 15, NCAA: 3
    int TwoPointYardLine { get; }          // NFL: 2, NCAA: 3
    int DefensiveConversionReturnPoints { get; }  // 2
}
```

### ICatchRulesProvider

```csharp
public interface ICatchRulesProvider
{
    int FeetRequiredInBounds { get; }      // NFL: 2, NCAA: 1
    bool MustSurviveGround { get; }        // true for both
}
```

## Pre-Built Configurations

```csharp
public static class RuleProviders
{
    public static IRuleProvider Nfl2024 { get; }
    public static IRuleProvider Nfl2023 { get; }
    public static IRuleProvider NflPlayoff { get; }
    public static IRuleProvider Ncaa2024 { get; }

    // For historical accuracy or what-if scenarios
    public static IRuleProvider Nfl2010 { get; }  // Before touchback changes
}
```

## Custom League Support

```csharp
// Users can override specific rules
var customRules = new CustomRuleProvider(RuleProviders.Nfl2024)
{
    Overtime = { AllowsTies = false },     // No ties in this league
    Kickoff = { TouchbackYardLine = 20 },  // House rule
    Clock = { HasTwoMinuteWarning = false } // Faster games
};
```

This allows Gridiron leagues to:
- Base on a standard ruleset
- Override specific rules as desired
- Create unique league experiences

## Overtime State Machine

The overtime flow is managed by the base class `ShouldGameEnd()` and `GetNextPossessionAction()` methods:

```
┌─────────────────┐
│  COIN_TOSS      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ FIRST_POSSESSION│ ◄─────────────────┐
└────────┬────────┘                   │
         │                            │
         ▼                            │
    ┌────────────┐                    │
    │  SCORED?   │                    │
    └────┬───────┘                    │
         │                            │
    ┌────┴────┐                       │
    │ YES     │ NO                    │
    ▼         ▼                       │
┌────────┐ ┌────────────────┐         │
│ TD?    │ │SECOND_POSSESSION│         │
└───┬────┘ └───────┬────────┘         │
    │              │                  │
┌───┴───┐          ▼                  │
│YES    │NO   ┌────────────┐          │
▼       ▼     │  SCORED?   │          │
GAME   OTHER  └────┬───────┘          │
OVER   TEAM        │                  │
       BALL   ┌────┴────┐             │
              │ YES     │ NO          │
              ▼         ▼             │
         ┌────────┐ ┌──────────┐      │
         │COMPARE │ │SUDDEN    │      │
         │SCORES  │ │DEATH     │      │
         └───┬────┘ └────┬─────┘      │
             │           │            │
             ▼           ▼            │
        ┌─────────┐ ┌─────────┐       │
        │ AHEAD?  │ │NEXT SCORE│       │
        └────┬────┘ │WINS      │       │
             │      └──────────┘       │
        ┌────┴────┐                   │
        │ YES     │ NO (tied)         │
        ▼         ▼                   │
   ┌──────────┐ ┌──────────┐          │
   │GAME_OVER │ │SUDDEN    │          │
   │(winner)  │ │DEATH     │──────────┘
   └──────────┘ └──────────┘
```

---

# Implementation Notes

## Provider Selection

```csharp
// Via registry (recommended)
var rules = OvertimeRulesRegistry.GetByName("NFL_PLAYOFF");

// Or via simulation options
var options = new SimulationOptions
{
    OvertimeRulesProvider = OvertimeRulesRegistry.NflPlayoff
};
```

## Extending for Custom Rules

Create a new provider by implementing the interface or extending the base class:

```csharp
public class CustomOvertimeRulesProvider : NflOvertimeRulesProviderBase
{
    public override string Name => "Custom League";
    public override string Description => "Custom overtime rules";
    public override bool AllowsTies => false;  // Override as needed
    public override int MaxOvertimePeriods => 2;  // Custom limit

    protected override OvertimePossessionResult HandlePeriodEnd(OvertimeState state)
    {
        // Custom period-end logic
        return state.CurrentPeriod >= MaxOvertimePeriods
            ? OvertimePossessionResult.GameOver
            : OvertimePossessionResult.NewPeriod;
    }
}

// Register for use
OvertimeRulesRegistry.Register("CUSTOM", new CustomOvertimeRulesProvider());
```

## Future Providers

If XFL, USFL, or other leagues are added:

```csharp
public class XflOvertimeRulesProvider : IOvertimeRulesProvider
{
    // XFL-specific:
    // - Shootout format (5 plays from 5-yard line each)
    // - 1, 2, 3 point conversions from different distances
    // - No game clock in OT
}
```

---

# Testing

Each provider should be tested independently:

1. Simulate 1000+ games using each provider
2. Verify overtime frequency matches expectations (~5-6% of games)
3. Verify tie rate for regular season (~0.5% of OT games)
4. Verify no ties in playoff provider
5. Verify state transitions match expected flow

---

# Related Issues

- **#26** - Documentation sync (this file)
- **#27** - Logarithmic modifier curves
- **#28** - Expand player attribute system
