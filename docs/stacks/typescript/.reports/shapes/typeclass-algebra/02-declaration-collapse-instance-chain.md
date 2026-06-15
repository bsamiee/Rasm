# Owner-Derived Instance Chain

[ONE_OWNER_IS_THE_HEAD_OF_THE_DERIVATION_FAN]:
- A value owner's first declaration is the only place a comparator, accumulator, identity, or hash key-set is spelled: a `Schema.Class<Self>(fields)`, a `Data.taggedEnum`, or a `Data.struct` fixes the runtime value, the static type, and the structural `Equal`/`Hash`, and from THAT one statement `Schema.equivalence(schema)`, `Equal.equivalence<A>()`, `Struct.getEquivalence(fieldEquivs)`, `Struct.getOrder(fieldOrders)`, `Semigroup.struct(fieldSemis)`, and `Hash.structureKeys(value, keys)` all derive — none re-states the field shape, the comparison rule, or the merge.
- The fan is asymmetric by what the owner CAN carry: `Schema.equivalence`, `Arbitrary.make`, `Pretty.make`, and `JSONSchema` are extracted from the schema owner directly because the AST carries an annotation slot for each (`EquivalenceAnnotationId`, `ArbitraryAnnotationId`, `PrettyAnnotationId`, `JSONSchemaAnnotationId`), but there is NO `OrderAnnotationId` — ordering is the one comparison surface the schema owner cannot auto-derive, so `Struct.getOrder` over per-field `Order` instances is the sanctioned assembly, never a `Schema.order` that does not exist.
- The collapse target is the four-link chain the loose form spells per concept — `const fields` → `interface Shape` → `const eq = Equivalence.make(...)` → `const ord: Order<Shape> = ...` → `const merge = Semigroup.make(...)` — where every link after the first re-reads the same field list and every re-read is a place the comparator drifts from the data; the owner deletes links two through five by carrying the fields once and yielding each instance from the same `keyof` projection.

[ADDING_ONE_FIELD_THREADS_INTO_EVERY_DERIVED_SURFACE_AT_COMPILE_TIME]:
- The proof of a correctly-collapsed owner is the diff of the next field, and the guarantee has a precise boundary: `Struct.getOrder`/`Struct.getEquivalence` INFER their result key-set from the argument map (`<R>(fields: R) => Order<{ [K in keyof R]: ... }>`), so a partial map silently yields an order over the partial record — the completeness check fires only when the result is annotated `Order.Order<T>`/`Equivalence.Equivalence<T>` with `T = Schema.Schema.Type<owner>`, at which point a field added to the owner makes the partial map fail to satisfy the full-record target and the omission surfaces as a checker error rather than a stale comparator. The result-type annotation is the load-bearing link; without it the lift is as drift-prone as a hand-written comparator.
- `Schema.equivalence` carries the same completeness for FREE because its result type is fixed by the schema owner, not inferred from a map — adding a field to the schema rewrites the derived `Equivalence<A>` with no second site to update, so the annotation-derived equivalence is unconditionally field-complete while the struct-lifted order is field-complete only under the explicit target annotation; the asymmetry is exactly the absent `OrderAnnotationId`, and the per-field `Order` map is the one structure in the chain a property test over the owner's full field set must guard.

```typescript
import { Schema, Struct, Order, Equivalence } from 'effect'

class Owner extends Schema.Class<Owner>('Owner')({
    rank: Schema.Number,
    label: Schema.String,
    span: Schema.Number,
}) {}
type T = Schema.Schema.Type<typeof Owner>

const fromOwner = Schema.equivalence(Owner)
const byField: { readonly [K in keyof T]: Order.Order<T[K]> } = {
    rank: Order.number,
    label: Order.string,
    span: Order.reverse(Order.number),
}
const ranked: Order.Order<T> = Struct.getOrder(byField)
const same: Equivalence.Equivalence<T> = Struct.getEquivalence({
    rank: Equivalence.number,
    label: Equivalence.string,
    span: Equivalence.number,
})
```

