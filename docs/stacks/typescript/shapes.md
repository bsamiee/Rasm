# [SHAPES]

A concept takes one owner, and a five-axis signature is read before the first field is written: the identity regime (structural value, reference cell, or nominal primitive), the variant cardinality (one shape, N tagged, or N closed keys), the payload timing (fixed at definition or per-occurrence per channel), the equality demand (deep structural, reference, or none), and the persistence demand (codec plus per-channel projection or in-memory only); field count never enters the read. The owner the read elects collapses its own declaration chain — `object → type → const → typeof → instantiation` deletes because the owner's first appearance is its only appearance, and the levers `as const`, `satisfies`, the `const` type parameter, and the class-as-both-type-and-value stack only until their bounded coverage unions to the need. The discriminants the read fixes are three at once: where a change detonates, what equality means, and which capabilities derive — every misplaced shape traces to one mis-answered axis. The proof of a correct shape is the conceivable next-feature diff, which the owner's `Type`/`Encoded`/`Context` triple sorts three ways at declaration: a within-family re-route touching one declaration, a cross-family loud break failing every consumer at compile time, and a forbidden silent shift moving runtime semantics while the static type holds. An axis with no loud break is sized pessimistically; an axis carrying its own loud break is sized to the instance.

## [1]-[OWNER_CHOOSER]

[OWNER_ROUTING]:
- Use: when a concept matches several signatures, the candidate that dominates under the collapse comparator wins; the most specific row is the dominant candidate, never a textual precedence.
- Reject: a flat `{ identity; wire?; cardinality? }` record, because `$match` narrows each arm by `Extract<R, Record<D, P>>`, so a free literal-union field collapses every arm to `never`; a signature parameterized over its own element type (`Data.taggedEnum<WithGenerics<1>>`) defeats per-arm narrowing silently, an arm refining the tier from the element falling to the widest tier with every arm still exhaustive.
- Law: a `Schema.transform` forces a total `encode`, so a lossy `decode` either pushes its loss onto `To` as a refinement or stays a post-decode projection with no round-trip claim; `transformLiterals` is total-iso only while both projected literal columns are injective.
- Boundary: `as const` leaves the value structurally mutable, `satisfies` discharges coverage only against a closed contract key set never an open `Record<string, Row>`, and `NoInfer` on the shape-defining position inverts the source of truth; a part whose refinement consults a service is structurally rejected from a `Schema.TemplateLiteral`/`TemplateLiteralParser` slot.

| [INDEX] | [SIGNATURE]                                   | [OWNER]                                 | [REJECT_SPELLING]                               |
| :-----: | :-------------------------------------------- | :-------------------------------------- | :---------------------------------------------- |
|   [1]   | structural value, one shape, deep equal, wire | `Schema.Class<Self>(id)({...})`         | `interface` + `Schema.Struct` + `Equivalence`   |
|   [2]   | structural value, N tagged, wire              | `Schema.Union` of `Schema.TaggedClass`  | discriminated `interface` union + decode switch |
|   [3]   | structural value, N tagged, in-memory         | `Data.taggedEnum<T>()`                  | hand constructors + type-guard zoo              |
|   [4]   | structural value, N tagged, failure trait     | `Schema.TaggedError<Self>(id?)(tag, f)` | `TaggedClass` shape beside a separate error     |
|   [5]   | nominal primitive carrying a refinement       | `Schema.<base>.pipe(filter, brand)`     | `number & Brand` beside a runtime check         |
|   [6]   | closed keyset of behaviors, decode-free       | `as const` read by `keyof typeof`       | `enum` + lookup `Record` + mapping switch       |
|   [7]   | closed bare literals, decode (no behavior)    | `Schema.Literal(...values)`             | `as const` array + an `is`-guard                |
|   [8]   | per-occurrence payload across channels        | `Model.Class<Self>(id)({...})`          | five `pick`/`omit` sibling schemas              |

Precedence is fixed: facets choose the family, cardinality picks tagged-versus-single, channels picks variant-versus-flat (channels-first reaches `Model.Class` on a concept with no structural identity), parsimony rides last. The parsimony key inverts at the first demanded annotation-facet — a table that beat `Schema.Class` for no-derivation dispatch loses the moment one `Equivalence` is demanded, and `arbitrary` is absent rather than expensive (a property-test demand is a hard `isSubset` failure). The four precedence keys are one lexicographic tuple order, never four sibling comparators: `Order.tuple` reads facets, cardinality, channels leftmost-decisive and parsimony reversed in the last slot. `Data.taggedEnum` exposes no static merge target, so the family map and precedence defaults ride one `as const satisfies Record<Signature['_tag'], …>` tier-table read by indexed access — a bare `$match` arm widens the elected `'Literal'` to `string` and loses the codomain, where the `as const` row pins the literal. The chooser is then the one value owner, the tier-table, and exactly two operations — `dominant` (the fused tuple order) and `route` (the entrypoint) — collapsing the four sibling `Order`s and the standalone `family` matcher; the `Family` literal union is `(typeof Tier)[Signature['_tag']]['family']` lifted into `route`'s codomain, the consumer reading `route(demanded, pool)`.

