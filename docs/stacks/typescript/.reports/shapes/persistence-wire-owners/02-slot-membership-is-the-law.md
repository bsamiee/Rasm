# Slot Membership Is The Law: One Presence Test Erases A Column Across Every Projection

[THE_GATE_LIVES_IN_THE_KEY_POSITION_NOT_THE_VALUE_POSITION]:
- The projection law is a mapped type whose membership decision rides the `as` key-remap clause, structurally distinct from the value clause that decides representation: `{ readonly [K in keyof Fields as [Fields[K]] extends [Field<infer Config>] ? V extends keyof Config ? K : never : K]: ... }` — a `Field` whose `Config` lacks `V` remaps its key to `never`, and a `never` key is ABSENT from the resulting object type, not present-and-nullable. The value clause `[Config[V]] extends [Schema.Schema.All | Schema.PropertySignature.All] ? Config[V] : never` then chooses WHICH schema a surviving key carries — two clauses, two axes, one mapped type: membership in the key position, representation in the value position.
- A bare `Schema.X` and a nested `Struct` both pass the key clause unconditionally (`: K`), so only a `Field` marker can ever delete a key — membership-gating is a property exclusively of the config-row markers, and a column built from a plain schema is structurally non-droppable across all six variants. The `as`-clause ternary is the entire mechanism by which `insert` lacks `id` while `select` carries it: not a filter applied afterward, but the key never being emitted.
- The dropped column is not `K?: undefined` and not `K: never` as a VALUE — it is the literal absence of `K` from `keyof` the projected struct: `"id" extends keyof Schema.Schema.Type<typeof Self.insert>` resolves `false`, so an attempt to read or write the dropped key is a property-does-not-exist error, not a type mismatch on a present-but-narrowed slot. Membership erasure and representation narrowing produce categorically different failure surfaces at the consumer.

[ONE_RUNTIME_PRESENCE_TEST_MIRRORS_THE_TYPE_GATE]:
- The value-level twin of the `as never` key-drop is one guard inside the single projection loop: iterating `Object.keys(self[TypeId])`, a `Field` value's column is written `fields[key] = value.schemas[variant]` ONLY under `if (variant in value.schemas)` — when the variant is absent from the config the assignment never runs, so `key` is never a property of the `fields` record handed to `Schema.Struct(fields)`. The type's `never` key and the runtime's never-written key are the SAME erasure expressed in two languages, and their agreement is what makes the projected struct's static shape a faithful description of its runtime AST.
- The presence test is `variant in value.schemas`, a plain key-existence check on the config object, NOT a sentinel comparison, a `null` slot, or a flag read — so a marker carries membership purely by which keys its `schemas` object owns, and a slot it omits is omitted from the JavaScript object, never present-and-falsy. There is no third state between member and non-member; a config row either has the key (project the schema there) or lacks it (the column does not exist in that variant), the binary the whole derivation rests on.

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    name: Schema.NonEmptyTrimmedString,
}) {}

type SelectKeys = keyof Schema.Schema.Type<typeof Owner>;
type InsertKeys = keyof Schema.Schema.Type<typeof Owner.insert>;
type IdInInsert = 'id' extends InsertKeys ? true : false;

