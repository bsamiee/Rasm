# [UI_PANEL]

Panel materializes the AppUi shell's generated surface program: stable surface identity, one root control tree, and the exact layout-program closure that tree names. `Panel.fold` builds receipt-reconciled rows carrying binding slots beside the gate verdict, `Panel.chrome` projects decoded widgets, `Panel.route` exhaustively dispatches the viewer's own interaction vocabulary, `Panel.solve` preserves Cassowary insertion order and strengths, and `Panel.surface` admits and solves the generated root as one application input. Payloads remain verbatim carriage; missing wire cases fail at their row. Module: `ui/viewer/src/panel.ts`.

## [01]-[INDEX]

- [02]-[EVENT_FOLD]: keyed shell fold, gate slot, receipt-reconciled optimistic round trip, pivot-board boundary; `Panel`.
- [03]-[PHASE_RENDER]: lifecycle and degradation tone axes, affordance projection, disposition and freshness rows; `Panel`.
- [04]-[WIDGET_RENDER]: kind-exhaustive part-and-children fold, emphasis ladder, chrome projection; `Panel`.
- [05]-[CONTROL_SINKS]: locally-minted interaction union, exhaustive routing over one Effect rail, intent egress; `Panel`.
- [06]-[LAYOUT_SOLVE]: wire-order kiwi fold, edit-variable drag, program-order determinism law; `Panel`.
- [07]-[SURFACE_PROGRAM]: exact generated root admission, referenced-layout closure, solved application surface; `Panel`.

## [02]-[EVENT_FOLD]

[EVENT_FOLD]:
- Owner: `Panel.fold` — the keyed accumulator: the event feed carries one exact family selector beside foreign bytes, and each arm decodes its own generated message before folding into a `HashMap<key, Panel.Row>` — `BindingStatus` advances the lifecycle state beside its transport, direction, and last-good instant (clearing the optimistic slot on `faulted` alone), `CoercedValue` records the canonical magnitude the host landed beside both units, `WriteReceipt` lands the canonical value, its rendered pair, and the write's own four-arm disposition while clearing the optimistic slot, `CommandGate` seats the `available`/`level` verdict; the fold is total over the selector union and every arm ends at `_at`, the one slot-seat combinator that also carries the optimistic stamp.
- Packages: `@rasm/ts/core` (`Wire`, `Hlc`); `@rasm\/contracts/rasm/contracts/binding/v1/status_pb` (`BindingState`); `@rasm\/contracts/rasm/contracts/compute/v1/control_pb` (`DegradationLevel`); `effect` (`Array`, `Effect`, `HashMap`, `Match`, `Option`, `Schema`, `Stream`); `@effect-atom/atom-react` (the board atom rides `system/atom#STORE_ROOT`).
- Law: the row is the panel's whole truth — lifecycle, transport, direction, freshness, coercion, landed value, disposition, optimistic, gate; a panel reads one row through an `Atom.family` keyed by cell name and re-renders only on its own row's change.
- Law: every row slot is option-seated, so a gate-only row states that no binding has spoken rather than seeding a lifecycle token no producer emits.
- Law: the ARM owns the key — the board's key space is the shell's addressable cell, and each arm names which cell its event addresses: the livewire triple addresses its `binding` path, `CommandGate` its `key`, the `CommandRow` key the C# deck freezes. Both columns arrive on one control from the producer's own intent binding (`valueKey` beside `command`), so an affordance reads its value row and its gate row off the one board with no side map, no re-keying, and no second accumulator.
- Law: the gate is a row slot, never a second board — a control's whole display truth is one row, so `available` and `level` land beside the binding slots and the render projection is `[03]`'s alone; `level` is adopted through `CommandGate`'s own field, which derives from the ONE degradation vocabulary the `Availability` landing owns, so a producer level added at core breaks `[03]`'s degradation table at the declaration.
- Law: writes are optimistic against the feed — a panel edit writes the intent through the app-wired write port AND stamps the row's optimistic slot; the display shows `optimistic` over `landed` while present, the reconciling `WriteReceipt` clears it, and a `faulted` status clears it with the refusal surfaced through the `view/form` field-error seam. Round trips are receipt-driven, never awaited-then-assumed — the feed is the truth channel, the write port's acknowledgement only gates re-submission, and display state always derives from the fold.
- Law: this board is the wire-receipt optimistic plane — `system/atom`'s `Atom.optimistic` reconciles against an effect's own `Result` and never appears here; the two optimism laws share a name, never a mechanism, and the board rides the one store like any other atom.
- Law: stale optimism ages out — an optimistic slot older than the patience window (`_PATIENCE`, a `Duration` policy row) degrades to the in-flight affordance without reverting, keeping slow transports honest without fabricating failure.
- Law: unknown-value payloads stay opaque — `offered`/`landed` are `Schema.Unknown` on the wire by design; the panel renders them through one value-presenter row, never assuming shape.
- Law: bursts coalesce before the store — `Panel.drain` shapes the feed with `Stream.groupedWithin(events, 128, Duration.millis(16))`, folds each chunk through the SAME `_fold`, and lands one atom write per window inside `Atom.batch`, so a livewire storm costs one notification pass per frame; `Stream.throttle` composes on the same rail where a transport demands rate-shaping, and a per-event atom write is the named defect.
- Law: imperative drivers read atomically — a non-React consumer (the solve seam, a test harness) reads and advances the board through `registry.modify(atom, f)`, value and next state in one step, never a get-then-set pair.
- Boundary: the app composition identifies which declared feed delivered each byte document; `Panel.fold` owns exact message admission, while the write path belongs to the shell producer and this module emits intents only; the telemetry timeline a panel renders over its own event history is `view/chart#SERIES_SURFACE` material — rows here, series there.
- Boundary: a board of LINKED PIVOT panels is `view/chart#PIVOT_SURFACE`'s workspace grain composed whole, never a second roster folded here — the master panels contribute their selection-derived clauses through the transient overlay every detail panel reads, that whole arrangement persists as the one `Chart.Config` value, and a per-panel edit rides its `{panel}` patch. This board's key space is the shell's addressable CELL, and a perspective panel id is not one: seating pivot panels in it keys two vocabularies on one map and hands the overlay a second owner.

