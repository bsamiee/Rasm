# Persistence Wire Owners Distilled: One Model.Class Fans Six Variant Codecs Off A Config-Row Field Map

[OWNER_IS_SCHEMA_AND_STRUCT_AT_ONCE]:
- `class Self extends Model.Class<Self>("Self")({ ...fields })` is one statement that is the value-constructor, the static `Type`, the `select` codec, structural `Equal`/`Hash`, AND the carrier of five sibling variant codecs — no `type Self = typeof Self`, no companion `const`, no separate `new`-target follows.
- The owner interface extends `Schema.Schema<Self, Simplify<I>, R>` AND `VariantSchema.Struct<Fields>` simultaneously: the schema half drives `Schema.decodeUnknown(Self)`, the struct half (`Self[VariantSchema.TypeId]`) feeds `Model.fields`, `Model.extract`, and the `.insert`/`.update`/`.json`/`.jsonCreate`/`.jsonUpdate` properties. The class instance IS the `select` variant — `Self["Type"]` is the prototyped read row, `Self["I"]` is `Schema.Struct.Encoded<ExtractFields<"select", Fields, true>>`.
- `readonly ast: AST.Transformation`, never a `Struct`: decode is the build-the-instance transform (read-struct to class instance), encode the inverse, so the owner composes into `Schema.compose`/`Schema.parseJson` as a transformation leaf. Accept: narrow via `Self.fields` or a write variant's `Self.insert.pick(...)`. Reject: `Self.pick`/`Self.omit` — they do not exist on a transformation.
- Boundary: `Self.annotations(...)` returns a PLAIN `Schema.SchemaClass<Self, I, R>` and forfeits `.insert`/`.json`/`.fields`; carry identifier/message/`equivalence`/`arbitrary`/`pretty` as the second `Model.Class` argument (typed `Schema.Annotations.Schema<Self, []>`, keyed to the DECODED `Self`) so the variant surface survives — an owner-level annotation describes the read instance, never the wire body.
- Trap: omitting the `Self` generic collapses the return type to the string literal `` "Missing `Self` generic ..." `` — a construction error at the declaration, not a downstream miss.

[MARKER_IS_A_SATURATED_CONFIG_ROW]:
- Every named marker is one interface `extends VariantSchema.Field<{ ...config }>`, and `Field<A>` is exactly `{ readonly [FieldTypeId]: FieldTypeId; readonly schemas: A }` — no method, no behavior, no second field. The entire difference between `Generated`, `Sensitive`, and `JsonFromString` is the object literal; a marker is a config row, not a subtype with overrides.
- `Field.Config` is `{ readonly [key: string]: Schema.Schema.All | Schema.PropertySignature.All | undefined }` — a partial slot map keyed by variant name. The `PropertySignature.All` admission is load-bearing: it lets ONE row hold three schema KINDS across slots — a plain leaf, a `Schema.tag` discriminant, a `VariantSchema.Overrideable` — so the per-variant difference can be a difference in signature kind, not only encoded leaf. `UuidV4Insert`'s row holds a branded `Uint8ArrayFromSelf` leaf in `select`/`update`/`json` and an `Overrideable` in `insert`.
- The named markers are PRESET rows over `FieldOnly`/`FieldExcept`, byte-identically replaceable: `Model.Generated(s)` ≡ `FieldExcept("insert", "jsonCreate", "jsonUpdate")(s)`, `Model.Sensitive(s)` ≡ `FieldOnly("select", "insert", "update")(s)`, `Model.GeneratedByApp(s)` ≡ `FieldExcept("jsonCreate", "jsonUpdate")(s)`. `FieldOnly`/`FieldExcept` fill every present slot with ONE schema (membership is a key set, the slot schema constant); a per-slot-DIFFERENT row is unreachable from them and routes through `Model.Field({...})`.
- A bare `Schema.X` field is the implicit all-six uniform row (`Field({ select: s, ..., jsonUpdate: s })`), so a marker is never an ADDITION to a base column — it is the narrowing of the universal row to a key subset or a per-slot schema rewrite. A column has exactly one row; stacking two markers is one rewrite, never a wrapper chain.
- The open `Field({...})` form makes the named markers a non-exhaustive convenience layer over a complete config algebra: a wire-only-store-redacted or insert-once-json-mutable modality lands as one `Field` whose `keyof Config` is the exact slot set, never a new marker constructor. The whole six-variant toolkit is one `VariantSchema.make({ variants, defaultVariant })`; a different alphabet (`"draft" | "live"`) yields a parallel config-row table with identical mechanics.

