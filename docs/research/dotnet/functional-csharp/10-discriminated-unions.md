<!-- Fully integrated into dotnet-coding/SKILL.md, dotnet-coding/references/results.md, and dotnet-coding-languageext/SKILL.md, each section carries its marker -->
# [DISCRIMINATED_UNIONS]

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [01]-[EXPLICIT_CASES]

Discriminated unions are types holding exactly one of several alternatives. Consumers pattern-match the value to discover its case and reach that case's data. Components that do not care which case they have can pass the union unchanged.

F# supports discriminated unions directly. The union generator supplies them in C#. `[Union]` marks an abstract partial record, each case is a sealed record nested inside it, and the generated `Switch` takes one arm per case.

Cases can be unrelated alternatives that share only an API type or collection.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [02]-[ALTERNATIVES_OVER_FLAGS]

Discriminator flags with fields meaningful only for one flag value permit invalid combinations:

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
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md [04.3], the case-as-alternative, per-case naming, and mixed-collection rules
Travel systems sell holidays and day trips. One class with all fields and an `IsDayTrip` flag violates Interface Segregation: every instance carries properties that do not describe its case, while names (`Destination`, `StartDate`) mean concepts named `Attraction` and `DateOfTrip`.

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

`DayTrip` is not an extension of `Holiday` but an alternative to it. The shared base supports a single `Seq<CustomerOffering>`, while each case retains only meaningful data and terminology.

```csharp
internal static class Offerings {
    public static string Format(CustomerOffering offering) =>
        offering.Switch(
            holiday: static x => string.Create(CultureInfo.InvariantCulture, $"{x.Destination.Name}, {x.DurationOfStay} nights"),
            dayTrip: static x => x.Attraction.Name);
}
```

`BritishName` can carry first, middle, and last names with an honorific placed first, and a `ChineseName` can carry family, given, courtesy, and honorific fields with a different output order. The abstract `Name` permits one collection, while variants preserve meaningful fields and formatting rather than forcing one culture's name structure onto another.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [03]-[OUTCOMES_AS_CASES]

Function return types must describe every expected outcome. Looking up a person has meaningful outcomes:
1. The person was found
2. No person has that identifier
3. The lookup failed

`OptionT<IO, Person>` names each: `Some` is the found person, `None` is absence, and a lookup failure is on the `IO` error channel. The effectful function translates each external outcome into one case:

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
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [04]-[INPUT_REFINEMENT]

Convert the console result to typed application data in these stages:
1. Pass console access as a `Func<string>` dependency, another implementation can replace it
2. `IO.lift` captures a console failure on the error channel
3. Classify successfully read text once
4. Let application code consume the already-classified case

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

The `Catch` overload with a predicate maps the captured error to the `ConsoleError` case at the boundary. The prompt reads again after any case. The console failure is a case the prompt matches. Code that requires an integer handles `IntegerInput` directly and can recursively prompt for the other cases. Parsing and exception handling are not duplicated at every call site. The side-effecting read is separate from deterministic classification.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [05]-[GENERIC_UNIONS]

`Option<A>`, `Fin<A>`, `Either<L, R>`, and `Validation<Error, A>` are the generic unions for absence, failure with a reason, two value types, and accumulated failures. Empty `Seq<A>` is a result, not absence. Producers wrap a collection in `Option` only where the consumer responds differently to no collection and to an empty one. Producers that map an operational failure to `None` hide the failure from the consumer.

`Option` and `Fin` are closed, a `Match` over their cases is total.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [06]-[RECURSIVE_CASES]

Union cases can carry the union type itself. Such a union models hierarchical data: a value is a scalar or a container of further values of the same type.

```csharp
[Union]
internal abstract partial record Json {
    internal sealed record Str(string Value) : Json;
    internal sealed record Num(decimal Value) : Json;
    internal sealed record Flag(bool Value) : Json;
    internal sealed record Nil : Json;
    internal sealed record Arr(Seq<Json> Items) : Json;
    internal sealed record Obj(Map<string, Json> Members) : Json;
}
```

