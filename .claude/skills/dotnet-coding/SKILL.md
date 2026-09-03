---
name: dotnet-coding
description: "MAKE DESCRIPTION LATER, 20<25 WORDS"
---

# [DOTNET_CODING]

Write functional programming (FP) expression-oriented code, express what to compute not how, describe the result and the transformations needed to produce it. Build complex operations from smaller ones. Emphasize immutability, pure functions as first-class, higher-order functions, and declarative data transformations:
- Immutability: Data does not change after creation, prevents bugs caused by later changes, avoid `null`, use data types that represent absence and errors explicitly
- Pure functions: Same input always produces same output, no side effects
- Higher-order functions: Functions that take or return functions

Mutation is C#'s default, fields and variables must be explicitly constrained, standard collections are mutable, use LanguageExt for immutable collections. Avoid state mutation, once created, an object does not change, variables are not reassigned, and transformations produce new values instead of destroying prior ones

## [01]-[FUNCTION_SIGNATURES]

Methods, delegates, and lambdas represent functions WITHOUT guaranteeing purity, they can capture context, read mutable state, or perform effects their signatures do not reveal:
- Methods are the conventional representation for class and interface design, with an instance method implicitly also taking the current instance as an argument
- Delegates are types that represent methods with a specific signature
- Lambdas define short functions inline and are converted to a compatible delegate type
- Dictionaries directly store arbitrary mappings that CANNOT be computed, and can retain results of expensive computations instead of recomputing them

```text
char -> char
Person -> Greeting
(T1, T2) -> R
```

The input and output types form the function's contract, what information enters and what value must come out:
- Use the `Func` and `Action` families when only the signature matters
- Use a custom delegate when its name conveys domain intent that a generic delegate type (`Func<T, bool>`) does not

## [02]-[LANGUAGE_FEATURES]

These language features support FP, reduce casts, mutation, and repeated code:
- Tuples carry short-lived groups of values without a class, named elements carry lookup results together between `Map` operations on a `Seq<A>`
- Named tuples carry temporary intermediate structures without inventing domain types that have no independent meaning
- Pattern matching keeps recognition, extraction, guards, and results together, and turns branching rules into expressions over input structures
- Expression-bodied members keep small functions readable and composable
- Nullable static analysis exposes unintended null use before values enter a pipeline
- readonly structs and init-only properties constrain reassignment, while records support nondestructive mutation
- LanguageExt supplies `Seq<A>`, `Map<K, V>`, `HashMap<K, V>`, and `Set<A>` immutable collections
- `using static` removes type qualification from calls to static functions but can introduce name conflicts
- Getter-only auto-properties have a compiler-generated readonly backing field and can be assigned only inline or in the constructor, which supports immutable types

The static import of `LanguageExt.Prelude` supplies constructors and functions as bare names: `Some`, `None`, `Seq`, `toSeq`, `Range`, and `parseInt`. `K<F, A>` pairs the witness `F` for the type constructor with the element type `A`. Traits (`Functor<F>`) state what a witness supports, and `.As()` restores the concrete type.

```csharp
internal static partial class Traits {
    public static K<F, int> Tripled<F>(K<F, int> values)
        where F : Functor<F> => values.Map(static x => x * 3);
    public static Option<int> TripledOption() => Tripled(Some(2)).As(); // Some(6)
    public static Seq<int> TripledSeq() => Tripled(Seq(1, 2, 3)).As();  // 3, 6, 9
}
```

LINQ:
- `Select` maps each element through a function
- `Where` filters through a predicate
- `OrderBy` and `OrderByDescending` produce ordered sequences from key selectors
- These operators accept functions and return new sequences instead of modifying their inputs

```csharp
Func<int, int> triple = static x => x * 3;
Seq<int> source = toSeq(Range(1, 3));
Seq<int> result = source.Map(triple); // 3, 6, 9
```

`Map`, the `Seq<int>` form of LINQ `Select`, receives a function as an argument and returns a new sequence, the original sequence is unchanged.

### [02.1]-[TUPLES]

