# Gridiron Engine

The backend and simulation engine for Gridiron, an NFL franchise management game.

## Repository

https://github.com/merciless-creations/gridiron-engine

## Project

https://github.com/orgs/merciless-creations/projects/2

## Tech Stack

| Layer | Technology |
|-------|------------|
| Language | C# |
| Database | Azure SQL |
| ORM | Entity Framework |

## Architecture

Controllers → Services → Repositories. RESTful API consumed by the React frontend.

## Simulation Engine Philosophy

The play-by-play engine is the core of Gridiron. Scott built the state machine. Claude writes supporting systems under Scott's direction.

### Outcome-First Simulation

The engine determines **what happened and who did it**. Nothing else.

**Inputs that matter:**
- The 11 players on the field for each side
- Each player's attributes (speed, strength, awareness, technique, etc.)
- Each player's current state (fatigue, injury status, confidence)
- Player intelligence and discipline (affects decision-making, penalties)
- Coaching staff influence (scheme effectiveness, play-calling tendencies, adjustments)
- Game situation (down, distance, field position, score, time)

**Inputs that do NOT matter during simulation:**
- Formations
- Specific play names
- Audibles
- Pre-snap reads
- Motion and shifts
- Broadcast-style presentation details

These are **rendering concerns**. They can be derived, inferred, or generated after the outcome is determined. The simulation engine does not model them.

### Why This Matters

1. **Performance** — Less state to track means faster simulation. Leagues with many games need speed.

2. **Flexibility** — Presentation can evolve independently. Add formation graphics later without touching the engine.

3. **Clarity** — The engine answers one question: "Given these players and this situation, what happened?" Everything else is downstream.

4. **Determinism** — Outcome logic stays pure. No presentation concerns leak into game mechanics.

### What the Engine Produces

For each play, the engine outputs:
- Play type (run, pass, punt, field goal, etc.)
- Primary actors (ball carrier, passer, receiver, tackler, etc.)
- Outcome (yards gained/lost, touchdown, turnover, penalty, etc.)
- Player state changes (fatigue, injury, stats)
- Game state changes (down, distance, field position, clock, score)

The frontend or a separate rendering layer can then dress this up with formation names, play diagrams, announcer-style descriptions, or whatever presentation is desired.

### Coaching Influence

Coaches and staff affect simulation through:
- Scheme fit (players performing better/worse in certain systems)
- Play-calling tendencies (run/pass ratio, aggressiveness on 4th down, etc.)
- In-game adjustments (halftime changes, responding to opponent tendencies)
- Player development (offseason progression, practice effects)

These are modeled as modifiers to player effectiveness, not as explicit play selection.

### Player Attributes That Drive Outcomes

Physical: Speed, acceleration, strength, agility, stamina, durability

Skill: Throwing accuracy, catching, route running, blocking, tackling, coverage, pass rush, ball carrier vision

Mental: Intelligence (reads, adjustments), discipline (penalties, assignments), awareness (reaction to the play), composure (pressure situations)

The engine uses these to resolve each play. Attribute weights vary by play type and player role.

## State Machine

The simulation state machine is Scott's domain. It manages:
- Game flow (kickoff → drives → scoring → halftime → etc.)
- Play resolution
- Clock management
- Penalty enforcement
- Turnover handling
- End-of-game scenarios

When working near the state machine, consult Scott before making changes.

## Development Guidelines

- Follow existing patterns in the codebase
- Where rules are able to diverge due to different leagues (NFL vs NCAA vs XFL), or rulesets for regular season or playoffs, use a provider model to encapsulate the differences elegantly, so we can easily inject the appropriate ruleset for the game.

### Centralized Constants Pattern

All probability values, thresholds, and configuration constants **must** be defined in `Simulation/Configuration/GameProbabilities.cs`. Do not create separate constants files scattered throughout the codebase.

**Structure:** Use nested static classes to organize constants by domain:
```csharp
public static class GameProbabilities
{
    public static class Passing { /* completion rates, interception chances, etc. */ }
    public static class Rushing { /* tackle break rates, big run chances, etc. */ }
    public static class FourthDown { /* go-for-it probabilities, field position thresholds, etc. */ }
    public static class Timeouts { /* timeout thresholds, ice kicker probability, etc. */ }
    // ... other domains
}
```

**Usage:** Reference constants as `GameProbabilities.DomainName.CONSTANT_NAME`

**Existing domains:** Passing, Rushing, Turnovers, FieldGoals, Kickoffs, Punts, GameDecisions, FourthDown, Timeouts

When adding new simulation logic that requires configuration values, add a new nested class to `GameProbabilities.cs` rather than creating a standalone constants file.

### Decision vs Mechanic Separation

Separate **decision logic** ("should we do X?") from **game mechanics** ("X was chosen, execute it"):

```
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│      CONTEXT        │     │  DECISION ENGINE    │     │    GAME MECHANIC    │
│                     │     │                     │     │                     │
│  "What's the        │────▶│  "What should       │────▶│  "Do it.            │
│   situation?"       │     │   we do?"           │     │   Update state."    │
│                     │     │                     │     │                     │
│  Game state,        │     │  Probabilistic,     │     │  Pure rule          │
│  player attributes, │     │  situational,       │     │  enforcement,       │
│  score, time, etc.  │     │  coach tendencies   │     │  deterministic      │
└─────────────────────┘     └─────────────────────┘     └─────────────────────┘
```

