# Struct-Class Owner

[OWNER_SHAPE]:
- `class Self extends Schema.Class<Self>("Identifier")({ ...fields })` is one declaration yielding six fused surfaces: the nominal class type, a runtime value-constructor (`new Self`), a `Schema<Self, Encoded, R>` codec, structural `Equal`/`Hash`, an `Arbitrary`, and a `Pretty` — the chain `interface X` + `const codecX` + `type XT = typeof` + `class XImpl` collapses to this line.
- The `<Self>` self-generic is mandatory and load-bearing: omitting it resolves the return type to the literal string `Missing \`Self\` generic - use \`class Self extends Class<Self>()({ ... })\`` (a `MissingSelfGeneric` template-literal type), so the error surfaces at the class header, not at a use site.
- `Schema.Class` is a curried two-call form: `Class<Self>(identifier)` returns the fields-taking function; the identifier string is positional and required, distinct from the trailing `annotations` argument.
- The generated class extends `Data.Class` as its prototype base, inheriting structural equality and hashing — two instances with equal fields satisfy `Equal.equals`, never reference identity; the structural base is unconditional, not an opt-in mixin.
- `Class<Self, Fields, I, R, C, Inherited, Proto>` constructs its instance type to `Struct.Type<Fields> & Inherited & Proto` — three orthogonal slots: `Fields` is the decoded field record, `Inherited` is the parent class on `.extend`, `Proto` is the prototype-supplied behavior; class-body methods and getters ride `Proto` and ARE part of the instance type, while the codec `I = Struct.Encoded<Fields>` never serializes them.
- `Proto` is `{}` for a plain `Schema.Class` and `cause_.YieldableError` for `TaggedErrorClass` — the throwable/`yield*`-able capability rides the `Proto` slot, not the field record, so an error owner is directly yieldable while its wire codec stays the field record alone.
- `Self.identifier` re-exposes the identifier string statically; `Self.ast` is an `AST.Transformation` (encoded-side ↔ instance), never a bare type-literal, because the class is a codec from its encoded form into instances.

[INSTANTIATION]:
- `new Self(props)` and `Self.make(props)` run the identical path — `make` is the `this`-polymorphic static `make<C extends new (...args) => any>(this: C, ...args): InstanceType<C>` invoking `new this` — both validate; `make` is the pipe-friendly static, not a validation bypass, and every `.extend` subclass inherits a `make` already typed to its own constructor parameters.
- The constructor parameter object is `void`-able exactly when `RequiredKeys<C> extends never`: every field defaulted or optional makes `new Self()` legal with no argument, and one required field forces the props object; across `.extend` the constructor type accrues as `C & Struct.Constructor<NewFields>`, so adding one required field to a previously all-defaulted subclass flips `new Sub()` from legal to a compile error at every call site, the widening surfacing as a loud break rather than a silent admission.
- `new Self(props, { disableValidation: true })` skips the `ParseResult.validateSync` pass — the second argument is `MakeOptions = boolean | { readonly disableValidation?: boolean }`, a bare `true` shorthand for `{ disableValidation: true }`; reserve it for values already proven valid, as it admits an ill-formed instance the refinement would reject.
- `disableValidation` suppresses construction-side validation alone; the decode path through `Schema.decodeUnknown(Self)` has no such argument on the value, so a wire value never bypasses the invariant — trust is grantable only at the in-process constructor seam, never at the admission boundary.
- Construction order is fixed: shallow-copy props, `delete props["_tag"]` for tagged kinds, `lazilyMergeDefaults`, then validate — so `withConstructorDefault` fills missing keys before the validator runs, and a field passed explicitly as `undefined` triggers its constructor default (the merge gate is `out[key] === undefined`).
- The constructor validates the *type* side (decoded shape), not the encoded side; decoding from wire input is a separate act through the same owner's codec, never the constructor.

[FIELD_ALGEBRA]:
- A field value is a `Schema.All | PropertySignature.All` (`Struct.Field`); every modifier returns a `PropertySignature`, so modifiers compose by `.pipe` — `Schema.String.pipe(Schema.propertySignature, Schema.fromKey("source_key"))` — because `PropertySignature` is `Pipeable`.
- `Schema.optional(s)` makes the key absent-or-present on both type and encoded sides with `| undefined` admitted on each; it is the unconfigured optional and adds no `exact`/`default`/`Option` semantics.
- `Schema.optionalWith(s, opts)` is the configured optional whose `OptionalOptions` shape is a closed discriminated union of legal combinations, not free-form flags: `{ exact?: true; nullable?: true }`, `{ default: LazyArg<A>; exact?; nullable? }`, and four `{ as: "Option"; ... }` rows; illegal mixes (e.g. `default` with `as`) are rejected at the type level because each is a separate union member.
- `optionalWith({ default: () => v })` flips the *type-side* token from `?:` to `:` — the field becomes required-with-default on the instance while staying optional on the encoded side; the decoded value is never `undefined`.
- `optionalWith({ as: "Option" })` lifts the type-side value to `Option<A>` and keeps the encoded side optional; `onNoneEncoding: () => Option.some(undefined)` controls whether a `None` round-trips as a present-`undefined` key or an absent key.
- `optionalWith({ exact: true })` removes `| undefined` from the admitted set: the key is either absent or a real value, never explicit-`undefined`; without `exact`, `undefined` is a legal present value.
- `Schema.fromKey(encodedKey)` remaps only the *encoded* property key, leaving the instance property name canonical — the boundary rename lives inline on the field, never as a parallel mapping object; it composes after `propertySignature` and preserves the `HasDefault` flag through the rewrite.
- `withConstructorDefault(() => v)` supplies a value only at construction (`new`/`make`), leaving decoding untouched; `withDecodingDefault(() => v)` supplies it only at decode-from-encoded; `withDefaults({ constructor, decoding })` supplies both — three distinct seams, not one knob, because construction and decoding are separate admission events.
- `optionalToOptional`/`optionalToRequired`/`requiredToOptional` are the `Option`-threaded property transforms: each takes `from`/`to` schemas plus `decode`/`encode` over `Option`, where `Option.none()` denotes a missing key on the relevant side — the mechanism that turns key-presence into a typed `Option` rather than a sentinel.

[FIELD_SOURCE_AND_BUNDLES]:
- `Self.fields` re-exposes the original `PropertySignature` objects, not their resolved schemas, as a static `{ readonly [K in keyof Fields]: Fields[K] }`, so spreading it carries each field's `optionalWith`, `withConstructorDefault`, `fromKey`, and nominal-brand modifiers intact — `class Sub extends Schema.Class<Sub>("Sub")({ ...Parent.fields, extra: Schema.Number })` derives a sibling owner whose shared keys reshape automatically on a parent modifier change, where a transcribed record silently drifts.
- A reusable field bundle is declared once at the vocabulary altitude as a `Schema.Struct` or `Schema.TaggedStruct` and consumed by many class owners by passing the struct value itself to `.extend` — `class Sub extends Parent.extend<Sub>("Sub")(SharedBundle)` — because `.extend` accepts `NewFields | HasFields<NewFields>` where `HasFields<F> = Struct<F> | { readonly [RefineSchemaId]: HasFields<F> }`; a field set shared by three owners is one struct, never three transcribed records or a `SharedBundle.fields` spread at each site.
- The `HasFields` arm `{ [RefineSchemaId]: … }` admits a `Schema.Struct.pipe(Schema.filter(...))` refinement as the field source, so a validated field bundle (struct plus invariant) is extended into a class and the refinement rides along — the bundle carries its own predicate, not a separate validation pass at the owner.

