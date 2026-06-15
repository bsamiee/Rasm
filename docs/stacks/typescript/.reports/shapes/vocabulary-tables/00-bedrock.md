# Vocabulary Tables

[AS_CONST_TABLE]:
- The behavior-column table is one `as const` object literal whose rows are keys and whose columns are the per-key data the program reads at runtime; it is the decode-free, dependency-free vocabulary owner — no schema, no codec, no library on the read path.
- `as const` freezes every leaf to its literal type and marks the whole structure `readonly` recursively, so each row column is a literal, not its widened primitive; a column written `0` types as `0`, a column `'<glyph-a>'` types as `'<glyph-a>'`, and the union of a column across rows is a closed literal union, not `number` or `string`.
- The runtime read is two operators: `keyof typeof Table` names the key set, and indexed access `Table[k]` returns the row; a column projection is `Table[k].rank`, and the column's value union is `(typeof Table)[keyof typeof Table]['rank']` — every projection derives from the one literal, none is restated.
- `keyof typeof` over an `as const` object yields only the string/number/symbol keys actually present; an empty table yields `never`, and a table widened to `Record<string, Row>` loses the literal key set and degrades `keyof typeof` to `string` — the literal table is the only form whose key set is a closed union.
- A row whose value is itself a nested `as const` object stays a vocabulary; a row whose value is a function makes the table a dispatch surface read by `Table[k](arg)`, but a function column cannot survive `JSON` projection and forfeits structural `Equal`/`Hash` — keep data columns and behavior columns separated by intent.

[SATISFIES_CLOSURE]:
- `as const satisfies RowContract` is the closed-row form: `as const` preserves every literal, then `satisfies` checks each row against the contract without widening the inferred type to the contract — the binding keeps its narrow literal shape and gains a compile-time guarantee that every row is complete and correctly typed.
- Operator order is load-bearing: `as const satisfies Record<Key, Row>` keeps literals and checks shape; `satisfies Record<Key, Row> as const` and a bare type annotation `const T: Record<Key, Row> = {...}` both widen the binding to the contract, collapsing `keyof typeof T` to `Key` and every column to its primitive — the annotation defeats the table.
- The contract is a `Record<string, Row>` upper bound, never the table's own type; it asserts each row satisfies `Row` while leaving inference free to discover the exact key set and exact column literals, so a missing column fails at the declaration site and the literal union still flows to consumers.
- A `satisfies`-closed table makes a forgotten row column a declaration-site error rather than a downstream `undefined`; adding a key requires its full column set or the `satisfies` check rejects the literal — the contract is the row's totality proof.

```typescript
import { pipe, Record } from 'effect'

const SEVERITY = {
    trace: { rank: 0, halts: false, glyph: '<glyph-a>' },
    warn: { rank: 1, halts: false, glyph: '<glyph-b>' },
    fatal: { rank: 2, halts: true, glyph: '<glyph-c>' }, // adding a row widens keyof typeof + every projection; a row missing a column fails the satisfies here
} as const satisfies Record<string, { readonly rank: number; readonly halts: boolean; readonly glyph: string }>

const halts = (key: keyof typeof SEVERITY): boolean => SEVERITY[key].halts // key set is keyof typeof SEVERITY inline; the rank-union is (typeof SEVERITY)[keyof typeof SEVERITY]['rank'], never a standalone alias
const ranked: Record.ReadonlyRecord<keyof typeof SEVERITY, number> = pipe(SEVERITY, Record.map((row) => row.rank))
```

[SCHEMA_LITERAL_VOCAB]:
- `Schema.Literal(...values)` is type plus value plus codec in one declaration: the static type is the union `values[number]`, the runtime value is a `Schema` that decodes/encodes the union, and `.literals` is a `readonly` tuple preserving the exact element order and arity passed in — the vocabulary, its codec, and its enumerable surface fuse.
- The admitted literal alphabet is `string | number | boolean | null | bigint`; a mixed `Schema.Literal(1, 2n, true, '<x>')` types as `1 | 2n | true | '<x>'`, so a numeric, bigint, and boolean vocabulary is a first-class codec, not only a string one.
- Single-argument `Schema.Literal('<only>')` narrows the schema type to the unit literal `'<only>'`; zero arguments returns `Never` — the arity of the call is the arity of the union, recoverable without a separate type.
- `.literals` is the runtime read that mirrors `keyof typeof` for an `as const` key list: iterating `.literals` enumerates the vocabulary at runtime while `typeof schema.Type` is the closed union at compile time, so a `Schema.Literal` replaces the `as const` key array plus its `keyof typeof` extraction with one owner that also carries the boundary codec.

[SCHEMA_ENUMS_VOCAB]:
- `Schema.Enums(obj)` admits a TS `enum` or an `EnumsDefinition` (`{ [k: string]: string | number }`); the static type is the value union `obj[keyof obj]`, `.enums` exposes the original object, and the codec decodes by VALUE — the keys are not the vocabulary the boundary accepts.
- For a numeric TS `enum`, the runtime constructs the value list by filtering out the reverse-mapping keys (`enums[enums[key]]` of numeric type), so `Schema.Enums(Level)` over `{ Low = 1, High = 2 }` decodes `1 | 2`, never the strings `'Low' | 'High'` and never the reverse-mapped numeric keys — feeding a key name to decode fails.
- The decode failure message enumerates the accepted values, not the keys; a string-valued enum `{ Red = 'red' }` accepts `'red'` and rejects `'Red'` — the source-of-truth at the boundary is the column of values, matching the table semantics where the read targets the row contents.
- `Schema.Enums` is the bridge form for an externally-owned `enum` whose identity must be preserved (`typeof Color.Type` is `Color`); a project-internal vocabulary prefers `Schema.Literal` because the literal tuple is directly enumerable via `.literals` while an enum's value list is only reachable through decode introspection.

[TABLE_VS_MATCH]:
- Data-only rows favor the table: when every case maps a key to inert data the read is a single indexed access `Table[k]`, total over `keyof typeof Table` by construction, with no per-arm function to maintain and no exhaustiveness combinator — the literal key type IS the totality proof.
- Behavior cases favor `Match`: when arms branch on shape beyond a discriminant tag, accumulate refinements, or return divergent computations, `Match` carries the exhaustiveness obligation in its types — `Match.exhaustive` and `Match.tagsExhaustive` reject an uncovered case at compile time where a table can only assert key presence.
- `Match.typeTags<Union>()(handlers)` and `Match.valueTags(handlers)` are the table-shaped face of `Match`: a single handler-row object keyed by `_tag`, exhaustive over the union; `typeTags` binds the input type first and returns a reusable matcher function, `valueTags` is the eager two-argument form `Match.valueTags(input, handlers)` — the handler-row object cannot infer the union from itself, so the input type must arrive via `typeTags<U>()` or the eager value argument.
- A handler-row object passed to a bare `Match.valueTags(handlers)` without the input type resolves each handler parameter to `never` and errors `(_) => _ is not assignable to never` — the trap is supplying handlers before binding the union; `Match.typeTags<U>()(handlers)` is the curried fix.
- The dividing line is whether the per-case payload is data or a continuation: a table where one row later needs a computation is a table with a function column read by `Table[k](arg)`, but once two-plus rows need shape-dependent branching the surface is a `Match` over the union, not a fatter table — the collapse direction is fixed, uniform-return data-keyed arms → function-column table read by indexed access, divergent-return or refinement-accumulating arms → `Match` over the union, never a table of mixed inert and computed columns read for both.

[FUNCTION_COLUMN_DISPATCH]:
- A behavior-column table whose column is a function turns the vocabulary into a dispatch surface read by `Table[k](arg)`; the indexed access selects the arm and immediately applies it, so dispatch is one operator over the literal key, never an if-else or switch.
- The arm union the table dispatches across is `(typeof Table)[keyof typeof Table]['run']` — the union of the function column over every row — so the call-site return type is the union of all arm returns recovered from the table, never restated.
- `as const satisfies Record<string, { readonly run: (a: A) => B }>` closes the arm contract: every row must carry a `run` of the exact arity and parameter type, so a row with a wrong-typed handler fails at the declaration site and the dispatch stays total over the literal key union by construction.
- Indexing a closed function table by a value of its own `keyof typeof` type is total without a fallback arm — the key union IS the case set — so a `default:` branch, an `orElse`, or a `?? throw` at the call site is the absent-case proof the table already carries and is deleted.
- A function column forfeits structural `Equal`/`Hash` and `JSON` projection on the table, so a dispatch table and a data-projection table over the same vocabulary are two reads of the same key set, never one mixed table of inert and computed columns.

