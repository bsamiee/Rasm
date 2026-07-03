# [WIRE_RESIDENCY]

`frame/residency.ts` decodes the `GeometryResidencyWire` protocol from `Rasm.AppUi/Render`: one kind-discriminated envelope carrying either the `ResidencyManifest` — the content-key-keyed statement of which meshes are resident, pending, or evicted at each LOD — or a residency `Delta`, the single-row transition that keeps a long viewing session's ledger current without re-shipping manifests. `browser/transport/pool` consumes the feed through `#vocab` to drive fetch scheduling: the manifest is the plan, deltas are the plan's evolution, and every coordinate is a verbatim content key so the residency ledger, the artifact receipts, and the viewer's scene keys all speak one identity.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                             |
| :-----: | :------------------- | :------------------------------------------------------------------------ |
|   [1]   | `RESIDENCY_PROTOCOL` | the state vocabulary, the manifest/delta envelope, the feed, the ledger fold |

## [2]-[RESIDENCY_PROTOCOL]

- Owner: `Residency` — one assembled owner: the closed `state` vocabulary (`resident`, `pending`, `evicted`), the `Manifest` class (mesh rows keyed by content key with LOD and extent), the `Delta` class (one row's transition), the envelope union both arrive on, and the one polymorphic ledger fold.
- Entry: `Residency.envelope` the wire byte schema — the `GeometryResidencyWire` message is a kind-discriminated envelope and the decoded value is `Manifest | Delta`; `Residency.stream(frames)` the feed; `Residency.folded(ledger, arrival)` the one fold — a `Manifest` arrival REPLACES the ledger, a `Delta` arrival evolves it — discriminated on the arrival value, never a second entrypoint.
- Receipt: the ledger IS the fetch plan — `pending` rows are the fetch queue, `resident` rows are addressable, `evicted` rows are reclaimable; the pool schedules against it and the artifact rail's receipts confirm arrivals.
- Growth: a new residency state (a `prefetch` tier) is one literal row plus its policy column — the fold and every exhaustive consumer break until handled; a new row axis (a priority score, a byte budget) is one field on the row struct.
- Law: content keys are the join — manifest rows, deltas, artifact receipts, and scene keys share the kernel `ContentKey`; a mesh named two ways cannot exist, so residency questions are `HashMap` lookups, never scans.
- Law: deltas are idempotent transitions — applying a delta that matches the ledger's current state is a no-op, not a fault; the wire may replay deltas across reconnects and the fold absorbs them, which is what makes the protocol resumable without a handshake.
- Law: the manifest is authoritative at arrival — the C# render side owns truth, so the manifest arm of `folded` discards the prior ledger whole; deltas only evolve the last manifest, and a delta referencing an unknown key folds to a `pending` insert because the manifest that introduces it may still be in flight.
- Law: one wire family, one envelope — manifest and delta are the two arms of the same `GeometryResidencyWire` message; a second decode surface per arm would fork the family the census fences to this page.
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

const _envelope = Schema.Union(Manifest, Delta)

declare namespace Residency {
  type State = (typeof _states)[number]
  type Row = Schema.Schema.Type<typeof _Row>
  type Arrival = Manifest | Delta
  type Ledger = HashMap.HashMap<ContentKey, Row>
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly Delta: typeof Delta
  readonly envelope: Schema.Schema<Residency.Arrival, Uint8Array>
  readonly stream: (frames: AsyncIterable<Uint8Array>) => Stream.Stream<Residency.Arrival, WireFault | ParseResult.ParseError>
  readonly folded: (ledger: Residency.Ledger, arrival: Residency.Arrival) => Residency.Ledger
} = {
  Manifest,
  Delta,
  envelope: ProtoCodec.family(ProtoCodec.suite.GeometryResidencyWire, _envelope),
  stream: (frames) =>
    ProtoCodec.stream(ProtoCodec.suite.GeometryResidencyWire, "GeometryResidencyWire")(frames).pipe(
      Stream.mapEffect(Schema.decodeUnknown(_envelope), { concurrency: 1 }),
    ),
  folded: (ledger, arrival) =>
    arrival instanceof Manifest
      ? Array.reduce(arrival.rows, HashMap.empty<ContentKey, Residency.Row>(), (acc, row) => HashMap.set(acc, row.mesh, row))
      : HashMap.set(ledger, arrival.mesh, { mesh: arrival.mesh, lod: arrival.lod, extent: arrival.extent, state: arrival.state }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Residency }
```