```typescript signature
import { type Clock, Wire } from "@rasm/ts/core"
import { BindingState } from "@rasm\/contracts/rasm/contracts/binding/v1/status_pb"
import { DegradationLevel } from "@rasm\/contracts/rasm/contracts/compute/v1/control_pb"
import { Array, Duration, Effect, HashMap, Match, Option, Schema, Stream } from "effect"
import type { Motion } from "../../src/system/act.ts"
import type { Theme } from "../../src/system/token.ts"

// The transport supplies the declared feed identity; this closed selector is routing evidence, not a second schema.
// `Panel.fold` decodes the selected family itself, so no decoded foreign value can bypass its one admission point.
type PanelEvent =
  | { readonly family: "BindingStatus"; readonly bytes: Uint8Array }
  | { readonly family: "CoercedValueWire"; readonly bytes: Uint8Array }
  | { readonly family: "WriteReceiptWire"; readonly bytes: Uint8Array }
  | { readonly family: "CommandGateWire"; readonly bytes: Uint8Array }

type PanelDecodedEvent =
  | { readonly family: "BindingStatus"; readonly value: Wire.BindingStatus }
  | { readonly family: "CoercedValueWire"; readonly value: Wire.CoercedValue }
  | { readonly family: "WriteReceiptWire"; readonly value: Wire.WriteReceipt }
  | { readonly family: "CommandGateWire"; readonly value: Wire.CommandGate }

// Protovalidate has already rejected zero and unknown enum values. These generated-value literals carry that proof
// into TypeScript's narrower table key types, which protobuf-es cannot derive from field options on its own.
const _State = Schema.Literal(
  BindingState.CONNECTING,
  BindingState.SUBSCRIBED,
  BindingState.POLLING,
  BindingState.STALE,
  BindingState.FAULTED,
)
const _Level = Schema.Literal(
  DegradationLevel.FULL,
  DegradationLevel.REDUCED_REMOTE,
  DegradationLevel.LOCAL_ONLY,
  DegradationLevel.READ_ONLY,
  DegradationLevel.SUSPENDED,
)

declare namespace Panel {
  type State = typeof _State.Type
  type Transport = Wire.BindingStatus["transport"]
  type Direction = Wire.BindingStatus["direction"]
  type Level = typeof _Level.Type
  type Row = {
    // Every slot is OPTION-seated because a row is reachable before any of its producers has spoken: a gate-only
    // row carries a command key and no livewire binding at all, so a seeded lifecycle token would assert a binding
    // state the host never published. The retired seed spelled exactly that, under a token no producer emits.
    readonly state: Option.Option<Panel.State>
    readonly transport: Option.Option<Panel.Transport>
    readonly direction: Option.Option<Panel.Direction>
    readonly lastGoodAt: Option.Option<NonNullable<Wire.BindingStatus["lastGoodAt"]>>
    // the coercion the host landed, on the producer's OWN columns — the canonical magnitude beside both units, so a
    // panel renders the value under the scheme the source published rather than under one it assumed
    readonly coercion: Option.Option<Omit<Wire.CoercedValue, "bindingId">>
    readonly landed: Option.Option<Wire.WriteReceipt["canonical"]>
    readonly rendered: Option.Option<NonNullable<Wire.WriteReceipt["rendered"]>>
    readonly renderedUnit: Option.Option<NonNullable<Wire.WriteReceipt["renderedUnit"]>>
    // the write's own four-arm verdict, kept WHOLE: a rejection, a rollback, and an indeterminate write are three
    // unlike repairs, and a boolean over them shows one badge for three states a user must act on differently
    readonly disposition: Option.Option<NonNullable<Wire.WriteReceipt["disposition"]>>
    readonly optimistic: Option.Option<{ readonly value: unknown; readonly since: Clock.Hlc }>
    readonly gate: Option.Option<{ readonly available: boolean; readonly level: Panel.Level }>
  }
  type Board = HashMap.HashMap<string, Row>
}

const _PATIENCE = Duration.seconds(4)

// ABSENCE is the true seed for a gate-only row: a command key carries no livewire binding, so every binding-sourced
// slot reads `none` until its producer speaks rather than reading a lifecycle token nothing sent.
const _EMPTY: Panel.Row = {
  state: Option.none(),
  transport: Option.none(),
  direction: Option.none(),
  lastGoodAt: Option.none(),
  coercion: Option.none(),
  landed: Option.none(),
  rendered: Option.none(),
  renderedUnit: Option.none(),
  disposition: Option.none(),
  optimistic: Option.none(),
  gate: Option.none(),
}

const _at = (board: Panel.Board, key: string, step: (row: Panel.Row) => Panel.Row): Panel.Board =>
  HashMap.modifyAt(board, key, (slot) => Option.some(step(Option.getOrElse(slot, () => _EMPTY))))

const _admitEvent = (event: PanelEvent) =>
  Match.value(event).pipe(
    Match.when({ family: "BindingStatus" }, ({ bytes }) =>
      Effect.map(Wire.decode("BindingStatus", bytes), (value) => ({ family: "BindingStatus", value }) as const)),
    Match.when({ family: "CoercedValueWire" }, ({ bytes }) =>
      Effect.map(Wire.decode("CoercedValueWire", bytes), (value) => ({ family: "CoercedValueWire", value }) as const)),
    Match.when({ family: "WriteReceiptWire" }, ({ bytes }) =>
      Effect.map(Wire.decode("WriteReceiptWire", bytes), (value) => ({ family: "WriteReceiptWire", value }) as const)),
    Match.when({ family: "CommandGateWire" }, ({ bytes }) =>
      Effect.map(Wire.decode("CommandGateWire", bytes), (value) => ({ family: "CommandGateWire", value }) as const)),
    Match.exhaustive,
  )

const _landEvent = (board: Panel.Board, event: PanelDecodedEvent): Panel.Board =>
  Match.value(event).pipe(
    Match.when({ family: "BindingStatus" }, ({ value: status }) => {
      const state = Schema.decodeSync(_State)(status.state)
      return _at(board, status.bindingId, (row) => ({
          ...row,
          state: Option.some(state),
          transport: Option.some(status.transport),
          direction: Option.some(status.direction),
          lastGoodAt: Option.fromNullable(status.lastGoodAt),
          // `faulted` alone clears the in-flight write: a STALE binding is live and still owes its echo, so dropping
          // the optimistic value there would erase a pending write the edge is about to acknowledge
          optimistic: state === BindingState.FAULTED ? Option.none() : row.optimistic,
        }))
    }),
    Match.when({ family: "CoercedValueWire" }, ({ value: { bindingId, canonical, canonicalUnit, sourceUnit, sourceAt } }) =>
      _at(board, bindingId, (row) => ({ ...row, coercion: Option.some({ canonical, canonicalUnit, sourceUnit, sourceAt }) }))),
    Match.when({ family: "WriteReceiptWire" }, ({ value: receipt }) =>
      _at(board, receipt.bindingId, (row) => ({
          ...row,
          landed: Option.some(receipt.canonical),
          rendered: Option.fromNullable(receipt.rendered),
          renderedUnit: Option.fromNullable(receipt.renderedUnit),
          disposition: Option.fromNullable(receipt.disposition),
          optimistic: Option.none(),
        })),
    Match.when({ family: "CommandGateWire" }, ({ value: gate }) => {
      const level = Schema.decodeSync(_Level)(gate.level)
      return _at(board, gate.key, (row) => ({ ...row, gate: Option.some({ available: gate.available, level }) }))
    }),
    Match.exhaustive,
  )

const _fold = (board: Panel.Board, event: PanelEvent) =>
  Effect.map(_admitEvent(event), (admitted) => _landEvent(board, admitted))

const _optimistic = (board: Panel.Board, binding: string, value: unknown, since: Clock.Hlc): Panel.Board =>
  _at(board, binding, (row) => ({ ...row, optimistic: Option.some({ value, since }) }))

const _drain = (
  events: Stream.Stream<PanelEvent>,
  commit: (fold: (board: Panel.Board) => Panel.Board) => void,
): Effect.Effect<void> =>
  Stream.runForEach(
    Stream.groupedWithin(events, 128, Duration.millis(16)),
    (window) => Effect.flatMap(
      Effect.forEach(window, _admitEvent, { concurrency: 1 }),
      (admitted) => Effect.sync(() => commit((board) => Array.reduce(admitted, board, _landEvent))),
    ),
  )
```