[SAME_RETURN_UNIFORM_ARMS]:
- The table is the dispatch surface when every arm returns the SAME type and reads only the dispatched value's own fields; the uniform-return shape is exactly what `Record.map`, `Array.map`, and `foldMap` consume, so a table whose arms all return `B` lifts to a `Record<K, B>` of pre-applied results or maps over a `ReadonlyArray<K>` in one combinator.
- Divergent arm returns force `Match`: when arm A returns `X` and arm B returns `Y` the call-site type is `X | Y`, and the consumer must narrow per branch — `Match.tagsExhaustive` carries that per-arm narrowing in its result type where the table flattens it into a union the consumer re-discriminates.
- Arms that need refinement beyond the discriminant — a range guard, a nested shape check, a predicate over a second field — exceed the table: `Match.when(predicate, f)` and `Match.whenAnd(...patterns, f)` accumulate those refinements as filters the matcher tracks in its residual type, where a table can key on one discriminant only and has no place to carry an accumulated guard.

[HANDLER_ROW_IS_SHARED]:
- `Match.tags`, `Match.tagsExhaustive`, `Match.discriminators(field)`, and `Match.discriminatorsExhaustive(field)` all consume the SAME handler-row object literal a dispatch table is — `{ readonly [Tag]: (_: Extract<R, ...>) => Ret }` keyed by the union's discriminant — so the handler-row table and the `Match` are one object shape, and the choice is whether the library proves exhaustiveness or the literal key union does.
- The `Match` handler-row carries a second guarantee the bare table cannot: its parameter type is `Extract<R, Record<D, Tag>>`, the union member narrowed to that tag, so each arm receives the precisely-narrowed case and a row with the wrong field access fails — a bare `as const` table indexes by key but does not narrow the dispatched value to the per-key payload.
- `Match.tagsExhaustive`/`discriminatorsExhaustive` reject a missing tag at compile time via the `{ [Tag in Exclude<keyof P, Tags>]: never }` intersection in the handler constraint; the bare table asserts only that the keys present are valid, so an incomplete handler-row table compiles while the exhaustive matcher does not — exhaustiveness is the matcher's guarantee, key-presence is the table's.
- `Match.tags` (non-exhaustive) is the partial handler-row consuming the same literal, leaving unmatched tags to a downstream `Match.orElse`/`Match.exhaustive`; a partial dispatch table has no such accumulating residue — an unkeyed access is `undefined` at runtime, so a partial dispatch always reads through the absent-key rail, never a partial bare index.
- `Match.discriminator('<field>')` and `Match.discriminatorsExhaustive('<field>')` move the handler-row dispatch off `_tag` onto any literal-typed field, so a vocabulary whose discriminant column is not the Effect-convention `_tag` dispatches through the field name as the discriminator while keeping the identical handler-row object shape.

[CURRIED_VS_EAGER_DISPATCH]:
- `Match.type<I>()` produces a `Matcher<I, ..., Pr = never>` whose terminal combinator returns a reusable `(input: I) => Result` function — the curried dispatcher built once and applied to many inputs; `Match.value(i)` produces `Matcher<I, ..., Pr = I>` whose terminal combinator returns the Result directly — eager dispatch on the value in hand.
- The `Pr` (provided) type parameter is the discriminant: `[Pr] extends [never]` selects the function-returning overload, otherwise the value-returning one, so the same terminal combinator `Match.exhaustive`/`Match.tagsExhaustive` returns a function under `type` and a value under `value` — the entry choice, not a terminal flag, selects eager vs deferred.
- A reusable dispatcher over a vocabulary is `Match.type<U>().pipe(Match.tagsExhaustive({...}))` defined once at module scope and shared across call sites; the eager `Match.value(u).pipe(Match.tagsExhaustive({...}))` re-builds the matcher per call, so the dispatcher-as-value form mirrors the function-column table's once-defined dispatch surface read everywhere.
- `Match.withReturnType<Ret>()` pins the arm-return type before the handler-row is supplied, constraining every arm to widen to `Ret` and rejecting an arm whose return escapes it — the explicit return contract for a divergent-arm dispatcher, the `Match` analogue of the `satisfies Record<K, { run: (a) => B }>` arm contract that closes a function-column table.

```typescript
import { Array, Match, Number, Record } from 'effect'

type Signal = { readonly _tag: 'open'; readonly at: number } | { readonly _tag: 'close'; readonly code: number } | { readonly _tag: 'idle' }

const COST = {
    open: { weight: 3, run: (s: Extract<Signal, { _tag: 'open' }>) => s.at + 1 },
    close: { weight: 5, run: (s: Extract<Signal, { _tag: 'close' }>) => s.code },
    idle: { weight: 0, run: (_s: Extract<Signal, { _tag: 'idle' }>) => 0 },
} as const satisfies Record<Signal['_tag'], { readonly weight: number; readonly run: (s: never) => number }>

const loadOf = (xs: ReadonlyArray<Signal>): number => Number.sumAll(Array.map(xs, (s) => COST[s._tag].weight)) // data-column read COST[s._tag].weight inline in a fold, total over the tag union
const score = Match.type<Signal>().pipe(Match.tagsExhaustive({ open: (s) => s.at + 1, close: (s) => s.code, idle: () => 0 })) // per-arm narrowing to Extract<Signal, tag> lets each arm read its own field; the bare table cannot narrow the dispatched value
const weights: Record.ReadonlyRecord<Signal['_tag'], number> = Record.map(COST, (row) => row.weight)
```

[STRUCT_POINT_FREE_DISPATCH]:
- `Struct.get('<col>')` is the point-free column extractor checked against the row shape: `(s) => s[col]` typed `MatchRecord<S, S[K] | undefined, S[K]>`, so a single-row read is `pipe(Table[k], Struct.get('rank'))` and lifting a whole column off every row composes as `Array.map(Record.values(Table), Struct.get('run'))` without a per-row arrow, the extracted arms feeding a fold or an applicative batch.
- The point-free column read keeps the row's narrow type: `Struct.get('weight')` over a row typed `{ readonly weight: 3 }` yields `3`, not `number`, so a literal weight column extracted point-free stays a literal usable as a discriminant or a closed-union member — the extractor never widens what indexed access preserves.
- A dispatch table read for BOTH a data column and a behavior column over one key uses `Struct.get` twice over the same row — `pipe(Table[k], Struct.get('weight'))` and `Table[k].run(arg)` — so the single owner serves the projection read and the dispatch read from one literal, the data column and the function column never split into two tables.
- `Struct.keys(Table)` returns `Array<(keyof typeof Table) & string>`, the same closed-key enumeration as `Record.keys` but typed against an arbitrary struct, so the vocabulary's key list is recoverable from either the record surface or the struct surface with identical element typing.
- `Struct.evolve(row, { col: f })` transforms named columns of a single row while leaving the rest, returning the evolved row type — a per-row column rewrite that keeps the unlisted columns at their literal types, where mapping the whole table is `Record.map(Table, (row) => Struct.evolve(row, { rank: (r) => r + 1 }))` preserving `K`.
- `Struct.getOrder({ col: Order.X })` and `Struct.getEquivalence({ col: Equivalence.X })` build an `Order`/`Equivalence` over the ROW shape from per-column instances; composed with `Order.mapInput(rowOrder, (k) => Table[k])` they lift a multi-column row order to an order over the vocabulary keys — the row-level instance and the key-level instance derive from the same column instances.

[DISPATCH_TABLE_FOLD]:
- A dispatch over a COLLECTION of vocabulary keys folds the per-key arm results through a `Monoid`: `Array.foldMap(Monoid)(keys, (k) => Table[k].run(arg))` reduces the dispatched results in one pass, so summing weights, concatenating outputs, or merging records across a key list is the monoid of the result type combined with the table read, never an accumulator mutated per key.
- The arm-return type IS the carrier the fold reduces, so a uniform-return dispatch table is foldable exactly when its return type has a `Monoid` — `number` under sum, `string` under concat, a record under struct-merge — and a divergent-return dispatch is not foldable, which is the same boundary that forces it to `Match`; foldability and uniform-return coincide.
- A dispatch table whose arms return `Effect` composes across a key collection through `Effect.forEach(keys, (k) => Table[k].run(arg))` for the batch and `Effect.all` over the pre-applied arms for the independent set, so the dispatch surface scales from one key to many keys to a concurrent batch by the combinator wrapping the same `Table[k].run` read, never by a second plural-named dispatch entrypoint.
- The fold over the dispatch table is the executable specification of the aggregate: adding a vocabulary row extends both the per-key dispatch and the collection fold with zero fold edits, because the fold reads the table's key set and the monoid carries the combine — the row is the only source the aggregate derives from.

