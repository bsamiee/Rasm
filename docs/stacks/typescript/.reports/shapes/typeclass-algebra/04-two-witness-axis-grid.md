# Two-Witness Axis Grid

[THE_FOUR_AXES_ARE_SUPERCLASS_INDEPENDENT_NOT_MERELY_ORTHOGONAL_IN_USE]:
- `Traversable<T> extends TypeClass<T>` directly — NOT `Covariant<T>`, NOT `Foldable<T>` — so the structure-walk witness carries `traverse` and nothing else; `Foldable<T> extends TypeClass<T>` carries only `reduce`; `Filterable<F> extends TypeClass<F>` carries only `partitionMap`/`filterMap`. The independence is structural, not conventional: the type lattice itself refuses to bundle walk, fold, and filter into one heritage, so an owner declares each capability it has and a combinator demands exactly the ones it consumes.
- The product space is therefore a grid of INDEPENDENTLY-PRESENT cells, not a single capability tier: an owner may be `Foldable` without `Traversable` (a fold needs no rebuild of the structure), `Traversable` without `Foldable` (`data/Record` carries `Traversable`/`Filterable`/`Covariant` but ships NO `Foldable` const — a keyed structure walks and filters but exposes no `reduce`), and `Filterable` without `Coproduct`. Each absent const is a foreclosed grid cell, read off the export list before any call.
- The naive shape writes one loop per occupied cell — a `for` that walks AND folds AND tests-and-drops AND summarizes is one body welding four independent decisions; the witness-grid factors that body into four arguments, each selecting one axis, so the body is written once and every cell is a different tuple of imports fed to the same combinator.

[THE_STRUCTURE_WITNESS_GATES_WHICH_ALGEBRA_CELLS_EXIST]:
- The structure axis is not free over the algebra axis: the carrier's own shape forecloses cells. `data/Either` carries `Foldable` and `Traversable` but NO `Filterable` and NO `TraversableFilterable`, because a single-success slot has no way to DROP an element — filtering an `Either<A, E>` would need a neutral `Left` witness to absorb the rejected case, which `Either` lacks (a `Left` demands a witness `E`). So the (Either × filter) cell is uninhabitable and the missing const makes it a compile fact, never a runtime "filtered to what `Left`?" question.
- `data/Option` carries the FULL filterable stack (`Filterable`, `TraversableFilterable`) AND the FULL coproduct stack (`Coproduct`, `Alternative`) because `None` IS both the drop target and the `zero` identity — one carrier datum serves the filter axis and the recovery-fold axis. `data/Array` carries everything EXCEPT `Coproduct`/`Alternative` (an array has no single-choice point) — so (Array × first-success-fold) is foreclosed and (Array × monoid-fold) is open.
- The reachability lattice reads: the structure witness chosen first restricts the algebra witnesses that can pair with it, and the pairing legality is a present-or-absent const, never a guarded branch. An algorithm constrained on `Traversable<T> & Filterable<T>` accepts `Array` and `Option` and REJECTS `Either` at the call site — the keep-out is the type checker discharging a grid coordinate, not a runtime assertion.

[THE_ALGEBRA_AXIS_IS_THE_SEED_STEP_PAIR_OF_ONE_REDUCE]:
- The three fold combinators over a single `Foldable<F>` are ONE `F.reduce` with a different `(seed, step)` per algebra witness: `combineMap(F)(M)` is `F.reduce(self, M.empty, (m, a) => M.combine(m, f(a)))`, `reduceKind(F)(G)` is `F.reduce(self, G.of(b), (gb, a) => G.flatMap(gb, b => f(b, a)))`, `coproductMapKind(F)(G)` is `F.reduce(self, G.zero(), (gb, a) => G.coproduct(gb, f(a)))` — the structure witness supplies the iteration, the algebra witness supplies both the seed AND the step, and swapping `M`/`Monad<G>`/`Coproduct<G>` rewrites neither the walk nor `f`.
- The seed is the algebra's identity at every cell: `M.empty` for the monoid summary, `G.of(b)` lifting the plain accumulator into the sequencing carrier, `G.zero()` for the choosing carrier — so the algebra-witness axis is precisely the (identity, binary-op) pair, and the reason `coproductMapKind` demands the FULL `Coproduct<G>` (not `SemiCoproduct<G>`) is that the empty-foldable case must seed with `G.zero()`; a `SemiCoproduct` has no `zero`, so a first-success fold over a possibly-empty structure is a missing-export compile fact, the identity boundary surfacing on the SECOND witness rather than the first.
- A standalone reducer hardcodes both axes into one body: a hand-written `arr.reduce((acc, x) => acc + score(x), 0)` pins (Array structure, sum monoid) into one cell, and changing the summary to a max, a first-success search, or a sequenced fold means rewriting the reducer, where `combineMap(F)(N.MonoidMax)` versus `coproductMapKind(F)(OptionInstances.Coproduct)(f)` reuses the same `F.reduce` walk under a swapped algebra witness.

