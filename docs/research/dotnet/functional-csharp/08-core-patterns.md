# [CORE_PATTERNS]

Functional programming applies common operations to values in contexts. `Option<A>` represents optionality; `Seq<A>` represents a sequence of values. Many operations have the same signatures for both.

## [01]-[CORE_OPERATIONS]

| [INDEX] | [OPERATION] | [SIGNATURE]                 | [PURPOSE]                                                      |
| :-----: | :---------- | :-------------------------- | :------------------------------------------------------------- |
|  [01]   | `Pure`      | `A -> F<A>`                 | Lift a plain value into `F<A>`.                                |
|  [02]   | `Map`       | `(F<A>, A -> B) -> F<B>`    | Apply a function to each value in `F<A>` while preserving `F`. |
|  [03]   | `Bind`      | `(F<A>, A -> F<B>) -> F<B>` | Apply `A -> F<B>` and flatten the nested result.               |
|  [04]   | `Filter`    | `(F<A>, A -> bool) -> F<A>` | Keep values in `F<A>` that satisfy a predicate.                |
|  [05]   | `Iter`      | `(F<A>, Action<A>) -> Unit` | Perform a side effect for each value in `F<A>`.                |

`Option<A>`, `Seq<A>`, and `Fin<A>` supply these operations under these names. `Fin<A>` has no `Filter`, and `Seq<A>` has no `Pure`.

In LINQ terminology, `Map` is `Select`, `Bind` is `SelectMany`, and `Filter` is `Where`. Other libraries name `Map` as `fMap`, `Project`, or `Lift`; `Bind` as `FlatMap`, `Chain`, `Collect`, or `Then`; and `Iter` as `ForEach`.

## [02]-[MAP]

For a sequence, `Map` applies a function lazily to every element:

```csharp
internal static partial class CorePatterns {
    public static Seq<int> Doubled(Seq<int> values) =>
        values.Map(static value => value * 2);
}
```

For an option, it transforms a present value and propagates absence:

```csharp
internal static partial class CorePatterns {
    public static Option<int> Doubled(Option<int> value) =>
        value.Map(static v => v * 2);
}
```

`Option<A>` contains zero or one value. Its `Map` applies the function once for `Some` and never for `None`.

```csharp
internal static partial class CorePatterns {
    public static Option<Risk> RiskOf(Subject subject) =>
        subject.Age.Map(CalculateRiskProfile);
}
```

The domain function `CalculateRiskProfile : Age -> Risk` stays unaware of absence.

A type whose side-effect-free `Map` obeys the functor laws is a functor. `Map` applies the function to values in the context while preserving that context. LanguageExt encodes the type constructor as the `F` in `K<F, A>`, and the trait `Functor<F>` declares `Map` over `K<F, A>`. A function generic over `F : Functor<F>` works for `Option` and `Seq`, and `.As()` recovers the concrete type at the boundary:

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

## [03]-[ITER]

`Map` transforms values. `Iter` performs side effects through an `Action<A>`.

`Iter` runs the action immediately and returns `Unit`. Limit the action to one side effect:

```csharp
internal static partial class CorePatterns {
    public static Unit Greet(Option<string> name) =>
        name
            .Map(static value => $"Hello {value}")
            .Iter(Console.WriteLine);
}
```

Formatting remains pure, and only output has a side effect. `Iter` needs a separate name because C# overload resolution cannot reliably distinguish `Action<A>` from `Func<A, B>` by return type.

`Do` performs a side effect and returns its input `F<A>`, which lets composition continue. `Option<A>` and `Seq<A>` supply `Do`, and `Fin<A>` supplies `IfSucc`. In an `IO` computation, the side effect is an `IO` step:

```csharp
internal static partial class CorePatterns {
    public static IO<int> Traced(IO<int> effect) =>
        from value in effect
        from _ in IO.lift(() => Console.WriteLine(value))
        select value;
}
```

`Do` and `IfSucc` run only for a value. Later side effects do not run after `None` or a failure.

## [04]-[BIND]

`Map` takes a function `A -> B`. With `A -> F<B>`, it produces a nested value:

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

`parseInt` returns `Option<int>`. `Bind` composes parsing with validation:

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

For `Seq<A>`, each source value can produce a sequence, and `Bind` flattens all produced sequences into one. `Map` produces `Seq<Seq<Pet>>`; `Bind` produces `Seq<Pet>`.

