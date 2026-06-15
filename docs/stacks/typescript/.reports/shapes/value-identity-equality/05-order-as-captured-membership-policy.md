# Order As Captured Membership Policy

[ORD_IS_FROZEN_AT_CONSTRUCTION]:
- `RedBlackTree.empty(ord)`, `SortedSet.empty(O)`, and `SortedMap.empty(ord)` each call `makeImpl`/`fromTree` which writes the comparator into a private `_ord` field once and never reads it from an argument again — `getOrder(tree)` returns `tree._ord` verbatim, so the `Order` supplied to the constructor is the structure's permanent membership-and-key law, not a per-operation parameter; there is no `add(self, value, order)` overload anywhere in the surface, the policy is recovered from the receiver alone.
- Every mutating return reconstructs through `makeImpl(self._ord, newRoot)`, so the captured comparator survives every `insert`/`remove`/`filter`/`map` as a structural invariant threaded by the structure, never re-passed by the caller — the one place a second `Order` enters (`SortedSet.map(self, O, f)`, `flatMap(self, O, f)`) is where a NEW structure is born and the new image's key policy is declared fresh, the old policy explicitly abandoned because the projected values no longer inhabit the source order's domain.
- The capture is the construction-time analogue of `DECLARATION_COLLAPSE`: the comparator that decides key identity, sort order, and bounded-range membership is one declaration the structure absorbs, so a sorted owner is `SortedSet.empty(ranked)` where `ranked` is the `Order.combine`/`Order.struct` table the value owner already declares — never an `Order` reconstructed at each `add` site, and never a comparator passed to `has` the way `Array.containsWith` takes one per call.

```typescript
import { Order, SortedSet } from 'effect';

type Row = { readonly key: string; readonly rank: number; readonly note: string };

// the captured Order IS the membership law: key precedence, then rank tiebreak — frozen at empty(),
// re-read by every subsequent add/has/remove, never re-passed
const policy: Order.Order<Row> = Order.combine(
    Order.mapInput(Order.string, (r: Row) => r.key),
    Order.mapInput(Order.number, (r: Row) => r.rank),
);
const seed: SortedSet.SortedSet<Row> = SortedSet.empty(policy);
const filled: SortedSet.SortedSet<Row> = SortedSet.fromIterable(policy)([
    { key: '<a>', rank: 1, note: '<n1>' },
    { key: '<a>', rank: 1, note: '<n2>' },
]);
// note differs yet the captured policy ties them at (key,rank) === 0: the second is the silent merge
const merged: number = SortedSet.size(filled);
```

[ROUTE_BY_ORDER_HIT_BY_EQUAL_IS_THE_DESCENT_SPLIT]:
- `findFirst` descends by the captured comparator (`const d = cmp(key, node.key)`, `d <= 0` steps left, `d > 0` steps right) but settles the hit by a SEPARATE test — `Equal.equals(key, node.key)` returns the value, so the `Order` is the navigator and the structural `Equal` is the acceptor; two distinct gates run at every visited node and they answer different questions, position versus identity.
- The split is silently lossy exactly where the captured `Order` is coarser than `Equal.equals`: a probe comparing `0` against a stored node but structurally unequal to it routes LEFT (`d <= 0` takes the left branch) past that node and continues, so a value that the comparator considers equal-positioned yet `Equal.equals` rejects is unreachable through `findFirst` — the descent walks past its only `0`-comparing neighbor and lands on `Option.none()`, reporting absent a value the tree may genuinely contain on a different branch the routing never visits.
- The same route-then-accept split governs `removeFirst`: it guards on `has(self, key)` (itself `findFirst`), then re-descends with `Equal.equals(key, node.key)` as the stop condition while `d <= 0`/`d > 0` steers — so removal locates by structural identity but can only reach the node the Order routing exposes, and a key whose `Order`-position collides with a non-equal sibling on the wrong branch is both unfound and unremovable. The hashed-structure invariant "route by hash, settle by Equal" has a sorted-structure twin "route by Order, settle by Equal", and the sorted twin's router is a TOTAL ORDER whose coarseness merges where the hash would merely collide.

