# [SMART_ENUMS]

[VOCABULARY_DECLARATION_AND_ITEM_SET]:
- Controlling rule: a smart-enum vocabulary is a sealed partial class with private constructors whose items are public static readonly fields declared with exactly the enum type — index, dispatch arity, callback order, and metadata identifiers all derive from the field declaration list, so the declaration list is the vocabulary.
- The generator binds only to partial `class` declarations; records, structs, and primary constructors are rejected. All constructors must be private, and a vocabulary without derived cases must be `sealed` — the closed-world guarantee is analyzer-enforced at error severity, not conventional.
- Item membership is a declared-type fact, never a value fact: a static property typed as the enum compiles and silently vanishes from `Items`, lookups, and `Switch` arity (warning only); a field declared with a derived case type vanishes identically. A non-public static field of the enum type is an error — there is no hidden-row tier; `[IgnoreMember]` is the only removal and it removes from items, lookups, and dispatch at once, so row visibility is decided at the type level, never per row.
- Severity stratification is itself the design signal: structural laws (partiality, private constructors, sealing, item publicity, named dispatch arguments, delegate-method shape) land as errors; the silent-vanish hazards (item-less vocabulary, static property mistaken for an item) land as warnings. A vocabulary-heavy codebase that does not promote those two warnings to errors has accepted that an item set can shrink without a build break.
- Declaration order is load-bearing: it fixes each item's index, the generated `Switch`/`Map` parameter order, and the metadata identifier table — index-based dispatch is exactly why unnamed call-site arguments are a compile error.
- Constructor protocol: every item is constructed through one generated private constructor — key first, then each assignable non-abstract instance member in declaration order, then one trailing parameter per behavior delegate. `static partial void ValidateConstructorArguments(ref TKey key, ref ...)` receives every argument by ref before assignment — declaration-time normalization runs once per item, never per lookup; the in-box trim-or-nullify string utility (whitespace-only to null, trim, optional length cap) keeps key normalization inside the hook a one-call expression.
- The key member generates as `public TKey Key { get; }` by default; kind, access, and name are attribute knobs, and the default name flips to `_key` when generated as a private field. A user-declared key member is honored when found and contract-checked, not conventional: a missing implementation or key-type mismatch is an error that spells out the expected member, so a bespoke key member stays structurally verified against the generated lookups that consume it.
- Keyless `[SmartEnum]` is the behavior-only vocabulary: `Items`, `Switch`/`Map`, reference equality with identity hash; no `Get`/`TryGet`/`Validate`, no conversions, no parsing, no `ToString` override. It still publishes runtime metadata — the lazy item table pairing each instance with its field-name identifier — so generic tooling enumerates behavior-only vocabularies; only key-conversion machinery is absent. Reach for keyless when rows carry policy and nothing crosses a wire keyed by the row.
- Boundary: `Switch`/`Map` generate only for 1–999 items; at 1,000 or more the type silently retains lookups but loses generated dispatch — that cliff marks where the concept stopped being a vocabulary. An item-less vocabulary is a warning.

[KEY_POLICY_AND_IDENTITY]:
- String keys are ordinal-case-insensitive by default across every surface at once — lookup dictionary, span alternate lookup, constructor-computed hash seed, key equality, `CompareTo` — case-insensitive admission with case-preserving storage. Two items differing only in key case are a duplicate-key materialization failure, not two items.
- Key policy is one policy point that swings lookup, hash seed, `CompareTo`, and the comparison operators together, with no per-surface override — a vocabulary whose domain rank differs from key order encodes rank as an item column and sorts by projection, never by bending the comparer to fake a domain order.
- Instance identity is reference identity: `Equals(T)` is `ReferenceEquals`, and `GetHashCode` returns an int computed once in the constructor from the key through the configured comparer — set and dictionary membership over items costs a field read, and hash semantics stay aligned with lookup semantics by construction.
- A keyed vocabulary carries two generated total orders that agree only by accident: `Items` enumerates in declaration order — the same order fixing dispatch indices and callback positions — while `CompareTo` and the comparison operators rank by key under the configured comparer. Items declared omega-then-alpha enumerate that way and sort alpha-then-omega; any consumer that sorts `Items` has silently left declaration order.
- The operator surface is generic-math-shaped: `IEqualityOperators<T,T,bool>` by default, plus `IEqualityOperators<T,TKey,bool>` under the key-overload setting so `item == key` compares without conversion. Comparison operators imply equality operators — the settings coerce each other in both directions and the inconsistent combination is flagged.
- Conversions default asymmetric by design: enum-to-key implicit and null-propagating; key-to-enum explicit because it routes through throwing `Get`. Both are suppressed when the key type is an interface or `object`, and either direction can be set to none for boundary-only vocabularies.
- `IComparable`/`IComparable<T>`/`IFormattable` ride the key's own capabilities and are individually skippable. `ToString` returns the key's `ToString`, so logs and interpolation speak the wire vocabulary by default.

