# Persistence And Wire Owners: Model.Class Variant Derivation

[OWNER_SHAPE]:
- `class Self extends Model.Class<Self>("Self")({ ...fields })` is one statement that is at once the value-constructor, the static `Type`, the `select` codec, structural `Equal`/`Hash`, and the carrier of five sibling variant codecs — no `type Self = typeof Self`, no companion `const`, no separate `new`-target follows the declaration.
- The class instance IS the `select` variant: the default-variant flag wires `Model.Class` over `ExtractFields<"select", Fields, true>`, so `Self["Type"]`, `new Self(...)`, and `Schema.decodeUnknown(Self)` all speak the read shape, while `Self.insert`, `Self.update`, `Self.json`, `Self.jsonCreate`, and `Self.jsonUpdate` hang off the class as sibling `Schema.Struct` properties.
- The owner interface extends two bases at once — `Schema.Schema<Self, Simplify<I>, R>` AND `VariantSchema.Struct<Fields>` — so one declaration is simultaneously the decode/encode schema for the default variant and the carrier of the raw `[VariantSchema.TypeId]` field map; the schema half drives `Schema.decodeUnknown(Self)`, the struct half feeds `Model.fields`, `Model.extract`, and the five sibling variant properties.
- `Self` carries `readonly fields` (`Simplify<SchemaFields>`, the simplified `select`-variant `Schema.Struct.Fields`) and `readonly identifier` alongside the schema variance; `Model.fields(Self)` returns the raw `Self[VariantSchema.TypeId]` map with markers intact, so key-driven derivation reads either the read-shape struct or the variant-aware map without a second declaration.
- `Self["I"]` (the encoded type) is `Schema.Struct.Encoded<SchemaFields>` where `SchemaFields = ExtractFields<"select", Fields, true>` — the encoded view is the SELECT-variant struct's encoded shape, not the raw field map's, and `Schema.Schema.Encoded<typeof Self>` lifts exactly that read row, a plain key-to-encoded-leaf record with no class prototype.
- `Self`, the decoded type, is the class instance bearing the body's methods and getters — `Schema.Schema.Type<typeof Self>` is the nominal prototyped instance, distinct from `Schema.Schema.Type<typeof Self.json>`, a plain struct with no prototype; the owner's `A` carries a prototype while its variant projections do not.
- `readonly ast: AST.Transformation` — the owner's AST node is a transformation, not a struct: decode is the build-the-instance transform (read-struct to class instance), encode the inverse; the owner composes into `Schema.parseJson`, `Schema.compose`, and transform pipelines as a transformation leaf, and its encoded type is the pre-instance read-struct shape. Because `Self` is a transformation rather than a `Struct`, `Self.pick`/`Self.omit` do not exist — narrowing routes through `Self.fields` or a write variant's own `Self.insert.pick(...)`.
- The `Self` generic is structurally load-bearing: omitting it collapses the return type to the literal string `` "Missing `Self` generic - use `class Self extends Class<Self>()({ ... })`" ``, which is not a class and surfaces as a construction error at the declaration site, never as a downstream miss.

[FIELD_VARIANT_PROJECTION]:
- A bare `Schema.X` field appears in all six variants identically; a `Model.*` marker is a `VariantSchema.Field` whose `schemas` config names which variant keys receive which per-variant schema, and `ExtractFields<V, ...>` keeps a field in variant `V` only when `V extends keyof Config` — an absent key drops the column from that variant entirely. A nested `Field` contributes a key only when `variant in value.schemas` (the runtime presence check, not a sentinel), so the absent-key column drop is a structural fact of the extracted `Schema.Struct`: `Self.insert` literally lacks the property at the type and value level.
- `Model.Generated(s)` lands in `select | update | json`, omitted from `insert | jsonCreate | jsonUpdate`: the database supplies it, so insert payloads neither accept nor require it while read, update, and json expose it.
- `Model.GeneratedByApp(s)` lands in `select | insert | update | json`, absent only from `jsonCreate | jsonUpdate`: the application generates it for the SQL write, so an external creator never names it while the insert demands it.
- `Model.Sensitive(s)` lands in `select | insert | update` only, wholly absent from every json variant: the field round-trips through the store yet cannot appear in any wire projection, the variant map — not a redaction call — enforcing it.
- `Model.FieldOption(s)` reshapes a field across all six: database variants become `Schema.OptionFromNullOr<s>` (decode `null` to `Option.none`), `json` becomes `optionalWith<s, { as: "Option" }>`, and `jsonCreate | jsonUpdate` become `optionalWith<s, { as: "Option", nullable: true }>` — a missing key, a `null`, and a present value all decode to one `Option<A>`, and the wire-create forms additionally tolerate explicit `null`.
- `Model.JsonFromString(s)` splits representation by variant family: `json | jsonCreate | jsonUpdate` carry `s` directly (a structured object on the wire) while `select | insert | update` carry `Schema.Schema<Schema.Schema.Type<s>, string, Schema.Schema.Context<s>>` — the database column is text, parsed in and stringified out, with the decoded object identical on both sides.

[VARIANT_FAMILY_PARTITION]:
- `Model.VariantsDatabase = "select" | "insert" | "update"` and `Model.VariantsJson = "json" | "jsonCreate" | "jsonUpdate"` are the canonical variant-family partition; `FieldOption`'s evolve arm dispatches on `K extends VariantsDatabase` to choose `OptionFromNullOr` versus nullable `optionalWith`, so a custom marker reuses this partition rather than re-enumerating six keys.
- A leaf's wire representation diverges by variant family while the decoded interior stays one type across all six: a `Model.JsonFromString` column's decoded object and a `Model.FieldOption` column's `Option<A>` are uniform, yet their encoded leaves split per family — the family partition is the encode-side axis, the decode-side staying invariant.

[LEAF_WIRE_OWNERS]:
- `Model.Date` is `transformOrFail<String, DateTimeUtcFromSelf>` serializing `DateTime.Utc` as a `YYYY-MM-DD` string — the wire owner for a calendar-date column, distinct from the timestamp markers whose json slot is full ISO `Schema.DateTimeUtc`.
- `Model.BooleanFromNumber` is `Schema.transform<Literal<[0, 1]>, Boolean>` — the leaf owner for a SQL `0 | 1` flag column decoding to `boolean`; the integer-encoded boolean round-trips without a hand-rolled `=== 1` step anywhere inside.
- `Model.JsonFromString(inner)` and `Model.Date` differ in axis: the former splits representation by variant family (text in database variants, structured object in json variants), the latter is uniform across every variant and only changes the encoded primitive — a per-variant split belongs to a `Field` config, a uniform leaf to a plain transform schema.

[CONSTRUCTOR_ELISION]:
- The auto-stamp timestamp markers set their `insert`/`update` slot to a `VariantSchema.Overrideable<DateTime.Utc, From>`, a `PropertySignature` with `HasDefault=true`; `Schema.Struct.Constructor` routes every `PropertySignatureWithDefault` field to an optional constructor key, so the timestamp auto-fills from the current `DateTime.Utc` on encode and is never passed to the write `.make`.
- `Model.DateTimeInsert`/`DateTimeInsertFromDate`/`DateTimeInsertFromNumber` carry no `update` key — written once and frozen across updates; `Model.DateTimeUpdate`/`DateTimeUpdateFromDate`/`DateTimeUpdateFromNumber` carry both `insert` and `update` overrideables, stamping on every write. Presence or absence of the `update` slot, not a flag, selects insert-once versus touch-on-write.
- `Model.Override(value)` is the runtime identity `value => value` whose `& Brand<"Override">` is type-only: an overrideable slot's decoded type is `(To & Brand<"Override">) | undefined`, so passing `Model.Override(x)` is the only admitted force of a non-default into a slot the constructor otherwise auto-supplies — an unbranded literal is rejected, and `constructorDefault` (default `constUndefined`) drives `generate(none)` otherwise. The brand is the opt-out of the generated default.
- The `select` slot for the `FromDate` timestamp markers is `Model.DateTimeFromDate` (`transform<ValidDateFromSelf, DateTimeUtcFromSelf>`), decoding a native `Date` column into `DateTime.Utc` and encoding the inverse; the `FromNumber` family does the same against epoch millis (`Schema.DateTimeUtcFromNumber`), and the `json` slot is plain `Schema.DateTimeUtc` (ISO string) for the string family.

