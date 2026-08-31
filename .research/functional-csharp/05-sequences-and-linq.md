# Functional C# with Existing Language Features

## Describe the result, not the procedure

Imperative code specifies storage, mutation, iteration, branching, and ordering. Functional-style code describes the value to produce and lets expressions carry data through the transformation.

Prefer a query over a mutable accumulator:

```csharp
internal static partial class Sequences {
    public static Seq<Film> FilmsByGenre(Seq<Film> films, string genre) =>
        films.Filter(film => string.Equals(film.Genre, genre, StringComparison.Ordinal));
}
```

`Seq<A>` is the strict sequence type and the default collection in domain code. This small change establishes several useful properties:
- The source is an explicit input rather than hidden data access.
- `Filter` receives behavior as a function.
- The source sequence is not modified.
- The filter is an expression rather than a loop containing mutable state.
- The predicate is referentially transparent if it neither mutates state nor depends on changing captured values.

LINQ operators enable a functional style but do not enforce it. A lambda can still capture mutable state or perform side effects, and a projection can still mutate an element.

The same result-oriented style applies to object construction. Define each property where the returned object is created instead of creating an empty object and assigning its properties across branches.

```csharp
internal static partial class Sequences {
    public static ComplexObject MakeObject(SourceData source) =>
        new() {
            PropertyA = source.Something + source.SomethingElse,
            PropertyB = source.Ping * source.Pong,
            PropertyC = source.Alternate
                ? source.FirstChoice
                : source.SecondChoice,
            PropertyD = source.Alternate
                ? source.ThirdChoice
                : source.FourthChoice,
        };
}
```

Every returned property and the inputs that determine it are visible in one place. If one calculation becomes too large, extract that calculation into a small function while keeping the final construction expression central.

## LINQ over `IEnumerable<T>` describes deferred work

Many LINQ operators return a description of how to produce a sequence. Creating the query does not execute it; execution normally begins when a consumer enumerates it. An enumerator exposes the current item and advances in one direction, without inherently knowing the sequence's length.

`Seq<A>` is strict. `Iterable<A>` is the lazy form over an `IEnumerable<A>`, and `AsIterable()` lifts one.

```csharp
internal static partial class Sequences {
    public static Seq<int> Transformed(IEnumerable<int> input) {
        Iterable<int> transformed = input.AsIterable()
            .Map(First)
            .Map(Second)
            .Map(Third);

        return transformed.ToSeq();
    }
}
```

Until enumeration, all three transformations remain pending. During enumeration, one input can pass through `First`, `Second`, and `Third` before the next input begins. This streams values through the pipeline without creating an intermediate collection after every stage.

`ToSeq()` forces enumeration and materializes a `Seq<int>`. Materializing after every stage changes the execution shape: each complete stage runs before the next one and each stage stores a collection. Delaying materialization can avoid work that is never demanded. Materialization is appropriate when execution is required at that point or when a captured result will be reused; otherwise, enumerating the same query repeatedly can repeat all of its work. Long deferred or recursive compositions can also carry memory and performance costs.

## Express sequence operations without mutable loop state

### Transform with `Select`

Use `Select` when each input becomes one output. `Map` on a `Seq<A>` is the same projection. A fluent chain and named intermediate sequences describe the same transformation, provided neither the sequences nor their elements are mutated.

```csharp
internal static partial class Sequences {
    public static Seq<string> Rendered(Seq<Film> films) {
        Seq<Film> normalized = films.Map(Normalize);
        Seq<Priced> priced = normalized.Map(CalculatePrice);
        return priced.Map(Render);
    }
}
```

Named stages reveal intent and make intermediate values inspectable. A single chain is useful when it remains equally clear.

Materializing a stage changes when it executes and whether its result is stored; it does not by itself make the transformation less functional. The functional condition is that neither the resulting collection nor its elements are subsequently mutated.

### Carry temporary context with tuples

A tuple can carry several short-lived related values between transformations without mutable locals or a dedicated class.

```csharp
internal static partial class Sequences {
    public static Seq<string> Descriptions(
        Seq<int> filmIds,
        Func<int, Film> getFilm,
        Func<int, Seq<string>> getCastList) {
        Seq<(Film Film, Seq<string> Cast)> filmsWithCast = filmIds.Map(id => (
            Film: getFilm(id),
            Cast: getCastList(id)));

        return filmsWithCast.Map(static item => $"{item.Film.Title}: {string.Join(", ", item.Cast)}");
    }
}
```

