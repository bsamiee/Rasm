# Dispatch Forms — Edge Constraints and Cross-Form Integration

[EXTENSION_BLOCK_HARD_LIMITS]:
- Expression trees reject two extension block member categories unconditionally: property access emits `An expression tree may not contain an extension property access`, and `&&`/`||` operators using extension-defined user-defined operators emit `An expression tree may not contain '&&' or '||' operators that use extension user defined operators`. Extension methods (non-property, non-`&&`/`||`) are still legal inside expression trees as static call rewriting — the prohibition is specific to the new property and logical-operator syntax. Any dispatch architecture where extension block surfaces feed LINQ `Expression<Func<T, bool>>` predicates, EF query translators, or library-internal expression-tree builders must fall back to classic static extension methods for those specific surfaces.
- Two extension blocks in the same enclosing static class that would generate the same content-based type name in metadata are a compile error: `ERR_ExtensionBlockCollision` — "They result in conflicting content-based type names in metadata, so must be in separate enclosing static classes." The content-based name is derived from the receiver type and member set. The consequence for module layout: a module contributing multiple extension blocks to the same receiver type must declare them in separate `static class` containers within the same file. Using one container per logical concern (projection, conversion, aspect attachment) naturally avoids the collision while preserving grouping clarity.
- Receiver parameter modifier constraints have three orthogonal rules. A `ref` receiver must be a value type or a generic type constrained to `struct` — generics are permitted for mutating extensions. A `ref readonly` or `in` receiver must be a **concrete (non-generic)** value type — `where T : struct` is rejected; the concrete type must be named. An instance operator on a struct receiver requires the receiver parameter to be `ref`: `Cannot declare instance operator for a struct unless containing extension block receiver parameter is a 'ref' parameter`. A static class receiver cannot contain user-defined operators at all: `An extension block extending a static class cannot contain user-defined operators`. These four constraints are independent and all enforced by Roslyn.
- `[OverloadResolutionPriorityAttribute]` carries a second constraint that prior analysis missed: `Cannot use 'OverloadResolutionPriorityAttribute' on an overriding member`. An extension block member that overrides an existing member (which is impossible under current extension semantics, since extension blocks cannot use `override`) is not the live concern; the live concern is that a regular method in the same class that is marked `override` and that happens to be in an ambiguity set cannot use priority to win. The practical implication: `[OverloadResolutionPriority]` resolves ambiguity only among non-override candidates. An overriding method participates in overload resolution under normal specificity rules; it cannot use priority to elevate itself above a more general non-overriding candidate.

[INTERCEPTOR_CONSTRAINTS_ON_DISPATCH]:
- Source-generator interceptors cannot redirect a dispatch call site when the interceptor method is declared inside a generic type: `Method '{0}' cannot be used as an interceptor because its containing type has type parameters.` This eliminates interceptors as a mechanism for substituting generated `Switch`/`Map` call sites on generic request types. The interceptor must live in a non-generic enclosing type. This is a hard architectural constraint: a source generator that attempts to replace a `Switch<TState, TResult>` call on a `[Union]` call site with a hand-crafted implementation cannot do so generically — it must emit a concrete interception site per concrete type binding.
- Interceptors must be ordinary member methods: `An interceptor method must be an ordinary member method.` Extension block members lower to static methods with `ExtensionMarkerAttribute`, which disqualifies them from serving as interceptors. A classic (non-block) static extension method is also not an ordinary member method and is disqualified. Only non-extension, non-operator, non-local, non-lambda methods in non-generic types can intercept. This means interceptors are useful for definition-time weaving at concrete, non-generic, module-scoped call sites — not as a general dispatch-replacement mechanism.
- The `InterceptsLocationAttribute(string, int, int)` form (file path, line, column) is explicitly unsupported: Roslyn emits `'InterceptsLocationAttribute(string, int, int)' is not supported. Move to 'InterceptableLocation'-based generation of these attributes instead.` The `GetInterceptableLocation` API (checksum-based `InterceptableLocation1`) is the only supported form. Source generators that emit interceptors must use `SemanticModel.GetInterceptableLocation(InvocationExpressionSyntax)` to obtain the location — the old position-based form is dead. This matters for any generator that targets `Switch` call sites for logging, telemetry wrapping, or policy injection at definition time.

