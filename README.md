# Cube Blast — Hybrid-Casual Puzzle Core

> A Unity portfolio project built around one specific production capability: **an engine-agnostic gameplay core that can be driven by an automated bot, without spinning up Unity**. Hybrid-casual success is largely a function of level-design quality, and level-design quality is largely a function of how cheaply you can simulate and compare thousands of plays. This codebase is shaped around making that loop practical.

---

## Table of Contents

- [Project Intent](#project-intent)
- [Gameplay](#gameplay)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Core vs View - What Lives Where & Why](#core-vs-view---what-lives-where--why)
- [Design Patterns](#design-patterns)
- [Engineering Highlights](#engineering-highlights)
- [Bot Harness & Headless Simulation](#bot-harness--headless-simulation)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Level Data Format](#level-data-format)
- [Scope & Status](#scope--status)

---

## Project Intent

### Why an engine-agnostic core?

In hybrid-casual puzzle games, sustained retention and monetization track directly with **level-design quality**: difficulty curves, solvability, intended-strategy validation, and pacing. **Automated bot playtesting** can become the backbone of that level-development loop: running thousands of headless simulations across policies, seeds, and tuning parameters to estimate win rates, expose unintended exploits, validate intended strategies, and filter levels before expensive manual playtesting.

That requirement shaped the architecture from the start. Concretely:

- The entire simulation in `Blast.Core.*` has **zero `UnityEngine` references**. It compiles in a plain C# project with no Unity SDK, which is what makes the same gameplay model usable by the headless bot harness.
- The simulation advances through an explicit `Tick(deltaTime)` instead of Unity's `Update()`, so the bot can run fixed-step simulations as fast as the CPU allows, without a render loop.
- Outcome-affecting tuning is kept separate from feel and animation settings (see [Core vs View](#core-vs-view---what-lives-where--why)), and is passed into the core explicitly. This lets bots vary the values that change simulation results while ignoring presentation-only values.
- Cross-layer communication does not pass `MonoBehaviour` references across the `GameUnity` boundary. Everything outside `GameUnity` communicates through IDs and engine-agnostic data, keeping the rest of the codebase Unity-free.

- The bot harness is a second composition root over the same core, not a separate reimplementation of the game rules. The Unity scene and the bot runner execute the same model through different entry points.

### Methodology

The project is structurally inspired by Voodoo's *This Is Blast!*, a published hybrid-casual title using the same family of mechanics: tap-to-dispatch shooters into firing slots, color-matched auto-fire, and color merges. Working from an existing shipped game rather than inventing a new mechanic was a deliberate choice, and the goal was not surface-level imitation: the aim was to study the reference game's behavior closely enough to infer the architectural decisions that could produce it.


In practice, that meant slowing the reference game down, isolating subsystem timings (fire cadence, merge resolution, projectile travel, tray arrival), and reconstructing what the underlying state model probably looked like from how the game responded under edge cases. Two things came out of that:

1. **Architectural decisions could be grounded in observed behavior, not guessed.** The clean separation between game logic and visual presentation, for example, was not applied just because it is a textbook rule. Slowing the reference game down made the seam visible: gameplay state appears to change the moment a shooter fires, while projectile travel behaves like visual feedback running on its own timeline. This project mirrors that separation: Core resolves the hit immediately, and the Unity view catches up later through projectile animation. That gap between logical resolution and presentation timing is the seam preserved between `Blast.Core.*` and the Unity view layer.
2. **Production-style constraints became visible.** Reproducing specific behaviors forced the implementation to respect constraints that are easy to miss when designing only from a mechanic description: merges should depend on logical arrival rather than animation completion, fire should continue without waiting for projectile travel, and visual feedback should follow the simulation rather than drive it. See [Engineering Highlights](#engineering-highlights) for how each is enforced.

This project does not aim to be a complete game. It is a vertical slice of a hybrid-casual gameplay core, written to be **inspected**.

---

## Gameplay

The player faces a board of colored cubes and a reserve of pre-placed shooters arranged in columns. The core loop:

1. **Tap a reserve column:** the front-most shooter is dispatched to the next free slot in the launch tray.
2. **Merge:** when three shooters of the same color are in the tray, they merge into one: the middle shooter stays and absorbs the other two's ammo, and the other two leave, freeing their slots. Merging is therefore both an ammo bonus and a tray-management mechanic; freeing slots is what keeps new shooters coming and stops the player from running out of tray space.
3. **Auto-fire:** an active shooter automatically targets the bottom-most cube of any column whose color matches it, on a fixed fire cooldown, until its ammo runs out and it leaves the tray.
4. **Board resolution:** on a hit, the bottom cube is destroyed, the column shifts down by one row, and the cube above takes its place at the bottom.

**Targeting** uses a round-robin per-color policy: each color remembers the last column it hit and continues from the next one, so fire fans out across the board rather than concentrating on a single column.

**Resolving a level.** A level is won when the board is fully cleared. It is lost when no progress is possible: no shooter in the tray can fire at anything and no new shooter can enter because the tray is full. Levels are played in a fixed catalog order; the active level is persisted across sessions, advances on a win, and is replayed on a loss.

---

## Tech Stack

### Game (Unity)

| Area | Technology |
|---|---|
| Engine | Unity, C# |
| Animation | DOTween |
| Serialization | Newtonsoft.Json |
| Input | Unity Input System |
| Pooling | `UnityEngine.Pool.ObjectPool<T>` |
| UI text | TextMesh Pro |

### Bot Harness

| Area | Technology |
|---|---|
| Runtime | .NET 9 console app (`net9.0`) |
| Serialization | Newtonsoft.Json |

The harness compiles the Unity-free gameplay, level, logging, and bot sources directly from `Assets/Scripts`.


---

## Architecture

**The architecture starts from one strict constraint: the gameplay core must be able to run without Unity.**

The Unity runtime path is organized into three main layers with a strict **compile-time dependency rule** enforced by assembly definitions: `Blast.Core` owns the gameplay rules, `Blast.GamePresentation` coordinates those rules from engine-agnostic code, and `Blast.GameUnity` contains the Unity-specific adapters. Supporting assemblies such as `Blast.Level`, `Blast.Logging`, and `Blast.Bot` stay Unity-free as well.

`Blast.Core` and `Blast.GamePresentation` never reference Unity. Presenters depend on `Blast.Core` and the output interfaces in `Blast.GamePresentation.Contract`; Unity views implement those interfaces in `Blast.GameUnity.View`, keeping presenters independent from concrete Unity view classes.

Inside the presentation layer, the structure is classic **Model-View-Presenter (Passive View)**: presenters own presentation flow, read or command the relevant Core logic, and talk to the screen only through contract interfaces. That render seam applies the **Dependency Inversion Principle** at the view boundary.

Input flows inward through `InputHandler → GamePresenter`, while frame updates flow through `GameFlowController → GamePresenter.Tick(dt)`. Core reports outward by enqueuing domain events on `GameEventQueue`, which `GamePresenter` drains each tick and dispatches through the contract interfaces.

The same Unity-free Core also backs the headless bot/simulation harness, so the gameplay model is not coupled to Unity's scene, frame loop, or rendering APIs.

### High-Level View

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│  Blast.GameUnity.*                                                               │
│                                                                                  │
│  .Boot / .Input / .UI ...                             .View                      │
│  ──────────────────────                            ──────────────────            │
│  GameBootstrapper     composition root             BoardView                     │
│  GameFlowController   per-frame driver             LaunchTrayView                │
│  InputHandler         tap input                    ShooterReserveView            │
│  LevelEndView         win/lose overlay             ProjectileLauncher            │
└──────────┬───────────────────────────────────────────────┬───────────────────────┘
           │ ▾ Tick(dt) · tap column                       │ 
           │ ▴ LevelCompleted / LevelFailed                ▼ (.View implements .Contract)
┌──────────┴───────────────────────────────────────────────┴───────────────────────┐
│  Blast.GamePresentation.*   (engine-agnostic)                                    │
│                                                                                  │
│  .Presenter                                        .Contract                     │
│  ──────────────────────                            ──────────────────            │
│  GamePresenter                                     IBoardView                    │
│  BoardPresenter             drives ports           ILaunchTrayView               │
│  LaunchTrayPresenter      ────────────────────>    IShooterReserveView           │
│  ShooterReservePresenter                           IProjectileLauncher           │
└──────────┬───────────────────────────────────────────────────────────────────────┘
           │ ▾ Tick(dt) · SendShooterToLaunchTray
           │ ▴ drains GameEventQueue each tick
┌──────────┴───────────────────────────────────────────────────────────────────────┐
│  Blast.Core.*   (engine-agnostic)                                                │
│                                                                                  │
│  GameplayLogic · BoardLogic · LaunchTrayLogic · ShooterReserveLogic              │
│  TargetSelector · FireCoordinator · LevelConditionEvaluator                      │
│                                    │ enqueue                                     │
│                                    ▼                                             │
│                               GameEventQueue                                     │
└──────────────────────────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Namespace | Role | Unity-aware? |
|---|---|---|
| `Blast.Core.Data` | Plain data structures for cubes, shooters, tray, and reserve. | ❌ |
| `Blast.Core.Config` | Game-affecting tuning (`CoreConfig`: fire cooldown, arrival duration), injected at each composition root. | ❌ |
| `Blast.Core.Logic` | Domain simulation: board state, tray merging, shooter cooldowns, target selection, fire coordination, win/loss evaluation. | ❌ |
| `Blast.Core.Event` | Domain events (`ShooterSentEvent`, `ShootersMergedEvent`, `ShooterFiredEvent`, ...) and the event queue. | ❌ |
| `Blast.Level` | Engine-agnostic level data and JSON parsing, shared by Unity and the bot harness. | ❌ |
| `Blast.Logging` | Engine-agnostic logging abstraction plus a static facade, configured by the active composition root. | ❌ |
| `Blast.GamePresentation.Contract` | Presentation output interfaces (`IBoardView`, `ILaunchTrayView`, `IProjectileLauncher`, ...). | ❌ |
| `Blast.GamePresentation.Presenter` | Translates logic state and events into view-side calls. Coordinates animation/logic synchronization. | ❌ |
| `Blast.GameUnity.View` | Unity adapters that implement the contract interfaces with `MonoBehaviour`s, DOTween, prefabs, object pooling, and UI text. | ✅ |
| `Blast.GameUnity.Boot` | Unity-side composition and flow: `GameBootstrapper` wires the object graph; `GameFlowController` owns tick flow and level transitions. | ✅ |
| `Blast.Bot.*` | Unity-free headless simulation layer: runner, observations, bot policies, batch execution, metrics, and replay recording. | ❌ |
| `Blast.Bot.Cli` | .NET 9 console entry point under `Tools/`: loads levels, parses CLI options, runs single/batch simulations, and writes CSV/replay outputs. | ❌ |

---

## Core vs View - What Lives Where & Why

Core is the source of truth for gameplay. It owns the state and rules that can change the outcome of a session: shooter dispatch, tray arrival, merging, cooldowns, target selection, hits, and win/loss resolution.

View reacts to Core, but does not drive it. Projectile speed, animation durations, colors, layout, and visual polish can change how the game feels, but they cannot change what the simulation decides. This one-way dependency is what makes bot testing meaningful: the headless bot can drop the entire View layer and still exercise the same gameplay model.

> If changing a value changes the outcome of a session, it belongs in Core. If changing it only changes how the session feels, it belongs in View.

### Game-affecting (Core)

| Parameter               | Where | Why it affects outcome |
| --- | --- | ---- |
| `FireCooldown`| `CoreConfig`, injected into `ShooterReserveLogic`| Controls how often an active shooter can fire. If the cooldown is short relative to tray-arrival timing, shooters may drain their ammo and leave the tray before the player can set up a 3-merge. |
| `ArrivalDuration`| `CoreConfig`, assigned to tray slots by `LaunchTrayLogic` | A shooter becomes merge-eligible only after arriving. Faster arrival makes triples easier to form; slower arrival makes timing stricter because shooters may deplete before the merge.            |
| Target-selection policy | `TargetSelector`| Uses round-robin per-color memory to decide which column each shooter clears. Changing this policy changes solvability. |

### Feel-affecting (View)

| Parameter | Where | Why it does **not** affect outcome |
| --- | --- | --- |
| Projectile travel speed | `ProjectileView.speed` | The hit is already committed in logic when the shot is fired (see [Engineering Highlights](#engineering-highlights)). The on-screen projectile is purely visual; slower or faster, the level resolves identically. |
| Cube drop / reserve shift / merge animation durations | `BoardView.dropDuration`, `LaunchTrayView._mergeAnimationDuration`, reserve shift settings | These determine perceived responsiveness but do not feed back into any logic decision. |
| Cube colors, grid background, layout spacing | `CubeColorPalette`, `BoardView`, `ShooterReserveView`| Pure presentation. |

This split has two practical consequences:

1. **A bot harness can vary the game-affecting parameters and trust that the simulation outcome reflects the design**; it doesn't need to fake or stub animations.
2. **Designers and engineers can tune feel independently** without risk of accidentally changing a level's difficulty. A producer can ask for "snappier projectiles" and the engineer changes one number in View, with no ripple into Core.

---

## Design Patterns

The codebase deliberately uses well-known patterns where they earn their place. Each entry points to concrete code that can be inspected directly.

| Pattern | Where | Purpose |
|---|---|---|
| **Model-View-Presenter (MVP)** | `Core.Logic` / `GamePresentation.Presenter` / `GameUnity.View` | Three-layer separation of state, orchestration, and rendering. |
| **Ports & Adapters boundary** | `GamePresentation.Contract` (output ports) ↔ `GameUnity.View` (adapters) | Unity rendering sits behind output ports: Core emits events, presenters call the ports, and Unity-side adapters implement them. Nothing in Core references Unity. The headless harness skips the ports entirely and reads Core state through `GameObservation`. |
| **Composition Root** | `GameBootstrapper.Awake()` / `HeadlessGame` | `GameBootstrapper` composes the Unity scene's object graph by hand; the headless side wires its core graph in `HeadlessGame`. No DI container. |
| **Event Queue / Domain Events** | `GameEventQueue`, `IGameEvent`, events in `Core.Event` | Logic emits intent ("Shooters Merged"); consumers decide how to render or record it. |
| **Object Pool** | `ProjectileViewPool` | Avoids per-shot `Instantiate`/`Destroy`, reducing allocation pressure during repeated firing. |
| **Registry** | `ShooterViewRegistry`, `PolicyRegistry` | Maps an ID or name to an instance: shooter views by `ShooterId`, bot policies by name. |
| **Strategy** | `IBotPolicy`, `StochasticPolicy` | Bot decision-making lives behind one interface, selected by name via `PolicyRegistry`; new policies plug in without touching the runner. |
| **Selector / Encapsulated Rule** | `TargetSelector` | Isolates per-color target selection from firing logic, so the targeting rule can evolve without spreading through Core. |
| **Observer (C# `event`)** | `ShooterLogic.Depleted` | Slot logic auto-clears when a shooter's ammo hits zero. |
| **Composite** | `IRunObserver`, `CompositeObserver` | `CompositeObserver` fans one run's decisions, events, and results out to several observers, such as metrics and replay, without the runner knowing about either. |
| **Tick-based simulation** | `GameplayLogic.Tick`, `LaunchTrayLogic.Tick`, `ShooterLogic.Tick`, ... | Advances gameplay through explicit step functions instead of tying Core logic to Unity frame callbacks. |

---

## Engineering Highlights

A few decisions in the codebase that go beyond a textbook example.

### 1. Pre-emptive logical hits, lazy visual resolution

When a shooter fires, the board state is updated **immediately** in `BoardLogic.LogicalHit`; the cube is considered destroyed *before* the projectile has visually arrived. The on-screen cube continues to exist until the projectile reaches it, but the logic is already one step ahead.

This is in service of **gameplay fluidity**. If another shooter wants to fire before the first projectile lands, it does not wait on animation time: the target-selection logic already sees the updated board state and can pick the next valid target. The result is that fire feels continuous instead of stuttering on projectile travel.

To keep the view side in sync, `BoardPresenter` keeps a per-column FIFO queue of pending hits:

```csharp
public void EnqueueHit(int column, int hitLogicalRow) { ... }

public void OnProjectileArrived(int column) {
    int hitLogicalRow = _pendingHitsPerColumn[column].Dequeue();
    ResolveHitInternal(column, hitLogicalRow);
}
```

Logical time is fully decoupled from animation time: the simulation never waits on the renderer, and the renderer resolves each arriving projectile against the next queued hit for that column.

### 2. Visible-row vs total-row separation

`BoardLogic` operates on a `totalRows × columns` grid, while the view only renders `visibleRows`. When a visible cube is removed from the bottom of a column, the view recycles that visual slot and the presenter feeds it the next color from the logical grid:

```csharp
CubeColor? newTopColor = newTopDataRow < _board.GetColumnTop(column)
    ? _board.GetDataAt(newTopDataRow, column).Color
    : (CubeColor?)null;
```

This lets level data be taller than the visible board without changing the rendering code.

### 3. Pooling-ready domain events

The event queue stores reference types rather than structs. A `Queue<IGameEvent>` over value-typed events would box on every enqueue/dequeue, since `IGameEvent` is an interface and any value-typed implementation would have to be boxed to be stored as one.

The current implementation is **not yet GC-free**; each event is still a heap allocation. The choice of reference types is deliberate: it keeps the event surface compatible with a future pooled event allocator, where event instances can be reused without changing call sites.

### 4. Round-robin with per-color memory

`TargetSelector` keeps a per-color cursor of the last column it hit and starts the next search from `cursor + 1`. Successive shots fan across the board rather than hammering the same column, which is both visually nicer and easier to balance for level design.

### 5. Allocation-free merge scan

`LaunchTrayLogic.TryMergeAll()` runs every tick to find three arrived shooters of the same color. Because this is a hot path, it uses stack-allocated buffers instead of heap collections:

```csharp
Span<int> count = stackalloc int[ColorCount];
Span<int> firstThree = stackalloc int[ColorCount * TripleSize];
```

`firstThree` is a flat 1D buffer used like a tiny 2D table: `color * TripleSize + n` stores the slot index of the n-th arrived shooter for that color. This avoids temporary lists or nested collections, so the **per-tick scan** creates no GC allocations.

---

## Bot Harness & Headless Simulation

The bot harness drives the same gameplay core as the Unity game, from the command line. `HeadlessGame` is a Unity-free second composition root for `Blast.Core.*`: it wires the same logic graph the Unity bootstrapper builds, and `SimulationRunner` advances it with a fixed `Tick(dt)` as fast as the CPU allows.

A bot policy reads the game through a read-only `GameObservation` and decides which reserve column to tap. Merging, firing, target selection, and win/loss resolution are all handled by the same Core used by the Unity game — not by a reimplementation of the rules.

Every run is classified as Win / Lose / Timeout and reported with per-run metrics: taps, sends, shots, merges, tray-full stalls, cubes cleared, cubes remaining, and reserve exhaustion. Each run also carries a deterministic event-stream fingerprint, making runs reproducible and comparable across code or tuning changes.

### Run modes

* Single run — one level, one policy, one seed.
* Seed sweep — one level over N seeds, used to estimate win rate.
* Batch — the full cross-product of levels × policies × seeds × repeat, with live progress: throughput, ETA, and running W/L/T counts.

### Observation modes

Two observation modes are wired through `GameObservation`:

* `oracle` exposes the full internal state.
* `fair` restricts observations toward player-available information, such as visible board rows and limited reserve depth.

This boundary is in place so future information-limited policies can be evaluated against omniscient ones without changing the simulation core.

### Outputs

* Console summary — outcome, metrics, and aggregate win rate.
* CSV metrics (`--csv DIR`) — a versioned `events.csv` + `runs.csv` pair, shaped for downstream aggregate analysis.
* Replay log (`--record path.json`) — seed, tap trace, core config, level hash, and event fingerprint: a self-describing record of a run, intended to support replay and verification tooling.

### Example: measuring level difficulty

A win-rate sweep is the level-design signal this project is built around. On an i9-13900HX, 1,000 stochastic runs complete in roughly a second per level:

```bash
dotnet run --project Tools/Blast.Bot.Cli -- Level_001 --policy stochastic --seeds 1000
```

```text
batch: 1 level(s) × 1 policy(ies) × 1000 seed(s) × 1 repeat = 1000 runs  [mode=oracle, dt=0.016666668]
done: 1000 runs in 1.1s (874 runs/s)  W208 L792 T0  win-rate=0.208
```

```bash
dotnet run --project Tools/Blast.Bot.Cli -- Level_002 --policy stochastic --seeds 1000
```

```text
batch: 1 level(s) × 1 policy(ies) × 1000 seed(s) × 1 repeat = 1000 runs  [mode=oracle, dt=0.016666668]
done: 1000 runs in 1.3s (799 runs/s)  W43 L957 T0  win-rate=0.043
```

The stochastic baseline clears `Level_001` 20.8% of the time and `Level_002` only 4.3% of the time. Even with a simple baseline policy, the harness is already separating levels by difficulty, which is exactly the kind of signal a level-design loop needs before expensive manual playtesting.


---

## Project Structure


```
Assets/Scripts/
├── Core/          # Game simulation and rules
├── Level/         # Level data and parsing
├── Logging/       # Logging abstraction
├── Bot/           # Headless simulation runner, observation modes, policies, metrics, replay
├── Presentation/  # Presenters and view contracts
└── GameUnity/     # Unity-specific views, input, animations, bootstrapping

Tools/Blast.Bot.Cli/     # .NET 9 headless CLI project
```

The headless CLI compiles only the Unity-free code — Core, Level, Logging, and Bot — directly from Assets/Scripts.


---

## Getting Started

### Requirements

#### Game (Unity)
- Unity 6.3 LTS (6000.3.10f1)
- Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`)
- DOTween
- Unity Input System
- TextMesh Pro

#### Bot Harness
- .NET 9 SDK

### Run the game
1. Clone the repository and open the project in Unity.
2. Open the game scene (`Assets/Scenes/GameScene.unity`).
3. Press **Play**. The loaded level comes from the saved progress index; on a fresh install, the first catalog entry is used. Winning advances to the next level, losing replays the current one.
> A [playable engineering demo](https://abturhan.itch.io/cube-blast-demo) is also available on itch.io.
### Run the headless bot
The same gameplay core can be driven without Unity through the bot harness: single runs, seed sweeps, batch runs, CSV metrics, and deterministic replay logs. See [Bot Harness](#bot-harness--headless-simulation).

```bash
# from the repo root

# build
dotnet build Tools/Blast.Bot.Cli -c Release

# single run
dotnet run --project Tools/Blast.Bot.Cli -- Level_001 --policy stochastic --seed 0

# win-rate sweep: single level, 1000 seeds
dotnet run --project Tools/Blast.Bot.Cli -- Level_001 --policy stochastic --seeds 1000

# batch: all levels × available policies × 100 seeds, CSV metrics written to ./out
dotnet run --project Tools/Blast.Bot.Cli -- --levels all --policies all --seeds 100 --csv out
```


---

## Level Data Format

Levels are authored as JSON and parsed by `LevelParser`.

```json
{
  "columns": 6,
  "totalRows": 2,
  "visibleRows": 2,
  "launchTrayCapacity": 5,
  "rows": [
    { "colors": ["Red", "Blue", "Green", "Yellow", "Red", "Blue"] },
    { "colors": ["Blue", "Blue", "Red", "Red", "Green", "Yellow"] }
  ],
  "reserveColumns": [
    {
      "shooters": [
        { "color": "Red", "ammo": 10 },
        { "color": "Blue", "ammo": 20 }
      ]
    }
  ]
}
```

`StringEnumConverter` is registered so colors can be authored as strings rather than numeric enum values. Rows are reversed on load so JSON can be written top-down while the simulation indexes from the bottom.

---
## Scope & Status

This project is a vertical slice of a hybrid-casual gameplay core, supported by headless tooling built around the same simulation. It is an engineering-focused portfolio project rather than a shippable game; that boundary is intentional.

**In scope and addressed:**
- Engine-agnostic core simulation that can run both from the Unity project and from a headless command-line runner.
- Clean separation between game-affecting and feel-affecting parameters — game-affecting tuning such as fire cooldown and tray arrival duration lives in an injected `CoreConfig`, passed explicitly by both the Unity bootstrapper and the headless runner.
- Event-driven flow between the core simulation and presentation code, backed by an allocation-conscious event queue.
- Minimum viable mechanic loop: dispatch, arrival, merge, auto-fire, and board resolution.
- Core-owned win/loss resolution, with presentation code reacting to simulation events instead of recomputing game rules.
- Headless bot harness (`Blast.Bot.*`) that runs the same core from the command line through a second composition root.
- Seeded single-run and batch simulation with Win / Lose / Timeout classification, CSV outputs, and replay-log recording.

**Deliberately out of scope for this snapshot:**
- Visual polish, juice, and "game feel" presentation. The current renders are functional, not finished; there is no particle work, impact feedback, or animation-curve tuning for satisfaction. This is a conscious tradeoff: time was spent on simulation architecture, bot tooling, and deterministic validation rather than presentation polish.
- Content breadth. A single mechanic is implemented; no obstacles, special blocks, boosters, economy, or live-ops features.

**Planned next:**
- Two additional bot policies beyond the stochastic baseline.
- Replay playback. Recording is implemented (seed, tap trace, core config, level hash, event-stream fingerprint); the missing half is a driver that replays a log headlessly to re-verify the fingerprint, and optionally plays it back in the Unity scene for visual inspection.
- A level-design pipeline: procedural level generation -> batch bot simulation -> metric analysis of the harness CSV output in Python. The CSV schema is already versioned and shaped for aggregate analysis.


---

## Author

**Ali Bahadır Turhan**

abahadirt[at]gmail.com  
[LinkedIn](https://www.linkedin.com/in/ali-bahadir-turhan/)

---