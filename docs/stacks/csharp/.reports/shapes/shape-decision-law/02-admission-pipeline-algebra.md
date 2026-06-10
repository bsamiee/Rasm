# Admission Pipeline Algebra

[SINGLE_FUNNEL_DERIVATION]:
- Every ingress verb on a generated owner is a thin projection of one generated `Validate` method, and the projections are mechanical: the boolean verb is `validationError = Validate(...); return validationError is null;`, the throwing verb is `Validate(...); if (validationError is not null) throw; return obj!;`, and the zero-arg overloads forward through the same `Validate` with `out _` discarding the error. There is exactly one place where raw material becomes evidence; everything else is a return-shape adapter over it. This is why disabling factory generation strips parsing, conversion, serialization, and the marker interface in one cascade — none are free-standing surfaces, all are `Validate` re-projections, and an owner that suppresses factories has structurally no admission and therefore no boundary capability of any kind.
- The funnel is two nested validation layers, not one, and they answer to different failure shapes. The outer layer is factory-scoped and error-returning: null guards on non-nullable reference members fire first and short-circuit to a typed error before any user code runs, then a user `ValidateFactoryArguments(ref error, ref ...members)` hook normalizes and rejects. The inner layer is constructor-scoped and throw-style: the generated constructor invokes `ValidateConstructorArguments(ref ...members)` before assigning a single field. The outer layer is the boundary admission path; the inner layer is the invariant-of-record that fires on every construction route including the ones that bypass the factory.
- The two layers partition by whether the caller is trusted. A consumer holding raw wire or user input routes the error-shaped outer layer and reads a typed error; a rehydration or deserialization route reconstructs through the bypass constructor and only the inner throw-style layer guards it. An invariant that must hold on stored data already admitted under an older rule belongs in `ValidateConstructorArguments`, because that is the only hook the rehydration constructor runs; an invariant that may legitimately reject fresh input belongs in `ValidateFactoryArguments`, because that is the only hook that produces a recoverable error instead of an exception.

[REF_NORMALIZATION_PROTOCOL]:
- The factory hook takes every member by `ref`, including the error slot, and the generated `Validate` reads the members back after the call. Normalization is therefore in-place mutation of the candidate before construction, not a returned replacement: trimming, casing, rounding, or canonicalizing a member writes through the `ref` and the constructed instance carries the normalized form, while the original submitted value is gone unless the hook captures it. The candidate that reaches the constructor is the post-normalization value, so the constructor-level invariant sees only canonical input and never re-normalizes.

```csharp
[ValueObject<string>]
public sealed partial class Handle
{
    private static partial string? ValidateFactoryArguments(
        ref ValidationError? validationError, ref string value)
    {
        var submitted = value;
        value = value.Trim().ToLowerInvariant();
        validationError = value switch
        {
            { Length: 0 } => ValidationError.Create("<blank>"),
            _ when value.AsSpan().ContainsAnyExceptInRange('a', 'z') =>
                ValidationError.Create("<charset>"),
            _ => null,
        };
        return submitted.Length == value.Length ? null : submitted;
    }
}
```

- The hook's return type is a free design axis with a load-bearing side effect: a `void` return leaves the hook a plain normalizer, while any non-`void` return flips the generated hook declaration to `private` and forwards the returned value into a generated `partial void FactoryPostInit(TReturn? factoryArgumentsValidationError)` invoked on the freshly constructed instance. Evidence computed once during static validation — the pre-normalization original, a parsed sidecar, a derived classification — is carried across the construction boundary and delivered to the live instance, with the unimplemented `FactoryPostInit` compiling to nothing. The return type is the channel for one-shot evidence that the members themselves cannot hold because they are already `init`-frozen by the time the instance exists.
- The post-init hook runs only on the success branch: construction and `FactoryPostInit` are inside `if (validationError is null)`, so a rejected candidate never constructs and never posts. This makes the pair a transactional admission — the side effect a `FactoryPostInit` performs (a one-time registration, a derived-field publish, a diagnostic emit) is reached if and only if the value is genuinely admitted, with no half-constructed instance observable to it.

[FAULT_SHAPE_FANOUT]:
- One `Validate` fans into four framework-native fault shapes, each a different boundary's idiom, and the typed error survives on exactly one of them. The throwing factory verb raises a data-annotations validation exception carrying only the error's string form; the parse-interface routes raise a format exception with the same text; the generated JSON converter raises a JSON exception with the text; a keyed-vocabulary miss raises an unknown-identifier exception that uniquely carries the owner type and the offending key as structured data rather than flattened text. A boundary that catches by exception type around heterogeneous owners must enumerate all four or leak one route, and a boundary that needs the structured error object must route the error-returning `Validate` and never the throwing verbs.
- The fanout constrains custom error-type design at the type level. The static creation contract accepts a bare string and every framework-origin failure arrives through it, so a custom error vocabulary with mandatory structured fields is unconstructable on parse, deserialization, and conversion routes — those paths can only call the string factory. Structured payloads survive solely on routes the owner itself populates inside `ValidateFactoryArguments`; every custom error type therefore needs a designed degenerate text-only case, and any consumer reading the structured fields must treat them as optional evidence that is present on owner-authored rejections and absent on framework-origin ones.
- The funnel-to-string is the reason a domain error type substituted at definition time is an error-vocabulary decision, not a transport one: the substituted type replaces the error across every generated factory, parser, and converter at once, but its richness is realizable only where the owner's own validation authored the rejection. The design pressure is to keep the structured fields advisory and the string form total, because the string is the one representation every route can produce.

