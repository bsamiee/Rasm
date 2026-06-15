# Reorder-Safety Predictor

[THE_FOUR_LAWS_ARE_FOUR_INDEPENDENT_EDIT_LICENSES_NOT_ONE_QUALITY_TIER]:
- Each of the four properties licenses exactly ONE class of fold-body edit, and a witness carrying a property AUTHORIZES that edit while carrying none of the others — the four are an orthogonal license vector, never a strength ordering: associativity licenses RE-PARENTHESIZING a fixed sequence (`(a⊕b)⊕c → a⊕(b⊕c)`, the premise of any tree/parallel fold), commutativity licenses REORDERING the sequence (`a⊕b → b⊕a`), idempotence licenses DEDUPLICATING adjacent-or-not repeats (`a⊕a → a`), and a two-sided identity licenses DROPPING-AND-SEEDING the empty case (`combineAll([]) → empty`).
- The predictor reads in one direction: name the edit a refactor performs on a fold, and the law that edit CONSUMES is fixed; then check the witness's law-profile for that exact property. A reassociating edit over a non-associative `combine` and a reordering edit over a non-commutative `combine` are the same category of fault — an edit cashing a license the instance never issued — and BOTH produce a defined, type-checking, silently-wrong value, because no law sits in the witness's runtime shape (`{ combine, combineMany }`) for the checker to read.
- The library's own derived combinators are pre-cashed licenses: `combineMany`'s default `reduce(self, combine)` cashes associativity (it picks ONE parenthesization), a custom `combineMany` cashes associativity AGAIN by choosing a DIFFERENT parenthesization, and the two agreeing IS the operational test of associativity — divergence between the default left-fold and any hand-supplied bulk fold is the proof the law was violated, surfaced as a value mismatch the type system cannot pre-empt.

[THE_ASSOC_COMM_IDEM_IDENTITY_PROFILE_OF_EVERY_SHIPPED_INSTANCE]:
- The concrete owners partition cleanly by which licenses they issue, and the partition is the lookup table a refactor consults before reordering, deduping, or tree-folding any accumulation built from them:

| instance source | assoc | comm | idem | identity | reorder-safe | dedup-safe |
| :-------------- | :---: | :--: | :--: | :------: | :----------- | :--------- |
| `Number.MonoidSum` / `BigInt.MonoidSum` | yes | yes | no | `0` | yes | no |
| `Number.MonoidMultiply` | yes | yes | no | `1` | yes | no |
| `Number.MonoidMin` / `MonoidMax` (extremum) | yes | yes | yes | bound | yes | yes |
| `Boolean.MonoidEvery` (∧) / `MonoidSome` (∨) | yes | yes | yes | `true`/`false` | yes | yes |
| `Boolean.MonoidXor` | yes | yes | NO | `false` | yes | NO |
| `Boolean.MonoidEqv` (↔) | yes | yes | NO | `true` | yes | NO |
| `String.Monoid` (concat) | yes | NO | no | `""` | NO | no |
| `Semigroup.first` / `last` | yes | NO | yes-on-equal | none | NO | yes-on-equal |
| `Order.combine` (comparator monoid) | yes | NO | yes | `empty()` | NO | yes |
| `Equivalence.combine` (∧ of relations) | yes | yes | yes | always-true | yes | yes |

- The two rows that look identical at the value level but invert the reorder license are the fork's spine: `Order.combine` and `Equivalence.combine` are BOTH leftmost/conjunction folds of comparison witnesses, both associative, both with a unit, yet `Order.combine` is NON-commutative (`combine(byA, byB) !== combine(byB, byA)` — two distinct total orders) while `Equivalence.combine` IS commutative (`a && b === b && a`) — so the SAME `combineAll(rows)` shape over the SAME field accessor is a correctness commitment to row ORDER on one surface and a free performance permutation on the other.

[WHY_THE_ORDER_OF_KEYS_FLIPS_BETWEEN_THE_TWO_COMPARISON_OWNERS]:
- `Order.combine` returns `self`'s verdict on a decided pair and falls to `that` only on a TIE (`out !== 0 ? out : that(a1, a2)`); a permutation of keys re-decides every pair where an earlier key would have broken a later key's tie, so swapping two keys with overlapping equivalence classes yields a genuinely different ordering of those classes — the fold is order-sensitive precisely BECAUSE the binary op short-circuits on the FIRST decisive operand, and short-circuit + asymmetric verdict is the mechanical signature of non-commutativity.
- `Equivalence.combine` ANDs every relation (`self(x,y) && that(x,y)`); conjunction's commutativity is unconditional, so a permutation cannot change the verdict — yet `combineMany` short-circuits on the FIRST disagreeing relation (`if (!equivalence(x, y)) return false`), so the WORK is order-sensitive even though the RESULT is not. The fork is exact: on `Order` the short-circuit changes the answer (correctness lever), on `Equivalence` the short-circuit changes only the cost (the cheapest-discriminating field belongs first to prune earliest, a perf lever).
- The trap is treating the two `combineAll(fieldRows)` call shapes as interchangeable because they read identically: reordering an `Order.combineAll` chain to "put the cheap comparison first" silently re-prioritizes the sort, while reordering an `Equivalence.combineAll` chain to put the cheap one first is the intended optimization — the same edit is a bug on one owner and the recommended tuning on the other, and only the law-profile column distinguishes them.

