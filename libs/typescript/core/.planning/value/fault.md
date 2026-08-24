# [CORE_FAULT]

`Fault` is the sole recovery-policy owner. Nested `Class`, `Capture`, `Enricher`, `Budget`, `Degrade`, `Drop`, and `Ledger` share taxonomy without merging their distinct semantics. Module: `core/src/value/fault.ts`.

## [01]-[CLASS_VOCABULARY]

- `Fault.Class` derives severity, blame, quarantine, recovery band, re-offer route, and family closure from exact ordered vocabularies.
- `Fault.Class.family` closes one owner's reason roster over a per-reason leg and that reason's own subject shape.
- Each row renders its own detail, so no raise carries a free-string `detail` field and no class hand-writes a `message` template.
- `Fault.Class.family(…).census` mints the accumulating-admission carrier from that same roster.
- A family value IS its own vocabulary, so a word-counting instrument takes the family and no consumer restates the roster.
- `Fault.Class.spent` is the estate's one bound-exhaustion family — `Shape.Bound` units ARE its reasons, so no owner mints a private spent row.
- Re-declaring `{ issues, class, message }` at a folder forks one taxonomy into two.
- Law: one stated window rides THREE altitudes under three words — wire `retry_after`, class band `throttled`, value `after` — and none renames.
- Each altitude answers a different producer, lifetime, and reader, so one shared spelling claims a single authority where three answer.
- `Fault.Budget` compiles retry schedules once; `Fault.Degrade` compiles default and caller-owned silence ladders once.
- `Fault.Capture` carries schema-admitted evidence; `Fault.Enricher` is the total enrichment port.

