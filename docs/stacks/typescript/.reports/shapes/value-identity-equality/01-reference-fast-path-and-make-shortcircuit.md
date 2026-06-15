# Reference Fast-Path And make Short-Circuit

[FAST_PATH_IS_THE_CONSTRUCTOR]:
- `Equivalence.make` is `isEquivalent => (self, that) => self === that || isEquivalent(self, that)` and `Order.make` is `compare => (self, that) => self === that ? 0 : compare(self, that)` — the reference identity gate is not a body the author writes, it is the constructor's outer layer, so EVERY instance built through `make` carries the gate and no supplied body can revoke or precede it.
- The leaf primitives split into two construction regimes that decide whether the gate is present: the `Equivalence` leaves (`string`/`number`/`boolean`/`bigint`/`symbol`) are `strict()` = the bare `(x, y) => x === y` and never touch `make`, so they ARE `===` with no second compare to short-circuit, while the `Order` leaves (`string`/`number`/`boolean`/`bigint`) ARE built through `make((self, that) => self < that ? -1 : 1)`, so an `Order` leaf double-tests reference identity (gate, then `<`) and an `Equivalence` leaf does not — the gate is redundant exactly where the body is itself `===` and load-bearing everywhere the body is structural.
- The contract a derived instance inherits is therefore reflexivity for free and at zero body cost: `make` guarantees `f(x, x) === true` / `O(x, x) === 0` for any reference-identical pair before the body runs, so a supplied comparator is relieved of stating its own reflexive case and is only ever invoked on two DISTINCT references — a body that returns the wrong answer for `f(x, x)` is unreachable and unobservable.

[GATE_PROPAGATES_THROUGH_EVERY_COMBINATOR]:
- `combine`, `combineMany`, `combineAll`, `mapInput`, `product`, `productMany`, `tuple`, `array`, `struct`, `reverse`, and `empty` each return their result through `make`, so the gate is multiplicatively present: a `struct` of five `mapInput`-projected leaves is six nested `make` gates, the aggregate's gate firing first on reference-identical operands and collapsing the entire fold before any field is read, projected, or compared.
- The nested gates are not wasted work but a short-circuit cascade with a single reachable layer: when the outer aggregate gate passes (operands are distinct references) the inner element gates still each re-test reference identity on the SUB-values the aggregate hands them, so a `struct` over operands sharing a by-reference field skips that field's body while comparing the rest — partial structural sharing is exploited per field, not only at the whole-value root.
- `reverse(O) = make((self, that) => O(that, self))` re-wraps in `make`, so the reversed instance has its OWN reference gate returning `0` BEFORE the argument swap — correct because the dual of a reflexive tie is a tie, and the inner `O` it delegates to fires its own gate again on the same now-swapped-but-still-identical pair; reversal never inverts the fast-path because `self === that` is symmetric under the swap.

```typescript
import { Equivalence as Eq, Order } from 'effect';

type Row = { readonly key: string; readonly rank: number; readonly note: string };

const byKey: Eq.Equivalence<Row> = Eq.mapInput(Eq.string, (r: Row) => r.key);
const ranked: Order.Order<Row> = Order.combine(
    Order.reverse(Order.mapInput(Order.number, (r: Row) => r.rank)),
    Order.mapInput(Order.string, (r: Row) => r.key),
);

const a: Row = { key: 'AB', rank: 3, note: '<note-a>' };
const reflexEq: boolean = byKey(a, a);
const reflexTie: number = ranked(a, a);
```

[REFLEXIVE_GATE_BYPASSES_THE_BODY]:
- The gate runs the body on exactly zero inputs for a reference-identical pair, so any effect the body would have — a projection, a field walk, a `Brand` check, a thrown branch — is silently skipped for `f(x, x)`; a comparator whose body is total and correct on distinct pairs is reflexive-correct for free, but a comparator whose body is NON-reflexive (returns `false`/non-`0` for some `f(x, x)`) is masked: the gate makes the instance behave reflexively while the body disagrees, and the disagreement surfaces only the day two structurally-equal-but-reference-distinct copies of that same value meet.
- `mapInput(O, f)`'s gate is on the UN-projected operands, so `f` is never called when `self === that`: a projection that is expensive, side-effecting, or partial runs zero times on the reflexive case and exactly once per side on the distinct case — the contravariant retarget is the canonical place where the gate makes a deliberately-coarse projection accidentally-exact, because `mapInput(Eq.number, pick)(x, x)` returns `true` via reference identity even though `pick` would have collapsed `x` into a coarser class.
- The accidental-exactness inverts the intended grain at the reflexive corner: a coarse projection is built to make MANY distinct values equal, yet on `x === x` the gate reports `true` not because the projection said so but because the references matched — for distinct operands the coarse class holds, for the identical operand the answer is trivially exact, so the instance is finest exactly where coarseness is least needed and the projection's discriminating power is observable only across distinct references.

