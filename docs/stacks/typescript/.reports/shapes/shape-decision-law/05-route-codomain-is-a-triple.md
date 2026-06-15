# Route Codomain Is A Triple

[CODOMAIN_IS_A_PRODUCT_NOT_A_FAMILY]:
- The chooser's output type is not the owner family alone — it is the product `(family, identifier, R)`, and a route is a total function only when all three coordinates project to a defined value off the one signature read. The family is the structural answer the five shape axes compute; the identifier is the corpus-namespace key the owner mints; the `R` is the requirement set the owner's decode threads. A route that resolves the family and leaves either of the other two coordinates implicit is a partial function masquerading as total — it type-checks at the owner declaration and fails at a consumption surface the family alone never touched.
- The three coordinates are mutually orthogonal and each fails on a disjoint surface, so the route's correctness is the conjunction of three independent predicates, never one: the family is wrong when the owner cannot carry a demanded structural facet (caught by the collapse comparator), the identifier is wrong when two owners collide in `$defs` (caught only by reading the whole namespace), and the `R` is wrong when a non-`never` context reaches a synchronous extraction site (caught only at that site). The five shape axes are the projection onto the first coordinate; the other two are the routability coordinates that complete the codomain, and a chooser that returns only the family has computed one third of its own output.
- The completeness obligation is checkable at declaration time because each coordinate is recoverable from the owner's type with no second declaration: the family is the owner's runtime construct, the identifier is read by `getIdentifierAnnotation` off the owner's `ast`, and the `R` is `Schema.Schema.Context<typeof Owner>` — the third `Variance` slot. The route is discharged iff all three projections are read against the corpus before any consumer binds, and skipping the second or third read is the silent misroute the family-only chooser cannot witness.

```typescript
interface Route<A, I, R> {
    readonly schema: Schema.Schema<A, I, R> // first coordinate: the owner family carries A, the structural answer
    readonly identifier: Option.Option<string> // second coordinate: the $defs key, read off the ast, not minted twice
    readonly _context: (r: R) => void // third coordinate phantom: R is type-level only, recovered via the Variance slot
}
const project = <A, I, R>(owner: Schema.Schema<A, I, R>): Route<A, I, R> => ({
    schema: owner,
    identifier: AST.getIdentifierAnnotation(owner.ast), // the only runtime read; family and context are type projections
    _context: (_r: R) => undefined,
})
```

[THIRD_COORDINATE_IS_COMPUTED_BY_FIELD_UNION]:
- The `R` coordinate is not chosen alongside the family — it is *derived* from the field map by structural union, so the owner does not decide its own context, its fields do: `Struct.Context<F> = Schema.Context<F[keyof F]>` unions every field's context, and a `Schema.Class`'s `R` is `Struct.Context<Fields>`. One service-consulting field among twenty pure ones lifts the whole owner's `R` off `never`, and the lift is invisible at every other field — the context coordinate of the route is a join over the field demands the chooser computes by reduction, never by inspecting the owner family. A family chosen correctly with a single `filterEffect` field embedded is a route whose third coordinate flipped without the family coordinate moving.
- The union is monotone and the monotonicity is the absorbed-growth guarantee on the context coordinate: adding a pure field unions `never` into the context and leaves `R` unchanged, while adding one service-consulting field unions its service into `R` and lifts it for the whole owner. `Class.extend<Next>(id)(fields)` carries this forward — its result context is `R | Struct.Context<NewFields>` — so a lineage member that introduces a service-consulting field lifts the child's `R` without touching the parent's, and the context of the route is recomputed per lineage member, never inherited as a constant.
- The field-union mechanism makes the context coordinate impossible to read off the owner family in isolation, which is exactly why it is a separate coordinate: two `Schema.Class` owners with identical structural signatures — same family, same identifier discipline, same five axes — carry different `R` iff one field's refinement consults a service, and the routing decision (which surfaces accept the owner) diverges entirely on a coordinate the shape axes are blind to. The reject is treating `R = never` as the default and discovering the lift at a consumption site; the route reads the field-union context at declaration and selects the consumption surfaces against it.

