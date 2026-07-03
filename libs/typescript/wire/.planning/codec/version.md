# [WIRE_VERSION]

`codec/version.ts` decodes the version-control plane of the C# wire — `CommitWire`, `BranchWire`, `VersionVectorWire`, `MerkleSummaryWire` from `Rasm.Persistence/Version` — INTO `state/causal`'s owned vocabulary over the shared `Pack` engine. The four families ride one MessagePack mint site, so this module is four composed schemas and one assembled owner: `state` owns the shapes (`Commit`, `Branch`, `VersionVector`, `MerkleSummary`), `wire` owns only the byte crossing, and comparison, happened-before folds, and Merkle divergence walks never appear here. Consumers reach the decode surfaces through `#vocab`; `state/causal/vector` consumes decoded values at the app root.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                          |
| :-----: | :--------------- | :----------------------------------------------------------------- |
|   [1]   | `VERSION_FAMILY` | the four byte→state-vocabulary schemas under one assembled owner    |

## [2]-[VERSION_FAMILY]

- Owner: `Version` — one assembled owner whose members are the four composed decode schemas plus their one-shot rails; the state shapes arrive by import and are never re-declared, re-fielded, or projected here.
- Entry: `Version.commit`/`Version.branch`/`Version.vector`/`Version.merkle` — `Schema<StateShape, Uint8Array>` each; `Version.decode` is the modality-polymorphic one-shot: the family discriminant selects the schema by census key, and the return type follows the key through the mapped contract.
- Receipt: decoded values are `state`-owned evidence — a `Commit` carries its author/`Hlc`/parent coordinates, a `VersionVector` its per-actor `bigint` entries, a `MerkleSummary` its level-keyed digests; every downstream read is a `state` fold.
- Growth: a new version-plane family is one census row, one state shape, and one member row here — the mapped decode contract picks it up and the `Version.decode` key union widens itself.
- Law: decode INTO the owner — the state shapes' encoded sides mirror the MessagePack maps field-for-field, `Hlc` fields arrive pre-interned by the `Pack` extension row, and vector entries stay `bigint`; a wire-local twin of any state shape is the parallel-shape defect.
- Law: Merkle summaries are comparison MATERIAL, not comparisons — the divergence walk that turns two summaries into a sync frontier is `state/causal`'s fold; this module lands the digests verbatim (16-byte cells under `tagUint8Array` handling in the engine).
- Law: the four schemas share one engine — a second configured decoder, a per-family ceiling override, or a local extension row is `Pack`'s monopoly violated; family-level policy lands in `Pack` or not at all.
- Boundary: `codec/crdt.ts` owns the engine; `state/causal/vector` owns the shapes and their algebra; OCC patch guards that reference version vectors are `codec/patch.ts`'s egress.

```typescript
import { Branch, Commit, MerkleSummary, VersionVector } from "@rasm/ts/state"
import { Effect, type ParseResult, Schema } from "effect"
import { Pack } from "./crdt.ts"

const _rows = {
  CommitWire: Pack.schema(Commit),
  BranchWire: Pack.schema(Branch),
  VersionVectorWire: Pack.schema(VersionVector),
  MerkleSummaryWire: Pack.schema(MerkleSummary),
} as const

declare namespace Version {
  type Family = keyof typeof _rows
  type Decoded<K extends Family> = Schema.Schema.Type<(typeof _rows)[K]>
  type Shape = {
    readonly commit: (typeof _rows)["CommitWire"]
    readonly branch: (typeof _rows)["BranchWire"]
    readonly vector: (typeof _rows)["VersionVectorWire"]
    readonly merkle: (typeof _rows)["MerkleSummaryWire"]
    readonly decode: <K extends Family>(family: K, octets: Uint8Array) => Effect.Effect<Decoded<K>, ParseResult.ParseError>
  }
  type _Keys<K extends Family = keyof typeof _rows> = K
}

const Version: Version.Shape = {
  commit: _rows.CommitWire,
  branch: _rows.BranchWire,
  vector: _rows.VersionVectorWire,
  merkle: _rows.MerkleSummaryWire,
  decode: (family, octets) => Schema.decodeUnknown(_rows[family])(octets),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Version }
```
