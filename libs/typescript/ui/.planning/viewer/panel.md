# [UI_PANEL]

The one wire-materializer of the shell plane: three C#-minted vocabularies — the livewire triple (`BindingStatus`/`CoercedValue`/`WriteReceipt`, `csharp:Rasm.AppHost`), the closed `ControlIntent` union, and the ordered `LayoutProgram` (`csharp:Rasm.AppUi/Shell`) — arrive decoded through `core/interchange/codec#LANDING_WIRE` and materialize through one owner with one shape: wire value → total fold or exhaustive dispatch → rows → emit. `Panel.fold` accumulates the livewire feed into per-binding board rows with a receipt-reconciled optimistic round trip; `Panel.route` closes a union-derived handler record so every control kind lands on exactly one owning plane; `Panel.solve` re-solves the C#-authored Cassowary program to bit-identical positions with edit-variable drag over the frozen program. Payloads are carriage on every arm — a clamp, remap, local default, or synthesized constraint is the cross-language drift defect — and a new wire case is one row here with the compile error at the missing row as the growth mechanism. The module is `ui/viewer/src/panel.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                            | [PUBLIC] |
| :-----: | :-------------- | :-------------------------------------------------------------------------------- | :------- |
|  [01]   | `EVENT_FOLD`    | the keyed livewire fold and the receipt-reconciled optimistic round trip          | `Panel`  |
|  [02]   | `PHASE_RENDER`  | the phase tone vocabulary, the coercion diff, the stamp and value-presenter rows  | `Panel`  |
|  [03]   | `CONTROL_SINKS` | the union-derived handler record, exhaustive routing, intent egress, availability | `Panel`  |
|  [04]   | `LAYOUT_SOLVE`  | the wire-order kiwi fold, edit-variable drag, the four-axis determinism law       | `Panel`  |

## [02]-[EVENT_FOLD]

[EVENT_FOLD]:
- Owner: `Panel.fold` — the keyed accumulator: the event feed (a `Stream` of the decoded triple the app wires from its transport, entering the view plane through the atom bridge) folds into a `HashMap<binding, Panel.Row>` where each event's arm updates exactly its slots — `BindingStatus` advances `phase` (clearing the optimistic slot on `refused`/`detached`), `CoercedValue` records the offered→landed pair with its path, `WriteReceipt` lands the value and `Hlc` stamp and clears the optimistic slot; the fold is total over the union by `Match.valueTags` — the one-shot record dispatch over the held event.
- Packages: `@rasm/ts/core` (`BindingStatus`, `CoercedValue`, `WriteReceipt`, `Hlc`); `effect` (`Chunk`, `HashMap`, `Match`, `Option`, `Stream`); `@effect-atom/atom-react` (the board atom rides `system/atom#STORE_ROOT`).
- Law: the row is the panel's whole truth — `phase`, `landed`, `optimistic`, `coercion`, `stamp`; a panel component reads one row through an `Atom.family` keyed by binding name and re-renders only on its own row's change.
- Law: writes are optimistic against the feed — a panel edit writes the intent through the app-wired write port AND stamps the row's optimistic slot; the display shows `optimistic` over `landed` while present, the reconciling `WriteReceipt` clears it, and a `refused` status clears it with the refusal surfaced through the `view/form` field-error seam. Round trips are receipt-driven, never awaited-then-assumed — the feed is the truth channel, the write port's acknowledgement only gates re-submission, and display state always derives from the fold.
- Law: this board is the wire-receipt optimistic plane — `system/atom`'s `Atom.optimistic` reconciles against an effect's own `Result` and never appears here; the two optimism laws share a name, never a mechanism, and the board rides the one store like any other atom.
- Law: stale optimism ages out — an optimistic slot older than the patience window (`_PATIENCE`, a `Duration` policy row) degrades to the in-flight affordance without reverting, keeping slow transports honest without fabricating failure.
- Law: unknown-value payloads stay opaque — `offered`/`landed` are `Schema.Unknown` on the wire by design; the panel renders them through one value-presenter row, never assuming shape.
- Law: bursts coalesce before the store — `Panel.drain` shapes the feed with `Stream.groupedWithin(events, 128, "16 millis")`, folds each chunk through the SAME `_fold`, and lands one atom write per window inside `Atom.batch`, so a livewire storm costs one notification pass per frame; `Stream.throttle` composes on the same rail where a transport demands rate-shaping, and a per-event atom write is the named defect.
- Law: imperative drivers read atomically — a non-React consumer (the solve seam, a test harness) reads and advances the board through `registry.modify(atom, f)`, value and next state in one step, never a get-then-set pair.
- Boundary: the feed's transport and decode are `core`/app composition; the write path is C#-owned and this module emits intents only; the telemetry timeline a panel renders over its own event history is `view/chart#SERIES_SURFACE` material — rows here, series there.