```typescript signature
import * as Bounded from "@effect/typeclass/Bounded"
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as RecordInstances from "@effect/typeclass/data/Record"
import {
  ATTR_CODE_COLUMN_NUMBER, ATTR_CODE_FILE_PATH, ATTR_CODE_FUNCTION_NAME, ATTR_CODE_LINE_NUMBER,
  ATTR_ERROR_TYPE, ATTR_EXCEPTION_MESSAGE, ATTR_EXCEPTION_STACKTRACE, ATTR_EXCEPTION_TYPE, EVENT_EXCEPTION,
} from "@opentelemetry/semantic-conventions"
import { Array, Cause, Chunk, Context, Duration, Effect, Layer, Metric, Option, Order, Predicate, Record, Schedule, Schema } from "effect"
import { Shape } from "./schema.ts"

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

// Retriability is TWO axes and never one bit. `recovery` bands what a re-drive can reach — `terminal` (no schedule
// outlasts it), `transient` (a blind curve reaches it), `throttled` (a producer states the window and the raised VALUE
// carries it) — and `reoffer` names the ROUTE the caller takes to make the next offer: `wait` re-invokes identically
// under the budget, `restart` re-establishes the dependency handle first, `rescope` hands the caller a narrowed offer
// to re-author. Both vocabularies rank ascending — recovery by finality, reoffer by disruption — so a fold over a
// mixed set answers the most final band and the widest route rather than whichever member iteration reached last.
const _recoveryKinds = ["throttled", "transient", "terminal"] as const
const _recoveryRows = { throttled: {}, transient: {}, terminal: {} } as const
const _recoveries = Shape.vocabulary(_recoveryKinds, _recoveryRows)

const _reofferKinds = ["wait", "restart", "rescope"] as const
const _reofferRows = { wait: {}, restart: {}, rescope: {} } as const
const _reoffers = Shape.vocabulary(_reofferKinds, _reofferRows)

// Every column is elected from the kind's own semantics, never from a blanket blame read. `conflicted` and `expired`
// re-drive only after the handle is re-taken — a lost CAS re-reads before it re-folds and a lapsed lease re-mints
// before it re-presents — so both take `restart` rather than a blind `wait`. `exhausted` is the ONE band whose
// producers state their window (a quota refusal, a gate window, a peer's own wire arm), so it alone is
// `throttled`-capable and its raise carries `Fault.Class.After`. The caller-blamed terminal kinds take `rescope`
// because the material itself is what a lawful next offer must change; `breached` and `defect` take `restart` because
// a torn invariant and an ungraded failure are both re-offered only from a fresh handle, never from narrower input.
const _classRows = {
  absent: { recovery: "terminal", reoffer: "rescope", blame: "caller", quarantine: false },
  conflicted: { recovery: "transient", reoffer: "restart", blame: "caller", quarantine: false },
  invalid: { recovery: "terminal", reoffer: "rescope", blame: "caller", quarantine: true },
  malformed: { recovery: "terminal", reoffer: "rescope", blame: "caller", quarantine: true },
  denied: { recovery: "terminal", reoffer: "rescope", blame: "caller", quarantine: false },
  expired: { recovery: "transient", reoffer: "restart", blame: "system", quarantine: false },
  exhausted: { recovery: "throttled", reoffer: "wait", blame: "system", quarantine: false },
  unavailable: { recovery: "transient", reoffer: "wait", blame: "system", quarantine: false },
  breached: { recovery: "terminal", reoffer: "restart", blame: "system", quarantine: false },
  defect: { recovery: "terminal", reoffer: "restart", blame: "system", quarantine: false },
} as const
const _classes = Shape.vocabulary(_kinds, _classRows)

const _blameKinds = ["caller", "system"] as const
const _blameRows = { caller: {}, system: {} } as const
const _blames = Shape.vocabulary(_blameKinds, _blameRows)

type _FaultKind = (typeof _kinds)[number]
type _ClassRow = (typeof _classRows)[_FaultKind]
type _Blame = (typeof _blameKinds)[number]
type _Recovery = (typeof _recoveryKinds)[number]
type _Reoffer = (typeof _reofferKinds)[number]

const _bounded: Bounded.Bounded<_FaultKind> = {
  compare: _classes.order,
  minBound: _kinds[0],
  maxBound: Array.lastNonEmpty(_kinds),
}
const _join: Semigroup.Semigroup<_FaultKind> = Semigroup.max(_classes.order)

const _probe = (fault: unknown): _FaultKind =>
  _classes.is(fault)
    ? fault
    : Predicate.hasProperty(fault, "class") && _classes.is(fault.class)
      ? fault.class
      : "defect"

const _harvest = (cause: Cause.Cause<unknown>): Option.Option<_FaultKind> =>
  Array.match(Chunk.toReadonlyArray(Chunk.map(Chunk.appendAll(Cause.failures(cause), Cause.defects(cause)), _probe)), {
    onEmpty: Option.none,
    onNonEmpty: (classes) => Option.some(Array.max(classes, _classes.order)),
  })

const _of = (fault: unknown): _FaultKind =>
  Cause.isCause(fault) ? Option.getOrElse(_harvest(fault), () => "defect" as const) : _probe(fault)

function _dominant(classes: Array.NonEmptyReadonlyArray<_FaultKind>): _FaultKind
function _dominant(cause: Cause.Cause<unknown>): Option.Option<_FaultKind>
function _dominant(input: Array.NonEmptyReadonlyArray<_FaultKind> | Cause.Cause<unknown>): _FaultKind | Option.Option<_FaultKind> {
  return Cause.isCause(input) ? _harvest(input) : Array.max(input, _classes.order)
}

// `throttled` on a class row says only that this kind MAY carry a producer-stated window; the raise carries a
// measured value or nothing at all, so no policy row holds a slot only a mount can fill. `Fault.Budget.schedule`
// reads it back off the refusal; a raise that measured none stays `Option.none()` and spends the curve unchanged.
const _After = Schema.optionalWith(Schema.DurationFromSelf, { as: "Option" })
const _Stated = Schema.Struct({ after: Schema.OptionFromSelf(Schema.DurationFromSelf) })
const _isStated: (input: unknown) => input is typeof _Stated.Type = Schema.is(_Stated)
const _statedOf = (fault: unknown): Option.Option<Duration.Duration> => _isStated(fault) ? fault.after : Option.none()

// One row declares the whole reason: its class, the owning surface leg, the SUBJECT record a raise must supply, and
// its renderer over that record. `render` is exact against its own `detail` at the declaration — a row taking
// `{ origin, path }` cannot be rendered against `{ mesh }` — and the constraint below takes `never` so contravariance
// admits every exact row without an erased parameter. Free-string `detail` re-opens the axis `reason` already closed.
type _FamilyRow = {
  readonly class: _FaultKind
  readonly leg: string
  readonly detail: Schema.Struct<Schema.Struct.Fields>
  readonly render: (subject: never) => string
}

const _row = <const Leg extends string, const Fields extends Schema.Struct.Fields>(spec: {
  readonly class: _FaultKind
  readonly leg: Leg
  readonly detail: Schema.Struct<Fields>
  readonly render: (issue: Schema.Struct.Type<Fields> & { readonly reason: string }) => string
}): typeof spec => Object.freeze(spec)

type _Issue<
  Reasons extends readonly [string, ...string[]],
  Rows extends { readonly [Reason in Reasons[number]]: _FamilyRow },
> = {
  readonly [Reason in Reasons[number]]:
    & { readonly reason: Reason }
    & Schema.Struct.Type<Rows[Reason]["detail"]["fields"]>
}[Reasons[number]]

const _family = <
  const Reasons extends readonly [string, ...string[]],
  const Rows extends { readonly [Reason in Reasons[number]]: _FamilyRow },
>(
  reasons: Reasons,
  rows: Shape.ExactRows<Reasons, Rows>,
) => {
  const vocabulary = Shape.vocabulary(reasons, rows)
  // Variadic union carries per-arm precision the spread cannot infer, and the correlated renderer cannot be proved
  // reason-by-reason inside a generic fold, so this body pays both narrowings ONCE and every consumer reads the
  // exact published type — the same seat `Shape.vocabulary` gives its own row snapshot.
  const payload = Schema.Union(
    ...Array.map(vocabulary.kinds, (reason) =>
      Schema.Struct({ reason: Schema.Literal(reason), ...vocabulary.at(reason).detail.fields })),
  ) as unknown as Schema.Schema<_Issue<Reasons, Rows>>
  const render = (issue: _Issue<Reasons, Rows>): string =>
    (vocabulary.at(issue.reason).render as (subject: typeof issue) => string)(issue)
  // Rank lattice lifts from classes to ISSUES, so a census recovers the dominant issue WHOLE — its row, its leg, and
  // its class all follow one election rather than the bare kind `Fault.Class.dominant` answers over a class array.
  const dominance: Order.Order<_Issue<Reasons, Rows>> = Order.mapInput(
    _classes.order,
    (issue: _Issue<Reasons, Rows>) => vocabulary.at(issue.reason).class,
  )
  // The family IS its roster: spreading the snapshot publishes `kinds`, `order`, and `is` beside `schema` and `at`,
  // which is exactly the shape a word-counting instrument's census parameter takes, so a consumer hands the family
  // itself to the aspect rather than restating the tuple as a second static nothing keeps aligned with this mint.
  return Object.freeze({
    ...vocabulary,
    payload,
    render,
    classOf: <Reason extends Reasons[number]>(reason: Reason): Rows[Reason]["class"] => vocabulary.at(reason).class,
    legOf: <Reason extends Reasons[number]>(reason: Reason): Rows[Reason]["leg"] => vocabulary.at(reason).leg,
    // Rows admitted INDEPENDENTLY census every offender in one refusal. Carrier is total at this owner — its only
    // fields are the issues — so the caller supplies the tag and everything else derives: class and leg elect on the
    // rank lattice and the message joins each issue through its OWN row renderer, which is what keeps a family
    // whose arms carry different subjects (one naming an expected/actual pair, another a bare detail) renderable
    // from one declaration. Tag doubles as the message prefix because `catchTag` already discriminates on that word.
    census: <const Tag extends string>(tag: Tag) => {
      class Census extends Schema.TaggedError<Census>()(tag, {
        issues: Schema.NonEmptyArray(payload),
      }) {
        get dominant(): _Issue<Reasons, Rows> {
          return Array.max(this.issues, dominance)
        }
        get class(): _FaultKind {
          return vocabulary.at(this.dominant.reason).class
        }
        get leg(): string {
          return vocabulary.at(this.dominant.reason).leg
        }
        override get message(): string {
          return `<${tag}:refused> ${Array.join(Array.map(this.issues, render), "; ")}`
        }
      }
      return Census
    },
  })
}

// Bound exhaustion is ONE family estate-wide, never a per-owner reason. `Shape.Bound` (`value/schema`) already closes
// the unit roster and mints the evidence row, so the UNITS ARE THE REASONS and every spent budget refuses through
// this mint. `unit` is OMITTED from the subject because `reason` already carries it — spreading the produced row
// whole would let a `fuel` reason ride a `hops` unit, exactly the invalid state a discriminant exists to foreclose.
// Every unit refuses identically today, so the roster folds one row rather than transcribing three; a unit that
// later earns its own class or renderer replaces its fold entry with one declaration and no consumer moves.
const _spentRow = _row({
  class: "breached",
  leg: "bound",
  detail: Shape.Bound.Spent.omit("unit"),
  render: (spent) => `<spent:${spent.reason}@${spent.ceiling}> reached ${spent.reached}`,
})
const _spentRows = Record.fromEntries(
  Array.map(Shape.Bound.kinds, (unit) => [unit, _spentRow] as const),
) as { readonly [Unit in Shape.BoundUnit]: typeof _spentRow }
const _spent = _family(Shape.Bound.kinds, _spentRows)

const _class = {
  ..._classes,
  blame: _blames,
  recovery: _recoveries,
  reoffer: _reoffers,
  bounded: _bounded,
  join: _join,
  After: _After,
  row: _row,
  family: _family,
  spent: _spent,
  of: _of,
  blameOf: (fault: unknown): _Blame => _classes.at(_of(fault)).blame,
  recoveryOf: (fault: unknown): _Recovery => _classes.at(_of(fault)).recovery,
  reofferOf: (fault: unknown): _Reoffer => _classes.at(_of(fault)).reoffer,
  // DERIVED, never a stored bit: the column is gone from the row table and this projection is the whole survivor, so
  // a consumer wanting the band reads `recoveryOf` and a consumer wanting only a gate keeps one boolean.
  retryable: (fault: unknown): boolean => _classes.at(_of(fault)).recovery !== "terminal",
  statedOf: _statedOf,
  dominant: _dominant,
} as const
```

