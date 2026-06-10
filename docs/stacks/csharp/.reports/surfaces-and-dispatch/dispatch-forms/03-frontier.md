# Dispatch Forms — Frontier Mechanisms

[GENERATED_SWITCH_FRONTIER]:
- `NestedUnionParameterNameGeneration.Simple` collapses intermediate type names from generated `Switch`/`Map` parameter identifiers: a case `Failure.NotFound` emits parameter `notFound` instead of `failureNotFound`. This is the correct form when the call site vocabulary is clear and uniqueness is guaranteed by case name cardinality. The failure mode is specific: when two sibling nested unions each have a case of the same simple name — `Success.Status` and `Failure.Status` — the outer union's generated `Switch` accumulates duplicate parameter names, which is a compile error (CS0100). `NestedUnionParameterNameGeneration.Default` is safe by construction; `Simple` is the writer-controlled tradeoff between call-site legibility and structural collision risk.
- `[UnionSwitchMapOverload(StopAt = ...)]` is `AllowMultiple = true`. Each attribute instance generates one additional overload named with the full set of stop-types. The generator's `FilterTypeMembersForOverload` first collects all ordered concrete type members from the flat type-walk, then calls `RemoveDerivedTypes` recursively for each stop type, and finally strips any remaining abstract members whose fully-qualified name is not in `StopAtTypeNames`. The result is that the stop type itself appears as an arm (leaf-shaped, no children), and its entire subtree disappears from the overload's parameter list. A stop type that is already a leaf has no practical effect — the overload is identical to the exhaustive form. Multiple stop types in one attribute instance produce one overload that simultaneously coarsens multiple subtrees.
- `SwitchMapMethodsGeneration.None` suppresses the `Switch` and `Map` family but does not suppress the backing field, implicit conversion operators, or equality members. A union with `None` is still a fully-functional union value type that can be inspected with `is` and property patterns; it simply has no generated dispatch surface. This is the correct choice for wire-boundary unions that feed a single consumer's structural pattern match and have no behavioral polymorphism.
- `FactoryMethodGeneration.Always` forces per-member named factory methods regardless of whether the type is a plain concrete class that would otherwise accept an implicit conversion operator. This is not a stylistic preference: `Always` is required when the dispatch architecture must guarantee that call sites never depend on the implicit conversion operator path — for example, when the union is used as a discriminant in a `FrozenDictionary` key and all construction must be traceable to one surface. `FactoryMethodGeneration.None` suppresses factory generation even when triggers are present (type parameter, interface, duplicate type, `object` member); the constructor is the only admitted construction path.
- `UnionConstructorAccessModifier.Private` simultaneously restricts constructors and implicit conversion operators to `private`, making factory methods the only external construction path. This is the enforcement mechanism for the `POLICY_VALUES` law applied to union construction: the vocabulary admits values only through declared factories, never through coercive assignment. `Internal` restricts to assembly scope; the default `Public` is the open construction form.
- The `allows ref struct` anti-constraint on ad-hoc union type parameters is TTRESG073 (error). Regular `[Union]` sub-types support `allows ref struct` via `#if NET9_0_OR_GREATER` guards emitted around `where TState : allows ref struct` and `where TResult : allows ref struct` in the generated `Switch`/`Map` overloads. The guard is conditional on `NET9_0_OR_GREATER`; targeting `net10.0` satisfies this condition. Ad-hoc unions using `TypeParamRef1`–`TypeParamRef5` cannot carry this constraint — TTRESG073 is unconditional for ad-hoc forms.
- `SkipEqualityComparison = true` on a regular `[Union]` does not suppress `Switch`/`Map`; both remain fully generated. The practical relevance to dispatch architecture is that a union intended as a non-equatable ephemeral dispatch carrier — holding delegates, mutable state, or native handles — should suppress equality to prevent accidental use as a key surface while retaining its dispatch capability.

