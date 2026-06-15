# Monoidal Precedence Use Of combine

[ONE_FOLD_TWO_CODOMAINS]:
- `combine` is one left-biased fold whose body differs only in the codomain's absorbing element: the order body is `out !== 0 ? out : that(a1, a2)` and the equality body is `self(x, y) && that(x, y)` — both consult the right operand ONLY where the left yielded the codomain's neutral verdict, so `0` for the `Ordering` lattice and `true` for the boolean lattice play the identical structural role of "left was inconclusive, defer to the right".
- The two halves are therefore the SAME monoidal layering at two codomains, not two combinators that happen to share a name: precedence-of-keys and conjunction-of-fields are one algebra, and a multi-key `Order` and the matching multi-field `Equivalence` derive from the IDENTICAL projection sequence — adding a tiebreak key and adding an identity field is one `combine` arm appended at both surfaces, never two independently authored comparators.
- The absorbing element of each codomain is what gives the short-circuit: a non-zero `Ordering` and a `false` are absorbing under the fold (no later operand can change the verdict), so the first decisive operand wins outright — `combineMany` returns the instant `out !== 0`, the equality `combineMany` returns the instant a `!equivalence(x, y)`, and every later operand is unreached. Precedence and short-circuit-AND are the same early-exit, read at opposite poles of the verdict.

```typescript
import { Equivalence as Eq, Order } from 'effect';

type Row = { readonly tier: number; readonly code: string; readonly stamp: number };

// the same projection sequence feeds both surfaces — one declaration, two codomains
const precedence: Order.Order<Row> = Order.combineAll([
    Order.mapInput(Order.number, (r: Row) => r.tier),
    Order.mapInput(Order.string, (r: Row) => r.code),
]);
const sameClass: Eq.Equivalence<Row> = Eq.combineAll([
    Eq.mapInput(Eq.number, (r: Row) => r.tier),
    Eq.mapInput(Eq.string, (r: Row) => r.code),
]);
```

[LEFT_MOST_DECISIVE_IS_CHEAPEST_FIRST]:
- The fold consults operands strictly left to right and exits on the first decisive one, so declaration order IS evaluation order IS a cost decision: the cheapest-discriminating instance belongs leftmost so the expensive projection is reached only across the tie the cheap one leaves, and a coarse-but-cheap key that resolves most pairs spares every later key its projection cost on the common case.
- The cost ordering is observable on the order half and the equality half in opposite directions because their absorbing elements differ: the `Order` exits when a key DISAGREES (`out !== 0`), so a high-cardinality discriminating key leftmost exits fastest; the `Equivalence` exits when a field DISAGREES (`!eq`), so the field most LIKELY to differ leftmost exits fastest — the same "cheapest decision first" rule resolves to "most-discriminating key first" for sorting and "most-volatile field first" for equality, never a single field-priority order shared blindly across both.
- Reordering the arm sequence preserves the partition into classes but not the representative: `combine(a, b)` and `combine(b, a)` agree on which pairs end equal/tied yet disagree on which key breaks a tie and on which payload survives an extremum fold, so a cost-driven reorder is verdict-safe for membership but representative-unsafe — correct only when no later combinator reads the resolved tie-breaker, the one boundary that licenses the cheapest-first reordering.

[TWO_OPPOSITE_EMPTY_IDENTITIES]:
- The empty fold is where the two surfaces diverge structurally: `Order.combineAll([])` seeds `Order.empty<A>()` = `make(() => 0)` and yields a STABLE total tie, while `Equivalence.combineAll([])` seeds the unexposed `isAlwaysEquivalent = (_x, _y) => true` and yields the EVERYTHING-EQUAL instance — the order identity leaves order untouched, the equality identity collapses every value into one class, so a dynamically assembled multi-key policy with zero selected keys is a benign no-op for sorting and a silent total merge for equality.
- The asymmetry is enforced at the type surface, not by convention: `Order` exports `empty` and `combineAll` as a true `Monoid` (the always-tie is the published identity), while `Equivalence` exports `combine`/`combineMany`/`combineAll` but withholds `empty` and `reverse` — it is a `Semigroup` only, its mathematical identity reachable through the variadic fold yet never handed out as a value, because the always-equal instance is a correctness hazard rather than a useful default.
- The runtime-selected variadic builder and the static `Struct.getOrder`/`getEquivalence` table are the SAME fold at two binding times — the table fixes the key set at declaration so the checker chases an owner-field addition into a missing row, the variadic builder fixes it at runtime from an assembled `ReadonlyArray<Order<A>>`/`ReadonlyArray<Equivalence<A>>` — so the choice is whether the identity grain is a compile-time owner property or a runtime policy value, and the zero-key edge is total for the order builder and a merge for the equality builder regardless of which binding time selected it.

