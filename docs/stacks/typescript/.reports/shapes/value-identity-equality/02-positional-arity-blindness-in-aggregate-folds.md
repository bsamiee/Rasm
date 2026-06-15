# Positional Arity Blindness In Aggregate Folds

[THE_TRUNCATING_FOLD_AND_ITS_SOLE_EXCEPTION]:
- One private fold backs every positional aggregate except one: `all(collection)` walks `len = Math.min(x.length, y.length)` positions, consults each instance against the head-aligned pair, and returns the all-positions-agree verdict (`true` for the equivalence fold, `0` for the order fold) the moment the shorter operand is exhausted — the longer operand's surplus positions are never read, so the fold is structurally blind to any arity past the shorter length.
- `tuple(...elements)` is literally `all(elements)` in both the equality and the order layer — the heterogeneous fixed-arity combinator inherits the `Math.min` truncation verbatim, so the tuple comparator built from N positional instances admits any two operands agreeing on their shared prefix regardless of declared arity.
- `array(item)` is the SOLE positional aggregate that does not route through `all`: the equality `array` opens with a `self.length !== that.length` early-`false` guard before the per-index loop, and the order `array` walks `Math.min(aLen, bLen)` then closes with `number(aLen, bLen)` — the one explicit length fallback in the entire surface. The conceptual operation "compare two sequences positionally" therefore splits into two opposite arity policies decided entirely by which combinator the author reached for, never by the values.
- The split is not an inconsistency to repair but the algebra each shape models: a tuple is a product of fixed slots whose arity is a type-level fact the runtime presumes already discharged, while an array is a list whose length is part of its value — so the array fold admits length as a comparison input and the tuple fold cannot, because for a well-typed tuple the lengths are equal by construction and the guard would be dead.

[THE_PREFIX_EQUAL_TRAP]:
- For the equivalence layer the truncation collapses to a prefix-equal verdict: two operands of unequal length whose shorter is a positional prefix of the longer compare `true` through `tuple`/`all`, because the fold returns `true` after the `Math.min` positions agree and never inspects the surplus tail — equality is satisfied by prefix agreement, not by whole-sequence agreement.
- For the order layer the same truncation collapses to a prefix-tie: `all` returns `0` when the compared prefix ties and reaches no length comparison, so `tuple`/`all` reports two unequal-length operands EQUIVALENT IN ORDER whenever their shared prefix ties — a sorted structure keyed by this comparator merges a sequence with any of its tied prefixes into one entry, the cardinality undercount no per-element test surfaces.
- `array` closes both traps at opposite ends: the equivalence `array` rejects the unequal-length pair before reading a single element, and the order `array` breaks the prefix-tie by `number(aLen, bLen)` so the shorter sequence sorts strictly before its longer prefix-extension — lexicographic order with length as the terminal tiebreak, the exact total order a dictionary imposes on words sharing a stem.

```typescript
import { Equivalence as Eq, Order } from 'effect';

const cells = [Eq.string, Eq.number, Eq.boolean] as const;
const positional: Eq.Equivalence<readonly [string, number, boolean]> = Eq.tuple(...cells);
const listed: Eq.Equivalence<ReadonlyArray<string>> = Eq.array(Eq.string);

const ranked: Order.Order<readonly [string, number]> = Order.tuple(Order.string, Order.number);
const lexical: Order.Order<ReadonlyArray<string>> = Order.array(Order.string);

// tuple/all truncate to Math.min length: a short prefix compares equal/tied to its extension
const prefixEqual: boolean = (positional as Eq.Equivalence<ReadonlyArray<unknown>>)(['<a>', 1], ['<a>', 1, true]);
const prefixTie: number = (ranked as Order.Order<ReadonlyArray<unknown>>)(['<a>'], ['<a>', 9]);

// array carries length: equality rejects the mismatch, order breaks the tie by length
const lengthRejected: boolean = listed(['<a>'], ['<a>', '<b>']);
const lengthBroken: number = lexical(['<a>'], ['<a>', '<b>']);
```

