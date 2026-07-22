# [CORE_FAULT]

One fault-policy owner serves the branch: `FaultClass` is the severity-ordered class vocabulary every rail inherits — the kind tuple is the single precedence declaration, rows carry the retryability, blame, and quarantine axes, and the dominance lattice ships as lawful `Order`/`Bounded`/`Semigroup` instances derived from tuple position — while `FaultCapture`/`FaultEnricher` carry crash evidence and its enrichment port, `Budget` compiles retry and deadline rows into gate-modal `Schedule` values, and `Degrade` compiles connection-silence policy into probe cadence.

Taxonomy, evidence, retry, and degradation remain clusters of one recovery-policy module, the temporal policies composing at rail and stream definition seams. Three fault altitudes stay distinct — interchange reconstruction, per-folder `Data.TaggedError` rails, outbound status mapping — and this floor imports none of them. Its module is `core/src/value/fault.ts`; a new fault class is one tuple entry with its row, a new budget one row, a new degradation posture one ladder rung.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                          | [PUBLIC]                        |
| :-----: | :------------------ | :-------------------------------------------------------------- | :------------------------------ |
|  [01]   | `CLASS_VOCABULARY`  | the ten-class table, classification fold, dominance lattice     | `FaultClass`                    |
|  [02]   | `ENRICHER_CONTRACT` | the capture evidence model and the enrichment port              | `FaultCapture`, `FaultEnricher` |
|  [03]   | `RETRY_BUDGET`      | the budget rows and their compiled gate-modal `Schedule` values | `Budget`                        |
|  [04]   | `DEGRADE_LADDER`    | the silence-threshold ladder and its parameterized level fold   | `Degrade`                       |

## [02]-[CLASS_VOCABULARY]

