# Chapter 3 - Designing Function Signatures and Types

## Type design and function design are one problem

A function signature is a contract. Its input types describe every value the function accepts, and its return type describes every normal outcome callers must handle. Precise types make the contract both more informative and harder to violate.

Arrow notation keeps signatures compact:

| Meaning                     | Arrow notation                        | C# delegate                               |
| --------------------------- | ------------------------------------- | ----------------------------------------- |
| Transform a value           | `int -> string`                       | `Func<int, string>`                       |
| Produce a value             | `() -> string`                        | `Func<string>`                            |
| Consume a value for effects | `int -> ()`                           | `Action<int>`                             |
| Perform an effect           | `() -> ()`                            | `Action`                                  |
| Combine two inputs          | `(int, int) -> int`                   | `Func<int, int, int>`                     |
| Accept a function           | `(string, (IDbConnection -> R)) -> R` | `Func<string, Func<IDbConnection, R>, R>` |

`()` means no input or no returned information. Grouped inputs are treated as a tuple.

### Read behavior from the signature's shape

Generic signatures strongly constrain plausible behavior:

```text
(IEnumerable<T>, (T -> bool)) -> IEnumerable<T>
(IEnumerable<A>, IEnumerable<B>, ((A, B) -> C)) -> IEnumerable<C>
```

The first suggests filtering a sequence with a predicate. The second suggests combining corresponding `A` and `B` values into `C` values. By contrast, `() -> ()` reveals almost nothing about the effect performed. A signature cannot express every semantic detail - `Where` and `TakeWhile` have the same shape - so precision in both types and naming matters.

Functional design usually keeps data and logic distinct: data objects carry inputs and outputs, while functions encode behavior. A constrained type can still own the validation needed to construct it and operations, such as comparison, that protect its hidden representation.

## Make invalid inputs unrepresentable

Primitive types are often broader than the domain. An `int` used as an age admits negative and implausibly large values. Validating inside every consumer duplicates the rule and mixes validation with the consumer's actual calculation.

A custom type can narrow the domain once. `Age` is a value object: the generator supplies the factory methods, equality, comparison, and the conversion to `int`. `InvalidAge` implements `IValidationError<InvalidAge>`, so the validation hook raises the typed `Expected` record. `From` is the smart constructor: it maps the generated `Validate` to `Fin<Age>`:

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

internal enum Risk {
    Low = 0,
    Medium = 1,
}

