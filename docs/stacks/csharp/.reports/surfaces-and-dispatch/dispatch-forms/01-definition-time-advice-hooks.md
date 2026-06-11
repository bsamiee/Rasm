# Definition-Time Advice Hooks Over Generated Dispatch Surfaces

[ADVICE_HOOK_CHAIN]:
- Every keyed generated owner emits three partial-method advice points that fire in a fixed order around construction, and they are the sole sanctioned weaving seams on the dispatch surface — no constructor body, no factory wrapper, no post-construction initializer exists outside them. `ValidateFactoryArguments` runs first inside the factory path; the object is constructed only if it produced no error; the constructor itself then re-runs `ValidateConstructorArguments`; the live instance finally receives `FactoryPostInit`. The chain is the definition-time advice stack: pre-construction normalization, construction-time invariant, post-construction completion.
- `ValidateFactoryArguments` is emitted as `static partial ... ValidateFactoryArguments(ref TError? validationError, ref TKey key)` — every argument including the key passes by `ref`, so the hook both rejects (set `validationError`) and rewrites (mutate `key` in place: trim, canonicalize case, clamp) before the value is ever stored. This is the only place pre-construction mutation is legal; the factory reads the mutated `key` to construct. A hook that only rejects and never rewrites is the same seam under-used, not a different one.
- `ValidateConstructorArguments` is emitted as `static partial void ValidateConstructorArguments(ref TKey key)` and is invoked unconditionally inside the generated constructor, after the factory's validation hook and again on any direct construction path that bypasses the factory. It is the last-line guard with `ref` mutation power: an invariant placed here holds even when a deserializer or EF materializer constructs the owner without routing through `Validate`. Placing a normalization in the factory hook alone leaves the direct-construction path unprotected; the constructor hook is the closure of that gap.
- `FactoryPostInit` is emitted as `partial void FactoryPostInit(...)` — an instance method, not static — and receives the live, fully-constructed owner as `this`. It is the only advice point that can observe the constructed value and derive secondary state from it. When `ValidateFactoryArguments` is declared to return a non-`void` type (the factory-validation return type), that returned value is threaded as the single parameter of `FactoryPostInit`: the pre-construction hook computes a side value (a parsed sub-structure, an expensive lookup result) and hands it forward to post-init without re-computing it. This forward channel is the non-obvious capability — `ValidateFactoryArguments` is normally read as a `void` validator, but a non-`void` return turns the pre/post pair into a single computation split across the construction boundary.

```csharp
// ValidateFactoryArguments returns a non-void carrier; FactoryPostInit consumes it.
// ref key is normalized in-place before construction; the constructor hook re-guards
// the direct (deserializer/EF) path that bypasses the factory entirely.
[ValueObject<string>]
public sealed partial class RouteKey
{
    private string[]? _segments;

    // Pre-construction: reject + rewrite + forward a derived carrier to post-init.
    private static partial string[]? ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string value)
    {
        value = value.Trim().ToLowerInvariant();
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        validationError = parts.Length is 0 ? ValidationError.Create("empty route") : null;
        return parts;
    }

    // Post-construction: receives the carrier; no re-parse.
    partial void FactoryPostInit(string[]? factoryArgumentsValidationError) =>
        _segments = factoryArgumentsValidationError;

    // Last-line guard on the direct-construction path.
    static partial void ValidateConstructorArguments(ref string value) =>
        value = value.Trim().ToLowerInvariant();
}
```

[KEY_TO_ITEM_ADMISSION_BRIDGE]:
- A keyed smart enum emits `static TError? Validate(TKey? key, IFormatProvider?, out TItem item)` whose body is `if (TItem.TryGet(key, out item)) return null; else return <no-such-item error>`. This is the generated boundary-admission entrypoint that converts a raw external key into a dispatch-ready item in one hop: the returned item is exactly the closed-vocabulary value that the generated `Switch`/`Map` then dispatches over. A hand-written `FromName` lookup followed by a separate `Switch` is two hops where the generated `Validate`-then-`Switch` is the canonical one — `Validate` is the admission and `Switch` is the dispatch, and nothing sits between them.
- The companion `static bool TryGet(TKey, out TItem)` is the allocation-free probe the `Validate` body delegates to; for `ReadOnlySpan<char>` keys a `Validate(ReadOnlySpan<char>, ...)` overload is emitted under the modern target guard, giving zero-allocation admission of protocol text directly into the dispatch vocabulary without a string materialization. The span overload is the correct edge for wire-shaped key decoding into a closed dispatch surface.
- The key→item bridge is exhaustive on the item side and open on the key side by construction: an unknown key yields a validation error, a known key yields a vocabulary item, and the subsequent `Switch` is compile-time-total over the items. The split places openness (any external key string) and closedness (the finite item set) on opposite sides of a single generated seam.

