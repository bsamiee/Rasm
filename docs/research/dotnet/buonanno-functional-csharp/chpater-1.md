# Introducing Functional Programming

## The functional model

Functional programming is a style built on two commitments:
1. **Treat functions as values.** A function can be assigned to a variable, passed as an argument, returned from another function, or stored in a collection.
2. **Avoid state mutation.** Once created, an object should not change, variables should not be reassigned, and transformations should produce new values instead of destroying prior ones.

These commitments reinforce each other. Functions express small transformations, while immutable inputs make those transformations easier to reason about, compose, test, and run concurrently.

```csharp
Func<int, int> triple = x => x * 3;

var source = Enumerable.Range(1, 3);
var result = source.Select(triple); // 3, 6, 9
```

`Select` receives behavior as data and returns a new sequence. The original sequence is unchanged.

## Prefer transformations over destructive updates

An in-place update destroys the prior value:

```csharp
var values = new List<int> { 7, 6, 1 };
values.Sort(); // values is now 1, 6, 7
```

A functional alternative preserves it:

```csharp
var values = new[] { 7, 6, 1 };
var sorted = values.OrderBy(x => x); // values remains 7, 6, 1
var odd = values.Where(x => x % 2 == 1); // 7, 1
```

This distinction becomes critical under concurrency. Two readers can safely observe the same stable value. If one concurrent operation reorders a shared list while another sums it, the reader can observe an inconsistent traversal and produce an unpredictable result. Producing a separate ordered view removes that interference.

Avoiding mutation eliminates complexities caused by mutable state. A functional design often makes later concurrency substantially easier because concurrent reads of stable values do not create inconsistencies.

Functional and object-oriented design are not opposites. Modularity, separation of concerns, layering, and loose coupling apply whether a component is a function or a class. The practical conflict is usually between functional transformations and imperative method bodies that mutate shared state or use explicit control flow.

## C# as a functional language

C# supports functions as first-class values well through delegates and lambdas. Garbage collection makes non-destructive updates practical because superseded versions can be reclaimed. C#'s main weakness is that mutation is the default: fields and variables must be explicitly constrained, user-defined immutable types require effort, and the standard collections are mutable even though an immutable collections library is available.

LINQ is the clearest built-in functional model:
- `Select` maps each element through a function.
- `Where` filters through a predicate.
- `OrderBy` and `OrderByDescending` produce ordered sequences from key selectors.
- These operators accept functions and return new sequences instead of modifying their inputs.

Useful language features reduce the ceremony around functional code:
- `using static` makes libraries of static functions concise to consume, but excessive use can pollute the namespace.
- Getter-only auto-properties have a compiler-generated readonly backing field and can be assigned only inline or in the constructor, making simple immutable types easier to define.
- Expression-bodied members keep small functions readable and composable.
- Local functions keep single-use helpers near their caller.
- Named tuples carry temporary intermediate structures without inventing domain types that have no independent meaning.

## Think in function signatures

A mathematical function maps each value in a domain to a value in a codomain. In a statically typed program, types represent those sets:

```text
char -> char
Person -> Greeting
(T1, T2) -> R
```

The input and output types form the function's interface and contract. This perspective directs attention to what information enters and what value must come out.

A mathematical function's result is determined exclusively by its input. A C# method, delegate, or lambda only represents a function; that representation does not guarantee the same property. It may capture context, read mutable state, or perform effects even when its visible signature does not reveal those dependencies.

C# can represent functions in several ways:
- **Methods** are the conventional representation and participate in class and interface design. An instance method can be understood as also taking the current instance as an implicit argument.
- **Delegates** are strongly typed function pointers. `Func<T, R>` represents value-returning functions; `Action<T>` represents operations with no return value.
- **Lambdas** define short functions inline and are converted to a compatible delegate type.
- **Dictionaries** directly store arbitrary mappings whose associations cannot be computed. The same representation can retain results of expensive computations instead of recomputing them.

Prefer the general `Func` and `Action` families when only the signature matters. A custom delegate can still be worthwhile when its name conveys domain intent more clearly than a structural type such as `Func<T, bool>`.

### Arity, tuples, and closures

