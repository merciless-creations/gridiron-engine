using Structurizr;
using Structurizr.IO.PlantUML;
using System.IO;
using PlantUml.Net;

// ================================================================================
// GRIDIRON.ENGINE - C4 ARCHITECTURE DIAGRAMS
// ================================================================================
// Generates comprehensive PlantUML diagrams for the Gridiron.Engine library
// A standalone NuGet package for NFL football game simulation
// ================================================================================

var workspace = new Workspace("Gridiron.Engine",
    "Standalone NFL game simulation engine library. Provides complete game simulation " +
    "with state machine, play execution, penalties, injuries, and statistical outcomes.");
var model = workspace.Model;

// ================================================================================
// LEVEL 1: SYSTEM CONTEXT
// ================================================================================

var gameApplication = model.AddPerson("Game Application",
    "Any application that consumes the Gridiron.Engine library to simulate football games");
var developer = model.AddPerson("Developer",
    "Software developer integrating the engine into their application");

var gridironEngine = model.AddSoftwareSystem("Gridiron.Engine",
    "Standalone NFL game simulation engine. Simulates complete games with realistic " +
    "play-by-play, injuries, penalties, and statistical outcomes. Supports deterministic replay.");

// The engine is a library - it doesn't have external dependencies
var loggingSystem = model.AddSoftwareSystem("Logging System",
    "Microsoft.Extensions.Logging for play-by-play output (injected by consumer)");

// Relationships
gameApplication.Uses(gridironEngine, "Simulates games using");
developer.Uses(gridironEngine, "Integrates, configures, extends");
gridironEngine.Uses(loggingSystem, "Outputs play-by-play logs via");

// ================================================================================
// LEVEL 2: CONTAINERS
// ================================================================================

var apiLayer = gridironEngine.AddContainer("Api",
    "Public API surface for the engine. Provides IGameEngine interface for consumers.",
    "C# .NET 8");

var domainLayer = gridironEngine.AddContainer("Domain",
    "Pure domain models: Game, Team, Player (25+ attributes), Plays, Penalties, Injuries. " +
    "No external dependencies.",
    "C# .NET 8");

var simulationLayer = gridironEngine.AddContainer("Simulation",
    "Game simulation engine with 19-state machine, 5 play types, 25+ skill checks, " +
    "penalty enforcement, injury system, and statistics accumulation.",
    "C# .NET 8");

var testProject = gridironEngine.AddContainer("Gridiron.Engine.Tests",
    "Comprehensive test suite covering all simulation scenarios.",
    "xUnit");

// Container relationships
gameApplication.Uses(apiLayer, "Creates IGameEngine, calls SimulateGame()");
apiLayer.Uses(simulationLayer, "Orchestrates via GameFlow");
apiLayer.Uses(domainLayer, "Uses Game, Team entities");
simulationLayer.Uses(domainLayer, "Manipulates game state");
simulationLayer.Uses(loggingSystem, "Logs play-by-play");
testProject.Uses(apiLayer, "Tests public API");
testProject.Uses(simulationLayer, "Tests game logic");
testProject.Uses(domainLayer, "Creates test scenarios");

// ================================================================================
// LEVEL 3: COMPONENTS - API Layer
// ================================================================================

var iGameEngine = apiLayer.AddComponent("IGameEngine",
    "Public interface for game simulation. Main entry point for consumers.",
    "Interface");
var gameEngine = apiLayer.AddComponent("GameEngine",
    "Default implementation. Creates GameFlow, injects dependencies, runs simulation.",
    "C# Class");

gameEngine.Uses(iGameEngine, "Implements");

// ================================================================================
// LEVEL 3: COMPONENTS - Domain Layer
// ================================================================================

// Core entities
var game = domainLayer.AddComponent("Game",
    "Central game state: field position, down, distance, score, possession, quarters, halves. " +
    "Contains list of all plays executed.",
    "C# Class");
var player = domainLayer.AddComponent("Player",
    "Player with 25+ attributes: Speed, Strength, Agility, Passing, Catching, Rushing, " +
    "Blocking, Tackling, Coverage, Kicking, Discipline, Fragility, Health, Potential, etc.",
    "C# Class");
var team = domainLayer.AddComponent("Team",
    "Team with roster, 8 depth charts (offense, defense, special teams), coaching staff, " +
    "scouts, trainers, and statistics.",
    "C# Class");
var person = domainLayer.AddComponent("Person",
    "Base class for Player, Coach, Scout, Trainer with name properties.",
    "C# Class");
var coach = domainLayer.AddComponent("Coach",
    "Coaching staff member with role, skills, and statistics.",
    "C# Class");

