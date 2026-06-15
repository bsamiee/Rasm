# Per-Variant Context Union Fold: One keyof-Indexed Union Makes Each Variant's R A Disjoint-Slot Family

[CONTEXT_IS_A_UNION_FOLD_WHILE_TYPE_IS_AN_INTERSECTION_FOLD]:
- `Struct.Context<F>` is `Schema.Schema.Context<F[keyof F]>` — a SINGLE indexed access feeding ONE conditional, structurally unlike the three sibling folds that walk `[K in keyof F]`. `Struct.Type` is `UnionToIntersection<{ [K in keyof F]: { readonly [H in K]: Type<F[H]> } }[keyof F]>`, `Struct.Encoded` a per-key mapped record, `Struct.Constructor` a `UnionToIntersection` over the elision predicate — each rebuilds a RECORD whose keys are the columns. `Context` builds NO record: `F[keyof F]` collapses the field map to the union of its VALUES, and `Schema.Context<S>` (`S extends Variance<_, _, R> ? R : never`) distributes across that union, summing every field's `R` into one requirement. The decoded type and the wire type are intersections of per-column shapes; the requirement is a union of per-column services — opposite lattice directions off the same field map.
- The distribution is automatic because `Schema.Context<S>` is a distributive conditional over a naked type parameter: `F[keyof F]` is `LeafA | LeafB | OverrideableC`, and `Context` evaluates `Context<LeafA> | Context<LeafB> | Context<OverrideableC>` = `never | never | ServiceC` = `ServiceC`. No `UnionToIntersection`, no key enumeration, no per-field term — the union is the indexed access and the fold is the conditional's own distribution, so an `R` cannot be lost to an intersection collapse the way an optional key can vanish from `Type`.
- `Schema.Context<S>` reads the SAME `R` slot whether `S` is a `Schema` or a `PropertySignature`, because both extend `Schema.Variance<A, I, R>` and `PropertySignature<Tok, Type, Key, ETok, Enc, HasDefault, R>` carries `R` as its terminal parameter. So a write slot holding an `Overrideable` (a `PropertySignature` with `R` in position seven) contributes to the union by the identical mechanism a plain leaf does — the union fold is signature-kind-blind, reading the variance `R` off every slot regardless of whether it is a leaf, a tag, or a generative property-signature.

```typescript
import { Context, Effect, Schema } from 'effect';

class Geo extends Context.Tag('Geo')<Geo, { readonly resolve: Effect.Effect<string> }>() {}

const Located = Schema.transformOrFail(Schema.String, Schema.String, {
    strict: true,
    decode: (raw) => Effect.flatMap(Geo, (g) => Effect.map(g.resolve, (region) => `${region}:${raw}`)),
    encode: (value) => Effect.succeed(value),
});

const Owner = Schema.Struct({
    plain: Schema.NonEmptyTrimmedString,
    tagged: Schema.tag('Owner'),
    located: Located,
});

type StructR = Schema.Schema.Context<typeof Owner>;
type LeafUnion = (typeof Owner)['fields'][keyof (typeof Owner)['fields']];
```

- `StructR` is `Geo` and `LeafUnion` is the raw value union `NonEmptyTrimmedString | tag<"Owner"> | typeof Located` — `Schema.Context` distributes `never | never | Geo` over it, so the whole struct's requirement is the single contaminating leaf's, contributed with zero per-field enumeration. Reject `type StructR = Geo` written by hand or a `Context.Tag` re-listed beside the struct: the union fold already names the requirement off the field map, and restating it forks the source the next contaminating column would silently desync.

