# Shape Decision Law

[SIGNATURE_AND_DOMINANT_AXIS]:
- The owner is a pure function of a five-axis signature read before any shape is written: identity regime (structural value / reference cell / nominal primitive), variant cardinality (one shape / N tagged / N closed behavior keys), payload timing (fixed at definition / per-occurrence per-channel), equality demand (deep `Equal`/`Hash` / reference / none), persistence demand (codec + per-channel projection / in-memory). Field count never enters the route.
- Identity regime splits the tree at the root: it selects whether the owner carries an `Equal`/`Hash` instance (`Schema.Class`/`Data.*`), a brand witness (`Schema.brand`), or neither (`as const` read by `keyof typeof`).
- The signature is itself a routed concept: its own axes (structural value, N-tagged on the dominant `identity` axis, fixed payload, no codec) route it to `Data.taggedEnum`, the dominant axis becoming the injected `_tag`, each member carrying only the secondary axes its tier reads. A flat `{ identity; wire?; cardinality? }` record is the reject — `Match.discriminators`/`$match` narrow each arm by `Extract<R, Record<D, P>>`, so a free literal-union field narrows every arm to `never` and the optionality lies (`Nominal` types `cardinality` present-but-undefined); the defect surfaces at the arm body's field access, never at the chooser declaration.
- A signature parameterized over the routed concept's own element type (`Data.taggedEnum<WithGenerics<1>>`) defeats per-arm narrowing silently: the data-last generic `$match` resolves slot `A` only at application, so an arm reading the element to refine the tier sees `A = unknown` and collapses to the widest tier with every arm still exhaustive — the signal is an arm inspecting slot `A`, which is the element's own signature leaking into the regime chooser and re-routes to a nested chooser run *after* the regime arm resolves.

```typescript
type Signature = Data.TaggedEnum<{
    Nominal: { readonly wire: boolean }
    Reference: {}
    Structural: { readonly cardinality: 'one' | 'many'; readonly wire: boolean }
}>
const Signature = Data.taggedEnum<Signature>()
const a = Signature.Nominal({ wire: true }) // _tag injected, args omit it; Reference() takes no args (VoidIfEmpty)
```

[ROUTING_TABLE]:
- Structural value, one shape, deep equality, wire need -> `Schema.Class<Self>(id)({...})`: one declaration yields constructor, `Type`/`Encoded`, codec, `Equal`/`Hash`, `make`. Reject: an `interface` plus a parallel `Schema.Struct` plus a hand-written `Equivalence`.
- Structural value, N tagged shapes, wire need -> a `Schema.Union` of `Schema.TaggedClass<Self>(id)(tag, {...})` folded by `Match.tagsExhaustive`. Reject: a discriminated `interface` union plus a decode `switch`.
- Structural value, N tagged shapes, in-memory only -> `Data.taggedEnum<T>()`; each tag constructor plus generated `$is`/`$match` ship in one statement. Reject: hand-written constructors plus a type-guard zoo.
- A concept that *is* a failure -> `Schema.TaggedError<Self>(id?)(tag, fields)`, whose type extends `Class<...>` with `cause_.YieldableError` in `Proto` — simultaneously a shape (codec, `Equal`/`Hash`), a `_tag` member, and a value the typed channel yields. The closed `E` alphabet and the `$defs` namespace are one set of tag literals. Reject: a `TaggedClass` shape declared beside a separate error type.
- Nominal primitive carrying a refinement -> `Schema.<base>.pipe(Schema.<filter>(), Schema.brand("X"))`. Reject: `type X = number & Brand<"X">` beside a separate runtime check.
- Closed keyset of behaviors, decode-free, dependency-free -> `as const` read by `keyof typeof`. Reject: an `enum` plus a parallel lookup `Record` plus a mapping `switch`.
- Closed set of bare literals, no payload, no behavior column -> `Schema.Literal(...values)` (`Type` = `values[number]`, codec, exhaustive matching). Reject: an `as const` array plus an `is`-guard; the reject above it is a behavior table with empty rows — a `Literal` wearing keys.
- Per-occurrence payload across channels -> `Model.Class<Self>(id)({...})` with `Model.Generated`/`Sensitive`/`FieldOption` modifiers. Reject: five `pick`/`omit` sibling schemas.

