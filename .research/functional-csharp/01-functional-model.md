# The Functional Model

## A paradigm, not a product

Functional programming is a programming paradigm: a style for structuring programs, not a language, API, package, or framework. C# is multi-paradigm, so functional and object-oriented techniques can coexist. Applying functional programming in C# does not require first learning a pure functional language.

The central contrast is between imperative and declarative code:
- Imperative code specifies how work proceeds through ordered instructions, mutable variables, loops, and branches.
- Declarative code describes the result and the transformations needed to produce it, leaving more execution detail to the runtime.

SQL illustrates the declarative mindset: a query describes the desired result without manually implementing every transformation or prescribing the runtime's exact execution order. Functional C# aims for a similar emphasis on what is computed rather than the mechanics of changing state.

Functional programming has a long mathematical and programming lineage rather than being a recent trend. Its roots include work on combinatory logic and lambda expressions, followed by early functional languages such as IPL and LISP. Some languages enforce the functional paradigm; hybrid languages such as C# expose useful functional features without implementing every part of it.

C# provides several important building blocks: delegates and lambdas make behavior passable as values, LINQ supports declarative transformations, switch expressions provide value-producing pattern matching, and records make immutable state transitions easier to express.

## The functional model

Functional programming is a style built on two commitments:
1. **Treat functions as values.** A function can be assigned to a variable, passed as an argument, returned from another function, or stored in a collection.
2. **Avoid state mutation.** Once created, an object should not change, variables should not be reassigned, and transformations should produce new values instead of destroying prior ones.

These commitments reinforce each other. Functions express small transformations, while immutable inputs make those transformations easier to reason about, compose, test, and run concurrently.

```csharp
Func<int, int> triple = static x => x * 3;
Seq<int> source = toSeq(Range(1, 3));
Seq<int> result = source.Map(triple); // 3, 6, 9
```

`Map`, the `Seq<int>` form of LINQ `Select`, receives behavior as data and returns a new sequence. The original sequence is unchanged.

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

This distinction becomes critical under concurrency. Two readers can safely observe the same stable value. If one concurrent operation reorders a shared list while another sums it, the reader can observe an inconsistent traversal and produce an unpredictable result. Producing a separate ordered view removes that interference.

Avoiding mutation eliminates complexities caused by mutable state. A functional design often makes later concurrency substantially easier because concurrent reads of stable values do not create inconsistencies.

Functional and object-oriented design are not opposites. Modularity, separation of concerns, layering, and loose coupling apply whether a component is a function or a class. The practical conflict is usually between functional transformations and imperative method bodies that mutate shared state or use explicit control flow.

## The core properties

### Immutability

An immutable value is fixed once created. When a different value is needed, derive and return a new value rather than changing the original. `string` and `DateTime` demonstrate this behavior in .NET: operations on them produce new values.

This changes how a program represents progress. Instead of revising an earlier step, each expression produces the next value from values already available. The result resembles mathematical working in which each line remains fixed and later lines build on it.

### Higher-order functions

Functions can be stored in variables, passed as arguments, and returned from other functions. In C#, delegates provide this capability:

```csharp
Func<int, int, string> describeSum = static (x, y) => string.Create(CultureInfo.InvariantCulture, $"{x} + {y} = {x + y}");
Action<string> log = static message => Console.WriteLine($"message received: {message}");
```

`Func<...>` represents behavior that returns a value. `Action<...>` represents `void` behavior. Value-returning delegates can be composed into larger operations from small functions. `Action` commonly marks effectful behavior; logging is an example of an effect that may remain necessary at the application's impure edge.

A higher-order function accepts a function, returns a function, or does both. This is the main capability unlocked by first-class functions.

### Expressions rather than statements

An expression evaluates to a value. A statement primarily issues an instruction or changes the route through the program. Functional code prefers expressions because each step contributes a value to the calculation.

```csharp
internal static partial class CoreProperties {
    public static string Describe(int value) => value == 10
        ? "It was ten"
        : "It was not ten";
}
```

The conditional here is an expression because both alternatives produce the value returned by the function. By contrast, loops, command-style calls, and branching used only to direct mutation are statements. The important distinction is whether a piece of code yields a value, not whether it happens to contain an equals sign.

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

Typical causes of impurity include reading or changing object fields, mutating an argument, consulting the ambient clock, performing I/O, or calling behavior whose result is not determined by the current function's arguments.