[KEYED_LOOKUP_AND_THROW_SURFACE]:
- Controlling rule: validity is a property of keys, not instances — no invalid item is constructible, every public instance is valid by definition, and the validity question collapses entirely into a closed admission triad ranked by failure shape: `Get` throws a typed unknown-identifier exception and backs the explicit key-to-enum conversion (explicit precisely because it can throw); `TryGet([AllowNull] key, out item)` is the bool form; `Validate(key, IFormatProvider?, out item)` returns the typed validation error or null. Code holding an item never re-checks; code holding a key chooses exactly one of the three verbs.
- The throwing surface is enumerable and intentional: `Get` and the explicit key conversion, `Parse`, lookup materialization on duplicate or null key, and the unreachable default arms of generated dispatch. Everything else is total — a vocabulary whose call sites catch exceptions is using the wrong verb. The unknown-identifier exception carries the enum `Type` and the offending key as structured data, so the boundaries that must catch (explicit conversion, component-model binding) render evidence without parsing message text.
- Null never reaches the dictionary: nullable key types are rejected at declaration, `Get(null)` null-propagates (`NotNullIfNotNull`) instead of throwing, and `TryGet(null)` is false — absence belongs to the caller's `Option`, not to the vocabulary.
- String-keyed vocabularies admit spans end-to-end: `Get`/`TryGet`/`Validate` span overloads ride a `FrozenDictionary<,>.AlternateLookup<ReadOnlySpan<char>>` with zero string allocation; span-based JSON conversion is on by default with a targeted opt-out knob (`DisableSpanBasedJsonConversion`) for wire producers emitting escaped or chunked text that span admission would misread — opted out per declaration without surrendering span conversion vocabulary-wide. String keys own the span route natively: redeclaring a span-shaped object factory on a string-keyed vocabulary is CS0111 — the collision is the design statement.
- Custom-comparer hazard at the span plane: on targets compiling the span plane, the string-key lookup factory acquires the alternate view unconditionally, and the alternate view demands a comparer implementing the span-alternate equality interface. Every prebuilt accessor and BCL string comparer satisfies it; a comparer produced via `EqualityComparer<string>.Create` — the standard ad hoc idiom — does not. No analyzer finding, compiles clean, surfaces as `InvalidOperationException` at the first lookup touch; one declaration materializes cleanly below the span plane and poisons itself above it — runtime behavior diverges per compiled target with zero source difference. A custom comparer therefore carries an implicit contract: implement the span-alternate interface or use a BCL comparer.
- `IParsable<T>`/`ISpanParsable<T>` are generated: string-keyed enums parse via string `Validate`; non-string keys parse raw text into the key type first, then validate. `Parse` throws `FormatException` carrying the validation error text; `TryParse` is total. Skipping `IParsable` transitively skips `ISpanParsable`.
- The generated `Validate` ignores its `IFormatProvider` argument; the parameter exists to unify signatures with hand-written and factory `Validate` implementations where culture matters — pass null at internal call sites.
- Custom error types attach via `[ValidationError<TError>]` with `TError : IValidationError<TError>` requiring static abstract `Create(string)`. The generated failure message is a fixed format and `Create` receives only the rendered text — the offending key is not passed through, so structured errors that need the key must capture it outside the error type.

