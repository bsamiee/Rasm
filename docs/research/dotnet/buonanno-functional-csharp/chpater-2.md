# Why Function Purity Matters

## The Purity Contract

A function is pure when both conditions hold:
1. Its return value is wholly determined by its inputs, including any immutable values fixed when the function or object was constructed.
2. Evaluating it causes no side effects.

A side effect includes:
- mutating state visible outside the function, including instance fields;
- mutating an input argument;
- throwing an exception;
- performing I/O, including reading the clock, console, filesystem, database, network, or another process.

Under this definition, throwing counts as an effect even though some definitions permit it: exception handling can redirect control flow, and an unhandled exception can terminate the program.

An instance method can therefore be impure without obvious I/O when it reads mutable fields. A lambda can be impure by closing over mutable variables. A readonly field fixed at construction can instead be part of a stable function environment. Purity depends on everything a function observes or changes, not merely its parameter list and return type.

Because a pure function always maps the same input to the same output, evaluation order does not affect its meaning. Pure computations are consequently easier to reason about and are safe candidates for:

- parallel evaluation;
- lazy evaluation;
- memoization.

Applying these transformations to impure functions can change behavior and introduce difficult bugs.

Purity is about observable behavior. Mutation of state that is local to a function and never escapes is not a side effect; mutation of an instance field is, even when that field is private, because other methods on the object can observe it.

## Shrink the Impure Footprint

Useful programs require I/O, so the goal is not universal purity. Different effects need different treatment: I/O must be isolated, argument mutation can be eliminated by returning data, errors can always be handled without exceptions, and non-local state mutation can often be avoided. Keep unavoidable I/O outside the computational core and place as much computation as possible in pure functions.

```csharp
WriteLine("Enter your name:");
var name = ReadLine();
WriteLine(GreetingFor(name));

static string GreetingFor(string name) => $"Hello {name}";
```

The outer workflow is necessarily impure. `GreetingFor` contains the reusable, deterministic logic. Impure functions may call pure functions; a supposedly pure function becomes impure as soon as it calls an I/O operation.

### Return information instead of mutating arguments

An output parameter represented by a mutable collection hides part of a function's result. It couples caller and callee through initialization rules and mutation order. Return every computed value explicitly instead:

```csharp
static (decimal Total, IEnumerable<OrderLine> LinesToDelete)
    RecomputeTotal(Order order)
    => (
        order.OrderLines.Sum(line => line.Product.Price * line.Quantity),
        order.OrderLines.Where(line => line.Quantity == 0)
    );
```

The signature now exposes the complete output. Neither side must know how the other manages a shared collection. Immutable objects can enforce the broader rule that values are not changed after construction.

## Purity and Concurrency

The chapter's list formatter makes the danger concrete. Sentence casing is pure, but numbering through an instance counter is not:

```csharp
static class StringExt
{
    public static string ToSentenceCase(this string value)
        => value.ToUpper()[0] + value.ToLower().Substring(1);
}

class ListFormatter
{
    private int counter;

    private string PrependCounter(string value)
        => $"{++counter}. {value}";

    public List<string> Format(List<string> items)
        => items
            .Select(StringExt.ToSentenceCase)
            .Select(PrependCounter)
            .ToList();
}
```

Concurrency does not guarantee evaluation order. Shared mutable state therefore turns ordinary read-modify-write operations into races. For example, applying a formatter that increments an instance counter through PLINQ lets multiple threads update the same counter; `++` is not atomic, so increments can be lost and results become nondeterministic.

Concurrency covers several overlapping ideas: asynchronous code begins another task before an outstanding operation completes; parallel code runs work simultaneously across processing cores; multithreading schedules concurrent threads even when hardware cannot execute all of them at the same instant. All make hidden dependencies on mutable state harder to control.

Locks or atomic operations can protect the counter, but a functional redesign removes the shared state. Generate the required values, then combine independent sequences:

```csharp
using static System.Linq.Enumerable;

static List<string> Format(List<string> items)
    => items
        .Select(StringExt.ToSentenceCase)
        .Zip(Range(1, items.Count), (item, index) => $"{index}. {item}")
        .ToList();
```

`Range` represents all indices as values. `Zip` pairs each item with an index. No running counter is updated, so the computation is pure and can be adapted to PLINQ without introducing a race.

```csharp
using static System.Linq.ParallelEnumerable;

static List<string> FormatInParallel(List<string> items)
    => items
        .AsParallel()
        .Select(StringExt.ToSentenceCase)
        .Zip(Range(1, items.Count), (item, index) => $"{index}. {item}")
        .ToList();
```

`AsParallel` changes the subsequent operators to their PLINQ implementations. Importing `ParallelEnumerable` also selects its `Range`, producing the parallel sequence required by parallel `Zip`. The shape of the solution remains the same because state was represented as input data rather than a shared update.