[ADDITIONAL_CONSTRUCTION_SURFACE_AS_ASPECT]:
- `[ObjectFactory<TValue>]` is applied multiply on one owner and weaves an additional `static abstract TError? Validate(TValue?, IFormatProvider?, out TOwner?)` construction surface keyed on a value type other than the canonical key — a parallel admission path from, e.g., an integer code, a tuple, or a foreign struct, each with its own factory-validation hook. The owner thereby admits raw material through several typed front doors while keeping one interior shape; the discriminant is the input value's static type, not a mode flag. This is arity-and-shape collapse applied to construction: several `FromX`/`ParseX` factories become one attribute-directed family of `Validate` overloads.
- The attribute carries boundary-routing flags that bind the extra factory to specific seams as definition-time policy rather than runtime wiring: `UseForSerialization` (scoped to named serialization frameworks) makes that value type the serialized representation; `UseWithEntityFramework` registers it as the EF value-conversion shape; `UseForModelBinding` exposes it to the model binder; `HasCorrespondingConstructor` asserts a matching constructor so the round-trip is structural. Each flag attaches a cross-cutting boundary concern at the definition site, so the codec, the persistence converter, and the binder all resolve to the same generated factory with no per-boundary adapter.
- The owner implements `IObjectFactory<TOwner, TValue, TError>` with `static abstract TError? Validate(TValue?, IFormatProvider?, out TOwner?)` per declared factory and exposes the full set through `IObjectFactoryOwner.ObjectFactories` as a metadata list. A boundary that must enumerate the construction surfaces of an unknown owner (a generic codec, a generic binder) discovers every admission path through that static-abstract metadata rather than reflection over constructors — the construction surface is itself a dispatchable, statically-typed vocabulary.

```csharp
// One interior shape, three typed admission front doors, each routed to a boundary.
// The discriminant is the argument's static type; no mode flag re-describes the input.
[ValueObject<Guid>]
[ObjectFactory<string>(UseForModelBinding = true, HasCorrespondingConstructor = true)]
[ObjectFactory<int>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public sealed partial class EntityId
{
    static ValidationError? IObjectFactory<EntityId, string, ValidationError>.Validate(
        string? value, IFormatProvider? provider, out EntityId? item) =>
        Guid.TryParse(value, out var g)
            ? (item = Create(g), default(ValidationError)).Item2
            : (item = null, ValidationError.Create("bad id")).Item2;

    static ValidationError? IObjectFactory<EntityId, int, ValidationError>.Validate(
        int value, IFormatProvider? provider, out EntityId? item) =>
        (item = Create(Deterministic(value)), default(ValidationError)).Item2;
}
```

[KEY_BEHAVIOR_CHANNELS_GOVERNING_DISPATCH]:
- `[KeyMemberEqualityComparer<TAccessor, TKey>]` and `[KeyMemberComparer<TAccessor, TKey>]` bind the owner's equality and ordering to a static-abstract accessor — `TAccessor : IEqualityComparerAccessor<TKey>` exposing `static abstract IEqualityComparer<TKey> EqualityComparer`, and the comparer counterpart for `IComparer<TKey>`. This is the definition-time channel that decides how the key participates in every value-keyed dispatch downstream: the same accessor governs the owner's own `Equals`/`GetHashCode` and is the comparer a frozen table or dictionary keyed by the owner must use to stay consistent. Choosing an ordinal-ignore-case accessor at definition time, for instance, makes case-insensitive frozen-table dispatch correct by construction rather than by remembering to pass the right comparer at every call site.
- `ConversionToKeyMemberType` and `ConversionFromKeyMemberType` independently select `None`/`Implicit`/`Explicit` for the two directions of key↔owner conversion (the canonical posture is implicit out, explicit in). The asymmetry is load-bearing for dispatch ergonomics: implicit out lets the owner be passed wherever the raw key is expected (frozen-table key, span lookup argument) with no ceremony, while explicit in forces every inbound raw key through a visible cast so admission is never silent. Setting both to `None` severs the owner from its key entirely, forcing all construction through the validated factory path — the strictest admission posture.
- `ConversionToKeyMemberType = Implicit` is what makes an owner usable as a `FrozenDictionary` key against a raw-key table without an explicit `.Value` projection: the owner converts to its key implicitly at the lookup site. The conversion-operator posture and the comparer accessor together determine whether owner-keyed and raw-key-keyed dispatch tables interoperate — both must agree on the same `TKey` equality, and the accessor is the single source that guarantees it.

[GENERATION_SUPPRESSION_AS_DISPATCH_POLICY]:
- `SwitchMethods` and `MapMethods` are independent `SwitchMapMethodsGeneration` axes on the same owner: an owner may generate `Map` (constant value-row table) while suppressing `Switch` (runtime continuations), or the reverse. Suppressing the form the vocabulary does not need is the dispatch-policy statement that the owner carries only the dispatch shape its consumers legitimately use — a data-carriage owner suppresses both and is inspected only by structural pattern, a constant-verdict owner keeps `Map` and drops `Switch`.
- `SkipIComparable`, `SkipIParsable`/`SkipISpanParsable` (the span form follows the value parsable), `SkipIFormattable`, and `SkipToString` each suppress a generated boundary surface, and their suppression is dispatch-relevant: `IParsable`/`ISpanParsable` is the generated inbound admission a binding or deserialization boundary dispatches through, and `IFormattable`/`ISpanFormattable` is the outbound projection a formatting boundary dispatches through. Keeping these surfaces is what lets a single owner be the admission and projection vocabulary at a wire boundary without a separate codec; skipping them is the explicit statement that this owner never crosses that boundary, so no consumer can accidentally bind to a surface the design forbids.
- `ComparisonOperators` (`OperatorsGeneration`) governs whether the owner emits `<`/`>`/`<=`/`>=` derived from its key ordering; combined with the comparer accessor, this is the channel that makes range-pattern and relational-guard dispatch over the owner legal. An owner whose dispatch never uses relational guards sets it to `None`, removing an operator surface that would otherwise invite ordering semantics the vocabulary does not define.
