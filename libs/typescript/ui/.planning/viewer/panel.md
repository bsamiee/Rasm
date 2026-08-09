# [UI_PANEL]

Panel materializes four shell vocabularies the AppUi shell and its host mint: the livewire binding triple, `CommandGate` verdicts, `ControlIntent` — the producer's whole widget vocabulary — and `LayoutProgram`. `Panel.fold` builds receipt-reconciled rows carrying binding slots beside the gate verdict, `Panel.admit` projects a row onto its affordance state, `Panel.chrome` projects a decoded widget onto the styling and affordance record a view row spreads, `Panel.route` exhaustively dispatches the viewer's own interaction vocabulary, and `Panel.solve` preserves Cassowary insertion order and strengths. Payloads remain verbatim carriage; missing wire cases fail at their row. Module: `ui/viewer/src/panel.ts`.

## [01]-[INDEX]

- [02]-[EVENT_FOLD]: the keyed shell fold, the gate slot, and the receipt-reconciled optimistic round trip; `Panel`.
- [03]-[PHASE_RENDER]: the phase and degradation tone axes, the affordance projection, the coercion diff and stamp rows; `Panel`.
- [04]-[WIDGET_RENDER]: the kind-exhaustive part-and-children fold, the emphasis ladder, the chrome projection; `Panel`.
- [05]-[CONTROL_SINKS]: the locally-minted interaction union, exhaustive routing, intent egress; `Panel`.
- [06]-[LAYOUT_SOLVE]: the wire-order kiwi fold, edit-variable drag, the four-axis determinism law; `Panel`.

## [02]-[EVENT_FOLD]

[EVENT_FOLD]:
- Owner: `Panel.fold` — the keyed accumulator: the event feed (a `Stream` of the decoded shell union the app wires from its transport, entering the view plane through the atom bridge) folds into a `HashMap<key, Panel.Row>` where each arm names its own key and updates exactly its slots — `BindingStatus` advances `phase` (clearing the optimistic slot on `refused`/`detached`), `CoercedValue` records the offered→landed pair with its path, `WriteReceipt` lands the value and `Hlc` stamp and clears the optimistic slot, `CommandGate` seats the `available`/`level` verdict; the fold is total over the union by `Match.valueTags` — the one-shot record dispatch over the held event — and every arm ends at `_at`, the one slot-seat combinator that also carries the optimistic stamp.
- Packages: `@rasm/ts/core` (`BindingStatus`, `CoercedValue`, `CommandGate`, `WriteReceipt`, `Hlc`); `effect` (`Chunk`, `HashMap`, `Match`, `Option`, `Stream`); `@effect-atom/atom-react` (the board atom rides `system/atom#STORE_ROOT`).
- Law: the row is the panel's whole truth — `phase`, `landed`, `optimistic`, `coercion`, `stamp`, `gate`; a panel component reads one row through an `Atom.family` keyed by cell name and re-renders only on its own row's change.
- Law: the ARM owns the key — the board's key space is the shell's addressable cell, and each arm names which cell its event addresses: the livewire triple addresses its `binding` path, `CommandGate` its `key`, the `CommandIntent` key the C# deck freezes. Both columns arrive on one control from the producer's own intent binding (`valueKey` beside `command`), so an affordance reads its value row and its gate row off the one board with no side map, no re-keying, and no second accumulator.
- Law: the gate is a row slot, never a second board — a control's whole display truth is one row, so `available` and `level` land beside the binding slots and the render projection is `[03]`'s alone; `level` is adopted through `CommandGate`'s own field, which derives from the ONE degradation vocabulary the `Availability` landing owns, so a producer level added at core breaks `[03]`'s degradation table at the declaration.
- Law: writes are optimistic against the feed — a panel edit writes the intent through the app-wired write port AND stamps the row's optimistic slot; the display shows `optimistic` over `landed` while present, the reconciling `WriteReceipt` clears it, and a `refused` status clears it with the refusal surfaced through the `view/form` field-error seam. Round trips are receipt-driven, never awaited-then-assumed — the feed is the truth channel, the write port's acknowledgement only gates re-submission, and display state always derives from the fold.
- Law: this board is the wire-receipt optimistic plane — `system/atom`'s `Atom.optimistic` reconciles against an effect's own `Result` and never appears here; the two optimism laws share a name, never a mechanism, and the board rides the one store like any other atom.
- Law: stale optimism ages out — an optimistic slot older than the patience window (`_PATIENCE`, a `Duration` policy row) degrades to the in-flight affordance without reverting, keeping slow transports honest without fabricating failure.
- Law: unknown-value payloads stay opaque — `offered`/`landed` are `Schema.Unknown` on the wire by design; the panel renders them through one value-presenter row, never assuming shape.
- Law: bursts coalesce before the store — `Panel.drain` shapes the feed with `Stream.groupedWithin(events, 128, "16 millis")`, folds each chunk through the SAME `_fold`, and lands one atom write per window inside `Atom.batch`, so a livewire storm costs one notification pass per frame; `Stream.throttle` composes on the same rail where a transport demands rate-shaping, and a per-event atom write is the named defect.
- Law: imperative drivers read atomically — a non-React consumer (the solve seam, a test harness) reads and advances the board through `registry.modify(atom, f)`, value and next state in one step, never a get-then-set pair.
- Boundary: the feed's transport and decode are `core`/app composition; the write path belongs to the shell producer and this module emits intents only; the telemetry timeline a panel renders over its own event history is `view/chart#SERIES_SURFACE` material — rows here, series there.