## [03]-[PHASE_RENDER]

[PHASE_RENDER]:
- Owner: `Panel.tone` and `Panel.degrade` — the two closed styling axes, with `Panel.admit` the affordance projection over the second: `_tone` keys the binding-lifecycle axis carrying tone and motion rows (`faulted` pulses a `Motion` row, `connecting` shows the in-flight affordance), `_degrade` keys the producer's degradation axis carrying the tone and whether the level is evidence at all; the lifecycle chip, the write disposition, the last-good instant (`Format.instant`, `system/intl`), and the gate badge are the display rows every binding panel composes.
- Law: each axis keys its own table — `satisfies Record<Panel.State, ...>` and `satisfies Record<Panel.Level, ...>`, so a wire vocabulary change on either axis breaks its own row at compile time and a state or level conditional in a panel body marks the table unused. Axes never merge: state is the binding's lifecycle, level is the host's degradation, and one joint table multiplies them into a cross product no producer emits.
- Law: `Panel.admit` is the one affordance read and it is TOTAL — `disabled` is the gate's `available` inverted, tone and badge visibility are its `level` row, and an EMPTY gate slot projects `_CLOSED`: inert, and silent about a level the producer never stated. Absence seeds CLOSED because the producer's own `CanExecute` stream seeds `false` before its first emission; seeding open renders every affordance live in the window before its first verdict lands, and the press then meets a deck that refuses.
- Law: CLOSED answers a MISSING verdict, never a missing command — a widget whose binding names no command key awaits nothing, so it projects `_OPEN` and the gate join runs on the command-bearing widgets alone; folding the two absences together renders every label, every readout, and every uncommanded field permanently inert.
- Law: the disposition renders per ARM and never as a boolean — an acknowledgement shows its echo class, a rollback shows the prior value it restored, and only a rejection or an indeterminate write renders on the danger tone and feeds the round-trip revert, because an indeterminate write leaves the external value unknown to this process and a rollback does not.
- Growth: a producer degradation level is one `_degrade` row and a binding lifecycle state one `_tone` row — the core vocabulary lands first, and each table fails at its declaration until its row exists.
- Boundary: chip/badge primitives are `system/primitive` recipes; plural and status text is `Message`'s (`system/intl`), so the badge's reason resolves from the level key through a catalog row and no level text is authored here.

```typescript signature
// the producer's own five lifecycle rows, in its rank order: two in-flight, two live, one broken
const _tone = {
  [BindingState.CONNECTING]: { tone: "accent", motion: Option.none<Motion.Hold>() },
  [BindingState.SUBSCRIBED]: { tone: "success", motion: Option.none<Motion.Hold>() },
  [BindingState.POLLING]: { tone: "success", motion: Option.none<Motion.Hold>() },
  [BindingState.STALE]: { tone: "caution", motion: Option.none<Motion.Hold>() },
  [BindingState.FAULTED]: { tone: "danger", motion: Option.some<Motion.Hold>("pulse") },
} as const satisfies Record<Panel.State, { readonly tone: Theme.Tone; readonly motion: Option.Option<Motion.Hold> }>

// producer's rows hold its own rank order; `full` is the undegraded floor and the one level carrying no badge
const _degrade = {
  [DegradationLevel.FULL]: { tone: "neutral", evident: false },
  [DegradationLevel.REDUCED_REMOTE]: { tone: "accent", evident: true },
  [DegradationLevel.LOCAL_ONLY]: { tone: "accent", evident: true },
  [DegradationLevel.READ_ONLY]: { tone: "caution", evident: true },
  [DegradationLevel.SUSPENDED]: { tone: "danger", evident: true },
} as const satisfies Record<Panel.Level, { readonly tone: Theme.Tone; readonly evident: boolean }>

declare namespace Panel {
  type Affordance = { readonly tone: Theme.Tone; readonly evident: boolean; readonly disabled: boolean }
}

const _CLOSED: Panel.Affordance = { ..._degrade[DegradationLevel.FULL], disabled: true }

// uncommanded widgets take this floor: no verdict is owed, so none is awaited
const _OPEN: Panel.Affordance = { ..._degrade[DegradationLevel.FULL], disabled: false }

const _admit = (row: Panel.Row): Panel.Affordance =>
  Option.match(row.gate, {
    onNone: () => _CLOSED,
    onSome: (gate) => ({ ..._degrade[gate.level], disabled: !gate.available }),
  })
```