[KEY_PRESERVING_TRANSFORM]:
- A value-only transform over the table preserves the closed key union: `Record.map(Table, (row) => row.col)` types as `Record<K, B>` where `K` is the original literal key union, so the projected column-table is still read by `keyof typeof` and indexed access with no widening — the rank-only or cap-only view of the vocabulary keeps the vocabulary's key set.
- The transform that erases keys unconditionally collapses the literal union to `string`: `Record.fromEntries`, `Record.filterMap`, `Record.filter`, and `Record.getSomes` return `Record<ReadonlyRecord.NonLiteralKey<K>, B>`, and `NonLiteralKey<K>` is `K extends string ? IsFiniteString<K> extends true ? string : K : symbol`, so a finite literal key widens to `string` and the closed vocabulary degrades to an open record.
- The key-mapping transform widens only when its function does: `Record.mapKeys` and `Record.mapEntries` return `Record<K2, B>` keyed by the function's RETURNED key type, so a function returning a literal preserves `K2` while one returning a widened key (`key.toUpperCase()`) collapses to `string` — the key-producing function, not the combinator, decides the widening.
- The widening is structural, not nominal: round-tripping a literal-key table through `Record.fromEntries(Record.toEntries(Table))` yields a binding whose `keyof typeof` is `string`, defeating exhaustiveness on every downstream `Match` or indexed read keyed on it — the round-trip silently erases the totality proof the `as const` table existed to carry.
- `Record.keys(Table)` is the runtime enumeration that preserves the union: its element type is `Array<K & string>`, the array-shaped mirror of `keyof typeof Table`, so iterating the vocabulary at runtime stays typed by the closed key union while `Record.fromEntries` of the same entries does not — enumeration preserves, reconstruction widens.
- A column subset that must keep the key union is derived with `Record.map`, never `Record.filterMap`: even when the projection drops a column, mapping each row to a smaller record keeps `K`, whereas `filterMap` returning `Option` per entry widens to `NonLiteralKey<K>` because the surviving key set is no longer statically known — drop columns inside the value position, never the key position.

[COLUMN_AS_INSTANCE]:
- A behavior column projects to a typeclass instance keyed by the vocabulary, not a comparator written by hand: `Order.mapInput(Order.number, (k) => Table[k].rank)` is an `Order<K>` that ranks vocabulary keys by their rank column, and the rank column IS the total order — adding a row extends the order with zero comparator edits because the comparator reads the table, never an inlined switch.
- The dual on the equality side is `Equivalence.mapInput(Equivalence.string, (k) => Table[k].group)`: an `Equivalence<K>` where two keys are equivalent exactly when they share a grouping column, so the column defines the partition and the partition is recomputed from the table whenever a row's column changes — equivalence-by-column never drifts from the data.
- `mapInput` is the single contravariant lever for both: `Order.mapInput<A, B>(self: Order<A>, f: (b: B) => A): Order<B>` and `Equivalence.mapInput<A, B>(self, f: (b: B) => A): Equivalence<B>` pull the base instance back along the column projection, so `number`-ordering and `string`-equality over raw columns become `K`-ordering and `K`-equality without a bespoke `Order.make` or `Equivalence.make` — the base instance carries the algebra, the projection carries the table read.
- Multi-column tie-breaking composes with `Order.combine`: `Order.combine(Order.mapInput(Order.number, (k) => Table[k].rank), Order.mapInput(Order.string, (k) => Table[k].label))` orders by rank then label as a single `Order<K>`, and `Order.reverse` flips it — a multi-key sort over the vocabulary is two column reads combined, never a comparator with nested conditionals.
- The projected `Order<K>` and `Equivalence<K>` feed the collection combinators directly: a `ReadonlyArray<K>` sorts with the rank-derived order and dedupes with the group-derived equivalence, so the vocabulary's columns drive ordering and grouping across the program from one definition site.

```typescript
import { Order, Equivalence, Record, pipe } from 'effect'

const TIER = {
    bronze: { rank: 0, cap: 10, group: '<group-a>' },
    silver: { rank: 1, cap: 50, group: '<group-a>' },
    gold: { rank: 2, cap: 200, group: '<group-b>' }, // a new tier extends byRank, sameGroup, capByKey, and ranked at once with zero edits below
} as const satisfies Record.ReadonlyRecord<string, { readonly rank: number; readonly cap: number; readonly group: string }>

const byRank: Order.Order<keyof typeof TIER> = Order.mapInput(Order.number, (k) => TIER[k].rank)
const sameGroup: Equivalence.Equivalence<keyof typeof TIER> = Equivalence.mapInput(Equivalence.string, (k) => TIER[k].group)
const capByKey: Record.ReadonlyRecord<keyof typeof TIER, number> = Record.map(TIER, (row) => row.cap)
const ranked: ReadonlyArray<keyof typeof TIER> = pipe(Record.keys(TIER), (ks) => [...ks].sort(byRank))
```

[REVERSE_INDEX]:
- The forward read `Table[k].id` is direct indexed access; the inverse read id → key is a derived reverse index built once at definition time, never a linear scan at each call site — and it is the rare place the key union is intentionally rebuilt, so its construction uses the widening transform on purpose.
- `Record.fromEntries(Record.toEntries(Table).map(([k, row]) => [row.id, k]))` builds the value→key map; the widening to `Record<string, K>` is correct here because the new keys are the id column, not the original vocabulary keys — the values retain the closed key union `K`, so the reverse lookup returns a typed vocabulary key.
- The reverse index is a derived owner, not a second source: it is computed from the forward table, so a renamed key or a changed id reshapes the reverse map automatically and a duplicate id column collapses two keys to one entry at construction, surfacing the collision as a smaller map rather than a runtime ambiguity downstream.
- A reverse lookup that must report absence reads through `Record.get(reverseIndex, id)` returning `Option<K>` carrying a typed vocabulary key on the `Some` arm, so an unmatched id is `Option.None` and the lookup never re-validates because the forward table is the closed source it derives from.

[PARTIAL_TABLE_READ]:
- A read keyed by a value WIDER than the table's key union — an arbitrary string from the boundary, a union the table covers only partially — is `Record.get(Table, key)` returning `Option.Option<Row>`, never a bare bracket access typed `Row` that is `undefined` at runtime; the partial vocabulary read carries absence as `Option`, not a lie about totality.
- `Record.get`'s key parameter is `NoInfer<K>` constrained to the table's literal `keyof typeof`, so a key outside the vocabulary is rejected at compile time when the key is itself literal-typed; the `Option` rail is for keys whose literal membership cannot be proven statically — the runtime-decided key from outside the closed source.
- `Record.has(Table, key)` is the boolean membership probe and `Predicate.isTagged(value, '<tag>')` narrows a union value to the member carrying that tag at runtime, so admitting a foreign discriminant into the dispatch is `isTagged` to narrow then total indexed access on the narrowed key — the foreign value is refined to a vocabulary key once at the seam, never re-checked per column read.
- A dispatch over a value whose key membership is unproven composes as `Record.get(Table, k)` then `Option.map((row) => row.run(arg))`, so an absent key short-circuits to `Option.None` and never enters an arm; the absence is one rail, not a thrown lookup or a sentinel arm in the table.

[SCHEMA_FIELD_VOCAB]:
- A `Schema.Struct` exposes `.fields` as the readonly field map and `.pick(...keys)` / `.omit(...keys)` as derived sub-structs typed by `Pick`/`Omit` over the fields — a column subset of a schema-backed vocabulary is one `.pick` call, never a re-declared struct, and the picked schema is itself a codec for the projection.
- A field typed `Schema.Literal(...)` inside a struct makes that field a closed vocabulary at the boundary, and `someStruct.fields.col` is the standalone field schema — the same `Schema` value the field decodes through, reusable on its own as the column codec without re-spelling a separate `Schema.Literal`.
- `someStruct.fields.col.literals` reads the field's enumerable alphabet at runtime through the field schema, so the composite struct owns both the whole-row codec and each column's runtime vocabulary read from one declaration — the field-level enumeration travels with the struct, never a sibling literal list beside it.

