# Why Function Purity Matters

## Definition of Purity

A function is pure when both conditions hold:
1. Its return value is determined only by its inputs, including immutable values fixed when the function or object was constructed.
2. Evaluating it causes no side effects.

A side effect includes:
- mutating state visible outside the function, including instance fields;
- mutating an input argument;
- throwing an exception;
- performing I/O, including reading the clock, console, filesystem, database, network, or another process.

Under this definition, throwing counts as an effect even though some definitions permit it: exception handling can redirect control flow, and an unhandled exception can terminate the program.

An instance method can be impure even without I/O when it reads mutable fields. A lambda can be impure by closing over mutable variables. A readonly field fixed at construction can instead be an immutable dependency. Purity depends on everything a function observes or changes, not merely its parameter list and return type.

Because a pure function always maps the same input to the same output, evaluation order does not affect its meaning. Pure computations are easier to reason about and are safe candidates for:
- parallel evaluation;
- lazy evaluation;
- memoization.

Applying these transformations to impure functions can change behavior and introduce bugs.

A pure function call can be replaced with its result for a given input without changing program behavior. This property is referential transparency.

Purity is about observable behavior. Mutation of state that is local to a function and never escapes is not a side effect; mutation of an instance field is, even when that field is private, because other methods on the object can observe it.

## Isolate Impure Operations

Useful programs require I/O, so the goal is not universal purity. Different effects need different treatment: I/O must be isolated, argument mutation can be eliminated by returning data, errors can always be handled without exceptions, and non-local state mutation can often be avoided. Keep unavoidable I/O outside the computational core and place as much computation as possible in pure functions.

```csharp
internal static class Greeting {
    public static Eff<RT, Unit> Greet<RT>() where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine("Enter your name:")
        from name in Console<RT>.readLine
        from __ in Console<RT>.writeLine(GreetingFor(name))
        select unit;
    public static string GreetingFor(string name) => $"Hello {name}";
}
```

`Greet` describes the console reads and writes as an `Eff<RT, Unit>`, and the host performs them at `Run(rt)`. `Greet` reads the console capability through `Console<RT>`. `GreetingFor` contains the reusable, deterministic logic. Impure functions may call pure functions.

### Return information instead of mutating arguments

An output parameter represented by a mutable collection hides part of a function's result. It couples caller and callee through initialization rules and mutation order. Return every computed value explicitly instead:

```csharp
internal sealed record Product(string Name, decimal Price);
internal sealed record OrderLine(Product Product, int Quantity);
internal sealed record Order(Seq<OrderLine> OrderLines);

internal static class Orders {
    public static (decimal Total, Seq<OrderLine> LinesToDelete) RecomputeTotal(Order order) =>
        (
            order.OrderLines.Fold(0m, static (total, line) => total + (line.Product.Price * line.Quantity)),
            order.OrderLines.Filter(static line => line.Quantity == 0)
        );
}
```

The signature exposes the complete output. `Fold` sums the lines and `Filter` selects the lines to delete, both over the same `Seq<OrderLine>`. Neither side must know how the other manages a shared collection. Immutable objects can ensure that values do not change after construction. If one operation both mutates an object and calculates a result, separate those responsibilities so the calculation can remain pure.

## Purity and Concurrency

In the chapter's list formatter, sentence casing is pure, but numbering through an instance counter is not:

```csharp
internal static class StringExt {
    public static string ToSentenceCase(this string value) =>
        char.ToUpperInvariant(value[0]) + string.Concat(value[1..].Select(char.ToLowerInvariant));
}

internal sealed class ListFormatter {
    private int counter;
    private string PrependCounter(string value) =>
        string.Create(CultureInfo.InvariantCulture, $"{++counter}. {value}");

    public Seq<string> Format(Seq<string> items) =>
        items
            .Map(StringExt.ToSentenceCase)
            .Map(PrependCounter);
}
```

Because concurrency does not guarantee evaluation order, shared mutable state turns read-modify-write operations into races. Applying the formatter in parallel lets multiple threads update the same counter; `++` is not atomic, so increments can be lost and results become nondeterministic.

Concurrency covers several overlapping ideas: asynchronous code begins another task before an outstanding operation completes; parallel code runs work simultaneously across processing cores; multithreading schedules concurrent threads even when hardware cannot execute all of them at the same instant. All make hidden dependencies on mutable state harder to control.

Locks or atomic operations can protect the counter, but a design without shared state removes the race. Generate the required values, then combine independent sequences:

```csharp
internal static class Formatting {
    public static Seq<string> Format(Seq<string> items) =>
        items
            .Map(StringExt.ToSentenceCase)
            .Zip(toSeq(Range(1, items.Count)), static (item, index) => string.Create(CultureInfo.InvariantCulture, $"{index}. {item}"));
}
```

`Range(1, items.Count)` generates that count of indices as values, and `toSeq` makes them a `Seq<int>`. `Zip` pairs each item with an index. No running counter is updated. State is input data rather than shared mutable state, so parallel evaluation preserves behavior.

`IO` determines how effects are evaluated: `Traverse` under `IO` starts every element effect before it awaits any. Effects built with `IO.liftAsync` overlap without a bound, and effects built with `IO.lift` run in order on the calling thread. `TraverseM` runs the effects one after another. `Fork` takes one thread per fork, so a large fan-out chunks the collection first.

Treat `Map` as a value transformation and keep its function pure. The API accepts an impure delegate, but parallel execution may change its behavior.