[AD_HOC_UNION_DISPATCH_SPECIFICS]:
- Ad-hoc unions use `is`-chain dispatch in the generated `Switch`/`Map` body, evaluating member types in declaration order (T1, T2, ..., T5). When a `TnIsStateless = true` member is present, no backing field entry is stored for it — only a discriminator index. The generated `Switch` arm for a stateless member receives `default(Tn)`, not a stored value. This means the arm is a pure sentinel path: the dispatch fires on discriminator index match without any value projection. The correct payload for stateless arms is a constant or a computation that derives entirely from the dispatch context (the state parameter), not from the case value.
- `T1Name`, `T2Name`, etc. rename all generated identifiers associated with that member: the factory method name, the `Switch`/`Map` parameter name, the `AsT1`-style accessor name, and implicit conversion member names. The renaming is total — no generated identifier retains the original type name. This matters when two members share the same simple type name (two different `Status` types from different namespaces): assigning distinct `TnName` values produces a well-formed union rather than a duplicate-name compile error.
- `UseSingleBackingField = true` on an ad-hoc union with a non-`object?` `SingleBackingFieldType` avoids boxing for reference-type members by storing them as the common interface type. The dispatch is still an `is`-chain, but the backing field read fetches from one field position rather than up to five. For value-type members, boxing still occurs regardless of `SingleBackingFieldType` because value types cannot be stored without boxing in a reference-typed backing field. `UseSingleBackingField = false` (default) stores each member in its own typed field; dispatch reads only the matching field after the `is` check confirms the correct arm.

[FROZEN_TABLE_FRONTIER]:
- `FrozenDictionary.GetValueRefOrNullRef(TKey key)` returns `ref readonly TValue` in .NET 10 (confirmed from decompiled BCL). The `ref readonly` qualifier prevents the caller from mutating the referenced value through the returned ref. The pattern `Unsafe.IsNullRef(in Unsafe.AsRef(in dict.GetValueRefOrNullRef(key)))` is the correct null check; `Unsafe.AsRef` converts `ref readonly TValue` to `ref TValue` for the `Unsafe.IsNullRef` probe while preserving the in-parameter contract. A local `ref readonly var` binding (`ref readonly var v = ref dict.GetValueRefOrNullRef(key)`) is the read-only single-probe form; mutation through the ref is rejected at compile time.
- `AlternateLookup<TAlternateKey>` (the nested struct on `FrozenDictionary<TKey, TValue>`) carries an `AlternateLookupDelegate<TAlternateKey>` function pointer to the implementation-specific alternate lookup path. The `TryGetAlternateLookup` path populates this delegate; on a comparer mismatch, the virtual `GetAlternateLookupDelegate` falls back to a static delegate that always returns `Unsafe.NullRef<TValue>()` — meaning a failed `TryGetAlternateLookup` at startup would produce a lookup surface that silently returns null-ref on every probe rather than throwing. This is the argument for treating a `false` result from `TryGetAlternateLookup` as a hard startup failure: the fallback is a completely broken lookup, not an exception.
- `SmallFrozenDictionary` is selected for collections of four or fewer entries (verified: `source.Count <= 4` at construction). Its lookup is a linear equality scan over a fixed-size array of keys — no hash computation at all. For a four-entry policy table with string keys, `SmallFrozenDictionary` is both the fastest path (linear over 4 is faster than any hash with seed computation) and the one selected automatically. This is the argument against premature switch from frozen tables to `switch` expressions for small vocabulary tables: the frozen table already selects the optimal implementation.
- Frozen table construction from `ReadOnlySpan<KeyValuePair<TKey, TValue>>` is available via the overload `FrozenDictionary.Create(IEqualityComparer<TKey>?, ReadOnlySpan<KeyValuePair<TKey, TValue>>)` — the span form avoids intermediate allocation at construction when the entries are inline constants. Collection expressions and `params ReadOnlySpan<T>` both feed this overload without a heap allocation for the source array.