```typescript conceptual
type Facet = 'equivalence' | 'arbitrary' | 'fallback' | 'jsonSchema';
type Signature = Data.TaggedEnum<{
    Nominal: { readonly facets: HashSet.HashSet<Facet>; readonly declarations: number };
    Reference: { readonly facets: HashSet.HashSet<Facet>; readonly declarations: number };
    Structural: { readonly facets: HashSet.HashSet<Facet>; readonly cardinality: number; readonly channels: number; readonly declarations: number };
}>;
const Signature = Data.taggedEnum<Signature>();
// one tier-table is the single source: `family` lifts the elected literal, the base columns seed the comparator — read by indexed access, never a parallel Match (a bare $match arm widens `'Literal'` to `string`, losing the codomain)
const Tier = {
    Nominal: { family: 'Literal', card: 1, chan: 0 },
    Reference: { family: 'taggedEnum', card: 1, chan: 0 },
    Structural: { family: 'TaggedClass.Union', card: 0, chan: 0 },
} as const satisfies Record<Signature['_tag'], { readonly family: string; readonly card: number; readonly chan: number }>;
// one lexicographic tuple over (facets, cardinality, channels, -declarations): the four sibling Orders collapse to one mapInput
const dominant = Order.mapInput(Order.tuple(Order.number, Order.number, Order.number, Order.reverse(Order.number)), Signature.$match({
    Nominal: (s) => [HashSet.size(s.facets), Tier.Nominal.card, Tier.Nominal.chan, s.declarations] as const,
    Reference: (s) => [HashSet.size(s.facets), Tier.Reference.card, Tier.Reference.chan, s.declarations] as const,
    Structural: (s) => [HashSet.size(s.facets), s.cardinality, s.channels, s.declarations] as const,
}));
const route = (demanded: HashSet.HashSet<Facet>, pool: ReadonlyArray<Signature>): Option.Option<{ readonly elected: (typeof Tier)[Signature['_tag']]['family']; readonly handSpell: HashSet.HashSet<Facet> }> =>
    pipe(pool, Array.filter((c) => HashSet.isSubset(demanded, c.facets)), Array.sort(dominant), Array.last,
        Option.map((s) => ({ elected: Tier[s._tag].family, handSpell: HashSet.difference(demanded, s.facets) })));
```

[ROUTE_CODOMAIN]:
- Law: one `filterEffect` field among twenty pure ones lifts the owner's `R` off `never`, so `decodeUnknownSync`/`decodeSync`/`encodeSync` vanish while `is`/`asserts`/`Pretty.make`/`Arbitrary.make`/`Schema.equivalence` survive; route to the family, identifier, and `R` together.
- Use: mint a write-once `$defs` identifier the moment an owner is embedded at three or more sites — first-writer-wins-silent collision is the JSON-Schema-explosion trigger.
- Law: row [3] is the lone silent axis — `Equal`/`Hash` bakes into the constructor at declaration, so a structural-to-reference flip leaves every `Equal.equals` call type-checking while its runtime answer inverts.
- Boundary: the `as const satisfies` tier-table read by indexed access discharges case-exhaustion and pre-access membership at once, so the family map needs no `default` arm and the elected literal survives where a `$match` arm widens it; a fallback arm is itself the diagnostic that the `Signature['_tag']` key proof was lost upstream.

## [2]-[STRUCT_CLASS_OWNERS]

`class Self extends Schema.Class<Self>("Id")({...fields})` is one declaration yielding six fused surfaces — the nominal class type, the runtime value-constructor, the `Schema<Self, Encoded, R>` codec, structural `Equal`/`Hash` off the `Data.Class` prototype, an `Arbitrary`, and a `Pretty` — collapsing `interface X` + `const codecX` + `type XT = typeof` + `class XImpl`. Class-body methods ride `Proto` and are part of the instance type while the codec never serializes them, so a getter changes the dispatch surface with zero wire-shape change and the round-trip reconstructs it because decode runs `new this`.

[CONSTRUCTOR_ADMISSION]:
- Law: the `<Self>` generic is mandatory; omitting it resolves the base to the `MissingSelfGeneric` template-literal string whose text spells the corrected re-declaration.
- Law: `new`/`make` validate the decoded type plus refinements over `typeAST(schema.ast)` while `Schema.decodeUnknown(Self)` validates the encoded type then runs every transform — so a `Schema.NumberFromString` field is a `number` at the constructor and a `string` at decode.
- Boundary: `new Self(props, { disableValidation: true })` skips only the `validateSync` call (`_tag` strip-and-regenerate and `lazilyMergeDefaults` still run); `decodeUnknown` has no such knob, so a wire value can never bypass the refinement.
- Use: `Schema.optionalWith(s, opts)` over a hand-built optional — `{ default }` flips the type token `?: → :`, `{ as: "Option" }` lifts to `Option<A>`, `{ exact: true }` removes `| undefined`, and the closed `OptionalOptions` union type-rejects illegal mixes like `default` with `as`; `Schema.fromKey(encodedKey)` remaps only the encoded key, preserving the `HasDefault` flag.

[DEFAULT_SEAM]:

| [INDEX] | [MODIFIER]               | [AST_WRITE]                       | [FIRES_AT]   | [HASDEFAULT] |
| :-----: | :----------------------- | :-------------------------------- | :----------- | :----------: |
|   [1]   | `withConstructorDefault` | type-side `defaultValue` slot     | `new`/`make` |    `true`    |
|   [2]   | `withDecodingDefault`    | `PropertySignatureTransformation` | decode       |   `false`    |
|   [3]   | `withDefaults`           | one call, both AST writes         | both         |    `true`    |

[REFINED_OWNER]:
- Use: `Schema.Struct({...}).pipe(Schema.filter(pred))` as the first `Schema.Class` argument for a cross-field invariant gating `new`/`make` and `decodeUnknown` alike; reject `class X` + free `assertValid(x)` + the call-site discipline that remembers to invoke it.
- Law: the predicate returns `undefined`/`true` (pass), `false`/`string` (the message), a `FilterIssue = { path; message }` or an array of them (path-targeted, multiple under `errors: "all"`), or a `ParseIssue`; a type-guard predicate narrows the owner to `C & B` on every projection, and `Schema.filterEffect` joins its requirement onto `R`.
- Use: `Self.extend<Sub>("SubId")({...newFields})` for structural widening — `Proto` survives, `instanceof Parent` holds, and a parent-versus-new key collision throws inside `extendFields` at module-load; the subclass re-declares no codec, `Equal`/`Hash`, or `make` — the parent type threads through the `Inherited` slot of `Class<…, Inherited, Proto>` — while a parent-declared static comparator is reused under the parent name (author statics do not propagate onto the subclass type).
- Law: the constructor obligation accrues `C & Struct.Constructor<NewFields>`, so the first bare addition breaks every `new Sub()` locally while a defaulted one leaves call sites untouched.
- Law: `.transformOrFail` decodes the computed field against the decoded parent, `.transformOrFailFrom` against the parent's encoded record — both leave `I` unchanged and union the function requirements onto `R`, so a context-dependent computed field is reachable only through them and stays wire-invisible.

