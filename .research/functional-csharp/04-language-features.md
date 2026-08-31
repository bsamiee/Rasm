# Functional Programming in C# 7 and Later Versions

C# supports functional programming through these language features:
- tuples carry short-lived groups of values through a pipeline;
- patterns turn branching rules into expressions over input structures;
- readonly structs and init-only properties constrain reassignment, while records support nondestructive mutation;
- nullable static analysis exposes unintended null use before values enter a pipeline.

These features reduce casts, mutation, and repetitive copying while keeping the rules visible.

## Tuples as Intermediate Values

Tuples group related values without a class. Named elements carry lookup results together between `Map` operations on a `Seq<A>`. Here, `Catalogue` supplies the lookup data:

```csharp
internal static class FilmReport {
    public static Seq<string> Render(Seq<int> filmIds) =>
        filmIds
            .Map(static id => (
                Film: Catalogue.GetFilm(id),
                Cast: Catalogue.GetCastList(id)))
            .Map(static x => string.Join(
                Environment.NewLine,
                $"Title: {x.Film.Title}",
                $"Director: {x.Film.Director}",
                $"Cast: {string.Join(", ", x.Cast)}"));
}
```

The first projection pairs each film with its cast. The next projection consumes both values and reduces them to one rendered result. The pipeline no longer needs the tuple after the next transformation.

## Pattern Matching as Branching Logic

Procedural type checks require explicit casts and nested branches. Moving the calculation into virtual methods spreads one complete rule across several classes. Pattern matching keeps recognition, extraction, guards, and results together.

C# provides these pattern-matching features:
- C# 7 type patterns test a runtime type and bind the typed value: `account is PremiumBankAccount premium`.
- C# 7 switch cases with type patterns collect subtype rules, and `when` adds a guard.
- Switch expressions make the decision return a value; `_` is the discard pattern for the fallback arm.
- Property patterns inspect the shape of an object.
- Relational and logical patterns such as `>`, `and`, and `not` express value ranges and exclusions inside a pattern.
- A type pattern need not bind a local when the result does not need subtype data.
- Extended property patterns inspect nested data, such as a player's first name, without extracting each intermediate object first.
- List patterns recognize values at the beginning, middle, or end of arrays with known headers or footers.

### Interest calculation

Ordinary interest equals balance times rate. Premium accounts receive bonus rates above specified balance thresholds; millionaire accounts also earn interest on overflow balance; eligible Monopoly accounts add a bonus for passing Go; and closed accounts earn nothing.

```csharp
internal abstract record StandardBankAccount(decimal Balance, decimal InterestRate);

internal sealed record PremiumBankAccount(decimal Balance, decimal InterestRate, decimal BonusInterestRate) : StandardBankAccount(Balance, InterestRate);
internal sealed record MillionairesBankAccount(decimal Balance, decimal InterestRate, decimal OverflowBalance) : StandardBankAccount(Balance, InterestRate);
internal sealed record Player(string FirstName, string LastName);
internal sealed record MonopolyPlayersBankAccount(
    decimal Balance,
    decimal InterestRate,
    Player Player,
    string CurrSquare,
    decimal PassingGoBonus) : StandardBankAccount(Balance, InterestRate);
internal sealed record ClosedBankAccount(decimal Balance, decimal InterestRate) : StandardBankAccount(Balance, InterestRate);

internal static class Interest {
    public static decimal CalculateInterest(StandardBankAccount account) =>
        account switch {
            PremiumBankAccount { Balance: > 20_000m } p => p.Balance * (p.InterestRate + (p.BonusInterestRate * 1.25m)),
            PremiumBankAccount { Balance: > 10_000m and <= 20_000m } p => p.Balance * (p.InterestRate + p.BonusInterestRate),
            MillionairesBankAccount m => (m.Balance * m.InterestRate) + (m.OverflowBalance * m.InterestRate),
            MonopolyPlayersBankAccount { CurrSquare: not "InJail" } m => (m.Balance * m.InterestRate) + m.PassingGoBonus,
            ClosedBankAccount => 0m,
            _ => account.Balance * account.InterestRate,
        };
}
```

Each arm states an input category beside its outcome. As an expression, the switch can form a function body or be stored in a `Func` and passed to a higher-order function. Bind a subtype only when the right-hand expression needs its properties.

Extended property patterns match nested properties:

```csharp
internal static class Recognition {
    public static bool IsSimon(StandardBankAccount account) =>
        account switch {
            MonopolyPlayersBankAccount { Player.FirstName: "Simon" } => true,
            _ => false,
        };
}
```

## Modeling Alternatives Explicitly

A discriminator flag combined with fields that are meaningful only for one flag value permits invalid combinations. Model the alternatives as distinct variants. `[Union]` on an abstract partial record with nested sealed record cases defines a closed set of cases, and the generated `Switch` takes one arm per case. `Option<A>` is a readonly struct, `Fin<A>` is an abstract class, `Either<L, R>` and `Validation<Error, A>` are record classes, and each of the four types supports `Match`.

### Active patterns

An active pattern runs a custom function during pattern matching and extracts a value on success. This F# example parses a date:

```fsharp
let (|IsDateTime|_|) (input: string) =
    let success, value = DateTime.TryParse input
    if success then Some value else None

let tryParseDateTime input =
    match input with
    | IsDateTime value -> Some value
    | _ -> None
```

`IsDateTime` both decides whether the case matches and supplies the parsed `DateTime` to the result expression.

## Immutability Tools

C# values are not immutable by default, but language features can prevent mutation.

### Readonly structs

Because structs are passed by value, a function receives a copy rather than the original value. A `readonly struct` prevents reassignment of its fields. In C# 7.2, a constructor initializes those fields:

```csharp
internal readonly struct MovieFields {
    public readonly string Title;
    public readonly string Director;
    public readonly Seq<string> Cast;

    public MovieFields(
        string title,
        string director,
        Seq<string> cast) =>
        (Title, Director, Cast) = (title, director, cast);
}
```

`readonly` protects the struct's fields, not objects reached through them. Because `Seq<A>` is immutable, it needs no defensive copy. A mutable child object still needs one.

### Init-only properties

An `init` accessor permits object-initializer syntax but prevents reassignment after initialization:

```csharp
internal readonly struct MovieInit {
    public string Title { get; init; }
    public string Director { get; init; }
    public Seq<string> Cast { get; init; }
}
```

`init` avoids a constructor parameter for every property. It does not make referenced child objects immutable. An `init` property can be omitted from a struct initializer; use `required` for a property that cannot be omitted.

### Records and nondestructive mutation

Records support nondestructive mutation through `with`:

```csharp
internal sealed record Movie {
    public required string Title { get; init; }
    public required string Director { get; init; }
    public Seq<string> Cast { get; init; }
}

internal static class Editions {
    public static Movie DirectorsCut(Movie bladeRunner) =>
        bladeRunner with {Title = $"{bladeRunner.Title} - The Director's Cut",};
}
```

The original remains unchanged, and `with` copies unchanged properties automatically. This avoids manually constructing a complete copy when one property changes. For a state machine, store state in a record, pattern match on the next interaction, and return the next state with a `with` expression.

The copy does not guarantee deep immutability because referenced child objects can remain shared. Nested values must also support immutability.

## Nullable Reference Types at Input Boundaries

Nullable reference types are compiler analysis, not a new runtime type. The compiler warns when a non-nullable property stays uninitialized and when a caller assigns null to a non-nullable reference. Write `?` only where null is a deliberate part of an external representation. A nullable reference enters the domain through `Optional(x)` as an `Option<A>`, and the domain receives no null.