Expose a variable dependency as input data:

```csharp
internal static partial class CoreProperties {
    public static string TimestampedGreeting(DateTimeOffset now, string? name) =>
        string.Create(CultureInfo.InvariantCulture, $"{now} - Hello {name ?? "Unknown Person"}");
}
```

The clock enters as a `DateTimeOffset` argument, and invariant formatting keeps the result independent of the ambient culture.

Required behavior can likewise be supplied as a delegate rather than hidden behind a call to mutable or unknown external state. If one operation both mutates an object and calculates a result, separate those responsibilities so the calculation can remain pure.

Complete purity is not possible at every point in a C# application. Interaction with users, files, APIs, libraries, and other external systems introduces effects. The pragmatic goal is a large pure region with the unavoidable impure region kept as small and explicit as possible.

### Recursion

Recursion replaces some mutation-driven loops with a function that calls itself using new argument values. A recursive function needs:
1. an end condition that returns the final value;
2. a recursive path that calls the same function with values closer to that condition;
3. a returned value on every path.

Each call receives a new set of argument values rather than mutating the previous call's values. Recursion can replace constructs such as `while` and `foreach`, but it requires caution in C# because a direct recursive formulation can have performance problems.

### Pattern matching

Pattern matching selects a result based on a value's type or properties. C# switch expressions can turn large nests of conditional statements into concise, value-producing alternatives. They make the relationship between each recognized case and its returned value visible.

### Stateless transitions

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

The operation receives the state and all information required for the transition, then returns the replacement. Records reduce the copying needed to express this old-state-to-new-state model.

## How the properties reinforce one another

1. Immutability prevents earlier values from changing invisibly.
2. Pure functions transform those values deterministically.
3. Higher-order functions combine small transformations into larger behavior.
4. Expressions keep the flow value-producing.
5. Pattern matching makes alternative outputs explicit.
6. Recursion or declarative collection operations replace mutation-based iteration.
7. State changes remain possible as explicit old-state-to-new-state functions.

This combination raises the ratio of business logic to boilerplate. The program emphasizes the transformation being performed rather than counters, flags, temporary variables, mutation, and control-flow machinery.

## C# as a functional language

C# supports functions as first-class values well through delegates and lambdas. Garbage collection makes non-destructive updates practical because superseded versions can be reclaimed. C#'s main weakness is that mutation is the default: fields and variables must be explicitly constrained, user-defined immutable types require effort, and the standard collections are mutable even though an immutable collections library is available. LanguageExt supplies `Seq<A>`, `Map<K, V>`, `HashMap<K, V>`, and `Set<A>`.

LINQ is the clearest built-in functional model:
- `Select` maps each element through a function.
- `Where` filters through a predicate.
- `OrderBy` and `OrderByDescending` produce ordered sequences from key selectors.
- These operators accept functions and return new sequences instead of modifying their inputs.

The static import of `LanguageExt.Prelude` supplies constructors and functions as bare names: `Some`, `None`, `Seq`, `toSeq`, `Range`, and `parseInt`. `K<F, A>` carries the witness `F` for the type constructor and the element type `A`. The traits `Functor<F>`, `Applicative<F>`, and `Monad<M>` state what a witness supports, so one function serves every type with a witness. `.As()` restores the concrete type at the concrete edge.

```csharp
internal static partial class Traits {
    public static K<F, int> Tripled<F>(K<F, int> values)
        where F : Functor<F> =>
        values.Map(static x => x * 3);
    public static Option<int> TripledOption() => Tripled(Some(2)).As(); // Some(6)
    public static Seq<int> TripledSeq() => Tripled(Seq(1, 2, 3)).As();  // 3, 6, 9
}
```

Useful language features reduce the ceremony around functional code:
- `using static` makes libraries of static functions concise to consume, but excessive use can pollute the namespace.
- Getter-only auto-properties have a compiler-generated readonly backing field and can be assigned only inline or in the constructor, making simple immutable types easier to define.
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

The input and output types form the function's interface and contract. This perspective directs attention to what information enters and what value must come out.

A mathematical function's result is determined exclusively by its input. A C# method, delegate, or lambda only represents a function; that representation does not guarantee the same property. It may capture context, read mutable state, or perform effects even when its visible signature does not reveal those dependencies.