// Play types
var iPlay = domainLayer.AddComponent("IPlay",
    "Interface for all play types. Common properties: possession, down, penalties, " +
    "fumbles, injuries, yards, timing.",
    "Interface");
var play = domainLayer.AddComponent("Play",
    "Base class with common play properties shared by all play types.",
    "C# Class");
var passPlay = domainLayer.AddComponent("PassPlay",
    "Pass play with passer, receiver, air yards, YAC, completion status, interception info.",
    "C# Class");
var runPlay = domainLayer.AddComponent("RunPlay",
    "Run play with ball carrier, direction, segments for tracking ball carrier changes.",
    "C# Class");
var kickoffPlay = domainLayer.AddComponent("KickoffPlay",
    "Kickoff with kicker, distance, touchback, return yards, onside kick support.",
    "C# Class");
var puntPlay = domainLayer.AddComponent("PuntPlay",
    "Punt with punter, distance, hang time, fair catch, block status, return info.",
    "C# Class");
var fieldGoalPlay = domainLayer.AddComponent("FieldGoalPlay",
    "Field goal/extra point with kicker, holder, distance, make/miss, block info.",
    "C# Class");

// Segments
var iPlaySegment = domainLayer.AddComponent("IPlaySegment",
    "Interface for play segments tracking ball carrier changes during fumbles/laterals.",
    "Interface");
var passSegment = domainLayer.AddComponent("PassSegment",
    "Pass segment with passer, receiver, air yards, YAC, completion, fumble tracking.",
    "C# Class");
var runSegment = domainLayer.AddComponent("RunSegment",
    "Run segment with ball carrier, direction, yards, fumble tracking.",
    "C# Class");
var returnSegment = domainLayer.AddComponent("ReturnSegment",
    "Return segment for kickoffs, punts, interceptions with yards and fumble tracking.",
    "C# Class");

// Penalties
var penalty = domainLayer.AddComponent("Penalty",
    "Penalty instance with name, yards, acceptance status, player who committed it.",
    "C# Class");
var iPenalty = domainLayer.AddComponent("IPenalty",
    "Interface defining penalty behavior: occurrence probability, eligibility, enforcement.",
    "Interface");
var penaltyBase = domainLayer.AddComponent("PenaltyBase",
    "Abstract base class with default implementations for penalty logic.",
    "C# Class");
var penaltyRegistry = domainLayer.AddComponent("PenaltyRegistry",
    "Central registry of all penalty types with lookup methods.",
    "C# Class");
var falseStart = domainLayer.AddComponent("FalseStartPenalty",
    "5-yard dead ball foul for offensive player moving before snap.",
    "C# Class");
var offensiveHolding = domainLayer.AddComponent("OffensiveHoldingPenalty",
    "10-yard penalty for illegally restraining a defender.",
    "C# Class");
var defensiveHolding = domainLayer.AddComponent("DefensiveHoldingPenalty",
    "5-yard penalty with automatic first down for illegal contact.",
    "C# Class");
var defensivePI = domainLayer.AddComponent("DefensivePassInterferencePenalty",
    "Spot foul with automatic first down - most impactful penalty.",
    "C# Class");

// Injuries and Fumbles
var injury = domainLayer.AddComponent("Injury",
    "Injury with type (Ankle/Knee/Shoulder/Concussion/Hamstring), severity, recovery time.",
    "C# Class");
var fumble = domainLayer.AddComponent("Fumble",
    "Fumble with fumbled by, recovered by, field position, possession change.",
    "C# Class");
var interception = domainLayer.AddComponent("Interception",
    "Interception with intercepted by, thrown by, return yards, fumble during return.",
    "C# Class");

// Depth Chart
var depthChart = domainLayer.AddComponent("DepthChart",
    "Maps positions to ordered lists of players (starter first, then backups).",
    "C# Class");
var depthChartBuilder = domainLayer.AddComponent("DepthChartBuilder",
    "Builds all 8 depth charts from team roster based on position and skills.",
    "C# Class");

// Time management
var half = domainLayer.AddComponent("Half",
    "Represents a half containing two quarters with time tracking.",
    "C# Class");
var quarter = domainLayer.AddComponent("Quarter",
    "Quarter with 900-second clock, expiration detection.",
    "C# Class");

// Helpers
var seedableRandom = domainLayer.AddComponent("SeedableRandom",
    "Deterministic random number generator for reproducible simulations.",
    "C# Class");
var replaySeedableRandom = domainLayer.AddComponent("ReplaySeedableRandom",
    "Replays recorded random sequences for exact game reproduction.",
    "C# Class");
var replayLog = domainLayer.AddComponent("ReplayLog",
    "Captures random values for deterministic replay.",
    "C# Class");