## [02]-[CAPTURE_AND_ENRICHMENT]

`Fault.Capture` admits crash evidence and folds well-known OpenTelemetry attributes through one last-write-wins band monoid. `Fault.Enricher` is a total endomorphism with an identity Layer.

```typescript signature
const _Attribute = Schema.Union(Schema.String, Schema.Number.pipe(Schema.finite()), Schema.Boolean)

const _Frame = Schema.Struct({
  column: Schema.Int.pipe(Schema.nonNegative()),
  file: Schema.NonEmptyString,
  function: Schema.NonEmptyString,
  line: Schema.Int.pipe(Schema.nonNegative()),
})

const _Evidence = Schema.Struct({
  message: Schema.String,
  stacktrace: Schema.optionalWith(Schema.String, { as: "Option" }),
  type: Schema.NonEmptyString,
  frame: Schema.optionalWith(_Frame, { as: "Option" }), // the parsed top frame: a minified or native stack lawfully yields none
})

// ONE correspondence, proven key-exact against the field records that declare the columns: each map is closed by
// `satisfies` over its own struct's keys, so a column added to `_Evidence` or `_Frame` lands unmapped at the compiler
// rather than unstamped at the exporter, and a semconv constant with no declared column cannot be spelled at all.
// `errorType` is the single stamped dimension no evidence field carries — it reads the capture's own class column.
type _Stamped<Fields> = { readonly [Key in keyof Fields]: string }
const _EVIDENCE_ATTRIBUTE = {
  message: ATTR_EXCEPTION_MESSAGE,
  stacktrace: ATTR_EXCEPTION_STACKTRACE,
  type: ATTR_EXCEPTION_TYPE,
} as const satisfies _Stamped<Omit<typeof _Evidence.fields, "frame">>
const _FRAME_ATTRIBUTE = {
  column: ATTR_CODE_COLUMN_NUMBER,
  file: ATTR_CODE_FILE_PATH,
  function: ATTR_CODE_FUNCTION_NAME,
  line: ATTR_CODE_LINE_NUMBER,
} as const satisfies _Stamped<typeof _Frame.fields>
const _FORENSIC = { ..._EVIDENCE_ATTRIBUTE, ..._FRAME_ATTRIBUTE, errorType: ATTR_ERROR_TYPE } as const

const _Attributes = Schema.Record({ key: Schema.String, value: _Attribute })
type _AttributeValue = typeof _Attribute.Type
type _AttributeBand = typeof _Attributes.Type

const _Band: Monoid.Monoid<_AttributeBand> = Monoid.fromSemigroup(
  RecordInstances.getSemigroupUnion(Semigroup.last<_AttributeValue>()),
  {},
)

const _isAttribute: (input: unknown) => input is _AttributeValue = Schema.is(_Attribute)

// One fold serves both column tables: an `Option`-carried column contributes only when the producer filled it, and a
// column carrying a shape no attribute band admits drops rather than encoding an object into a telemetry value.
const _carried = (value: unknown): Option.Option<_AttributeValue> =>
  Option.flatMap(Option.isOption(value) ? value : Option.some(value), (held) =>
    _isAttribute(held) ? Option.some(held) : Option.none())

const _stamped = (attributes: Record<string, string>, held: Record<string, unknown>): _AttributeBand =>
  Record.fromEntries(Array.filterMap(
    Record.toEntries(attributes),
    ([key, attribute]) => Option.map(_carried(held[key]), (value) => [attribute, value] as const),
  ))

class _Capture extends Schema.Class<_Capture>("Fault.Capture")({
  class: _classes.schema,
  tag: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  detail: Schema.String,
  correlation: Schema.optionalWith(Shape.Refined.Guid, { as: "Option" }),
  at: Schema.DateTimeUtcFromSelf,
  attributes: _Attributes,
}) {
  static readonly aspect = <Type, In, Out>(metric: Metric.Metric<Type, In, Out>, input: (capture: _Capture) => In) =>
    <E, R>(self: Effect.Effect<_Capture, E, R>) =>
      self.pipe(Effect.withSpan(EVENT_EXCEPTION), Effect.withMetric(Metric.mapInput(metric, input)))
  static readonly Evidence: typeof _Evidence = _Evidence
  static readonly Forensic: typeof _FORENSIC = _FORENSIC
  static readonly event: typeof EVENT_EXCEPTION = EVENT_EXCEPTION
  get policy(): _ClassRow {
    return _classes.at(this.class)
  }
  enriched(added: _AttributeBand): _Capture {
    return new _Capture({ ...this, attributes: _Band.combine(this.attributes, added) })
  }
  forensic(evidence: typeof _Evidence.Type): _Capture {
    return this.enriched(_Band.combineAll([
      { [_FORENSIC.errorType]: this.class }, // the bounded dimension: the class column, never a second copy of the free-form type
      _stamped(_EVIDENCE_ATTRIBUTE, evidence),
      Option.match(evidence.frame, {
        onNone: (): _AttributeBand => ({}),
        onSome: (frame) => _stamped(_FRAME_ATTRIBUTE, frame),
      }),
    ]))
  }
}

class _Enricher extends Context.Tag("@rasm/core/Fault.Enricher")<_Enricher, {
  readonly enrich: (capture: _Capture) => Effect.Effect<_Capture>
}>() {
  static readonly identity: Layer.Layer<_Enricher> = Layer.succeed(_Enricher, { enrich: Effect.succeed })
}
```