[GENERATIVE_SUBSTRATE]:
- `VariantSchema.Overrideable(from, to, { generate, decode?, constructorDefault? })` is the substrate every auto-stamped slot expands to: `Schema.transformOrFail(from, Schema.Union(Schema.Undefined, to))` piped through `Schema.propertySignature` then `Schema.withConstructorDefault(constructorDefault ?? constUndefined)` — the `Schema.Union(Schema.Undefined, to)` admits the absent value, the result type is `Schema.PropertySignature<":", (To & Brand<"Override">) | undefined, never, ":", From, true, R>` with `HasDefault=true`, and `constUndefined` is the default that fires `generate(none)`.
- `generate` is `(Option<ITo>) => Effect<From, ParseResult.ParseIssue, R>`: it runs on encode, receiving `Option.some(value)` when an `Override` was supplied and `Option.none()` when the constructor default fired, and produces the persisted `From` — so the current-time or current-uuid value is computed inside the encode pass against `R`, never at field-declaration time.
- `Schema.Struct.Constructor` routes any `PropertySignatureWithDefault` (the `HasDefault=true` arm) to an OPTIONAL constructor key; because `Overrideable` is exactly that signature, every generative slot is elided from the construct payload — the field is unnameable at the write `.make` unless deliberately forced.
- `Model.UuidV4Insert(brandedSchema)` is the generative analogue of the timestamp family: `select | update | json` carry `Schema.brand<Uint8ArrayFromSelf, B>` while `insert` is `Overrideable<Uint8Array & Brand<B>, Uint8Array>` whose `generate` mints a v4 UUID — the binary key is auto-stamped on insert, branded identically across the read/update/json shapes, and absent from the insert constructor.
- `Model.DateWithNow`, `DateTimeWithNow`, `DateTimeFromDateWithNow`, `DateTimeFromNumberWithNow`, and `UuidV4WithGenerate` are the raw `Overrideable` values the field markers wrap into the `insert`/`update` slots, so a bespoke generative column composes a raw `*WithNow` into a hand-built `Model.Field` config rather than reaching for a new marker.

[CONSTRUCT_KEY_TAXONOMY]:
- The class constructor input `C` is `Schema.Struct.Constructor<SchemaFields>` derived from the SELECT variant ALONE — `new Self(...)` and `Self.make(...)` build the read shape and require every selectable column, NOT the insert payload; a generative column is required at construction unless its SELECT slot itself carries a default.
- `Model.Generated(s)`'s select slot is the bare schema `s`, so `id` is a REQUIRED constructor key of type `Schema.Schema.Type<s>` — the database-supplied value must be passed to `new Self(...)` even though it is absent from `Self.insert`; construction is not the insert path.
- A `Model.DateTimeUpdate` column's select slot is `Schema.DateTimeUtc` (the string family) or `Model.DateTimeFromDate` (the `FromDate` family) — a plain transform, NOT the overrideable — so its constructor key is a REQUIRED `DateTime.Utc`; the current-time auto-stamp lives in the insert/update slots, never in the read-shape constructor, and `new Self(...)` must be handed the timestamp.
- Only `Schema.tag(literal)` — `PropertySignature<":", Tag, never, ":", Tag, true, never>` with `HasDefault=true` in the select slot — yields an elidable `kind?: Tag | undefined` constructor key, the lone discriminant the read constructor auto-fills; `Model.FieldOption`'s select slot is `OptionFromNullOr<s>`, a REQUIRED `Option<A>` key, so `Option.none()` is passed explicitly.
- `RequiredKeys<T>` is `{ [K in keyof T]-?: {} extends Pick<T, K> ? never : K }[keyof T]` — a key survives only when `{}` is NOT assignable to its single-key pick; when it resolves to `never` the props argument collapses to `void | C`, so a variant whose every constructor key is optional constructs argument-free, one required key flipping the branch to a mandatory payload.
- `Schema.Struct.Constructor` reaches the optional `readonly [H in K]?: Schema.Type<F[H]>` arm through two disjoint signature predicates — `OptionalTypePropertySignature` (the `"?:"` type token) and `PropertySignatureWithDefault` (the `":"` token with `HasDefault=true`) — and only a bare schema falls to the mandatory arm; these two predicates are the entire taxonomy of elidable keys across all three construct surfaces.
- One `PropertySignatureWithDefault` predicate unifies the discriminant and the generative slot: a `Schema.tag` and an `Overrideable` share the `true` `HasDefault` slot and both elide, so the read constructor elides only the tag (the sole defaulted SELECT slot) while a write `.make` additionally elides every overrideable — the SAME predicate applied to DIFFERENT variant slots yields the per-construct asymmetry.

[WRITE_VARIANT_MAKE]:
- Each variant property `Self.insert`/`Self.update`/`Self.json` is a `Schema.Struct<Fields>` carrying its OWN `make(props: RequiredKeys<Struct.Constructor<F>> extends never ? void | Simplify<C> : Simplify<C>)` — three independent construct surfaces off one owner, each derived from its variant's slot signatures, and the generative auto-stamp is elided at the WRITE variant's `.make`, never at the class constructor.
- `Self.insert.make(...)` is where `Model.Generated` columns vanish (absent from the insert variant entirely) and a `Model.DateTimeUpdate`/`UuidV4Insert` slot becomes an OPTIONAL `(To & Brand<"Override">) | undefined` key — the insert slot IS the `Overrideable` (`PropertySignature` with `HasDefault=true`), so `Schema.Struct.Constructor` routes it to the elidable arm and the timestamp or uuid is minted on encode unless `Model.Override(x)` forces it.
- `Self.update.make(...)` differs from insert by exactly the `Generated` columns: `Model.Generated` is present in `update`, so `id` is a REQUIRED update-construct key while ABSENT from insert — the update payload names the primary key (the row to touch), the insert payload does not (the database mints it), variant slot membership deciding key presence with no flag.
- The `(To & Brand<"Override">) | undefined` type of a write-variant overrideable key is the type-level proof that `Model.Override(value)` is the only admitted non-default: an unbranded literal is rejected at the elidable key, so a write `.make` either omits the slot (auto-generate fires) or passes `Model.Override(x)` (forced value) — there is no third spelling.
- `Self.insert.pick(...keys)` and `Self.insert.omit(...keys)` narrow a write variant to a `Schema.Struct` over a key subset for a partial-write codec, each preserving the per-slot signature so an omitted overrideable still auto-stamps and a picked `Generated` stays required — variant-local narrowing without a second model.