[THE_ANNOTATION_OVERRIDE_LANDS_INSIDE_THE_OWNER_NEVER_BESIDE_IT]:
- When a field's structural deep-equality is wrong — a normalized string, a tolerance numeric, a set-valued field — the correction is an `equivalence` annotation on the FIELD inside the owner declaration, not a parallel `Equivalence.make` at a consumer: the field's schema carries `{ equivalence: () => Equivalence<A> }` and `Schema.equivalence` of the whole owner folds the per-field overrides through the conjunction monoid, so the correction composes into the one derived instance and every consumer shifts at compile time because they read it; the annotation is a zero-argument thunk `() => Equivalence<A>` at the declaration surface, so the override is a closed value, never a parameter the consumer re-supplies.
- The two equality paths converge on a `Schema.Class` by construction: the class instance carries structural `Equal` (so `Equal.equivalence<T>()` compares it by deep value) AND its schema yields `Schema.equivalence` (the annotation-folded field conjunction), so a divergence between them is a DECLARED annotation difference, never an accident — when no annotation is set the two paths produce the same field-wise conjunction, when one is set the annotation is the single source of the difference.
- `Schema.Data(innerSchema)` is the lift that gives a plain decoded record the `Equal`/`Hash` protocol a `Schema.Class` carries natively, so a struct-shaped schema participates in structural identity without becoming a class — the reject is decoding to a plain object and then writing `Equivalence.make` to recover the equality the `Schema.Data` lift would have derived from the same owner.

```typescript
import { Schema, Equivalence } from 'effect'

class Owner extends Schema.Class<Owner>('Owner')({
    key: Schema.Number,
    name: Schema.String.annotations({ equivalence: () => Equivalence.strict<string>() }),
}) {}

const same = Schema.equivalence(Owner)
const verdict = same(new Owner({ key: 1, name: '<value-a>' }), new Owner({ key: 1, name: '<value-b>' }))
```

[EQUAL_HASH_FUSION_FORCES_THE_KEYED_STRUCTURE_OFF_THE_SAME_OWNER]:
- `Equal extends Hash` is one obligation, not two: a value owner carrying structural `Equal` is OBLIGATED to carry `[Hash.symbol](): number`, so the bucket a value lands in inside a `HashSet<T>`/`HashMap<T, V>` is computed by the same protocol that decides its equality — equality and hashing cannot drift because they are members of one interface the owner implements once, and a `HashSet.fromIterable` over owner values needs NO equality or hash argument, the structure reading both off the element.
- The data-structure choice is the witness selection made structural: `HashSet`/`HashMap`/`MutableHashMap` key by the owner's `Equal`/`Hash` with NO argument, while `SortedSet.fromIterable(ord)`/`SortedSet.make(ord)` key by an `Order` instance passed EXPLICITLY — the annotation asymmetry surfaced in the collection API, because the owner carries identity but cannot carry order, so the hashed structure reads its key off the element and the sorted structure demands the one instance the owner does not derive.
- The schema-level codec makes the asymmetry sharpest: `Schema.SortedSetFromSelf(value, ordA, ordI)` requires TWO orders — one over `Schema.Type<Value>` and one over `Schema.Encoded<Value>` — because neither the decoded nor the encoded side is derivable from the codec that owns both types, whereas `Schema.equivalence` of that same `value` needs nothing; an owner that fuses the type, the decoder, the encoder, and the equality still hands order back to the caller on BOTH channels, the absent `OrderAnnotationId` made unavoidable at the persistence boundary.

```typescript
import { Schema, HashSet, Equal } from 'effect'

class Owner extends Schema.Class<Owner>('Owner')({ id: Schema.Number, payload: Schema.String }) {}

const seen = HashSet.fromIterable([
    new Owner({ id: 1, payload: '<value-a>' }),
    new Owner({ id: 1, payload: '<value-a>' }),
])
const isDuplicate = Equal.equals(new Owner({ id: 1, payload: '<value-a>' }), new Owner({ id: 1, payload: '<value-a>' }))
const count = HashSet.size(seen)
```

