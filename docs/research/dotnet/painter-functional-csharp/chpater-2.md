# Functional C# with Existing Language Features

## Describe the result, not the procedure

Imperative code specifies storage, mutation, iteration, branching, and ordering. Functional-style code describes the value to produce and lets expressions carry data through the transformation.

Prefer a query over a mutable accumulator:

```csharp
IEnumerable<Film> FilmsByGenre(
    IEnumerable<Film> films,
    string genre) =>
    films.Where(film => film.Genre == genre);
```

This small change establishes several useful properties:

- The source is an explicit input rather than hidden data access.
- `Where` receives behavior as a function.
- The source sequence is not modified.
- The filter is an expression rather than a loop containing mutable state.
- The predicate is referentially transparent if it neither mutates state nor depends on changing captured values.

LINQ operators enable a functional style but do not enforce it. A lambda can still capture mutable state or perform side effects, and a projection can still mutate an element.

The same result-oriented style applies to object construction. Define each property where the returned object is created instead of creating an empty object and assigning its properties across branches.

```csharp
ComplexObject MakeObject(SourceData source) =>
    new()
    {
        PropertyA = source.Something + source.SomethingElse,
        PropertyB = source.Ping * source.Pong,
        PropertyC = source.Alternate
            ? source.FirstChoice
            : source.SecondChoice,
        PropertyD = source.Alternate
            ? source.ThirdChoice
            : source.FourthChoice
    };
```

Every returned property and the inputs that determine it are visible in one place. If one calculation becomes too large, extract that calculation into a small function while keeping the final construction expression central.

## LINQ over `IEnumerable<T>` describes deferred work

Many LINQ operators return a description of how to produce a sequence. Creating the query does not execute it; execution normally begins when a consumer enumerates it. An enumerator exposes the current item and advances in one direction, without inherently knowing the sequence's length.

```csharp
var transformed = input
    .Select(First)
    .Select(Second)
    .Select(Third);

var result = transformed.ToArray();
```

Until enumeration, all three transformations remain pending. During enumeration, one input can pass through `First`, `Second`, and `Third` before the next input begins. This streams values through the pipeline without creating an intermediate collection after every stage.

`ToArray()` and `ToList()` force enumeration. Materializing after every stage changes the execution shape: each complete stage runs before the next one and each stage stores a collection. Delaying materialization can avoid work that is never demanded. Materialization is appropriate when execution is required at that point or when a captured result will be reused; otherwise, enumerating the same query repeatedly can repeat all of its work. Long deferred or recursive compositions can also carry memory and performance costs.

## Express sequence operations without mutable loop state

### Transform with `Select`

Use `Select` when each input becomes one output. A fluent chain and named intermediate sequences describe the same transformation, provided neither the sequences nor their elements are mutated.

```csharp
var normalized = films.Select(Normalize);
var priced = normalized.Select(CalculatePrice);
var rendered = priced.Select(Render);
```

Named stages reveal intent and make intermediate values inspectable. A single chain is useful when it remains equally clear.

Materializing a stage changes when it executes and whether its result is stored; it does not by itself make the transformation less functional. The functional condition is that neither the resulting collection nor its elements are subsequently mutated.

### Carry temporary context with tuples

A tuple can carry several short-lived related values between transformations without mutable locals or a dedicated class.

```csharp
var filmsWithCast = filmIds.Select(id => (
    film: GetFilm(id),
    cast: GetCastList(id)));

var descriptions = filmsWithCast.Select(item =>
    $"{item.film.Title}: {string.Join(", ", item.cast)}");
```

This technique depends on tuple support from C# 7 or an equivalent package on older versions.

### Obtain positions with indexed `Select`

The indexed overload supplies each element and its zero-based position, eliminating a counter that must be declared and incremented manually.

```csharp
var lines = orderedFilms.Select(
    (film, index) => $"{index} - {film.Title}");

var text = string.Join(Environment.NewLine, lines);
```

`Select` performs the one-to-one transformation; `string.Join` then reduces the sequence of strings to one string.

## Reduce many values to one

Use a specific reduction when one already exists:

```csharp
var total = values.Sum();
var revenue = films.Sum(film => film.BoxOfficeRevenue);
var mean = films.Average(film => film.BoxOfficeRevenue);
```

There is no corresponding built-in median operator in the chapter's baseline. For a non-empty sequence, sorting and choosing the middle value or pair expresses it without a mutable accumulator:

```csharp
var sorted = numbers.OrderBy(number => number).ToArray();

var median = sorted.Length % 2 == 0
    ? sorted.Skip((sorted.Length / 2) - 1).Take(2).Average()
    : sorted.Skip(sorted.Length / 2).First();
```

For a custom reduction, `Aggregate` takes a seed and an accumulator function. The function combines the current accumulated state with the next input. A tuple seed can calculate several results in one enumeration:

```csharp
var totals = films.Aggregate(
    (Budget: 0.0m, Revenue: 0.0m),
    (state, film) => (
        state.Budget + film.Budget,
        state.Revenue + film.BoxOfficeRevenue));
```

Each step returns a new accumulated value. No external running total is mutated, and both totals are produced in one pass rather than by two separate `Sum` calls.

## Recursion is expressive but stack-sensitive

A recursive iteration has a base condition that returns the answer and a recursive step that calls the function with updated state. It can model an early stop without mutable locals. For example, given deltas known to reach zero before they run out:

```csharp
int FirstPositionAtZero(int currentValue, int nextIndex = 0) =>
    currentValue == 0
        ? nextIndex - 1
        : FirstPositionAtZero(
            currentValue + deltas[nextIndex],
            nextIndex + 1);

var position = FirstPositionAtZero(10);
```

With deltas `2, -12, 9`, the calls carry values `10`, `12`, then `0`, returning index `1` without evaluating `9`.

This example deliberately assumes that zero is reached. A complete implementation also needs an exhaustion case; otherwise indexing beyond the deltas fails. An unreachable base condition can likewise recurse indefinitely.

Each call may remain on the stack while the deeper call completes. Even though the final result simply travels back through those waiting frames, a large input can exhaust the stack. Do not assume ordinary C# compilation will optimize a tail-position recursive call; recursion therefore carries a real memory cost.

## Immutability is deeper than private setters

Public setters plainly allow property replacement. Private setters limit replacement by callers, but do not make the object graph deeply immutable: code inside the class can still assign properties, a referenced list can still change, and a nested object can expose its own mutation operations.

For the older C# baseline used here:

- Construct a value completely, then do not reassign or mutate it.
- Expose `IEnumerable<T>` or `IReadOnlyList<T>` when callers do not need mutation operations.
- Apply the same discipline to every nested object.
- Remember that a read-only interface does not guarantee that the underlying collection or its elements can never change.

These measures support functional use but cannot universally enforce deep immutability, particularly across types whose implementation is outside the current code.

## End-to-end shape: one value, many values, one value

A compact reporting pipeline can read one CSV text, split it into records, parse typed values, group them, aggregate each group, format report lines, and join those lines into one result.

```csharp
var stories = File.ReadAllText(filePath)
    .Split(Environment.NewLine)
    .Select(line => line.Split(','))
    .Select(fields => new Story
    {
        SeasonNumber = int.Parse(fields[0]),
        StoryName = fields[1],
        Writer = fields[2],
        Director = fields[3],
        NumberOfEpisodes = int.Parse(fields[4]),
        NumberOfMissingEpisodes = int.Parse(fields[5])
    });

var totals = stories
    .GroupBy(story => story.SeasonNumber)
    .Select(group => group.Aggregate(
        (Season: group.Key, Episodes: 0, Missing: 0),
        (state, story) => (
            state.Season,
            state.Episodes + story.NumberOfEpisodes,
            state.Missing + story.NumberOfMissingEpisodes)));

var reportLines = totals.Select(total =>
{
    var percentage = total.Episodes == 0
        ? 0m
        : (decimal)total.Missing / total.Episodes * 100m;

    return $"{total.Season},{total.Episodes},{total.Missing},{percentage}";
});

var reportBody = string.Join(Environment.NewLine, reportLines);
var reportHeader =
    "Season,No Episodes,No Missing Eps,Percentage Missing";

var finalReport =
    $"{reportHeader}{Environment.NewLine}{reportBody}";
```

The CSV parsing is intentionally simple: it does not handle quoted fields, embedded commas, or other CSV complexities. Its purpose is to expose the functional data flow:

```text
single text -> records -> typed values -> groups -> totals -> lines -> single report
```

Each stage produces a new value. Named stages expose the changing data shape and aid inspection; one long fluent chain is equivalent only when it preserves the same operations and remains readable.
