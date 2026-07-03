# [KERNEL_CLASSIFY]

The cross-language fault classification law: `FaultClass` is the one severity-ordered vocabulary every rail in the branch inherits — ten classes, each row carrying rank, retryability, and blame — so routing, dominance folds, and budget gates read one table instead of re-deriving semantics per folder. `FaultCapture` is the kernel-shaped crash-evidence value, and `FaultEnricher` is the enricher CONTRACT declared here and only here: `telemetry` consumes the port, `wire` implements the Layer that reconstructs wire-grade forensics, and the app root wires them — this floor imports neither (invariant 6 keeps the three fault altitudes distinct: `wire/fault` reconstruction, per-folder `Data.TaggedError` rails, `edge/problem` outbound mapping). The module is `kernel/src/fault/classify.ts`; a new fault class is one tuple entry plus one row, inherited by every rail with zero new branches.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                       | [PUBLIC]                       |
| :-----: | :---------------- | :------------------------------------------------------------ | :----------------------------- |
|  [01]   | `CLASS_VOCABULARY` | the ten-class table, classification fold, dominance lattice   | `FaultClass`                   |
|  [02]   | `ENRICHER_CONTRACT` | the capture evidence model and the enrichment port           | `FaultCapture`, `FaultEnricher` |

## [2]-[CLASS_VOCABULARY]

[CLASS_VOCABULARY]:
- Owner: `FaultClass`, the assembled vocabulary — the interior key tuple fixes severity order as iteration order (rank ascends with position, a load-bearing sequence), the interior row table carries the axes, the merged hub carries every derived type plus the guard pair, and the exported owner assembles rows, `kinds`, `schema`, and the operation members under a `typeof`-derived stated annotation.
- Law: the roster is sized by cross-language routing, never by cause — `absent`, `conflicted`, `invalid`, `malformed`, `denied`, `expired`, `exhausted`, `unavailable`, `breached`, `defect` — and a finer cause is a `reason` row inside the owning folder's fault class, never an eleventh entry minted for one surface.
- Law: rows carry three axes — `rank` (the dominance lattice a report folds under), `retryable` (the transient family a budget gate re-drives), `blame` (`"caller"` | `"system"`, the accountability split `edge/problem` and `telemetry` project) — and behavior variation across the branch reads these columns, never a `switch` over class names.
- Law: the classification convention is structural — a folder fault carries `readonly class: FaultClass.Kind` as a field, `FaultClass.of` probes any value for it and folds everything else to `"defect"`, so classification is total over `unknown` and an unclassified foreign throw lands at the correct terminal severity.
- Law: `dominant` collapses an accumulated non-empty fault set to its representative through the rank lattice — the fold `Effect.validateAll`-shaped reports feed — and `retryable` is the one gate `fault/budget` compiles into every schedule.
- Law: `schema` is the wire-facing literal union derived from the tuple spread — the non-empty overload keeps the exact literal tuple — so a class crossing a wire or a config row decodes against the same anchor the type plane derives from.
- Growth: a new class is one tuple entry plus one row — every guard, schema, fold, and budget gate inherits it at compile time; a new axis is one `Row` field plus its column on each row.
- Boundary: `edge/problem` owns the class-to-status outbound mapping as its own governed record; the kernel table stays transport-free.
- Packages: `effect` (`Schema`, `Order`, `Array`, `Predicate`).

