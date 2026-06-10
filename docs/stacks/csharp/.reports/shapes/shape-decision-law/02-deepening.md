# Shape Decision Law — Deepening

[GENERIC_ALGEBRA_SURFACE]:
- Every keyed owner implements the static-abstract factory contract `IObjectFactory<T, TValue, TError>` — static `TError? Validate(TValue value, IFormatProvider? provider, out T? item)` — and keyed smart enums additionally implement `ISmartEnum<TKey, T, TError>` with static `Items`, `Get`, and `TryGet`. One generic constraint therefore owns boundary plumbing across every owner kind: admission sweeps, cache hydration, and configuration readers are written once against the contract, and re-deciding an owner's kind later never touches the generic plumbing. Marker forms (`IObjectFactory<T>`, `ISmartEnum<TKey>`, `IKeyedObject<TKey>`) carry kind evidence for non-generic infrastructure.
- `Validate` threads `IFormatProvider?` through the canonical admission signature: culture is an explicit admission input on the owner contract, never an ambient read — parse-shaped owners receive formatting policy at the call, and generic admission code forwards it without knowing whether the key is numeric, temporal, or textual.
- Arithmetic generation emits implementations of the numeric operator interfaces (`IAdditionOperators<T,T,T>` and siblings) including the `operator checked` variants, so owners satisfy generic-math constraints directly; generation per axis is conditional on the key type itself implementing the interface or exposing the corresponding `op_Addition`/`op_CheckedAddition` — an axis declared on a non-arithmetic key is silently inert, which makes a no-effect axis declaration a latent lie worth deleting.
- Per-axis operator selection encodes dimensional analysis: generated products are homogeneous (`T * T -> T`) or key-typed (`T * TKey -> T`), and a like-quantity product changes dimension, so quantity-shaped owners enable addition/subtraction axes and hold multiply/divide at `None`, hand-writing only the cross-dimension operators the algebra actually has. Equality and comparison operator generation likewise declares the corresponding generic-math equality/comparison interfaces, with key-type overloads adding the heterogeneous implementations.
- Interface synthesis is capability-conditional, which makes capability subtraction meaningful: `IParsable<T>` appears only when the key is `string` or itself parsable (and skipping it forcibly skips `ISpanParsable<T>` because the latter inherits the former); `IFormattable` appears only over a formattable key; `IComparable<T>` appears when the key compares — or when a key comparer accessor is declared over a non-comparable key, so ordering is grantable by declaration, not only inherited from the key.

```csharp
[ValueObject<decimal>(
    AdditionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    SubtractionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    MultiplyOperators = OperatorsGeneration.None,
    DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Grams;

static TOwner Net<TOwner>(TOwner seed, IEnumerable<TOwner> deltas)
    where TOwner : IAdditionOperators<TOwner, TOwner, TOwner> =>
    deltas.Aggregate(seed, static (acc, next) => acc + next);

static IReadOnlyList<TOwner> Admitted<TOwner>(IEnumerable<string> raw, IFormatProvider? culture)
    where TOwner : IObjectFactory<TOwner, string, ValidationError> =>
    [.. raw.Select(value => (Error: TOwner.Validate(value, culture, out var owner), Owner: owner))
           .Where(static slot => slot.Error is null)
           .Select(static slot => slot.Owner!)];
```

[CAPABILITY_SUBTRACTION]:
- Skip flags are semantic statements, not omissions: a surrogate identifier holds `SkipIComparable = true` with comparison operators at `None` because its ordering is meaningless — identity without order is a declared stance the type system then enforces at every sort and range expression. The subtraction lattice is consistent in both directions: enabling ordering coerces equality upward, and `SkipEqualityComparison` forces both operator families to `None` downward — no configuration can express ordering without equality or operators without identity.
- The throwing factory verb raises the BCL `System.ComponentModel.DataAnnotations.ValidationException` carrying only `validationError.ToString()`: the throw edge is framework-typed and message-shaped, while the typed error object travels exclusively on the `Validate` route. A boundary that needs the typed error must route `Validate`; choosing the throwing verb there silently flattens a structured error vocabulary to a string.
- `SkipKeyMember = true` with `KeyMemberName` hands the key declaration to the author — a field, or a property whose backing field is set through an `init` accessor — while factories, equality, conversions, and serializer wiring continue to generate against it. The pressure that earns it: attribute placement, documentation, or layout control on the key member itself, with zero loss of generated capability.

