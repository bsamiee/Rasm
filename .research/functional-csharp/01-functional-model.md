# Functional Programming in C#

## A paradigm, not a product

Functional programming is a programming paradigm: a style for structuring programs, not a language, API, package, or framework. C# is multi-paradigm, so functional and object-oriented techniques can coexist. Applying functional programming in C# does not require first learning a pure functional language.

The contrast is between imperative and declarative code:
- Imperative code specifies how work proceeds through ordered instructions, mutable variables, loops, and branches.
- Declarative code describes the result and the transformations needed to produce it, leaving more execution detail to the runtime.

SQL illustrates the declarative style: a query describes the desired result without manually implementing every transformation or prescribing the runtime's exact execution order. Functional C# aims for a similar emphasis on what is computed rather than the mechanics of changing state.

Functional programming has a long mathematical and programming lineage. Its roots include work on combinatory logic and lambda expressions, followed by early functional languages such as IPL and LISP. Some languages enforce the functional paradigm; hybrid languages such as C# expose functional features without implementing every part of it.

C# provides these features: delegates and lambdas let functions be passed as values, LINQ supports declarative transformations, switch expressions use pattern matching to return values, and records help express immutable state transitions.

## Core principles

Functional programming is a style built on two commitments:
1. **Treat functions as values.** A function can be assigned to a variable, passed as an argument, returned from another function, or stored in a collection.
2. **Avoid state mutation.** Once created, an object should not change, variables should not be reassigned, and transformations should produce new values instead of destroying prior ones.

These commitments reinforce each other. Functions express transformations, while immutable inputs support reasoning, composition, testing, and concurrency.

```csharp
Func<int, int> triple = static x => x * 3;
Seq<int> source = toSeq(Range(1, 3));
Seq<int> result = source.Map(triple); // 3, 6, 9
```

`Map`, the `Seq<int>` form of LINQ `Select`, receives a function as an argument and returns a new sequence. The original sequence is unchanged.

## Prefer transformations over destructive updates

An in-place update destroys the prior value:

```csharp
List<int> values = [7, 6, 1];
values.Sort(); // values is now 1, 6, 7
```

A functional alternative preserves it:

```csharp
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());              // values remains 7, 6, 1
Seq<int> odd = values.Filter(static x => x % 2 == 1); // 7, 1
```

`Seq<int>` is the immutable sequence, `Filter` selects by a predicate, and `toSeq` builds a `Seq<int>` from the LINQ `Order()` result.

This distinction matters under concurrency. Two readers can safely observe the same stable value. If one concurrent operation reorders a shared list while another sums it, the reader can observe an inconsistent traversal and produce an unpredictable result. Producing a separate ordered view removes that interference.

Functional and object-oriented design are not opposites. Modularity, separation of concerns, layering, and loose coupling apply whether a component is a function or a class. The conflict is between functional transformations and imperative method bodies that mutate shared state or use explicit control flow.

## The core properties

### Immutability

An immutable value is fixed once created. When a different value is needed, derive and return a new value rather than changing the original. `string` and `DateTime` demonstrate this behavior in .NET: operations on them produce new values.

This changes how a program represents progress. Instead of revising an earlier step, each expression produces the next value from values already available. The result resembles a mathematical derivation in which each line remains fixed and later lines build on it.

### Higher-order functions

In C#, delegates represent function values:

```csharp
Func<int, int, string> describeSum = static (x, y) => string.Create(CultureInfo.InvariantCulture, $"{x} + {y} = {x + y}");
Action<string> log = static message => Console.WriteLine($"message received: {message}");
```

`Func<...>` represents behavior that returns a value. `Action<...>` represents `void` behavior. Value-returning delegates compose functions into larger operations. `Action` represents effectful behavior; logging is an effect at the application boundary.

A higher-order function accepts a function, returns a function, or does both. Higher-order functions rely on first-class functions.

### Expressions rather than statements

An expression evaluates to a value. A statement performs an action or controls program execution. Functional code prefers expressions because each step contributes a value to the calculation.