## [04]-[WIDGET_RENDER]

[WIDGET_RENDER]:
- Owner: `Panel.arms` exhausts the generated `ControlIntentWire.arm` oneof and returns the part key beside its optional constraint-program reference; `Panel.children` reads the one core walk projection, and `Panel.chrome` joins the view row with the generated emphasis value, icon slot, motion hold, and board affordance.
- Packages: `@rasm/ts/core` (`Wire.ControlIntent`); `@rasm\/contracts/rasm/contracts/ui/v1/controls_pb` (generated enum values); `effect` (`HashMap`, `Match`, `Option`); `../../src/system/token.ts` (`Theme.Tone`); `../../src/system/act.ts` (`Motion.Hold`).
- Law: the core walk row is the ONE child projection over the generated key space; this view fold owns only the part and constraint reference the walk cannot decide. Kinds whose part narrows on their own posture, form, or temporal column read that column off their own arm, because the narrowing belongs with the value carrying it.
- Law: the same exhaustive arm row projects a container's generated `constraintProgram`; surface admission reads that projection, so no second Panel/Dock roster can drift from render or recursion.
- Law: `Match.discriminatorsExhaustive("case")` dispatches the generated oneof with no fallback, so every new producer arm breaks this declaration before a nesting arm can masquerade as a leaf.
- Law: generated numeric enum values drive every emphasis, posture, temporal, and progress decision directly; an unspecified or foreign numeric value yields absence at the projection boundary instead of entering a string-keyed twin table.
- Law: this module holds NO styling literal — tone keys the token roster, fill keys a recipe variant `system/primitive` owns, motion keys a `Motion.Hold` row, and the class string is composed where the view row calls `Primitive.styled`; a hex value, a utility string, or a pixel here forks the authority `system/token` closes.
- Law: an icon is a SLOT, never a resolved image — the asset key, placement, and size cross verbatim and the app-wired asset source answers the glyph, so a decoded intent never carries pixels and both heads mount the same slot from the same three columns.
- Law: motion ranks refusal over work — a faulted binding pulses through the lifecycle row even while its control is pending, because the repair the user owes outranks the work the host is doing; below it a pending icon spins and an indeterminate progress reads its form's own hold, and a determinate fraction holds nothing at all.
- Law: the gate join is the binding's two keys — `binding.command` addresses the verdict row and `binding.valueKey` the livewire row, both on the ONE board `[02]` folds, so a widget's whole display truth is two reads with no side map; a command-free widget projects `_OPEN` per `[03]`'s law and never awaits a verdict nobody will send.
- Law: payloads stay verbatim — a bound, a fraction, a visible limit, and a window spec land as the producer wrote them; a clamp, a remap, or a head-local default for an omitted column forks the emitting peer's semantics, which is exactly what the every-field-has-a-wire-representation law at the producer boundary exists to make impossible.
- Boundary: mounting a part is the view layer's — this module is pure projection and imports no component, so the part key is the seam a view row resolves against its own registry; label, header, and hint TEXT resolves through `system/intl` from the key columns, and the constraint program named by a container arm is `[06]`'s to solve.
- Growth: a producer oneof arm extends the exhaustive record, while a generated enum member extends its numeric match at the decision that consumes it.

