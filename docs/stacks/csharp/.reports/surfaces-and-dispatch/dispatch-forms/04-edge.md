# Dispatch Forms — Edge Constraints and Cross-Form Integration

[EXTENSION_BLOCK_HARD_LIMITS]:
- Expression trees reject two extension block member categories unconditionally: property access emits `An expression tree may not contain an extension property access`, and `&&`/`||` operators using extension-defined user-defined operators emit `An expression tree may not contain '&&' or '||' operators that use extension user defined operators`. Extension methods (non-property, non-`&&`/`||`) are still legal inside expression trees as static call rewriting — the prohibition is specific to the new property and logical-operator syntax. Any dispatch architecture where extension block surfaces feed LINQ `Expression<Func<T, bool>>` predicates, EF query translators, or library-internal expression-tree builders must fall back to classic static extension methods for those specific surfaces.
- Two extension blocks in the same enclosing static class that would generate the same content-based type name in metadata are a compile error: `ERR_ExtensionBlockCollision` — "They result in conflicting content-based type names in metadata, so must be in separate enclosing static classes." The content-based name derives from the receiver type and member set. A module contributing multiple extension blocks to the same receiver type must declare them in separate `static class` containers within the same file. Using one container per logical concern (projection, conversion, aspect attachment) naturally avoids the collision while preserving grouping clarity.
- Receiver parameter modifier constraints have four independent rules: a `ref` receiver requires a value type or a generic type constrained to `struct`; a `ref readonly` or `in` receiver requires a concrete (non-generic) value type — `where T : struct` is rejected, the concrete type must be named; an instance operator on a struct receiver requires the receiver parameter to be `ref`; a static class receiver cannot contain user-defined operators. All four are enforced independently by Roslyn.
- `[OverloadResolutionPriorityAttribute]` cannot be applied to an overriding member. The live concern is that a regular method marked `override` that appears in an ambiguity set cannot use priority to win — overriding methods participate in overload resolution under normal specificity rules, and priority cannot elevate a non-override above a more general non-overriding candidate. `[OverloadResolutionPriority]` resolves ambiguity only among non-override candidates within the same declaration space.

[INTERCEPTOR_CONSTRAINTS_ON_DISPATCH]:
- Source-generator interceptors cannot redirect a dispatch call site when the interceptor method is declared inside a generic type: `Method '{0}' cannot be used as an interceptor because its containing type has type parameters.` This eliminates interceptors as a mechanism for substituting generated `Switch`/`Map` call sites on generic request types. The interceptor must live in a non-generic enclosing type — a hard architectural constraint for any generator that attempts to replace `Switch<TState, TResult>` calls on generic union types.
- Interceptors must be ordinary member methods: `An interceptor method must be an ordinary member method.` Extension block members lower to static methods with `ExtensionMarkerAttribute`, disqualifying them. Classic (non-block) static extension methods are also disqualified. Only non-extension, non-operator, non-local, non-lambda methods in non-generic types can intercept. Interceptors are useful for definition-time weaving at concrete, non-generic, module-scoped call sites only.
- The `InterceptsLocationAttribute(string, int, int)` form (file path, line, column) is explicitly unsupported: Roslyn emits `'InterceptsLocationAttribute(string, int, int)' is not supported. Move to 'InterceptableLocation'-based generation of these attributes instead.` The `GetInterceptableLocation` API (`InterceptableLocation1`, checksum-based) is the only supported form. Source generators that target `Switch` call sites for logging, telemetry wrapping, or policy injection must use `SemanticModel.GetInterceptableLocation(InvocationExpressionSyntax)` to obtain the location.

[SCHEDULE_TRANSFORMER_COMPOSITION]:
- `ScheduleTransformer.op_Addition(f, g)` composes two transformers in sequence: `f` runs first on the schedule, `g` runs on the result — equivalent to `s => g.Apply(f.Apply(s))`. This is distinct from the `|` and `&` operators, which compose a `ScheduleTransformer` with a `Schedule` by calling `transformer.Apply(schedule)`. The `+` operator is the transformer pipeline form: `static readonly ScheduleTransformer RetryPolicy = Schedule.NoDelayOnFirst + Schedule.maxDelay(2000 * ms) + Schedule.decorrelate()` chains transformers into a single reusable object that can then be applied to any base schedule via `|`. Separating transformer composition from schedule instantiation means policy objects are defined once and composed with per-call-site schedules at point of use.
- `Schedule.Transform(Func<Schedule, Schedule>)` is the `ScheduleTransformer` constructor that lifts any schedule-to-schedule function into a transformer. Custom transformer logic — delay normalization, budget-aware capping, environment-conditioned delay — enters through `Schedule.Transform(s => ...)` and can then be composed via `+` with library-provided transformers. The custom transformer receives the full `Schedule` stream as input and must return a valid `Schedule`; it is a whole-stream morphism, not a per-step callback.