## [03]-[RETRY_BUDGET]

`Fault.Budget` keeps deadline geometry as private rows and compiles every blind schedule once at module evaluation.

`at(kind)` retrieves deadline values; `schedule(kind, gate, stated)` gates the input and honors a `throttled` refusal's own window.

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
    attempts: 0,
    window: Duration.zero,
    reset: Duration.zero,
    attempt: Duration.seconds(5),
    total: Duration.seconds(5),
  },
} as const
const _budgetVocabulary = Shape.vocabulary(_budgets, _budgetRows)
type _BudgetKind = (typeof _budgets)[number]
type _BudgetRow = (typeof _budgetRows)[_BudgetKind]
type _BudgetSchedule = Schedule.Schedule<[Duration.Duration, number], unknown>

const _compileBudget = (row: _BudgetRow): _BudgetSchedule =>
  Schedule.exponential(row.base, row.factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(row.reset),
    Schedule.intersect(Schedule.recurs(row.attempts)),
    Schedule.upTo(row.window),
  )

const _schedules: { readonly [Kind in _BudgetKind]: _BudgetSchedule } = Record.map(_budgetRows, _compileBudget)

const _budget = {
  ..._budgetVocabulary,
  // Band selects and the VALUE times: a producer-stated window re-seats the row's `base` so the next step waits
  // exactly what the refusal named, and the curve grows from there under the row's own factor, attempts, window, and
  // reset — those bounds still terminate the loop and no hand sleep sits beside the policy value. Blind curves
  // compile once at module evaluation; only a stated re-offer pays a compile, and it pays it once per refusal.
  schedule: (
    kind: _BudgetKind,
    gate: Predicate.Predicate<unknown> = _class.retryable,
    stated: Option.Option<Duration.Duration> = Option.none(),
  ): _BudgetSchedule =>
    Option.match(stated, {
      onNone: () => _schedules[kind],
      onSome: (delay) => _compileBudget({ ..._budgetVocabulary.at(kind), base: delay }),
    }).pipe(Schedule.whileInput(gate)),
} as const
```

## [04]-[DEGRADE_LADDER]

`Fault.Degrade` compiles threshold order once. A caller with different thresholds invokes `compile(rows)` once and reuses the returned `level`/`cadence` policy.

```typescript signature
const _levels = ["live", "lagging", "severed"] as const
const _ladder = {
  live: { after: Duration.zero, cadence: Duration.seconds(30) },
  lagging: { after: Duration.seconds(10), cadence: Duration.seconds(5) },
  severed: { after: Duration.minutes(2), cadence: Duration.seconds(30) },
} as const
type _DegradeKind = (typeof _levels)[number]
type _DegradeRow = { readonly after: Duration.Duration; readonly cadence: Duration.Duration }
type _DegradeRows = { readonly [Kind in _DegradeKind]: _DegradeRow }
type _DegradeVocabulary = Shape.Vocabulary<typeof _levels, _DegradeRows>