[IDENTITY_TIER_LADDER]:
- Four owners satisfy the structural-value-one-shape signature, ranked by what each adds: `as const` (no instance) -> `Data.struct` (`Equal`/`Hash`, no codec) -> `Schema.Data(struct)` (`Equal`/`Hash` plus codec) -> `Schema.Class<Self>(id)(fields)` (all prior plus named constructor, methods, `extend` lineage). The tie-break is the lowest tier carrying every demanded axis — over-tiering is dead surface, under-tiering is the missing axis.
- `Data.struct` is terminal exactly when deep equality is demanded but no value decodes: it has no `ast`, so `Schema.decodeUnknown` cannot consume it and `pick`/`omit` cannot project it. A wire channel re-routes to `Schema.Data` or `Schema.Class`.
- The presence of a method in the body promotes `Schema.Data` to `Schema.Class`: an empty-bodied class over a struct is `Schema.Data` wearing a constructor, and the constructor is ceremony. The class earns its tier only by carrying behavior or anchoring an `extend`/`transformOrFail` lineage.
- The class is individuated by its identifier, not its structure: its `ast` is an `AST.Transformation`, so two classes with identical fields but distinct identifiers are distinct owners with distinct codecs and `$defs` keys, and a copy-pasted body with a fresh identifier is a genuinely new owner. It presents to `Schema.pick`/`omit`/`encodedSchema` through a `SurrogateAnnotation` carrying the struct `ast`, so re-spelling the struct beside the class to make it projectable is the reject.

```typescript
const Coordinate = Schema.Data(Schema.Struct({ lat: Schema.Number, lon: Schema.Number }))
class Waypoint extends Schema.Class<Waypoint>('Waypoint')({
    at: Coordinate,
    label: Schema.NonEmptyTrimmedString,
}) {
    get key(): string {
        return `${this.at.lat}:${this.at.lon}`
    }
}
const a = Waypoint.make({ at: Data.struct({ lat: 0, lon: 0 }), label: '<value-a>' })
const b = Waypoint.make({ at: Data.struct({ lat: 0, lon: 0 }), label: '<value-a>' })
const same = Equal.equals(a.at, b.at) && Equal.equals(a, b) // structural Equal recurses through the embedded Data
```

[ROUTE_IS_A_COMPARATOR_OVER_A_DISCONTINUOUS_LATTICE]:
- The owner is the candidate that *dominates* under an `Order<Candidate>`, never a scalar minimum of a `C × V × F` product: two candidates with equal products carry disjoint facet sets, so scalarizing hides coverage. The covered-facet set is a `HashSet` one declaration yields; `HashSet.isSubset(demanded, covered)` is the admissibility gate, `HashSet.difference(demanded, covered)` the exact inventory of hand-spelled facets.
- The comparator is `Order.combineAll([byFacets, byCardinality, byChannels])` in fixed precedence and the combine order is the law: facets choose the family, cardinality tagged-vs-single, channels variant-vs-flat — channels-first reaches `Model.Class` on a concept with no structural identity. Parsimony rides last as `Order.reverse(Order.number)` over `c.declarations` (because *fewer* is *greater*): un-reversed it ranks the most verbose candidate highest, hoisted above the family keys it picks the smallest declaration regardless of coverage.
- The route is `Array.max` over the admissible candidates; the lattice is `Array.sort`-ed, not walked — the chosen owner is the last element, the demotion target the next-lower admissible one, and demotion is the same comparator reversed over the *filtered* set so it cannot strip a demanded facet. The break-even is per-member: a union of `M` members each needing `F ≥ 2` facets costs `M × F` hand-spelled against `M + 1` generated, overtaking at the first two-facet member.
- The functional is *not continuous* across the facet axis — the discontinuity is the annotation-derived facets (`equivalence`, `arbitrary`, `pretty`, `fallback`, JSON Schema): a generated owner derives each from its `ast` at zero marginal declaration, a raw owner pays a unit per facet, so the parsimony key inverts at the first demanded one — a table that beat `Schema.Class` for no-derivation dispatch loses the moment one `Equivalence` is demanded. Each facet's step differs: `equivalence` is steepest because `EquivalenceAnnotation<A, TypeParameters>` is `(...equivalences) => Equivalence<A>`, compositional over type parameters a flat hand comparator severs; `arbitrary` is *absent* not expensive (no `Arbitrary.make` source — a property-test demand is a hard `isSubset` admissibility failure); `fallback` scales with read sites (`(issue) => Effect<A, ParseIssue>` once versus `Effect.orElse` at every read). The step is per-*side*: the same facet on both decoded and encoded representations is one slot on the generated owner's three-slot annotation tuple, two hand functions on the raw — and `Class.annotations(record)` re-annotates but *sheds* the `new`/`make` constructor, so a late annotation costs the `value` facet it drops.