var fieldPositionHelper = domainLayer.AddComponent("FieldPositionHelper",
    "Converts 0-100 field position to NFL notation (e.g., 'KC 25').",
    "C# Class");
var gameHelper = domainLayer.AddComponent("GameHelper",
    "Static helper for creating and initializing game instances.",
    "C# Class");

// Enums
var positions = domainLayer.AddComponent("Positions (enum)",
    "Player positions: QB, RB, WR, TE, OL, DL, LB, CB, S, K, P, etc.",
    "Enum");
var downs = domainLayer.AddComponent("Downs (enum)",
    "First, Second, Third, Fourth, None (for kickoffs).",
    "Enum");
var possession = domainLayer.AddComponent("Possession (enum)",
    "None, Home, Away - tracks ball possession.",
    "Enum");
var playType = domainLayer.AddComponent("PlayType (enum)",
    "Kickoff, FieldGoal, Punt, Pass, Run.",
    "Enum");

// Domain relationships
game.Uses(team, "Has home and away teams");
game.Uses(iPlay, "Contains list of plays");
game.Uses(quarter, "Tracks current quarter");
game.Uses(half, "Tracks current half");
team.Uses(player, "Has roster of players");
team.Uses(depthChart, "Has 8 depth charts");
team.Uses(coach, "Has coaching staff");
player.Uses(person, "Extends");
coach.Uses(person, "Extends");
iPlay.Uses(penalty, "Can have penalties");
iPlay.Uses(fumble, "Can have fumbles");
iPlay.Uses(injury, "Can have injuries");
passPlay.Uses(iPlay, "Implements");
runPlay.Uses(iPlay, "Implements");
kickoffPlay.Uses(iPlay, "Implements");
puntPlay.Uses(iPlay, "Implements");
fieldGoalPlay.Uses(iPlay, "Implements");
passPlay.Uses(passSegment, "Contains segments");
runPlay.Uses(runSegment, "Contains segments");
kickoffPlay.Uses(returnSegment, "Contains return");
puntPlay.Uses(returnSegment, "Contains return");
penaltyBase.Uses(iPenalty, "Implements");
falseStart.Uses(penaltyBase, "Extends");
offensiveHolding.Uses(penaltyBase, "Extends");
defensiveHolding.Uses(penaltyBase, "Extends");
defensivePI.Uses(penaltyBase, "Extends");
penaltyRegistry.Uses(iPenalty, "Contains all penalties");
depthChartBuilder.Uses(depthChart, "Builds");

// ================================================================================
// LEVEL 3: COMPONENTS - Simulation Layer
// ================================================================================

// State machine
var gameFlow = simulationLayer.AddComponent("GameFlow",
    "19-state machine orchestrator using Stateless library. Controls game progression, " +
    "manages state transitions, injects dependencies.",
    "C# Class");

// Actions
var preGame = simulationLayer.AddComponent("PreGame (Action)",
    "Pre-game activities and setup.",
    "C# Class");
var coinToss = simulationLayer.AddComponent("CoinToss (Action)",
    "Executes coin toss, determines initial possession with defer option.",
    "C# Class");
var prePlay = simulationLayer.AddComponent("PrePlay (Action)",
    "Huddle phase: determines play type, substitutes players, checks pre-snap penalties.",
    "C# Class");
var snap = simulationLayer.AddComponent("Snap (Action)",
    "Snap execution with bad snap possibility (~1%).",
    "C# Class");
var postPlay = simulationLayer.AddComponent("PostPlay (Action)",
    "Post-play: checks penalties, scores, injuries, quarter expiration.",
    "C# Class");
var halftime = simulationLayer.AddComponent("Halftime (Action)",
    "Halftime activities between 2nd and 3rd quarters.",
    "C# Class");
var quarterExpired = simulationLayer.AddComponent("QuarterExpired (Action)",
    "Quarter transition handling.",
    "C# Class");
var postGame = simulationLayer.AddComponent("PostGame (Action)",
    "Post-game activities and finalization.",
    "C# Class");
var fumbleAction = simulationLayer.AddComponent("Fumble (Action)",
    "Handles fumble outcomes and possession changes.",
    "C# Class");
var interceptionAction = simulationLayer.AddComponent("Interception (Action)",
    "Handles interception outcomes.",
    "C# Class");

// Event Checks
var scoreCheck = simulationLayer.AddComponent("ScoreCheck",
    "Checks for and processes scoring events.",
    "C# Class");
var quarterExpireCheck = simulationLayer.AddComponent("QuarterExpireCheck",
    "Checks if quarter has expired based on elapsed time.",
    "C# Class");
var halfExpireCheck = simulationLayer.AddComponent("HalfExpireCheck",
    "Checks if half has expired, triggers halftime.",
    "C# Class");
