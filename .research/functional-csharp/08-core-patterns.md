# Patterns in Functional Programming

Functional programming repeatedly applies a small set of operations to values held inside structures. `Option<A>` represents optionality; `Seq<A>` represents aggregation. Their meanings differ, but the same operation shapes work for both.

## The core operations

| Operation | Function shape              | Purpose                                                               |
| --------- | --------------------------- | --------------------------------------------------------------------- |
| `Pure`    | `A -> F<A>`                 | Lift a regular value into a structure.                                |
| `Map`     | `(F<A>, A -> B) -> F<B>`    | Transform every available inner value while preserving the structure. |
| `Bind`    | `(F<A>, A -> F<B>) -> F<B>` | Run a structure-producing function and flatten the nested result.     |
| `Filter`  | `(F<A>, A -> bool) -> F<A>` | Keep inner values satisfying a predicate.                             |
| `Iter`    | `(F<A>, Action<A>) -> Unit` | Perform a side effect for each available value.                       |

`Option<A>`, `Seq<A>`, and `Fin<A>` supply these operations under these names. `Fin<A>` has no `Filter`, and `Seq<A>` has no `Pure`.

In LINQ terminology, `Map` is `Select`, `Bind` is `SelectMany`, and `Filter` is `Where`. Elsewhere, `Map` may be called `fMap`, `Project`, or `Lift`; `Bind` may be `FlatMap`, `Chain`, `Collect`, or `Then`; and `Iter` may be `ForEach`.

## Map: transform while preserving structure

For a sequence, `Map` applies a function lazily to every element:

```csharp
internal static partial class CorePatterns {
    public static Seq<int> Doubled(Seq<int> values) =>
        values.Map(static value => value * 2);
}
```

`Map` and LINQ's `Select` are synonymous for sequences.

For an option, it transforms a present value and propagates absence:

```csharp
internal static partial class CorePatterns {
    public static Option<int> Doubled(Option<int> value) =>
        value.Map(static v => v * 2);
}
```

`Option<A>` can be understood as a container holding zero or one value. This makes its `Map` behavior consistent with sequence mapping: the function runs once for `Some` and never for `None`.

```csharp
internal static partial class CorePatterns {
    public static Option<Risk> RiskOf(Subject subject) =>
        subject.Age.Map(CalculateRiskProfile);
}
```

Optionality propagates automatically. The domain function `CalculateRiskProfile : Age -> Risk` stays unaware of absence; `Map` handles it.

For practical purposes, a type with a suitable, side-effect-free `Map` is a functor: mapping applies the function to inner values and does nothing else. LanguageExt encodes the type constructor as the `F` in `K<F, A>`, and the trait `Functor<F>` declares `Map` over `K<F, A>`. A function generic over `F : Functor<F>` works for `Option` and `Seq`, and `.As()` recovers the concrete type at the edge:

```csharp
internal static partial class CorePatterns {
    public static K<F, int> Lengths<F>(K<F, string> values)
        where F : Functor<F> =>
        values.Map(static value => value.Length);
    public static Option<int> OptionLengths() =>
        Lengths(Some("ab")).As();
    public static Seq<int> SeqLengths() =>
        Lengths(Seq("a", "bb")).As();
}
```

## Iter: isolate effects

`Map` is for value transformations. `Iter` accepts an `Action<A>` and exists specifically to perform effects.

`Iter` runs the action at once and returns `Unit`. Keep the effecting action as small as possible:

```csharp
internal static partial class CorePatterns {
    public static Unit Greet(Option<string> name) =>
        name
            .Map(static value => $"Hello {value}")
            .Iter(Console.WriteLine);
}
```

This keeps formatting pure and limits the side effect to output. A dedicated `Iter` is preferable to overloading `Map` with `Action<A>` because C# overload resolution cannot reliably distinguish `Action<A>` from `Func<A, B>` using the return type.

A pass-through `Do` operation performs a side effect and returns the same elevated value so composition can continue. `Option<A>` and `Seq<A>` supply `Do`, and `Fin<A>` supplies `IfSucc`. Inside an effect chain, the observer is an `IO` step:

```csharp
internal static partial class CorePatterns {
    public static IO<int> Traced(IO<int> effect) =>
        from value in effect
        from _ in IO.lift(() => Console.WriteLine(value))
        select value;
}
```

`Do` and `IfSucc` run only for a value, so a propagated `None` or failure passes every later observer in silence.

## Bind: compose structure-producing functions

`Map` is correct when the supplied function returns a regular value. If the function already returns the same kind of structure, `Map` produces an unwanted nested value:

```text
Option<A> + (A -> Option<B>) + Map  = Option<Option<B>>
```

`Bind` applies the function and removes that extra layer. `Age.From` returns `Fin<Age>`, and `ToOption` converts the rejection into absence at the boundary:

```csharp
internal sealed record InvalidAge() : Expected("age out of range", 1001), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

[ValueObject<int>]
[ValidationError<InvalidAge>]
internal readonly partial struct Age {
    public static Fin<Age> From(int value) => Validate(value, provider: null, out Age item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidAge? validationError, ref int value) {
        if (value is < 0 or >= 120)
            validationError = new InvalidAge();
    }
}
```

`parseInt` returns `Option<int>`, so `Bind` joins the two:

```csharp
internal static partial class CorePatterns {
    public static Option<Age> ParseAge(string input) =>
        parseInt(input).Bind(static value => Age.From(value).ToOption());
}
```

If parsing fails, `Age.From` is skipped. If parsing succeeds but the number is not a valid age, `Age.From` returns `InvalidAge` and `ToOption` maps it to `None`. The composed result remains `Option<Age>`.

For sequences, `Bind` maps a sequence-producing function and concatenates every resulting sequence:

```csharp
internal static partial class CorePatterns {
    public static Seq<Pet> PetsOf(Seq<Neighbor> neighbors) =>
        neighbors.Bind(static neighbor => neighbor.Pets);
}
```

For `Seq<A>`, `Bind` and LINQ's `SelectMany` are the same operation: each source value can produce a sequence, and all produced sequences are flattened into one. `Map` would produce `Seq<Seq<Pet>>`; `Bind` produces one flat `Seq<Pet>`.

A type with suitable `Pure` and `Bind` operations that obey the monad laws is a monad. `MonadLaw<F>.validate()` checks the laws for a type such as `Option`. The laws constrain these operations, but the essential implementation guidance here is that `Pure` must do only the minimum required to introduce the structure. `Pure(value)` converts into `Option<A>`, `Fin<A>`, and `IO<A>`:

```csharp
internal static partial class CorePatterns {
    public static Option<int> PureOption(int value) => Pure(value);
    public static Fin<int> PureFin(int value) => Pure(value);
    public static IO<int> PureIO(int value) => Pure(value);
}
```

`Seq(value)` builds the one-element `Seq<A>`, and `F.Pure(value)` under `F : Applicative<F>` builds the structure for any `F`.

`Map` can be derived from `Bind` and `Pure` by lifting the transform result before binding. A direct `Map` implementation may still be more efficient. Every monad can therefore supply `Map`, but a functor does not necessarily support `Bind`.

## Filter: filter inside the structure

For an option, `Filter` preserves a present value only when it satisfies the predicate, and LINQ `where` still works:

```csharp
internal static partial class CorePatterns {
    public static Option<int> ToNatural(string input) =>
        parseInt(input).Filter(static value => value >= 0);
}
```

Both parse failure and predicate failure become `None`; a valid non-negative integer remains `Some`.

## Combining Option and Seq

An option can be promoted to a zero-or-one-element sequence with `ToSeq`:

```csharp
internal static partial class CorePatterns {
    public static Seq<Age> AsSequence(Option<Age> age) => age.ToSeq();
}
```

This enables practical combinations of the two structures. `Choose` maps each element to an `Option<B>` and keeps the `Some` values in one pass, and `Somes` does the same for a `Seq<Option<A>>` that already exists:

```csharp
internal static partial class CorePatterns {
    public static Seq<Age> StatedAges(Seq<Subject> population) => population.Choose(static subject => subject.Age);
    public static Seq<Age> Disclosed(Seq<Option<Age>> ages) => ages.Somes();
    public static Seq<Age> Flattened(Option<Seq<Age>> ages) => ages.ToSeq().Flatten();
}
```

Each yields a plain `Seq<A>` and discards `None`. `Flattened` turns the other direction, an `Option<Seq<A>>`, into a `Seq<A>`. These conversions are valid because an option can always be represented as a sequence, though `Option` and `Seq` normally serve different purposes and are combined only when flattening between them is useful.

```csharp
internal static partial class CorePatterns {
    public static int TotalAge(Seq<Subject> population) =>
        StatedAges(population).Fold(0, static (total, age) => total + age);
}
```

Here each `Some(age)` becomes one sequence element and each `None` becomes none, so a later aggregate can operate only on disclosed ages. `age` converts to `int` through the generated implicit conversion, so the sum reads the key.

## Regular and elevated values

A regular value has type `A`. An elevated value has type `F<A>`, where `F` adds a computational effect. This kind of effect describes what the abstraction contributes; it is distinct from a side effect:
- `Option<A>` adds possible absence.
- `Seq<A>` adds aggregation.
- `Func<A>` adds deferred evaluation.
- `Fin<A>` adds expected failure with a reason.
- `IO<A>` adds a deferred side effect with a failure channel.

Functions can be classified by how they move between these levels:

| Direction            | Shape                 | Examples                           |
| -------------------- | --------------------- | ---------------------------------- |
| Regular to regular   | `A -> B`              | `int -> string` transformations    |
| Elevated to elevated | `(F<A>, ...) -> F<B>` | `Map`, `Bind`, `Filter`, ordering  |
| Regular to elevated  | `A -> F<B>`           | `parseInt`, `Pure`                 |
| Elevated to regular  | `F<A> -> B`           | `Match`, `Count`, `Fold`, `IfNone` |

These shapes describe the net movement between levels; an elevated-to-elevated operation may also take a function, predicate, or other arguments. `Match` and `RunSafe` belong to the host, and domain functions never run an effect.

There is not always an obvious general operation from elevated to regular. An `Option<A>` may be empty, and a sequence may have zero or many values, so neither can always be reduced to one `A`.

The practical distinction between `Map` and `Bind` follows directly:
- Use `Map` with `A -> B`.
- Use `Bind` with `A -> F<B>`.
- Using `Map` with `A -> F<B>` produces `F<F<B>>`.

Prefer staying within one useful abstraction across a pipeline:

```csharp
internal static partial class CorePatterns {
    public static Seq<string> Percentages() =>
        toSeq(Range(1, 100))
            .Filter(static value => value % 20 == 0)
            .Rev()
            .Map(static value => string.Create(CultureInfo.InvariantCulture, $"{value}%"));
    // ["100%", "80%", "60%", "40%", "20%"]
}
```

After `toSeq` elevates the range into `Seq<int>`, every remaining operation stays within `Seq`. Working only with regular values tends to reintroduce low-level loops and absence checks; stacking abstractions too deeply produces types such as `A<B<C<D<T>>>>`, where the underlying value becomes difficult to reach and compose with. When two abstractions stack, a transformer such as `OptionT<IO, A>` keeps one `Map` and one `Bind` over the pair.