[EFF_CONTEXT_SCOPING_FOR_DISPATCH_ARMS]:
- `localEff<TOuterRT, TInnerRT, A>(Func<TOuterRT, TInnerRT> f, Eff<TInnerRT, A> ma)` narrows the runtime environment for a specific computation: it maps the outer `RT` to an inner `RT` and runs `ma` in that narrowed context. This is the correct form when different dispatch arms need different runtime capabilities: the outer computation carries a broad `RT`; each arm that requires a narrower capability receives it via `localEff`. The arm is a pure `Eff<TInnerRT, A>` with no awareness of the outer `RT`.
- `Eff<A>.WithRuntime<RT>()` widens a runtime-agnostic `Eff<A>` into `Eff<RT, A>` so it can be sequenced with runtime-aware computations. `WithRuntime` serves the lift site (arm produces `Eff<A>`, outer sequence requires `Eff<RT, A>`); `localEff` serves the narrowing site (outer `RT` is too broad for the arm's effects). These two are the complete RT-scoping pair at dispatch boundaries.
- `IO<A>.Local()` creates an isolated cancellation scope — not an `RT` scope. `localCancel<A>(Eff<A> ma)` is the `Eff`-level equivalent. Both create a fresh `CancellationTokenSource` linked to the parent's token, run `ma` within that child scope, and cancel the child scope on `ma`'s completion. The scope isolation means cancelling the child does not cancel the parent; parent cancellation propagates into the child. This is the correct form when a dispatch arm fires an operation that should be independently cancellable without tearing down the outer computation.

[FROZEN_COLLECTION_STRATEGY_DEPTH]:
- In .NET 10, `FrozenDictionary` resolves to three distinct small-collection strategies: `SmallFrozenDictionary<TKey, TValue>` (reference types and non-`IComparable` value types), `SmallValueTypeComparableFrozenDictionary<TKey, TValue>` (small value types implementing `IComparable<TKey>`), and `SmallValueTypeDefaultComparerFrozenDictionary<TKey, TValue>` (small value types with the default equality comparer). All three perform linear equality scanning; the distinction is the comparison code path. Smart-enum keys — generated value types implementing `IEquatable<T>` — route through `SmallValueTypeDefaultComparerFrozenDictionary` for small tables and through `DefaultFrozenDictionary` for larger ones, using the perfect-hash construction path.
- `FrozenSet<T>` exposes `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup)` with the same comparer-compatibility requirement as `FrozenDictionary`. `FrozenSet<string>.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)` produces a `lookup.Contains(ReadOnlySpan<char>)` surface for zero-allocation membership dispatch over protocol text. `FrozenSet` does not expose `GetValueRefOrNullRef` — membership is a boolean test, not a value projection.

[COLLECTION_DISPATCH_TRAVERSE_FORMS]:
- `TraverseM<M, A, B>(Func<A, K<M, B>> f)` on `Iterable<A>`, `Arr<A>`, `Lst<A>`, and `HashSet<A>` threads effects through `Bind`: each element is mapped to an `M`-effect and chained sequentially. The monad `M` determines short-circuit behavior — `IO` and `Eff` short-circuit on first failure; `Validation` with `Applicative` semantics accumulates without short-circuiting via the `Traverse` (not `TraverseM`) form. For dispatch over a bounded collection where the first failure should abort, `TraverseM` with `Eff` is the correct form; for dispatch over an independent collection where all errors should be reported, `Traverse` with `Validation` is correct.
- `Iterable<A>.TraverseIO<M, B>(Func<A, K<M, B>> f, K<Iterable, A> ta)` is the static form with an explicit traversable argument, available when the `Iterable` instance is not the receiver. This is the correct form for effect-sequenced dispatch over a pre-computed sequence when the caller does not own the sequence as a receiver.

[AD_HOC_UNION_TYPE_PARAMETER_DISPATCH_CONSTRAINTS]:
- Ad-hoc union type parameters use `TypeParamRef1`–`TypeParamRef5` as placeholders throughout the declaration. Implicit conversion operators are not generated for type-parameter member types: C# prohibits user-defined conversions involving unconstrained type parameters. When any member triggers factory-method generation (due to type parameter, interface, duplicate type, or `object` member), the generator produces factory methods for all members — the construction surface becomes uniformly factory-method-only. Mixed construction paths (some implicit, some factory) are not possible on ad-hoc unions with type-parameter members.
- The `AllowsRefStructNotSupportedOnAdHocUnion` diagnostic fires unconditionally on ad-hoc unions when the `allows ref struct` anti-constraint is attempted on type parameters. Regular `[Union]` sub-types support `allows ref struct` via `#if NET9_0_OR_GREATER` guards in generated `Switch`/`Map` overloads; net10.0 targets satisfy this condition. Ad-hoc unions are categorically excluded from `allows ref struct` — the backing-field strategy requires reference semantics incompatible with ref structs.

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
// ScheduleTransformer composition: + chains transformers, | applies a transformer to a schedule.
// Schedule.decorrelate() is a Schedule static method returning a ScheduleTransformer.
// RetryPolicy is a reusable transformer chain, applied once per call site via |.
static readonly ScheduleTransformer RetryPolicy =
    Schedule.NoDelayOnFirst
    + Schedule.maxDelay(2000 * ms)
    + Schedule.decorrelate();

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
