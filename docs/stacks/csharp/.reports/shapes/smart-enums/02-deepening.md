# [SMART_ENUMS_DEEPENING]

[SERIALIZER_SEAM]:
- Converter participation is decided at definition time: the generator stamps the converter-factory attribute directly onto the vocabulary type, so serializers bind without options registration, converter lists, or reflection walks. Generation activates per framework only when the matching serializer-integration assembly is referenced by the compilation; the per-enum framework flags then narrow within what is present, so a flags value naming an absent framework is inert, never an error.
- Suppression is per framework, not global: a hand-placed JSON converter attribute, alternative-JSON converter attribute, or binary formatter attribute on the type each back off exactly that framework's generated converter — a custom JSON converter coexists with the generated binary formatter. The suppression flags are tracked independently in generator state, so the escape hatch composes.
- String-keyed vocabularies receive a span-parsable converter factory: deserialization admits directly from raw character windows with no intermediate string. The dedicated opt-out knob (`DisableSpanBasedJsonConversion`) downgrades that one type to string-based conversion — the targeted fix when a wire producer emits escaped or chunked text that span admission would misread, without surrendering span conversion vocabulary-wide.
- The generated component-model converter chains the key type's own converter: any source type convertible to the key admits transitively into the vocabulary (text to numeric key to item), and both directions answer for the nullable forms of the enum and key types. Failed component-model admission throws a `FormatException` carrying the rendered validation error — this seam is a throwing boundary by contract, unlike the null-returning admission triad, so binding layers that route through it need the exception rail, not the validation rail.

[WIRE_SHAPE_REDIRECTION]:
- An object factory claiming serialization redirects the wire contract away from the key: for every framework the factory claims, the keyed converter generator stands down by construction — the two are mutually exclusive per framework, so the wire can speak a projection (a rank, a code, a composite) while in-process identity, lookup, and logging keep speaking the key.
- The redirection contract is a triple, each analyzer-enforced with the expected member spelled out in the diagnostic: a hand-written static `Validate` with the exact factory signature (TTRESG061), an instance `ToValue()` returning the factory value whenever serialization or persistence claims the factory (TTRESG062), and never a corresponding constructor on a vocabulary (TTRESG060) — items are looked up, not constructed. Factory multiplicity partitions cleanly: overlapping serialization claims (TTRESG070), a second model-binding factory (TTRESG069), or a second persistence factory (TTRESG068) are each errors, so every integration axis has exactly one owner.
- The generated key-conversion `ToValue` is an explicit interface implementation, so the factory's public `ToValue` does not collide even though the language forbids return-type-only overloads — the two conversion routes occupy different declaration spaces by design.

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

[RUNTIME_CONVERSION_RESOLUTION]:
- The runtime metadata query walks base types, so a derived-case instance's concrete type resolves to the owning vocabulary's metadata — integrations handed a case instance find the family without special-casing. Results are cached per queried type in a process-wide concurrent map, and the single reflection touch (one static explicit-interface property read) happens once per type, ever.
- Candidate fast-rejection (primitives, arrays, language enums, pointers) and nullable unwrapping run before any cache interaction, so probing arbitrary types through the lookup in hot serializer paths is cheap and never poisons the cache with non-candidates.
- Conversion resolution is factory-first and selects the last declared factory matching the integration's filter — object-factory attribute declaration order is a precedence list where later wins; key conversion is only the fallback when no factory matches. A serialization-claiming factory therefore rewires every metadata-driven integration that resolves conversion at runtime, not merely the converters stamped on the type — wire-shape redirection propagates to integrations the declaration never names.
- Keyless vocabularies still publish runtime metadata — the lazy item table pairing each instance with its field-name identifier — so generic tooling and diagnostic surfaces enumerate behavior-only vocabularies. Only key-conversion machinery is absent from their metadata shape.

[CROSS_SHAPE_GENERIC_PLANE]:
- The vocabulary interface inherits the object-factory interface, so one constraint carries `Items`/`Get`/`TryGet` and static `Validate` together; constraining on `IObjectFactory<T, TValue, TError>` alone widens the same algorithm to every keyed generated shape. One admission bridge serves the whole program — vocabularies and keyed value objects alike — and per-type `Admit` methods collapse into it:

```csharp
public static Fin<T> Admit<T, TValue>(TValue raw)
   where T      : class, IObjectFactory<T, TValue, ValidationError>
   where TValue : notnull
   => T.Validate(raw, null, out var item) is { } error
      ? Fin<T>.Fail(Error.New(error.Message))
      : Fin<T>.Succ(item!);
```