[CATCH_HANDLER_FRONTIER]:
- `catchOf<E, M, A>` (non-fold) uses `matchError` which branches on whether the error is directly of type `E` or whether an aggregate `Error` is the carrier. For the aggregate case, it calls `es.Filter<E>().ForAllM(f)` — `ForAllM` threads the handler monadically through the filtered errors, chaining `Bind` so each error in the aggregate becomes a `K<M, A>` that must succeed for the chain to continue. The `fold` variants (`catchOfFold`) replace `ForAllM` with `FoldM`, which uses `MonoidK<M>` to accumulate handlers across aggregate errors in one accumulation step rather than a sequential `Bind` chain. `FoldM` is correct when all errors in an aggregate of the matching subtype should be reported or recovered independently; `ForAllM` short-circuits on the first match that succeeds (or fails if the monad is fail-fast).
- `@catch(int errorCode, ...)` matches by `error.Code` equality, not by type. This creates a selection pressure: an `int` code can disambiguate errors of different semantic types that happen to share a hierarchy ancestor, whereas `catchOf<E>` cannot distinguish two `Expected` subtypes that differ only by code. The two forms are orthogonal, and composing them — `@catch(specificCode, ...)` before `catchOf<E>(...)` — correctly narrows the code-specific error before the type-wide handler sees it.
- The `|` operator for `IO<A>` (and `Eff<A>`, `Eff<RT, A>`) is overloaded not just for `CatchM<Error, M, A>` but also for `K<IO, A>` (alternative computation), `Pure<A>` (constant recovery), `Fail<Error>` (re-fail with a different error), and bare `Error` (re-fail). The `K<IO, A> | K<IO, A>` form is the alternative pattern: if the left computation fails for any reason, the right computation is run without error inspection. This is structurally distinct from catch handler composition and should not be confused with it: `|` as alternative is unconditional fallback; `|` as catch composition is predicate-guarded recovery.
- `@catch<M, A>(Func<Error, K<M, A>> Fail)` with no predicate parameter is a total catch handler: it fires for every error. Positioned last in a handler chain, it ensures no error escapes. The recommended composition for bounded error domains is ordered catch handlers from most specific to least specific, with a total catch as the final term — the first-match-wins left-associative ordering makes this natural. A `@catch` with a predicate that evaluates to `true` for all errors is functionally equivalent to the no-predicate form but carries unnecessary evaluation cost on every failure.

[DISPATCH_FORM_INTERACTION_FRONTIER]:
- A `[Union]` whose root is declared `abstract` with `private` constructors and whose cases are `sealed` nested records satisfies TTRESG054 (union must be sealed or have private constructors only). The `abstract` root with `private` constructor form is the canonical shape for regular discriminated unions. A `[Union]` on a `sealed` class with no nested hierarchy is the sealed-leaf form — it admits only `SwitchMapMethodsGeneration.None` usefully, because a sealed single-case union has only one arm and no dispatch value. The `record`-based nested case form is TTRESG055-guarded: a union implemented using a record must be sealed; if nesting is needed, use a class. This means nested `[Union]` hierarchies (abstract intermediate unions) require class nodes, not records, for every non-leaf.
- Structural pattern dispatch and generated `Switch` compose at different hierarchy levels cleanly: generated `Switch` owns closed case dispatch at the union boundary; structural patterns own payload destructuring inside the `Switch` arm. The arm body receives a statically-typed case payload, and the compiler knows the exact type within that arm — no additional `is` check is needed. Positional patterns over primary constructor record cases (`circle: static (_, c) => c is (double r) ? ... : ...`) are redundant; the positional pattern can bind directly in the arm parameter without a nested pattern match.
- The extension block `|` operator on `K<F, A>` for catch handlers is defined per concrete monad type (`IO`, `Eff`, `Eff<RT>`), not generically on `K<F, A>`. This means the catch composition pattern requires one of these three concrete carrier types as the left-hand operand. An intermediate `K<M, A>` in a generic method must be `.As()`-cast to the concrete type before the `|` chain is valid. The presence of `extension<E, M, A>(CatchM<E, M, A> self)` in LanguageExt 5 uses C# 14 extension block syntax directly in the library — catch handler operations are themselves implemented via extension blocks, confirming the production viability of this pattern at library scale.