[WIRE_VIEW_DERIVATION]:
- `Schema.Schema.Encoded<typeof Self.json>` is the wire body and `Schema.Schema.Type<typeof Self.json>` the decoded interior; the pair is one indexed access per variant, so all six variants yield twelve free type derivations off the one owner with no parallel interface — `Schema.Schema.Encoded<S>` unwraps the `I` slot of `Schema.Variance<A, I, R>`. `Schema.Schema.Type<typeof Self.jsonCreate>` is the decoded create payload, the encoded/decoded pair of each variant one indexed access away.
- A leaf's encoded shape is the leaf's `I` slot read recursively through the variant: `Model.JsonFromString`'s database-variant encoded is `string` against its json-variant structured object, and `Model.FieldOption`'s database encoded is `OptionFromNullOr`'s `I` (`null | A`) against its jsonCreate optional-key form — the wire view reads each column's own encoded leaf with no separate projection pass.
- `Schema.parseJson(Self.json)` is `transform<SchemaClass<unknown, string>, typeof Self.json>` — its encoded leaf is `string`, so it composes the JSON text seam ONTO the json variant: decode parses the request string straight into the decoded json type, encode `JSON.stringify`s the projection, and a `ParseJsonOptions` configures both `JSON.parse` and `JSON.stringify` in the same value. A text column body and an HTTP request body need no separate parse step — the codec owns the textual boundary as a transformation leaf.
- `Schema.instanceOf(Ctor)` is `AnnotableDeclare<instanceOf<A>, A>` — encoded EQUALS decoded (identity on a self type), so a non-serializable host instance lives in the `select`/`Type` view but has no distinct wire body; a variant that must cross the wire pairs it with a real `Schema.transformOrFail` whose encoded leaf is the serializable form, otherwise the encoded view re-emits the live object and the wire body is non-portable.

[REDACTION_AND_REPRESENTATION_STACK]:
- `Schema.Redacted(value)` is `transform<value, RedactedFromSelf<SchemaClass<Type<value>>>>`: decode wraps the value in `Redacted` (hidden from logs and `JSON.stringify`), encode unwraps to the bare `value`'s encoded — so the decoded interior is `Redacted<A>` while the encoded store/wire leaf is the bare `A`. Redaction is a property of the DECODED owner, orthogonal to `Model.Sensitive`, which removes the column from json variants outright.
- A column's wire representation is a STACK of transforms whose decoded interior and encoded leaf diverge twice: `Model.JsonFromString(Schema.Redacted(inner))` on a database variant is `Schema<Redacted<Type<inner>>, string, _>` — the json codec stringifies the structured object AND the redaction transform wraps the parsed interior, so the decoded type is `Redacted<A>` while the stored column is plain text; the redaction rides INSIDE the text seam, not beside it.
- The two axes are independent and stack on one field: `Model.Sensitive` decides VARIANT MEMBERSHIP (erased from json bodies), `Schema.Redacted` decides DECODED REPRESENTATION (the select/insert/update interior is `Redacted<A>` while the stored leaf is plain text) — neither recoverable from the other, so a `Model.Sensitive(Schema.Redacted(inner))` slot redacts in the select/insert/update interior AND erases from every wire body, a new wire-erased-but-store-redacted column landing as one such stacked field with every projection absorbing it.

[KEY_RENAME_IS_ENCODED_ONLY]:
- `VariantSchema.fromKey<S, Key>` is `PropertySignature<":", Type<S>, Key, ":", Encoded<S>, false, Context<S>>` — `Key` occupies the THIRD parameter (the encoded key) while the decoded key stays the canonical field name; the foreign name surfaces ONLY in `Schema.Schema.Encoded`, so `Model.fieldFromKey({ json: "<external-name>" })` rewires the wire body's key without touching `Type`, the decoded interior, the constructor, or any other variant.
- `fieldFromKey` dispatches per variant: a mapping naming only `json` renames the wire key for the json body while `insert`/`update`/`select` keep the column name, so one boundary rename is isolated to the exact variant that crosses the seam — the SQL row and the JSON body carry different key spellings for one canonical field with no second model and no decoded drift.
- `Schema.Struct.Encoded<F>` reads each property's encoded key through `fromKey`'s `Key` parameter, so renaming is a structural fact of the encoded type, not a runtime remap step the consumer writes: `Schema.Schema.Encoded<typeof Self.json>` already carries the foreign key, and decode/encode against `Self.json` round-trips the rename with the canonical name inside.

[DOWNSTREAM_WIRE_SURFACE_DERIVATION]:
- `JSONSchema.make(Self.jsonCreate, { target: "openApi3.1" })` derives the create-body contract off the wire-create variant from its ENCODED AST, so a `Model.Generated` column (absent from `jsonCreate`) never appears, a `fromKey` rename surfaces under the foreign key, and `Model.FieldOption`'s `optionalWith` jsonCreate slot emits an optional-plus-nullable property; the published contract is a derivation off the same owner, not a hand-kept mirror, and `target` selects one of `"jsonSchema7" | "jsonSchema2019-09" | "jsonSchema2020-12" | "openApi3.1"`.
- `Schema.standardSchemaV1(Self.json)` lifts the json variant to a `StandardSchemaV1<I, A>` for a foreign validator speaking the Standard Schema interface — the encoded `I` is the input it validates, the decoded `A` the output; the variant codec, not a re-authored validator, is the cross-tool wire contract, and the signature requires `Schema<A, I, never>`, so a variant carrying an unresolved `Context` cannot be lifted until its services are provided.
- `Schema.encodedSchema(Self.json)` strips every transform to expose the raw wire `SchemaClass<I>` (the JSON body schema with no decode work), while `Schema.encodedBoundSchema(Self.json)` retains refinement bounds at the encoded boundary — the former is the permissive wire-shape gate, the latter keeps a `NonEmptyTrimmedString`'s emptiness rejection on the wire body; a publish path that must reject empty strings before decode reaches for the bound form.

[ANNOTATION_PROPAGATION_LIMITS]:
- `Self.annotations(...)` returns `Schema.SchemaClass<Self, I, R>` — a PLAIN schema, NOT the variant-carrying class — so annotating the owner directly forfeits `.insert`/`.json`/`.fields` access; identifier, message, and documentation annotations pass as the second argument to `Model.Class<Self>("Self")({ ...fields }, { ... })` at declaration so the class keeps its variant surface while carrying the annotation.
- The `annotations` argument to `Model.Class` types as `Schema.Annotations.Schema<Self, readonly []>`, keyed to the DECODED `Self` — so a custom `equivalence`, `pretty`, or `arbitrary` annotation describes the prototyped read INSTANCE, never the wire body; a wire-body override is set on the field leaves (propagating to the json variant) or derived fresh via `Schema.equivalence(Self.json)`, never reachable from the owner-level annotation.
- A per-leaf `message`/`jsonSchema`/`arbitrary`/`equivalence`/`pretty` annotation set on the field schema rides into EVERY variant the field appears in, because each variant projection re-reads the same annotated leaf AST node — annotating the field schema once propagates the custom generator or comparator across `select`/`insert`/`json` simultaneously, the single-source rule applied to the derived family.

[INSTANCE_EQUALITY_IS_THE_SELECT_VARIANT]:
- The decoded class instance fuses structural `Equal`/`Hash` over its declared fields — `new Self(...)` returns `Struct.Type<Fields> & Inherited & Proto`, and the `Equal`/`Hash` symbol slots compare by field value, so two read rows decoded from identical columns are `Equal.equals(a, b) === true` with no hand-written comparator; the owner is a value, not a reference identity.
- Field equality is recursive through the field's DECODED representation, not its column: a `Model.JsonFromString(Struct({...}))` column compares by the parsed object's structure, a `Model.FieldOption` column by `Option` equivalence (`none`/`none` equal, `some(x)`/`some(y)` by inner), a `Model.UuidV4Insert` branded `Uint8Array` by byte content — so dedup, `HashSet` membership, and memo keys over read rows are correct without unwrapping a single field.
- The branded primitive participates in equality by its underlying value, not its brand: `NodeId & Brand<"NodeId">` hashes and compares as the bare primitive, so a branded id is a legal `HashMap`/`HashSet` key and the brand is a type-level nominal gate erased at the equality layer — a `Map` keyed on raw and branded forms of the same value collides as intended.
- A body getter or method added to the class participates in NEITHER equality NOR hashing — `Equal`/`Hash` read the field set the field map declares, so a derived `get screaming()` is invisible to value comparison; two instances differing only in a computed projection are `Equal`, the correct semantics for a persisted row whose identity is its columns.