[VOCABULARY_ADMISSION_AS_MEMBERSHIP]:
- A keyed vocabulary's `Validate` is generated, not authored: it is a closed-set membership test that calls the owner's own `TryGet` and, on miss, materializes a typed error naming the owner and the unknown key. Admission for a bounded vocabulary is therefore identity resolution against the compile-time item set with no user validation code, and the same generated method backs parse, deserialization, and conversion — the membership test is the single admission for every route into the vocabulary. Hand-writing a key-to-item dictionary plus a missing-key throw duplicates a method the owner already generates and forfeits the typed error.
- The membership test ships in a span-keyed form for string keys, so admission reads a character span directly and resolves against the same frozen alternate-lookup table the in-process path uses, with no intermediate string allocation on the wire-deserialize or parse path. Span admission is the default posture for string-keyed vocabulary and is opted out, not opted in: a parse-shaped probe that allocates a string to look up an item is paying for an allocation the generated span overload deletes.
- Vocabulary admission and value-object admission converge on the same static contract but diverge on what the hook does: a value object's `Validate` runs author-written invariant logic over a free key, while a vocabulary's `Validate` runs a generated membership probe over a closed key population. The contract identity is what lets one generic admission routine — a sweep that selects valid owners out of raw input, a cache hydrator, a configuration reader — operate over both kinds without knowing which it holds; re-deciding an owner from value object to smart enum later never touches that generic code because the admission signature is invariant across the choice.

[GENERIC_ADMISSION_OVER_THE_CONTRACT]:
- The admission signature is a static-abstract interface member, so admission is callable through a generic constraint without naming the concrete owner, and a single bulk-admission routine partitions raw input into admitted owners and discarded rejects in one pass over any owner family. The constraint carries the owner, the raw value type, and the error type, so the routine forwards culture explicitly and never reads ambient formatting state.

```csharp
static IReadOnlyList<T> AdmitMany<T, TValue, TError>(
    IEnumerable<TValue> raw, IFormatProvider? culture)
    where TValue : notnull
    where TError : class, IValidationError<TError>
    where T : IObjectFactory<T, TValue, TError> =>
    [.. raw.Select(value => (Error: T.Validate(value, culture, out var item), Item: item))
           .Where(static slot => slot.Error is null)
           .Select(static slot => slot.Item!)];
```

- Culture threads through the canonical admission signature as an explicit parameter on the contract, never an ambient read, so a parse-shaped owner receives formatting policy at the call and generic admission forwards it blind to whether the key is numeric, temporal, or textual. A bulk routine over heterogeneous owners passes one culture to all of them and each owner's `Validate` interprets or ignores it; there is no per-owner culture configuration to reconcile because the input is uniform across the contract.
- The fully dynamic edge — reconstructing an aggregate from a runtime type with no compile-time owner name — routes through a static-abstract invoker that calls the constrained `Validate` from a runtime type alone, so reflection-driven hydration still passes every nested owner through its real admission rather than bypassing it. This bridges the boundary between fully generic admission and fully dynamic admission: the same `Validate` is reachable by generic constraint when the type is known and by runtime invoker when it is not, and neither path is a re-validation reimplementation.

[CHURN_GRADIENT_AND_PLACEMENT]:
- Admission cost is reads-of-evidence per write, and the gradient decides placement. A value rewritten per frame or per solver step pays its full invariant on every write while interior code consumes none of the evidence mid-loop, so accumulation lives in a plain record and admission happens once at the seam where the accumulated result becomes domain material. Admitting inside a hot loop pays for evidence nothing reads; never admitting leaves the seam unguarded. The decision variable is the consumption ratio, not type aesthetics, and a validated owner in a tight accumulation loop is a misplaced admission regardless of how clean the owner is.
- Generated owners delete the external mutation surface structurally — private constructors, forced-private `init`, members forced required and read-only, with `with`-cloning suppressed even on struct owners the language otherwise grants it to — so every derived state is a fresh admission through the factory and there is no rebind path that skips the funnel. This is why the churn gradient is a hard placement law and not a preference: a high-churn value cannot cheaply derive a next state from a generated owner because the only derivation route is full re-admission, so the loop either re-validates every step or holds a plain record and admits once at exit.
- Invariant tightening is the one admission change with zero compile signal: the factory narrows silently and values persisted under the looser rule fail rehydration. The bypass constructor exists exactly for this seam — stored data outlives invariant drift — so a tightening is a data-migration decision with a code rider, and because the type system stays silent the proof obligation moves entirely to tests. The placement of the tightened check decides the failure mode: in `ValidateFactoryArguments` it rejects fresh input with a recoverable error while letting old stored data rehydrate; in `ValidateConstructorArguments` it throws on the rehydration path too and turns invariant drift into a load-time fault.