[PROJECTION_VS_CODEC_COLLAPSE]:
- `Schema.pick(...keys)`, `Schema.omit(...keys)`, `Schema.pluck(key)`, `Schema.partial`, `Schema.partialWith({ exact: true })`, and `Schema.required` over a `Class` all return `SchemaClass<A', I', R>` — a bare codec view; the `new`/`make` value-constructor, the `Data.Class` `Equal`/`Hash` base, the `_tag` static, and every instance method are gone, because `SchemaClass` is `Schema` plus `make` only, never the owner.
- `Schema.Struct(fields)` returns a `Struct<Fields>` carrying its own `.fields`, `.records: readonly []`, a validating `make`, and instance methods `.pick(...keys): Struct<Pick<Fields, …>>` / `.omit(...keys): Struct<Omit<Fields, …>>` — struct-level projection preserves the struct (with `make` and field source), where the top-level combinators collapse it; reach for the instance method when the projection must stay an owner.
- `Schema.Struct(fields, indexSignature)` returns `TypeLiteral<Fields, Records>`, which exposes `.fields`, `.records`, and `make` but no `.pick`/`.omit` — admitting one index signature forfeits struct-level projection, so a struct that must stay projectable keeps its open-ended keys as a `Schema.Record` field rather than a struct index signature.
- A projection meant to remain instantiable derives field-side from `Self.fields` into a new `Schema.Class`/`Schema.Struct`; a projection used only as a transient decode/encode contract uses the top-level combinator and accepts the codec collapse — the choice is owner-retention versus throwaway view, never style.
- `Schema.asSchema(self)` erases an owner's `api interface` view down to `Schema<Type, Encoded, Context>` — used only to force the structural schema where a heterogeneous registry constrains its entries to `Schema.Any`; it is a viewing cast at the registry boundary, never a step that should precede `new`/`make`.

[TAGGED_KINDS]:
- `Schema.TaggedClass<Self>()("Tag", fields)` injects `_tag: tag<"Tag">` as the first field and exposes `_tag` as a static readonly discriminant; the tag is a `withConstructorDefault`-backed literal, so `new Self({ ...fieldsWithoutTag })` never passes `_tag` and the constructor deletes any supplied `_tag` before merging.
- `Schema.TaggedStruct("Tag", fields)` is the value-only sibling: a `Struct<{ _tag: tag<Tag> } & Fields>` with no class, no `Equal`/`Hash` base, no `new`; `Self.make({ ...fields })` omits `_tag` because `tag` carries a constructor default. Reach for it when no instance methods or nominal identity are needed.
- `Schema.TaggedError<Self>()("Tag", fields)` extends `Data.Error` (a `YieldableError`): an instance is directly `yield*`-able inside `Effect.gen` and flows in the typed `E` channel — it is the schema-backed failure owner, fusing codec, tag, and throwable in one declaration.
- `TaggedError` sets `Base.prototype.name = tag` and synthesizes a `message` getter only when no `message` field is declared; declaring a `message: Schema.String` field overrides the synthesized getter, and the synthesized form renders the field record — so a custom `get message()` method on the class body is the controlled override.
- The `_tag` literal travels as a compiler-checked symbol (`Self._tag` static, `instance._tag` literal type), never a restated string — exhaustive dispatch over a tagged-class union keys off this member with no string literals at the call sites.

[TAGGED_REQUEST_FUSION]:
- `Schema.TaggedRequest<Self>()("Tag", { failure, success, payload })` fuses the request's `_tag`, its payload fields, and its `success`/`failure` result schemas into one owner exposing static `success`/`failure` schemas and the `Serializable` machinery — the options object is the closed triple `{ failure; success; payload }`, three schema slots in one argument, where `payload` is the `Struct.Fields` record decoded as the request's own fields and `success`/`failure` are full schemas, so the result codecs live inside the request owner, deleting the sibling response schema a bare `Request` forces beside it.
- The generated `TaggedRequestClass` extends `Class<Self, Payload, Struct.Encoded<Payload>, Struct.Context<Payload>, Struct.Constructor<Omit<Payload, "_tag">>, TaggedRequest<...>, {}>` — the `TaggedRequest` interface threaded through the `Inherited` slot is `Request.Request<SuccessType, FailureType> & SerializableWithResult<...>`, so an instance is simultaneously a request value (its `Success`/`Failure` parameters are the resolved-outcome contract) and its own serializer; the `Class` base supplies `new Self`/`Equal`/`Hash`, the `Request` base the success/failure outcome identity.
- The constructor argument is `Struct.Constructor<Omit<Payload, "_tag">>` over the injected `_tag: tag<Tag>`, so the payload-encoded type `I` is `Struct.Encoded<Payload>` — the wire payload is the field record minus the merged tag's constructor obligation, the same shape `serialize` produces.
- `TaggedRequest.Any`/`.All` and `SerializableWithResult.Any`/`.All` are the upper bounds for a heterogeneous request registry: a router over many request kinds constrains its handler map to `TaggedRequest.Any`, recovering each kind's success/failure types per entry through the `WithResult.Success`/`WithResult.Failure` extractors rather than a parallel response-type table.

[REQUEST_TRAIT_SYMBOLS]:
- The serialization schema rides a unique symbol, not a named property: `Serializable<A, I, R>` is `{ readonly [symbolSerializable]: Schema<A, I, R> }`, so the request's payload codec is keyed by `Schema.symbolSerializable` and cannot collide with a domain field named `schema` or `serializable`.
- `WithResult<Success, SuccessEncoded, Failure, FailureEncoded, ResultR>` is `{ readonly [symbolWithResult]: { readonly success: Schema<...>; readonly failure: Schema<...> } }` — both result codecs hang off one symbol slot sharing a single `ResultR` requirement, so success and failure decode under the same context, never two divergent `R`s.
- `SerializableWithResult` is the intersection `Serializable<A, I, R> & WithResult<...>` carrying both symbols — the payload-codec symbol and the result-pair symbol coexist on one instance, which is why a `TaggedRequest` self-serializes its payload AND its outcome from one value.
- The trait is structural, not declared: any object bearing `[symbolSerializable]`/`[symbolWithResult]` satisfies it, so `asSerializable`/`asWithResult`/`asSerializableWithResult` re-type an arbitrary procedure into the trait without a wrapper — the cast is the trait, never an adapter shell.

[REQUEST_REQUIREMENT_FUSION]:
- `SerializableWithResult.Context<P>` collapses the payload requirement `SR` and the result requirement `RR` into one set `SR | RR` — a request whose `payload`, `success`, or `failure` schema pulls a `Context.Tag` surfaces that requirement once on the request's own context, so providing it once at the composition root satisfies decode of payload and both outcomes.
- `TaggedRequest`'s `ResultR` slot is exactly `Schema.Context<Success> | Schema.Context<Failure>` — the result requirement is the union of the two result codecs' contexts, never a third independent `R`, so a context-bearing success schema and a pure failure schema yield a result codec requiring only the success context.

