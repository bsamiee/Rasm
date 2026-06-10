# Dispatch Forms — Selection Law and Composition Architecture

[FORM_TAXONOMY]:
- Five structurally distinct dispatch forms exist and are not interchangeable: state-threaded generated `Switch` for closed-family context sharing; case-owned delegate rows for behavior belonging to the case; frozen lookup tables for bounded vocabulary keys; extension blocks for receiver-owned surfaces on foreign types; structural pattern dispatch where input shape — not type tag — discriminates. Selection is governed by where ownership lives, not by call-site convenience.
- The selection criterion is ownership of behavior: delegate rows when the vocabulary item is the behavior and every consumer wants the same effect; state-threaded `Switch` when the consumer owns the logic but the vocabulary owns exhaustiveness; frozen tables when the key is a value and the result is static data derivable at type-load time; extension blocks when the receiver type is foreign but the behavior belongs to the consuming module; structural pattern dispatch when the input's shape (list length, property combination, span content) is the discriminant rather than its nominal type.
- Mixing forms at one call site is an architecture failure: a `Switch` arm that opens a secondary lookup table and then invokes a delegate means the boundary between what the vocabulary owns and what the consumer owns is unresolved. Resolve the boundary first; the form selection is then mechanical.

[STATE_THREADED_GENERATED_SWITCH]:
- Generated `Switch` over a union or smart enum threads exactly one explicit state value: `Switch<TState>(TState state, Action<TState, CaseA> caseA, ...)` and `Switch<TState, TResult>(TState state, Func<TState, CaseA, TResult> caseA, ...)`. Both `TState` and `TResult` carry `allows ref struct`, enabling `ReadOnlySpan<T>`, `Span<T>`, and ref-struct accumulator types to traverse dispatch without boxing. The stateless overloads (`Switch<TResult>(Func<CaseA, TResult> caseA, ...)`) serve trivial projections where no external context is needed.
- State threading is the closure-free discipline: a non-static lambda in a `Switch` or `Map` call draws TTRESG1001 (info severity), steering every capture through the state parameter. Each arm is a pure transformation of (state, case-payload) with zero implicit capture — the state parameter is the sole sanctioned channel.
- Context with multiple fields enters as a value tuple or a record struct: `Switch<(int Score, double Threshold), bool>(state: (score, threshold), alpha: static (s, _) => s.Score >= s.Threshold, ...)`. Tuples are zero-allocation for small arities; record structs serve when the context has identity and the state participates in further dispatch inside the arm.
- The state parameter name defaults to `"state"` and is configurable per-owner via `SwitchMapStateParameterName` to resolve collisions when a case name camel-cases to `state`. The parameter name is part of the named-argument API that TTRESG046 enforces: callers must use named arguments at `Switch` and `Map` call sites, so a collision between the state parameter name and a case parameter name produces a compile error. The default name `state` collides only when a case type's camelCase rendering is exactly `state` — the type named `State` is the canonical trigger; a type named `StateX` camelCases to `stateX`, which does not collide.
- `Map<TResult>` is eager value dispatch with the overload signature `Map<TResult>(TResult caseA, TResult caseB, ...)` — raw values, not functions. Every branch value is computed unconditionally before dispatch executes: passing a computed expression to a `Map` argument pre-computes it at the call site regardless of which case is live. Use only for constant verdict tables and O(1) projections; never for computations whose cost depends on which case is live.
- `SwitchMapMethodsGeneration.None` suppresses the entire `Switch`/`Map` family on a vocabulary whose purpose is data carriage: correct for DTO-shaped or wire-protocol unions that feed a single consumer's structural pattern match and have no behavioral polymorphism. The backing field, implicit conversion operators, and equality members are unaffected — a `None` union is still a fully-functional value type inspectable with `is` and property patterns.
- Generic union type parameters use `TypeParamRef1`–`TypeParamRef5` placeholders in the union declaration, which the source generator substitutes with actual type parameters in all generated members including `Switch` and `Map`. Implicit conversion operators are not generated for type-parameter member types because C# prohibits user-defined conversions involving unconstrained type parameters. When any member triggers factory-method generation (due to type-parameter, interface, duplicate-type, or `object` member), factory methods are generated for all members — the construction surface becomes uniformly factory-method-only; mixed construction paths (some implicit, some factory) are categorically impossible on ad-hoc unions with type-parameter members.
- The `allows ref struct` anti-constraint TTRESG073 (error) targets the ad-hoc union's own declared type parameters: the diagnostic reads "Ad-hoc union '{0}' has type parameter '{1}' with 'allows ref struct' which is not supported." It fires on the union declaration itself when any of its type parameters carry `allows ref struct` — not on the generated `Switch<TState>`/`Switch<TResult>` overloads. Regular `[Union]` sub-types emit `#if NET9_0_OR_GREATER` guards around these constraints, so the ref-struct path is available. When ref-struct state threading is required, regular `[Union]` over named nested cases is the admitted form.
- `SkipEqualityComparison = true` on a union — regular or ad-hoc — suppresses `Equals`, `GetHashCode`, and equality operator generation but has no effect on dispatch: `Switch` and `Map` remain fully generated. The correct use is an ephemeral dispatch carrier holding delegates, mutable state, or native handles — suppress equality to prevent accidental use as a dictionary key while retaining dispatch capability.
- All dispatch in generated ad-hoc unions routes through `private readonly int _valueIndex`: `Switch`, `Map`, `ToString`, `GetHashCode`, and `Equals` all execute `switch (this._valueIndex)` — an integer-discriminator switch, not an `is`-chain over member types. With `UseSingleBackingField = false` (the default), separate typed fields are stored per member type and the relevant field is read inside the matching `_valueIndex` arm. With `UseSingleBackingField = true` (a `bool` property; the XML doc states the backing field is typed `Object` for all members), all values are stored in a single `object?` field — value types are boxed on write and cast on read inside the arm.
- Stateless member types (`T1IsStateless = true`) store no backing field entry — only the discriminator index. The generated `Switch`/`Map` arm for a stateless member has the same `Func<TState, Tn, TResult>` shape as non-stateless arms; the generator passes `default(Tn)` at the call site rather than a stored value. The arm lambda is therefore `static (s, _) => ...` — it receives two parameters and discards the case payload. `T1Name`, `T2Name`, etc. rename all generated identifiers for that member: the factory method name, the `Switch`/`Map` parameter name, the `AsT1`-style accessor name, and implicit conversion member names — the renaming is total. Use `TnName` when two members share the same simple type name (two `Status` types from different namespaces) to produce a well-formed union rather than a duplicate-name compile error.
- `NestedUnionParameterNameGeneration.Simple` collapses intermediate type names from generated `Switch`/`Map` parameter identifiers: a case `Failure.NotFound` emits parameter `notFound` instead of `failureNotFound`. The failure mode is specific: when two sibling nested unions each have a case with the same simple name, the outer union's generated `Switch` accumulates duplicate parameter names (CS0100). `NestedUnionParameterNameGeneration.Default` is safe by construction; `Simple` is the writer-controlled tradeoff between call-site legibility and structural collision risk.
- A `[Union]` whose root is declared `abstract` with `private` constructors and whose cases are `sealed` nested records is the canonical discriminated union shape and satisfies TTRESG054. Nested `[Union]` hierarchies (abstract intermediate unions) require class nodes at non-leaf levels, not records. A `[Union]` on a `sealed` class with no nested hierarchy is the sealed-leaf form — `SwitchMapMethodsGeneration.None` is its only useful configuration, because a sealed single-case union has one arm and no dispatch value.