```typescript
import type { Clock, Wire } from "@rasm/ts/core"
import { Chunk, Duration, Effect, HashMap, Match, Option, Stream } from "effect"
import type { Motion } from "../../src/system/act.ts"
import type { Theme } from "../../src/system/token.ts"

type PanelEvent = Wire.BindingStatus | Wire.CoercedValue | Wire.WriteReceipt | Wire.CommandGate

declare namespace Panel {
  type Phase = Wire.BindingStatus["phase"]
  type Level = Wire.CommandGate["level"]
  type Row = {
    readonly phase: Panel.Phase
    readonly landed: Option.Option<unknown>
    readonly optimistic: Option.Option<{ readonly value: unknown; readonly since: Clock.Hlc }>
    readonly coercion: Option.Option<{ readonly offered: unknown; readonly landed: unknown; readonly path: string }>
    readonly stamp: Option.Option<Clock.Hlc>
    readonly gate: Option.Option<{ readonly available: boolean; readonly level: Panel.Level }>
  }
  type Board = HashMap.HashMap<string, Row>
}

const _PATIENCE = Duration.seconds(4)

// `detached` is the true seed for a gate-only row: a command key carries no livewire binding to be bound to
const _EMPTY: Panel.Row = {
  phase: "detached",
  landed: Option.none(),
  optimistic: Option.none(),
  coercion: Option.none(),
  stamp: Option.none(),
  gate: Option.none(),
}

const _at = (board: Panel.Board, key: string, step: (row: Panel.Row) => Panel.Row): Panel.Board =>
  HashMap.modifyAt(board, key, (slot) => Option.some(step(Option.getOrElse(slot, () => _EMPTY))))

const _fold = (board: Panel.Board, event: PanelEvent): Panel.Board =>
  Match.valueTags(event, {
    BindingStatus: (status) =>
      _at(board, status.binding, (row) => ({
        ...row,
        phase: status.phase,
        optimistic: status.phase === "refused" || status.phase === "detached" ? Option.none() : row.optimistic,
      })),
    CoercedValue: (coerced) =>
      _at(board, coerced.binding, (row) => ({
        ...row,
        coercion: Option.some({ offered: coerced.offered, landed: coerced.landed, path: coerced.path }),
      })),
    WriteReceipt: (receipt) =>
      _at(board, receipt.binding, (row) => ({
        ...row,
        landed: Option.some(receipt.landed),
        optimistic: Option.none(),
        stamp: Option.some(receipt.stamp),
      })),
    CommandGate: (gate) =>
      _at(board, gate.key, (row) => ({ ...row, gate: Option.some({ available: gate.available, level: gate.level }) })),
  })

const _optimistic = (board: Panel.Board, binding: string, value: unknown, since: Clock.Hlc): Panel.Board =>
  _at(board, binding, (row) => ({ ...row, optimistic: Option.some({ value, since }) }))

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
- Owner: `Panel.tone` and `Panel.degrade` — the two closed styling axes, with `Panel.admit` the affordance projection over the second: `_tone` keys the binding-phase axis carrying tone and motion rows (`refused` pulses a `Motion` row, `coercing` shows the in-flight affordance), `_degrade` keys the producer's degradation axis carrying the tone and whether the level is evidence at all; the phase chip, the coercion diff (offered versus landed with the path as a breadcrumb), the receipt stamp (`Format.instant` on the `Hlc`'s wall half, `system/intl`), and the gate badge are the display rows every binding panel composes.
- Law: each axis keys its own table — `satisfies Record<Panel.Phase, ...>` and `satisfies Record<Panel.Level, ...>`, so a wire vocabulary change on either axis breaks its own row at compile time and a phase or level conditional in a panel body marks the table unused. The axes never merge: phase is the binding's lifecycle, level is the host's degradation, and one joint table would multiply them into a cross product no producer emits.
- Law: `Panel.admit` is the one affordance read and it is TOTAL — `disabled` is the gate's `available` inverted, tone and badge visibility are its `level` row, and an EMPTY gate slot projects `_CLOSED`: inert, and silent about a level the producer never stated. Absence seeds CLOSED because the producer's own `CanExecute` stream seeds `false` before its first emission; seeding open renders every affordance live in the window before its first verdict lands, and the press then meets a deck that refuses.
- Law: CLOSED answers a MISSING verdict, never a missing command — a widget whose binding names no command key awaits nothing, so it projects `_OPEN` and the gate join runs on the command-bearing widgets alone; folding the two absences together renders every label, every readout, and every uncommanded field permanently inert.
- Law: a coercion is information, not an error — the diff renders as neutral evidence (the C# side coerced and landed the write); only `refused` renders on the danger tone and feeds the round-trip revert.
- Growth: a producer degradation level is one `_degrade` row and a binding phase one `_tone` row — the core vocabulary lands first, and each table fails at its declaration until its row exists.
- Boundary: chip/badge primitives are `system/primitive` recipes; plural and status text is `Message`'s (`system/intl`), so the badge's reason resolves from the level key through a catalog row and no level text is authored here.

```typescript
const _tone = {
  bound: { tone: "success", motion: Option.none<Motion.Hold>() },
  coercing: { tone: "accent", motion: Option.none<Motion.Hold>() },
  refused: { tone: "danger", motion: Option.some<Motion.Hold>("pulse") },
  detached: { tone: "neutral", motion: Option.none<Motion.Hold>() },
} as const satisfies Record<Panel.Phase, { readonly tone: Theme.Tone; readonly motion: Option.Option<Motion.Hold> }>