[TRANSFORM_LITERALS_CODEC]:
- `Schema.transformLiterals(...pairs)` fuses a two-column remap table into one bidirectional codec: each pair `[encoded, decoded]` declares a literal-to-literal mapping, decode rewrites the wire literal to the domain literal and encode reverses it, so the encoded-side union is the first column and the decoded-side union is the second — the entire remap vocabulary is the argument list, type plus value plus invertible codec.
- The `const` type parameter preserves every pair literal: the result is `Union<{ ... transformLiteral<to, from> }>` whose `Type` is the second-column union and whose `Encoded` is the first-column union, so a wire-protocol alphabet decoding to a domain alphabet is one declaration replacing a paired `Schema.Literal` plus a hand-rolled `Schema.transform` mapping table.
- A single pair `Schema.transformLiterals(['<wire>', 'domain'])` types as `transformLiteral<'domain', '<wire>'>` — unit-to-unit remap; the multi-pair form is the closed remap vocabulary, and a wire value outside the encoded column fails decode with a `ParseResult.ParseError` whose message enumerates the accepted encoded literals — the encoded column is the admitted boundary alphabet.
- The remap is the executable specification of the boundary-to-domain vocabulary correspondence: adding a protocol value plus its domain meaning is one new pair, both unions widen together, and no decode arm, encode arm, or reverse map is written by hand — the pair list is the only source.

```typescript
import { Schema, type ParseResult, type Effect } from 'effect'

const Channel = Schema.transformLiterals( // Type 'primary'|'replica'|'archive'; Encoded '<wire-a>'|'<wire-b>'|'<wire-c>'; adding a pair widens both unions together
    ['<wire-a>', 'primary'],
    ['<wire-b>', 'replica'],
    ['<wire-c>', 'archive'],
)
const toDomain: (i: typeof Channel.Encoded) => Effect.Effect<typeof Channel.Type, ParseResult.ParseError> = Schema.decode(Channel)
const toWire: (a: typeof Channel.Type) => Effect.Effect<typeof Channel.Encoded, ParseResult.ParseError> = Schema.encode(Channel)
```

[UNION_FLATTENS_VOCABULARIES]:
- `Schema.Union(...members)` is the closed combine over vocabulary owners: its type is `Schema.Type<Members[number]>` and `.members` is the runtime member list, so unioning two `Schema.Literal` vocabularies yields one schema whose alphabet is the flattened union of both literal tuples — the combine of two closed vocabularies is itself a closed vocabulary, not a nested wrapper.
- The combinator self-normalizes through `AST.Union.make`: zero members returns `Never`, a single member returns that member unwrapped (the `Union<Member>(member: Member): Member` overload), and nested unions flatten — so `Schema.Union(Schema.Union(A, B), C)` and `Schema.Union(A, B, C)` produce the same flat alphabet with no intermediate union node to peel.
- The variadic combine is constrained `Members extends AST.Members<Schema.All>` where `Members<A>` is `readonly [A, A, ...Array<A>]`, so the multi-member overload requires two-or-more members at the type level; a spread that the checker cannot prove non-empty falls to the widened `Schema<Type<Members[number]>, ...>` overload that drops the `.members` accessor — the precise `.members` read survives only when the member count is statically two-plus.
- `.members` is the runtime enumeration of the composed vocabulary's constituents, the union-shaped analogue of a `Literal`'s `.literals`: iterating `.members` walks the contributing schemas while `typeof U.Type` is the flattened closed union at compile time — the composite owns both the merged codec and the per-constituent schema list from one declaration.
- Duplicate literal members across the unioned vocabularies collapse at AST normalization, so `Schema.Union(Schema.Literal('<a>', '<b>'), Schema.Literal('<b>', '<c>'))` decodes the three-element alphabet without a doubled `'<b>'` arm — the combine de-duplicates the literal vocabulary rather than carrying redundant decode branches.

[EXTEND_IS_INTERSECTION_ASYMMETRY]:
- `Schema.extend(Self, That)` carries an asymmetry the union does not: its `Type` is `Schema.Type<Self> & Schema.Type<That>`, its `Encoded` is `Schema.Encoded<Self> & Schema.Encoded<That>`, and its `Context` is `Schema.Context<Self> | Schema.Context<That>` — type and encoded intersect, requirement unions, so extending a struct vocabulary with another struct merges field sets while the `R` set accumulates every contributing requirement.
- `extend` over two struct vocabularies is the row-widening combine: a base struct extended with a second struct's fields is one schema carrying both column sets, so adding a column family to a schema-backed vocabulary is one `extend` rather than a re-declared struct restating the original columns — the union of two field vocabularies, not a parallel third.
- The supported extension shapes are bounded by AST, not arbitrary: a struct extends with an index signature, with a union of supported schemas, with a refinement of a struct, with a suspend of a struct, or with a struct-to-struct transformation whose from/to sides do not collide — extending two arbitrary schemas is rejected, so `extend` is the field-and-index composition operator, never a general merge.
- Extending a struct with `Schema.Record({ key, value })` adds an index signature to the closed field vocabulary, fusing the named-column vocabulary with an open key family in one schema; the named columns keep their literal field types while the index signature admits the open tail — the closed-plus-open vocabulary is one owner, never a struct beside a separate record.
- A field-name collision across the two extended structs is the rejection point, not a silent last-wins: `extend` requires the overlapping field's from/to sides to agree, so a column the two vocabularies both declare with divergent codecs fails at the `extend` call rather than producing an ambiguous merged decode — the collision surfaces at composition, the same place the as-const spread's later-wins silently overwrites.

[PICKLITERAL_NARROWS_THE_ALPHABET]:
- `Schema.pickLiteral(...subset)` is the closed narrow over a literal vocabulary: applied to a `Schema<A, ...>` it returns a fresh `Literal<[...L]>` whose `.literals` is the narrowed tuple and whose codec rejects any value outside the subset, so a sub-vocabulary is derived from the wider owner as a piped refinement — the subset never re-spells a second `Schema.Literal`.
- The subset is constrained `L extends array_.NonEmptyReadonlyArray<A>` where `A` is the source literal type, so `pickLiteral` admits only members already in the source alphabet; a literal outside the wider vocabulary is a compile error at the narrow site — the narrow cannot widen, and the sub-vocabulary is provably a subset of its source.
- `pickLiteral` chains as a closed operation: `Schema.Literal('<a>', '<b>', '<c>').pipe(Schema.pickLiteral('<a>', '<b>'), Schema.pickLiteral('<a>'))` successively narrows to the unit `'<a>'` vocabulary, each step a fresh `Literal` carrying its own `.literals` — the narrow is iterable, and each intermediate is a usable codec at its own arity.
- The narrowed `Literal` is a fresh owner forked from the source, not an alias: it carries its own `.literals` enumeration and its own annotations independent of the parent, so a sub-alphabet decoded at one boundary annotates its own decode message and identifier and fails a domain-valid value the seam forbids with only the narrowed alphabet enumerated in its `ParseError` — each boundary forks its accepted sub-alphabet from the single source, never a parallel allowlist re-listing the permitted members.

[KEYOF_DERIVES_A_KEY_SET_CODEC]:
- `Schema.keyof(struct)` returns `SchemaClass<keyof A>`, a codec whose admitted alphabet is the struct vocabulary's key set: the column names of a schema-backed table become a first-class decodable vocabulary, so a boundary value naming a column decodes through `Schema.keyof(Table)` to a typed key, never a raw string re-checked against the field list.
- The key-set codec is the boundary-facing dual of `keyof typeof` over an `as const` table: where `keyof typeof Table` is the compile-time key union of a structural table, `Schema.keyof(Struct)` is the runtime codec that admits exactly those keys, so a foreign string naming a vocabulary column is admitted once through the key-set decode and travels inward as a typed key.
- `Schema.keyof` derives from the struct in one hop, so adding a field to the struct vocabulary widens the key-set codec's admitted alphabet with zero edits to the codec — the accepted key vocabulary is recomputed from the field set, never a parallel `Schema.Literal` of column names drifting from the struct it describes.
- The decode of a non-key string fails with the field-set enumerated, so feeding an absent column name to `Schema.keyof(Table)` is a `ParseError` naming the valid keys — the absence is a typed boundary failure, the same totality the structural `keyof typeof` carries at compile time, now enforced at the runtime seam.