var gameExpireCheck = simulationLayer.AddComponent("GameExpireCheck",
    "Checks if game has ended.",
    "C# Class");

// Play execution
var runExecution = simulationLayer.AddComponent("Run (Play)",
    "Executes run play: blocking, direction, yards, breakaway, fumbles, penalties, injuries.",
    "C# Class");
var passExecution = simulationLayer.AddComponent("Pass (Play)",
    "Executes pass play: protection, completion, interception, YAC, sacks, penalties, injuries.",
    "C# Class");
var kickoffExecution = simulationLayer.AddComponent("Kickoff (Play)",
    "Executes kickoff: distance, touchback, return, onside kick, muffed catch, injuries.",
    "C# Class");
var puntExecution = simulationLayer.AddComponent("Punt (Play)",
    "Executes punt: distance, hang time, block, fair catch, return, downed, injuries.",
    "C# Class");
var fieldGoalExecution = simulationLayer.AddComponent("FieldGoal (Play)",
    "Executes field goal: distance calculation, block check, make/miss determination.",
    "C# Class");

// Play results
var runResult = simulationLayer.AddComponent("RunResult",
    "Processes run play: updates field position, checks first down, TD, safety, penalties.",
    "C# Class");
var passResult = simulationLayer.AddComponent("PassResult",
    "Processes pass play: updates field position, checks first down, TD, interceptions, penalties.",
    "C# Class");
var kickoffResult = simulationLayer.AddComponent("KickoffResult",
    "Processes kickoff: updates possession, field position, handles touchbacks.",
    "C# Class");
var puntResult = simulationLayer.AddComponent("PuntResult",
    "Processes punt: updates possession, field position, handles touchbacks, blocked punts.",
    "C# Class");
var fieldGoalResult = simulationLayer.AddComponent("FieldGoalResult",
    "Processes field goal: updates score, possession on miss/block.",
    "C# Class");

// Skills Checks (occurrence checks)
var badSnapCheck = simulationLayer.AddComponent("BadSnapOccurredSkillsCheck",
    "Checks for bad snaps on punts based on long snapper skill.",
    "C# Class");
var bigRunCheck = simulationLayer.AddComponent("BigRunSkillsCheck",
    "Checks if run becomes a breakaway play.",
    "C# Class");
var blockingSuccessCheck = simulationLayer.AddComponent("BlockingSuccessSkillsCheck",
    "Determines if O-line creates running lane vs D-line.",
    "C# Class");
var fairCatchCheck = simulationLayer.AddComponent("FairCatchOccurredSkillsCheck",
    "Checks if returner calls fair catch.",
    "C# Class");
var fieldGoalBlockCheck = simulationLayer.AddComponent("FieldGoalBlockOccurredSkillsCheck",
    "Checks if field goal is blocked.",
    "C# Class");
var fieldGoalMakeCheck = simulationLayer.AddComponent("FieldGoalMakeOccurredSkillsCheck",
    "Determines if field goal is made based on distance and kicker skill.",
    "C# Class");
var fumbleOccurredCheck = simulationLayer.AddComponent("FumbleOccurredSkillsCheck",
    "Checks for fumbles based on ball carrier, tacklers, gang tackles.",
    "C# Class");
var injuryOccurredCheck = simulationLayer.AddComponent("InjuryOccurredSkillsCheck",
    "Determines if injury occurs based on fragility, play type, position.",
    "C# Class");
var interceptionCheck = simulationLayer.AddComponent("InterceptionOccurredSkillsCheck",
    "Checks if incomplete pass is intercepted.",
    "C# Class");
var muffedCatchCheck = simulationLayer.AddComponent("MuffedCatchOccurredSkillsCheck",
    "Checks if returner muffs the catch.",
    "C# Class");
var passCompletionCheck = simulationLayer.AddComponent("PassCompletionSkillsCheck",
    "Determines if pass is completed based on QB, receiver, coverage.",
    "C# Class");
var passProtectionCheck = simulationLayer.AddComponent("PassProtectionSkillsCheck",
    "Determines if QB is sacked based on O-line vs D-line.",
    "C# Class");
var puntBlockCheck = simulationLayer.AddComponent("PuntBlockOccurredSkillsCheck",
    "Checks if punt is blocked.",
    "C# Class");
var puntDownedCheck = simulationLayer.AddComponent("PuntDownedOccurredSkillsCheck",
    "Checks if punt is downed by coverage team.",
    "C# Class");
var puntOutOfBoundsCheck = simulationLayer.AddComponent("PuntOutOfBoundsOccurredSkillsCheck",
    "Checks if punt goes out of bounds.",
    "C# Class");