[SCHEDULE_TRANSFORMER_COMPOSITION]:
- `ScheduleTransformer.op_Addition(f, g)` composes two transformers in sequence: `f` runs first on the schedule, `g` runs on the result — equivalent to `s => g.Apply(f.Apply(s))`. This is distinct from the `|` and `&` operators, which compose a `ScheduleTransformer` with a `Schedule` by calling `transformer.Apply(schedule)`. The `+` operator is the transformer pipeline form: when building a named policy object (`static readonly ScheduleTransformer RetryPolicy = NoDelayOnFirst + maxDelay(2000 * ms) + decorrelate(0.1)`), `+` chains transformers into a single reusable transformer that can then be applied to any base schedule via `|`. Separating transformer composition from schedule instantiation means policy objects are defined once as transformer chains and composed with per-call-site schedules at the point of use: `io.Retry(Schedule.exponential(100 * ms) | RetryPolicy | Schedule.recurs(5))`.
- `Schedule.Transform(Func<Schedule, Schedule>)` is the `ScheduleTransformer` constructor that lifts any schedule-to-schedule function into a transformer. Custom transformer logic — delay normalization, budget-aware capping, environment-conditioned delay — enters through `Schedule.Transform(s => ...)` and can then be composed via `+` with library-provided transformers. The custom transformer receives the full `Schedule` stream as input and must return a valid `Schedule`; it is not a per-step callback but a whole-stream morphism.

