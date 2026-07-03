# [WIRE_BIM]

`codec/bim.ts` decodes the BIM exchange plane from `Rasm.Bim/Exchange` — `BimWire`, `DiffWire`, `IdsAuditWire` — under the golden-byte law: these are the families whose byte-for-byte parity against the `tests/contracts` corpus is the cross-language proof (BM:98), so every decode holds its source octets recoverable and re-encode is byte-identity or defect. `BimWire` carries the model exchange envelope, `DiffWire` the element-level change set keyed by GlobalId anchors, `IdsAuditWire` the IDS validation verdict rows; `ui/viewer` marks and overlays consume all three through `#vocab`, and IFC vocabulary stays fenced to this codec and the viewer marks (invariant 7).

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                |
| :-----: | :---------------- | :--------------------------------------------------------------------------- |
|   [1]   | `EXCHANGE_FAMILY` | the three exchange owners, the GlobalId anchor refinement, the keyed decode    |
|   [2]   | `GOLDEN_PARITY`   | the byte-identity law: held octets, re-encode proof, corpus coordination       |

## [2]-[EXCHANGE_FAMILY]

- Owner: `BimExchange` — one assembled owner over three `Schema.Class` families: `Model` (the `BimWire` envelope: schema dialect, element census, content key), `Diff` (`DiffWire`: added/removed/modified anchor sets), `IdsAudit` (`IdsAuditWire`: per-specification verdict rows over a closed verdict vocabulary).
- Entry: `BimExchange.model`/`BimExchange.diff`/`BimExchange.audit` per-family byte schemas; `BimExchange.decode(family, octets)` the keyed one-shot whose return follows the family key.
- Receipt: a `Diff` is the overlay's worklist — three anchor sets the viewer paints; an `IdsAudit` is compliance evidence — specification, verdict row, and the failing anchors, rendered without TS re-validation because the audit ran where the model lives.
- Growth: a new exchange projection is one census row plus one class here; a new IDS verdict kind is one literal row every exhaustive consumer breaks on.
- Law: GlobalId anchors are brand-refined fields shared across the three families — one interior `_GlobalId` schema composed into each field record, exported by none; anchors cross verbatim and resolve against the loaded model only in the viewer.
- Law: the audit verdict vocabulary is closed — `pass`, `fail`, `unapplicable` — mirroring the C# IDS oracle rows; a free-string verdict is the rejected shape, and TS never re-runs a specification (the audit is decode-only evidence).
- Law: the diff is anchor-precise — `modified` rows carry the changed attribute names beside the anchor so overlays annotate WHAT changed, not merely that something did.
- Boundary: mark rendering is `ui/viewer` `mark/bcf` and its siblings; graph-level content-keyed decode is `codec/graph.ts`; the IDS oracle itself is C#'s (`Rasm.Bim` + the `ifc` toolchain), never re-implemented.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _GlobalId = Schema.String.pipe(
  Schema.length(22),
  Schema.pattern(/^[0-9A-Za-z_$]{22}$/),
  Schema.brand("GlobalId"),
)

const _verdicts = ["pass", "fail", "unapplicable"] as const

class Model extends Schema.Class<Model>("Model")({
  key: ContentKey.FromCell,
  dialect: Schema.NonEmptyString,
  elements: Schema.Int.pipe(Schema.nonNegative()),
  minted: Schema.DateTimeUtc,
}) {}

class Diff extends Schema.Class<Diff>("Diff")({
  base: ContentKey.FromCell,
  next: ContentKey.FromCell,
  added: Schema.Array(_GlobalId),
  removed: Schema.Array(_GlobalId),
  modified: Schema.Array(Schema.Struct({ anchor: _GlobalId, attributes: Schema.Array(Schema.NonEmptyString) })),
}) {}

class IdsAudit extends Schema.Class<IdsAudit>("IdsAudit")({
  specification: Schema.NonEmptyString,
  verdicts: Schema.Array(Schema.Struct({
    requirement: Schema.NonEmptyString,
    verdict: Schema.Literal(..._verdicts),
    anchors: Schema.Array(_GlobalId),
  })),
}) {}

const _rows = {
  BimWire: ProtoCodec.family(ProtoCodec.suite.BimWire, Model),
  DiffWire: ProtoCodec.family(ProtoCodec.suite.DiffWire, Diff),
  IdsAuditWire: ProtoCodec.family(ProtoCodec.suite.IdsAuditWire, IdsAudit),
} as const

declare namespace BimExchange {
  type Family = keyof typeof _rows
  type Decoded<K extends Family> = Schema.Schema.Type<(typeof _rows)[K]>
  type Verdict = (typeof _verdicts)[number]
}
```

## [3]-[GOLDEN_PARITY]

- Owner: the byte-identity proof on the same module — `roundtrip`, the golden-byte check: decode held octets, re-encode through the same schema, compare byte-for-byte.
- Entry: `BimExchange.roundtrip(family, octets)` — `Effect<void, ParseError | WireFault>`: a mismatch is a `parity` `WireFault` carrying both byte extents; the `tests/contracts` corpus driver runs this over the pinned golden fixtures, and a production consumer runs it only where forwarding demands proof.
- Law: golden-byte parity is the exchange contract — the C# writer's bytes decode and re-encode identically because the proto engine preserves unknown fields and field order is deterministic; a re-encode that differs proves either engine drift or contract drift, and the fault's evidence carries both lengths for the first-divergence read.
- Law: the corpus is the authority — fixtures pin the golden bytes (BM:98), the driver in `tests/typescript/_testkit` asserts them per generation, and this module ships the check as a value so the driver and a forwarding path share one spelling.
- Boundary: fixture storage and the assertion harness are the tests estate's; this module owns only the check.

```typescript
import { Option } from "effect"
import { WireFault } from "../fault/quarantine.ts"

const _roundtrip = <K extends BimExchange.Family>(family: K, octets: Uint8Array): Effect.Effect<void, ParseResult.ParseError | WireFault> =>
  Effect.gen(function* () {
    const decoded = yield* Schema.decodeUnknown(_rows[family])(octets)
    const emitted = yield* Schema.encode(_rows[family])(decoded)
    const identical = emitted.length === octets.length && emitted.every((byte, index) => byte === octets[index])
    return identical
      ? undefined
      : yield* new WireFault({
          family,
          reason: "parity",
          detail: "<golden-byte-divergence>",
          evidence: Option.some({ actual: emitted.length, expected: octets.length }),
        })
  })

const BimExchange: {
  readonly Model: typeof Model
  readonly Diff: typeof Diff
  readonly IdsAudit: typeof IdsAudit
  readonly model: (typeof _rows)["BimWire"]
  readonly diff: (typeof _rows)["DiffWire"]
  readonly audit: (typeof _rows)["IdsAuditWire"]
  readonly decode: <K extends BimExchange.Family>(family: K, octets: Uint8Array) => Effect.Effect<BimExchange.Decoded<K>, ParseResult.ParseError>
  readonly roundtrip: typeof _roundtrip
} = {
  Model,
  Diff,
  IdsAudit,
  model: _rows.BimWire,
  diff: _rows.DiffWire,
  audit: _rows.IdsAuditWire,
  decode: (family, octets) => Schema.decodeUnknown(_rows[family])(octets),
  roundtrip: _roundtrip,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { BimExchange }
```
