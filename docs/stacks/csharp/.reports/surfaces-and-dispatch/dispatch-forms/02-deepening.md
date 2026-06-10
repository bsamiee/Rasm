# Dispatch Forms — Deeper Mechanisms and Failure Surfaces

[GENERATED_SWITCH_EDGE_CASES]:
- `recurs` is a `ScheduleTransformer`, not a `Schedule` — it composes with `|` by applying `s.Take(n)` to the schedule it transforms, not by constructing an independent capped stream. `Schedule.recurs(5) | Schedule.exponential(10 * ms)` is well-formed because `|` has a `(Schedule, ScheduleTransformer)` overload that calls `transformer.Apply(schedule)`. Reversing the sides gives the `(ScheduleTransformer, Schedule)` overload with identical semantics. However `recurs(5) & exponential(10 * ms)` calls the `(ScheduleTransformer, Schedule)` `&` overload, also applying the transformer — the `&` semantics of max-duration intersect applies only to two `Schedule` operands. The transformer overloads for both `|` and `&` simply apply the transformer regardless of which operator was used, so `recurs` is direction-agnostic but is not a duration-capping intersect.
- `Schedule.Union` (the `|` method) produces `SchUnion`, which zips the two duration streams and yields the min at each step for as long as either stream has elements — union extends to the longer schedule's length. `Schedule.Intersect` (the `&` method) produces `SchIntersect`, which zips and yields the max at each step for as long as both streams have elements — intersect truncates to the shorter schedule's length. The practical consequence: `recurs(3) | exponential(100*ms)` recurs 3 times with exponential delays; `exponential(100*ms) | spaced(2000*ms)` recurs indefinitely taking the min, which caps the exponential at 2000ms once the exponential grows past that — `spaced` functions as a delay ceiling under `|`. `exponential(100*ms) & spaced(300*ms)` recurs indefinitely taking the max, which enforces a minimum delay of 300ms — `spaced` functions as a delay floor under `&`.
- `Schedule.NoDelayOnFirst` is a `ScheduleTransformer` that rewrites the schedule's tail: `s => s.Tail.Prepend(Duration.Zero)`. Applied to a retry schedule, the first retry fires immediately with no delay, and the schedule's original delays begin from the second retry onward. This is significant for connection-reset and transient-fault scenarios where an immediate first retry has high success probability: `io.Retry(Schedule.recurs(5) | Schedule.exponential(100*ms) | Schedule.NoDelayOnFirst)`.
- `Map` over a vocabulary is eager: each branch value is computed unconditionally before dispatch resolves. The generated overload signature is `Map<TResult>(TResult caseA, TResult caseB, ...)` — raw values, not functions. Passing a computed expression to a `Map` argument pre-computes that expression at the call site regardless of which case is live. This is never correct for any result whose computation has side effects, allocates, or is conditional on external state. The distinction from `Switch` is not stylistic; it is the difference between O(1) constant selection and O(N) unconditional evaluation.
- `SwitchMapStateParameterName` renames the generated `state` parameter only when a case name camel-cases to `state`. The parameter name is part of the named-argument API that TTRESG046 enforces: callers must use named arguments at `Switch` and `Map` call sites. If the state parameter name collides with a case parameter name and the rename is not applied, the generated method will have duplicate parameter names, which is a compile error. The default name `state` collides when any case type is named `State`, `StateX`, or any type whose camel-case rendering is `state`.
- Generic union type parameters use `TypeParamRef1`–`TypeParamRef5` placeholders in the union declaration, which the source generator substitutes with actual type parameters in all generated members including `Switch` and `Map`. Implicit conversion operators are not generated for type-parameter member types because C# prohibits user-defined conversions involving unconstrained type parameters. When any member requires a factory method (due to type-parameter, interface, or duplicate-type constraints), factory methods are generated for all members — the API surface becomes uniformly factory-method-only for that union rather than a mix.
- `SkipEqualityComparison = true` on an ad-hoc union suppresses `Equals`, `GetHashCode`, and equality operator generation entirely. This has no effect on dispatch — `Switch` and `Map` are not affected. The relevance is that without generated equality, the union cannot be used as a dictionary key or in set operations without providing a custom comparer. This is an intentional trade for unions whose members contain types that are inherently non-equatable (delegates, mutable collections, native handles).