```typescript
const Unique = Schema.String.pipe(
    Schema.filterEffect((handle) => Registry.pipe(Effect.flatMap((r) => r.absent(handle)))), // FD threads to the field's R
)
class Account extends Schema.Class<Account>('Account')({
    handle: Unique, // one service-consulting field
    seats: Schema.Int, // pure
    label: Schema.NonEmptyTrimmedString, // pure
}) {} // R = Struct.Context<Fields> = Registry — the union over fields, not a property of the family
type Needs = Schema.Schema.Context<typeof Account> // Registry, not never; the third coordinate, computed by union
```

[NON_NEVER_CONTEXT_PARTITIONS_THE_WHOLE_SURFACE]:
- The non-`never` context cut is not the narrow "synchronous decode is forbidden" fact — it is one structural principle that partitions *every* surface consuming the owner: a surface whose work is the decode/encode boundary is typed `<A, I>(schema: Schema<A, I, never>)` and is unavailable the moment `R` lifts, while a surface whose work is type-side derivation off the `ast` is typed `<A, I, R>` and tolerates any context. The principle predicts the membership of both halves from one question — does the surface run the effectful filter, or only read the node — so the cut generalizes past the known sync-decode trio to the entire derivation surface: `Pretty.make`, `Arbitrary.make`, and `Schema.equivalence` are all `<A, I, R>` and survive the lift exactly as `validateSync`/`is`/`asserts` do, because none of them executes the decode.
- The generalization is the load-bearing advance: a service-consulting owner is still property-testable, still pretty-printable, still structurally comparable, and still type-guardable — only its synchronous admission is gone, because the annotation-derived facets ride the `ast` the context never touches. The reject is concluding a `filterEffect` owner forfeits its annotation facets along with its sync decode; the context coordinate gates admission alone, and the facet derivation is orthogonal to it, so the same owner routes to the full derivation half of the surface and only the admission half is amputated.
- The context coordinate's flip is the loud-break inventory on exactly the forbidden half: lifting an owner's `R` off `never` after consumers bound breaks every `decodeUnknownSync`/`decodeSync`/`encodeSync` site at compile time and leaves every `is`/`equivalence`/`Arbitrary.make` site intact, so the broken set is the precise inventory of synchronous-admission sites and the surviving set is the proof those consumers only ever derived or guarded. The asymmetric gate is the type-level proof of which kind of refinement was attached: `R` non-`never` is the readable witness that decode now requires a service, and the surfaces split along it without a hand-tracked list.

```typescript
const sync: (u: unknown) => Account = Schema.decodeUnknownSync(Account) // typed <A, I, never> — REJECTED when R = Registry
const guard: (u: unknown) => u is Account = Schema.is(Account) // typed <A, I, R> — tolerates context: structural shape only
const gen: FastCheck.Arbitrary<Account> = Arbitrary.make(Account) // <A, I, R> — derives from ast, ignores the effectful filter
const eqv: Equivalence.Equivalence<Account> = Schema.equivalence(Account) // <A, I, R> — comparator rides the ast, context-blind
const admit: (u: unknown) => Effect.Effect<Account, ParseResult.ParseError, Schema.Schema.Context<typeof Account>> =
    Schema.decodeUnknown(Account) // the only admission path the lifted context leaves standing — R surfaces in the signature
```

[CONTEXT_FORBIDS_THE_PARAMETER_SLOT_NOT_JUST_THE_DECODE]:
- The context coordinate gates *structural embedding positions*, not only the synchronous decode call, and this is the deeper reach of the partition: `Schema.TemplateLiteral` and `Schema.TemplateLiteralParser` take parameters typed `Schema.AnyNoContext | AST.LiteralValue` (`AnyNoContext = Schema<any, any, never>`), so a vocabulary part whose refinement consults a service cannot occupy a template-parameter slot at all — the owner is structurally rejected from the position, not merely from a call. A grammar whose part needs a service lookup re-routes the entire owner out of the template family into a `filterEffect` over the assembled string, because the slot itself forbids the third coordinate.
- The slot-level forbiddance means the context coordinate can re-route the *family* coordinate: a concept that would route to `TemplateLiteralParser` on its five shape axes but whose part carries a service-consulting refinement cannot be that family, because the family's parameter slot is `AnyNoContext`. The context coordinate is therefore not purely downstream of the family — when an embedding position constrains `R = never`, a non-`never` third coordinate invalidates the family the shape axes chose, and the route is recomputed with the family answer changed by the context constraint. The reject is forcing the part into the template slot and discovering the `AnyNoContext` rejection as an opaque type error; the route reads the slot's context constraint before committing the family.
- The same slot constraint recurs at every position that publishes a synchronous or wire-only contract: a position demanding `AnyNoContext` is a position whose work must be context-free, and an owner whose field-union context is non-`never` is excluded from it structurally. The context coordinate is read against the *embedding position's* constraint, not only the owner's own decode, so the route's third projection is a pair — the owner's computed `R` and the position's required `R` — and the route is admissible iff the computed context is a subtype of the position's tolerance.

