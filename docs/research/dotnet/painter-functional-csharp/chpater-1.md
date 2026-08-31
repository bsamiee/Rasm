# Chapter 1: Functional Programming in C#

## A paradigm, not a product

Functional programming is a programming paradigm: a style for structuring programs, not a language, API, package, or framework. C# is multi-paradigm, so functional and object-oriented techniques can coexist. Applying functional programming in C# does not require first learning a pure functional language.

The central contrast is between imperative and declarative code:
- Imperative code specifies how work proceeds through ordered instructions, mutable variables, loops, and branches.
- Declarative code describes the result and the transformations needed to produce it, leaving more execution detail to the runtime.

SQL illustrates the declarative mindset: a query describes the desired result without manually implementing every transformation or prescribing the runtime's exact execution order. Functional C# aims for a similar emphasis on what is computed rather than the mechanics of changing state.

Functional programming has a long mathematical and programming lineage rather than being a recent trend. Its roots include work on combinatory logic and lambda expressions, followed by early functional languages such as IPL and LISP. Some languages enforce the functional paradigm; hybrid languages such as C# expose useful functional features without implementing every part of it.

C# provides several important building blocks: delegates and lambdas make behavior passable as values, LINQ supports declarative transformations, switch expressions provide value-producing pattern matching, and records make immutable state transitions easier to express.

## The core properties

### Immutability

An immutable value is fixed once created. When a different value is needed, derive and return a new value rather than changing the original. `string` and `DateTime` demonstrate this behavior in .NET: operations on them produce new values.

This changes how a program represents progress. Instead of revising an earlier step, each expression produces the next value from values already available. The result resembles mathematical working in which each line remains fixed and later lines build on it.

### Higher-order functions

Functions can be stored in variables, passed as arguments, and returned from other functions. In C#, delegates provide this capability:

```csharp
Func<int, int, string> describeSum =
    (x, y) => $"{x} + {y} = {x + y}";

Action<string> log =
    message => logger.LogInfo($"message received: {message}");
```

`Func<...>` represents behavior that returns a value. `Action<...>` represents `void` behavior. Value-returning delegates can be composed into larger operations from small functions. `Action` commonly marks effectful behavior; logging is an example of an effect that may remain necessary at the application's impure edge.

### Expressions rather than statements

An expression evaluates to a value. A statement primarily issues an instruction or changes the route through the program. Functional code prefers expressions because each step contributes a value to the calculation.

```csharp
string Describe(int value) => value == 10
    ? "It was ten"
    : "It was not ten";
```

The conditional here is an expression because both alternatives produce the value returned by the function. By contrast, loops, command-style calls, and branching used only to direct mutation are statements. The important distinction is whether a piece of code yields a value, not whether it happens to contain an equals sign.

### Referential transparency and pure functions

A pure function:
- changes nothing outside the function;
- returns the same result for the same arguments, regardless of ambient state;
- has no unexpected side effects, including unexpected exceptions.

Such a function call can be replaced with its result for a given input without changing program behavior. This is referential transparency.

```csharp
int Add(int left, int right) => left + right;

string Greeting(string? name) =>
    "Hello " + (string.IsNullOrWhiteSpace(name)
        ? "Unknown Person"
        : name);
```

Typical causes of impurity include reading or changing object fields, mutating an argument, consulting the ambient clock, performing I/O, or calling behavior whose result is not determined by the current function's arguments.

Expose a variable dependency as input data:

```csharp
string TimestampedGreeting(DateTime now, string? name) =>
    $"{now} - Hello {name ?? "Unknown Person"}";
```

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
DoctorWho RegenerateDoctor(DoctorWho oldState, string newActorName) =>
    new DoctorWho
    {
        NumberOfStories = oldState.NumberOfStories,
        CurrentDoctor = oldState.CurrentDoctor + 1,
        CurrentDoctorActor = newActorName,
        SeasonNumber = oldState.SeasonNumber
    };
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