[ONE_PROJECTION_RETARGETS_THE_THREE_ALGEBRAS_AT_ONCE]:
- A value object wrapping a leaf retargets its parent's three algebras through the SAME projection: the accumulation flips by the invariant `Semigroup.imap(S, to, from)`, the comparison and the equality flip by the contravariant `Order.mapInput(O, get)`/`Equivalence.mapInput(E, get)`, and when `get = to` the one wrap function threads into the combine, the order, AND the equality — so the wrapped owner declares one `(to, from)` and its derived algebra triple moves together, the contravariant retarget and the invariant retarget sharing the projection the owner names rather than three hand-written instances over the wrapped type.
- The retarget direction is the asymmetry the projection encodes: `imap` needs BOTH `to` and `from` because accumulation produces a wrapped value the result must re-wrap, while `mapInput` needs only the input flip `B → A` because comparison/equality CONSUME the value and return a `-1 | 0 | 1`/boolean that needs no re-wrap — so a comparison-only retarget over a one-way projection is lawful where a `Semigroup` retarget is not, the value slot's invariance versus the consumer's contravariance surfaced as which functions each retarget demands.
- `Monoid` exposes NO `imap`, so an identity-bearing accumulation over the wrapped owner goes through `Monoid.fromSemigroup(Semigroup.imap(M, to, from), to(M.empty))` — the identity is retargeted explicitly through `to`, the missing `Monoid.imap` the API refusing to let a bare `(to, from)` pair smuggle an unproven two-sided identity into the chain.

[STRUCT_LIFT_IS_THE_SAME_FIELD_MAP_AT_THREE_ALGEBRAS]:
- `Struct.getOrder(fields)`, `Struct.getEquivalence(fields)`, and `Semigroup.struct(fields)`/`Monoid.struct(fields)` are ONE shape — a `Record<keyof R, Instance>` mapped to an `Instance` over the record — at three algebras: comparison, equality, accumulation, each yielding the record instance by lifting per-field instances field-wise, and `Monoid.struct` additionally derives the record `empty` as the per-field-`empty` record, so the identity extends with the field with zero hand-edit while a hand-written `Monoid.make` would re-spell the whole `empty` record per field.
- The three field maps and the `keys` array of `Hash.structureKeys(value, keys)` all index the same `keyof R`, so the owner's identity, ordering, accumulation, and hash bucket are four reads of one field set — and the lawful invariant the chain enforces is that the hash `keys` subset equals the field set the equality reads, so equality and hashing agree by construction; a hand-written `Equal` comparing three fields while `structureKeys` hashes two is the bucketing defect the single field set forecloses.
- `Order` and `Equivalence` are themselves carriers with `OrderTypeLambda`/`EquivalenceTypeLambda`, so `Order.product`/`Equivalence.product` build tuple comparisons through the SAME `SemiProduct` applicative that builds value tuples — a record's order, its equality, and its struct accumulation are three instances composed by the one product algebra over the one field map, so the comparison family and the value family share one composition surface and a fourth field is one row added to each of the three maps in lockstep, never three parallel record comparators drifting.

```typescript
import { Struct, Order, Equivalence } from 'effect'
import { Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import * as B from '@effect/typeclass/data/Boolean'

type T = { readonly hits: number; readonly peak: number; readonly clean: boolean }

const ranked: Order.Order<T> = Struct.getOrder({ hits: Order.number, peak: Order.number, clean: Order.boolean })
const same: Equivalence.Equivalence<T> = Struct.getEquivalence({
    hits: Equivalence.number,
    peak: Equivalence.number,
    clean: Equivalence.boolean,
})
const tally: Monoid.Monoid<T> = Monoid.struct({ hits: N.MonoidSum, peak: N.MonoidMax, clean: B.MonoidEvery })
const folded = tally.combineAll([{ hits: 1, peak: 7, clean: true }, { hits: 1, peak: 3, clean: false }])
```