[REQUEST_RESULT_CODEC_ACCESS]:
- `Schema.successSchema(self)` and `Schema.failureSchema(self)` lift the two result codecs off the `symbolWithResult` slot as standalone `Schema<SA, SI, R>` / `Schema<FA, FI, R>` — the read shape, not a re-declaration; the request's outcome codecs are recoverable from any `WithResult` value, not only the originating class.
- `Schema.serializableSchema(self)` lifts the payload codec off `symbolSerializable` as `Schema<A, I, R>` — the same codec `new Self`/`decodeUnknown(Self)` use, so the request payload has one source of truth read three ways.
- `Schema.exitSchema(self)` derives a single `Schema<Exit<SA, FA>, ExitEncoded<SI, FI, unknown>, R>` from the success/failure pair — one codec for the whole outcome, fused from two result schemas, never two parallel exit codecs.

[REQUEST_EXIT_WIRE_SHAPE]:
- `ExitEncoded<A, E, D>` is the closed two-arm union `{ _tag: "Failure"; cause: CauseEncoded<E, D> } | { _tag: "Success"; value: A }` — the wire form of an outcome is itself `_tag`-discriminated, so a serialized result round-trips through the same tag-keyed admission as any tagged owner.
- The failure arm carries `CauseEncoded<E, D>`, not a bare error — the full interruption/die/sequential cause tree serializes, with the `D` defect slot carrying non-`E` failures; `exitSchema` fixes `D` to `unknown` (the `Schema.Defect` codec), so an unmodeled defect still round-trips as an opaque encoded cause rather than being dropped.
- `Schema.serializeExit(self, exit)` encodes an `Exit<SA, FA>` to `ExitEncoded<SI, FI, unknown>` and `Schema.deserializeExit(self, value)` decodes `unknown` back to `Exit<SA, FA>`, both returning `Effect<_, ParseError, R>` — the outcome crosses the wire as one value carrying success-or-cause, decoded under the request's own context.

[REQUEST_ROUNDTRIP_OPERATIONS]:
- `Schema.serialize(self)` returns `Effect<I, ParseError, R>` encoding the request instance to its payload-encoded form; `Schema.deserialize(self, value)` returns `Effect<A, ParseError, R>` decoding `unknown` back into the request instance — the request encodes itself with no external codec argument, the payload schema travels on the value.
- `Schema.serializeSuccess`/`deserializeSuccess` and `Schema.serializeFailure`/`deserializeFailure` are the per-arm outcome codecs: `serializeSuccess(self, sa)` yields `Effect<SI, ParseError, R>`, `deserializeFailure(self, value)` yields `Effect<FA, ParseError, R>` — success and failure each round-trip independently when only one arm crosses, the full `Exit` codec used only when the outcome is unknown at the seam.
- Every round-trip operation carries data-first and data-last overloads (`(self, value)` and `(value) => (self) => ...`), so each composes pipe-first or applies directly; the request value is always the trait carrier, the second argument the raw material.