```typescript
import type { BindingStatus, CoercedValue, Hlc, WriteReceipt } from "@rasm/ts/core"
import { Chunk, Duration, Effect, HashMap, Match, Option, Stream } from "effect"

type PanelEvent = BindingStatus | CoercedValue | WriteReceipt

declare namespace Panel {
  type Phase = BindingStatus["phase"]
  type Row = {
    readonly phase: Panel.Phase
    readonly landed: Option.Option<unknown>
    readonly optimistic: Option.Option<{ readonly value: unknown; readonly since: Hlc }>
    readonly coercion: Option.Option<{ readonly offered: unknown; readonly landed: unknown; readonly path: string }>
    readonly stamp: Option.Option<Hlc>
  }
  type Board = HashMap.HashMap<string, Row>
}

const _PATIENCE = Duration.seconds(4)

const _EMPTY: Panel.Row = {
  phase: "detached",
  landed: Option.none(),
  optimistic: Option.none(),
  coercion: Option.none(),
  stamp: Option.none(),
}

const _fold = (board: Panel.Board, event: PanelEvent): Panel.Board =>
  HashMap.modifyAt(board, event.binding, (slot) => {
    const row = Option.getOrElse(slot, () => _EMPTY)
    return Option.some(
      Match.valueTags(event, {
        BindingStatus: (status): Panel.Row => ({
          ...row,
          phase: status.phase,
          optimistic: status.phase === "refused" || status.phase === "detached" ? Option.none() : row.optimistic,
        }),
        CoercedValue: (coerced): Panel.Row => ({
          ...row,
          coercion: Option.some({ offered: coerced.offered, landed: coerced.landed, path: coerced.path }),
        }),
        WriteReceipt: (receipt): Panel.Row => ({
          ...row,
          landed: Option.some(receipt.landed),
          optimistic: Option.none(),
          stamp: Option.some(receipt.stamp),
        }),
      }),
    )
  })

const _optimistic = (board: Panel.Board, binding: string, value: unknown, since: Hlc): Panel.Board =>
  HashMap.modifyAt(board, binding, (slot) =>
    Option.some({
      ...Option.getOrElse(slot, () => _EMPTY),
      optimistic: Option.some({ value, since }),
    }))

const _drain = (
  events: Stream.Stream<PanelEvent>,
  commit: (fold: (board: Panel.Board) => Panel.Board) => void,
): Effect.Effect<void> =>
  Stream.runForEach(
    Stream.groupedWithin(events, 128, Duration.millis(16)),
    (window) => Effect.sync(() => commit((board) => Chunk.reduce(window, board, _fold))),
  )
```

## [03]-[PHASE_RENDER]

