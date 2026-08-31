# [SEQUENCES_AND_LINQ]

## [01]-[DECLARATIVE_STYLE]

Imperative code specifies storage, mutation, iteration, branching, and ordering. Functional-style code describes the value to produce and lets expressions carry data through the transformation.

Prefer a query over a mutable accumulator:

```csharp
internal static partial class Sequences {
    public static Seq<Film> FilmsByGenre(Seq<Film> films, string genre) =>
        films.Filter(film => string.Equals(film.Genre, genre, StringComparison.Ordinal));
}
```

`Seq<A>` is the strict sequence type and the default collection in domain code. This example has these properties:
- The source is an explicit input rather than hidden data access.
- `Filter` receives behavior as a function.
- The source sequence is not modified.
- The filter is an expression rather than a loop containing mutable state.
- The predicate is referentially transparent if it neither mutates state nor depends on changing captured values.

LINQ operators enable a functional style but do not enforce it. A lambda can still capture mutable state or perform side effects, and a projection can still mutate an element.

The same expression-oriented style applies to object construction. Define each property where the returned object is created instead of creating an empty object and assigning its properties across branches.

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

Every returned property and the inputs that determine it are visible in one place. If a calculation obscures the construction, extract it into a function and keep the construction expression central.

## [02]-[DEFERRED_EXECUTION]

Many LINQ operators return a description of how to produce a sequence. Creating the query does not execute it; execution begins when a consumer enumerates it. An enumerator exposes the current item and advances in one direction without knowing the sequence length.

`Iterable<A>` is the lazy form over `IEnumerable<A>`, and `AsIterable()` lifts an `IEnumerable<A>` into it.

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

Until enumeration, all three transformations remain pending. During enumeration, each input passes through `First`, `Second`, and `Third` before the next input begins. This streams values through the pipeline without creating an intermediate collection after every stage.

`ToSeq()` forces enumeration and materializes a `Seq<int>`. Materializing after every stage changes the evaluation order and storage: each complete stage runs before the next one and stores a collection. Delaying materialization can avoid work that is never demanded. Materialization is appropriate when execution is required at that point or when a materialized sequence will be reused; otherwise, enumerating the same query repeatedly can repeat all of its work. Long deferred or recursive compositions can carry memory and performance costs.

## [03]-[SEQUENCE_OPERATIONS]

### [03.1]-[SELECT]

Use `Select` when each input becomes one output. `Map` on a `Seq<A>` is the same projection. A fluent chain and named intermediate sequences describe the same transformation when neither the sequences nor their elements are mutated.

```csharp
internal static partial class Sequences {
    public static Seq<string> Rendered(Seq<Film> films) {
        Seq<Film> normalized = films.Map(Normalize);
        Seq<Priced> priced = normalized.Map(CalculatePrice);
        return priced.Map(Render);
    }
}
```

Named stages state each operation and let you inspect intermediate values. Use a single chain when it remains clear.

Materialization does not introduce mutation. The resulting collection and its elements must remain unchanged.

### [03.2]-[TUPLE_CONTEXT]

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

Tuples require C# 7 or an equivalent package on older versions.

### [03.3]-[INDEXED_SELECT]

The indexed overload supplies each element and its zero-based position, eliminating a counter that must be declared and incremented.

```csharp
internal static partial class Sequences {
    public static string Numbered(Seq<Film> orderedFilms) {
        Seq<string> lines = orderedFilms.Map(static (film, index) =>
            string.Create(CultureInfo.InvariantCulture, $"{index} - {film.Title}"));

        return string.Join(Environment.NewLine, lines);
    }
}
```

The indexed `Map` performs the one-to-one transformation. `string.Join` reduces the sequence of strings to one string.

## [04]-[AGGREGATION]

Use a specific reduction when one exists. Because LanguageExt and LINQ define ambiguous `Sum()` and `Average()` calls on `Seq`, this example uses `Fold`. A sum is a `Fold` with a zero seed and an addition step:

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

For a custom reduction, `Fold` takes a seed and a step function. The function combines the accumulator with the next input. A tuple seed can calculate several results:

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

`Fold` is a left fold equivalent to LINQ's `Aggregate`. `FoldBack` uses the same seed and reducer but visits elements from last to first:

```text
Fold     : (Seq<A>, S, (S, A) -> S) -> S
FoldBack : (Seq<A>, S, (S, A) -> S) -> S

Seq(item0, item1, item2) = item0.Cons(item1.Cons(item2.Cons(Seq<A>())))

Fold     = f(f(f(seed, item0), item1), item2) ...
FoldBack = f(f(f(seed, item2), item1), item0) ...
```

The seed defines the empty-input result, and its type is independent of the element type. The seed can be `0` for a sum, `0` with an incrementing reducer for a count, or an empty immutable tree with an insertion reducer.

A sequence has two constructors: the empty sequence and cons, which prepends one element. `FoldBack` replaces both: the seed replaces the empty sequence, and the step replaces each cons. The same derivation produces a fold for any recursive type: one replacement per constructor, where each recursive position receives an already-folded result.

`Fold` can express `Map`, `Filter`, and `Bind`.

## [05]-[RECURSION]

A recursive function has a base condition that returns the answer and a recursive step that calls the function with updated state. It models an early stop without mutable locals. An implementation needs an exhaustion case because a function without a reachable base condition does not terminate. Because C# does not guarantee tail-call optimization, recursion uses stack space.

## [06]-[NON_MUTATING_UPDATES]

Use an indexed `Map` to replace one item:

```csharp
internal static partial class Sequences {
    public static Seq<A> ReplaceAt<A>(Seq<A> source, int index, Func<A, A> replace) =>
        source.Map((item, currentIndex) =>
            currentIndex == index ? replace(item) : item);
}
```

The function derives the replacement from the old item at one position. `Map` returns a new `Seq<A>`, and the source remains unchanged.

## [07]-[DEEP_IMMUTABILITY]

Private setters and read-only interfaces do not make an object graph deeply immutable.

## [08]-[SEQUENCE_TRAVERSAL]

`Seq<A>` supplies `Tail` and `Zip`. `LanguageExt.List.unfold` produces a sequence of states.

### [08.1]-[ADJACENT_ELEMENTS]

`source.Zip(source.Tail)` produces the pairs `(First, Second)`. A quantifier over those pairs decides the result.

`AnyAdjacent` uses `Exists` and returns `true` when one pair matches. `AllAdjacent` uses `ForAll` and returns `true` when no pair disproves the condition. Because fewer than two elements produce no pairs, `AnyAdjacent` returns `false` and `AllAdjacent` returns `true`.

```csharp
internal static partial class Sequences {
    public static bool AnyAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).Exists(pair => matches(pair.First, pair.Second));
    public static bool AllAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).ForAll(pair => matches(pair.First, pair.Second));
}
```

For example, sort a number sequence, then test whether any adjacent pair differs by one.

## [09]-[END_TO_END_PIPELINE]

A reporting pipeline can read one CSV text, split it into records, parse typed values, group them, aggregate each group, format report lines, and join those lines into one result.

The text enters as a `string` argument. `At` reads each field as an `Option`, `parseInt` parses the numbers, and `Traverse` turns the records into `Option<Seq<Story>>`. One failed parse makes the whole input `None`. Because an empty line parses to `None`, the text cannot have a trailing newline. `Fold` into a `Map<int, ...>` groups the stories by season, and `AddOrUpdate` adds each story to its season total.

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

The CSV parser does not handle quoted fields or embedded commas. It illustrates this functional data flow:

```text
single text -> records -> typed values -> groups -> totals -> lines -> single report
```

Each stage produces a new value.