// the producer's rows in its own rank order; `full` is the undegraded floor and is the one level carrying no badge
const _degrade = {
  full: { tone: "neutral", evident: false },
  "reduced-remote": { tone: "accent", evident: true },
  "local-only": { tone: "accent", evident: true },
  "read-only": { tone: "caution", evident: true },
  suspended: { tone: "danger", evident: true },
} as const satisfies Record<Panel.Level, { readonly tone: Theme.Tone; readonly evident: boolean }>

declare namespace Panel {
  type Affordance = { readonly tone: Theme.Tone; readonly evident: boolean; readonly disabled: boolean }
}

const _CLOSED: Panel.Affordance = { ..._degrade.full, disabled: true }

// the uncommanded floor: no verdict is owed, so none is awaited
const _OPEN: Panel.Affordance = { ..._degrade.full, disabled: false }

const _admit = (row: Panel.Row): Panel.Affordance =>
  Option.match(row.gate, {
    onNone: () => _CLOSED,
    onSome: (gate) => ({ ..._degrade[gate.level], disabled: !gate.available }),
  })
```

## [04]-[WIDGET_RENDER]

[WIDGET_RENDER]:
- Owner: `Panel.arms` — the ONE kind-exhaustive fold over the decoded widget vocabulary, each arm answering both facts a mount needs: the part key the view row binds and the child intents the walk descends; `Panel.chrome(board, intent)` the projection joining that fold with the emphasis ladder, the icon slot, the motion hold, and the affordance the board's own rows carry; `_EMPHASIS` the producer's emphasis ladder read onto one tone plus one non-colour fill.
- Packages: `@rasm/ts/core` (`ControlIntent` — the decoded widget union, discriminated on the `kind` column its producer ships); `effect` (`Array`, `HashMap`, `Match`, `Option`); `../../src/system/token.ts` (`Theme.Tone`); `../../src/system/act.ts` (`Motion.Hold`).
- Law: ONE fold answers every per-kind question — a part table beside a children walk is two exhaustiveness proofs over one key space that drift the moment a kind lands in one and not the other, so `_arms` returns the pair and `Panel.part`/`Panel.children` are projections of it. A kind whose part narrows on its own posture, form, or temporal column reads that column off its own arm, because the narrowing belongs with the value carrying it.
- Law: the fold is TOTAL by the producer's own key space — `Match.discriminatorsExhaustive("kind")` over the decoded union means a kind the producer adds is a compile break at this table, never a render-time fallthrough, and the leaf arms are spelled rather than swept by a default precisely so a new NESTING kind cannot silently answer with no children.
- Law: emphasis resolves to TONE plus a non-colour FILL — the producer's six-row ladder is two axes at this head, and the folder's one-tone-per-element ruling means only the semantic axis reaches the palette while quiet, soft, inverted, and link postures ride the recipe's own variant; a second tone column would resolve two palettes onto one control.
- Law: this module holds NO styling literal — tone keys the token roster, fill keys a recipe variant `system/primitive` owns, motion keys a `Motion.Hold` row, and the class string is composed where the view row calls `Primitive.styled`; a hex value, a utility string, or a pixel here forks the authority `system/token` closes.
- Law: an icon is a SLOT, never a resolved image — the asset key, placement, and size cross verbatim and the app-wired asset source answers the glyph, so a decoded intent never carries pixels and both heads mount the same slot from the same three columns.
- Law: motion ranks refusal over work — a refused write pulses through the phase row even while its control is pending, because the repair the user owes outranks the work the host is doing; below it a pending icon spins and an indeterminate progress reads its form's own hold, and a determinate fraction holds nothing at all.
- Law: the gate join is the binding's two keys — `binding.command` addresses the verdict row and `binding.valueKey` the livewire row, both on the ONE board `[02]` folds, so a widget's whole display truth is two reads with no side map; a command-free widget projects `_OPEN` per `[03]`'s law and never awaits a verdict nobody will send.
- Law: payloads stay verbatim — a bound, a fraction, a visible count, and a window spec land as the producer wrote them; a clamp, a remap, or a head-local default for an omitted column forks the emitting peer's semantics, which is exactly what the every-field-has-a-wire-representation law at the producer boundary exists to make impossible.
- Boundary: mounting a part is the view layer's — this module is pure projection and imports no component, so the part key is the seam a view row resolves against its own registry; label, header, and hint TEXT resolves through `system/intl` from the key columns, and the constraint program named by a container arm is `[06]`'s to solve.
- Growth: a producer kind is one `_arms` row; a producer emphasis is one `_EMPHASIS` row; a new indeterminate progress form is one `_INDETERMINATE` row — zero dispatch edits and zero new surface.

```typescript
import type { Wire } from "@rasm/ts/core"
import { Array } from "effect"

