# [CORE_EVIDENCE]

`Evidence` owns decoded receipts, admitted progress state, and tenant-partitioned availability lattices.

## [01]-[INDEX]

- [02]-[RECEIPT_FAMILY]: the closed outcome union and its lifecycle rank vocabulary; `Receipt`.
- [03]-[ENVELOPE_OWNER]: the decoded envelope, its orders, the LWW instance, the fold plan; `ReceiptEnvelope`.
- [04]-[PROGRESS_FOLD]: the tally reading, the state product, read-time verdicts, the roll-up; `Tally`, `Progress`.
- [05]-[AVAILABILITY_LATTICE]: level rows, verdict family, worst-wins snapshot merge, the gate read; `Availability`.

## [02]-[RECEIPT_FAMILY]

[RECEIPT_FAMILY]:
- Growth: a new receipt kind is one tagged case, one `_RANKS` row, and zero envelope edits.

```typescript signature
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Array, Data, Duration, Equal, Equivalence, HashMap, HashSet, Match, Number, Option, Order, pipe, Record, Schema } from "effect"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Fault } from "../value/fault.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Causal } from "./causal.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Accepted = Schema.TaggedStruct("Accepted", {
})

const _Applied = Schema.TaggedStruct("Applied", {
  touched: Schema.HashSet(Digest.Key.content),
})

const _EvidenceValue = Schema.Union(Schema.String, Schema.Number.pipe(Schema.finite()), Schema.Boolean)
const _Evidence = Schema.HashMap({ key: Schema.NonEmptyString, value: _EvidenceValue })

// Retriability is DERIVED from the class column, never stored beside it: a producer writing its own bit could contradict
// the recovery band the class already decides, and the two would diverge silently the first time the row table moved.
const _Refused = Schema.TaggedStruct("Refused", {
  fault: Fault.Class.schema,
  evidence: _Evidence,
})

const _Superseded = Schema.TaggedStruct("Superseded", {
  by: Digest.Key.content,
})

const _Receipt: Schema.Union<[typeof _Accepted, typeof _Applied, typeof _Refused, typeof _Superseded]> = Schema.Union(
  _Accepted,
  _Applied,
  _Refused,
  _Superseded,
)
type _Receipt = Schema.Schema.Type<typeof _Receipt>

const _RANKS = {
  Accepted: { rank: 0, terminal: false },
  Applied: { rank: 1, terminal: true },
  Refused: { rank: 1, terminal: true },
  Superseded: { rank: 2, terminal: true },
} as const satisfies Record<_Receipt["_tag"], { readonly rank: number; readonly terminal: boolean }>

type _ReceiptKey = readonly [Identity.Tenant.Scope, Digest.Key<"content">]

const _basisCell = (basis: Option.Option<Causal.Vector>): Fold.Cell =>
  Option.match(basis, {
    onNone: () => Fold.cell(["basis:none"]),
    onSome: (vector) => Fold.cell([
      "basis:some",
      ...Array.sort(
        Array.map(HashMap.toEntries(vector.clocks), ([replica, count]) => Fold.cell([replica, count])),
        Order.string,
      ),
    ]),
  })

const _receiptCell = (receipt: _Receipt): Fold.Cell => Match.valueTags(receipt, {
  Accepted: () => Fold.cell(["Accepted"]),
  Applied: ({ touched }) => Fold.cell(["Applied", ...Array.sort(Array.fromIterable(touched), Order.string)]),
  Refused: ({ evidence, fault }) => Fold.cell([
    "Refused",
    fault,
    String(Fault.Class.retryable(fault)),
    ...Array.sort(Array.map(HashMap.toEntries(evidence), ([key, value]) =>
      Fold.cell([key, typeof value, typeof value === "boolean" ? String(value) : value])), Order.string),
  ]),
  Superseded: ({ by }) => Fold.cell(["Superseded", by]),
})
```

## [03]-[ENVELOPE_OWNER]

[ENVELOPE_OWNER]:

```typescript signature
class _ReceiptEnvelope extends Schema.Class<_ReceiptEnvelope>("Evidence.ReceiptEnvelope")({
  command: Digest.Key.content,
  subject: Schema.optionalWith(Digest.Key.content, { as: "Option" }),
  stamp: Clock.Hlc,
  tenant: Identity.Tenant,
  basis: Schema.optionalWith(Causal.Vector, { as: "Option" }),
  receipt: _Receipt,
}) {
  static readonly alike: Equivalence.Equivalence<_ReceiptEnvelope> = Schema.equivalence(_ReceiptEnvelope)
  static readonly byStamp: Order.Order<_ReceiptEnvelope> = Order.mapInput(
    Clock.Hlc.Order,
    (envelope: _ReceiptEnvelope) => envelope.stamp,
  )
  static readonly byLifecycle: Order.Order<_ReceiptEnvelope> = Order.combineAll([
    Order.mapInput(Order.number, (envelope: _ReceiptEnvelope) => _RANKS[envelope.receipt._tag].rank),
    _ReceiptEnvelope.byStamp,
    Order.mapInput(Order.string, (envelope: _ReceiptEnvelope) => envelope.receipt._tag),
    Order.mapInput(Order.string, (envelope: _ReceiptEnvelope) => Fold.cell([
      envelope.tenant.scope,
      envelope.command,
      Option.getOrElse(envelope.subject, () => "subject:none"),
      _basisCell(envelope.basis),
      _receiptCell(envelope.receipt),
    ])),
  ])
  static readonly latest: Merge.Instance<_ReceiptEnvelope> = Merge.max(_ReceiptEnvelope.byLifecycle)
  static readonly plan: Fold.Plan<_ReceiptEnvelope, _ReceiptKey, _ReceiptEnvelope> = {
    name: "state/receipt",
    key: (envelope) => Data.tuple(envelope.tenant.scope, envelope.command),
    cell: ([tenant, command]) => Fold.cell([tenant, command]),
    keyAlike: Equal.equivalence(),
    lift: (envelope) => envelope,
    merge: _ReceiptEnvelope.latest,
    identity: Option.none(),
  }
  get settled(): boolean {
    return _RANKS[this.receipt._tag].terminal
  }
  get outcome(): Option.Option<_Receipt> {
    return this.settled ? Option.some(this.receipt) : Option.none()
  }
}
```

## [04]-[PROGRESS_FOLD]

[PROGRESS_FOLD]:
- Law: `Tally` counts DONE units against a total for one operation in a parent tree, so it shares no axis with the producer phase frame `csharp:Rasm.Compute/Runtime/progress#PROGRESS_CELL` streams — that frame carries a phase vocabulary and a fraction, crosses as `ProgressUpdate`, and mirrors as `ProgressUpdateWire`. Two disjoint field sets under one spelling is what the separate names foreclose.
- Growth: a new progress verdict is one read member; a new mark axis (weight, priority) is one field plus one product row.

```typescript signature
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

## [05]-[AVAILABILITY_LATTICE]

[AVAILABILITY_LATTICE]:

```typescript signature
// The five rows and their ranks ARE the producer's frozen `DegradationLevelKey` roster, key for key and rank for
// rank; `admits` is this end's gate projection of the capability set each row retains — write capability retained
// admits every command, store-read alone admits reads, and the row retaining neither withholds.
const _LEVELS = ["full", "reduced-remote", "local-only", "read-only", "suspended"] as const
const _POSTURES = ["all", "reads", "none"] as const

const _ROWS = {
  "full": { rank: 0, admits: "all" },
  "reduced-remote": { rank: 1, admits: "all" },
  "local-only": { rank: 2, admits: "all" },
  "read-only": { rank: 3, admits: "reads" },
  "suspended": { rank: 4, admits: "none" },
} as const satisfies Record<(typeof _LEVELS)[number], { readonly rank: number; readonly admits: (typeof _POSTURES)[number] }>

const _Level = Shape.vocabulary(_LEVELS, _ROWS)
const _Command = Schema.NonEmptyString.pipe(Schema.brand("CommandName"))

const _byRank: Order.Order<(typeof _LEVELS)[number]> = Order.mapInput(Order.number, (level) => _Level.at(level).rank)

const _Available = Schema.TaggedStruct("Available", {})