The compiler cannot infer whether an arbitrary delegate is pure, so parallel execution must be requested explicitly. Its overhead is justified only by sufficient work and input size.

### Static methods are not the problem

Pure methods can safely be static because all required data is explicit or immutable. Static methods become hazardous when they:
- read or write mutable static fields;
- perform I/O that callers cannot replace in tests.

Avoid mutable static fields and direct dependencies on static I/O methods.

## Purity and Testability

A unit test for a pure function supplies inputs and asserts the returned output. It is isolated and repeatable by construction.

An impure function has hidden inputs, hidden outputs, or both:
- the current time, database contents, or environment are implicit inputs;
- an email sent, file written, or field changed is an implicit output.

An impure function behaves like a larger pure transformation:

```text
(arguments, current program state, current world state)
    -> (return value, new program state, new world state)
```

This explains the extra cost of testing effects. Arrange must construct substitute external state and program state; Assert must inspect both explicit results and externally visible changes. Mocks can model external state, while assertions over internal mutation tend to be brittle and break encapsulation.

Parameterized tests make inputs and expected outputs explicit: each test case supplies values, adapts them into the function's input, and returns or asserts the expected output across boundary cases.

## Push Effects Outward

### Abstraction improves control but does not create purity

Wrapping the system clock behind an interface does not make the consuming method pure. It is pure only when the injected implementation is pure. A production implementation that reads the clock still carries I/O into the validator. This approach improves test control without reducing the production effect itself.

Choose the narrowest dependency that represents what the consumer needs:
- inject a value for a stable snapshot;
- inject an `IO<A>` for an operation that must run on demand;
- inject a runtime for a consumer that reads many capabilities.

An interface remains appropriate as a common contract for distinct implementations. Systematically creating a one-method interface for every effect adds unnecessary infrastructure.

### Inject a value when one snapshot is enough

Reading `DateTime.UtcNow` inside validation makes the result depend on the system clock. Let the code that constructs the validator read the date once and inject that value:

```csharp
internal sealed record MakeTransfer(DateTime Date, string Bic);

internal sealed class DateNotPastValidator(DateTime today) {
    public bool IsValid(MakeTransfer command) => today <= command.Date.Date;
}
```

`IsValid` is deterministic. This applies to configuration, environment settings, and request-scoped values. The tradeoff is lifetime: the object must not outlive the validity of the captured snapshot.

Where a consumer reads many capabilities, a runtime record carries them, one `Has<Eff<RT>, T>` trait per capability. The consumer is an `Eff<RT, A>` generic over `RT`:

```csharp
internal sealed record Clock(DateTime Today);
internal sealed record Runtime(Clock Clock, ConsoleIO Console) : Has<Eff<Runtime>, Clock>, Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, Clock> Has<Eff<Runtime>, Clock>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Clock);
    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Capabilities {
    public static Eff<RT, bool> DateNotPast<RT>(MakeTransfer command) where RT : Has<Eff<RT>, Clock> =>
        RT.Ask.Map(clock => new DateNotPastValidator(clock.Today).IsValid(command)).As();
}
```

`DateNotPast` reads the `Clock` through `RT.Ask` and passes the snapshot to the validator. A test runtime carries a fixed `Clock`. The same runtime also carries `ConsoleIO`, so it runs `Greet`.

### Inject an Effect When the Value Must Be Acquired on Demand

A validator needs the list of valid bank codes. The caller loads the codes as an effect and passes the `Seq<string>`, so the validator is pure:

```csharp
internal sealed class BicExistsValidator(Seq<string> validCodes) {
    public bool IsValid(MakeTransfer command) =>
        validCodes.Exists(code => string.Equals(code, command.Bic, StringComparison.Ordinal));
}

internal static class Transfers {
    public static IO<bool> BicExists(IO<Seq<string>> loadCodes, MakeTransfer command) =>
        loadCodes.Map(codes => new BicExistsValidator(codes).IsValid(command));
}
```

Production composition supplies the `IO<Seq<string>>` that queries the codes. A test supplies `IO.pure`. The `IO<Seq<string>>` defers the query until `BicExists` binds it, so the codes are read only when the check runs. The validator stays pure and the effect is explicit and replaceable. The host runs `BicExists` with `RunSafe()` and matches the `Fin<bool>`.

A function signature is a narrow interface. Injecting an effect value can replace a one-method interface, its implementation, constructor wiring, dependency-injection registration, and test fake.

## Why Purity Matters More

Distributed systems perform more I/O because programs increasingly delegate computation to other processes and services. That makes fully pure programs less attainable while increasing the need for asynchronous work, where hidden mutable state is troublesome.

At the same time, performance gains increasingly come from multiple processors rather than ever-faster individual CPUs. Computations built from pure functions are easier to parallelize safely. The growth of both asynchronous I/O and multicore execution therefore makes a small, explicit impure boundary more valuable even though useful software cannot eliminate effects.

## Design Checklist

- List every non-local value a function reads and every externally visible change it makes.
- Extract deterministic computation from I/O workflows.
- Return all computed information rather than mutating arguments.
- Replace a shared counter with generated values and `Zip`.
- Treat `Map` transformations as pure so sequential and parallel evaluation preserve meaning.
- Inject stable snapshots as values, deferred effects as `IO`, and many capabilities through a runtime `RT`.
- Keep effects near application boundaries and let those boundaries call inward to pure logic.
