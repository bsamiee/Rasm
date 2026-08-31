# [DISCRIMINATED_UNIONS]

## [01]-[EXPLICIT_CASES]

A discriminated union is a type whose value is exactly one of several alternatives. A consumer pattern-matches the value to discover its case and gain access to that case's data. A component that does not care which case it has can pass the union onward unchanged.

F# supports discriminated unions directly. The union generator supplies them in C#. `[Union]` marks an abstract partial record, each case is a sealed record nested inside it, and the generated `Switch` takes one arm per case.

Cases can be unrelated alternatives that share only an API type or collection.

## [02]-[ALTERNATIVES_OVER_FLAGS]

A discriminator flag combined with fields that are meaningful only for one flag value permits invalid combinations:

```csharp
internal sealed record FlaggedCustomer(string Email, bool IsRegistered, string Name, bool IsEligible);
```

When `IsRegistered` is false, `Name` and `IsEligible` have no meaning. Model the alternatives as distinct variants:

```csharp
[Union]
internal abstract partial record Customer {
    internal sealed record RegisteredCustomer(string Name, string Email, bool IsEligible) : Customer;
    internal sealed record GuestCustomer(string Email) : Customer;
}
```

Consumers call `Switch` on named cases, and each case carries only its required data.

A travel system sells holidays and day trips. One class with all fields and an `IsDayTrip` flag violates Interface Segregation: every instance carries properties that do not describe its case, while names such as `Destination` and `StartDate` mean concepts named `Attraction` and `DateOfTrip`.

Use an abstract union base:

```csharp
internal sealed record Location(string Name);

[Union]
internal abstract partial record CustomerOffering {
    public required int Id { get; init; }

    internal sealed record Holiday : CustomerOffering {
        public required Location Destination { get; init; }
        public required Location DepartureAirport { get; init; }
        public required DateTime StartDate { get; init; }
        public required int DurationOfStay { get; init; }
    }
    internal sealed record DayTrip : CustomerOffering {
        public required DateTime DateOfTrip { get; init; }
        public required Location Attraction { get; init; }
        public required bool CoachTripRequired { get; init; }
    }
}
```

`DayTrip` is not an extension of `Holiday`; it is an alternative to it. The shared base supports a single `Seq<CustomerOffering>`, while each case retains only meaningful data and terminology.

```csharp
internal static class Offerings {
    public static string Format(CustomerOffering offering) =>
        offering.Switch(
            holiday: static x => string.Create(CultureInfo.InvariantCulture, $"{x.Destination.Name}, {x.DurationOfStay} nights"),
            dayTrip: static x => x.Attraction.Name);
}
```

A `BritishName` can carry first, middle, and last names with an honorific placed first; a `ChineseName` can carry family, given, courtesy, and honorific fields with a different output order. A common abstract `Name` permits one collection, while the variants preserve meaningful fields and formatting rather than forcing one culture's name structure onto another.

## [03]-[OUTCOMES_AS_CASES]

A function's return type must describe every expected outcome. Looking up a person has three meaningful outcomes:
1. the person was found;
2. no person has that identifier;
3. the lookup failed.

`OptionT<IO, Person>` names all three: `Some` is the found person, `None` is absence, and a lookup failure is on the `IO` error channel. The effectful function translates each external outcome into one case:

```csharp
internal sealed record Person(int Id, string Name);

internal static class People {
    public static OptionT<IO, Person> GetPerson(Func<int, Person?> database, int id) =>
        OptionT.lift<IO, Person>(IO.lift(() => Optional(database(id))));
}
```

`Optional` maps the `null` of a missing row to `None`. `IO.lift` captures a thrown lookup failure as an `Exceptional` error. The caller matches `Some` and `None` with `OptionT.Match`, which returns `K<IO, B>`, and `.As()` restores `IO<B>`:

```csharp
internal static class Greeting {
    public static IO<string> Describe(OptionT<IO, Person> lookup) =>
        lookup.Match(Some: static person => person.Name, None: static () => "no such person").As();
}
```

The host runs the effect with `RunSafe()` and receives a lookup failure in the `Fin` result. The caller does not infer the outcome from `null`, a status flag, or optional metadata.

Email sending returns `IO<Unit>`: `Unit` represents completion, and the error channel carries transport failures.

```csharp
internal static class Mail {
    public static IO<Unit> SendEmail(Action<string> transport, string address) => IO.lift(() => transport(address));
}
```

## [04]-[INPUT_REFINEMENT]

Convert the console result to typed application data in these stages:
1. Pass console access as a `Func<string>` dependency so another implementation can replace it.
2. `IO.lift` captures a console failure on the error channel.
3. Classify successfully read text once.
4. Let application code consume the already-classified case.

`parseInt` returns `Option<int>`, and `Match` builds the `IntegerInput` case from `Some`.

```csharp
[Union]
internal abstract partial record UserInput {
    internal sealed record TextInput(string Input) : UserInput;
    internal sealed record NoInput : UserInput;
    internal sealed record IntegerInput(int Input) : UserInput;
    internal sealed record ConsoleError(Error Error) : UserInput;
}

internal static class Input {
    public static IO<UserInput> Read(Func<string> console) =>
        IO.lift(() => Classify(console()))
            .Catch(static error => error.IsExceptional, static error => IO.pure<UserInput>(new UserInput.ConsoleError(error)));
    public static UserInput Classify(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? new UserInput.NoInput()
            : parseInt(text).Match<UserInput>(Some: static value => new UserInput.IntegerInput(value), None: () => new UserInput.TextInput(text));
}
```

The `Catch` overload with a predicate maps the captured error to the `ConsoleError` case at the boundary. The prompt reads again after any case, so the console failure is a case the prompt matches. Code that requires an integer handles `IntegerInput` directly and can recursively prompt for the other cases. Parsing and exception handling are not duplicated at every call site. The side-effecting read is separate from deterministic classification.

## [05]-[GENERIC_UNIONS]

`Option<A>` covers a value or nothing, and `Fin<A>` covers a value or a failure with a reason. `Either<L, R>` represents one of two value types, and independent failures accumulate in `Validation<Error, A>`. An empty `Seq<A>` is a result, not absence, so a producer wraps a collection in `Option` only where the consumer responds differently to no collection and to an empty one. A producer that maps an operational failure to `None` hides the failure from the consumer.

`Option` and `Fin` are closed, so a `Match` over their cases is total. `Some` does not check for `null`, so use `Optional` to convert `null` at the boundary. A generated union is closed: the base has a private constructor, and `Switch` names every case.

## [06]-[DESIGN_RULES]

- Model each valid state as a distinct case, not as a Boolean discriminator with conditionally meaningful fields.
- Put only shared data on the abstract base; keep case-specific data in its case.
- Use a domain-specific union when the consumer needs domain outcome names instead of `Option`, `Fin`, or `Either`.
- Keep absence and failure separate whenever the consumer needs to respond differently.
- Interpret external values once near their source, then pass typed cases inward.
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere.
- Declaring a union requires one attribute and nested cases. The generated `Switch` replaces repeated case checks.
