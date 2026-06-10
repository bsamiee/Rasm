# [SMART_ENUMS_CORE]

[DECLARATION_AND_ITEM_SET]:
- The generator binds only to partial `class` declarations; records, structs, and primary constructors are rejected. All constructors must be private, and a vocabulary without derived cases must be `sealed` — the closed-world guarantee is analyzer-enforced at error severity, not conventional.
- An item is exactly a static, explicitly declared field whose declared type equals the enum type symbol and that is not marked `[IgnoreMember]`. A static property typed as the enum compiles and silently vanishes from `Items`, lookups, and `Switch` arity — the analyzer emits only a warning. A field declared with a derived case type instead of the base type vanishes the same way. Item-set membership is a declared-type fact, never a value fact.
- Every item field must be public — a non-public static field of the enum type is an error, not a silently skipped member. A private scratch field of the enum type is legal only under `[IgnoreMember]`, which removes it from items, lookups, and dispatch entirely: there is no hidden-row tier, and row visibility is decided at the type level, never per row.
- Field declaration order is load-bearing: it fixes each item's index, the generated `Switch`/`Map` parameter order, and the metadata identifier table. Index-based dispatch is exactly why unnamed call-site arguments are a compile error — named arguments are the reorder shield.
- Every item is constructed through one generated private constructor: key first, then each assignable non-abstract instance field/property in declaration order, then one trailing parameter per behavior delegate. `static partial void ValidateConstructorArguments(ref TKey key, ref ...)` receives every argument by ref before assignment — declaration-time normalization runs once per item, never per lookup.
- The key member generates as `public TKey Key { get; }` by default; kind, access modifier, and name are attribute knobs, and the default name flips to `_key` when generated as a private field. A user-declared key member is honored when found; absence or type mismatch is an analyzer error.
- Keyless `[SmartEnum]` is the behavior-only vocabulary: `Items`, `Switch`/`Map`, reference equality with identity hash; no `Get`/`TryGet`/`Validate`, no conversions, no parsing, no `ToString` override. Keyless vocabularies still publish runtime metadata — the lazy item table pairing each instance with its field-name identifier — so generic tooling and diagnostic surfaces enumerate behavior-only vocabularies. Only key-conversion machinery is absent from their metadata shape. Reach for keyless when rows carry policy and nothing crosses a wire keyed by the row.
- An item-less vocabulary is a warning. `Switch`/`Map` generate only for 1–999 items; at 1,000 or more the type silently retains lookups but loses generated dispatch — that cliff marks where the concept stopped being a vocabulary.

[ADMISSION_AND_LOOKUP]:
- All lookup state hides behind one `Lazy<Lookups>` with `ExecutionAndPublication`: a frozen dictionary, the item list, and — for string keys — a `FrozenDictionary<,>.AlternateLookup<ReadOnlySpan<char>>`. Materialization walks the items once and throws on null items, null keys, and duplicate keys. Fail-fast is deferred to first lookup, not type load, so a duplicate-key defect hides until the first `Items`/`Get`/`TryGet` touch — force `Items` in a startup probe or law test to surface it deterministically. The `Lazy` is publication-locked with a factory delegate, so any materialization failure caches the thrown exception instance itself: every later `Items`, `Get`, `TryGet`, `Validate`, parse, conversion, or serializer touch rethrows the identical instance for the life of the process. Recovery is restart-only, which upgrades the startup probe from hygiene to the only place this class of defect is cheap.
- The admission triad is closed: `Get` throws a typed unknown-identifier exception and backs the explicit key-to-enum conversion operator, which is explicit precisely because it can throw; `TryGet([AllowNull] key, out item)` is the bool form; `Validate(key, IFormatProvider?, out item)` returns the typed validation error or null and is the single rail-bridge point:

```csharp
[SmartEnum<string>]
public sealed partial class Channel
{
   public static readonly Channel Alpha = new("alpha");
   public static readonly Channel Beta  = new("beta");

   public static Fin<Channel> Admit(ReadOnlySpan<char> raw) =>
      Validate(raw, null, out var item) is { } error ? Error.New(error.ToString()!) : item!;
}
```