declare namespace Panel {
  type Widget = Wire.ControlIntent["kind"]
  type Arm<K extends Panel.Widget> = Extract<Wire.ControlIntent, { readonly kind: K }>
  type Binding = Wire.ControlIntent["binding"]
  type Emphasis = Panel.Binding["emphasis"]
  type Fill = "solid" | "soft" | "ghost" | "inverted" | "link"
}

const _EMPHASIS = {
  quiet: { tone: "neutral", fill: "ghost" },
  secondary: { tone: "neutral", fill: "soft" },
  primary: { tone: "accent", fill: "solid" },
  danger: { tone: "danger", fill: "solid" },
  inverted: { tone: "neutral", fill: "inverted" },
  link: { tone: "accent", fill: "link" },
} as const satisfies Record<Panel.Emphasis, { readonly tone: Theme.Tone; readonly fill: Panel.Fill }>

// the posture, form, and temporal sub-tables: each closes over ONE arm's own column, so the narrowing that picks a
// part reads the value that carries it instead of a second keying of the whole widget space
const _COLOR = { inline: "ColorArea", flyout: "ColorPicker" } as const
const _SELECT = { closed: "Select", editable: "ComboBox" } as const
const _MULTI = { bound: "MultiSelect", free: "TagField" } as const
const _SEGMENT = { select: "ToggleButtonGroup", command: "Toolbar" } as const
const _CHIP = { static: "Tag", toggle: "ToggleButton", removable: "Tag" } as const
const _PROGRESS = { bar: "ProgressBar", ring: "Meter", skeleton: "Skeleton" } as const
const _TEMPORAL = { date: "DatePicker", time: "TimeField", datetime: "DatePicker", range: "DateRangePicker" } as const