const _compileDegrade = (vocabulary: _DegradeVocabulary) => {
  const entry = Order.mapInput(Duration.Order, (kind: _DegradeKind) => vocabulary.at(kind).after)
  const ordered = Array.sort(vocabulary.kinds, entry)
  const level = (silence: Duration.DurationInput): _DegradeKind => Option.getOrElse(
    Array.findLast(ordered, (kind) => Duration.greaterThanOrEqualTo(silence, vocabulary.at(kind).after)),
    () => Array.min(ordered, entry),
  )
  return {
    ...vocabulary,
    level,
    cadence: (silence: Duration.DurationInput): Duration.Duration => vocabulary.at(level(silence)).cadence,
  } as const
}

const _degradeVocabulary: _DegradeVocabulary = Shape.vocabulary(_levels, _ladder)
const _degradePolicy = _compileDegrade(_degradeVocabulary)
const _degrade = {
  ..._degradePolicy,
  compile: (rows: Shape.ExactRows<typeof _levels, _DegradeRows>) =>
    _compileDegrade(Shape.vocabulary(_levels, rows)),
} as const

```

## [05]-[DROP_LEDGER]

`Fault.Drop` closes the estate's bounded-loss vocabulary; `Fault.Ledger` folds those facts into one per-reason census.

- A drop is a MEASURED loss, never a refusal — the fact rides the same band as the survivors, so one stream carries both.
- A silent `filter`, `take`, or empty return fuses "the peer sent nothing" with "this fold discarded what it sent".
- The census DERIVES from occurrences; a tally kept beside the band is the shape no arrival can reconcile against.
- Law: every bounded loss names the subject it dropped and the extent it dropped, so an operator reads damage, not silence.

```typescript signature
// `class` grades what the loss WAS — lawful duplication, a spent bound, unadmitted material — so a reader banding
// drops by severity spends the same lattice every refusal does. The roster is CLOSED estate-wide, so a future silent
// drop has no row to hide in and a new loss kind lands as one row every consumer already renders.
const _dropKinds = ["coalesced", "replayed", "truncated", "oversize", "unanchored", "foreign", "unparsed"] as const
type _DropReason = (typeof _dropKinds)[number]
const _dropRows = {
  coalesced: { class: "conflicted", lost: "restated readings collapsed onto the held one" },
  replayed: { class: "conflicted", lost: "redelivered coordinates the delivery contract already settled" },
  truncated: { class: "exhausted", lost: "entries past the count ceiling" },
  oversize: { class: "exhausted", lost: "bytes past the encoded-size budget" },
  unanchored: { class: "absent", lost: "entries whose anchor this fold could not recover" },
  foreign: { class: "denied", lost: "entries under a key no roster names" },
  unparsed: { class: "malformed", lost: "entries no grammar admits" },
} as const satisfies { readonly [Reason in _DropReason]: { readonly class: _FaultKind; readonly lost: string } }

