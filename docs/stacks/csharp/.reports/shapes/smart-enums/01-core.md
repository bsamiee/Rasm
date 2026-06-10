# [SMART_ENUMS_CORE]

[DECLARATION_AND_ITEM_SET]:
- The generator binds only to partial `class` declarations; records, structs, and primary constructors are rejected. All constructors must be private, and a vocabulary without derived cases must be `sealed` — the closed-world guarantee is analyzer-enforced at error severity, not conventional.
- An item is exactly a static, explicitly declared field whose declared type equals the enum type symbol and that is not marked `[IgnoreMember]`. A static property typed as the enum compiles and silently vanishes from `Items`, lookups, and `Switch` arity — the analyzer emits only a warning. A field declared with a derived case type instead of the base type vanishes the same way. Item-set membership is a declared-type fact, never a value fact.
- Field declaration order is load-bearing: it fixes each item's index, the generated `Switch`/`Map` parameter order, and the metadata identifier table. Index-based dispatch is exactly why unnamed call-site arguments are a compile error — named arguments are the reorder shield.
- Every item is constructed through one generated private constructor: key first, then each assignable non-abstract instance field/property in declaration order, then one trailing parameter per behavior delegate. `static partial void ValidateConstructorArguments(ref TKey key, ref ...)` receives every argument by ref before assignment — declaration-time normalization runs once per item, never per lookup.
- The key member generates as `public TKey Key { get; }` by default; kind, access modifier, and name are attribute knobs, and the default name flips to `_key` when generated as a private field. A user-declared key member is honored when found; absence or type mismatch is an analyzer error.
- Keyless `[SmartEnum]` is the behavior-only vocabulary: `Items`, `Switch`/`Map`, reference equality with identity hash; no `Get`/`TryGet`/`Validate`, no conversions, no parsing, no `ToString` override. Reach for it when rows carry policy and nothing crosses a wire keyed by the row.
- An item-less vocabulary is a warning. `Switch`/`Map` generate only for 1–999 items; at 1,000 or more the type silently retains lookups but loses generated dispatch — that cliff marks where the concept stopped being a vocabulary.

[ADMISSION_AND_LOOKUP]:
- All lookup state hides behind one `Lazy<Lookups>` with `ExecutionAndPublication`: a frozen dictionary, the item list, and — for string keys — a `FrozenDictionary<,>.AlternateLookup<ReadOnlySpan<char>>`. Materialization walks the items once and throws on null items, null keys, and duplicate keys. Fail-fast is deferred to first lookup, not type load, so a duplicate-key defect hides until the first `Items`/`Get`/`TryGet` touch — force `Items` in a startup probe or law test to surface it deterministically.
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

- String-keyed vocabularies admit spans end-to-end: `Get`/`TryGet`/`Validate` span overloads ride the alternate lookup with zero string allocation, and the enum implements the span-shaped factory interface so serializers admit from raw character windows; span-based JSON conversion is on by default with a targeted opt-out knob.
- Null never reaches the dictionary: nullable key types are rejected at declaration, `Get(null)` null-propagates (`NotNullIfNotNull`) instead of throwing, and `TryGet(null)` is false — absence belongs to the caller's `Option`, not to the vocabulary.
- `IParsable<T>`/`ISpanParsable<T>` are generated: string-keyed enums parse via string `Validate`; non-string keys parse raw text into the key type first, then validate. `Parse` throws `FormatException` carrying the validation error text; `TryParse` is total. Skipping `IParsable` transitively skips `ISpanParsable`.
- The generated `Validate` ignores its `IFormatProvider` argument; the parameter exists to unify signatures with hand-written and factory `Validate` implementations where culture matters. Pass null at internal call sites.
- `[ObjectFactory<TValue>]` opens additional admission routes — for example, numeric input into a string-keyed vocabulary. The enum must hand-implement `static TError? Validate(TValue, IFormatProvider?, out T?)` (enforced), must not claim a corresponding constructor (enforced — items are looked up, never constructed), and needs `ToValue` only when the factory drives serialization.
- Custom error types attach via `[ValidationError<TError>]` with `TError : IValidationError<TError>` requiring a static abstract `Create(string)`. The generated failure message is a fixed format, and `Create` receives only the rendered message — the offending key is not passed through. Structured errors that need the key must capture it at the bridge, not inside the error type.

[KEY_POLICY_AND_IDENTITY]:
- String keys are ordinal-case-insensitive by default across every surface at once: the lookup dictionary, the span alternate lookup, the constructor-computed hash seed, key equality, and `CompareTo` — case-insensitive admission with case-preserving storage. Two items differing only in key case are a duplicate-key materialization failure, not two items.
- Key policy attaches as a type, not an instance: `[KeyMemberEqualityComparerAttribute<TAccessor, TKey>]` and `[KeyMemberComparerAttribute<TAccessor, TKey>]` take comparer-accessor types exposing static `EqualityComparer`/`Comparer` properties, so policy is available at generation time and in static contexts. Prebuilt accessors in `ComparerAccessors` cover the ordinal/invariant/current-culture matrix. Mismatched accessor/key types and one-sided comparer declarations (equality comparer without comparer, or vice versa) are analyzer findings.
- Instance identity is reference identity: `Equals(T)` is `ReferenceEquals`, and `GetHashCode` returns an int computed once in the constructor from the key through the configured comparer — set and dictionary membership over items costs a field read, and hash semantics stay aligned with lookup semantics by construction.
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