[THE_NO_MERGE_COLUMN_KEEPS_A_FIELD_INTACT_ACROSS_THE_FOLD]:
- A `Semigroup.struct` over an owner whose discriminant or identity field must SURVIVE a fold rather than merge selects `Semigroup.first<K>()`/`Semigroup.last<K>()` for that column — associative, non-commutative, the lawful "pick a side, do not combine" — so a struct accumulation folds the numeric columns by `MonoidSum`/`MonoidMax` while the key column passes through untouched, never a fold that wrongly sums a discriminant.
- The struct lift exposes the per-field algebra as a per-column CHOICE, so the same owner's accumulation differs from its equality and its order in exactly which fields participate: the order chain and the equality conjunction read every field, the accumulation may freeze a field with `first`/`last` — the three derivations diverge at the column policy, all three still rooted in the one owner declaration and the one `keyof` field set, so a field added to the owner is a `first`/`last` decision for the merge and an automatic row for the order and the equality.
- `Bounded<A>` fuses `{ compare; maxBound; minBound }` so an extremum fold over an owner field reads its identity off the bound the field's order already declares: `Bounded.max(B)` seeds with `minBound`, `Bounded.min(B)` with `maxBound`, and `Bounded.reverse(B)` flips both atomically — an ascending and descending extremum column over one field is one bound and its involution, never two records, the identity never a literal the struct-builder could get wrong.

[THE_DRIFT_IS_THE_REJECT_AND_THE_LAW_IS_UNCHECKED]:
- The rejected shape is any parallel hand-written second comparator, equality, or merge living BESIDE the owner: a `const orderByName = (a, b) => a.name.localeCompare(b.name)` next to a `Schema.Class` that already yields `Struct.getOrder({ name: Order.string, ... })` is the link the collapse deletes, because the next field updates the owner-derived chain and forgets the loose one — the drift is not a style fault, it is the comparator answering a question about a stale field set.
- The collapse concentrates the law obligation onto the LEAF instances the owner feeds: `Semigroup`/`Equivalence` carry no law field, so the whole derived family (`Struct.getOrder`, `Struct.getEquivalence`, the struct lift, every `combineAll`) inherits the leaf's unverified associativity/transitivity — one unlawful leaf annotated on one owner field silently poisons every surface derived from that owner, the multiplicative fan-out working in reverse as a fault-amplifier, so the property test that admits a leaf witness guards the entire chain at one point rather than each derived site.
- `Equivalence.make(isEquivalent)` wraps every comparator as `self === that || isEquivalent(self, that)`, so reference identity short-circuits BEFORE the user predicate — reflexivity is enforced for identical references and merely ASSUMED for distinct ones, so a tolerance leaf (`|x - y| < ε`) annotated on one field passes `make`, folds cleanly through the conjunction `Schema.equivalence` derives, yet violates transitivity and corrupts `HashSet` bucketing where membership assumes a true equivalence; the conjunction monoid ANDs transitive relations into a transitive one but cannot synthesize transitivity a leaf lacks, so the owner's annotated leaf must already carry the law the whole derived chain assumes.

[ORDER_IS_NON_COMMUTATIVE_SO_THE_OWNER_FIELD_SEQUENCE_IS_CORRECTNESS]:
- `Order.combine`/`Struct.getOrder` is leftmost-decisive and associative but NOT commutative, so the ORDER in which the owner's fields enter the derived comparator is a correctness decision — a rank-then-name priority and a name-then-rank priority over the same field set are different total orders — while `Equivalence.combine` is commutative AND idempotent so field order in the equality conjunction is a pure performance lever (cheapest-discriminating field first prunes earliest); the one owner feeds both, but the two derivations treat the field sequence inversely, the exact place a single field map cannot encode both intents in its key order alone.
- The owner-derived order and equivalence agree on the field SET but the divergence between `Order.min`/`Semigroup.min` tie-breaks (`Order.min` keeps the leftmost equal element via `<=`, `Semigroup.min` keeps the rightmost via strict `<`) is load-bearing the moment the order is BY PROJECTION onto one field: ordering an owner by `rank` ties every record sharing a rank, and which tied record survives an extremum fold depends on which surface owns the comparison — a divergence invisible until two owner values compare equal-by-order but differ by structural identity, the boundary where the owner's `Equal` and its projected `Order` disagree by design.