var qbPressureCheck = simulationLayer.AddComponent("QBPressureSkillsCheck",
    "Determines if QB is under pressure (affects accuracy).",
    "C# Class");
var tackleBreakCheck = simulationLayer.AddComponent("TackleBreakSkillsCheck",
    "Determines if ball carrier breaks tackle for extra yards.",
    "C# Class");
var yacCheck = simulationLayer.AddComponent("YardsAfterCatchSkillsCheck",
    "Determines if receiver gains extra yards after catch.",
    "C# Class");

// Penalty checks
var preSnapPenaltyCheck = simulationLayer.AddComponent("PreSnapPenaltyOccurredSkillsCheck",
    "Checks for pre-snap penalties (False Start, Offsides, etc.).",
    "C# Class");
var blockingPenaltyCheck = simulationLayer.AddComponent("BlockingPenaltyOccurredSkillsCheck",
    "Checks for blocking penalties (Holding, Illegal Block, etc.).",
    "C# Class");
var coveragePenaltyCheck = simulationLayer.AddComponent("CoveragePenaltyOccurredSkillsCheck",
    "Checks for coverage penalties (Pass Interference, Holding, etc.).",
    "C# Class");
var tacklePenaltyCheck = simulationLayer.AddComponent("TacklePenaltyOccurredSkillsCheck",
    "Checks for tackle penalties (Facemask, Horse Collar, etc.).",
    "C# Class");
var postPlayPenaltyCheck = simulationLayer.AddComponent("PostPlayPenaltyOccurredSkillsCheck",
    "Checks for post-play penalties (Taunting, Unsportsmanlike, etc.).",
    "C# Class");

// Skills Check Results (calculations)
var airYardsResult = simulationLayer.AddComponent("AirYardsSkillsCheckResult",
    "Calculates air yards for passes based on type and field position.",
    "C# Class");
var badSnapYardsResult = simulationLayer.AddComponent("BadSnapYardsSkillsCheckResult",
    "Calculates yards lost on bad snaps.",
    "C# Class");
var blockedFGReturnResult = simulationLayer.AddComponent("BlockedFieldGoalReturnYardsSkillsCheckResult",
    "Calculates return yards for blocked field goals.",
    "C# Class");
var breakawayYardsResult = simulationLayer.AddComponent("BreakawayYardsSkillsCheckResult",
    "Calculates extra yards on breakaway runs.",
    "C# Class");
var fumblePossessionResult = simulationLayer.AddComponent("FumblePossessionChangeSkillsCheckResult",
    "Determines which team recovers fumble.",
    "C# Class");
var fumbleRecoveryResult = simulationLayer.AddComponent("FumbleRecoverySkillsCheckResult",
    "Determines fumble recovery with bounce physics.",
    "C# Class");
var injuryEffectResult = simulationLayer.AddComponent("InjuryEffectSkillsCheckResult",
    "Determines injury type, severity, recovery time.",
    "C# Class");
var interceptionPossessionResult = simulationLayer.AddComponent("InterceptionPossessionChangeSkillsCheckResult",
    "Handles possession change for interceptions.",
    "C# Class");
var interceptionReturnResult = simulationLayer.AddComponent("InterceptionReturnSkillsCheckResult",
    "Calculates interception return yardage.",
    "C# Class");
var interceptionResult = simulationLayer.AddComponent("InterceptionSkillsCheckResult",
    "Complete interception scenario including returns and fumbles.",
    "C# Class");
var kickoffDistanceResult = simulationLayer.AddComponent("KickoffDistanceSkillsCheckResult",
    "Calculates kickoff distance based on kicker skill.",
    "C# Class");
var kickoffReturnResult = simulationLayer.AddComponent("KickoffReturnYardsSkillsCheckResult",
    "Calculates kickoff return yardage.",
    "C# Class");
var penaltyEffectResult = simulationLayer.AddComponent("PenaltyEffectSkillsCheckResult",
    "Determines penalty details, player who committed, yardage.",
    "C# Class");
var puntDistanceResult = simulationLayer.AddComponent("PuntDistanceSkillsCheckResult",
    "Calculates punt distance based on punter skill.",
    "C# Class");
var puntHangTimeResult = simulationLayer.AddComponent("PuntHangTimeSkillsCheckResult",
    "Calculates punt hang time.",
    "C# Class");
var puntReturnResult = simulationLayer.AddComponent("PuntReturnYardsSkillsCheckResult",
    "Calculates punt return yardage with coverage quality.",
    "C# Class");
var runYardsResult = simulationLayer.AddComponent("RunYardsSkillsCheckResult",
    "Calculates base run yardage from O-line vs D-line battle.",
    "C# Class");