- A custom delegate type is generated only when a parameter carries ref, in, or out, or when `DelegateName` is set; otherwise plain `Action`/`Func` backs the field. `DelegateName` renames both the delegate type and the constructor argument. Delegated methods must be partial and non-generic; declared accessibility is preserved, so private behavior rows are legal.
- A behavior-row method dispatches through one delegate-field invocation — no `Switch`, no index read. The cost model is one delegate field per behavior per item, so N parallel behaviors sharing inputs collapse into one delegate returning a composite value rather than N columns.
- Plain readonly delegate-typed instance properties also become constructor parameters and invoke identically at call sites. `[UseDelegateFromConstructor]` differs by hiding the delegate behind a true method surface — ref-kind parameters, documented contracts, and no public delegate-valued state.
- Abstract members are the third behavior tier: excluded from generated constructor parameters and implemented per derived case. Justified only when a case needs its own state or overrides several members coherently; otherwise delegate rows beat subclassing on density.

[DERIVED_CASES_AND_GENERIC_VOCABULARIES]:
- Derived-case topology is prescribed: the enum stays unsealed only because derived types exist; first-level nested derived classes must be private, and deeper levels are public — public instances remain exactly the declared items while case classes stay invisible. For every base constructor, each derived case receives a mirrored constructor (own members first, then base arguments, with name collisions deduplicated by numeric suffix).
- A derived-case instance is an item only when its field is declared with the base type; the generator matches the declared field type to the enum type symbol exactly, so a field typed as the case class is not part of the vocabulary.
- Generic smart enums generate the full surface per closed construction — items, lookups, dispatch, parsing, and operators are per-instantiation static state, so one generic declaration yields a family of independent vocabularies keyed by type argument. Only the component-model type-converter attribute is suppressed when the enum or any containing type is generic; factory and metadata conversion routes remain.

[METADATA_AND_GENERIC_ALGORITHMS]:
- Every enum implements a static, editor-hidden metadata property carrying: the lazy item table with key, item, and identifier (resolved through a generated index switch — no reflection), key conversion as both delegates and expression trees, a boxed key-validation closure, and the key and error types; a runtime lookup resolves it from a bare `Type`. Direct use trips the internal-API analyzer — this is the serializer/ORM seam, not an application surface.
- The boxed validation closure coerces a wrong-typed key with `key is TKey k ? k : default` — a mis-typed boxed key validates the default key and fails with the empty-identifier message rather than a type error. Boundary code feeding boxed keys must pre-type them.
- The smart-enum interface exposes `Items`/`Get`/`TryGet` as static abstracts and chains the factory `Validate`, so vocabulary-generic algorithms need no reflection:

```csharp
static Option<T> Find<T, TKey>(TKey key)
   where T    : class, ISmartEnum<TKey, T, ValidationError>
   where TKey : notnull
   => T.TryGet(key, out var item) ? Some(item) : None;
```

- Derived read-mostly indexes mirror the generator's own internal shape: one lazy frozen dictionary projected from `Items`, materialized once — never a per-call scan and never an eager static initializer that races item initialization, because projecting from `Items` forces materialization order correctly:

```csharp
private static readonly Lazy<FrozenDictionary<int, Strategy>> _byWeight =
   new(static () => Items.ToFrozenDictionary(static s => s.Weight),
       LazyThreadSafetyMode.ExecutionAndPublication);
```

- Keyed enums receive a generated type-converter wired to the validation pipeline — component-model binding admits through `Validate`, never through activation. Serialization framework participation is a flags knob defaulting to all generated serializers.

[DESIGN_PRESSURE]:
- Rows beat external dispatch when more than one consumer switches over the same vocabulary or when the selected behavior is policy the vocabulary owns: the delegate column is declared once, new items cannot omit it (constructor arity forces the column), and call sites shrink to `item.Behavior(args)`. Generated `Switch` wins for single-consumer, consumer-owned logic: totality is compiler-checked and the vocabulary stays free of consumer concerns. External dictionaries keyed by items are the rejected middle — they re-derive a correspondence a row or `Switch` already owns and miss new items silently where `Switch` arity-breaks loudly.
- Validity is a property of keys, not instances: no invalid item is constructible, every public instance is valid by definition, and the validity question collapses entirely into the admission triad. Code holding an item never re-checks; code holding a key chooses among exactly three admission verbs ranked by failure shape — throwing, boolean, typed error.
- The throwing surface is enumerable and intentional: `Get` and the explicit key conversion, `Parse`, lookup materialization on duplicate or null key, and the unreachable default arms of generated dispatch. Everything else is total — rails route through `Validate`/`TryGet`/`TryParse`, and a vocabulary whose call sites catch exceptions is using the wrong verb.
- Three or more sibling static lookup helpers, parallel dictionaries over the same items, or repeated full-coverage `Switch` calls with identical arms across consumers are collapse signals: the first two fold into derived frozen indexes or factory admission routes on the vocabulary itself; the last folds into a delegate row.