A type whose `Pure` and `Bind` obey the monad laws is a monad. `MonadLaw<F>.validate()` checks these laws for types such as `Option`. `Pure` only constructs `F<A>` from `A`. `Pure(value)` converts into `Option<A>`, `Fin<A>`, and `IO<A>`:

```csharp
internal static partial class CorePatterns {
    public static Option<int> PureOption(int value) => Pure(value);
    public static Fin<int> PureFin(int value) => Pure(value);
    public static IO<int> PureIO(int value) => Pure(value);
}
```

`Seq(value)` builds the one-element `Seq<A>`, and `F.Pure(value)` under `F : Applicative<F>` builds the structure for any `F`.

`Map` can be derived from `Bind` and `Pure` by lifting the transform result before binding. A direct `Map` implementation can be more efficient. Every monad supplies `Map`, but not every functor supplies `Bind`.

## [05]-[FILTER]

For an option, `Filter` preserves a present value only when it satisfies the predicate. LINQ `where` also works:

```csharp
internal static partial class CorePatterns {
    public static Option<int> ToNatural(string input) =>
        parseInt(input).Filter(static value => value >= 0);
}
```

Both parse failure and predicate failure become `None`; a valid non-negative integer remains `Some`.

## [06]-[OPTION_AND_SEQ]

`ToSeq` converts an option to a zero-or-one-element sequence:

```csharp
internal static partial class CorePatterns {
    public static Seq<Age> AsSequence(Option<Age> age) => age.ToSeq();
}
```

`Choose` maps each element to an `Option<B>` and keeps the `Some` values in one pass, and `Somes` does the same for a `Seq<Option<A>>` that already exists:

```csharp
internal static partial class CorePatterns {
    public static Seq<Age> StatedAges(Seq<Subject> population) => population.Choose(static subject => subject.Age);
    public static Seq<Age> Disclosed(Seq<Option<Age>> ages) => ages.Somes();
    public static Seq<Age> Flattened(Option<Seq<Age>> ages) => ages.ToSeq().Flatten();
}
```

`Flattened` converts `Option<Seq<A>>` to `Seq<A>`. Use these conversions when a pipeline must flatten between `Option` and `Seq`. The two types otherwise serve different purposes.

```csharp
internal static partial class CorePatterns {
    public static int TotalAge(Seq<Subject> population) =>
        StatedAges(population).Fold(0, static (total, age) => total + age);
}
```

Each `Some(age)` becomes one sequence element, and each `None` contributes no element. The aggregate operates only on disclosed ages. The generated implicit conversion converts `age` to `int`, the sum uses the underlying integer.

## [07]-[VALUES_IN_CONTEXT]

A plain value has type `A`. A value in a context has type `F<A>`, where the type constructor `F` supplies a computational effect. A computational effect is distinct from a side effect:
- `Option<A>` adds possible absence.
- `Seq<A>` adds zero or more values.
- `Func<A>` adds deferred evaluation.
- `Fin<A>` adds expected failure with a reason.
- `IO<A>` adds a deferred side effect with a failure channel.

Input and output types classify these functions:

| [INDEX] | [DESCRIPTION]                            | [SIGNATURE]           | [EXAMPLES]                         |
| :-----: | :--------------------------------------- | :-------------------- | :--------------------------------- |
|  [01]   | Plain value to plain value               | `A -> B`              | `int -> string` transformations    |
|  [02]   | Value in a context to value in a context | `(F<A>, ...) -> F<B>` | `Map`, `Bind`, `Filter`, ordering  |
|  [03]   | Plain value to value in a context        | `A -> F<B>`           | `parseInt`, `Pure`                 |
|  [04]   | Value in a context to plain value        | `F<A> -> B`           | `Match`, `Count`, `Fold`, `IfNone` |

An `F<A> -> F<B>` operation can also take a function, predicate, or other arguments. `Match` and `RunSafe` belong to the host, and domain functions never run an effect.

No general operation extracts one `A` from every `F<A>`. An `Option<A>` can be empty, and a sequence can contain zero or many values.

Prefer keeping a pipeline within one abstraction:

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

After `toSeq` converts the range to `Seq<int>`, all later operations remain in `Seq`. Using only plain values can reintroduce low-level loops and absence checks. Deeply nested contexts can produce types such as `A<B<C<D<T>>>>`, which require traversal of several layers to access and compose the value. A transformer such as `OptionT<IO, A>` provides one `Map` and one `Bind` for the pair.