[DISPATCH_INDEX_PROTOCOL]:
- Dispatch is an integer jump table, not a lookup: each item carries a write-once reference cell initialized to a negative sentinel; the materializer walks items in declaration order and compare-exchanges each cell from sentinel to position, throwing on a second assignment. The index is a runtime fact established exactly once, on first touch of the lookup or item list, equal to declaration order by construction — `Switch` over N items compiles to a native `switch` on a field read, with no dictionary probe, no virtual call, no delegate array.
- The hot-path read is deliberately non-volatile and inlined: a non-negative result returns with no fence; a negative result means the cell has not yet been published to this thread, so the slow path forces materialization by touching the item list, then re-reads through a volatile load to acquire the publication — one field read and a branch on the fast path, a fence paid only by the first dispatch that races materialization on a given thread. Dispatch issued before any admission therefore self-heals: it pays materialization on its own thread rather than failing.
- The negative sentinel doubles as the not-yet-materialized signal — absence of an index and absence of materialization are the same state, so staleness needs no separate flag. The cell is one sealed heap allocation per item for the life of the type, bought back by the jump-table collapse.
- Index assignment, lookup construction, and the duplicate-key fail-fast are one atomic single pass: a vocabulary whose materialization throws never assigns indices, so dispatch over a poisoned vocabulary fails at the index-read's materialization touch, never with a stale jump into a half-built table. The frozen lookup is built from the mutable build dictionary with the same comparer instance, so build-time and freeze-time equality laws cannot diverge.

[TOTAL_AND_PARTIAL_DISPATCH]:
- Total dispatch emits a real `switch` over the index read with one case per item in declaration order and a default arm throwing an out-of-range exception that interpolates the instance itself — routed through the key-derived `ToString`, so a corrupted-dispatch diagnostic speaks the wire vocabulary. The default arm is structurally unreachable for a correctly materialized vocabulary; its existence is the witness that materialization completed, not a branch callers can reach.
- Totality is enforced by method arity, not by analyzer passes over call sites: adding an item appends a case, shifts no existing index, and adds a parameter to every total overload — the type system itself refuses to compile a stale exhaustive dispatch, the closed-vocabulary evolution guarantee made mechanical. `SwitchPartially`'s `@default` is the explicit, visible opt-out of totality.
- Unnamed callback arguments at `Switch`/`Map` call sites are an analyzer error (the leading state argument alone is exempt): index-based dispatch would silently rebind positional callbacks when items are added or reordered, so the argument name is the binding contract and the reorder shield.
- Generated `Switch`/`Map` is the only dispatch the type system can hold total: items are static readonly singletons, not constants — a case label naming an item does not compile, so the language's own `switch` can never be total over a vocabulary; a property pattern over the key re-enters string literals while silently dropping items added later; a guard chain over equality operators re-derives the correspondence one dispatch call already owns and misses new items identically.
- The overload matrix covers action and func forms, each stateless and state-threaded, plus `SwitchPartially`/`MapPartially` under the partial-overloads setting. State-threaded callbacks receive only the state — item identity is positional and never passed. Both `TState` and `TResult` allow ref structs, so span-carrying contexts thread through dispatch without boxing; non-static lambdas draw an info-level diagnostic steering toward static lambdas plus the state overload — closure-free dispatch is analyzer-encoded posture:

```csharp
var verdict = item.Switch(
   state: (score, threshold),
   alpha: static s => s.score >= s.threshold,
   beta:  static s => s.score >  s.threshold * 2);
```