// the indeterminate holds: a progress with no fraction reads its form's own sustained row
const _INDETERMINATE = { bar: "pulse", ring: "spin", skeleton: "pulse" } as const satisfies Record<
  Panel.Arm<"progress">["form"],
  Motion.Hold
>

const _NONE: ReadonlyArray<Wire.ControlIntent> = []

// the generic keeps each arm's part LITERAL, so `Panel.Part` derives from the fold instead of standing beside it
const _node = <P extends string>(part: P, children: ReadonlyArray<Wire.ControlIntent> = _NONE) => ({ part, children })

const _arms = Match.type<Wire.ControlIntent>().pipe(
  Match.discriminatorsExhaustive("kind")({
    button: () => _node("Button"),
    label: () => _node("Text"),
    textInput: (intent) => _node(intent.multiline ? "TextArea" : "TextField"),
    numberInput: () => _node("NumberField"),
    dateInput: (intent) => _node(_TEMPORAL[intent.temporalKind]),
    pathInput: () => _node("FileTrigger"),
    colorInput: (intent) => _node(_COLOR[intent.posture]),
    select: (intent) => _node(_SELECT[intent.posture]),
    multiSelect: (intent) => _node(_MULTI[intent.posture]),
    slider: () => _node("Slider"),
    range: () => _node("Slider"),
    toggle: () => _node("Switch"),
    radio: () => _node("RadioGroup"),
    segmented: (intent) => _node(_SEGMENT[intent.posture]),
    chip: (intent) => _node(_CHIP[intent.posture]),
    progress: (intent) => _node(_PROGRESS[intent.form]),
    avatar: () => _node("AvatarGroup"),
    breadcrumb: () => _node("Breadcrumbs"),
    tooltip: () => _node("Tooltip"),
    menu: () => _node("Menu"),
    // the nesting arms: the grid contributes its cell AND its editor, because an editing template a walk never
    // reaches is a column that cannot enter edit
    emptyState: (intent) => _node("EmptyState", intent.action === null ? _NONE : [intent.action]),
    grid: (intent) =>
      _node("Table", Array.flatMap(intent.columns, (column) => column.editor === null ? [column.cell] : [column.cell, column.editor])),
    tree: (intent) => _node("Tree", [intent.item]),
    toolbar: (intent) => _node("Toolbar", Array.map(intent.rows, (row) => row.item)),
    tab: (intent) => _node("Tabs", Array.map(intent.pages, (page) => page.body)),
    accordion: (intent) => _node("DisclosureGroup", Array.map(intent.sections, (section) => section.body)),
    panel: (intent) => _node("Group", intent.children),
    dock: (intent) => _node("Group", intent.regions),
    splitter: (intent) => _node("SplitPane", [intent.first, intent.second]),
  }),
)