```typescript conceptual
class Bounded extends Schema.Class<Bounded>('Bounded')(
    Schema.Struct({ low: Schema.Number, high: Schema.Number, mark: Schema.Number }).pipe(
        Schema.filter((s): Schema.FilterIssue | undefined =>
            s.low > s.high ? { path: ['high'], message: '<high-below-low>' }
                : s.mark < s.low || s.mark > s.high ? { path: ['mark'], message: '<mark-out-of-bounds>' }
                : undefined),
    ),
) {
    get width(): number { return this.high - this.low; }
    static readonly admit = Schema.decodeUnknown(Bounded);
    static readonly order = Order.mapInput(Order.number, (s: Bounded) => s.width);
}
// the family widens with zero re-declared surface: Proto, codec, and Equal/Hash thread through Inherited; the parent's `order` static is reused under the parent name
class Refined extends Bounded.extend<Refined>('Refined')({
    factor: Schema.optionalWith(Schema.Positive, { default: () => 1 }),
}) {
    get scaled(): number { return this.width * this.factor; }
    // `Bounded.order` is the parent's derived comparator reused verbatim — `Order<Bounded>` sorts `Refined` rows by contravariance, never re-declared on the subclass
    static readonly widest = (rows: ReadonlyArray<Refined>): Option.Option<Refined> => Array.last(Array.sort(rows, Bounded.order));
}
```

`Schema.TaggedClass`/`TaggedStruct`/`TaggedError` differ only by `Proto`: `TaggedClass` injects `_tag` as a `withConstructorDefault`-backed first field (`Proto = {}`), `TaggedError` the same with `Proto = cause_.YieldableError` (making the instance value-keyed and a member of every `E` alphabet, so `yield* error` is the value itself), `TaggedStruct` the value-only sibling with no `new`. Derive a projection that must stay instantiable field-side from `Self.fields` into a new `Schema.Class`; reach for top-level `Schema.pick`/`omit`/`partial` (returning a bare `SchemaClass`) only for a transient decode contract.

One ladder fixes which owner terminates the climb by what each rung adds: `as const` for a decode-free vocabulary, `Data.struct` for deep equality with no value decodes, `Schema.Data(struct)` for a wire need with no methods or `extend`, `Schema.Class<Self>(id)` the moment a method or an `extend`/`transform` line appears.

[RECURSIVE_OWNER]:
- Use: `Schema.suspend((): Schema.Schema<Self> => Self)` is the sole legal forward reference inside a field record; the single-param form holds only while the encoded face equals the instance type — a `Proto` method or a mutual cycle forces a hand-declared encoded `interface` as the `I` argument `Schema.suspend((): Schema.Schema<Self, SelfEncoded> => Self)`.
- Law: select the deferral by cycle domain — `Schema.suspend` for a codec cycle, `Layer.suspend` for a requirement cycle, `Effect.suspend` for a computation cycle — admitted only on a genuine value-level back-edge; an acyclic forward reference is repaired by source reorder, the precautionary thunk plus `var` hoist rejected as a loud TDZ throw converted to silent corruption.
- Boundary: a recursive leaf clamps its `Arbitrary` depth (`maxDepth`/`depthIdentifier`) overriding a wider `maxItems`, and a `Suspend` always demands an identifier for JSON-Schema — the named class supplies it where an anonymous struct forces `.annotations({ identifier })`.

[DECLARATION_MERGING]:
- Use: `static readonly`/`static` members on the class body are the value-side merge under `erasableSyntaxOnly` (a value-bearing `namespace X` is rejected by `TS1294`), fusing the derived `Order`, `Equivalence`, key-proof, and any projector onto the single imported name dotted `X.order`/`X.equivalence`/`X.dedupe`; deletes a companion `const XStatics`, a mirror `interface XShape`, a `type X = typeof X` re-export, and every loose top-level `const` beside the owner.
- Law: a self-referential static (`Schema.equivalence(X)`, `Struct.getOrder` over `keyof X`) is legal because the static initializer runs after the class binding is bound, so the back-reference resolves where a value-`namespace` above the class would read a TDZ binding; an unexported class needs no `isolatedDeclarations` annotation, an exported one cannot extend an expression at all (`TS9021`).
- Boundary: free-static availability is per-family — `Data.Class`/`Data.TaggedClass` reserve nothing, `Schema.Class` walls `make`/`fields`/`extend`/`annotations`/`ast` (a colliding static fails `TS2417`), `Effect.Service` walls `Default`/`make`/`use`/`key` plus every `accessors: true` method name, and `Data.taggedEnum` is plain-object factories with no static surface (later author-attached seeds forcing a `Schema.Union` of `TaggedClass`).

```typescript conceptual
class Span extends Schema.Class<Span>('Span')({
    label: Schema.NonEmptyString, started: Schema.Number, elapsed: Schema.Number,
}) {
    get ended(): number { return this.started + this.elapsed; }
    // `equivalence` self-references Span: legal as a static (initializer runs post-binding), TDZ-throws inside a value-`namespace` above the class
    static readonly equivalence = Schema.equivalence(Span);
    static readonly order = Order.mapInput(Order.number, (s: Span) => s.ended);
    static recent(rows: ReadonlyArray<Span>): ReadonlyArray<Span> { return Array.sort(Array.dedupeWith(rows, Span.equivalence), Order.reverse(Span.order)); }
}
```

## [3]-[VARIANT_OWNERS]

