# Dispatch Forms — Frontier Mechanisms

[GENERATED_SWITCH_FRONTIER]:
- `NestedUnionParameterNameGeneration.Simple` collapses intermediate type names from generated `Switch`/`Map` parameter identifiers: a case `Failure.NotFound` emits parameter `notFound` instead of `failureNotFound`. The failure mode is specific: when two sibling nested unions each have a case with the same simple name — `Success.Status` and `Failure.Status` — the outer union's generated `Switch` accumulates duplicate parameter names, which is a compile error (CS0100). `NestedUnionParameterNameGeneration.Default` is safe by construction; `Simple` is the writer-controlled tradeoff between call-site legibility and structural collision risk.
- `[UnionSwitchMapOverload(StopAt = ...)]` is `AllowMultiple = true`. Each attribute instance generates one additional overload. The generator collects all ordered concrete type members from the flat type-walk, then removes derived types recursively for each stop type, and strips any remaining abstract members not in `StopAtTypeNames`. The stop type itself appears as a leaf arm; its entire subtree disappears from the overload's parameter list. A stop type that is already a leaf has no practical effect — the overload is identical to the exhaustive form. Multiple stop types in one attribute instance produce one overload that simultaneously coarsens multiple subtrees.
- `SwitchMapMethodsGeneration.None` suppresses the `Switch` and `Map` family but does not suppress the backing field, implicit conversion operators, or equality members. A union with `None` is still a fully-functional union value type inspectable with `is` and property patterns; it simply has no generated dispatch surface. This is the correct choice for wire-boundary unions that feed a single consumer's structural pattern match and have no behavioral polymorphism.
- `FactoryMethodGeneration.Always` forces per-member named factory methods regardless of whether the type is a plain concrete class that would otherwise accept an implicit conversion operator. `Always` is required when the dispatch architecture must guarantee that call sites never depend on the implicit conversion operator path — for example, when all construction must be traceable to one surface for audit. `FactoryMethodGeneration.None` suppresses factory generation even when triggers are present; the constructor becomes the only admitted construction path.
- `UnionConstructorAccessModifier.Private` simultaneously restricts constructors and implicit conversion operators to `private`, making factory methods the only external construction path. This is the enforcement mechanism for the `POLICY_VALUES` law applied to union construction: the vocabulary admits values only through declared factories, never through coercive assignment. `Internal` restricts to assembly scope; the default `Public` is the open construction form.
- The `allows ref struct` anti-constraint on ad-hoc union type parameters is TTRESG073 (error). Regular `[Union]` sub-types support `allows ref struct` via `#if NET9_0_OR_GREATER` guards emitted around `where TState : allows ref struct` and `where TResult : allows ref struct` in the generated `Switch`/`Map` overloads; targeting net10.0 satisfies this condition. Ad-hoc unions using `TypeParamRef1`–`TypeParamRef5` cannot carry this constraint — TTRESG073 is unconditional for ad-hoc forms.
- `SkipEqualityComparison = true` on a regular `[Union]` does not suppress `Switch`/`Map`; both remain fully generated. A union intended as a non-equatable ephemeral dispatch carrier — holding delegates, mutable state, or native handles — should suppress equality to prevent accidental use as a key surface while retaining its dispatch capability.

[AD_HOC_UNION_DISPATCH_SPECIFICS]:
- Ad-hoc unions use `is`-chain dispatch in the generated `Switch`/`Map` body, evaluating member types in declaration order (T1, T2, ..., T5). When a `TnIsStateless = true` member is present, no backing field entry is stored for it — only a discriminator index. The generated `Switch` arm for a stateless member receives `default(Tn)`, not a stored value. The correct payload for stateless arms is a constant or a computation that derives entirely from the dispatch context (the state parameter), not from the case value.
- `T1Name`, `T2Name`, etc. rename all generated identifiers associated with that member: the factory method name, the `Switch`/`Map` parameter name, the `AsT1`-style accessor name, and implicit conversion member names. The renaming is total — no generated identifier retains the original type name. This matters when two members share the same simple type name (two different `Status` types from different namespaces): assigning distinct `TnName` values produces a well-formed union rather than a duplicate-name compile error.
- `UseSingleBackingField = true` on an ad-hoc union with a non-`object?` `SingleBackingFieldType` avoids boxing for reference-type members by storing them as the common interface type. The dispatch is still an `is`-chain, but the backing field read fetches from one field position rather than up to five. Value-type members box regardless of `SingleBackingFieldType` because value types cannot be stored without boxing in a reference-typed backing field.