Tuples group related values without a class. Named elements carry lookup results together between `Map` operations on a `Seq<A>`. The first projection pairs each film with its cast, the next projection consumes both values and reduces them to one rendered result. The pipeline no longer needs the tuple after the next transformation.

```csharp
internal static class FilmReport {
    public static Seq<string> Render(Seq<int> filmIds) =>
        filmIds
            .Map(static id => (
                Film: Catalogue.GetFilm(id),      // `Catalogue` supplies the lookup data
                Cast: Catalogue.GetCastList(id))) // `Catalogue` supplies the lookup data
            .Map(static x => string.Join(
                Environment.NewLine,
                $"Title: {x.Film.Title}",
                $"Director: {x.Film.Director}",
                $"Cast: {string.Join(", ", x.Cast)}"));
}
```

## [03]-[TRANSFORMATIONS_OVER_MUTATION]

This distinction matters under concurrency. Two readers can safely observe the same stable value. If one concurrent operation reorders a shared list while another sums it, the reader can observe an inconsistent traversal and produce an unpredictable result. Producing a separate ordered view removes that interference. Modularity, separation of concerns, layering, and loose coupling apply whether a component is a function or a class.

```csharp
// BAD: In-place updates destroy the prior value
List<int> values = [7, 6, 1];
values.Sort(); // values is now 1, 6, 7

// GOOD: Functional alternatives preserve it
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());              // values remains 7, 6, 1
Seq<int> odd = values.Filter(static x => x % 2 == 1); // 7, 1
```

`Seq<int>` is the immutable sequence, `Filter` selects by a predicate, and `toSeq` builds a `Seq<int>` from the LINQ `Order()` result.

## [04]-[CORE_PRINCIPLES]

EXPRESSION:
- Expressions evaluate to values, statements perform an action or control execution.
- Each step contributes a value to the calculation

```csharp
internal static partial class CoreProperties {
    public static string Describe(int value) => value == 10
        ? "It was ten"
        : "It was not ten";
}
```

The conditional is an expression because both alternatives return a value. Loops, calls made only for side effects, and branches that direct mutation are statements.

RECURSION: Recursive functions need all of the following:
1. Base case that returns the final value
2. Recursive case that calls the same function with values closer to that condition
3. Returned value on every path

Recursion can replace `while` and `foreach`, and some mutation-driven loops with a function that calls itself using new argument values.

ARITY:
- Arity is the number of arguments a function accepts: nullary, unary, or binary
- Any multi-argument function can be viewed as a unary function over a tuple of its arguments

Closures combine a lambda with its declaring context. The delegate's declared signature stays unary, but the computation can also depend on captured context:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> DaysStartingWith(string pattern) => days.Filter(day => day.ToString().StartsWith(pattern, StringComparison.Ordinal));
Seq<DayOfWeek> weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
```

The predicate supplied to `Filter` has the signature `DayOfWeek -> bool`, yet it depends on both `day` and the captured `pattern`.

### [04.1]-[IMMUTABILITY]

Immutability is achieved through `record`, `readonly`, and `init`. Immutable values are fixed once created. When a different value is needed, derive and return a new value rather than changing the original, each expression produces the next value from values already available.

`readonly`:
- Structs are passed by value, functions receives a copy rather than the original value
- `readonly struct` prevents reassignment of its fields
- `readonly` protects the struct's fields, NOT objects reached through them

```csharp
internal readonly struct MovieFields {
    public readonly string Title;
    public readonly string Director;
    public readonly Seq<string> Cast; // `Seq<A>` is immutable, it needs no defensive copy, mutable child objects still need one
    public MovieFields(
        string title,
        string director,
        Seq<string> cast) => (Title, Director, Cast) = (title, director, cast);
}
```

`record`:
- Records support nondestructive mutation through `with`, the original remains unchanged, `with` copies unchanged properties automatically
- This avoids manually constructing a complete copy when one property changes
- The copy does not guarantee deep immutability because referenced child objects can remain shared, nested values must also support immutability
- For state machines, store state in a record, pattern match on the next interaction, and return the next state with a `with` expression

```csharp
internal sealed record Movie {
    public required string Title { get; init; }
    public required string Director { get; init; }
    public Seq<string> Cast { get; init; }
}