[AD_HOC_UNION_BACKING_FIELD_STRATEGY]:
- Ad-hoc unions have no type discriminator stored at runtime: the union holds only the value in a backing field, and the `Switch`/`Map` dispatch chains `is` type checks against each member type in declaration order. The backing field strategy affects the cost of these `is` checks. With separate typed fields (the default), the dispatch performs one reference check per member type and the correct field read. With `UseSingleBackingField = true`, all values are stored in a single `object?` field — value types are boxed on write and unboxed on the `is` check, but the check is still O(member-count) in the worst case.
- `SingleBackingFieldType` types the backing field as a specified common interface or base class rather than `object?`. This serves two purposes: it narrows the `is` check to types satisfying the interface and allows the backing field to hold the value without boxing when all member types implement a shared reference interface. The default `UseSingleBackingField` boxes; `SingleBackingFieldType` avoids boxing only when all members are reference types implementing the common type. This choice is irrelevant to dispatch performance but matters for GC pressure in tight loops.
- Stateless member types (`T1IsStateless = true`) store no backing field entry for that member — the union stores only a discriminator index. The generated factory for stateless members is parameterless. `Switch`/`Map` arms for stateless members receive no payload value; the arm is a zero-argument lambda. This reduces allocation to the discriminator cost only (typically one field in the owning struct/class) and is the correct form for sentinel or state-only union arms that carry no data.
- The `Normalize` partial method (`static partial void NormalizeMemberName(ref TMember value)`) intercepts value normalization on all construction paths: explicit factory, implicit conversion, JSON deserialization, and EF Core materialization. This executes before the value is stored in the backing field, not at `Switch`/`Map` dispatch time. A normalized union guarantees that every dispatch arm sees the post-normalize form; there is no per-call normalization overhead at dispatch sites.

[EXTENSION_BLOCK_DISPATCH_CONSTRAINTS]:
- Extension block members cannot have the modifiers `abstract`, `virtual`, `override`, `new`, `sealed`, `partial`, or `protected`. This means extension blocks cannot participate in inheritance-based dispatch: they cannot define virtual methods for subtypes to override or abstract contracts for derived types to fulfill. Extension blocks are purely additive behavior on the receiver's existing type hierarchy. The rejected form is an extension block attempting to serve as a mixin or trait system.
- The receiver in an extension block can specify `ref`, `ref readonly`, or `in` modifiers, but only when the receiver type is a struct or is known to be a value type. An extension block with `extension(ref ValueType v)` enables mutating extension methods on value types without copying. This is the correct form for high-throughput span-shaped dispatch that needs to mutate accumulator state while avoiding copy overhead.
- Type parameters for extension blocks must satisfy inferrability: for every non-method extension member (properties, operators), all of the extension block's type parameters must appear in the combined parameter set of the extension plus the member. A generic type parameter that appears only in the body but not in any parameter is a compile error. This constraint is tighter than for methods, where inference from the receiver alone can satisfy type parameters.
- Multiple extension blocks in the same static class targeting overlapping receiver types share a single declaration space when the receiver types are identical modulo identity conversion and type-parameter substitution. Duplicate member signatures across such blocks are compile errors. The uniqueness check treats classic extension methods in the same static class as having an implicit receiver specification matching their `this` parameter — a classic extension method and a new-style extension block member with the same signature on the same receiver type are duplicate declarations.
- `[OverloadResolutionPriority]` (BCL, target: `Method | Constructor | Property`, not applicable to operators or indexers) accepts any `int` value; priority 0 is the default. Higher numbers win. The priority is evaluated after all other overload resolution rules have produced an ambiguous candidate set — it breaks ties only when two candidates are otherwise equally applicable. A higher-priority extension method does not suppress a more specifically typed regular method: specificity in type conversion still precedes priority. Applying priority to extension block members versus classic extension methods resolves ambiguity within the same declaration space only.