declare namespace Panel {
  type Part = ReturnType<typeof _arms>["part"]
  type Chrome = {
    readonly part: Panel.Part
    readonly tone: Theme.Tone
    readonly fill: Panel.Fill
    readonly icon: Option.Option<NonNullable<Panel.Binding["icon"]>>
    readonly hold: Option.Option<Motion.Hold>
    readonly affordance: Panel.Affordance
    readonly value: Option.Option<Panel.Row>
  }
}

const _slot = (board: Panel.Board, key: string | null): Option.Option<Panel.Row> =>
  Option.flatMap(Option.fromNullable(key), (named) => HashMap.get(board, named))

const _pending = (intent: Wire.ControlIntent): Option.Option<Motion.Hold> =>
  intent.kind === "progress" && intent.fraction === null
    ? Option.some<Motion.Hold>(_INDETERMINATE[intent.form])
    : Option.as(
      Option.filter(Option.fromNullable(intent.binding.icon), (icon) => icon.pending !== null),
      "spin" as const,
    )

const _chrome = (board: Panel.Board, intent: Wire.ControlIntent): Panel.Chrome => {
  const emphasis = _EMPHASIS[intent.binding.emphasis]
  const value = _slot(board, intent.binding.valueKey)
  return {
    part: _arms(intent).part,
    tone: emphasis.tone,
    fill: emphasis.fill,
    icon: Option.fromNullable(intent.binding.icon),
    hold: Option.orElse(Option.flatMap(value, (row) => _tone[row.phase].motion), () => _pending(intent)),
    affordance: Option.match(_slot(board, intent.binding.command), { onNone: () => _OPEN, onSome: _admit }),
    value,
  }
}

const _children = (intent: Wire.ControlIntent): ReadonlyArray<Wire.ControlIntent> => _arms(intent).children
```

## [05]-[CONTROL_SINKS]

[CONTROL_SINKS]:
- Owner: `Panel.route` — the derived dispatch over `Panel.Interaction`, this viewer's OWN interaction vocabulary: `Panel.Sinks` is one mapped handler record whose key space IS the union's discriminant space, so a new case breaks the record loudly at compile time, and `Panel.route(sinks)` closes it as the reusable terminal — one exhaustive match is the only place interaction cases meet handlers.
- Packages: `effect` (`Match`); `../../src/system/hook.ts` (`Hook`).
- Law: this union is LOCALLY minted and wears no wire family name — no C# page produces an orbit, a section, or a measure, so the vocabulary homes at the surface that mints it and the `ControlIntentWire` name stays the producer's widget family; a consumer vocabulary wearing a producer's family name is the fabrication `typescript:core` `RULINGS.md` `[02]-[SHAPE]` forecloses.
- Law: one discriminant, spelled once — the union discriminates on `kind` exactly as the decoded widget family does, so this page carries one dispatch spelling and an egress record needs no tag-to-kind mapping on the way to the gateway.
- Law: each case has exactly one owning plane — `orbit`/`pan` mint `Camera.Intent` values through `geo#CAMERA`'s adapters (a yaw/pitch or dx/dy delta becomes an `EaseTo` over the live camera state); `select` mints `Selection.Op` (`additive` selects `Add` versus `Replace` — modality in the op value, `mark`'s law); `focus` mints a fit intent over the target's bounds; `section` and `measure` land on the scene-tool rows `scene` earns — one sink, one plane, never a case handled twice.
- Law: sinks are app-composed — the shell binds each sink to the owning plane's atom write at composition; this module never imports the planes, because the record IS the seam and a direct plane import couples the panel to every surface it drives.
- Law: payloads are carriage — `yaw`/`pitch`, `dx`/`dy`, `targets`/`additive`, section `origin`/`normal`, measure `from`/`to`, focus `target` land verbatim on the sink; an out-of-range value is upstream evidence. Routing is replayable — every interaction lands as an op/intent value on a plane's fold, composing with `History` undo and the probe plane exactly like a locally-recognized gesture.
- Law: interactions emit values, never calls — an affordance mints an egress record (the kind with its payload) written to the app-wired command gateway; the gateway owns encode and transport, and this module never encodes, never names a transport. Affordance state rides atoms — active tool, additive modifier, in-flight measure endpoint live in `Atom.family` rows keyed by control id, RAC components running controlled. Availability gates RENDER, never dispatch — a widget's `isDisabled` prop and its degradation badge are `Panel.chrome`'s affordance over that widget's own gate row, so an unavailable command renders inert with its level as tooltip evidence rather than failing on press, while `_route` stays ungated because an interaction carries no command key a verdict could match on.
- Law: egress records publish once on the `rasm.ui.panel.egress` hook point (`system/hook`, observe modality) as they reach the command gateway — this page contributes the point and `Panel.egress(registry, gateway)` composes publish-before-send on one Effect rail, so telemetry taps, probe boards, and replay journals never wrap the gateway.
- Growth: a new interaction is one union case plus one handler row; zero dispatch edits.