- `Map` is the value-row form — one `TResult` per item, no lambdas. `MapPartially` parameters are `Argument<TResult>`, an implicit-conversion ref struct whose `IsSet` distinguishes omitted from explicitly default, so null and zero remain legal mapped values.
- Partial-arm asymmetry: partial switch takes nullable per-item callbacks defaulting to null and emits a per-case null guard that breaks to the trailing default — the omitted-arm signal and the explicitly-null-arm signal are identical, because partiality over delegates is a presence test and a delegate has no value distinct from absence; a caller cannot pass null to mean "do nothing and skip the default." Partial map's presence flag resolves exactly what partial switch cannot: it breaks on unset rather than on null, so it carries every legal result value including the type's default.
- The default-arm contract differs between the partial families for one mechanical reason: the func-shaped fall-through is invoked unconditionally after the switch and must produce a value, so its `@default` is required non-null; the action-shaped fall-through is a null-conditional invocation, so its default tolerates null — and an action-form partial dispatch with an omitted default makes every unhandled case a silent no-op indistinguishable from success.
- Callback parameter names derive from item field names through acronym-aware camel-casing (leading underscores stripped, an initial uppercase run lowered as a unit) and are `@`-escaped in generated code — keyword-named items remain legal, and the state parameter is renameable via an attribute knob when an item name would collide with it.
- No-op arms have a canonical spelling: the empty-action family spans arities zero through sixteen, and a stateless action-form arm takes the bare method group — allocation-free under the compiler's static method-group conversion caching — while the partial action overload remains the choice when most arms are ignored.

[BEHAVIOR_ROWS]:
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

- A behavior-row method dispatches through one delegate-field invocation — no `Switch`, no index read. The cost model is one delegate field per behavior per item, so N parallel behaviors sharing inputs collapse into one delegate returning a composite value rather than N columns.
- Constructor arity forces the column: a new item cannot omit a behavior — the row contract is structural, not reviewed.
- A custom delegate type is generated only when a parameter carries ref, in, or out, or when `DelegateName` is set; otherwise plain `Action`/`Func` backs the field. `DelegateName` renames both the delegate type and the constructor argument. Delegated methods must be partial and non-generic — delegate fields cannot be generic.
- The tier ladder: plain readonly delegate-typed instance properties also become constructor parameters and invoke identically — `[UseDelegateFromConstructor]` differs by hiding the delegate behind a true method surface (ref-kind parameters, documented contracts, no public delegate-valued state). Abstract members are the third tier — excluded from generated constructor parameters, implemented per derived case — justified only when a case needs its own state or overrides several members coherently; otherwise delegate rows beat subclassing on density.
- Design pressure: rows beat external dispatch when more than one consumer switches over the same vocabulary or when the selected behavior is policy the vocabulary owns — the delegate column is declared once, new items cannot omit it, and call sites shrink to `item.Behavior(args)`. Generated `Switch` wins for single-consumer, consumer-owned logic: totality is compiler-checked and the vocabulary stays free of consumer concerns. External dictionaries keyed by items are the rejected middle — they re-derive a correspondence a row or `Switch` already owns and miss new items silently where `Switch` arity-breaks loudly. Repeated full-coverage `Switch` calls with identical arms across consumers are the fold-to-row signal.

[DERIVED_CASES_AND_GENERIC_VOCABULARIES]:
- A generic per-case behavior takes the inheritance tier, because delegate fields cannot be generic: declare the enum `abstract`, override with private nested sealed cases. Abstractness is legal precisely because the sealing requirement lifts when derived cases exist; nesting keeps the topology closed because nested cases reach the generated private constructor through enclosing-type access — case construction needs no accessibility widening and case identity stays unobservable:

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

- Derived-case topology is prescribed: first-level nested derived classes must be private, deeper levels are public — public instances remain exactly the declared items while case classes stay invisible; type tests on cases are inexpressible outside the declaring type, so consumers see exactly the item set plus dispatch.
- A derived-case instance is an item only when its field is declared with the base type — the generator matches the declared field type to the enum type symbol exactly, so a field typed as the case class is not part of the vocabulary.
- For every base constructor, each derived case receives a mirrored constructor (own members first, then base arguments, name collisions deduplicated by numeric suffix).
- Generic smart enums generate the full surface per closed construction — items, lookups, dispatch, parsing, and operators are per-instantiation static state, so one generic declaration yields a family of independent vocabularies keyed by type argument. Only the component-model type-converter attribute is suppressed when the enum or any containing type is generic; factory and metadata conversion routes remain.