[NAN_AND_THE_NON_REFLEXIVE_GATE]:
- `Order.number` is `make((self, that) => self < that ? -1 : 1)`, a body that never returns `0` — a tie on two distinct number references is delivered ENTIRELY by `make`'s `self === that ? 0` gate, so the only path to an `Order` tie is reference identity and the body alone is a strict trichotomy with the `0` arm structurally unreachable.
- The gate is exactly as reflexive as `===`, and `===` is non-reflexive on primitive `NaN`: `O(n, n)` for a primitive `NaN` bound to one variable still evaluates `n === n` to `false`, skips the gate, and falls into the body where `NaN < NaN` is `false`, returning `1` — so the relation reports `NaN > NaN`, antisymmetry fails on the diagonal, and the reflexive fast-path cannot rescue it because the value the gate tests is the value `===` already rejects.
- The hazard is reference-class-dependent: a primitive `NaN` is never tied to anything including itself (`1` always), so a `SortedSet`/`RedBlackTree` keyed by `Order.number` admits every `NaN` as a fresh strictly-greater entry and locates none by content — `NaN` membership is a structural sink, the single place the constructor's gate is provably insufficient because its sole law-discharge mechanism is the operator that fails on the value.
- `Equivalence.number = strict()` is the bare `(x, y) => x === y` with no `make` gate and no body to fall into, so `NaN`-vs-`NaN` is uniformly `false`: the equivalence half consistently reports `NaN` unequal to itself while the order half reports `NaN` strictly greater than itself, and the two halves of identity disagree on the diagonal precisely because the order body manufactures a `1` where the equivalence has no body to manufacture anything but the operator's verdict.

[GATE_LOCATION_DECIDES_THE_TIE_REPRESENTATIVE]:
- The fast-path is a property of the constructor, so a value-selecting fold's tie behavior is set by WHICH constructor wraps the selection body: `Semigroup.make = (combine, combineMany) => ({ combine, combineMany })` builds a record with no reference gate, so `Semigroup.min(O) = make((self, that) => O(self, that) === -1 ? self : that)` owns no `self === that` layer and reads the gate only second-hand through `O`.
- `Order.min(O) = dual(2, (self, that) => self === that || O(self, that) < 1 ? self : that)` re-asserts the gate at the selection site itself, so on a reference-identical pair it returns `self` directly while `Semigroup.min` lets `O`'s gate return `0`, fails the `0 === -1` test, and returns `that` — the opposite representative, decided not by the `Order` (identical in both) but by whether the selecting constructor re-states the gate.
- The choice is invisible until the `Order` is coarser than structural identity: two reference-DISTINCT operands tying under an `Order.mapInput` projection fire no reference gate on either path, both fall to the body (`< 1` keeps first, `=== -1` keeps last), and the incidental fields the projection dropped survive differently — so an extremum fold over a projected order is non-deterministic in payload, and the determinant is the gate's presence at the fold constructor, not at the comparator the fold reads.

[REFLEXIVE_COHERENCE_WITH_THE_HASH_GATE]:
- `Equal.equivalence()` returns `equals` itself — not a `make`-wrapped instance — so its reference fast-path is the `compareBoth` gate's own `if (self === that) return true`, the SAME first-line gate the structural `[Equal.symbol]` path runs, never a second redundant layer; lifting structural identity into the `Equivalence` surface inherits one gate, not the doubled gate a `make`-built bridge would impose.
- The `compareBoth` gate is strictly stronger than a `make` gate because it precedes the `Hash.hash(self) === Hash.hash(that)` precondition: a reference-identical pair returns `true` before the hash is ever consulted, so a value whose memoized `[Hash.symbol]` has desynced from its mutated fields still compares EQUAL to itself by reference even though the hash-gate would report it unequal to a fresh structural copy — reflexivity survives the cache desync that breaks transitive structural comparison, and only the distinct-copy comparison surfaces the `Hash.cached` mutation hazard.
- A `Data`/`Schema.Class` value dropped into a `HashSet` is located by `Hash.hash` then settled by `Equal.equals`, and the settle step's `compareBoth` gate makes the stored reference always match itself: re-adding the exact same reference is a no-op via the gate before any structural body runs, so reference-identity dedup is the free floor under content-identity dedup and the structural comparator is reached only for two reference-distinct buckets-mates — the gate is the reason a `HashSet` never re-hashes a value against its own stored copy.

[REJECT_THE_HAND_ROLLED_REFLEXIVE_GUARD]:
- A comparator authored with an explicit `self === that ? 0 : ...` head restates the gate `Order.make` already supplies, and routing through `Order.make` to obtain proper typing means the gate is written twice — once by the author, once by the constructor — so the author's reflexive arm is dead code the constructor's outer gate has already short-circuited; the gate is owned by the constructor, so the body states ONLY the distinct-pair comparison and never its own reflexive case.

```typescript
import { Order } from 'effect';

type Span = { readonly lo: number; readonly hi: number };

const byWidth: Order.Order<Span> = Order.mapInput(Order.number, (s: Span) => s.hi - s.lo);

// reject: the reflexive arm is dead — Order.make's outer gate already returned 0 before this body runs
const handRolled: Order.Order<Span> = Order.make((self, that) =>
    self === that ? 0 : Order.number(self.hi - self.lo, that.hi - that.lo),
);
```

[GATE_AS_THE_REFLEXIVITY_LAW_BOUNDARY]:
- `make`'s gate discharges the reflexivity obligation of an equivalence relation (`x ~ x`) and the antisymmetry corner of a total order (`x <= x` and `x >= x` ⇒ tie) structurally, so the supplied body is only required to be symmetric/antisymmetric and transitive over DISTINCT references — the one law the body cannot be checked against is reflexivity, because the gate makes every reflexive call unreachable, so a property test feeding `f(x, x)` proves nothing about the body and a generator must yield two reference-distinct structurally-equal values to exercise the reflexive law the body actually owns.
- The boundary of the fast-path's authority is the `===` operator's own semantics, bounded by twin edges: it is UNDER-eager on `NaN` (never reflexive, so the gate cannot discharge the diagonal and the body's verdict stands), and OVER-eager on signed zero (`+0 === -0` is `true`, so `Order.number(+0, -0)` returns `0` from the gate and a signed-zero distinction the body could draw via `1/x` is unreachable for that pair) — the gate is exactly as correct as reference equality, so the reflexivity it grants and the reflexivity it withholds are both inherited verbatim from `===`, and no supplied body can widen or narrow that boundary.