```csharp
internal static partial class CoreProperties {
    public static string Describe(int value) => value == 10
        ? "It was ten"
        : "It was not ten";
}
```

The conditional is an expression because both alternatives return a value. Loops, calls made only for side effects, and branches that direct mutation are statements. The distinction is whether code returns a value, not whether it contains an equals sign.

### Referential transparency and pure functions

A pure function:
- changes nothing outside the function;
- returns the same result for the same arguments, regardless of ambient state;
- has no unexpected side effects, including unexpected exceptions.

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

Causes of impurity include reading or changing object fields, mutating an argument, consulting the ambient clock, performing I/O, or calling behavior whose result is not determined by the current function's arguments.

Expose a variable dependency as input data:

```csharp
internal static partial class CoreProperties {
    public static string TimestampedGreeting(DateTimeOffset now, string? name) =>
        string.Create(CultureInfo.InvariantCulture, $"{now} - Hello {name ?? "Unknown Person"}");
}
```

The clock enters as a `DateTimeOffset` argument, and invariant formatting keeps the result independent of the ambient culture.

Required behavior can be supplied as a delegate rather than hidden behind a call to mutable or unknown external state. If one operation both mutates an object and calculates a result, separate those responsibilities so the calculation can remain pure.

Interaction with users, files, APIs, libraries, and other external systems introduces effects. Keep most application logic pure, and keep unavoidable effects small and explicit.

### Recursion

Recursion replaces some mutation-driven loops with a function that calls itself using new argument values. A recursive function needs:
1. a base case that returns the final value;
2. a recursive case that calls the same function with values closer to that condition;
3. a returned value on every path.

Recursion can replace `while` and `foreach`, but direct recursion can reduce performance in C#.

### Pattern matching

Pattern matching selects a result based on a value's type or properties. C# switch expressions can replace nested conditional statements. They show the relationship between each pattern and its result.

### Immutable state transitions

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

## How the properties reinforce one another

1. Immutability prevents earlier values from changing invisibly.
2. Pure functions transform those values deterministically.
3. Higher-order functions combine small transformations into larger behavior.
4. Each expression returns a value.
5. Pattern matching makes alternative outputs explicit.
6. Recursion or declarative collection operations replace mutation-based iteration.
7. State changes remain possible as immutable state-transition functions.

This combination emphasizes business logic and reduces boilerplate such as counters, flags, temporary variables, mutation, and explicit control-flow code.

## C# as a functional language

Garbage collection makes non-destructive updates practical because superseded versions can be reclaimed. Mutation remains C#'s default: fields and variables must be explicitly constrained, user-defined immutable types require effort, and the standard collections are mutable even though an immutable collections library is available. LanguageExt supplies `Seq<A>`, `Map<K, V>`, `HashMap<K, V>`, and `Set<A>`.

LINQ is C#'s clearest built-in example of functional programming:
- `Select` maps each element through a function.
- `Where` filters through a predicate.
- `OrderBy` and `OrderByDescending` produce ordered sequences from key selectors.
- These operators accept functions and return new sequences instead of modifying their inputs.

The static import of `LanguageExt.Prelude` supplies constructors and functions as bare names: `Some`, `None`, `Seq`, `toSeq`, `Range`, and `parseInt`. `K<F, A>` carries the witness `F` for the type constructor and the element type `A`. The traits `Functor<F>`, `Applicative<F>`, and `Monad<M>` state what a witness supports, so one function serves every type with a witness. `.As()` restores the concrete type at a call site that requires it.

```csharp
internal static partial class Traits {
    public static K<F, int> Tripled<F>(K<F, int> values)
        where F : Functor<F> =>
        values.Map(static x => x * 3);
    public static Option<int> TripledOption() => Tripled(Some(2)).As(); // Some(6)
    public static Seq<int> TripledSeq() => Tripled(Seq(1, 2, 3)).As();  // 3, 6, 9
}
```