[FROZEN_TABLE_FRONTIER]:
- `AlternateLookup<TAlternate>` (the nested struct on `FrozenDictionary<TKey, TValue>`) exposes `ContainsKey(TAlternate)`, `TryGetValue(TAlternate, out TValue)`, `Dictionary`, and an indexer. The zero-allocation probe path is `TryGetValue(ReadOnlySpan<char>, out TValue)` when the comparer is `OrdinalIgnoreCase`. `GetValueRefOrNullRef` is not exposed on `AlternateLookup` — it is only on `FrozenDictionary<TKey, TValue>` directly with a `TKey` argument. The single-probe ref path for a span key requires converting to `TKey` first: obtain the ref via `GetValueRefOrNullRef(spanKey.ToString())`, or use `AlternateLookup.TryGetValue` to avoid the allocation only when the value copy is acceptable.
- `GetValueRefOrNullRef(TKey key)` on `FrozenDictionary<TKey, TValue>` returns a `ref TValue` into the internal storage, tested with `Unsafe.IsNullRef`. A local `ref var` binding directly in the internal array avoids the value copy of `TryGetValue`. The reference is valid for the lifetime of the frozen dictionary instance and must not be retained across a dictionary replacement.
- Small frozen collection strategy selection: for small input, `FrozenDictionary` chooses among `SmallFrozenDictionary` (reference types and non-`IComparable` value types), `SmallValueTypeComparableFrozenDictionary`, and `SmallValueTypeDefaultComparerFrozenDictionary` — all three perform linear equality scanning with no hash computation. Smart-enum keys route through `SmallValueTypeDefaultComparerFrozenDictionary` for small tables and `DefaultFrozenDictionary` (perfect-hash construction) for larger ones. For a small policy table with string keys, the linear path is both faster than any hash path and selected automatically — switch expressions over small vocabularies are not faster.
- Frozen table construction from `ReadOnlySpan<KeyValuePair<TKey, TValue>>` is available via the overload `FrozenDictionary.Create(IEqualityComparer<TKey>?, ReadOnlySpan<KeyValuePair<TKey, TValue>>)`, avoiding intermediate allocation when the entries are inline constants. Collection expressions and `params ReadOnlySpan<T>` both feed this overload without a heap allocation for the source array.
- A frozen table constructed with duplicate keys does not throw at construction — `Create` silently retains the last entry for each duplicated key, unlike `Dictionary` (which throws). The correct enforcement is that the construction source — a `SmartEnum.Items` projection or a static constant span — is structurally incapable of producing duplicates: `Items` is deduplicated by definition, and an inline constant span is audited at code review. Do not rely on construction-time deduplication detection; make the source non-duplicating.

[CATCH_HANDLER_FRONTIER]:
- `@catchOf<E, M, A>` (non-fold) uses `matchError` which branches on whether the error is directly of type `E` or whether an aggregate `Error` is the carrier. For the aggregate case, it calls `es.Filter<E>().ForAllM(f)` — `ForAllM` threads the handler monadically via `Bind` so each error in the aggregate becomes a `K<M, A>` that must succeed for the chain to continue; short-circuits on first failure. The `fold` variants (`catchOfFold`) replace `ForAllM` with `FoldM`, which uses `MonoidK<M>` to accumulate handlers across aggregate errors. `FoldM` is correct when all errors in an aggregate of the matching subtype should be reported or recovered independently; `ForAllM` is correct when the first success or failure is conclusive.
- `@catch(int errorCode, ...)` matches by `error.Code` equality, not by type. An `int` code can disambiguate errors of different semantic types that share a hierarchy ancestor, whereas `@catchOf<E>` cannot distinguish two `Expected` subtypes that differ only by code. The two forms are orthogonal: composing them — `@catch(specificCode, ...)` before `@catchOf<E>(...)` — correctly narrows the code-specific error before the type-wide handler sees it.
- The `|` operator for `IO<A>` (and `Eff<A>`, `Eff<RT, A>`) is overloaded not just for `CatchM<Error, M, A>` but also for `K<IO, A>` (alternative computation), `Pure<A>` (constant recovery), `Fail<Error>` (re-fail with a different error), and bare `Error` (re-fail). The `K<IO, A> | K<IO, A>` form is the alternative pattern: if the left computation fails for any reason, the right computation is run without error inspection. This is structurally distinct from catch handler composition and must not be confused with it: `|` as alternative is unconditional fallback; `|` as catch composition is predicate-guarded recovery.
- `@catch<M, A>(Func<Error, K<M, A>> Fail)` with no predicate parameter is a total catch handler: it fires for every error. Positioned last in a handler chain, it ensures no error escapes. The recommended composition for bounded error domains is ordered catch handlers from most specific to least specific, with a total catch as the final term — the first-match-wins left-associative ordering makes this natural.