[THE_OWNER_R_IS_THE_SELECT_UNION_ALONE_NOT_THE_FIELD_MAP_UNION]:
- The class owner's own `R` is `Schema.Schema.Context<ExtractFields<"select", Fields, true>[keyof ExtractFields<"select", Fields, true>]>` — the union fold runs over the SELECT PROJECTION's slots, not the raw field map. A field whose `select` slot is context-free but whose `insert` slot carries a service contributes `never` to `Self["Context"]`: the projection installs the insert-slot schema only in `Self.insert`, so the owner-level requirement union never sees it. `Self["Context"]`, `Schema.decodeUnknown(Self)`, and the read-shape constructor all carry exactly the select union, the write-slot services structurally absent from the read decode.
- Each variant property is a `Schema.Struct<ExtractFields<V, Fields, false>>`, so `Schema.Schema.Context<typeof Self.insert>` re-runs the union fold over the INSERT projection's slot set — a DIFFERENT `F[keyof F]` union than select, because the membership gate and the per-slot schema both differ per variant. The six variant codecs each carry their own requirement union computed by the identical `Struct.Context` law over six disjoint slot families: `Self["insert"]["Context"]`, `Self["update"]["Context"]`, `Self["json"]["Context"]` are independent unions, never restrictions of one owner-level `R`.
- The same column contributes a DIFFERENT term to each variant's union when its slot schema changes representation: a marker whose `select` slot is a bare leaf (`R = never`) and whose `insert` slot is an `Overrideable<To, From, Service>` adds `never` to the select union and `Service` to the insert union — one column, two requirement contributions, the variant slot deciding which union inherits the service. The disjoint-slot-family structure is the entire reason select-R, insert-R, and update-R diverge rather than coincide.

[A_SINGLE_LEAF_CONTAMINATES_EXACTLY_THE_VARIANTS_THAT_RETAIN_IT]:
- A field's contaminating `R` propagates to a variant's union if and only if that field's slot survives the variant's membership gate AND the surviving slot's schema carries the `R`. The contamination footprint is therefore the INTERSECTION of two facts already owned: which variants the marker's `schemas` config includes (membership), and which of those slots hold the service-carrying schema versus a context-free substitute. A `Model.Sensitive` leaf requiring a service contaminates `select`/`insert`/`update` and is structurally absent from every json union; an `Overrideable` slot requiring a service contaminates only the write variants holding it.
- `Model.JsonFromString(inner)` threads `Schema.Schema.Context<inner>` into ALL SIX slots — the db slots wrap `inner` in `Schema<Type<inner>, string, Context<inner>>` (the text transform preserving the inner R) and the json slots are `inner` itself — so a `JsonFromString` whose inner schema requires a service contaminates EVERY variant's union, the widest possible footprint off one column. This is the structural opposite of an `Overrideable` stamp: the text-codec leaf cannot drop its inner requirement on any variant, while the generative slot confines its mint's requirement to the write variants alone. The footprint is a property of WHERE the marker places the contaminating schema, not of the service.
- A `Model.FieldOption(inner)` carries `Schema.Schema.Context<inner>` into all six slots through whichever option-wrapper the variant family selects (`OptionFromNullOr<inner>` on db, `optionalWith<inner, …>` on json) — both preserve the inner R — so an optional column requiring a service contaminates every union exactly as a `JsonFromString` does. The wrapper changes the encoded leaf, never the requirement: `Schema.Context<OptionFromNullOr<inner>>` is `Schema.Context<inner>`, the option transform requirement-transparent.

```typescript
import { Model } from '@effect/sql';
import { Context, Effect, Schema } from 'effect';

class Vault extends Context.Tag('Vault')<Vault, { readonly open: (raw: string) => Effect.Effect<string> }>() {}

const Unsealed = Schema.transformOrFail(Schema.String, Schema.String, {
    strict: true,
    decode: (raw) => Effect.flatMap(Vault, (v) => v.open(raw)),
    encode: (value) => Effect.succeed(value),
});

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    everywhere: Model.JsonFromString(Schema.Struct({ token: Unsealed })),
    writeOnly: Model.Sensitive(Unsealed),
}) {}

type SelectR = Schema.Schema.Context<typeof Owner>;
type JsonR = Schema.Schema.Context<typeof Owner.json>;
type CreateR = Schema.Schema.Context<typeof Owner.jsonCreate>;
```

- `SelectR` and `JsonR` and `CreateR` are all `Vault`: `everywhere` (a `JsonFromString` over a `Vault`-carrying inner) contaminates every variant, so no projection escapes the requirement — but `writeOnly` (a `Sensitive` marker) contributes `Vault` to `SelectR` while contributing NOTHING to `JsonR`/`CreateR`, because `Sensitive` omits the json family entirely and the absent slot adds no term to those unions. Two leaves, two contamination footprints, each the membership of its marker intersected with the slot that carries the service.
- The diff of removing `everywhere`'s `Vault` requirement is the proof: replacing `Unsealed` with `Schema.String` at that one leaf drops `Vault` from all six unions at once, and the only union left carrying `Vault` is `SelectR`/insert-R/update-R via `writeOnly` — the requirement diff of a leaf edit is the exact set of variant unions whose `F[keyof F]` retained the contaminating slot, recomputed at compile time with no union respelled.

