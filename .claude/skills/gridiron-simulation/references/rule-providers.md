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

## Where Rules Diverge

Reference tables for key rule differences. Use these when implementing new providers.

### Overtime Rules

| Rule | NFL Regular | NFL Playoff | NCAA |
|------|-------------|-------------|------|
| Period length | 10 minutes | 10 minutes | Untimed |
| Ties allowed | Yes (after 1 OT) | No | No |
| First TD wins | Yes* | Yes* | No |
| Starting position | Kickoff | Kickoff | 25-yard line |

*Note: 2025 NFL rules require both teams to possess — see #40.

### Kickoff Rules

| Rule | NFL (Pre-2024) | NFL (2024+) | NCAA |
|------|----------------|-------------|------|
| Kickoff from | 35-yard line | 35-yard line | 35-yard line |
| Touchback to | 25-yard line | **30-yard line** | 25-yard line |
| Fair catch touchback | 25-yard line | 25-yard line | N/A |

### Clock Rules

| Rule | NFL | NCAA |
|------|-----|------|
| Clock on first down | Runs (except <2 min) | **Stops briefly** |
| Two-minute warning | **Yes** | No |
| Play clock | 40 seconds | 40 seconds |

### Penalty Enforcement

| Rule | NFL | NCAA |
|------|-----|------|
| Pass interference | Spot foul | **15 yards max** |
| Targeting | 15 yards | 15 yards + **ejection** |
| Defensive holding | 5 yards + auto 1st | 10 yards |

### Catch Rules

| Rule | NFL | NCAA |
|------|-----|------|
| Feet in bounds | Two feet | **One foot** |
| Must survive ground | Yes | Yes |

### Scoring Rules

| Rule | NFL | NCAA |
|------|-----|------|
| PAT distance | 15-yard line (33-yard kick) | 3-yard line (20-yard kick) |
| 2-point from | 2-yard line | 3-yard line |

---

## Current Implementation: Overtime Rules

The first rule provider implemented is for overtime mechanics.

### Interface: `IOvertimeRulesProvider`

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

### Supporting Enums

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

### Implemented Providers

#### NflRegularSeasonOvertimeRulesProvider

**File:** `Gridiron.Engine/Simulation/Overtime/NflRegularSeasonOvertimeRulesProvider.cs`

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
- First TD wins immediately (no response possession)*
- If first possession ends in FG, other team gets possession
- After both possess, sudden death (any score wins)
- Game ends in tie if still tied after 10 minutes

*Note: This is pre-2025 behavior. See #40 for 2025 rule update.

#### NflPlayoffOvertimeRulesProvider

**File:** `Gridiron.Engine/Simulation/Overtime/NflPlayoffOvertimeRulesProvider.cs`

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

### Registry Access

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
OvertimeRulesRegistry.Register("CUSTOM", new CustomOvertimeRulesProvider());
```

### Base Class: `NflOvertimeRulesProviderBase`

Both NFL providers inherit from this base class which implements the shared NFL overtime logic:

- Defensive TD always ends game immediately
- First possession TD wins immediately
- FG/Safety on first possession → other team gets ball
- Second team scores more → they win
- Scores tied after both possess → sudden death

---

## Extending the Provider Pattern

When implementing new rule providers, follow these patterns:

### Creating a Custom Overtime Provider

```csharp
public class CustomOvertimeRulesProvider : NflOvertimeRulesProviderBase
{
    public override string Name => "Custom League";
    public override string Description => "Custom overtime rules";
    public override bool AllowsTies => false;
    public override int MaxOvertimePeriods => 2;

    protected override OvertimePossessionResult HandlePeriodEnd(OvertimeState state)
    {
        return state.CurrentPeriod >= MaxOvertimePeriods
            ? OvertimePossessionResult.GameOver
            : OvertimePossessionResult.NewPeriod;
    }
}

// Register for use
OvertimeRulesRegistry.Register("CUSTOM", new CustomOvertimeRulesProvider());
```

### Provider Selection

```csharp
// Via registry (recommended)
var rules = OvertimeRulesRegistry.GetByName("NFL_PLAYOFF");

// Or via simulation options
var options = new SimulationOptions
{
    OvertimeRulesProvider = OvertimeRulesRegistry.NflPlayoff
};
```

---

## Testing

Each provider should be tested independently:

1. Simulate 1000+ games using each provider
2. Verify overtime frequency matches expectations (~5-6% of games)
3. Verify tie rate for regular season (~0.5% of OT games)
4. Verify no ties in playoff provider
5. Verify state transitions match expected flow

---

## Related Issues

- **#29** - Implement NCAA overtime rules provider
- **#40** - Update NFL overtime to 2025 rules
- **#41** - Implement comprehensive rule provider system (extends pattern to kickoff, clock, penalties, scoring, catches)
