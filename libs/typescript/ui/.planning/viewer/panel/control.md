# [UI_CONTROL]

`viewer/panel/control.ts` materializes the control plane (AU:82): the `ControlIntent` union arrives decoded through `wire/vocab` — one kind-discriminated vocabulary whose `_tag` was attached at the wire seam — and this module is its one exhaustive materializer: every intent case renders as exactly one RAC primitive row (button, switch, slider, select) styled through `Primitive.styled` recipes, controlled values bind to the panel's atoms, and interactions emit intent values back through the app-wired command gateway. A new control kind is one wire union member plus one dispatch arm here — the compile error at the missing arm IS the growth mechanism, and a control materialized anywhere else re-implements the vocabulary.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              |
| :-----: | :---------------- | :------------------------------------------------------------------- |
|   [1]   | `RENDER_DISPATCH` | the exhaustive intent-case → primitive-row materializer               |
|   [2]   | `VALUE_BINDING`   | controlled-value law — intent state in atoms, RAC running controlled  |
|   [3]   | `INTENT_EGRESS`   | interaction → intent emission through the gateway boundary            |

## [2]-[RENDER_DISPATCH]

- Owner: `ControlPanel.render` — one total dispatch: `Match.tagsExhaustive` over the decoded `ControlIntent` union maps each case to its materializer row — a command case to a pressable row, a toggle case to a switch row, a range case to a slider row, a choice case to a select row — each row a contract (`ControlPanel.Row`) carrying the RAC component choice, the recipe variants, and the label resolution (`intl/message` key from the intent's own label field); the dispatch is the ONLY place intent cases meet components, so coverage is compiler-checked and a new case breaks here loudly.
- Packages: `@rasm/ts/wire/vocab` (`ControlIntent` — the closed union), `effect` (`Match`), `view/primitive` (`Primitive.styled` recipes), `react-aria-components` (the field/toggle/slider/select families as settled spine law).
- Law: the materializer is presentation-total — every case renders; an unknown-kind arm cannot exist because the union is closed at the wire, and `Match.tagsExhaustive` proves it.
- Law: rows are contracts, not JSX in this module — each arm yields a `Row` value (`component` discriminant + props projection) the panel shell renders; the shell owns markup, this module owns the correspondence.
- Growth: a new control kind = one wire case (C# emits it, `wire` mirrors it) + one arm here + zero shell edits.

```typescript
import { ControlIntent } from "@rasm/ts/wire/vocab"
import { Data, Match } from "effect"

declare namespace ControlPanel {
  type Row = Data.TaggedEnum<{
    Press: { readonly id: string; readonly label: string; readonly disabled: boolean }
    Switch: { readonly id: string; readonly label: string; readonly on: boolean }
    Slider: { readonly id: string; readonly label: string; readonly value: number; readonly floor: number; readonly ceiling: number; readonly step: number }
    Choice: { readonly id: string; readonly label: string; readonly chosen: string; readonly options: ReadonlyArray<string> }
  }>
}

const _Row = Data.taggedEnum<ControlPanel.Row>()

const _render: (intent: ControlIntent) => ControlPanel.Row = Match.type<ControlIntent>().pipe(
  Match.tagsExhaustive({
    Command: (intent) => _Row.Press({ id: intent.id, label: intent.label, disabled: !intent.enabled }),
    Toggle: (intent) => _Row.Switch({ id: intent.id, label: intent.label, on: intent.value }),
    Range: (intent) =>
      _Row.Slider({
        id: intent.id,
        label: intent.label,
        value: intent.value,
        floor: intent.floor,
        ceiling: intent.ceiling,
        step: intent.step,
      }),
    Choice: (intent) =>
      _Row.Choice({ id: intent.id, label: intent.label, chosen: intent.chosen, options: intent.options }),
  }),
)
```

## [3]-[VALUE_BINDING]

- Law: control values are atom state — each control's live value rides an `Atom.family` keyed by control id; RAC components run controlled (`isSelected`/`value`/`selectedKey` from `useAtomValue`, `onChange` through `useAtomSet`), so panel state is undoable, persistable, and shared like every other fold — never component-local `useState`.
- Law: wire values seed, atoms carry — the decoded intent's current value initializes the family entry (`useAtomInitialValues` at panel mount); subsequent wire refreshes reconcile through the same board pattern `viewer/panel/binding` legislates (receipt wins, optimism clears).
- Law: slider/number rows respect wire bounds verbatim — floor/ceiling/step are carriage from the intent case; a local clamp policy is the drift defect.
- Boundary: the atom mechanics are `atom/binding`/`atom/derive` settled law; validation display (a refused control write) rides the `view/compose` field-error seam.

## [4]-[INTENT_EGRESS]

- Law: interactions emit values, never calls — a press/change mints an egress intent record (`{ id, kind, payload }`) written to the app-wired gateway port; the gateway (typed against the wire command vocabulary, availability-gated per AU:59's law at `wire/gateway/command`) owns encode and transport — `ui` never encodes, never names a transport.
- Law: availability gates render — the gateway's availability verdict (an atom the app derives from `state/evidence/availability`) projects into the `disabled`/`isDisabled` prop of every materialized row, so an unavailable command renders inert with its reason as tooltip evidence rather than failing on press.
- Law: emission is fire-and-observe — the press writes `"value"`-mode; consequences arrive as fresh `ControlIntent`/livewire events through the feed, keeping the panel a projection of decoded truth.

```typescript
declare namespace Egress {
  type Intent = { readonly id: string; readonly kind: string; readonly payload: unknown }
}

const ControlPanel: {
  readonly Row: typeof _Row
  readonly render: typeof _render
} = {
  Row: _Row,
  render: _render,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ControlPanel }
```