C# can represent functions in several ways:
- **Methods** are the conventional representation and participate in class and interface design. An instance method can be understood as also taking the current instance as an implicit argument.
- **Delegates** are strongly typed function pointers. `Func<T, R>` represents value-returning functions; `Action<T>` represents operations with no return value.
- **Lambdas** define short functions inline and are converted to a compatible delegate type.
- **Dictionaries** directly store arbitrary mappings whose associations cannot be computed. The same representation can retain results of expensive computations instead of recomputing them.

Prefer the general `Func` and `Action` families when only the signature matters. A custom delegate can still be worthwhile when its name conveys domain intent more clearly than a structural type such as `Func<T, bool>`.

### Arity, tuples, and closures

Arity is the number of arguments a function accepts: nullary, unary, binary, and so on. Any multi-argument function can be viewed as a unary function over a tuple of its arguments.

A closure combines a lambda with the context in which it was declared. The delegate's declared signature may remain unary, but the computation can also depend on captured context:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> DaysStartingWith(string pattern) => days.Filter(day => day.ToString().StartsWith(pattern, StringComparison.Ordinal));
Seq<DayOfWeek> weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
```

The predicate supplied to `Filter` has the signature `DayOfWeek -> bool`, yet it depends on both `day` and the captured `pattern`. Its unary interface and its two actual inputs are both valid ways to view it.

## Benefits

### Concision and clarity

Declarative code concentrates on what is required rather than the low-level mechanics of how variables must change. Fewer loops, flags, and intermediate updates can make the program shorter and make its purpose easier to identify. Concision is useful when the resulting transformation remains clear.

### Signal over noise

The useful signal in a function is its business logic; the noise is the machinery required to carry it out, such as loop setup, mutable counters, flags, and branching boilerplate. Functional transformations improve this signal-to-noise ratio by placing the intended result and its transformations in the foreground.

### Testability and predictability

A pure function can be tested with inputs and expected outputs because its behavior does not depend on hidden state. Referential transparency makes results repeatable and failures easier to reproduce.

Expression-oriented value flow also reduces surprising transfers of control caused by mutable flags, nested conditionals, and broadly scoped exception handling. Functional style does not make poor error handling impossible, but it discourages hidden paths and uncontrolled effects.

### Robustness

Immutability removes bugs caused by unexpected changes over time. The paradigm also avoids `null` and favors structures intended to prevent errors or stop them from causing unexpected later behavior while making the problem easier to report. These properties combine with testability to reduce defects.

### Concurrency

Code that does not mutate shared in-memory state is easier to run concurrently. This is useful for asynchronous processing, multiple workers handling the same kind of input, containerized workloads, and serverless functions. Shared external resources can still cause contention, so stateless application logic reduces rather than abolishes concurrency hazards.

## Where the style fits

Functional programming is particularly strong where work is predictable and transformational:
- converting data from one form to another;
- applying business logic to input before passing the result onward;
- highly asynchronous or concurrent processing;
- serverless functions and independently running workers;
- critical logic that benefits from deterministic behavior and extensive testing.

Effects are unavoidable when interacting with user input, storage, web APIs, third-party packages, logging, and other external entities. These are natural places to compromise on purity while preserving pure transformations wherever possible.

C# also places practical limits on the paradigm. Framework base classes and some libraries are object-oriented, and C# cannot express every feature available in a pure functional language. Functional C# is not inherently slow, but it is not guaranteed to be the highest-performance formulation. If raw performance outweighs readability and modularity, the tradeoff may favor another style.

The standard is pragmatic rather than absolute purity: make as much of the application as possible immutable, deterministic, expression-oriented, and free of hidden state while acknowledging C# and external-system boundaries.

## Judgment and tradeoffs

Higher-order functions add callback frames. The performance cost is usually negligible, but debugging call stacks can become less direct. Excessive abstraction can also obscure behavior.

Use higher-order functions when they separate otherwise interleaved logic, remove meaningful duplication, adapt an interface, or create reusable behavior. Keep lambdas short, choose clear names, and format nested callbacks so control flow remains visible.

Functional techniques are tools, not a prohibition on objects. Their main benefits are cleaner, more concise, maintainable, expressive, robust, readable, and testable code; better support for concurrency; and a second problem-solving perspective alongside object-oriented design.
