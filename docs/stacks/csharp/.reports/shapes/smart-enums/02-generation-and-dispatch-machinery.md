# [SMART_ENUM_GENERATION_AND_DISPATCH_MACHINERY]

[INCREMENTAL_MODEL_EQUALITY]:
- The whole generator is a tree of value-equatable record-like state structs reached from `ForAttributeWithMetadataName`, never from a syntax-node scan: the attribute index is the entry, and every node downstream is an immutable value carrying only extracted primitives. The per-type state implements `IEquatable<T>` over fully-qualified type name strings, key-member state, settings, item set, assignable fields, containing-type chain, generic parameters, and delegate methods — and crucially holds no `ISymbol`, no `SyntaxNode`, and no `Location`. Roslyn's incremental cache compares these values to decide whether downstream emission reruns; because no symbol is retained, an edit elsewhere in the compilation that changes binding but not the extracted strings produces an equal state and short-circuits before any string building.
- Item equality is the sharpest expression of this discipline: an item compares equal by field name alone, dropping the field symbol, its declared-type symbol, and its source location. Reordering whitespace, moving the field within the file, or recompiling an unrelated dependency leaves the item-set value identical, so the cache holds. The corollary is the failure mode of careless generator authorship inverted into a guarantee — anything the equality does not read cannot trigger regeneration, and anything it reads must be a comparable primitive, which is why the model is strings and never symbols.
- A vocabulary spread across several partial declarations is collected into one array, deduplicated by a comparer that keys on fully-qualified type name only, then the array is wrapped in a set comparer whose `Equals` is an order-independent membership test (each element of one side must exist in the other) and whose hash is the element count alone. The collection of all vocabularies in the compilation is therefore cache-stable under file-order and declaration-order permutation: adding a vocabulary in a new file does not invalidate the emission of every other vocabulary, and the multi-file partial case regenerates identically regardless of which partial the compiler visits first.
- Satellite capabilities — parsing, span parsing, comparison and equality operators, formatting — fan out as their own incremental value providers, each deduplicated by a type-name-only comparer. Each satellite recomputes only when its narrow projected state changes; editing a delegate-method body invalidates the core emission but not the parsable or comparable emission, because those projections never read the delegate set. The pipeline is partitioned so that the unit of cache invalidation is the capability, not the vocabulary.
- Serializer adapters take a structurally different route: they derive from the metadata-references provider (the set of referenced assemblies), are deduplicated by the same order-independent set comparer, and are emitted through the implementation-source-output channel rather than the source-output channel. Implementation output does not participate in the semantic model the IDE consumes for IntelliSense, so serializer converters never perturb the editor's view of the type and only materialize for the build. Diagnostics and thrown generator exceptions ride two further independent source-output channels, so a reported analyzer finding never shares a cache node with the code it describes.

[WRITE_ONCE_INDEX_PROTOCOL]:
- Dispatch is an integer `switch`, and the integer is not stored in a constant — it is assigned at lookup-materialization time. Each item carries a write-once reference cell initialized to a negative sentinel; the materializer walks items in declaration order and assigns each cell its position via an interlocked compare-exchange against the sentinel, throwing if a cell was already set. The index is therefore a runtime fact established exactly once, on first touch of the lookup or item list, and equal to declaration order by construction.

```csharp
public sealed class WriteOnceCell
{
   private int _value = -1;
   public int Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value; }
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int ReadVolatile() => Volatile.Read(in _value);
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Set(int value)
   {
      if (Interlocked.CompareExchange(ref _value, value, -1) != -1)
         throw new InvalidOperationException("Value was already set.");
   }
}
```

- The hot-path index read is deliberately non-volatile and inlined: dispatch reads the plain field, and a non-negative result returns immediately with no fence. A negative result means the cell has not yet been published to this thread, so the slow path forces materialization by touching the item list, then re-reads through a volatile load to acquire the publication. The fast path costs one field read and a branch; the fence is paid only on the first dispatch that races materialization on a given thread.
- The cell is a sealed heap object, not a value box, so it carries one allocation per item for the life of the type — the cost is amortized to once-ever and bought back by collapsing dispatch to a jump table that needs no dictionary probe, no virtual call, and no delegate array. The negative sentinel doubles as the "not yet materialized" signal, which is why the slow path can detect staleness without a separate flag: absence of an index and absence of materialization are the same state.
- Because the index is assigned during the same single-pass materialization that builds the lookup and validates non-null items and keys, the integer correspondence and the duplicate-key fail-fast are established atomically. A vocabulary whose lookup throws on a duplicate key never assigns indices, so dispatch over a poisoned vocabulary fails on the lookup touch inside `GetItemIndex`, not later with a stale jump.