One union owner is selected by codec demand, never arm count: a wire-crossing family takes `Schema.Union` of tagged members, an interior-only family of any size takes `Data.taggedEnum`. The bare inline `type T = A | B` floor holds only at one discriminant reader never serialized, admitting exactly one `x.kind === "<lit>"` read before the minimum moves to `Data.taggedEnum` whose `$is("<lit>")` is that guard derived once. Re-wrapping a self-`_tag` shape through `Data.taggedEnum` is rejected by `ChildrenAreTagged<A>`, forcing the hand-spelled inline tagged union or the generated owner, never both layered.

Reach for `Schema.tag(literal)` as the default discriminant — the only spelling decode-validated, encode-emitted, and constructor-elided at once (`HasDefault`-defaulted). Reject `_tag: Schema.Literal(...)` where the tag should not be a required constructor argument, and reach for `attachPropertySignature` (the one combinator owning both coupling directions) for a legacy tag that must not serialize, the paired add-on-decode plus delete-on-encode `transform` being the reject; a `symbol` discriminant maximizes decoupling, unreachable by JSON yet live for `Match.discriminator`. A discriminant is admitted only when `AST.isLiteral(type) && !isOptional`, so a tag typed `Schema.Literal("a", "b")` or an optional tag silently falls to `otherwise` — the same gate keying decode and `Match.discriminator`; `onExcessProperty: "error"` then forces non-overlapping field sets per shared bucket.

Two totality detectors differ on what they catch. `Match.exhaustive` gates on `Remaining` subtracting to `never` (the one-sided proof) and cannot catch a stale arm — a dropped member leaves a `Match.tag(removed)` ladder type-checking; `$match`/`tagsExhaustive`/`discriminatorsExhaustive` gate on a record whose keys must equal the tag alphabet (the two-sided proof) and reject a removed key as `never`. Reject the optional-handler twins `Match.tags`/`Match.discriminators` outside a deliberately-open family — they thread the residual into a trailing `orElse` so appending an arm silently widens the default; place `Match.withReturnType<Ret>()` first or it stops gating and admits a divergent return. Of the widening vectors, `Schema.Union(...e, NewArm)` is the loudest (re-opens both detectors); a new `Data.taggedEnum` key re-opens only the record detector; a matcher over an absorbed union re-opens `Remaining` silently at the finalizer; `Schema.NullOr(Owner)` re-opens only `exhaustive` (leaving `null` in `Remaining`), reconciled by admitting `null` as a `Schema.tag`-defaulted arm.

```typescript conceptual
class Pending extends Schema.TaggedClass<Pending>()('Pending', { queued: Schema.Int }) {}
class Active extends Schema.TaggedClass<Active>()('Active', { since: Schema.Number, worker: Schema.NonEmptyString }) {}
class Closed extends Schema.TaggedClass<Closed>()('Closed', { code: Schema.Int }) {
    get terminal(): boolean { return true; }
}
const Phase = Schema.Union(Pending, Active, Closed); // Union returns a Schema value, not a class — no static merge target, the codec and dispatch stay free consts
const weigh: (phase: typeof Phase.Type) => number = Match.type<typeof Phase.Type>().pipe(
    Match.withReturnType<number>(),
    Match.tagsExhaustive({ Pending: (p) => p.queued, Active: (a) => a.worker.length, Closed: (c) => c.code }),
);
const admit = Schema.decodeUnknown(Phase);
```

[NARROWING_ALGEBRA]:
- Law: union narrowing is one closed algebra — a literal sub-vocabulary, a sub-union, a stream filtered to one arm, one arm's recovered codec each yield a value the algebra accepts again, closure breaking only when a step drops to `union.ast.types` (a stripped node with no codec).
- Use: `Schema.Literal(...values).literals` is the legal set (never a parallel `as const`), `pickLiteral(...subset)` carves discriminant literals, `Extract<Type, { _tag: Sub }>` the arms; `Array.partition($is("Open"))` threads `Extract`/`Exclude` both halves exact — reject `Array.groupBy(values, v => v._tag)`, whose key selector widens `_tag` to `string`, and treat `$is(tag)` as a `_tag`-only membership guard never full-value validity.
- Use: provenance selects the matcher form — `Match.type<I>()` finalizes to a reusable `(input: I) => …`, `Match.value(i)` to the value directly — the residual disposition then read off the [ABSENCE_DISPOSITION] dispatch column.
- Reject: a bare `Predicate` for route narrowing — it subtracts nothing and propagates a phantom domain breaking two stages downstream; use a `Refinement`.
- Boundary: a staged channel composes through `Either.orElse`, so stage one's residual `E` is literally stage two's `Match.type<E>` input.

[REQUEST_OWNER]:
- Use: `Schema.TaggedRequest<Self>()(tag, { payload, success, failure })` fuses six surfaces (tagged value-constructor, payload codec, structural `Equal`/`Hash`, runnable `Request`, `SerializableWithResult` owner, `Schema.Union`-joinable class) in one declaration.
- Use: `Request.completeEffect(arm, effect)` checks the per-arm `Exit<SA, FA>` shape while `Match.tagsExhaustive` over the `Schema.Union` proves discriminant coverage — outcome and dispatch at once.
- Reject: a hand-written `Schema.Exit({...})` beside a separate `Effect<A, E>`, two contracts the next outcome desynchronizes.
- Law: a fault that must survive serialization typed belongs in `failure` as a `TaggedError` arm, an infallible arm as `Schema.Never`; the third `ExitEncoded` slot is pinned to `unknown` by the hardcoded `Schema.Defect` codec.