```typescript signature
import type { Wire } from "@rasm/ts/core"
import * as controls from "@rasm\/contracts/rasm/contracts/ui/v1/controls_pb"
import { Match, Option } from "effect"

declare namespace Panel {
  type Widget = Wire.ControlIntent["arm"]["case"]
  type Arm<K extends Panel.Widget> = Extract<Wire.ControlIntent["arm"], { readonly case: K }>["value"]
  type Binding = Wire.ControlIntent["binding"]
  type Fill = "solid" | "soft" | "ghost" | "inverted" | "link"
}

const _emphasis = (value: Panel.Binding["emphasis"]): Option.Option<{ readonly tone: Theme.Tone; readonly fill: Panel.Fill }> =>
  Match.value(value).pipe(
    Match.when(controls.ControlEmphasis.QUIET, () => ({ tone: "neutral", fill: "ghost" } as const)),
    Match.when(controls.ControlEmphasis.SECONDARY, () => ({ tone: "neutral", fill: "soft" } as const)),
    Match.when(controls.ControlEmphasis.PRIMARY, () => ({ tone: "accent", fill: "solid" } as const)),
    Match.when(controls.ControlEmphasis.DANGER, () => ({ tone: "danger", fill: "solid" } as const)),
    Match.when(controls.ControlEmphasis.INVERTED, () => ({ tone: "neutral", fill: "inverted" } as const)),
    Match.when(controls.ControlEmphasis.LINK, () => ({ tone: "accent", fill: "link" } as const)),
    Match.option,
  )

const _color = (value: controls.ColorPosture) => Match.value(value).pipe(
  Match.when(controls.ColorPosture.INLINE, () => "ColorArea" as const),
  Match.when(controls.ColorPosture.FLYOUT, () => "ColorPicker" as const),
  Match.option,
)
const _select = (value: controls.SelectPosture) => Match.value(value).pipe(
  Match.when(controls.SelectPosture.CLOSED, () => "Select" as const),
  Match.when(controls.SelectPosture.EDITABLE, () => "ComboBox" as const),
  Match.option,
)
const _multi = (value: controls.MultiPosture) => Match.value(value).pipe(
  Match.when(controls.MultiPosture.BOUND, () => "MultiSelect" as const),
  Match.when(controls.MultiPosture.FREE, () => "TagField" as const),
  Match.option,
)
const _segment = (value: controls.SegmentPosture) => Match.value(value).pipe(
  Match.when(controls.SegmentPosture.SELECT, () => "ToggleButtonGroup" as const),
  Match.when(controls.SegmentPosture.COMMAND, () => "Toolbar" as const),
  Match.option,
)
const _chip = (value: controls.ChipPosture) => Match.value(value).pipe(
  Match.when(controls.ChipPosture.STATIC, () => "Tag" as const),
  Match.when(controls.ChipPosture.TOGGLE, () => "ToggleButton" as const),
  Match.when(controls.ChipPosture.REMOVABLE, () => "Tag" as const),
  Match.option,
)
const _temporal = (value: controls.TemporalKind) => Match.value(value).pipe(
  Match.when(controls.TemporalKind.DATE, () => "DatePicker" as const),
  Match.when(controls.TemporalKind.TIME, () => "TimeField" as const),
  Match.when(controls.TemporalKind.DATETIME, () => "DatePicker" as const),
  Match.when(controls.TemporalKind.RANGE, () => "DateRangePicker" as const),
  Match.option,
)
const _progress = (value: controls.ProgressForm) => Match.value(value).pipe(
  Match.when(controls.ProgressForm.BAR, () => ({ part: "ProgressBar", hold: "pulse" } as const)),
  Match.when(controls.ProgressForm.RING, () => ({ part: "Meter", hold: "spin" } as const)),
  Match.when(controls.ProgressForm.SKELETON, () => ({ part: "Skeleton", hold: "pulse" } as const)),
  Match.option,
)

const _node = <P extends string>(
  part: P,
  constraintProgram: Option.Option<string> = Option.none(),
) => ({ part, constraintProgram })

const _arms = (intent: Wire.ControlIntent) => Match.value(intent.arm).pipe(
  Match.discriminatorsExhaustive("case")({
    button: () => Option.some(_node("Button")),
    label: () => Option.some(_node("Text")),
    textInput: ({ value }) => Option.some(_node(value.multiline ? "TextArea" : "TextField")),
    numberInput: () => Option.some(_node("NumberField")),
    dateInput: ({ value }) => Option.map(_temporal(value.kind), _node),
    pathInput: () => Option.some(_node("FileTrigger")),
    colorInput: ({ value }) => Option.map(_color(value.posture), _node),
    select: ({ value }) => Option.map(_select(value.posture), _node),
    multiSelect: ({ value }) => Option.map(_multi(value.posture), _node),
    slider: () => Option.some(_node("Slider")),
    range: () => Option.some(_node("Slider")),
    toggle: () => Option.some(_node("Switch")),
    radio: () => Option.some(_node("RadioGroup")),
    segmented: ({ value }) => Option.map(_segment(value.posture), _node),
    chip: ({ value }) => Option.map(_chip(value.posture), _node),
    progress: ({ value }) => Option.map(_progress(value.form), ({ part }) => _node(part)),
    avatar: () => Option.some(_node("AvatarGroup")),
    breadcrumb: () => Option.some(_node("Breadcrumbs")),
    tooltip: () => Option.some(_node("Tooltip")),
    banner: () => Option.some(_node("Banner")),
    emptyState: () => Option.some(_node("EmptyState")),
    grid: () => Option.some(_node("Table")),
    tree: () => Option.some(_node("Tree")),
    overview: () => Option.some(_node("Overview")),
    menu: () => Option.some(_node("Menu")),
    toolbar: () => Option.some(_node("Toolbar")),
    tab: () => Option.some(_node("Tabs")),
    accordion: () => Option.some(_node("DisclosureGroup")),
    panel: ({ value }) => Option.some(_node("Group", Option.some(value.constraintProgram))),
    dock: ({ value }) => Option.some(_node("Group", Option.some(value.constraintProgram))),
    splitter: () => Option.some(_node("SplitPane")),
  }),
)

declare namespace Panel {
  type Part = ReturnType<typeof _arms> extends Option.Option<infer N extends { readonly part: string }> ? N["part"] : never
  type Chrome = {
    readonly part: Panel.Part
    readonly tone: Theme.Tone
    readonly fill: Panel.Fill
    readonly icon: Panel.Binding["icon"]
    readonly hold: Option.Option<Motion.Hold>
    readonly affordance: Panel.Affordance
    readonly value: Option.Option<Panel.Row>
  }
}

const _slot = (board: Panel.Board, key: Option.Option<string>): Option.Option<Panel.Row> =>
  Option.flatMap(key, (named) => HashMap.get(board, named))

const _pending = (intent: Wire.ControlIntent): Option.Option<Motion.Hold> =>
  intent.arm.case === "progress" && intent.arm.value.fraction === undefined
    ? Option.map(_progress(intent.arm.value.form), ({ hold }) => hold)
    : Option.as(Option.flatMap(Option.fromNullable(intent.binding.icon), (icon) => Option.fromNullable(icon.pending)), "spin" as const)

const _chrome = (board: Panel.Board, intent: Wire.ControlIntent): Option.Option<Panel.Chrome> => {
  const value = _slot(board, Option.fromNullable(intent.binding.valueKey))
  return Option.map(
    Option.all({ node: _arms(intent), emphasis: _emphasis(intent.binding.emphasis) }),
    ({ node, emphasis }) => ({
      part: node.part,
      tone: emphasis.tone,
      fill: emphasis.fill,
      icon: intent.binding.icon,
      hold: Option.orElse(Option.flatMap(value, (row) => Option.flatMap(row.state, (state) => _tone[state].motion)), () => _pending(intent)),
      affordance: Option.match(_slot(board, Option.fromNullable(intent.binding.command)), { onNone: () => _OPEN, onSome: _admit }),
      value,
    }),
  )
}

const _children = (intent: Wire.ControlIntent): ReadonlyArray<Wire.ControlIntent> =>
  Wire.Walk.children("ControlIntentWire", intent)
```

## [05]-[CONTROL_SINKS]

