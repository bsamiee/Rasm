# LanguageExt Result Types

Every function in this set returns a value that names its outcome. This file owns the vocabulary of those values. One type serves one concern, and a value moves between types at one named boundary.

## One type per concern

| Type                   | Concern                                       | Shape                 |
| ---------------------- | --------------------------------------------- | --------------------- |
| `Option<A>`            | absence without a reason                      | readonly struct       |
| `Fin<A>`               | expected failure with a reason, short-circuit | abstract class        |
| `Either<L, R>`         | two value types, neither an error             | abstract record class |
| `Validation<Error, A>` | independent failures, accumulate              | abstract record class |
| `Try<A>`               | synchronous exception capture, deferred       | record class          |
| `IO<A>`                | side effects with a failure channel           | abstract record class |
| `Eff<RT, A>`           | effects that read a capability                | record class          |

- `Option<A>` holds a value or nothing. A lookup that misses has no reason to give, so `None` is the whole answer.
- `Fin<A>` holds a value or an `Error`. A domain transition that rejects its input explains the rejection, and a dependent chain of `Fin` stops at the first rejection.
- `Either<L, R>` holds one of two values, and neither side is a failure. It serves a fork in the data, not a fork in the outcome.
- `Validation<Error, A>` holds a value or every `Error` that independent checks raised. A form with an empty name and an impossible age reports both.
- `Try<A>` holds a deferred synchronous computation that can throw. Nothing runs until `Run`, and a thrown exception arrives as an `Error`.
- `IO<A>` holds a deferred effect with an `Error` channel. A domain rejection inside an effect is a typed `Expected` on that channel.
- `Eff<RT, A>` is an effect that reads a capability from a runtime `RT`. An `IO<A>` enters by implicit conversion.

Each of them exposes `Match` with one function per case, and `Match` on `IO` and `Eff` returns an effect that the host runs. The domain type `Age` and the error records appear in the sections that follow.

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
    public static Fin<Age> Admit(int years) => Age.From(years);
    public static Either<Guest, Member> Visitor(string name, int id) => id > 0 ? Right(new Member(id)) : Left(new Guest(name));
    public static Validation<Error, Person> Register(string name, int years) =>
        (ValidName(name), Age.From(years).ToValidation()).Apply(static (n, a) => new Person(n, a)).As();
    public static Try<int> Parse(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture));
    public static IO<Person> Load(Map<string, Person> people, string name) => IO.lift(() => people.Find(name).ToFin(new NotFound()));
    public static Eff<RT, DateTimeOffset> Stamp<RT>() where RT : Has<Eff<RT>, Clock> => RT.Ask.Map(static clock => clock.Now).As();

    private static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;
}
```

`Find` returns `Option` because a missing person has no reason. `Admit` returns `Fin` because an out-of-range age has one. `Register` combines independent checks and reports every failure. `Load` returns `IO` whose failure channel carries the typed `NotFound`. `Stamp` reads the `Clock` capability from any runtime that declares `Has<Eff<RT>, Clock>`.

## The boundary rule

The result type is chosen where input enters, and the domain keeps it. Conversion between types happens at one named boundary. `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations, and domain functions never run an effect. In the block that follows, `Register` admits the raw form as `Validation`. `Handle` converts with `ToFin` and binds the domain transition, and `Respond` matches at the host.

```csharp
internal sealed record Ticket(string Holder);

internal static class Boundary {
    public static Fin<Ticket> Issue(Person person) => person.Age >= 18 ? new Ticket(person.Name) : new Underage();
    public static Fin<Ticket> Handle(string name, int years) => Concerns.Register(name, years).ToFin().Bind(Issue);
    public static string Respond(string name, int years) => Handle(name, years).Match(Succ: static ticket => ticket.Holder, Fail: static error => error.Message);
}
```

## Implicit lifts

A bare `A` lifts into `Fin<A>` and `Validation<Error, A>`. A bare `Error`, including an `Expected` subclass, lifts into the failure case. `Pure(x)` and `Fail<Error>(e)` lift with an explicit intent when the two branches of a conditional differ in type. A smart constructor maps the value object's generated `Validate` to `Fin<Age>`, so every consumer receives a validated value.

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

`From` returns the `InvalidAge` from `Validate` on one branch and `item` on the other. The return type `Fin<Age>` selects the lift for both branches.

## Conversions

Each conversion is a method on the source type, and the name states the target. A conversion of `Option` to `Fin` or `Validation` names the `Error`, because absence has no reason of its own. `Validation` becomes `Fin` at the exit of the admitting boundary. `Fin` widens to `Validation` when a smart constructor joins an accumulating form. `Try`, `IO`, and `Eff` collapse to `Fin` when they run, and running belongs to the host.