[COMPOSE_BY_PER_SLOT_REWRITE_NEVER_BY_NESTING]:
- Markers compose by per-slot REWRITE of a flat row, never a `Field<Field<...>>`: `fieldEvolve({ <variant>: s => ... })` over `Field<S>` types `Field<{ [K in keyof S]: K extends keyof Mapping ? ReturnType<Mapping[K]> : S[K] }>` — named slots replaced by the mapper's return, unmapped slots passed through, same `keyof` membership.
- `fieldEvolve` has two operand-selected branches: over a bare schema (the universal row) the mapping is keyed by ALL variants and PROMOTES it to an explicit six-slot row; over an existing `Field<S>` the mapping is `{ [K in keyof S]?: ... }`, keyed by EXISTING membership, so it CANNOT add a slot the row lacks — evolving a `jsonCreate` slot on a `Generated` row is a type error. Reject conflating the operations: membership change routes through `Field`/`FieldOnly`/`FieldExcept`, representation change of present slots through `fieldEvolve`.
- `Model.FieldOption` over a `Field<infer S>` REWRITES every present slot in place — `{ [K in keyof S]: K extends VariantsDatabase ? OptionFromNullOr<S[K]> : optionalWith<S[K], { as: "Option"; nullable: true }> }` — so optionality layers onto a `Generated`/`JsonFromString` row by per-slot remap, NEVER by nesting. Boundary: the rewrite is `[K in keyof S]`, not `[K in AllVariants]`, so a `Sensitive` row's already-missing json slot stays missing (`never` in, `never` out); a custom slot-rewrapping marker reuses `VariantsDatabase = "select" | "insert" | "update"` rather than re-enumerating six keys.

[SLOT_MEMBERSHIP_IS_THE_KEY_CLAUSE_NOT_THE_VALUE_CLAUSE]:
- The projection is one mapped type whose membership rides the `as` key-remap while representation rides the value clause: `{ [K in keyof Fields as [Fields[K]] extends [Field<infer Config>] ? V extends keyof Config ? K : never : K]: [Config[V]] extends [Schema.All | PropertySignature.All] ? Config[V] : never }`. A `never` key is ABSENT from the result — not `K?: undefined`, not `K: never` as a value — so `"id" extends keyof Schema.Schema.Type<typeof Self.insert>` resolves `false` and reading it is a property-does-not-exist error. Membership erasure and representation narrowing produce categorically different failure surfaces.
- Only a `Field` marker can delete a key; a bare `Schema.X` and a nested `Struct` pass the key clause unconditionally (`: K`) and are structurally non-droppable across all six. "Insert drops Generated", "json drops Sensitive", "update names the id" are three evaluations of ONE predicate `V extends keyof Config` over three key sets — the behavioral vocabulary is shorthand for set membership, a marker has no behavior beyond which keys its `schemas` owns.
- The value-level twin is one guard in the single projection loop: iterating `Object.keys(self[TypeId])`, a `Field` column writes `fields[key] = value.schemas[variant]` ONLY under `if (variant in value.schemas)` — `variant in value.schemas` is a plain key-existence check, NOT a sentinel, `null` slot, or flag. There is no third state between member and non-member; the type's `never` key and the runtime's never-written key are one erasure in two languages.
- One absence is FOUR type-level erasures plus N runtime ones at once: `Schema.Schema.Type`, `Encoded`, `Struct.Constructor`, and `Struct.Context` each map `[K in keyof F]` over the same field record, and `Arbitrary.make`, `Pretty.make`, `Schema.equivalence`, `JSONSchema.make`, `Schema.is`/`asserts` each walk the projected struct's `propertySignatures` array. The boundary is the array, not a re-run — the marker is consulted ONCE at projection; what reaches every derivation is a struct whose column list is already pruned, and the only path back to a dropped column is a different variant's projection, never a transform of this one.
- Membership recurses one fragment level per `extract` call (a nested `Model.Struct`'s `Model.Generated` drops from the parent's `insert` sub-object), but the `select` projection is not privileged — it memoizes under `"__default"` while non-default variants memoize by name, repeated `Self.json` returning one cached `Schema.Struct` node.