```typescript
const Region = Schema.Literal('us', 'eu', 'ap') // pure: R = never, admissible as a template parameter
const Tenant = Schema.TemplateLiteralParser(Region, ':', Schema.NumberFromString) // parts are AnyNoContext | LiteralValue
// reject: a service-consulting part cannot occupy the slot — AnyNoContext forbids R != never structurally:
const Verified = Schema.String.pipe(Schema.filterEffect((s) => Registry.pipe(Effect.as(s)))) // R = Registry
// Schema.TemplateLiteralParser(Region, ':', Verified) — rejected at the slot; the family re-routes to filterEffect over the whole string
const decoded: (u: unknown) => Effect.Effect<readonly ['us' | 'eu' | 'ap', string, number], ParseResult.ParseError> =
    Schema.decodeUnknown(Tenant) // the parser's decode splits into the typed tuple; R = never keeps it sync-eligible too
```

[IDENTIFIER_IS_A_WRITE_ONCE_NAMESPACE_KEY]:
- The identifier coordinate is a write-once key in a global `$defs` record, and the collision semantics are first-writer-wins-silent, not last-writer-overwrite: JSON Schema generation walks the owner graph and, at each identified node, guards `Record.has($defs, id)` before writing — the first owner to claim a key writes its shape and every subsequent owner sharing that key skips the write and aliases to the already-present definition. Two distinct concepts sharing one identifier therefore both `$ref` `$defs/<id>` while only the first's structure is recorded; the second's shape is silently dropped from the published contract with no diagnostic, and a single definition describes two unrelated concepts.
- The collision is invisible to the family coordinate because identifier identity, not structural identity, individuates an owner in the schema graph: two `Schema.Class` owners with identical fields but distinct identifiers are distinct nodes with distinct `$defs` keys, and two with distinct fields but an identical identifier collapse to one key. The five shape axes compute the structure; they cannot see the namespace, so a structurally-correct owner with a duplicated identifier is a misroute the shape axes are constitutionally blind to. Uniqueness is read against the whole `$defs` namespace the corpus publishes, never the local module, and the third reference to an unidentified widely-embedded owner is the trigger that mints the key and collapses inlined copies to one definition with N references.
- The identifier coordinate's `make`-vs-optional split is part of the route, not a downstream convenience: `Schema.Class<Self>(identifier)` and `Class.extend<Next>(identifier)` demand the key (`identifier: string`), while `Schema.TaggedClass<Self>(identifier?)`, `Schema.TaggedStruct`, and `Schema.TaggedError<Self>(identifier?)` default it to the tag literal — so a tagged owner's identifier coordinate is resolved by its discriminant and a structural class must mint a distinct name. Supplying an explicit identifier to a tagged owner is the route only when two concepts share a `_tag` across bounded contexts and must stay namespace-distinct; the explicit name is the namespace-collision escape, and choosing it is the second coordinate's deliberate resolution.

```typescript
class Invoice extends Schema.Class<Invoice>('Invoice')({ total: Schema.BigDecimal }) {} // mints $defs/Invoice
class Receipt extends Schema.Class<Receipt>('Invoice')({ ref: Schema.String }) {} // reject: same key — first-writer-wins
const schemaA = JSONSchema.make(Invoice) // $defs/Invoice describes { total }
const schemaB = JSONSchema.make(Receipt) // $defs/Invoice still describes { total } — Receipt's shape silently aliased away
class Drawn extends Schema.TaggedClass<Drawn>()('Drawn', { at: Schema.Number }) {} // identifier defaults to the tag "Drawn"
```