[FROZEN_TABLE_INTERNAL_STRATEGY]:
- `FrozenDictionary` selects its internal implementation from a fixed strategy hierarchy at construction time. For empty input, a singleton `EmptyFrozenDictionary` is returned. For small collections below `Constants.MaxItemsInSmallFrozenCollection` (a compile-time constant in the runtime), `SmallFrozenDictionary` performs a linear search — no hash computation. For integral value-type keys with default comparers, `Int32FrozenDictionary` (exact int keys) or `DenseIntegralFrozenDictionary` (dense range of integral values mapping to array indices) are selected when the key range is dense enough. For string keys with ordinal comparers, `LengthBucketsFrozenDictionary` or one of the `OrdinalStringFrozenDictionary_*` variants (keyed by left-justified or right-justified substring analysis of minimum uniqueness) is selected. The default fallback is `DefaultFrozenDictionary` using computed perfect-hash slots.
- The construction cost of `FrozenDictionary` is dominated by the strategy selection analysis: key characteristics, range density, and string uniqueness analysis are computed once at construction. Passing untrusted or adversarially crafted keys into construction can degrade the strategy analysis by forcing the fallback. The docs note: "should only be initialized with trusted keys" — untrusted keys that happen to have pathological hash distribution will force the generic path and increase construction time.
- `GetAlternateLookup<TAlternate>()` throws `InvalidOperationException` if the dictionary's comparer does not implement `IAlternateEqualityComparer<TAlternate, TKey>`. `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate>)` is the safe form that returns `false` instead of throwing. The mismatch is always a configuration bug (wrong comparer at construction), so `TryGetAlternateLookup` at startup with a hard fail on `false` is the correct startup probe.
- `GetValueRefOrNullRef(TKey key)` returns a `ref TValue` into the internal storage, tested with `Unsafe.IsNullRef`. This is a single-probe path — the reference points directly into the frozen array without a copy. This is the correct pattern for read-heavy kernels where the double-lookup of `TryGetValue` (hash probe + value copy) is measurably slower. The reference is valid for the lifetime of the frozen dictionary instance. It must not be used across a dictionary replacement boundary: a new `FrozenDictionary` instance has a different internal array.

[PARTIAL_DISPATCH_BOUNDARY]:
- `[UnionSwitchMapOverload(StopAt = new[] { typeof(IntermediateCase) })]` generates a `Switch`/`Map` overload that accepts handlers for the direct children of the union up to and including the stop type, treating the stop type's entire derived subtree as one arm. The generated overload does not verify exhaustiveness below the stop type. Adding a new leaf case under `IntermediateCase` will not break the call site using the stop-at overload — a gap that total dispatch would catch at compile time. The stop-at overload is warranted only when a specific caller legitimately treats a sub-hierarchy as an opaque unit and further decomposition belongs elsewhere.
- `SwitchPartially`/`MapPartially` (generated when `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`) require a `@default` case as the first named argument and accept only the cases the caller explicitly provides. The `@default` arm fires for any case not listed. This form is permanent caller-owned incompleteness; the cases the caller omits are handled uniformly. The critical difference from stop-at: partial overloads express "I handle some cases and delegate the rest to `@default`"; stop-at overloads express "I handle all cases but treat this subtree as one atom". A stop-at overload with the stop type as the only arm is the same as a partial overload with one case — both compile, but the stop-at form carries the semantic claim that the subtree is intentionally opaque.
- Total `Switch` with a `@default` arm that unconditionally throws is distinguishable from a total switch with no `@default` by the fact that the generated exhaustive `Switch` has no `@default` parameter at all — it is a compile-time guarantee. A manually written switch expression with `_ => throw` is only a runtime guarantee. TTRESG046 enforces named arguments on `Switch`/`Map` calls, which makes a missing case argument a compile error rather than a silent fallthrough or a runtime throw. The distinction between generated exhaustiveness and authored exhaustiveness is relevant when the union grows: generated total `Switch` breaks at all call sites on case addition; authored `_ => throw` compiles but fails only at runtime.

