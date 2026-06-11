# Dispatch Index Protocol and Partial-Arm Asymmetry

[WRITE_ONCE_INDEX_PROTOCOL]:
- The dispatch integer is not a stored constant — it is assigned at lookup-materialization time. Each item carries a write-once reference cell initialized to a negative sentinel; the materializer walks items in declaration order and compare-exchanges each cell from the sentinel to its position, throwing on a second assignment. The index is a runtime fact established exactly once, on first touch of the lookup or item list, equal to declaration order by construction.

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

- The hot-path read is deliberately non-volatile and inlined: dispatch reads the plain field and a non-negative result returns with no fence. A negative result means the cell has not yet been published to this thread, so the slow path forces materialization by touching the item list, then re-reads through a volatile load to acquire the publication — one field read and a branch on the fast path, a fence paid only by the first dispatch that races materialization on a given thread. Dispatch issued before any admission therefore self-heals: it pays the materialization on its own thread rather than failing.
- The negative sentinel doubles as the not-yet-materialized signal, so staleness detection needs no separate flag — absence of an index and absence of materialization are the same state. The cell is a sealed heap object, one allocation per item for the life of the type, bought back by collapsing dispatch to a jump table with no dictionary probe, no virtual call, and no delegate array.
- Index assignment, lookup construction, and the duplicate-key fail-fast are one atomic single pass: a vocabulary whose materialization throws never assigns indices, so dispatch over a poisoned vocabulary fails at the index-read's materialization touch, never with a stale jump into a half-built table. The frozen lookup is built from the mutable build dictionary with the same comparer instance, so build-time and freeze-time equality laws cannot diverge.

[TOTAL_DISPATCH_EMISSION]:
- Total dispatch emits a real `switch` over the index read with one case per item in declaration order and a default arm throwing an out-of-range exception that interpolates the instance itself — routed through the key-derived `ToString`, so a corrupted-dispatch diagnostic speaks the wire vocabulary. The default arm is structurally unreachable for a correctly materialized vocabulary; its existence is the witness that materialization completed, not a branch callers can reach.
- Totality is enforced by method arity, not by an analyzer pass over call sites: adding an item appends a case, shifts no existing index, and adds a parameter to every total overload — the type system itself refuses to compile a stale exhaustive dispatch, which is the closed-vocabulary evolution guarantee made mechanical.

[PARTIAL_ARM_ASYMMETRY]:
- Partial action and func dispatch take nullable per-item callbacks defaulting to null and emit a per-case null guard that breaks to the trailing default. The omitted-arm signal and the explicitly-null-arm signal are therefore identical: partiality over delegates is a presence test, and a delegate has no value distinct from absence — a caller cannot pass null to mean "do nothing for this case and skip the default."

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

- Partial map resolves the asymmetry partial switch cannot: its per-item parameters are a one-field ref-struct wrapper carrying a presence flag with an implicit conversion from the result type, and the emitted body breaks on unset rather than on null — a mapped value of null or zero is distinguishable from an omitted arm, so partial mapping carries every legal result value including the type's default, where partial switching cannot carry a deliberately-null callback.
- The default-arm contract differs between the partial families for one mechanical reason: the func-shaped fall-through is invoked unconditionally after the switch and must produce a value, so its default is non-null; the action-shaped fall-through is a null-conditional invocation, so its default tolerates null — and an action-form partial dispatch with an omitted default makes every unhandled case a silent no-op indistinguishable from success.

[DERIVED_INDEX_HAPPENS_BEFORE]:
- A secondary index keyed on an item column must project from the item accessor, never eager-initialize from a static field: reading through the item list inserts a happens-before edge through the materialization lazy, so the projection cannot begin until the single pass that assigned every index and validated every key has published. An eager static initializer can run before that pass and observe items whose index cells still hold the sentinel and whose constructor arguments still hold unresolved cross-references — the materialization order is the synchronization primitive, and projecting from the item accessor is how a consumer subscribes to it.

```csharp
private static readonly Lazy<FrozenDictionary<int, Tier>> _byRank =
   new(static () => Items.ToFrozenDictionary(static t => t.Rank),
       LazyThreadSafetyMode.ExecutionAndPublication);
```