internal static class Editions {
    public static Movie DirectorsCut(Movie bladeRunner) => bladeRunner with {Title = $"{bladeRunner.Title} - The Director's Cut",};
}
```

`Init`:
- `init` accessors permit object-initializer syntax but prevent reassignment after initialization
- `init` avoids a constructor parameter for every property, it does not make referenced child objects immutable
- `init` properties can be omitted from a struct initializer
- Use `required` for a property that cannot be omitted.

```csharp
internal readonly struct MovieInit {
    public string Title { get; init; }
    public string Director { get; init; }
    public Seq<string> Cast { get; init; }
}
```

### [04.2]-[PURE_FUNCTIONS]

Pure functions change nothing outside the function, return the same result for the same arguments, regardless of ambient state, and have no unexpected side effects, including unexpected exceptions Referential tarnsparency is when a function call can be replaced with its result for a given input without changing program bheaviopr:

```csharp
internal static partial class CoreProperties {
    public static int Add(int left, int right) => left + right;
    public static string Greeting(string? name) =>
        "Hello " + (string.IsNullOrWhiteSpace(name)
            ? "Unknown Person"
            : name);
}
```

Expose a variable dependency as input data:

```csharp
internal static partial class CoreProperties {
    // The clock enters as a `DateTimeOffset` argument, and invariant formatting keeps the result independent of the ambient culture
    public static string TimestampedGreeting(DateTimeOffset now, string? name) => string.Create(CultureInfo.InvariantCulture, $"{now} - Hello {name ?? "Unknown Person"}");
}
```

Causes of impurity include reading or changing object fields, mutating an argument, consulting the ambient clock, performing I/O, or calling behavior that the current function's arguments do not determine. Interaction with users, files, APIs, libraries, and other external systems introduces effects. Keep most application logic pure, and keep unavoidable effects small and explicit.

### [04.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or do both, and they rely on first-class functions. Delegates represent function values:

```csharp
Func<int, int, string> describeSum = static (x, y) => string.Create(CultureInfo.InvariantCulture, $"{x} + {y} = {x + y}");
Action<string> log = static message => Console.WriteLine($"message received: {message}"); // Logging is an effect at the application boundary
```

Value-returning delegates compose functions into larger operations:
- `Func<...>` represents behavior that returns a value
- `Action<...>` represents `void` behavior
- `Action` represents effectful behavior

## [6]-[PATTERN_MATCHING]

Pattern matching selects a result based on a value's type or properties, and keeps recognition, extraction, guards, and results together:
- Switch expressions can replace nested conditional statements
- Procedural type checks require explicit casts and nested branches
- Moving the calculation into virtual methods spreads one complete rule across several classes

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

Pattern-matching features:
- Type patterns test a runtime type and bind the typed value: `account is PremiumBankAccount premium`
- Switch cases with type patterns collect subtype rules, and `when` adds a guard
- Switch expressions make the decision return a value, and `_` is the discard pattern for the fallback arm
- Relational and logical patterns (`>`, `and`, `not`) express value ranges and exclusions inside a pattern
- Type patterns do not need to bind a local when the result ignores subtype data
- Property patterns inspect the shape of an object, extended property patterns inspect nested data without extracting each intermediate object first
- List patterns recognize values at the beginning, middle, or end of arrays with known headers or footers



















## [XXXXXXXXXXXX]-[STATE_TRANSITIONS]

Stateless programming does not mean information can never change. It means a change is represented as a function from an old immutable state to a new state rather than as an update to a central mutable object.

```csharp
internal sealed record DoctorWho(int NumberOfStories, int CurrentDoctor, string CurrentDoctorActor, int SeasonNumber);

internal static partial class CoreProperties {
    public static DoctorWho RegenerateDoctor(DoctorWho oldState, string newActorName) =>
        oldState with {
            CurrentDoctor = oldState.CurrentDoctor + 1,
            CurrentDoctorActor = newActorName,
        };
}
```

The operation receives the state and all information required for the transition and returns the replacement. Records reduce the copying needed to express this immutable state-transition model.