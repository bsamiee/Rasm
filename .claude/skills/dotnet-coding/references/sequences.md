# [SEQUENCES]

Covers when sequence work runs, how a sequence reduces, and how neighbors compare.

## [01]-[DEFERRAL]

`toSeq` over an `IEnumerable<A>`, `ToSeq()` on an `Iterable<A>`, and `Map` on the resulting `Seq<A>` defer and cache: each element computes on first demand, a partial read (`Head`, `Take`) runs only the elements it reaches, and a second read reruns nothing. `Iterable<A>` and LINQ queries rerun the whole chain on every enumeration and fit one streaming pass over a source too large to hold. `Strict()` computes every element at the call, and a sequence whose enumeration performs I/O calls it inside the `IO` that owns the read, else the read runs wherever a consumer first enumerates it:

```csharp
internal static class Readings {
    public static IO<Seq<A>> Read<A>(string path, Func<string, Option<A>> parse) =>
        IO.lift(() => toSeq(File.ReadLines(path)).Choose(parse).Strict());
}
```

## [02]-[REDUCTION]

A keyed reduction holds one accumulator per key where `GroupBy` holds every element of every group, a tuple seed computes every result in one pass, and a seed that depends on its key comes from the `Func<TKey, TAccumulate>` overload. `Fold` takes the same seed and step unkeyed, and `FoldBack` takes the same `(state, item)` step and visits from the last element:

```csharp
internal static class Totals {
    public static FrozenDictionary<int, (int Count, int Missing)> ByGroup(Seq<Entry> entries) =>
        entries
            .AggregateBy(static entry => entry.Group, (Count: 0, Missing: 0), static (total, entry) => (total.Count + entry.Count, total.Missing + entry.Missing))
            .ToFrozenDictionary();
}
```

## [03]-[ADJACENCY]

SuperLinq `Window(2, pair => …)` reads each element of a `Seq<A>` beside its successor in one span, `Contains(true)` answers whether some pair matches, `Contains(false)` whether some pair fails, and a sequence shorter than the window yields no pairs. `Segment((current, previous, index) => …)` cuts runs where a pair disagrees, `GroupAdjacent` cuts runs of one key, and `Lag` or `Lead` pairs each element with the one at a fixed offset:

```csharp
internal static class Gaps {
    public static bool Exceeds(Seq<int> sorted, int gap = 1) =>
        sorted.Window(2, pair => pair[1] - pair[0] > gap).Contains(true);
}
```
