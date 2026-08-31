# Patterns in Functional Programming

Functional programming repeatedly applies a small set of operations to values held inside structures. `Option<T>` represents optionality; `IEnumerable<T>` represents aggregation. Their meanings differ, but the same operation shapes work for both.

## The core operations

| Operation | Function shape                 | Purpose                                                               |
| --------- | ------------------------------ | --------------------------------------------------------------------- |
| `Return`  | `T -> C<T>`                    | Lift a regular value into a structure.                                |
| `Map`     | `(C<T>, T -> R) -> C<R>`       | Transform every available inner value while preserving the structure. |
| `Bind`    | `(C<T>, T -> C<R>) -> C<R>`    | Run a structure-producing function and flatten the nested result.     |
| `Where`   | `(C<T>, T -> bool) -> C<T>`    | Keep inner values satisfying a predicate.                             |
| `ForEach` | `(C<T>, Action<T>) -> C<Unit>` | Perform a side effect for each available value.                       |

In LINQ terminology, `Map` is `Select`, `Bind` is `SelectMany`, and `Where` keeps its name. Elsewhere, `Map` may be called `fMap`, `Project`, or `Lift`; `Bind` may be `FlatMap`, `Chain`, `Collect`, or `Then`; `Where` may be `Filter`; and `ForEach` may be `Iter`.

## Map: transform while preserving structure

For a sequence, `Map` applies a function lazily to every element:

```csharp
public static IEnumerable<R> Map<T, R>(
    this IEnumerable<T> values,
    Func<T, R> transform) =>
    values.Select(transform);
```

`Map` and LINQ's `Select` are synonymous for sequences. Defining `Map` through `Select` can also benefit from LINQ's optimizations for particular `IEnumerable<T>` implementations.

For an option, it transforms a present value and propagates absence:

```csharp
public static Option<R> Map<T, R>(
    this Option<T> option,
    Func<T, R> transform) =>
    option.Match(
        () => None,
        value => Some(transform(value)));
```

`Option<T>` can be understood as a container holding zero or one value. This makes its `Map` behavior consistent with sequence mapping: the function runs once for `Some` and never for `None`.

```csharp
Option<Risk> RiskOf(Subject subject) =>
    subject.Age.Map(CalculateRiskProfile);
```

Optionality propagates automatically. The domain function `CalculateRiskProfile : Age -> Risk` stays unaware of absence; `Map` handles it.

For practical purposes, a type with a suitable, side-effect-free `Map` is a functor: mapping applies the function to inner values and does nothing else. C# cannot directly encode the general `C<_>` shape as an interface because a type parameter cannot stand for a generic type constructor such as `C<>`. Functor behavior is therefore expressed as a pattern rather than one universal interface.

## ForEach: isolate effects

`Map` is for value transformations. `ForEach` accepts an `Action<T>` and exists specifically to perform effects.

```csharp
public static Option<Unit> ForEach<T>(
    this Option<T> option,
    Action<T> action) =>
    Map(option, action.ToFunc());

public static IEnumerable<Unit> ForEach<T>(
    this IEnumerable<T> values,
    Action<T> action) =>
    values.Map(action.ToFunc()).ToImmutableList();
```

The sequence implementation must force enumeration; otherwise deferred execution means the effects may never run. Keep the effecting action as small as possible:

```csharp
option
    .Map(name => $"Hello {name}")
    .ForEach(Console.WriteLine);
```

This keeps formatting pure and limits the side effect to output. A dedicated `ForEach` is preferable to overloading `Map` with `Action<T>` because C# overload resolution cannot reliably distinguish `Action<T>` from `Func<T, R>` using the return type.

## Bind: compose structure-producing functions

`Map` is correct when the supplied function returns a regular value. If the function already returns the same kind of structure, `Map` produces an unwanted nested value:

```text
Option<T> + (T -> Option<R>) + Map  = Option<Option<R>>
```

`Bind` applies the function and removes that extra layer:

```csharp
public static Option<R> Bind<T, R>(
    this Option<T> option,
    Func<T, Option<R>> next) =>
    option.Match(
        () => None,
        value => next(value));
```

```csharp
Option<Age> ParseAge(string input) =>
    Int.Parse(input).Bind(Age.Of);
```

If parsing fails, `Age.Of` is skipped. If parsing succeeds but the number is not a valid age, `Age.Of` returns `None`. The composed result remains `Option<Age>`.

For sequences, `Bind` maps a sequence-producing function and concatenates every resulting sequence:

```csharp
public static IEnumerable<R> Bind<T, R>(
    this IEnumerable<T> values,
    Func<T, IEnumerable<R>> next)
{
    foreach (var value in values)
        foreach (var result in next(value))
            yield return result;
}
```

For `IEnumerable<T>`, `Bind` and LINQ's `SelectMany` are the same operation: each source value can produce a sequence, and all produced sequences are flattened into one.