```typescript conceptual
class FetchFault extends Schema.TaggedError<FetchFault>()('FetchFault', { key: Schema.String }) {}
class Fetch extends Schema.TaggedRequest<Fetch>()('Fetch', {
    payload: { key: Schema.NonEmptyString }, success: Schema.Number, failure: FetchFault,
}) {}
class Probe extends Schema.TaggedRequest<Probe>()('Probe', {
    payload: { text: Schema.String }, success: Schema.Int, failure: Schema.Never,
}) {}
const Demand = Schema.Union(Fetch, Probe);
// Match.type finalizes a reusable dispatcher — a fourth request lands as one arm the record detector demands, its Exit shape checked by completeEffect
const dispatch: (request: typeof Demand.Type) => Effect.Effect<void> = Match.type<typeof Demand.Type>().pipe(
    Match.tagsExhaustive({
        Fetch: (arm) => Request.completeEffect(arm, arm.key.length > 0 ? Effect.succeed(arm.key.length) : Effect.fail(new FetchFault({ key: arm.key }))),
        Probe: (arm) => Request.completeEffect(arm, Effect.succeed(arm.text.length)),
    }),
);
```

## [4]-[IDENTITY_AND_ALGEBRA]

`class X extends Data.Class<{...}>`/`Data.TaggedClass(tag)<{...}>` is type, value-constructor, and `Equal`/`Hash` instance with no separate comparator; `Schema.Data(schema)` stamps `[Equal.symbol]`/`[Hash.symbol]` onto decoded output so boundary-admitted records are `HashSet`-key-correct by construction. The prototype fixes the tier permanently and is never selectable at the comparison site — `StructuralPrototype` (`Object.keys` equality), `ArrayProto` (index-wise), `EffectPrototype` (`this === that`) — so a `Layer` in a `HashSet` deduplicates by reference for its lifetime.

[COMPARATOR_GATE]:
- Law: every comparator opens on the reference gate (`Equal.equals` runs `self === that` first, `Equivalence.make(f)` is `self === that || f(...)`, `Order.make(c)` is `self === that ? 0 : c(...)`), so a hand-rolled `self === that` head is dead code and identity fusion is required on both operands.
- Boundary: `Order` leaves never return `0`, so the gate is the sole tie path — under-eager on `NaN` (the `SortedSet` structural sink) and over-eager on signed zero.
- Law: a custom `[Hash.symbol]` must call `Hash.cached` or every `HashMap` operation re-hashes, and mutating a `Data` value after first `Hash.hash` desyncs the frozen memo; a fast-wrong `Equal` is a correctness bug a passing `Hash` will not surface.
- Use: place the highest-cardinality key leftmost in `Order` (a correctness order, it exits on the first disagreeing key) but the most-volatile field leftmost in `Equivalence` (a pure performance lever, conjunction commutes).
- Boundary: equality bridges to `Equivalence` via `Equal.equivalence<A>()` but ordering does not — `Order` derives no `Equal` and `Equal` no `Order`; `Order.reverse(O)` flips one key while `Equivalence` has no `reverse`, and `Order.empty()` is a stable total tie while `Equivalence.combineAll([])` seeds the everything-equal instance, a correctness hazard not a default.

[FOUR_LAW_SAFETY]:

| [INDEX] | [INSTANCE]               | [REPARENTH_SAFE] | [REORDER_SAFE] | [DEDUP_SAFE] |
| :-----: | :----------------------- | :--------------: | :------------: | :----------: |
|   [1]   | `MonoidSum`/`Multiply`   |       yes        |      yes       |      no      |
|   [2]   | extremum / `MonoidEvery` |       yes        |      yes       |     yes      |
|   [3]   | `MonoidXor`/`MonoidEqv`  |       yes        |      yes       |      no      |
|   [4]   | `String.Monoid`          |       yes        |       no       |      no      |

- Law: each algebraic law licenses exactly one fold-body edit — associativity reparenthesizes, commutativity reorders, idempotence deduplicates, a two-sided identity drops-and-seeds the empty case; reject `MonoidXor` where `MonoidSome` was meant, since `Xor` counts parity and even multiplicity cancels, passing every distinct-element test until a value repeats.
- Law: a struct fold is reorder-safe iff every column's algebra is commutative, so one `Semigroup.last` discriminant poisons the whole record, and lifting non-commutative `String.Monoid` through an unbounded-concurrency applicative scrambles the concatenation with no type error.

[ALGEBRA_STRENGTH]:
- Law: one value owner derives its whole algebra family from one declaration; keep distinct channel sets through `Product.struct(F)` where each field's channels union, never `combineMany`/`combineAll` which pin one `R, O, E, A` and narrow a runtime-length fold to the common channel.
- Law: prove associativity at the leaf `Semigroup` admission, never at any `fromSemigroup`/`getOptionalMonoid`/`Monoid.min` re-check site — strength promotion funnels every identity through `Monoid.fromSemigroup(S, empty)` and is a one-way trust gate, and a literal `empty` is the lone source that can be wrong because `combineAll([])`/`combineAll([a])` touch only one side, so a left-neutral-only unit corrupts only a multi-element fold; `Semigroup.intercalate` admits no `Monoid` promotion (unit unsettable in principle).
- Law: each witness cell is a present-or-absent const read the carrier's shape forecloses — `Either` carries no `Filterable` because a single-success slot cannot drop, `Option` the full coproduct stack because `None` is both drop target and `zero`; `Traversable<T>` extends `TypeClass` directly, so walk/fold/filter never bundle into one heritage.
- Boundary: the export form is a five-way signature — a bare const fixes behavior by the carrier, `get*(subAlgebra)` an undetermined inner algebra, `get*(options)` a runtime policy, `<A>()` a floating element type, an absent export an uninhabitable operation.

```typescript conceptual
class Tally extends Schema.Class<Tally>('Tally')({
    hits: Schema.Int, peak: Schema.Int, label: Schema.String,
}) {
    // each column carries its own combine: sum commutative+dedup-unsafe, max idempotent, last the reorder-poison — one record Semigroup would pin a single law across all three
    private static readonly additive = Semigroup.struct({ hits: Number.SemigroupSum, peak: Semigroup.max(Order.number), label: Semigroup.last<string>() });
    // one-way trust gate: associativity proven at the leaf, the literal `empty` the lone wrongable source — a left-neutral-only unit corrupts a multi-element fold
    private static readonly monoid = Monoid.fromSemigroup(Tally.additive, { hits: 0, peak: 0, label: '' });
    static fold(rows: ReadonlyArray<Tally>): Tally { return new Tally(Tally.monoid.combineAll(rows)); }
}
```

