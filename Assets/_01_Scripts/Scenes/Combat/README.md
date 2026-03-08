# Combat Architecture Quick Guide

## Purpose

This document explains the combat architecture in a few minutes, so you can:

* understand the main structure quickly
* know where to add new **GameActions** and **Effects**
* avoid breaking the separation between runtime logic and presentation

---

## 1. Mental model

The combat scene is split into three responsibilities:

### Domain / Runtime state

Owns the game state and game rules.

Examples:

* cards
* combatants
* enemies
* perks
* effects
* intents
* stats / statuses

This layer should answer questions like:

* What happens when a card is played?
* How much damage is dealt?
* Which target is valid?
* Which perk is active?

This layer should **not** know about:

* `CardView`
* `EnemyView`
* `ArrowView`
* `GameObject`
* transforms / hover / animation details
* UI widgets

### Application / Orchestration

Coordinates use cases and gameplay flow.

Examples:

* play a card
* resolve an enemy attack
* draw / discard a card
* trigger actions in sequence
* publish combat events

This layer is the bridge between state changes and scene flow.
It is allowed to coordinate steps, but it should still work with gameplay concepts like:

* `CombatantId`
* `Card`
* `Perk`
* events
* effect contexts

It should not directly manipulate scene views.

### Presentation

Owns everything visual and input-related.

Examples:

* card visuals
* enemy visuals
* hover previews
* manual targeting arrow
* animation sequences
* VFX / SFX
* hand UI / mana UI / perk UI

Presentation listens to gameplay events and updates the scene.

---

## 2. Dependency direction

The safe dependency direction is:

**Domain -> Application -> Presentation**

In practice, the important rule is:

* runtime logic must not depend on concrete view classes
* presentation may react to runtime/application events

Good:

* application publishes `CardPlayedEvent`
* presentation animates the card play

Bad:

* runtime service returns `EnemyView`
* application service directly updates `PerksUI`

---

## 3. Event-driven flow

A lot of combat flow works through the combat event bus.

Typical sequence:

1. gameplay logic changes state
2. application publishes a domain/application event
3. presentation router listens
4. presentation updates visuals

Example:

1. card is played
2. `CardPlayedEvent` is published
3. presentation plays animation / VFX
4. runtime continues after the presentation sync point

This keeps the architecture decoupled:

* gameplay decides **what happened**
* presentation decides **how it looks**

---

## 4. What belongs where

### Add here: Domain / Runtime

Put code here when it changes game rules or state.

Examples:

* damage calculation
* status effect behavior
* card cost / block / attack values
* perk activation behavior
* target validation rules
* effect resolution

### Add here: Application

Put code here when it coordinates a use case.

Examples:

* resolve a play-card flow
* sequence multiple effects
* trigger events after state changes
* wait for action presentation to complete

### Add here: Presentation

Put code here when it is visual, input-based, or scene-specific.

Examples:

* hover preview
* drag and drop behavior
* target arrow and target highlighting
* card animations
* enemy spawn / death visuals
* perk UI elements
* VFX / SFX

---

## 5. Where to expand GameActions

GameActions should represent **gameplay commands / use cases**, not visual behavior.

A new `GameAction` is the right choice when you want to model a player or system decision such as:

* play a card
* end turn
* discard a card
* trigger an enemy action
* apply a generated effect chain

### Rules for GameActions

A `GameAction` should:

* read gameplay state
* modify gameplay state
* call runtime systems/services
* publish events if something important happened

A `GameAction` should not:

* spawn UI
* move transforms
* access `CardView` / `EnemyView`
* play animations directly
* manipulate hover / drag / targeting visuals

### Good example

`PlayCardGA`

* validate playability
* consume mana
* move card to the correct state in runtime
* resolve effects
* publish card-play related events

### Bad example

A `GameAction` that:

* moves a card object on screen
* toggles an arrow GameObject
* calls a UI method directly

If you catch yourself using a view class inside a `GameAction`, that is usually a sign the code belongs in Presentation.

---

## 6. Where to expand Effects

Effects should describe **what they do to gameplay**, not how they are shown.

Examples of good effects:

* deal damage
* gain block
* draw cards
* apply poison
* add or remove a perk
* generate another action or status

### Rules for Effects

Effects should:

* operate on gameplay entities and IDs
* use effect contexts / combat state
* modify state deterministically
* publish events only when needed to reflect important state transitions

Effects should not:

* know about views
* reference sprites / prefabs / scene objects
* trigger animations directly
* contain drag/drop/hover logic

### Good example

A damage effect:

* receives source and target IDs
* computes damage
* updates target HP
* publishes `DamageAppliedEvent`

Presentation can then decide:

* show hit VFX
* shake target
* show damage number

---

## 7. Current architectural guardrails

These are the rules we want to keep going forward.

### Guardrail 1

Use IDs, models, and events across runtime boundaries.

Prefer:

* `CombatantId`
* `Card`
* `Perk`
* event objects

Avoid passing:

* `EnemyView`
* `CardView`
* `ArrowView`
* `GameObject`

### Guardrail 2

Presentation reacts to events.

Prefer:

* runtime publishes `PerkAddedEvent`
* presentation router updates `PerksUI`

Avoid:

* runtime service directly calling UI components

### Guardrail 3

Input belongs to Presentation.

Examples:

* hover detection
* drag logic
* manual target picking
* arrow rendering

The result of input may be converted into gameplay data such as `CombatantId?`, but the input handling itself stays in Presentation.

### Guardrail 4

Keep synchronization abstract.

It is okay that gameplay sometimes waits for presentation to finish an action sequence.
But application code should depend on an abstraction or boundary, not on concrete visual implementation details.

---

## 8. Practical extension recipes

### Recipe A: Add a new card effect

Use this path:

1. add or extend the runtime effect class
2. update effect resolution if needed
3. publish an event only if presentation needs feedback
4. let presentation react separately

Example:

* new effect: `ApplyBurnEffect`
* runtime applies burn status
* publish status-changed or effect-applied event
* presentation shows burn VFX

### Recipe B: Add a new enemy action

Use this path:

1. define enemy intent / behavior in runtime
2. resolve the action in application/runtime
3. publish attack/action event
4. presentation animates it

### Recipe C: Add a new perk

Use this path:

1. create perk behavior in runtime
2. register/add the perk through perk runtime service
3. publish `PerkAddedEvent`
4. presentation updates perk UI

### Recipe D: Add a new targeting mode

Use this path:

1. keep input handling in presentation
2. convert selected object into gameplay data (`CombatantId?`, list of IDs, etc.)
3. pass only gameplay data into the action/effect layer

---

## 9. Red flags during code review

Stop and rethink the design if you see any of these in runtime/application code:

* references to `CardView`, `EnemyView`, `ArrowView`
* direct UI calls like `AddPerkUI()`
* `GameObject` or transform manipulation
* hover or drag logic in a gameplay class
* raycasts in application/runtime services
* animation or VFX code inside effects or GameActions

These usually mean presentation logic is leaking into runtime.

---

## 10. Fast checklist before merging

Before merging combat changes, ask:

1. Does this code change gameplay rules or only visuals?
2. Am I passing IDs/models, or am I passing views?
3. Did I publish an event instead of calling UI directly?
4. Could this still run in a headless test without scene objects?
5. If I remove the animation, does the gameplay still make sense?

If the answer to 4 or 5 is no, the code is probably too coupled to presentation.

---

## 11. One-sentence summary

**Gameplay decides what happens, presentation decides how it looks.**

That is the core rule that keeps the combat architecture extensible.