[FINDFIRST_VERSUS_FINDALL_IS_THE_TIE_SCOPE]:
- `findFirst` is `Order`-guided single-hit and inherits the routing-coarseness blindness, while `findAll` abandons routing entirely — it performs a FULL in-order traversal (`stack` walk over every node, left-then-self-then-right) collecting every node satisfying `Equal.equals(key, current.key)` — so the multi-match path visits all `O(n)` nodes and is immune to the route-past hazard, at the cost of never using the captured order to prune.
- This is the structural witness that the bare `RedBlackTree` is a MULTIMAP: `insert` appends a fresh node on every call (the descent ends at a leaf and a new red node is pushed, with NO equal-key replacement), so distinct insertions of `Order`-tied keys coexist as separate nodes and `findAll` is the only retrieval honoring that multiplicity — `findFirst` returns whichever single tied node the routing reaches first, a representative the insertion order and rebalancing rotations jointly determine, not the caller.
- The dedup the application layer expects lives ONE layer up, never in the tree: `SortedSet.add` and `SortedMap.set` impose single-occupancy by consulting `RBT.has` before inserting, so the merge-on-tie semantics are a property of the set/map wrapper, and dropping to the raw `RedBlackTree` to gain `findAll`/`getAt` positional access silently forfeits the dedup — the same captured `Order` yields multiset semantics at the tree and set semantics at the wrapper, the discriminant being which constructor the value flowed through.

[THE_ZERO_COMPARISON_IS_THE_KEY_MERGE]:
- `SortedSet.add` is `RBT.has(self.keyTree, value) ? self : insert(...)` and `SortedMap.set` is `RBT.has(self.tree, key) ? insert(removeFirst(self.tree, key), key, value) : insert(...)` — both gate on `has`, which is `findFirst` reporting `Some`, so the membership question every `add`/`set` answers is "does a node already compare-and-`Equal`-match this key", and a `value` whose captured `Order` returns `0` against a stored node is NOT auto-merged unless it also passes `Equal.equals` — the merge fires at the conjunction of order-tie AND structural-equality, never on order-tie alone.
- The genuine collapse surfaces when the captured `Order` is coarser than the value's structural `Equal` AND the routing reaches the tied node: `set` then does `insert(removeFirst(tree, key), key, value)` — it evicts the structurally-equal incumbent and reinserts under the new value, so a `SortedMap` keyed by `Order.mapInput(Order.string, byField)` over a record whose other fields differ collapses two distinct records to one entry the moment they share the projected field, last-write-wins, with the evicted record's payload silently gone — the cardinality of a sorted map is a direct readout of its captured comparator's discriminating power, never of the distinct values fed to it.
- The merge is asymmetric to the hashed-structure twin in WHAT it loses: `HashMap` routes by hash then settles by `Equal`, so a coarse hash over-collides into longer buckets but never merges distinct-by-`Equal` keys; a coarse `Order` whose `0` arm a routed node hits IS the merge, because the sorted structure has no within-bucket disambiguation past the comparator — the `Order` is simultaneously the router and the equality, so its coarseness is a correctness loss the hash's coarseness (a mere complexity loss) never incurs. A sorted key MUST be as discriminating as the identity the application wants preserved, or distinct values vanish.

```typescript
import { Order, SortedMap } from 'effect';

type Entry = { readonly id: string; readonly version: number; readonly body: string };

// the captured Order keys on id ALONE: two versions of one id are Order-tied (=== 0) and Equal-distinct,
// so set evicts-and-replaces — last write wins, the prior body is silently dropped
const byId: Order.Order<string> = Order.string;
const store: SortedMap.SortedMap<string, Entry> = SortedMap.fromIterable(byId)([
    ['<x>', { id: '<x>', version: 1, body: '<v1>' }],
    ['<x>', { id: '<x>', version: 2, body: '<v2>' }],
]);
const survivors: number = SortedMap.size(store);
const kept: Order.Order<string> = SortedMap.getOrder(store);
```

