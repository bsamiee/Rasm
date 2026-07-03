# [STATE_CONVERGE]

`crdt/converge.ts` states the convergence laws as values: every `Merge.Instance` carries obligations — associativity always, commutativity and idempotence exactly when its posture claims them, identity exactly when an `empty` exists — and this module owns the obligation selector, the pure witnesses that evaluate one law against one sample, the replay-commutation predicate that ties instance law to fold law, and the fixture-reference vocabulary that pins each proof to the `tests/contracts` corpus. The library owns predicates; the proof harness lives in the tests estate — `tests/typescript/_testkit` lifts these witnesses over Schema-derived arbitraries and the frozen corpus fixtures, so the same law body runs against generated samples and pinned cross-language bytes without a bespoke harness.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                             | [SURFACE]                                            |
| :-----: | :---------------- | :------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | [LAW_VOCABULARY]  | the law rows, obligation selection, per-law witnesses, breach fault | `Converge.Law`, `Converge.obligations`, `Converge.witness`, `Breach` |
|  [02]   | [REPLAY_COMMUTES] | permutation-invariance of plan folds, table equivalence             | `Converge.commutes`, `Converge.tables`                  |
|  [03]   | [FIXTURE_HOOKS]   | corpus fixture references binding laws to frozen bytes              | `Converge.fixture`, `Converge.Fixture`                  |

## [2]-[LAW_VOCABULARY]

- Owner: the `_LAWS` anchor and the `_WITNESSES` record — one witness per law, each a total predicate over an instance and a three-value sample, so a law is data a harness enumerates, never prose a spec restates.
- Packages: `effect` (`Array`, `Data`, `Either`, `Option`); `./merge.ts`.
- Law: obligations derive from the instance itself — `associativity` unconditionally, `commutativity`/`idempotence` from the posture, `identity` from `Option.isSome(empty)` — so an instance cannot under-declare its proof surface, and `Merge.counter`'s non-idempotent posture routes it around the idempotence law toward the op-identity dedup the replay lane's structural `consolidate` provides.
- Law: every witness compares through the instance's own `alike` — the equivalence declared at the instance is the equality the law is proven under, so structural classes, plain records, and branded scalars all prove under one spelling.
- Law: `Converge.witness` folds the obligation set over one sample and reports the first breach as a typed fault carrying the law name and the sample operands themselves — evidence as data the harness shrinks; rendering happens at the reporting edge, never by lossy string coercion inside the library.
- Growth: a new law is one `_LAWS` row plus one witness arm — the record contract turns a missing witness into a compile error; a new instance adds zero lines here.

```typescript
import { Array, Data, Either, type Equivalence, HashMap, Option } from "effect"
import { Merge } from "./merge.ts"
import { Fold } from "../fold/algebra.ts"

const _LAWS = ["associativity", "commutativity", "idempotence", "identity"] as const

declare namespace Converge {
  type Law = (typeof _LAWS)[number]
  type Sample<A> = { readonly first: A; readonly second: A; readonly third: A }
  type Fixture = { readonly corpus: string; readonly seam: string; readonly asset: string; readonly laws: ReadonlyArray<Law> }
  type Shape = {
    readonly obligations: <A>(instance: Merge.Instance<A>) => ReadonlyArray<Law>
    readonly witness: <A>(instance: Merge.Instance<A>, sample: Sample<A>) => Either.Either<ReadonlyArray<Law>, Breach>
    readonly commutes: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
    ) => (left: ReadonlyArray<Op>, right: ReadonlyArray<Op>) => boolean
    readonly tables: <K, S>(
      alike: Equivalence.Equivalence<S>,
    ) => (left: Fold.Table<K, S>, right: Fold.Table<K, S>) => boolean
    readonly fixture: (spec: Fixture) => Fixture
  }
}

class Breach extends Data.TaggedError("Breach")<{
  readonly law: Converge.Law
  readonly operands: ReadonlyArray<unknown>
}> {}

const _WITNESSES: { readonly [L in Converge.Law]: <A>(instance: Merge.Instance<A>, sample: Converge.Sample<A>) => boolean } = {
  associativity: (instance, { first, second, third }) =>
    instance.alike(
      instance.combine.combine(instance.combine.combine(first, second), third),
      instance.combine.combine(first, instance.combine.combine(second, third)),
    ),
  commutativity: (instance, { first, second }) =>
    instance.alike(instance.combine.combine(first, second), instance.combine.combine(second, first)),
  idempotence: (instance, { first }) => instance.alike(instance.combine.combine(first, first), first),
  identity: (instance, { first }) =>
    Option.match(instance.empty, {
      onNone: () => true,
      onSome: (empty) =>
        instance.alike(instance.combine.combine(first, empty), first)
        && instance.alike(instance.combine.combine(empty, first), first),
    }),
}
```