var sackYardsResult = simulationLayer.AddComponent("SackYardsSkillsCheckResult",
    "Calculates yards lost on sacks.",
    "C# Class");
var tackleBreakYardsResult = simulationLayer.AddComponent("TackleBreakYardsSkillsCheckResult",
    "Calculates extra yards when breaking tackles.",
    "C# Class");
var yacResult = simulationLayer.AddComponent("YardsAfterCatchSkillsCheckResult",
    "Calculates yards after catch with big play potential.",
    "C# Class");

// Services
var penaltyEnforcement = simulationLayer.AddComponent("PenaltyEnforcement",
    "Enforces penalties: calculates net yards, handles acceptance, offsetting, updates game state.",
    "C# Class");
var statsAccumulator = simulationLayer.AddComponent("StatsAccumulator",
    "Accumulates player stats (passing, rushing, receiving, tackles, etc.).",
    "C# Class");

// Calculators
var lineBattleCalc = simulationLayer.AddComponent("LineBattleCalculator",
    "Calculates O-line vs D-line effectiveness.",
    "C# Class");
var teamPowerCalc = simulationLayer.AddComponent("TeamPowerCalculator",
    "Calculates team power for blocking, rushing, coverage.",
    "C# Class");

// Configuration
var gameProbabilities = simulationLayer.AddComponent("GameProbabilities",
    "Constants for game probabilities (completion %, fumble rates, play selection, etc.).",
    "C# Class");
var injuryProbabilities = simulationLayer.AddComponent("InjuryProbabilities",
    "Constants for injury probabilities by play type, position, multipliers.",
    "C# Class");

// Base classes
var skillsCheckResult = simulationLayer.AddComponent("SkillsCheckResult<T>",
    "Generic base class for skills check results.",
    "C# Class");
var actionOccurredCheck = simulationLayer.AddComponent("ActionOccurredSkillsCheck",
    "Base class for checks that determine if an action occurred.",
    "C# Class");
var yardageCheckResult = simulationLayer.AddComponent("YardageSkillsCheckResult",
    "Base class for skills check results that return yardage.",
    "C# Class");
var possessionCheckResult = simulationLayer.AddComponent("PossessionChangeSkillsCheckResult",
    "Base class for skills check results that change possession.",
    "C# Class");

// Interfaces
var iGameAction = simulationLayer.AddComponent("IGameAction",
    "Interface for game actions (Command pattern).",
    "Interface");

// Simulation relationships
gameFlow.Uses(game, "Manipulates game state");
gameFlow.Uses(seedableRandom, "Uses for deterministic simulation");
gameEngine.Uses(gameFlow, "Creates and executes");

// Actions implement interface
preGame.Uses(iGameAction, "Implements");
coinToss.Uses(iGameAction, "Implements");
prePlay.Uses(iGameAction, "Implements");
snap.Uses(iGameAction, "Implements");
postPlay.Uses(iGameAction, "Implements");

// State flow
prePlay.Uses(preSnapPenaltyCheck, "Checks pre-snap penalties");
prePlay.Uses(snap, "Executes snap");

// Play execution uses skills checks
runExecution.Uses(blockingSuccessCheck, "Checks blocking");
runExecution.Uses(tackleBreakCheck, "Checks tackle breaks");
runExecution.Uses(bigRunCheck, "Checks breakaway");
runExecution.Uses(fumbleOccurredCheck, "Checks fumbles");
runExecution.Uses(blockingPenaltyCheck, "Checks holding");
runExecution.Uses(injuryOccurredCheck, "Checks injuries");

passExecution.Uses(passProtectionCheck, "Checks sacks");
passExecution.Uses(passCompletionCheck, "Checks completion");
passExecution.Uses(interceptionCheck, "Checks interceptions");
passExecution.Uses(yacCheck, "Checks YAC");
passExecution.Uses(coveragePenaltyCheck, "Checks PI");

kickoffExecution.Uses(kickoffDistanceResult, "Calculates distance");
kickoffExecution.Uses(fairCatchCheck, "Checks fair catch");
kickoffExecution.Uses(muffedCatchCheck, "Checks muffed catch");

puntExecution.Uses(puntDistanceResult, "Calculates distance");
puntExecution.Uses(puntBlockCheck, "Checks blocks");
puntExecution.Uses(fairCatchCheck, "Checks fair catch");

fieldGoalExecution.Uses(fieldGoalMakeCheck, "Checks make/miss");
fieldGoalExecution.Uses(fieldGoalBlockCheck, "Checks blocks");

// Results use services
runResult.Uses(penaltyEnforcement, "Enforces penalties");
runResult.Uses(statsAccumulator, "Accumulates stats");
passResult.Uses(penaltyEnforcement, "Enforces penalties");
passResult.Uses(statsAccumulator, "Accumulates stats");