const _Gated = Schema.TaggedStruct("Gated", {
  reason: Schema.NonEmptyString,
  until: Schema.optionalWith(Clock.Hlc, { as: "Option" }),
})

const _Withheld = Schema.TaggedStruct("Withheld", {
  level: _Level.schema,
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

const _Commands = Schema.transform(
  Shape.Record(_Command, _Verdict),
  Schema.HashMapFromSelf({ key: Schema.typeSchema(_Command), value: Schema.typeSchema(_Verdict) }),
  {
    strict: true,
    decode: (record) => HashMap.fromIterable(Record.toEntries(record)),
    encode: (map) => Record.fromEntries(HashMap.toEntries(map)),
  },
)

const _byRestrictiveness: Order.Order<Schema.Schema.Type<typeof _Verdict>> = Order.mapInput(
  Order.tuple(Order.number, Order.number, Order.bigint, Order.bigint, Order.number, Order.string),
  (verdict: Schema.Schema.Type<typeof _Verdict>) => {
    const until = verdict._tag === "Gated" ? verdict.until : Option.none<Clock.Hlc>()
    return [
      _VERDICT_RANKS[verdict._tag],
      Option.isNone(until) ? 1 : 0,
      Option.match(until, { onNone: () => 0n, onSome: (stamp) => stamp.physical }),
      Option.match(until, { onNone: () => 0n, onSome: (stamp) => stamp.logical }),
      verdict._tag === "Withheld" ? _Level.at(verdict.level).rank : 0,
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
  reads: (level) => _Gated.make({ reason: level, until: Option.none() }),
  none: (level) => _Withheld.make({ level, reason: level }),
} satisfies Record<(typeof _POSTURES)[number], (level: (typeof _LEVELS)[number]) => Schema.Schema.Type<typeof _Verdict>>)

class _Availability extends Schema.Class<_Availability>("Evidence.Availability")({
  level: _Level.schema,
  commands: _Commands,
  since: Clock.Hlc,
  tenant: Identity.Tenant,
}) {
  static readonly Verdict: typeof _Verdict = _Verdict
  static readonly worst: Merge.Instance<Availability.State> = _fieldwise
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
      () => _Posture.at(_Level.at(snapshot.posture.level).admits)(snapshot.posture.level),
    )
  }
}

declare namespace Availability {
  type State = {
    readonly posture: Posture
    readonly commands: HashMap.HashMap<Schema.Schema.Type<typeof _Command>, Schema.Schema.Type<typeof _Verdict>>
  }
  type Level = (typeof _LEVELS)[number]
  type Access = (typeof _POSTURES)[number]
  type Posture = { readonly level: Level; readonly since: Clock.Hlc }
  type Command = Schema.Schema.Type<typeof _Command>
  type Verdict = Schema.Schema.Type<typeof _Verdict>
}

type _ProgressKey = Progress.Key
type _ProgressState = Progress.State
type _AvailabilityLevel = Availability.Level
type _AvailabilityAccess = Availability.Access
type _AvailabilityPosture = Availability.Posture
type _AvailabilityCommand = Availability.Command
type _AvailabilityVerdict = Availability.Verdict
type _AvailabilityState = Availability.State

const Evidence = {
  Receipt: _Receipt,
  ReceiptEnvelope: _ReceiptEnvelope,
  Tally: _Tally,
  Progress: _Progress,
  Availability: _Availability,
} as const

namespace Evidence {
  export type Receipt = _Receipt
  export namespace Receipt {
    export type Kind = keyof typeof _RANKS
    export type Rank = (typeof _RANKS)[Kind]
  }
  export type ReceiptEnvelope = _ReceiptEnvelope
  export type ReceiptKey = _ReceiptKey
  export type Tally = _Tally
  export namespace Progress {
    export type Key = _ProgressKey
    export type State = _ProgressState
    export type Patience = _Patience
  }
  export type Availability = _Availability
  export namespace Availability {
    export type Level = _AvailabilityLevel
    export type Access = _AvailabilityAccess
    export type Posture = _AvailabilityPosture
    export type Command = _AvailabilityCommand
    export type Verdict = _AvailabilityVerdict
    export type State = _AvailabilityState
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Evidence }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