internal static class Underwriting {
    public static Risk CalculateRiskProfile(Age age) => age < 60 ? Risk.Low : Risk.Medium;
}
```

`Age.From` has the shape `int -> Fin<Age>`. Callers cannot bypass the invariant, and consumers neither repeat validation nor inspect the underlying integer. A consumer that recovers from the failure pattern matches `InvalidAge` through `IsType` or `HasCode`, not a message. `Create` throws when the value is invalid, so it reports a defect in the calling code and not an expected input outcome.

### Honest functions

An honest function always honors its signature. Given a value of the declared input type, it returns a value of the declared output type; it does not return `null` or throw an exception as an intrinsic outcome that the signature fails to describe.

```text
Age -> Risk
```

This is honest when every constructible `Age` produces a `Risk`. By contrast, `int -> Risk` is dishonest if some integers cause validation exceptions. Repair the contract either by narrowing the input to a validated type or by widening the output to represent the possibility of failure.

Honesty is weaker than purity. Honesty asks whether behavior agrees with the signature; purity additionally excludes observable side effects and dependence on mutable state.

### Types as sets

Thinking of types as sets of possible values clarifies design. If `Age` has 120 values and `Gender` has two, `(Age, Gender)` has `120 * 2 = 240` possible values. A tuple or object containing both is a product type: each field adds another dimension to the space of possible states.

`Option<A>` is a union: all `Some(A)` values plus the single `None` value, so a type with `n` values yields an option with `n + 1` values. Counting possible instances helps expose types that admit states the domain does not need. Once leaf values are constrained, they can be composed into larger data objects without reintroducing invalid primitive states.

## Represent no information with `Unit`

`void` is a language special case rather than an ordinary return type. This splits delegates into `Func` and `Action` families and can force duplicate higher-order-function implementations. The same split appears between `Task<T>` and `Task`.

`Unit` is an ordinary type with exactly one value and no information. LanguageExt ships `Unit` with its single value `unit`, and the Prelude function `fun` converts an `Action` into a `Func<Unit>`.

An `Action` overload can adapt and delegate to the one generic implementation:

```csharp
internal static class Timing {
    public static Unit Time(string operation, Action action) => Time<Unit>(operation, fun(action));
    public static T Time<T>(string operation, Func<T> body) {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        T result = body();
        stopwatch.Stop();
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{operation} took {stopwatch.ElapsedMilliseconds}ms"));
        return result;
    }
}
```

Use `void` when an ordinary imperative API performs effects and returns no information. Use `Unit` when an ordinary return value enables uniform functional handling. Returning `Unit` does not make an effectful function pure; it only removes a special-case return shape.

## Represent possible absence with `Option<A>`

Framework lookup APIs demonstrate why hidden absence is unsafe: a missing key may yield `null` from one collection and throw from another, even though both indexers appear to have the shape `key -> value`.

`Option<A>` makes absence part of the result type:

```text
Option<A> = None | Some(A)
```

- `None` contains no value.
- `Some(A)` contains one value.
- `Match` requires behavior for both states and returns one common result type.

```csharp
internal static class Greetings {
    public static string GreetingFor(Option<string> name) =>
        name.Match(
            Some: static value => $"Dear {value.ToUpperInvariant()},",
            None: static () => "Dear Subscriber,");
}
```

`Match` belongs at the host, where a result type becomes a rendered value. Inside the domain, `Bind` and `Map` carry the `Some` case forward.

Changing a required `string` property to `Option<string>` is deliberately breaking: code that treats the property as a `string` stops compiling until it handles absence. This trades latent `NullReferenceException`s for explicit compile-time work.

### The implementation shape in C#

`Option<A>` is one readonly struct. It holds a flag and an inner value, exposes the flag as `IsSome` and `IsNone`, and selects the case through `Match`. The Prelude supplies `Some(x)` and `None`. The implicit conversion from `A` maps `null` to `None`, and `Optional(x)` does the same at a null boundary. `Some(x)` wraps the value as given and is not a null check:

```csharp
internal static class Construction {
    public static Option<string> Present(string value) => Some(value);
    public static Option<string> Absent() => None;
    public static Option<string> AtTheBoundary(string? external) => Optional(external);
    public static Option<string> Lifted(string? external) => external;
}
```

The default inner value in the `None` state is ignored. The essential contract is an empty case, a present case, and a way to handle both cases safely. The abstraction is also commonly called `Maybe`, with cases named `Nothing` and `Just`. Class-hierarchy encodings name the cases `Something<T>` and `Nothing<T>`. The abstraction is the same, and an open hierarchy cannot stop a caller from adding a third case.

## Turn partial functions into total functions

A total function is defined for every value in its declared input domain; a partial function is not. Returning `Option` totalizes a partial computation: return `Some(result)` where the computation is defined and `None` otherwise.

```csharp
internal static class Totality {
    public static Option<int> ParseAge(string text) => parseInt(text);
    public static Option<string> Setting(HashMap<string, string> settings, string key) => settings.Find(key);
    public static Option<Age> AgeOf(HashMap<string, string> settings) =>
        Setting(settings, "age").Bind(ParseAge).Bind(static value => Age.From(value).ToOption());
}
```

`string -> int` hides the undefined parsing cases; `string -> Option<int>` describes every outcome. The Prelude function `parseInt` has that shape. `Find` on `HashMap<K, V>` has the shape `K -> Option<V>`, so a missing key is a value and not an exception. `Bind` chains the two lookups, and `ToOption` on `Fin<Age>` drops the failure reason where the caller has no use for it.

## Nullable reference types as a boundary guard

Nullable reference types are compiler analysis, not a new runtime type. Enable the analysis in the project file:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

The compiler then warns when a non-nullable property may be uninitialized or when `null` is assigned to a non-nullable reference. These are warnings; enabling the feature does not itself change runtime behavior.

Use `?` only when null is a deliberate part of the representation:

```csharp
internal sealed record ExternalMovie {
    public string? Title { get; init; }
    public string? Director { get; init; }
    public IEnumerable<string>? Cast { get; init; }
}
```

Null adds another possible state that every consumer must account for. If an external data source requires nullable values, isolate that representation in the parsing code and convert it to a safer internal shape before passing it through the rest of the system.

## Design rules

- Design the signature early; make inputs and outcomes as specific as possible.
- Prefer constrained domain types to primitives plus repeated validation.
- Map the generated `Validate` to `Fin` through `From` when primitive-to-domain conversion can fail.
- The generator keeps the constructor private, so every value passes through the validation hook.
- Do not explicitly return `null` from functions.
- Reject unexpected `null` at public API inputs; optional parameters are the exceptional case because defaults must be compile-time constants.
- Enable nullable reference types so the compiler exposes accidental nullability before a value enters a pipeline.
- Use `Option<A>` for optional properties, parsing, lookup, and other computations that may legitimately produce no value.
- Use `Match` when a concrete result must be selected from the `None` and `Some` cases.
- Use `Unit` and `fun` to adapt `Action` into `Func`-based higher-order APIs without duplicating their behavior.