postPlay.Uses(scoreCheck, "Checks scoring");
postPlay.Uses(quarterExpireCheck, "Checks quarter expiration");
postPlay.Uses(postPlayPenaltyCheck, "Checks post-play penalties");

// ================================================================================
// VIEWS
// ================================================================================

var views = workspace.Views;

// System Context View
var contextView = views.CreateSystemContextView(gridironEngine, "SystemContext",
    "System context showing Gridiron.Engine as a consumable library");
contextView.AddAllSoftwareSystems();
contextView.AddAllPeople();
contextView.PaperSize = PaperSize.A4_Landscape;

// Container View
var containerView = views.CreateContainerView(gridironEngine, "Containers",
    "Container view showing the major layers of the engine");
containerView.AddAllContainers();
containerView.Add(gameApplication);
containerView.Add(developer);
containerView.Add(loggingSystem);
containerView.PaperSize = PaperSize.A4_Landscape;

// Component View - API
var apiComponentView = views.CreateComponentView(apiLayer, "ApiComponents",
    "Public API surface for game simulation");
apiComponentView.AddAllComponents();
apiComponentView.Add(simulationLayer);
apiComponentView.Add(domainLayer);
apiComponentView.PaperSize = PaperSize.A4_Landscape;

// Component View - Domain
var domainComponentView = views.CreateComponentView(domainLayer, "DomainComponents",
    "Domain entities, value objects, and helpers");
domainComponentView.AddAllComponents();
domainComponentView.PaperSize = PaperSize.A2_Landscape;

// Component View - Simulation
var simulationComponentView = views.CreateComponentView(simulationLayer, "SimulationComponents",
    "Game simulation engine: state machine, plays, skills checks, services");
simulationComponentView.AddAllComponents();
simulationComponentView.Add(domainLayer);
simulationComponentView.PaperSize = PaperSize.A2_Landscape;

// Focused Views

// State Machine Flow
var stateMachineView = views.CreateComponentView(simulationLayer, "StateMachineFlow",
    "GameFlow state machine with actions and play execution");
stateMachineView.Add(gameFlow);
stateMachineView.Add(preGame);
stateMachineView.Add(coinToss);
stateMachineView.Add(prePlay);
stateMachineView.Add(snap);
stateMachineView.Add(runExecution);
stateMachineView.Add(passExecution);
stateMachineView.Add(kickoffExecution);
stateMachineView.Add(puntExecution);
stateMachineView.Add(fieldGoalExecution);
stateMachineView.Add(runResult);
stateMachineView.Add(passResult);
stateMachineView.Add(kickoffResult);
stateMachineView.Add(puntResult);
stateMachineView.Add(fieldGoalResult);
stateMachineView.Add(postPlay);
stateMachineView.Add(halftime);
stateMachineView.Add(quarterExpired);
stateMachineView.Add(postGame);
stateMachineView.PaperSize = PaperSize.A3_Landscape;

// Skills Checks
var skillsChecksView = views.CreateComponentView(simulationLayer, "SkillsChecks",
    "Skills checks for realistic gameplay outcomes");
skillsChecksView.Add(passCompletionCheck);
skillsChecksView.Add(passProtectionCheck);
skillsChecksView.Add(interceptionCheck);
skillsChecksView.Add(blockingSuccessCheck);
skillsChecksView.Add(tackleBreakCheck);
skillsChecksView.Add(bigRunCheck);
skillsChecksView.Add(fumbleOccurredCheck);
skillsChecksView.Add(injuryOccurredCheck);
skillsChecksView.Add(fieldGoalMakeCheck);
skillsChecksView.Add(fieldGoalBlockCheck);
skillsChecksView.Add(puntBlockCheck);
skillsChecksView.Add(fairCatchCheck);
skillsChecksView.Add(muffedCatchCheck);
skillsChecksView.Add(qbPressureCheck);
skillsChecksView.Add(yacCheck);
skillsChecksView.Add(actionOccurredCheck);
skillsChecksView.PaperSize = PaperSize.A3_Landscape;

// Penalty System
var penaltySystemView = views.CreateComponentView(simulationLayer, "PenaltySystem",
    "Penalty checks and enforcement");
penaltySystemView.Add(preSnapPenaltyCheck);
penaltySystemView.Add(blockingPenaltyCheck);
penaltySystemView.Add(coveragePenaltyCheck);
penaltySystemView.Add(tacklePenaltyCheck);
penaltySystemView.Add(postPlayPenaltyCheck);
penaltySystemView.Add(penaltyEffectResult);
penaltySystemView.Add(penaltyEnforcement);
penaltySystemView.Add(penalty);
penaltySystemView.Add(penaltyRegistry);
penaltySystemView.Add(falseStart);
penaltySystemView.Add(offensiveHolding);
penaltySystemView.Add(defensiveHolding);
penaltySystemView.Add(defensivePI);
penaltySystemView.PaperSize = PaperSize.A3_Landscape;

