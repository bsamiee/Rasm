# Discriminated Unions

## One type, several explicit cases

A discriminated union is a type whose value is exactly one of several alternatives. A consumer pattern-matches the value to discover its case and gain access to that case's data. A component that does not care which case it has can pass the union onward unchanged.

F# supports discriminated unions directly. The union generator supplies them in C#. `[Union]` marks an abstract partial record, each case is a sealed record nested inside it, and the generated `Switch` takes one arm per case.

This is more than ordinary specialization. The cases may be unrelated alternatives that share only the need to travel through one API or collection.

## Model alternatives instead of flagged field bundles

A discriminator flag combined with fields that are meaningful only for one flag value permits invalid combinations:

```csharp
internal sealed record FlaggedCustomer(string Email, bool IsRegistered, string Name, bool IsEligible);
```

When `IsRegistered` is false, callers must remember that `Name` and `IsEligible` have no meaning. Model the alternatives as distinct variants instead:

```csharp
[Union]
internal abstract partial record Customer {
    internal sealed record RegisteredCustomer(string Name, string Email, bool IsEligible) : Customer;
    internal sealed record GuestCustomer(string Email) : Customer;
}
```

Consumers call `Switch` on descriptive cases, and each case carries only the data it needs.

Suppose a travel system sells holidays and day trips. One class containing every possible field plus an `IsDayTrip` flag creates bundles of irrelevant fields and violates Interface Segregation: every instance carries properties that do not describe its actual case, while names such as `Destination` and `StartDate` are repurposed for concepts better called `Attraction` and `DateOfTrip`.

Keeping the types wholly unrelated preserves their vocabulary, but loses a common type for storing and processing every offering together. An abstract union base provides both:

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

The same design handles structurally different naming conventions. A `BritishName` can carry first, middle, and last names with an honorific placed first; a `ChineseName` can carry family, given, courtesy, and honorific fields with a different output order. A common abstract `Name` permits one collection, while the variants preserve meaningful fields and formatting rather than forcing one culture's shape onto another.

## Make every expected function outcome a case

A function's return type should describe more than its happy-path payload. Looking up a person has three meaningful outcomes:
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

An operation with no success payload still benefits. Email sending returns `IO<Unit>`: `unit` is the completed send, and a transport failure is on the error channel. The absence of a success value does not require hiding whether the operation completed.

```csharp
internal static class Mail {
    public static IO<Unit> SendEmail(Action<string> transport, string address) => IO.lift(() => transport(address));
}
```

## Move from impure input to typed input in stages

External input is uncontrolled. A console boundary can return one of four cases: text, an integer, no input, or a console error. This turns the move from an impure boundary to controlled program data into a sequence:
1. Keep console access behind a narrow dependency, so the uncontrolled console can be replaced by a controlled implementation. A delegate carries a one-operation dependency.
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

The `Catch` overload with a predicate maps the captured error to the `ConsoleError` case at the boundary. The prompt reads again after any case, so the console failure is a case the prompt matches. Code that requires an integer now handles `IntegerInput` directly and can recursively prompt again for the other cases. Parsing and exception handling are not duplicated at every call site. The side-effecting read remains small; classification can remain ordinary deterministic logic.

## Reusable generic unions

Situation-specific cases communicate the domain best, but recurring shapes are generic. `Option<A>` covers a value or nothing, and `Fin<A>` covers a value or a failure with a reason. One of two value types is `Either<L, R>`, and independent failures accumulate in `Validation<Error, A>`. An empty `Seq<A>` is a result, not absence, so a producer wraps a collection in `Option` only where the receiver responds differently to no collection and to an empty one. A producer that maps an operational failure to `None` hides the failure from the consumer.

`Option` and `Fin` are closed, so a `Match` over their cases is total. `Some` does not check for `null`, so `Optional` is the null boundary. A regular union is closed as well: the base has a private constructor, and `Switch` names every case.

## Design rules

- Model distinct valid states as distinct cases, not a boolean discriminator plus bundles of partially meaningful fields.
- Put only genuinely shared data on the abstract base; keep case-specific data on its case.
- Use domain-specific unions when their names explain the possible outcomes better than generic `Option`, `Fin`, or `Either` names.
- Keep absence and failure separate whenever the receiver needs to respond differently.
- Interpret uncontrolled values once near their source, then pass typed cases inward.
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere.
- Declaring the union costs one attribute and nested cases, and the generated `Switch` removes repeated checks and makes downstream handling descriptive.

The result is a model in which valid alternatives are named, irrelevant field bundles are avoided, and receiving code is directed toward every meaningful outcome.