[EFF_CONTEXT_SCOPING_FOR_DISPATCH_ARMS]:
- `localEff<TOuterRT, TInnerRT, A>(Func<TOuterRT, TInnerRT> f, Eff<TInnerRT, A> ma)` narrows the runtime environment for a specific computation: it maps the outer `RT` to an inner `RT` and runs `ma` in that narrowed context. This is the correct form when different dispatch arms need different runtime capabilities: the outer computation carries a broad `RT`; each arm that requires a narrower capability receives it via `localEff`. The arm is a pure `Eff<TInnerRT, A>` with no awareness of the outer `RT`, and the scoping is the dispatcher's concern.
- `Eff<A>.WithRuntime<RT>()` goes in the opposite direction: it widens a runtime-agnostic `Eff<A>` into `Eff<RT, A>` so it can be sequenced with runtime-aware computations. The pairing is `WithRuntime` at the lift site (arm produces `Eff<A>`, outer sequence requires `Eff<RT, A>`) and `localEff` at the narrowing site (outer `RT` is too broad for the arm's effects). These two are the complete RT-scoping pair at dispatch boundaries.
- `IO<A>.Local()` creates an isolated cancellation scope — not an `RT` scope. `localCancel<A>(Eff<A> ma)` is the `Eff`-level equivalent. Both create a fresh `CancellationTokenSource` linked to the parent's token, run `ma` within that child scope, and cancel the child scope on `ma`'s completion (success or failure). The scope isolation means cancelling the child does not cancel the parent; parent cancellation propagates into the child. This is the correct form when a dispatch arm fires an operation that should be independently cancellable without tearing down the outer computation — for example, a timeout-guarded arm inside a larger pipeline.

[FROZEN_COLLECTION_STRATEGY_DEPTH]:
- In the .NET 10 runtime, `FrozenDictionary` resolves to three distinct small-collection strategies rather than the single `SmallFrozenDictionary` of earlier releases: `SmallFrozenDictionary<TKey, TValue>` (reference types and non-IComparable value types), `SmallValueTypeComparableFrozenDictionary<TKey, TValue>` (small value types implementing `IComparable<TKey>`), and `SmallValueTypeDefaultComparerFrozenDictionary<TKey, TValue>` (small value types with the default equality comparer). All three perform linear equality scanning; the distinction is the comparison code path. Smart-enum keys — generated value types implementing `IEquatable<T>` but not necessarily `IComparable<T>` — route through `SmallValueTypeDefaultComparerFrozenDictionary` for small tables and through `DefaultFrozenDictionary` for larger ones, using the perfect-hash construction path.
- `FrozenSet<T>` exposes `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup)` with the same comparer-compatibility requirement as `FrozenDictionary`. `FrozenSet<string>.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)` produces a `lookup.Contains(ReadOnlySpan<char>)` surface for zero-allocation membership dispatch over protocol text. `FrozenSet` does not expose `GetValueRefOrNullRef` — membership is a boolean test, not a value projection; the ref path is exclusive to `FrozenDictionary`.
- A frozen table constructed with duplicate keys does not throw at construction — `Create` silently retains the last entry for each duplicated key. This is a construction-time silent-failure mode that differs from `Dictionary` (which throws). The correct enforcement is that the construction source — typically a `SmartEnum.Items` projection or a static constant span — is structurally incapable of producing duplicates: smart-enum `Items` is deduplicated by definition, and a `ReadOnlySpan<KeyValuePair<K,V>>` inline constant is audited at code review. Do not rely on construction-time deduplication detection; make the source non-duplicating.

[COLLECTION_DISPATCH_TRAVERSE_FORMS]:
- `TraverseM<M, A, B>(Func<A, K<M, B>> f)` on `Iterable<A>`, `Arr<A>`, `Lst<A>`, and `HashSet<A>` threads effects through `Bind`: each element is mapped to an `M`-effect and chained sequentially. The monad `M` determines short-circuit behavior — `IO` and `Eff` short-circuit on first failure; `Validation` accumulates errors (but `Validation.TraverseM` is `Bind`-based and short-circuits; use `Traverse` with `Applicative` for accumulation). `Traverse<M, A, B>(Func<A, K<M, B>> f)` uses `Apply` and accumulates without short-circuiting when the monad has `Applicative` semantics distinct from its `Monad` semantics. For dispatch over a bounded collection where the first failure should abort, `TraverseM` with `Eff` is the correct form; for dispatch over an independent collection where all errors should be reported, `Traverse` with `Validation` is correct.
- `Iterable<A>.TraverseIO<M, B>(Func<A, K<M, B>> f, K<Iterable, A> ta)` is the static form with an explicit traversable argument, available when the `Iterable` instance is not the receiver. This is the correct form for effect-sequenced dispatch over a pre-computed sequence when the caller does not own the sequence as a receiver: `Iterable.TraverseIO(process, items)` rather than `items.TraverseIO(process)`.

[AD_HOC_UNION_TYPE_PARAMETER_DISPATCH_CONSTRAINTS]:
- Ad-hoc union type parameters use `TypeParamRef1`–`TypeParamRef5` as placeholders throughout the declaration. The source generator resolves these to actual type parameters in all generated members. Implicit conversion operators are not generated for type-parameter member types: C# prohibits user-defined conversions involving unconstrained type parameters, so any ad-hoc union with a `TypeParamRefN` member must use factory methods for construction. When any member triggers factory-method generation (due to type parameter, interface, duplicate type, or `object` member), the generator produces factory methods for **all** members — the construction surface becomes uniformly factory-method-only for that union. Mixed construction paths (some implicit, some factory) are not possible on ad-hoc unions with type-parameter members.
- The `AllowsRefStructNotSupportedOnAdHocUnion` diagnostic fires unconditionally on ad-hoc unions when the `allows ref struct` anti-constraint is attempted on type parameters. Regular `[Union]` sub-types support `allows ref struct` via `#if NET9_0_OR_GREATER` guards in generated `Switch`/`Map` overloads, and net10.0 targets satisfy this condition. Ad-hoc unions are categorically excluded from `allows ref struct` — the backing-field strategy (single `object?` field or separate typed fields) requires reference semantics that is incompatible with ref structs.

```csharp
// Extension block collision: two blocks on the same receiver in the same static class fail.
// Separate static classes prevent conflicting content-based metadata names.
static class ProjectionExtensions
{
    extension(ForeignValue v)
    {
        public DomainType ToDomain() => new(v.Id, v.Name);
    }
}

static class ValidationExtensions
{
    extension(ForeignValue v)
    {
        // Declared in a separate static class — no collision.
        public bool IsValid => v.Id != Guid.Empty;
    }
}
```

```csharp
// ref receiver: works for generic T constrained to struct.
// in receiver: requires concrete (non-generic) value type — T:struct is rejected.
static class SpanExtensions
{
    extension<T>(ref T accumulator) where T : struct
    {
        // ref receiver is legal with generic struct constraint.
        public void Accumulate(T value) => accumulator = Combine(accumulator, value);
    }
}

static class ConcreteExtensions
{
    extension(in Vector3d v)
    {
        // in receiver requires concrete type — Vector3d, not T where T:struct.
        public double Magnitude => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }
}
```

```csharp
// ScheduleTransformer composition: + chains transformers, | applies to a schedule.
// RetryPolicy is a reusable transformer chain, applied once per call site.
static readonly ScheduleTransformer RetryPolicy =
    Schedule.NoDelayOnFirst
    + Schedule.Transform(s => s.Select(d => d > 2000 * ms ? 2000 * ms : d))  // maxDelay
    + ScheduleTransformer.decorrelate(factor: 0.1);

static IO<T> Resilient<T>(IO<T> op) =>
    op.Retry(Schedule.exponential(100 * ms) | RetryPolicy | Schedule.recurs(5));
```

```csharp
// localEff narrows RT for per-arm context; WithRuntime widens Eff<A> to Eff<RT, A>.
// IO.Local() creates isolated cancellation scope independent of RT.
static Eff<TOuterRT, Result> Dispatch<TOuterRT>(
    Request request,
    Func<TOuterRT, TInnerRT> narrow)
    where TOuterRT : struct, IHasBaseCapabilities =>
    request.Switch(
        alpha: static (ctx, a) =>
            localEff(ctx.Narrow, processAlpha(a)).WithRuntime<TOuterRT>(),
        beta: static (ctx, b) =>
            localCancel(processBeta(b).WithRuntime<TOuterRT>()),
        state: (Narrow: narrow));
```

```csharp
// TraverseM for sequential abort-on-first-failure dispatch over a collection.
// Traverse with Validation for accumulating all errors from independent items.
static Eff<IEnumerable<Result>> ProcessAll(Iterable<Item> items) =>
    items.TraverseM<Eff, Item, Result>(item =>
        item.Switch(
            active: static (_, a) => validate(a),
            inactive: static (_, _) => SuccessEff(Result.Skipped),
            state: unit));
```