[THE_TYPE_PRESUMES_THE_ARITY_THE_RUNTIME_DROPS]:
- `tuple` is typed to return the heterogeneous fixed-arity carrier `Equivalence<Readonly<{ [I in keyof T]: ... }>>` (mapped over the instance tuple's keys), while its backing `all` is typed `Equivalence<ReadonlyArray<A>>` — homogeneous and length-free. The static face promises a fixed-arity tuple; the runtime body enforces only a prefix. The presumption holds exactly while the operands enter through the typed surface, because the type then guarantees both arities equal and the `Math.min` becomes a no-op.
- The presumption is the load-bearing precondition, and it fails the instant the typed wall is breached: a value admitted as `unknown` and narrowed to the tuple type without a length check, a tuple widened to its array supertype, or a heterogeneous comparator fed a homogeneous array all reach the same length-blind body with no compile-time arity proof — the comparator silently answers a prefix question for a whole-value contract.
- `productMany(self, collection)` is typed `Equivalence<readonly [A, ...Array<A>]>` — a non-empty array whose head is mandatory and tail variadic. The body reads `self(x[0], y[0])` unconditionally then folds `all(collection)` over `x.slice(1)`/`y.slice(1)`, so the head access presumes the non-empty type the signature declares; an operand wrongly admitted as the empty array dereferences `x[0]` to `undefined`, comparing two absent heads as a present pair — the one positional combinator whose blindness is to the ZERO-arity edge rather than the surplus tail.
- The reject is the explicit positional head-tuple authored as a struct-shaped product: when the slots are named and fixed, a record-keyed comparator over the declared keys carries the arity as the key set and never truncates, where a positional `tuple` over the same concept silently admits prefixes — reach for the positional combinator only when position, not name, is the load-bearing identity.

```typescript
import { Equivalence as Eq } from 'effect';

type Pair = readonly [string, number];

// reject: a positional tuple over a fixed-arity concept admits any prefix-agreeing operand
const positional: Eq.Equivalence<Pair> = Eq.tuple(Eq.string, Eq.number);

// the named struct carries arity as its key set — no Math.min, no prefix admission
const keyed: Eq.Equivalence<{ readonly head: string; readonly count: number }> = Eq.struct({
    head: Eq.string,
    count: Eq.number,
});
```

[STRUCT_FIXES_ARITY_AT_DECLARATION_NOT_AT_THE_OPERANDS]:
- The struct fold reads its arity from `Object.keys(fields)` — the DECLARED instance record, captured once at construction — never from `self` or `that`, so the comparison walks exactly the declared key set against both operands and an operand carrying extra own keys is invisible while an operand missing a declared key reads `undefined` against the present value through the field instance. The struct comparator is arity-fixed at declaration, the polar opposite of the positional fold's read-the-shorter-operand policy and of `array`'s read-both-lengths policy.
- This is the third arity regime, and it is why struct-vs-tuple-vs-array fold algebras diverge precisely on arity admission: the struct fixes arity at the instance (the key list is the comparator's, not the value's), the tuple presumes arity from the type and truncates at runtime, the array reads arity from the values and either rejects (equality) or tiebreaks (order). One concept — positional or keyed aggregate equality — three distinct answers to "what counts as the same shape", each a property of the combinator chosen, none recoverable by inspecting the operands alone.
- Folding the SAME concept through the struct surface rather than the positional surface is the arity-safe collapse: a record of field-keyed instances breaks loudly on a renamed or dropped key at the `keyof`-anchored declaration, whereas the positional tuple over the same fields silently re-admits any prefix — the declaration-anchored key set is the arity proof the positional index sequence cannot carry.

[THE_SCHEMA_TUPLE_DERIVATION_GUARDS_WHERE_THE_COMBINATOR_TRUNCATES]:
- One construction route truncates and one guards for the same conceptual tuple equality: the hand-built `Equivalence.tuple` is `all`, which never reads a length, while the schema-derived `TupleType` arm opens with `len !== b.length` returning `false` before any element is read — so deriving the comparator from a `Schema.Tuple` AST yields length-SENSITIVE identity and composing it from positional instances yields length-BLIND identity, opposite arity policies behind one surface name, and only the derived route admits length as identity.
- The `Math.min` that appears in both routes means opposite things by its position relative to the guard: in `all` it IS the truncation, the sole arity bound and the source of prefix admission; in the derived arm `Math.min(len, ast.elements.length)` is pure zone-partitioning that can never truncate because the equal-length precondition is already discharged above it — truncation and partition are syntactically identical and diverge entirely on whether a length guard precedes them, so reading `Math.min` in a positional fold is undecidable for arity policy until the preceding guard is known.
- After the guard the derived fold partitions into three zones — fixed `elements` by index, a `rest` head across the middle span `i < len - tail.length`, and post-rest `tail` aligned from the end — so a schema tuple with a rest element compares head-fixed, middle-uniform, and tail-fixed positions under distinct instances in one length-checked pass, a richer arity model than the combinator's single uniform truncation.
- The arity-admission divergence is therefore the precise seam between the two tuple routes: a hand-composed `Order.tuple` over schema field instances ties two unequal-length operands on their shared prefix, while `Schema.equivalence` over the corresponding `Schema.Tuple` rejects the length mismatch up front — composing a sorted key by hand from `Order.tuple` and an equality by derivation from the schema yields two surfaces that disagree on arity for the same owner, the coherence hazard mirroring the ordering-derivation gap where one identity surface does not self-propagate.

[ARITY_AS_A_HASH_GATE_HAZARD]:
- The prefix-equal trap is a direct `HashSet`/`HashMap` membership defect when the structure keys on the truncating equivalence: a stored short tuple and a probe that extends it compare `true` under `tuple`/`all`, yet `Hash.array` folds `combine` over ALL elements of each — the longer operand's surplus positions enter the hash and shift the bucket, so the equal-by-prefix pair hashes to different buckets and the `Hash.hash(self) === Hash.hash(that)` precondition fails before the comparator runs. Hash sees the full arity, the truncating comparator does not, and the gate reports unequal what the comparator alone would call equal — the equal-implies-hash-equal law broken not by a hand-rolled hash but by a length-blind equality paired with a length-sensitive hash.
- `Hash.array`'s element fold is length-sensitive by construction (every element contributes), so the only equality coherent with it is the length-sensitive `array` guard, never the truncating `tuple` fold — a `HashSet` of variable-length sequences MUST key on `Array.getEquivalence`/`Equivalence.array`, and routing it through `Equivalence.tuple` desyncs the gate at exactly the unequal-length pairs. The arity policy of the equality and the arity policy of the hash must match, and the positional `tuple` combinator is the one place they silently do not.
- The asymmetry inverts under order: `Order.array`'s `number(aLen, bLen)` length tiebreak makes a `SortedSet`/`RedBlackTree` key TOTAL over unequal lengths, so the shorter sequence and its extension occupy distinct entries — but a tree keyed by `Order.tuple` ties them at the prefix and collapses them into one, the same undercount the truncating equivalence produces, surfacing here as a missing sorted entry rather than a missing hashed one. The membership cardinality of any variable-arity sequence structure is a direct readout of whether its captured comparator was the guarded `array` form or the truncating `tuple` form.