[EXTENSION_AND_WIDENING]:
- `Self.extend<Sub>("SubId")({ ...newFields })` produces a subclass `Class<Sub, Fields & NewFields, I & Struct.Encoded<NewFields>, R | Struct.Context<NewFields>, C & Struct.Constructor<NewFields>, Self, Proto>` — the parent class becomes `Inherited` (`Self`), the prototype chain and methods survive, `Proto` is preserved (a subclass of a `TaggedError` stays yieldable), and `instanceof Parent` holds on every `Sub` instance; it is structural widening, not re-declaration.
- A parent-versus-new key collision throws `getASTDuplicatePropertySignatureErrorMessage` at class creation — overlapping a parent field name in `.extend` is a hard throw, not a silent override, so widening is total or it fails loudly.
- The top-level `Schema.extend(that)` is the schema-combinator widening with broader reach than `Class.extend`: it fuses a struct with another struct, with a `Schema.Record` index signature, with a union of structs, or with a refinement/suspend of a struct — overlapping fields are admitted only when the overlap itself supports extension, where `Class.extend` instead throws the duplicate-property error on any field-name collision.
- `Self.transformOrFail<Sub>("SubId")(newFields, { decode, encode })` widens by an effectful transform on the *type* side: `decode` sees `Struct.Type<Fields>` (the parent's decoded instance shape, plus new encoded fields) and yields `Struct.Type<Fields & NewFields>`; `Self.transformOrFailFrom<Sub>("SubId")(...)` instead feeds `decode` the encoded side `I` and yields `I & Struct.Encoded<NewFields>` — the suffix selects whether the transform runs before or after the parent's decode, and that choice decides which representation the new fields are derived from.
- Each transform-widening result is still a `Class<Sub, Fields & NewFields, …, Self, Proto>` — a full owner with `new`/`make`/methods — so a computed field added by `transformOrFail` lands as one widening row and every consumer of the parent type keeps type-checking against the narrowed view.

[RECURSION]:
- A self-referential field uses `Schema.suspend(() => Self)` inside the field record to defer the owner reference past the class declaration; the annotation `Schema<A, I, R>` type parameters are supplied explicitly at the suspend site when the checker cannot infer the recursive type, breaking the otherwise-circular initialization.
- `suspend` is the only legal forward reference to the enclosing class within its own fields — a bare `Self` reference in the field record reads `Self` before the class binding exists and fails; the thunk closes over the eventual binding.

[CLASS_INVARIANT]:
- `Schema.Class`/`TaggedClass`/`TaggedError` take `fieldsOr: Fields | HasFields<Fields>` where `HasFields<F> = Struct<F> | { readonly [RefineSchemaId]: HasFields<F> }`, so a `Schema.Struct({...}).pipe(Schema.filter(pred))` is a legal first argument — the owner is born from a refined struct, never a plain field record plus a separate post-construction check.
- `refine<A, From>` (the `Schema.filter` result type) declares `readonly [RefineSchemaId]: From` precisely so a filtered struct satisfies the `HasFields` arm; the class constructor unwraps that discriminant to recover the underlying `.fields`, so `Self.fields` still re-exposes the per-field `PropertySignature` objects after the invariant wraps them.
- The cross-field invariant rides the owner declaration as a definition-time aspect, gating every admission event the owner generates — `new Self`, `Self.make`, and `Schema.decodeUnknown(Self)` all run the refinement, because the constructor validates the type-side AST and the refinement is a node on that AST, not a wrapper around the codec; a refined struct whose every field is defaulted yields a `void`-able `new Self()` that still runs the refinement over the defaulted instance, the invariant never skipped for the empty-argument form.
- A class whose validity spans two or more fields (`upper >= lower`, `endsAt > startsAt`, `parts sum to total`) collapses the spelling `class X` + free `assertValid(x)` + `if (!ok) throw` into one `Schema.filter`-refined struct passed as `fieldsOr`; the free predicate, the throw site, and the call-site discipline that remembers to invoke it are the exact spellings the refined owner deletes.
- A refinement whose predicate is a type guard (`(a): a is B`) narrows the owner's type to `C & B`, so a class built from a guard-refined struct carries the narrowed type on every instance and projection — the invariant that narrows the type and the invariant that only rejects share one `Schema.filter` surface, the guard arm narrowing where the predicate arm merely gates.
- `Schema.filter` annotations carry `schemaId` and `jsonSchema`, so the invariant projects into the owner's JSON Schema and is recoverable by `schemaId` for a registry keyed on refinement identity — the cross-field gate is not opaque to downstream projection, it travels as a typed node every derived surface reads.

[INVARIANT_ISSUE_ALGEBRA]:
- The `Schema.filter` predicate returns `FilterReturnType = FilterOutput | ReadonlyArray<FilterOutput>` where `FilterOutput = undefined | boolean | string | ParseResult.ParseIssue | FilterIssue` — `undefined`/`true` passes, `false` reports the annotation's default message, a bare `string` reports that message, and a `FilterIssue` targets a path.
- `FilterIssue` is `{ readonly path: ReadonlyArray<PropertyKey>; readonly message: string }`, so a cross-field invariant attributes its failure to a specific field rather than the whole owner — returning `{ path: ['upper'], message: '<upper-below-lower>' }` lands the issue on the `upper` key the consumer's form binds to, not on the anonymous record.
- Returning `ReadonlyArray<FilterOutput>` emits multiple path-targeted issues from one predicate pass, so a single invariant reports every violated field at once under `errors: "all"`; the array form is the one-pass multi-field report, never a sequence of sibling single-field filters chained by `.pipe`.
- A refinement returning a `ParseResult.ParseIssue` (the structured issue, not a `FilterIssue`) lets the invariant emit a `Pointer`/`Composite` issue tree identical to a nested decode failure, so a cross-field invariant and a field-level decode failure converge on one issue algebra the consumer's `ParseError` handler walks uniformly.
- A predicate needing an injected dependency to decide validity is `Schema.filterEffect(f)` whose `f` returns `Effect<FilterReturnType, never, FD>` and which extends `transformOrFail<S, SchemaClass<Schema.Type<S>>, FD>` — the requirement `FD` joins the owner's `R`, so the context-dependent invariant surfaces its requirement on the owner's own context and is provided once at the composition root.

[DEFINITION_TIME_ANNOTATION_ASPECTS]:
- `ClassAnnotations<Self, A>` is `Annotations.Schema<Self> | readonly [Annotations.Schema<Self> | undefined, (Annotations.Schema<Self> | undefined)?, Annotations.Schema<A>?]` — the object form annotates the type side; the three-slot tuple targets the type-side AST, the encoded↔type transformation AST, and the encoded-side AST independently, because the owner is a three-AST codec and each AST carries its own annotation map.
- Class annotations are baked into `Self.ast` and cached per class in a `WeakMap` (`astCache`), so the aspects supplied at the header ride the owner declaration and propagate to every derived projection without a separate annotation pass.
- The aspect split is by whether the aspect's type names `Self`: `parseOptions`, `parseIssueTitle`, `title`, `message`, and `decodingFallback` are typed against the issue or the encoded value, never `Self`, so they sit inline at the class header; `equivalence`, `arbitrary`, and `pretty` are typed `Equivalence<Self>`/`LazyArbitrary<Self>`/`Pretty<Self>`, so any value supplied there forces the checker to resolve `Self` inside its own base expression and reports `TS2310: Type 'Self' recursively references itself as a base type` (with `TS2506` on the base) — these aspects are unsuppliable at the class header and instead ride the `fieldsOr` struct via `Schema.annotations({ equivalence, pretty })` over the field-record type before the class wraps it, the class inheriting them through the annotated source.
- `parseOptions` on the owner declaration fixes per-owner parse behavior as a baked policy value: `errors: "first" | "all"`, `onExcessProperty: "ignore" | "error" | "preserve"`, `propertyOrder: "none" | "original"`, `exact: true` — so an owner that must report every field failure or reject excess keys declares it once at definition, never threaded as an override argument at each decode call site.
- `decodingFallback: (issue: ParseIssue) => Effect<A, ParseIssue>` rides the owner so a decode failure recovers to a default value or re-raises under the owner's own policy — the recovery is a definition-time aspect of the owner, not a `catch` at every consumer; the fallback's `Effect` lets recovery itself fail with a refined issue.
- `parseIssueTitle: (issue: ParseIssue) => string | undefined` and `message: (issue) => string | Effect<string> | {...}` shape the owner's failure rendering at declaration, taking the issue rather than the instance, so the human-facing message for an invariant violation lives with the invariant, never reconstructed by the consumer inspecting the raw issue tree.
- `equivalence`, `arbitrary`, and `pretty` supplied on the field-record source override the generated comparison, sampler, and renderer respectively for the codec-derived projections (`Schema.equivalence`, `Arbitrary`, `Pretty`), the structural/generated default standing only when no annotation overrides it; they sit on the struct source rather than the class header precisely because their `Self`-typed signatures trip the recursive-base error there.
- `Self.annotations(a)` returns a fresh `SchemaClass<Self, I, R>` — a codec view stripped of `new`/`make` and the instance base — so an aspect meant to ride the owner is supplied at the declaration's trailing `ClassAnnotations` argument; bolting it on with `.annotations` after declaration yields a non-instantiable view, the trap that splits the aspect from the owner it should define.
- Both the refined `fieldsOr` and the `ClassAnnotations` argument coexist on one declaration: the struct refinement carries the cross-field gate, the annotation tuple carries the parse policy, the recovery, the message, and the semantic equality — one owner statement fuses the invariant, the failure rendering, and the comparison the family needs, where the loose spelling scatters them across a validator, a try/catch, a message map, and a custom equals.
- Both aspects survive `.extend`/`.transformOrFail`: the subclass's `Inherited` slot carries the parent prototype, the parent's refinement stays on the inherited AST, and the subclass supplies its own `ClassAnnotations` for the widened shape — widening a refined owner never strips the parent invariant, and a subclass adding a field that participates in the invariant restates the refinement on its own refined `fieldsOr`.

[TAGGED_UNION_FAMILY]:
- `Schema.Union(MemberA, MemberB, MemberC)` over tagged-class owners is one declaration yielding the family's codec, its static type `Schema.Schema.Type<typeof Family>` (the bare union of member instance types, methods included), and `Schema.Schema.Encoded<typeof Family>` (the union of each member's encoded record) — the chain `type X = A | B | C` + `const xCodec = ...` + per-member `is`-guards collapses to this line.
- The single-member call `Union<Member>(member): Member` returns the member unchanged and the no-arg call `Union(): typeof Never` returns `Never`, so a family generated by mapping over a variable member list degenerates correctly at one or zero cases without a branch — the same combinator owns the empty, singleton, and N-ary family.
- `Family.members` re-exposes the member tuple as `Readonly<Members>` statically, so a registry, a per-member projection map, or a fan-out decode iterates the actual member schemas off the owner — the member list is read from the union, never transcribed beside it as a parallel `[A, B, C]` array.
- The union of `TaggedClass` members keys decode on `_tag` automatically: the generated `AST.Union` carries each member's tag literal, so `Schema.decodeUnknown(Family)` dispatches to the matching member's full codec by discriminant with no manual `_tag` switch at the admission seam — one owner admits the whole family.
- Each member's `_tag` is a `withConstructorDefault`-backed literal injected as the first field, exposed as `Member._tag` static and `instance._tag` literal type — the union's discriminant is the member set's tag literals, recoverable by `Types.Tags<Schema.Schema.Type<typeof Family>>` (the discriminant is hardwired to `_tag`), so exhaustive dispatch and decode both key off one compiler-checked member, never a restated string.
- `Schema.is(Family)` derives `(u: unknown) => u is FamilyType` from the union owner — the family's runtime guard is read off the codec, not hand-written as `("_tag" in u && (u._tag === "A" || …))`; the guard and the type narrow to the identical union, so a guard drift is impossible.
- Adding a member to the `Schema.Union` call widens `Types.Tags<"_tag", …>` by one tag, so every `Match.tagsExhaustive` over the family fails to compile until the new arm is supplied — the family owner makes the next case a loud break at every total dispatch site, the absorbed-growth proof.