## [3]-[REPLAY_COMMUTES]

- Owner: `Converge.commutes` — the bridge law between instance and fold: for a plan whose instance is convergence-legal, folding any two permutations of one delivered op set yields equivalent tables; this is the algebraic half of `fold/replay`'s REPLAY_LAW, stated where the merge law lives so instance proofs and replay proofs share one predicate.
- Law: table equivalence compares key census and per-key states under the plan's own `alike` — `Converge.tables` is the one table comparison every convergence and replay assertion uses; a `JSON.stringify` table diff or reference comparison is the deleted spelling.
- Law: `commutes` consults `Merge.convergent` first — a non-convergent instance answers permutation-invariance `false` by construction rather than sampling its way to a lie, and the harness routes such plans to ordered-delivery proofs instead.
- Boundary: which permutations to generate, how many runs, and shrinking are `tests/typescript/_testkit` decisions over `it.prop`; this module owns only the decidable predicate.

```typescript
const _tables = <K, S>(alike: Equivalence.Equivalence<S>) =>
(left: Fold.Table<K, S>, right: Fold.Table<K, S>): boolean =>
  HashMap.size(left) === HashMap.size(right)
  && HashMap.reduce(left, true, (holds, state, key) =>
    holds && Option.match(HashMap.get(right, key), { onNone: () => false, onSome: (held) => alike(state, held) }))

const _commutes = <Op, K, S>(plan: Fold.Plan<Op, K, S>) =>
(left: ReadonlyArray<Op>, right: ReadonlyArray<Op>): boolean =>
  Merge.convergent(plan.merge)
  && _tables<K, S>(plan.merge.alike)(Fold.run(plan, left), Fold.run(plan, right))
```

## [4]-[FIXTURE_HOOKS]

- Owner: `Converge.Fixture` — the reference row binding a law set to a frozen corpus asset: `corpus` names the estate root, `seam` the cross-language seam id, `asset` the fixture coordinate; rows are minted by the tests estate as the corpus lands, and the constructor keeps the row shape closed here so a fixture reference is one vocabulary everywhere.
- Law: fixtures pin the C#-minted op families — the wire CRDT op log (seam `PE:44`, decoded by `wire/codec/crdt` into `Merge` instances) proves its instance laws against pinned bytes, not only generated samples, so cross-language convergence is asserted at the byte altitude.
- Boundary: corpus bytes live at `tests/contracts`; TS readers and the `it.prop` law harness live at `tests/typescript/_testkit` — neither is imported here, and no fixture path is hardcoded in the library.

```typescript
const Converge: Converge.Shape = {
  obligations: (instance) => [
    "associativity" as const,
    ...(instance.posture.commutative ? ["commutativity" as const] : []),
    ...(instance.posture.idempotent ? ["idempotence" as const] : []),
    ...(Option.isSome(instance.empty) ? ["identity" as const] : []),
  ],
  witness: (instance, sample) =>
    Option.match(
      Array.findFirst(Converge.obligations(instance), (law) => !_WITNESSES[law](instance, sample)),
      {
        onNone: () => Either.right(Converge.obligations(instance)),
        onSome: (law) =>
          Either.left(new Breach({ law, operands: [sample.first, sample.second, sample.third] })),
      },
    ),
  commutes: _commutes,
  tables: _tables,
  fixture: (spec) => spec,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Breach, Converge }
```