[VARIANT_KEYED_DERIVATION]:
- `Schema.equivalence(schema)` returns `Equivalence.Equivalence<A>` over the schema's DECODED `A`, so each variant yields a structurally distinct equivalence: `Schema.equivalence(Self)` is read-instance equality, `Schema.equivalence(Self.json)` is wire-body equality over the plain json struct (no prototype), `Schema.equivalence(Self.insert)` is store-write-payload equality — the same column contributes a different equivalence when its variant slot changes representation, and a `Model.Sensitive` column drops out of every json-variant equivalence because it is absent from that variant's field set.
- `Arbitrary.make(Self.insert)` is the property-test generator for the store-write payload, derived off the variant codec: it generates only the columns the insert variant carries (no `Model.Generated` id, the `Overrideable` slots absent because their constructor key is elided), so a generator for a SQL insert row is one indexed access — `Arbitrary.make(Self.jsonCreate)` is the create-body generator, distinct in shape, off the same owner.
- `Pretty.make(Self.json)` is the wire-body renderer keyed to the json variant's decoded interior; `Schema.is(Self.json)` returns `(u: unknown) => u is Schema.Schema.Type<typeof Self.json>` and `Schema.asserts(Self.json)` returns the throwing `(u: unknown) => asserts u is …` — each a free derivation off the precise variant, never a hand-written predicate beside the owner.
- `Schema.is`/`Schema.asserts` check the DECODED `A` synchronously with no decode/transform pass — they are the structural shape gate, NOT the admission boundary; a `Model.JsonFromString` column passes `is` against the parsed object, not the text, so the guard answers "is this already the decoded shape" and never substitutes for `Schema.decodeUnknown` at the seam.
- `Arbitrary.make`, `Pretty.make`, `Schema.equivalence`, `Schema.is`, and `Schema.asserts` all type the schema as `Schema<A, I, R>` and operate on `A`/`I` regardless of `R`, so an unresolved `Context` does NOT block these derivations — distinct from `Schema.standardSchemaV1`, whose `Schema<A, I, never>` signature must run the decode effect; a variant carrying a pending service still yields its equivalence, generator, and guard.

[ANNOTATION_OVERRIDE_RECURSION]:
- The `equivalence`/`arbitrary`/`pretty` annotations are functions over the field's `TypeParameters`, not flat values: `EquivalenceAnnotation<A, TP>` is `(...equivalences: { [K]: Equivalence<TP[K]> }) => Equivalence<A>`, `ArbitraryAnnotation<A, TP>` is `(...arbitraries, ctx: ArbitraryGenerationContext) => LazyArbitrary<A>`, `PrettyAnnotation<A, TP>` likewise — so a leaf override RECEIVES the derived instances of its inner schemas and composes them, never re-deriving the children.
- `ArbitraryGenerationContext` carries `maxDepth`, `depthIdentifier`, and a `constraints` union (`StringConstraints | NumberConstraints | BigIntConstraints | DateConstraints | ArrayConstraints`) — a recursive owner annotates its self-referential field's arbitrary against `depthIdentifier` to bound recursion, so a tree-shaped persisted entity generates finite sample rows without a hand-written depth counter.

[PER_VARIANT_ERROR_SURFACE]:
- Each variant schema is installed via `Object.defineProperty` with `identifier`/`title` = `"${owner}.${variant}"`, so a `ParseError` from `Schema.decodeUnknown(Self.jsonCreate)` names the exact crossing variant (`"Self.jsonCreate"`) in its message and AST inspection — the failure surface distinguishes a malformed create body from a malformed store row at variant granularity, with no per-variant error type authored, while the variant still shares the owner's field provenance.
- `ParseResult.ArrayFormatter` renders a `ParseError` into `Array<{ _tag: ParseIssue["_tag"]; path: ReadonlyArray<PropertyKey>; message: string }>` — the `path` is keyed by the variant's ENCODED key, so a `fieldFromKey` rename surfaces the foreign name in the issue path for the json variant while the same column reports the canonical name for the insert variant; the formatter reads the variant whose decode failed, not the canonical field map.
- `ParseResult.TreeFormatter` is the single-string render of the same error tree; the two formatters are one error value projected two ways, so a validation-failure response body and a log line derive from one `ParseError` with no second traversal — the variant identity in the node title threads into both renders.

[GROUP_UNION_ALGEBRA]:
- `VariantSchema.make({ variants, defaultVariant })` returns the whole toolkit — `Struct`, `Field`, `FieldOnly`, `FieldExcept`, `Class`, `Union`, `fieldEvolve`, `fieldFromKey`, `extract` — closed over one variant alphabet; the six-variant persistence toolkit is one application of this factory, and a different alphabet (`"draft" | "live"`) yields a parallel toolkit with identical mechanics from the same constructor.
- `Union(...members)` fuses several owner structs into one schema that is itself the `defaultVariant` union AND carries one sibling property per variant, each a `Schema.Union` of that variant's per-member extract — `U.insert` is `Schema.Union<[A.insert, B.insert]>`, `U.json` is `Schema.Union<[A.json, B.json]>`; six variant unions derive from one `Union` call over `N` owners with zero per-pair restatement.
- The base union is `Schema.Union(...members.filter(Schema.isSchema))` — only members that are themselves schemas (class-backed owners) join the default discriminated union; a bare `Struct` group with no schema identity contributes to the variant properties but not the default decode surface, so mixing `Model.Class` owners with raw `Struct` owners in one `Union` is well-typed yet the latter is reachable only through the variant projections.
- A new member lands as one positional argument to `Union`; every downstream variant union absorbs it because the member set is the union's `const Members` tuple and each variant property maps `Extract<variant, member>` across it — the `_tag` discriminant the members already carry closes each decode, so the added owner is checked into all six wire/store surfaces by one declaration.