`Str`, `Num`, and `Flag` carry scalar payloads, and `Nil` is a stateless case. `Arr` holds a `Seq<Json>`, and `Obj` holds a `Map<string, Json>`. The cases without recursive payloads are the leaves, and `Arr` and `Obj` are the containers. The recursive payloads are ordinary case properties, and the generator's case discovery is unaffected. The union models trees the domain owns: configuration, expressions, UI hierarchies, and document fragments. Wire serialization stays with `System.Text.Json` at the host boundary.

One operation names every case through `Switch`, and the arms for recursive cases recurse:

```csharp
internal static class Sizes {
    public static int Count(Json json) =>
        json.Switch(
            str: static _ => 1,
            num: static _ => 1,
            flag: static _ => 1,
            nil: static _ => 1,
            arr: static x => 1 + x.Items.Fold(0, static (sum, child) => sum + Count(child)),
            obj: static x => 1 + x.Members.Fold(0, static (sum, child) => sum + Count(child)));
}
```

Each leaf arm answers one, and the `arr` and `obj` arms add the node to the counts of its children. Every arm is a `static` lambda that calls a static function, because an arm without the `static` modifier is `TTRESG1001`. Context that an operation needs travels through the `Switch` state overload.

The union has six constructors, a fold takes one replacement function per constructor. Each scalar replacement receives that case's payload, `Nil` receives nothing, and the replacements for `Arr` and `Obj` receive already-folded child results:

```csharp
internal sealed record JsonFold<R>(
    Func<string, R> Str,
    Func<decimal, R> Num,
    Func<bool, R> Flag,
    Func<R> Nil,
    Func<Seq<R>, R> Arr,
    Func<Map<string, R>, R> Obj);

internal static partial class Folds {
    public static R Fold<R>(Json json, JsonFold<R> fold) =>
        json.Switch(
            fold,
            str: static (f, x) => f.Str(x.Value),
            num: static (f, x) => f.Num(x.Value),
            flag: static (f, x) => f.Flag(x.Value),
            nil: static (f, _) => f.Nil(),
            arr: static (f, x) => f.Arr(x.Items.Map(child => Fold(child, f))),
            obj: static (f, x) => f.Obj(x.Members.Map(child => Fold(child, f))));
}
```

The recursion lives in `Fold` once, and the handler record travels as the state, every arm stays `static`. `Count` is this fold with one for every leaf and addition for the containers. Another operation supplies other replacements to the same scheme:

```csharp
internal static partial class Folds {
    public static int Depth(Json json) =>
        Fold(json, new JsonFold<int>(
            Str: static _ => 1,
            Num: static _ => 1,
            Flag: static _ => 1,
            Nil: static () => 1,
            Arr: static depths => 1 + depths.Fold(0, Math.Max),
            Obj: static depths => 1 + depths.Fold(0, Math.Max)));
}
```

`Depth` replaces each leaf with one and each container with one more than its deepest child.

The remaining operations follow the return-type rules. Member lookup passes the requested key as the state and returns `Option<Json>`: an `Obj` without the key and every other case answer `None`. Typed extraction returns `Fin<A>` with distinct `Expected` records, a consumer classifies a wrong shape by code. Operations that preserve a case take the case type directly when the caller already holds one, and the signature removes the wrong-shape error. When the shape arrives as data, the operation covers the union and returns a `Fin` failure for every other case.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [07]-[DESIGN_RULES]

- Model each valid state as a distinct case, not as a Boolean discriminator with conditionally meaningful fields
- Match the representation to the growth axis: a union for growing operations, abstract members for growing cases. New cases are compile errors at every `Switch` until each gains an arm.
- Direct recursion over a recursive union suits document-sized trees. C# does not guarantee tail-call optimization, unbounded depth folds through `Trampoline<A>`.
- Put only shared data on the abstract base and keep case-specific data in its case
- Use a domain-specific union when the consumer needs domain outcome names instead of `Option`, `Fin`, or `Either`
- Keep absence and failure separate whenever the consumer needs to respond differently
- Interpret external values once near their source, then pass typed cases inward
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere
- Declaring a union requires one attribute and nested cases
-->
