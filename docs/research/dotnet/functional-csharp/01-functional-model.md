<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
# [FUNCTIONAL_MODEL]

## [01]-[PARADIGM]

Functional programming is a programming paradigm: a style for structuring programs, not a language, API, package, or framework. C# is multi-paradigm: functional and object-oriented techniques coexist. Applying functional programming in C# does not require first learning a pure functional language.

The contrast is between imperative and declarative code:
- Imperative code specifies how work proceeds through ordered instructions, mutable variables, loops, and branches
- Declarative code describes the result and the transformations needed to produce it, leaving more execution detail to the runtime

C# provides these features: delegates and lambdas let functions be passed as values, LINQ supports declarative transformations, switch expressions use pattern matching to return values, and records help express immutable state transitions.

## [02]-[CORE_PRINCIPLES]

Functional programming is a style built on commitments:
1. Treat functions as values. Functions can be assigned to a variable, passed as an argument, returned from another function, or stored in a collection.
2. Avoid state mutation. Once created, an object does not change, variables are not reassigned, and transformations produce new values instead of destroying prior ones.

These commitments reinforce each other. Functions express transformations, while immutable inputs support reasoning, composition, testing, and concurrency.

```csharp
Func<int, int> triple = static x => x * 3;
Seq<int> source = toSeq(Range(1, 3));
Seq<int> result = source.Map(triple); // 3, 6, 9
```

`Map`, the `Seq<int>` form of LINQ `Select`, receives a function as an argument and returns a new sequence. The original sequence is unchanged.

## [03]-[TRANSFORMATIONS_OVER_MUTATION]

In-place updates destroy the prior value:

```csharp
List<int> values = [7, 6, 1];
values.Sort(); // values is now 1, 6, 7
```

Functional alternatives preserve it:

```csharp
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());              // values remains 7, 6, 1
Seq<int> odd = values.Filter(static x => x % 2 == 1); // 7, 1
```

`Seq<int>` is the immutable sequence, `Filter` selects by a predicate, and `toSeq` builds a `Seq<int>` from the LINQ `Order()` result.

This distinction matters under concurrency. Two readers can safely observe the same stable value. If one concurrent operation reorders a shared list while another sums it, the reader can observe an inconsistent traversal and produce an unpredictable result. Producing a separate ordered view removes that interference.

Functional and object-oriented design are not opposites. Modularity, separation of concerns, layering, and loose coupling apply whether a component is a function or a class. The conflict is between functional transformations and imperative method bodies that mutate shared state or use explicit control flow.

## [04]-[CORE_PROPERTIES]

### [04.1]-[IMMUTABILITY]

Immutable values are fixed once created. When a different value is needed, derive and return a new value rather than changing the original. `string` and `DateTime` demonstrate this behavior: operations on them produce new values.

This changes how a program represents progress. Instead of revising an earlier step, each expression produces the next value from values already available. The result resembles a mathematical derivation in which each line remains fixed and later lines build on it.

### [04.2]-[HIGHER_ORDER_FUNCTIONS]

Delegates represent function values:

```csharp
Func<int, int, string> describeSum = static (x, y) => string.Create(CultureInfo.InvariantCulture, $"{x} + {y} = {x + y}");
Action<string> log = static message => Console.WriteLine($"message received: {message}");
```

`Func<...>` represents behavior that returns a value. `Action<...>` represents `void` behavior. Value-returning delegates compose functions into larger operations. `Action` represents effectful behavior. Logging is an effect at the application boundary.

Higher-order functions accept a function, return one, or do both. Higher-order functions rely on first-class functions.

### [04.3]-[EXPRESSIONS]

Expressions evaluate to values. Statements perform an action or control execution. Functional code prefers expressions because each step contributes a value to the calculation.

```csharp
internal static partial class CoreProperties {
    public static string Describe(int value) => value == 10
        ? "It was ten"
        : "It was not ten";
}
```

The conditional is an expression because both alternatives return a value. Loops, calls made only for side effects, and branches that direct mutation are statements. The distinction is whether code returns a value, not whether it contains an equals sign.

### [04.4]-[REFERENTIAL_TRANSPARENCY]

Pure functions:
- Change nothing outside the function
- Return the same result for the same arguments, regardless of ambient state
- Have no unexpected side effects, including unexpected exceptions

Such a function call can be replaced with its result for a given input without changing program behavior. This is referential transparency.

```csharp
internal static partial class CoreProperties {
    public static int Add(int left, int right) => left + right;
    public static string Greeting(string? name) =>
        "Hello " + (string.IsNullOrWhiteSpace(name)
            ? "Unknown Person"
            : name);
}
```

Causes of impurity include reading or changing object fields, mutating an argument, consulting the ambient clock, performing I/O, or calling behavior that the current function's arguments do not determine.

Expose a variable dependency as input data:

```csharp
internal static partial class CoreProperties {
    public static string TimestampedGreeting(DateTimeOffset now, string? name) =>
        string.Create(CultureInfo.InvariantCulture, $"{now} - Hello {name ?? "Unknown Person"}");
}
```

The clock enters as a `DateTimeOffset` argument, and invariant formatting keeps the result independent of the ambient culture.

Interaction with users, files, APIs, libraries, and other external systems introduces effects. Keep most application logic pure, and keep unavoidable effects small and explicit.

### [04.5]-[RECURSION]

