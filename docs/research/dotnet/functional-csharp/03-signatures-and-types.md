<!-- Fully integrated, [04.1] into dotnet-languageext and the rest into dotnet-coding/SKILL.md, the Timing snippet of [03] and the Match snippet of [04] stay as prose rules, and the optional-parameter null exception of [07] is superseded by `Option<A> = default` parameters -->
# [SIGNATURES_AND_TYPES]

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [01]-[TYPE_AND_FUNCTION_DESIGN]

Function signatures are contracts. Input types describe every value the function accepts, and the return type describes every normal outcome callers must handle. Precise types make the contract both more informative and harder to violate.

Arrow notation keeps signatures compact:

| [INDEX] | [PURPOSE]                   | [ARROW_NOTATION]                      | [DELEGATE]                                |
| :-----: | :-------------------------- | :------------------------------------ | :---------------------------------------- |
|  [01]   | Transform a value           | `int -> string`                       | `Func<int, string>`                       |
|  [02]   | Produce a value             | `() -> string`                        | `Func<string>`                            |
|  [03]   | Consume a value for effects | `int -> ()`                           | `Action<int>`                             |
|  [04]   | Perform an effect           | `() -> ()`                            | `Action`                                  |
|  [05]   | Combine two inputs          | `(int, int) -> int`                   | `Func<int, int, int>`                     |
|  [06]   | Accept a function           | `(string, (IDbConnection -> R)) -> R` | `Func<string, Func<IDbConnection, R>, R>` |

`()` means no input or no returned information. Grouped inputs are treated as a tuple.

### [01.1]-[READING_SIGNATURES]

Generic signatures constrain plausible behavior:

```text
(IEnumerable<T>, (T -> bool)) -> IEnumerable<T>
(IEnumerable<A>, IEnumerable<B>, ((A, B) -> C)) -> IEnumerable<C>
```

The first suggests filtering a sequence with a predicate. The second suggests combining corresponding `A` and `B` values into `C` values. `() -> ()` reveals almost nothing about the effect performed. Signatures cannot express every semantic detail: `Where` and `TakeWhile` have identical type signatures. Precise types and names are necessary.

Functional design keeps data and logic distinct: data objects carry inputs and outputs, while functions encode behavior. Constrained types can still own the validation to construct them and operations (comparison) that protect their hidden representation.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [02]-[INVALID_INPUTS]

Primitive types can hold values outside the domain. `int` used as an age accepts values less than 0 or greater than 119. Validating inside every consumer duplicates the rule and mixes validation with the consumer's calculation.

Custom types can narrow the domain once. `Age` is a value object: the generator supplies the factory methods, equality, comparison, and the conversion to `int`. `InvalidAge` implements `IValidationError<InvalidAge>`. The validation hook raises the typed `Expected` record. `From` is the smart constructor: it maps the generated `Validate` to `Fin<Age>`:

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

`Age.From` has the signature `int -> Fin<Age>`. Callers cannot bypass the invariant, and consumers neither repeat validation nor inspect the underlying integer. `Create` throws when the value is invalid. This behavior reports a defect in the calling code, not an expected input outcome.

### [02.1]-[HONEST_FUNCTIONS]

Functions honor their signature when each declared input produces a declared output. They do not return `null` or throw an exception as an intrinsic outcome that the signature fails to describe.

```text
Age -> Risk
```

This signature is accurate when every constructible `Age` produces a `Risk`. `int -> Risk` is incomplete if some integers cause validation exceptions. Repair the contract either by narrowing the input to a validated type or by widening the output to represent the possibility of failure.

Honoring a signature does not require purity. Purity also excludes observable side effects and dependence on mutable state.

### [02.2]-[TYPES_AS_SETS]

Types can be modeled as sets of possible values. If `Age` has 120 values and `Gender` has two, `(Age, Gender)` has `120 * 2 = 240` possible values. Tuples or objects containing both are product types: each field adds a dimension to the space of possible states.

`Option<A>` is a union: all `Some(A)` values and the single `None` value. Types with `n` values yield options with `n + 1` values. Counting possible instances exposes types that hold states the domain does not need. Once component types are constrained, they can be composed into larger data objects without reintroducing invalid primitive states.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md [01.1] and [04]
## [03]-[UNIT]

`void` is a language special case rather than an ordinary return type. This splits delegates into `Func` and `Action` families and can force duplicate higher-order-function implementations. The same split appears between `Task<T>` and `Task`.

`Unit` is an ordinary LanguageExt type with exactly one value, `unit`, and no information. The Prelude function `fun` converts an `Action` into a `Func<Unit>`.