```typescript
import { Foldable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as OptionInstances from '@effect/typeclass/data/Option'
import * as N from '@effect/typeclass/data/Number'
import { Option } from 'effect'

const walk = ArrayInstances.Foldable
const score = (s: string): number => s.length
const probe = (s: string): Option.Option<number> => (s.length > 2 ? Option.some(s.length) : Option.none())

const total: number = Foldable.combineMap(walk)(N.MonoidSum)(score)(['<value-a>', '<value-bb>'])
const peak: number = Foldable.combineMap(walk)(N.MonoidMax)(score)(['<value-a>', '<value-bb>'])
const firstWide: Option.Option<number> = Foldable.coproductMapKind(walk)(OptionInstances.Coproduct)(probe)(['<value-a>', '<value-bbb>'])
```

[THE_INNER_APPLICATIVE_AXIS_IS_STRUCTURALLY_DISJOINT_FROM_THE_OUTER_WALK]:
- `traverse(F: Applicative<F>)` and `reduce`/`combineMap` (which take no inner carrier) prove the inner-applicative is a SEPARATE axis from the algebra axis, not a richer slot of it: a `Foldable` fold's algebra witness is a `Monoid`/`Monad<G>`/`Coproduct<G>` consumed by `F.reduce`, while a `Traversable` walk's inner witness is an `Applicative<F>` that must REBUILD the structure (`Kind<F,…,Kind<T,…,B>>`), so the two axes differ in whether the structure survives the effect — the fold collapses `T` to a scalar, the traverse threads `F` through and re-emits `T`.
- The cell legality on this axis is which carriers expose `Applicative`: `data/Either` is `Foldable`+`Traversable` yet has no `Filterable`, so (Either-outer × any-inner) traverses but never filters, while ANY of `Array`/`Option`/`Either` may serve as the INNER applicative because each ships the const — the inner-witness slot and the outer-walk slot draw from overlapping but not identical instance sets, and the role is assigned by argument position, never by carrier type.
- The inner axis uniquely admits a POLICY sub-witness the algebra axis forbids: `data/Effect` ships NO `Traversable`/`Foldable`/`Filterable` const — an `Effect` is uninhabitable on the structure axis (not a walkable container) — yet ships `getApplicative(options?: ConcurrencyOptions)` as a FACTORY, so a traverse over an array of effects carries a concurrency degree as a nested datum on the inner witness: `traverse(getApplicative({ concurrency: 'unbounded' }))` evaluates per-element effects in parallel, `traverse(getApplicative())` sequentially — same walk, same `f`, the parallelism a parameter of the inner witness, an axis-of-an-axis no fold combinator can express.

```typescript
import { Traversable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as EitherInstances from '@effect/typeclass/data/Either'
import * as EffectInstances from '@effect/typeclass/data/Effect'
import { Effect, Either } from 'effect'

type Fault = { readonly _tag: 'Fault'; readonly at: number }
const parse = (n: number): Either.Either<number, Fault> => (n > 0 ? Either.right(n * 2) : Either.left({ _tag: 'Fault', at: n }))
const probe = (n: number): Effect.Effect<number> => Effect.succeed(n + 1)

const firstFault: Either.Either<ReadonlyArray<number>, Fault> = ArrayInstances.Traversable.traverse(EitherInstances.Applicative)([3, 1, 8], parse)
const parallel = Traversable.traverse(ArrayInstances.Traversable)(EffectInstances.getApplicative({ concurrency: 'unbounded' }))
const probed: Effect.Effect<ReadonlyArray<number>> = parallel([2, 5, 9], probe)
```