**This pattern applies universally to all game mechanisms:**

| Mechanism | Context | Decision | Mechanic |
|-----------|---------|----------|----------|
| Timeouts | Game situation, score, time | Call timeout? | Execute timeout |
| Penalties | Play result + fouls called | Accept/decline? | Enforce penalty |
| Fourth down | Field position, score, time | Go/punt/FG? | Execute play type |
| Play calling | Down, distance, situation | Run/pass? | Execute play |
| Onside kick | Score, time remaining | Onside? | Execute kick type |
| Two-point | Score differential, time | 2pt/PAT? | Execute conversion |
| Fair catch | Punt hang time, coverage | Fair catch? | Signal/return |

**File organization:**
- **Context structs** go in `Simulation/Decision/` (e.g., `TimeoutContext`)
- **Decision engines** go in `Simulation/Decision/` (e.g., `TimeoutDecisionEngine`)
- **Game mechanics** go in `Simulation/Mechanics/` (e.g., `TimeoutMechanic`)

**Reference implementations:** `TimeoutDecisionEngine` + `TimeoutMechanic`

### Other Guidelines

- Ask Scott before modifying core simulation logic
- Keep simulation concerns separate from presentation concerns
- Do not add formation/play-name logic to the engine
- Test edge cases (overtime, safeties, two-point conversions, end-of-half scenarios)

## Testing Guidelines

### Core Principle: Deterministic Tests

**ALL tests must be deterministic.** Tests must produce consistent results every time they run.

**Forbidden patterns:**
- Random values without fixed seeds
- Conditional assertions based on random outcomes
- Time-dependent assertions without mocking
- Tests that depend on external state

### Required: Use Fluent RNG Methods

**ALL unit tests MUST use `TestFluentSeedableRandom` with fluent methods.**

Located in: `tests/Gridiron.Engine.Tests/Helpers/TestFluentSeedableRandom.cs`

```csharp
// ✅ CORRECT - Use fluent methods with descriptive names
var rng = new TestFluentSeedableRandom()
    .PassProtectionCheck(0.5)      // Protection holds
    .PassCompletionCheck(0.3)      // Pass completes
    .AirYards(15)                  // 15 yards in air
    .YACOpportunityCheck(0.8)      // No YAC opportunity
    .ImmediateTackleYards(1);      // 1 yard after catch

// ❌ WRONG - Do not use raw arrays or generic methods
var rng = new TestFluentSeedableRandom();
rng.__NextDouble = new double[] { 0.5, 0.3, 0.8 };  // DEPRECATED
rng.NextDouble(0.5).NextDouble(0.3);                 // Not descriptive
```

**Why fluent methods:**
1. **Self-documenting** - Each method name explains what the value controls
2. **Validated** - Methods validate ranges and provide helpful error messages
3. **Deterministic** - Values are consumed in order, making tests reproducible
4. **Debuggable** - Clear error messages when values run out

**Common fluent methods:**
| Method | Range | Purpose |
|--------|-------|---------|
| `PassProtectionCheck(double)` | 0.0-1.0 | < ~0.75 = protection holds |
| `PassCompletionCheck(double)` | 0.0-1.0 | < completion% = complete |
| `InterceptionOccurredCheck(double)` | 0.0-1.0 | < INT% = intercepted |
| `RunBlockingCheck(double)` | 0.0-1.0 | Success of run blocking |
| `FumbleCheck(double)` | 0.0-1.0 | < ~1-3% = fumble occurs |
| `FieldGoalMakeCheck(double)` | 0.0-1.0 | < make% = kick good |
| `AirYards(int)` | -10 to 100 | Pass distance in air |
| `RunYards(int)` | -10 to 99 | Yards gained on run |

See `TestFluentSeedableRandom.cs` for the full list with documentation.

## Git Workflow

> ⛔ **ABSOLUTE RULE: NEVER COMMIT OR PUSH DIRECTLY TO MASTER OR MAIN** ⛔
>
> This is non-negotiable. Violations break CI/CD and require manual cleanup.

### Required Process for ALL Changes

1. **Create a feature branch** from master:
   ```bash
   git checkout master
   git pull
   git checkout -b feature/your-change-description
   ```

2. **Make changes and commit** to the feature branch:
   ```bash
   git add .
   git commit -m "Description of change"
   ```

3. **Push the feature branch** to origin:
   ```bash
   git push -u origin feature/your-change-description
   ```

4. **Create a Pull Request** for Scott to review

5. **Wait for approval** — Scott will merge after CI passes

### Branch Naming
- `feature/` — New features or enhancements
- `fix/` — Bug fixes
- `chore/` — Maintenance, refactoring, docs

This applies to ALL changes, no matter how small—even single-line fixes.

### Checkpoint Commits

**Never miss an opportunity to commit a checkpoint.** When you reach a stable state (tests pass, feature partially complete, architecture in place), commit it immediately. These checkpoints serve as rollback points if subsequent work goes sideways.

Good checkpoint moments:
- Architecture/scaffolding complete, before implementation details
- Tests written and passing, before refactoring
- One component complete, before starting the next
- After any significant milestone within a feature

Checkpoint commits should:
- Have clear commit messages describing what's complete
- Pass all existing tests
- Be atomic (don't mix unrelated changes)

## When Uncertain

Ask Scott. He is precise in his requirements. Do not assume or estimate.
