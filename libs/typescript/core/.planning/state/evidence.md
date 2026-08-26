# [CORE_EVIDENCE]

`Evidence` owns the command lifecycle outcome, admitted progress state, and tenant-partitioned availability levels.

## [01]-[INDEX]

- [02]-[OUTCOME]: closed command lifecycle union and its settlement read; `Outcome`.
- [03]-[PROGRESS_FOLD]: tally reading, state product, read-time verdicts, roll-up; `Tally`, `Progress`.
- [04]-[AVAILABILITY_LEVELS]: enum-keyed level rows, verdict family, the `CommandAvailability` crossing, worst-wins merge, the gate read; `Availability`.

## [02]-[OUTCOME]

- Law: a command's outcome crosses runtimes as CloudEvent `data` — correlation, tenant, content key, and stamp ride the attributes `interchange/carrier#EVENT_ENVELOPE` mints and reads, so no column here repeats one.
- Law: `Accepted` is the one unsettled case and `settled` reads the lifecycle row, so no consumer enumerates the terminal arms.
- Growth: a new lifecycle case is one tagged case and one `_LIFECYCLE` row.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { create, enumToJson, isMessage, type MessageShape, type UnknownEnum } from "@bufbuild/protobuf"
import { EmptySchema, timestampFromMs, timestampMs, TimestampSchema } from "@bufbuild/protobuf/wkt"
import * as availability from "@rasm\/contracts/rasm/contracts/availability/availability_pb"
import * as control from "@rasm\/contracts/rasm/contracts/compute/control_pb"
import { Array, Data, DateTime, Duration, Effect, Either, Equal, Equivalence, HashMap, HashSet, Match, Number, Option, Order, ParseResult, pipe, Record, Schema, type SchemaAST } from "effect"
import { Wire } from "../interchange/codec.ts"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Fault } from "../value/fault.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Accepted = Schema.TaggedStruct("Accepted", {})
const _Applied = Schema.TaggedStruct("Applied", { touched: Schema.HashSet(Digest.Key.content) })
const _Refused = Schema.TaggedStruct("Refused", {
  fault: Fault.Class.schema,
  evidence: Schema.HashMap({
    key: Schema.NonEmptyString,
    value: Schema.Union(Schema.String, Schema.Number.pipe(Schema.finite()), Schema.Boolean),
  }),
})
const _Superseded = Schema.TaggedStruct("Superseded", { by: Digest.Key.content })

const _Outcome: Schema.Union<[typeof _Accepted, typeof _Applied, typeof _Refused, typeof _Superseded]> = Schema.Union(
  _Accepted,
  _Applied,
  _Refused,
  _Superseded,
)
type _Outcome = Schema.Schema.Type<typeof _Outcome>
type _OutcomeKind = _Outcome["_tag"]

const _LIFECYCLE = {
  Accepted: { terminal: false },
  Applied: { terminal: true },
  Refused: { terminal: true },
  Superseded: { terminal: true },
} as const satisfies Record<_OutcomeKind, { readonly terminal: boolean }>

const _OutcomeOwner: { readonly schema: typeof _Outcome; readonly settled: (outcome: _Outcome) => boolean } = {
  schema: _Outcome,
  settled: (outcome) => _LIFECYCLE[outcome._tag].terminal,
}
```

## [03]-[PROGRESS_FOLD]

[PROGRESS_FOLD]:
- Law: `Tally` counts DONE units against a total for one operation in a parent tree, so it shares no axis with the producer phase frame `dotnet:Rasm.Compute/Runtime/progress#PROGRESS_CELL` streams — that frame carries a phase vocabulary and a fraction, crosses as `ProgressUpdate`, and mirrors as `ProgressUpdateWire`. Two disjoint field sets under one spelling is what the separate names foreclose.
- Growth: a new progress verdict is one read member; a new mark axis (weight, priority) is one field plus one product row.

