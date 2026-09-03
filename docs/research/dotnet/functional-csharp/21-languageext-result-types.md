<!-- Seed of dotnet-languageext, the skill dotnet-coding holds the [01] table without its SHAPE column, the [02] and [03] rules, and the [08] table -->
# [LANGUAGEEXT_RESULT_TYPES]

Every function in this set returns an explicit result type.

## [01]-[ONE_TYPE_PER_CONCERN]

| [INDEX] | [TYPE]                 | [CONCERN]                                       | [SHAPE]               |
| :-----: | :--------------------- | :---------------------------------------------- | :-------------------- |
|  [01]   | `Option<A>`            | Absence without an `Error`                      | readonly struct       |
|  [02]   | `Fin<A>`               | Expected failure with an `Error`, short-circuit | abstract class        |
|  [03]   | `Either<L, R>`         | Two value types, neither an error               | abstract record class |
|  [04]   | `Validation<Error, A>` | Independent failures, accumulate                | abstract record class |
|  [05]   | `Try<A>`               | Synchronous exception capture, deferred         | record class          |
|  [06]   | `IO<A>`                | Side effects with a failure channel             | abstract record class |
|  [07]   | `Eff<RT, A>`           | Effects that read a capability                  | record class          |

- `Option<A>` holds a value or `None`. Missed lookups provide no error information.
- `Fin<A>` holds a value or an `Error`. Domain transitions rejecting input explain the rejection, and a dependent chain of `Fin` stops at the first rejection.
- `Either<L, R>` holds one of two values. It represents alternative data values, not success and failure.
- `Validation<Error, A>` holds a value or all `Error` values produced by independent checks. Forms with an empty name and an impossible age report both.
- `Try<A>` holds a deferred synchronous computation that can throw. Nothing runs until `Run`, and a thrown exception arrives as an `Error`.
- `IO<A>` holds a deferred effect with an `Error` channel. Domain rejection inside an effect is a typed `Expected` on that channel.
- `Eff<RT, A>` is an effect that reads a capability from a runtime `RT`. `IO<A>` converts implicitly to `Eff<RT, A>`.

Each type exposes `Match` with one function per case. `Match` on `IO` and `Eff` returns an effect.

```csharp
internal sealed record Person(string Name, Age Age);
internal sealed record Guest(string Name);
internal sealed record Member(int Id);
internal sealed record Clock(DateTimeOffset Now);
internal sealed record Runtime(Clock Clock) : Has<Eff<Runtime>, Clock> {
    static K<Eff<Runtime>, Clock> Has<Eff<Runtime>, Clock>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Clock);
}

internal static class Concerns {
    public static Option<Person> Find(Map<string, Person> people, string name) => people.Find(name);
    public static Fin<Age> Restrict(int years) => Age.From(years);
    public static Either<Guest, Member> Visitor(string name, int id) => id > 0 ? Right(new Member(id)) : Left(new Guest(name));
    public static Validation<Error, Person> Register(string name, int years) =>
        (ValidName(name), Age.From(years).ToValidation()).Apply(static (n, a) => new Person(n, a)).As();
    public static Try<int> Parse(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture));
    public static IO<Person> Load(Map<string, Person> people, string name) => IO.lift(() => people.Find(name).ToFin(new NotFound()));
    public static Eff<RT, DateTimeOffset> Stamp<RT>() where RT : Has<Eff<RT>, Clock> => RT.Ask.Map(static clock => clock.Now).As();

    private static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;
}
```

`Find` returns `Option` because absence carries no `Error`. `Restrict` returns `Fin` because an out-of-range age produces an `Error`. `Register` combines independent checks and reports every failure. `Load` returns `IO` with the typed `NotFound` on its failure channel. `Stamp` reads the `Clock` capability from any runtime that declares `Has<Eff<RT>, Clock>`.

## [02]-[BOUNDARY_RULE]

The input boundary selects the result type, and domain functions preserve it. Conversion between types happens at one named boundary. `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations, and domain functions never run an effect. `Register` validates the raw form and returns `Validation`. `Handle` converts with `ToFin` and binds the domain transition, and `Respond` matches at the host.

```csharp
internal sealed record Ticket(string Holder);

