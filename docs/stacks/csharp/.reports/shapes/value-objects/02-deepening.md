# Generated Value Objects — Deep Mechanisms and Failure Surfaces

[OWNER_SHAPE_IMPOSITION]:
- The generated partial declares the modifiers, not the author: every reference-type owner is emitted `sealed partial class` and every struct owner `readonly partial struct` — derivability and member mutability are closed by generation. An author-declared `abstract` or a mutable field collides at compile time rather than by convention.
- The read-only discipline reaches inherited state: fields and properties of a base class must also be read-only (TTRESG034/035), so a value-object owner cannot launder mutability through a base type.
- The auto layout attribute on struct owners is suppressed when the author declares an explicit layout attribute — interop-shaped owners are possible at the cost of owning the consequences; the default remains auto.
- A type parameter in the key position must carry a `notnull` constraint (TTRESG074) — generic owners cannot quietly admit nullable type arguments into identity.
- `SkipEqualityComparison` deletes `IEquatable<T>`, both equality operators, and the `Equals`/`GetHashCode` overrides in one cut: a class owner silently degrades to reference identity and a struct owner to reflection-based field equality. Flipping it without supplying replacements turns a value type into an entity type with no diagnostic.

[VALIDATION_STATE_THREADING]:
- `ValidateFactoryArguments` is signature-polymorphic on its return type: declaring the partial with a non-`void` return changes the generated declaration to `private static partial TState ValidateFactoryArguments(...)` and rewires `FactoryPostInit` to accept that value — `Validate` captures the hook's return into a local and passes it to `FactoryPostInit(state)` after construction. Expensive intermediate results computed during validation (parsed components, derived caches) flow to post-construction without a second pass and without widening any public surface.

```csharp
private static partial (int Major, int Minor) ValidateFactoryArguments(
   ref TagError? validationError, ref string value)
{
   var parsed = TryParseParts(value);
   validationError = parsed is null ? TagError.Create("<message-a>") : null;
   return parsed ?? default;
}

partial void FactoryPostInit((int Major, int Minor) state);
```

- The threaded value exists only on the factory path; constructor-route materialization invokes neither hook nor `FactoryPostInit`, so any state reconstructed from it must be derivable again or the rehydrated instance silently lacks it — the same asymmetry that governs normalization governs this side channel.

[OPERATOR_NULL_ALGEBRA]:
- The generated operator family encodes four distinct null contracts on one class owner: equality operators are null-tolerant (`null == null` is true); comparison operators throw `ArgumentNullException` via `ThrowIfNull` on either operand; arithmetic operators likewise throw `ArgumentNullException`; the unsafe owner→key conversion throws `NullReferenceException` deliberately. Code that treats `<` as null-safe because `==` is will fault at runtime.
- The key→owner conversion operator on a class owner with a reference-type key null-propagates before admission: a null key casts to a null owner with no factory call and no error, even when the null-yield factory modes are off. The cast is therefore not a strict `Create` alias — it is `Create` for non-null inputs and identity-null otherwise; a pipeline that admits via cast accepts an absence the factory would reject.
- Interface-typed and `object`-typed keys suppress the entire conversion-operator matrix in both directions; identity escape for such owners is only the explicit-interface `ToValue()`.

[RAW_KEY_EQUALITY_OVERLOADS]:
- `EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads` generates `owner == rawKey` and `rawKey == owner` backed by one private static `Equals(owner, key)` that applies the owner's comparer policy — string keys compare via the configured comparer accessor, exactly as owner-to-owner equality does. Boundary code can test an admitted owner against unadmitted raw material without constructing a second owner and without re-validating.
- The raw-side operand is never validated: `owner == "<value-a>"` happily compares against a value that could not be admitted, returning false rather than failing — equality against an impossible raw value is indistinguishable from inequality against a possible one.