const idAtInsert = 'insert' in Model.Generated(Schema.Number).schemas;
const idAtSelect = 'select' in Model.Generated(Schema.Number).schemas;
```

- `SelectKeys` is `"id" | "name"` and `InsertKeys` is `"name"` alone — `IdInInsert` resolves `false` because `Model.Generated`'s `schemas` object owns `select`/`update`/`json` and omits `insert`, so the `as`-clause remaps `id`'s key to `never` in the insert projection. The owner derives both keysets from one declaration; no parallel insert interface restates the read shape minus a column.
- `idAtInsert` is `false` and `idAtSelect` is `true` against the raw config — `'variant' in marker.schemas` is the runtime witness of the same predicate the type gate evaluates as `V extends keyof Config`, so the projection's drop of `id` from the insert variant is one application of this one test, not a special case coded per marker; a config whose keys differ shifts membership with zero change to the projecting loop.

[INSERT_DROPS_JSON_DROPS_UPDATE_NAMES_ARE_ONE_THEOREM]:
- The bedrock's separate behavioral claims are corollaries of a single membership statement, not independent rules: `Model.Generated`'s config is `{ select, update, json }`, `Model.Sensitive`'s is `{ select, insert, update }`, `Model.GeneratedByApp`'s is `{ select, insert, update, json }` — each marker differs from the others ONLY in which keys its `schemas` object owns, the per-slot schema being the same `S` throughout. "Insert drops Generated" is `"insert" extends keyof { select; update; json }` resolving `false`; "json drops Sensitive" is `"json" extends keyof { select; insert; update }` resolving `false`; "update names the id" is `"update" extends keyof { select; update; json }` resolving `true` — three instances of one predicate over three key sets.
- The behavioral vocabulary is therefore non-fundamental: a marker has no behavior, no override, no method — it is a key set, and every sentence about what a variant "does to" a column is a restatement of set membership over that key set. This is why a custom marker reproduces a named marker's behavior with byte-identical membership: `Model.FieldOnly("select", "insert", "update")(s)` IS `Model.Sensitive(s)` because both author `{ select, insert, update }`, the json-erasure a fact of the absent keys rather than a redaction step either one runs.
- Because membership is the only axis, the partition `VariantsDatabase = "select" | "insert" | "update"` and `VariantsJson = "json" | "jsonCreate" | "jsonUpdate"` are the natural grouping of the six keys, and a marker's behavior is fully specified by stating its config as a subset of these six — "store round-trip, wire-erased" is `VariantsDatabase` exactly, "database-supplied, never created" is the six minus `insert`/`jsonCreate`/`jsonUpdate`. The behavioral name is shorthand for a subset; the subset is the law.

[ONE_ABSENCE_ERASES_THE_COLUMN_FROM_EVERY_PROJECTION_AT_ONCE]:
- The projected variant is one `Schema.Struct(fields)`, hence one `AST.TypeLiteral` whose `propertySignatures: ReadonlyArray<PropertySignature>` is the column manifest — a membership-dropped column is a `PropertySignature` that is NEVER added to that array, so it has no node for any AST-walking surface to encounter. `Schema.Schema.Type`, `Schema.Schema.Encoded`, `Schema.Struct.Constructor`, and `Schema.Struct.Context` are each `[K in keyof F]` mapped over the same field record, so the single dropped key vanishes from the decoded type, the wire type, the constructor payload, AND the requirement union in one stroke — one absence, four type-level erasures, with no per-projection enumeration.
- The same single absence propagates identically into every runtime derivation that walks the struct AST: `Arbitrary.make`, `Pretty.make`, `Schema.equivalence`, `JSONSchema.make`, `Schema.is`, and `Schema.asserts` all receive `Schema<A, I, R>` and iterate `ast.propertySignatures`, so a column absent from a variant generates no arbitrary clause, no pretty clause, no equivalence comparison, no JSON Schema property, and no membership check — `Arbitrary.make(Self.insert)` never mints the `Generated` id, `Schema.equivalence(Self.json)` never compares the `Sensitive` column, `JSONSchema.make(Self.jsonCreate)` never emits the create-absent column, each because the property signature was never written. The "membership decides existence simultaneously across type, value, constructor, equivalence, arbitrary, and JSON Schema" boundary is the literal consequence of every surface sharing one `propertySignatures` array.
- The boundary is the array, not a re-run: there is no membership filter inside `Arbitrary.make` or `JSONSchema.make` that re-consults the marker — the marker was consulted once, at projection time, and what reaches the derivation is a struct whose column list already omits the dropped key. A consumer cannot recover the dropped column from the variant schema because the information is gone from the AST; the only path back to it is a different variant's projection off the owner, never a transform of this one.

[THE_DEFAULT_VARIANT_IS_NOT_AN_EXTRA_PROJECTION]:
- The `select` projection is memoized under the key `"__default"` while every non-default variant memoizes under its own name, and the default branch preserves a nested member as-is when it `Schema.isSchema` rather than re-wrapping it — so the read shape that feeds the class constructor and `Schema.decodeUnknown(Self)` is the membership projection of `select`, computed by the same loop with the `isDefault` flag, not a privileged hand-authored shape. The class instance's columns are exactly the keys whose config owns `select`, the default variant carrying no special membership rule beyond being the one the class body inherits.
- Repeated access to `Self.json` returns the identical cached `Schema.Struct` node, so the twelve type derivations off the six variants and every downstream surface share ONE struct instance per variant — the membership decision is made once per variant per owner and reused, never recomputed at each consumer, which is why `Schema.equivalence(Self.json)` and `JSONSchema.make(Self.json)` describe the byte-identical column set without coordination.

[MEMBERSHIP_RECURSES_EXACTLY_ONE_LEVEL_PER_CALL]:
- A nested `Model.Struct` field is dispatched by the projecting loop to `extract(value, variant)` — a fresh full pass over the fragment's own field map under the PARENT's active variant, so membership threads one level down per recursion: a `Model.Generated` column inside a nested fragment is dropped from the parent's `insert` sub-object by the same `variant in schemas` test applied to the fragment's config. The recursion is unbounded in depth but flat in rule — each level re-runs the identical loop, no level carrying a membership policy distinct from any other.
- Membership recursion is structural, but the optional-key elision that membership shares a mechanism with at the top level does NOT recurse: a top-level overrideable's constructor key is elided from the parent payload because `RequiredKeys` reads the parent's own keys, yet the SAME marker inside a nested struct surfaces as a required nested key under `exactOptionalPropertyTypes` — the membership drop threads down (the column is absent or present in the nested sub-object), but the default-elision is a parent-level `RequiredKeys` property, not a recursive one. Membership recurses; constructor-key elision does not, and conflating the two is the trap that fails a nested `{ audit: {} }` construct.
- A `Schema.suspend` field is admitted as a leaf by the projection's final arm, so a self-referential entity recurses INSIDE one variant's interior — the membership recursion stops at the leaf and the value recursion proceeds under the suspended schema, the two recursions orthogonal: membership threads down one fragment level per `extract` call, the suspended self-reference threads down the value tree at decode time, neither reaching across the variant axis.

[THE_VALIDATION_GATE_IS_THE_MEMBERSHIP_PREDICATE_RUN_AT_DECLARATION]:
- The field-map gate is the same membership predicate evaluated as a compile-time admission: `Struct.Validate<A, Variant>` resolves a `Field<Config>` position to `{}` when `[keyof Config] extends [Variant]` and to the literal `"field must have valid variants"` otherwise — it checks that a marker's declared key set is a SUBSET of the six known variants, the dual of the projection's per-variant `V extends keyof Config` membership test. A marker whose config owns an out-of-alphabet key is rejected at the field-map argument, because a key the projection could never match would silently produce a column reachable from no variant.
- `Validate` recurses the nested struct (`A[K] extends { readonly [TypeId]: infer _ } ? Validate<A[K], Variant>`), so the membership-validity law threads one level down at declaration exactly as the projection threads membership down at extraction — the two recursions are the same shape over the same fragment tree, one rejecting a malformed key set before any owner forms, the other reading a well-formed key set into a column manifest. The gate guarantees every config the projection will ever test is a subset of the alphabet, so the projection's membership test never encounters a key it cannot decide.

[A_COLUMNS_ABSENCE_IS_ITS_ABSENCE_FROM_EVERY_KEYOF_FOLD]:
- Every fold ranging over a variant's keys ranges over the PRUNED key set, so a membership-dropped column is invisible to each `keyof`-driven derivation by the same single absence: the column it does not appear under cannot be named at the constructor payload, cannot anchor a `keyof`-constrained position, and cannot contribute to any per-variant union folded across `Fields[keyof Fields]` — the absent key is absent from the domain of every map and fold the variant feeds, not present-and-skipped. A surface that consumes a variant's key set consumes a manifest the membership gate already shortened.
- The repository's id-eligibility constraint is membership stated as an intersection of key sets: `idColumn` is typed `keyof Self["Type"] & keyof Self["update"]["Type"] & keyof Self["fields"]`, so a column qualifies to anchor lookup only when it is a MEMBER of both the select projection and the update projection — a marker shaped to skip `update` is structurally barred from being an id, the disqualification a fact of its absent `update` key rather than a flag. `Model.Generated` (owns `select` and `update`) qualifies; a hypothetical insert-once column (no `update`) does not, the eligibility computed by intersecting the variants' surviving key sets, the membership predicate evaluated across two projections at once.

[MEMBERSHIP_THEOREM_REALIZED]:

```typescript
import { Model } from '@effect/sql';
import { Arbitrary, JSONSchema, Schema } from 'effect';