[KEYED_IDENTITY]:
- Use: `Struct.getEquivalence(table)`/`Struct.getOrder(table)` fold a field-keyed record of bare leaf instances into one aggregate — reject the N scattered `mapInput` closures the manual spelling spreads across use sites.
- Law: anchor the non-auto-derived `Order` over a `keyof`-array entry forced at the same site by `satisfies { readonly [K in keyof Owner]: Order.Order<Owner[K]> }`, because no `OrderAnnotationId` exists — a schema field addition widens codec and `Schema.equivalence` silently-correct while only the `keyof`-anchored table breaks sort precedence loudly; equal-implies-hash-equal then holds by anchor, not by audit.

```typescript conceptual
class Entry extends Schema.Class<Entry>('Entry')({
    key: Schema.String, stamp: Schema.Number, weight: Schema.Number,
}) {
    static readonly keys = ['key', 'stamp', 'weight'] as const satisfies ReadonlyArray<keyof Entry>;
    static readonly equivalence = Schema.equivalence(Entry);
    // no OrderAnnotationId: a new field widens `equivalence` silently-correct but breaks this `keyof`-anchored table loudly
    static readonly order = Struct.getOrder({
        key: Order.string, stamp: Order.reverse(Order.number), weight: Order.number,
    } satisfies { readonly [K in keyof Entry]: Order.Order<Entry[K]> });
    static digest(self: Entry): number { return Hash.structureKeys(self, Entry.keys); }
    static dedupe(rows: ReadonlyArray<Entry>): ReadonlyArray<Entry> { return Array.dedupeWith(rows, Entry.equivalence); }
}
```

[STRUCTURE_ROUTING]:
- Law: a too-coarse identity costs at the locate site never the insert site, so spread hash entropy low — a HAMT slices one 5-bit window per level and bits 30-31 fragment four-way from depth seven alone while bits 0-29 branch 32-way.
- Law: binary `union`/`intersection` re-admit the second operand through the receiver's `Order`, lawful only when every operand was built under one shared comparator.
- Use: `mapInput` turns full-value identity into field-scoped identity, so one owner feeds a content-deep `HashSet` and a one-field-keyed `RedBlackTree` at once; `Array.dedupe` consumes default identity, `dedupeWith` the override seam.

[CONTAINER_GRAIN]:

| [INDEX] | [STRUCTURE]             | [IDENTITY_SOURCE]             | [LOCATE_HAZARD]                  |
| :-----: | :---------------------- | :---------------------------- | :------------------------------- |
|   [1]   | `HashSet`/`HashMap`     | stored value `[Equal.symbol]` | wide hash scatters, narrow scans |
|   [2]   | `SortedSet`/`SortedMap` | captured `Order<K>`           | order-tie evicts distinct key    |
|   [3]   | `RedBlackTree`          | captured `Order<K>` multimap  | route-past hides a present value |

[PROJECTOR_FOLD]:
- Use: author a bespoke `Match<ColumnSpec>` off the owner's `ast` to derive a genuinely new family — a SQL-column plan, a form descriptor, a binary layout — inheriting `path` and totality; the codec, `Pretty`, `JSONSchema`, `Arbitrary`, and `Schema.equivalence` are five carriers over one `getCompiler(match)` recursion, so a missing arm is a key error and a new node kind extends every projector at once.
- Reject: a hand-walked `if (ast._tag === ...)` tree where `getCompiler` ties the open recursion total, or reading/rebuilding `.ast` where `typeAST`/`encodedAST`/`pick`/`partial` already expresses the rewrite — descend to fold, never reconstruct a node a combinator constructs.

[BRAND_WALL]:
- Use: `Brand.nominal<T>()` only when two same-typed values must not be confused, `Brand.refined<T>(predicate, onFailure)` for a checked one exposing throw/`.option`/`.either`/`.is` off one constructor; reject a `makeCode`/`tryCode`/`isCode` sibling family, and a brand with no predicate and no real nominal collision.
- Law: branding is identity-transparent — it guards assignment never membership, so a branded value in a `HashSet` locates by the base's `Hash`/`Equal` and two brand stacks over one base dedup to one; the schema brand's `Refinement` arm descends straight to the base, so a triple-branded schema's runtime identity is byte-identical to the bare base.
- Boundary: a branded sort key surfaces the wall at the one hand-composed `Order.struct` — the brand erases there, so the field-key anchor that closes the ordering gap also declares the wall once for every aggregate the table folds into.

## [5]-[VOCABULARY_OWNERS]

The behavior-column table is one `as const satisfies Record<Key, Row>` literal — rows are keys, columns per-key data, the binding the decode-free, dependency-free vocabulary owner. Operator order is the closure: `as const` freezes every leaf and marks the structure `readonly` recursively, then `satisfies` checks each row against `Record<string, Row>` without widening, so a forgotten column is a declaration-site error. The whole read surface is two operators — `keyof typeof Table` names the key union, indexed access `Table[k]` returns the row — and a function column makes the same owner a dispatch surface read `Table[k](arg)` but forfeits structural `Equal`/`Hash` and JSON projection, so a data-projection table and a dispatch table over one vocabulary are two reads of the same key set, never one mixed table. Reach for the codec-backed `Schema.Literal(...values)` (fusing the union, the decoding codec, and `.literals`) the moment a boundary supplies the alphabet, an annotation message must speak the rejection, or a `jsonSchema` projection is needed.

[LEAF_DEFEAT]:

| [INDEX] | [LEVER]                           | [KEY_AXIS] | [LEAF_AXIS] |
| :-----: | :-------------------------------- | :--------- | :---------- |
|   [1]   | `as const satisfies Record<K,R>`  | preserved  | preserved   |
|   [2]   | `: Record<K, Row>` or a wide sink | widened    | widened     |
|   [3]   | `satisfies` without `as const`    | preserved  | widened     |
|   [4]   | `<const R extends ...>` parameter | preserved  | preserved   |

Under no-unchecked-indexed-access a `keyof typeof`-keyed dispatch discharges case-exhaustion and pre-access membership at once and needs neither a `default` arm nor a membership probe — a fallback arm is itself the diagnostic that the key proof was lost upstream. A clean `as const satisfies` table flowed into a contextual position typed `Record<K, Row>` is re-widened even with a correct annotation, the fix the `const` type parameter freezing both axes at the entrypoint once (a per-column `rank: 0 as number` the deliberate escape for a tunable threshold sharing one owner). The proof cannot be rebuilt from a runtime structure (`Record.fromEntries` widens to `NonLiteralKey<K>`), and `Table[k]` over an erased `k: string` never refuses to compile while a `Match.exhaustive` on the same union fails loud, so audit the read site's key type, never the read's compilation.

[PROOF_PRESERVING]:

| [INDEX] | [COMBINATOR]                     | [KEY_FATE] | [WHY]                          |
| :-----: | :------------------------------- | :--------- | :----------------------------- |
|   [1]   | `Record.map`                     | preserves  | callback-position `NoInfer<K>` |
|   [2]   | `Record.union`/`difference`      | preserves  | nameable union `K0 \| K1`      |
|   [3]   | `Record.intersection`            | guarded    | erases only once a side leaked |
|   [4]   | `Record.fromEntries`/`filterMap` | erases     | key flows back to output       |

A combinator keeps the proof iff `K` reaches only the callback position and the result key set is a nameable union, so drop columns inside the value position with `Record.map`, never key-position through `filterMap`. The composite-key vocabulary is `Schema.TemplateLiteral(...params)` synthesizing the cartesian product of its factor alphabets — `StringKeyword`/`NumberKeyword` are the only open nodes, so a `Schema.Boolean` factor synthesizes the closed `'true' | 'false'`, openness decided by primitive identity not cardinality. `Schema.TemplateLiteralParser` is mandatory exactly when `Encoded` differs from `Type` for any segment, the per-factor `Match.value(slot)` dispatch over its tuple collapsing `n₁·n₂` table rows to `n₁ + n₂` arms.

```typescript conceptual
const Major = Schema.Literal('a', 'b', 'c');
const Minor = Schema.Literal('x', 'y', 'z');
const Code = Schema.TemplateLiteral(Major, '.', Minor);
const Decomposed = Schema.TemplateLiteralParser(Major, '.', Minor);
// the factor literals seed the product table directly: one row per cartesian member, a missing combination a declaration-site error naming the absent composite key — no parallel allowlist, no hand-spelled Row type
const Policy = {
    'a.x': { flag: false, rank: 0 }, 'a.y': { flag: false, rank: 0 }, 'a.z': { flag: false, rank: 1 },
    'b.x': { flag: true, rank: 1 }, 'b.y': { flag: true, rank: 1 }, 'b.z': { flag: true, rank: 2 },
    'c.x': { flag: true, rank: 2 }, 'c.y': { flag: true, rank: 2 }, 'c.z': { flag: true, rank: 3 },
} as const satisfies Record<typeof Code.Type, { readonly flag: boolean; readonly rank: number }>;
const weigh = (code: typeof Code.Type): number => {
    const [major, , minor] = Schema.decodeSync(Decomposed)(code);
    return Policy[code].rank +
        Match.value(major).pipe(Match.when('c', () => 4), Match.when('b', () => 2), Match.when('a', () => 1), Match.exhaustive) *
        Match.value(minor).pipe(Match.when('z', () => 3), Match.when('y', () => 2), Match.when('x', () => 1), Match.exhaustive);
};
```

Admission-as-decode and admission-as-dispatch are one gate selected by provenance — a foreign value crosses the decode gate that produces the typed value, a value already of the type crosses the dispatch gate that routes one it does not construct (`decodeUnknown` ↔ `Match.type<I>()`, `decode` ↔ `Match.value(i)`).

[ABSENCE_DISPOSITION]:

| [INDEX] | [DISPOSITION]    | [DISPATCH_TERMINAL]  | [DECODE_TERMINAL]       | [CARRIES]             |
| :-----: | :--------------- | :------------------- | :---------------------- | :-------------------- |
|   [1]   | prove-impossible | `Match.exhaustive`   | `Schema.Literal` static | residual `never`      |
|   [2]   | assert-phantom   | `Match.orElseAbsurd` | `decodeUnknownSync`     | escapes at runtime    |
|   [3]   | resolve-fallback | `Match.orElse(f)`    | a default               | any residual accepted |
|   [4]   | carry-Option     | `Match.option`       | `decodeUnknownOption`   | reason erased         |
|   [5]   | carry-Either     | `Match.either`       | `decodeUnknownEither`   | residual preserved    |

A vocabulary that must fail the build uses the dispatch gate (adding a member breaks `Match.exhaustive` at compile time), one that must fail the request the decode gate (a widened `Schema.Literal` keeps compiling and rejects the new value at runtime); the decode `Left` carries a runtime `ParseError`, the dispatch `Left` a type. Resolution folds the missing key back to a designated member along the [DEFAULT_SEAM] axes — a make-side fallback rides `HasDefault`, a decode-side fallback the type token, `optionalWith({ default })` seeds a nullary `LazyArg` constant, `optionalToRequired` reads context through its `onNone` closure, `optionalWith({ as: "Option" })` the mutually-exclusive carry lifting the field to `Option<Type>`. A resolution needing a member off the alphabet is the signal the alphabet is wrong; add the member as a vocabulary row first, then name it as the default.

## [6]-[MODEL_OWNER]

