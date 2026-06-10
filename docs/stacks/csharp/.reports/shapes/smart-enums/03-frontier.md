# [SMART_ENUMS_FRONTIER]
[SPAN_FACTORY_ADMISSION]:
- The object-factory attribute's type parameter carries `allows ref struct`, so a character-window admission route is declared literally as `[ObjectFactory<ReadOnlySpan<char>>]` — the factory value type of a vocabulary can be a span, and the hand-written `Validate` composes span parsing of the raw text with the generated key lookup.
- The span factory is exclusively a non-string-key instrument: a string-keyed vocabulary already generates the span-shaped `Validate`, and declaring the span factory there produces a duplicate-member compile error (CS0111) — the collision is the design statement that string keys own the span route natively and a factory in that position re-declares what exists.
- The generic plane reaches spans without naming the enum: a bridge constrained on `IObjectFactory<T, ReadOnlySpan<char>, TError>` calls the static abstract `Validate` directly. The static-abstract forwarding utility that ships beside it is internal-API-marked and trips the internal-API usage diagnostic (TTRESG1000) — the public plane is the constraint itself; the utility exists only for layers that cannot name static abstracts, such as expression trees and boxed dispatch.

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

[LOOKUP_POISONING]:
- On targets that compile the span plane, the string-key lookup factory acquires the span alternate view unconditionally — even under a custom comparer accessor — and the alternate view demands a comparer implementing the span-alternate equality interface. Every prebuilt accessor and every BCL string comparer, culture-sensitive families included, satisfies it; a hand-rolled comparer does not, and `EqualityComparer<string>.Create` is the standard way to produce exactly such a comparer. The mismatch raises no analyzer finding and compiles clean; it surfaces as an invalid-operation failure at the first lookup touch.
- The lookup `Lazy` is publication-locked with a factory delegate, so any materialization failure — duplicate key, null item, null key, unsupported alternate comparer — caches the thrown exception instance itself: every later `Items`, `Get`, `TryGet`, `Validate`, parse, conversion, or serializer touch rethrows the identical instance for the life of the process. Recovery is restart-only, which upgrades the startup probe that forces `Items` from hygiene to the only place this class of defect is cheap.
- Because the alternate view is compiled per target, one declaration with a non-alternate custom comparer materializes cleanly on a target below the span plane and poisons itself above it — runtime behavior diverges per compiled target with zero source difference. A custom comparer accessor therefore carries an implicit contract: implement the span-alternate interface on its comparer, or pin to a BCL comparer.
[ORDER_ALGEBRA]:
- A keyed vocabulary carries two generated total orders that agree only by accident: `Items` enumerates declaration order — the same order that fixes dispatch indices and callback positions — while `CompareTo` and the comparison operators rank by key under the configured comparer, ordinal-case-insensitive for unconfigured string keys. Items declared omega-then-alpha enumerate in that order and sort alpha-then-omega; any consumer that sorts `Items` has silently left declaration order.
- The comparer accessor is one policy point that swings lookup, hash seed, `CompareTo`, and comparison operators together, with no per-surface override — a vocabulary whose domain rank differs from key order encodes rank as an item column and sorts by projection, never by bending the comparer to fake a domain order.
[GENERIC_CASE_TIER]:
- A generic per-case behavior is inexpressible as a delegate row — delegate fields cannot be generic — so the working shape is an abstract generic member overridden by private nested sealed cases. The owner must then be declared abstract (an abstract member inside a non-abstract enum class is CS0513), and abstractness is legal precisely because the sealing requirement already lifts when derived cases exist.
- Nesting is what keeps the topology closed: nested cases reach the generated private constructor through enclosing-type access, so case construction needs no accessibility widening, and private first-level cases keep case identity unobservable — consumers hold exactly the declared items, with generic dispatch riding virtual calls instead of delegate fields.

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

[KEY_PROJECTION_VARIANCE]:
- The key-conversion interface is covariant in its output, so every vocabulary with a reference-typed key lifts to the object-keyed view `IConvertible<object>` and heterogeneous keyed shapes share one collection element type; value-typed keys do not lift — variance stops at boxing — and their conversion view stays vocabulary-specific.
- Covariance composed with the string-key comparer family yields cross-vocabulary key-equated sets with no adapter: `new HashSet<IConvertible<string>>(StringKeyedObjectComparer<IConvertible<string>>.OrdinalIgnoreCase)` admits items from any string-keyed vocabulary or keyed single-value owner, equated and hashed by the chosen key policy in one container.
- The validation-error parameter of the enum and factory interfaces is covariant, so an algorithm constrained on a base error contract accepts vocabularies declaring richer custom error types without re-abstraction; the key-only enum interface is the minimal constraint when an algorithm needs key projection but neither lookup nor admission.
- The attribute's key-member-type property is a runtime-readable mirror, not an input: generation derives the key type from the attribute's generic argument, and assigning the property changes nothing in the generated surface — it is reflection metadata for runtime integrations, never an override knob.
[METADATA_TAXONOMY]:
- The shape-metadata layer is itself a generated closed union: six sealed cases — keyed vocabulary, keyless vocabulary, keyed single-value owner, complex owner, and two union forms — dispatched through the same named-argument total `Switch`/`Map` grammar, including state-threaded and ref-struct-capable overloads, that instances use. Tooling over vocabularies inherits the closed-world evolution guarantee at the metadata level: a new shape kind arity-breaks every metadata dispatch site at compile time.
- Keyless and keyed vocabularies are distinct metadata cases, not a nullable-key variant of one case — generic tooling that probes vocabularies owns both arms explicitly, and the taxonomy turns a forgotten arm into a compile failure rather than a runtime surprise.
[ISOLATION_TOPOLOGY]:
- Items are static state, and static state is per load context: an assembly loaded into multiple isolation contexts yields disjoint item universes whose instances fail both reference identity and type identity across the boundary. Vocabulary values cross isolation seams as keys re-admitted on the far side, never as instances; lookup poisoning, item indices, and lazy materialization state are likewise per-context.
[SUPPORT_SURFACES]:
- The throwing admission verb raises a typed unknown-identifier exception carrying the enum `Type` and the offending key as structured data — the boundaries that must catch (the explicit key conversion, component-model binding) render evidence without parsing message text.
- A projection equality comparer ships in the box — selector plus optional inner comparer, null-tolerant on element and projection — so derived item indexes and allow-list sets keyed by a row column take it directly instead of a hand-rolled comparer class.
- No-op dispatch arms have a canonical spelling: the empty-action family spans arities zero through sixteen, and a stateless action-form `Switch` arm takes the bare method group — with the compiler's static method-group conversion caching this is an allocation-free ignored arm, while the partial action overload remains the choice when most arms are ignored.
- A trim-or-nullify string utility — whitespace-only to null, trim, optional length cap — keeps declaration-time key normalization inside the by-ref constructor-validation hook a one-call expression.