// One occurrence shape serves every reason, so the roster folds one row rather than transcribing the whole set; a reason that
// later earns its own subject replaces its fold entry with one declaration and no consumer moves. `extent` is the
// MEASURE the reason's own sentence names — entries for a ceiling, bytes for a budget, readings for a coalesce — so
// no second column exists to disagree with the word.
const _Occurrence = Schema.Struct({
  key: Schema.NonEmptyString,
  extent: Schema.Int.pipe(Schema.positive()),
})
const _dropFamily = _family(
  _dropKinds,
  Record.map(_dropRows, (row) =>
    _row({
      class: row.class,
      leg: "drop",
      detail: _Occurrence,
      render: ({ extent, key, reason }) => `<drop:${reason}@${key}> ${extent} ${row.lost}`,
    })),
)
type _DropFact = Schema.Schema.Type<typeof _dropFamily.payload>
const _fact = (reason: _DropReason, key: string, extent: number): _DropFact => ({ reason, key, extent })
const _drop = { ..._dropFamily, fact: _fact } as const

// `Semigroup.struct` sums each reason's own cell and `Monoid.fromSemigroup` seats the identity, so a stream fold and
// an array fold reach the same value through one instance and no page spells a hand `concat` beside it. Zero here is
// the monoid IDENTITY over a folded band rather than a stand-in for an unmeasured tally: a reason reading `count: 0`
// states that THIS fold observed no such drop, which is exactly the reading a silent filter could never publish.
const _Cell = Schema.Struct({
  count: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.nonNegative()),
})
type _Census = { readonly [Reason in _DropReason]: typeof _Cell.Type }
const _cell: Semigroup.Semigroup<typeof _Cell.Type> = Semigroup.struct({
  count: NumberInstances.SemigroupSum,
  extent: NumberInstances.SemigroupSum,
})
const _quiet: _Census = Record.map(_dropRows, () => ({ count: 0, extent: 0 }))
const _censusMonoid: Monoid.Monoid<_Census> = Monoid.fromSemigroup(
  Semigroup.struct(Record.map(_dropRows, () => _cell)),
  _quiet,
)
const _counted = (fact: _DropFact): _Census =>
  Record.map(_quiet, (cell, reason) => reason === fact.reason ? { count: 1, extent: fact.extent } : cell)