[EXTRACT_MEMOIZATION]:
- `extract(self, variant)` memoizes on a per-struct `cacheSymbol`, keyed `"__default"` for the default-variant projection and by variant name otherwise — repeated variant access on one owner returns the identical schema instance, so `Self.json` referenced across many call sites is one cached node, not a re-derivation.
- The default branch keeps a nested member as-is when it `Schema.isSchema` (preserving the class's own self-type and identity) but re-extracts a nested `Struct` for any non-default variant; the default-variant projection of a class-owning field is the field's own schema, not a re-wrapped struct.
- `Model.extract("insert")(Self)` (curried) or `Model.extract(Self, "insert")` (tupled) lifts a single variant struct out of the owner for standalone composition; the default variant `"select"` returns the schema as-is through the `IsDefault` branch, so extracting the read shape yields the class's own self type rather than a re-wrapped struct.

[NESTED_STRUCT_IS_A_VARIANT_FRAGMENT]:
- A field map value is one of four kinds — a `Schema.Schema.All`, a `Schema.PropertySignature.All`, a `VariantSchema.Field` marker, or a nested `VariantSchema.Struct` — and `ExtractFields` dispatches on the last by recursing `[Fields[K]] extends [Struct<infer _>] ? Extract<V, Fields[K], IsDefault>`, so a `Model.Struct({...})` placed as a field value projects ITS OWN columns through the SAME parent variant, yielding a nested per-variant sub-object on every projection; a `Schema.suspend` is admitted as a leaf by the final arm.
- The nested struct carries its own `Model.*` markers and they reshape by the parent's active variant: a `Model.Generated` column inside the nested struct is dropped from the parent's `insert` sub-object and present in `select`/`update`, a nested `DateTimeInsertFromDate` auto-stamps inside the parent's insert sub-object — the variant decision threads one level down with no per-level restatement, so `Schema.Schema.Type<typeof Self.insert>["audit"]` differs in shape from `Schema.Schema.Type<typeof Self>["audit"]`.
- `Model.Struct(fields)` is a `VariantSchema.Struct<A>` — the `[VariantSchema.TypeId]` carrier ALONE, with no `.insert`/`.json`/`.select` sibling properties and no `Schema` identity; `Audit.insert` does not exist and `Schema.decodeUnknown(Audit)` is ill-typed. The variant projections of a nested struct are reachable only THROUGH a parent owner's extract or by lifting it with `Model.extract("insert")(Audit)`, so a shared fragment is composed, never decoded standalone.

[FRAGMENT_REUSE_AXIS]:
- Two composition sources read off an owner and they are NOT interchangeable: `Self.fields` is `Simplify<SchemaFields>` — the SELECT-variant `Schema.Struct.Fields`, each column flattened to its select schema with the `VariantSchema.Field` markers ERASED — while `Model.fields(Self)` is `Self[VariantSchema.TypeId]`, the raw variant-aware field map with every marker INTACT.
- Spreading `Self.fields` into a fresh `Model.Class` re-admits every column as its bare select schema, so a `Model.Generated` id REJOINS the new owner's `insert` variant as a required column — the marker's variant gating is lost because the select projection already collapsed it; the wide-read-shape reuse path is correct only when the borrowed columns must be writable on the consumer.
- Spreading `Model.fields(Self)` into a fresh `Model.Class` re-admits the `VariantSchema.Field` markers themselves, so a `Model.Generated` id stays DROPPED from the new owner's `insert` and a nested `DateTimeUpdate` keeps auto-stamping — the consumer owner inherits the donor's full variant algebra, the canonical fragment-reuse path for a shared column set whose write semantics must survive.
- `Schema.Struct(Self.fields)` rebuilds a plain struct from the select fields and exposes `.pick(...keys)`/`.omit(...keys)` returning `Schema.Struct<Pick<...>>`/`Schema.Struct<Omit<...>>` — a key-subset read codec derived from the owner with no second model.
- `Model.fieldEvolve({ <variant>: s => ... })` transforms named variant slots of an existing field in place, and `Model.FieldExcept(...keys)(s)` / `Model.FieldOnly(...keys)(s)` build a field present in the complement of / exactly the listed variants — the open path for a column whose variant set matches no built-in marker, derived from one schema rather than spelled per variant.
- `Model.FieldOption` accepts either a raw `Schema` or an existing `VariantSchema.Field` and remaps each present slot in place, so markers do not stack by wrapping: optionality layers onto an already-variant-shaped field by per-slot rewrite rather than nesting a field in a field.

[VALUE_CLASS_PROMOTION_BOUNDARY]:
- A value-class promotion takes a VARIANT STRUCT as its fields argument, never the owner: `class View extends Schema.Class<View>("View")(Self.json) {}` is well-typed because `Self.json` is a `Schema.Struct` satisfying the `HasFields` bound, whereas `Schema.Class<...>(Self)` fails to typecheck — the owner is `Schema.Schema<Self, I, R>` missing `records`/`pick`/`omit`, so the variant struct, not the variant-carrying owner, is the only legal field source.
- The promoted class is a fresh independent owner: `View` exposes the json variant's columns plus its body getters and `extend`/`transformOrFail` chaining the variant owner's static side lacks, but carries NO `.insert`/`.json` variant properties of its own — promotion crosses out of the variant algebra into a single-shape `Schema.Class`, so a getter-bearing wire model is one prototype over one variant's field set, and a second prototype over a different variant (`Schema.Class(Self.insert)`) is a parallel promotion off the same owner.
- `Schema.extend(Self.json, Schema.Struct({ ... }))` fuses an extra wire fragment onto a single variant — supported because both sides are structs with non-overlapping fields — producing a struct that decodes the json columns plus the appended ones; the fusion targets ONE variant, so an `etag` added to the wire body never touches `select`/`insert`, isolating a transport-only column to the exact projection that carries it.

[RECURSIVE_OWNER_INTERIOR]:
- A self-referential entity recurses inside a single variant's interior, never across the variant axis: `Model.JsonFromString(Schema.Struct({ kids: Schema.Array(Schema.suspend((): Schema.Schema<TreeJson> => TreeJsonSchema)) }))` makes the recursion a property of ONE leaf's decoded interior — the database variants store the recursive object as text, the json variants carry it structured, and the thunk defers the self-reference past the binding's own initialization.
- The recursion lives below the variant boundary because `Schema.suspend` is a `Schema.Schema` admitted as a leaf by `ExtractFields`' final arm; the per-variant divergence (text versus structured) is the `JsonFromString` marker wrapping the recursive schema, the recursion the suspended interior, and the two axes compose without a recursive `Model.Class` (which would need a self-referential variant set the toolkit does not model).
- A recursive leaf annotates its self-referential arbitrary against `ArbitraryGenerationContext.depthIdentifier`/`maxDepth` so a property test over the owner's insert variant generates finite sample rows, the bound riding the leaf and propagating into every variant the column appears in.

[CONSTRUCTOR_NESTING_TRAP]:
- The class constructor builds the nested struct's SELECT shape: `new Self({ audit: { createdAt, revision } })` requires the nested select columns (`Generated`'s `revision` and the select-slot `DateTimeFromDate` timestamp), because the constructor input `C` is `Schema.Struct.Constructor<SchemaFields>` over the SELECT projection and the nested struct contributes its own select-constructor sub-record.
- The overrideable elision does NOT thread into a nested struct under `exactOptionalPropertyTypes`: a TOP-LEVEL `DateTimeInsertFromDate` slot is elided from `Self.insert.make` (absent from the payload), but the SAME marker INSIDE a nested struct surfaces as a required nested key typed `(DateTime.Utc & Brand<"Override">) | undefined`, so `Self.insert.make({ audit: {} })` fails `TS2741` and the nested overrideable must be spelled `{ audit: { createdAt: undefined } }` to fire the auto-stamp — the elision is a parent-key property of `RequiredKeys`, not a recursive one.
- `new Self(props, { disableValidation: true })` is the second constructor argument that skips the decode/refinement pass at construction, the escape used when re-wrapping an already-validated read instance (`new Self(existing, { disableValidation: true })`) without re-running every nested struct's refinements.

[VALIDATION_GATE]:
- `VariantSchema.Struct.Validate<Fields, Variant>` is a compile-time field gate: any `Model.*` field whose `Config` keys are not a subset of the six known variants resolves to the literal `"field must have valid variants"` in the fields-argument position, rejecting a malformed marker at the field map rather than at first use.
- The validator recurses the nested struct — `A[K] extends { readonly [TypeId]: infer _ } ? Validate<A[K], Variant>` — so a malformed marker buried in a nested fragment surfaces `"field must have valid variants"` at the nested field position, and the `Field` constructor's own `{ readonly [K in Exclude<keyof A, Variants[number]>]: never }` clause types an unknown variant key as `never` at the fragment, rejecting a bad column at the fragment declaration rather than at parent assembly.