[FAMILY_BEHAVIOR_DISPATCH]:
- A class-body method is not in `Fields`, not in `I`, and not in the constructor parameter `C` — it lives only on the instance type `Self`; the codec encodes and decodes the field record alone, so adding `get label()` or `apply()` to a member changes the dispatch surface with zero wire-shape change, and the round-trip reconstructs the method-bearing instance because decode runs `new Member`.
- A method of identical name and signature declared on every union member is callable on the union type directly — `(value: Schema.Schema.Type<typeof Family>).render()` type-checks when each arm declares `render(): string`, so a uniform operation across the family is one method-per-member, not an external `Match` over `_tag`; the union arm carries its own behavior and the call resolves by JS dispatch.
- An operation that differs structurally per member but shares a return contract is the per-arm method: each member's body computes from its own fields, the union exposes the common signature, and a new member lands as one class with the method — the family grows by a member, every shared-method call site untouched.
- A method needing data not on the instance (an injected dependency, a sibling lookup) is NOT a body method; it is external dispatch over the union via `Match.type<…>().pipe(Match.tagsExhaustive({...}))`, keyed on the members' own `_tag` — the instance carries field-derived behavior, the boundary carries context-derived behavior, and the split is by data locality, never style.
- `.extend<Sub>` threads the parent class into the subclass's `Inherited` slot (`Class<…, Self, Proto>`), preserving `Proto` — a subclass of a `TaggedError` stays yieldable and inherits the parent's body methods through the real prototype chain, while the subclass's own body methods attach to its concrete `Self`; widening a family member never strips its behavior.

[VARIANT_PROJECTING_MODEL]:
- `Model.Class<Self>("Identifier")({ ...fields })` is one declaration yielding the `select`-variant class owner PLUS six sibling struct projections — `Self.insert`, `Self.update`, `Self.json`, `Self.jsonCreate`, `Self.jsonUpdate`, and the base `Self`/`Self.select` itself — so the chain `class Entity` + `CreateInput` + `UpdateInput` + `JsonView` + their codecs collapses to this line, where one field-modifier algebra projects every persistence and wire shape.
- The base class extends `ClassFromFields<Self, Fields, ExtractFields<"select", Fields, true>>`: the owner's own instance type, `new Self`, `Self.make`, `Equal`/`Hash`, and `.fields` ARE the `select` projection, so the in-memory entity and the read shape are the same value, never two declarations — the other five variants are static struct codecs hanging off the class.
- Each variant projection is a `Schema.Struct`, not a `Class` — `Self.insert.make(...)` validates and `Self.insert.fields` re-exposes the per-variant field source, but `new Self.insert` and an `instanceof` identity do not exist; the owner is the `select` instance, the variants are transient decode/encode contracts.
- `ExtractFields<V, Fields, IsDefault>` is the mapped type that drives projection: a key whose value is a `Field<Config>` survives into variant `V` only when `V extends keyof Config`, a key whose value is a plain `Schema`/`PropertySignature` appears in every variant unchanged, and a key whose value is a nested `Struct` recurses — the variant set of each column is data on the field, read by the projector, never branched per call site.
- `Struct.Validate<Fields, "select" | "insert" | "update" | "json" | "jsonCreate" | "jsonUpdate">` gates the field record at the type level: a `Field<Config>` whose `keyof Config` is not a subset of the six legal variants resolves the field's position to the literal `"field must have valid variants"`, so an out-of-vocabulary variant key fails at the class header, not at a projection use site.

[MODEL_FIELD_VARIANT_ALGEBRA]:
- `Model.Generated(s)` is a `Field<{ select: S; update: S; json: S }>` — present in reads, updates, and JSON, absent from `insert` and `jsonCreate`: a database-assigned key the application never supplies on create, so `Self.insert.fields` has no `id` and `Self.jsonCreate` rejects a supplied `id`, the omission carried by the field config, never an `omit("id")` at the create boundary.
- `Model.GeneratedByApp(s)` is `Field<{ select; insert; update; json }>` — required by every database variant but absent from `jsonCreate`/`jsonUpdate`: an application-generated key (a client-minted id) supplied to the database write yet never accepted from the JSON create/update payload, the split between DB-required and JSON-rejected riding one field.
- `Model.Sensitive(s)` is `Field<{ select; insert; update }>` — the three database variants only, stripped from all JSON variants: a secret column readable and writable in-process but never projected to the wire, so `Self.json` cannot encode it and a leak is a compile error rather than a redaction discipline.
- `Model.FieldOption(s)` projects asymmetrically across the variant split: the database variants become `Schema.OptionFromNullOr<S>` (decoding SQL `NULL` to `Option.none()`), `json` becomes `optionalWith({ as: "Option" })` (a missing key is `None`), and `jsonCreate`/`jsonUpdate` add `nullable: true` so an explicit JSON `null` also reads as `None` — one optionality declaration yields the four distinct presence semantics SQL and JSON each demand.
- `Model.JsonFromString(s)` makes the database variants a `Schema<Type<S>, string, Context<S>>` (the value is stored and retrieved as serialized text) while the JSON variants use `S` directly (the value travels as a structured object) — a column persisted as text but exposed structurally, the serialize-at-the-DB-seam encoded inline on the field, not a transform wrapped at the repository.
- `Model.Field(config)` is the open per-variant field builder taking the raw `{ select?; insert?; update?; json?; jsonCreate?; jsonUpdate? }` config with excess keys rejected as `never`; `Model.FieldOnly(...variants)(s)` restricts a schema to exactly the named variants and `Model.FieldExcept(...variants)(s)` admits every variant but the named — a bespoke presence matrix lands as one field, never a column duplicated across hand-written variant structs.