[THE_FILTER_AXIS_IS_THE_OPTION_VERSUS_EITHER_DISCRIMINANT_AND_TUPLE_SLOT_ORDER_IS_A_TRAP]:
- `Filterable<F>` reduces the keep-or-drop decision to ONE discriminant: `filterMap(f: (a) => Option<B>)` keeps `Some` payloads (drop-the-rest), `partitionMap(f: (a) => Either<C, B>)` splits into BOTH halves by the `Either` tag (keep-both-sorted). The filter axis is therefore the choice of return-carrier of `f`, never a boolean knob threading a second pass — `Option`-valued is lossy keep, `Either`-valued is lossless split, and `filter`/`partition`/`compact`/`separate` are all derived from those two cores.
- The tuple slot order of `partitionMap` is the load-bearing trap: it returns `[Kind<F,…,B>, Kind<F,…,C>]` where `B` is the `Right` payload and `C` is the `Left` payload — the FIRST slot is `Right`/accepted, the SECOND is `Left`/rejected, the inverse of the `Either` value's left-then-right reading position. A consumer destructuring `const [rejected, accepted] = partitionMap(f)` silently swaps the two halves; the type checker permits it whenever `B` and `C` unify, so the slot order is a semantic fact the value-position intuition contradicts.
- `partition`'s refinement overload is sharper still: `partition(F)(refinement)` returns `[Kind<F,…,C>, Kind<F,…,B>]` — the FIRST slot keeps the ORIGINAL `C` (the elements the guard rejected, untouched) and the SECOND NARROWS to `B` (the guard's `is B` witnesses). So `filter`/`partition` carry a type-narrowing `(a) => a is B` overload no boolean predicate can — a type-guard filter tightens `T<A>` to `T<B>` in the kept carrier, a compile guarantee a `(a) => boolean` cannot make, but the narrowed half is the SECOND tuple slot, not the first.

[THE_COMPOSITION_DERIVERS_PROMOTE_A_NESTED_LAYER_TO_A_THIRD_COMPOSED_WITNESS]:
- A stacked structure (records of arrays, arrays of options) gets its walk/fold/filter from COMPOSING the two layer witnesses, never a hand-written double loop: `traverseComposition(T, G)(F)` walks `Kind<T,…,Kind<G,…,A>>` threading the inner applicative `F` through both layers in one signature, `reduceComposition(F, G)` folds the nested structure with one `(b, a) => b` step, `filterMapComposition(F, G)` filters the inner layer of an outer-covariant nesting. The nesting is the third witness, supplied by composition, and a third layer is a third composition, not a rewritten recursion.
- The composition derivers state EXACTLY which capability each layer must carry, and the asymmetry is load-bearing: `filterMapComposition(F: Covariant<F>, G: Filterable<G>)` and `partitionMapComposition(F: Covariant<F>, G: Filterable<G>)` demand only `Covariant` of the OUTER `F` (it merely maps the inner filtered structure back into place) but `Filterable` of the INNER `G` (the drop happens at the leaf) — so filtering a structure of filterable structures needs the outer to map and the inner to drop, the two roles assigned by argument position, and an outer that is only `Covariant` (never `Filterable`) still composes because the outer never drops.
- This is the multiplicative collapse the witness-pair buys made one layer deeper: a flat grid is (structure × algebra) cells; the *Composition derivers make it (outerStructure × innerStructure × algebra) WITHOUT a new combinator per depth, because each layer contributes its own witness and the deriver fuses them. Adding a nesting level adds one composition call and zero algorithm edits — where the naive form adds one more nested loop welding the new layer's walk into the same body.

```typescript
import { Traversable, Filterable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as OptionInstances from '@effect/typeclass/data/Option'
import { Option } from 'effect'

const probe = (n: number): Option.Option<number> => (n > 0 ? Option.some(n) : Option.none())
const keep = (n: number): Option.Option<number> => (n < 100 ? Option.some(n) : Option.none())

const walkNested = Traversable.traverseComposition(ArrayInstances.Traversable, ArrayInstances.Traversable)(OptionInstances.Applicative)
const validated: Option.Option<ReadonlyArray<ReadonlyArray<number>>> = walkNested([[3, 7], [1]], probe)
const filterInner: ReadonlyArray<ReadonlyArray<number>> = Filterable.filterMapComposition(ArrayInstances.Covariant, ArrayInstances.Filterable)([[5, 200], [9]], keep)
```