const _ledger = {
  Cell: _Cell,
  monoid: _censusMonoid,
  of: _counted,
  // Both producer shapes fold through the SAME instance: a stream runs the monoid step, a settled roster runs this.
  from: (facts: ReadonlyArray<_DropFact>): _Census => _censusMonoid.combineAll(Array.map(facts, _counted)),
  // Consumers gate on EVIDENCE rather than on a length, so a clean census and an unread one cannot read alike.
  quiet: (census: _Census): boolean => Array.every(_dropKinds, (reason) => census[reason].count === 0),
} as const

const Fault = {
  Class: _class,
  Capture: _Capture,
  Enricher: _Enricher,
  Budget: _budget,
  Degrade: _degrade,
  Drop: _drop,
  Ledger: _ledger,
} as const

declare namespace Fault {
  export namespace Class {
    export type Kind = _FaultKind
    export type Row = _ClassRow
    export type Blame = _Blame
    export type Recovery = _Recovery
    export type Reoffer = _Reoffer
    export type Stated = typeof _After.Type
    export type Family<
      Reasons extends readonly [string, ...string[]],
      Rows extends { readonly [Reason in Reasons[number]]: _FamilyRow },
    > = ReturnType<typeof _family<Reasons, Rows>>
    export type Issue<
      Reasons extends readonly [string, ...string[]],
      Rows extends { readonly [Reason in Reasons[number]]: _FamilyRow },
    > = _Issue<Reasons, Rows>
  }
  export type Capture = _Capture
  export namespace Capture {
    export type Attribute = _AttributeValue
    export type Attributes = _AttributeBand
    export type Evidence = typeof _Evidence.Type
    export type Frame = typeof _Frame.Type
    export type Forensic = (typeof _FORENSIC)[keyof typeof _FORENSIC]
  }
  export type Enricher = _Enricher
  export namespace Budget {
    export type Kind = _BudgetKind
    export type Row = _BudgetRow
    export type Gated = _BudgetSchedule
  }
  export namespace Degrade {
    export type Kind = _DegradeKind
    export type Row = _DegradeRow
    export type Rows = _DegradeRows
  }
  export namespace Drop {
    export type Fact = _DropFact
    export type Reason = _DropReason
  }
  export namespace Ledger {
    export type Cell = typeof _Cell.Type
    export type Census = _Census
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Fault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