```typescript
type Facet = 'value' | 'type' | 'encoded' | 'codec' | 'equal' | 'arbitrary' | 'equivalence' | 'pretty' | 'fallback'
interface Candidate {
    readonly tier: number
    readonly declarations: number
    readonly covers: HashSet.HashSet<Facet>
    readonly cardinality: number // tagged members spanned: 1 single, N tagged
    readonly channels: number // representation channels spanned: 1 flat, N variant
}
const admits = (demanded: HashSet.HashSet<Facet>, c: Candidate): boolean => HashSet.isSubset(demanded, c.covers)
const byCollapse = (demanded: HashSet.HashSet<Facet>): Order.Order<Candidate> =>
    Order.combineAll([
        Order.mapInput(Order.number, (c: Candidate) => HashSet.size(HashSet.intersection(demanded, c.covers))), // byFacets: choose the family
        Order.mapInput(Order.number, (c: Candidate) => c.cardinality), // byCardinality: tagged-vs-single
        Order.mapInput(Order.number, (c: Candidate) => c.channels), // byChannels last — channels-first reaches Model.Class on a no-identity concept
        Order.mapInput(Order.reverse(Order.number), (c: Candidate) => c.declarations), // parsimony tie-break: fewer declarations ranks higher
    ])
const route = (demanded: HashSet.HashSet<Facet>, candidates: Array.NonEmptyReadonlyArray<Candidate>): Candidate =>
    Array.max(Array.filter(candidates, (c) => admits(demanded, c)) as Array.NonEmptyReadonlyArray<Candidate>, byCollapse(demanded))
```

[NEXT_DIFF_PARTITION]:
- Correct sizing is discharged, not asserted: the conceivable next-feature diff must sort into exactly one of three outcome classes the owner's `Type`/`Encoded`/`Context` triple fixes at declaration. Within-family re-route touches one declaration and no consumer; cross-family break fails every consumer at compile time; silent corruption shifts runtime semantics while the static `Type` is stable — forbidden, the only class with no compile-time witness.
- Within-family widening is decidable by one test: does the flip leave every prior member's `Type` and every prior call site's argument shape unchanged. Only two flips pass — cardinality one->N when the owner was already a `Schema.Union` of one `TaggedClass`, and persistence none->wire on an existing `Schema.Class` (the codec was always present). The widening splits two consumer sets the one diff hits differently: the *producing* surface gains a `new`/`make` with zero edit, every *consuming* `Match.tagsExhaustive` fails until its record gains the arm — a diff zero-churn on both is the signal the new case routed to a default arm and is silently mishandled.
- Identity regime is the lone silent axis: `Equal`/`Hash` is baked into the constructor at declaration and `Schema.equivalence` derives the comparator from the `ast` then, so a structural->reference flip leaves every `Equal.equals` call type-checking while its runtime answer inverts — no type error drives the migration. The conservatism gradient collapses the read to one asymmetric posture: read identity regime pessimistically (when reference identity is conceivable, the value owner is the wrong first route even though every consumer in hand wants structural comparison), read the four recoverable axes instance-sized (each carries its own loud break to drive its own widening). The hazard recurs one tier down — a `TaggedStruct` and a `TaggedClass` union member share one decoded `Type`, so a `TaggedStruct` used as a `HashMap` key compiles and dedupes by reference, corrupting the map with no diagnostic.

