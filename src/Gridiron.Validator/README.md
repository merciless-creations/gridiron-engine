# Gridiron.Validator

A CLI tool for validating simulation statistics against NFL statistical targets.

## Purpose

This tool runs multiple game simulations and compares aggregate statistics against real NFL averages. It helps identify areas where the simulation engine needs tuning to produce realistic outcomes.

## Usage

```bash
# Run with default 1000 games
dotnet run --project src/Gridiron.Validator

# Run with custom game count
dotnet run --project src/Gridiron.Validator -- 500

# Run compiled binary directly
./Gridiron.Validator 1000
```

## Command Line Arguments

| Argument | Description | Default |
|----------|-------------|---------|
| `[game_count]` | Number of games to simulate | 1000 |

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | All statistical targets passed |
| 1 | One or more targets failed |
| 2 | Test data files not found |
| 3 | Unexpected error |

Exit code 1 is useful for CI pipelines to flag when simulation changes cause statistical drift.

## Output

The tool produces a formatted report showing:

- Games simulated and duration
- Overall pass/fail rate
- Results grouped by category (Rushing, Passing, etc.)
- For each metric: target range, actual value, and deviation percentage

Example output:
```
╔══════════════════════════════════════════════════════════════════════════════╗
║                    GRIDIRON STATISTICAL VALIDATION REPORT                    ║
╚══════════════════════════════════════════════════════════════════════════════╝

  Games Simulated: 1,000
  Duration: 53.2 seconds (53.2 ms/game)
  Generated: 2024-01-15 14:30:00 UTC

  Overall: 15/20 passed (75.0%)

  ┌─ Rushing ─────────────────────────────────────────────────────────────
  │
  │ [✓] Yards per carry               Target: 4.0-4.6  Actual: 4.32  (+0.5%)
  │ [✗] Negative plays %              Target: 13.0-17.0  Actual: 22.50  (+50.0%)
  ...
```

## Statistical Targets

Targets are defined in `StatisticalTargets.cs` and sourced from `.claude/skills/gridiron-simulation/references/statistical-targets.md`.

Each target includes:
- **Category**: Grouping (Rushing, Passing, etc.)
- **Metric**: What's being measured
- **MinTarget / MaxTarget**: Acceptable range based on NFL data
- **Tolerance**: Additional buffer (default 5%)

### Current Targets

| Category | Metric | Target Range |
|----------|--------|--------------|
| Rushing | Yards per carry | 4.0 - 4.6 |
| Rushing | Negative plays % | 13 - 17% |
| Rushing | 10+ yard runs % | 10 - 14% |
| Rushing | 20+ yard runs % | 2 - 4% |
| Rushing | Fumble rate % | 0.5 - 1.1% |
| Passing | Completion % | 64 - 67% |
| Passing | Yards per attempt | 7.0 - 7.5 |
| Passing | Interception rate % | 2.0 - 2.5% |
| Passing | TD rate % | 4.5 - 5.0% |
| Turnovers | Per game (team) | 1.2 - 1.4 |
| Penalties | Per game (team) | 5.5 - 6.5 |
| Penalties | Yards per game | 48 - 55 |
| Third Down | Conversion % | 38 - 42% |
| Fourth Down | Conversion % | 52 - 58% |
| Game Flow | Plays per game | 130 - 140 |
| Game Flow | Points per game (team) | 21 - 24 |
| Punting | Gross yards per punt | 45 - 47 |
| Kickoffs | Touchback % | 55 - 65% |
| Overtime | Overtime rate % | 5 - 7% |
| Injuries | Per game (both teams) | 0.6 - 1.5 |

## Adding New Targets

1. Add the metric to `AggregateStats.cs`:
   - Add tracking field(s)
   - Update `Accumulate()` or `AccumulatePlay()` to collect data
   - Add calculated property for the rate/average

2. Add the target to `StatisticalTargets.cs`:
   ```csharp
   new StatisticalTarget
   {
       Category = "Category Name",
       Metric = "Metric Name",
       MinTarget = 10.0,
       MaxTarget = 15.0,
       GetActualValue = s => s.YourNewMetric
   }
   ```

3. Update this README with the new target.

## Test Data

The validator uses team data from `tests/Gridiron.Engine.Tests/TestData/`:
- `AtlantaFalcons.json`
- `PhiladelphiaEagles.json`

These files are copied to the output directory at build time.

## Related Issues

- [#30](https://github.com/merciless-creations/gridiron-engine/issues/30) - Build statistical validation framework
- [#27](https://github.com/merciless-creations/gridiron-engine/issues/27) - Implement logarithmic modifier curves

## Architecture

```
Program.cs          - CLI entry point, game loop
AggregateStats.cs   - Accumulates statistics across games
StatisticalTargets.cs - Defines target ranges from NFL data
ValidationReport.cs - Compares actuals to targets, formats output
```