[THE_FUSED_CELL_DERIVES_ITS_OWN_WITNESS_BUDGET_AS_A_GRID_COORDINATE]:
- `TraversableFilterable<T>` fuses walk, inner-applicative effect, and filter into ONE pass — `traverseFilterMap(F)(f: (a) => Kind<F,…,Option<B>>)` walks `T`, runs each element's `F`, keeps the inner `Some`s, where the unfused form is `traverse(F)` then `filterMap` (two passes, two intermediate carriers). The fusion is not a new combinator atop the grid; it is a DERIVED instance whose member set is computed from the constituent axes' instances.
- The deriver's constraint difference IS the partition-versus-filter cost made checkable: `traverseFilterMap(T)` derives from `Traversable<T> & Filterable<T>`, but `traversePartitionMap(T)` ADDS `Covariant<T>` — because a partition's single walk must rebuild TWO output carriers (accepted `B`, rejected `C`) and `Covariant` is the witness that re-maps the one traversal result into the second carrier, while a filter rebuilds only one. So an owner carrying walk+filter gets effectful filtering free and effectful partitioning ONLY if additionally covariant — the fused cell's witness budget is a grid coordinate read off the deriver signature, never a fourth hand-written instance member, and the extra `Covariant` is the algebraic price of the second output channel.
- `traverseFilter(F)(predicate: (a) => Kind<F,…,boolean>)` is where the boolean knob the grid otherwise forbids is LAWFULLY re-admitted: the keep-test runs INSIDE the inner applicative, so it accumulates `F`'s channels (a fallible or requiring check) and the refinement narrows `B extends A` while the channel survives — a `(a) => boolean` is permitted only because it is wrapped in the inner witness, the synchronous-second-pass form the fusion deletes.

```typescript
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as O from '@effect/typeclass/data/Option'
import { Either, Option } from 'effect'

const classify = (n: number): Option.Option<Either.Either<number, number>> =>
    n === 0 ? Option.none() : Option.some(n > 0 ? Either.right(n) : Either.left(-n))

const split = ArrayInstances.TraversableFilterable.traversePartitionMap(O.Applicative)
const halves: Option.Option<[ReadonlyArray<number>, ReadonlyArray<number>]> = split([5, -2, 0, 8], classify)
```

[CONFLATING_TWO_AXES_INTO_ONE_BODY_IS_THE_REJECT_THE_GRID_DELETES]:
- The rejected shape welds independent axes into one loop: a body that walks an array, tests each element with an inline `if`, pushes survivors to a fresh array, and accumulates a running total in a `let` pins FOUR coordinates — Array structure, synchronous short-circuit-free walk, boolean-filter, sum-monoid — into one statement, so a change on any axis (a different container, an effectful check, a partition instead of a filter, a max instead of a sum) forces rewriting the whole body. The grid factors that into `traverseFilter`/`combineMap` calls where each axis is a swapped witness.
- The conflation is also a CORRECTNESS hazard, not only a density one: a single body that both filters and folds in one pass fixes the filter-versus-fold ORDER, but the witness grid keeps them as separate combinators precisely because the filter axis (`Option`-keep) and the fold axis (monoid summary) compose only in a declared sequence — `pipe(self, filterMap(probe), combineMap(M)(f))` versus a fused `traverseFilterMap` differ in whether the dropped elements still contribute to the channel accumulation, and welding them hides which semantics is meant.
- The structure-witness drift is the subtler reject: passing `Either.Applicative` as the inner traverse witness where every element's fault must survive keeps only the leftmost `Left` (mechanically, `product` returns the first non-`Right`), so a validator that should accumulate all faults silently reports one — the bug is choosing the wrong CELL on the inner-applicative axis, and it surfaces as a missing-faults runtime symptom, never a type error, because both `Either.Applicative` and a lifted accumulating applicative satisfy the same `Applicative<F>` constraint the walk demands.

[THE_DECLARATION_COUNT_IS_THE_SUM_WHILE_THE_CELL_COUNT_IS_THE_PRODUCT]:
- The grid's economy is the arithmetic gap between declarations and cells: `m` structure owners crossed with `n` algebra witnesses span `m × n` reachable cells from `m + n` declarations, where the naive form pays `m × n` hand-written loops — every loop welding one (structure, algebra) pair into one body. Each declaration is a witness const or factory; the combinator body is written once; the cell is the tuple of witnesses fed to it.
- The `Record.getTraversable<K>()` factory is the proof a new structure owner costs one declaration with zero algorithm edits AND preserves the key type through every fold/filter/traverse already written over the grid — the literal-key map walks with `K` intact, where a defaulted const would widen it, so the structure axis grows by one factory call that recovers a type axis the naive loop discards.
- The *Composition derivers turn the SUM-versus-PRODUCT gap into one that survives nesting depth: `m` outer owners times `k` inner owners times `n` algebras span `m × k × n` cells from `m + k + n` declarations plus the fixed `traverseComposition`/`reduceComposition`/`filterMapComposition` combinators — a stacked structure's traversal grows by one composition call, never a rewritten recursion per (outer, inner) pair, so the declaration count stays the SUM of the axes while the capability stays their PRODUCT as both axis count and nesting depth rise. The naive form's loop count multiplies on every new axis or layer; the grid's declaration count only adds.