- Owner: `FaultClass`, the assembled vocabulary — the interior key tuple is the ONE severity declaration (severity ascends with position, a load-bearing sequence with no parallel restatement), the interior row table carries the behavior axes, the merged hub carries every derived type and the guard pair, and the exported owner assembles rows, `kinds`, `blames`, `schema`, the lattice instances, the fault-family declaration fold, and the operation members under a `typeof`-derived stated annotation.
- Law: the roster is sized by cross-language routing, never by cause — `absent`, `conflicted`, `invalid`, `malformed`, `denied`, `expired`, `exhausted`, `unavailable`, `breached`, `defect` — and a finer cause is a `reason` row inside the owning folder's fault class, never an eleventh entry minted for one surface.
- Law: severity is positional — `severity` (`Order.mapInput` over tuple position), `bounded` (the `Bounded` completing the lattice: `compare` with the tuple's own first and last entries), and `join` (`Semigroup.max(severity)`, the lawful join-semilattice a `Merge.struct` field row composes directly) all derive from the tuple, so a tuple reorder IS the severity edit and no rank literal exists to go stale, duplicate, or disagree with iteration order.
- Law: rows carry three axes — `retryable` (the transient family a budget gate re-drives), `blame` (`caller`/`system`, the accountability split the serving edge's `blame === "caller"` exposure fold and telemetry project), `quarantine` (the evidence-preserving divert: `malformed`/`invalid` continue as `Either.left` into a typed intake wherever the fault feeds a repair report, never a dropped element) — and behavior variation across the branch reads these columns, never a `switch` over class names.
- Law: blame is anchored like kind — `blames` is the value tuple, `Blame` derives from it, `blame` is the wire-facing literal schema, and `blameOf` projects the classified row's column — so an exhaustive per-blame fold, a blame-keyed dashboard row, and a blame value crossing a config row all read one anchor.
- Law: classification is total and idempotent — `of` answers identity on a bare kind literal (a consumer handing back an already-classified value gets its fixed point, never a `defect` fold), reads the structural `readonly class: FaultClass.Kind` convention off any value, folds a whole `Cause` tree to its dominant class through the position lattice, and folds all residue to `defect` — so an unclassified foreign throw lands at the correct terminal severity and `Schedule.whileInput(FaultClass.retryable)` gates correctly on faults, kinds, and causes alike.
- Law: `dominant` discriminates on input shape — a non-empty kind set folds to its representative through `severity` (the fold `Effect.validateAll`-shaped reports feed), and a `Cause` tree folds every failure and defect node through the same lattice to `Option` of the representative, `none` exactly when the tree carries no fault (interruption-only) — so parallel-failure dominance reads the lattice, never a squash ordering.
- Law: `schema` is the wire-facing literal union derived from the tuple spread — the non-empty overload keeps the exact literal tuple — so a class crossing a wire or a config row decodes against the same anchor the type plane derives from.
- Law: `family(reasons, rows)` closes a folder fault family once — frozen snapshots of the non-empty reason tuple and every exact-key family row derive the literal schema and `classOf` projection, so caller mutation cannot drift the published reasons, schema, rows, or classification and a tagged fault class carries no local `_Rows`/`_Closed` guard pair or repeated reason switch.
- Growth: a new class is one tuple entry with its row — every guard, schema, fold, budget gate, and the serving edge's governed `Record<FaultClass.Kind, _>` status record inherit it at compile time; a new axis is one `Row` field with its column on each row; a new folder fault family is one `family` call with its reason tuple and rows.
- Boundary: the class-to-status outbound mapping is the serving edge's governed record; the floor table stays transport-free.
- Packages: `effect` (`Schema`, `Order`, `Array`, `Cause`, `Chunk`, `Option`, `Predicate`); `@effect/typeclass` (`Bounded`, `Semigroup`).

```typescript signature
import * as Bounded from "@effect/typeclass/Bounded"
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as RecordInstances from "@effect/typeclass/data/Record"
import {
  ATTR_CODE_COLUMN_NUMBER, ATTR_CODE_FILE_PATH, ATTR_CODE_FUNCTION_NAME, ATTR_CODE_LINE_NUMBER,
  ATTR_ERROR_TYPE, ATTR_EXCEPTION_MESSAGE, ATTR_EXCEPTION_STACKTRACE, ATTR_EXCEPTION_TYPE, EVENT_EXCEPTION,
} from "@opentelemetry/semantic-conventions"
import {
  Array, Cause, Chunk, Context, Duration, Effect, Layer, Metric, Option, Order, Predicate, Record, Schedule, Schema, type Types,
} from "effect"
import { Refined } from "./schema.ts"

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
] as const // severity ascends with position: the tuple is the ONE precedence declaration — no rank literal exists to drift from it

const _blames = ["caller", "system"] as const

const _rows = {
  absent: { retryable: false, blame: "caller", quarantine: false },
  conflicted: { retryable: true, blame: "caller", quarantine: false },
  invalid: { retryable: false, blame: "caller", quarantine: true },
  malformed: { retryable: false, blame: "caller", quarantine: true },
  denied: { retryable: false, blame: "caller", quarantine: false },
  expired: { retryable: true, blame: "system", quarantine: false },
  exhausted: { retryable: true, blame: "system", quarantine: false },
  unavailable: { retryable: true, blame: "system", quarantine: false },
  breached: { retryable: false, blame: "system", quarantine: false },
  defect: { retryable: false, blame: "system", quarantine: false },
} as const

const _Kind = Schema.Literal(..._kinds)
const _Blame = Schema.Literal(..._blames)
const _is = Schema.is(_Kind)

const _severity: Order.Order<FaultClass.Kind> = Order.mapInput(Order.number, (kind: FaultClass.Kind) => _kinds.indexOf(kind))
const _bounded: Bounded.Bounded<FaultClass.Kind> = { compare: _severity, minBound: _kinds[0], maxBound: Array.lastNonEmpty(_kinds) }
const _join: Semigroup.Semigroup<FaultClass.Kind> = Semigroup.max(_severity)

const _probe = (fault: unknown): FaultClass.Kind =>
  _is(fault) ? fault : Predicate.hasProperty(fault, "class") && _is(fault.class) ? fault.class : "defect"

const _harvest = (cause: Cause.Cause<unknown>): Option.Option<FaultClass.Kind> =>
  Array.match(Chunk.toReadonlyArray(Chunk.map(Chunk.appendAll(Cause.failures(cause), Cause.defects(cause)), _probe)), {
    onEmpty: Option.none,
    onNonEmpty: (classes) => Option.some(Array.max(classes, _severity)),
  })

const _of = (fault: unknown): FaultClass.Kind =>
  Cause.isCause(fault) ? Option.getOrElse(_harvest(fault), () => "defect" as const) : _probe(fault)

function _dominant(classes: Array.NonEmptyReadonlyArray<FaultClass.Kind>): FaultClass.Kind
function _dominant(cause: Cause.Cause<unknown>): Option.Option<FaultClass.Kind>
function _dominant(
  input: Array.NonEmptyReadonlyArray<FaultClass.Kind> | Cause.Cause<unknown>,
): FaultClass.Kind | Option.Option<FaultClass.Kind> {
  return Cause.isCause(input) ? _harvest(input) : Array.max(input, _severity)
}

const _family = <
  const Reasons extends readonly [string, ...string[]],
  const Rows extends { readonly [Reason in Reasons[number]]: FaultClass.FamilyRow },
>(
  reasons: Reasons,
  rows: Rows & { readonly [Key in Exclude<keyof Rows, Reasons[number]>]: never },
) => {
  const heldReasons = Object.freeze(structuredClone(reasons))
  const heldRows = structuredClone(rows)
  Object.values(heldRows).forEach((row) => Object.freeze(row))
  Object.freeze(heldRows)
  return Object.freeze({
    reasons: heldReasons,
    rows: heldRows,
    schema: Schema.Literal(...heldReasons),
    classOf: (reason: Reasons[number]): FaultClass.Kind => heldRows[reason].class,
  })
}

declare namespace FaultClass {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Blames = typeof _blames
  type Blame = Blames[number]
  type Row = { readonly retryable: boolean; readonly blame: Blame; readonly quarantine: boolean }
  type FamilyRow = { readonly class: Kind }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly blames: Blames
      readonly schema: typeof _Kind
      readonly blame: typeof _Blame
      readonly severity: Order.Order<Kind>
      readonly bounded: Bounded.Bounded<Kind>
      readonly join: Semigroup.Semigroup<Kind>
      readonly family: typeof _family
      readonly is: (input: unknown) => input is Kind
      readonly of: (fault: unknown) => Kind
      readonly blameOf: (fault: unknown) => Blame
      readonly retryable: (fault: unknown) => boolean
      readonly dominant: {
        (classes: Array.NonEmptyReadonlyArray<Kind>): Kind
        (cause: Cause.Cause<unknown>): Option.Option<Kind>
      }
    }
  >
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const FaultClass: FaultClass.Shape = {
  ..._rows,
  kinds: _kinds,
  blames: _blames,
  schema: _Kind,
  blame: _Blame,
  severity: _severity,
  bounded: _bounded,
  join: _join,
  family: _family,
  is: _is,
  of: _of,
  blameOf: (fault) => _rows[_of(fault)].blame,
  retryable: (fault) => _rows[_of(fault)].retryable,
  dominant: _dominant,
}
```

## [03]-[ENRICHER_CONTRACT]

- Owner: `FaultCapture`, the floor-shaped crash-evidence model — class, fault tag, owning surface, detail, optional `Refined.Guid` correlation, capture instant, and an open OpenTelemetry scalar attribute band — the value the runtime crash owner constructs from a folded `Cause` and every enrichment round-trips; `policy` projects the class row so retryability, blame, and quarantine are recoverable from any capture, `enriched` is the successor constructor merging attribute bands, and `forensic` is the exception-evidence successor writing the well-known crash rows through the typed key vocabulary.
- Owner: `FaultEnricher`, the enrichment port — one `Context.Tag` whose service is a single endo-arrow `enrich: (capture) => Effect<FaultCapture>` — the interchange codec binds the Layer that reconstructs wire-grade forensics into the attribute band, the runtime crash owner yields the Tag, and the app root wires them; the interchange-owned reconstruction names never appear in this contract, keeping the adopted-verbatim vocabulary on its owning side.
- Law: forensic evidence has one declared admission and absence regime — `FaultCapture.Evidence` is a Schema-declared field block riding the owner as a static with its type on the merged hub, `stacktrace` and the `frame` block are `Option` admitted by `Schema.optionalWith(..., { as: "Option" })`, and `forensic` folds both through `Option.match` — so `undefined` never reaches the projection body, a loose structural companion type cannot drift beside the class authority, and evidence-field growth exerts compile pressure on the projection.
- Law: frame attribution rides the stable `code.*` quartet — `forensic` projects the `Option`-admitted `Evidence.frame` block into the function/file/line/column keys, absent where a minified or native stack yields no frame; `ATTR_CODE_STACKTRACE` stays unrowed because `ATTR_EXCEPTION_STACKTRACE` already carries the crash stack on this floor.
- Law: enrichment is total by signature — the error channel is `never`, so a failing enricher implementation resolves its own faults internally (degrade to the unenriched capture) and crash capture can never be broken by its own forensics.
- Law: `FaultEnricher.identity` is the shipped no-wire Layer — pass-through enrichment for the archetypes that select no interchange — so every composition root wires the port and absence of an implementation is a selection, never a crash.
- Law: the attribute band carries the OpenTelemetry scalar algebra — finite `number`, `string`, or `boolean` — so a boolean fact (a wire retryability column, a flag verdict) enters the band AS a boolean and enrichment never stringifies typed evidence; identifier-grade context rides it per occurrence, while bounded metric dimensions derive from `class` and `blame` columns. `exception.escaped` carries zero information on this floor — only escaping faults are captured — so the vocabulary omits the deprecated key.
- Law: the two type keys carry two truths — `forensic` writes the bounded class column into `ATTR_ERROR_TYPE` (the low-cardinality dimension backends group on) and the free-form runtime exception type into `ATTR_EXCEPTION_TYPE` — so the well-known pair never duplicates one unbounded string; `FaultCapture.Forensic` anchors the `ATTR_EXCEPTION_*`/`ATTR_ERROR_TYPE` vocabulary and `FaultCapture.event` anchors `EVENT_EXCEPTION`, so a misspelled forensic key is a compile error while the band stays open for context beyond the vocabulary, and `observe/convention` still owns attribute-space naming for its own projections.
- Law: band merging is one instance, never a per-site combine — `getSemigroupUnion(Semigroup.last())` declares the last-write-wins keyed merge once, `Monoid.fromSemigroup` names the empty band as its lawful identity, and `enriched` and `forensic` both project the one instance so enrichment stages fold with no emptiness guard.
- Law: `FaultCapture.aspect(metric, input)` is the definition-time signal weave — one transformer maps the capture into the metric's admitted update through `Metric.mapInput`, then composes `Effect.withSpan(EVENT_EXCEPTION)` and `Effect.withMetric` around the capture-producing effect, so crash owners instrument their declaration once and every invocation inherits the same span and metric policy without a call-site wrapper.
- Growth: a new evidence field is one `Evidence` schema field; a new well-known key is one `Forensic` row; a second enrichment stage is a Layer composing the same Tag, never a second port.
- Boundary: which captures reach the enricher, redaction-at-capture, stack-frame parsing, and OTLP egress encoding are the runtime telemetry owners' policies; reconstruction internals are the interchange codec's; this floor declares the shapes, the key vocabulary, and the seam.
- Packages: `effect` (`Schema`, `Context`, `Effect`, `Layer`, `Metric`, `Option`); `@effect/typeclass` (`Monoid`, `Semigroup`, `data/Record`); `@opentelemetry/semantic-conventions` (`ATTR_EXCEPTION_*`, `ATTR_ERROR_TYPE`, `ATTR_CODE_*`, `EVENT_EXCEPTION`); `schema#REFINED_FLOOR` (`Refined.Guid`).

```typescript signature
const _FORENSIC = {
  column: ATTR_CODE_COLUMN_NUMBER,
  errorType: ATTR_ERROR_TYPE,
  file: ATTR_CODE_FILE_PATH,
  function: ATTR_CODE_FUNCTION_NAME,
  line: ATTR_CODE_LINE_NUMBER,
  message: ATTR_EXCEPTION_MESSAGE,
  stacktrace: ATTR_EXCEPTION_STACKTRACE,
  type: ATTR_EXCEPTION_TYPE,
} as const

const _Attribute = Schema.Union(Schema.String, Schema.Number.pipe(Schema.finite()), Schema.Boolean)

const _Band: Monoid.Monoid<FaultCapture.Attributes> = Monoid.fromSemigroup(
  RecordInstances.getSemigroupUnion(Semigroup.last<FaultCapture.Attribute>()), // one keyed merge law: keys union, collisions last-write-wins
  {},                                                                           // the empty band is the lawful identity, named explicitly — last() alone admits none
)

const _Frame = Schema.Struct({
  function: Schema.NonEmptyString,
  file: Schema.NonEmptyString,
  line: Schema.Int.pipe(Schema.nonNegative()),
  column: Schema.Int.pipe(Schema.nonNegative()),
})

const _Evidence = Schema.Struct({
  type: Schema.NonEmptyString,
  message: Schema.String,
  stacktrace: Schema.optionalWith(Schema.String, { as: "Option" }),
  frame: Schema.optionalWith(_Frame, { as: "Option" }), // the parsed top frame: a minified or native stack lawfully yields none
})

class FaultCapture extends Schema.Class<FaultCapture>("FaultCapture")({
  class: _Kind,
  tag: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  detail: Schema.String,
  correlation: Schema.optionalWith(Refined.Guid, { as: "Option" }),
  at: Schema.DateTimeUtcFromSelf,
  attributes: Schema.Record({ key: Schema.String, value: _Attribute }),
}) {
  static readonly aspect = <Type, In, Out>(metric: Metric.Metric<Type, In, Out>, input: (capture: FaultCapture) => In) =>
    <E, R>(self: Effect.Effect<FaultCapture, E, R>) =>
      self.pipe(Effect.withSpan(EVENT_EXCEPTION), Effect.withMetric(Metric.mapInput(metric, input)))
  static readonly Evidence: typeof _Evidence = _Evidence
  static readonly Forensic: typeof _FORENSIC = _FORENSIC
  static readonly event: typeof EVENT_EXCEPTION = EVENT_EXCEPTION
  get policy(): FaultClass.Row {
    return _rows[this.class]
  }
  enriched(added: FaultCapture.Attributes): FaultCapture {
    return new FaultCapture({ ...this, attributes: _Band.combine(this.attributes, added) })
  }
  forensic(evidence: FaultCapture.Evidence): FaultCapture {
    return this.enriched({
      [_FORENSIC.errorType]: this.class, // the bounded dimension: the class column, never a second copy of the free-form type
      [_FORENSIC.message]: evidence.message,
      [_FORENSIC.type]: evidence.type,
      ...Option.match(evidence.stacktrace, {
        onNone: () => ({}),
        onSome: (stacktrace) => ({ [_FORENSIC.stacktrace]: stacktrace }),
      }),
      ...Option.match(evidence.frame, {
        onNone: () => ({}),
        onSome: (frame) => ({
          [_FORENSIC.column]: frame.column,
          [_FORENSIC.file]: frame.file,
          [_FORENSIC.function]: frame.function,
          [_FORENSIC.line]: frame.line,
        }),
      }),
    })
  }
}

declare namespace FaultCapture {
  type Attribute = typeof _Attribute.Type
  type Attributes = FaultCapture["attributes"]
  type Evidence = typeof _Evidence.Type
  type Forensic = (typeof _FORENSIC)[keyof typeof _FORENSIC]
}

class FaultEnricher extends Context.Tag("@rasm/ts/core/FaultEnricher")<FaultEnricher, {
  readonly enrich: (capture: FaultCapture) => Effect.Effect<FaultCapture>
}>() {
  static readonly identity: Layer.Layer<FaultEnricher> = Layer.succeed(FaultEnricher, { enrich: Effect.succeed })
}
```

## [04]-[RETRY_BUDGET]

- Owner: `Budget`, the assembled budget vocabulary — the interior key tuple anchors the roster, the row table carries every axis as `Duration` policy values, the merged hub carries derived types and the guard pair, and the exported owner assembles rows, `kinds`, and the `schedule` lookup under a `typeof`-derived stated annotation; the ingress decode ceilings are `schema#INGRESS_CEILING`'s `Ingress` — the two vocabularies never share a concept.
- Law: five rows ride the floor — `pulse` (interactive point ops: 40ms base, 4 attempts, 2s window), `lease` (infrastructure ops: 250ms base, 6 attempts, 20s window), `bulk` (batch work: 1s base, 8 attempts, 5m window), `feed` (long-lived reconnection: 500ms base, 64 attempts, 2m window, 90s reset), `once` (non-idempotent critical ops: zero attempts, deadline budgets only — the safe envelope for work that must never re-drive yet still names its per-try and whole-call deadlines) — floors a folder policy references by kind; a genuinely novel envelope is a new row, never a per-site literal.
- Law: every row carries the two deadline budgets the rails layering law consumes — `attempt` composes below the retry transformer (per-try), `total` above it (whole-call) — so the interchange invocation client and runtime work activities read `Budget[kind].attempt`/`.total` and the budget's whole geometry lives in one row.
- Law: compilation is fixed-form, total, and generative — `exponential(base, factor)` → `jittered` → `resetAfter(reset)` → `intersect(recurs(attempts))` → `upTo(window)` compiles once at module init through one `Record.map` over the row table under a governed mapped annotation, so a new row compiles the moment it lands and no second compiled-key roster exists to maintain; jitter is unconditional (a bare curve synchronizes a fleet into waves), `resetAfter` re-arms base delay after quiet so the next outage never inherits the last one's escalated tail, and the attempt/elapsed bounds stack because a budget names both.
- Law: the gate is a modality of the one compile — `schedule(kind, gate?)` composes `Schedule.whileInput(gate)` over the shared compiled base with `FaultClass.retryable` as the owner default, so only the transient family re-drives by default, the gate travels with the policy value, and a lane whose transience is already gated at another altitude (`HttpClient.retryTransient`) passes its own gate (`Function.constTrue`) instead of re-spelling the compile chain; a call-site predicate re-deriving retryability is still policy leakage.
- Law: the schedule input is `unknown` — one policy value serves every fault channel in the branch, and classification, not typing, decides re-drive.
- Growth: a new budget is one tuple entry with its row — the governed compile record inherits it at compile time; a new axis (a fleet-cost weight, a hedge delay) is one `Row` field consumed by the surfaces that name it.
- Boundary: which budget a surface selects is that folder's policy row; deadline transformers (`Effect.timeoutFail`) compose at the owning `Effect.fn` seam with the row's durations — the floor ships values, never wrappers.
- Packages: `effect` (`Duration`, `Schedule`, `Record`, `Predicate`).

```typescript signature
const _budgets = ["pulse", "lease", "bulk", "feed", "once"] as const
const _budgetRows = {
  pulse: {
    base: Duration.millis(40),
    factor: 2,
    attempts: 4,
    window: Duration.seconds(2),
    reset: Duration.seconds(30),
    attempt: Duration.seconds(1),
    total: Duration.seconds(8),
  },
  lease: {
    base: Duration.millis(250),
    factor: 2,
    attempts: 6,
    window: Duration.seconds(20),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(5),
    total: Duration.seconds(45),
  },
  bulk: {
    base: Duration.seconds(1),
    factor: 2,
    attempts: 8,
    window: Duration.minutes(5),
    reset: Duration.minutes(10),
    attempt: Duration.minutes(2),
    total: Duration.minutes(15),
  },
  feed: {
    base: Duration.millis(500),
    factor: 2,
    attempts: 64,
    window: Duration.minutes(2),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(10),
    total: Duration.minutes(30),
  },
  once: {
    base: Duration.zero,
    factor: 1,
    attempts: 0, // recurs(0): the compiled schedule never re-drives; the row exists for its deadline budgets
    window: Duration.zero,
    reset: Duration.zero,
    attempt: Duration.seconds(5),
    total: Duration.seconds(5),
  },
} as const

declare namespace Budget {
  type Kinds = typeof _budgets
  type Kind = keyof typeof _budgetRows
  type Row = {
    readonly base: Duration.Duration
    readonly factor: number
    readonly attempts: number
    readonly window: Duration.Duration
    readonly reset: Duration.Duration
    readonly attempt: Duration.Duration
    readonly total: Duration.Duration
  }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Gated = Schedule.Schedule<[Duration.Duration, number], unknown>
  type Shape = Types.Simplify<
    typeof _budgetRows & {
      readonly kinds: Kinds
      readonly schedule: (kind: Kind, gate?: Predicate.Predicate<unknown>) => Gated
    }
  >
  type _Rows<T extends Contract = typeof _budgetRows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _compile = (row: Budget.Row): Budget.Gated =>
  Schedule.exponential(row.base, row.factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(row.reset),
    Schedule.intersect(Schedule.recurs(row.attempts)),
    Schedule.upTo(row.window),
  )

const _compiled: { readonly [K in Budget.Kind]: Budget.Gated } = Record.map(_budgetRows, _compile) // generative: a new row compiles with zero named arms

const Budget: Budget.Shape = {
  ..._budgetRows,
  kinds: _budgets,
  schedule: (kind, gate = FaultClass.retryable) => _compiled[kind].pipe(Schedule.whileInput(gate)),
}
```

## [05]-[DEGRADE_LADDER]

- Owner: `Degrade`, the connection-degradation ladder — an interior level tuple anchoring the closed vocabulary, a row table carrying per-level entry threshold and probe cadence, and the exported owner assembling rows, `levels`, and the `level`/`cadence` folds under a stated annotation.
- Law: three rungs ride the ladder — `live` (healthy: zero threshold, 30s heartbeat cadence), `lagging` (10s of silence: 5s probe cadence), `severed` (2m of silence: 30s probe cadence) — and `level(silence, ladder?)` derives threshold order from the selected table before choosing the highest entered rung, a silence beneath every threshold folding to that table's least rung, so a caller-composed override remains data and never inherits a hidden module-tuple ordering constraint.
- Law: the fold is parameterized over its table — the module ladder is the shipped default row set, and a per-surface override is a caller-composed `Contract` handed to the same `level`/`cadence` shape, so threshold edits and rung additions flow through one generated order with no re-derived fold.
- Law: `cadence(silence, ladder?)` is the one-hop probe read — the rung's `Duration` policy value a consumer hands to `Schedule.spaced` or `Stream.repeatEffectWithSchedule` at its own seam; the ladder prices the probe, the surface owns the loop.
- Law: the ladder is a reconnection BUDGET, not evidence — event-log sync, live flag feeds, and presence streams fold their silence through it to pick probe cadence; the wire-decoded degradation-level evidence vocabulary is the `state` evidence family's sibling concern and the two never merge.
- Growth: a new rung is one tuple entry with its row; a per-surface ladder override is a caller-composed `Contract` folded through the same shape.
- Boundary: what counts as silence — missed heartbeats, an idle socket, a stalled pull — is the consuming surface's measurement; the ladder folds the span it is handed, stays class-free by design, and composes nothing from `[02]`.
- Packages: `effect` (`Duration`, `Option`, `Array`, `Order`).

```typescript signature
const _levels = ["live", "lagging", "severed"] as const
const _ladder = {
  live: { after: Duration.zero, cadence: Duration.seconds(30) },
  lagging: { after: Duration.seconds(10), cadence: Duration.seconds(5) },
  severed: { after: Duration.minutes(2), cadence: Duration.seconds(30) },
} as const

declare namespace Degrade {
  type Levels = typeof _levels
  type Kind = keyof typeof _ladder
  type Row = { readonly after: Duration.Duration; readonly cadence: Duration.Duration }
  type Contract = { readonly [K in Levels[number]]: Row }
  type Shape = Types.Simplify<
    typeof _ladder & {
      readonly levels: Levels
      readonly level: (silence: Duration.DurationInput, ladder?: Contract) => Kind
      readonly cadence: (silence: Duration.DurationInput, ladder?: Contract) => Duration.Duration
    }
  >
  type _Rows<T extends Contract = typeof _ladder> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _entry = (ladder: Degrade.Contract): Order.Order<Degrade.Kind> =>
  Order.mapInput(Duration.Order, (kind: Degrade.Kind) => ladder[kind].after)

const _leveled = (silence: Duration.DurationInput, ladder: Degrade.Contract = _ladder): Degrade.Kind =>
  Option.getOrElse(
    Array.findLast(Array.sort(_levels, _entry(ladder)), (kind) => Duration.greaterThanOrEqualTo(silence, ladder[kind].after)),
    () => Array.min(_levels, _entry(ladder)), // below every threshold: the selected table's least rung, never the module tuple's first entry
  )

const Degrade: Degrade.Shape = {
  ..._ladder,
  levels: _levels,
  level: _leveled,
  cadence: (silence, ladder = _ladder) => ladder[_leveled(silence, ladder)].cadence,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Budget, Degrade, FaultCapture, FaultClass, FaultEnricher }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