[HASDEFAULT_IS_THE_ONE_COMBINATOR_MINTING_EVERY_ELIDABLE_KEY]:
- The discriminant slot and every generative slot are two call sites of ONE terminal pipe step, not two constructs sharing a flag: `tag` is `Literal(t).pipe(propertySignature, withConstructorDefault(() => t))`, the auto-stamp is `transformOrFail(from, Union(Undefined, to)).pipe(propertySignature, withConstructorDefault(constructorDefault ?? constUndefined))`. `withConstructorDefault` is typed `(self: PropertySignature<…, boolean, R>) => PropertySignature<…, true, R>` — it flips `HasDefault` to literal `true` leaving `R` UNTOUCHED, the sole author-reachable producer of the elision predicate. A bespoke elidable column is `Schema.propertySignature(leaf).pipe(Schema.withConstructorDefault(() => seed))`, zero new machinery.
- Three orthogonal tokens on one `PropertySignature` each drive a different projection: `TypeToken` (`"?:"` read by `OptionalTypePropertySignature`), `EncodedToken` (`"?:"` read by `EncodedOptionalKeys`), and `HasDefault` (`true` read by `PropertySignatureWithDefault`). Because `withConstructorDefault` keeps `TypeToken=":"` and `EncodedToken=":"`, an elidable slot is constructor-optional but ENCODED-MANDATORY — present and required on the wire, the default firing to fill it. Reject `Schema.optional(Schema.Literal(t))` for a tag: `optional` flips `TypeToken` to `"?:"` and drops the literal from the wire body; only `withConstructorDefault` keeps it present-on-encode.
- The predicate is FIXED, the slot set is NOT: `C` is `Struct.Constructor<ExtractFields<"select", Fields, true>>` (the read constructor) while each write `.make` carries `Struct.Constructor` over its own variant. The class constructor elides ONLY the tag (the lone defaulted `select` slot — `Generated`'s `id` and `DateTimeUpdate`'s `revisedAt` sit in `select` as bare/plain-transform mandatory leaves); `insert.make` additionally elides every `Overrideable`; `update.make` re-requires `id` (membership rejoins) yet still elides `revisedAt`. The SAME predicate over DIFFERENT variant slots yields the per-construct asymmetry, no per-variant rule authored.
- `RequiredKeys<T> = { [K in keyof T]-?: {} extends Pick<T, K> ? never : K }[keyof T]` and `make`'s signature `RequiredKeys<C> extends never ? void | Simplify<C> : Simplify<C>`: when EVERY constructor key is HasDefault-elided or `"?:"`-optional, the payload collapses to argument-free `.make()`; ONE bare-schema leaf flips the branch back to a mandatory payload. This toggles the whole `.make` arity with no flag, and adding one required column breaks the bare `.make()` at compile time.

[OVERRIDEABLE_IS_THE_ENCODE_ARM_OF_ONE_TRANSFORM]:
- `VariantSchema.Overrideable(from, to, { generate, decode?, constructorDefault? })` is `transformOrFail(from, Union(Undefined, to))` piped through `propertySignature` then `withConstructorDefault(constructorDefault ?? constUndefined)`, yielding `PropertySignature<":", (To & Brand<"Override">) | undefined, never, ":", From, true, R>`. `generate: (Option<ITo>) => Effect<From, ParseIssue, R>` is bound to the `encode` callback `dt => generate(dt === undefined ? Option.none() : Option.some(dt))` — the mint fires on the WRITE pass, computing the persisted `From` against the live world, never at construction or read.
- The `decode` arm collapses to `ParseResult.succeed(undefined)` unless a `decode: Schema<ITo, From>` is supplied — the structural reason the timestamp/uuid markers place the overrideable ONLY in `insert`/`update` and a plain schema in `select`/`json`: an overrideable in `select` would decode every read to `undefined`. The `Option.some`/`none` discriminant is computed by `dt === undefined` (the brand is erased before encode runs), `generate` sees only `Option<ITo>` — its OWN slot's forced-or-absent ENCODED value, no row context — so a cross-field mint cannot be an `Overrideable` and routes through a post-encode transform.
- `Override = value => value` is the runtime identity whose `& Brand<"Override">` is type-only: the HasDefault predicate makes the key optional, the brand on its type makes a non-default value rejected unless branded. A write `.make` has exactly three spellings — omit (default fires), pass `undefined` (default fires, needed only nested), pass `Model.Override(x)` (forced) — the only construct where optionality and value-shape are two independent gates on one slot. `constUndefined` is load-bearing: it threads `undefined` to the encode arm to fire `generate(none)`; a concrete `constructorDefault` makes `generate(some)` fire on EVERY write, converting the slot to "always forced."
- Forcing feeds the effect, never skips it: `generate(Option.some(x))` STILL RUNS on a forced row, so `onSome` validates/re-mints `x` and can `Effect.fail(new ParseResult.Forbidden(...))` — `Override(x)` makes the write feed a fallible mint, not infallible. The slot's `R` is `RFrom | R` (the `from` codec's context UNIONED with `generate`'s requirement, plus an optional `decode`'s) — a context-free `from` with a service-reading `generate` lifts only the write variants, while `Override` cannot discharge `R`: the requirement is on the codec, not the absence of a forced value.