internal static class Boundary {
    public static Fin<Ticket> Issue(Person person) => person.Age >= 18 ? new Ticket(person.Name) : new Underage();
    public static Fin<Ticket> Handle(string name, int years) => Concerns.Register(name, years).ToFin().Bind(Issue);
    public static string Respond(string name, int years) => Handle(name, years).Match(Succ: static ticket => ticket.Holder, Fail: static error => error.Message);
}
```

## [03]-[IMPLICIT_LIFTS]

Values of type `A` lift into `Fin<A>` and `Validation<Error, A>`. `Error` values, including `Expected` subclasses, lift into the failure case. `Pure(x)` and `Fail<Error>(e)` make the intended lift explicit when the two branches of a conditional differ in type. Smart constructors map the value object's generated `Validate` to `Fin<Age>`. This gives every consumer a validated value.

```csharp
[ValueObject<int>]
[ValidationError<InvalidAge>]
internal readonly partial struct Age {
    public static Fin<Age> From(int value) => Validate(value, provider: null, out Age item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidAge? validationError, ref int value) {
        if (value is < 0 or >= 120)
            validationError = new InvalidAge();
    }
}

internal static class Lifts {
    public static Fin<int> FromValue(int value) => value;
    public static Fin<int> FromError(Error error) => error;
    public static Fin<int> Halve(int value) => value <= 100 ? Pure(value / 2) : Fail<Error>(new TooLarge());
}
```

The return type `Fin<Age>` selects the lift for the `InvalidAge` and `item` branches.

## [04]-[CONVERSIONS]

Each conversion is a method on the source type, and the name states the target. Converting `Option` to `Fin` or `Validation` requires an `Error`, because `Option` contains none. `Validation` becomes `Fin` at the end of input validation. `Fin` from a smart constructor converts to `Validation` before combining with independent validations. `Try`, `IO`, and `Eff` return `Fin` when run.

```csharp
internal static class Conversions {
    public static Fin<int> Required(Option<int> value) => value.ToFin(new NotFound());
    public static Validation<Error, int> Checked(Option<int> value) => value.ToValidation<Error>(new NotFound());
    public static Option<Age> Present(Fin<Age> age) => age.ToOption();
    public static Either<Error, Age> Split(Fin<Age> age) => age.ToEither();
    public static Seq<int> Items(Option<int> value) => value.ToSeq();
    public static Fin<Person> Exit(Validation<Error, Person> form) => form.ToFin();
    public static Validation<Error, Age> Widen(Fin<Age> age) => age.ToValidation();
    public static Fin<int> Captured(Try<int> attempt) => attempt.Run();
    public static Fin<Person> Ran(IO<Person> effect) => effect.RunSafe();
    public static Fin<DateTimeOffset> Stamped(Runtime runtime) => Concerns.Stamp<Runtime>().Run(runtime);
}
```

## [05]-[ERROR_MODEL]

Domain errors are `sealed record`s extending `Expected` with a message and a code. `Codes` holds the codes of the package declaring the errors. `Exceptional` is the error that `Try` produces from a captured exception. `ManyErrors` is the error that `+` and `Validation` produce from accumulation. Errors a value object raises also implement `IValidationError<T>`, and the generated `Validate` returns them. LanguageExt's `Errors` class holds shared values (`Errors.TimedOut`, `Errors.None`).

```csharp
internal static class Codes {
    public const int InvalidAge = 2101;
    public const int EmptyName = 2102;
    public const int NotFound = 2103;
    public const int Underage = 2104;
    public const int TooLarge = 2105;
    public const int RegistrationFailed = 2106;
}

internal sealed record InvalidAge() : Expected("age out of range", Codes.InvalidAge), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

internal sealed record EmptyName() : Expected("name is empty", Codes.EmptyName);
internal sealed record NotFound() : Expected("person not found", Codes.NotFound);
internal sealed record Underage() : Expected("person is under age", Codes.Underage);
internal sealed record TooLarge() : Expected("value is too large", Codes.TooLarge);
internal sealed record RegistrationFailed : Expected {
    public RegistrationFailed(Error cause) : base("registration failed", Codes.RegistrationFailed, cause) { }
}

