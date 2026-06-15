# Behavior Column As Instance Source

[THE_COLUMN_IS_THE_PULLBACK_ARGUMENT]:
- `mapInput` is implemented `make((b1, b2) => self(f(b1), f(b2)))` — the projection `f` is pre-composed onto BOTH arguments of the base instance, so `Order.mapInput(Order.number, (k) => Table[k].rank)` does not write a comparator, it pulls `number`-comparison back along the rank read; the derived `Order<K>` runs `Order.number(Table[a].rank, Table[b].rank)` and nothing else.
- The lever is one contravariant functor `(b) => a` shared by both instances with identical body shape — `Order.mapInput<A,B>(self: Order<A>, f: (b: B) => A): Order<B>` and `Equivalence.mapInput<A,B>(self, f): Equivalence<B>` — so the rank-column read `(k) => Table[k].rank` builds the key-level Order and the group-column read `(k) => Table[k].group` builds the key-level Equivalence from the SAME map, never two hand-rolled comparators.
- The library's own canonical instances are `mapInput` projections: `Order.Date = mapInput(Order.number, (d) => d.getTime())` and `Equivalence.Date = mapInput(Equivalence.number, (d) => d.getTime())`, so projecting a vocabulary column to an instance is the same construction the package applies to its primitives — the column-to-instance lift is not a vocabulary-table idiom but the standard instance-derivation shape, reached for in place of `Order.make`/`Equivalence.make`.
- `mapInput` composes by function composition with zero instance algebra: `Order.mapInput(Order.mapInput(Order.number, g), f)` equals `Order.mapInput(Order.number, (x) => g(f(x)))`, so reading a column THROUGH a derived column — rank-of-the-group-leader, weight-after-a-scaling — is one composed projection over one base instance, never a chain of intermediate `Order<intermediate>` owners.

```typescript
import { Order, Equivalence } from 'effect'

const TABLE = {
    alpha: { rank: 0, group: '<group-a>' },
    beta: { rank: 1, group: '<group-a>' },
    gamma: { rank: 2, group: '<group-b>' },
} as const satisfies Record<string, { readonly rank: number; readonly group: string }>

const byRank = Order.mapInput(Order.number, (k: keyof typeof TABLE) => TABLE[k].rank) // pulls number-compare back along the rank read; no comparator written
const sameGroup = Equivalence.mapInput(Equivalence.string, (k: keyof typeof TABLE) => TABLE[k].group) // same (b)=>a lever, equality side; the column read IS the algebra
```

[COMBINE_IS_A_MONOID_FOLD_OVER_COLUMNS]:
- `Order<K>` is a Monoid under `combine`/`empty`: `combine(self, that)` returns the left-most non-zero `Ordering` (`self` first, `that` only on a tie), `empty()` is `make(() => 0)` (everything equal — the identity that contributes no ordering), and `combineAll = combineMany(empty(), collection)`, so a multi-column key order is the monoidal fold of single-column orders, not a comparator with nested conditionals.
- The fold makes the column the only source of the composite order: `Order.combineAll([Order.mapInput(Order.number, byRank), Order.mapInput(Order.string, byLabel)])` lexicographically orders by rank then label, and prepending a tie-break column is one more element in the folded collection — the comparator never reopens because `combine` carries the priority and `empty` carries the no-op identity.
- The identity-laden fold is what deletes the empty-case branch: `Order.combineAll([])` is `empty()` (all-equal), so a dynamically-assembled sort key — a runtime-chosen subset of columns folded together — degrades gracefully to a stable no-op order with zero rows folded, never an `undefined` comparator or a guarded "no sort columns" branch.
- `combineMany(primary, [secondary, tertiary])` is the same fold seeded with a designated primary, so a vocabulary's intrinsic order (its rank column) seeds the fold and presentation tie-breaks (label, then key string) append as the collection — the primary instance is the seed, the secondary columns the folded tail, and the precedence is the collection order, never re-encoded per pair.