- String-keyed vocabularies admit spans end-to-end: `Get`/`TryGet`/`Validate` span overloads ride the alternate lookup with zero string allocation; span-based JSON conversion is on by default with a targeted opt-out knob (`DisableSpanBasedJsonConversion`) — use it when a wire producer emits escaped or chunked text that span admission would misread, without surrendering span conversion vocabulary-wide.
- `[ObjectFactory<TValue>]` opens additional admission routes; the factory value type parameter carries `allows ref struct`, so `[ObjectFactory<ReadOnlySpan<char>>]` is a legal declaration for non-string-keyed vocabularies (a string-keyed vocabulary already generates the span-shaped `Validate`, and redeclaring the span factory there produces CS0111 — the collision is the design statement that string keys own the span route natively). The hand-written `Validate` composes span parsing of the raw text with the generated key lookup. A generic bridge constrained on `IObjectFactory<T, ReadOnlySpan<char>, TError>` calls the static abstract `Validate` directly — zero-allocation admission generalizes across every non-string-keyed vocabulary with a span factory:

```csharp
[SmartEnum<int>]
[ObjectFactory<ReadOnlySpan<char>>]
public sealed partial class Slot
{
   public static readonly Slot Primary = new(7);

   public static ValidationError? Validate(ReadOnlySpan<char> value, IFormatProvider? provider, out Slot? item)
   {
      item = int.TryParse(value, out var key) && TryGet(key, out var found) ? found : null;
      return item is null ? ValidationError.Create($"unknown: {value}") : null;
   }
}

public static class Admission
{
   public static ValidationError? Admit<T>(ReadOnlySpan<char> raw, out T? item)
      where T : class, IObjectFactory<T, ReadOnlySpan<char>, ValidationError>
      => T.Validate(raw, null, out item);
}
```

- Factory admission rules: the enum must hand-implement `static TError? Validate(TValue, IFormatProvider?, out T?)` (TTRESG061 if absent), must not set `HasCorrespondingConstructor = true` on a vocabulary (TTRESG060 — items are looked up, never constructed via factory), and needs `ToValue` only when the factory drives serialization or persistence (TTRESG062 if missing with `UseForSerialization` or `UseWithEntityFramework`).
- On targets that compile the span plane, the string-key lookup factory acquires the span alternate view unconditionally — even under a custom comparer accessor — and the alternate view demands a comparer implementing the span-alternate equality interface. Every prebuilt accessor and every BCL string comparer satisfies it; a hand-rolled comparer produced via `EqualityComparer<string>.Create` does not, and that factory is the standard idiom for ad hoc comparers. The mismatch raises no analyzer finding and compiles clean; it surfaces as an `InvalidOperationException` at the first lookup touch. One declaration with a non-alternate custom comparer materializes cleanly on a target below the span plane and poisons itself on a target above it — runtime behavior diverges per compiled target with zero source difference. A custom comparer accessor therefore carries an implicit contract: implement the span-alternate interface on its comparer, or use a BCL comparer.
- Null never reaches the dictionary: nullable key types are rejected at declaration, `Get(null)` null-propagates (`NotNullIfNotNull`) instead of throwing, and `TryGet(null)` is false — absence belongs to the caller's `Option`, not to the vocabulary.
- `IParsable<T>`/`ISpanParsable<T>` are generated: string-keyed enums parse via string `Validate`; non-string keys parse raw text into the key type first, then validate. `Parse` throws `FormatException` carrying the validation error text; `TryParse` is total. Skipping `IParsable` transitively skips `ISpanParsable`.
- The generated `Validate` ignores its `IFormatProvider` argument; the parameter exists to unify signatures with hand-written and factory `Validate` implementations where culture matters. Pass null at internal call sites.
- Custom error types attach via `[ValidationError<TError>]` with `TError : IValidationError<TError>` requiring a static abstract `Create(string)`. The generated failure message is a fixed format, and `Create` receives only the rendered message — the offending key is not passed through. Structured errors that need the key must capture it at the bridge, not inside the error type.

