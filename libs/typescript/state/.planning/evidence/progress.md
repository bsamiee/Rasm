# [STATE_PROGRESS]

`evidence/progress.ts` owns progress-mark evidence: `ProgressMark` — the decoded mark the C# Compute runtime emits per operation stage (seam `CO:73`, decoded by `wire/codec/progress`) — and `Progress` — the fold family that turns a mark stream into monotone per-operation state, hierarchical roll-ups over the parent axis, and read-time verdicts (fraction, stalled) that take the observation horizon as a parameter instead of reading an ambient clock. The state instance is a `Merge.struct` product — stage by stamped LWW, units by monotone max, span by min/max stamps — so progress convergence is a composition of proven rows, and the fold is one `Fold.Plan` row every altitude runs unchanged.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                            | [SURFACE]                                          |
| :-----: | :--------------- | :------------------------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | [MARK_SHAPE]     | the decoded progress mark                                           | `ProgressMark`                                         |
|  [02]   | [PROGRESS_FOLD]  | the state product instance, the fold plan, read-time verdicts, roll-up | `Progress.state/.plan/.fraction/.stalled/.rollup` |

## [2]-[MARK_SHAPE]

- Owner: `ProgressMark` — operation coordinate (`ContentKey`), optional parent operation (the hierarchy axis), stage label, done/total units, `Hlc` stamp, tenant — one decoded shape for every emitting surface, with the stamp order riding the class.
- Packages: `effect` (`Schema`, `Order`, `Option`, `Number`, `HashMap`, `Equivalence`); `@rasm/ts/kernel` (`ContentKey`, `Hlc`, `TenantContext`); `../crdt/merge.ts`; `../fold/algebra.ts`.
- Law: units are dimensionless counts — done and total are non-negative integers whose meaning the emitting operation declares; a `{value, unit}` shape never exists here (kernel `Quantity` owns dimensioned magnitudes, invariant 4).
- Law: `total` is optional evidence — unbounded operations emit marks without totals, and every downstream read that divides is `Option`-shaped through `Number.divide`, so an unknown total folds to absent fraction, never `NaN`.
- Boundary: the mark's wire twin is `wire/codec/progress`'s; the ProgressStore stream projection (seam `CO:76`) is `wire`'s concern — this owner receives decoded marks only.

```typescript
import { Array, type Duration, Equivalence, HashMap, Number, Option, Order, Schema, pipe } from "effect"
import { ContentKey, Hlc, TenantContext } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"

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
```

## [3]-[PROGRESS_FOLD]

- Owner: `Progress` — the fold family: `state` (the `Merge.struct` product instance), `plan` (the per-operation `Fold.Plan`), `fraction`/`stalled` (read-time verdicts), `rollup` (the parent-axis weighted aggregation).
- Law: the state product composes proven rows — `head` (stage paired with its stamp, merged by stamped LWW so the field cannot drift from the clock that justified it), `done` monotone max (a regressing counter is late evidence, not regression), `total` optional max (totals grow as work is discovered), `parent` first-wins, `first`/`last` min/max stamps — and the product's posture derives as the conjunction, so the whole instance is convergence-legal by construction.
- Law: verdicts are read-time and horizon-parameterized — `fraction` divides through the `Option` rail and clamps to the unit interval; `stalled` measures the last stamp's physical distance from a caller-supplied horizon against a `Duration` policy converted once through the kernel's `Hlc.delta` unit site — an ambient `Date.now` read and a millisecond re-derivation never appear in the fold (`Clock` is a service concern of consumers, not of state).
- Law: `rollup` folds the parent axis — the children index derives from the folded table in exactly one `_children` pass, the recursion walks that index to data depth, and the subtree aggregate sums done/total pairs, answering `Option.none` where no total is known anywhere in the subtree.
- Growth: a new progress verdict is one read member; a new mark axis (weight, priority) is one field plus one product row.
- Boundary: `evidence/timeline` wraps marks into feed entries; `query/live` serves the folded operation table; `ui` progress surfaces consume fractions, never raw marks.

```typescript
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
): readonly [done: number, total: Option.Option<number>] =>
  Array.reduce(
    Option.getOrElse(HashMap.get(children, root), (): ReadonlyArray<ContentKey> => []),
    [
      Option.match(HashMap.get(table, root), { onNone: () => 0, onSome: (state) => state.done }),
      Option.flatMap(HashMap.get(table, root), (state) => state.total),
    ] as readonly [number, Option.Option<number>],
    (acc, child) =>
      pipe(_weights(table, children, child), ([done, total]) => [
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
    name: "evidence/progress",
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
    pipe(_weights(table, _children(table), root), ([done, total]) => Option.flatMap(total, (units) => Number.divide(done, units))),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Progress, ProgressMark }
```