[STRUCTURAL_SPREAD_VS_CODEC_COMPOSE]:
- The `as const` spread is the structural combine of two decode-free vocabularies: `{ ...TableA, ...TableB } as const` merges row sets and re-freezes every leaf to its literal, so `keyof typeof` over the merged binding is the union of both key sets — the combine of two structural tables is a structural table, no codec on the path.
- The spread and the codec-backed `extend`/`Union` diverge on collision: the spread silently last-wins when both tables carry a key, overwriting the earlier row's columns, while `extend` rejects a divergent overlapping field and `Union` de-duplicates identical literal members — the structural spread trades collision-safety for zero dependency, so the spread combine demands the merged tables share no key or share identical rows.
- The spread combine cannot cross the decode boundary: the merged `as const` table is a runtime read by indexed access only, with no codec, so a boundary value selecting a merged row must first decode through a separate `Schema.keyof` or `Schema.Literal` over the merged key union — the structural combine owns the read, the codec owns the admission, and the two stay orthogonal over one merged key set.
- The decision is dependency versus admission: a vocabulary read entirely inside the trusted interior composes through the `as const` spread for zero library on the read path; a vocabulary whose alphabet a boundary supplies composes through `Schema.Union`/`extend` so the merged alphabet carries its own decode — the same combine, selected by whether a foreign value ever names a row.

```typescript
import { Schema, pipe } from 'effect'

const Action = Schema.Union(Schema.Literal('<a>', '<b>'), Schema.Literal('<c>', '<d>')) // Type '<a>'|'<b>'|'<c>'|'<d>'; .members survives only when the member count is statically two-plus
const members: ReadonlyArray<Schema.Schema<string>> = Action.members
const InternalOnly = pipe(Action, Schema.pickLiteral('<a>', '<b>')) // Type '<a>'|'<b>', a fresh Literal forked from the source, never a parallel allowlist

const Row = Schema.Struct({ rank: Schema.Number, halts: Schema.Boolean, glyph: Schema.String })
const ColumnName = Schema.keyof(Row) // Type 'rank'|'halts'|'glyph'; adding a field widens the admitted key alphabet with zero codec edits
const admitColumn: (raw: string) => Schema.Schema.Type<typeof ColumnName> = Schema.decodeUnknownSync(ColumnName)
```

[ANNOTATION_AS_VOCABULARY_METADATA]:
- A codec-backed vocabulary carries presentation and validation metadata inline through `.annotations({...})`, the lever the `as const` table lacks: `Schema.Literal('<a>', '<b>').annotations({ identifier: '<id>', title: '<t>' })` fuses the alphabet with its `title`, `description`, `documentation`, `examples`, `default`, `identifier`, `message`, `jsonSchema`, `arbitrary`, `pretty`, and `equivalence` — the per-vocabulary metadata column lives on the owner, not in a sibling lookup keyed by the same alphabet.
- `Annotations.Schema<A>` is the typed metadata contract: `examples` is `ExamplesAnnotation<A>` and `default` is `DefaultAnnotation<A>`, both parameterized by the vocabulary's own type, so an example or default outside the literal alphabet fails at the annotation site — the metadata column is checked against the vocabulary it annotates, never free-form.
- `annotations` merges rather than replaces, overwriting only the keys supplied, so a vocabulary annotated at declaration with `identifier` and later piped through `.annotations({ message })` keeps the identifier and gains the message — metadata accretes on the owner as the vocabulary's concern grows, the same accretion the behavior table grows by adding a column.
- The `message` annotation makes a decode failure speak the vocabulary's own diagnostic instead of the generic literal-enumeration message, so a boundary rejecting a foreign value reports the vocabulary's intended error — the failure text is a metadata column on the alphabet, fused with the codec, never a catch-site re-message after a generic parse error.
- The metadata column is the codec-path payload the structural `as const` table forfeits: a behavior-column table read by indexed access carries arbitrary data columns but no decode message, no example, no `jsonSchema`; once a vocabulary needs a boundary diagnostic or a schema-export projection the owner is the annotated codec, and the metadata rides the same declaration as the alphabet.

[CROSSING_IS_NARROWING]:
- The forward read `Table[k]` assumes `k` is already a vocabulary key; the admission gate is the prior step where a value of WIDER type crosses into a key, and the gate's whole job is the narrowing from `unknown`/`string` to the closed key union — never a re-validation, the crossing happens exactly once and the interior reads the narrowed value with no further check.
- `Schema.is(Vocab)` is the gate's guard form: `(u: unknown, options?) => u is A` over a codec-backed vocabulary, returning the predicate that narrows a foreign value to the vocabulary type in place, so a downstream `if`/filter/`Match.when` reads the narrowed member — the guard the `as const` table cannot synthesize because its key set is a compile-time fact with no runtime predicate.
- `Schema.asserts(Vocab)` is the throwing dual: `(u: unknown, options?) => asserts u is A`, narrowing the variable for the rest of the lexical scope at the cost of an exception on a non-member — the asserts-narrows-the-binding form for a measured kernel where the foreign value is trusted by construction, never the domain-flow rail.
- The `as const` table's runtime membership probe is `Record.has(Table, key)` (boolean) and the narrowing read is `Record.get(Table, key)` returning `Option<Row>`; the codec-backed vocabulary's probe is `Schema.is(Vocab)` (typed guard) and its narrowing read is a decode — the structural table answers presence, the codec answers admission, and the gate is whichever the foreign value's provenance demands.

[ABSENCE_CARRIER_SELECTS_THE_FORM]:
- The decode family is one operation with the carrier picked by where a non-member goes: `Schema.decodeUnknown(Vocab)` carries the `ParseError` on the typed failure channel of `Effect<A, ParseError, R>`; `Schema.decodeUnknownOption(Vocab)` discards the error and carries absence as `Option<A>`; `Schema.decodeUnknownEither(Vocab)` carries `Either<A, ParseError>`; `Schema.decodeUnknownSync(Vocab)` throws — the same admission, four absence rails, selected by the consumer's algebra not a flag.
- `decodeUnknown*` admits from `unknown`; `decode*` admits from the schema's `Encoded` type — the latter trusts the value already has the wire shape and only runs the transform, so a vocabulary whose `Encoded` differs from its `Type` (a `transformLiterals` remap, a `NumberFromString` column) gates a typed wire value through `decode` and an arbitrary boundary value through `decodeUnknown`.
- The `Option`-returning decode forms (`decodeUnknownOption`, `decodeOption`) constrain the schema to `Schema<A, I, never>`, erasing the `R` requirement to `never` and the error to `Option.None`, so a context-free vocabulary admitting a foreign value where the rejection reason is irrelevant is `decodeUnknownOption` — the absence is `None` carried inward, never a `ParseError` caught and discarded at a try/catch. `validateOption` keeps `R` (`Schema<A, I, R>`), so re-asserting an interior value against a context-carrying vocabulary stays in the `R`-aware family while still discarding the reason to `Option`.
- `validate*` admits a value already of type `A` and re-checks it against the schema's own constraints (filters, refinements) WITHOUT running the decode transform, so re-asserting an interior value still satisfies a narrowed vocabulary is `Schema.validateOption(Sub)(value)` — distinct from `decode`, which would re-run the transform the value already passed.

[LITERAL_ALPHABET_DRIVES_MATCH_IS]:
- `Match.is(...literals)` is the literal-vocabulary refinement built into dispatch: `(...literals: Literals) => SafeRefinement<Literals[number]>` where `Literals extends ReadonlyArray<string | number | bigint | boolean | null>`, so a vocabulary's alphabet passed directly to `Match.is` becomes a `Match.when(Match.is(...alphabet), f)` arm that narrows the matched value to that sub-alphabet — the admission predicate and the dispatch arm fuse, no separate guard.
- `Match.when` accepts a `Schema.is(Vocab)`-shaped refinement as its pattern position (`Types.PatternPrimitive<R>` admits a `Refinement`), so a codec-backed vocabulary gates an arm by `Match.when(Schema.is(Vocab), (v) => ...)` where `v` is narrowed to the vocabulary type inside the arm — the boundary admission and the branch selection are one combinator, and the matched-out residue carries the complement type forward.
- `Match.is` over a literal subset and `Match.not(pattern, f)` are complementary partitions of the alphabet: `Match.is('<a>', '<b>')` narrows to the named members while `Match.not('<a>', f)` narrows to everything except, so a vocabulary split into an admitted sub-alphabet and its complement is two arms over the same literal column, each carrying its narrowed type — the partition the bare table expresses only as a `keyof typeof` and its `Exclude`.
- `Match.discriminator(field)('<v1>', '<v2>', f)` admits a foreign value by its literal-typed discriminant COLUMN rather than `_tag`, narrowing to `Extract<R, Record<field, '<v1>' | '<v2>'>>`, so a vocabulary whose rows discriminate on a non-`_tag` literal column gates the multi-member arm through that column name — the same admission the `_tag`-keyed `Match.tag('<v1>', '<v2>', f)` performs on the conventional discriminant.

