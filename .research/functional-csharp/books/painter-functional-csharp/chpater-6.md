# Discriminated Unions

## One Type, Several Explicit Cases

A discriminated union is a type whose value is exactly one of several alternatives. A consumer pattern-matches the value to discover its case and gain access to that case's data. A component that does not care which case it has can pass the union onward unchanged.

F# supports discriminated unions directly. C# does not, but can approximate them with:

- an abstract base class representing the union;
- one concrete subclass per case;
- pattern matching wherever behavior differs by case.

This is more than ordinary specialization. The cases may be unrelated alternatives that share only the need to travel through one API or collection.

## Represent Alternatives Instead of Flagged Field Bundles

Suppose a travel system sells holidays and day trips. One class containing every possible field plus an `IsDayTrip` flag creates bundles of irrelevant fields and violates Interface Segregation: every instance carries properties that do not describe its actual case, while names such as `Destination` and `StartDate` are repurposed for concepts better called `Attraction` and `DateOfTrip`.

Keeping the types wholly unrelated preserves their vocabulary, but loses a common type for storing and processing every offering together. An abstract union base provides both:

```csharp
public abstract class CustomerOffering
{
    public int Id { get; init; }
}

public class Holiday : CustomerOffering
{
    public Location Destination { get; init; }
    public Location DepartureAirport { get; init; }
    public DateTime StartDate { get; init; }
    public int DurationOfStay { get; init; }
}

public sealed class HolidayWithMeals : Holiday
{
    public int NumberOfMeals { get; init; }
}

public sealed class DayTrip : CustomerOffering
{
    public DateTime DateOfTrip { get; init; }
    public Location Attraction { get; init; }
    public bool CoachTripRequired { get; init; }
}
```

`DayTrip` is not an extension of `Holiday`; it is an alternative to it. The shared base supports a single `CustomerOffering[]`, while each case retains only meaningful data and terminology.

```csharp
public string Format(CustomerOffering offering) => offering switch
{
    HolidayWithMeals x => FormatHolidayWithMeals(x),
    Holiday x          => FormatHoliday(x),
    DayTrip x          => FormatDayTrip(x),
    _                  => throw new ArgumentOutOfRangeException(nameof(offering))
};
```

When one case derives from another, match the narrower case first: matching `Holiday` first would also capture `HolidayWithMeals`.

The same design handles structurally different naming conventions. A `BritishName` can carry first, middle, and last names with an honorific placed first; a `ChineseName` can carry family, given, courtesy, and honorific fields with a different output order. A common abstract `Name` permits one collection, while the variants preserve meaningful fields and formatting rather than forcing one culture's shape onto another.

## Make Every Expected Function Outcome a Case

A function's return type should describe more than its happy-path payload. Looking up a person has three meaningful outcomes:

1. the person was found;
2. no person has that identifier;
3. the lookup failed.

```csharp
public abstract class PersonLookupResult
{
    public int Id { get; init; }
}

public sealed class PersonFound : PersonLookupResult
{
    public Person Person { get; init; }
}

public sealed class PersonNotFound : PersonLookupResult { }

public sealed class PersonLookupError : PersonLookupResult
{
    public Exception Error { get; init; }
}
```

The effectful function translates each external outcome into one case:

```csharp
public PersonLookupResult GetPerson(int id)
{
    try
    {
        var person = database.LookupPerson(id);
        return person is null
            ? new PersonNotFound { Id = id }
            : new PersonFound { Id = id, Person = person };
    }
    catch (Exception error)
    {
        return new PersonLookupError { Id = id, Error = error };
    }
}
```

The caller pattern-matches `PersonFound`, `PersonNotFound`, and `PersonLookupError`. It does not infer the outcome from `null`, a status flag, or optional metadata.

An operation with no success payload still benefits. Email sending can return either an empty `EmailSuccess` case or an `EmailFailure` carrying the exception. The absence of a success value does not require hiding whether the operation completed.

## Move from Impure Input to Typed Input in Stages

External input is uncontrolled. A console boundary can return one of four cases: text, an integer, no input, or a console error. This turns the move from an impure boundary to controlled program data into a sequence:

1. Keep console access behind a narrow interface, so the uncontrolled console can be replaced by a controlled implementation.
2. Catch console failures at that boundary and return an error case.
3. Classify successfully read text once.
4. Let application code consume the already-classified case.