[TOTAL_DISPATCH_EMISSION]:
- Total `Switch`/`Map` emit a real `switch (GetItemIndex())` with one `case` per item in declaration order and a `default` arm that throws an out-of-range exception interpolating the instance itself. The default is structurally unreachable for a correctly materialized vocabulary — every public instance has an assigned index in range — so the throw is the witness that materialization completed, not a runtime branch the caller can hit. The interpolation of `this` in the message routes through the key-derived `ToString`, so a corrupted-dispatch diagnostic speaks the wire vocabulary.
- Adding an item appends a `case` and shifts no existing index, but it adds a parameter to every total overload, so each full-coverage call site breaks at compile time until the new arm is supplied. This is the closed-vocabulary evolution guarantee made mechanical: totality is enforced not by an analyzer pass over call sites but by method arity, so the type system itself refuses to compile a stale exhaustive dispatch.
- Func-shaped total dispatch is emitted with the ref-struct anti-constraint on its result and state type parameters under a target gate (`where TResult : allows ref struct`, `where TState : allows ref struct`), conditionally compiled inside the generated source. A span-carrying state or a ref-struct result threads through dispatch without boxing only on targets that compile the constraint; the same source declaration yields a dispatch surface whose generic permissiveness is a function of the compiled target.
- The lookup record itself is target-conditional: above the frozen-collection threshold it returns a frozen dictionary, and above the alternate-lookup threshold a string-keyed vocabulary additionally carries a span alternate view materialized from the frozen dictionary. The frozen dictionary is built with the exact comparer used for the mutable build dictionary — the key-member equality-comparer accessor when present, ordinal-ignore-case for string keys otherwise — so the build-time and freeze-time equality laws cannot diverge. Below the threshold the same record carries only the plain dictionary, and the span overloads do not compile.

[PARTIAL_DISPATCH_ASYMMETRY]:
- Partial action and func dispatch take nullable per-item callbacks defaulting to null and emit a per-case null guard: a case whose callback is null breaks out of the switch and falls through to the trailing default invocation. The omitted-arm signal and the explicitly-null-arm signal are therefore identical for `SwitchPartially` — a caller cannot pass null to mean "run nothing for this case and do not fall to default," because null is exactly the fall-to-default signal. Partiality over actions and funcs is a presence test on a delegate, and a delegate has no value distinct from absence.

```csharp
switch (GetItemIndex())
{
   case 0:
      if (alpha is null) break;
      return alpha(state);
   case 1:
      if (beta is null) break;
      return beta(state);
   default:
      throw new ArgumentOutOfRangeException($"Unknown item '{this}'.");
}
return @default(state, this);
```

- Partial `Map` resolves the asymmetry that partial `Switch` cannot: its per-item parameters are not raw values but a one-field ref-struct wrapper carrying an `IsSet` flag, with an implicit conversion from the result type and a default that is unset. The emitted body breaks on `!arg.IsSet` and returns `arg.Value` otherwise, so a mapped value of null or zero is distinguishable from an omitted arm — `IsSet` recovers the presence bit that a bare nullable callback erased. Partial mapping can therefore carry every legal result value including the type's default, where partial switching cannot carry a deliberately-null callback.
- The func-shaped partial overloads require a non-null trailing default because the fall-through is invoked unconditionally after the switch; the action-shaped partial overload tolerates a null default because its fall-through is a null-conditional invocation. The default's nullability is thus a direct consequence of whether the fall-through must produce a value, and the two partial families differ in their default contract for that reason alone.

[DERIVED_INDEX_MATERIALIZATION_ORDER]:
- A secondary index keyed on an item column must project from the item list, never eager-initialize from a static field that races the item cells, and never scan per call. Projecting from the item accessor forces the same single-pass materialization that assigns the dispatch indices, so the derived index observes a fully-built, index-assigned, duplicate-checked vocabulary and inherits its fail-fast. A lazy frozen projection materialized once is the canonical form — it mirrors the generator's own internal lookup shape and cannot observe a half-initialized item graph.

```csharp
private static readonly Lazy<FrozenDictionary<int, Tier>> _byRank =
   new(static () => Items.ToFrozenDictionary(static t => t.Rank),
       LazyThreadSafetyMode.ExecutionAndPublication);
```

- The derived index and the dispatch jump table share one materialization event, so the order of three operations is fixed and load-bearing: the item list is built, each cell's index is compare-exchanged from its sentinel, the lookup is frozen — and only then can a projection read a fully-formed vocabulary. A derived index built from an eager static initializer rather than from the item accessor can run before this event and observe items whose index cells still hold the sentinel and whose constructor arguments still hold sentinel cross-references; projecting from the item accessor instead inserts a happens-before edge through the lazy item list, so the projection cannot begin until the single pass that assigned every index has published. The materialization order is the synchronization primitive, and reading through `Items` is how a consumer subscribes to it.

[GENERATION_OBSERVABILITY]:
- The generated index correspondence is observable for churn auditing through an opt-in counter that watermarks every emitted file with a monotonically increasing header. Because the incremental cache should regenerate a file only when its value-equatable state changed, a counter that climbs on edits to unrelated declarations is direct evidence that a model is over-reading — that some state node retained a symbol or location and broke the string-only equality contract. The counter instruments the cache contract itself, not the output.
- When a matched declaration produces no code, the generator emits a warning through a file-logger channel keyed on compiler-visible build properties; without a configured log path it self-logs only its own errors, so the evidence for a silent non-generation exists only after the logger is switched on. The log answers why a type did not generate with the rejected-state reason rather than leaving attribute inspection as the only recourse, and it is the companion to the counter: the counter proves the cache is churning, the log proves why a type fell out of the pipeline.