```typescript
import { Equivalence as Eq, Order } from 'effect';

type Row = { readonly tier: number; readonly code: string };
const cells: ReadonlyArray<keyof Row> = ['tier', 'code'];

// runtime-selected key sequence: order builder is total at [], equality builder merges at []
const dynamicOrder: Order.Order<Row> = Order.combineAll(
    cells.map((k) => Order.mapInput(Order.string, (r: Row) => String(r[k]))),
);
const dynamicClass: Eq.Equivalence<Row> = Eq.combineAll(
    cells.map((k) => Eq.mapInput(Eq.string, (r: Row) => String(r[k]))),
);
```

[REVERSE_IS_A_PER_ARM_DIRECTION_FLIP]:
- `Order.reverse(O) = make((self, that) => O(that, self))` is an argument swap, never a codomain sign-flip, and because each `combine` arm carries its own instance, reversing ONE arm flips that key's sort direction alone while leaving its precedence rank and every other key untouched — descending-by-rank-then-ascending-by-code is `combine(reverse(mapInput(Order.number, byRank)), mapInput(Order.string, byCode))`, two cells, never a hand-rolled `(a, b) => b.rank - a.rank || a.code.localeCompare(b.code)`.
- The equality half has no `reverse` and needs none: conjunction is symmetric in direction (an `Equivalence` is its own dual), so a multi-field equality carries no per-arm direction and the two folds diverge exactly on the one axis ordering owns — the order arm sequence encodes precedence AND direction, the equality arm sequence encodes only membership in the identity grain.
- Reversing the WHOLE precedence with one outer `Order.reverse` over a `combineAll` is distinct from reversing each arm: the outer reverse flips every key's direction in lockstep AND inverts the lattice the fold walks, so the global descending order is one wrap while a mixed direction is per-cell — conflating the two by reversing the aggregate when only one key should descend silently flips every tiebreak.

[PRECEDENCE_FOLD_FEEDS_THE_VALUE_FOLD]:
- A `combineAll`-built precedence is consumed by `Monoid.max(B)`/`Semigroup.max(O)` as an opaque comparator, so a tiebreak appended as one `combine` arm reshapes the extremum a value fold selects with zero edits at the fold site — the precedence fold and the value-selecting fold are the same left-biased algebra stacked at two altitudes, the codomain fold deciding WHICH key breaks the tie and the value fold reading that verdict to decide WHICH value wins.
- The precedence being a genuine Monoid (`Order.empty` published) is exactly what lets the value fold above it be one: `Monoid.max(B)` seeds `B.minBound` as its empty, the bound that loses to every real value, so the precedence's own total-tie identity composes upward into a total extremum-of-empty — the equality Semigroup carries no such empty and therefore no analogous upward lift, the order/equality asymmetry surfacing one level higher than the fold that creates it.
- The asymmetry is also the source of a payload non-determinism the fold site cannot see: a precedence omitting a field ties order-equal-but-structurally-distinct operands, so which incidental payload survives the extremum is decided not by the precedence (which tied) but by the selecting constructor's first-vs-last retention — appending a `combine` arm that discriminates the tie removes the non-determinism, so the precedence's arm count and the extremum's retention surface are two independent knobs that must agree on how finely the winning value is pinned.

```typescript
import { Monoid } from '@effect/typeclass';
import type { Bounded } from '@effect/typeclass';
import { Order } from 'effect';

type Row = { readonly tier: number; readonly weight: number };
const precedence: Order.Order<Row> = Order.combine(
    Order.mapInput(Order.number, (r: Row) => r.tier),
    Order.mapInput(Order.number, (r: Row) => r.weight),
);
const bound: Bounded.Bounded<Row> = {
    compare: precedence,
    maxBound: { tier: Number.POSITIVE_INFINITY, weight: Number.POSITIVE_INFINITY },
    minBound: { tier: Number.NEGATIVE_INFINITY, weight: Number.NEGATIVE_INFINITY },
};
// the precedence comparator feeds the value fold; minBound is the empty for max-of-empty totality
const peak: Monoid.Monoid<Row> = Monoid.max(bound);
const winner: Row = peak.combineAll([{ tier: 1, weight: 9 }, { tier: 2, weight: 3 }]);
```