const Audit = Model.Struct({
    revision: Model.Generated(Schema.Number),
    note: Schema.String,
});

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    label: Schema.NonEmptyTrimmedString,
    secret: Model.Sensitive(Schema.NonEmptyTrimmedString),
    audit: Audit,
}) {}

type InsertKeys = keyof Schema.Schema.Type<typeof Owner.insert>;
type JsonKeys = keyof Schema.Schema.Type<typeof Owner.json>;
type NestedInsertKeys = keyof Schema.Schema.Type<typeof Owner.insert>['audit'];

const insertGen = Arbitrary.make(Owner.insert);
const createContract = JSONSchema.make(Owner.jsonCreate, { target: 'openApi3.1' });
const wireEq = Schema.equivalence(Owner.json);
```

- `InsertKeys` is `"label" | "secret" | "audit"` — `id` absent because `Model.Generated` omits `insert` from its config — while `JsonKeys` is `"id" | "label" | "audit"` with `secret` absent because `Model.Sensitive` omits the entire json family; one membership predicate over two key sets yields two different column manifests off one declaration, the bedrock's "insert drops Generated, json drops Sensitive" stated as two evaluations of `variant in schemas`.
- `NestedInsertKeys` is `"note"` alone — the nested `revision` is dropped from the parent's insert sub-object because the membership recursion re-ran the same `Model.Generated` config test one level down under the parent's `insert` variant, the drop threading into the fragment with no per-level rule.
- `insertGen`, `createContract`, and `wireEq` each consume the single absence without re-consulting any marker: `insertGen` mints no `id` (absent from the insert struct's `propertySignatures`), `createContract` publishes no `id` and no `secret` (absent from the jsonCreate manifest), `wireEq` compares no `secret` (absent from the json manifest) — three derivation surfaces erasing the same columns because each walks a struct AST whose property-signature array was already pruned at projection time, the simultaneity boundary realized as one absence consumed N ways.