[MODEL_OVERRIDEABLE_GENERATION]:
- `VariantSchema.Overrideable<To, From, R>` is `PropertySignature<":", (To & Brand<"Override">) | undefined, never, ":", From, true, R>` — a required-with-default property whose type-side value is either an `Override`-branded `To` or `undefined`, whose encoded side is `From`, and whose `HasDefault` flag is `true`, so a column generated at write time carries its own constructor default and is never supplied by the caller's insert/update value.
- `VariantSchema.Overrideable(from, to, { generate, decode?, constructorDefault? })` is the constructor: `generate: (_: Option<ITo>) => Effect<From, ParseIssue, R>` runs at the write variant to produce the encoded value (receiving the caller's `Override` value as `Option.some` or `Option.none` when absent), `decode` reads it back, and `constructorDefault` supplies the in-process default — the generation requirement `R` joins the variant struct's context and is provided once at the composition root.
- `Model.DateTimeInsert` is `Field<{ select: DateTimeUtc; insert: Overrideable<DateTime.Utc, string>; json: DateTimeUtc }>` — selectable and JSON-visible as a `DateTime.Utc`, generated on insert (stamped to the current instant, encoded as a string), and entirely absent from `update`: a created-at column whose value materializes at the insert seam without a `new Date()` in domain code, the now-stamp riding the field.
- `Model.DateTimeUpdate` adds the `update: Overrideable<...>` arm so the column re-stamps on every write — an updated-at column generated on both insert and update — while `DateTimeInsertFromDate`/`DateTimeUpdateFromDate` and the `FromNumber` siblings only change the `From` encoded representation (`globalThis.Date`, `number`) the database column expects, the type side staying `DateTime.Utc` across all of them.
- `Model.Override(value)` brands a concrete value as `value & Brand<"Override">` so a caller forces a specific timestamp or generated key past the auto-generation thunk: passing `Model.Override(explicitInstant)` to `Self.insert.make` substitutes the branded value, while omitting the key triggers the `Overrideable` generate effect — the escape hatch is the brand, not a parallel non-generating field.
- `Model.UuidV4Insert(brandedSchema)` is the binary-key analogue: `Field<{ select; update; json: brand<Uint8ArrayFromSelf, B>; insert: Overrideable<Uint8Array & Brand<B>, Uint8Array> }>` — a branded UUID generated as bytes on insert and read back on select/update/json, the brand `B` preserved across every variant so the key's nominal identity survives projection.

[MODEL_FIELD_EVOLUTION]:
- `Model.fieldEvolve(mapping)(field)` remaps a field's per-variant schemas through `{ [variant]?: (schema) => newSchema }`, so an existing field constructor is specialized one variant at a time — wrapping only the `insert` arm of a `Generated` field in a refinement, or tightening only the `json` arm — without re-spelling the whole presence matrix; the unmapped variants pass through unchanged.
- `Model.fieldFromKey(mapping)` remaps the encoded property key per variant via `fromKey.Rename`, so one logical column reads from a different wire key per representation — a snake-cased database column and a camel-cased JSON key off one field — the boundary rename carried per variant on the field, never a parallel key map per projection.
- A plain `Schema` passed to `fieldEvolve`/`fieldFromKey` (not yet a `Field`) is promoted across the full variant set before the mapping applies, so a bare schema column gains per-variant key or schema divergence in one call rather than first being widened to a `Field` by hand.

[MODEL_UNION_AND_EXTRACTION]:
- `Model.Union(...members)` over variant structs is `VariantSchema.Union<Members> & Union.Variants<Members, variants>`, so the union itself projects per variant: `Family.insert` is the `Schema.Union` of each member's insert projection, `Family.json` the union of each member's json projection — a polymorphic persisted entity yields one read-union and one write-union per variant, the member set carrying its own variant algebra.
- `Model.extract(variant)(struct)` lifts a single variant's `Schema.Struct` off any variant struct as a standalone codec — `Model.extract("jsonCreate")(Self)` is the create-payload schema read from the owner, the same shape `Self.jsonCreate` exposes — so a variant projection is recoverable from any model value at the boundary that needs only that representation.
- `Model.Struct(fields)` is the class-free variant owner: a `VariantSchema.Struct` carrying the same six projections (`Model.extract` reads them) but no `new`/`instanceof`/`Equal`-base — reach for it when the entity needs no instance methods or nominal identity, and promote to `Model.Class` the moment a getter, a tag, or a structural-equality consumer appears.

[MODEL_REPOSITORY_DERIVATION]:
- `Model.makeRepository(Self, { tableName, spanPrefix, idColumn })` derives a CRUD surface whose `insert` takes `Self["insert"]["Type"]` and returns `Self["Type"]`, `update` takes `Self["update"]["Type"]`, and `findById` takes `Schema.Schema.Type<Self["fields"][Id]>` returning `Option<Self["Type"]>` — every method's argument and result type is read from the variant projections by indexed access, so a column's variant presence (Generated absent from insert, Sensitive absent from json) shapes the repository signature with no hand-written DTO.
- The `idColumn` is constrained to `(keyof Self["Type"]) & (keyof Self["update"]["Type"]) & (keyof Self["fields"])`, so a key that is generated-only-on-select (absent from the update variant) cannot be named the id — the variant algebra makes an ill-chosen identity column a compile error at the repository declaration.
- `makeRepository` requires `SqlClient` and threads `Self["Context"] | Self["insert"]["Context"]` (the union of the select and insert variant contexts) onto each method, so a context-bearing field's requirement surfaces on exactly the operations that touch that variant — `makeDataLoaders` adds `Scope` and a `window`/`maxBatchSize` batching policy over `AnyNoContext` models, the loader variant requiring context-free projections.

[GENERATED_EQUALITY]:
- The owner's `Equal` member is supplied by the `Data.Class` base via the `StructuralPrototype`'s `[Equal.symbol](that)`: it reads `Object.keys(this)` and `Object.keys(that)`, fails fast on a length mismatch, then for every own key requires `key in that && Equal.equals(this[key], that[key])` — own-enumerable keys only, length-gated, and recursive through `Equal.equals` at each field, so structural comparison is the generated default no instance method declares.
- Field comparison recurses by value-or-identity per field kind: a field that is itself an `Equal` instance (a nested class owner, an `Option`, a `HashSet`) compares structurally; a primitive compares by `===`; a `Date` field compares by `getTime()` (with both-`NaN` treated equal) and a `URL` field by `.href`; any other plain object or array field compares by reference identity outside a structural region — a class owner with a bare `{ }`-literal or `Array` field has reference-fragile equality on that field, the trap a nested owner or a `Schema.Array` of owners deletes.
- Prototype identity is not part of equality: the `[Equal.symbol]` body never compares constructors or `_tag`, so two instances of different tagged classes with the identical field set and equal `_tag` value compare equal — discrimination across a family rides the `_tag` field VALUE being unequal, never a class-identity check, which is why a tagged union member's `_tag` literal is load-bearing for equality, not only for dispatch.
- The comparison is symmetric in result under the length gate: after `selfKeys.length === thatKeys.length`, checking each `selfKey in that` is sufficient because equal lengths plus full containment of one key set forces set equality — a `that` carrying an extra non-enumerable or symbol-keyed property does not perturb it, since only string-enumerable own keys participate.

[HASH_GATE]:
- `Equal.equals(self, that)` runs a four-step funnel before the structural body: `self === that` returns `true` immediately; a `typeof` mismatch returns `false`; both must be `Equal` instances (`isEqual` = `hasProperty(u, Equal.symbol)`); then the decisive gate `Hash.hash(self) === Hash.hash(that) && self[Equal.symbol](that)` — the hashes must match BEFORE the structural body even runs.
- The hash gate is the membership trap: two field-wise-equal instances whose cached hashes diverge are reported UNEQUAL without the structural body firing, so any field that hashes non-deterministically across the two instances breaks equality silently — a class owner used as a `HashSet`/`HashMap` key must carry only fields whose `Hash.hash` is value-stable, never reference-random.
- A non-`Equal` object field is the hash-divergence source: `Hash.hash` dispatches `Date`→ISO-string hash, `URL`→href hash, `Equal`-instance→its own `[Hash.symbol]()`, and EVERY other object → `Hash.random(self)`, a per-reference random integer memoized in a global `WeakMap` — so two owners holding distinct-but-value-equal plain-object fields receive different random field hashes, fail the hash gate, and compare unequal despite structural-body equality; the fix is making that field a nested owner or a `Schema`-decoded `Equal` value, not a raw record.
- The hash gate is also the fast path for inequality: distinct instances almost always differ in cached hash, so the expensive recursive `[Equal.symbol]` walk runs only on a hash collision or true equality — equality cost is dominated by hashing, making field hash quality the performance lever, not the comparison body.

[GENERATED_HASH]:
- The owner's `Hash` member is `Hash.cached(this, Hash.structure(this))`: `Hash.structure` seeds `12289` and folds own-enumerable keys by `h ^= Hash.string(key) combine Hash.hash(value)` where `combine(b)(self) = self * 53 ^ b` and a final `optimize` — the per-field contribution XORs key-name hash with value hash, so the structure hash is sensitive to the key SET and each field's value hash, and order-independent because the accumulator is XOR.
- `Hash.cached` memoizes by defining a non-enumerable `[Hash.symbol]` own property on the instance returning the frozen integer, so the structure hash is computed once per instance and every subsequent `Hash.hash` reads the cached closure — the cache write does not perturb equality, because the cached symbol property is non-enumerable and excluded from `Object.keys`.
- The cache assumes immutability: the readonly class fields make the cached hash sound, but a `disableValidation` instance whose fields are later mutated retains the stale first-computed hash and silently mis-keys in a `HashSet` — the readonly contract of the owner is what makes the hash cache correct, mutation under the cache being the defect, not the cache.
- An `Array`-typed field does not hash structurally unless it is a `Data.array`/`Schema.Array`-decoded value: a raw JS array field reaches the `object`→`Hash.random` arm and hashes by identity, so a class owner with a plain `ReadonlyArray` field has identity-fragile membership — decoding the field through `Schema.Array` (whose decoded value carries the structural `ArrayProto` `[Hash.symbol]` = `Hash.array`) restores value-hashing across the element sequence.

[STRUCTURAL_REGION_ESCAPE]:
- `Hash.hash` returns `0` whenever `structuralRegionState.enabled` is true, collapsing the hash gate so `Equal.equals` falls through to a deep structural walk that, inside a region, also compares plain `Object.prototype` records and arrays element-wise — the testing-time mode where reference-random field hashes are neutralized and equality is purely structural.
- Inside a structural region the `compareBoth` fallback additionally walks `Object.getPrototypeOf === Object.prototype` records key-by-key and `Array.isArray` pairs element-by-element, and routes unresolved cases to `structuralRegionState.tester` — so an assertion harness comparing two owners with plain-object fields succeeds structurally where the production hash gate would have failed; production membership and region-mode assertion are NOT the same equality, and a test passing under the region does not prove `HashSet` membership in production.

[DERIVED_EQUIVALENCE_VS_STRUCTURAL]:
- `Schema.equivalence(schema)` derives an `Equivalence<A>` by walking the codec AST, distinct from the runtime `Equal` instance: the structural `Equal` compares the decoded instance's own keys by `Equal.equals`, whereas the schema-derived `Equivalence` compares per the field schemas' own equivalences (a `Schema.Number` field by numeric `===`, a transformed field by the target-type equivalence) — the two coincide for primitive fields but diverge wherever a field's schema imposes an equivalence the structural walk cannot see.
- The `equivalence` annotation on the field-record source overrides the structurally-derived comparison for `Schema.equivalence`, NOT the runtime `Equal.equals`: the `Data.Class` base's `[Equal.symbol]` is fixed structural and unaffected by annotations, so an owner whose semantic equality differs from field-wise structural equality (width-equal spans, normalized key) carries TWO equality notions — the structural one membership and `Equal.equals` use, and the annotated `Equivalence` only `Schema.equivalence`-derived comparisons consult; reconciling them requires the semantic identity to be a real field the structural walk reads, never an annotation alone.
- A semantic-equality owner that must key a `HashSet` by its normalized identity makes the normalized value a field (a `brand`ed normalized key, a canonical form computed in the constructor default) so the structural `Equal` and the cached `Hash.structure` both key on it — pushing semantic identity into the field set is the only way structural membership honors it, because the membership path never consults the derived `Equivalence`.

[MEMBERSHIP_AND_DERIVED_ORDERINGS]:
- A class owner is a value-keyed `HashSet`/`HashMap` key directly: `HashSet.has`/`HashMap.get` resolve through `Hash.hash` then `Equal.equals`, so two distinct instances with equal value-stable fields collide to one bucket and compare equal — deduplication of a decoded `ReadonlyArray<Owner>` is `HashSet.fromIterable` with no key function, the owner being its own key, deleting the `Map<string, Owner>` keyed on a stringified id.
- `Equivalence.struct({ ... })` and `Order.struct({ ... })` build a record-shaped comparison from per-field `Equivalence`/`Order` values read off `Self.fields`, so a custom comparison projects from the owner's field source rather than a transcribed field list — `Order.mapInput((o: Self) => o.field)` and `Equivalence.mapInput` lift a single-field projection into a whole-owner comparator, the projection function the one source the comparator and the field share.
- `Order.combine` chains field orders into a lexicographic owner order (primary field, then tiebreak field) so a multi-key sort of owner instances is one combined `Order` value passed to `Array.sort`/`Array.sortBy`, the next sort key landing as one more `Order.combine` arm — never a hand-written comparator re-reading the fields per call site.
- `Equal.equivalence()` returns the global structural `equals` as an `Equivalence<A>`, the membership equality reified as a combinator for use where an API takes an `Equivalence` — distinct from `Schema.equivalence(schema)`, which is the codec-derived one; passing `Equal.equivalence()` aligns an external `Array.dedupeWith`/`Chunk.dedupe` with the same structural identity `HashSet` uses, where `Schema.equivalence` would dedupe by the codec's per-field notion instead.

[COLLAPSE_TRIGGERS]:
- Three or more parallel record shapes modeling one concept, or sibling factory functions building near-identical structs, are the collapse trigger: fold them into one `Schema.Class` (or one `TaggedClass` union member) whose field modifiers carry the per-variant optionality and defaults inline.
- Three or more record shapes that are sub-views of one entity — a create-input lacking the server keys, a patch-input with every field optional, a read-projection of a few columns — collapse to one `Class` plus derivations: the create-input is `Schema.Struct({ ...Entity.fields }).omit("id", "createdAt")`, the patch-input is `Schema.partial(Entity)` or the struct's own `.omit` then `partial`; a standalone `type SubT = Pick<EntityT, "a" | "b">` paired with a hand-written `subCodec` is the exact spelling these projections delete — `Entity.fields` spread or a struct `.pick("a", "b")` yields the type and the codec from one source, the static type extracted and never restated beside the projection.
- Three or more co-varying persistence shapes of one entity — an in-memory record, an insert DTO lacking server keys, an update DTO with re-stamped timestamps, a JSON view hiding secrets, a create payload rejecting generated ids — collapse to one `Model.Class` whose field constructors (`Generated`, `Sensitive`, `FieldOption`, `DateTimeInsert`) carry the per-variant presence and representation; a `Schema.Class` entity paired with a hand-spread `Schema.Struct({...fields}).omit("id")` create-input and a `Schema.partial` update-input is the exact spelling this owner deletes, because those projections drift the moment a column's create/update/read rules diverge.
- Three or more co-traveling shapes for one operation — a request payload type, a result type, an error type, and a hand-written `{ encode, decode }` pair — fold into one `TaggedRequest` whose `payload`/`success`/`failure` slots carry them, the serializer deriving from the trait symbols, never written; a standalone `Request.of`/`Request.tagged` paired with a separately-declared response schema is the exact spelling this owner deletes.
- Three or more sibling tagged classes consumed together — decoded from one wire field, dispatched by one switch, stored in one column — fuse into one `Schema.Union`: derive the type by `Schema.Schema.Type`, the guard by `Schema.is`, dispatch by `Match.tagsExhaustive`; the per-member `is`-guard zoo and the `A | B | C` type restatement are the exact spellings this owner deletes.
- Three or more sites hand-rolling a `Map<string, Owner>` keyed on a stringified id, an `eq`-by-id helper, and a sort comparator over the same entity are the collapse trigger: the owner already IS a `HashSet`/`HashMap` key by structural `Equal`/`Hash`, the dedup is `HashSet.fromIterable`, and the sort is one `Order.struct`/`Order.combine` read from `Self.fields` — the string-key map, the id-equality helper, and the inline comparator are the exact spellings the generated instance and the field-derived `Order` delete.
- A `type X = Schema.Schema.Type<typeof Self>` restating what the class already is, a second `const` rebuilding the instance shape, or a standalone `new XImpl()` following the declaration are the exact spellings the owner deletes — the class is simultaneously the type and the constructor.

[OWNER_PROMOTION_LADDER]:
- `Schema.Struct` (no `_tag`) is admitted as the field-bearing value owner only when no nominal identity, instance methods, or `Equal`/`Hash` base are needed; the moment a method, a tag, or a structural-equality consumer appears, the shape is already a `Schema.Class` and sizing it as a bare struct first multiplies the surface.
- A `Data.taggedEnum` is the value-only family with no codec and no class instances; the moment a member needs a validating constructor, a wire codec, structural `Equal`/`Hash`, or an `instanceof` identity, the family is already a `Schema.Union` of `TaggedClass` members — sizing it as a bare tagged enum first and bolting schemas on later splits each member's codec from its value across two declarations.
- A `Schema.Class`/`Schema.TaggedClass` is the owner when the shape is data with no remote outcome; the moment a success/failure result codec attaches to the same concept, the shape is already a `TaggedRequest` — sizing it as a tagged class first and bolting result schemas on later splits the result codec from its request across two declarations.
- A `Schema.Class` is the owner when the entity has one representation; the moment a second persistence or wire variant (a DB-only secret, a generated key, a re-stamped timestamp, a JSON-hidden column) attaches to the same concept, the shape is already a `Model.Class` — sizing it as a plain class and bolting variant structs on later splits each variant's codec from the entity across separate declarations.
- A class owner with a raw object or array field used as a `HashSet` key is the silent defect: decode every value-participating field through a `Schema` whose decoded value is itself an `Equal` instance (`Schema.Array`, a nested owner, `Schema.OptionFromNullOr`) so the hash gate and structural walk both see value identity — a field that must stay a plain record but not key the set is excluded by projecting the membership key to a struct-`omit` view, never by leaving an identity-fragile field on the keying owner.

[BOUNDARY_DELEGATION]:
- Boundary admission and `ParseError` conversion remain at the owning seam; an owner declares its codec, the act that runs `Schema.decodeUnknown`/`serialize`/`deserialize` and converts the `ParseError` belongs to the boundary that crosses the wire.
- The decode act running `Schema.decodeUnknown(Family)` over a tagged union, and the external `Match.tagsExhaustive` dispatch surface, are the consuming boundary and dispatch seams; the family owner declares the member set, the discriminant, and the body-carried behavior, and every consumer reads the type, codec, and member list from that one union.
- The decode act running `Schema.decodeUnknown(Self.insert)` and the `SqlClient` that executes the derived statement are the consuming boundary; the model owner declares the field algebra, the six variant projections, and the generation thunks, and every repository, loader, and wire handler reads its required shape from the one declaration.
- The decode act that admits the wire value and the `HashSet`/`HashMap`/`Array.sortBy` consumer are the boundary and the membership seam; the owner declares the field set whose structural `Equal`/`Hash` IS the membership identity, and every dedup, lookup, and ordering reads that one identity — the comparison notion is never re-derived beside the owner.

```typescript
import { Match, Schema, pipe } from 'effect';

class Captured extends Schema.TaggedClass<Captured>()('Captured', {
    at: Schema.Number,
    weight: Schema.Number.pipe(Schema.between(0, 1)),
}) {
    get magnitude(): number {
        return this.weight * this.at;
    }
}
class Pending extends Schema.TaggedClass<Pending>()('Pending', {
    since: Schema.Number,
}) {
    get magnitude(): number {
        return 0;
    }
}
class Voided extends Schema.TaggedError<Voided>()('Voided', {
    reason: Schema.String,
}) {
    get magnitude(): number {
        return Number.NaN;
    }
}

const Reading = Schema.Union(Captured, Pending, Voided);
type Reading = Schema.Schema.Type<typeof Reading>;

const totalCarried = (rows: ReadonlyArray<Reading>): number => rows.reduce((sum, row) => sum + row.magnitude, 0);

const classify: (row: Reading) => string = pipe(
    Match.type<Reading>(),
    Match.tagsExhaustive({
        Captured: (row) => `held ${row.magnitude}`,
        Pending: (row) => `idle since ${row.since}`,
        Voided: (row) => row.reason,
    }),
);

const admit = Schema.decodeUnknown(Reading);
const guard: (u: unknown) => u is Reading = Schema.is(Reading);
const [held] = Reading.members;
const heldTag: 'Captured' = held._tag;
```

```typescript
import { Schema } from 'effect';

const SpanFields = Schema.Struct({
    lower: Schema.Number,
    upper: Schema.Number,
}).pipe(
    Schema.filter((s) =>
        s.upper >= s.lower
            ? undefined
            : [
                  { path: ['upper'], message: '<upper-below-lower>' },
                  { path: ['lower'], message: '<lower-above-upper>' },
              ],
    ),
);

class Span extends Schema.Class<Span>('Span')(SpanFields, {
    title: '<span-title>',
    parseOptions: { errors: 'all', onExcessProperty: 'error' },
    parseIssueTitle: (issue) => (issue._tag === 'Composite' ? '<span-invalid>' : undefined),
}) {
    get width(): number {
        return this.upper - this.lower;
    }
}

const trusted = new Span({ lower: 5, upper: 1 }, { disableValidation: true });
const lower = Span.fields.lower;
const admit = Schema.decodeUnknown(Span);
```

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

const RowId = Schema.Number.pipe(Schema.brand('RowId'));

class Record extends Model.Class<Record>('Record')({
    id: Model.Generated(RowId),
    label: Schema.NonEmptyTrimmedString,
    secret: Model.Sensitive(Schema.Redacted(Schema.String)),
    note: Model.FieldOption(Schema.String),
    payload: Model.JsonFromString(Schema.Struct({ kind: Schema.String, size: Schema.Number })),
    createdAt: Model.DateTimeInsertFromDate,
    updatedAt: Model.DateTimeUpdateFromDate,
}) {
    get headline(): string {
        return `${this.label}:${this.id}`;
    }
}

const admitCreate = Schema.decodeUnknown(Record.jsonCreate);
const admitUpdate = Schema.decodeUnknown(Record.jsonUpdate);
const insertSchema = Record.insert;
const exposed = Model.extract('json')(Record);
const repository = Model.makeRepository(Record, { tableName: '<rows>', spanPrefix: '<rows-repo>', idColumn: 'id' });
```