[REQUIREMENT_IS_ONE_UNION_FOLD_EVALUATED_AT_OWNER_AND_PER_OP]:
- `Struct.Context<F>` is `Schema.Schema.Context<F[keyof F]>` — a SINGLE indexed access feeding ONE distributive conditional, structurally opposite the sibling folds: `Struct.Type` is `UnionToIntersection` over `[K in keyof F]`, `Struct.Constructor` likewise, but `Context` builds NO record. `F[keyof F]` collapses the field map to its VALUE union and `Schema.Context<S>` (`S extends Variance<_, _, R> ? R : never`) distributes `never | never | ServiceC` = `ServiceC` — no key enumeration, so an `R` cannot be lost to an intersection collapse the way an optional key vanishes from `Type`. The fold is signature-kind-blind: it reads the variance `R` off a leaf, a tag, or an `Overrideable` `PropertySignature` (position-seven `R`) identically.
- The owner-level `R` and the per-op `R` are TWO EVALUATIONS of this one union-fold over DISJOINT variant slot families: `Self["Context"]` folds the SELECT union ALONE (`Schema.Schema.Context<ExtractFields<"select", Fields, true>[keyof …]>`), so a field whose `select` slot is context-free but whose `insert` slot carries a service contributes `never` to it; `Self["insert"]["Context"]`, `Self["update"]["Context"]`, `Self["json"]["Context"]` are independent folds over their own slot families, never restrictions of one owner-level `R`.
- A contaminating leaf's footprint is the INTERSECTION of two owned facts — which variants the marker's config includes AND which slots hold the service-carrying schema. `Model.JsonFromString(inner)` and `Model.FieldOption(inner)` thread `Schema.Context<inner>` into ALL SIX slots (the text/option wrappers are requirement-transparent) — the WIDEST footprint; an `Overrideable` stamp confines its mint's `R` to the write variants alone — the NARROWEST. A `Model.Sensitive(serviceLeaf)` contaminates `select`/`insert`/`update` and is absent from every json union. The footprint is a property of WHERE the marker places the schema, never of the service.
- `Model.Union(A, B)`'s variant `R` is a UNION OF UNIONS — `A.insert["Context"] | B.insert["Context"]`, each itself a per-member slot-family fold — associative across the member axis: a contaminated `insert` slot in member `A` puts its service in `Node.insert["Context"]` while `B`'s context-free `insert` adds `never`. The `kind` discriminant closes each variant union's decode; the `Context` fold sums the requirement orthogonally, both running off one `Model.Union` call.
- Each repository method evaluates the same fold over the codecs it ACTUALLY runs: one `SqlSchema.{single,findOne,void}` call binds a variant codec to a SQL template, its requirement the union of the `Request` schema's encode-`R` and the `Result` schema's decode-`R` after `SqlClient` is discharged at the enclosing `Effect.gen`. `insert`/`update` bind `single(Model.insert|update, Model)` carrying `S["Context"] | S[variant]["Context"]`; `findById` binds `findOne(idSchema, Model)` carrying `S["Context"] | Schema.Context<S["fields"][Id]>`; `delete` binds `void(idSchema)` — NO `Result`, NO `AR` — so its requirement is `Schema.Context<S["fields"][Id]>` ALONE, the select-decode term STRUCTURALLY ABSENT. The `…Void` suffix is a DIFFERENT constructor (`void` vs `single`) whose missing `Result` deletes the decode term — `insertVoid`'s requirement diff from `insert` is exactly `S["Context"]`.
- The id-leaf term is the one requirement shared by `findById` and `delete` (the only methods whose `Request` is the id schema) and absent from every write method; move a service off the id leaf onto a sibling column and `delete` drops to `never` while `findById`/`update` retain it through `S["Context"]`. The per-op requirement is a theorem off variant membership — a `Model.Sensitive(serviceLeaf)` adds the service to the methods binding `select`/`insert`/`update` codecs that retain it and to NEITHER `delete` NOR the json surfaces, recomputed off the widened field map with no method respelled.
- The error channel is ALWAYS `never` because every op pipes `Effect.orDie`: `single` raises `SqlError | ParseError | Cause.NoSuchElementException` and `findOne`/`void` raise `SqlError | ParseError` pre-`orDie`, all collapsed to a defect. The SAME codec contributes its decode `R` (visible, providable) AND its `ParseError` (invisible, untyped) — a typed parse-failure rail is rebuilt by `Effect.catchAllDefect` over the `never`-failing method, never read off a typed `E`. Empty-row semantics diverge by constructor: `findOne` models absence as `Option.none()`, `single` raises `NoSuchElementException` that `orDie` makes a defect, so a `findById` miss is a value while an `update` miss is a defect.
- `idColumn` is constrained `(keyof S["Type"]) & (keyof S["update"]["Type"]) & (keyof S["fields"])` — a three-way intersection partitioning markers by lookup qualification with no flag: `Model.Generated`/`GeneratedByApp`/`Sensitive`/bare `Schema.X` keep `update` and qualify; a `FieldOnly("select","insert")` insert-once column is STRUCTURALLY barred (absent `update` resolves the intersection to `never`). The qualifier is `update`-membership, not json — a json-erased column still anchors lookup. In the dataloader path the same `idColumn` additionally partitions the request-coalescing window (`SqlResolver.findById` reads `request[idColumn]`, `void` batches `sql.in(idColumn, ids)`), so one constraint governs the lookup address, the per-op id requirement, and the batch key.