```typescript
class Money extends Schema.Class<Money>('Money')({ amount: Schema.BigDecimal, currency: Schema.String }) {}
const sameValue = (a: Money, b: Money): boolean => Equal.equals(a, b) // structural: bakes Equal/Hash into the Class ctor
const ledger = HashMap.empty<Money, number>().pipe(
    HashMap.set(new Money({ amount: BigDecimal.fromBigInt(1n), currency: '<x>' }), 1),
)
// reject: a flip to reference identity leaves `sameValue`'s TYPE intact while its runtime answer inverts —
// `ledger` would dedupe by reference; no `Equal.equals` site fails to compile, so the migration is undriven.
const reused: Equivalence.Equivalence<Money> = Schema.equivalence(Money) // derived from the ast at declaration, never re-derivable
```

[ROUTE_CODOMAIN_IS_THE_FAMILY_AND_TWO_SILENT_COORDINATES]:
- The chooser's output is the product `(family, identifier, R)`; a route returning the family alone has computed one third of its output and fails at a consumption surface the family never touched. The three fail on disjoint surfaces — family caught by the collapse comparator, identifier only by reading the whole `$defs` namespace, `R` only at a synchronous-extraction site.
- The `R` coordinate is *derived* from the field map by structural union — `Struct.Context<F> = Schema.Context<F[keyof F]>` — so the owner does not decide its context, its fields do: one `filterEffect` field among twenty pure ones lifts the whole owner's `R` off `never`, invisible at every other field, and `Class.extend` carries `R | Struct.Context<NewFields>` per lineage member. The non-`never` cut partitions *every* consuming surface by one question — does it run the effectful filter or only read the `ast`: decode/encode surfaces typed `<A, I>(Schema<A, I, never>)` vanish (`decodeUnknownSync`, `decodeSync`, `encodeSync`), derivation surfaces typed `<A, I, R>` survive (`is`, `asserts`, `Pretty.make`, `Arbitrary.make`, `Schema.equivalence`). A service-consulting owner stays property-testable, comparable, and guardable — only synchronous admission is amputated. The cut even re-routes the *family*: `Schema.TemplateLiteral`/`TemplateLiteralParser` take `Schema.AnyNoContext | AST.LiteralValue`, so a part whose refinement consults a service is structurally rejected from the slot, and the route reads the slot's context constraint before committing the family.
- The identifier is a write-once `$defs` key with first-writer-wins-silent collision: generation guards `Record.has($defs, id)` per node, so two distinct owners sharing one identifier both `$ref` `$defs/<id>` while only the first's structure is recorded — one definition for two concepts, no diagnostic, read against the whole published namespace before any owner is written. It is mandatory on `Schema.Class<Self>(id)`/`Class.extend(id)`, optional on `TaggedClass`/`TaggedStruct`/`TaggedError` (defaulting to the tag), and resolves into two decoupled graphs whose correctness is a conjunction: the `$defs` graph keyed by `getJSONIdentifier` (a JSON-facing annotation overriding the plain identifier) and the parse-error path keyed by the plain `identifier` — the override lets two concepts publish under one wire contract deliberately while distinct plain identifiers keep paths attributable. An owner reused at three or more embed sites without an identifier is the JSON-Schema-explosion trigger: minting the key collapses inlined copies to one `$defs` definition and recovers legible attribution simultaneously — the missing-name cost the `as const`-table exemption pays in both graphs.

```typescript
const Unique = Schema.String.pipe(Schema.filterEffect((handle) => Registry.pipe(Effect.flatMap((r) => r.absent(handle))))) // FD threads to the field's R
class Account extends Schema.Class<Account>('Account')({ handle: Unique, seats: Schema.Int }) {} // R = Struct.Context<Fields> = Registry
type Needs = Schema.Schema.Context<typeof Account> // Registry, not never — the third coordinate, computed by union
const guard: (u: unknown) => u is Account = Schema.is(Account) // <A, I, R> — tolerates context: structural shape only
const admit: (u: unknown) => Effect.Effect<Account, ParseResult.ParseError, Needs> = Schema.decodeUnknown(Account) // the only admission path the lift leaves
class Receipt extends Schema.Class<Receipt>('Invoice')({ ref: Schema.String }) {} // reject: 'Invoice' already claimed — Receipt's shape silently aliased away
```

