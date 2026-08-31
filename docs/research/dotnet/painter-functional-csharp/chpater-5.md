# Higher-Order Functions

## Functions as values

A higher-order function accepts a function, returns a function, or both. C# usually represents the passed behavior with delegates:
- `Func<T, TResult>` accepts a value and returns a value.
- `Action<T>` accepts a value and returns nothing.
- A lambda supplies an inline delegate, as in `items.Where(x => x.IsActive)`.
- A function can create a new function: `Func<int, int> AddBy(int amount) => value => amount + value;`.

This makes behavior configurable. A workflow can keep its fixed sequence while callers provide only the calculation, formatting, or effect that varies.

## Thunks: defer the variable calculation

A thunk bundles a calculation so it can be executed later. In C#, a `Func` delegate can fill a deliberate hole in a larger function. A reporting workflow, for example, can accept the property used to group its input:

```csharp
public static Report BuildSummary(IEnumerable<EnemyShip> ships,
    Func<EnemyShip, string> summarizeBy,
    string title) =>
    new Report {
        Title = title,
        Rows = ships.GroupBy(summarizeBy)
            .Select(group => new ReportItem {
                ColumnOne = group.Key, ColumnTwo = group.Count().ToString()
            })
    };

var byType = BuildSummary(ships, ship => ship.Type, "Enemy Ship Type");
var byWeaponry = BuildSummary(
    ships,
    ship => ship.WeaponryLevel,
    "Enemy Ship Weaponry Level");
```

The summary construction is written once. A new report supplies a selector and title rather than copying the grouping and row construction. The same higher-order boundary can also centralize retrieval, empty-result routing, transmission, and error handling while leaving only the selector and report name variable.

The same opening can be widened deliberately: another `Func` can control formatting, while an `Action` can supply logging or event handling. Exposing the configurable operation lets callers extend the report set without modifying the implementation; keeping small named wrappers preserves specific intent where that is useful.

## Combinators

A combinator applies or combines functions to build richer behavior. A small vocabulary covers common transformation shapes.

### Map: transform one whole value

This value-level `Map`, also called Chain or Pipe, applies one function to one value. LINQ `Select` instead applies a function to each element of a sequence; mapping an enumerable with this extension treats the enumerable itself as the value.

```csharp
public static TOut Map<TIn, TOut>(
    this TIn value,
    Func<TIn, TOut> transform) =>
    transform(value);

string Celsius(decimal fahrenheit) =>
    fahrenheit
        .Map(x => x - 32)
        .Map(x => x * 5)
        .Map(x => x / 9)
        .Map(x => Math.Round(x, 2))
        .Map(x => $"{x} degrees C");
```

The generic input and output types allow each step to change type. The chain expresses a multi-stage calculation without throwaway variables.

If every transformation has the same input and output type, one overload can accept all steps at once:

```csharp
public static T Map<T>(
    this T value,
    params Func<T, T>[] transforms) =>
    transforms.Aggregate(value, (current, transform) => transform(current));
```

This form is shorter, but it cannot represent a type change between steps.

### Fork: derive several values, then join

`Fork`, also called Converge, gives the same input to multiple prong functions and passes their outputs to a join function:

```csharp
public static TOut Fork<TIn, TLeft, TRight, TOut>(
    this TIn value,
    Func<TIn, TLeft> left,
    Func<TIn, TRight> right,
    Func<TLeft, TRight, TOut> join) =>
    join(left(value), right(value));

double Average(IEnumerable<double> values) =>
    values.Fork(
        sequence => sequence.Sum(),
        sequence => sequence.Count(),
        (sum, count) => sum / count);
```

Separate generic result types let fixed prongs produce different kinds of value. More fixed prongs require more overloads. An arbitrary number of prongs is possible when every prong returns the same intermediate type:

```csharp
public static TOut Fork<TIn, TPart, TOut>(
    this TIn value,
    Func<IEnumerable<TPart>, TOut> join,
    params Func<TIn, TPart>[] prongs) =>
    join(prongs.Select(prong => prong(value)));
```

The shown implementations invoke their prongs through ordinary calls; the important relationship is that every prong receives the original input before their results are joined.

### Compose: build a reusable function

`Map` produces a transformed value. `Compose` joins functions and produces another function:

```csharp
public static Func<TIn, TOut> Compose<TIn, TMiddle, TOut>(
    this Func<TIn, TMiddle> first,
    Func<TMiddle, TOut> second) =>
    input => second(first(input));

Func<decimal, decimal> fahrenheitToCelsius =
    x => (x - 32) * 5 / 9;

Func<decimal, string> format =
    x => $"{Math.Round(x, 2)} degrees";

Func<decimal, string> formattedConversion =
    fahrenheitToCelsius.Compose(format);
```

