# [CORE_FAULT]

`Fault` is the sole recovery-policy owner. Nested `Class`, `Capture`, `Enricher`, `Budget`, and `Degrade` share taxonomy without merging their distinct semantics. Module: `core/src/value/fault.ts`.

## [01]-[FAULT_OWNER]

- `Fault.Class` derives classification, severity, blame, quarantine, and family closure from exact ordered vocabularies.
- `Fault.Budget` compiles retry schedules once; `Fault.Degrade` compiles default and caller-owned silence ladders once.
- `Fault.Capture` carries schema-admitted evidence; `Fault.Enricher` is the total enrichment port.

```typescript signature
import * as Bounded from "@effect/typeclass/Bounded"
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
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

const _classRows = {
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
const _classes = Shape.vocabulary(_kinds, _classRows)

const _blameKinds = ["caller", "system"] as const
const _blameRows = { caller: {}, system: {} } as const
const _blames = Shape.vocabulary(_blameKinds, _blameRows)

type _FaultKind = (typeof _kinds)[number]
type _ClassRow = (typeof _classRows)[_FaultKind]
type _Blame = (typeof _blameKinds)[number]

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

const _family = <
  const Reasons extends readonly [string, ...string[]],
  const Rows extends { readonly [Reason in Reasons[number]]: { readonly class: _FaultKind } },
>(
  reasons: Reasons,
  rows: Shape.ExactRows<Reasons, Rows>,
) => {
  const vocabulary = Shape.vocabulary(reasons, rows)
  return Object.freeze({
    reasons: vocabulary.kinds,
    schema: vocabulary.schema,
    at: vocabulary.at,
    classOf: <Reason extends Reasons[number]>(reason: Reason): Rows[Reason]["class"] => vocabulary.at(reason).class,
  })
}

const _class = {
  ..._classes,
  blame: _blames,
  bounded: _bounded,
  join: _join,
  family: _family,
  of: _of,
  blameOf: (fault: unknown): _Blame => _classes.at(_of(fault)).blame,
  retryable: (fault: unknown): boolean => _classes.at(_of(fault)).retryable,
  dominant: _dominant,
} as const
```

## [02]-[CAPTURE_AND_ENRICHMENT]

`Fault.Capture` admits crash evidence and folds well-known OpenTelemetry attributes through one last-write-wins band monoid. `Fault.Enricher` is a total endomorphism with an identity Layer.

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

const _Attributes = Schema.Record({ key: Schema.String, value: _Attribute })
type _AttributeValue = typeof _Attribute.Type
type _AttributeBand = typeof _Attributes.Type

const _Band: Monoid.Monoid<_AttributeBand> = Monoid.fromSemigroup(
  RecordInstances.getSemigroupUnion(Semigroup.last<_AttributeValue>()),
  {},
)

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

class _Enricher extends Context.Tag("@rasm/ts/core/Fault.Enricher")<_Enricher, {
  readonly enrich: (capture: _Capture) => Effect.Effect<_Capture>
}>() {
  static readonly identity: Layer.Layer<_Enricher> = Layer.succeed(_Enricher, { enrich: Effect.succeed })
}
```

## [03]-[RETRY_BUDGET]

`Fault.Budget` keeps deadline geometry as private rows and compiles every schedule once at module evaluation; `at(kind)` retrieves deadline values and `schedule(kind, gate)` adds the input gate.

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
  schedule: (kind: _BudgetKind, gate: Predicate.Predicate<unknown> = _class.retryable): _BudgetSchedule =>
    _schedules[kind].pipe(Schedule.whileInput(gate)),
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

const Fault = {
  Class: _class,
  Capture: _Capture,
  Enricher: _Enricher,
  Budget: _budget,
  Degrade: _degrade,
} as const

declare namespace Fault {
  export namespace Class {
    export type Kind = _FaultKind
    export type Row = _ClassRow
    export type Blame = _Blame
  }
  export type Capture = _Capture
  export namespace Capture {
    export type Attribute = _AttributeValue
    export type Attributes = _AttributeBand
    export type Evidence = typeof _Evidence.Type
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
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Fault }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
