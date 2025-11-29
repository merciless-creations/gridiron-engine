# Gridiron Engine

Football game simulation engine with statistical modeling and realistic game flow.

## Features

- State machine-based game flow (19 game states)
- Probability-driven play outcomes based on player skills
- Complete NFL-style rules (downs, scoring, penalties)
- Player skill calculations and matchups
- Penalty system with realistic enforcement
- Injury tracking and player availability
- Deterministic simulation with seed support
- 800+ unit tests

## Installation

### From GitHub Packages

```bash
# Add GitHub Packages source (one-time setup)
dotnet nuget add source "https://nuget.pkg.github.com/merciless-creations/index.json" \
  --name "github-merciless" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT

# Add package to your project
dotnet add package Gridiron.Engine
```

### From Source

```bash
git clone https://github.com/merciless-creations/gridiron-engine.git
cd gridiron-engine
dotnet build
```

## Quick Start

```csharp
using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;

// Create engine
var engine = new GameEngine();

// Simulate a game (with optional seed for reproducibility)
var result = engine.SimulateGame(homeTeam, awayTeam, new SimulationOptions
{
    RandomSeed = 12345
});

// Access results
Console.WriteLine($"Final: {result.HomeScore} - {result.AwayScore}");
Console.WriteLine($"Winner: {result.Winner?.Name}");
Console.WriteLine($"Total plays: {result.TotalPlays}");
```

## Creating Teams

Teams require a full roster with players at each position:

```csharp
var team = new Team { Name = "My Team", City = "My City" };

// Add players for all positions
team.Players.Add(new Player
{
    FirstName = "John",
    LastName = "Quarterback",
    Position = Positions.QB,
    Number = 12,

    // Core attributes (0-100)
    Speed = 75,
    Strength = 70,
    Agility = 80,
    Awareness = 85,

    // Position-specific skills
    Passing = 90,      // QB
    Rushing = 60,      // RB, QB
    Catching = 50,     // WR, TE, RB
    Blocking = 40,     // OL, TE
    Tackling = 30,     // Defense
    Coverage = 30,     // CB, S, LB
    Kicking = 20,      // K, P

    Health = 100,
    Morale = 80
});

// Required positions for a complete roster:
// Offense: QB, RB, FB, WR (3-4), TE, T (2), G (2), C
// Defense: DE (2), DT (2), LB (3), OLB (2), CB (2-3), S (2), FS (2)
// Special: K, P, LS, H
```

## Integration with Gridiron App

### Option 1: Project Reference (Development)

```xml
<ItemGroup>
  <ProjectReference Include="..\gridiron-engine\src\Gridiron.Engine\Gridiron.Engine.csproj" />
</ItemGroup>
```

### Option 2: NuGet Package (Production)

```xml
<ItemGroup>
  <PackageReference Include="Gridiron.Engine" Version="0.1.0" />
</ItemGroup>
```

### Mapping Persistence Entities

The engine uses clean domain objects without EF attributes. Map between your persistence layer:

```csharp
// Your EF entity
public class TeamEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ... EF navigation properties
}

// Mapper
public static Team ToEngineTeam(TeamEntity entity)
{
    return new Team
    {
        Name = entity.Name,
        City = entity.City,
        Players = entity.Players.Select(ToEnginePlayer).ToList()
    };
}

public static void UpdateFromResult(TeamEntity entity, GameResult result)
{
    // Update stats, records, etc.
    entity.Wins += result.Winner == result.HomeTeam ? 1 : 0;
}
```

## API Reference

### IGameEngine

```csharp
public interface IGameEngine
{
    GameResult SimulateGame(Team homeTeam, Team awayTeam, SimulationOptions? options = null);
}
```

### SimulationOptions

| Property | Type | Description |
|----------|------|-------------|
| RandomSeed | int? | Seed for deterministic simulation |
| Logger | ILogger? | Logger for play-by-play output |

### GameResult

| Property | Type | Description |
|----------|------|-------------|
| Game | Game | Complete game state |
| HomeScore | int | Final home team score |
| AwayScore | int | Final away team score |
| Winner | Team? | Winning team (null if tie) |
| IsTie | bool | Whether game ended in tie |
| Plays | IReadOnlyList<IPlay> | All plays executed |
| TotalPlays | int | Number of plays |
| RandomSeed | int? | Seed used (for replay) |

## Running Tests

```bash
dotnet test
```

## License

MIT