```typescript
import { Hook } from "../../src/system/hook.ts"

declare namespace Panel {
  type Vec3 = readonly [number, number, number]
  type Interaction =
    | { readonly kind: "orbit"; readonly yaw: number; readonly pitch: number }
    | { readonly kind: "pan"; readonly dx: number; readonly dy: number }
    | { readonly kind: "select"; readonly targets: ReadonlyArray<string>; readonly additive: boolean }
    | { readonly kind: "section"; readonly origin: Panel.Vec3; readonly normal: Panel.Vec3 }
    | { readonly kind: "measure"; readonly from: Panel.Vec3; readonly to: Panel.Vec3 }
    | { readonly kind: "focus"; readonly target: string }
  type Gesture = Panel.Interaction["kind"]
  type Reach<K extends Panel.Gesture> = Extract<Panel.Interaction, { readonly kind: K }>
  type Sinks = { readonly [K in Panel.Gesture]: (interaction: Panel.Reach<K>) => void }
  // correlated mapped union: each egress record carries exactly its kind's payload, never an erased slot
  type Egress = { readonly [K in Panel.Gesture]: { readonly kind: K; readonly payload: Omit<Panel.Reach<K>, "kind"> } }[Panel.Gesture]
}

declare module "../../src/system/hook.ts" {
  interface Points {
    readonly "rasm.ui.panel.egress": { readonly modality: "observe"; readonly payload: Panel.Egress }
  }
}

const _panelHook: Hook.Row<"rasm.ui.panel.egress"> = { modality: "observe", depth: 32, source: Option.none() }

const _egress = <E, R>(registry: Hook.Registry, gateway: (record: Panel.Egress) => Effect.Effect<void, E, R>) =>
  (record: Panel.Egress): Effect.Effect<void, E, R> =>
    Effect.zipRight(Effect.asVoid(Hook.publish(registry, "rasm.ui.panel.egress", record)), gateway(record))

const _route = (sinks: Panel.Sinks): ((interaction: Panel.Interaction) => void) =>
  Match.type<Panel.Interaction>().pipe(Match.discriminatorsExhaustive("kind")(sinks))
```

## [06]-[LAYOUT_SOLVE]