[PARTIAL_ADMISSION_TERMINALS]:
- A `Match` over a vocabulary that does NOT cover every foreign value chooses its residual carrier at the terminal: `Match.option` returns `(input) => Option<A>` putting an unmatched value in `Option.None`, `Match.either` returns `(input) => Either<A, R>` carrying the unmatched residual type `R` in `Either.Left`, and `Match.orElse(f)` supplies the total fallback arm — the unmatched complement is a typed residue, never a runtime `undefined` from a bare partial index.
- `Match.either`'s `Left` type is the matcher's residual `R` — the union of cases not yet handled, computed by the checker as arms subtract from the input union — so a partial vocabulary admission surfaces exactly which members remain uncovered in the `Left` type, the type-level analogue of the `as const` table's `Exclude<keyof typeof Table, Covered>`.
- `Match.exhaustive` requires the residual `R` to be `never` — `(self: Matcher<I, F, never, ...>)` — so a gate claiming totality over a vocabulary compiles only when every member is matched, and adding a member to the alphabet breaks the exhaustive matcher loudly at compile time; `Match.orElseAbsurd` is the same totality proof asserting the residue is impossible at runtime.
- The carrier choice mirrors the decode family exactly: `Match.exhaustive`↔`decodeSync` (total, throws on impossible), `Match.option`↔`decodeOption` (absence as `Option`), `Match.either`↔`decodeEither` (residual as `Either.Left`), so a vocabulary admission expressed as dispatch and one expressed as decode share the same absence algebra under different owners — the gate is one concept, the owner is the value's provenance.

[REJECTION_IS_A_STRUCTURED_VALUE]:
- The non-member rejection a vocabulary owns is a `ParseError` whose `issue: ParseIssue` is a tagged tree — `Type`, `Composite`, `Refinement`, `Transformation`, `Pointer`, `Unexpected`, `Missing`, `Forbidden` — so the rejection is itself a discriminable value carried on the failure channel, not a string thrown and re-parsed; a vocabulary's gate fails with the same algebra its successes flow through.
- `ParseResult.TreeFormatter` renders the issue to a single indented string and `ParseResult.ArrayFormatter` renders it to `ReadonlyArray<{ _tag, path, message }>` where `path` is the `ReadonlyArray<PropertyKey>` locating the offending column — so a vocabulary's rejection report is one formatter call over the structured issue, and the per-column path is recovered without a hand-built error message.
- The `message` annotation on the vocabulary owner (`Mode.annotations({ message: () => '<msg>' })`) replaces the generic literal-enumeration text in the rendered `ParseError`, so the gate's diagnostic is a metadata column on the alphabet rather than a catch-site re-message — the rejection speaks the vocabulary's own diagnostic at the seam that admits it.
- `ParseOptions.errors: 'all'` switches the gate from first-failure to full-failure: a struct-shaped vocabulary admitting a row reports every bad column in one `ParseError` instead of halting on the first, and `onExcessProperty: 'error'` rejects a foreign value carrying columns outside the vocabulary's field set — the admission strictness is a decode option, never a second validation pass.

[GATE_FEEDS_THE_DERIVED_READ]:
- A foreign string naming a vocabulary key admits through `Schema.keyof(Table)` to a typed key, then the SAME value indexes the `as const` table — `Schema.decodeUnknownOption(Schema.keyof(Row))(raw)` yields `Option<Column>` and `Option.map((k) => DATA[k].cap)` reads the data column — so the codec gates admission and the structural table owns the read, the two orthogonal over one key set with no second source.
- The gate narrows once and the projection family reads the narrowed key with zero re-checking: an admitted `Mode` flows into the rank-derived `Order<Mode>`, the group-derived `Equivalence<Mode>`, the function-column dispatch `Table[k].run(arg)`, and the column projection `Table[k].cap` — every derived surface consumes the gated value because admission proved membership, and re-validating inside any of them is the redundant hop the gate exists to delete.
- A boundary admitting only a sub-alphabet gates through the narrowed vocabulary, not the wide one: `Schema.decodeUnknownOption(pipe(Wide, Schema.pickLiteral('<a>', '<b>')))` rejects a domain-valid value the seam forbids, so the admission alphabet is derived from the source by `pickLiteral` and the rejection enumerates only the seam's accepted members — the gate's accepted set is recomputed from the source, never a hand-written allowlist beside it.
- The admitted value's type is recovered from the gate, never restated: `Schema.Schema.Type<typeof Mode>` is the narrowed union the guard/decode produces, so a consumer signature typed by the gate's output derives from the vocabulary owner — a parallel `type AdmittedMode = ...` mirroring what `decodeUnknown(Mode)` already yields is the restatement the extraction deletes.

```typescript
import { Match, Option, Schema, type ParseResult, type Effect } from 'effect'

const Mode = Schema.Literal('<a>', '<b>', '<c>').annotations({ identifier: '<id>', message: () => '<msg>' })

const admitMode: (u: unknown) => Option.Option<typeof Mode.Type> = Schema.decodeUnknownOption(Mode)
const isMode: (u: unknown) => u is typeof Mode.Type = Schema.is(Mode)
const decodeMode: (u: unknown) => Effect.Effect<typeof Mode.Type, ParseResult.ParseError> = Schema.decodeUnknown(Mode)

const route = Match.type<unknown>().pipe(
    Match.when(Match.is(...Mode.literals), (m) => m),
    Match.option,
)
```

[DEFAULT_IS_AN_ALPHABET_MEMBER]:
- Resolution differs from admission: admission carries a non-member as absence on a rail, resolution maps a missing or out-of-alphabet key to a DESIGNATED member of the same closed vocabulary, so the resolved value re-enters the closed key union and every downstream projection, order, and dispatch reads it without a second check.
- The fallback member is checked to be in the alphabet because the default surface is parameterized by the vocabulary's own type: `Schema.optionalWith(Vocab, { default })` types its default as `LazyArg<Schema.Type<S>>`, so a default literal outside the vocabulary alphabet fails at the field declaration — the fallback cannot name a value the vocabulary forbids.
- The annotation form is the same constraint at the AST: `Schema.Literal('<a>', '<b>').annotations({ default })` carries `DefaultAnnotation<A>` which resolves to `A` itself, so the annotated default is the bare member value checked against the literal union — a default off the alphabet is rejected at the annotation site, never a free-form fallback.
- A resolution that needs a member NOT in the vocabulary is the signal the alphabet is wrong, not that the default should widen: the fallback member is added as a vocabulary row first — one literal in the `Schema.Literal` tuple or one row in the `as const` table — then named as the default, so the alphabet and its designated fallback grow from the one owner.

[OPTIONAL_WITH_DEFAULT_FLIPS_THE_TOKEN]:
- `Schema.optionalWith(Vocab, { default })` is the codec-fused resolution: the field is optional on the `Encoded` side (token `"?:"`, the wire may omit it) and required-with-resolved-value on the `Type` side (token `":"`), because `Types.Has<Options, "as" | "default"> extends true ? ":" : "?:"` flips the Type token to required when `default` is present — the decode resolves a missing key to the fallback member and the interior field is never `undefined`.
- The presence of `default` also strips `undefined` from the Type: the optionalWith Type adds `undefined` only when `Types.Has<Options, "as" | "default" | "exact">` is false, so a defaulted field's decoded type is exactly the vocabulary union with no `undefined` arm — the resolution is total at the codec, the consumer reads a closed member.
- `exact: true` narrows what counts as missing: without it an explicit `undefined` on the wire also triggers the default, with it only an absent key does and an explicit `undefined` fails decode — the resolution trigger is a decode option, never a branch counting `undefined` versus missing at the read.
- `nullable: true` adds `null` to the accepted `Encoded` alphabet that resolves to the default, so a wire vocabulary that signals "use default" with `null` resolves through the same field — the null-to-default mapping is a decode knob on the field, not a `?? DEFAULT` after a nullable read.