[PHASE_RENDER]:
- Owner: `Panel.tone` — the phase styling vocabulary: one `as const` table keyed by the closed phase axis carrying tone, glyph, and motion rows (`refused` pulses a `Motion` row, `coercing` shows the in-flight affordance); the phase chip, the coercion diff (offered versus landed with the path as a breadcrumb), and the receipt stamp (`Format.instant` on the `Hlc`'s wall half, `system/intl`) are the three display rows every binding panel composes.
- Law: phase keys the table — `satisfies Record<Panel.Phase, ...>` so a wire vocabulary change breaks this row at compile time; a phase conditional in a panel body marks the table unused.
- Law: a coercion is information, not an error — the diff renders as neutral evidence (the C# side coerced and landed the write); only `refused` renders on the danger tone and feeds the round-trip revert.
- Boundary: chip/badge primitives are `system/primitive` recipes; plural and status text is `Message`'s (`system/intl`).

```typescript
const _tone = {
  bound: { tone: "success", motion: Option.none<string>() },
  coercing: { tone: "accent", motion: Option.none<string>() },
  refused: { tone: "danger", motion: Option.some("pulse") },
  detached: { tone: "neutral", motion: Option.none<string>() },
} as const satisfies Record<Panel.Phase, { readonly tone: "neutral" | "accent" | "success" | "danger"; readonly motion: Option.Option<string> }>
```

## [04]-[CONTROL_SINKS]

[CONTROL_SINKS]:
- Owner: `Panel.route` — the derived dispatch: `Panel.Sinks` is one mapped handler record computed from the wire union (the record's key space IS the union's tag space, so a new case breaks the record loudly at compile time), and `Panel.route(sinks)` closes it as the reusable terminal — `Match.tagsExhaustive` over the sinks record is the only place intent cases meet handlers.
- Packages: `@rasm/ts/core` (`ControlIntent` — the closed union, `_tag`-stamped at the decode seam); `effect` (`Match`).
- Law: each case has exactly one owning plane — `Orbit`/`Pan` mint `Camera.Intent` values through `geo#CAMERA`'s adapters (a yaw/pitch or dx/dy delta becomes an `EaseTo` over the live camera state); `Select` mints `Selection.Op` (`additive` selects `Add` versus `Replace` — modality in the op value, `mark`'s law); `Focus` mints a fit intent over the target's bounds; `Section` and `Measure` land on the scene-tool rows `scene` earns — one sink, one plane, never a case handled twice.
- Law: sinks are app-composed — the shell binds each sink to the owning plane's atom write at composition; this module never imports the planes, because the record IS the seam and a direct plane import couples the panel to every surface it drives.
- Law: payloads are carriage — `yaw`/`pitch`, `dx`/`dy`, `targets`/`additive`, section `origin`/`normal`, measure `from`/`to`, focus `target` land verbatim on the sink; an out-of-range value is upstream evidence. Routing is replayable — every arriving intent lands as an op/intent value on a plane's fold, composing with `History` undo and the probe plane exactly like locally-minted interaction.
- Law: interactions emit values, never calls — a panel affordance mints an egress record (the wire tag with its payload) written to the app-wired command gateway; the gateway owns encode and transport, and this module never encodes, never names a transport. Affordance state rides atoms — active tool, additive modifier, in-flight measure endpoint live in `Atom.family` rows keyed by control id, RAC components running controlled. Availability gates render — the gateway's availability verdict projects into the `isDisabled` prop of every affordance, so an unavailable command renders inert with its reason as tooltip evidence rather than failing on press.
- Law: egress records publish once on the `rasm.ui.panel.egress` hook point (`system/hook`, observe modality) as they reach the command gateway — telemetry taps, probe boards, and replay journals consume the record stream without wrapping the gateway, and the gateway stays the one transport owner.
- Growth: a new control kind = one wire case (C# emits it, the codec mirrors it) + one handler row here + zero dispatch edits.

```typescript
import type { ControlIntent } from "@rasm/ts/core"

declare namespace Panel {
  type Kind = ControlIntent["_tag"]
  type Arm<K extends Panel.Kind> = Extract<ControlIntent, { readonly _tag: K }>
  type Sinks = { readonly [K in Panel.Kind]: (intent: Panel.Arm<K>) => void }
  // correlated mapped union: each egress record carries exactly its kind's wire payload, never an erased slot
  type Egress = { readonly [K in Panel.Kind]: { readonly kind: K; readonly payload: Omit<Panel.Arm<K>, "_tag"> } }[Panel.Kind]
}

const _route = (sinks: Panel.Sinks): ((intent: ControlIntent) => void) =>
  Match.type<ControlIntent>().pipe(Match.tagsExhaustive(sinks))
```

## [05]-[LAYOUT_SOLVE]

[LAYOUT_SOLVE]:
- Owner: `Panel.solve(program)` — the one fold: walk `program.constraints` in received order, minting each `Variable` at FIRST APPEARANCE (an interior name→`Variable` ledger — first-appearance order is the wire's variable order by construction), fold each constraint's `terms` into an `Expression`, map the closed `relation` vocabulary onto `Operator` and the closed `strength` vocabulary onto the `Strength` constants, `addConstraint` in order, register `program.edits` as edit variables at `Strength.strong` (sub-required by kiwi's own law), run `updateVariables()`, and read every variable's `value()` into the positions map.
- Packages: `@lume/kiwi` (`Variable`, `Expression`, `Operator`, `Constraint`, `Strength`, `Solver`); `@rasm/ts/core` (`LayoutProgram`); `effect` (`Data`, `Effect`, `HashMap`, `Iterable`, `SynchronizedRef`).
- Fault: `Panel.Fault` — an unsatisfiable required set throws inside kiwi; the fold catches it into the one tagged fault carrying the surface name and the offending constraint's zero-based rank (`-1` when the refusal lands past the constraint walk — edit registration, the initial solve, or a live `suggest`) — a program-construction defect surfaced as operator evidence, never retried. `maxIterations` stays at kiwi's default — a pathological program fails loud through the iteration cap; tuning it to make a bad program pass hides the upstream defect.
- Law: the fold inserts, never authors — no constraint is synthesized, reordered, re-strengthened, or dropped; TS-side layout intelligence is the drift defect this cluster's existence guards against. Drag is suggestion, never structure — a pointer drag feeds `suggest(edit, value)` per frame (the gesture source is `system/act#CONTINUOUS_OWNER`), the frozen program re-optimizes incrementally, and only wire-enumerated edits are suggestible — a suggestion against a non-edit variable is a construction error kiwi rejects, surfaced through the same fault.
- Law: the four determinism axes are fixed by construction — identical constraint SET, identical insertion ORDER, identical STRENGTHS, identical EDIT sequence — so the TS tableau converges to the C# tableau; equal-strength competition resolves identically because insertion order is preserved. Drift is evidence, not tolerance — a position mismatch against a C#-provided expectation reports with the variable name and both values (`probe` consumes it); a fuzzy-match re-solve loop is the named defect.
- Law: positions flow to render as one atom write per settle — the returned map replaces the positions atom (`Atom.batch` coalesces multi-panel updates), and panel components read their own cell through a selector so a 60fps drag never re-renders the board.
- Law: the live solver is a RESOURCE, not a kernel — kiwi's incremental `suggestValue` requires the solver and its variable ledger to persist for the `Solved` lifetime, so the draft lives inside one `SynchronizedRef` and every `suggest` routes through `SynchronizedRef.modifyEffect`: concurrent suggestions serialize by construction, no mutable reference escapes, and the sole egress is the immutable positions map; the construction walk is the marked boundary seam.
- Growth: a new constraint kind, variable class, or strength tier is a C# solver change mirrored at the codec — the fold's vocabulary maps grow a row each, nothing else moves.

```typescript
import { Constraint, Expression, Operator, Solver, Strength, Variable } from "@lume/kiwi"
import type { LayoutProgram } from "@rasm/ts/core"
import { Data, Effect, HashMap, Iterable, SynchronizedRef } from "effect"

const _relations = { le: Operator.Le, ge: Operator.Ge, eq: Operator.Eq } as const

const _strengths = {
  required: Strength.required,
  strong: Strength.strong,
  medium: Strength.medium,
  weak: Strength.weak,
} as const

class SolveFault extends Data.TaggedError("SolveFault")<{
  readonly surface: string
  readonly rank: number
  readonly detail: string
}> {}

declare namespace Panel {
  type Positions = HashMap.HashMap<string, number>
  type Solved = {
    readonly positions: Panel.Positions
    readonly suggest: (edit: string, value: number) => Effect.Effect<Panel.Positions, SolveFault>
  }
}

type _Draft = { readonly solver: Solver; readonly cells: ReadonlyMap<string, Variable> }

const _read = (draft: _Draft): Panel.Positions =>
  HashMap.fromIterable(Iterable.map(draft.cells, ([name, cell]) => [name, cell.value()] as const))

const _build = (program: LayoutProgram): Effect.Effect<_Draft, SolveFault> =>
  Effect.suspend(() => {
    // BOUNDARY ADAPTER
    const cursor = { rank: -1 }
    return Effect.try({
      try: () => {
        const solver = new Solver()
        const cells = new Map<string, Variable>()
        const named = (name: string): Variable => {
          const held = cells.get(name) ?? new Variable(name)
          cells.set(name, held)
          return held
        }
        program.constraints.forEach((row, at) => {
          cursor.rank = at
          const terms = row.terms.map((term): [number, Variable] => [term.coefficient, named(term.variable)])
          const lhs = new Expression(...terms, row.constant)
          solver.addConstraint(new Constraint(lhs, _relations[row.relation], undefined, _strengths[row.strength]))
        })
        cursor.rank = -1
        program.edits.forEach((edit) => solver.addEditVariable(named(edit), Strength.strong))
        solver.updateVariables()
        return { solver, cells }
      },
      catch: (defect) => new SolveFault({ surface: program.surface, rank: cursor.rank, detail: String(defect) }),
    })
  })

const _solve = (program: LayoutProgram): Effect.Effect<Panel.Solved, SolveFault> =>
  Effect.gen(function* () {
    const draft = yield* _build(program)
    const held = yield* SynchronizedRef.make(draft)
    return {
      positions: _read(draft),
      suggest: (edit, value) =>
        SynchronizedRef.modifyEffect(held, (live) =>
          Effect.try({
            try: () => {
              // BOUNDARY ADAPTER
              const cell = live.cells.get(edit)
              if (cell === undefined) {
                throw new Error(edit)
              }
              live.solver.suggestValue(cell, value)
              live.solver.updateVariables()
              return [_read(live), live] as const
            },
            catch: (defect) => new SolveFault({ surface: program.surface, rank: -1, detail: String(defect) }),
          })),
    }
  })

declare namespace Panel {
  type Shape = {
    readonly Fault: typeof SolveFault
    readonly empty: Panel.Row
    readonly patience: typeof _PATIENCE
    readonly fold: typeof _fold
    readonly optimistic: typeof _optimistic
    readonly drain: typeof _drain
    readonly tone: typeof _tone
    readonly route: typeof _route
    readonly relations: typeof _relations
    readonly strengths: typeof _strengths
    readonly solve: typeof _solve
  }
}

const Panel: Panel.Shape = {
  Fault: SolveFault,
  empty: _EMPTY,
  patience: _PATIENCE,
  fold: _fold,
  optimistic: _optimistic,
  drain: _drain,
  tone: _tone,
  route: _route,
  relations: _relations,
  strengths: _strengths,
  solve: _solve,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Panel }
```
