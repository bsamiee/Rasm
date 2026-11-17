# [SEQUENCES]

Covers the sequence flows behind the expression rules, from deferral to the pipeline that turns one text into one report.

## [01]-[DEFERRAL]

LINQ operators over an `IEnumerable<A>` return a description of how to produce a sequence, and execution begins when a consumer enumerates it. `Iterable<A>` is the lazy form over `IEnumerable<A>`, and `AsIterable()` lifts into it:

```csharp
internal static class Stages {
    public static Seq<Item> Transformed(IEnumerable<Item> input) {
        Iterable<Item> pending = input.AsIterable()
            .Map(First)
            .Map(Second)
            .Map(Third);

        return pending.ToSeq().Strict();
    }
}
```

Until enumeration every transformation stays pending, during enumeration each input passes through `First`, `Second`, and `Third` before the next input begins, and no intermediate collection exists after a stage. `ToSeq()` wraps this iterable in a lazy memoizing sequence; `Strict()` forces enumeration. Materializing after every stage completes each stage before the next and stores a collection per stage. Delaying materialization avoids work that is never demanded. Enumerating the same lazy query repeatedly repeats all of its work, materialize when execution must happen at that point or when the sequence is read more than once. Long deferred or recursive compositions have memory and performance costs.

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

Tuple seeds compute more than one result in one pass, and each step returns a new accumulated value with no external running total. MathNet.Numerics supplies median selection without sorting every sample. Empty input has no median:

```csharp
using MathNet.Numerics.Statistics;

internal static class Medians {
    public static Option<decimal> Median(Seq<int> values) =>
        values.IsEmpty ? None : (decimal)Enumerable.Select(values, static value => (double)value).Median();
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

Sequence constructors are the empty sequence and `Cons`, `FoldBack` replaces both: the seed replaces the empty sequence and the step replaces each `Cons`. The same derivation produces a fold for any recursive type, one replacement per constructor with each recursive position receiving an already-folded result, and `Fold` can express `Map`, `Filter`, and `Bind`.

## [04]-[REPLACEMENT]

Indexed `Map` replaces one item and derives the replacement from the old item at that position, and the source stays unchanged:

```csharp
internal static class Replacement {
    public static Seq<A> ReplaceAt<A>(Seq<A> source, int index, Func<A, A> replace) =>
        source.Map((item, current) => current == index ? replace(item) : item);
}
```

## [05]-[ADJACENCY]

Reference `SuperLinq` directly and import its namespace for `Window(2, selector)`, which projects each adjacent pair from a span. LINQ `Any` and `All` short-circuit over the projected results. Fewer than 2 elements produce no windows, so `Any` returns `false` and `All` returns `true`:

```csharp
using SuperLinq;

internal static class Adjacency {
    public static bool AnyAdjacent<A>(IEnumerable<A> source, Func<A, A, bool> matches) =>
        source.Window(2, window => matches(window[0], window[1])).Any(identity);
    public static bool AllAdjacent<A>(IEnumerable<A> source, Func<A, A, bool> matches) =>
        source.Window(2, window => matches(window[0], window[1])).All(identity);
}
```

Sorting first and then testing whether any adjacent pair differs by one answers a gap question without a loop.

SuperLinq also owns `Segment((current, previous, index) => …)` for cuts between neighbors, `GroupAdjacent` for runs of one key, and `Window(n, selector)` for n-wide neighbors. A `Fold` over `Last` and `Init` or a `Zip` with `Tail` restates them.

## [06]-[PIPELINE]

Report pipelines read one text, split it into records, parse typed values, group them, aggregate each group, format lines, and join them into one result, and each stage produces a new value:

```text
single text -> records -> typed values -> groups -> totals -> lines -> single report
```

Text enters as a `string` argument with its delimiter, `ReplaceLineEndings` reads every line break the text holds, a list pattern binds the fields in `Record`'s declared order, tuple `Apply` joins the independent `parseInt` results, `Traverse` turns the records into an `Option<Seq<Record>>` that one failed parse makes `None`, and `Fold` into a `Map<int, ...>` groups the records with `AddOrUpdate` adding each to its group total:

```csharp
internal sealed record Record(int Group, string Name, int Count, int Missing);

internal static class Summary {
    public static Option<Record> Parse(string line, char delimiter) =>
        line.Split(delimiter) switch {
            [var groupText, var name, var countText, var missingText] =>
                (parseInt(groupText), parseInt(countText), parseInt(missingText)).Apply((group, count, missing) => new Record(group, name, count, missing)).As(),
            _ => None,
        };
    public static Option<Seq<Record>> Records(string text, char delimiter) =>
        toSeq(text.ReplaceLineEndings().Split(Environment.NewLine)).Traverse(line => Parse(line, delimiter)).As();
    public static Map<int, (int Count, int Missing)> Totals(Seq<Record> records) =>
        records.Fold(
            Map<int, (int Count, int Missing)>(),
            static (state, record) => state.AddOrUpdate(
                record.Group,
                total => (total.Count + record.Count, total.Missing + record.Missing),
                (record.Count, record.Missing)));
    public static Option<decimal> MissingShare(int missing, int count) =>
        count is 0 ? None : Some((decimal)missing / count * 100m);
    public static Seq<string> Lines(Map<int, (int Count, int Missing)> totals, char delimiter) =>
        totals.ToSeq().Map(total => string.Create(
            CultureInfo.InvariantCulture,
            $"{total.Key}{delimiter}{total.Value.Count}{delimiter}{total.Value.Missing}{delimiter}{MissingShare(total.Value.Missing, total.Value.Count).Match(Some: static share => share.ToString(CultureInfo.InvariantCulture), None: static () => string.Empty)}"));
    public static Option<string> Render(string text, char delimiter) =>
        Records(text, delimiter).Map(records => string.Join(
            Environment.NewLine,
            [string.Join(delimiter, nameof(Record.Group), nameof(Record.Count), nameof(Record.Missing), nameof(MissingShare)), .. Lines(Totals(records), delimiter)]));
}
```

Empty lines and lines of another field count parse to `None`, the text cannot end with a line break, and the parser handles no quoted fields or embedded delimiters. Header columns are the `nameof` of the members they report, and a group with no count has no share and renders an empty field.