[COMPARISON_SURFACE_DERIVATION]:
- `CompareTo` and the comparison operators derive independently: `CompareTo` routes accessor → string-policy comparer → a cast of the key to `IComparable<TKey>`, while the operators forward the key's own `<`, `<=`, `>`, `>=` directly when no accessor is configured. With a comparer accessor both surfaces route through `Comparer.Compare`, restoring agreement — one more reason the explicit-comparer diagnostics treat accessor pairing as a single policy.
- Operator availability is detected per operator from the key's `op_*` method set, not only from generic-math interface implementation — custom key types with hand-written operators qualify. The `IComparisonOperators<T,T,bool>` interface declaration is emitted only when all four operators exist (or an accessor or string policy covers them); a key with a partial operator set yields the matching partial operator subset on the owner with no generic-math interface, so the owner silently fails `where T : IComparisonOperators<T,T,bool>` constraints while supporting `<` at call sites.
- `CompareTo(object)` returns 1 for null and throws `ArgumentException` for foreign types — sorted non-generic containers fail loudly on heterogeneous content rather than ordering arbitrarily.

[ARITHMETIC_REENTRY_MECHANICS]:
- Renaming the throwing factory renames the arithmetic interior: operator bodies call the configured create-method name, so a `From`-renamed owner's `+` re-enters admission through `From`. The `checked` operator variants wrap only the key arithmetic in `checked(...)` and still route the result through the throwing factory — one `checked +` therefore has two failure species: `OverflowException` from the key math and `ValidationException` from re-admission.
- Narrow integer keys insert a truncating cast in the unchecked path: `byte + byte` promotes to `int` and is cast back before the factory call, so unchecked overflow wraps silently and then validates the wrapped value — an owner whose invariant tolerates the full key range will admit a wrapped result with no signal. Range-invariant owners get accidental protection; full-range owners get silent modular arithmetic.
- Mixed-operand overloads null-check only the owner side on class owners; the raw side is the key's own domain.

[PROVIDER_FLOW_AND_TYPECONVERTER]:
- `Create` and `TryCreate` hard-code a null `IFormatProvider` into their `Validate` call; the provider-aware admission entries are `Validate` itself, `Parse`/`TryParse` (which thread the provider through both the key parse and the validation), and the component-model converter (which forwards the ambient culture). A culture-sensitive validation partial is unreachable from the bare factory pair — provider-dependent admission must enter through `Validate` or the parse surface by design.
- The single generic `TypeConverter` is transitive through the key's own converter in both directions: any source type the key's converter accepts becomes admissible, and any destination it can produce becomes an egress, with validation applied at the key→owner hop. Converter-route admission failures throw `FormatException` carrying the flattened error text — not `ValidationException` — and converting null to a struct owner throws `NotSupportedException`.
- The converter attribute is not emitted for generic owners or owners nested inside generic types: component-model binding and designer support silently see no converter on generic owners while the rest of the generated surface is intact.

[COMPLEX_EQUALITY_MECHANICS]:
- Per-member equality dispatch is a fixed four-tier chain: explicit accessor, then the owner-level string policy for direct string members, then null-tolerant `a.Equals(b)` for reference members, then direct `.Equals` for value members. No tier consults `EqualityComparer<T>.Default` for references — a collection-typed member (`IReadOnlyList<T>`, arrays) therefore compares by the collection's own `Equals`, which is reference identity. The only sanctioned fix is an explicit member comparer accessor implementing sequence semantics.
- `GetHashCode` has two shapes: a `HashCode.Combine` fast path used only when equality membership has fewer than eight members, zero accessors, and zero string members; otherwise incremental `HashCode.Add` with the comparer threaded per member. Adding one string member or one accessor flips the whole method's form, keeping hash and equality policy-aligned by construction.
- An equality membership of zero members generates `Equals` returning constant true and `GetHashCode` returning 0 — every instance is one equivalence class. Structurally legal for marker values; a trap when members were removed from equality incrementally.