[ITEM_GRAPH_INITIALIZATION]:
- Item initializers execute in declaration order, so a row whose constructor argument references a later-declared item captures null silently — and the materialization guard never catches it, because the guard checks that items and keys are non-null, not what a row captured. Cross-row references must defer behind a delegate evaluated at call time (`static () => Other`) or derive as a lazy projection from the item list.
- The by-ref constructor-validation hook is the declaration-time place to reject null cross-references when eager capture was intended.
- Deferral is the only resolution for cyclic row graphs — mutual successor/predecessor vocabularies admit no declaration order that satisfies eager capture, so the delegate indirection is structural, not defensive.

[MATERIALIZATION_LIFECYCLE]:
- The entire runtime state of a keyed vocabulary funnels through one publication-locked lazy (`ExecutionAndPublication`) whose payload holds the frozen dictionary, the optional span alternate view, and the item list; every admission verb opens with a read of that lazy's value and `Items` is its list. A keyless vocabulary substitutes an items-only lazy — the two are mutually exclusive per declaration, so no vocabulary ever runs two materializers that could both publish indices, and "is this vocabulary initialized" reduces exactly to "has the lazy's value been read."
- Materialization is one closure walking items in declaration order: reject a null item, reject a null key, reject a duplicate key, assign the dispatch index, append. Index correspondence, lookup contents, and fail-fast are products of the same pass — every consumer reading through the public item accessor inherits the validated, fully-indexed view or the original failure, never an intermediate.
- Fail-fast is deferred to first lookup touch, not type load, so a duplicate-key defect hides until the first `Items`/`Get`/`TryGet` touch. The publication-locked factory caches the thrown exception instance itself: every later `Items`, lookup, parse, conversion, or serializer touch rethrows the identical instance for the life of the process. Recovery is restart-only — which upgrades forcing `Items` in a startup probe from hygiene to the only place this defect class is cheap.
- Poisoning is transitive to the metadata plane: the generated metadata's item table projects over the vocabulary's own public item accessor — a second lazy stacked on the first — so a serializer or persistence layer that merely walks metadata items triggers the identical process-lifetime poisoning the application's own admission would; wire infrastructure is not insulated from a declaration defect by living on the metadata plane. That plane is the serializer/ORM seam, not an application surface — direct use trips a warning-severity diagnostic.
- The identifier table inside the metadata projection is an index switch mapping ordinal position to the source field-name literal — declaration order is the join key between the materialized item list and the compile-time-known identifiers, so runtime name recovery costs no reflection and no per-item name field.
- The metadata record itself materializes in the type's static constructor as a static auto-property initializer, but carries only delegates and lazies and forces no item materialization — a vocabulary referenced but never admitted from pays only the record allocation; heavy work is uniformly deferred to first lazy touch, never type load.
- Derived-index happens-before law: a secondary index keyed on an item column must project from the item accessor, never eager-initialize from a static field — reading through the item list inserts a happens-before edge through the materialization lazy, so the projection cannot begin until the single pass that assigned every index and validated every key has published. An eager static initializer can run before that pass and observe items whose index cells still hold the sentinel and whose constructor arguments still hold unresolved cross-references — the materialization order is the synchronization primitive, and projecting from the item accessor is how a consumer subscribes to it:

```csharp
private static readonly Lazy<FrozenDictionary<int, Strategy>> _byWeight =
   new(static () => Items.ToFrozenDictionary(static s => s.Weight),
       LazyThreadSafetyMode.ExecutionAndPublication);
```

- `ProjectionEqualityComparer<TElement, TKey>` ships in the box — selector plus optional inner comparer, null-tolerant on element and projection — so derived item indexes and allow-list sets keyed by a row column take it directly instead of a hand-rolled comparer class. Three or more sibling static lookup helpers or parallel dictionaries over the same items fold into derived frozen indexes on the vocabulary itself.
- Erased-plane failure shapes: the metadata's compiled from-key delegate routes through the throwing lookup verb (typed unknown-identifier exception), while the weakly-typed try-get routes through null-returning `Validate` with a boxed-key value pattern that coerces a wrong-typed key to the default key — a type fault is swallowed into an admission failure that reads as an empty or zero key. A generic caller holding boxed keys must pre-type them before entering the erased plane, because on that plane type mismatch and zero-key admission are indistinguishable. Each conversion delegate has an expression-tree twin carrying the same body — the delegate is the in-memory path, the lambda the translatable one, and the two planes cannot drift.
- Items are static state, and static state is per load context: an assembly loaded into multiple isolation contexts yields disjoint item universes whose instances fail both reference identity and type identity across the boundary — vocabulary values cross isolation seams as keys re-admitted on the far side, never as instances. Lookup poisoning, item indices, and lazy materialization state are likewise per-context.