[CONTROL_SINKS]:
- Owner: `Panel.route` — the derived dispatch over `Panel.Interaction`, this viewer's OWN interaction vocabulary: `Panel.Sinks` is one mapped handler record whose key space IS the union's discriminant space, so a new case breaks the record loudly at compile time, and `Panel.route(sinks)` closes it as the reusable terminal — one exhaustive match is the only place interaction cases meet handlers.
- Packages: `effect` (`Match`); `../../src/system/hook.ts` (`Hook`).
- Law: this union is LOCALLY minted and wears no wire family name — no C# page produces an orbit, a section, or a measure, so the vocabulary homes at the surface that mints it and the `ControlIntentWire` name stays the producer's widget family; a consumer vocabulary wearing a producer's family name is the fabrication `typescript:core` `RULINGS.md` `[02]-[SHAPE]` forecloses.
- Law: one discriminant, spelled once — the union discriminates on `kind` exactly as the decoded widget family does, so this page carries one dispatch spelling and an egress record needs no tag-to-kind mapping on the way to the gateway.
- Law: each case has exactly one owning plane — `orbit`/`pan` hand their delta to the SAME recognizer-to-intent seam `system/act#CONTINUOUS_OWNER` opens, a `Gesture.Reading` the owning plane's `emit` folds over live camera state into `viewer/geo#CAMERA`'s `Camera.gestured`, because a control-driven orbit and a dragged one are the same axis arriving by a different route; `select` mints `Selection.Op` (`additive` selects `Add` versus `Replace` — modality in the op value, `mark`'s law); `focus` mints a fit intent over the target's bounds; `section` and `measure` land on `scene#DRAW_COLLAPSE`'s tool rows — `Glb.section(tree, plane)` and `Glb.measure(tree, from, to)` over the accelerated BVH surface, the sink adapting its `origin`/`normal` payload through `Plane.setFromNormalAndCoplanarPoint` at the shell edge — one sink, one plane, never a case handled twice.
- Law: a control NEVER re-mints a camera intent — the camera shape, its extra axes, and its intent family are the projection plane's, and that plane already folds a reading to the destination it is at, so a second mint here forks the fold and eases an intent the plane deliberately jumps; the recogniser floor and this surface are two producers of ONE reading, which is exactly what makes a dragged orbit and a pressed one indistinguishable in the replay journal.
- Law: sinks are app-composed and EFFECT-shaped — the shell binds each sink to the owning plane's atom write at composition, every handler answers an `Effect` on the same rail `Panel.egress` publishes to, and this module never imports the planes because the record IS the seam. `void` handlers force the shell to run an effect at the call site, which is the fire-and-forget seam a replayable routing law cannot have; a direct plane import couples the panel to every surface it drives.
- Law: payloads are carriage — `yaw`/`pitch`, `dx`/`dy`, `targets`/`additive`, section `origin`/`normal`, measure `from`/`to`, focus `target` land verbatim on the sink; an out-of-range value is upstream evidence. Routing is replayable — every interaction lands as an op/intent value on a plane's fold, composing with `History` undo and the probe plane exactly like a locally-recognized gesture.
- Law: interactions emit values, never calls — an affordance mints an egress record (the kind with its payload) written to the app-wired command gateway; the gateway owns encode and transport, and this module never encodes, never names a transport. Affordance state rides atoms — active tool, additive modifier, in-flight measure endpoint live in `Atom.family` rows keyed by control id, RAC components running controlled. Availability gates RENDER, never dispatch — a widget's `isDisabled` prop and its degradation badge are `Panel.chrome`'s affordance over that widget's own gate row, so an unavailable command renders inert with its level as tooltip evidence rather than failing on press, while `_route` stays ungated because an interaction carries no command key for a verdict to match on.
- Law: egress records publish once on the `rasm.ui.panel.egress` hook point (`system/hook`, observe modality) as they reach the command gateway — this page contributes the point and `Panel.egress(registry, gateway)` composes publish-before-send on one Effect rail, so telemetry taps, probe boards, and replay journals never wrap the gateway.
- Growth: a new interaction is one union case and one handler row; zero dispatch edits.

```typescript signature
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
  // one rail: each handler answers an Effect, so routing and egress compose with no boundary adapter between them
  type Sinks<E = never, R = never> = { readonly [K in Panel.Gesture]: (interaction: Panel.Reach<K>) => Effect.Effect<void, E, R> }
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

const _route = <E, R>(sinks: Panel.Sinks<E, R>): ((interaction: Panel.Interaction) => Effect.Effect<void, E, R>) =>
  Match.type<Panel.Interaction>().pipe(Match.discriminatorsExhaustive("kind")(sinks))
```

## [06]-[LAYOUT_SOLVE]

[LAYOUT_SOLVE]:
- Owner: `Panel.solve` consumes the decoded generated `LayoutProgram` whole: `introduction` mints the variable ledger in producer order, nested `left` and `right` expressions compile each constraint, explicit edits retain their generated strengths, authored suggestions register missing medium edit variables, and measurement suggestions apply after them.
- Law: `LayoutVarWire` identity remains its generated `owner` and `edge` pair through introduction, expressions, edits, and values; only the Kiwi boundary derives its private `owner.edge` handle name, so no flattened wire identity or parallel alias type survives.
- Law: generated `LayoutRelation` and `LayoutStrength` numeric constants map directly onto Kiwi values at construction; no string vocabulary or hand-authored enum twin stands beside either generated owner.
- Law: constraints, explicit edits, authored suggestions, and applicable measurements retain their separate producer sequences, and `updateVariables` runs after the received sequence has driven the tableau.
- Law: drag remains a generated `LayoutValue` suggestion against the retained edit-variable set, so a live update carries the same structured variable identity as the program and adds no constraint.
- Law: constraint order, variable introduction order, strengths, authored suggestion order, and measurement order fix the parity tableau; positional drift reports through `probe` and never triggers a tolerance solve.
- Law: positions flow to render as one atom write per settle — the returned owner/edge map replaces the positions atom (`Atom.batch` coalesces multi-panel updates), and panel components read their own cell through a selector so a 60fps drag never re-renders the board.
- Law: the live solver is a RESOURCE, not a kernel — kiwi's incremental `suggestValue` requires the solver and its variable ledger to persist for the `Solved` lifetime, so the draft lives inside one `SynchronizedRef` and every `suggest` routes through `SynchronizedRef.modifyEffect`: concurrent suggestions serialize by construction, no mutable reference escapes, and the sole egress is the immutable positions map; the construction walk is the marked boundary seam.
- Growth: a generated relation or strength member extends its numeric match, while a new program field breaks this direct consumer until its semantics land.

```typescript signature
import { Expression, Operator, Solver, Strength, Variable } from "@lume/kiwi"
import { Fault, Wire } from "@rasm/ts/core"
import * as layout from "@rasm\/contracts/rasm/contracts/ui/v1/layout_pb"
import { Effect, HashMap, Iterable, Option, Schema, SynchronizedRef } from "effect"

const _relation = (value: layout.LayoutRelation): Operator => {
  switch (value) {
    case layout.LayoutRelation.EQ: return Operator.Eq
    case layout.LayoutRelation.LE: return Operator.Le
    case layout.LayoutRelation.GE: return Operator.Ge
    default: throw new Error(`layout-relation:${value}`)
  }
}

const _strength = (value: layout.LayoutStrength): number => {
  switch (value) {
    case layout.LayoutStrength.REQUIRED: return Strength.required
    case layout.LayoutStrength.STRONG: return Strength.strong
    case layout.LayoutStrength.MEDIUM: return Strength.medium
    case layout.LayoutStrength.WEAK: return Strength.weak
    default: throw new Error(`layout-strength:${value}`)
  }
}

// Three legs partition the solve and each reason renders its OWN subject, because the three refusals answer three
// different repairs: a numbered constraint the tableau rejected names its wire position, a registration or solve
// past that walk names no position at all, and a live suggestion names the edit variable it was aimed at. One row
// carrying `rank: -1` spelled the second and third as the first, so the sentinel WAS the reason discriminant —
// which is the state a closed reason vocabulary exists to make unrepresentable.
const _family = Fault.Class.family(["constraint", "program", "suggest"] as const, {
  constraint: Fault.Class.row({
    class: "invalid",
    leg: "walk",
    detail: Schema.Struct({
      surface: Schema.String,
      rank: Schema.Int.pipe(Schema.nonNegative()), // the constraint's position in the wire walk, never a severity rank
      cause: Schema.String,
    }),
    render: ({ cause, rank, surface }) => `${surface} constraint #${rank} refused: ${cause}`,
  }),
  program: Fault.Class.row({
    class: "invalid",
    leg: "register",
    detail: Schema.Struct({ surface: Schema.String, cause: Schema.String }),
    render: ({ cause, surface }) => `${surface} edit registration refused: ${cause}`,
  }),
  suggest: Fault.Class.row({
    class: "invalid",
    leg: "edit",
    detail: Schema.Struct({ surface: Schema.String, edit: Schema.String, cause: Schema.String }),
    render: ({ cause, edit, surface }) => `${surface} suggestion against ${edit} refused: ${cause}`,
  }),
})