[DISPATCH_FORM_INTERACTION_PRESSURE]:
- A frozen lookup table keyed by smart-enum items restates what delegate rows already own, and adds a failure mode: a new smart-enum item whose delegate row is defined at construction will not appear in the frozen table unless the table construction code is also updated. The delegate row is constructor-enforced at compile time (missing argument is a compile error); the frozen table entry is runtime-populated (missing entry is a silent lookup miss). When the result is a static data mapping from one of the vocabulary's own keys, delegate rows are always the correct form.
- The state-threading closure-free discipline and the frozen table form create an integration pressure in algorithms that need both vocabulary-owned behavior and cross-vocabulary join data. The correct architecture: delegate rows own behavior that is per-item and vocabulary-scoped; frozen tables own cross-vocabulary correspondences. An arm in a state-threaded `Switch` that probes a frozen table to complete its logic is a valid composition — the Switch arm is the dispatch mechanism and the frozen table lookup is a data retrieval step, not a second dispatch mechanism.
- Extension blocks and generated `Switch` interact when the receiving module adds domain projection to a foreign union type. The extension block provides the instance-method syntax (`foreignUnion.ToMyDomainType()`); the `Switch` call inside the extension method is generated exhaustive dispatch over the foreign union. This is the correct boundary form: the extension block owns the module-to-domain translation surface; the Switch arms are the per-case projections; no helper method exists between them.
- Structural pattern dispatch and generated `Switch` must not coexist on the same closed vocabulary. A structural pattern switch over a `[Union]`'s case types compiles but loses generated exhaustiveness: adding a new case to the `[Union]` does not break the structural switch site. The correct form for closed vocabularies is always generated `Switch`. Structural patterns are the correct form only when the vocabulary is genuinely open (third-party subtypes, protocol-driven inputs) or when the discriminant is on the value's shape rather than its type tag.

[COMPOSITION_TIME_ASPECTS_DEEPER]:
- `CatchM<E, M, A>` is a `readonly record struct` holding a `(Func<E, bool> Match, Func<E, K<M, A>> Action)` pair. The `|` operator on `K<M, A>` is defined in an extension block: `static K<F, A> operator |(K<F, A> lhs, CatchM<E, F, A> rhs) => lhs.Catch(rhs)`. `Catch` runs `rhs.Run(error, otherwise)` which calls `rhs.Match(error)` before invoking `rhs.Action`. The chaining `computation | @catch(errorA, ...) | @catch(errorB, ...)` applies left-associatively: the first `|` wraps the computation with the first handler; the second `|` wraps the result with the second handler. The outer handler sees failures that the inner handler did not match and re-threw. First match wins only because inner handlers execute before outer handlers — the composition is handler wrapping, not handler list scanning.
- `exceptional<M, A>` matches `Exceptional` (the subtype of `Error` representing unhandled runtime exceptions, not domain expected failures). `catchOf<E, M, A>` matches errors of subtype `E` using `e is E || e.IsType<E>()` — the `IsType<E>()` test handles `Error` aggregates where one of the nested errors is of type `E`. `@catch` with a predicate function is the most general form and composes with any custom error discrimination logic. The `Fold` variants (`catchOfFold`, `exceptionalFold`) use `MonoidK<M>` to fold across aggregate errors; the non-fold variants use `ForAllM` which short-circuits on the first match inside an aggregate. When aggregate errors carry mixed types, `catchOf<E>` + fold is more complete than `catchOf<E>` + ForAll.
- `BracketFail` releases resources only on failure, returning them live to the caller on success. The three-argument `Bracket<B,C>(Use, Catch, Fin)` runs `Catch` between `Use` failure and `Fin` cleanup, allowing failure-specific cleanup logic that differs from success cleanup. The `Fin` projection runs in both cases — it is the unconditional finalizer, while `Catch` is the conditional failure handler. Nesting `Bracket` within a `Retry`: resources acquired inside the bracketed computation are released on each retry's failure before the next attempt, because the bracket scope is per-attempt. Resources acquired outside the bracket persist across retries. This ordering matters when the retried computation holds a borrowed connection from a pool: the bracket must scope to the per-attempt acquire/release, and the retry wraps the outer bracket.

