# Dispatch Forms — Deeper Mechanisms and Failure Surfaces

[GENERATED_SWITCH_EDGE_CASES]:
- `recurs` is a `ScheduleTransformer`, not a `Schedule` — it applies `s.Take(n)` to the schedule it transforms. `Schedule.recurs(5) | Schedule.exponential(10 * ms)` is well-formed because `|` has a `(Schedule, ScheduleTransformer)` overload that calls `transformer.Apply(schedule)`. Reversing the sides gives the `(ScheduleTransformer, Schedule)` overload with identical semantics. `recurs(5) & exponential(10 * ms)` calls the same `Apply` path — the `&` semantics of max-duration intersect applies only to two `Schedule` operands; the transformer overloads for both `|` and `&` simply apply the transformer regardless of the operator, so `recurs` is direction-agnostic.
- `Schedule.Union` (the `|` method) produces `SchUnion`, which zips the two duration streams and yields the min at each step for as long as either stream has elements — union extends to the longer schedule's length. `Schedule.Intersect` (the `&` method) produces `SchIntersect`, which zips and yields the max at each step for as long as both streams have elements — intersect truncates to the shorter schedule's length. The practical consequence: `recurs(3) | exponential(100*ms)` recurs three times with exponential delays; `exponential(100*ms) | spaced(2000*ms)` recurs indefinitely taking the min, capping the exponential at 2000ms once it grows past that — `spaced` functions as a delay ceiling under `|`. `exponential(100*ms) & spaced(300*ms)` recurs indefinitely taking the max, enforcing a minimum delay of 300ms — `spaced` functions as a delay floor under `&`.
- `Schedule.NoDelayOnFirst` is a `ScheduleTransformer` that rewrites the schedule's tail: `s => s.Tail.Prepend(Duration.Zero)`. Applied to a retry schedule, the first retry fires immediately with no delay, and the schedule's original delays begin from the second retry onward. This is significant for connection-reset and transient-fault scenarios where an immediate first retry has high success probability: `io.Retry(Schedule.recurs(5) | Schedule.exponential(100*ms) | Schedule.NoDelayOnFirst)`.
- `Map` over a vocabulary is eager: each branch value is computed unconditionally before dispatch resolves. The generated overload signature is `Map<TResult>(TResult caseA, TResult caseB, ...)` — raw values, not functions. Passing a computed expression to a `Map` argument pre-computes that expression at the call site regardless of which case is live. The distinction from `Switch` is not stylistic; it is the difference between O(1) constant selection and O(N) unconditional evaluation.
- `SwitchMapStateParameterName` renames the generated `state` parameter only when a case name camel-cases to `state`. The parameter name is part of the named-argument API that TTRESG046 enforces: callers must use named arguments at `Switch` and `Map` call sites. If the state parameter name collides with a case parameter name and the rename is not applied, the generated method has duplicate parameter names, which is a compile error. The default name `state` collides when any case type is named `State`, `StateX`, or any type whose camelCase rendering is `state`.
- Generic union type parameters use `TypeParamRef1`–`TypeParamRef5` placeholders in the union declaration, which the source generator substitutes with actual type parameters in all generated members including `Switch` and `Map`. Implicit conversion operators are not generated for type-parameter member types because C# prohibits user-defined conversions involving unconstrained type parameters. When any member requires a factory method (due to type-parameter, interface, or duplicate-type constraints), factory methods are generated for all members — the API surface becomes uniformly factory-method-only for that union rather than a mix.
- `SkipEqualityComparison = true` on an ad-hoc union suppresses `Equals`, `GetHashCode`, and equality operator generation but has no effect on dispatch — `Switch` and `Map` are unaffected. Without generated equality, the union cannot be used as a dictionary key or in set operations without a custom comparer. This is an intentional trade for unions whose members contain types that are inherently non-equatable (delegates, mutable collections, native handles).