[REBALANCING_RESTS_ON_TOTALITY_OF_THE_CAPTURED_ORDER]:
- `insert` records the sign `d` of every `cmp(key, n.key)` along the descent into a `d_stack`, then rebuilds the path and runs the red-black rotation loop (`p.color`/`n3.color` checks, left/right rotations, `recount`) — the rotations preserve the binary-search invariant ONLY if the comparator that produced `d_stack` is a consistent total order: trichotomous, antisymmetric, and transitive across every pair the tree will ever compare. The structure trusts the captured `Order` to be lawful and discharges none of those laws itself.
- A comparator that violates transitivity (`O(a,b)<0` and `O(b,c)<0` yet `O(a,c)>0`) corrupts the search invariant silently: the rotation loop rebalances colors and heights against routing decisions that no longer form a consistent order, so a later `findFirst` routes by the SAME broken comparator and may step away from a node whose subtree holds the key — the failure is not an exception or a height violation the tree detects, it is a key that is present in the materialized iteration yet unreachable by lookup, the deepest form of the route-past hazard scaled to the whole structure.
- This is why `Order.make`'s constructor-level reference gate and the `-1 | 0 | 1` codomain matter at the sorted-structure layer specifically: a hand-rolled comparator returning raw subtraction (`a.rank - b.rank`) instead of a clamped sign feeds the `d <= 0` branch an unbounded integer whose sign is correct but whose magnitude is irrelevant — tolerable here because `insert` reads only `d <= 0`, yet the moment the same comparator is `Order.reverse`d or `combine`d the fold reads `out !== 0` and an `NaN`-producing subtraction (a non-total comparator on `NaN` fields) makes `out !== 0` true on a tie, routing inconsistently and poisoning the invariant. The captured `Order` must be a genuine total order over the FULL inhabited key domain, `NaN` keys included, or the rebalancing builds a tree whose shape contradicts its own routing.

[BOUNDED_RANGE_TRAVERSAL_IS_PURE_ORDER_NO_EQUAL]:
- `greaterThanEqual`, `lessThan`, `between`, and the `forEach*` range walkers descend by `cmp(key, node.key)` alone and NEVER consult `Equal.equals` — they collect every node on the correct side of the bound by recording the last node where `d <= 0` held and truncating the stack there, so a range query is membership-by-order-POSITION, a strictly coarser question than the `findFirst` route-then-settle and a strictly different one than structural identity.
- The consequence is that range membership and point membership disagree on the same captured `Order`: a value structurally absent from the tree (no node `Equal.equals` it) is still INSIDE a `between(lo, hi)` span if its order-position falls in the interval — the iterator yields the order-neighbors, not the probe, so `between` answers "which stored keys occupy this order-interval" while `findFirst` answers "is this exact key stored", and conflating them treats an order-neighborhood as identity. The captured `Order` is the sole authority for range queries with no identity escape hatch, so the coarser the comparator the broader and less identity-meaningful every range slice becomes.
- `getAt(tree, i)` and `at(tree, i)` index by the in-order RANK the captured order imposes, not by any key — positional access is a pure readout of the order the tree materializes, so a structure built under a coarse merging `Order` has fewer ranks than distinct values fed to it, and `getAt` over that compressed rank space returns the surviving representative of each merge class, never the merged-away siblings.