[ANYNOCONTEXT_IS_A_TYPE_LEVEL_BAN_NOT_A_PROVISION_CHECK]:
- `Model.AnyNoContext` constrains EVERY variant property to `Schema.Schema.AnyNoContext` = `Schema<any, any, never>`, while `Model.Any` constrains them to `Schema.Schema.Any` = `Schema<any, any, unknown>`. The ban is a structural subtype check: a variant typed `Schema<any, any, Vault>` is assignable to `Schema<any, any, unknown>` (the `R = unknown` top admits any requirement) but NOT to `Schema<any, any, never>` (the `R = never` bottom admits only `never`). An owner with one service-carrying codec fails the `AnyNoContext` bound at the variable position — the constraint is checked at the call to `Model.makeDataLoaders(Self, …)`, never at provision time, so the disqualification is a compile error long before any layer is wired.
- The two operation surfaces partition exactly by this bound: `makeRepository<S extends Model.Any>` admits a service-carrying owner and surfaces the requirement per operation, while `makeDataLoaders<S extends Model.AnyNoContext>` rejects it. The SAME owner drives both only when every variant's `F[keyof F]` union resolves `never`; a generative slot whose mint reads a service, or any leaf requiring a service in any variant, silently bars the batched path because that variant property is no longer a `Schema<any, any, never>`.
- Because `AnyNoContext` reads the variant properties' `R` slots — themselves each a `Struct.Context` union over a disjoint slot family — context closure is a property of the FIELD DECLARATIONS, achievable ONLY by collapsing the requirement to `never` at the leaf. The fix is a context-free field: a leaf codec built over a `Layer`-provided dependency stays `Schema<A, I, Service>` and propagates through the union fold into every variant retaining it, so providing the service must happen BEFORE the field is declared — `Effect.provide` at the leaf's construction, never at the operation. The gate reads the field map; an `R` cannot be erased at the operation because the operation never re-folds the field union, it consumes the variant `Context` the fold already produced.

[THE_VARIANT_UNION_IS_IRREDUCIBLE_AT_EVERY_DOWNSTREAM_LIFTER]:
- A variant's `Struct.Context` union is the requirement EVERY derivation off that variant inherits, and no lifter re-folds the field map to shrink it — the union is computed once at projection and consumed as the variant's `R`. `Schema.standardSchemaV1(Self.json)` typed `<A, I>(schema: Schema<A, I, never>) => StandardSchemaV1<I, A>` is the sharpest gate: its `R = never` parameter rejects a variant whose union resolves non-`never` at the variable position, so a json variant carrying one service-requiring leaf CANNOT be lifted to a Standard Schema until that leaf's `R` is `never`. The cross-tool wire contract is admissible only over a closed union, the `never` constraint the type-level proof the variant union is irreducible downstream.
- The derivations that operate on `A`/`I` regardless of `R` — `Schema.equivalence`, `Arbitrary.make`, `Pretty.make`, `Schema.is`, `Schema.asserts`, `JSONSchema.make` — all type the variant as `Schema<A, I, R>` and never run the decode effect, so they yield off a variant whose union is non-`never` while the effect-running lifters (`standardSchemaV1`, any decode) are gated by it. The variant `Context` union partitions the lifters into two classes: the structure-only ones blind to `R`, and the effect-running ones whose signatures demand `R = never`. A variant carrying a pending service still yields its comparator and generator, the union surfacing only where an effect must actually run.
- A consumer cannot discharge a variant's union by providing the service at the lifter: the union is a property of the variant schema's `R` slot, fixed at projection, so `standardSchemaV1` over a service-carrying json variant is a compile error no provision at the call repairs. Closure routes back to the leaf — a field codec folded to `Schema<A, I, never>` before declaration — because the projection re-reads the leaf's `R` into the variant union, and the only edit that shrinks the union is one that shrinks a slot's requirement at its source.

```typescript
import { Model } from '@effect/sql';
import { Arbitrary, JSONSchema, Schema } from 'effect';

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    region: Model.JsonFromString(Schema.Struct({ city: Schema.NonEmptyTrimmedString })),
    secret: Model.Sensitive(Schema.Redacted(Schema.String)),
}) {}

const wireGate = Schema.standardSchemaV1(Owner.json);
const wireEq = Schema.equivalence(Owner.json);
const wireGen = Arbitrary.make(Owner.json);
const wireContract = JSONSchema.make(Owner.json, { target: 'openApi3.1' });
```