[CONTEXT_IS_THE_R_SLOT_LIFTED]:
- `Schema.Schema.Context<S>` reads the `R` of `Schema.Variance<A, I, R>`, and `Schema.Struct.Context<Fields>` is `Schema.Schema.Context<Fields[keyof Fields]>` — the indexed-by-`keyof` form is a UNION fold, so a struct's requirement is the union of every field's requirement with no per-field enumeration, and a single field carrying an unresolved service contaminates the whole struct's `R`.
- The owner's own `R` is `Schema.Schema.Context<ExtractFields<"select", Fields, true>[keyof ...]>` — the SELECT projection ALONE drives `Self["Context"]`, so a service required only by an `insert`-slot codec never appears in the owner's decode requirement; the read-shape requirement and the write-shape requirement are structurally distinct unions off one field map.
- `Schema.Schema.Context<typeof Self.insert>` is the union of the INSERT projection's per-slot `R`, computed over a different slot set than the select union — the same column contributes a different requirement (or none) when its variant slot changes schema, so the six variant codecs each carry their own `R` derived by the same union fold over disjoint slot families.
- A leaf `R` is the only path a service reaches the requirement union: an `as const` table, a `Schema.tag`, and a `Schema.NonEmptyTrimmedString` all carry `R = never`, so an owner built from those decodes context-free, and the moment one field's codec is `Schema<A, I, SomeService>` every variant projection that retains that field inherits `SomeService` in its `Context`.

[GENERATIVE_R_RIDES_THE_WRITE_VARIANTS]:
- `VariantSchema.Overrideable<To, From, R>` carries its `R` in the THIRD type parameter, and `Overrideable(from, to, { generate })` returns `Overrideable<To, IFrom, RFrom | R>` where `R` is the requirement of the `generate` effect `(Option<ITo>) => Effect<From, ParseResult.ParseIssue, R>`, so a generative slot whose mint reads a service threads that service into exactly the variant slots that hold the overrideable.
- The auto-stamp markers place the overrideable in `insert`/`update` slots and a plain non-overrideable schema in the `select`/`json` slots, so a generative `R` enters `Self["insert"]["Context"]` and `Self["update"]["Context"]` but NEVER `Self["Context"]` — the read constructor and the read decode stay context-free while the write codec carries the mint's requirement, slot membership deciding which operation inherits the service.
- The library's own `DateTimeWithNow`/`DateTimeFromDateWithNow`/`DateTimeFromNumberWithNow`/`DateWithNow`/`UuidV4WithGenerate` are declared `Overrideable<DateTime.Utc, From, never>` — their `generate` mints from the ambient clock or entropy already provided, so the built-in stampers add NOTHING to any variant's `R`; a bespoke `VariantSchema.Overrideable` whose `generate` yields a tagged service is the only way a write variant acquires a non-`never` requirement.
- `Model.Override(value)` cannot satisfy a non-`never` overrideable `R`: forcing a value bypasses the elidable key but the slot's schema type is unchanged, so the variant codec still declares the `generate`-effect requirement whether or not the default fires — a write whose stamper needs a service requires that service provided regardless of `Override`, the requirement being a property of the codec, not the value passed.

[VALIDATION_AND_REPOSITORY]:
- `Model.Any` / `Model.AnyNoContext` are the structural bounds that gate every operation derivation: a `Schema.Schema.Any` carrying `fields` plus `insert`/`update`/`json`/`jsonCreate`/`jsonUpdate` schema properties — any owner satisfying this shape, class-built or hand-assembled, drives `makeRepository`; the `AnyNoContext` bound additionally constrains every variant property to `Schema.Schema.AnyNoContext` (`Schema<any, any, never>`), a STRUCTURAL ban on a non-`never` `R` in any variant — an owner with one service-requiring codec fails the constraint at the type level, not at provision time.
- `Model.makeRepository(Self, { tableName, spanPrefix, idColumn })` derives `insert`/`insertVoid`/`update`/`updateVoid`/`findById`/`delete` from the owner, each typed in its matching variant: `insert` takes `Self["insert"]["Type"]` and returns `Self["Type"]`, `findById`/`delete` take `Schema.Schema.Type<Self["fields"][Id]>`. The `idColumn` is constrained to `keyof Self["Type"] & keyof Self["update"]["Type"] & keyof Self["fields"]`, so a non-updatable or non-selectable id is a type error at the call.
- The id-key constraint partitions the markers by qualification: `Model.Generated` (present in `select`/`update`), `Model.GeneratedByApp`, and a bare `Schema.X` all satisfy `keyof Type & keyof update["Type"] & keyof fields`, while a column shaped to skip `update` (an insert-once marker, a `FieldOnly("insert")`) is structurally barred from anchoring lookup — the marker's variant set decides eligibility, not a flag.
- `makeDataLoaders(Self, { tableName, spanPrefix, idColumn, window, maxBatchSize? })` is the batched sibling of `makeRepository`: `insert`/`insertVoid`/`findById`/`delete` (no `update`), each returning a context-free `Effect<S["Type"], never, never>`, with the whole surface requiring `SqlClient | Scope` because the request-coalescing window owns a scoped resource — the `window` `DurationInput` and `maxBatchSize` are batching policy values, not call-site knobs.
- The two surfaces partition by requirement closure: `makeRepository` is `S extends Model.Any` (per-op `R` unions allowed, surfaced on each method) against `makeDataLoaders` is `S extends Model.AnyNoContext` (every variant `R` must already be `never`); the same owner drives both only when every codec resolves context-free, so a generative slot reading a service is admissible to `makeRepository` but silently disqualifies the batched path. An owner reaches `AnyNoContext` by providing the service at the leaf before declaration — a field codec built over a `Layer`-provided dependency stays `Schema<A, I, SomeService>` and propagates, so context closure is a property of the FIELD declarations, achieved by composing the requirement away at the leaf, never by erasing it at the operation; the gate reads the field map, so the only fix is a context-free field.

[OPERATION_REQUIREMENT_IS_A_PER_OP_UNION]:
- `Model.makeRepository` fuses requirements per operation by which variant codec it runs: `insert`/`insertVoid` declare `S["Context"] | S["insert"]["Context"]`, `update`/`updateVoid` declare `S["Context"] | S["update"]["Context"]`, so each write op carries the owner's SELECT-decode requirement (the returned row is decoded as `S["Type"]`) UNIONED with its own write-variant encode requirement — the operation's `R` is the sum of the codec it encodes the payload with and the codec it decodes the result with.
- `findById` declares `S["Context"] | Schema.Schema.Context<S["fields"][Id]>` — the owner decode union PLUS the id column's own decode requirement, because the lookup encodes the id through its leaf codec and decodes the row through the select codec; a branded id whose brand carries no service adds nothing, but an id whose codec requires a service surfaces that service on the read op alone.
- `delete` declares `Schema.Schema.Context<S["fields"][Id]>` with NO `S["Context"]` term — it encodes the id but never decodes a row, so the select requirement is structurally absent, the sharpest proof that operation `R` is derived from the codecs an operation actually runs, not a flat owner requirement copied onto every method.
- Every derived operation declares its error channel `never` and threads `SqlClient` as the sole ambient surface requirement (the dataloader surface adds `Scope`) — decode/encode `ParseError` is raised as a defect inside the operation, not surfaced as a typed `E`, so the owner-derived surface trades a typed parse failure for a defect and a caller wanting a typed `E` wraps the operation; the requirement union is the only channel the variant codecs widen, the error channel they do not.

[GROUP_COMPOSITION]:

```typescript
import { Model } from '@effect/sql';
import { Option, Schema } from 'effect';

const NodeId = Schema.Uint8ArrayFromSelf.pipe(Schema.brand('NodeId'));

class Leaf extends Model.Class<Leaf>('Leaf')({
    id: Model.UuidV4Insert(NodeId),
    kind: Schema.tag('Leaf'),
    weight: Model.JsonFromString(Schema.Struct({ value: Schema.Number })),
    archivedAt: Model.FieldOption(Model.Date),
    touchedAt: Model.DateTimeUpdateFromDate,
}) {}

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

const seed = (kind: 'Leaf' | 'Branch'): Leaf | Branch =>
    kind === 'Leaf'
        ? new Leaf({ weight: { value: 0 }, archivedAt: Option.none() })
        : new Branch({ fanout: true });
```

- One `Model.Union(Leaf, Branch)` is six discriminated wire/store surfaces: `Node` decodes the `select` union on `kind`, `Node.insert` the store-write union, `Node.jsonCreate` the external-create union — a third member is one positional argument and every projection absorbs it, the `kind` tag closing each union's decode.
- The construct payload names neither `id`, `touchedAt`, nor `kind`: `UuidV4Insert` mints the branded binary key through its `insert` overrideable's `generate`, `DateTimeUpdateFromDate` stamps via its current-time overrideable, and `Schema.tag` is itself a `HasDefault=true` property signature auto-filling its literal — all three fall into the optional-key arm of `Schema.Struct.Constructor`, leaving only the genuine inputs at the write `.make`.
- `NodeInsertWire` and `NodeStoreRow` are free indexed-access derivations off the one union: the external create body and the SQL insert row diverge in shape (`UuidV4Insert` absent from `jsonCreate`, present in `insert`; `weight` a string in `insert`, an object in `jsonCreate`) yet both descend from a single owner with no parallel interface.

[CONSTRUCT_ALGEBRA]:

```typescript
import { Model } from '@effect/sql';
import { DateTime, Option, Schema } from 'effect';

const NodeId = Schema.Number.pipe(Schema.brand('NodeId'));

class Entry extends Model.Class<Entry>('Entry')({
    id: Model.Generated(NodeId),
    kind: Schema.tag('Entry'),
    label: Schema.NonEmptyTrimmedString,
    note: Model.FieldOption(Schema.String),
    revisedAt: Model.DateTimeUpdate,
}) {
    get screaming(): string {
        return this.label.toUpperCase();
    }
}

class EntryView extends Schema.Class<EntryView>('EntryView')(Entry.json) {
    get age(): Option.Option<DateTime.Utc> {
        return Option.some(this.revisedAt);
    }
}

const read: Entry = new Entry({
    id: NodeId.make(1),
    label: '<value-a>',
    note: Option.none(),
    revisedAt: DateTime.unsafeNow(),
});
const toWrite = Entry.insert.make({ label: '<value-b>', note: Option.none() });
const forced = Entry.update.make({ id: NodeId.make(1), label: '<value-c>', note: Option.none(), revisedAt: Model.Override(DateTime.unsafeNow()) });

type ReadCtor = ConstructorParameters<typeof Entry>[0];
type InsertCtor = Parameters<typeof Entry.insert.make>[0];
```

- `new Entry({ id, label, note, revisedAt })` names `id` AND `revisedAt`: the class constructor is the SELECT shape, where `Generated`'s slot is the bare `NodeId` (required) and `DateTimeUpdate`'s slot is `Schema.DateTimeUtc` (required `DateTime.Utc`); only `kind` (the tag's `HasDefault` select slot) is elided — the read constructor is not the insert path and demands the persisted values.
- `Entry.insert.make({ label, note })` omits both `id` and `revisedAt`: the insert variant drops `Generated`'s `id` outright, and `DateTimeUpdate`'s insert slot is the `Overrideable` whose elidable key auto-mints the current time on encode — the same owner exposes a third constructor (`Entry.update.make`) that re-requires `id` because `Generated` rejoins the update variant.
- `Model.Override(DateTime.unsafeNow())` is the only admitted value for the overrideable `revisedAt` key on a write `.make`: its type is `(DateTime.Utc & Brand<"Override">) | undefined`, so an unbranded `DateTime.unsafeNow()` is rejected and the slot either omits (auto-stamp) or carries the brand (forced) — no unbranded write.
- `ReadCtor` and `InsertCtor`, both lifted by indexed access, are the construct-payload pair the owner derives — `ReadCtor` requires `id`/`revisedAt`, `InsertCtor` elides both — never restated as hand-written interfaces; adding a required column reshapes every constructor and breaks each existing site at compile time, the absorbed-growth proof.
- `EntryView extends Schema.Class(Entry.json)` promotes the json variant to a getter-bearing class: `Entry.json` (a `Schema.Struct`) is the legal fields argument, `EntryView` gains the `extend`/`transformOrFail` chaining the variant owner's static side lacks, and `screaming` lives on the read instance while `age` lives on the wire instance — two prototypes off one field map with no parallel schema.

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
        stampedAt: Model.DateTimeInsertFromNumber,
    },
    { identifier: '<owner-identifier>', title: '<owner-title>' },
) {}

type WireBody = Schema.Schema.Encoded<typeof Entry.json>;
type CreateBody = Schema.Schema.Type<typeof Entry.jsonCreate>;
type StoreRow = Schema.Schema.Encoded<typeof Entry.insert>;

const transit = Schema.parseJson(Entry.jsonCreate);
const contract = JSONSchema.make(Entry.jsonCreate, { target: 'openApi3.1' });
const bound = Schema.encodedBoundSchema(Entry.json);
```

- `WireBody` carries `payload` under `"<external-name>"` (the `fromKey` rename surfacing in the encoded view alone) and omits `secret` (dropped from every json variant by `Model.Sensitive`); `StoreRow` keeps the canonical `payload` key as a `string` (the `JsonFromString` database leaf) and `secret` as the unredacted primitive (the `Redacted` encode-unwrap) — three columns project three different wire shapes from one declaration with no parallel DTO.
- `secret` proves the two orthogonal axes stacked: `Model.Sensitive` decides VARIANT MEMBERSHIP (erased from json bodies), `Schema.Redacted` decides DECODED REPRESENTATION (the select/insert/update interior is `Redacted<string>` while the stored leaf is plain text) — neither recoverable from the other, and a new wire-erased-but-store-redacted column lands as one such stacked field with every projection absorbing it.
- `transit` is the request-body codec whose encoded leaf is `string` — decode parses the inbound JSON straight into `CreateBody`, encode stringifies it; `contract` is the OpenAPI document derived off the same variant; `bound` retains the `NonEmptyTrimmedString` emptiness rejection at the wire boundary — three downstream wire surfaces, each a derivation off one variant of one owner, the next variant or column reshaping all three at compile time.

[DERIVED_FAMILY_FUSION]:

```typescript
import { Model } from '@effect/sql';
import { Arbitrary, Equal, HashSet, Schema } from 'effect';

const NodeId = Schema.Uint8ArrayFromSelf.pipe(Schema.brand('NodeId'));

class Entry extends Model.Class<Entry>('Entry')({
    id: Model.UuidV4Insert(NodeId),
    label: Schema.NonEmptyTrimmedString,
    note: Model.FieldOption(Schema.String),
    payload: Model.JsonFromString(Schema.Struct({ region: Schema.NonEmptyTrimmedString })),
    secret: Model.Sensitive(Schema.Redacted(Schema.NonEmptyTrimmedString)),
    touchedAt: Model.DateTimeUpdate,
}) {
    get loud(): string {
        return this.label.toUpperCase();
    }
}

type WireBody = Schema.Schema.Type<typeof Entry.json>;

