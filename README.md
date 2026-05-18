# Cube Blast - Hybrid Casual Cube Shooter

> A Unity portfolio project built around one specific production capability: **an engine-agnostic gameplay core that can be driven by an automated bot, without spinning up Unity**. Hybrid-casual success is largely a function of level-design quality, and level-design quality is largely a function of how cheaply you can simulate thousands of plays. This codebase is shaped around making that simulation possible.

<!-- TODO image or gif...
<p align="center">
  <img src="docs/gameplay.gif" alt="Gameplay preview" width="320"/>
</p>
-->
---

## Table of Contents

- [Project Intent](#project-intent)
- [Gameplay](#gameplay)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Core vs View - What Lives Where & Why](#core-vs-view--what-lives-where--why)
- [Design Patterns](#design-patterns)
- [Engineering Highlights](#engineering-highlights)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Level Data Format](#level-data-format)
- [Scope & Status](#scope--status)

---

## Project Intent

### Why an engine-agnostic core?

In hybrid-casual puzzle games, sustained retention and monetization track directly with level-design quality: difficulty curves, solvability, intended-strategy validation, and pacing. Studios calibrate these with **automated bot playtesting** - running thousands of headless simulations to estimate win rates, detect unintended exploits, and tune parameters before any human sees the level.

That requirement was the first design constraint of this project, before any Unity code was written. Concretely:

- The entire simulation in `Blast.Core.*` has **zero `UnityEngine` references**. It compiles in a plain C# project with no Unity SDK.
- The simulation advances via an explicit `Tick(deltaTime)` rather than `Update()`, so a bot can run it at arbitrary speed (or step-by-step) without a render loop.
- All gameplay-affecting parameters live in the core, separated from feel/animation parameters (see [Core vs View](#core-vs-view--what-lives-where--why)); so bots can vary the parameters that actually matter and ignore the ones that don't.
- Cross-layer communication uses IDs and value types, never `MonoBehaviour` references, which keeps the surface area for a bot driver clean.

A bot harness is not part of this snapshot, but adding one means writing a driver that calls the same logic methods the Unity scene already calls; nothing has to be moved or refactored.

### Methodology

The project is structurally inspired by **Voodoo's *This Is Blast!***; a published hybrid-casual title using the same family of mechanics (tap-to-dispatch shooters into firing slots, color-matched auto-fire, color merges). Studying an existing shipped game rather than inventing a new mechanic was a deliberate choice, and the goal was not surface-level imitation: the aim was to reproduce the reference game's **behavior** closely enough that the implementation choices a studio would have made surfaced naturally.

In practice that meant slowing the reference game down, isolating subsystem timings (fire cadence, merge resolution, projectile travel, tray arrival), and reconstructing what the underlying state model probably looked like from how the game responded under edge cases. Two things came out of that:

1. **Architectural decisions could be validated, not guessed.** The clean separation between game logic and visual presentation, for example, wasn't applied because it's a textbook rule; slowing the reference game down made it clear that fire timing, merge resolution, and projectile travel were three independent subsystems, which strongly suggested the original codebase had drawn a similar line. The structure of this project mirrors that.
2. **Studio-flavored implementation patterns became visible.** Reproducing specific behaviors (e.g., why fire feels continuous and never stutters on projectile travel; see [Engineering Highlights](#engineering-highlights)) forced design choices that a from-scratch reimplementation would likely have missed.

This project does not aim to be a complete game. It is a vertical slice of a hybrid-casual gameplay core, written to be **inspected**.

---

## Gameplay

The player faces a board of colored cubes and a reserve of pre-placed shooters arranged in columns. The core loop:

1. **Tap a reserve column** → the front-most shooter is dispatched to the next free slot in the **launch tray**.
2. **Merge mechanic** → whenever **three same-color shooters** sit in the tray simultaneously, the middle one survives and absorbs the others' ammo. Merging is therefore both an ammo bonus *and* a tray-management mechanic: it frees up slots so new shooters can be dispatched. Strategic merging is what keeps the player from running out of tray space.
3. **Auto-fire** → arrived, active shooters automatically target the bottom-most cube in any column whose color matches them.
4. **Board resolution** → on hit, the cube is destroyed, the column shifts down by one row, and the cube above takes its place at the bottom.

Targeting uses a **round-robin per color** policy (each color remembers its last hit column and continues from the next one), which produces an even visual distribution rather than concentrated fire on a single column.

---

## Tech Stack

| Area | Technology |
|---|---|
| Engine | Unity (C#) |
| Animation | [DOTween](http://dotween.demigiant.com/) |
| Serialization | Newtonsoft.Json (Unity package) |
| Input | Unity Input System (new) |
| Pooling | `UnityEngine.Pool.ObjectPool<T>` |
| UI text | TextMesh Pro |

---

## Architecture

The codebase is organized into three top-level concerns, with a strict dependency rule: **Core ← Presenter ← View. Core never depends on Unity. Presenters never instantiate Unity types.** The shape combines **Hexagonal Architecture (Ports & Adapters)** at the application boundary with classic **Model-View-Presenter (MVP)** inside the presentation layer: the output ports live in the `Contract` namespace, and the Unity-side adapters in `GameUnity.View` implement them.

### High-Level View

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Unity Scene (MonoBehaviours)                  │
│                                                                      │
│   InputHandler ───tap column──► GameBootstrapper (Composition Root)  │
│                                          │                           │
│                                          │ wires & forwards          │
│                                          ▼                           │
└──────────────────────────────────────────│───────────────────────────┘
                                           │
        ┌──────────────────────────────────▼───────────────────────────┐
        │             Presentation Layer  (engine-agnostic)            │
        │                                                              │
        │                       GamePresenter                          │
        │       ┌──────────────┬───────┴───────┬──────────────┐        │
        │       │ Board        │ Tray          │ Reserve      │        │
        │       │ Presenter    │ Presenter     │ Presenter    │        │
        │       └──────┬───────┴──────┬────────┴──────┬───────┘        │
        │              │              │               │                │
        │  (depends on presentation output interfaces from Contract)   │
        └──────────────│──────────────│───────────────│────────────────┘
                       │              │               │
                  IBoardView    ILaunchTrayView   IShooterReserveView
                       │              │               │
        ┌──────────────▼──────────────▼───────────────▼────────────────┐
        │               View Layer  (Unity adapters)                   │
        │     BoardView   LaunchTrayView   ShooterReserveView   ...    │
        └──────────────────────────────────────────────────────────────┘

        ┌──────────────────────────────────────────────────────────────┐
        │                  Core Logic  (engine-agnostic)               │
        │                                                              │
        │                       GameplayLogic                          │
        │     ┌───────────┬──────────────┬────────────┬────────────┐   │
        │     │ Board     │ LaunchTray   │ Reserve    │ Fire       │   │
        │     │ Logic     │ Logic        │ Logic      │ Coordinator│   │
        │     └─────┬─────┴──────┬───────┴────────────┴─────┬──────┘   │
        │           │            │                          │          │
        │           └────────────┴──────────┬───────────────┘          │
        │                                   ▼                          │
        │                          GameEventQueue                      │
        │           (Logic side enqueues domain events;                │
        │            GamePresenter dequeues and dispatches each tick)  │
        └──────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Namespace | Role | Unity-aware? |
|---|---|---|
| `Blast.Core.Data` | Plain data structures (`CubeData`, `ShooterData`, `LaunchTrayData`, ...). | ❌ |
| `Blast.Core.Logic` | Domain simulation: board state, tray merging, shooter cooldowns, target selection, fire coordination. | ❌ |
| `Blast.Core.Event` | Domain events (`ShooterSentEvent`, `ShootersMergedEvent`, `ShooterFiredEvent`, ...) and the queue. | ❌ |
| `Blast.GamePresentation.Contract` | Presentation output interfaces (`IBoardView`, `ILaunchTrayView`, `IProjectileLauncher`, ...). | ❌ |
| `Blast.GamePresentation.Presenter` | Translates logic state and events into view-side calls. Owns animation/logic sync. | ❌ |
| `Blast.GameUnity.View` | Unity adapters that implement the contract interfaces with `MonoBehaviour`s, DOTween, prefabs, Object Pool. | ✅ |
| `Blast.GameUnity.Boot` | `GameBootstrapper`: the single composition root. | ✅ |
| `Blast.Logging` | `ILog` abstraction + static facade (configured at boot with `UnityLogger` in-engine, swappable for tests). | ❌ |

---

## Core vs View - What Lives Where & Why

A central design rule in this codebase is the distinction between **game-affecting parameters** and **feel-affecting parameters**. Bot testing only works if these two categories are not entangled; otherwise a bot cannot say anything meaningful about the level, because the level's behavior depends on values an animation system happens to use.

The rule applied here:

> **If changing a value changes the outcome of a session, it belongs in Core. If changing it only changes how the session feels, it belongs in View.**

### Game-affecting (Core)

| Parameter | Where | Why it affects outcome |
|---|---|---|
| `ShooterData.FireCooldown` | `Core.Data` | Determines how many shots a shooter gets off before it depletes, which gates merge windows. If the cooldown is short relative to the tray-arrival rate, the player has no realistic chance to set up a 3-merge before active shooters drain their ammo and despawn. |
| Tray arrival duration | `LaunchTraySlotLogic` (`_data.arrivalDuration`) | A shooter only becomes a merge candidate once it has **arrived**. Faster arrival → more reliable merging; slower → more reliance on reading the queue. |
| Target-selection policy | `TargetSelector` | Round-robin per-color memory: directly determines which column each shooter clears. Changing the policy changes solvability. |

### Feel-affecting (View)

| Parameter | Where | Why it does **not** affect outcome |
|---|---|---|
| Projectile travel speed | `ProjectileView._speed` | The hit is already committed in logic when the shot is fired (see [Pre-emptive hits](#engineering-highlights) below). The on-screen ball is purely visual; slower or faster, the level resolves identically. |
| Cube drop / shift / merge animation durations | `BoardView.dropDuration`, `LaunchTrayView._mergeAnimationDuration`, reserve shift settings | These determine perceived responsiveness but do not feed back into any logic decision. |
| Cube colors, grid background, layout spacing | `CubeColorPalette`, `BoardView`, `ShooterReserveView` | Pure render. |

This split has two practical consequences:

1. **A bot harness can vary the game-affecting parameters and trust that the simulation outcome reflects the design**; it doesn't need to fake or stub animations.
2. **Designers and engineers can tune feel independently** without risk of accidentally changing a level's difficulty. A producer can ask for "snappier projectiles" and the engineer changes one number in View, with no ripple into Core.

---

## Design Patterns

The codebase deliberately uses well-known patterns. Each entry points to a concrete file so the implementation can be inspected directly.

| Pattern | Where | Purpose |
|---|---|---|
| **Model-View-Presenter (MVP)** | `Core.Logic` / `Presentation.Presenter` / `GameUnity.View` | Three-layer separation of state, orchestration, and rendering. |
| **Hexagonal Architecture (Ports & Adapters)** | `Presentation.Contract` (output ports) ↔ `GameUnity.View` (adapters) | The application core never talks to Unity directly; it talks to ports, and Unity-side adapters implement those ports. Lets the same core be driven by a different adapter: e.g. a headless bot. |
| **Composition Root / Pure DI** | `GameBootstrapper.Awake()` | All dependency wiring happens in one place. |
| **Event Queue (Domain Events)** | `GameEventQueue`, `IGameEvent`, events in `Core.Event` | Logic emits intent ("Shooters Merged"); presenter decides how to render. |
| **Object Pool** | `ProjectileViewPool` | Avoids per-shot `Instantiate`/`Destroy`, reducing GC pressure on mobile. |
| **Registry** | `ShooterViewRegistry` | Maps logical `ShooterId` → `ShooterView` so the presenter can address views by ID. |
| **Strategy** | `TargetSelector` | Encapsulates "which column do I shoot next?" behind a single interface. The reference game shows that this policy can become non-trivial; for example, shooters sometimes intentionally miss the "correct" target to nudge the player toward a winnable line. Isolating the policy in one class keeps that kind of feature additive rather than invasive. |
| **Observer (C# `event`)** | `ShooterLogic.Depleted` | Slot logic auto-clears when ammo hits zero. |
| **Tick-based simulation** | `GameplayLogic.Tick`, `LaunchTrayLogic.Tick`, `ShooterLogic.Tick`, ... | Deterministic, frame-agnostic step function. |
| **Facade** | `GamePresenter` | Single entry point for the bootstrapper; hides multi-presenter wiring. |

---

## Engineering Highlights

A few decisions in the codebase that go beyond a textbook example.

### 1. Pre-emptive logical hits, lazy visual resolution

When a shooter fires, the board state is updated **immediately** in `BoardLogic.LogicalHit`, the cube is considered destroyed *before* the projectile has visually arrived. The on-screen cube continues to exist until the projectile reaches it, but the logic is already one step ahead.

This is in service of **gameplay fluidity**. If a second shooter wants to fire at the same column, it doesn't have to wait for the first projectile to land, the target-selection logic already knows that column's bottom cube is "spoken for", and the second shooter picks the *next* cube above. The result is that fire feels continuous instead of stuttering on round-trip animation time.

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

`BoardLogic` operates on a `totalRows × columns` grid (the full level), while the view only renders `visibleRows`. When a row clears, the view recycles its bottom cube up to the top and the presenter feeds it the next color from the logical grid:

```csharp
CubeColor? newTopColor = newTopDataRow < _board.GetColumnTop(column)
    ? _board.GetDataAt(newTopDataRow, column).Color
    : (CubeColor?)null;
```

Level data can be arbitrarily tall without changing the rendering code.

### 3. Boxing-free event queue

The event queue stores reference types (classes) rather than structs. A `Queue<IGameEvent>` over value-typed events would box on every enqueue/dequeue, since `IGameEvent` is an interface and any value-typed implementation would have to be boxed to be stored as one.

The current implementation is **not yet GC-friendly**; each event is still a heap allocation, but the choice of reference types is deliberate: it positions the code to move to a **pooled event allocator** later (reusing instances instead of allocating new ones) without changing any call site. On mobile, where this genre is consumed, that path matters; this is the first step on it.

### 4. Round-robin with per-color memory

`TargetSelector` keeps a per-color cursor of the last column it hit and starts the next search from `cursor + 1`. Successive shots fan across the board rather than hammering the same column, which is both visually nicer and easier to balance for level design.

---

## Project Structure

```
Assets/
└── Scripts/
    ├── Core/
    │   ├── Data/           ← Plain data (CubeData, ShooterData, ...)
    │   ├── Event/          ← GameEventQueue, IGameEvent, concrete events
    │   └── Logic/          ← BoardLogic, LaunchTrayLogic, FireCoordinator, ...
    │
    ├── Presentation/
    │   ├── Contract/       ← IBoardView, ILaunchTrayView, IProjectileLauncher, ...
    │   └── Presenter/      ← GamePresenter, BoardPresenter, LaunchTrayPresenter, ...
    │
    ├── GameUnity/
    │   ├── Boot/           ← GameBootstrapper (composition root)
    │   ├── Input/          ← InputHandler
    │   ├── Logging/        ← UnityLogger
    │   ├── Pool/           ← ProjectileViewPool
    │   ├── Registry/       ← ShooterViewRegistry
    │   └── View/           ← BoardView, LaunchTrayView, ShooterReserveView, ...
    │
    └── Logging/            ← ILog + Log facade
```

---

## Getting Started

### Requirements

- Unity **6.3 LTS (6000.3.10f1)**
- Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`)
- DOTween (free or Pro)
- Input System package

### Run

1. Clone the repository.
2. Open the project in Unity.
3. Open the scene `Assets/Scenes/SampleScene.unity`.
4. Assign a level JSON file to `GameBootstrapper → Level Json File` if not already wired.
5. Press Play.

---

## Level Data Format

Levels are authored as JSON and parsed at boot.

```json
{
  "columns": 6,
  "totalRows": 20,
  "visibleRows": 8,
  "launchTrayCapacity": 5,
  "rows": [
    { "colors": ["Red", "Blue", "Green", "Yellow", "Red", "Blue"] },
    { "colors": ["Blue", "Blue", "Red", "Red", "Green", "Yellow"] }
  ],
  "reserveColumns": [
    { "shooters": [
        { "color": "Red", "ammo": 10 },
        { "color": "Blue", "ammo": 20 }
    ]}
  ]
}
```

`StringEnumConverter` is registered so colors can be authored as strings rather than indices. Rows are reversed on load so JSON reads top-down while the simulation indexes from the bottom.

---

## Scope & Status

This project is a **vertical slice of a hybrid-casual gameplay core**, written as an engineering artifact rather than a finished, shippable game. The boundary is intentional.

**In scope and addressed:**
- Engine-agnostic core simulation
- Clean separation between game-affecting and feel-affecting parameters
- Event-driven architecture with a boxing-free event queue
- The minimum-viable feature set of the core mechanic: dispatch, arrival, merge, auto-fire, board resolution

**Deliberately out of scope for this snapshot:**
- **Visual polish, juice, and "game feel" presentation.** The current renders are functional, not finished; there is no particle work, no impact feedback, no animation curves tuned for satisfaction. This is the most visible thing the project is *not* doing, and that is a conscious tradeoff: time was spent on the parts that are not visible in a screenshot.
- **Content breadth.** A single mechanic is implemented; no obstacles, special blocks, boosters, or progression systems.
- **Bot harness.** The architecture supports one; building one is the next step.

**Planned next:**
- Lose condition: tray full with no valid target for any active shooter.
- A pooled allocator for `IGameEvent` instances.
- A headless bot driver that calls the existing `GameplayLogic.Tick` loop, demonstrating the engine-agnostic claim in practice.

---

## Author

**Ali Bahadır Turhan**

abahadirt[at]gmail.com  
[LinkedIn](https://www.linkedin.com/in/ali-bahadir-turhan/)
