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

## Modifier Curve

Use a logarithmic curve to prevent extreme ratings from breaking the simulation:

```
modifier = sign(rating - 50) * log(1 + |rating - 50| / 10) * 0.15
```

This produces diminishing returns at extremes. A jump from 90→99 matters less than 50→59.

> **Implementation Note:** Issue #27 tracks updating the codebase to use this logarithmic formula. Current implementation uses linear divisors.

---

# Current Implementation

The following attributes are implemented in `Player.cs` and used by the simulation engine.

## Physical Attributes

| Attribute | Range | Used In Simulation | Notes |
|-----------|-------|-------------------|-------|
| Speed | 0-100 | Yes | Pursuit, breakaway, separation, returns |
| Strength | 0-100 | Yes | Blocking matchups, tackling, breaking tackles |
| Agility | 0-100 | Yes | Elusiveness, YAC potential |
| Awareness | 0-100 | Yes | Decision-making, fumble recovery, reads |
| Fragility | 0-100 | Yes | Injury probability (higher = more injury prone, default 50) |

## Skill Attributes

| Attribute | Range | Positions | Used In Simulation |
|-----------|-------|-----------|-------------------|
| Passing | 0-100 | QB | Completion probability, interception avoidance |
| Catching | 0-100 | WR/TE/RB | Catch probability |
| Rushing | 0-100 | RB/QB | Ball carrier effectiveness, yards gained |
| Blocking | 0-100 | OL/TE/FB | Run lanes, pass protection |
| Tackling | 0-100 | DL/LB/S/CB | Tackle success, preventing YAC |
| Coverage | 0-100 | CB/S/LB | Pass defense, interception chance |
| Kicking | 0-100 | K/P | FG accuracy, punt distance, kickoff distance |

## Mental Attributes

| Attribute | Range | Used In Simulation | Notes |
|-----------|-------|-------------------|-------|
| Morale | 0-100 | Not yet | Defined in Player.cs, future use |
| Discipline | 0-100 | Not yet | Defined in Player.cs, future use for penalties |

## Development Attributes

| Attribute | Range | Notes |
|-----------|-------|-------|
| Health | 0-100 | Current health level |
| Potential | 0-100 | Player ceiling for development |
| Progression | 0-100 | Skill development rate |

## Current Matchup Formulas

These are the actual formulas used in the SkillsCheck classes:

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
// Random variance: -15 to +10 yards
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
// Base rates: 1.5% normal, 12% on sacks, 2.5% on returns
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

# Target Design (Future)

The following expanded attribute system is the target for deeper simulation. See issue #28.

## Physical Attributes (Target)

| Attribute | Affects |
|-----------|---------|
| Speed | Pursuit angles, breakaway probability, separation on routes |
| Acceleration | First-step advantage, closing speed |
| Strength | Blocking matchups, tackling, breaking tackles |
| Agility | Elusiveness, change of direction |
| Stamina | Fatigue accumulation rate |
| Durability | Injury probability (inverse of current Fragility) |

## Skill Attributes - Offense (Target)

| Attribute | Affects |
|-----------|---------|
| Throw Power | Deep ball completion %, ball velocity |
| Throw Accuracy (Short) | Completion % on short routes |
| Throw Accuracy (Medium) | Completion % on intermediate routes |
| Throw Accuracy (Deep) | Completion % on deep routes |
| Throw Under Pressure | Accuracy modifier when pressured |
| Catching | Catch probability baseline |
| Catch in Traffic | Catch modifier when contested |
| Route Running | Separation from defender |
| Release | Beating press coverage |
| Run Blocking | Run lane creation |
| Pass Blocking | Sack prevention |
| Ball Carrier Vision | Yards after contact, finding holes |
| Elusiveness | Broken tackle probability |
| Trucking | Yards after contact vs. smaller defenders |
| Stiff Arm | Broken tackle alternative |
| Ball Security | Fumble probability reduction |

## Skill Attributes - Defense (Target)

| Attribute | Affects |
|-----------|---------|
| Tackling | Solo tackle probability, missed tackles |
| Hit Power | Fumble forcing, YAC reduction |
| Block Shedding | Defeating blocks |
| Pursuit | Angle to ball carrier |
| Play Recognition | Reading play type, reacting |
| Man Coverage | Tight coverage on receiver |
| Zone Coverage | Break on ball in zone |
| Press | Disrupting release |
| Pass Rush | Pressure rate, sack probability |
| Finesse Moves | Pass rush vs. power blockers |
| Power Moves | Pass rush vs. agile blockers |

## Mental Attributes (Target)

| Attribute | Affects |
|-----------|---------|
| Intelligence | Pre-snap reads, audible effectiveness, adjustment speed |
| Discipline | Penalty probability, assignment adherence |
| Awareness | Reaction to developing plays |
| Composure | Performance in high-pressure situations |