[EXEMPTION_REVOCATION_IS_ONE_PREDICATE]:
- Revocation from the generated owner is one predicate parameterized by tier, not four rules: an exemption survives iff every demanded facet is covered by the tier's structural surface, and the first demanded facet the tier lacks promotes the value one rung up. It is the inverse of demotion — demotion strips a dead capability, revocation grants a demanded one the floor tier has no slot to hold — and the destination is a pure function of (held tier, forfeiting axis), read off the signature before the floor tier is named.
- The `as const` table is decode-free, dependency-free, instance-free, and namespace-free simultaneously, so four independent axes revoke it to four rungs: equality-demand -> `Data.struct`/`Schema.Data` (a frozen literal read by string `keyof typeof` has no `[Equal.symbol]` to dedupe a `HashSet` by structure); codec-demand -> `Schema.Class` (or `Schema.Literal` if rows carry no behavior); dependency-edge revokes the *exemption* not the *form* (a row holding a `Schedule`/`Layer`/closure is an owned policy-value table read at the composition root); identifier-need -> any `ast`-bearing owner at the third embed site.
- The bare brand revokes on codec alone: `Brand.nominal`/`Brand.refined` return a `Brand.Constructor` with `.option`/`.either`/`.is` sufficient for in-memory admission but carry no `ast`. The promotion is in-owner via `Schema.fromBrand(constructor)`, reusing the constructor's predicate as the schema filter — a parallel `Schema.brand` re-spelling it is two declarations that drift; a second invariant fuses via `Brand.all(a, b, c)` (collecting every failure in one `.either` under `Brand.EnsureCommonBase`), never crossing to a schema.
- The native primitive revokes on two independent bits one rung apart: nominal-collision -> `Brand.nominal` (two same-base values must never be confused by meaning), refinement-invariant -> `Schema.filter` (returning `refine<C & B, From>` — narrows the static type to the subtype while the runtime stays the base). The fork is whether two same-base values both satisfying the invariant must still be kept apart: a positive integer that must never index is nominal; a merely non-empty string is refined, and a brand there mints an identity that distinguishes nothing.
- The kernel statement revokes by *re-expressing* the same logic in the spine the rail demands — no new owner — the moment its produced value can fail, decode, or branch on a tag: fallible accumulation -> `Effect.reduce`/`Effect.forEach`, bounded state fold -> `Effect.loop`/`Effect.iterate`, tag-branch -> `Match.tagsExhaustive`. The reject is a statement producing a `ParseError`-capable value plus a separate post-loop check the interior must remember — the un-railed value escaped before the check ran.

```typescript
type Handle = string & Brand.Brand<'Handle'>
const Handle = Brand.refined<Handle>(
    (s) => s.length > 0 && s.length <= 32,
    (s) => Brand.error(`<bad-handle:${s}>`),
) // exemption holds: in-memory admission, no ast, no decode
const HandleSchema = Schema.String.pipe(Schema.fromBrand(Handle)) // codec-demand: lift the SAME constructor, never a parallel filter
const admitAll = (raws: ReadonlyArray<unknown>): Effect.Effect<ReadonlyArray<Handle>, ParseResult.ParseError> =>
    Effect.forEach(raws, Schema.decodeUnknown(HandleSchema)) // the rail threads the failure a `for` loop could not carry
```