[COMBINE_NEVER_SEES_ARITY]:
- `Order.combine`/`combineMany` and `Equivalence.combine`/`combineMany` are arity-blind by construction — each operand is a whole-value comparator handed the SAME two operands, so the fold never indexes a position or counts a length; arity admission is entirely a property of the leaf instances the arms wrap, never of the precedence fold itself. A `combine` of `mapInput`-projected keys carries no truncation because no arm reads a sequence; a `combine` whose arms are themselves `Order.tuple`/`Order.array` inherits each arm's distinct arity policy unchanged.
- This is the seam where precedence composition stays coherent while positional aggregation does not: appending a key to a precedence is appending a whole-value arm the fold consults after the prior tie, so the multi-KEY surface grows safely under `combine` exactly because it is not a multi-POSITION surface — the fold's input is a list of comparators, not a list of operand slots, and the operand pair is constant across every arm.
- A precedence whose arms mix a guarded `Order.array` key and a truncating `Order.tuple` key is law-coherent at the `combine` level yet inherits the tuple arm's prefix-tie at exactly that arm's tie — so the precedence fold cannot repair an arm's arity defect, only sequence it, and a sorted structure keyed by such a precedence collapses prefix-tied operands precisely when every arm before the tuple arm also tied.

[GATE_TIMES_PRECEDENCE_IS_A_PRODUCT_OF_GATES]:
- Every `combine` arm and the aggregate are each built through `make`, so a `combineAll` of N projected leaves is N+1 nested reference gates: the aggregate gate fires first on a reference-identical pair and collapses the entire precedence before any projection runs, and where it passes the inner arm gates re-test reference identity on the SAME pair the fold hands down — partial structural sharing short-circuits a shared-by-reference key while the rest of the precedence still evaluates.
- The gate makes the precedence's leftmost-decisive contract hold even for self-comparison without entering the body: `combineAll(keys)(x, x)` returns the codomain identity (`0` for order, `true` for equality) from the outermost `make` gate, so the cost-ordering of the keys is irrelevant on the reflexive diagonal and observable only across reference-distinct operands — the cheapest-first discipline pays off exactly on the distinct pairs the gate cannot short-circuit.
- The `Equal.equivalence<A>()` bridge is NOT a `make`-built arm: it returns `equals` itself whose own `if (self === that) return true` gate precedes the `Hash.hash` precondition, so folding it as a `combine` arm contributes one gate plus the hash-gate, not a doubled reference layer — mixing the structural bridge and projected leaves in one precedence stacks heterogeneous gate kinds whose reflexive verdicts nonetheless agree.

[COMBINE_IS_NOT_THE_HASH_GRAIN]:
- A `combineAll`-built equality declares the field grain but is not itself a hash, and its defining property — verdict-invariant under arm reordering while evaluation-sensitive for cost — is exactly the property that makes it pairable with the XOR-accumulated `Hash.structureKeys`, which is likewise order-INDEPENDENT over its key set: the combine fold's freedom to sequence its arms cheapest-first cannot perturb the verdict the hash must agree with, so cost-ordering and hash-coherence are non-interfering.
- The pairing breaks the instant a precedence arm is positional: a `combine` whose arm is `Equivalence.tuple`/`array` makes the aggregate's verdict depend on element position, and `Hash.array` folds the order-SENSITIVE `combine` over elements — so a positional arm couples the fold's verdict to a hash whose own combination is order-sensitive, and the two can only stay coherent over equal-arity operands the prefix-truncating arm does not guarantee.
- The arm-reordering invariance is therefore the lever that lets one `keyof`-anchored field set drive both surfaces: the same literal key vocabulary feeds the `combine`-folded equality and the `Hash.structureKeys` array, the over-collide-versus-scatter coherence (an owned hazard) holds by anchor rather than audit, and a field promoted to identity is one arm plus one array entry forced at the same `keyof` site — provided every arm stays whole-value, never positional.

[REJECT_THE_HAND_ROLLED_PRECEDENCE_CHAIN]:
- A hand-written `||`-chain over subtractions and `localeCompare` restates the leftmost-non-zero fold `combine` already supplies, hard-codes each key's direction into the body where `reverse` would flip one cell, and silently truncates the `Ordering` codomain to whatever `-` and `localeCompare` return rather than the `-1 | 0 | 1` the lattice demands — the chain is the spelling `combineAll` over projected leaves deletes, the precedence sequence is the arm list, and direction is `reverse` per cell.

```typescript
import { Order } from 'effect';

type Row = { readonly tier: number; readonly code: string };

// reject: a hand-rolled precedence hard-codes direction and re-derives the leftmost-non-zero fold
const handRolled: Order.Order<Row> = Order.make((self, that) =>
    self.tier > that.tier ? -1 : self.tier < that.tier ? 1 : self.code < that.code ? -1 : self.code > that.code ? 1 : 0,
);

// the fold owns precedence; reverse owns the one descending cell; the codomain stays -1 | 0 | 1
const precedence: Order.Order<Row> = Order.combine(
    Order.reverse(Order.mapInput(Order.number, (r: Row) => r.tier)),
    Order.mapInput(Order.string, (r: Row) => r.code),
);
```