```csharp
// Generated totality vs authored: adding AlphaCase to [Union] below breaks
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

```csharp
// Ad-hoc union with generic member: typeof(Value<>) — unbound form, not typeof(Value<T>).
// T1IsStateless and SkipEqualityComparison are property initializers, not constructor params.
// Stateless arm discards the case payload; generator passes default(NotFound) at the call site.
[AdHocUnion(typeof(NotFound), typeof(Value<>),
    T1IsStateless = true,
    SkipEqualityComparison = true)]
public partial struct Outcome<T> where T : notnull { }

double resolved = outcome.Switch(
    notFound: static (s, _) => s,
    value: static (_, v) => v.Data,
    state: 0.0);
```

```csharp
// Nested union: Simple parameter naming; AllowMultiple stop-at overloads.
// Coarse overload coarsens Failure subtree to one arm; exhaustive overload exposes all.
// Adding a new leaf under Failure breaks exhaustive overload call sites; coarse overload is unaffected.
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

// Coarse: success + failure subtree as one atom.
string label = response.Switch(
    success: static (_, _) => "ok",
    failure: static (_, _) => "error",
    state: unit);

// Exhaustive: success + notFound + unauthorized.
string detail = response.Switch(
    success: static (_, _) => "200",
    notFound: static (_, _) => "404",
    unauthorized: static (_, _) => "401",
    state: unit);
