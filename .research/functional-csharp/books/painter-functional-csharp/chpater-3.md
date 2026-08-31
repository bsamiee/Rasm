# Functional Coding in C# 7 and Beyond

Modern C# supports functional design through a set of complementary language features:

- tuples carry short-lived groups of values through a pipeline;
- patterns turn branching rules into expressions over data shapes;
- readonly structs and init-only properties constrain reassignment, while records make copy-and-change transitions concise;
- nullable-reference analysis exposes accidental nullability before values enter a pipeline.

These features reduce casts, mutation, and repetitive copying while keeping the important rules visible.

## Tuples as Temporary Pipeline Data

A tuple is a quick way to group related values without declaring and maintaining a class. Named elements are especially useful when several lookup results must travel together between `Select` operations:

```csharp
var renderedFilmDetails = filmIds
    .Select(id => (
        Film: GetFilm(id),
        Cast: GetCastList(id)))
    .Select(x => $@"
        Title: {x.Film.Title}
        Director: {x.Film.Director}
        Cast: {string.Join(", ", x.Cast)}
    ".Trim());
```

The first projection pairs each film with its cast. The next projection consumes both values and reduces them to one rendered result. The tuple is appropriate because this grouped shape is local and disappears after the next transformation.

## Pattern Matching as a Rule Expression

Procedural type checks require explicit casts and nested branches. Moving the calculation into virtual methods spreads one complete rule across several classes. Pattern matching instead keeps recognition, extraction, guards, and results together.

The language's pattern-matching additions build on one another:

- C# 7 type patterns test a runtime type and bind the correctly typed value: `account is PremiumBankAccount premium`.
- C# 7 type-switch cases collect subtype rules, and `when` adds a guard.
- Switch expressions make the entire decision return a value; `_` is the discard pattern for the fallback arm.
- Property patterns inspect the shape of an object directly.
- Relational and logical patterns such as `>`, `and`, and `not` express value ranges and exclusions inside a pattern.
- A type pattern need not bind a local when the result does not need subtype data.
- Extended property patterns inspect nested data, such as a player's first name, without extracting each intermediate object first.
- List patterns can recognize significant values at the beginning, middle, or end of an array, which is useful when a file or device format has recognizable headers or footers.

### One complete calculation

The example calculates ordinary interest as balance times rate. Premium accounts receive bonus rates above specified balance thresholds; millionaire accounts also earn interest on overflow balance; eligible Monopoly accounts add a passing-Go bonus; and closed accounts earn nothing.

```csharp
public static decimal CalculateInterest(StandardBankAccount account) =>
    account switch
    {
        PremiumBankAccount { Balance: > 20_000m } p =>
            p.Balance * (p.InterestRate + p.BonusInterestRate * 1.25m),

        PremiumBankAccount { Balance: > 10_000m and <= 20_000m } p =>
            p.Balance * (p.InterestRate + p.BonusInterestRate),

        MillionairesBankAccount m =>
            (m.Balance * m.InterestRate) +
            (m.OverflowBalance * m.InterestRate),

        MonopolyPlayersBankAccount { CurrSquare: not "InJail" } m =>
            (m.Balance * m.InterestRate) + m.PassingGoBonus,

        ClosedBankAccount => 0m,

        _ => account.Balance * account.InterestRate
    };
```

Each arm states an input category beside its outcome. The switch is an expression, so it can form the body of a small function or be stored in a `Func` and passed as a higher-order argument. Bind a subtype only when the right-hand expression needs its properties. Keep alternatives that belong to one decision in the same expression so the complete rule remains readable in one place.

Extended property patterns keep recognition of nested shapes equally direct:

```csharp
var isSimon = account is MonopolyPlayersBankAccount
{
    Player.FirstName: "Simon"
};
```

The pattern reaches through `Player` to `FirstName` without first binding each intermediate object. List patterns apply the same idea to significant values at the beginning, middle, or end of an array, which can distinguish data with known headers or footers.

## Modeling Alternatives Explicitly

A discriminator flag combined with fields that are meaningful only for one flag value permits invalid combinations:

```csharp
public sealed class Customer
{
    public string Email { get; init; }
    public bool IsRegistered { get; init; }
    public string Name { get; init; }
    public bool IsEligible { get; init; }
}
```

When `IsRegistered` is false, callers must remember that `Name` and `IsEligible` have no meaning. Model the alternatives as distinct variants instead:

```csharp
public abstract class Customer { }

public sealed class RegisteredCustomer : Customer
{
    public string Name { get; init; }
    public string Email { get; init; }
    public bool IsEligible { get; init; }
}

public sealed class GuestCustomer : Customer
{
    public string Email { get; init; }
}
```

Consumers can pattern match on descriptive cases, and each case carries only the data it needs. This approximates a discriminated union in C#, although the abstract base and variant declarations require more ceremony than a native union definition.

### Active patterns

An active pattern lets a custom function perform recognition on the pattern side and extract a value when it succeeds. The chapter illustrates the idea in F# with a date parser:

```fsharp
let (|IsDateTime|_|) (input: string) =
    let success, value = DateTime.TryParse input
    if success then Some value else None

let tryParseDateTime input =
    match input with
    | IsDateTime value -> Some value
    | _ -> None
```

`IsDateTime` both decides whether the case matches and supplies the parsed `DateTime` to the result expression. The chapter presents this as an F# capability rather than a C# feature.

## Immutability Tools

C# does not make values immutable by default, but several features reduce the work needed to keep state stable.

### Readonly structs

Structs are passed by value, so a function receives a copy rather than the original value. A `readonly struct` additionally prevents reassignment of its fields. In the C# 7.2 readonly-field form, a constructor assigns the intended field values:

```csharp
public readonly struct Movie
{
    public readonly string Title;
    public readonly string Director;
    public readonly IEnumerable<string> Cast;

    public Movie(
        string title,
        string director,
        IEnumerable<string> cast) =>
        (Title, Director, Cast) = (title, director, cast);
}
```

`readonly` protects the struct's own slots, not objects reached through them. A mutable collection or child object can still be changed independently, so the referenced values must also be chosen carefully.

### Init-only properties

An `init` accessor permits object-initializer syntax but prevents reassignment after initialization:

```csharp
public readonly struct Movie
{
    public string Title { get; init; }
    public string Director { get; init; }
    public IEnumerable<string> Cast { get; init; }
}
```

This avoids a constructor parameter for every property while preserving a stable outer value. It still does not make referenced collections or child objects immutable.

### Records and non-destructive updates

Records make copy-and-change state transitions concise with `with`:

```csharp
public record Movie
{
    public string Title { get; init; }
    public string Director { get; init; }
    public IEnumerable<string> Cast { get; init; }
}

var directorsCut = bladeRunner with
{
    Title = $"{bladeRunner.Title} - The Director's Cut"
};
```

The original remains unchanged, and unchanged properties are copied automatically. This removes the boilerplate of manually constructing a complete copy whenever one property changes. It is particularly effective for state machines: hold progress in a record, pattern match on the next interaction, and return the next state with a focused `with` expression.

The copy is not a guarantee of deep immutability. Referenced collections and child objects can still be shared, so the nested values must also support the intended immutable design.

## Nullable Reference Types as a Boundary Guard

Nullable reference types are compiler analysis, not a new runtime type. Enable the analysis in the project file:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

The compiler then warns when a non-nullable property may be uninitialized or when `null` is assigned to a non-nullable reference. These are warnings; enabling the feature does not itself change runtime behavior.

Use `?` only when null is a deliberate part of the representation:

```csharp
public record ExternalMovie
{
    public string? Title { get; init; }
    public string? Director { get; init; }
    public IEnumerable<string>? Cast { get; init; }
}
```

Null adds another possible state that every consumer must account for. If an external data source requires nullable values, isolate that representation in the parsing code and convert it to a safer internal shape before passing it through the rest of the system.

Together, these tools keep data shapes, complete decisions, and returned state visible while reducing casts, hidden mutation, and repetitive copying.