[KEY_POLICY_AND_IDENTITY]:
- String keys are ordinal-case-insensitive by default across every surface at once: the lookup dictionary, the span alternate lookup, the constructor-computed hash seed, key equality, and `CompareTo` — case-insensitive admission with case-preserving storage. Two items differing only in key case are a duplicate-key materialization failure, not two items.
- Key policy attaches as a type, not an instance: `[KeyMemberEqualityComparerAttribute<TAccessor, TKey>]` and `[KeyMemberComparerAttribute<TAccessor, TKey>]` take comparer-accessor types exposing static `EqualityComparer`/`Comparer` properties, so policy is available at generation time and in static contexts. Prebuilt accessors in `ComparerAccessors` cover the ordinal/invariant/current-culture matrix. Mismatched accessor/key types and one-sided comparer declarations (equality comparer without comparer, or vice versa) are analyzer findings. The comparer accessor is one policy point that swings lookup, hash seed, `CompareTo`, and comparison operators together, with no per-surface override — a vocabulary whose domain rank differs from key order encodes rank as an item column and sorts by projection, never by bending the comparer to fake a domain order.
- Instance identity is reference identity: `Equals(T)` is `ReferenceEquals`, and `GetHashCode` returns an int computed once in the constructor from the key through the configured comparer — set and dictionary membership over items costs a field read, and hash semantics stay aligned with lookup semantics by construction.
- A keyed vocabulary carries two generated total orders that agree only by accident: `Items` enumerates in declaration order — the same order that fixes dispatch indices and callback positions — while `CompareTo` and the comparison operators rank by key under the configured comparer. Items declared omega-then-alpha enumerate in that order and sort alpha-then-omega; any consumer that sorts `Items` has silently left declaration order.
- The operator surface is generic-math-shaped: `IEqualityOperators<T,T,bool>` by default, plus `IEqualityOperators<T,TKey,bool>` under the key-overload setting so `item == key` compares without conversion. Comparison operators imply equality operators — the settings coerce each other in both directions and the inconsistent combination is flagged.
- Conversions default asymmetric by design: enum-to-key implicit and null-propagating; key-to-enum explicit because it routes through throwing `Get`. Both are suppressed when the key type is an interface or `object`, and either direction can be set to none for boundary-only vocabularies.
- `IComparable`/`IComparable<T>`/`IFormattable` ride the key's own capabilities and are individually skippable. `ToString` returns the key's `ToString`, so logs and interpolation speak the wire vocabulary by default.

[GENERATED_DISPATCH]:
- Dispatch is an integer jump table, not a lookup: each item carries a write-once index assigned during lookup materialization, and the hot path is a non-volatile int read with a staleness fallback. No dictionary probe, no virtual call, no delegate array — `Switch` over N items compiles to a native `switch` on a field read.
- The overload matrix covers action and func forms, each in stateless and state-threaded variants, plus `SwitchPartially`/`MapPartially` under the partial-overloads setting. State-threaded callbacks receive only the state — item identity is positional and never passed. Both `TState` and `TResult` allow ref structs, so span-carrying contexts thread through dispatch without boxing.
- Unnamed callback arguments at `Switch`/`Map` call sites are an analyzer error (the leading state argument alone is exempt): index-based dispatch would silently rebind positional callbacks when items are added or reordered, so the argument name is the binding contract. Adding an item arity-breaks every full-coverage call site at compile time — the closed-vocabulary evolution guarantee. `SwitchPartially`'s `@default` is the explicit, visible opt-out of totality.
- Non-static lambdas in `Switch` draw an info-level diagnostic steering toward static lambdas plus the state overload; closure-free dispatch is analyzer-encoded posture:

```csharp
var verdict = channel.Switch(
   state: (score, threshold),
   alpha: static s => s.score >= s.threshold,
   beta:  static s => s.score >  s.threshold * 2);
```

- `Map` is the value-row form — one `TResult` per item, no lambdas. `MapPartially` parameters are `Argument<TResult>`, an implicit-conversion ref struct whose `IsSet` distinguishes "omitted" from "explicitly default", so null and zero remain legal mapped values. The func-shaped `SwitchPartially` and `MapPartially` require `@default`; only the action-shaped `SwitchPartially` accepts a null default.
- Callback parameter names derive from item field names through acronym-aware camel-casing (leading underscores stripped, an initial uppercase run lowered as a unit) and are `@`-escaped in generated code — keyword-named items remain legal, and the state parameter is renameable via an attribute knob when an item name would collide with it.
- Items are static readonly singletons, not constants: a case label naming an item does not compile, so the language's own `switch` can never be total over a vocabulary, and a property pattern over the key re-enters string literals while silently dropping items added later. Generated `Switch`/`Map` is therefore not a convenience layer — it is the only dispatch the type system can hold total under item addition. A guard chain over items via equality operators re-derives the correspondence one dispatch call already owns and misses new items identically.