```typescript
import { Order, Equivalence, Equal } from 'effect'

type Owner = { readonly rank: number; readonly tier: number; readonly tag: string }
const byField = <K extends keyof Owner>(k: K, O: Order.Order<Owner[K]>): Order.Order<Owner> => Order.mapInput(O, (o) => o[k])

const ranked: Order.Order<Owner> = Order.combineAll([byField('tier', Order.number), byField('rank', Order.number)])
const reordered: Order.Order<Owner> = Order.combineAll([byField('rank', Order.number), byField('tier', Order.number)])
const same: Equivalence.Equivalence<Owner> = Equivalence.combineAll([
    Equivalence.mapInput(Equal.equivalence<string>(), (o: Owner) => o.tag),
    Equivalence.mapInput(Equivalence.number, (o: Owner) => o.rank),
])
const diverge: boolean = ranked({ rank: 1, tier: 2, tag: 'a' }, { rank: 2, tier: 1, tag: 'a' }) === reordered({ rank: 1, tier: 2, tag: 'a' }, { rank: 2, tier: 1, tag: 'a' })
```

[IDEMPOTENCE_IS_THE_MULTIPLICITY_LICENSE_AND_XOR_IS_ITS_PRECISE_VIOLATION]:
- Idempotence is the one license a duplicated row in a fold cashes: under `MonoidEvery`/`MonoidSome`/extremum a predicate or value appearing twice in the `combineAll` collection is HARMLESS (`x ∧ x = x`, `max(x, x) = x`), so a validator collection accreting redundant rows, a maxima fold over a multiset with repeats, and a deduplication-blind pipeline all produce the same answer the deduplicated input would — the fold is multiplicity-insensitive by the leaf's own algebra, never by a guarding pass.
- `MonoidXor` and `MonoidEqv` are the surgical counterexamples that make the license checkable: both are associative AND commutative (so reorder-safe) but NOT idempotent (`x ⊻ x = false`, `x ↔ x = true` flips on the SECOND occurrence), so `MonoidXor.combineAll` over a multiset counts PARITY — every value appearing an even number of times cancels to the identity. The reorder-safe column and the dedup-safe column DIVERGE on exactly these two rows: a refactor that "removes a duplicate predicate for clarity" is sound under `MonoidEvery` and corrupts the result under `MonoidXor`, and the only difference is the idempotence cell.
- The hazard is the any-pass/parity confusion: reaching for `MonoidXor` where `MonoidSome` (∨) was meant passes every single-element and every distinct-element test, because XOR and OR agree until a value repeats — the parity collapse surfaces only on a multiset with even multiplicity, the input a small fixture never carries, so the missing idempotence is invisible until the fold runs over duplicated production data.

```typescript
import * as B from '@effect/typeclass/data/Boolean'

const flags: ReadonlyArray<boolean> = [true, true, false]
const anyPass: boolean = B.MonoidSome.combineAll(flags)
const parity: boolean = B.MonoidXor.combineAll(flags)
const dedupAnyPass: boolean = B.MonoidSome.combineAll([true, false])
const dedupParity: boolean = B.MonoidXor.combineAll([true, false])
const idempotentAgrees: boolean = anyPass === dedupAnyPass
const parityDiverges: boolean = parity !== dedupParity
```