Treat `Select` as a value transformation and keep its selector pure. The API accepts an impure delegate, but parallel execution may change its behavior.

Purity makes parallelization semantically safe, but the runtime cannot infer that an arbitrary delegate is pure. Parallel execution must still be requested explicitly, and it is worthwhile only when the work and input size justify its overhead.

### Static methods are not the problem

Pure methods can safely be static because all required data is explicit or immutable. Static methods become hazardous when they:
- read or write mutable static fields;
- perform I/O that callers cannot replace in tests.

Practical rule: make pure functions static, avoid mutable static fields, and avoid direct dependencies on static I/O methods.

## Purity and Testability

A pure unit test supplies inputs and asserts the returned output. It is isolated and repeatable by construction.

An impure function has hidden inputs, hidden outputs, or both:
- the current time, database contents, or environment are implicit inputs;
- an email sent, file written, or field changed is an implicit output.

Conceptually, an impure function behaves like a larger pure transformation:

```text
(arguments, current program state, current world state)
    -> (return value, new program state, new world state)
```

This explains the extra cost of testing effects. Arrange must construct a substitute world and program state; Assert must inspect both explicit results and externally visible changes. Mocks can model external state, while assertions over internal mutation tend to be brittle and break encapsulation.

Parameterized tests encourage functional thinking: each test case supplies explicit values, adapts them into the function's input, and returns or asserts the expected output across boundary cases.

## Push Effects Outward

### Abstraction improves control but does not create purity

Wrapping the system clock behind an interface makes it replaceable in tests, but the consuming method is pure only when the injected implementation is pure. A production implementation that reads the clock still carries I/O into the validator. The interface-based approach therefore improves test control without reducing the production effect itself, and using it systematically creates one-method interfaces, implementations, registrations, and test fakes.

Choose the narrowest dependency that represents what the consumer actually needs:
- inject a value for a stable snapshot;
- inject a function for an operation that must run on demand.

An interface remains appropriate as a common contract for genuinely distinct implementations. The avoidable noise is the systematic creation of a one-method interface for every effect merely to make that operation replaceable.

### Inject a value when one snapshot is enough

Reading `DateTime.UtcNow` inside validation makes the result depend on the system clock. Let the code that constructs the validator read the date once and inject that value:

```csharp
public sealed class DateNotPastValidator
{
    private readonly DateTime today;

    public DateNotPastValidator(DateTime today) => this.today = today;

    public bool IsValid(MakeTransfer command)
        => today <= command.Date.Date;
}
```

`IsValid` is now deterministic. The caller performs the clock read once and the validator receives an immutable snapshot. This works especially well for configuration, environment settings, and request-scoped values. The tradeoff is lifetime: the object must not outlive the validity of the captured snapshot.

### Inject a function when the value must be acquired on demand

Sometimes retrieving a value early is wasteful or assigns work to the wrong component. A validator may need a changing list of valid bank codes only if earlier checks pass. Inject the required operation directly:

```csharp
public sealed class BicExistsValidator
{
    private readonly Func<IEnumerable<string>> getValidCodes;

    public BicExistsValidator(Func<IEnumerable<string>> getValidCodes)
        => this.getValidCodes = getValidCodes;

    public bool IsValid(MakeTransfer command)
        => getValidCodes().Contains(command.Bic);
}
```

Production composition supplies the impure query; a test supplies a deterministic function. The validator remains impure to exactly the extent that the injected function is impure, but the effect is explicit, replaceable, and acquired only when needed.

A function signature is already a narrow interface. Injecting a delegate can replace a one-method "header interface" and its implementation, constructor plumbing, bootstrapping registration, and test fake.

## Why the Pressure Toward Purity Is Increasing

Distributed systems perform more I/O because programs increasingly delegate computation to other processes and services. That makes fully pure programs less attainable while increasing the need for asynchronous work, where hidden mutable state is especially troublesome.

At the same time, performance gains increasingly come from multiple processors rather than ever-faster individual CPUs. Computations built from pure functions are easier to parallelize safely. The growth of both asynchronous I/O and multicore execution therefore makes a small, explicit impure boundary more valuable even though useful software cannot eliminate effects.

## Design Checklist

- List every non-local value a function reads and every externally visible change it makes.
- Extract deterministic computation from I/O workflows.
- Return all computed information rather than mutating arguments.
- Replace shared running state with generated values and sequence combinators when possible.
- Treat `Select` transformations as pure so sequential and parallel evaluation preserve meaning.
- Inject stable snapshots as values and deferred effects as functions.
- Keep effects near application boundaries and let those boundaries call inward to pure logic.
