# Attribute Mappings

How player attributes translate to outcome probability modifiers.

## Core Principle

Attributes create **probability modifiers**, not deterministic outcomes. A 99-rated attribute shifts the probability curve favorably but does not guarantee success. A 50-rated attribute represents league average.

## Attribute Scale

| Rating | Meaning | Modifier Range |
|--------|---------|----------------|
| 0-29 | Below replacement | -20% to -10% |
| 30-49 | Below average | -10% to -2% |
| 50 | League average | 0% (baseline) |
| 51-69 | Above average | +2% to +10% |
| 70-84 | Pro Bowl level | +10% to +18% |
| 85-94 | All-Pro level | +18% to +25% |
| 95-99 | Elite/HOF | +25% to +30% |

## Current Modifier Formula

The simulation uses **linear divisors** for attribute modifiers:

```csharp
// Example from PassCompletionSkillsCheck.cs
completionProbability = BASE_PROBABILITY + (skillDifferential / SKILL_DENOMINATOR);
```

Each point of skill differential has equal impact. Constants are defined in `GameProbabilities.cs`.

> **Future Enhancement:** Issue #27 tracks implementing logarithmic curves for diminishing returns at extremes.

---

## Player Attributes

Defined in `Player.cs`. All ratings are 0-100 scale.

### Physical Attributes

| Attribute | Used In Simulation | Notes |
|-----------|-------------------|-------|
| Speed | Yes | Pursuit, breakaway, separation, returns |
| Strength | Yes | Blocking matchups, tackling, breaking tackles |
| Agility | Yes | Elusiveness, YAC potential |
| Awareness | Yes | Decision-making, fumble recovery, ball security proxy |
| Fragility | Yes | Injury probability (higher = more injury prone, default 50) |

### Skill Attributes

| Attribute | Positions | Used In Simulation |
|-----------|-----------|-------------------|
| Passing | QB | Completion probability, interception avoidance |
| Catching | WR/TE/RB | Catch probability |
| Rushing | RB/QB | Ball carrier effectiveness, yards gained |
| Blocking | OL/TE/FB | Run lanes, pass protection |
| Tackling | DL/LB/S/CB | Tackle success, preventing YAC |
| Coverage | CB/S/LB | Pass defense, interception chance |
| Kicking | K/P | FG accuracy, punt distance, kickoff distance |

### Mental Attributes

| Attribute | Used In Simulation | Notes |
|-----------|-------------------|-------|
| Morale | Not yet | Defined in Player.cs, future use |
| Discipline | Not yet | Defined in Player.cs, future use for penalties |

### Development Attributes

| Attribute | Notes |
|-----------|-------|
| Health | Current health level (0-100) |
| Potential | Player ceiling for development |
| Progression | Skill development rate |

> **Future Enhancement:** Issue #28 tracks expanding to 30+ granular attributes.

---

## Matchup Formulas

These are the actual formulas used in the SkillsCheck classes.

### Pass Completion (`PassCompletionSkillsCheck.cs`)

```csharp
passingPower = (QB.Passing * 2 + QB.Awareness) / 3.0
receivingPower = (Receiver.Catching + Receiver.Speed + Receiver.Agility) / 3.0
coveragePower = Average(Defender.Coverage + Defender.Speed + Defender.Awareness) / 3.0
offensivePower = (passingPower + receivingPower) / 2.0
skillDifferential = offensivePower - coveragePower

completionProbability = 0.60 + (skillDifferential / 250.0)
if pressured: completionProbability -= 0.20
// Clamped to 0.25 - 0.85
```

### Run Yards (`RunYardsSkillsCheckResult.cs`)

```csharp
blockingPower = Average(Blocker.Blocking)
ballCarrierPower = (BallCarrier.Rushing * 2 + BallCarrier.Speed + BallCarrier.Agility) / 4.0
offensivePower = (blockingPower + ballCarrierPower) / 2.0
defensivePower = Average((Defender.Tackling + Defender.Strength + Defender.Speed) / 3.0)

skillDifferential = offensivePower - defensivePower
baseYards = 3.0 + (skillDifferential / 20.0)
// Random variance applied
```

### Tackle Break (`TackleBreakSkillsCheck.cs`)

```csharp
ballCarrierPower = (BallCarrier.Rushing + BallCarrier.Strength + BallCarrier.Agility) / 3.0
tacklerPower = Average((Tackler.Tackling + Tackler.Strength + Tackler.Speed) / 3.0)

skillDifferential = ballCarrierPower - tacklerPower
breakProbability = 0.25 + (skillDifferential / 250.0)
// Clamped to 0.05 - 0.50
```

### Fumble (`FumbleOccurredSkillsCheck.cs`)

```csharp
// Base rates from GameProbabilities.Turnovers:
// - 1.5% normal plays
// - 12% on sacks
// - 2.5% on returns

carrierSecurity = BallCarrier.Awareness
securityFactor = 1.0 - (carrierSecurity / 200.0)  // 0.5x to 1.0x

defenderPressure = (Defender.Strength + Defender.Speed) / 2.0
pressureFactor = 0.5 + (defenderPressure / 200.0)

fumbleProbability = baseProbability * securityFactor * pressureFactor
// Gang tackle: 1.3x multiplier for 3+ defenders
// Clamped to 0.003 - 0.25
```

### Injury (`InjuryOccurredSkillsCheck.cs`)

```csharp
fragilityFactor = 0.5 + (Player.Fragility / 100.0)  // 0.5x to 1.5x
probability = baseProbability * fragilityFactor * positionMultiplier * contactMultiplier
```

---

## Design Principles

### Ceiling and Floor

To prevent absurd outcomes:

- No play has > 99% success probability
- No play has < 1% success probability
- Attribute modifiers are capped at ±35%
- Combined modifiers are capped at ±50%

Even the best QB throwing to the best WR against a practice squad corner can still throw an incompletion. Even a backup RB can occasionally break a 50-yard run.

### Configuration

All probability constants are centralized in `GameProbabilities.cs` for easy tuning.

---

## Related Issues

- **#27** - Implement logarithmic modifier curves
- **#28** - Expand player attribute system
- **#36** - Implement fatigue system
- **#37** - Implement composure system
- **#39** - Implement coaching scheme fit modifiers