internal static class Classify {
    public static Fin<int> Captured(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture)).Run();
    public static bool Retryable(Error error) => error.Is(Errors.TimedOut) || error.HasException<IOException>();
    public static bool Rejected(Error error) => error.HasCode(Codes.InvalidAge) || error.IsType<EmptyName>();
    public static int AgeFaults(Error error) => error.Filter<InvalidAge>().Count;
}
```

Consumers classify with `Is`, `HasCode`, `IsType<E>`, `Filter<E>`, `Count`, and `Head`, never with the message text. `IsType<E>` and `Filter<E>` search the leaves of a `ManyErrors`. `Count` returns the number of accumulated errors, and `Head` returns the first leaf. `HasCode` and `Catch(int)` select a code the same package declares. Codes from several packages meet in one `ManyErrors`, and `IsType<E>` separates them. The message is for the host to render.

## [06]-[RECOVERY]

Recovery is a function from an error to the same result type. The `Catch` overloads select by code, by error value, or by predicate. The code and error-value overloads are extensions that return `K<F, A>`, `.As()` restores the concrete type. `IO<A>` declares the predicate overload as an instance method that returns `IO<A>`. The `|` operator uses the right alternative when the left one fails. `BindFail` lets the recovery function return either `Fin` case. `MapFail` adds context by wrapping the original error as the inner error. `IfFail` returns a non-`Fin` value.

```csharp
internal static class Recovery {
    public static Fin<Age> ByCode(Fin<Age> age) => age.Catch(Codes.InvalidAge, static _ => Age.From(0)).As();
    public static Fin<Age> ByValue(Fin<Age> age) => age.Catch(new InvalidAge(), static _ => Age.From(0)).As();
    public static Fin<Age> ByPredicate(Fin<Age> age) => age.Catch(static error => error.IsExpected, static _ => Age.From(0)).As();
    public static IO<Person> Cached(IO<Person> load, Person cached) => load.Catch(Codes.NotFound, _ => IO.pure(cached)).As();
    public static IO<Person> Fallback(IO<Person> primary, IO<Person> secondary) => primary | secondary;
    public static Fin<Age> Rebound(Fin<Age> age) => age.BindFail(static error => error.HasCode(Codes.InvalidAge) ? Age.From(0) : error);
    public static Fin<Age> WithContext(Fin<Age> age) => age.MapFail(static error => new RegistrationFailed(error));
    public static int AtHost(Fin<int> result) => result.IfFail(static _ => -1);
}
```

## [07]-[LINQ_GUARDS]

`guard` raises an `Error` when its flag is false. `when` runs its alternative when the flag is true, and `unless` runs it when the flag is false. The alternative is a failed `Fin<Unit>` or a failed `IO<Unit>`, and the query continues with the same type. `guard<Error>` names the type argument because an `Expected` subclass selects the generic overload.

```csharp
internal static class Guards {
    public static Fin<int> Bounded(int value) =>
        from v in Pure(value).ToFin()
        from _ in guard<Error>(v >= 0, new InvalidAge())
        from __ in when(v > 100, Reject(new TooLarge()))
        select v;
    public static IO<int> Metered(IO<int> read) =>
        from v in read
        from _ in unless(v <= 100, IO.fail<Unit>(new TooLarge()))
        select v;

    private static Fin<Unit> Reject(Error error) => error;
}
```

## [08]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                                       | [CORRECT_FORM]                                                |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `Match` in the middle of a pipeline unwraps a value that the next step lifts again | `Bind` the next step, as `Older` shows                        |
|  [02]   | `IfNone` with an arbitrary default hides absence                                   | `ToFin` with an `Error`, as `Required` shows                  |
|  [03]   | Matching on message text couples the consumer to prose                             | `HasCode` or `IsType<E>`                                      |
|  [04]   | `Option` nested inside an effect forces the consumer to unwrap two layers          | `OptionT<IO, A>`, as `Lookup` shows                           |
|  [05]   | `Fin` nested inside an effect duplicates the failure channel                       | Typed `Expected` on the `IO` error channel, as `Load` shows   |
|  [06]   | `Run` inside the domain performs the effect before the host runs the program       | Keep the `IO` and `Bind` the next step                        |
|  [07]   | `Some` as a null guard, because `Some(null)` holds `null`                          | `Optional` at the null boundary, as `Nickname` shows          |

At the host, evaluating `Lookup` with `Run`, `As`, and `RunSafe` produces `Fin<Option<Person>>`.

```csharp
internal static class CorrectForms {
    public static Fin<Age> Older(int years) => Age.From(years).Bind(static age => Age.From(age + 1));
    public static Fin<Age> Required(Option<int> years) => years.ToFin(new NotFound()).Bind(Age.From);
    public static OptionT<IO, Person> Lookup(Map<string, Person> people, string name) => OptionT.lift<IO, Person>(IO.lift(() => people.Find(name)));
    public static Option<string> Nickname(string? raw) => Optional(raw);
}
```