[LAYOUT_SOLVE]:
- Owner: `Panel.solve(program)` — the one fold: walk `program.constraints` in received order, minting each `Variable` at FIRST APPEARANCE (an interior name→`Variable` ledger — first-appearance order is the wire's variable order by construction), fold each constraint's `terms` into an `Expression`, map the closed `relation` vocabulary onto `Operator` and the closed `strength` vocabulary onto the `Strength` constants, `addConstraint` in order, register `program.edits` as edit variables at `Strength.strong` (sub-required by kiwi's own law), run `updateVariables()`, and read every variable's `value()` into the positions map.
- Law: the fold inserts, never authors — no constraint is synthesized, reordered, re-strengthened, or dropped; TS-side layout intelligence is the drift defect this cluster's existence guards against. Drag is suggestion, never structure — a pointer drag feeds `suggest(edit, value)` per frame (the gesture source is `system/act#CONTINUOUS_OWNER`), the frozen program re-optimizes incrementally, and only wire-enumerated edits are suggestible — a suggestion against a non-edit variable is a construction error kiwi rejects, surfaced through the same fault.
- Law: the four determinism axes are fixed by construction — identical constraint SET, identical insertion ORDER, identical STRENGTHS, identical EDIT sequence — so the TS tableau converges to the C# tableau; equal-strength competition resolves identically because insertion order is preserved. Drift is evidence, not tolerance — a position mismatch against a C#-provided expectation reports with the variable name and both values (`probe` consumes it); a fuzzy-match re-solve loop is the named defect.
- Law: positions flow to render as one atom write per settle — the returned map replaces the positions atom (`Atom.batch` coalesces multi-panel updates), and panel components read their own cell through a selector so a 60fps drag never re-renders the board.
- Law: the live solver is a RESOURCE, not a kernel — kiwi's incremental `suggestValue` requires the solver and its variable ledger to persist for the `Solved` lifetime, so the draft lives inside one `SynchronizedRef` and every `suggest` routes through `SynchronizedRef.modifyEffect`: concurrent suggestions serialize by construction, no mutable reference escapes, and the sole egress is the immutable positions map; the construction walk is the marked boundary seam.
- Growth: a new constraint kind, variable class, or strength tier is a C# solver change mirrored at the codec — the fold's vocabulary maps grow a row each, nothing else moves.

```typescript
import { Constraint, Expression, Operator, Solver, Strength, Variable } from "@lume/kiwi"
import { Fault, type Wire } from "@rasm/ts/core"
import { Effect, HashMap, Iterable, Schema, SynchronizedRef } from "effect"

const _relations = { le: Operator.Le, ge: Operator.Ge, eq: Operator.Eq } as const

const _strengths = {
  required: Strength.required,
  strong: Strength.strong,
  medium: Strength.medium,
  weak: Strength.weak,
} as const

class SolveFault extends Schema.TaggedError<SolveFault>()("SolveFault", {
  surface: Schema.String,
  rank: Schema.Int, // the constraint's position in the wire walk, `-1` past it — never a severity rank
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return "invalid"
  }
  override get message(): string {
    return `<solve:${this.surface}#${this.rank}> ${this.detail}`
  }
}

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

const _build = (program: Wire.LayoutProgram): Effect.Effect<_Draft, SolveFault> =>
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

const _solve = (program: Wire.LayoutProgram): Effect.Effect<Panel.Solved, SolveFault> =>
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
    readonly egress: typeof _egress
    readonly hook: typeof _panelHook
    readonly tone: typeof _tone
    readonly degrade: typeof _degrade
    readonly admit: typeof _admit
    readonly open: typeof _OPEN
    readonly emphasis: typeof _EMPHASIS
    readonly arms: typeof _arms
    readonly chrome: typeof _chrome
    readonly children: typeof _children
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
  egress: _egress,
  hook: _panelHook,
  tone: _tone,
  degrade: _degrade,
  admit: _admit,
  open: _OPEN,
  emphasis: _EMPHASIS,
  arms: _arms,
  chrome: _chrome,
  children: _children,
  route: _route,
  relations: _relations,
  strengths: _strengths,
  solve: _solve,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Panel }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