[DECODING_VS_CONSTRUCTOR_DEFAULT_SPLIT]:
- The default applies on different paths depending on the combinator, and the split is load-bearing: `Schema.withConstructorDefault(ps, () => member)` resolves only at the validating constructor (the `make` path) and leaves `Type`, `Encoded`, and both tokens untouched, so building a value omits the field while decoding a wire value still requires it.
- `Schema.withDecodingDefault(ps, () => member)` resolves only at decode and flips the source `"?:"` to a required `":"` Type with `undefined` excluded, so a missing wire key resolves to the member but the constructor still demands it — the dual of the constructor default.
- `Schema.withDefaults(ps, { constructor, decoding })` carries both functions so the fallback member resolves on both paths from one declaration; the two functions are separate so the make-time fallback and the wire-time fallback may differ when the boundary's missing-key meaning differs from the builder's omitted-field meaning.
- The chain is direction-fixed: `withDecodingDefault` and `withDefaults` require the source property signature to already be optional (`PropertySignature<"?:", ...>`), so the lift order is `Schema.optional(Vocab).pipe(Schema.withDecodingDefault(() => member))` — applying the default to a required signature is a type error, the optionality is the precondition the default consumes.

```typescript
import { Schema } from 'effect'

const Record_ = Schema.Struct({ // Type { mode: '<a>'|'<b>'|'<c>'; retries: number }; Encoded { mode?: '<a>'|'<b>'|'<c>'|null|undefined; retries?: number|undefined }
    mode: Schema.optionalWith(Schema.Literal('<a>', '<b>', '<c>'), { default: () => '<a>', nullable: true }), // default names an alphabet member; off-alphabet fails here
    retries: Schema.optional(Schema.Number).pipe(Schema.withDecodingDefault(() => 0)),
})
const decodeRecord = Schema.decodeUnknownSync(Record_)
const resolved: typeof Record_.Type = decodeRecord({}) // { mode: '<a>', retries: 0 } — defaults total at the codec, both tokens flipped to required Type
```

[GET_OR_ELSE_WIDENS_OFF_THE_UNION]:
- The `as const` table resolves a foreign key at the read, and the lever is `Record.get(Table, key)` returning `Option<Row>` followed by `Option.getOrElse(() => Table[fallbackKey])`, never a bare bracket access that is `undefined` — the absence resolves to the fallback ROW from the same table, so the resolution reads the closed source on both arms.
- `Option.getOrElse<A, B>(self: Option<A>, onNone: LazyArg<B>): A | B` is the trap: the result is the UNION of the Some type and the fallback type, so a fallback whose type the checker infers wider than the key union silently widens the resolved key off the closed union and defeats every downstream exhaustiveness — the fallback thunk must be annotated to the key type, `(): Key => '<member>'`, so `A | B` collapses back to `Key`.
- Resolving a foreign KEY to a fallback KEY (not a row) keeps the resolution composable: `pipe(Record.get(Table, raw), Option.map((_) => raw as Key), Option.getOrElse((): Key => '<fallback>'))` yields a `Key` that then indexes the table, so the resolved key threads into the rank-order, the group-equivalence, and the function-column dispatch identically to an admitted key — resolve to the key, read the row once.
- `Record.has(Table, key)` gates the resolution decision when the fallback depends on membership versus a wider policy: present keys read their own row, absent keys resolve to the fallback row, and the boolean probe selects the arm — but the `Option`-returning `Record.get` plus `getOrElse` fuses probe and resolution into one expression where `has` plus a conditional splits it into two reads of the same key.

[MATCH_ORELSE_IS_THE_DISPATCH_FALLBACK]:
- The function-column dispatch resolves an uncovered key through the matcher terminal, not a table arm: `Match.orElse((k) => Table[fallbackKey].run(arg))` supplies the total fallback for a partial dispatch, so a key the handler rows do not cover resolves to the fallback arm and the matcher returns a value, never a runtime `undefined` from a bare partial index.
- `Match.orElse` and `Match.exhaustive` are the resolution-versus-totality choice on the same matcher: `orElse` resolves the residual `R` to a designated arm and returns a function over the full input, `exhaustive` requires `R` to be `never` and refuses to compile until every member is covered — a vocabulary that designates a catch-all member uses `orElse`, one that forbids unhandled members uses `exhaustive`.
- `Match.orElseAbsurd` is the resolution that asserts the residual is impossible: it returns the matched-arm union assuming `R` is unreachable, so a vocabulary whose alphabet the type system proves total but whose matcher the checker cannot narrow to `never` resolves through `orElseAbsurd` rather than a fabricated fallback member — the absurd terminal is the totality claim, not a default.
- A literal default partition splits the alphabet at the arm: `Match.when(Match.is('<a>', '<b>'), f)` handles the named sub-alphabet and a trailing `Match.orElse(g)` resolves the complement, so the explicit members and their fallback are two arms over the one literal column, the complement narrowed to `Exclude<Key, '<a>' | '<b>'>` inside `orElse`.

[OPTIONAL_TO_REQUIRED_COMPUTES_THE_FALLBACK]:
- `Schema.optionalToRequired(From, To, { decode, encode })` is the deepest resolution: `decode` receives `Option<FromType>` where `Option.none` IS the missing key, so the fallback member is computed from absence as a transform — not a constant default but a function of presence, returning the resolved `To` member, and `encode` maps the resolved member back to `Option` to drop it on the wire when it equals the implied default.
- The decode signature `(o: Option<FA>) => TI` makes the resolution a fold over the absence: `Option.match({ onNone: () => '<fallback>', onSome: (v) => v })` is the body, so a vocabulary whose fallback depends on OTHER decoded fields or a policy resolves through the transform where `optionalWith({ default })` carries only a thunk of no arguments — the transform is the resolution that reads context, the thunk the resolution that does not.
- The source and target schemas may be DIFFERENT vocabularies, so `optionalToRequired` resolves a missing narrow-alphabet wire key to a wide-alphabet domain member: the `From` codec admits the wire sub-vocabulary, `decode` maps `none` to a domain-only fallback member outside the wire alphabet, and the round-trip stays invertible because `encode` returns `none` for that fallback — the seam's missing-key default is a domain member the wire never carries.
- The token result is fixed `PropertySignature<":", TA, never, "?:", FI, false, FR | TR>`: required Type, optional Encoded, requirement unions both schemas' `R` — so a resolution transform whose fallback computation needs a service surfaces that requirement in the field's `R`, the same `R`-accumulation `extend` performs, where a constant `default` thunk carries no requirement.

[TEMPLATE_LITERAL_VOCAB]:
- `Schema.TemplateLiteral(...params)` is a string-shaped vocabulary synthesized by recursing `GetTemplateLiteralType` over the param list: each param contributes either its raw `LiteralValue` (a `string | number | boolean | null | bigint` passed bare, a fixed segment) or, for a `Schema` param, its `Type` interpolated through `` `${Template}${A}` ``, so `Schema.TemplateLiteral('v', Schema.Number)` types and validates `` `v${number}` `` — the pattern is the codec and the type at once, no parallel regex, the alphabet computed from the params and never enumerated by hand.
- A param typed as a `Schema.Literal` UNION distributes the interpolation across every member: `Schema.TemplateLiteral(Scope, '.', Kind)` with `Scope` a two-member literal and `Kind` a two-member literal yields the four-element cartesian product `'app.warn' | 'app.fail' | 'db.warn' | 'db.fail'` — the product of the param alphabets is the synthesized vocabulary, generated, not written.
- The combinatorial growth is multiplicative in the params: adding one member to any param vocabulary multiplies the synthesized alphabet by that param's new cardinality, and the new product members appear with zero edits to the template owner — the alphabet's growth axis is the constituent vocabularies, each of which is itself a single source.
- The synthesized alphabet stays finite and closed exactly when every schema param is a finite literal union; one primitive-schema param (`Schema.Number`, `Schema.String`) opens that segment to `${number}`/`${string}` and the synthesized type is a template-string pattern rather than a closed key union — finiteness of the product requires finiteness of every factor, a structural-vs-open distinction recovered from whether the param is a finite literal union or a primitive schema.

[PRODUCT_SEEDS_THE_BEHAVIOR_TABLE]:
- The synthesized product alphabet is a closed key union usable as the `keyof` contract of an `as const satisfies` table: `as const satisfies Record<typeof Code.Type, Row>` requires one row per product member, so the behavior table over a composite key is closed against the generated cartesian product, not a hand-listed key set drifting from its factors.
- A missing product member is a declaration-site error, not a downstream `undefined`: omitting one row from the cartesian set fails the `satisfies` check naming the absent composite key, so the table's totality proof is the product of the param vocabularies and the checker enforces every combination is covered.
- The composite key indexes the row directly — `POLICY[code].halts` — total over the synthesized union by construction, so a dispatch keyed on a structured composite (scope-dot-kind, version-slash-channel) reads as one indexed access over the generated key, never a split into two nested indexed reads on the decomposed segments.
- The table and its key vocabulary stay synchronized through the constituent literals: widening `Kind` with a member both widens `typeof Code.Type` and breaks the `satisfies` table until the new product rows are supplied — the factor vocabulary is the single source, the product alphabet and the behavior table both derive from it.