[CONSTRUCTION_TIME_ASPECT_FRONTIER]:
- `bracketIO(computation)` is the semantic preference over `bracket(acq, use, fin)` per the LanguageExt documentation comments embedded in the decompiled source: "Consider using `bracketIO(computation)` rather than `bracket(acq, use, fin)`, the semantics are more attractive." The `bracketIO(computation)` form wraps any `MonadUnliftIO<M>` effect and installs a resource-tracking scope; `bracket(acq, use, fin)` is the explicit three-argument form. `IO<A>.Bracket<B, C>(Use, Fin)` is receiver-scoped bracket where `this IO<A>` is the acquisition; `IO<A>.Bracket<B, C>(Use, Catch, Fin)` adds a failure-specific `Catch` projection that runs between `Use` failure and `Fin`. The difference between `Catch` and `BracketFail` is that `BracketFail` runs only on failure and keeps the resource live on success, while the three-argument `Bracket`'s `Catch` still runs `Fin` unconditionally — `Catch` is a failure-specific hook, not a release.
- `ScheduleTransformer.maxDelay(Duration max)` caps individual delay duration: no single wait exceeds `max`. `ScheduleTransformer.maxCumulativeDelay(Duration max)` caps the total accumulated delay across all retries: the schedule terminates when the sum of all previous delays would exceed `max`. The two transformers compose: `Schedule.exponential(50*ms) | Schedule.maxDelay(2000*ms) | Schedule.maxCumulativeDelay(30_000*ms) | Schedule.recurs(20)` produces at most 20 retries, each at most 2 seconds, stopping when cumulative delay would exceed 30 seconds — whichever bound triggers first wins under `|` (union extends to the longer stream; truncation happens when the `maxCumulativeDelay` transformer exhausts its budget). `ScheduleTransformer.decorrelate(factor: 0.1)` applies jitter proportional to the previous delay, reducing retry thundering-herd while preserving the backoff shape.
- The `ScheduleTransformer.RepeatForever` static field converts any finite schedule into an infinite repeating one by wrapping it in `SchRepeatForever`. Applying `RepeatForever` before `recurs(n)` creates an `n`-capped repeating schedule where each repetition of the base schedule counts as one; applying it after produces an infinite schedule that ignores the `recurs` cap. Composition order with transformers is left-to-right: `Schedule.exponential(100*ms) | Schedule.recurs(3) | Schedule.RepeatForever` repeats the 3-retry exponential pattern indefinitely; `Schedule.exponential(100*ms) | Schedule.RepeatForever | Schedule.recurs(3)` produces an infinite exponential that recurs 3 times (the outer `recurs` caps the overall repetition, not the inner base).

```csharp
// Ad-hoc union: T1IsStateless for sentinel arm; T1Name for collision resolution;
// stateless arm receives default(T1) — dispatch is on discriminator index, not value.
[AdHocUnion(typeof(NotFound), typeof(Value), t1IsStateless: true)]
public partial struct Result<T> where T : notnull { }

double area = result.Switch(
    notFound: static (s, _) => s.DefaultValue,
    value: static (s, v) => v.Data,
    state: (DefaultValue: 0.0));
```

```csharp
// Frozen table: ref readonly return in .NET 10; AlternateLookup for span key probe;
// TryGetAlternateLookup at startup gates the alternate path.
static readonly FrozenDictionary<string, Policy> _table = new Dictionary<string, Policy>
{
    ["alpha"] = Policy.Alpha,
    ["beta"] = Policy.Beta,
}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

static readonly FrozenDictionary<string, Policy>.AlternateLookup<ReadOnlySpan<char>> _spanLookup =
    _table.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)
        ? lookup
        : throw new InvalidOperationException("comparer incompatible with span lookup");

static bool TryResolve(ReadOnlySpan<char> key, out Policy policy)
{
    ref readonly Policy found = ref _spanLookup.GetValueRefOrNullRef(key);
    policy = Unsafe.IsNullRef(in Unsafe.AsRef(in found)) ? default : found;
    return !Unsafe.IsNullRef(in Unsafe.AsRef(in found));
}
```

```csharp
// Catch composition: catchOf ForAllM vs FoldM for aggregate errors;
// @catch(int code) for code-discriminated narrowing before type-wide handler.
static IO<T> Resilient<T>(IO<T> op) =>
    op.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(3) | Schedule.exponential(100 * ms))
    | @catch<IO, T>(Errors.Cancelled.Code, _ => IO.fail<T>(Errors.Cancelled))
    | catchOfFold<TransientError, IO, T>(e => IO.fail<T>(Error.New(e.Code, e.Message)))
    | @catch<IO, T>(e => e.IsExpected, IO.fail<T>);
```

```csharp
// Nested union with Simple naming; multiple [UnionSwitchMapOverload] for coarse consumer.
[Union(NestedUnionParameterNames = NestedUnionParameterNameGeneration.Simple)]
[UnionSwitchMapOverload(StopAt = new[] { typeof(Failure) })]
public partial class Response
{
    public sealed class Success : Response;

    public abstract partial class Failure : Response
    {
        private Failure() { }
        public sealed class NotFound : Failure;
        public sealed class Unauthorized : Failure;
    }
}

// Coarse overload: success, failure — failure subtree is one arm.
string label = response.Switch(
    success: static (_, _) => "ok",
    failure: static (_, _) => "error",
    state: unit);

// Exhaustive overload: success, notFound, unauthorized.
string detail = response.Switch(
    success: static (_, _) => "200",
    notFound: static (_, _) => "404",
    unauthorized: static (_, _) => "401",
    state: unit);
```