These language features reduce functional-code boilerplate:
- `using static` removes type qualification from calls to static functions but can introduce name conflicts.
- Getter-only auto-properties have a compiler-generated readonly backing field and can be assigned only inline or in the constructor, which supports immutable types.
- Expression-bodied members keep small functions readable and composable.
- Local functions keep single-use helpers near their caller.
- Named tuples carry temporary intermediate structures without inventing domain types that have no independent meaning.

## Think in function signatures

A mathematical function maps each value in a domain to a value in a codomain. In a statically typed program, types represent those sets:

```text
char -> char
Person -> Greeting
(T1, T2) -> R
```

The input and output types form the function's contract. This perspective directs attention to what information enters and what value must come out.

A mathematical function's result is determined exclusively by its input. A C# method, delegate, or lambda only represents a function; that representation does not guarantee the same property. It may capture context, read mutable state, or perform effects even when its visible signature does not reveal those dependencies.

C# represents functions as:
- **Methods** are the conventional representation and participate in class and interface design. An instance method can be understood as also taking the current instance as an implicit argument.
- **Delegates** are types that represent methods with a specific signature.
- **Lambdas** define short functions inline and are converted to a compatible delegate type.
- **Dictionaries** directly store arbitrary mappings whose associations cannot be computed. The same representation can retain results of expensive computations instead of recomputing them.

Use the `Func` and `Action` families when only the signature matters. Use a custom delegate when its name conveys domain intent that a generic delegate type such as `Func<T, bool>` does not.

### Arity, tuples, and closures

Arity is the number of arguments a function accepts: nullary, unary, or binary. Any multi-argument function can be viewed as a unary function over a tuple of its arguments.

A closure combines a lambda with the context in which it was declared. The delegate's declared signature may remain unary, but the computation can also depend on captured context:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> DaysStartingWith(string pattern) => days.Filter(day => day.ToString().StartsWith(pattern, StringComparison.Ordinal));
Seq<DayOfWeek> weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
```

The predicate supplied to `Filter` has the signature `DayOfWeek -> bool`, yet it depends on both `day` and the captured `pattern`. The predicate has a one-parameter signature, but its result depends on both values.

## Benefits

### Concision and clarity

Fewer loops, flags, and intermediate updates can shorten the program and clarify its purpose.

### Testability and predictability

A pure function can be tested with input-output pairs because it does not depend on hidden state. Referential transparency makes results repeatable and lets the same failing input reproduce a failure.

Expressions that return values reduce implicit control flow from mutable flags, nested conditionals, and broadly scoped exception handling. Functional style does not prevent error-handling defects, but it makes control flow and effects more explicit.

### Robustness

Immutability prevents bugs caused by later changes. The paradigm also avoids `null` and favors data types that represent absence and errors explicitly.

### Concurrency

Code that does not mutate shared in-memory state supports concurrent execution. Examples include asynchronous processing, multiple workers handling similar inputs, containerized workloads, and serverless functions. Shared external resources can still contend, so stateless application logic reduces but does not eliminate concurrency hazards.

## Where the style fits

Functional programming fits work based on predictable data transformations:
- converting data from one form to another;
- applying business logic to input before passing the result onward;
- asynchronous or concurrent processing;
- serverless functions and independently running workers;
- logic that benefits from deterministic behavior and extensive testing.

C# places limits on the paradigm. Framework base classes and some libraries are object-oriented, and C# cannot express every feature of a pure functional language. Functional C# is not inherently slow, but it does not guarantee the best performance. If performance outweighs readability and modularity, another style can be a better tradeoff.

## Judgment and tradeoffs

Higher-order functions add stack frames for callbacks. Their performance cost depends on the workload, and callbacks can make call stacks harder to debug. Excessive abstraction can obscure behavior.

Use higher-order functions when they separate otherwise interleaved logic, remove duplication, adapt an interface, or create reusable behavior. Keep lambdas short, choose clear names, and format nested callbacks so control flow remains visible.
