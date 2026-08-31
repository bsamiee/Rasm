# Currying and Partial Application

## Core distinction

Currying changes a function's shape. Invoking some stages specializes the curried function; invoking every stage completes the application. Partial application specializes the original function directly.

- **Currying** transforms a function of `N` arguments into a chain of `N` unary functions. Each call accepts exactly one argument and returns the next function; the final call returns the result.
- **Partial application** supplies fewer than all of the original arguments at once and returns a function for the arguments that remain. In the C# helpers shown here, it fixes a leading group of arguments, and the returned function may still accept several arguments. Supplying every argument is full application and produces the result.

For a two-argument function:

```csharp
Func<decimal, decimal, decimal> add = (x, y) => x + y;

// Ordinary shape: (decimal, decimal) -> decimal
decimal total = add(100, 200);

// Curried shape: decimal -> (decimal -> decimal)
Func<decimal, Func<decimal, decimal>> curriedAdd =
    x => y => add(x, y);

Func<decimal, decimal> add100 = curriedAdd(100);
decimal first = add100(200);  // 300
decimal second = add100(900); // 1000
```

The returned function retains each supplied value but does not invoke the original function until the remaining arguments arrive. A single general implementation can therefore produce many small, reusable specializations.

## Building families of specialized functions

Consider one configurable parser:

```csharp
IEnumerable<Book> ParseBooks(
    bool skipHeader,
    string lineBreak,
    string fieldDelimiter,
    string fileName) =>
    File.ReadAllText(fileName)
        .Split(lineBreak)
        .Skip(skipHeader ? 1 : 0)
        .Select(line => line.Split(fieldDelimiter))
        .Select(fields => new Book
        {
            Title = fields[0],
            Author = fields[1],
            PublicationDate = fields[2]
        });
```

Currying turns its four-argument shape into:

```text
bool -> string -> string -> string -> IEnumerable<Book>
```

Each stage can be retained and branched:

```csharp
Func<bool, string, string, string, IEnumerable<Book>> parseBooks = ParseBooks;
var curried = parseBooks.Curry();

var parseWithHeader = curried(true);
var parseWindowsWithHeader = parseWithHeader(Environment.NewLine);

var parseWindowsComma = parseWindowsWithHeader(",");
var parseWindowsPipe = parseWindowsWithHeader("|");

IEnumerable<Book> books1 = parseWindowsComma("books.csv");
IEnumerable<Book> books2 = parseWindowsPipe("books2.csv");

IEnumerable<Book> sameBooks =
    curried(true)(Environment.NewLine)(",")("books.csv");
```

The same principle can specialize a logger by fixing its `LogLevel`. The resulting `logInfo`, `logWarning`, and `logError` functions each need only a message and can be passed wherever that focused behavior is required.

Parameter order determines which specializations are convenient. Put stable configuration arguments first and the frequently changing input last when the goal is a reusable function whose final call accepts only the varying value.

## Currying in C#

C# has no built-in automatic currying. There are three practical forms.

### Define a function as curried from the start

```csharp
Func<decimal, Func<decimal, decimal>> add =
    x => y => x + y;
```

This is compact when the function will always be consumed one argument at a time.

### Use a reusable static helper

```csharp
public static class F
{
    public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(
        Func<T1, T2, TResult> function) =>
        x => y => function(x, y);
}

var add = F.Curry((decimal x, decimal y) => x + y);
```

Explicit lambda parameter types may be needed because the compiler does not always infer the delegate's generic arguments at this call site.

### Use extension methods

```csharp
public static class FunctionalExtensions
{
    public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(
        this Func<T1, T2, TResult> function) =>
        x => y => function(x, y);

    public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, TResult> function) =>
        x => y => z => function(x, y, z);

    public static Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>>
        Curry<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function) =>
        x => y => z => a => function(x, y, z, a);
}
```

Give the source function an explicit `Func<...>` delegate type so the extension receiver and its generic types resolve reliably:

```csharp
Func<decimal, decimal, decimal> add = (x, y) => x + y;
var curriedAdd = add.Curry();
var add10 = curriedAdd(10);
decimal answer = add10(100); // 110
```

Each supported arity needs its own `Curry` overload. Higher arities repeat the same nesting pattern and increase the helper's boilerplate.

## Curried functions in higher-order pipelines

Currying can turn general operations into unary functions suitable for mapping or composition. For noncommutative operations, choose parameter order deliberately: the first parameter is the one fixed first, while the last one is typically the pipeline value.

```csharp
public static class ValueExtensions
{
    public static TResult Map<T, TResult>(
        this T value,
        Func<T, TResult> transform) =>
        transform(value);
}

Func<decimal, decimal, decimal> addBase = (fixedValue, input) => input + fixedValue;
Func<decimal, decimal, decimal> subtractBase = (fixedValue, input) => input - fixedValue;
Func<decimal, decimal, decimal> multiplyBase = (fixedValue, input) => input * fixedValue;
Func<decimal, decimal, decimal> divideBase = (fixedValue, input) => input / fixedValue;

var add = addBase.Curry();
var subtract = subtractBase.Curry();
var multiply = multiplyBase.Curry();
var divide = divideBase.Curry();

decimal CelsiusToFahrenheit(decimal value) => value
    .Map(multiply(9))
    .Map(divide(5))
    .Map(add(32));

decimal FahrenheitToCelsius(decimal value) => value
    .Map(subtract(32))
    .Map(multiply(5))
    .Map(divide(9));
```

## Partial application in C#

Partial application avoids unnecessary unary stages when several arguments should always be fixed together. It specializes the original multi-argument function directly; it does not first require the function to be curried:

```csharp
var parseLinuxComma = parseBooks.Partial(false, "\n", ",");
var parseWindows = parseBooks.Partial(true, Environment.NewLine);

var parseWindowsComma = parseWindows.Partial(",");
var parseWindowsPipe = parseWindows.Partial("|");
```

C# requires an overload for every supported combination of original arity and bound-argument count:

```csharp
public static class PartialApplicationExtensions
{
    // Two arguments become one.
    public static Func<T2, TResult> Partial<T1, T2, TResult>(
        this Func<T1, T2, TResult> function,
        T1 one) =>
        two => function(one, two);

    // Four arguments become two.
    public static Func<T3, T4, TResult> Partial<T1, T2, T3, T4, TResult>(
        this Func<T1, T2, T3, T4, TResult> function,
        T1 one,
        T2 two) =>
        (three, four) => function(one, two, three, four);

    // Four arguments become one.
    public static Func<T4, TResult> Partial<T1, T2, T3, T4, TResult>(
        this Func<T1, T2, T3, T4, TResult> function,
        T1 one,
        T2 two,
        T3 three) =>
        four => function(one, two, three, four);
}
```

Add only the overloads a codebase actually uses, or provide a deliberately bounded set. Attempting to cover every arity and binding combination creates substantial infrastructure.

## Choosing whether to use them

These techniques are useful when they:
- eliminate near-duplicate specialized functions;
- expose reusable intermediate configurations;
- produce unary functions that fit higher-order APIs;
- keep one general implementation behind many focused call sites.

Their costs are specific to C#:
- no native currying or general partial-application mechanism;
- `Func` conversion and occasional explicit type annotations;
- one curry helper per arity;
- one partial helper per arity and bound-argument combination;
- nested delegate types that become difficult to read at higher arities.

Use them when the resulting specialized functions simplify real call sites. If the helper machinery is larger or less readable than the duplication it removes, ordinary functions are the clearer choice.