`class Self extends Model.Class<Self>("Self")({...fields})` is one statement that is the value-constructor, the static `Type`, the `select` codec, structural `Equal`/`Hash`, and the carrier of five sibling variant codecs (`insert`, `update`, `json`, `jsonCreate`, `jsonUpdate`) — no `type Self = typeof Self`, no companion `const`, no separate `new`-target. Because `ast` is an `AST.Transformation` never a `Struct`, `Self.pick`/`omit` do not exist; narrow via `Self.fields` or a write variant's `Self.insert.pick(...)`, and `Self.annotations(...)` returns a plain `SchemaClass` forfeiting the variant surface, so identifier/message/`equivalence` ride the second `Model.Class` argument.

[MARKER_CONFIG]:

| [INDEX] | [MARKER]               | [EQUALS]                                          | [SLOT_SET]              |
| :-----: | :--------------------- | :------------------------------------------------ | :---------------------- |
|   [1]   | `Model.Generated`      | `FieldExcept("insert","jsonCreate","jsonUpdate")` | select/update/json      |
|   [2]   | `Model.Sensitive`      | `FieldOnly("select","insert","update")`           | no json bodies          |
|   [3]   | `Model.GeneratedByApp` | `FieldExcept("jsonCreate","jsonUpdate")`          | all but create variants |
|   [4]   | bare `Schema.X`        | the implicit uniform six-slot row                 | all six                 |
|   [5]   | `Model.Field({...})`   | the open per-slot config algebra                  | exact `keyof Config`    |

Every named marker is one config row, not a subtype with overrides — `Field<A>` is exactly `{ [FieldTypeId]; schemas: A }` with no method. A column has exactly one row; markers compose by per-slot rewrite of a flat row (`fieldEvolve` for representation, `Model.FieldOption` rewriting every present slot to `Option`), never a `Field<Field<...>>` nesting. Membership rides the `as` key-remap to `never`, so reading `Self.insert`'s dropped `id` is a property-does-not-exist error.

`withConstructorDefault` is the one combinator minting every elidable key — the [DEFAULT_SEAM] `HasDefault` flip leaving `R` untouched and keeping both tokens `":"`, so an elidable slot is constructor-optional but encoded-mandatory. `Schema.tag` and `VariantSchema.Overrideable` are two call sites of it: `Overrideable(from, to, { generate })` is `transformOrFail(from, Union(Undefined, to))` whose `generate: (Option<ITo>) => Effect<From, ParseIssue, R>` mints on the write pass against the live world, its `decode` arm collapsing to `succeed(undefined)` so the marker sits only in `insert`/`update`. `RequiredKeys<C> extends never ? void : C` toggles the whole `.make` arity with no flag — when every constructor key is elided the payload collapses to `.make()`, one bare-schema leaf flipping it back and breaking the bare call at compile time.

[PER_OP_REQUIREMENT]:

| [INDEX] | [METHOD]     | [REQUIREMENT_TERMS]                         |
| :-----: | :----------- | :------------------------------------------ |
|   [1]   | `insert`     | `Self["Context"] \| Self.insert["Context"]` |
|   [2]   | `insertVoid` | `Self.insert["Context"]` (no decode term)   |
|   [3]   | `findById`   | `Self["Context"] \| Context<id leaf>`       |
|   [4]   | `delete`     | `Context<id leaf>` alone                    |

The per-op requirement is a theorem off variant membership — a `Model.Sensitive(serviceLeaf)` adds the service to methods binding `select`/`insert`/`update` and to neither `delete` nor the json surfaces, recomputed off the widened field map with no method respelled; the derived methods carry no typed `E`, so failure provenance is recoverable only as a defect, never read off a typed channel. `Model.makeDataLoaders<S extends Model.AnyNoContext>` is a structural subtype ban closed only at the leaf — a service-carrying codec fails the `never` bound before any layer is wired, so close the gate by folding the requirement to `never` at the leaf's construction, never erasing it at the operation.

[WIRE_AXES]:

| [INDEX] | [AXIS]             | [POSITION]                                 | [SETS]                           |
| :-----: | :----------------- | :----------------------------------------- | :------------------------------- |
|   [1]   | membership         | `V extends keyof Config`                   | whether the wire carries the key |
|   [2]   | representation     | which transform wraps the interior         | `Redacted` vs bare leaf          |
|   [3]   | text-vs-structured | the encoded leaf `string` vs object        | `JsonFromString` db-vs-json slot |
|   [4]   | encoded-key        | the I-slot third `PropertySignature` token | `fieldFromKey` rename            |

A column's wire behavior is the product of four orthogonal axes, none recoverable from another, each a per-slot rewrite of one flat row — stacking a fifth modality one more rewrite, never a nesting. `Schema.Schema.Encoded<S>` peels every transform to the encoded leaf while the decoded interior stays invariant, so `JsonFromString(Redacted(inner))` peels both to `string`. A value-class promotion takes a variant struct (`Schema.Class<View>("View")(Self.json)`) never the owner, gaining body getters invisible to `Equal`/`Hash`. `Schema.encodedBoundSchema(Self.json)` keeps a `Refinement` only when `[StableFilterAnnotationId]: true` (set on `minItems`/`maxItems`/`itemsCount` alone), dropping a string `minLength`; the degenerate `Schema.instanceOf(Ctor)` has no `from` to peel, so its encoded leaf equals its decoded type until paired with a `transformOrFail`.

```typescript conceptual
class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Int),
    label: Schema.NonEmptyString,
    hidden: Model.Sensitive(Schema.Redacted(Schema.NonEmptyString)),
    payload: Model.JsonFromString(Schema.Struct({ inner: Schema.String })).pipe(Model.fieldFromKey({ select: 'payload_json', update: 'payload_json' })),
    mark: Model.Field({ select: Schema.String, insert: Schema.String, update: Schema.String }),
    createdAt: Model.DateTimeInsert,
    updatedAt: Model.DateTimeUpdate,
}) {}
const { select, insert, update, json, jsonCreate, jsonUpdate } = Owner;
```