This technique depends on tuple support from C# 7 or an equivalent package on older versions.

### Obtain positions with indexed `Select`

The indexed overload supplies each element and its zero-based position, eliminating a counter that must be declared and incremented manually.

```csharp
internal static partial class Sequences {
    public static string Numbered(Seq<Film> orderedFilms) {
        Seq<string> lines = orderedFilms.Map(static (film, index) =>
            string.Create(CultureInfo.InvariantCulture, $"{index} - {film.Title}"));

        return string.Join(Environment.NewLine, lines);
    }
}
```

The indexed `Map` performs the one-to-one transformation. `string.Join` then reduces the sequence of strings to one string.

## Reduce many values to one

Use a specific reduction when one already exists. On a `Seq` the simple `Sum()` and `Average()` calls are ambiguous between the LanguageExt and LINQ forms, so `Fold` is the reduction shown. A sum is a `Fold` with a zero seed and an addition step:

```csharp
internal static partial class Sequences {
    public static int Total(Seq<int> values) =>
        values.Fold(0, static (sum, value) => sum + value);
    public static decimal Revenue(Seq<Film> films) =>
        films.Fold(0m, static (sum, film) => sum + film.BoxOfficeRevenue);
}
```

There is no built-in median operator. Sorting and choosing the middle value or pair expresses it without a mutable accumulator, and `At` reads the middle element as an `Option`:

```csharp
internal static partial class Sequences {
    public static Option<decimal> Median(Seq<int> numbers) {
        Seq<int> sorted = toSeq(numbers.Order());
        int middle = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted.At(middle).Map(static value => (decimal)value)
            : sorted.At(middle).Bind(right =>
                sorted.At(middle - 1).Map(left => (left + right) / 2m));
    }
}
```

`Order` from LINQ sorts, and `toSeq` materializes the sorted `Seq<int>`. An empty sequence has no middle element, so its median is `None`.

For a custom reduction, `Fold` takes a seed and a step function. The function combines the current accumulated state with the next input. A tuple seed can calculate several results in one pass:

```csharp
internal static partial class Sequences {
    public static (decimal Budget, decimal Revenue) Totals(Seq<Film> films) =>
        films.Fold(
            (Budget: 0.0m, Revenue: 0.0m),
            static (state, film) => (
                state.Budget + film.Budget,
                state.Revenue + film.BoxOfficeRevenue));
}
```

Each step returns a new accumulated value. No external running total is mutated, and both totals are produced in one pass rather than by two separate folds.

`Fold` is a left fold, the LINQ `Aggregate`. It consumes a seed and a reducer and returns one accumulated value. `FoldBack` consumes the same seed and reducer and visits the elements from last to first:

```text
Fold     : (Seq<A>, S, (S, A) -> S) -> S
FoldBack : (Seq<A>, S, (S, A) -> S) -> S

Fold     = f(f(f(seed, item0), item1), item2) ...
FoldBack = f(f(f(seed, item2), item1), item0) ...
```

The seed defines the empty-input result, and its type is independent of the element type. Examples include `0` for a sum, `0` plus an incrementing reducer for a count, or an empty immutable tree plus an insertion reducer for building a tree.

`Fold` is general enough to express `Map`, `Filter`, and `Bind`.

## Recursion is expressive but stack-sensitive

A recursive iteration has a base condition that returns the answer and a recursive step that calls the function with updated state. It models an early stop without mutable locals. A complete implementation also needs an exhaustion case, because an unreachable base condition recurses without end. C# does not guarantee tail-call optimization, so recursion carries a real stack cost.

## Non-mutating updates

An indexed `Map` describes a replacement without changing the source `Seq<A>`:

```csharp
internal static partial class Sequences {
    public static Seq<A> ReplaceAt<A>(Seq<A> source, int index, Func<A, A> replace) =>
        source.Map((item, currentIndex) =>
            currentIndex == index ? replace(item) : item);
}
```

The function derives the replacement from the old item at one position. `Map` returns a new `Seq<A>`, and the source remains unchanged.

## Immutability is deeper than private setters