[CONSTRAINT_AXIS_IS_ORTHOGONAL]:
- A constraint (a narrowing invariant, a representation remap) is a sixth routing question orthogonal to the shape tuple: the shape axes choose the owner family, the constraint axis chooses which combinator attaches the invariant *inside* it, and the constraint never earns its own owner. The fork reads two bits: nominal (brand) vs property-proof (refine), and pure (`Schema.filter`, `R = never`) vs service-consulting (`Schema.filterEffect`, lifting `R`). `filterEffect` is the only refinement combinator that lifts context — the type-level proof of which kind was attached — its `FD` threading into `R`, provided once at the composition root.
- A representation *change* (not narrowing) routes to `transform`/`transformOrFail`, never `filter`: `filter` keeps `Encoded === Type`; `transform` declares distinct `From`/`To`. The fallible-vs-total fork is `transformOrFail` (`Effect<To, ParseIssue, R>`) vs `transform` (total bijection). A closed wire<->domain literal correspondence routes to `Schema.transformLiterals(...pairs)` deriving both directions from the table; the moment the decode must *compute*, the route is `transformOrFail`. A tag-free wire shape the interior must route routes through `Schema.attachPropertySignature("_tag", literal)`, injecting at decode and stripping at encode — never demanding the producer stamp a tag it has no field for.
- The constraint attaches at exactly one altitude: a field invariant on the field schema, a cross-field invariant a whole-record `Schema.filter` *piped over the class* returning a `FilterIssue` whose `path` attributes the failure, a service-consulting cross-field `filterEffect` piped over it. Stacking is left-to-right and order is semantic — `String.pipe(minLength(1), pattern(re), brand("X"))` mints the brand last on the proven base; a brand before a filter brands an unrefined value its type no longer reflects. The annotation (`equivalence`, `arbitrary`) attaches *after* the chain because it derives from the refined `ast`.
- Every degenerate combinator demotes to the next-lower the signature demands: a `transform` with identity decode/encode is a `filter` wearing two function slots; a constantly-true `filter` is dead surface; a `transformLiterals` of one pair is a `transformLiteral`; an `attachPropertySignature` on a one-member union is a `tag` field.

```typescript
const Slug = Schema.String.pipe(Schema.minLength(1), Schema.pattern(/^[a-z]+(?:-[a-z]+)*$/), Schema.brand('Slug')) // brand last, on the proven base
const Tier = Schema.transformLiterals([0, 'free'], [1, 'pro'], [2, 'team']) // both directions derive from one pair table
class Account extends Schema.Class<Account>('Account')({ slug: Slug, tier: Tier, seats: Schema.Number, active: Schema.Number }) {}
const Seated = Account.pipe(
    Schema.filter((a) => (a.active <= a.seats ? undefined : { path: ['active'], message: '<over-seat-limit>' })), // cross-field at class altitude; FilterIssue.path attributes the failure
)
```

[VARIANT_ALPHABET_SIZING_THRESHOLD]:
- The `C ≥ 3` promotion is a field-arity flip, not a channel count: a field at one or two channels is one `PropertySignature`, at three the field routes out of the in-owner schema to the `Model.Class` variant owner. The number is derived — `Schema.optionalWith({ as: "Option" })` already carries two representations (`Type` = `Option<A>`, `Encoded` = `A | null | undefined`) in one decode/encode bracket; the third distinct representation is the first no single `PropertySignature` covers. The discriminant is per-call (present-or-absent per construction, any channel count) vs per-channel (admitted *shape* differs); the two never sum — five channels of one shape is `C = 1`.
- Crossing the threshold is one diff: the field changes from an in-owner `optionalWith(...)` to a per-channel field on the `Model.Class` variant owner, and nothing else moves. Promotion is mechanical because the sub-threshold modifier embeds verbatim as one channel's value, so the route reads the channel count and the modifier survives the move.
- The alphabet is sized at the *second* divergent channel, read pessimistically: promoting a flat field to a two-element alphabet breaks no consumer, whereas leaving it flat until the third channel forces the cross-owner break when call sites exist. A uniform field stays a plain schema *inside* the variant owner (no per-variant config), so the anticipatory cost is one unused-but-uniform owner against re-spelling every projection when the third channel breaks the flat field.

```typescript
class Entity extends Model.Class<Entity>('Entity')({
    slug: Schema.String.pipe(Schema.minLength(1), Schema.brand('Slug')), // C = 1: a plain schema, no per-variant config
    seats: Schema.optionalWith(Schema.Int, { as: 'Option' }), // C = 2: the sub-threshold field, in-owner
    note: Model.FieldOption(Schema.String), // C >= 3: routed to the variant owner's per-channel field
}) {}
```