[CASE_BEHAVIOR_ROWS]:
- `[UseDelegateFromConstructor]` on a partial method generates a private delegate field, a trailing constructor parameter, and the forwarding body — each item row injects its own implementation at declaration, making the vocabulary a behavior table with per-row columns:

```csharp
[SmartEnum<string>]
public sealed partial class Strategy
{
   public static readonly Strategy Eager = new("eager", weight: 2, ScoreEager);
   public static readonly Strategy Idle  = new("idle",  weight: 1, ScoreIdle);

   public int Weight { get; }

   [UseDelegateFromConstructor]
   public partial double Score(double input);

   private static double ScoreEager(double input) => input * input;
   private static double ScoreIdle(double input)  => input / 2;
}
```

- A custom delegate type is generated only when a parameter carries ref, in, or out, or when `DelegateName` is set; otherwise plain `Action`/`Func` backs the field. `DelegateName` renames both the delegate type and the constructor argument. Delegated methods must be partial and non-generic — delegate fields cannot be generic — so a generic per-case behavior takes the inheritance tier: declare the enum `abstract`, override with private nested sealed cases. Abstractness is legal precisely because the sealing requirement lifts when derived cases exist; nesting keeps the topology closed because nested cases reach the generated private constructor through enclosing-type access, so case construction needs no accessibility widening and case identity stays unobservable:

```csharp
[SmartEnum<string>]
public abstract partial class Picker
{
   public static readonly Picker Head = new HeadCase();
   public static readonly Picker Tail = new TailCase();

   public abstract A Pick<A>(IReadOnlyList<A> source);

   private sealed class HeadCase : Picker
   {
      public HeadCase() : base("head") { }
      public override A Pick<A>(IReadOnlyList<A> source) => source[0];
   }

   private sealed class TailCase : Picker
   {
      public TailCase() : base("tail") { }
      public override A Pick<A>(IReadOnlyList<A> source) => source[^1];
   }
}
```

- A behavior-row method dispatches through one delegate-field invocation — no `Switch`, no index read. The cost model is one delegate field per behavior per item, so N parallel behaviors sharing inputs collapse into one delegate returning a composite value rather than N columns.
- Plain readonly delegate-typed instance properties also become constructor parameters and invoke identically at call sites. `[UseDelegateFromConstructor]` differs by hiding the delegate behind a true method surface — ref-kind parameters, documented contracts, and no public delegate-valued state.
- Abstract members are the third behavior tier: excluded from generated constructor parameters and implemented per derived case. Justified only when a case needs its own state or overrides several members coherently; otherwise delegate rows beat subclassing on density.

[DERIVED_CASES_AND_GENERIC_VOCABULARIES]:
- Derived-case topology is prescribed: the enum stays unsealed only because derived types exist; first-level nested derived classes must be private, and deeper levels are public — public instances remain exactly the declared items while case classes stay invisible. For every base constructor, each derived case receives a mirrored constructor (own members first, then base arguments, with name collisions deduplicated by numeric suffix).
- A derived-case instance is an item only when its field is declared with the base type; the generator matches the declared field type to the enum type symbol exactly, so a field typed as the case class is not part of the vocabulary. First-level derived cases are forced private, so type tests on cases are inexpressible outside the declaring type: case identity is deliberately unobservable, and consumers see exactly the item set plus dispatch.
- Generic smart enums generate the full surface per closed construction — items, lookups, dispatch, parsing, and operators are per-instantiation static state, so one generic declaration yields a family of independent vocabularies keyed by type argument. Only the component-model type-converter attribute is suppressed when the enum or any containing type is generic; factory and metadata conversion routes remain.

[ITEM_GRAPH_INITIALIZATION]:
- Item initializers execute in declaration order, so a row whose constructor argument references a later-declared item captures null silently — and the lookup materialization guard never catches it, because the guard checks that items and keys are non-null, not what references an item captured. Cross-row references must defer behind a delegate evaluated at call time (`static () => Other`) or derive as a lazy projection from the item list. The by-ref constructor-validation hook is the declaration-time place to reject null cross-references when eager capture was intended.
- Deferral is also the only resolution for cyclic row graphs — mutual successor/predecessor vocabularies admit no declaration order that satisfies eager capture, so the delegate indirection is structural, not defensive.

