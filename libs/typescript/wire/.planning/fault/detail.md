# [WIRE_DETAIL]

`fault/detail.ts` is the wire-only fault altitude: it reconstructs the C#-minted `FaultDetail` — the one fault shape that crossed a process boundary — over the closed sixteen-row `HopReason` vocabulary, and it owns `fromConnect`, the total fold that turns every `ConnectError` the invocation client catches into a reconstructed `FaultDetail` with its hop chain intact. `FaultDetail` is an adopted-verbatim name (invariant 8): C# owns the shape, this module decodes and reconstructs it, and no TS rail mints one for a local failure — local failures ride `WireFault` and every other folder's own `Data.TaggedError` rail. The module also provides the Layer implementing the `kernel/fault` enricher contract, so `telemetry` crash capture consumes reconstructed faults without ever importing `wire`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                        |
| :-----: | :--------------- | :----------------------------------------------------------------------------- |
|   [1]   | `HOP_VOCABULARY` | the closed sixteen-row `HopReason` table: gRPC code, retryability, terminality  |
|   [2]   | `FAULT_DETAIL`   | `Hop` + the `FaultDetail` owner: wire twin, `fromConnect` fold, enricher Layer  |

## [2]-[HOP_VOCABULARY]

- Owner: `Hops` — one `as const` table keyed by hop reason, aligned row-for-row with the sixteen-value Connect `Code` enum (1–16) the C# vocabulary shares; the merged hub derives the reason union, the code→reason reverse record, and the wire literal.
- Entry: `Hops[reason]` policy lookup; `Hops.fromCode` the total code→reason projection `fromConnect` folds through; `Hops.wire` the schema literal `FaultDetail` fields decode with.
- Growth: the table is closed by the gRPC status contract — a new policy axis (budget class, alarm tier) is a column edit; a seventeenth reason is a wire-contract change that lands here first and breaks every exhaustive consumer loudly.
- Law: `retryable` is the one retry gate the invocation client reads — `deadline`, `exhausted`, `aborted`, and `unavailable` re-drive; everything else is terminal for the attempt. A call-site predicate re-deriving retryability from codes is policy leakage.
- Law: `terminal` marks reasons no schedule may re-drive even under a wider budget — `denied`, `unimplemented`, `dataloss`, `unauthenticated` — the distinction between a transient wire and a wrong program.
- Law: the code→reason projection is generated from the table's own `code` column inside one `Array.reduce` over the key tuple, so the two directions cannot drift; a hand-listed reverse map is the parallel-restatement defect.

```typescript
import { Array, HashMap, Option, Schema } from "effect"

const _reasons = [
  "canceled", "unknown", "invalid", "deadline", "notfound", "exists", "denied", "exhausted",
  "precondition", "aborted", "range", "unimplemented", "internal", "unavailable", "dataloss", "unauthenticated",
] as const

const _hops = {
  canceled: { code: 1, retryable: false, terminal: false },
  unknown: { code: 2, retryable: false, terminal: false },
  invalid: { code: 3, retryable: false, terminal: false },
  deadline: { code: 4, retryable: true, terminal: false },
  notfound: { code: 5, retryable: false, terminal: false },
  exists: { code: 6, retryable: false, terminal: false },
  denied: { code: 7, retryable: false, terminal: true },
  exhausted: { code: 8, retryable: true, terminal: false },
  precondition: { code: 9, retryable: false, terminal: false },
  aborted: { code: 10, retryable: true, terminal: false },
  range: { code: 11, retryable: false, terminal: false },
  unimplemented: { code: 12, retryable: false, terminal: true },
  internal: { code: 13, retryable: false, terminal: false },
  unavailable: { code: 14, retryable: true, terminal: false },
  dataloss: { code: 15, retryable: false, terminal: true },
  unauthenticated: { code: 16, retryable: false, terminal: true },
} as const

const _byCode: HashMap.HashMap<number, Hops.Reason> = Array.reduce(
  _reasons,
  HashMap.empty<number, Hops.Reason>(),
  (acc, reason) => HashMap.set(acc, _hops[reason].code, reason),
)

declare namespace Hops {
  type Reason = keyof typeof _hops
  type Row = { readonly code: number; readonly retryable: boolean; readonly terminal: boolean }
  type Shape = typeof _hops & {
    readonly reasons: typeof _reasons
    readonly wire: Schema.Literal<typeof _reasons>
    readonly fromCode: (code: number) => Reason
  }
  type _Rows<T extends Record<Reason, Row> = typeof _hops> = T
  type _Keys<K extends Reason = (typeof _reasons)[number]> = K
}

const Hops: Hops.Shape = {
  ..._hops,
  reasons: _reasons,
  wire: Schema.Literal(..._reasons),
  fromCode: (code) => Option.getOrElse(HashMap.get(_byCode, code), () => "unknown"),
}
```