[CHOOSER_TOTALITY_IS_A_CARRIED_RESIDUAL]:
- The chooser collapses to one `Match.type<Signature>()` value the moment a third concept routes, and totality is a type *carried through every arm* — the residual `R` slot of `Matcher<I, F, R, A, Pr, Ret>` — not a terminal check. Each combinator subtracts handled cases by `ApplyFilters<I, AddWithout<F, handled>>`, so `R` shrinks arm by arm and `exhaustive` is typed `(self: Matcher<I, F, never, ...>)`, applying only at the fixed point. A widened signature breaks at the *terminal* naming the exact unrouted regime, the residual being the literal `Exclude<I, handled>` the new member inhabits. The invariant is monotone: subtracting only shrinks `R`, so the chooser is total under arm reordering and breaks only under signature widening or arm deletion — one growth axis.
- Two gates enforce it on disjoint failure shapes: the *subtractive* gate (`R extends never` on `exhaustive`) catches an under-handled union; the *additive* gate (the mapped type `{ [Tag in Types.Tags<D, R> & string]: (...) => Ret }` on `discriminatorsExhaustive`/`tagsExhaustive`) makes every live discriminant a required key, and its conjunct `{ [Tag in Exclude<keyof P, Types.Tags<D, R>>]: never }` resolves a *stale* arm (renamed/removed discriminant) to `never`. The record form fuses both and needs no terminal; `Match.tag`/`Match.when` arms drive the subtractive gate alone and *require* the `exhaustive` terminal — omitting it compiles a partial function whose residual is silently non-`never`.
- The one move defeating both gates silently is the fallback tail: `orElse` is typed `<RA, Ret, F>(f) => ...` with *no* `extends never` constraint, so it absorbs whatever a widening left unrouted and routes the new regime to the default with no diagnostic. `Match.discriminators(D)` (no `Exhaustive`) is the same breach: its case-record keys are `?`-optional, admitting a gap. A total chooser ends in `exhaustive` or a `*Exhaustive` record, never a catch-all — auditing corpus totality is reading the one shared chooser's terminal, and a second chooser owner splits the invariant into two fixed points that drift exactly as a parallel schema splits single-source.
- The `Pr` slot is the composition contract for nesting: `Match.type<I>()` builds `Pr = never` (terminal yields a reusable `(s) => Tier` function), `Match.value(i)` builds `Pr = I` (terminal yields the resolved tier). A structural arm receives a value narrowed to `Extract<Signature, {_tag: "Structural"}>` and re-runs the chooser over the residual `(cardinality, wire)` axes via `Match.value`, whose `orElse` resolves the tier directly; a sub-chooser mis-built with `Match.type` fails at the arm because its terminal hands back a function where a tier was demanded — `[Pr] extends [never]` makes resolved-value-vs-function a checked invariant. The recursion's floor: a one-inhabitant residual is resolved directly, and forcing a `Match.value` there is the degenerate sub-dispatch the demotion direction strips.
- `Match.withReturnType<Tier>()` is totality on the codomain — the first pipe step pinning `Ret`, typed `[Ret] extends [...] ? Matcher<...Ret> : "withReturnType constraint does not extend Result type"` — so an arm drifting in return shape fails at the arm, not downstream, and the pin flows into every nested terminal. The reject is the unpinned chooser inferring `Ret` as the union of whatever the arms returned — total by accident, admitting a wrong-shape arm as a silent codomain widening.

```typescript
type Signature =
    | { readonly _tag: 'Nominal'; readonly wire: boolean }
    | { readonly _tag: 'Reference' }
    | { readonly _tag: 'Structural'; readonly cardinality: 'one' | 'many'; readonly wire: boolean }
const choose: (s: Signature) => string = Match.type<Signature>().pipe(
    Match.withReturnType<string>(), // codomain pinned first: an arm drifting in return shape fails at the arm
    Match.discriminatorsExhaustive('_tag')({ // additive + subtractive gates fused; a fourth member becomes a required key
        Nominal: (s) => (s.wire ? 'Schema.brand' : 'Brand.refined'),
        Reference: () => 'as-const-table', // one-inhabitant residual: resolved directly, no sub-chooser
        Structural: (s) =>
            Match.value(s).pipe( // Pr = narrowed member, so orElse resolves the tier, not a (s) => Tier thunk
                Match.whenAnd({ cardinality: 'many' }, { wire: true }, () => 'Schema.Union<TaggedClass>'),
                Match.whenAnd({ cardinality: 'many' }, { wire: false }, () => 'Data.taggedEnum'),
                Match.orElse(() => 'Schema.Class'),
            ),
    }),
)
```