- `wireGate` compiles because `Owner.json`'s union is `never` — every json-variant leaf (`NonEmptyTrimmedString`, the `JsonFromString` structured inner, `secret` dropped by `Sensitive`) folds `never` into the json union, so `standardSchemaV1`'s `R = never` parameter accepts it. A single json-variant leaf built over a service-requiring codec would fold that service into the union and reject the lift at the variable position — the gate is the type-level proof the variant union must be closed before crossing to a foreign validator, no provision at the call discharging it.
- `wireEq`, `wireGen`, and `wireContract` would yield even were the union non-`never` — `equivalence`, `Arbitrary.make`, and `JSONSchema.make` type the variant as `Schema<A, I, R>` and never run the decode effect, blind to `R` — so the variant `Context` union partitions the lifters: the structure-only family consumes any variant unconditionally, the effect-running `standardSchemaV1` only a closed one. The union surfaces exactly where an effect must run, the irreducible boundary read off the variant's `R` slot once at projection and consumed downstream with no path to shrink it but a context-free leaf.

[THE_UNION_ALGEBRA_TRANSCENDS_THE_OWNER_INTO_THE_GROUP]:
- `Model.Union(A, B)`'s variant properties are each `Schema.Union<{ [K]: Extract<Variant, Members[K]> }>`, so a group variant's requirement is `Schema.Schema.Context<Members[number]>` distributed over the per-member extract — a UNION OF UNIONS. `Node.insert`'s `R` is `A.insert["Context"] | B.insert["Context"]`, each member's insert union itself a `Struct.Context` fold over that member's insert slots, so the group's per-variant requirement is the sum of the same disjoint-slot-family folds across every member. The union fold is associative across the member axis: adding a member adds its variant unions as new terms, every group operation widening its requirement to the new sum with no per-pair restatement.
- A contaminating leaf in ONE member contaminates the group variant union that member's slot survives into, and no other: a `Mint`-requiring insert slot in member `A` puts `Mint` in `Node.insert["Context"]` while member `B`'s context-free insert slot adds `never` — the group inherits the disjoint footprint of each member's columns, the requirement the union of every member's per-variant fold. The `AnyNoContext` bound over the group rejects it if ANY member's ANY variant carries a service, so a batched group surface demands every column of every member fold to `never`, the closure a property of the whole member set's field declarations.

```typescript
import { Model } from '@effect/sql';
import { Context, Effect, Schema } from 'effect';

class Seq extends Context.Tag('Seq')<Seq, { readonly tick: Effect.Effect<number> }>() {}

const Counted = Schema.transformOrFail(Schema.Number, Schema.Number, {
    strict: true,
    decode: (n) => Effect.succeed(n),
    encode: () => Effect.flatMap(Seq, (s) => s.tick),
});

class Leaf extends Model.Class<Leaf>('Leaf')({
    kind: Schema.tag('Leaf'),
    order: Model.Field({ select: Schema.Number, insert: Counted, update: Counted, json: Schema.Number }),
}) {}

class Branch extends Model.Class<Branch>('Branch')({
    kind: Schema.tag('Branch'),
    width: Schema.Number,
}) {}

const Node = Model.Union(Leaf, Branch);

type GroupInsertR = Schema.Schema.Context<typeof Node.insert>;
type GroupSelectR = Schema.Schema.Context<typeof Node>;
```

- `GroupInsertR` is `Seq` — `Node.insert` is `Schema.Union<[Leaf.insert, Branch.insert]>`, so its `Context` distributes to `Leaf.insert["Context"] | Branch.insert["Context"]` = `Seq | never` = `Seq`, the single contaminating member's insert slot summed across the member axis. `GroupSelectR` is `never`: both members' select slots are context-free, so the group read decode is requirement-free while the group write encode carries `Seq` — the disjoint-slot-family divergence lifts from one owner to the discriminated group with no new machinery.
- Adding a third member contaminated in its update slot adds exactly `MemberC.update["Context"]` to `Node.update["Context"]` and nothing to `Node.insert`/`Node.json` — the next member's requirement diff is the union of its per-variant folds joined to the group's existing sum, the `kind` discriminant closing each variant union's decode while the `Context` union fold sums the requirement orthogonally. The group requirement is the member-axis union of the slot-family unions, both folds running off one `Model.Union` call.