[INGRESS_TOPOLOGY]:
- Ad-hoc conversion operators withdraw exactly where the language forbids them — type-parameter members, interface members, `object` members, and duplicate member types — and any one trigger flips factory generation on for all members (`Create{MemberName}`), keeping ingress symmetric: no union has half conversion-ingress and half factory-ingress by accident. `Always` forces the named verbs even where conversions exist; `None` under a trigger leaves constructors as the only route. One accessibility knob governs the whole ingress surface: `ConstructorAccessModifier` sets constructors, conversion operators, and factory methods together.
- Default member naming derives from type names, so an unnamed union dispatches and projects through `@string:`, `int32:`, `IsString`, `AsInt32` — keyword-escaped, representation-leaking identifiers at every call site. Naming every member (`T1Name`…) is therefore not cosmetic: arm names are the named-argument contract, and the rename converts representation vocabulary into domain vocabulary across all dispatch sites at once.
- Duplicate member types are a deliberate modeling tool: the same representation under two names yields a union whose slots are semantically distinct, whose only ingress is the named factories, and whose equality discriminates the slot before the payload — equal payloads under different names are unequal values. The state a wrapper pair or a boolean tag would have smuggled in lives in the discriminator:

```csharp
[Union<string, string>(T1Name = "Draft", T2Name = "Published")]
public partial class Copy;

static bool Promoted(Copy before, Copy after) =>
    before.IsDraft && after.IsPublished && before.AsDraft == after.AsPublished;
```

- Stateless members store nothing but the discriminator: parameterless factory, accessor returning `default`, equality by index alone — outcome markers ride at zero bytes beside payload members instead of as sibling sentinel types.
- Member nullability is declared at the attribute (`T1IsNullableReferenceType`) because type arguments cannot carry the annotation: null becomes an admissible state of that member and its accessor returns the annotated type. Undeclared, a null reference member is rejected — absence inside a union member is an owner decision, not an accident of reference types.

[PARTIAL_AND_ABSENT_DISPATCH]:
- `MapPartially` takes a required `@default` first and each case as an optional `Argument<TResult> name = default`; the implicit `T -> Argument<T>` conversion marks any explicitly passed value as set, so an arm explicitly mapped to `null` or to the result type's default still counts as handled rather than falling to `@default`. The wrapper solves the omitted-versus-default ambiguity that optional parameters cannot express, without an overload per subset of cases.
- `SwitchPartially` in action form declares `@default` as a nullable delegate defaulting to null: omitting it makes every unhandled case a silent no-op. Selection law: value-form partial dispatch is total-with-fallback by construction (the default is the one non-optional parameter); action-form partial dispatch is reserved for surfaces where no-op is the documented semantics, because a dropped case there is indistinguishable from success.
- Heterogeneous arm returns break `TResult` inference and flag the entire dispatch call rather than the offending arm; pinning the type argument (`Switch<TAbstraction>(...)`) restores per-arm diagnostics and is mandatory whenever arms return siblings under an abstraction.
- `SwitchMapMethodsGeneration.None` deletes the dispatch surface from an owner whose consumers must not enumerate cases — transport-only carriers interpreted elsewhere — making arm-style consumption inexpressible rather than discouraged.
- Native `switch` over public union cases inverts the exhaustiveness direction: generated dispatch breaks every call site when a case is added, while a discard arm absorbs the new case silently — the nullable-payload-bag failure mode re-imported into a sound type. Smart-enum items close the other door: items are static readonly instances, not constants, so they cannot appear as case labels at all — identity dispatch is inexpressible in language switch and exists only as generated dispatch or delegate rows.