[REVERSE_IS_A_COMMUTATIVITY_PROBE_AND_INTERCALATE_PROVES_THE_CELLS_ARE_INDEPENDENT]:
- `Semigroup.reverse(S)` is a runtime commutativity probe, not merely a dual: `reverse(S)` differs OBSERVABLY from `S` iff `S` is non-commutative, so `combine(reverse(S)(a, b), S(a, b))`-style comparison detects the property the witness shape cannot record — over a reorder-safe instance the flip is the identity transform (`reverse(MonoidSum) = MonoidSum`), so reaching for `reverse` there is a no-op the checker permits but the algebra empties of meaning, and the only rows where `reverse` carries information are exactly the non-commutative ones the predictor flags (`String.Monoid`, `Semigroup.last`, an order-sensitive struct column). `reverse(reverse(S)) = S` is the involution proving the dual re-sequences accumulation rather than defining a new algebra.
- `Semigroup.intercalate(S, sep)` injects `sep` between every pair (`combineMany(self, [sep, that])`) and is the ONE combinator the library refuses to give a `Monoid` counterpart: a separating combine has no two-sided identity because any candidate unit forces a leading separator, so the absent `Monoid.intercalate` export is the strength boundary "associative, NO unit" made a compile fact. Intercalation is associative but neither commutative nor idempotent nor identity-bearing — the exact algebraic slot proving the four laws are independent: an instance can occupy any subset of the four cells, and `intercalate` is the witness that the bottom-right (identity) cell can be empty while associativity holds.
- The dual's bulk fold is the law-coherence check made executable: `reverse`'s custom `combineMany` reverses the collection then appends `self` LAST — the mirror of the forward seed-first fold — and it equals the flipped forward fold ONLY because associativity lets the reassociated reverse-traversal agree; a non-associative `combine` makes the reverse fold and the flipped forward fold diverge, the same default-vs-custom divergence test one rung up.

[THE_SEMIGROUP_MONOID_SPLIT_IS_THE_IDENTITY_LICENSE_MADE_A_MISSING_EXPORT]:
- The empty-case license is the only one of the four that the type system CAN enforce, because identity is the one law that lives in the export shape rather than the unchecked premise: `combineAll([])` requires a two-sided identity and exists only on `Monoid`, so a witness that cannot lawfully supply a unit ships as a `Semigroup`-only export and the empty fold is foreclosed at the type level — `combineMany` (non-empty, seeded by `self`) is the rung where no identity is needed and `combineAll` (empty-admitting) is the rung where it is, the same boundary across every owner.
- The unbounded owner is the cleanest demonstration: `BigInt` ships `SemigroupMin`/`SemigroupMax` but NO `MonoidMin`/`MonoidMax` and NO `Bounded`, because an unbounded order has no losing bound to seed the extremum identity, so a `combineAll`-shaped maxima fold over `bigint` is a missing-export compile fact while the same fold over `number` (whose `Bounded` ships `±Infinity`) is open — the identity license is denied by absence, never by a runtime sentinel a consumer must guard.
- `getSemigroupIntersection` over `data/Record` is the keyed analogue: record union HAS an identity (`{}` passes through every key) so `getMonoidUnion` exists, while record INTERSECTION has none (the would-be unit must contain every possible key to leave all survivors, an absorbing element not a neutral one) so only `getSemigroupUnion`'s sibling `getSemigroupIntersection` ships — the set-algebra of the merge dictates whether the empty-fold license is issuable, and the merge that lacks a neutral surfaces as the missing monoid factory.

```typescript
import { getMonoidUnion, getSemigroupIntersection } from '@effect/typeclass/data/Record'
import * as N from '@effect/typeclass/data/Number'

const overlay = getMonoidUnion(N.MonoidSum)
const shared = getSemigroupIntersection(N.MonoidSum)
const merged = overlay.combineAll([{ a: 1, b: 2 }, { b: 3, c: 5 }, overlay.empty])
const common = shared.combine({ a: 1, b: 4 }, { b: 6, c: 9 })
```

[NON_COMMUTATIVE_KEYED_MERGE_RE_INTRODUCES_THE_ORDER_FORK_INSIDE_A_COMMUTATIVE_SHELL]:
- `getMonoidUnion(M)`'s STRUCTURE (which keys survive) is commutative — union is symmetric on key sets — but its VALUE merge inherits the commutativity of the supplied `M`: feed a commutative value witness (`MonoidSum`, `MonoidMax`) and the whole keyed fold is reorder-safe; feed a non-commutative value witness (`Semigroup.last` lifted, a `String.Monoid` per-key concatenation) and the SHARED-key cells become order-sensitive while the disjoint-key cells stay free — so a single keyed accumulator can be reorder-safe on most keys and order-dependent on the colliding ones, the fork hiding inside a structurally symmetric merge.
- The predictor composes by the WEAKEST contributing law, never the merge's own: a struct or record fold is reorder-safe iff EVERY column's value algebra is commutative, dedup-safe iff every column is idempotent, and empty-foldable iff every column carries a unit — one non-commutative column (a `Semigroup.last` discriminant that must keep the latest value) poisons the whole record's reorder license even though the surrounding `MonoidSum` columns are commutative, so the composite's law-profile is the field-wise MINIMUM of its columns' profiles.
- This is why a discriminant frozen by `Semigroup.first`/`last` forces the whole struct to a `Semigroup`: those columns are unit-LESS (a side-projection has no neutral), so a struct containing one breaks the empty-fold license for the entire record — the missing column identity surfaces as the inability to build `Monoid.struct`, the composite law-profile demoting the strongest fold the record admits down to the weakest column's rung.