```typescript
class _Tally extends Schema.Class<_Tally>("Evidence.Tally")(
  Schema.Struct({
    operation: Digest.Key.content,
    parent: Schema.optionalWith(Digest.Key.content, { as: "Option" }),
    stage: Schema.NonEmptyString,
    done: Schema.Int.pipe(Schema.nonNegative()),
    total: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: "Option" }),
    stamp: Clock.Hlc,
    tenant: Identity.Tenant,
  }).pipe(Schema.filter((mark) => Option.forall(mark.total, (total) => mark.done <= total))),
) {
  static readonly transition: Equivalence.Equivalence<_Tally> = Schema.equivalence(
    Schema.Struct(_Tally.fields).pipe(Schema.pick("operation", "parent", "stage", "done", "total")),
  )
  static readonly byStamp: Order.Order<_Tally> = Order.mapInput(
    Clock.Hlc.Order,
    (mark: _Tally) => mark.stamp,
  )
}

const _Patience = Schema.DurationFromSelf.pipe(
  Schema.filter((duration) => Duration.isFinite(duration) && Duration.greaterThan(duration, Duration.zero)),
)
type _Patience = Schema.Schema.Type<typeof _Patience>

declare namespace Progress {
  type Key = readonly [Identity.Tenant.Scope, Digest.Key<"content">]
  type Head = { readonly stage: string; readonly stamp: Clock.Hlc }
  type Parent = { readonly value: Option.Option<Digest.Key<"content">>; readonly stamp: Clock.Hlc }
  type State = _ProgressState
  type Shape = {
    readonly Patience: typeof _Patience
    readonly State: typeof _ProgressState
    readonly state: Merge.Instance<State>
    readonly plan: Fold.Plan<_Tally, Key, State>
    readonly fraction: (state: State) => Option.Option<number>
    readonly stalled: (state: State, horizon: Clock.Hlc, patience: _Patience) => boolean
    readonly rollup: (table: Fold.Table<Key, State>, root: Key) => Option.Option<number>
  }
}

class _ProgressState extends Schema.Class<_ProgressState>("Evidence.Progress.State")(
  Schema.Struct({
    head: Schema.Struct({ stage: Schema.NonEmptyString, stamp: Clock.Hlc }),
    done: Schema.Int.pipe(Schema.nonNegative()),
    total: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: "Option" }),
    parent: Schema.Struct({
      value: Schema.optionalWith(Digest.Key.content, { as: "Option" }),
      stamp: Clock.Hlc,
    }),
    first: Clock.Hlc,
    last: Clock.Hlc,
  }).pipe(Schema.filter((state) => Option.forall(state.total, (total) => state.done <= total))),
) {}

const _byHeadStamp: Order.Order<Progress.Head> = Order.combine(
  Order.mapInput(Clock.Hlc.Order, (head: Progress.Head) => head.stamp),
  Order.mapInput(Order.string, (head: Progress.Head) => head.stage),
)
const _byParentStamp: Order.Order<Progress.Parent> = Order.combine(
  Order.mapInput(Clock.Hlc.Order, (parent: Progress.Parent) => parent.stamp),
  Order.mapInput(Order.string, (parent: Progress.Parent) => Option.getOrElse(parent.value, () => "")),
)

const _progressFields = Merge.struct({
  head: Merge.max(_byHeadStamp),
  done: Merge.max(Order.number),
  total: Merge.optional(Merge.max(Order.number)),
  parent: Merge.max(_byParentStamp),
  first: Merge.min(Clock.Hlc.Order),
  last: Merge.max(Clock.Hlc.Order),
})

const _state: Merge.Instance<Progress.State> = {
  combine: Semigroup.make((self, that) => {
    const merged = _progressFields.combine.combine(self, that)
    return new _ProgressState({
      ...merged,
      total: Option.filter(merged.total, (total) => merged.done <= total),
    })
  }),
  law: "semilattice",
  alike: Schema.equivalence(_ProgressState),
  empty: Option.none(),
}

const _lifted = (mark: _Tally): Progress.State => new _ProgressState({
  head: { stage: mark.stage, stamp: mark.stamp },
  done: mark.done,
  total: mark.total,
  parent: { value: mark.parent, stamp: mark.stamp },
  first: mark.stamp,
  last: mark.stamp,
})

const _children = (
  table: Fold.Table<Progress.Key, Progress.State>,
): HashMap.HashMap<Progress.Key, ReadonlyArray<Progress.Key>> =>
  HashMap.reduce(table, HashMap.empty<Progress.Key, ReadonlyArray<Progress.Key>>(), (acc, state, key) =>
    Option.match(state.parent.value, {
      onNone: () => acc,
      onSome: (parent) => {
        const parentKey = Data.tuple(key[0], parent)
        return HashMap.modifyAt(acc, parentKey, (slot) =>
          Option.some(Option.match(slot, {
            onNone: (): ReadonlyArray<Progress.Key> => [key],
            onSome: (kids) => Array.append(kids, key),
          })))
      },
    }))

const _weights = (
  table: Fold.Table<Progress.Key, Progress.State>,
  children: HashMap.HashMap<Progress.Key, ReadonlyArray<Progress.Key>>,
  root: Progress.Key,
  seen: HashSet.HashSet<Progress.Key>,
): Option.Option<readonly [done: number, total: number]> =>
  HashSet.has(seen, root) || !HashMap.has(table, root)
    ? Option.none()
    : pipe(
        HashMap.get(children, root),
        Option.filter(Array.isNonEmptyReadonlyArray),
        Option.match({
          onNone: () => Option.flatMap(HashMap.get(table, root), (state) =>
            Option.map(state.total, (total) => [state.done, total] as const)),
          onSome: (descendants) =>
            Array.reduce(
              descendants,
              Option.some<readonly [number, number]>([0, 0]),
              (acc, child) => Option.flatMap(acc, ([done, total]) =>
                Option.flatMap(_weights(table, children, child, HashSet.add(seen, root)), ([childDone, childTotal]) => {
                  const next = [done + childDone, total + childTotal] as const
                  return globalThis.Number.isSafeInteger(next[0]) && globalThis.Number.isSafeInteger(next[1])
                    ? Option.some(next)
                    : Option.none()
                })),
            ),
        }),
      )

const _Progress: Progress.Shape = {
  Patience: _Patience,
  State: _ProgressState,
  state: _state,
  plan: {
    name: "state/progress",
    key: (mark) => Data.tuple(mark.tenant.scope, mark.operation),
    cell: ([tenant, operation]) => Fold.cell([tenant, operation]),
    keyAlike: Equal.equivalence(),
    lift: _lifted,
    merge: _state,
    identity: Option.none(),
  },
  fraction: (state) => Option.flatMap(state.total, (total) => Number.divide(state.done, total)),
  stalled: (state, horizon, patience) => horizon.physical - state.last.physical > Clock.Hlc.delta(patience),
  rollup: (table, root) =>
    Option.flatMap(_weights(table, _children(table), root, HashSet.empty()), ([done, total]) => Number.divide(done, total)),
}
```