```typescript
import { Array, Schema, pipe, Record } from 'effect'

const Scope = Schema.Literal('app', 'db') // a factor vocabulary; adding 'cache' here multiplies the product and breaks the table below until its rows land
const Kind = Schema.Literal('warn', 'fail')
const Code = Schema.TemplateLiteral(Scope, '.', Kind) // typeof Code.Type = 'app.warn' | 'app.fail' | 'db.warn' | 'db.fail', the cartesian product of the factors

const POLICY = {
    'app.warn': { halts: false, rank: 0 },
    'app.fail': { halts: true, rank: 2 },
    'db.warn': { halts: false, rank: 1 },
    'db.fail': { halts: true, rank: 3 }, // omitting one product member fails the satisfies naming the absent composite key
} as const satisfies Record<typeof Code.Type, { readonly halts: boolean; readonly rank: number }>

const blocking: ReadonlyArray<typeof Code.Type> = pipe(Record.keys(POLICY), Array.filter((c) => POLICY[c].halts)) // keys preserve the product union; POLICY[c].halts read inline, total by construction
const ranks: Record.ReadonlyRecord<typeof Code.Type, number> = pipe(POLICY, Record.map((row) => row.rank))
```

[PARSER_IS_A_STRUCTURED_TUPLE_VOCABULARY]:
- `Schema.TemplateLiteralParser(...params)` upgrades the synthesized string alphabet from a single union type to a `readonly` TUPLE aligned positionally to the params: `GetTemplateLiteralParserType` maps each param to its decoded `A` for a `Schema` param or keeps the bare `LiteralValue` for a literal param, so the decoded value is the structured decomposition of the matched string segment by segment, encoding back to the joined string, and `.params` exposes the original param list.
- A literal-value param survives in the decoded tuple as its literal: `Schema.TemplateLiteralParser('id', Schema.NumberFromString, ':', Schema.Literal('x', 'y'))` decodes to `readonly ['id', number, ':', 'x' | 'y']` — the fixed `'id'` and `':'` separators are tuple slots typed as their unit literals, so the tuple carries both the variable segments and the structural punctuation as discriminable elements.
- The `Encoded` side is the joined string built by `GetTemplateLiteralParserEncoded` from each param's ENCODED `I`: a `Schema.NumberFromString` param contributes `${string}` to the encoded template (its `Encoded` is `string`) while contributing `number` to the decoded tuple, so the parser's two sides are the wire string and the structured tuple, and encode rejoins the tuple into the matching string.
- A literal-UNION param inside the parser produces a tuple slot typed as that union, not a widened string: the `Schema.Literal('x', 'y')` param above makes the fourth tuple element `'x' | 'y'`, a closed sub-alphabet inside the structured value, so a downstream read of that slot dispatches over the slot's own vocabulary without re-decoding.
- `.params` is the parser's enumerable runtime surface, the structured analogue of a `Literal`'s `.literals`: the synthesized string type, the decoded tuple type, the encoding, and the param enumeration all fuse in one declaration, no parallel split-and-coerce at the seam.

[SEGMENT_DISPATCH_OFF_THE_TUPLE]:
- The decoded tuple destructures into typed segments each carrying its own narrow type, so dispatch over a parsed composite reads the discriminating slot directly: a `Match.value(scope).pipe(Match.when('app', ...), Match.when('db', ...), Match.exhaustive)` over the literal-typed first slot is total because the slot's type is the closed param vocabulary — the parse and the dispatch share the same alphabet, no re-validation between them.
- The structural separator slots (`':'`, `'.'`) are positionally fixed unit literals the destructure discards by binding, so the meaningful segments are the variable slots and the parse already proved the punctuation matched — a downstream consumer never re-checks the shape of the string, the tuple IS the proof the format held.
- A parsed segment that is itself a vocabulary key indexes a behavior table from inside the tuple: `POLICY[`${scope}.${kind}`]` reconstructs the composite key, OR the table is keyed on the tuple's discriminating slot directly — the parser decomposes the wire string into vocabulary keys and the keys index the rows, the decode and the projection orthogonal over one structured value.
- The tuple's element types are recovered by indexed tuple access, never restated: a consumer typed by the third element reads `(typeof Parser.Type)[2]`, so a segment-specific signature derives from the parser owner rather than a parallel `type Segment = ...` mirroring what the parser already computes.

```typescript
import { Schema, Match } from 'effect'

const Addr = Schema.TemplateLiteralParser(Schema.Literal('app', 'db'), ':', Schema.NumberFromString) // Type readonly ['app' | 'db', ':', number]; the slot literal IS the dispatch alphabet below

const route = Schema.decodeUnknownSync(Addr)
const back = Schema.encodeSync(Addr) // (a: typeof Addr.Type) => `${'app' | 'db'}:${string}`

const port = (raw: string): number => {
    const [scope, _sep, base] = route(raw)
    return Match.value(scope).pipe(
        Match.when('app', () => base + 1000),
        Match.when('db', () => base + 2000),
        Match.exhaustive, // total because scope's type is the closed param vocabulary; adding a Scope member breaks this loudly
    )
}
```

[VALIDATOR_VS_PARSER_CONTEXT_BOUNDARY]:
- The two constructors diverge on the requirement channel by their param constraint: `TemplateLiteral` admits `Schema.AnyNoContext | AST.LiteralValue` and returns a `TemplateLiteral<A>` carrying no `R`, so a validate-only string vocabulary is always context-free; `TemplateLiteralParser` admits `Schema.Any | AST.LiteralValue` and propagates `Schema.Context<Params[number]>` into the schema's `R`, so a parser whose param schema needs a service surfaces that requirement in the parser's own `R`.
- The boundary is decode capability: `TemplateLiteral` only validates the synthesized string matches the pattern — its result `Type` is the string union and there is nothing to decode INTO, so no param transform runs and no context is consumable; `TemplateLiteralParser` runs each param's decode to build the tuple, so a context-carrying param transform is admissible only in the parser form where its decode actually executes.
- A vocabulary that must decode wire segments into typed parts (a `NumberFromString` port, a `transformLiterals` channel remap inside a segment) requires `TemplateLiteralParser`; the same params handed to `TemplateLiteral` would type-check only when context-free and would still discard the decoded parts, returning the matched string rather than the structured tuple — the constructor selects validate-only versus decode-into-parts, never a flag.
- `TemplateLiteral` exposes no runtime accessor beyond `SchemaClass<A>` (no `.params`, no `.literals`); the synthesized alphabet is reachable only as `typeof Schema.Type`, while `TemplateLiteralParser` adds `.params` — so a string vocabulary that must enumerate its constituent params at runtime is the parser form, and a pure pattern-membership gate is the lighter validator.

[COMPOSING_GENERATED_ALPHABETS]:
- A template-literal vocabulary is a `Schema` member of `Schema.Union`, so disjoint structured patterns flatten into one alphabet: `Schema.Union(Schema.TemplateLiteral('a', Schema.Number), Schema.TemplateLiteral('b', Schema.Number))` admits `` `a${number}` | `b${number}` `` as one codec, the union of two synthesized patterns rather than a nested wrapper — the closed-combine lifted to generated alphabets, the disjunction of formats the product within one format cannot express.
- A subset of the product alphabet a boundary admits is NOT a `pickLiteral` over the template (the synthesized owner is a `TemplateLiteral<A>`, not a `Literal` carrying `.literals`, so the subset selector has nothing to read); the seam narrows by re-synthesizing from a narrowed FACTOR — `Schema.TemplateLiteral(pipe(Scope, Schema.pickLiteral('app')), '.', Kind)` yields the `'app.*'` sub-product, so the seam restriction is a smaller factor and the product recomputes from it, never a hand-listed allowlist of composite strings.
- `.annotations` `examples` and `default` are checked against the synthesized product union, not a free-form string: `ExamplesAnnotation<A>` types each example as a product member, so an example off the cartesian set fails at the annotation site — the annotation payload proves itself a member of the generated alphabet the same way the `satisfies` table proves its rows.
- The synthesized pattern carries no enumerable runtime surface (no `.literals`, no `.params`), so an annotation that must list the alphabet spells the product members as the `examples` array while the membership check stays compile-time — the generated owner annotates its decode `message` and `identifier` like any codec, but its alphabet is enumerated only by re-listing, the price of synthesis over a written literal tuple.
