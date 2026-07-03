# [UI_CONTROL]

`viewer/panel/control.ts` materializes the control plane (AU:82): the `ControlIntent` union arrives decoded through `wire/vocab` — the closed kind-discriminated vocabulary the host shell emits toward viewer surfaces (`Orbit`, `Pan`, `Select`, `Section`, `Measure`, `Focus`), each case `_tag`-attached at the wire seam — and this module is its one exhaustive materializer: a handler record DERIVED from the union routes every case onto the owning plane's op, payloads land verbatim as carriage, and the panel's own affordances emit the same vocabulary back through the app-wired command gateway. A new control kind is one wire union member plus one handler row here — the compile error at the missing row IS the growth mechanism, and a control materialized anywhere else re-implements the vocabulary.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                              |
| :-----: | :-------------- | :------------------------------------------------------------------- |
|   [1]   | `SINK_DISPATCH` | the derived handler record and the exhaustive intent dispatch         |
|   [2]   | `PLANE_ROUTING` | which plane owns each case — camera, selection, scene-tool routing    |
|   [3]   | `INTENT_EGRESS` | affordance state and interaction → intent emission through the gateway |

## [2]-[SINK_DISPATCH]

- Owner: `ControlPanel` — the derived dispatch: `ControlPanel.Sinks` is one mapped handler record computed from the wire union (`{ [K in ControlIntent.Tag]: (intent: Arm<K>) => void }` — the record's key space IS the wire's tag space, so a new case breaks the record loudly at compile time), and `ControlPanel.route(sinks)` closes it as the reusable record terminal — `Match.tagsExhaustive` over the sinks record is the only place intent cases meet handlers.
- Packages: `@rasm/ts/wire/vocab` (`ControlIntent` — the closed union with its `Intent`/`Tag` namespace types), `effect` (`Match`).
- Law: the materializer is total by the record — every tag demands a handler; an unknown-kind arm cannot exist because the union is closed at the wire, and the mapped contract proves coverage.
- Law: payloads are carriage — `yaw`/`pitch`, `dx`/`dy`, `targets`/`additive`, section `origin`/`normal`, measure `from`/`to`, focus `target` land verbatim on the sink; a clamp, remap, or local default is the drift defect, and an out-of-range value is upstream evidence.
- Growth: a new control kind = one wire case (C# emits it, `wire` mirrors it) + one handler row here + zero dispatch edits.

```typescript
import type { ControlIntent } from "@rasm/ts/wire/vocab"
import { Match } from "effect"

declare namespace ControlPanel {
  type Arm<K extends ControlIntent.Tag> = Extract<ControlIntent.Intent, { readonly _tag: K }>
  type Sinks = { readonly [K in ControlIntent.Tag]: (intent: ControlPanel.Arm<K>) => void }
  type Egress = { readonly kind: ControlIntent.Tag; readonly payload: unknown }
}

const _route = (sinks: ControlPanel.Sinks): ((intent: ControlIntent.Intent) => void) =>
  Match.type<ControlIntent.Intent>().pipe(Match.tagsExhaustive(sinks))
```

## [3]-[PLANE_ROUTING]

- Law: each case has exactly one owning plane — `Orbit`/`Pan` mint `Camera.Intent` values through `viewer/geo/project`'s adapters (a yaw/pitch or dx/dy delta becomes an `EaseTo` over the live camera state); `Select` mints `Selection.Op` (`additive` selects `Add` versus `Replace` — modality lives in the op value, `viewer/mark/selection`'s law); `Focus` mints a fit intent over the target's bounds; `Section` and `Measure` land on the scene-tool rows their owners earn at `viewer/scene` — one sink, one plane, never a case handled twice.
- Law: sinks are app-composed — the shell binds each sink to the owning plane's atom write at composition; this module never imports the planes, because the record IS the seam and a direct plane import would couple the panel to every surface it can drive.
- Law: routing is replayable — every arriving intent lands as an op/intent value on a plane's fold, so the control stream composes with `History` undo and the probe planes exactly like locally-minted interaction.

## [4]-[INTENT_EGRESS]

- Law: interactions emit values, never calls — a panel affordance (a tool button, a section handle) mints an egress record (`ControlPanel.Egress` — the wire tag plus its payload) written to the app-wired gateway port; the gateway (typed against the wire command vocabulary, availability-gated per AU:59's law at `wire/gateway/command`) owns encode and transport — `ui` never encodes, never names a transport.
- Law: affordance state rides atoms — the active tool, an additive modifier, an in-flight measure endpoint live in `Atom.family` rows keyed by control id, RAC components running controlled (`isSelected`/`value` from `useAtomValue`, `onChange` through `useAtomSet`) — never component-local `useState`.
- Law: availability gates render — the gateway's availability verdict (an atom the app derives from `state/evidence/availability`) projects into the `disabled`/`isDisabled` prop of every affordance, so an unavailable command renders inert with its reason as tooltip evidence rather than failing on press.
- Law: emission is fire-and-observe — the press writes `"value"`-mode; consequences arrive as fresh `ControlIntent`/livewire events through the feed, keeping the panel a projection of decoded truth.

```typescript
const ControlPanel: {
  readonly route: typeof _route
} = {
  route: _route,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ControlPanel }
```