The reusable formatting suffix can now be composed with conversions in either direction. Improvements to that suffix are made once. In C#, first give a lambda a compatible delegate type so `Compose` can be resolved as an extension on `Func<...>`.

### Transduce: transform a sequence, then aggregate

A transduction combines any `Where` and `Select` pipeline with an aggregation that collapses the resulting sequence to one value:

```csharp
public static TOut Transduce<TIn, TItem, TOut>(
    this IEnumerable<TIn> source,
    Func<IEnumerable<TIn>, IEnumerable<TItem>> transform,
    Func<IEnumerable<TItem>, TOut> aggregate) =>
    aggregate(transform(source));

var message = numbers.Transduce(
    values => values
        .Select(x => x + 5)
        .Select(x => x * 10)
        .Where(x => x > 100),
    values => string.Join(", ", values));
```

Returning a delegate instead makes the entire policy reusable with many sequences:

```csharp
public static Func<IEnumerable<TIn>, TOut>
    ToTransducer<TIn, TItem, TOut>(
        this Func<IEnumerable<TIn>, IEnumerable<TItem>> transform,
        Func<IEnumerable<TItem>, TOut> aggregate) =>
    source => aggregate(transform(source));
```

### Tap: observe a chain without changing its value

`Tap` passes the current value to an `Action`, then returns that same value so the chain can continue:

```csharp
public static T Tap<T>(this T value, Action<T> action)
{
    action(value);
    return value;
}
```

It can log or otherwise inspect an intermediate result between transformations. The action is an effect, but it does not replace the value moving through the chain.

## Encapsulating exception handling

Pure transformations should not fail because of external conditions, but calls at the edge - databases, web APIs, network files - can. A higher-order wrapper can centralize `try/catch`, reduce repeated boilerplate, and keep exception-driven jumps from being scattered across call layers. The chapter develops several versions, each with a tradeoff:
- Returning `default` on failure is concise but swallows the exception and makes failure indistinguishable from a legitimate default result.
- Giving a reusable wrapper a logger preserves the exception, but the wrapper lacks the caller's specific context unless more information is supplied.
- Returning result-and-error metadata preserves both possibilities, but a container with both fields forces every success to carry an unused error field and every failure to carry an unused result field.
- An `OnError` operation reduces the caller's checking boilerplate and makes the error action explicit, while retaining that imperfect container shape.

```csharp
public sealed record ExecutionResult<T>(T? Result, Exception? Error);

public static ExecutionResult<TOut> MapWithTryCatch<TIn, TOut>(
    this TIn value,
    Func<TIn, TOut> operation)
{
    try { return new(operation(value), null); }
    catch (Exception error) { return new(default, error); }
}

public static T? OnError<T>(
    this ExecutionResult<T> result,
    Action<Exception> handle)
{
    if (result.Error is not null)
        handle(result.Error);

    return result.Result;
}
```

This technique packages unsafe execution and lets the call site provide contextual handling. It is deliberately only a lightweight solution: a representation with mutually exclusive success and failure cases avoids the container's unused and potentially inconsistent fields.

## Unless: conditionally perform an effect

`Unless` executes an action only when its predicate is false:

```csharp
public static void Unless<T>(
    this T value,
    Func<T, bool> condition,
    Action<T> action)
{
    if (!condition(value))
        action(value);
}
```

The skipped branch has no computed result, so this form uses `Action<T>` and returns `void`. It can guard an effect such as using optional coordinates: the action runs unless the coordinates are absent.

## Non-mutating updates to lazy sequences

LINQ projections can describe replacements without changing the source enumerable:

```csharp
public static IEnumerable<T> ReplaceAt<T>(
    this IEnumerable<T> source,
    int index,
    T replacement) =>
    source.Select((item, currentIndex) =>
        currentIndex == index ? replacement : item);

public static IEnumerable<T> ReplaceAt<T>(
    this IEnumerable<T> source,
    int index,
    Func<T, T> replace) =>
    source.Select((item, currentIndex) =>
        currentIndex == index ? replace(item) : item);

public static IEnumerable<T> ReplaceWhen<T>(
    this IEnumerable<T> source,
    Func<T, bool> shouldReplace,
    Func<T, T> replace) =>
    source.Select(item => shouldReplace(item) ? replace(item) : item);
```

The fixed-value overload substitutes one position. The function overload can derive a replacement from the old item. `ReplaceWhen` updates every item selected by a predicate. Each operation returns a new lazy projection: the source remains unchanged, and the replacements occur when the returned sequence is enumerated.