declare namespace SolveFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class SolveFault extends Schema.TaggedError<SolveFault>()("SolveFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

declare namespace Panel {
  type Positions = HashMap.HashMap<string, HashMap.HashMap<string, number>>
  type Solved = {
    readonly positions: Panel.Positions
    readonly suggest: (suggestion: Wire.LayoutProgram["suggestions"][number]) => Effect.Effect<Panel.Positions, SolveFault>
  }
}

type _Variable = Wire.LayoutProgram["introduction"][number]
type _Draft = { readonly solver: Solver; readonly cells: ReadonlyMap<string, ReadonlyMap<string, Variable>> }

const _read = (draft: _Draft): Panel.Positions =>
  HashMap.fromIterable(Iterable.map(draft.cells, ([owner, edges]) => [
    owner,
    HashMap.fromIterable(Iterable.map(edges, ([edge, cell]) => [edge, cell.value()] as const)),
  ] as const))

const _label = (variable: _Variable): string =>
  `${variable.owner.length}:${variable.owner}${variable.edge.length}:${variable.edge}`

const _variable = (cells: _Draft["cells"], variable: _Variable): Variable | undefined =>
  cells.get(variable.owner)?.get(variable.edge)

const _build = (program: Wire.LayoutProgram): Effect.Effect<_Draft, SolveFault> =>
  Effect.suspend(() => {
    const cursor = { at: Option.none<number>() }
    return Effect.try({
      try: () => {
        const solver = new Solver()
        const cells = new Map<string, Map<string, Variable>>()
        program.introduction.forEach((variable) => {
          const edges = cells.get(variable.owner) ?? new Map<string, Variable>()
          edges.set(variable.edge, new Variable(_label(variable)))
          cells.set(variable.owner, edges)
        })
        const named = (variable: _Variable): Variable => {
          const held = _variable(cells, variable)
          if (held === undefined) throw new Error(`unintroduced:${_label(variable)}`)
          return held
        }
        const expression = (row: Wire.LayoutProgram["constraints"][number]["left"]): Expression =>
          new Expression(...row.terms.map((term): [number, Variable] => [term.coefficient, named(term.variable)]), row.constant)
        program.constraints.forEach((row, at) => {
          cursor.at = Option.some(at)
          solver.createConstraint(expression(row.left), _relation(row.relation), expression(row.right), _strength(row.strength))
        })
        cursor.at = Option.none()
        program.edits.forEach((edit) => solver.addEditVariable(named(edit.variable), _strength(edit.strength)))
        program.suggestions.forEach((suggestion) => {
          const variable = named(suggestion.variable)
          if (!solver.hasEditVariable(variable)) solver.addEditVariable(variable, Strength.medium)
          solver.suggestValue(variable, suggestion.value)
        })
        program.measurements.forEach((measurement) => {
          const variable = named(measurement.variable)
          solver.suggestValue(variable, measurement.value)
        })
        solver.updateVariables()
        return { solver, cells }
      },
      catch: (defect) =>
        new SolveFault({
          case: Option.match(cursor.at, {
            onNone: () => ({ reason: "program", surface: program.surface, cause: String(defect) }) as const,
            onSome: (rank) => ({ reason: "constraint", surface: program.surface, rank, cause: String(defect) }) as const,
          }),
        }),
    })
  })

const _solve = (program: Wire.LayoutProgram): Effect.Effect<Panel.Solved, SolveFault> =>
  Effect.gen(function* () {
    const draft = yield* _build(program)
    const held = yield* SynchronizedRef.make(draft)
    return {
      positions: _read(draft),
      suggest: (suggestion) =>
        SynchronizedRef.modifyEffect(held, (live) =>
          Effect.try({
            try: () => {
              const edit = _label(suggestion.variable)
              const cell = _variable(live.cells, suggestion.variable)
              if (cell === undefined) {
                throw new Error(edit)
              }
              live.solver.suggestValue(cell, suggestion.value)
              live.solver.updateVariables()
              return [_read(live), live] as const
            },
            catch: (defect) => new SolveFault({
              case: { reason: "suggest", surface: program.surface, edit: _label(suggestion.variable), cause: String(defect) },
            }),
          })),
    }
  })

```

## [07]-[SURFACE_PROGRAM]

[SURFACE_PROGRAM]:
- Owner: `Panel.surface` admits one generated `AppUiSurfaceProgram` byte document, traverses its generated root through the core walk rail, proves unique control identity and the exact set of Panel/Dock `constraintProgram` references equals the generated `layouts[].surface` set, and solves that closed layout family before returning the application projection.
- Packages: `@rasm/ts/core` (`Wire.AppUiSurface`, generated ProtoJSON decoder and bounded `Wire.Walk`); `effect` (`Array`, `Effect`, `HashMap`, `HashSet`, `Option`, `Schema`).
- Law: `AppUiSurfaceProgram` is the only independently seated UI application input; `ControlIntentWire` and `LayoutProgram` are its generated support closure, not alternate decoder roots.
- Law: surface identity, root intent, and layouts enter together through `Wire.decode("AppUiSurfaceProgram", bytes)`; no leaf path, wrapper interface, or hand JSON schema can construct a competing application input.
- Law: the reference census reads `constraintProgram` from `[04]`'s exhaustive generated arm row and recursion from `Wire.Walk`, so no second nesting vocabulary or unbounded traversal stands beside either owner.
- Law: control keys are unique across the whole root tree before value binding, automation, or solved positions read one; layout equality is bidirectional — a referenced program absent from `layouts` and an unreferenced provided program both refuse; generated validation has already proved layout-surface and structured-variable uniqueness.
- Law: the returned value is an interior application projection, not a wire twin: stable identity and the root remain generated values while received layout programs become live solver resources keyed by their generated surface identity.
- Growth: a new control arm breaks `[04]`'s exhaustive projection; a new generated root field breaks this direct projection; a new constraint reference cannot enter without joining the one arm row.

```typescript signature
import { Fault, type Wire } from "@rasm/ts/core"
import { Array, Effect, HashMap, HashSet, Option, Schema } from "effect"

const _surfaceFamily = Fault.Class.family(["projection", "identity", "closure"] as const, {
  projection: Fault.Class.row({
    class: "invalid",
    leg: "admit",
    detail: Schema.Struct({ key: Schema.String }),
    render: ({ key }) => `control ${key} has no admitted generated arm projection`,
  }),
  identity: Fault.Class.row({
    class: "invalid",
    leg: "admit",
    detail: Schema.Struct({ keys: Schema.Array(Schema.String) }),
    render: ({ keys }) => `control keys are not unique: [${keys.join(",")}]`,
  }),
  closure: Fault.Class.row({
    class: "invalid",
    leg: "admit",
    detail: Schema.Struct({ referenced: Schema.Array(Schema.String), provided: Schema.Array(Schema.String) }),
    render: ({ referenced, provided }) =>
      `layout closure differs: referenced=[${referenced.join(",")}] provided=[${provided.join(",")}]`,
  }),
})

class SurfaceFault extends Schema.TaggedError<SurfaceFault>()("SurfaceFault", {
  case: _surfaceFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _surfaceFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _surfaceFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _surfaceFamily.render(this.case)
  }
}

declare namespace Panel {
  type Surface = {
    readonly workspace: Wire.AppUiSurface["workspace"]
    readonly route: Wire.AppUiSurface["route"]
    readonly instance: Wire.AppUiSurface["instance"]
    readonly root: Wire.ControlIntent
    readonly layouts: HashMap.HashMap<string, Panel.Solved>
  }
}

const _surface = (bytes: Uint8Array) => Effect.flatMap(Wire.decode("AppUiSurfaceProgram", bytes), (program) => Effect.gen(function* () {
  const controls = yield* Wire.Walk.nodes("ControlIntentWire", program.root, Wire.Walk.floor)
  const identities = Array.map(controls, (control) => control.key)
  if (HashSet.size(HashSet.fromIterable(identities)) !== identities.length) {
    return yield* new SurfaceFault({ case: { reason: "identity", keys: identities } })
  }
  const projections = yield* Effect.forEach(controls, (control) =>
    Option.match(_arms(control), {
      onNone: () => Effect.fail(new SurfaceFault({ case: { reason: "projection", key: control.key } })),
      onSome: Effect.succeed,
    }), { concurrency: 1 })
  const referenced = HashSet.fromIterable(Array.filterMap(projections, (row) => row.constraintProgram))
  const provided = HashSet.fromIterable(Array.map(program.layouts, (layout) => layout.surface))
  if (HashSet.size(referenced) !== HashSet.size(provided) || !HashSet.isSubset(referenced, provided)) {
    return yield* new SurfaceFault({
      case: {
        reason: "closure",
        referenced: Array.fromIterable(referenced),
        provided: Array.fromIterable(provided),
      },
    })
  }
  const layouts = yield* Effect.forEach(program.layouts, (layout) =>
    Effect.map(_solve(layout), (solved) => [layout.surface, solved] as const), { concurrency: 1 })
  return {
    workspace: program.workspace,
    route: program.route,
    instance: program.instance,
    root: program.root,
    layouts: HashMap.fromIterable(layouts),
  } satisfies Panel.Surface
}))

declare namespace Panel {
  type Shape = {
    readonly SolveFault: typeof SolveFault
    readonly SurfaceFault: typeof SurfaceFault
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
    readonly emphasis: typeof _emphasis
    readonly arms: typeof _arms
    readonly chrome: typeof _chrome
    readonly children: typeof _children
    readonly route: typeof _route
    readonly solve: typeof _solve
    readonly surface: typeof _surface
  }
}

const Panel: Panel.Shape = {
  SolveFault,
  SurfaceFault,
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
  emphasis: _emphasis,
  arms: _arms,
  chrome: _chrome,
  children: _children,
  route: _route,
  solve: _solve,
  surface: _surface,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Panel }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