[METADATA_AND_GENERIC_ALGORITHMS]:
- Every enum implements a static, editor-hidden metadata property carrying: the lazy item table with key, item, and identifier (resolved through a generated index switch — no reflection), key conversion as both delegates and expression trees, a boxed key-validation closure, and the key and error types; a runtime lookup resolves it from a bare `Type`. Direct use trips TTRESG1000 (warning severity) — this is the serializer/ORM seam, not an application surface.
- The boxed validation closure coerces a wrong-typed key with `key is TKey k ? k : default` — a mis-typed boxed key validates the default key and fails with the empty-identifier message rather than a type error. Boundary code feeding boxed keys must pre-type them.
- The runtime metadata query walks base types, so a derived-case instance's concrete type resolves to the owning vocabulary's metadata — integrations handed a case instance find the family without special-casing. Results are cached per queried type in a process-wide concurrent map, and the single reflection touch (one static explicit-interface property read) happens once per type, ever. Candidate fast-rejection (primitives, arrays, language enums, pointers) and nullable unwrapping run before any cache interaction, so probing arbitrary types through the lookup in hot serializer paths is cheap and never poisons the cache with non-candidates.
- The shape-metadata layer is itself a generated closed union of six sealed cases: `Keyed.SmartEnum`, `KeylessSmartEnum`, `Keyed.ValueObject`, `ComplexValueObject`, `AdHocUnion`, and `RegularUnion` — dispatched through the same named-argument total `Switch`/`Map` grammar, including state-threaded and ref-struct-capable overloads, that instances use. Keyless and keyed vocabularies are distinct metadata cases, not a nullable-key variant of one case — generic tooling that probes vocabularies owns both arms explicitly, and the taxonomy turns a forgotten arm into a compile failure rather than a runtime surprise. A new shape kind arity-breaks every metadata dispatch site at compile time.
- The smart-enum interface inherits the object-factory interface, so one constraint carries `Items`/`Get`/`TryGet` and static `Validate` together; constraining on `IObjectFactory<T, TValue, TError>` alone widens the same algorithm to every keyed generated shape — vocabularies and keyed value objects alike. One admission bridge serves the whole program:

```csharp
public static Fin<T> Admit<T, TValue>(TValue raw)
   where T      : class, IObjectFactory<T, TValue, ValidationError>
   where TValue : notnull
   => T.Validate(raw, null, out var item) is { } error
      ? Fin<T>.Fail(Error.New(error.Message))
      : Fin<T>.Succ(item!);
```

- `StaticAbstractInvoker` forwards to `Validate`/`Parse`/`TryParse` from contexts that hold the constraint generically but cannot name static abstracts directly — expression trees and boxed dispatch are the primary consumers. It is an internal API tripping TTRESG1000 on direct use; the constraint is the public plane.
- `IConvertible<T>` is covariant in its output, so every vocabulary with a reference-typed key lifts to the object-keyed view and heterogeneous keyed shapes share one collection element type; value-typed keys do not lift — variance stops at boxing. Covariance composed with `StringKeyedObjectComparer<T>` yields cross-vocabulary key-equated sets: `new HashSet<IConvertible<string>>(StringKeyedObjectComparer<IConvertible<string>>.OrdinalIgnoreCase)` admits items from any string-keyed vocabulary or keyed single-value owner, equated and hashed by the chosen key policy in one container. The validation-error parameter of the enum and factory interfaces is covariant, so an algorithm constrained on a base error contract accepts vocabularies declaring richer custom error types without re-abstraction. The key-only enum interface (`ISmartEnum<TKey>`) is the minimal constraint when an algorithm needs key projection but neither lookup nor admission.
- `StringKeyedObjectComparer<T>` operates over anything implementing `IConvertible<string>` — vocabularies and string-keyed value objects equate and hash by key under ordinal or culture policy in one comparer. Its `OrdinalIgnoreCase` field matches the vocabulary's default case-insensitive identity policy; the other fields (`Ordinal`, `CurrentCulture`, etc.) diverge and require deliberate justification.
- Derived read-mostly indexes mirror the generator's own internal shape: one lazy frozen dictionary projected from `Items`, materialized once — never a per-call scan and never an eager static initializer that races item initialization, because projecting from `Items` forces materialization order correctly:

```csharp
private static readonly Lazy<FrozenDictionary<int, Strategy>> _byWeight =
   new(static () => Items.ToFrozenDictionary(static s => s.Weight),
       LazyThreadSafetyMode.ExecutionAndPublication);
```

- Keyed enums receive a generated type-converter wired to the validation pipeline — component-model binding admits through `Validate`, never through activation. Serialization framework participation is a flags knob defaulting to all generated serializers.
- Items are static state, and static state is per load context: an assembly loaded into multiple isolation contexts yields disjoint item universes whose instances fail both reference identity and type identity across the boundary. Vocabulary values cross isolation seams as keys re-admitted on the far side, never as instances. Lookup poisoning, item indices, and lazy materialization state are likewise per-context.

[SERIALIZER_SEAM]:
- Converter participation is decided at definition time: the generator stamps the converter-factory attribute directly onto the vocabulary type, so serializers bind without options registration, converter lists, or reflection walks. Generation activates per framework only when the matching serializer-integration assembly is referenced by the compilation; the per-enum framework flags then narrow within what is present, so a flags value naming an absent framework is inert, never an error.
- Suppression is per framework, not global: a hand-placed JSON converter attribute, alternative-JSON converter attribute, or binary formatter attribute on the type each back off exactly that framework's generated converter — a custom JSON converter coexists with the generated binary formatter. The suppression flags are tracked independently in generator state, so the escape hatch composes.
- The generated component-model converter chains the key type's own converter: any source type convertible to the key admits transitively into the vocabulary (text to numeric key to item), and both directions answer for the nullable forms of the enum and key types. Failed component-model admission throws a `FormatException` carrying the rendered validation error — this seam is a throwing boundary by contract, unlike the null-returning admission triad, so binding layers that route through it need the exception rail, not the validation rail.

[WIRE_SHAPE_REDIRECTION]:
- An object factory claiming serialization redirects the wire contract away from the key: for every framework the factory claims, the keyed converter generator stands down by construction — the two are mutually exclusive per framework. The wire can speak a projection (a rank, a code, a composite) while in-process identity, lookup, and logging keep speaking the key.
- The redirection contract is a triple, each analyzer-enforced: a hand-written static `Validate` with the exact factory signature (TTRESG061), an instance `ToValue()` returning the factory value whenever serialization or persistence claims the factory (TTRESG062), and `HasCorrespondingConstructor` must not be set to true on a vocabulary (TTRESG060). Factory multiplicity partitions cleanly: overlapping serialization claims (TTRESG070), a second model-binding factory (TTRESG069), or a second persistence factory (TTRESG068) are each errors, so every integration axis has exactly one owner.
- The generated key-conversion `ToValue` is an explicit interface implementation, so the factory's public `ToValue` does not collide even though the language forbids return-type-only overloads — the two conversion routes occupy different declaration spaces by design.
- Conversion resolution is factory-first and selects the last declared factory matching the integration's filter — object-factory attribute declaration order is a precedence list where later wins (`LastOrDefault` over the factory list); key conversion is only the fallback when no factory matches. A serialization-claiming factory therefore rewires every metadata-driven integration that resolves conversion at runtime, not merely the converters stamped on the type — wire-shape redirection propagates to integrations the declaration never names.

```csharp
[SmartEnum<string>]
[ObjectFactory<int>(UseForSerialization = SerializationFrameworks.All)]
public sealed partial class Tier
{
   public static readonly Tier Low  = new("low",  rank: 1);
   public static readonly Tier High = new("high", rank: 9);

   public int Rank { get; }

   public int ToValue() => Rank;

   public static ValidationError? Validate(int rank, IFormatProvider? provider, out Tier? item)
   {
      item = Items.FirstOrDefault(t => t.Rank == rank);
      return item is null ? ValidationError.Create($"Unknown rank: {rank}") : null;
   }
}
```

