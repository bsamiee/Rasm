# Functional Coding in C# 7 and Beyond

Modern C# supports functional design through a set of complementary language features:
- tuples carry short-lived groups of values through a pipeline;
- patterns turn branching rules into expressions over data shapes;
- readonly structs and init-only properties constrain reassignment, while records make copy-and-change transitions concise;
- nullable-reference analysis exposes accidental nullability before values enter a pipeline.

These features reduce casts, mutation, and repetitive copying while keeping the important rules visible.

## Tuples as Temporary Pipeline Data

A tuple is a quick way to group related values without declaring and maintaining a class. Named elements are especially useful when several lookup results must travel together between `Map` operations on a `Seq<A>`, where `Catalogue` is the lookup source the pipeline reads:

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

Each arm states an input category beside its outcome. The switch is an expression, so it can form the body of a small function or be stored in a `Func` and passed as a higher-order argument. Bind a subtype only when the right-hand expression needs its properties. Keep alternatives that belong to one decision in the same expression so the complete rule remains readable in one place.

Extended property patterns keep recognition of nested shapes equally direct:

```csharp
internal static class Recognition {
    public static bool IsSimon(StandardBankAccount account) =>
        account switch {
            MonopolyPlayersBankAccount { Player.FirstName: "Simon" } => true,
            _ => false,
        };
}
```

The pattern reaches through `Player` to `FirstName` without first binding each intermediate object. List patterns apply the same idea to significant values at the beginning, middle, or end of an array, which can distinguish data with known headers or footers.

## Modeling Alternatives Explicitly

A discriminator flag combined with fields that are meaningful only for one flag value permits invalid combinations. Model the alternatives as distinct variants instead. `[Union]` on an abstract partial record with nested sealed record cases closes the set, and the generated `Switch` takes one arm per case. `Option<A>` is a readonly struct, `Fin<A>` is an abstract class, `Either<L, R>` and `Validation<Error, A>` are record classes, and `Match` reads all four.

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

`readonly` protects the struct's own slots, not objects reached through them. `Seq<A>` is immutable, so the nested value needs no defensive copy. A mutable child object still needs one.

### Init-only properties

An `init` accessor permits object-initializer syntax but prevents reassignment after initialization:

```csharp
internal readonly struct MovieInit {
    public string Title { get; init; }
    public string Director { get; init; }
    public Seq<string> Cast { get; init; }
}
```

This avoids a constructor parameter for every property while preserving a stable outer value. It still does not make a referenced child object immutable. A struct with `init` properties admits an initializer that omits a property, so mark a property the value cannot do without as `required`.

### Records and non-destructive updates

Records make copy-and-change state transitions concise with `with`:

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

The original remains unchanged, and unchanged properties are copied automatically. This removes the boilerplate of manually constructing a complete copy whenever one property changes. It is particularly effective for state machines: hold progress in a record, pattern match on the next interaction, and return the next state with a focused `with` expression.

The copy is not a guarantee of deep immutability. A referenced child object can still be shared, so the nested values must also support the intended immutable design.

## Nullable Reference Types as a Boundary Guard

Nullable reference types are compiler analysis, not a new runtime type. The compiler warns when a non-nullable property stays uninitialized and when a caller assigns null to a non-nullable reference. Write `?` only where null is a deliberate part of an external representation. That representation stops at the boundary that reads it: a nullable reference enters the domain through `Optional(x)` as an `Option<A>`, and the domain receives no null.

Together, these tools keep data shapes, complete decisions, and returned state visible while reducing casts, hidden mutation, and repetitive copying.