```

[DELEGATE_ROWS_AND_CASE_OWNED_BEHAVIOR]:
- `[UseDelegateFromConstructor]` on a partial method in a smart enum converts the method into a per-item delegate column: each item row injects its own implementation via a trailing constructor argument. The dispatch cost is one private delegate field read plus an invoke — no index lookup, no switch, no dictionary probe. The column is enforced: every new item must provide an argument for every delegate column, and missing arguments are compile-time arity failures.
- A custom delegate type is generated only when the method has `ref`/`in`/`out` parameters or when `DelegateName` is set; otherwise `Func<TArg, TResult>` or `Action<TArg>` back the field. The method's declared access is preserved: private behavior rows are legal and expose no delegate-typed public state.
- N parallel behaviors sharing the same inputs collapse into one delegate returning a value tuple rather than N delegate columns: the cost model is one field per column per item, and redundant parameter marshaling across N separate invocations on the same input is the signal. A single delegate returning `(double Weight, double Score)` from one input outperforms two separate columns once N exceeds two.
- Abstract members are the most expensive behavior tier: they require derived-case nested types, private intermediate constructors, and case-typed fields at declaration sites. Justified only when a case needs its own state that varies per instance, not per vocabulary item. Delegate rows beat abstract members for every per-item policy where the implementation can be a pure static function.
- `Map` over a vocabulary functions as a static value-row table when all branches return constants; it is the legitimate form when the result is a compile-time-known verdict per item. `Switch` is for runtime-computed continuations. Conflating them — using `Switch` for constant selection or `Map` for computed results — misuses both forms.
- Delegate rows reject the external dispatch anti-pattern: three or more consumers each writing a full-coverage `Switch` with identical or near-identical arms is the collapse signal; the arms expose behavior the vocabulary item already owns, and a delegate row centralizes it under analyzer-enforced column completeness.

[FROZEN_LOOKUP_TABLES]:
- `FrozenDictionary<TKey, TValue>` is the correct form when the dispatch key is a bounded vocabulary value, the results are static or deterministic data, and the table is populated once at startup. `FrozenDictionary.Create(IEqualityComparer<TKey>?, ReadOnlySpan<KeyValuePair<TKey, TValue>>)` constructs from a span without intermediate allocation when the entries are inline constants; collection expressions and `params ReadOnlySpan<T>` feed this overload without a heap allocation for the source array. `IEnumerable<T>.ToFrozenDictionary(keySelector)` is the LINQ projection path when the source is already materialized — this path calls `ToDictionary` internally and **throws `ArgumentException` on duplicate keys**.
- `FrozenDictionary` selects its internal implementation from a fixed strategy hierarchy at construction time: empty input returns a singleton `EmptyFrozenDictionary`; small collections use linear-scan variants with no hash computation — `SmallFrozenDictionary` for reference-type keys and value types with no `IComparable<TKey>`, `SmallValueTypeComparableFrozenDictionary` for small value types implementing `IComparable<TKey>`, `SmallValueTypeDefaultComparerFrozenDictionary` for small value types with the default equality comparer; dense integral value-type keys use `Int32FrozenDictionary` or `DenseIntegralFrozenDictionary`; string keys with ordinal comparers use `OrdinalStringFrozenDictionary_*` variants that analyze minimum-uniqueness substrings; the default fallback is `DefaultFrozenDictionary` using computed perfect-hash slots. Construction cost is dominated by this one-time strategy analysis. Smart-enum keys are reference types (`[SmartEnum]` targets `AttributeTargets.Class` only) and route through `SmallFrozenDictionary` for small tables (linear scan, no hash) — authored switch expressions over small smart-enum vocabularies are not faster.
- `FrozenSet<T>` exposes `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup)` with the same comparer-compatibility requirement as `FrozenDictionary`. `FrozenSet<string>.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)` produces a `lookup.Contains(ReadOnlySpan<char>)` surface for zero-allocation membership dispatch over protocol text. `FrozenSet` does not expose `GetValueRefOrNullRef` — membership is a boolean test, not a value projection; the single-probe ref path exists only on `FrozenDictionary<TKey, TValue>` directly with a `TKey` argument, not on any `AlternateLookup` surface.
- `GetAlternateLookup<TAlternate>()` enables key-type-polymorphic lookup without conversion: `FrozenDictionary<string, TValue>.GetAlternateLookup<ReadOnlySpan<char>>()` returns a lookup surface whose `TryGetValue(ReadOnlySpan<char>, out TValue)` probes the frozen table with zero string allocation when the comparer implements `IAlternateEqualityComparer<ReadOnlySpan<char>, string>`. `OrdinalIgnoreCase` is the canonical span-compatible string comparer. The `TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup)` form is safer at startup: on comparer mismatch it returns `false` and produces a default instance that silently returns null-ref on every probe — a `false` result must be treated as a hard startup failure, not a fallback path. `GetValueRefOrNullRef` is not exposed on `AlternateLookup`; the single-probe ref path for a span key requires converting to `TKey` first — use `AlternateLookup.TryGetValue` when value copy is acceptable and the allocation is preferable to a `ToString` allocation.
- `GetValueRefOrNullRef(TKey key)` returns `ref readonly TValue` into the dictionary's internal value storage, tested with `Unsafe.IsNullRef`. This is the single-probe path for read-heavy kernels — the reference points directly into the frozen array without a hash probe plus value copy. The reference is valid for the lifetime of the frozen dictionary instance and must not be used across a dictionary replacement boundary.
- `FrozenDictionary.Create(comparer, span)` silently retains the **last** entry for duplicate keys — the construction loop executes `dictionary[key] = value` for each span entry, so later entries overwrite earlier ones. `ToFrozenDictionary` lowers to `ToDictionary` and throws `ArgumentException` on the first duplicate. The correct enforcement for `Create` is a structurally non-duplicating source: `SmartEnum.Items` is deduplicated by definition; inline constant spans are audited at code review.
- Frozen tables are rejected when the key set is open (new entries arrive at runtime), when the results are computed from context (that context belongs in the state-threaded arm), or when the same table re-derives a correspondence a vocabulary's delegate rows already own. A frozen dictionary keyed by smart-enum items restates what delegate rows already capture with a duplicate-entry maintenance burden and a missed-new-item silent failure mode — delegate rows are constructor-enforced at compile time; frozen table entries are runtime-populated.
- The correct host for a derived frozen index is a `Lazy<FrozenDictionary<TKey, TValue>>` field initialized from `Items.ToFrozenDictionary(...)`: lazy because item materialization must precede projection, `ExecutionAndPublication` for thread safety, and static because it is process-global policy data. Eager static initializers on the vocabulary type race item initialization.

```csharp
// FrozenDictionary with span-lookup path; TryGetAlternateLookup false is a hard startup failure.
// OrdinalIgnoreCase implements IAlternateEqualityComparer<ReadOnlySpan<char>, string>.
// Create(comparer, ReadOnlySpan<KVP>) avoids intermediate heap allocation for inline entries.
// GetValueRefOrNullRef is NOT on AlternateLookup — ref path requires TKey, not TAlternate.
static readonly FrozenDictionary<string, Policy> _table =
    FrozenDictionary.Create(
        StringComparer.OrdinalIgnoreCase,
        (ReadOnlySpan<KeyValuePair<string, Policy>>)[
            new("strict", Policy.Strict),
            new("lenient", Policy.Lenient),
        ]);

