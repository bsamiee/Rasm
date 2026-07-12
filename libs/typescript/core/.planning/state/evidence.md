# [CORE_EVIDENCE]

The decoded evidence vocabularies as one bounded family: `Receipt`/`ReceiptEnvelope` — the closed command-outcome union the C# AppHost mints, carried with its stamp, tenant, and causal basis; `ProgressMark`/`Progress` — the per-operation progress evidence the C# Compute runtime emits, folded into monotone state with horizon-parameterized verdicts; `Availability` — the degradation-level and per-command verdict snapshot the serving gate types against, merged worst-wins as a lattice. Every shape decodes through the interchange codec INTO this module, composes `Hlc`, `TenantContext`, `ContentKey`, `FaultClass`, and `Vector` from their owners — never re-mints — and every fold is a `fold#PLAN_CONTRACT` plan row on a `merge#INSTANCE_CONTRACT` instance, so evidence runs identically as a pure snapshot, a stream trace, a live handle, or a durable projection. The C# typed families never collapse into erased TS shapes: every kind is its own union member with kind-specific evidence fields, and evidence is fields policy reads, never message strings. The module is `core/src/state/evidence.ts`; a new receipt kind, progress axis, or availability level is one union member or row with every exhaustive consumer breaking loudly until its arm exists.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]              | [OWNS]                                                               | [PUBLIC]                   |
| :-----: | :--------------------- | :------------------------------------------------------------------- | :------------------------- |
|  [01]   | `RECEIPT_FAMILY`       | the closed outcome union and its lifecycle rank vocabulary           | `Receipt`                  |
|  [02]   | `ENVELOPE_OWNER`       | the decoded envelope, its orders, the LWW instance, the fold plan    | `ReceiptEnvelope`          |
|  [03]   | `PROGRESS_FOLD`        | the decoded mark, the state product, read-time verdicts, the roll-up | `ProgressMark`, `Progress` |
|  [04]   | `AVAILABILITY_LATTICE` | level rows, verdict family, worst-wins snapshot merge, the gate read | `Availability`             |

## [02]-[RECEIPT_FAMILY]