[ANYNOCONTEXT_IS_A_TYPE_LEVEL_BAN_CLOSED_ONLY_AT_THE_LEAF]:
- `Model.makeRepository<S extends Model.Any>` constrains variant properties to `Schema<any, any, unknown>` (admits any `R`, surfaces it per op); `Model.makeDataLoaders<S extends Model.AnyNoContext>` constrains them to `Schema<any, any, never>`. The ban is a structural subtype check at the call site, not a provision check: `Schema<any, any, Vault>` is assignable to the `unknown` top but NOT the `never` bottom, so one service-carrying codec fails the `AnyNoContext` bound long before any layer is wired.
- Context closure is a property of the FIELD declarations: the gate reads the variant properties' `R` slots — each a `Struct.Context` union over a disjoint slot family — so the only fix is a context-free field. A leaf built over a dependency stays `Schema<A, I, Service>` and propagates through the fold into every variant retaining it; closing it means folding the requirement to `never` AT the leaf's construction, never erasing it at the operation, because the operation consumes the variant `Context` the fold already produced and never re-folds the field map.
- The variant union is irreducible at every downstream lifter. `Schema.standardSchemaV1(Self.json)` typed `(schema: Schema<A, I, never>) => StandardSchemaV1<I, A>` is the sharpest gate — a json variant with one service-requiring leaf cannot be lifted until that leaf's `R` is `never`, no provision at the call repairing it. The structure-only family — `Schema.equivalence`, `Arbitrary.make`, `Pretty.make`, `Schema.is`/`asserts`, `JSONSchema.make` — types the variant as `Schema<A, I, R>` and never runs the decode effect, yielding off a non-`never` union; the union surfaces ONLY where an effect must run.