Recursion can replace `while` and `foreach`, and some mutation-driven loops with a function that calls itself using new argument values. Recursive functions need:
1. Base case that returns the final value
2. Recursive case that calls the same function with values closer to that condition
3. Returned value on every path

### [04.6]-[PATTERN_MATCHING]

Pattern matching selects a result based on a value's type or properties. Switch expressions can replace nested conditional statements. They show the relationship between each pattern and its result.

### [04.7]-[STATE_TRANSITIONS]

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

## [05]-[LANGUAGE_SUPPORT]

Garbage collection makes non-destructive updates practical because superseded versions can be reclaimed. Mutation remains C#'s default: fields and variables must be explicitly constrained, user-defined immutable types require effort, and the standard collections are mutable even though an immutable collections library is available. LanguageExt supplies `Seq<A>`, `Map<K, V>`, `HashMap<K, V>`, and `Set<A>`.

LINQ is the clearest built-in example of functional programming:
- `Select` maps each element through a function
- `Where` filters through a predicate
- `OrderBy` and `OrderByDescending` produce ordered sequences from key selectors
- These operators accept functions and return new sequences instead of modifying their inputs
-->

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
The static import of `LanguageExt.Prelude` supplies constructors and functions as bare names: `Some`, `None`, `Seq`, `toSeq`, `Range`, and `parseInt`. `K<F, A>` pairs the witness `F` for the type constructor with the element type `A`. Traits (`Functor<F>`) state what a witness supports, and `.As()` restores the concrete type.

```csharp
internal static partial class Traits {
    public static K<F, int> Tripled<F>(K<F, int> values)
        where F : Functor<F> =>
        values.Map(static x => x * 3);
    public static Option<int> TripledOption() => Tripled(Some(2)).As(); // Some(6)
    public static Seq<int> TripledSeq() => Tripled(Seq(1, 2, 3)).As();  // 3, 6, 9
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
These language features reduce functional-code boilerplate:
- `using static` removes type qualification from calls to static functions but can introduce name conflicts
- Getter-only auto-properties have a compiler-generated readonly backing field and can be assigned only inline or in the constructor, which supports immutable types
- Expression-bodied members keep small functions readable and composable
- Local functions keep single-use helpers near their caller
- Named tuples carry temporary intermediate structures without inventing domain types that have no independent meaning

## [06]-[FUNCTION_SIGNATURES]

Mathematical functions map each domain value to a codomain value. In a statically typed program, types represent those sets:

```text
char -> char
Person -> Greeting
(T1, T2) -> R
```

The input and output types form the function's contract. This perspective directs attention to what information enters and what value must come out.

Methods, delegates, or lambdas represent functions without guaranteeing purity. They can capture context, read mutable state, or perform effects their signatures do not reveal.

C# represents functions as:
- Methods are the conventional representation and participate in class and interface design, with an instance method implicitly also taking the current instance as an argument
- Delegates are types that represent methods with a specific signature
- Lambdas define short functions inline and are converted to a compatible delegate type
- Dictionaries directly store arbitrary mappings that cannot be computed, and can retain results of expensive computations instead of recomputing them

Use the `Func` and `Action` families when only the signature matters. Use a custom delegate when its name conveys domain intent that a generic delegate type (`Func<T, bool>`) does not.

### [06.1]-[ARITY_TUPLES_CLOSURES]

Arity is the number of arguments a function accepts: nullary, unary, or binary. Any multi-argument function can be viewed as a unary function over a tuple of its arguments.

Closures combine a lambda with its declaring context. The delegate's declared signature stays unary, but the computation can also depend on captured context:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> DaysStartingWith(string pattern) => days.Filter(day => day.ToString().StartsWith(pattern, StringComparison.Ordinal));
Seq<DayOfWeek> weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
```

The predicate supplied to `Filter` has the signature `DayOfWeek -> bool`, yet it depends on both `day` and the captured `pattern`.

## [07]-[BENEFITS]

### [07.1]-[CLARITY]

Fewer loops, flags, and intermediate updates can shorten the program and clarify its purpose.

### [07.2]-[TESTABILITY]

Input-output pairs test a pure function because it does not depend on hidden state. Referential transparency makes results repeatable and lets the same failing input reproduce a failure.

Expressions that return values reduce implicit control flow from mutable flags, nested conditionals, and broadly scoped exception handling. Functional style does not prevent error-handling defects, but it makes control flow and effects more explicit.

### [07.3]-[SAFETY]

Immutability prevents bugs caused by later changes. The paradigm also avoids `null` and favors data types that represent absence and errors explicitly.

### [07.4]-[CONCURRENCY]

Code that does not mutate shared in-memory state supports concurrent execution. Examples include asynchronous processing, multiple workers handling similar inputs, containerized workloads, and serverless functions. Shared external resources can still contend. Stateless application logic reduces but does not eliminate concurrency hazards.

## [08]-[APPLICABILITY]

Functional programming fits work based on predictable data transformations:
- Converting data from one form to another
- Applying business logic to input before passing the result onward
- Asynchronous or concurrent processing
- Serverless functions and independently running workers
- Logic that benefits from deterministic behavior and extensive testing

C# places limits on the paradigm. Framework base classes and some libraries are object-oriented, and C# cannot express every feature of a pure functional language. Functional C# is not inherently slow, but it does not guarantee the best performance. If performance outweighs readability and modularity, another style can be a better tradeoff.
-->