## [04]-[AVAILABILITY_LEVELS]

[AVAILABILITY_LEVELS]:

```typescript
type _Level = Exclude<control.DegradationLevel, UnknownEnum | typeof control.DegradationLevel.UNSPECIFIED>
const _LEVELS = [
  control.DegradationLevel.FULL,
  control.DegradationLevel.REDUCED_REMOTE,
  control.DegradationLevel.LOCAL_ONLY,
  control.DegradationLevel.READ_ONLY,
  control.DegradationLevel.SUSPENDED,
] as const
const _POSTURES = ["all", "reads", "none"] as const

const _ROWS = {
  [control.DegradationLevel.FULL]: { rank: 0, admits: "all" },
  [control.DegradationLevel.REDUCED_REMOTE]: { rank: 1, admits: "all" },
  [control.DegradationLevel.LOCAL_ONLY]: { rank: 2, admits: "all" },
  [control.DegradationLevel.READ_ONLY]: { rank: 3, admits: "reads" },
  [control.DegradationLevel.SUSPENDED]: { rank: 4, admits: "none" },
} as const satisfies Record<_Level, { readonly rank: number; readonly admits: (typeof _POSTURES)[number] }>
type _Rows<T extends Record<(typeof _LEVELS)[number], unknown> = typeof _ROWS> = T

const _LevelSchema = Schema.Literal(..._LEVELS)
const _levelOf = Schema.is(_LevelSchema)
const _word = (level: _Level): string => enumToJson(control.DegradationLevelSchema, level)
const _Command = Schema.NonEmptyString.pipe(Schema.brand("CommandName"))

const _byRank: Order.Order<_Level> = Order.mapInput(Order.number, (level) => _ROWS[level].rank)

const _Available = Schema.TaggedStruct("Available", {})

const _Gated = Schema.TaggedStruct("Gated", {
  reason: Schema.NonEmptyString,
  until: Schema.optionalWith(Clock.Hlc, { as: "Option" }),
})

const _Withheld = Schema.TaggedStruct("Withheld", {
  level: _LevelSchema,
  reason: Schema.NonEmptyString,
})

const _Verdict: Schema.Union<[typeof _Available, typeof _Gated, typeof _Withheld]> = Schema.Union(
  _Available,
  _Gated,
  _Withheld,
)

const _VERDICT_RANKS = { Available: 0, Gated: 1, Withheld: 2 } as const satisfies Record<
  Schema.Schema.Type<typeof _Verdict>["_tag"],
  number
>

const _Commands = Schema.HashMapFromSelf({ key: Schema.typeSchema(_Command), value: Schema.typeSchema(_Verdict) })

const _byRestrictiveness: Order.Order<Schema.Schema.Type<typeof _Verdict>> = Order.mapInput(
  Order.tuple(Order.number, Order.number, Order.bigint, Order.bigint, Order.number, Order.string),
  (verdict: Schema.Schema.Type<typeof _Verdict>) => {
    const until = verdict._tag === "Gated" ? verdict.until : Option.none<Clock.Hlc>()
    return [
      _VERDICT_RANKS[verdict._tag],
      Option.isNone(until) ? 1 : 0,
      Option.match(until, { onNone: () => 0n, onSome: (stamp) => stamp.physical }),
      Option.match(until, { onNone: () => 0n, onSome: (stamp) => stamp.logical }),
      verdict._tag === "Withheld" ? _ROWS[verdict.level].rank : 0,
      verdict._tag === "Available" ? "" : verdict.reason,
    ] as const
  },
)

const _worstVerdict: Merge.Instance<Schema.Schema.Type<typeof _Verdict>> = Merge.max(_byRestrictiveness)

const _fieldwise: Merge.Instance<{
  readonly posture: Availability.Posture
  readonly commands: HashMap.HashMap<Schema.Schema.Type<typeof _Command>, Schema.Schema.Type<typeof _Verdict>>
}> = Merge.struct({
  posture: Merge.max(Order.combine(
    Order.mapInput(_byRank, (posture: Availability.Posture) => posture.level),
    Order.mapInput(Clock.Hlc.Order, (posture: Availability.Posture) => posture.since),
  )),
  commands: Merge.hashMap(_worstVerdict),
})

const _Posture = Shape.vocabulary(_POSTURES, {
  all: () => _Available.make({}),
  reads: (level) => _Gated.make({ reason: _word(level), until: Option.none() }),
  none: (level) => _Withheld.make({ level, reason: _word(level) }),
} satisfies Record<(typeof _POSTURES)[number], (level: _Level) => Schema.Schema.Type<typeof _Verdict>>)

class _Reading extends Schema.Class<_Reading>("Evidence.Availability.Reading")({
  level: _LevelSchema,
  commands: _Commands,
  since: Clock.Hlc,
}) {}

const _Wire: Schema.Schema<MessageShape<typeof availability.CommandAvailabilitySchema>> = Schema.declare(
  (input: unknown): input is MessageShape<typeof availability.CommandAvailabilitySchema> =>
    isMessage(input, availability.CommandAvailabilitySchema),
  { identifier: availability.CommandAvailabilitySchema.typeName },
)

const _stampOf = (stamp: MessageShape<typeof TimestampSchema>): Clock.Hlc =>
  new Clock.Hlc({
    physical: Clock.Hlc.physicalOf(DateTime.unsafeMake(timestampMs(stamp))),
    logical: Clock.Hlc.genesis.logical,
  })
const _TICKS_PER_MILLI = 10_000n

const _verdictOf = (
  wire: availability.CommandVerdictWire,
  ast: SchemaAST.AST,
): Either.Either<Schema.Schema.Type<typeof _Verdict>, ParseResult.ParseIssue> =>
  Match.value(wire.verdict).pipe(
    Match.when({ case: "available" }, () => Either.right(_Available.make({}))),
    Match.when({ case: "gated" }, ({ value }) => Either.right(_Gated.make({ reason: value.reason, until: Option.none() }))),
    Match.when({ case: "withheld" }, ({ value }) =>
      _levelOf(value.level)
        ? Either.right(_Withheld.make({ level: value.level, reason: value.reason }))
        : Either.left(new ParseResult.Type(ast, value, "<level-undefined>"))),
    Match.orElse(() => Either.left(new ParseResult.Type(ast, wire, "<verdict-unset>"))),
  )

const _verdictWire = (verdict: Schema.Schema.Type<typeof _Verdict>): availability.CommandVerdictWire =>
  create(availability.CommandVerdictWireSchema, {
    verdict: Match.valueTags(verdict, {
      Available: () => ({ case: "available" as const, value: create(EmptySchema) }),
      Gated: ({ reason }) => ({ case: "gated" as const, value: create(availability.CommandVerdictWire_GatedSchema, { reason }) }),
      Withheld: ({ level, reason }) => ({
        case: "withheld" as const,
        value: create(availability.CommandVerdictWire_WithheldSchema, { level, reason }),
      }),
    }),
  })

const _FromWire: Schema.Schema<_Reading, MessageShape<typeof availability.CommandAvailabilitySchema>> = Schema.transformOrFail(
  _Wire,
  _Reading,
  {
    strict: true,
    decode: (wire, _options, ast) =>
      Either.map(
        Either.all({
          level: _levelOf(wire.level) ? Either.right(wire.level) : Either.left(new ParseResult.Type(ast, wire, "<level-undefined>")),
          since: Option.match(Option.fromNullable(wire.since), {
            onNone: () => Either.left(new ParseResult.Type(ast, wire, "<since-unset>")),
            onSome: (stamp) => Either.right(_stampOf(stamp)),
          }),
          commands: Either.all(Array.map(Record.toEntries(wire.commands), ([name, verdict]) =>
            Either.all([
              Either.mapLeft(Schema.decodeEither(_Command)(name), (error) => error.issue),
              _verdictOf(verdict, ast),
            ]))),
        }),
        ({ level, since, commands }) => new _Reading({ level, since, commands: HashMap.fromIterable(commands) }),
      ),
    encode: (reading) =>
      Either.right(create(availability.CommandAvailabilitySchema, {
        level: reading.level,
        commands: Record.fromEntries(Array.map(HashMap.toEntries(reading.commands), ([name, verdict]) => [name, _verdictWire(verdict)] as const)),
        since: timestampFromMs(Number(reading.since.physical / _TICKS_PER_MILLI)),
      })),
  },
)

class _Availability extends Schema.Class<_Availability>("Evidence.Availability")({
  ..._Reading.fields,
  tenant: Identity.Tenant,
}) {
  static readonly Reading: typeof _Reading = _Reading
  static readonly Verdict: typeof _Verdict = _Verdict
  static readonly levels: typeof _LEVELS = _LEVELS
  static readonly worst: Merge.Instance<Availability.State> = _fieldwise
  static readonly of = (
    octets: Uint8Array,
    tenant: Identity.Tenant,
  ): Effect.Effect<_Availability, ParseResult.ParseError | Wire.Fault> =>
    Effect.flatMap(Wire.decode("CommandAvailability", octets), (wire) =>
      Effect.map(Schema.decode(_FromWire)(wire), (reading) =>
        new _Availability({ level: reading.level, commands: reading.commands, since: reading.since, tenant })))
  static readonly plan: Fold.Plan<_Availability, Identity.Tenant.Scope, Availability.State> = {
    name: "state/availability",
    key: (snapshot) => snapshot.tenant.scope,
    cell: (tenant) => Fold.cell([tenant]),
    keyAlike: Equivalence.string,
    lift: (snapshot) => ({ posture: { level: snapshot.level, since: snapshot.since }, commands: snapshot.commands }),
    merge: _Availability.worst,
    identity: Option.none(),
  }
  static admits(snapshot: Availability.State, command: Availability.Command): Availability.Verdict {
    return Option.getOrElse(
      HashMap.get(snapshot.commands, command),
      () => _Posture.at(_ROWS[snapshot.posture.level].admits)(snapshot.posture.level),
    )
  }
}

declare namespace Availability {
  type State = {
    readonly posture: Posture
    readonly commands: HashMap.HashMap<Schema.Schema.Type<typeof _Command>, Schema.Schema.Type<typeof _Verdict>>
  }
  type Reading = _Reading
  type Level = _Level
  type Access = (typeof _POSTURES)[number]
  type Posture = { readonly level: Level; readonly since: Clock.Hlc }
  type Command = Schema.Schema.Type<typeof _Command>
  type Verdict = Schema.Schema.Type<typeof _Verdict>
}

type _ProgressKey = Progress.Key
type _ProgressState = Progress.State
type _AvailabilityReading = Availability.Reading
type _AvailabilityLevel = Availability.Level
type _AvailabilityAccess = Availability.Access
type _AvailabilityPosture = Availability.Posture
type _AvailabilityCommand = Availability.Command
type _AvailabilityVerdict = Availability.Verdict
type _AvailabilityState = Availability.State

const Evidence = {
  Outcome: _OutcomeOwner,
  Tally: _Tally,
  Progress: _Progress,
  Availability: _Availability,
} as const

namespace Evidence {
  export type Outcome = _Outcome
  export namespace Outcome {
    export type Kind = _OutcomeKind
  }
  export type Tally = _Tally
  export namespace Progress {
    export type Key = _ProgressKey
    export type State = _ProgressState
    export type Patience = _Patience
  }
  export type Availability = _Availability
  export namespace Availability {
    export type Reading = _AvailabilityReading
    export type Level = _AvailabilityLevel
    export type Access = _AvailabilityAccess
    export type Posture = _AvailabilityPosture
    export type Command = _AvailabilityCommand
    export type Verdict = _AvailabilityVerdict
    export type State = _AvailabilityState
  }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Evidence }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
