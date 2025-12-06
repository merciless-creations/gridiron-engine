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
- Where rules are able to diverge due to different leagues (NFL vs NCAA vs XFL), or rulesets for regular season or playoffs, use a provider model to encapsulate the differences elegantly, so we can easily inject the apprpriate rulest for the game.

### Centralized Constants Pattern

All probability values, thresholds, and configuration constants **must** be defined in `Simulation/Configuration/GameProbabilities.cs`. Do not create separate constants files scattered throughout the codebase.

**Structure:** Use nested static classes to organize constants by domain:
```csharp
public static class GameProbabilities
{
    public static class Passing { /* completion rates, interception chances, etc. */ }
    public static class Rushing { /* tackle break rates, big run chances, etc. */ }
    public static class FourthDown { /* go-for-it probabilities, field position thresholds, etc. */ }
    // ... other domains
}
```

**Usage:** Reference constants as `GameProbabilities.DomainName.CONSTANT_NAME`

**Existing domains:** Passing, Rushing, Turnovers, FieldGoals, Kickoffs, Punts, GameDecisions, FourthDown

When adding new simulation logic that requires configuration values, add a new nested class to `GameProbabilities.cs` rather than creating a standalone constants file.

### Other Guidelines

- Ask Scott before modifying core simulation logic
- Keep simulation concerns separate from presentation concerns
- Do not add formation/play-name logic to the engine
- Test edge cases (overtime, safeties, two-point conversions, end-of-half scenarios)

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

## When Uncertain

Ask Scott. He is precise in his requirements. Do not assume or estimate.