## Target Matchup Resolution

When the expanded attribute system is implemented, matchups will resolve with more granularity:

### Passing Matchup (Target)

```
receiver_separation = (RouteRunning + Speed) / 2
defender_coverage = (ManCoverage + Speed) / 2
separation_differential = receiver_separation - defender_coverage

If separation_differential > 0:
  Easier catch (receiver open)
Else:
  Contested catch (use CatchInTraffic)
```

### Blocking Matchup (Target)

```
blocker_strength = (RunBlocking or PassBlocking) + Strength
defender_shed = BlockShedding + Strength
blocking_differential = blocker_strength - defender_shed

If blocking_differential > 0:
  Block holds
Else:
  Defender penetrates (timing based on differential)
```

### Tackle Attempt (Target)

```
tackle_probability = Tackling + (Pursuit * pursuit_angle_factor)
evasion = Elusiveness + BallCarrierVision

If tackle_probability > evasion:
  Tackle made
Else:
  Broken tackle (additional yards)
```

---

# Planned Systems (Not Yet Implemented)

The following systems are designed but not yet built into the simulation.

## Fatigue System

> **Status:** Not implemented. No stamina tracking currently exists.

```
effective_attribute = base_attribute * (stamina_remaining / 100)
```

At 70% stamina, a 90-rated attribute performs like 63. This matters for late-game drives.

## Composure System

> **Status:** Not implemented. No Composure attribute exists.

In high-pressure situations (4th quarter, close game, red zone):

```
effective_attribute = base_attribute * (0.8 + Composure * 0.004)
```

A player with 50 Composure performs at 100%. A player with 25 Composure performs at 90%. A player with 75 Composure performs at 110%.

## Weather Effects

> **Status:** Not implemented. No weather system exists.

| Condition | Passing | Kicking | Fumble Risk |
|-----------|---------|---------|-------------|
| Clear | 100% | 100% | 100% |
| Rain | 90% | 92% | 120% |
| Snow | 85% | 85% | 130% |
| Wind (10+ mph) | 95% | -1% per mph | 100% |
| Cold (< 32°F) | 97% | 95% | 105% |

## Coaching Modifiers

> **Status:** Not implemented. Coach entity exists in `Coach.cs` but is not used in simulation.

### Scheme Fit

Players perform better in schemes that match their attributes:

| Scheme Type | Favored Attributes |
|-------------|-------------------|
| West Coast | Short/Medium Accuracy, Route Running, Catching |
| Air Raid | Deep Accuracy, Speed, Throw Power |
| Power Run | Strength, Run Blocking, Trucking |
| Zone Run | Agility, Ball Carrier Vision, Athleticism |
| 3-4 Defense | OLB Pass Rush, NT Strength |
| 4-3 Defense | DE Pass Rush, Speed |
| Cover 2 | Zone Coverage, Speed, Pursuit |
| Man Press | Man Coverage, Press, Speed |

A player with high scheme-fit attributes gets +5% effectiveness. Mismatch gets -5%.

### Play-Calling Tendencies

Coaches have tendency weights that influence play selection:

```
run_pass_ratio: 0.0 to 1.0 (0.5 = balanced)
aggressiveness: 0.0 to 1.0 (4th down decisions, deep passes)
tempo: 0.0 to 1.0 (hurry-up frequency)
```

### In-Game Adjustments

Coaches with high adjustment ability can modify tendencies mid-game:

```
adjustment_ability = CoachIntelligence * 0.01
tendency_shift = base_tendency + (adjustment_ability * opponent_tendency_differential)
```

---

# Design Principles

## Target Aggregation

When the expanded attribute system is implemented, multiple attributes will affect outcomes using weighted averages:

### Run Play (Target)
```
success_factors = {
  RB.BallCarrierVision: 0.25,
  RB.Speed: 0.15,
  RB.Elusiveness: 0.15,
  OL_avg.RunBlocking: 0.30,
  DL_avg.BlockShedding: -0.15  # Negative = defender attribute hurts offense
}
```

### Pass Play (Target)
```
success_factors = {
  QB.Accuracy[depth]: 0.25,
  WR.RouteRunning: 0.20,
  WR.Catching: 0.15,
  CB.Coverage: -0.20,
  OL_avg.PassBlocking: 0.10,
  DL_avg.PassRush: -0.10
}
```

## Ceiling and Floor

To prevent absurd outcomes (this principle applies to both current and future implementations):

- No play has > 99% success probability
- No play has < 1% success probability
- Attribute modifiers are capped at ±35%
- Combined modifiers are capped at ±50%

Even the best QB throwing to the best WR against a practice squad corner can still throw an incompletion. Even a backup RB can occasionally break a 50-yard run.

---

# Related Issues

- **#27** - Implement logarithmic modifier curves
- **#28** - Expand player attribute system