[FOUR_ORTHOGONAL_WIRE_AXES_MULTIPLY_ON_ONE_SLOT]:
- A column's wire behavior is the product of four axes, each in a different position of its `Field<Config>` row or `PropertySignature` tokens, none recoverable from another: MEMBERSHIP (`V extends keyof Config`), REPRESENTATION (which transform wraps the interior — `Schema.Redacted` vs bare), TEXT-VS-STRUCTURED (the encoded leaf `string` vs object — `Model.JsonFromString`'s db-vs-json slot), and ENCODED-KEY (the I-slot's third token — `Model.fieldFromKey`). They multiply: `JsonFromString(Redacted(inner)).pipe(fieldFromKey({...}))` narrowed by membership sets all four, so the distinct wire bytes one column projects is the product of the variants its membership spans and the representations its slots carry.
- `Schema.Schema.Encoded<S>` is the one-step variance read `S extends Variance<_, infer I, _> ? I`, but `I` is the fixpoint of `encodedAST_(ast, false)`, which on a `Transformation` UNCONDITIONALLY returns `encodedAST_(ast.from)` and recurses — the wire body is the encoded leaf at the BOTTOM of the transform stack. `JsonFromString(Schema.Redacted(inner))` peels BOTH transforms to `string`, never the intermediate `Redacted<Encoded<inner>>`; the decoded interior is INVARIANT (`Type<inner>` read off the same node from the opposite end), so the divergence is entirely on the I axis.
- `Schema.Redacted` (DECODED REPRESENTATION: interior `Redacted<A>`, encoded leaf bare `A`) and `Model.Sensitive` (VARIANT MEMBERSHIP: erased from json bodies) are independent and stack on one field — a `Model.Sensitive(Schema.Redacted(inner))` slot redacts the select/insert/update interior AND erases from every wire body. The axes do not nest: each is a per-slot rewrite of one flat row, stacking a fifth modality is one more rewrite, never a wrapper chain the consumer unwinds.
- `fieldFromKey` rewrites ONLY the encoded key: `Key<F, K>` reads the THIRD `PropertySignature` parameter and falls back to `K` only when it is `never`, and `fromKey.Rename<S, Key>` rebuilds the signature with every other token (TypeToken, EncodedToken, Encoded, HasDefault, R) preserved verbatim — renaming an `Overrideable` keeps its elision, a service-carrying slot its `R`. It dispatches per variant (`{ [K in keyof S]?: string }`), so the SQL row spells a column canonically and the JSON body foreign with the decoded key fixed at `K` across all six.
- Wire arity is a THIRD axis (`EncodedToken`, read by `EncodedOptionalKeys` for `"?:"`) disjoint from membership and HasDefault: `Model.FieldOption`'s db slot is `OptionFromNullOr<inner>` (a `":"` REQUIRED wire key admitting `null`) while its json-create slot is `optionalWith<inner, { as: "Option", nullable: true }>` (a `"?:"` OPTIONAL wire key admitting `null` AND absence) — one `Option<A>` interior, two wire arities. `keyof Encoded` distinguishes "the wire never carries this" (`Sensitive`, no key) from "the wire may omit this" (`FieldOption` create, optional key).
- Boundary: `Schema.encodedBoundSchema(Self.json)` keeps a `Refinement` only when `getTransformationFrom(ast.from) === undefined && hasStableFilter(ast)` — and `[StableFilterAnnotationId]: true` is set on `minItems`/`maxItems`/`itemsCount` ALONE. A `NonEmptyTrimmedString`'s `minLength(1)` carries no stable annotation, so the bound form DROPS its emptiness check — `encodedBoundSchema` is the stable-array-filter wire shape, not the bounded wire shape; a string emptiness invariant must be re-asserted on the decoded side. A refinement OVER a transform collapses even when stable-flagged (the first condition fails), so a bound above a `JsonFromString`/`Trim` is unconditionally dropped.
- The degenerate axis is `Schema.instanceOf(Ctor)` — `AnnotableDeclare<instanceOf<A>, A>`, a `Declaration` node with no `from` for `encodedAST_` to descend, so its encoded leaf EQUALS its decoded `A`: a column built from a bare `instanceOf` has no distinct wire body, its json variant re-emits the live host object, and `JSONSchema.make` over a variant containing it fails to produce a serializable property. The fix re-introduces a `Transformation` the recursion can peel — pair the host instance with a `Schema.transformOrFail` whose encoded `from` is the serializable form — so the wire body differs from the interior only when the slot's stack carries at least one transform.

[VARIANT_STRUCT_PROMOTES_OUT_OF_THE_ALGEBRA]:
- A value-class promotion takes a VARIANT STRUCT, never the owner: `class View extends Schema.Class<View>("View")(Self.json) {}` is well-typed because `Self.json` is a `Schema.Struct` satisfying `HasFields`, while `Schema.Class(Self)` fails — the owner is a `Schema.Schema` missing `pick`/`omit`/`records`. The promoted class is a fresh independent owner: it gains `extend`/`transformOrFail` chaining and body getters but carries NO variant properties — a getter-bearing wire model is one prototype over one variant's field set, a second prototype over a different variant a parallel promotion off the same owner.
- A body getter or method participates in NEITHER `Equal` NOR `Hash` — both read the declared field set, not the prototype — so two instances differing only in a computed `get` are `Equal`, the correct semantics for a row whose identity is its columns. Field equality is recursive through the DECODED representation: a `JsonFromString` column by parsed-object structure, a `FieldOption` by `Option` equivalence, a branded `Uint8Array` by byte content — dedup and `HashSet` membership over read rows are correct with no unwrapping, a branded id a legal `HashMap` key (the brand erased at the equality layer).
- `Schema.extend(Self.json, Schema.Struct({ etag: Schema.String }))` fuses a transport-only column onto ONE variant — both sides structs, non-overlapping fields — so an `etag` added to the wire body never touches `select`/`insert`. `Schema.equivalence(Self.json)` is a structurally DIFFERENT equivalence off the same owner (`secret` absent via `Sensitive`, `payload` compared by parsed object), and `Arbitrary.make(Self.insert)` generates only the insert columns (no `Generated` id, the `Overrideable` slots elided) — each a free derivation off the precise variant, adding a column reshaping all of them at once.
- A per-leaf `message`/`jsonSchema`/`arbitrary`/`equivalence`/`pretty` annotation set on a FIELD schema rides into EVERY variant the field appears in, because each variant projection re-reads the same annotated leaf AST node — annotating once propagates the custom generator or comparator across `select`/`insert`/`json` simultaneously. These annotations are FUNCTIONS over the field's `TypeParameters` (`ArbitraryAnnotation<A, TP>` is `(...arbitraries, ctx: ArbitraryGenerationContext) => LazyArbitrary<A>`), so a leaf override RECEIVES the derived instances of its inner schemas and composes them, never re-deriving the children.
- Two reuse paths are NOT interchangeable across the variant boundary: spreading `Self.fields` (the SELECT `Schema.Struct.Fields`, markers ERASED) into a fresh `Model.Class` re-admits a `Generated` id as a REQUIRED insert column, while spreading `Model.fields(Self)` (the raw `[VariantSchema.TypeId]` map, markers INTACT) keeps it DROPPED from `insert` and a nested `DateTimeUpdate` auto-stamping — the canonical fragment-reuse path for a column set whose write semantics must survive.

[NESTED_STRUCT_SWAPS_CONSTRUCTOR_FOR_TYPE]:
- `Model.Struct(fields)` is a `VariantSchema.Struct<A>` — the `[VariantSchema.TypeId]` carrier ALONE, no `.insert`/`.json` siblings, no `Schema` identity — so `Audit.insert` does not exist and `Schema.decodeUnknown(Audit)` is ill-typed; a fragment's variant projections are reachable only through a parent's `extract` or `Model.extract("insert")(Audit)`. `ExtractFields` dispatches a nested struct by recursing `[Fields[K]] extends [Struct<infer _>]`, so its `Model.*` markers reshape by the PARENT's active variant — `Schema.Schema.Type<typeof Self.insert>["audit"]` differs in shape from `Schema.Schema.Type<typeof Self>["audit"]`.
- The construct trap is precise: the elision predicate is consulted by `Struct.Constructor` and IGNORED by `Struct.Type`, and a nested fragment projects as a `Schema.Struct` field whose value is a plain `Schema.Schema`, so the parent's `Constructor` routes it to the MANDATORY arm and embeds `Schema.Type<nested>`, NOT `Struct.Constructor<nested>`. A nested overrideable — elidable in a `Constructor` — is a REQUIRED key typed `(To & Brand<"Override">) | undefined` inside the parent's view: where a top-level slot is OMITTED to fire its default, a nested slot must be EXPLICITLY passed `undefined`. Reject `audit: {}` (fails the required nested `createdAt`); spell `audit: { createdAt: undefined }`. Membership recurses through `extract`; constructor-elision does not, because nesting swaps the `Constructor` projection for the `Type` projection.
- A self-referential entity recurses INSIDE one variant's interior, never across the variant axis: `Model.JsonFromString(Schema.Struct({ kids: Schema.Array(Schema.suspend((): Schema.Schema<TreeJson> => TreeJsonSchema)) }))` makes recursion a property of ONE leaf — `Schema.suspend` is admitted as a leaf by `ExtractFields`' final arm, so the db variants store the recursive object as text and the json variants carry it structured. A recursive leaf annotates its arbitrary against `ArbitraryGenerationContext.depthIdentifier`/`maxDepth` so a property test over the insert variant generates finite rows, the bound riding the leaf and propagating into every variant the column appears in.
- `Struct.Validate<Fields, Variant>` is the membership predicate run at declaration: it resolves a `Field<Config>` to `"field must have valid variants"` when `[keyof Config]` escapes the six, and recurses a nested struct (`A[K] extends { [TypeId]: infer _ } ? Validate<A[K], Variant>`). Two disjoint gates close the key domain — `Field`'s own `A & { [K in Exclude<keyof A, Variants[number]>]: never }` catches a directly-written literal at the `Field({...})` call, `Validate` catches a row that BYPASSED the constructor (spread from `Model.fields(donor)`, assembled by `fieldEvolve`). A directly-written `Field` is checked twice, a programmatic row once, which is why the assembly gate cannot fold into the constructor.

[GROUP_COMPOSITION_AND_CONSTRUCT_ALGEBRA]:

```typescript
import { Model } from '@effect/sql';
import { DateTime, Option, Schema } from 'effect';

const NodeId = Schema.Uint8ArrayFromSelf.pipe(Schema.brand('NodeId'));

class Leaf extends Model.Class<Leaf>('Leaf')({
    id: Model.UuidV4Insert(NodeId),
    kind: Schema.tag('Leaf'),
    weight: Model.JsonFromString(Schema.Struct({ value: Schema.Number })),
    archivedAt: Model.FieldOption(Model.Date),
    touchedAt: Model.DateTimeUpdateFromDate,
}) {
    get loud(): string {
        return this.kind.toUpperCase();
    }
}

class Branch extends Model.Class<Branch>('Branch')({
    id: Model.UuidV4Insert(NodeId),
    kind: Schema.tag('Branch'),
    fanout: Model.BooleanFromNumber,
    touchedAt: Model.DateTimeUpdateFromDate,
}) {}

const Node = Model.Union(Leaf, Branch);

type NodeRead = Schema.Schema.Type<typeof Node>;
type NodeInsertWire = Schema.Schema.Encoded<typeof Node.jsonCreate>;
type NodeStoreRow = Schema.Schema.Type<typeof Node.insert>;

const made = Leaf.insert.make({ weight: { value: 0 }, archivedAt: Option.none() });
const forced = Leaf.update.make({ id: NodeId.make(new Uint8Array()), weight: { value: 0 }, archivedAt: Option.none(), touchedAt: Model.Override(DateTime.unsafeNow()) });
```

- One `Model.Union(Leaf, Branch)` is six discriminated wire/store surfaces: `Node` decodes the `select` union on `kind`, `Node.insert` the store-write union, `Node.jsonCreate` the external-create union — a third member is one positional argument every projection absorbs, the `kind` tag closing each union's decode.
- `Leaf.insert.make` names neither `id`, `touchedAt`, nor `kind`: `UuidV4Insert`'s insert overrideable mints the branded key, `DateTimeUpdateFromDate`'s overrideable stamps the time, and `Schema.tag` auto-fills its literal — all three the `HasDefault=true` arm of `Struct.Constructor`. `Leaf.update.make` re-requires `id` (the `UuidV4Insert` row keeps `update`) yet still elides `touchedAt`, and `Model.Override(DateTime.unsafeNow())` is the only admitted force of the overrideable key — an unbranded value rejected.
- `NodeInsertWire` and `NodeStoreRow` diverge in shape off one union (`UuidV4Insert` absent from `jsonCreate`, present in `insert`; `weight` a `string` in `insert`, an object in `jsonCreate`) yet both descend from a single owner with no parallel interface; `loud` is invisible to `Equal`/`Hash`.

[WIRE_PROJECTION_STACK]:

```typescript
import { Model } from '@effect/sql';
import { JSONSchema, Schema } from 'effect';

const TokenId = Schema.Uint8ArrayFromSelf.pipe(Schema.brand('TokenId'));

class Entry extends Model.Class<Entry>('Entry')(
    {
        id: Model.UuidV4Insert(TokenId),
        secret: Model.Sensitive(Schema.Redacted(Schema.NonEmptyTrimmedString)),
        payload: Model.JsonFromString(Schema.Struct({ region: Schema.NonEmptyTrimmedString })).pipe(
            Model.fieldFromKey({ json: '<external-name>', jsonCreate: '<external-name>' }),
        ),
        tags: Schema.Array(Schema.String).pipe(Schema.minItems(1)),
        stampedAt: Model.DateTimeInsertFromNumber,
    },
    { identifier: '<owner-identifier>' },
) {}

type WireBody = Schema.Schema.Encoded<typeof Entry.json>;
type StoreRow = Schema.Schema.Encoded<typeof Entry.insert>;

const transit = Schema.parseJson(Entry.jsonCreate);
const contract = JSONSchema.make(Entry.jsonCreate, { target: 'openApi3.1' });
const bound = Schema.encodedBoundSchema(Entry.json);
```

- `WireBody` carries `payload` under `"<external-name>"` (the `fromKey` rename in the encoded view alone), omits `secret` (dropped from every json variant by `Sensitive`), and keeps `tags` with its `minItems(1)` survivable on the bound read; `StoreRow` keeps the canonical `payload` key as a `string` (the `JsonFromString` db leaf) and `secret` as the unredacted primitive (the `Redacted` encode-peel) — three columns, three wire shapes, no parallel DTO.
- `secret` stacks the two orthogonal axes — `Sensitive` decides membership (json-erased), `Redacted` decides representation (`Redacted<string>` interior, plain-text leaf) — a new wire-erased-but-store-redacted column landing as one such field every projection absorbs.
- `transit` is the request-body codec whose encoded leaf is `string` (decode parses inbound text straight into the create payload); `contract` derives the OpenAPI document off the encoded AST of the same variant; `bound` retains `tags`' `minItems(1)` (stable-flagged array filter) yet would drop a string `minLength` — the next variant or column reshaping all three at compile time.

[REQUIREMENT_AND_OPERATION_DERIVATION]:

```typescript
import { Model } from '@effect/sql';
import { VariantSchema } from '@effect/experimental';
import { Cause, Context, Effect, Option, ParseResult, Schema } from 'effect';

class Minter extends Context.Tag('Minter')<Minter, { readonly next: Effect.Effect<string> }>() {}

const StampWithMinter: VariantSchema.Overrideable<string, string, Minter> = VariantSchema.Overrideable(Schema.String, Schema.String, {
    generate: (forced: Option.Option<string>) =>
        Option.match(forced, {
            onNone: () => Effect.flatMap(Minter, (m) => m.next),
            onSome: (value) => (value.length > 0 ? Effect.succeed(value) : Effect.fail(new ParseResult.Forbidden(Schema.String.ast, value, 'empty'))),
        }),
});

class Entry extends Model.Class<Entry>('Entry')({
    id: Model.Generated(Schema.Number),
    label: Schema.NonEmptyTrimmedString,
    ticket: Model.Field({ select: Schema.String, insert: StampWithMinter, update: StampWithMinter, json: Schema.String }),
    touchedAt: Model.DateTimeUpdateFromDate,
}) {}

type ReadR = Schema.Schema.Context<typeof Entry>;
type InsertR = Schema.Schema.Context<typeof Entry.insert>;

const typedInsert = Effect.gen(function* () {
    const repo = yield* Model.makeRepository(Entry, { tableName: '<table>', spanPrefix: '<span>', idColumn: 'id' });
    return (payload: (typeof Entry)['insert']['Type']) =>
        repo.insert(payload).pipe(
            Effect.catchAllDefect((defect) =>
                ParseResult.isParseError(defect)
                    ? Effect.fail({ _tag: 'DecodeFailed' as const, issue: defect.issue })
                    : Cause.isNoSuchElementException(defect)
                      ? Effect.fail({ _tag: 'RowMissing' as const })
                      : Effect.die(defect),
            ),
        );
});
```

- `StampWithMinter`'s `generate` reads `Minter`, so `Minter` rides `Entry.insert`/`update`'s `Context` while the `select` slot (`Schema.String`, `R = never`) keeps `ReadR` equal to `never` — `InsertR` is `Minter`, the read and write requirement unions diverging off one field map; `onSome`'s `Effect.fail` proves the mint can reject a forced value with a `ParseIssue`.
- `makeRepository`'s `insert` declares `Entry["Context"] | Entry["insert"]["Context"]` = `never | Minter` = `Minter`, and `Entry` is barred from `makeDataLoaders` (the `ticket` write slots fail `AnyNoContext`) — closing the gate means folding `ticket`'s slots to `Schema<string, string, never>` at the leaf, never erasing `R` at the operation.
- `typedInsert` is the only path to a typed failure off a derived op: the method's `E` is `never` (every op pipes `Effect.orDie`), so `Effect.catchAllDefect` re-types the swallowed `ParseError` and `single`'s empty-`returning` `NoSuchElementException` — `findById` would model a miss as `Option.none()` instead, the empty-row semantics diverging by which `SqlSchema` constructor binds.