static readonly FrozenDictionary<string, Policy>.AlternateLookup<ReadOnlySpan<char>> _spanLookup =
    _table.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup)
        ? lookup
        : throw new InvalidOperationException("comparer incompatible with span lookup");

static bool TryResolve(ReadOnlySpan<char> key, out Policy policy) =>
    _spanLookup.TryGetValue(key, out policy);
```

[EXTENSION_BLOCKS]:
- Extension blocks (`extension(ReceiverType) { ... }`) add instance members, static members, and non-conversion operators to a receiver type without subclassing. The compiler lowers extension block members to static methods and emits `ExtensionMarkerAttribute(string name)` on each member to associate it with its declaring marker type.
- Extension blocks over foreign BCL or library receivers replace static extension-method files: the block groups all extensions on one receiver under one syntactic scope, and instance-style property syntax (`value.Magnitude`) is available where methods (`GetMagnitude(value)`) were the only option before. `[OverloadResolutionPriorityAttribute]` accepts any `int` value (default 0; higher wins) and prunes lower-priority applicable candidates before betterness rules apply — a `[OverloadResolutionPriority(1)] M(object)` wins over an unattributed `M(string)` for the call `M("x")` because priority pruning precedes betterness evaluation; specificity in type conversion does not save a lower-priority candidate. `[OverloadResolutionPriority]` cannot be applied to an overriding member.
- Two extension blocks in the same enclosing static class do not collide when their receiver types are the same — multiple extension blocks on the same receiver type within one static class merge into one declaration space and compile cleanly; only duplicate member signatures within that merged space are compile errors. The `ERR_ExtensionBlockCollision` ("conflicting content-based type names in metadata") is triggered by a distinct structural condition separate from having two extension blocks on the same receiver type; members from two blocks on the same receiver in one static class are accessible together with no separation required.
- Extension blocks own receiver-scoped projection: a module that knows how to convert a foreign value to its own domain type expresses that knowledge as an extension block on the foreign type rather than a static helper class. The helper class breaks one-hop resolution by requiring a name disambiguation step.
- Extension blocks cannot add state (fields, backing store) to the receiver; they are pure projection and behavioral enrichment. Any state the projection needs enters through a frozen table keyed by the receiver's identity, through a thread-local slot, or as a parameter.
- Extension block members cannot have the modifiers `abstract`, `virtual`, `override`, `new`, `sealed`, `partial`, or `protected`. Extension blocks cannot participate in inheritance-based dispatch: they cannot define virtual methods for subtypes to override or abstract contracts for derived types to fulfill. Any architecture that attempts to use extension blocks as a mixin or trait system must fall back to interface-based or abstract-class-based dispatch.
- The receiver in an extension block can specify `ref`, `ref readonly`, or `in` modifiers when the receiver type is a struct or is known to be a value type. `extension(ref ValueType v)` enables mutating extension methods on value types without copying — the correct form for high-throughput span-shaped dispatch that needs to mutate accumulator state. Receiver parameter modifier constraints are enforced independently by Roslyn on four rules: a `ref` receiver requires a value type or a generic type constrained to `struct`; a `ref readonly` or `in` receiver requires a concrete (non-generic) value type — `where T : struct` is rejected, the concrete type must be named; an instance operator on a struct receiver requires the receiver parameter to be `ref`; a static class receiver cannot contain user-defined operators.
- Type parameters for extension blocks must satisfy inferrability: for every non-method extension member (properties, operators), all of the extension block's type parameters must appear in the combined parameter set of the extension plus the member. A generic type parameter that appears only in the body but not in any parameter is a compile error — this constraint is tighter than for methods, where inference from the receiver alone can satisfy type parameters.
- Expression trees reject two extension block member categories unconditionally: property access emits `An expression tree may not contain an extension property access`, and `&&`/`||` operators using extension-defined user-defined operators emit `An expression tree may not contain '&&' or '||' operators that use extension user defined operators`. Extension methods (non-property, non-`&&`/`||`) are still legal inside expression trees as static call rewriting. Any dispatch architecture where extension block surfaces feed LINQ `Expression<Func<T, bool>>` predicates, EF query translators, or library-internal expression-tree builders must fall back to classic static extension methods for those specific surfaces.
- `params IEnumerable<T>`, `params ReadOnlySpan<T>`, `params IReadOnlyList<T>`, and `params ImmutableArray<T>` are all legal collection forms for arity collapse; `ParamCollectionAttribute` is the compiler's lowering attribute distinguishing the new forms from the legacy `params T[]`. A `params ReadOnlySpan<T>` overload at `OverloadResolutionPriority(1)` and `params IEnumerable<T>` at priority 0 gives callers the span path for inline construction and the enumerable path for already-allocated sequences without ambiguity.

```csharp
// Two extension blocks on the same receiver in the same static class compile cleanly.
// Members from both blocks are available on the receiver — no separation required.
static class ForeignValueExtensions
{
    extension(ForeignValue v)
    {
        public DomainType ToDomain() => new(v.Id, v.Name);
    }