[DIAGNOSTIC_TOPOLOGY]:
- Severity stratification is itself the design signal: structural laws land as errors (partiality, private constructors, sealing, item publicity, named dispatch arguments, delegate-method shape, the factory triple, single shape attribute); the silent-vanishing hazards land as warnings (vocabulary with no items, static property mistaken for an item); allocation hygiene is info (non-static lambda in dispatch). A vocabulary-heavy codebase that does not promote those two warnings to errors has accepted that an item set can shrink without a build break.
- Custom key members are contract-checked, not conventional: a missing hand-written implementation and a key-type mismatch are errors that spell out the expected member, so owning the key member (private field, bespoke name) stays structurally verified against the generated lookups that consume it.
- Shape admission is single-owner twice over: stacking a vocabulary attribute with a value-object or union attribute is an error at the family level (TTRESG063), and a second vocabulary attribute is an error within the family (TTRESG064) — a type declares exactly one generated identity.

[TARGET_SURFACE_TOPOLOGY]:
- The span admission plane — span parsing, span lookup, span factory, span identifier projection, and the ref-struct-permitting dispatch constraints — is compiled conditionally inside the generated source. The public surface of one declaration is therefore a function of the compiled target. Multi-targeted libraries get per-target surfaces from a single vocabulary declaration, and shared source written against the span plane must be confined to targets that generate it.

[GENERATOR_OBSERVABILITY]:
- The generator reads compiler-visible build properties for a file logger (path, level, unique-path-per-process toggle, initial buffer size) and an opt-in counter that watermarks every generated file with a monotonically increasing `// COUNTER:` header. The counter is the direct instrument for incremental-generation churn — a counter that climbs on edits to unrelated files marks a broken incremental pipeline — and the log answers why a type did not generate with evidence instead of attribute inspection.
- When a code generator produces no code for a matched declaration, the generator logs a warning through this channel. Without a configured log path it degrades to self-logging errors only, so the file logger must be switched on before the evidence exists.
- Generated members carry tooling annotations (eager-invocation hints on dispatch callbacks) by default, with a build-property toggle to suppress them — relevant only when annotation assemblies collide at the IDE layer.

[DESIGN_PRESSURE]:
- Rows beat external dispatch when more than one consumer switches over the same vocabulary or when the selected behavior is policy the vocabulary owns: the delegate column is declared once, new items cannot omit it (constructor arity forces the column), and call sites shrink to `item.Behavior(args)`. Generated `Switch` wins for single-consumer, consumer-owned logic: totality is compiler-checked and the vocabulary stays free of consumer concerns. External dictionaries keyed by items are the rejected middle — they re-derive a correspondence a row or `Switch` already owns and miss new items silently where `Switch` arity-breaks loudly.
- Validity is a property of keys, not instances: no invalid item is constructible, every public instance is valid by definition, and the validity question collapses entirely into the admission triad. Code holding an item never re-checks; code holding a key chooses among exactly three admission verbs ranked by failure shape — throwing, boolean, typed error.
- The throwing surface is enumerable and intentional: `Get` and the explicit key conversion, `Parse`, lookup materialization on duplicate or null key, and the unreachable default arms of generated dispatch. Everything else is total — rails route through `Validate`/`TryGet`/`TryParse`, and a vocabulary whose call sites catch exceptions is using the wrong verb.
- Three or more sibling static lookup helpers, parallel dictionaries over the same items, or repeated full-coverage `Switch` calls with identical arms across consumers are collapse signals: the first two fold into derived frozen indexes or factory admission routes on the vocabulary itself; the last folds into a delegate row.
- The throwing admission verb raises a typed unknown-identifier exception carrying the enum `Type` and the offending key as structured data — the boundaries that must catch (the explicit key conversion, component-model binding) render evidence without parsing message text.
- `ProjectionEqualityComparer<TElement, TKey>` ships in the box — selector plus optional inner comparer, null-tolerant on element and projection — so derived item indexes and allow-list sets keyed by a row column take it directly instead of a hand-rolled comparer class.
- No-op dispatch arms have a canonical spelling: the empty-action family spans arities zero through sixteen, and a stateless action-form `Switch` arm takes the bare method group — with the compiler's static method-group conversion caching this is an allocation-free ignored arm, while the partial action overload remains the choice when most arms are ignored.
- A trim-or-nullify string utility — whitespace-only to null, trim, optional length cap — keeps declaration-time key normalization inside the by-ref constructor-validation hook a one-call expression.
