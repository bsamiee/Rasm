# [WIRE_RESIDENCY]

`frame/residency.ts` decodes the `GeometryResidencyWire` protocol from `Rasm.AppUi/Render`: the `ResidencyManifest` — the content-key-keyed statement of which meshes are resident, pending, or evicted at each LOD — and the residency delta stream that keeps a long viewing session's ledger current without re-shipping manifests. `browser/transport/pool` consumes both through `#vocab` to drive fetch scheduling: the manifest is the plan, deltas are the plan's evolution, and every coordinate is a verbatim content key so the residency ledger, the artifact receipts, and the viewer's scene keys all speak one identity.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                             |
| :-----: | :------------------- | :------------------------------------------------------------------------ |
|   [1]   | `RESIDENCY_PROTOCOL` | the state vocabulary, the manifest owner, the delta stream, the ledger fold |

## [2]-[RESIDENCY_PROTOCOL]

- Owner: `Residency` — one assembled owner: the closed `state` vocabulary (`resident`, `pending`, `evicted`), the `Manifest` class (mesh rows keyed by content key with LOD and extent), the `Delta` class (one row's transition), and the ledger fold that applies deltas to a manifest-derived ledger.
- Entry: `Residency.manifest` the manifest byte schema; `Residency.stream(frames)` the delta feed; `Residency.folded(ledger, delta)` the one transition fold the pool's ledger cell applies.
- Receipt: the ledger IS the fetch plan — `pending` rows are the fetch queue, `resident` rows are addressable, `evicted` rows are reclaimable; the pool schedules against it and the artifact rail's receipts confirm arrivals.
- Growth: a new residency state (a `prefetch` tier) is one literal row plus its policy column — the fold and every exhaustive consumer break until handled; a new row axis (a priority score, a byte budget) is one field on the row struct.
- Law: content keys are the join — manifest rows, deltas, artifact receipts, and scene keys share the kernel `ContentKey`; a mesh named two ways cannot exist, so residency questions are `HashMap` lookups, never scans.
- Law: deltas are idempotent transitions — applying a delta that matches the ledger's current state is a no-op, not a fault; the wire may replay deltas across reconnects and the fold absorbs them, which is what makes the protocol resumable without a handshake.
- Law: the manifest is authoritative at arrival — a fresh manifest REPLACES the ledger (the C# render side owns truth); deltas only evolve the last manifest, and a delta referencing an unknown key folds to a `pending` insert because the manifest that introduces it may still be in flight.
- Boundary: fetch scheduling, worker transfer, and byte budgets are `browser/transport/pool`'s policy; the artifact verify that flips `pending` to `resident` is `frame/artifact.ts`'s receipt, joined at the pool.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Array, HashMap, type ParseResult, Schema, Stream } from "effect"
import type { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

const _states = ["resident", "pending", "evicted"] as const

const _Row = Schema.Struct({
  mesh: ContentKey.FromCell,
  lod: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.nonNegative()),
  state: Schema.Literal(..._states),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  scene: ContentKey.FromCell,
  rows: Schema.Array(_Row),
  minted: Schema.DateTimeUtc,
}) {}

class Delta extends Schema.Class<Delta>("Delta")({
  mesh: ContentKey.FromCell,
  lod: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.nonNegative()),
  state: Schema.Literal(..._states),
}) {}

declare namespace Residency {
  type State = (typeof _states)[number]
  type Row = Schema.Schema.Type<typeof _Row>
  type Ledger = HashMap.HashMap<ContentKey, Row>
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly Delta: typeof Delta
  readonly manifest: Schema.Schema<Manifest, Uint8Array>
  readonly stream: (frames: AsyncIterable<Uint8Array>) => Stream.Stream<Delta, WireFault | ParseResult.ParseError>
  readonly opened: (manifest: Manifest) => Residency.Ledger
  readonly folded: (ledger: Residency.Ledger, delta: Delta) => Residency.Ledger
} = {
  Manifest,
  Delta,
  manifest: ProtoCodec.family(ProtoCodec.suite.GeometryResidencyWire, Manifest),
  stream: (frames) =>
    ProtoCodec.stream(ProtoCodec.suite.GeometryResidencyWire, "GeometryResidencyWire")(frames).pipe(
      Stream.mapEffect(Schema.decodeUnknown(Delta), { concurrency: 1 }),
    ),
  opened: (manifest) =>
    Array.reduce(manifest.rows, HashMap.empty<ContentKey, Residency.Row>(), (acc, row) => HashMap.set(acc, row.mesh, row)),
  folded: (ledger, delta) =>
    HashMap.set(ledger, delta.mesh, { mesh: delta.mesh, lod: delta.lod, extent: delta.extent, state: delta.state }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Residency }
```