```typescript
import { Array, Context, Effect, Layer, Order, Predicate, Record, Schema, type Types } from "effect"
import { Refined } from "../schema/brand.ts"

const _kinds = [
  "absent",
  "conflicted",
  "invalid",
  "malformed",
  "denied",
  "expired",
  "exhausted",
  "unavailable",
  "breached",
  "defect",
] as const

const _rows = {
  absent: { rank: 1, retryable: false, blame: "caller" },
  conflicted: { rank: 2, retryable: true, blame: "caller" },
  invalid: { rank: 3, retryable: false, blame: "caller" },
  malformed: { rank: 4, retryable: false, blame: "caller" },
  denied: { rank: 5, retryable: false, blame: "caller" },
  expired: { rank: 6, retryable: true, blame: "system" },
  exhausted: { rank: 7, retryable: true, blame: "system" },
  unavailable: { rank: 8, retryable: true, blame: "system" },
  breached: { rank: 9, retryable: false, blame: "system" },
  defect: { rank: 10, retryable: false, blame: "system" },
} as const

const _Kind = Schema.Literal(..._kinds)
const _is = Schema.is(_Kind)
const _byRank = Order.mapInput(Order.number, (kind: FaultClass.Kind) => _rows[kind].rank)

const _of = (fault: unknown): FaultClass.Kind =>
  Predicate.hasProperty(fault, "class") && _is(fault.class) ? fault.class : "defect"

declare namespace FaultClass {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Blame = "caller" | "system"
  type Row = { readonly rank: number; readonly retryable: boolean; readonly blame: Blame }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly schema: typeof _Kind
      readonly is: (input: unknown) => input is Kind
      readonly of: (fault: unknown) => Kind
      readonly retryable: (fault: unknown) => boolean
      readonly dominant: (classes: Array.NonEmptyReadonlyArray<Kind>) => Kind
    }
  >
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const FaultClass: FaultClass.Shape = {
  ..._rows,
  kinds: _kinds,
  schema: _Kind,
  is: _is,
  of: _of,
  retryable: (fault) => _rows[_of(fault)].retryable,
  dominant: (classes) => Array.max(classes, _byRank),
}
```

## [3]-[ENRICHER_CONTRACT]

[ENRICHER_CONTRACT]:
- Owner: `FaultCapture`, the kernel-shaped crash-evidence model — class, fault tag, owning surface, detail, optional `Refined.Guid` correlation, capture instant, and an open string attribute band — the value `telemetry/crash` constructs from a folded `Cause` and every enrichment round-trips; `policy` projects the class row so severity and blame are recoverable from any capture, and `enriched` is the successor constructor merging attribute bands last-write-wins.
- Owner: `FaultEnricher`, the enrichment port — one `Context.Tag` whose service is a single endo-arrow `enrich: (capture) => Effect<FaultCapture>` — `wire` provides the Layer that reconstructs `FaultDetail`-grade forensics into the attribute band, `telemetry` yields the Tag, and the app root wires them; the wire-owned `FaultDetail` name never appears in this contract, keeping the adopted-verbatim name on its owning side.
- Law: enrichment is total by signature — the error channel is `never`, so a failing enricher implementation resolves its own faults internally (degrade to the unenriched capture) and crash capture can never be broken by its own forensics.
- Law: `FaultEnricher.identity` is the shipped no-wire Layer — pass-through enrichment for the archetypes that select no `wire` — so every composition root wires the port and absence of an implementation is a selection, never a crash.
- Law: the attribute band is string-to-string data — identifier-grade context rides it per-occurrence; bounded dimensions for metrics derive from `class` and `blame` columns, never from band values.
- Growth: a new evidence field is one `FaultCapture` field; a second enrichment stage is a Layer composing the same Tag, never a second port.
- Boundary: which captures reach the enricher, redaction-at-capture, and OTLP egress encoding are `telemetry` policies; reconstruction internals are `wire`'s; this floor declares the shapes and the seam.
- Packages: `effect` (`Schema`, `Context`, `Effect`, `Layer`, `Record`); `kernel/schema/brand` (`Refined.Guid`).

```typescript
class FaultCapture extends Schema.Class<FaultCapture>("FaultCapture")({
  class: _Kind,
  tag: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  detail: Schema.String,
  correlation: Schema.optionalWith(Refined.Guid, { as: "Option" }),
  at: Schema.DateTimeUtcFromSelf,
  attributes: Schema.Record({ key: Schema.String, value: Schema.String }),
}) {
  get policy(): FaultClass.Row {
    return _rows[this.class]
  }
  enriched(added: { readonly [key: string]: string }): FaultCapture {
    return new FaultCapture({ ...this, attributes: Record.union(this.attributes, added, (_, next) => next) })
  }
}

class FaultEnricher extends Context.Tag("@rasm/ts/kernel/FaultEnricher")<FaultEnricher, {
  readonly enrich: (capture: FaultCapture) => Effect.Effect<FaultCapture>
}>() {
  static readonly identity: Layer.Layer<FaultEnricher> = Layer.succeed(FaultEnricher, { enrich: Effect.succeed })
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { FaultCapture, FaultClass, FaultEnricher }
```