// ================================================================================
// EXPORT TO PLANTUML
// ================================================================================

var plantUmlWriter = new PlantUMLWriter();

// Get the solution root directory
var currentDir = Directory.GetCurrentDirectory();
var solutionDir = currentDir;

while (solutionDir != null && !Directory.GetFiles(solutionDir, "*.sln").Any())
{
    var parent = Directory.GetParent(solutionDir);
    if (parent == null) break;
    solutionDir = parent.FullName;
}

if (!Directory.GetFiles(solutionDir!, "*.sln").Any())
{
    solutionDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
}

var outputPath = Path.Combine(solutionDir!, "diagram");

if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

void WriteView(View view, string filename)
{
    var fullPath = Path.Combine(outputPath, filename);
    using (var writer = new StreamWriter(fullPath))
    {
        switch (view)
        {
            case SystemLandscapeView v: plantUmlWriter.Write(v, writer); break;
            case SystemContextView v: plantUmlWriter.Write(v, writer); break;
            case ContainerView v: plantUmlWriter.Write(v, writer); break;
            case ComponentView v: plantUmlWriter.Write(v, writer); break;
        }
    }
    Console.WriteLine($"  Generated {filename}");
}

Console.WriteLine("================================================================================");
Console.WriteLine("GRIDIRON.ENGINE - C4 DIAGRAM GENERATION");
Console.WriteLine("================================================================================");
Console.WriteLine($"System: {workspace.Name}");
Console.WriteLine($"Output: {outputPath}");
Console.WriteLine();
Console.WriteLine("Generating PlantUML diagrams...");
Console.WriteLine();

WriteView(contextView, "01-SystemContext.puml");
WriteView(containerView, "02-Containers.puml");
WriteView(apiComponentView, "03-ApiComponents.puml");
WriteView(domainComponentView, "04-DomainComponents.puml");
WriteView(simulationComponentView, "05-SimulationComponents.puml");
WriteView(stateMachineView, "06-StateMachineFlow.puml");
WriteView(skillsChecksView, "07-SkillsChecks.puml");
WriteView(penaltySystemView, "08-PenaltySystem.puml");

Console.WriteLine();
Console.WriteLine("================================================================================");
Console.WriteLine("COMPLETE - 8 diagrams generated");
Console.WriteLine("================================================================================");
Console.WriteLine();
Console.WriteLine("Generating PNG images from PlantUML files...");
Console.WriteLine();

try
{
    var factory = new RendererFactory();
    var renderer = factory.CreateRenderer(new PlantUmlSettings
    {
        RemoteUrl = "http://www.plantuml.com/plantuml"
    });

    var pumlFiles = Directory.GetFiles(outputPath, "*.puml");
    int pngCount = 0;

    foreach (var pumlFile in pumlFiles)
    {
        try
        {
            var pumlContent = File.ReadAllText(pumlFile);
            var pngBytes = renderer.Render(pumlContent, OutputFormat.Png);

            var pngPath = Path.ChangeExtension(pumlFile, ".png");
            File.WriteAllBytes(pngPath, pngBytes);

            Console.WriteLine($"  Generated {Path.GetFileName(pngPath)}");
            pngCount++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Failed to generate PNG for {Path.GetFileName(pumlFile)}: {ex.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Generated {pngCount} PNG images");
}
catch (Exception ex)
{
    Console.WriteLine($"PNG generation failed: {ex.Message}");
    Console.WriteLine("Note: PNG generation requires internet connection to PlantUML server");
}

Console.WriteLine();
Console.WriteLine("================================================================================");
Console.WriteLine("Diagrams:");
Console.WriteLine("  01 - System Context (library consumers and logging)");
Console.WriteLine("  02 - Containers (API, Domain, Simulation, Tests)");
Console.WriteLine("  03 - API Components (IGameEngine, GameEngine)");
Console.WriteLine("  04 - Domain Components (Game, Team, Player, Plays, Penalties)");
Console.WriteLine("  05 - Simulation Components (GameFlow, Actions, Skills Checks)");
Console.WriteLine("  06 - State Machine Flow (19-state game flow)");
Console.WriteLine("  07 - Skills Checks (gameplay outcome checks)");
Console.WriteLine("  08 - Penalty System (penalty checks and enforcement)");
Console.WriteLine("================================================================================");