const rowEq = Schema.equivalence(Entry);
const wireEq = Schema.equivalence(Entry.json);
const insertGen = Arbitrary.make(Entry.insert);
const isWire = Schema.is(Entry.json);

const dedup = (rows: ReadonlyArray<Entry>): HashSet.HashSet<Entry> => HashSet.fromIterable(rows);
const sameRow = (a: Entry, b: Entry): boolean => Equal.equals(a, b) && rowEq(a, b);
const sameOnWire = (a: WireBody, b: unknown): boolean => isWire(b) && wireEq(a, b);
```

- `rowEq` and `Equal.equals` agree on the read instance — the class fuses structural `Equal`/`Hash` so `HashSet.fromIterable` dedups rows by column value with no key function, and `loud` (a body getter) is invisible to both because equality reads the declared field set, not the prototype.
- `wireEq` is a STRUCTURALLY DIFFERENT equivalence off the same owner: `secret` is absent from it entirely (`Model.Sensitive` drops the column from json variants), `payload` compares by the parsed object (`Model.JsonFromString`'s json-variant interior), and `touchedAt` by the ISO-string-backed `DateTime.Utc` — the wire-body comparator and the read-instance comparator diverge per column with no parallel declaration.
- `insertGen` generates only the insert-variant columns — `id` absent (the `UuidV4Insert` constructor key elides), `touchedAt` absent (the `DateTimeUpdate` insert slot is an elided `Overrideable`), `secret` present as the unredacted primitive — so a property test over the store-write payload is one indexed access, and adding a column reshapes `insertGen`, `rowEq`, and `wireEq` at once with every consumer breaking loudly or absorbing it silently.
- `isWire` narrows `unknown` to `WireBody` synchronously and composes with `wireEq` in `sameOnWire` — it is NOT the admission seam, answering structural membership over `Schema.Schema.Type<typeof Entry.json>` with no decode pass; the inbound raw body still crosses through `Schema.decodeUnknown(Entry.jsonCreate)`, the guard reserved for narrowing a value already on the decoded side before the variant equivalence runs.

[COMPOSITION_STACK]:

```typescript
import { Model } from '@effect/sql';
import { DateTime, Option, Schema } from 'effect';

const NodeId = Schema.Number.pipe(Schema.brand('NodeId'));

const Audit = Model.Struct({
    createdAt: Model.DateTimeInsertFromDate,
    revision: Model.Generated(Schema.Number),
});

class Entry extends Model.Class<Entry>('Entry')({
    id: Model.Generated(NodeId),
    label: Schema.NonEmptyTrimmedString,
    audit: Audit,
}) {}

class Wider extends Model.Class<Wider>('Wider')({
    ...Model.fields(Entry),
    kind: Schema.tag('Wider'),
    note: Model.FieldOption(Schema.String),
}) {}

class Wire extends Schema.Class<Wire>('Wire')(Entry.json) {
    get loud(): string {
        return this.label.toUpperCase();
    }
}

const transport = Schema.extend(Entry.json, Schema.Struct({ etag: Schema.String }));

const read = new Entry({ id: NodeId.make(1), label: '<value-a>', audit: { createdAt: DateTime.unsafeNow(), revision: 0 } });
const stored = Wider.insert.make({ label: '<value-b>', note: Option.none(), audit: { createdAt: undefined } });

type WiderInsert = Schema.Schema.Type<typeof Wider.insert>;
type WireBody = Schema.Schema.Encoded<typeof transport>;
```

- `Wider` spreads `Model.fields(Entry)` to inherit the FULL variant algebra: `id` stays dropped from `Wider.insert` (the `Generated` marker survives the raw-map spread), `audit.revision` stays a select-only nested column, and `kind`/`note` land as new markers — spreading `Entry.fields` instead would have re-admitted `id` as a required insert column, the variant-shape boundary deciding which reuse path is correct.
- `WiderInsert` omits `id` and carries the nested `audit` whose `createdAt` is an elided-at-make overrideable yet a required nested key at the constructor — the two construct surfaces of one owner diverge precisely because `RequiredKeys` elides parent-level defaults but not nested-struct defaults, the trap the nested `{ createdAt: undefined }` spelling resolves.
- `Wire` promotes the json variant to a getter class and `transport` fuses an `etag` onto the same variant: `WireBody` is the encoded json columns plus `etag`, `Wire` the prototyped json instance with `loud` — two independent owners off one variant, neither carrying a variant algebra, the next transport column landing as one more `Schema.extend` member and the next read getter as one more class body line.

[REQUIREMENT_THREADING]:

```typescript
import { Model } from '@effect/sql';
import { VariantSchema } from '@effect/experimental';
import { Context, Effect, Option, Schema } from 'effect';

class Minter extends Context.Tag('Minter')<Minter, { readonly next: Effect.Effect<string> }>() {}

const StampWithMinter: VariantSchema.Overrideable<string, string, Minter> = VariantSchema.Overrideable(
    Schema.String,
    Schema.String,
    {
        generate: (forced: Option.Option<string>) =>
            Option.match(forced, {
                onNone: () => Effect.flatMap(Minter, (m) => m.next),
                onSome: (value: string) => Effect.succeed(value),
            }),
    },
);

const NodeId = Schema.Number.pipe(Schema.brand('NodeId'));

class Entry extends Model.Class<Entry>('Entry')({
    id: Model.Generated(NodeId),
    label: Schema.NonEmptyTrimmedString,
    note: Model.FieldOption(Schema.String),
    ticket: Model.Field({ select: Schema.String, insert: StampWithMinter, update: StampWithMinter, json: Schema.String }),
    touchedAt: Model.DateTimeUpdateFromDate,
}) {}

type ReadR = Schema.Schema.Context<typeof Entry>;
type InsertR = Schema.Schema.Context<typeof Entry.insert>;

const wired = (
    repo: Effect.Effect.Success<ReturnType<typeof Model.makeRepository<typeof Entry, 'id'>>>,
): Effect.Effect<Entry, never, Minter> =>
    repo.insert(Entry.insert.make({ label: '<value-a>', note: Option.none() }));
```

- `StampWithMinter` is a `VariantSchema.Overrideable<string, string, Minter>` whose `generate` reads the `Minter` service, so `Minter` rides into `Entry.insert`'s and `Entry.update`'s `Context` (the slots holding the overrideable) while `Entry`'s select slot (`Schema.String`, `R = never`) keeps `ReadR` equal to `never` — `InsertR` is `Minter`, the read and write requirement unions diverging off one field map.
- `Model.makeRepository(Entry, { idColumn: 'id', ... })`'s `insert` op declares `Entry["Context"] | Entry["insert"]["Context"]` = `never | Minter` = `Minter`, so `wired` surfaces exactly `Minter` as the remaining requirement — the per-op requirement is the union of the encode codec's `R` and the decode codec's `R`, derived with zero hand-written threading, and `Minter` is provided once at the composition root.
- The same owner is barred from `Model.makeDataLoaders` because `Entry` fails `AnyNoContext` (the `ticket` insert/update slots carry `Minter`); dropping the service — fully applying the codec before declaration so `ticket`'s slots are `Schema<string, string, never>` — is the only path that admits the batched surface, the requirement closure being a field-level property the gate reads off the variant slots.
- A second service-requiring column lands as one more `Model.Field` with its overrideable's `R`; the variant `Context` union absorbs it and every `makeRepository` write op widens its requirement to the new union with no operation respelled, while `findById` (decoding the select codec and the id leaf) stays unaffected unless the new column joins the select slot — the requirement diff of the next feature is one leaf `R` flowing into exactly the ops whose codecs touch it.
