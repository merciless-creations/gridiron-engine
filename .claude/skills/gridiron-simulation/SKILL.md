---
name: gridiron-simulation
description: Statistical simulation engine skill for the Gridiron NFL management game. Use this skill when implementing play-by-play simulation logic, outcome calculations, player attribute effects, coaching modifiers, or rule variations. Ensures simulation outputs match realistic NFL statistical distributions. Contains reference files for statistical targets, attribute mappings, and pluggable rule systems (NFL Regular Season, NFL Playoffs, NCAA).
---

# Gridiron Simulation Engine

This skill guides development of the play-by-play simulation engine to produce statistically realistic NFL outcomes.

## Core Principle

The engine produces outcomes that, when aggregated across a season of games with normally-distributed player abilities and coaching, match real NFL statistical profiles from 2020-2024.

## Reference Files

Before implementing simulation logic, read the relevant reference file:

- **[references/statistical-targets.md](references/statistical-targets.md)** — Target distributions for all play outcomes (rushing, passing, turnovers, penalties, kicking, downs)
- **[references/attribute-mappings.md](references/attribute-mappings.md)** — How player attributes affect outcome probabilities
- **[references/rule-providers.md](references/rule-providers.md)** — Pluggable rule systems for NFL Regular Season, NFL Playoffs, and NCAA

## Simulation Flow

1. **Input**: Game situation (down, distance, field position, score, time) + 22 players on field + coaching context
2. **Play Type Selection**: Weighted by situation, coaching tendencies, game state
3. **Outcome Resolution**: Player attributes + coaching modifiers → probability distributions → sampled outcome
4. **State Update**: Yards, stats, fatigue, clock, score, injuries

## Key Implementation Rules

### Attribute Application

Player attributes create probability modifiers, not deterministic outcomes. A 99-rated attribute does not guarantee success. See `references/attribute-mappings.md` for modifier curves.

### Distribution Sampling

Do not use uniform random. Most football outcomes follow:
- **Rushing yards**: Right-skewed with mode around 3, long tail to 20+, negative values for losses
- **Passing yards**: Bimodal (incomplete = 0, completions cluster by route depth)
- **Turnovers**: Rare events with situational triggers (pressure, fatigue, discipline)

### Coaching Influence

Coaches modify outcomes through:
- Scheme fit (attribute effectiveness multipliers)
- Tendencies (play type selection weights)
- Adjustments (in-game modifier changes based on opponent tendencies)

### Fatigue

Fatigue accumulates per snap and degrades attributes. Recovery occurs during drives on sideline. Starters playing 90%+ of snaps should show measurable late-game degradation.

### Rule Providers

The engine uses a provider pattern for rules that differ across league types. Current providers:

| Provider | Use Case |
|----------|----------|
| `NFLRegularSeasonRules` | Default for Gridiron leagues |
| `NFLPlayoffRules` | Postseason games |
| `NCAACollegeRules` | If college mode is ever added |

Key differences are in overtime handling. See `references/rule-providers.md`.

## Validation

After implementing changes to simulation logic, validate by running 1000+ simulated games and comparing aggregate statistics to targets in `references/statistical-targets.md`. Acceptable variance is ±5% on primary metrics.

## What NOT to Model

Do not add complexity for:
- Formation names
- Specific play names
- Audibles or pre-snap reads
- Motion/shifts
- Broadcast-style presentation

These are rendering concerns. The engine outputs what happened; presentation adds the flavor.