[DISPATCH_FORM_INTERACTION_FRONTIER]:
- A `[Union]` whose root is declared `abstract` with `private` constructors and whose cases are `sealed` nested records satisfies TTRESG054. The `abstract` root with `private` constructor form is the canonical shape for regular discriminated unions. A `[Union]` on a `sealed` class with no nested hierarchy is the sealed-leaf form — `SwitchMapMethodsGeneration.None` is its only useful configuration, because a sealed single-case union has one arm and no dispatch value. Nested `[Union]` hierarchies (abstract intermediate unions) require class nodes, not records, for every non-leaf.
- Structural pattern dispatch and generated `Switch` compose at different hierarchy levels without conflict: generated `Switch` owns closed case dispatch at the union boundary; structural patterns own payload destructuring inside the `Switch` arm. The arm body receives a statically-typed case payload, and the compiler knows the exact type within that arm — no additional `is` check is needed. Positional patterns over primary constructor record cases bind directly in the arm parameter without a nested pattern match.
- The extension block `|` operator on `K<F, A>` for catch handlers is defined per concrete monad type (`IO`, `Eff`, `Eff<RT>`), not generically on `K<F, A>`. The catch composition pattern requires one of these three concrete carrier types as the left-hand operand. An intermediate `K<M, A>` in a generic method must be `.As()`-cast to the concrete type before the `|` chain is valid.

[CONSTRUCTION_TIME_ASPECT_FRONTIER]:
- `bracketIO(computation)` is the semantic preference over `bracketIO(acq, use, fin)` per the LanguageExt documentation: "Consider using `bracketIO(computation)` rather than `bracket(acq, use, fin)`, the semantics are more attractive." The `bracketIO(computation)` form takes a `K<M, A>` with `M` constrained to `MonadUnliftIO` and installs a resource-tracking scope; `bracketIO(acq, use, fin)` is the explicit three-argument form. `IO<A>.Bracket(Use, Fin)` is receiver-scoped bracket where `this IO<A>` is the acquisition; `IO<A>.Bracket(Use, Catch, Fin)` adds a failure-specific `Catch` projection that runs between `Use` failure and `Fin`. The difference between `Catch` and `BracketFail` is that `BracketFail` runs only on failure and keeps the resource live on success, while the three-argument `Bracket`'s `Catch` still runs `Fin` unconditionally — `Catch` is a failure-specific hook, not a release.
- `Schedule.maxDelay(Duration max)` (a `ScheduleTransformer`) caps individual delay duration: no single wait exceeds `max`. `Schedule.maxCumulativeDelay(Duration max)` caps the total accumulated delay across all retries: the schedule terminates when the sum of all previous delays would exceed `max`. Composing: `Schedule.exponential(50*ms) | Schedule.maxDelay(2000*ms) | Schedule.maxCumulativeDelay(30_000*ms) | Schedule.recurs(20)` produces at most 20 retries, each at most 2 seconds, stopping when cumulative delay would exceed 30 seconds — whichever bound triggers first wins. `Schedule.decorrelate()` (a `ScheduleTransformer`) applies proportional jitter to each delay, reducing retry thundering-herd while preserving the backoff shape.
- The `Schedule.RepeatForever` static field converts any finite schedule into an infinite repeating one. Applying `RepeatForever` before `recurs(n)` creates an `n`-capped repeating schedule where each repetition of the base schedule counts as one; applying it after produces an infinite schedule that ignores the `recurs` cap. Composition order with transformers is left-to-right: `Schedule.exponential(100*ms) | Schedule.recurs(3) | Schedule.RepeatForever` repeats the 3-retry exponential pattern indefinitely; `Schedule.exponential(100*ms) | Schedule.RepeatForever | Schedule.recurs(3)` produces an infinite exponential that is then capped at 3 repetitions.

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
// Frozen table: AlternateLookup exposes TryGetValue(ReadOnlySpan<char>, out TValue)
// for zero-allocation span-key probes; GetValueRefOrNullRef is on FrozenDictionary directly.
// TryGetAlternateLookup at startup gates the alternate path; false is a hard startup failure.
static readonly FrozenDictionary<string, Policy> _table = new Dictionary<string, Policy>
{
    ["alpha"] = Policy.Alpha,
    ["beta"] = Policy.Beta,
}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

static readonly FrozenDictionary<string, Policy>.AlternateLookup<ReadOnlySpan<char>> _spanLookup =
    _table.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)
        ? lookup
        : throw new InvalidOperationException("comparer incompatible with span lookup");

static bool TryResolve(ReadOnlySpan<char> key, out Policy policy) =>
    _spanLookup.TryGetValue(key, out policy);
```

```csharp
// Catch composition: catchOfFold for aggregate errors; @catch(int code) for code-discriminated narrowing.
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
