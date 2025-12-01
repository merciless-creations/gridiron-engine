# Statistical Targets

Target distributions derived from NFL data 2020-2024. These are the **goals** for simulation output — what the engine should produce when aggregated across 1000+ games with normally-distributed player abilities.

**Acceptable variance:** ±5% on primary metrics.

**Validation:** See issue #30 for the framework to automatically validate simulation output against these targets.

## Rushing

| Metric | Target | Notes |
|--------|--------|-------|
| Yards per carry (league avg) | 4.3 | RBs specifically ~4.3, QBs higher (~5.5 for mobile QBs) |
| Negative plays | ~15% | Runs for loss or no gain |
| 10+ yard runs | ~12% | Explosive plays |
| 20+ yard runs | ~3% | Big plays |
| Fumble rate (per carry) | ~0.8% | ~44% of fumbles are lost |

### Rushing Yard Distribution

Approximate probability mass:
```
-3 to -1: 8%
0: 7%
1-2: 20%
3-4: 25%
5-6: 18%
7-9: 12%
10-14: 6%
15-19: 2.5%
20+: 1.5%
```

Use a right-skewed distribution (e.g., shifted exponential or gamma) with mode ~3.

## Passing

| Metric | Target | Notes |
|--------|--------|-------|
| Completion percentage | 64-67% | League has trended upward |
| Yards per attempt | 7.0-7.5 | Includes incompletes and sacks |
| Yards per completion | 11.0-12.0 | |
| Interception rate | 2.0-2.5% | Per pass attempt |
| Sack rate | 6.0-7.0% | Per dropback |
| TD rate | 4.5-5.0% | Per pass attempt |

### Completion by Depth

| Depth | Completion % | Avg YAC |
|-------|--------------|---------|
| Behind LOS | 78-82% | 5-7 |
| Short (1-10) | 68-72% | 4-5 |
| Medium (11-20) | 52-58% | 3-4 |
| Deep (21+) | 38-45% | 2-3 |

### Air Yards Distribution

When a pass is attempted, target depth distribution:
```
Behind LOS (screens): 12%
Short (1-10): 48%
Medium (11-20): 28%
Deep (21+): 12%
```

## Turnovers

| Metric | Target | Notes |
|--------|--------|-------|
| Turnovers per game (team) | 1.2-1.4 | Trending downward over time |
| Interceptions per game | 0.7-0.9 | |
| Fumbles lost per game | 0.5-0.6 | |
| Fumble recovery rate | 44% | Offense recovers 56% |

### Turnover Context Modifiers

Turnovers are not uniform. Increase probability when:
- Player is fatigued (stamina < 60%)
- Player discipline is low
- Pressure on QB (rush vs. protection mismatch)
- Ball carrier hit by multiple defenders
- Weather conditions (if modeled)

Decrease probability when:
- Player has high ball security attribute
- Short, safe passes
- Handoffs to RBs with high discipline

## Penalties

| Metric | Target | Notes |
|--------|--------|-------|
| Penalties per game (team) | 5.5-6.5 | Accepted penalties |
| Penalty yards per game | 48-55 | |
| Penalties per play | ~4.5% | Per 100 plays |

### Penalty Type Distribution

| Penalty | % of Total | Typical Yards |
|---------|------------|---------------|
| Offensive holding | 22% | 10 |
| False start | 14% | 5 |
| Defensive pass interference | 10% | Spot |
| Defensive holding | 8% | 5 + auto 1st |
| Defensive offside | 8% | 5 |
| Unnecessary roughness | 6% | 15 |
| Illegal contact | 4% | 5 + auto 1st |
| Delay of game | 4% | 5 |
| Other | 24% | Varies |

### Penalty Triggers

Model penalties based on player discipline attribute. Low discipline = higher penalty probability. Specific triggers:
- **Holding**: Offensive line losing blocking matchup
- **False start**: High-pressure situations, road games
- **DPI**: Corner beaten on deep route, attempts to recover
- **Offside**: Defensive lineman with low discipline

## Kicking

### Field Goals by Distance

| Distance | Make % | Notes |
|----------|--------|-------|
| < 30 yards | 97-99% | Near automatic |
| 30-39 yards | 92-95% | |
| 40-49 yards | 82-86% | |
| 50-54 yards | 68-75% | Improving over time |
| 55-59 yards | 55-65% | |
| 60+ yards | 40-50% | Rare attempts |

### Extra Points

| Metric | Target |
|--------|--------|
| XP make % | 94-95% |

### Punting

| Metric | Target |
|--------|--------|
| Gross yards per punt | 45-47 |
| Net yards per punt | 41-43 |
| Touchback % | 8-12% |
| Inside 20 % | 38-42% |

## Down Conversions

### Third Down

| Distance | Conversion % |
|----------|--------------|
| 3rd & 1 | 68-72% |
| 3rd & 2-3 | 55-60% |
| 3rd & 4-6 | 42-48% |
| 3rd & 7-9 | 32-38% |
| 3rd & 10+ | 22-28% |
| Overall | 38-42% |

### Fourth Down

| Metric | Target | Notes |
|--------|--------|-------|
| Conversion % | 52-58% | When attempted |
| Attempts per game | 1.5-2.0 | Trending upward |

Fourth down attempts are situationally driven:
- More likely: 4th & short, losing, late game, aggressive coach
- Less likely: Winning, own territory, conservative coach

## Game Flow

| Metric | Target |
|--------|--------|
| Plays per game (both teams) | 130-140 |
| Plays per drive | 5.5-6.5 |
| Time per play | 25-30 seconds |
| Total yards per game (team) | 320-350 |
| Points per game (team) | 21-24 |

### Scoring Distribution

| Outcome | % of Drives |
|---------|-------------|
| Touchdown | 20-24% |
| Field goal | 12-15% |
| Punt | 42-48% |
| Turnover | 8-12% |
| End of half | 4-6% |
| Downs | 3-5% |

## Red Zone

| Metric | Target |
|--------|--------|
| TD % (when reaching RZ) | 55-60% |
| FG % (when reaching RZ) | 25-30% |
| Turnover % (in RZ) | 5-8% |

## Special Teams

### Kickoff Returns

| Metric | Target |
|--------|--------|
| Touchback % | 55-65% | Varies by rule changes |
| Return average | 22-25 yards |
| Return TD rate | ~0.5% |

### Punt Returns

| Metric | Target |
|--------|--------|
| Fair catch % | 45-55% |
| Return average | 8-10 yards |
| Return TD rate | ~1% |

## Injury Rates

| Severity | Per Game (team) | Notes |
|----------|-----------------|-------|
| Minor (1-2 weeks) | 0.3-0.5 | |
| Moderate (3-6 weeks) | 0.1-0.2 | |
| Severe (season) | 0.02-0.05 | |

Injury probability increases with:
- Player fatigue
- High-impact collisions
- Low durability attribute
- Certain play types (kickoff returns historically highest)

---

# Validation

These targets are validated by running large-scale simulations and comparing aggregate output.

## How to Use This Document

1. **During development** — Reference targets when tuning probabilities in `GameProbabilities.cs`
2. **After changes** — Run validation to ensure statistics remain realistic
3. **Debugging** — If a metric is off, trace back to the relevant skills check

## Current Status

> **Note:** A formal validation framework is not yet implemented. See issue #30.

Once implemented, this section will document:
- Which metrics are currently within tolerance
- Which need tuning
- Historical drift over engine versions

---

# Related Issues

- **#27** - Logarithmic modifier curves (will affect all statistics)
- **#28** - Expanded attributes (will enable finer-grained targeting)
- **#30** - Statistical validation framework