```csharp
public UserInput Classify(UserInput input) => input switch
{
    TextInput x when string.IsNullOrWhiteSpace(x.Input) => new NoInput(),
    TextInput x when int.TryParse(x.Input, out var value) =>
        new IntegerInput { Input = value },
    _ => input
};
```

Code that requires an integer now handles `IntegerInput` directly and can recursively prompt again for the other cases. Parsing and exception handling are not duplicated at every call site. The side-effecting read remains small; classification can remain ordinary deterministic logic.

## Reusable Generic Unions

Situation-specific cases communicate the domain best, but recurring shapes can be generic. C# pays declaration and repeated-type-argument boilerplate for these encodings.

### Maybe: Something or Nothing

Use `Maybe<T>` when a function may or may not produce a value:

```csharp
public abstract class Maybe<T> { }

public sealed class Something<T> : Maybe<T>
{
    public Something(T value) => Value = value;
    public T Value { get; }
}

public sealed class Nothing<T> : Maybe<T> { }
```

`Something<T>` carries the value; `Nothing<T>` explicitly represents absence. The producer must uphold that distinction by mapping missing data to `Nothing<T>`; this class definition alone does not stop a caller from constructing `Something<T>` with a null payload. If a producer catches an operational error and returns `Nothing<T>`, even after logging it, the caller cannot distinguish failure from a legitimate miss. That prevents an unhandled exception but can hide a useful failure state.

### Result: Success or Failure

Use `Result<T>` when the alternatives are a returned value and an operational failure:

```csharp
public abstract class Result<T> { }

public sealed class Success<T> : Result<T>
{
    public Success(T value) => Value = value;
    public T Value { get; }
}

public sealed class Failure<T> : Result<T>
{
    public Failure(Exception error) => Error = error;
    public Exception Error { get; }
}
```

`Result<T>` preserves the error for the receiver, but `Success<T>` does not by itself prevent its payload from being `null`. A lookup consumer may still need a null check to distinguish a missing value from a present value.

### Three-State Maybe: Something, Nothing, or Error

When presence, absence, and failure all matter, extend the `Maybe<T>` model with a third case:

```csharp
public sealed class Error<T> : Maybe<T>
{
    public Error(Exception exception) => Exception = exception;
    public Exception Exception { get; }
}
```

The producer maps a found value to `Something<T>`, no value to `Nothing<T>`, and an exception to `Error<T>`. The receiver can respond differently to all three without nullable success values or swallowed errors.

The same shape applies to collections. Under the shown producer convention, `Something<IEnumerable<T>>` means one or more results were returned, `Nothing<IEnumerable<T>>` means none were found, and `Error<IEnumerable<T>>` preserves the failed query. The producer checks for both a missing collection and an empty collection before choosing `Nothing`.

### Either: One of Two Value Types

Use `Either<TLeft, TRight>` when both alternatives carry values but the value types differ:

```csharp
public abstract class Either<TLeft, TRight> { }

public sealed class Left<TLeft, TRight> : Either<TLeft, TRight>
{
    public Left(TLeft value) => Value = value;
    public TLeft Value { get; }
}

public sealed class Right<TLeft, TRight> : Either<TLeft, TRight>
{
    public Right(TRight value) => Value = value;
    public TRight Value { get; }
}
```

Pattern matching exposes the correctly typed payload. More than two alternatives can be encoded in the same style, but every added case increases the generic-parameter and class boilerplate.

## Design Rules

- Model distinct valid states as distinct cases, not a boolean discriminator plus bundles of partially meaningful fields.
- Put only genuinely shared data on the abstract base; keep case-specific data on its case.
- Use domain-specific unions when their names explain the possible outcomes better than generic `Maybe`, `Result`, or `Either` names.
- Keep absence and failure separate whenever the receiver needs to respond differently.
- Interpret uncontrolled values once near their source, then pass typed cases inward.
- Pattern-match only where behavior depends on the case; otherwise pass the union onward.
- Expect a C# tradeoff: declaring the union costs boilerplate, but it removes repeated checks and makes downstream state handling descriptive.

The result is a model in which valid alternatives are named, irrelevant field bundles are avoided, and receiving code is directed toward every meaningful outcome.