[RECEIPT_FAMILY]:
- Owner: `Receipt` — one `Schema.Union` over four tagged case owners: `Accepted` (admitted, awaiting application), `Applied` (carries the resulting causal `Vector` basis and the touched `ContentKey` set), `Refused` (carries the fault classification and retryability as data), `Superseded` (carries the superseding command's key) — the `_tag` is simultaneously the wire discriminant and the dispatch key.
- Law: evidence is fields, never message strings — `Refused` carries `fault: FaultClass.schema` plus `retryable` exactly as the wire mints them, so gateway retry policy reads data; a receipt kind whose evidence lives in prose is the erased-family defect.
- Law: `_RANKS` is the lifecycle lattice — `Accepted` below the three terminal kinds — an interior vocabulary row table contract-checked at the expression seam: `as const satisfies Record<Receipt["_tag"], …>` closes the table against the union both ways, so a union member without a rank row or a rank row without a member fails at the declaration, lifecycle comparison derives from one anchor, and a new kind is one union member plus one rank row.
- Boundary: the member roster mirrors the C# AppHost runtime-port family one-to-one at the vocabulary level; roster parity pins at the interchange decode seam, and a C#-side kind lands here as a union member the same release.
- Growth: a new receipt kind is one tagged case, one `_RANKS` row, and zero envelope edits.
- Packages: `@effect/typeclass` (`Semigroup`); `effect` (`Schema`, `Array`, `Duration`, `Equivalence`, `HashMap`, `HashSet`, `Number`, `Option`, `Order`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../value/contentKey.ts` (`ContentKey`); `../value/fault.ts` (`FaultClass`); `./causal.ts` (`Vector`); `./merge.ts` (`Merge`); `./fold.ts` (`Fold`).

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Array, type Duration, Equivalence, HashMap, HashSet, Number, Option, Order, pipe, Record, Schema } from "effect"
import { Hlc } from "../value/clock.ts"
import { ContentKey } from "../value/contentKey.ts"
import { FaultClass } from "../value/fault.ts"
import { TenantContext } from "../value/identity.ts"
import { Vector } from "./causal.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Accepted = Schema.TaggedStruct("Accepted", {
  at: Hlc,
})

const _Applied = Schema.TaggedStruct("Applied", {
  at: Hlc,
  basis: Vector,
  touched: Schema.Array(ContentKey),
})

const _Refused = Schema.TaggedStruct("Refused", {
  at: Hlc,
  fault: FaultClass.schema,
  retryable: Schema.Boolean,
  detail: Schema.NonEmptyString,
})

const _Superseded = Schema.TaggedStruct("Superseded", {
  at: Hlc,
  by: ContentKey,
})

const Receipt: Schema.Union<[typeof _Accepted, typeof _Applied, typeof _Refused, typeof _Superseded]> = Schema.Union(
  _Accepted,
  _Applied,
  _Refused,
  _Superseded,
)
type Receipt = typeof Receipt.Type

const _RANKS = {
  Accepted: { rank: 0, terminal: false },
  Applied: { rank: 1, terminal: true },
  Refused: { rank: 1, terminal: true },
  Superseded: { rank: 2, terminal: true },
} as const satisfies Record<Receipt["_tag"], { readonly rank: number; readonly terminal: boolean }>

declare namespace Receipt {
  type Kind = keyof typeof _RANKS
  type Rank = (typeof _RANKS)[Kind]
}
```

## [03]-[ENVELOPE_OWNER]

[ENVELOPE_OWNER]:
- Owner: `ReceiptEnvelope` — the one decoded evidence owner: `command` (the content-keyed command coordinate — commands are content-addressed), `subject` (the optional target entity key), `stamp`/`tenant` (composed vocabulary), `basis` (the optional `Vector` the receipt observed), `receipt` (the family) — with orders, the merge instance, and the fold plan riding it as statics so one import carries shape, policy, and fold.
- Law: `ReceiptEnvelope.latest` is `Merge.max` over the lifecycle-then-stamp-then-tag order — LWW at its correct altitude: rank decides (a terminal receipt outranks `Accepted` regardless of clock skew), stamp tie-breaks within a rank, the receipt tag closes the order over distinct kinds at one stamp, and the composed `Order` is the single policy edit-site; a residual tie is a structural duplicate idempotence absorbs, because the AppHost mint is HLC-monotone per authority.
- Law: `ReceiptEnvelope.plan` keys by `command` — the per-command receipt table is one plan row, so it runs identically as a pure snapshot, a stream trace, a live handle, or a durable projection; `settled` reads terminality from the `_RANKS` row, never a `_tag` ladder.
- Law: dedup is structural — two decodes of one wire receipt compare equal under the derived equivalence, so idempotent delivery through the engine's `consolidate` costs nothing here.
- Law: correlating receipts with their produced artifacts at live altitude is `fold#DATAFLOW_VERBS`'s `joined` handle over the command key — never a hand walk over two folded tables.
- Boundary: decode placement and the wire twin are the interchange codec's; `feed#ENTRY_FAMILY` wraps envelopes into feed entries; shells consume the folded table through `fold#MEMORY_LANE` views.

```typescript
class ReceiptEnvelope extends Schema.Class<ReceiptEnvelope>("ReceiptEnvelope")({
  command: ContentKey,
  subject: Schema.optionalWith(ContentKey, { as: "Option" }),
  stamp: Hlc,
  tenant: TenantContext,
  basis: Schema.optionalWith(Vector, { as: "Option" }),
  receipt: Receipt,
}) {
  static readonly byStamp: Order.Order<ReceiptEnvelope> = Order.mapInput(
    Hlc.Order,
    (envelope: ReceiptEnvelope) => envelope.stamp,
  )
  static readonly byLifecycle: Order.Order<ReceiptEnvelope> = Order.combineAll([
    Order.mapInput(Order.number, (envelope: ReceiptEnvelope) => _RANKS[envelope.receipt._tag].rank),
    ReceiptEnvelope.byStamp,
    Order.mapInput(Order.string, (envelope: ReceiptEnvelope) => envelope.receipt._tag),
  ])
  static readonly latest: Merge.Instance<ReceiptEnvelope> = Merge.max(ReceiptEnvelope.byLifecycle)
  static readonly plan: Fold.Plan<ReceiptEnvelope, ContentKey, ReceiptEnvelope> = Fold.plan({
    name: "state/receipt",
    key: (envelope) => envelope.command,
    lift: (envelope) => envelope,
    merge: ReceiptEnvelope.latest,
  })
  get settled(): boolean {
    return _RANKS[this.receipt._tag].terminal
  }
  get outcome(): Option.Option<Receipt> {
    return this.settled ? Option.some(this.receipt) : Option.none()
  }
}
```

## [04]-[PROGRESS_FOLD]

[PROGRESS_FOLD]:
- Owner: `ProgressMark` — operation coordinate (`ContentKey`), optional parent operation (the hierarchy axis), stage label, done/total units, `Hlc` stamp, tenant — one decoded shape for every emitting surface; `Progress` — the fold family: `state` (the `Merge.struct` product instance), `plan` (the per-operation plan row), `fraction`/`stalled` (read-time verdicts), `rollup` (the parent-axis weighted aggregation).
- Law: units are dimensionless counts — done and total are non-negative integers whose meaning the emitting operation declares; a `{value, unit}` shape never exists here — `value/quantity` owns dimensioned magnitudes — and `total` is optional evidence: unbounded operations emit marks without totals, every dividing read is `Option`-shaped through `Number.divide`, so an unknown total folds to absent fraction, never `NaN`.
- Law: the state product composes proven rows — `head` (stage paired with its stamp, merged by stamped LWW so the field cannot drift from the clock that justified it), `done` monotone max (a regressing counter is late evidence, not regression), `total` optional max (totals grow as work is discovered), `parent` first-wins, `first`/`last` min/max stamps — and the product's posture derives as the conjunction, so the whole instance is convergence-legal by construction.
- Law: verdicts are read-time and horizon-parameterized — `fraction` divides through the `Option` rail and clamps to the unit interval; `stalled` measures the last stamp's physical distance from a caller-supplied horizon against a `Duration` policy converted once through `Hlc.delta` — an ambient clock read and a millisecond re-derivation never appear in the fold.
- Law: `rollup` folds the parent axis — the children index derives from the folded table in exactly one `_children` pass, the recursion walks that index to data depth carrying its visited path, and the subtree aggregate answers `Option.none` where no total is known anywhere in the subtree; a decoded `parent` cycle folds each operation once — a re-entrant key contributes the zero row, so a corrupt parent link degrades to an under-count, never divergence; the live incremental rollup over a churning table is `fold#DATAFLOW_VERBS`'s `grouped`/`closure` lanes, and this read is the bounded snapshot fold.
- Growth: a new progress verdict is one read member; a new mark axis (weight, priority) is one field plus one product row.
- Boundary: the mark's wire twin and its stream projection are the interchange codec's; `feed#ENTRY_FAMILY` wraps marks into feed entries; ui progress surfaces consume fractions, never raw marks.

```typescript
class ProgressMark extends Schema.Class<ProgressMark>("ProgressMark")({
  operation: ContentKey,
  parent: Schema.optionalWith(ContentKey, { as: "Option" }),
  stage: Schema.NonEmptyString,
  done: Schema.Int.pipe(Schema.nonNegative()),
  total: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: "Option" }),
  stamp: Hlc,
  tenant: TenantContext,
}) {
  static readonly byStamp: Order.Order<ProgressMark> = Order.mapInput(
    Hlc.Order,
    (mark: ProgressMark) => mark.stamp,
  )
}

declare namespace Progress {
  type Head = { readonly stage: string; readonly stamp: Hlc }
  type State = {
    readonly head: Head
    readonly done: number
    readonly total: Option.Option<number>
    readonly parent: Option.Option<ContentKey>
    readonly first: Hlc
    readonly last: Hlc
  }
  type Shape = {
    readonly state: Merge.Instance<State>
    readonly plan: Fold.Plan<ProgressMark, ContentKey, State>
    readonly fraction: (state: State) => Option.Option<number>
    readonly stalled: (state: State, horizon: Hlc, patience: Duration.Duration) => boolean
    readonly rollup: (table: Fold.Table<ContentKey, State>, root: ContentKey) => Option.Option<number>
  }
}

const _byHeadStamp: Order.Order<Progress.Head> = Order.mapInput(Hlc.Order, (head: Progress.Head) => head.stamp)

const _state: Merge.Instance<Progress.State> = Merge.struct({
  head: Merge.max(_byHeadStamp),
  done: Merge.max(Order.number),
  total: Merge.optional(Merge.max(Order.number)),
  parent: Merge.optional(Merge.first<ContentKey>(Equivalence.string)),
  first: Merge.min(Hlc.Order),
  last: Merge.max(Hlc.Order),
})

const _lifted = (mark: ProgressMark): Progress.State => ({
  head: { stage: mark.stage, stamp: mark.stamp },
  done: mark.done,
  total: mark.total,
  parent: mark.parent,
  first: mark.stamp,
  last: mark.stamp,
})

const _children = (
  table: Fold.Table<ContentKey, Progress.State>,
): HashMap.HashMap<ContentKey, ReadonlyArray<ContentKey>> =>
  HashMap.reduce(table, HashMap.empty<ContentKey, ReadonlyArray<ContentKey>>(), (acc, state, key) =>
    Option.match(state.parent, {
      onNone: () => acc,
      onSome: (parent) =>
        HashMap.modifyAt(acc, parent, (slot) =>
          Option.some(Option.match(slot, {
            onNone: (): ReadonlyArray<ContentKey> => [key],
            onSome: (kids) => Array.append(kids, key),
          }))),
    }))

const _weights = (
  table: Fold.Table<ContentKey, Progress.State>,
  children: HashMap.HashMap<ContentKey, ReadonlyArray<ContentKey>>,
  root: ContentKey,
  seen: HashSet.HashSet<ContentKey>,
): readonly [done: number, total: Option.Option<number>] =>
  HashSet.has(seen, root)
    ? [0, Option.none()] as const
    : Array.reduce(
        Option.getOrElse(HashMap.get(children, root), (): ReadonlyArray<ContentKey> => []),
        [
          Option.match(HashMap.get(table, root), { onNone: () => 0, onSome: (state) => state.done }),
          Option.flatMap(HashMap.get(table, root), (state) => state.total),
        ] as readonly [number, Option.Option<number>],
        (acc, child) =>
          pipe(_weights(table, children, child, HashSet.add(seen, root)), ([done, total]) => [
            acc[0] + done,
            Option.match(acc[1], {
              onNone: () => total,
              onSome: (held) => Option.some(held + Option.getOrElse(total, () => 0)),
            }),
          ] as const),
      )

const Progress: Progress.Shape = {
  state: _state,
  plan: Fold.plan({
    name: "state/progress",
    key: (mark) => mark.operation,
    lift: _lifted,
    merge: _state,
  }),
  fraction: (state) =>
    Option.map(
      Option.flatMap(state.total, (total) => Number.divide(state.done, total)),
      Order.clamp(Order.number)({ minimum: 0, maximum: 1 }),
    ),
  stalled: (state, horizon, patience) => horizon.physical - state.last.physical > Hlc.delta(patience),
  rollup: (table, root) =>
    pipe(_weights(table, _children(table), root, HashSet.empty()), ([done, total]) => Option.flatMap(total, (units) => Number.divide(done, units))),
}
```

## [05]-[AVAILABILITY_LATTICE]

[AVAILABILITY_LATTICE]:
- Owner: `Availability` — the decoded snapshot class: `level`, the per-command verdict `HashMap`, the `since` stamp, and the tenant; `worst` (the snapshot lattice), `admits` (the total gate read), and `plan` (the per-tenant fold) ride it as statics. The serving gate consumes it as an injected value typed against this module — ordinary dependency over a legal import, never a port.
- Law: the level column is the lawful bounded lattice — `Merge.lattice` over the rank `Bounded` with `full` as `minBound` and `offline` as `maxBound`, the `join` row carried in the field product — so zero health feeds fold to the `full` bottom through the lawful empty, severity is the only comparison, and no consumer compares level names lexically or through a hand ladder; gate policy is the `admits` column, data a total read projects, and `_ROWS` is contract-checked at the expression seam against the `_LEVELS` and `_POSTURES` anchors — a level without its row, an excess row, or an off-vocabulary posture fails at the declaration.
- Law: verdicts order by restrictiveness — `Available < Gated < Withheld`, `Withheld` tie-broken by level rank — and `_worstVerdict` is `Merge.max` over that order, so merging two sources never loosens a constraint; a verdict carries its evidence, and retry surfacing derives from `Gated.until`, never from prose parsing.
- Law: `Availability.worst` is the `Merge.struct` field product exactly as `Progress._state` and `presence` compose it — level through the bounded lattice join, commands through `Merge.hashMap(_worstVerdict)` per-command worst-wins, `since` by stamp max, tenant first-wins — the posture derives as the field conjunction instead of a literal claim, the class re-lands through one constructor wrap, and the convergence proof rides `Converge` like every sibling instance; a hand-rolled `Semigroup.make` with an inline `HashMap.reduce` beside the roster is the deleted spelling. The convergence domain is one tenant lane — the plan partitions by tenant BEFORE any merge and first-wins carries `self.tenant` through, so a cross-tenant combine is an upstream fold-key defect, never a merge question.
- Law: `Availability.admits` is total — a command absent from the map answers from the level row's posture through the `_FALLBACKS` lookup, so the gate never meets `undefined`, never re-implements the fallback, and posture-to-verdict stays a keyed row, never a branch ladder.
- Law: the command map crosses the wire as a keyed object — the protobuf map shape — and `_Commands` respells it into the interior `HashMap` at the field, so the decoded gate keys structurally while the encoded twin stays exactly what the C# mint emits; a pairs-array wire spelling is the shape no proto map produces.
- Law: gating durations and retry posture type against `value/fault` budget rows — the gate composes budget vocabulary with these verdicts; neither is re-declared here.
- Boundary: the level roster mirrors the C# AppHost health plane one-to-one; roster parity pins at the interchange decode seam; `feed#ENTRY_FAMILY` records level shifts.
- Growth: a new gate posture is one `_POSTURES` row; a new level is one `_LEVELS` entry plus its `_ROWS` row, and the bounded lattice re-tops in the same edit.

```typescript
const _LEVELS = ["full", "degraded", "readonly", "offline"] as const
const _POSTURES = ["all", "reads", "none"] as const

const _ROWS = {
  full: { rank: 0, admits: "all" },
  degraded: { rank: 1, admits: "all" },
  readonly: { rank: 2, admits: "reads" },
  offline: { rank: 3, admits: "none" },
} as const satisfies Record<(typeof _LEVELS)[number], { readonly rank: number; readonly admits: (typeof _POSTURES)[number] }>

const _Level = Schema.Literal(..._LEVELS)
const _Command = Schema.NonEmptyString.pipe(Schema.brand("CommandName"))

const _byRank: Order.Order<(typeof _LEVELS)[number]> = Order.mapInput(
  Order.number,
  (level: (typeof _LEVELS)[number]) => _ROWS[level].rank,
)

const _Available = Schema.TaggedStruct("Available", {})

const _Gated = Schema.TaggedStruct("Gated", {
  reason: Schema.NonEmptyString,
  until: Schema.optionalWith(Hlc, { as: "Option" }),
})

const _Withheld = Schema.TaggedStruct("Withheld", {
  level: _Level,
  reason: Schema.NonEmptyString,
})

const _Verdict: Schema.Union<[typeof _Available, typeof _Gated, typeof _Withheld]> = Schema.Union(
  _Available,
  _Gated,
  _Withheld,
)

const _VERDICT_RANKS = { Available: 0, Gated: 1, Withheld: 2 } as const satisfies Record<(typeof _Verdict.Type)["_tag"], number>

const _Commands = Schema.transform(
  Schema.Record({ key: _Command, value: _Verdict }),
  Schema.HashMapFromSelf({ key: Schema.typeSchema(_Command), value: Schema.typeSchema(_Verdict) }),
  {
    strict: true,
    decode: (record) => HashMap.fromIterable(Record.toEntries(record)),
    encode: (map) => Record.fromEntries(HashMap.toEntries(map)),
  },
)

const _byRestrictiveness: Order.Order<typeof _Verdict.Type> = Order.combine(
  Order.mapInput(Order.number, (verdict: typeof _Verdict.Type) => _VERDICT_RANKS[verdict._tag]),
  Order.mapInput(Order.number, (verdict: typeof _Verdict.Type) =>
    verdict._tag === "Withheld" ? _ROWS[verdict.level].rank : 0),
)

const _worstVerdict: Merge.Instance<typeof _Verdict.Type> = Merge.max(_byRestrictiveness)

const _fieldwise: Merge.Instance<{
  readonly level: (typeof _LEVELS)[number]
  readonly commands: HashMap.HashMap<typeof _Command.Type, typeof _Verdict.Type>
  readonly since: Hlc
  readonly tenant: TenantContext
}> = Merge.struct({
  level: Merge.lattice<(typeof _LEVELS)[number]>({ compare: _byRank, minBound: "full", maxBound: "offline" }).join,
  commands: Merge.hashMap(_worstVerdict),
  since: Merge.max(Hlc.Order),
  tenant: Merge.first(Schema.equivalence(TenantContext)),
})

const _FALLBACKS: Record<(typeof _POSTURES)[number], (level: (typeof _LEVELS)[number]) => typeof _Verdict.Type> = {
  all: () => _Available.make({}),
  reads: (level) => _Gated.make({ reason: level, until: Option.none() }),
  none: (level) => _Withheld.make({ level, reason: level }),
}

class Availability extends Schema.Class<Availability>("Availability")({
  level: _Level,
  commands: _Commands,
  since: Hlc,
  tenant: TenantContext,
}) {
  static readonly worst: Merge.Instance<Availability> = Merge.instance({
    combine: Semigroup.make((self: Availability, that: Availability) =>
      new Availability(_fieldwise.combine.combine(self, that))),
    posture: _fieldwise.posture,
    alike: Schema.equivalence(Availability),
    empty: Option.none(),
  })
  static readonly plan: Fold.Plan<Availability, TenantContext, Availability> = Fold.plan({
    name: "state/availability",
    key: (snapshot) => snapshot.tenant,
    lift: (snapshot) => snapshot,
    merge: Availability.worst,
  })
  static admits(snapshot: Availability, command: Availability.Command): Availability.Verdict {
    return Option.getOrElse(HashMap.get(snapshot.commands, command), () => _FALLBACKS[_ROWS[snapshot.level].admits](snapshot.level))
  }
}

declare namespace Availability {
  type Level = (typeof _LEVELS)[number]
  type Posture = (typeof _POSTURES)[number]
  type Command = typeof _Command.Type
  type Verdict = typeof _Verdict.Type
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Availability, Progress, ProgressMark, Receipt, ReceiptEnvelope }
```