```csharp
internal static class Conversions {
    public static Fin<int> Required(Option<int> value) => value.ToFin(new NotFound());
    public static Validation<Error, int> Admitted(Option<int> value) => value.ToValidation<Error>(new NotFound());
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

## The `Error` model

A domain error is a `sealed record` that extends `Expected` with a message and a code. The codes live in one closed block, so a consumer reads a code from one place. `Exceptional` is the error that `Try` produces from a captured exception. `ManyErrors` is the error that `+` and `Validation` produce from accumulation. An error that a value object raises also implements `IValidationError<T>`, and the generated `Validate` returns it. The package `Errors` class holds shared values such as `Errors.TimedOut` and `Errors.None`.

```csharp
internal static class Codes {
    public const int InvalidAge = 2101;
    public const int EmptyName = 2102;
    public const int NotFound = 2103;
    public const int Underage = 2104;
    public const int TooLarge = 2105;
}

internal sealed record InvalidAge() : Expected("age out of range", Codes.InvalidAge), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

internal sealed record EmptyName() : Expected("name is empty", Codes.EmptyName);
internal sealed record NotFound() : Expected("person not found", Codes.NotFound);
internal sealed record Underage() : Expected("person is under age", Codes.Underage);
internal sealed record TooLarge() : Expected("value is too large", Codes.TooLarge);

internal static class Classify {
    public static Fin<int> Captured(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture)).Run();
    public static bool Retryable(Error error) => error.Is(Errors.TimedOut) || error.IsType<Exceptional>();
    public static bool Rejected(Error error) => error.HasCode(Codes.InvalidAge) || error.IsType<EmptyName>();
    public static int AgeFaults(Error error) => error.Filter<InvalidAge>().Count;
}
```

A consumer classifies with `Is`, `HasCode`, `IsType<E>`, `Filter<E>`, `Count`, and `Head`, never with the message text. `IsType<E>` and `Filter<E>` search the leaves of a `ManyErrors`, so `Count` reports accumulation and `Head` reads the first leaf. The message is for the host to render.

## Recovery

Recovery is a function from an error to the same result type, and it lives at the boundary that owns the error. The `Catch` overloads select by code, by error value, or by predicate. The code and error-value overloads are extensions that return `K<F, A>`, so `.As()` restores the concrete type. `IO<A>` declares the predicate overload as an instance method that returns `IO<A>`. The `|` operator names an alternative. `BindFail` rebinds the failure case with the full `Fin` vocabulary. `MapFail` adds context by wrapping the original error as the inner error. `IfFail` escapes to a plain value and belongs to the host.

```csharp
internal static class Recovery {
    public static Fin<Age> ByCode(Fin<Age> age) => age.Catch(Codes.InvalidAge, static _ => Age.From(0)).As();
    public static Fin<Age> ByValue(Fin<Age> age) => age.Catch(new InvalidAge(), static _ => Age.From(0)).As();
    public static Fin<Age> ByPredicate(Fin<Age> age) => age.Catch(static error => error.IsExpected, static _ => Age.From(0)).As();
    public static IO<Person> Cached(IO<Person> load, Person cached) => load.Catch(Codes.NotFound, _ => IO.pure(cached)).As();
    public static IO<Person> Fallback(IO<Person> primary, IO<Person> secondary) => primary | secondary;
    public static Fin<Age> Rebound(Fin<Age> age) => age.BindFail(static error => error.HasCode(Codes.InvalidAge) ? Age.From(0) : error);
    public static Fin<Age> WithContext(Fin<Age> age) => age.MapFail(static error => Error.New("registration", error));
    public static int AtHost(Fin<int> result) => result.IfFail(static _ => -1);
}
```

## Guards inside LINQ

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

## Anti-patterns

Each rule in the table names the wrong form and the correct form beside it.

| Rule                                                                               | Correct form                                                  |
| ---------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| `Match` in the middle of a pipeline unwraps a value that the next step lifts again | `Bind` the next step, as `Older` shows                        |
| `IfNone` with an invented default hides a missing reason                           | `ToFin` with an `Error`, as `Required` shows                  |
| Matching on message text couples the consumer to prose                             | `HasCode` or `IsType<E>`                                      |
| An `Option` nested inside an effect forces the consumer to unwrap two layers       | `OptionT<IO, A>`, as `Lookup` shows                           |
| A `Fin` nested inside an effect duplicates the failure channel                     | a typed `Expected` on the `IO` error channel, as `Load` shows |
| `Run` inside the domain performs the effect before the host runs the program       | keep the `IO` and `Bind` the next step                        |
| `Some` as a null guard, because `Some(null)` holds `null`                          | `Optional` at the null boundary, as `Nickname` shows          |

`Lookup` exits at the host with `Run`, `As`, and `RunSafe`, which yields `Fin<Option<Person>>`.

```csharp
internal static class CorrectForms {
    public static Fin<Age> Older(int years) => Age.From(years).Bind(static age => Age.From(age + 1));
    public static Fin<Age> Required(Option<int> years) => years.ToFin(new NotFound()).Bind(Age.From);
    public static OptionT<IO, Person> Lookup(Map<string, Person> people, string name) => OptionT.lift<IO, Person>(IO.lift(() => people.Find(name)));
    public static Option<string> Nickname(string? raw) => Optional(raw);
}
```