[AD_HOC_UNION_BACKING_FIELD_STRATEGY]:
- Ad-hoc unions have no type discriminator stored at runtime: the union holds only the value in a backing field, and the `Switch`/`Map` dispatch chains `is` type checks against each member type in declaration order. With separate typed fields (the default), dispatch performs one reference check per member type and the correct field read. With `UseSingleBackingField = true`, all values are stored in a single `object?` field — value types are boxed on write and unboxed on the `is` check, but the check is still O(member-count) in the worst case.
- `SingleBackingFieldType` types the backing field as a specified common interface or base class rather than `object?`. It avoids boxing only when all member types are reference types implementing the common type. The default `UseSingleBackingField` boxes value types regardless; `SingleBackingFieldType` narrows the `is` check to types satisfying the interface. This choice is irrelevant to dispatch performance but matters for GC pressure in tight loops.
- Stateless member types (`T1IsStateless = true`) store no backing field entry for that member — the union stores only a discriminator index. The generated factory for stateless members is parameterless. `Switch`/`Map` arms for stateless members receive no payload value; the arm is a zero-argument lambda. This reduces allocation to the discriminator cost only and is the correct form for sentinel or state-only union arms that carry no data.
- The `Normalize` partial method (`static partial void NormalizeMemberName(ref TMember value)`) intercepts value normalization on all construction paths: explicit factory, implicit conversion, JSON deserialization, and EF Core materialization. Normalization executes before the value is stored in the backing field, not at `Switch`/`Map` dispatch time — every dispatch arm sees the post-normalize form with no per-call normalization overhead.

[EXTENSION_BLOCK_DISPATCH_CONSTRAINTS]:
- Extension block members cannot have the modifiers `abstract`, `virtual`, `override`, `new`, `sealed`, `partial`, or `protected`. Extension blocks cannot participate in inheritance-based dispatch: they cannot define virtual methods for subtypes to override or abstract contracts for derived types to fulfill. Extension blocks are purely additive behavior on the receiver's existing type hierarchy. Any architecture that attempts to use extension blocks as a mixin or trait system must fall back to interface-based or abstract-class-based dispatch.
- The receiver in an extension block can specify `ref`, `ref readonly`, or `in` modifiers, but only when the receiver type is a struct or is known to be a value type. An extension block with `extension(ref ValueType v)` enables mutating extension methods on value types without copying. This is the correct form for high-throughput span-shaped dispatch that needs to mutate accumulator state while avoiding copy overhead.
- Type parameters for extension blocks must satisfy inferrability: for every non-method extension member (properties, operators), all of the extension block's type parameters must appear in the combined parameter set of the extension plus the member. A generic type parameter that appears only in the body but not in any parameter is a compile error. This constraint is tighter than for methods, where inference from the receiver alone can satisfy type parameters.
- Multiple extension blocks in the same static class targeting overlapping receiver types share a single declaration space when the receiver types are identical modulo identity conversion and type-parameter substitution. Duplicate member signatures across such blocks are compile errors. A classic extension method and a new-style extension block member with the same signature on the same receiver type are duplicate declarations.
- `[OverloadResolutionPriority]` accepts any `int` value; priority 0 is the default. Higher numbers win. The priority is evaluated after all other overload resolution rules have produced an ambiguous candidate set — it breaks ties only when two candidates are otherwise equally applicable. A higher-priority extension method does not suppress a more specifically typed regular method: specificity in type conversion precedes priority.

