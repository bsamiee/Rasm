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

```typescript
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

const _recoveryKinds = ["throttled", "transient", "terminal"] as const
const _recoveryRows = { throttled: {}, transient: {}, terminal: {} } as const
const _recoveries = Shape.vocabulary(_recoveryKinds, _recoveryRows)

const _reofferKinds = ["wait", "restart", "rescope"] as const
const _reofferRows = { wait: {}, restart: {}, rescope: {} } as const
const _reoffers = Shape.vocabulary(_reofferKinds, _reofferRows)

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

const _After = Schema.optionalWith(Schema.DurationFromSelf, { as: "Option" })
const _Stated = Schema.Struct({ after: Schema.OptionFromSelf(Schema.DurationFromSelf) })
const _isStated: (input: unknown) => input is typeof _Stated.Type = Schema.is(_Stated)
const _statedOf = (fault: unknown): Option.Option<Duration.Duration> => _isStated(fault) ? fault.after : Option.none()

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
  const payload = Schema.Union(
    ...Array.map(vocabulary.kinds, (reason) =>
      Schema.Struct({ reason: Schema.Literal(reason), ...vocabulary.at(reason).detail.fields })),
  ) as unknown as Schema.Schema<_Issue<Reasons, Rows>>
  const render = (issue: _Issue<Reasons, Rows>): string =>
    (vocabulary.at(issue.reason).render as (subject: typeof issue) => string)(issue)
  const dominance: Order.Order<_Issue<Reasons, Rows>> = Order.mapInput(
    _classes.order,
    (issue: _Issue<Reasons, Rows>) => vocabulary.at(issue.reason).class,
  )
  return Object.freeze({
    ...vocabulary,
    payload,
    render,
    classOf: <Reason extends Reasons[number]>(reason: Reason): Rows[Reason]["class"] => vocabulary.at(reason).class,
    legOf: <Reason extends Reasons[number]>(reason: Reason): Rows[Reason]["leg"] => vocabulary.at(reason).leg,
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
  retryable: (fault: unknown): boolean => _classes.at(_of(fault)).recovery !== "terminal",
  statedOf: _statedOf,
  dominant: _dominant,
} as const
```

## [02]-[CAPTURE_AND_ENRICHMENT]

`Fault.Capture` admits crash evidence and folds well-known OpenTelemetry attributes through one last-write-wins band monoid. `Fault.Enricher` is a total endomorphism with an identity Layer.

```typescript
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
  frame: Schema.optionalWith(_Frame, { as: "Option" }),
})

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
      { [_FORENSIC.errorType]: this.class },
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

```typescript
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

```typescript
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

```typescript
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
  from: (facts: ReadonlyArray<_DropFact>): _Census => _censusMonoid.combineAll(Array.map(facts, _counted)),
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Fault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