[SERIALIZER_SEAM_AND_WIRE_REDIRECTION]:
- Converter participation is decided at definition time: the generator stamps the converter-factory attribute directly onto the vocabulary type, so serializers bind without options registration, converter lists, or reflection walks. Generation activates per framework only when the matching serializer-integration assembly is referenced by the compilation; the per-enum framework flags then narrow within what is present — a flags value naming an absent framework is inert, never an error.
- Suppression is per framework, not global: a hand-placed JSON converter attribute, alternative-JSON converter attribute, or binary formatter attribute each back off exactly that framework's generated converter; the suppression flags are tracked independently, so the escape hatch composes — a custom JSON converter coexists with the generated binary formatter.
- The generated component-model converter chains the key type's own converter: any source type convertible to the key admits transitively (text to numeric key to item), and both directions answer for the nullable forms of the enum and key types. Failed component-model admission throws a `FormatException` carrying the rendered validation error — this seam is a throwing boundary by contract, unlike the null-returning admission triad, so binding layers routing through it need the exception rail, not the validation rail.
- Wire-shape redirection: an object factory claiming serialization stands the keyed converter down per framework by construction — the two are mutually exclusive per framework — so the wire can speak a projection (a rank, a code, a composite) while in-process identity, lookup, and logging keep speaking the key.
- The redirection contract is an analyzer-enforced triple: a hand-written static `Validate` with the exact factory signature (TTRESG061), an instance `ToValue()` returning the factory value whenever serialization or persistence claims the factory (TTRESG062), and `HasCorrespondingConstructor` must not be set on a vocabulary (TTRESG060 — items are looked up, never constructed). Factory multiplicity partitions cleanly: overlapping serialization claims, a second model-binding factory, or a second persistence factory are each errors — every integration axis has exactly one owner.
- The generated key-conversion `ToValue` is an explicit interface implementation, so the factory's public `ToValue` does not collide even though the language forbids return-type-only overloads — the two conversion routes occupy different declaration spaces by design.
- Conversion resolution is factory-first and selects the last declared factory matching the integration's filter — object-factory attribute declaration order is a precedence list where later wins; key conversion is only the fallback when no factory matches. A serialization-claiming factory therefore rewires every metadata-driven integration that resolves conversion at runtime, not merely the converters stamped on the type — wire-shape redirection propagates to integrations the declaration never names:

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

[AOT_AND_TARGET_SURFACE]:
- A non-generic vocabulary's serializer participation is fully static: the converter-factory attribute is stamped with both type arguments closed at generation time — no runtime generic construction, no dynamic code; trim- and AOT-clean by construction.
- A generic vocabulary cannot close its converter at generation time, so the generator emits a file-scoped converter factory whose create path constructs the closed converter from the runtime type via runtime generic instantiation — a dynamic-code operation. The same declaration is AOT-clean closed and dynamic-code-dependent open: leaving a vocabulary generic moves its wire path across the AOT cliff while its in-process surface stays identical.
- Generic vocabularies additionally lose only the component-model type-descriptor seam; the factory and metadata conversion routes survive — admission is never the casualty; design-time binding and the JSON path are.
- The span admission plane — span parsing, span lookup, span factory, span identifier projection, and the ref-struct-permitting dispatch constraints — is compiled conditionally inside the generated source. The public surface of one declaration is therefore a function of the compiled target: multi-targeted libraries get per-target surfaces from a single vocabulary declaration, and shared source written against the span plane must be confined to targets that generate it.