```csharp
// Schedule algebra: Union extends to longer, Intersect truncates to shorter.
// recurs(3) | exponential(100*ms) — three retries with exponential delays; union extends to recurs length.
// exponential(100*ms) | spaced(2000*ms) — indefinite retries; union takes min, so spaced caps the delay.
// exponential(100*ms) & spaced(300*ms) — indefinite retries; intersect takes max, so spaced floors the delay.
// NoDelayOnFirst | recurs(5) | exponential(50*ms) — five retries, first immediate, then exponential.
static IO<TResult> WithPolicy<TResult>(IO<TResult> op) =>
    op.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(4) | Schedule.exponential(100 * ms))
    | @catch<IO, TResult>(Errors.Cancelled, IO.fail<TResult>(Errors.Cancelled))
    | @catch<IO, TResult>(e => e.IsExpected, e => IO.fail<TResult>(e));
```

```csharp
// CatchM chaining: left-associative wrapping. Inner handlers see errors first.
// @catch(specificCode, recovery) wraps the computation.
// The outer @catch(IsExpected, ...) wraps the wrapped result.
// An error matching specificCode is handled by recovery; the outer handler never sees it.
// An Expected error not matching specificCode passes through to the outer handler.
IO<TValue> guarded =
    operation
    | @catch<IO, TValue>(ErrorCodes.Transient, _ => fallback)
    | @catch<IO, TValue>(e => e.IsExpected, e => IO.fail<TValue>(e));
```

```csharp
// Frozen strategy: string keys use length-bucket or substring-uniqueness analysis.
// OrdinalIgnoreCase implements IAlternateEqualityComparer<ReadOnlySpan<char>, string>.
// TryGetAlternateLookup at startup gates the span-based path safely.
static readonly FrozenDictionary<string, Policy> Policies =
    FrozenDictionary.Create(
        StringComparer.OrdinalIgnoreCase,
        (ReadOnlySpan<KeyValuePair<string, Policy>>)[
            new("strict", Policy.Strict),
            new("lenient", Policy.Lenient),
        ]);

static readonly bool _hasSpanLookup =
    Policies.TryGetAlternateLookup<ReadOnlySpan<char>>(out var _spanPolicies);

static bool TryResolve(ReadOnlySpan<char> key, out Policy policy)
{
    ref var v = ref Policies.GetValueRefOrNullRef(key.ToString());
    policy = Unsafe.IsNullRef(ref v) ? default : v;
    return !Unsafe.IsNullRef(ref v);
}
```

```csharp
// Generated Switch totality vs authored: adding AlphaCase to [Union] below breaks
// every site calling Switch without @default — compile-time error.
// Authored switch with _ => throw compiles but silently misses AlphaCase at runtime.
[Union]
public partial record Shape
{
    public sealed record Circle(double Radius) : Shape;
    public sealed record Rect(double W, double H) : Shape;
}

// Correct — generated totality:
double area = shape.Switch(
    circle: static (_, c) => Math.PI * c.Radius * c.Radius,
    rect: static (_, r) => r.W * r.H,
    state: unit);

// Wrong — authored exhaustiveness with runtime-only guarantee:
double area2 = shape switch
{
    Shape.Circle c => Math.PI * c.Radius * c.Radius,
    Shape.Rect r => r.W * r.H,
    _ => throw new UnreachableException(),
};
```