    extension(ForeignValue v)
    {
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

[INTERCEPTOR_CONSTRAINTS_ON_DISPATCH]:
- Source-generator interceptors cannot redirect a dispatch call site when the interceptor method is declared inside a generic type: `Method '{0}' cannot be used as an interceptor because its containing type has type parameters.` This eliminates interceptors as a mechanism for substituting generated `Switch`/`Map` call sites on generic request types. The interceptor must live in a non-generic enclosing type — a hard architectural constraint for any generator that attempts to replace `Switch<TState, TResult>` calls on generic union types.
- Interceptors must be ordinary member methods: `An interceptor method must be an ordinary member method.` Extension block members lower to static methods with `ExtensionMarkerAttribute`, disqualifying them; classic (non-block) static extension methods are also disqualified. Only non-extension, non-operator, non-local, non-lambda methods in non-generic types can intercept. Interceptors are therefore useful for definition-time weaving at concrete, non-generic, module-scoped call sites only.
- The `InterceptsLocationAttribute(string, int, int)` form (file path, line, column) is explicitly unsupported: Roslyn emits `'InterceptsLocationAttribute(string, int, int)' is not supported. Move to 'InterceptableLocation'-based generation of these attributes instead.` The `GetInterceptableLocation` API (`InterceptableLocation1`, checksum-based) is the only supported form. Source generators that target `Switch` call sites for logging, telemetry wrapping, or policy injection must use `SemanticModel.GetInterceptableLocation(InvocationExpressionSyntax)` to obtain the location.

[STRUCTURAL_PATTERN_DISPATCH]:
- Structural pattern dispatch — switch expressions whose arms match on list/slice patterns, positional patterns, property + relational combinations, and constant content — is the correct form when the input's shape is the discriminant and a closed case hierarchy does not exist or would not express the semantics more clearly. The canonical domain is span-shaped protocol decoding, arity detection on incoming collections, and composite-condition branching over value-typed records.
- List and slice patterns over `ReadOnlySpan<T>` and arrays are exhaustively closed when combined with a `_` wildcard arm; omitting the total arm produces CS8509 (warning) and a runtime `SwitchExpressionException` — not a compile-time error. Structural patterns are open to extension (any caller may add a new case) while generated `Switch` over a `[Union]` is closed. Structural dispatch is chosen precisely when the input vocabulary is open; generated `Switch` is chosen when it is closed.
- Property patterns over record payloads compose with generated dispatch: the outer switch is generated (exhaustive, closed) and each arm uses a property or positional pattern to destructure the case payload — two layers of exhaustiveness with zero imperative branching. The inner pattern targets the case's own type, which the compiler knows statically inside the arm.
- Type-pattern `switch (value)` over non-union hierarchies is the structurally open form: it admits any subtype of the switch subject and never proves totality. Where totality matters, the generated surface is the correct owner. A `_` arm that proves totality by comment rather than by type closure is the diagnostic to eliminate.
- Guard conditions (`when` clauses) are legitimate in structural pattern dispatch for constraint that cannot be expressed as a pattern: range guards, derived-equality conditions, and invariant probes. A guard that merely restates a property pattern the language can already express directly is the form to collapse.
- Structural patterns and generated `Switch` compose correctly at different hierarchy levels: generated `Switch` owns closed case dispatch at the union boundary; structural patterns own payload destructuring inside the `Switch` arm. The arm body receives a statically-typed case payload and the compiler knows the exact type — no additional `is` check is needed inside the arm. Positional patterns over primary constructor record cases bind directly in the arm parameter without a nested pattern match expression.

[SELECTION_LAW_AND_PRESSURE_BOUNDARIES]:
- State-threaded `Switch` is selected over structural pattern dispatch when the input type is a closed `[Union]` or `[SmartEnum]` vocabulary: generated exhaustiveness is mechanically verified and adding a case arity-breaks every dispatch site, while structural patterns over a closed hierarchy trade compile-time totality for a silent `_` arm without adding expressiveness.
- Delegate rows are selected over state-threaded `Switch` when three or more consumers write the same or structurally identical switch arms over the same vocabulary — three occurrences is the collapse trigger, not a preference. The delegate row centralizes behavior the vocabulary item already owns and makes completeness a constructor invariant rather than a distributed call-site discipline.
- Frozen tables are selected over delegate rows when the result is a pure static data mapping and when multiple unrelated vocabulary types must be cross-referenced: a frozen table is the correct join surface when neither vocabulary owns the other's keys. When the result derives from one vocabulary's own key, a delegate row owns it more directly.
- Deliberate partial dispatch is expressed via `[UnionSwitchMapOverload(StopAt = new[] { typeof(IntermediateCase) })]`: the generated overload treats the stop type's entire derived subtree as one arm. The attribute is `AllowMultiple = true` — each instance generates one additional overload; multiple stop types in one attribute instance produce one overload that simultaneously coarsens multiple subtrees. The generator collects all ordered concrete type members from the flat type-walk, removes derived types recursively for each stop type, and strips any remaining abstract members not in `StopAtTypeNames`; the stop type itself appears as a leaf arm. A stop type that is already a leaf has no practical effect — the overload is identical to the exhaustive form. Adding a new leaf under a stop type will not break the call site using the stop-at overload — a gap that total dispatch would catch at compile time. The stop-at overload is warranted only when a specific caller legitimately treats a sub-hierarchy as an opaque unit. `SwitchPartially`/`MapPartially` (generated when `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`) require a `@default` case as the first named argument — they express "I handle some cases and delegate the rest to `@default`", while stop-at expresses "I handle all cases but treat this subtree as one atom." A stop-at overload with the stop type as the only arm is structurally the same as a partial overload with one case; the stop-at form carries the semantic claim that the subtree is intentionally opaque.
- Total `Switch` with a `@default` arm that unconditionally throws is distinguishable from generated total `Switch` in a critical way: the generated exhaustive `Switch` has no `@default` parameter at all — it is a compile-time guarantee. TTRESG046 enforces named arguments on `Switch`/`Map` calls, making a missing case argument a compile error rather than a silent fallthrough. When the union grows, generated total `Switch` breaks at all call sites; authored `_ => throw` compiles but silently misses the new case at runtime. Prefer partial form when incompleteness is permanent, total with throw only as a defensive temporary.
- The state-threading closure-free discipline and the frozen table form create an integration pressure in algorithms that need both vocabulary-owned behavior and cross-vocabulary join data: delegate rows own behavior that is per-item and vocabulary-scoped; frozen tables own cross-vocabulary correspondences. A `Switch` arm that probes a frozen table to complete its logic is a valid composition — the `Switch` arm is the dispatch mechanism and the frozen table lookup is a data retrieval step, not a second dispatch mechanism.
- Extension blocks and generated `Switch` interact at foreign union boundaries: the extension block provides the instance-method syntax (`foreignUnion.ToMyDomainType()`); the `Switch` call inside the extension method is generated exhaustive dispatch over the foreign union. The extension block owns the module-to-domain translation surface; the `Switch` arms are the per-case projections; no helper method exists between them.
- The `|` operator for catch composition is defined generically on `K<F, A>` in `FallibleExtensions` as `extension<E, F, A>(K<F, A> _) where F : Fallible<E, F> { static K<F, A> operator |(K<F, A> lhs, CatchM<E, F, A> rhs) }` — it applies to any `K<F, A>` whose carrier `F` implements `Fallible<E, F>`. No `.As()` pre-cast to a concrete type is required; a generic `K<M, A>` where `M : Fallible<Error, M>` composes directly with `|`.

[COLLECTION_DISPATCH_TRAVERSE_FORMS]:
- `TraverseM<M, B>(Func<A, K<M, B>> f)` on `Iterable<A>`, `Arr<A>`, `Lst<A>`, and `HashSet<A>` is an instance method with constraint `where M : Monad<M>`; it threads effects through `Bind` and returns `K<M, Iterable<B>>`. The `.As()` projection to a concrete carrier type is required at the call site. The monad `M` determines short-circuit behavior — `IO` and `Eff` short-circuit on first failure; `Validation` with `Applicative` semantics accumulates without short-circuiting via the `Traverse` (not `TraverseM`) form. For dispatch over a bounded collection where the first failure should abort, `TraverseM` with `Eff` is the correct form; for dispatch over an independent collection where all errors should be reported, `Traverse` with `Validation` is correct.
- `Iterable<A>.TraverseIO<F, B>(Func<A, K<F, B>> f, K<Iterable, A> kta)` is an instance method with constraint `where F : MonadIO<F>` and an explicit traversable argument — available when the `Iterable` instance is not the receiver. This is the correct form for effect-sequenced dispatch over a pre-computed sequence when the caller does not own the sequence as a receiver.

```csharp
// TraverseM for sequential abort-on-first-failure dispatch over a collection.
// Traverse with Validation for accumulating all errors from independent items.
// .As() is required: TraverseM returns K<M, Iterable<B>>, not Eff<Iterable<Result>>.
static Eff<Iterable<Result>> ProcessAll(Iterable<Item> items) =>
    items.TraverseM<Eff, Result>(item =>
        item.Switch(
            active: static (_, a) => validate(a),
            inactive: static (_, _) => SuccessEff(Result.Skipped),
            state: unit))
    .As();
```

[COMPOSITION_TIME_ASPECTS]:
- `Schedule`-driven retry attaches to `IO<A>` or `Eff<RT, A>` as a transformer: `io.Retry(Schedule.exponential(100 * ms) | Schedule.recurs(5))` retries on any failure up to five times with exponential backoff. `recurs` is a `ScheduleTransformer`, not a `Schedule` — it applies `s.Take(n)` to the schedule it transforms. Both `|` and `&` have `(Schedule, ScheduleTransformer)` and `(ScheduleTransformer, Schedule)` overloads that call `transformer.Apply(schedule)` — `recurs` is direction-agnostic.
- `ScheduleTransformer.op_Addition(f, g)` composes two transformers in sequence: `f` runs first on the schedule, `g` runs on the result — equivalent to `s => g.Apply(f.Apply(s))`. This is the transformer pipeline form, distinct from `|` and `&` which compose a `ScheduleTransformer` with a `Schedule` by calling `transformer.Apply(schedule)`: `static readonly ScheduleTransformer RetryPolicy = Schedule.NoDelayOnFirst + Schedule.maxDelay(2000 * ms) + Schedule.decorrelate()` chains transformers into a single reusable object applied to any base schedule via `|`. Separating transformer composition from schedule instantiation means policy objects are defined once and composed with per-call-site schedules at point of use.
- `Schedule.Transform(Func<Schedule, Schedule>)` is the `ScheduleTransformer` constructor that lifts any schedule-to-schedule function into a transformer. Custom transformer logic — delay normalization, budget-aware capping, environment-conditioned delay — enters through `Schedule.Transform(s => ...)` and composes via `+` with library-provided transformers. The custom transformer receives the full `Schedule` stream as input and must return a valid `Schedule`; it is a whole-stream morphism, not a per-step callback.
- `Schedule.Union` (the `|` method) produces `SchUnion`, which zips two duration streams and yields the min at each step for as long as either stream has elements — union extends to the longer schedule's length. `Schedule.Intersect` (the `&` method) produces `SchIntersect`, which zips and yields the max at each step for as long as both streams have elements — intersect truncates to the shorter schedule's length. Practical consequences: `exponential(100*ms) | spaced(2000*ms)` recurs indefinitely with spaced acting as a delay ceiling (union min caps the exponential); `exponential(100*ms) & spaced(300*ms)` recurs indefinitely with spaced acting as a delay floor (intersect max enforces a minimum delay).
- `Schedule.NoDelayOnFirst` is a `ScheduleTransformer` that rewrites the schedule's tail: `s => s.Tail.Prepend(Duration.Zero)`. Applied to a retry schedule, the first retry fires immediately with no delay, and the schedule's original delays begin from the second retry onward. Significant for connection-reset and transient-fault scenarios where an immediate first retry has high success probability: `io.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(4) | Schedule.exponential(100*ms))`.
- `Schedule.maxDelay(Duration max)` caps individual delay duration: no single wait exceeds `max`. `Schedule.maxCumulativeDelay(Duration max)` caps total accumulated delay: the schedule terminates when the sum of all previous delays would exceed `max`. Composing: `Schedule.exponential(50*ms) | Schedule.maxDelay(2000*ms) | Schedule.maxCumulativeDelay(30_000*ms) | Schedule.recurs(20)` — whichever bound triggers first wins. `Schedule.decorrelate()` applies proportional jitter per step, reducing retry thundering-herd while preserving the backoff shape. `Schedule.RepeatForever` converts any finite schedule into an infinite repeating one; composition order matters: `recurs(3) | RepeatForever` repeats the 3-retry exponential pattern indefinitely, whereas `RepeatForever | recurs(3)` produces an infinite schedule that is then capped at 3 repetitions by `recurs`.
- Named catch combinators compose with `|`: `CatchM<E, M, A>` is a `readonly record struct` holding a `(Func<E, bool> Match, Func<E, K<M, A>> Action)` pair. The `|` operator on `K<F, A>` is defined generically in `FallibleExtensions` via an extension block: `static K<F, A> operator |(K<F, A> lhs, CatchM<E, F, A> rhs) => lhs.Catch(rhs)`. The chaining `computation | @catch(errorA, ...) | @catch(errorB, ...)` applies left-associatively — the first `|` wraps the computation with the first handler; the second wraps the result with the second. The outer handler sees failures the inner handler did not match and re-threw; first match wins because inner handlers execute before outer handlers — the composition is handler wrapping, not handler list scanning. `@catch(Error, K<M,A>)` matches by exact error value; `@catch(int errorCode, ...)` matches by `error.Code` equality and can disambiguate errors of different semantic types that share a hierarchy ancestor, whereas `@catchOf<E>` cannot distinguish two subtypes differing only by code — the two forms are orthogonal. `@catchOf<E, M, A>` matches errors of subtype `E` using `e is E || e.IsType<E>()` — the `IsType<E>()` test handles `Error` aggregates where one nested error is of type `E`. For aggregate errors, `@catchOf<E>` (non-fold) calls `es.Filter<E>().ForAllM(f)` — `ForAllM` threads the handler monadically via `Bind`, short-circuiting on first failure; `catchOfFold<E>` replaces `ForAllM` with `FoldM`, which uses `MonoidK<M>` to accumulate handlers across all matching errors independently — use fold when all errors in an aggregate of the matching subtype must be reported; use non-fold when the first success or failure is conclusive. `@catch<M, A>(Func<Error, K<M, A>> Fail)` with no predicate is a total catch handler firing for every error; positioned last, it ensures no error escapes.
- The `|` operator for `IO<A>` is overloaded for `CatchM<Error, M, A>` (predicate-guarded catch), `K<IO, A>` (unconditional alternative), `Pure<A>` (constant recovery), `Fail<Error>` (re-fail with different error), and bare `Error` (re-fail). The `K<IO, A> | K<IO, A>` form is the unconditional alternative pattern: if the left computation fails for any reason, the right runs without error inspection. This is structurally distinct from catch handler composition and must not be confused with it: `|` as alternative is unconditional fallback; `|` as catch composition is predicate-guarded recovery.
- `RetryWhile(schedule, predicate)` and `RetryUntil(schedule, predicate)` add error-predicate gates to retry schedules. Composition-time policy stacks deterministically: `retry` transforms the computation before `catch` sees failures; a `catch` inside a `retry` recovers per attempt; a `catch` outside a `retry` sees only the final failure after all retries are exhausted.
- `localEff<TOuterRT, TInnerRT, A>(Func<TOuterRT, TInnerRT> f, Eff<TInnerRT, A> ma)` narrows the runtime environment for a specific computation: it maps the outer `RT` to an inner `RT` and runs `ma` in that narrowed context. This is the correct form when different dispatch arms need different runtime capabilities — the outer computation carries a broad `RT`; each arm that requires a narrower capability receives it via `localEff`. The arm is a pure `Eff<TInnerRT, A>` with no awareness of the outer `RT`. `Eff<A>.WithRuntime<RT>()` widens a runtime-agnostic `Eff<A>` into `Eff<RT, A>` so it can be sequenced with runtime-aware computations. `WithRuntime` serves the lift site (arm produces `Eff<A>`, outer sequence requires `Eff<RT, A>`); `localEff` serves the narrowing site (outer `RT` is too broad for the arm's effects) — these two are the complete RT-scoping pair at dispatch boundaries.
- `IO<A>.Local()` creates an isolated cancellation scope — not an `RT` scope. `localCancel<A>(Eff<A> ma)` is the `Eff`-level equivalent. Both create a fresh `CancellationTokenSource` linked to the parent's token, run `ma` within that child scope, and cancel the child scope on `ma`'s completion. The scope isolation means cancelling the child does not cancel the parent; parent cancellation propagates into the child. This is the correct form when a dispatch arm fires an operation that should be independently cancellable without tearing down the outer computation.
- `bracketIO(computation)` is the semantic preference over `bracketIO(acq, use, fin)`: the single-argument form takes a `K<M, A>` with `M` constrained to `MonadUnliftIO` and installs a resource-tracking scope. `IO<A>.Bracket(Use, Fin)` is the receiver-scoped two-argument form where `this IO<A>` is the acquisition. `IO<A>.Bracket(Use, Catch, Fin)` adds a failure-specific `Catch` projection that runs between `Use` failure and `Fin` cleanup — `Catch` is a failure-specific hook, not a release: `Fin` still runs unconditionally. `IO<A>.BracketFail` runs only on error and keeps the resource live on success. Nesting `Bracket` within `Retry`: resources acquired inside the bracketed computation are released on each retry's failure before the next attempt because the bracket scope is per-attempt; resources acquired outside persist across retries. When the retried computation holds a borrowed connection, the bracket must scope to the per-attempt acquire/release, and the retry wraps the outer bracket.

```csharp
// Schedule algebra: Union extends to longer, Intersect truncates to shorter.
// recurs(3) | exponential(100*ms) — three retries with exponential delays.
// exponential(100*ms) | spaced(2000*ms) — indefinite; spaced caps exponential delay via union min.
// exponential(100*ms) & spaced(300*ms) — indefinite; spaced floors delay via intersect max.
// NoDelayOnFirst | recurs(5) | exponential(50*ms) — five retries, first immediate, then exponential.
// maxDelay(2000*ms) | maxCumulativeDelay(30_000*ms) | recurs(20) — whichever bound triggers first.
static IO<TResult> WithPolicy<TResult>(IO<TResult> op) =>
    op.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(4) | Schedule.exponential(100 * ms)
             | Schedule.maxDelay(2_000 * ms) | Schedule.decorrelate())
    | @catch<IO, TResult>(Errors.Cancelled.Code, _ => IO.fail<TResult>(Errors.Cancelled))
    | @catch<IO, TResult>(e => e.IsExpected, e => IO.fail<TResult>(e));
```

```csharp
// CatchM chaining: left-associative wrapping. Inner handlers see errors first.
// @catch(int code) narrows before @catchOf<E> sees the aggregate.
// catchOfFold accumulates across all matching errors in an aggregate independently.
static IO<T> Resilient<T>(IO<T> op) =>
    op.Retry(Schedule.NoDelayOnFirst | Schedule.recurs(3) | Schedule.exponential(100 * ms))
    | @catch<IO, T>(Errors.Cancelled.Code, _ => IO.fail<T>(Errors.Cancelled))
    | catchOfFold<TransientError, IO, T>(e => IO.fail<T>(Error.New(e.Code, e.Message)))
    | @catch<IO, T>(e => e.IsExpected, IO.fail<T>);
```

```csharp
// ScheduleTransformer composition: + chains transformers, | applies a transformer to a schedule.
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
// localCancel creates an isolated cancellation scope independent of RT.
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