Private setters and read-only interfaces do not make the resulting object graph deeply immutable.

## Custom enumeration

`Seq<A>` supplies `Tail` and `Zip`. `Zip` with `Tail` pairs every element with its neighbor, and an indefinite sequence of states is `LanguageExt.List.unfold`.

Pairing a sequence with its own tail allows traversal policies that ordinary single-element predicates cannot express.

### Compare adjacent elements

`source.Zip(source.Tail)` produces the pairs `(First, Second)`. A quantifier over those pairs decides the result.

`AnyAdjacent` uses `Exists` and returns `true` when one pair matches. Fewer than two elements produce no pair, so the result is `false`. `AllAdjacent` uses `ForAll` and returns `true` when no pair disproves the condition. Fewer than two elements therefore produce `true`.

```csharp
internal static partial class Sequences {
    public static bool AnyAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).Exists(pair => matches(pair.First, pair.Second));
    public static bool AllAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).ForAll(pair => matches(pair.First, pair.Second));
}
```

This is useful when a condition depends on neighboring values. For example, sort a number sequence, then test whether any adjacent pair differs by one.

## End-to-end shape: one value, many values, one value

A compact reporting pipeline can read one CSV text, split it into records, parse typed values, group them, aggregate each group, format report lines, and join those lines into one result.

The text enters as a `string` argument. `At` reads each field as an `Option`, `parseInt` parses the numbers, and `Traverse` turns the records into `Option<Seq<Story>>`. One failed parse makes the whole input `None`. An empty line parses to `None`, so the text carries no trailing newline. `Fold` into a `Map<int, ...>` groups the stories by season, and `AddOrUpdate` adds each story to its season total.

```csharp
internal sealed record Story(
    int SeasonNumber,
    string StoryName,
    string Writer,
    string Director,
    int NumberOfEpisodes,
    int NumberOfMissingEpisodes);

internal static partial class Sequences {
    public static Option<Story> ParseStory(string line) {
        Seq<string> fields = toSeq(line.Split(','));
        return from season in fields.At(0).Bind(static field => parseInt(field))
               from name in fields.At(1)
               from writer in fields.At(2)
               from director in fields.At(3)
               from episodes in fields.At(4).Bind(static field => parseInt(field))
               from missing in fields.At(5).Bind(static field => parseInt(field))
               select new Story(season, name, writer, director, episodes, missing);
    }
    public static Option<Seq<Story>> Stories(string csv) =>
        toSeq(csv.Split(Environment.NewLine)).Traverse(static line => ParseStory(line)).As();
    public static Map<int, (int Episodes, int Missing)> SeasonTotals(Seq<Story> stories) =>
        stories.Fold(
            Map<int, (int Episodes, int Missing)>(),
            static (state, story) => state.AddOrUpdate(
                story.SeasonNumber,
                total => (
                    total.Episodes + story.NumberOfEpisodes,
                    total.Missing + story.NumberOfMissingEpisodes),
                (story.NumberOfEpisodes, story.NumberOfMissingEpisodes)));
    public static decimal MissingPercentage(int missing, int episodes) =>
        episodes == 0 ? 0m : (decimal)missing / episodes * 100m;
    public static Seq<string> ReportLines(Map<int, (int Episodes, int Missing)> totals) =>
        totals.ToSeq().Map(static total => string.Create(
            CultureInfo.InvariantCulture,
            $"{total.Key},{total.Value.Episodes},{total.Value.Missing},{MissingPercentage(total.Value.Missing, total.Value.Episodes)}"));
    public static Option<string> Report(string csv) =>
        Stories(csv).Map(static stories => {
            string reportBody = string.Join(Environment.NewLine, ReportLines(SeasonTotals(stories)));
            const string reportHeader = "Season,No Episodes,No Missing Eps,Percentage Missing";
            return $"{reportHeader}{Environment.NewLine}{reportBody}";
        });
}
```

The CSV parsing is intentionally simple: it does not handle quoted fields, embedded commas, or other CSV complexities. Its purpose is to expose the functional data flow:

```text
single text -> records -> typed values -> groups -> totals -> lines -> single report
```

Each stage produces a new value. Named stages expose the changing data shape and aid inspection; one long fluent chain is equivalent only when it preserves the same operations and remains readable.