[THE_FOUR_LICENSES_MAP_ONE_TO_ONE_ONTO_THE_FOUR_FOLD_EXECUTION_STRATEGIES]:
- Each license unlocks exactly one execution rewrite the consumer may apply to a `combineAll`/`combineMany`, and the mapping is the operational payoff of reading the law-profile: associativity unlocks TREE/PARALLEL evaluation (any parenthesization, so chunked or concurrent reduction is legal); commutativity unlocks UNORDERED ingestion (a `Set` or hash-grouped stream folds without preserving arrival order); idempotence unlocks AT-LEAST-ONCE delivery tolerance (a duplicated message in the fold is absorbed, so retries are safe); identity unlocks EMPTY-and-CHUNK-BOUNDARY seeding (a parallel fold seeds each partition with `empty` and the partition results combine).
- A parallel/chunked `combineAll` therefore needs BOTH associativity (to split) AND identity (to seed each chunk's accumulator), which is exactly the `Monoid` contract — `combineAll` over a partitioned collection seeds every partition with `M.empty`, folds each, then combines the partial results, lawful iff `empty` is two-sided and `combine` associative, and the partition-result combine being order-free additionally needs commutativity if partitions complete out of order. So an unbounded-concurrency `getApplicative`-lifted accumulation is reorder-safe ONLY when the inner value monoid is commutative, the concurrency policy and the value law being two independent gates on the same fold.
- The hazard is cashing a parallelization license the value algebra never issued: lifting `String.Monoid` (associative, NON-commutative) through an unbounded-concurrency applicative and folding produces a concatenation whose chunk-completion order scrambles the result, because the parallel partition-combine reorders the chunks and concatenation is order-sensitive — the type checks (a `Monoid<Effect<string>>` is well-formed), the fold runs, and the string is assembled in nondeterministic order, the non-commutativity surfacing only under the concurrency the applicative factory admitted.

```typescript
import { Applicative } from '@effect/typeclass'
import * as EffectInstances from '@effect/typeclass/data/Effect'
import * as N from '@effect/typeclass/data/Number'
import { Effect } from 'effect'

const Reducible = Applicative.getMonoid(EffectInstances.getApplicative({ concurrency: 'unbounded' }))<number, never, never, never>(N.MonoidMax)
const peak: Effect.Effect<number> = Reducible.combineAll([Effect.succeed(2), Effect.succeed(9), Reducible.empty])
```

[THE_TIE_BREAK_IS_AN_IDENTITY_SELECTION_THE_FOUR_LAWS_DO_NOT_CONSTRAIN]:
- Both extremum algebras are associative, commutative, AND idempotent, so the four-law profile predicts FULL reorder/dedup/parallel safety of the extremum VALUE — yet the surviving IDENTITY of an order-tied pair is order-dependent and the law table is SILENT on it, because the laws constrain the result up to the order's equivalence, not up to structural identity. The fork the predictor cannot resolve: `Semigroup.min(O)` keeps the RIGHTMOST tied element (strict `O(self,that) === -1 ? self : that`) while `Order.min(O)` keeps the LEFTMOST (`O(self,that) < 1 ? self : that` plus a reference fast-path), so the two surfaces agree on the extremum value and disagree on which tied record carries it.
- This is harmless for a total order over a primitive (order-equal means value-equal, so identity selection is moot) and load-bearing the instant the order is BY PROJECTION: `Order.mapInput(Order.number, (r) => r.priority)` ties every record sharing a priority, and a maxima fold's surviving record now depends on which surface owns the tie-break and on the collection order — a reorder that the four-law profile certified safe FOR THE VALUE silently changes WHICH record survives, the one place reorder-safety of the value and reorder-safety of the identity diverge.
- The predictor's boundary is therefore stated precisely: the four-law table governs reorder/dedup/parallel safety of the COMBINED VALUE up to the algebra's own equivalence, and any consumer that reads structural identity off an extremum result (which tied record, which `Some` won a first-success fold) inherits a tie-break policy the laws never promised — that policy is the witness's documented behavior, not a derivable consequence, so it must be pinned by choosing the surface (`Semigroup.min` vs `Order.min`) deliberately, never assumed from the law-profile.