## [3]-[FAULT_DETAIL]

- Owner: `FaultDetail` — one `Schema.TaggedError` class: the reconstructed wire fault is yieldable on the rail, serializes structurally, and carries its hop chain as typed `Hop` rows; `Hop` is the per-boundary evidence row (site, reason, elapsed span). Every derived surface — wire twin, transport fold, enricher Layer — rides the class as a static, so one import carries the whole altitude.
- Entry: `FaultDetail.FromWire` — the wire twin composing `codec/proto.ts`'s suite row, so proto-framed fault payloads decode through the one proto engine; `FaultDetail.fromConnect(caught)` — the total transport fold; `detail.retryable`/`detail.terminal` project the reason's policy row.
- Receipt: the hop chain is the forensic spine — each boundary the fault crossed appended one `Hop`; reconstruction preserves order, so the first hop is the origin, the last is the local edge, and `origin` projects it.
- Growth: a new evidence axis (span identity, quota coordinate) is one field on `FaultDetail` plus its wire twin field — the C# shape moves first, this reconstruction follows field-for-field.
- Law: wire-only altitude (invariant 6) — `FaultDetail` is constructed at exactly two sites: the `FromWire` decode of a C#-minted payload and the `fromConnect` fold over a transport error. A third construction site anywhere in the branch is the defect the architecture suite audits.
- Law: the fold is total — `ConnectError.from` normalizes any caught value (an `AbortError`/`TimeoutError` lands as `Canceled`), `Hops.fromCode` maps the closed enum with `unknown` as the residue row, and `findDetails` against the `FaultDetailWire` descriptor decodes a server-attached detail whose hop chain merges ahead of the local edge hop; no `if` ladder inspects codes.
- Law: `EnricherLive` satisfies the kernel-declared `FaultEnricher` Tag with this module's reconstruction — telemetry consumes the contract, wire provides the Layer at the app root, and the `telemetry -> wire` import stays structurally impossible.
- Law: policy is a projection — `retryable` reads `Hops`; `message` derives from fields and is never stored.
- Boundary: `ConnectError`/`Code`/`findDetails` spellings are the `@connectrpc/connect` surface; the invocation seam that catches them is `invoke/client.ts`; the `FaultDetailWire` generated schema and its descriptor-option vocabulary hook live at `codec/proto.ts`; the `FaultEnricher` Tag declaration is `kernel/fault/classify`.

```typescript
import { Code, ConnectError } from "@connectrpc/connect"
import { FaultEnricher } from "@rasm/ts/kernel"
import { Array, Duration, Layer, Option, Schema } from "effect"
import { ProtoCodec } from "../codec/proto.ts"

class Hop extends Schema.Class<Hop>("Hop")({
  site: Schema.NonEmptyString,
  reason: Hops.wire,
  elapsed: Schema.Duration,
}) {}

const _edge = (error: ConnectError): Hop =>
  new Hop({ site: "<local-edge>", reason: Hops.fromCode(error.code), elapsed: Duration.zero })

class FaultDetail extends Schema.TaggedError<FaultDetail>()("FaultDetail", {
  reason: Hops.wire,
  surface: Schema.NonEmptyString,
  detail: Schema.NonEmptyString,
  hops: Schema.Array(Hop),
  tenant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {
  static readonly Hop: typeof Hop = Hop
  static readonly FromWire: Schema.Schema<FaultDetail, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.FaultDetailWire, FaultDetail)
  static readonly fromConnect = (caught: unknown): FaultDetail => {
    const error = ConnectError.from(caught, Code.Unknown)
    const carried = Array.head(error.findDetails(ProtoCodec.suite.FaultDetailWire))
    return Option.match(
      Option.flatMap(carried, (wire) => Option.getRight(Schema.decodeUnknownEither(FaultDetail)(wire))),
      {
        onNone: () =>
          new FaultDetail({
            reason: Hops.fromCode(error.code),
            surface: "<transport>",
            detail: error.rawMessage,
            hops: [_edge(error)],
            tenant: Option.none(),
          }),
        onSome: (detail) => new FaultDetail({ ...detail, hops: [...detail.hops, _edge(error)] }),
      },
    )
  }
  static readonly EnricherLive: Layer.Layer<FaultEnricher> = Layer.succeed(
    FaultEnricher,
    FaultEnricher.of({ enrich: (caught: unknown) => FaultDetail.fromConnect(caught) }),
  )
  get retryable(): boolean {
    return Hops[this.reason].retryable
  }
  get terminal(): boolean {
    return Hops[this.reason].terminal
  }
  get origin(): Option.Option<Hop> {
    return Array.head(this.hops)
  }
  override get message(): string {
    return `<${this.surface}:${this.reason}> ${this.detail}`
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { FaultDetail, Hops }
```