[ORDER_AND_EQUIVALENCE_COMBINE_ON_DUAL_LATTICES]:
- The two instances are monoids whose `combine` operate on DUAL lattices, and the asymmetry is load-bearing: `Order.combine` is left-biased lexicographic priority (the first non-zero column decides), while `Equivalence.combine` is conjunctive — `make((x, y) => self(x, y) && that(x, y))` — so combining order columns REFINES precedence and combining equivalence columns REFINES the partition (two keys equivalent only when they agree on EVERY combined column).
- The identities mirror the duality: `Order.empty()` contributes no ordering (everything equal, the fold's neutral), while `Equivalence.combineAll([])` is `isAlwaysEquivalent` (one block, the coarsest partition) — so folding more order columns can only break more ties (finer order) and folding more equivalence columns can only split more blocks (finer partition), each monoid moving monotonically toward finer in its own lattice.
- This is why a multi-column sort and a multi-column grouping read the same table columns through opposite combine semantics: `Order.combineAll([byRank, byLabel])` makes rank dominate and label subordinate, but `Equivalence.combineAll([sameGroup, sameTier])` requires BOTH group and tier to match — the order fold is an ordered priority list, the equivalence fold is an unordered conjunction, and a column added to each tightens its instance in the dual direction.
- The consequence for the row contract: an order over a vocabulary needs a total preorder on every combined column (a `number`/`string`/`boolean` column with a base `Order`), while an equivalence needs only a decidable equality per column, so a boolean `halts` column is a legitimate first sort key (false before true under `Order.boolean`) AND a legitimate equivalence partition (group by halting), the same column feeding both folds with no second declaration.

```typescript
import { Order, Equivalence, Array as Arr } from 'effect'

const TABLE = {
    a: { rank: 0, halts: false, group: '<group-a>' },
    b: { rank: 1, halts: true, group: '<group-a>' },
    c: { rank: 2, halts: true, group: '<group-b>' },
} as const satisfies Record<string, { readonly rank: number; readonly halts: boolean; readonly group: string }>

type Key = keyof typeof TABLE
const halting = Order.combineAll<Key>([ // lexicographic: halts (false<true) dominates, rank breaks the tie; prepend a column = one more fold element
    Order.mapInput(Order.boolean, (k: Key) => TABLE[k].halts),
    Order.mapInput(Order.number, (k: Key) => TABLE[k].rank),
])
const congruent = Equivalence.combineAll<Key>([ // conjunctive: equivalent only when BOTH group AND halts agree; adding a column splits more blocks
    Equivalence.mapInput(Equivalence.string, (k: Key) => TABLE[k].group),
    Equivalence.mapInput(Equivalence.boolean, (k: Key) => TABLE[k].halts),
])
const sorted: ReadonlyArray<Key> = Arr.sort(Arr.fromIterable<Key>(['c', 'a', 'b']), halting)
const distinct: ReadonlyArray<Key> = Arr.dedupeWith(['a', 'b', 'c'], congruent) // dedupeWith's (a,b)=>boolean param IS the Equivalence callable; no adapter
```

[STRUCT_INSTANCE_SHARES_THE_ROW_READ_THE_COMBINE_CHAIN_REPEATS_IT]:
- `Struct.getOrder({ col: Order.X })` and the `combineAll` of per-column `mapInput`s are observationally identical orderings that differ in projection ARITY, and the difference decides the owner: `getOrder` reads every column from one already-projected ROW value, so lifting it to a key order is ONE pullback `Order.mapInput(getOrder({...}), (k) => Table[k])` that reads `Table[k]` once and the struct walks its columns; the `combineAll` chain reads each column from `K` directly, so it applies a separate `(k) => Table[k].col` projection PER column — the same key order, one row read versus N.
- The struct form is the denser owner when three-plus columns of one row participate (the row read is shared, the struct instance reusable over both raw rows and projected keys), the `combineAll` chain when the columns come from heterogeneous sources no single row value covers — a column from the table plus a column from a sibling vocabulary fold into one `combineAll` where `getOrder` would require both to live in one struct.
- The struct instance is itself a Monoid element: a row order built by `getOrder` combines with another row order under `Order.combine` and reverses under `Order.reverse`, so a base row order (intrinsic columns) and an overlay row order (presentation columns) fold into one row instance before the single pullback to the key — the composition happens at the row altitude, the key altitude inherits it through the one `mapInput`.
- The struct instance preserves the row's literal column types — `getOrder({ rank: Order.number })` over rows typed `{ readonly rank: 0 | 1 | 2 }` compares the frozen leaf — so a new table row extends the struct order's input domain and, through the single pullback, the key order's projection domain at once: the proof of the correct owner is the next row landing with no instance, no struct field, and no fold element touched.

```typescript
import { Order, Struct } from 'effect'

const TABLE = {
    a: { rank: 0, label: '<label-a>' },
    b: { rank: 1, label: '<label-b>' },
} as const satisfies Record<string, { readonly rank: number; readonly label: string }>

const rowOrder = Struct.getOrder({ rank: Order.number, label: Order.string }) // composes per-column instances once over the row shape
const keyOrder = Order.mapInput(rowOrder, (k: keyof typeof TABLE) => TABLE[k]) // ONE more pullback lifts the row order to a key order; no recomposition
```

[THE_INSTANCE_NEVER_REFUSES_A_WIDENED_COLUMN]:
- The column-as-instance read is the one consumer the leaf-axis defeat passes ENTIRELY silently: `Order.number` accepts any `number` and `Equivalence.string` any `string`, so `Order.mapInput(Order.number, (k) => Table[k].rank)` type-checks against a frozen literal column and a re-widened primitive column identically — the instance never refuses the wider column, it merely stops proving the row set, which is why the instance site is a worse audit point than an indexed read that at least lies and a `Match.exhaustive` that at least breaks.
- The audit that the instance still totalizes the vocabulary is therefore never whether the `mapInput` compiles but whether the projected column reads as a literal union — `(typeof Table)[keyof typeof Table]['rank']` a closed union, not `number` — so an instance derived from a vocabulary carries a hidden precondition the consumer cannot see at the instance and must check at the table: the base `Order` instance is total, the projection is total only while the leaf is.
- The key-axis dual hits the instance's INPUT domain: a derived `Order<K>` is total over the row set only when `K` is the closed key union, so an instance built over a key-erased binding becomes an `Order<string>` admitting keys the table never declared — the instance is built over the literal-key binding, never a reconstructed record whose key union has already collapsed, because the projection's input type IS the membership claim the instance makes.
- The `const`-type-parameter lever that preserves both axes at a consuming surface is also the only way to derive an instance generically without forcing every caller to re-freeze: an entrypoint `<const Row extends Record<string, { rank: number }>>(rows: Row)` that builds `Order.mapInput(Order.number, (k) => rows[k].rank)` internally derives an instance over the caller's frozen leaf and closed key union, so a reusable instance factory over an unknown vocabulary declares `const` once and the projection inherits the preservation rather than each call site re-asserting it.

[CONSUMERS_READ_ONE_INSTANCE_NOT_A_VARIADIC]:
- The collection combinators consume the folded instance directly as one value: `Array.sort(self, Order.combineAll([...]))`, `Array.min(self, order)`, `Array.max(self, order)`, `Order.clamp(order)({minimum, maximum})`, and `Order.between(order)({minimum, maximum})` all take a single `Order<K>`, so the multi-column key order folded once threads through sorting, extremal selection, clamping, and range testing from one definition site — never a comparator re-spelled per consumer.
- `Array.sortBy(...orders)` is the variadic FACE of the same fold and the trap is conflating it with `combineAll`: `sortBy` takes a rest list of `Order<A>` and applies them as successive sort keys, which is the same lexicographic result as sorting once by `combineAll(orders)`, but the folded instance is a reusable named owner shared across `min`/`max`/`clamp` while the `sortBy` spread is consumed inline by one sort — prefer the folded `Order<K>` owner when more than one consumer reads the precedence, the `sortBy` spread only for a one-shot sort.
- `Array.dedupeWith` and `Array.groupBy` consume the equivalence side: `dedupeWith` takes the `(a, b) => boolean` shape an `Equivalence<K>` IS (the instance is callable as that signature, no adapter), so the conjunctive `combineAll` equivalence dedupes on the multi-column partition directly, while `groupBy` keys on a `string`-returning projection — a multi-column GROUP partitions through `dedupeWith` over the equivalence, a single-column group through `groupBy` over the projection, the partition column the same in both.
- `Order.min`/`Order.max`/`Order.clamp`/`Order.lessThan` derive boolean and selection operations from the one folded instance, so the vocabulary's column-derived order yields not just a sort but the entire comparison surface — extremal row, in-range predicate, clamp-to-range — from one fold, and a new vocabulary row extends every one of those operations with zero edits because each reads the instance, never an inlined comparison.

```typescript
import { Order, Array as Arr } from 'effect'

const TABLE = {
    low: { rank: 0, label: '<label-a>' },
    mid: { rank: 1, label: '<label-b>' },
    high: { rank: 2, label: '<label-c>' },
} as const satisfies Record<string, { readonly rank: number; readonly label: string }>

type Key = keyof typeof TABLE
const byRank = Order.combineAll<Key>([Order.mapInput(Order.number, (k: Key) => TABLE[k].rank), Order.mapInput(Order.string, (k: Key) => TABLE[k].label)])
const ceiling: Key = Arr.max(['low', 'high', 'mid'] as const, byRank) // 'high'; one folded instance, not a re-spelled comparator
const within: boolean = Order.between(byRank)({ minimum: 'low', maximum: 'high' })('mid') // same instance drives the range predicate
const ordered: ReadonlyArray<Key> = Arr.sort(['high', 'low', 'mid'] as const, byRank) // REJECT Arr.sortBy(self, o1, o2) when min/between also read the precedence — fold once, share the owner
```

[REVERSE_IS_A_DUAL_NOT_A_RECOMPUTE]:
- `Order.reverse(O) = make((self, that) => O(that, self))` swaps the comparison arguments rather than recomputing, so reversing a folded multi-column order — `Order.reverse(Order.combineAll([byRank, byLabel]))` — flips the WHOLE lexicographic precedence (descending rank, then descending label) in one wrap, never a parallel descending comparator with re-spelled columns.
- Reverse distributes over the fold but not commutatively with combine: `Order.reverse(Order.combine(a, b))` descends by `a` then `b`, while `Order.combine(Order.reverse(a), b)` descends by `a` but ascends by `b` on the tie — so a mixed-direction sort (descending rank, ascending label) folds per-column reversed instances, `Order.combineAll([Order.reverse(byRank), byLabel])`, the direction a per-column choice inside the fold, never a single outer reverse.
- The equivalence side has no `reverse` because a partition has no direction — equivalence is symmetric by construction (`make` carries the reflexive `self === that ||` short-circuit) — so the order/equivalence duality breaks exactly at `reverse`: the order monoid lives on a directed lattice that `reverse` inverts, the equivalence monoid on an undirected partition lattice with no inverse, which is why a column drives a reversible sort and an irreversible grouping from the one read.