[FROZEN_TABLE_INTERNAL_STRATEGY]:
- `FrozenDictionary` selects its internal implementation from a fixed strategy hierarchy at construction time. For empty input, a singleton `EmptyFrozenDictionary` is returned. For small collections, three distinct strategies are selected based on the key type: `SmallFrozenDictionary` (reference types and non-`IComparable` value types), `SmallValueTypeComparableFrozenDictionary` (small value types implementing `IComparable<TKey>`), and `SmallValueTypeDefaultComparerFrozenDictionary` (small value types with the default equality comparer) — all three perform linear equality scanning with no hash computation. For integral value-type keys with default comparers, `Int32FrozenDictionary` or `DenseIntegralFrozenDictionary` are selected when the key range is dense enough. For string keys with ordinal comparers, one of the `OrdinalStringFrozenDictionary_*` variants (left-justified or right-justified substring analysis of minimum uniqueness) is selected. The default fallback is `DefaultFrozenDictionary` using computed perfect-hash slots.
- The construction cost of `FrozenDictionary` is dominated by the strategy selection analysis: key characteristics, range density, and string uniqueness analysis are computed once at construction. The docs note: "should only be initialized with trusted keys" — untrusted keys with pathological hash distribution force the generic path and increase construction time.
- `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup)` is the safe startup form for the span-lookup path; on comparer mismatch it returns `false` and produces a default instance that silently returns null-ref on every probe. A `false` result must be treated as a hard startup failure — the fallback is a completely broken lookup, not an exception.
- `GetValueRefOrNullRef(TKey key)` returns a `ref TValue` into the internal storage, tested with `Unsafe.IsNullRef`. This is the single-probe path for read-heavy kernels — the reference points directly into the frozen array without a copy. The reference is valid for the lifetime of the frozen dictionary instance. It must not be used across a dictionary replacement boundary: a new `FrozenDictionary` instance has a different internal array.

[PARTIAL_DISPATCH_BOUNDARY]:
- `[UnionSwitchMapOverload(StopAt = new[] { typeof(IntermediateCase) })]` generates a `Switch`/`Map` overload that treats the stop type's entire derived subtree as one arm. Adding a new leaf case under `IntermediateCase` will not break the call site using the stop-at overload — a gap that total dispatch would catch at compile time. The stop-at overload is warranted only when a specific caller legitimately treats a sub-hierarchy as an opaque unit and further decomposition belongs elsewhere.
- `SwitchPartially`/`MapPartially` (generated when `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`) require a `@default` case as the first named argument and accept only the cases the caller explicitly provides. The critical difference from stop-at: partial overloads express "I handle some cases and delegate the rest to `@default`"; stop-at overloads express "I handle all cases but treat this subtree as one atom." A stop-at overload with the stop type as the only arm is structurally the same as a partial overload with one case — both compile, but the stop-at form carries the semantic claim that the subtree is intentionally opaque.
- Total `Switch` with a `@default` arm that unconditionally throws is distinguishable from total switch with no `@default` by the fact that the generated exhaustive `Switch` has no `@default` parameter at all — it is a compile-time guarantee. TTRESG046 enforces named arguments on `Switch`/`Map` calls, which makes a missing case argument a compile error rather than a silent fallthrough or a runtime throw. The distinction between generated exhaustiveness and authored exhaustiveness is critical when the union grows: generated total `Switch` breaks at all call sites on case addition; authored `_ => throw` compiles but silently misses the new case at runtime.

[DISPATCH_FORM_INTERACTION_PRESSURE]:
- A frozen lookup table keyed by smart-enum items restates what delegate rows already own and adds a failure mode: a new smart-enum item whose delegate row is defined at construction will not appear in the frozen table unless the table construction code is also updated. The delegate row is constructor-enforced at compile time (missing argument is a compile error); the frozen table entry is runtime-populated (missing entry is a silent lookup miss). When the result is a static data mapping from one of the vocabulary's own keys, delegate rows are always the correct form.
- The state-threading closure-free discipline and the frozen table form create an integration pressure in algorithms that need both vocabulary-owned behavior and cross-vocabulary join data. Delegate rows own behavior that is per-item and vocabulary-scoped; frozen tables own cross-vocabulary correspondences. A `Switch` arm that probes a frozen table to complete its logic is a valid composition — the `Switch` arm is the dispatch mechanism and the frozen table lookup is a data retrieval step, not a second dispatch mechanism.
- Extension blocks and generated `Switch` interact at foreign union boundaries: the extension block provides the instance-method syntax (`foreignUnion.ToMyDomainType()`); the `Switch` call inside the extension method is generated exhaustive dispatch over the foreign union. The extension block owns the module-to-domain translation surface; the `Switch` arms are the per-case projections; no helper method exists between them.
- Structural pattern dispatch and generated `Switch` must not coexist on the same closed vocabulary. A structural pattern switch over a `[Union]`'s case types compiles but loses generated exhaustiveness: adding a new case to the `[Union]` does not break the structural switch site. The correct form for closed vocabularies is always generated `Switch`. Structural patterns are the correct form only when the vocabulary is genuinely open (third-party subtypes, protocol-driven inputs) or when the discriminant is on the value's shape rather than its type tag.