`Action` overloads can adapt and delegate to the one generic implementation:

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

Use `void` when an ordinary imperative API performs effects and returns no information. Use `Unit` when an ordinary return value enables uniform functional handling. Returning `Unit` does not make an effectful function pure, it only removes a special-case return type.

## [04]-[OPTION]

Framework lookup APIs show that absence omitted from the result type is unsafe: a missing key can yield `null` from one collection and throw from another, even though both indexers appear to have the signature `key -> value`.

`Option<A>` makes absence part of the result type:

```text
Option<A> = None | Some(A)
```

`Match` requires behavior for both states and returns one common result type.

```csharp
internal static class Greetings {
    public static string GreetingFor(Option<string> name) =>
        name.Match(
            Some: static value => $"Dear {value.ToUpperInvariant()},",
            None: static () => "Dear Subscriber,");
}
```

`Match` belongs at the host, where a result type becomes a rendered value. Inside the domain, `Bind` sequences computations for `Some`, and `Map` transforms its value.

Changing a required `string` property to `Option<string>` is a breaking change: code that treats the property as a `string` stops compiling until it handles absence. This trades possible runtime `NullReferenceException`s for compile-time errors that require explicit handling.
-->

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
### [04.1]-[IMPLEMENTATION]

`Option<A>` is one readonly struct. It holds a flag and an inner value, exposes the flag as `IsSome` and `IsNone`, and selects the case through `Match`. The Prelude supplies `Some(x)` and `None`. The implicit conversion from `A` maps `null` to `None`, and `Optional(x)` does the same when nullable input enters the domain. `Some(x)` wraps the value as given and is not a null check:

```csharp
internal static class Construction {
    public static Option<string> Present(string value) => Some(value);
    public static Option<string> Absent() => None;
    public static Option<string> AtTheBoundary(string? external) => Optional(external);
    public static Option<string> Lifted(string? external) => external;
}
```

The default inner value in the `None` state is ignored. The contract is an empty case, a present case, and a way to handle both cases safely. Other libraries name the same abstraction `Maybe`, with cases `Nothing` and `Just`. Open class hierarchies cannot stop a caller from adding a third case.

(Integrated into .claude/skills/dotnet-coding/SKILL.md)

## [05]-[TOTALITY]

Total functions cover every value in their declared input domains, partial functions do not. Returning `Option` totalizes a partial computation: return `Some(result)` where the computation is defined and `None` otherwise.

```csharp
internal static class Totality {
    public static Option<int> ParseAge(string text) => parseInt(text);
    public static Option<string> Setting(HashMap<string, string> settings, string key) => settings.Find(key);
    public static Option<Age> AgeOf(HashMap<string, string> settings) =>
        Setting(settings, "age").Bind(ParseAge).Bind(static value => Age.From(value).ToOption());
}
```

`string -> int` hides the undefined parsing cases, `string -> Option<int>` describes every outcome. The Prelude function `parseInt` has that signature. `Find` on `HashMap<K, V>` has the signature `K -> Option<V>`. Missing keys are values, not exceptions. `Bind` chains the two lookups, and `ToOption` on `Fin<Age>` drops the failure reason when the caller does not need it.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [06]-[NULLABLE_REFERENCE_TYPES]

Nullable reference types are compiler analysis, not a new runtime type. Enable the analysis in the project file:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

The compiler warns when a non-nullable property can be uninitialized or when `null` is assigned to a non-nullable reference.

Use `?` only when null is a deliberate part of the representation:

```csharp
internal sealed record ExternalMovie {
    public string? Title { get; init; }
    public string? Director { get; init; }
    public IEnumerable<string>? Cast { get; init; }
}
```

Null adds another possible state that every consumer must account for. If an external data source requires nullable values, isolate that representation in the parsing code and convert it to a validated internal type before other code uses it.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [07]-[DESIGN_RULES]

- Design the signature early, specify exact input and outcome types
- Prefer constrained domain types to primitives and repeated validation
- Map the generated `Validate` to `Fin` through `From` when primitive-to-domain conversion can fail
- The generator keeps the constructor private, every value passes through the validation hook
- Do not explicitly return `null` from functions
- Reject unexpected `null` at public API inputs, except for optional parameters because their defaults must be compile-time constants
- Enable nullable reference types, the compiler exposes accidental nullability before a value enters a pipeline
- Use `Option<A>` for optional properties, parsing, lookup, and other computations that can produce no value
- Use `Match` when a concrete result must be selected from the `None` and `Some` cases
- Use `Unit` and `fun` to adapt `Action` into `Func`-based higher-order APIs without duplicating their behavior
-->