[DEFAULT_GATE_PROPAGATION]:
- The zero-init gate escapes the owner: any embedding type declaring an instance field or settable property whose type is a default-disallowing struct owner draws the must-be-`required` diagnostic (TTRESG104) unless the member is initialized inline, read-only, static, less accessible than its container, or already `required`. The generated admission discipline thereby disciplines every composition site in the graph.
- The generator computes disallowance independently of the requested permission: a complex struct owner with `AllowDefaultStructs = true` still emits the `IDisallowDefaultValue` marker when any member's type denies defaults — the computed state overrides the attribute setting, so downstream embedding diagnostics key off the computed truth, never the requested one.

[ALTERNATE_ADMISSION_WEAVE]:
- `[ObjectFactory<TValue>]` weaves `IObjectFactory<TOwner,TValue,TError>` onto the owner unconditionally, but `IConvertible<TValue>` — and therefore the `ToValue()` egress obligation — only when the factory participates in serialization or persistence; a parse-only factory is one-way by type system, not just by convention.
- `HasCorrespondingConstructor = true` is a persistence-trust declaration: a single-parameter constructor of the factory value type that persistence materialization uses directly, bypassing `Validate` on the premise that the store holds already-admitted truth. Wire deserialization and model binding ignore the flag and always route through `Validate` — the trust boundary is persistence-only by design.
- The factory value type admits `ref struct` arguments end-to-end (`where TValue : allows ref struct` on both the attribute and the factory interface): a `ReadOnlySpan<char>` factory gives allocation-free wire admission where the deserializer transcodes UTF-8 directly to a char span and calls the span `Validate`; span pattern matching against known literals then admits hot values with zero allocation. Only the span-capable serialization gate supports it — declaring it for other frameworks buys nothing.
- A string object factory on a complex owner unlocks the parse surface: `Parse`/`TryParse` route through the static-abstract `Validate` over `string`, so a multi-member owner gains string-shaped admission — and with it host parameter binding via the parsable interface — while keeping its member-wise factory triad intact.

[GENERIC_ADMISSION_ARROW]:
- The static-abstract factory interface makes admission itself generic: one boundary arrow validates any generated owner from any of its admission key types with the typed error preserved until the single rail projection. Type inference cannot recover the error type from the constraint, so call sites name all three arguments — the cost of one polymorphic admitter over per-owner bridge proliferation.

```csharp
public static Fin<TOwner> Admit<TOwner, TRaw, TError>(TRaw raw, IFormatProvider? provider = null)
   where TOwner : IObjectFactory<TOwner, TRaw, TError>
   where TRaw : notnull
   where TError : class, IValidationError<TError> =>
   TOwner.Validate(raw, provider, out var owner) is { } error
      ? Fin<TOwner>.Fail(Error.New(error.ToString() ?? "<message-a>"))
      : Fin<TOwner>.Succ(owner!);
```

- The runtime's internal infrastructure namespace is policed by a dedicated analyzer diagnostic on every use site outside generated code; generated types carry a per-type suppression. Boundary code reusing the internal admission arrows pays one auditable warning per use site — the intended audit trail for stepping off the public surface.

[SERIALIZATION_EGRESS_GAP]:
- Without the converter-generating integration assembly, reflection-based wire serialization sees a keyed class owner with the default private key field as an empty object — `{}` out, data silently gone — and cannot rehydrate it through the private constructor, failing at deserialization time. The always-generated component-model converter does not participate in this path; the gap is invisible at compile time because the serialization flags are dormant metadata until the integration assembly exists.
- The struct variant is worse than failure: reflection deserialization of a struct owner zero-initializes and sets nothing, producing exactly the unvalidated default instance the compile-time gate exists to forbid — admission bypass by the serializer, with no exception and no diagnostic. Wire exposure of any owner is a hard dependency on its converter gate, not an optimization.

[GENERATOR_OBSERVABILITY]:
- The generator is build-observable through MSBuild properties: a log-file path property (folder or file-name template), a log-level property spanning trace through error, and a uniqueness toggle that separates one log file per generator instance from a shared file; on logger failure it falls back to a diagnostic file in the temporary directory. Generation pathologies on owners — stale surfaces, missing factories, incremental-cache misses — are diagnosed from these logs rather than from speculation about the build.