[UNION_MEMORY_AND_RUNTIME]:
- The backing-field lattice has three tiers, not two: with at most one reference member every member gets a typed field; with two or more reference members the generator merges the references into one object-typed field automatically while value members keep typed fields (no boxing); `UseSingleBackingField` only adds the value members into the shared field at boxing cost. The knob's real subject is value types — reference merging already happens without it.
- A struct ad-hoc union's discriminator reserves zero for uninitialized, and a `default` instance throws `InvalidOperationException` from `Switch`, `Map`, `Value`, and the typed accessors — at use, not at creation. Generated unions implement `IDisallowDefaultValue`, so expression-level default use is analyzer-flagged, but array allocation and collection growth materialize defaults invisibly: struct unions hydrated from pooled or array storage need a liveness gate, or the concept takes the class form.
- Generated equality runs reference-equality short-circuit, then discriminator inequality short-circuit, then a single-member comparison through the default equality comparer or the declared string comparison baked into the method body; `GetHashCode` and `ToString` switch on the discriminator with throwing default arms — every runtime surface of the union is discriminator-rooted.
- Regular-union dispatch switches on the runtime type with a throwing default arm, and the generator sorts case arms by inheritance hierarchy — refusing to generate when the hierarchy cannot be ordered. A foreign subclass admitted through a widened constructor compiles at every call site and detonates at first dispatch: the closure rules are runtime-totality guarantees, not style.

[CASE_TOPOLOGY]:
- Case discovery is lexically scoped: regular-union cases must be declared nested inside the owner's body, so the owner's body is the case list — file or namespace organization cannot fragment a family, and a derived type declared outside simply never joins it.
- Intermediate grouping is a per-node dispatch decision: a non-sealed nested class with private constructors groups cases without becoming a dispatch surface, while placing the union attribute on the intermediate makes the generator emit it abstract with a private constructor and give it its own exhaustive dispatch — node-owned dispatch and root-level stop-at overloads are complementary granularities over the same tree.
- Smart enums may extend a foreign base class; the generated constructor threads key first, own instance properties second, base-class constructor parameters last — host-mandated base types are not a manual-owner pressure.
- One private generic case class closed at several type arguments yields type-indexed items: distinct items carry distinct compile-time type payloads while the vocabulary itself stays non-generic — the route for per-item type evidence (handler types, payload schemas) without genericizing the enum or storing runtime type objects.
- Vocabulary graphs — items referencing sibling items, as in transition tables — take lazily evaluated list-valued constructor columns: eager sibling references resolve to null for items not yet initialized in declaration order, and the lazy column defers resolution to first read. Edges between items are a column pressure, not a union pressure.
- Owners nest anywhere, but every enclosing type must itself be partial — a non-partial container is a generation-blocking compile error, not a degraded mode.

[WIRE_CLOSURE]:
- Neither union flavor is wire-capable alone: no type discriminator is emitted, so polymorphic serialization does not exist by construction, and the wire route is an object factory collapsing the union to one primitive representation or a protocol DTO at the edge. The design consequence runs backward into owner selection: a closed alternative set whose cases must round-trip a boundary either admits a single-scalar grammar (union plus object factory), re-shapes as a keyed owner where the key is the discriminator, or accepts an edge DTO — a union whose cases cannot fold into one grammar is interior-only by construction.

[REIFIED_METADATA]:
- The owner taxonomy itself is reified as a closed family — keyed smart enum, keyed value object, complex value object, keyless smart enum, ad-hoc union, regular union — with its own exhaustive state-threaded `Switch`/`Map`, resolvable from a runtime type via a lookup. Infrastructure that must discriminate owner kind (mappers, codec registries, schema emitters) rides that exhaustive dispatch instead of bespoke attribute reflection, inheriting compile breaks when the taxonomy grows.
- Keyed metadata carries the key projection both as a compiled delegate and as a lambda expression tree, so expression-translating consumers compose key access without the owner exposing a public key member — private-key owners stay query-translatable. Factory metadata exposes the per-framework route partition (serialization, persistence, binding) at runtime, and a static-abstract invoker bridges from runtime types to the static contracts for fully dynamic edges.
- The metadata namespace is declared unstable across releases; the law that follows: metadata dispatch belongs to infrastructure adapters only — domain code already holds the typed owner and never needs the mirror.

[EXTENSION_AXIS]:
- Generated families fix an expression-problem stance: the case axis is closed by analyzer law while the operation axis stays open — extension members attach receiver-owned operations to a closed owner from outside, dispatching internally over the generated exhaustive forms. Foreign code that needs new operations over a family is served without touching the owner; only foreign code that must add new cases justifies leaving generated owners for an interface hierarchy. The manual-owner exemption is therefore narrower than "extension point implies manual owner": it triggers on case extension alone, never on operation extension.