[COMPOSITION_TIME_ASPECTS_DEEPER]:
- `CatchM<E, M, A>` is a `readonly record struct` holding a `(Func<E, bool> Match, Func<E, K<M, A>> Action)` pair. The `|` operator on `K<M, A>` is defined in an extension block: `static K<F, A> operator |(K<F, A> lhs, CatchM<E, F, A> rhs) => lhs.Catch(rhs)`. The chaining `computation | @catch(errorA, ...) | @catch(errorB, ...)` applies left-associatively: the first `|` wraps the computation with the first handler; the second `|` wraps the result with the second handler. The outer handler sees failures that the inner handler did not match and re-threw. First match wins only because inner handlers execute before outer handlers — the composition is handler wrapping, not handler list scanning.
- `@catchOf<E, M, A>` matches errors of subtype `E` using `e is E || e.IsType<E>()` — the `IsType<E>()` test handles `Error` aggregates where one of the nested errors is of type `E`. `@catch` with a predicate function is the most general form and composes with any custom error discrimination logic. The `Fold` variants (`catchOfFold`, `exceptionalFold`) use `MonoidK<M>` to fold across aggregate errors in one accumulation step; the non-fold variants use `ForAllM`, which threads the handler monadically through filtered errors via `Bind` and short-circuits on the first failure. When aggregate errors carry mixed types, `catchOf<E>` + fold is more complete than `catchOf<E>` + ForAll.
- `BracketFail` releases resources only on failure, returning them live to the caller on success. The three-argument `IO<A>.Bracket(Use, Catch, Fin)` runs `Catch` between `Use` failure and `Fin` cleanup, allowing failure-specific cleanup logic that differs from success cleanup; `Fin` runs unconditionally in both cases. Nesting `Bracket` within a `Retry`: resources acquired inside the bracketed computation are released on each retry's failure before the next attempt, because the bracket scope is per-attempt. Resources acquired outside the bracket persist across retries. This ordering matters when the retried computation holds a borrowed connection from a pool: the bracket must scope to the per-attempt acquire/release, and the retry wraps the outer bracket.

```csharp
// Schedule algebra: Union extends to longer, Intersect truncates to shorter.
// recurs(3) | exponential(100*ms) — three retries with exponential delays.
// exponential(100*ms) | spaced(2000*ms) — indefinite retries; spaced caps the exponential delay via union min.
// exponential(100*ms) & spaced(300*ms) — indefinite retries; spaced floors the delay via intersect max.
// NoDelayOnFirst | recurs(5) | exponential(50*ms) — five retries, first immediate, then exponential.
static IO<TResult> WithPolicy<TResult>(IO<TResult> op) =>
    op.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(4) | Schedule.exponential(100 * ms))
    | @catch<IO, TResult>(Errors.Cancelled, IO.fail<TResult>(Errors.Cancelled))
    | @catch<IO, TResult>(e => e.IsExpected, e => IO.fail<TResult>(e));
```

```csharp
// CatchM chaining: left-associative wrapping. Inner handlers see errors first.
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
// TryGetAlternateLookup at startup gates the span-based path; false result is a hard failure.
static readonly FrozenDictionary<string, Policy> Policies =
    FrozenDictionary.Create(
        StringComparer.OrdinalIgnoreCase,
        (ReadOnlySpan<KeyValuePair<string, Policy>>)[
            new("strict", Policy.Strict),
            new("lenient", Policy.Lenient),
        ]);

static readonly FrozenDictionary<string, Policy>.AlternateLookup<ReadOnlySpan<char>> SpanPolicies =
    Policies.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)
        ? lookup
        : throw new InvalidOperationException("comparer incompatible with span lookup");

static bool TryResolve(ReadOnlySpan<char> key, out Policy policy) =>
    SpanPolicies.TryGetValue(key, out policy);
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