Arity is the number of arguments a function accepts: nullary, unary, binary, and so on. Any multi-argument function can be viewed as a unary function over a tuple of its arguments.

A closure combines a lambda with the context in which it was declared. The delegate's declared signature may remain unary, but the computation can also depend on captured context:

```csharp
var days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

IEnumerable<DayOfWeek> DaysStartingWith(string pattern)
    => days.Where(day => day.ToString().StartsWith(pattern));

var weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
```

The predicate supplied to `Where` has the signature `DayOfWeek -> bool`, yet it depends on both `day` and the captured `pattern`. Its unary interface and its two actual inputs are both valid ways to view it.

## Higher-order functions

A higher-order function accepts a function, returns a function, or does both. This is the main capability unlocked by first-class functions.

### Delegate part of an algorithm

A higher-order function can own stable control flow while the caller supplies the varying rule:

```csharp
public static IEnumerable<T> Where<T>(
    this IEnumerable<T> sequence,
    Func<T, bool> predicate)
{
    foreach (T item in sequence)
        if (predicate(item))
            yield return item;
}
```

The function owns iteration; the caller owns the inclusion criterion. This separates concerns that would otherwise be interleaved. The same shape supports:
- **Iterated execution:** invoke a selector, predicate, or comparison for each relevant element.
- **Conditional execution:** invoke a callback only when needed, such as computing a value after a cache miss.
- **Inversion of control:** the caller chooses what behavior to supply; the higher-order function chooses when to run it.

When optional work may be expensive, accept it as a function so it is evaluated only when needed:

```csharp
class Cache<T> where T : class
{
    public T Get(Guid id) => // look up the cached value

    public T Get(Guid id, Func<T> onMiss)
        => Get(id) ?? onMiss();
}
```

### Adapt an existing function

An adapter returns a new function with a more useful interface while delegating to the original:

```csharp
static Func<T2, T1, R> SwapArgs<T1, T2, R>(
    this Func<T1, T2, R> function)
    => (second, first) => function(first, second);
```

Function interfaces are therefore not fixed at the call site. Small adapters can reshape them without modifying the underlying implementation.

### Create specialized functions

A function factory converts configuration data into behavior:

```csharp
Func<int, bool> IsMod(int divisor)
    => value => value % divisor == 0;

var multiplesOfThree = Enumerable.Range(1, 20)
    .Where(IsMod(3));
```

The factory centralizes a general rule and produces readable, reusable specializations.

## Encapsulate resource lifecycles

Setup, body, and teardown form another useful higher-order pattern. Parameterize the changing body while keeping resource management in one place:

```csharp
public static R Using<TDisposable, R>(
    TDisposable resource,
    Func<TDisposable, R> body)
    where TDisposable : IDisposable
{
    using (resource)
        return body(resource);
}

public static R Connect<R>(
    string connectionString,
    Func<IDbConnection, R> body)
    => Using(
        new SqlConnection(connectionString),
        connection =>
        {
            connection.Open();
            return body(connection);
        });
```

Database operations can now state only their domain-specific work. Connection acquisition, opening, and disposal remain centralized. Turning the lifecycle into a value-returning expression also allows expression-bodied methods and further composition.

The synchronous connection body demonstrates the abstraction without the extra complexity of asynchrony. Real I/O operations should normally be performed asynchronously.

This technique becomes more valuable as lifecycle logic grows more intricate or is reused more widely. It provides:
- less duplication;
- a clear boundary between resource management and domain behavior;
- concise callers that expose their actual intent;
- guaranteed disposal through the underlying `using` semantics, including exceptional exits.

## Judgment and tradeoffs

Higher-order functions add callback frames. The performance cost is usually negligible, but debugging call stacks can become less direct. Excessive abstraction can also obscure behavior.

Use higher-order functions when they separate otherwise interleaved logic, remove meaningful duplication, adapt an interface, or create reusable behavior. Keep lambdas short, choose clear names, and format nested callbacks so control flow remains visible.

Functional techniques are tools, not a prohibition on objects. Their main benefits are cleaner, more concise, maintainable, expressive, robust, readable, and testable code; better support for concurrency; and a second problem-solving perspective alongside object-oriented design.
