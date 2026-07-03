# [UI_PANEL_BINDING]

`viewer/panel/binding.ts` renders the livewire plane (AH:65): the `Livewire.Event` union — `BindingStatus` (the standing phase row over the closed `bound`/`coercing`/`refused`/`detached` vocabulary), `CoercedValue` (offered→landed with the coercion path), `WriteReceipt` (the landed value under its `Hlc` stamp) — arrives decoded through `wire/vocab` as one feed, and this module folds it into per-binding panel rows: one keyed accumulator holding each binding's phase, last coercion, and last receipt. Writes round-trip optimistically: an intent writes the panel's optimistic value, the matching `WriteReceipt` reconciles it, and a `refused` status reverts with the refusal surfaced as field evidence.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                  |
| :-----: | :----------------- | :--------------------------------------------------------------------------- |
|   [1]   | `FEED_FOLD`        | the keyed event fold — `Livewire.Event` stream → per-binding panel rows       |
|   [2]   | `STATUS_RENDER`    | the phase vocabulary rendering rows and the coercion/receipt displays         |
|   [3]   | `WRITE_ROUND_TRIP` | the optimistic write reconciled by receipt, reverted by refusal               |

## [2]-[FEED_FOLD]

- Owner: `BindingPanel.fold` — the keyed accumulator: the event feed (a `Stream<Livewire.Event>` the app wires from its transport, entering the view plane through `Atom.make(stream)`) folds via `Stream.mapAccum` into a `HashMap<binding, BindingPanel.Row>` where each event's arm updates exactly its slots — `BindingStatus` advances `phase` (and clears the optimistic value on `refused`/`detached`), `CoercedValue` records the offered→landed pair with its path, `WriteReceipt` lands the value and stamp and clears the optimistic slot; the fold is total over the union by `Match.tagsExhaustive` on the `_tag` the wire classes carry.
- Packages: `@rasm/ts/wire/vocab` (`Livewire` — `Status`/`Coerced`/`Receipt` classes and the `Event` union), `effect` (`Stream`, `HashMap`, `Match`, `Option`), `@effect-atom/atom-react`.
- Law: the row is the panel's whole truth — `phase`, `landed`, `optimistic`, `coercion`, `stamp`; a panel component reads one row through a `Atom.family` keyed by binding name and re-renders only on its own row's change.
- Law: unknown-value payloads stay opaque — `offered`/`landed` are `Schema.Unknown` on the wire by design; the panel renders them through one value-presenter row (string projection with type badge), never assuming shape.
- Boundary: the feed's transport and decode are `wire`/app composition; `Hlc` display is `intl/format` + the kernel brand's own projection.

```typescript
import { Livewire } from "@rasm/ts/wire/vocab"
import type { Hlc } from "@rasm/ts/kernel"
import { HashMap, Match, Option } from "effect"

declare namespace BindingPanel {
  type Phase = Livewire.Phase
  type Row = {
    readonly phase: Phase
    readonly landed: Option.Option<unknown>
    readonly optimistic: Option.Option<unknown>
    readonly coercion: Option.Option<{ readonly offered: unknown; readonly landed: unknown; readonly path: string }>
    readonly stamp: Option.Option<Hlc>
  }
  type Board = HashMap.HashMap<string, Row>
}

const _EMPTY: BindingPanel.Row = {
  phase: "detached",
  landed: Option.none(),
  optimistic: Option.none(),
  coercion: Option.none(),
  stamp: Option.none(),
}

const _fold = (board: BindingPanel.Board, event: Livewire.Event): BindingPanel.Board =>
  HashMap.modifyAt(board, event.binding, (slot) => {
    const row = Option.getOrElse(slot, () => _EMPTY)
    return Option.some(
      Match.value(event).pipe(
        Match.tagsExhaustive({
          BindingStatus: (status): BindingPanel.Row => ({
            ...row,
            phase: status.phase,
            optimistic: status.phase === "refused" || status.phase === "detached" ? Option.none() : row.optimistic,
          }),
          CoercedValue: (coerced): BindingPanel.Row => ({
            ...row,
            coercion: Option.some({ offered: coerced.offered, landed: coerced.landed, path: coerced.path }),
          }),
          WriteReceipt: (receipt): BindingPanel.Row => ({
            ...row,
            landed: Option.some(receipt.landed),
            optimistic: Option.none(),
            stamp: Option.some(receipt.stamp),
          }),
        }),
      ),
    )
  })
```

## [3]-[STATUS_RENDER]

- Owner: `BindingPanel.tone` — the phase styling vocabulary: one `as const` table keyed by the closed phase axis carrying tone, glyph, and motion rows (`refused` pulses `Motion.panel`, `coercing` shows the in-flight affordance); the phase chip, the coercion diff (offered vs landed with the path as a breadcrumb), and the receipt stamp (`Format.instant` through `useDateFormatter` on the `Hlc`'s wall half) are the three display rows every binding panel composes.
- Law: phase keys the table — `satisfies Record<Livewire.Phase, ...>` so a wire vocabulary change breaks this row at compile time; a phase conditional in a panel body marks the table unused.
- Law: a coercion is information, not an error — the diff renders as neutral evidence (the C# side coerced and landed the write); only `refused` renders on the danger tone and feeds the round-trip revert.
- Boundary: chip/badge primitives are `view/primitive` recipes; plural/status text is `intl/message`.

## [4]-[WRITE_ROUND_TRIP]

- Law: writes are optimistic against the feed — a panel edit writes the intent through the app-wired write port AND stamps the row's `optimistic` slot (one `Selection`-style op on the board atom); the panel displays `optimistic` over `landed` while present; the reconciling `WriteReceipt` clears it (the fold's receipt arm), and a `refused` status clears it with the refusal surfaced through the `view/compose` field-error seam.
- Law: the round trip is receipt-driven, never awaited-then-assumed — the feed is the truth channel; the write port's own acknowledgement only gates re-submission, and display state always derives from the fold.
- Law: stale optimism ages out — an optimistic slot older than the panel's patience window (a `Duration` policy row) degrades to the in-flight affordance without reverting, keeping slow transports honest without fabricating failure.
- Boundary: the write port's shape is app composition (the livewire write path is C#-owned; `ui` emits intents); refusal text localizes through `intl/message`.

```typescript
const _optimistic = (board: BindingPanel.Board, binding: string, value: unknown): BindingPanel.Board =>
  HashMap.modifyAt(board, binding, (slot) =>
    Option.some({ ...Option.getOrElse(slot, () => _EMPTY), optimistic: Option.some(value) }))

const BindingPanel: {
  readonly empty: BindingPanel.Row
  readonly fold: typeof _fold
  readonly optimistic: typeof _optimistic
} = {
  empty: _EMPTY,
  fold: _fold,
  optimistic: _optimistic,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { BindingPanel }
```