- The factory's value type parameter permits ref structs, so a character-window admission route is a legal factory dimension and the generic bridge admits from spans wherever the shape implements the span factory — zero-allocation admission generalizes; it is not a string-key special case.
- `Thinktecture.Internal.StaticAbstractInvoker` forwards to `Validate`/`Parse`/`TryParse` from contexts that hold the constraint generically but cannot name static abstracts directly — expression trees and boxed dispatch are the primary consumers. It is an internal API (tripping TTRESG1000 on direct use); the public plane is the constraint itself.
- `StringKeyedObjectComparer<T>` operates over anything key-convertible to string: vocabularies and string-keyed value objects equate and hash by key under ordinal or culture policy in one comparer, replacing per-type comparer declarations for boundary set and dictionary membership. Its `OrdinalIgnoreCase` field matches the vocabulary's default case-insensitive identity policy; the other fields (`Ordinal`, `CurrentCulture`, etc.) diverge and require deliberate justification.

[DIAGNOSTIC_TOPOLOGY]:
- Severity stratification is itself the design signal: structural laws land as errors (partiality, private constructors, sealing, item publicity, named dispatch arguments, delegate-method shape, the factory triple, single shape attribute); the silent-vanishing hazards land as warnings (vocabulary with no items, static property mistaken for an item); allocation hygiene is info (non-static lambda in dispatch). A vocabulary-heavy codebase that does not promote those two warnings to errors has accepted that an item set can shrink without a build break.
- Every item field must be public — a non-public static field of the enum type is an error, not a silently skipped member. A private scratch field of the enum type is legal only under `[IgnoreMember]`, which removes it from items, lookups, and dispatch entirely: there is no hidden-row tier, and row visibility is decided at the type level, never per row.
- Custom key members are contract-checked, not conventional: a missing hand-written implementation and a key-type mismatch are errors that spell out the expected member, so owning the key member (private field, bespoke name) stays structurally verified against the generated lookups that consume it.
- The delegate-row constraint pair — partial, non-generic — carries explicit guidance toward the inheritance tier: a generic per-case behavior cannot be a delegate field, so the diagnostic's remedy is abstract members on derived cases. The pair encodes the behavior-tier ladder as an enforced rule: delegate rows for monomorphic policy, inheritance for generic or multi-member case behavior.
- Shape admission is single-owner twice over: stacking a vocabulary attribute with a value-object or union attribute is an error at the family level, and a second vocabulary attribute is an error within the family — a type declares exactly one generated identity.

[ITEM_GRAPH_INITIALIZATION]:
- Item initializers execute in declaration order, so a row whose constructor argument references a later-declared item captures null silently — and the lookup materialization guard never catches it, because the guard checks that items and keys are non-null, not what references an item captured. Cross-row references must defer behind a delegate evaluated at call time (`static () => Other`) or derive as a lazy projection from the item list. The by-ref constructor-validation hook is the declaration-time place to reject null cross-references when eager capture was intended.
- Deferral is also the only resolution for cyclic row graphs — mutual successor/predecessor vocabularies admit no declaration order that satisfies eager capture, so the delegate indirection is structural, not defensive.

[LANGUAGE_DISPATCH_PRESSURE]:
- Items are static readonly singletons, not constants: a case label naming an item does not compile, so the language's own `switch` can never be total over a vocabulary, and a property pattern over the key re-enters string literals while silently dropping items added later. Generated `Switch`/`Map` is therefore not a convenience layer — it is the only dispatch the type system can hold total under item addition.
- First-level derived cases are forced private, so type tests on cases are inexpressible outside the declaring type: case identity is deliberately unobservable, and consumers see exactly the item set plus dispatch. Item equality operators (reference identity, optional key overloads) support guards, but a guard chain over items re-derives the correspondence one dispatch call already owns.

[TARGET_SURFACE_TOPOLOGY]:
- The span admission plane — span parsing, span lookup, span factory, span identifier projection, and the ref-struct-permitting dispatch constraints — is compiled conditionally inside the generated source. The public surface of one declaration is therefore a function of the compiled target. Multi-targeted libraries get per-target surfaces from a single vocabulary declaration, and shared source written against the span plane must be confined to targets that generate it.

[GENERATOR_OBSERVABILITY]:
- The generator reads compiler-visible build properties for a file logger (path, level, unique-path-per-process toggle, initial buffer size) and an opt-in counter that watermarks every generated file with a monotonically increasing `// COUNTER:` header. The counter is the direct instrument for incremental-generation churn — a counter that climbs on edits to unrelated files marks a broken incremental pipeline — and the log answers why a type did not generate with evidence instead of attribute inspection.
- When a code generator produces no code for a matched declaration, the generator logs a warning through this channel. Without a configured log path it degrades to self-logging errors only, so the file logger must be switched on before the evidence exists.
- Generated members carry tooling annotations (eager-invocation hints on dispatch callbacks) by default, with a build-property toggle to suppress them — relevant only when annotation assemblies collide at the IDE layer.
