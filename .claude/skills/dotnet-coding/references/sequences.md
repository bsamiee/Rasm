# [SEQUENCES]

Holds the worked sequence flows behind the expression rules of `dotnet-coding`: when a pipeline defers and when it materializes, how stages are named, how `Fold` expresses every reduction, how one element is replaced without mutation, how adjacent elements are compared, and how one text becomes one report.

## [01]-[DEFERRAL]

A LINQ operator over an `IEnumerable<A>` returns a description of how to produce a sequence, and execution begins when a consumer enumerates it. `Iterable<A>` is the lazy form over `IEnumerable<A>`, and `AsIterable()` lifts into it:

```csharp
internal static class Stages {
    public static Seq<Item> Transformed(IEnumerable<Item> input) {
        Iterable<Item> pending = input.AsIterable()
            .Map(First)
            .Map(Second)
            .Map(Third);

        return pending.ToSeq();
    }
}
```

Until enumeration every transformation stays pending, and during enumeration each input passes through `First`, `Second`, and `Third` before the next input begins, so no intermediate collection exists after each stage. `ToSeq()` forces enumeration and stores the result. Materializing after every stage changes the evaluation order and the storage, because each stage completes before the next and stores a collection. Delaying materialization avoids work that is never demanded, and materializing is right when execution must happen at that point or when the sequence is read more than once, because enumerating the same lazy query repeatedly repeats all of its work. Long deferred or recursive compositions carry memory and performance costs.

## [02]-[STAGES]

Fluent chains and named intermediate sequences describe the same transformation when nothing mutates the sequences or their elements. Named stages state each operation and expose intermediate values for inspection, and one chain stays readable while it stays short:

```csharp
internal static class Rendering {
    public static Seq<string> Rendered(Seq<Item> items) {
        Seq<Item> normalized = items.Map(Normalize);
        Seq<Priced> priced = normalized.Map(Price);
        return priced.Map(Render);
    }
}
```

Materialization does not introduce mutation, and the resulting collection and its elements stay unchanged.

## [03]-[AGGREGATION]

Every reduction is a `Fold` with a seed and a step, the seed defines the result for an empty input, and the seed type is independent of the element type: `0` with addition sums, `0` with an incrementing step counts, and an empty tree with an insertion step builds an index. Use a specific reduction when one exists, and use `Fold` where the specific call is ambiguous or absent:

```csharp
internal static class Totals {
    public static decimal Revenue(Seq<Item> items) =>
        items.Fold(0m, static (sum, item) => sum + item.Revenue);
    public static (decimal Cost, decimal Revenue) Both(Seq<Item> items) =>
        items.Fold(
            (Cost: 0m, Revenue: 0m),
            static (state, item) => (state.Cost + item.Cost, state.Revenue + item.Revenue));
}
```

A tuple seed computes several results in one pass, and each step returns a new accumulated value with no external running total. No median operator exists, so sorting and reading the middle element or pair expresses it without an accumulator, `At` reads the middle as an `Option`, and an empty sequence has no middle:

```csharp
internal static class Medians {
    public static Option<decimal> Median(Seq<int> values) {
        Seq<int> sorted = toSeq(values.Order());
        int middle = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted.At(middle).Map(static value => (decimal)value)
            : sorted.At(middle).Bind(right =>
                sorted.At(middle - 1).Map(left => (left + right) / 2m));
    }
}
```

`Fold` is a left fold equivalent to LINQ `Aggregate`, and `FoldBack` visits the same elements from last to first:

```text
Fold     : (Seq<A>, S, (S, A) -> S) -> S
FoldBack : (Seq<A>, S, (S, A) -> S) -> S

Seq(item0, item1, item2) = item0.Cons(item1.Cons(item2.Cons(Seq<A>())))

Fold     = f(f(f(seed, item0), item1), item2)
FoldBack = f(f(f(seed, item2), item1), item0)
```

A sequence has 2 constructors, the empty sequence and `Cons`, and `FoldBack` replaces both: the seed replaces the empty sequence and the step replaces each `Cons`. The same derivation produces a fold for any recursive type, one replacement per constructor, where each recursive position receives an already-folded result, and `Fold` can express `Map`, `Filter`, and `Bind`.

## [04]-[REPLACEMENT]

An indexed `Map` replaces one item and derives the replacement from the old item at that position, and the source stays unchanged:

```csharp
internal static class Replacement {
    public static Seq<A> ReplaceAt<A>(Seq<A> source, int index, Func<A, A> replace) =>
        source.Map((item, current) => current == index ? replace(item) : item);
}
```

## [05]-[ADJACENCY]

`source.Zip(source.Tail)` pairs each element with its successor as `(First, Second)`, and a quantifier over the pairs decides the result: `Exists` answers whether one pair matches, `ForAll` answers whether no pair disproves the condition, and fewer than 2 elements produce no pairs, so `Exists` returns `false` and `ForAll` returns `true`:

```csharp
internal static class Adjacency {
    public static bool AnyAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).Exists(pair => matches(pair.First, pair.Second));
    public static bool AllAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).ForAll(pair => matches(pair.First, pair.Second));
}
```

Sorting first and then testing whether any adjacent pair differs by one answers a gap question without a loop.

## [06]-[PIPELINE]

A report pipeline reads one text, splits it into records, parses typed values, groups them, aggregates each group, formats lines, and joins them into one result, and each stage produces a new value:

```text
single text -> records -> typed values -> groups -> totals -> lines -> single report
```

The text enters as a `string` argument, `At` reads each field as an `Option`, `parseInt` parses the numbers, `Traverse` turns the records into `Option<Seq<Record>>` so one failed parse makes the whole input `None`, and `Fold` into a `Map<int, ...>` groups the records with `AddOrUpdate` adding each to its group total:

```csharp
internal sealed record Record(int Group, string Name, int Count, int Missing);

internal static class Summary {
    public static Option<Record> Parse(string line) {
        Seq<string> fields = toSeq(line.Split(','));
        return from key in fields.At(0).Bind(static field => parseInt(field))
               from name in fields.At(1)
               from count in fields.At(2).Bind(static field => parseInt(field))
               from missing in fields.At(3).Bind(static field => parseInt(field))
               select new Record(key, name, count, missing);
    }
    public static Option<Seq<Record>> Records(string text) =>
        toSeq(text.Split(Environment.NewLine)).Traverse(static line => Parse(line)).As();
    public static Map<int, (int Count, int Missing)> Totals(Seq<Record> records) =>
        records.Fold(
            Map<int, (int Count, int Missing)>(),
            static (state, record) => state.AddOrUpdate(
                record.Group,
                total => (total.Count + record.Count, total.Missing + record.Missing),
                (record.Count, record.Missing)));
    public static decimal MissingShare(int missing, int count) =>
        count == 0 ? 0m : (decimal)missing / count * 100m;
    public static Seq<string> Lines(Map<int, (int Count, int Missing)> totals) =>
        totals.ToSeq().Map(static total => string.Create(
            CultureInfo.InvariantCulture,
            $"{total.Key},{total.Value.Count},{total.Value.Missing},{MissingShare(total.Value.Missing, total.Value.Count)}"));
    public static Option<string> Render(string text) =>
        Records(text).Map(static records => {
            const string header = "Group,Count,Missing,Percentage Missing";
            return $"{header}{Environment.NewLine}{string.Join(Environment.NewLine, Lines(Totals(records)))}";
        });
}
```

An empty line parses to `None`, so the text cannot end with a newline, and the parser handles no quoted fields or embedded commas.