[IDENTIFIER_RESOLVES_INTO_TWO_DECOUPLED_GRAPHS]:
- The identifier coordinate resolves into two graphs that consume it independently, so its correctness is a conjunction over both, not one namespace check: the `$defs` graph keyed by `getJSONIdentifier` (the JSON-facing annotation overriding the plain `identifier`), and the parse-error path graph keyed by the plain `identifier` alone. The two decouple precisely because the override exists — an owner can set distinct `$defs` keys over a shared domain `identifier`, or one shared `$defs` key over distinct domain names — so the collision calculus runs per graph: a `$defs` collision corrupts the published shape, a parse-error path collision merely produces an ambiguous issue label, and the two failures are surfaced on different consumers with no shared diagnostic.
- The decoupling is the mechanism that lets two genuinely distinct domain concepts publish under one wire contract on purpose: setting the same `JSONIdentifierAnnotation` collides them in `$defs` deliberately (one published shape, two domain owners) while their distinct plain `identifier`s keep their parse-error paths attributable. The identifier coordinate's resolution is therefore which of the two slots carries the namespace key for which graph, and reading only the plain `identifier` misses the wire-facing override that the `$defs` graph actually keys on — the route's second coordinate is under-read when only one of the two graphs is checked.
- An owner unnamed in *either* graph is a route with its second coordinate unresolved, and the two failure surfaces are asymmetric: an owner with no `identifier` inlines its full shape at every `$defs` use site (a widely-embedded owner explodes the schema instead of referencing one definition), and the same owner produces an anonymous structural parse-error path no concept name can attribute. The third-reference trigger that mints the key collapses the inlined copies in the first graph and recovers legible attribution in the second simultaneously — one identifier resolves both coordinates of the pair, and the missing-name cost is paid twice.

```typescript
class Customer extends Schema.Class<Customer>('Customer')(
    { id: Schema.UUID, name: Schema.NonEmptyTrimmedString },
    { identifier: 'Customer', jsonSchema: { $id: 'Party' } }, // domain term Customer; raw $id merges the published name Party
) {}
const wireKey = AST.getJSONIdentifier(Customer.ast) // reads JSONIdentifierAnnotation, falls back here to identifier: Some('Customer')
const domainName = AST.getIdentifierAnnotation(Customer.ast) // Some('Customer') — the parse-error path label, the plain key
```

[THREE_COORDINATES_THREE_FLIP_FAILURE_MODES]:
- Each coordinate has a distinct flip-failure mode, and the three modes complete the routability-misroute taxonomy the five shape axes leave open: the family coordinate flips loud (the owner's `Type`/`Encoded`/constructor changes and the checker chases every consumer), the context coordinate flips loud-on-one-half (the synchronous-admission sites break, the derivation sites survive), and the identifier coordinate flips silent (a colliding key produces no type error and a wrong published shape). The silent mode is the identifier's alone, mirroring the identity-regime silence on the family axis — and exactly as identity regime is the most conservatively-read shape axis, the identifier is the coordinate read against the whole corpus before any owner is written.
- The three modes are non-substitutable diagnostics: the family's loud break is driven by the constructor differential, the context's half-loud break is driven by the `Schema<A, I, never>` constraint on the sync surfaces, and the identifier's silence has no compile-time witness because `Record.has` resolves the collision at generation time with the first definition standing. A route whose family and context are correct but whose identifier collides type-checks end-to-end and corrupts only the published contract, so the third coordinate's verification is necessarily a corpus-wide namespace read, not a per-owner type-check — the only one of the three that a local checker cannot discharge.
- The completeness law closes the codomain: a route is total iff the family covers the demanded facets, the context matches every embedding position's tolerance, and the identifier is unique across the published namespace and named in the parse-error graph — three predicates over three coordinates, conjoined. The reject is the family-only chooser that returns a correct owner and declares the route done; the route is done only when the triple resolves, and a coordinate left implicit is the partial function whose undefined point surfaces as a sync-surface rejection or a silent `$defs` aliasing the family answer never predicted.

```typescript
const isTotal = <A, I, R>(r: Route<A, I, R>, namespace: HashSet.HashSet<string>): boolean =>
    Option.match(r.identifier, {
        onNone: () => false, // unnamed owner: $defs explodes, parse-error path is anonymous — second coordinate unresolved
        onSome: (id) => !HashSet.has(namespace, id), // unique across the published namespace, or the silent collision fires
    }) // family coverage and context tolerance are the other two conjuncts, each checked at its own surface
```