```csharp
IEnumerable<Pet> pets = neighbors.Bind(neighbor => neighbor.Pets);
```

`Map` would produce `IEnumerable<IEnumerable<Pet>>`; `Bind` produces one flat `IEnumerable<Pet>`.

A type with suitable `Return` and `Bind` operations that obey the monad laws is a monad. The laws constrain these operations, but the essential implementation guidance here is that `Return` must do only the minimum required to introduce the structure. For `Option<T>`, `Some` serves as `Return`; for `IEnumerable<T>`, a singleton call to `List` does:

```csharp
public static IEnumerable<T> List<T>(params T[] values) =>
    values.ToImmutableList();
```

For the formal `Return : T -> C<T>` operation, `List` is called with one value; the `params` form also provides convenient empty and multi-value initialization.

`Map` can be derived from `Bind` and `Return` by lifting the transform result before binding. A direct `Map` implementation may still be more efficient. Every monad can therefore supply `Map`, but a functor does not necessarily support `Bind`.

## Where: filter inside the structure

For an option, `Where` preserves a present value only when it satisfies the predicate:

```csharp
public static Option<T> Where<T>(
    this Option<T> option,
    Func<T, bool> predicate) =>
    option.Match(
        () => None,
        value => predicate(value) ? option : None);
```

```csharp
Option<int> ToNatural(string input) =>
    Int.Parse(input).Where(value => value >= 0);
```

Both parse failure and predicate failure become `None`; a valid non-negative integer remains `Some`.

## Combining Option and IEnumerable

An option can be promoted to a zero-or-one-element sequence:

```csharp
public struct Option<T>
{
    public IEnumerable<T> AsEnumerable()
    {
        if (IsSome) yield return Value;
    }
}
```

This enables practical `Bind` overloads that combine the two structures:

```csharp
public static IEnumerable<R> Bind<T, R>(
    this IEnumerable<T> values,
    Func<T, Option<R>> next) =>
    values.Bind(value => next(value).AsEnumerable());

public static IEnumerable<R> Bind<T, R>(
    this Option<T> option,
    Func<T, IEnumerable<R>> next) =>
    option.AsEnumerable().Bind(next);
```

The first overload turns `IEnumerable<Option<R>>` into `IEnumerable<R>`, naturally discarding `None`. The second turns `Option<IEnumerable<R>>` into `IEnumerable<R>`. These conversions are valid because an option can always be represented as a sequence, though `Option` and `IEnumerable` normally serve different purposes and are combined only when flattening between them is useful.

```csharp
IEnumerable<Age> statedAges = population.Bind(subject => subject.Age);
double averageAge = statedAges.Map(age => age.Value).Average();
```

Here each `Some(age)` becomes one sequence element and each `None` becomes none, so a later aggregate can operate only on disclosed ages.

## Regular and elevated values

A regular value has type `T`. An elevated value has type `A<T>`, where `A` adds a computational effect. This kind of effect describes what the abstraction contributes; it is distinct from a side effect:
- `Option<T>` adds possible absence.
- `IEnumerable<T>` adds aggregation.
- `Func<T>` adds deferred evaluation.
- `Task<T>` adds asynchrony.

Functions can be classified by how they move between these levels:

| Direction            | Shape                 | Examples                           |
| -------------------- | --------------------- | ---------------------------------- |
| Regular to regular   | `T -> R`              | `int -> string` transformations    |
| Elevated to elevated | `(A<T>, ...) -> A<R>` | `Map`, `Bind`, `Where`, ordering   |
| Regular to elevated  | `T -> A<R>`           | `Int.Parse`, `Return`              |
| Elevated to regular  | `A<T> -> R`           | `Match`, `Count`, `Sum`, `Average` |

These shapes describe the net movement between levels; an elevated-to-elevated operation may also take a function, predicate, or other arguments.

There is not always an obvious general operation from elevated to regular. An `Option<T>` may be empty, and a sequence may have zero or many values, so neither can always be reduced to one `T`.

The practical distinction between `Map` and `Bind` follows directly:
- Use `Map` with `T -> R`.
- Use `Bind` with `T -> A<R>`.
- Using `Map` with `T -> A<R>` produces `A<A<R>>`.

Prefer staying within one useful abstraction across a pipeline:

```csharp
IEnumerable<string> percentages = Enumerable
    .Range(1, 100)
    .Where(value => value % 20 == 0)
    .OrderBy(value => -value)
    .Map(value => $"{value}%");
// ["100%", "80%", "60%", "40%", "20%"]
```

After `Range` elevates the values into `IEnumerable<int>`, every remaining operation stays within `IEnumerable`. Working only with regular values tends to reintroduce low-level loops and absence checks; stacking abstractions too deeply produces types such as `A<B<C<D<T>>>>`, where the underlying value becomes difficult to reach and compose with.