[CROSS_STRUCTURE_UNION_RE_KEYS_UNDER_THE_RECEIVER]:
- `SortedSet.union(self, that)` opens `const ord = RBT.getOrder(self.keyTree)`, builds `empty(ord)`, then folds `add` over `self` AND over `that` — so every element of the second operand is re-admitted through the RECEIVER's captured comparator and its own captured `Order` is discarded entirely; `intersection` and `difference` are the same shape (`empty(self._ord)` seeded, `that` re-judged by `self`'s `has`/`remove`), and `isSubset(self, that)` tests `every(self, a => has(that, a))` so each membership re-routes through `that`'s comparator. The binary operators take `that` as a value source, never as a co-equal policy carrier.
- This is the cross-structure trap the types not only fail to catch but actively conceal: `union`, `intersection`, and `difference` are typed `(self: SortedSet<A>, that: Iterable<A>) => SortedSet<A>` — the second operand is a bare `Iterable`, so the type system has ALREADY erased whatever captured comparator `that` carried before the call ever runs, and a `SortedSet` flows in as a mere element source whose policy is structurally invisible to the receiver; `isSubset` alone tightens `that` to `SortedSet<A>`, yet still re-routes every `has` through `self`'s comparator. Unioning a set built under `Order.string` with one built under `Order.mapInput(Order.string, normalize)` re-admits the second set's elements through the first's `add`, so two elements the second kept distinct collapse to one in the result, and the result's cardinality is governed entirely by `self._ord` — a fact the `Iterable` parameter type guarantees can never appear in the signature.
- The asymmetry makes `union(a, b)` and `union(b, a)` produce structurally different results whenever the two comparators disagree on any tie — not merely different iteration orders but different MEMBERSHIPS, because each re-keys the other under its own policy — so the set algebra is non-commutative in cardinality exactly when the operands' captured orders diverge, the membership trap no per-element comparison surfaces and the reason a sorted-set algebra is only lawful when every operand was constructed under one shared `Order` value declared at the owner.

```typescript
import { Order, SortedSet } from 'effect';

// two sets, two comparators: the fine one separates by full string, the coarse one by first char
const fine: Order.Order<string> = Order.string;
const coarse: Order.Order<string> = Order.mapInput(Order.string, (s: string) => s[0] ?? '');

const left: SortedSet.SortedSet<string> = SortedSet.fromIterable(coarse)(['<aa>', '<ab>']);
const right: SortedSet.SortedSet<string> = SortedSet.fromIterable(fine)(['<aa>', '<ab>']);

// union's `that` is typed Iterable<A>: right's fine policy is erased at the type level before the call,
// so right's two distinct elements re-key under left's coarse policy, tie, and collapse to one
const reKeyed: number = SortedSet.size(SortedSet.union(left, right));
const flipped: number = SortedSet.size(SortedSet.union(right, left));
// non-commutative in cardinality: the receiver's captured Order alone governs each result
const diverges: boolean = reKeyed !== flipped;
```

[TREE_IDENTITY_IGNORES_THE_CAPTURED_ORDER]:
- A `RedBlackTree`'s own `[Equal.symbol]` compares `(this._root?.count ?? 0) !== (that._root?.count ?? 0)` then walks both in-order materializations zipping `Equal.equals` over each `[key, value]` pair — it NEVER compares `this._ord` against `that._ord`, so two trees built under DIFFERENT captured comparators are `Equal.equals` whenever their in-order sequences coincide; the policy that governs every membership decision is itself excluded from the structure's identity.
- `SortedSet`'s `[Equal.symbol]` is `Equal.equals(this.keyTree, that.keyTree)` and `SortedMap`'s is `Equal.equals(this.tree, that.tree)`, so both delegate to the tree's order-blind sequence comparison — but `SortedSet.getEquivalence()` is a DIFFERENT, weaker instance: `(a, b) => isSubset(a, b) && isSubset(b, a)`, mutual containment that re-keys each operand through the other's `has`, so the structural `Equal` (order-blind sequence match) and the derived `Equivalence` (mutual subset under each captured order) answer different questions — the structural one demands identical materialized sequences, the derived one demands equal membership classes, and they diverge precisely when the two sets' captured orders merge different elements.
- The `Hash.symbol` mirrors the order-blindness: a tree hashes `Hash.hash(RedBlackTreeSymbolKey)` XOR-folded with `combine(hash(key))(hash(value))` per in-order item, an ORDER-INDEPENDENT XOR over the materialized pairs — so the hash agrees with the sequence-based `Equal` (both ignore `_ord`) and a `SortedSet`/`SortedMap` dropped into a `HashSet` deduplicates by its materialized content, not by its key policy. The captured `Order` is a behavioral policy that shapes the materialization and is then erased from identity — two structures that arrived at the same sequence by different policies are one value to every outer hashed structure, the policy's effect surviving only in the elements it admitted, never in the identity it conferred.
