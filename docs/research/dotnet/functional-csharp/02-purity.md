# [PURITY]

## [01]-[DEFINITION]

Purity means both conditions hold:
1. Its return value is determined only by its inputs, including immutable values fixed when the function or object was constructed
2. Evaluating it causes no side effects

Side effects include:
- Mutating state visible outside the function, including instance fields;
- Mutating an input argument;
- Throwing an exception;
- Performing I/O, including reading the clock, console, filesystem, database, network, or another process

Under this definition, throwing counts as an effect even though some definitions permit it: exception handling can redirect control flow, and an unhandled exception can terminate the program.

Instance methods can be impure even without I/O when they read mutable fields. Lambdas can be impure by closing over mutable variables. Readonly fields fixed at construction serve as immutable dependencies. Purity depends on everything a function observes or changes, not on its parameter list and return type alone.

Because a pure function always maps the same input to the same output, evaluation order does not affect its meaning. Pure computations are easier to reason about and are safe candidates for:
- Parallel evaluation;
- Lazy evaluation;
- memoization

Applying these transformations to impure functions can change behavior.

Replacing the call with its result for a given input changes nothing; this property is referential transparency.

Purity is about observable behavior. Mutation of state that is local to a function and never escapes is not a side effect; mutation of an instance field is, even when that field is private, because other methods on the object can observe it.

## [02]-[ISOLATING_IMPURITY]

Useful programs require I/O; the goal is not universal purity. Different effects need different treatment: I/O must be isolated, argument mutation can be eliminated by returning data, errors can always be handled without exceptions, and non-local state mutation can be designed away. Keep unavoidable I/O outside the computational core and place as much computation as possible in pure functions.

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

`Greet` describes the console reads and writes as an `Eff<RT, Unit>`, and the host performs them at `Run(rt)`. `Greet` reads the console capability through `Console<RT>`. `GreetingFor` contains the reusable, deterministic logic. Impure functions can call pure functions.

### [02.1]-[RETURN_OVER_MUTATION]

Output parameters represented by a mutable collection hide part of a function's result and couple caller and callee through initialization rules and mutation order. Return every computed value explicitly instead:

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

The signature exposes the complete output. `Fold` sums the lines and `Filter` selects the lines to delete, both over the same `Seq<OrderLine>`. Neither side must know how the other manages a shared collection. If one operation both mutates an object and calculates a result, separate those responsibilities, the calculation can remain pure.

## [03]-[CONCURRENCY]

In this list formatter, sentence casing is pure, and numbering through an instance counter is not:

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

Because concurrency does not guarantee evaluation order, shared mutable state turns read-modify-write operations into races. Applying the formatter in parallel lets multiple threads update the same counter. `++` is not atomic: increments can be lost and results become nondeterministic.

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

`Range(1, items.Count)` generates that count of indices as values, and `toSeq` makes them a `Seq<int>`. `Zip` pairs each item with an index. No running counter is updated. State is input data rather than shared mutable state. Parallel evaluation preserves behavior.

`IO` determines how effects are evaluated: `Traverse` under `IO` starts every element effect before it awaits any. Effects built with `IO.liftAsync` overlap without a bound, and effects built with `IO.lift` run in order on the calling thread. `TraverseM` runs the effects one after another. `Fork` takes one thread per fork, a large fan-out chunks the collection first.

Treat `Map` as a value transformation and keep its function pure. The API accepts an impure delegate, but parallel execution can change its behavior.

The compiler cannot infer whether an arbitrary delegate is pure, parallel execution must be requested explicitly. Its overhead is justified only by sufficient work and input size.

### [03.1]-[STATIC_METHODS]

Pure methods can safely be static because all required data is explicit or immutable. Static methods become hazardous when they:
- Read or write mutable static fields;
- Perform I/O that callers cannot replace in tests

Avoid mutable static fields and direct dependencies on static I/O methods.

## [04]-[TESTABILITY]

Unit tests for pure functions supply inputs and assert the output. They are isolated and repeatable by construction.

Impure functions have hidden inputs, hidden outputs, or both:
- The current time, database contents, or environment are implicit inputs;
- Emails sent, files written, or fields changed are implicit outputs

Impure functions behave like a larger pure transformation:

```text
(arguments, current program state, current world state)
    -> (return value, new program state, new world state)
```

This explains the extra cost of testing effects. Arrange must construct substitute external state and program state; Assert must inspect both explicit results and externally visible changes. Mocks can model external state, while assertions over internal mutation are brittle and break encapsulation.

Parameterized tests make inputs and expected outputs explicit: each test case supplies values, adapts them into the function's input, and returns or asserts the expected output across boundary cases.

## [05]-[PUSHING_EFFECTS_OUTWARD]

### [05.1]-[ABSTRACTION_LIMITS]

Wrapping the system clock behind an interface does not make the consuming method pure. It is pure only when the injected implementation is pure. Production implementations that read the clock still carry I/O into the validator. This approach improves test control without reducing the production effect itself.

Choose the narrowest dependency that represents what the consumer needs:
- Inject a value for a stable snapshot;
- Inject an `IO<A>` for an operation that must run on demand;
- Inject a runtime for a consumer that reads many capabilities

Interfaces remain appropriate as a common contract for distinct implementations. One-method interfaces for every effect add unnecessary infrastructure.

### [05.2]-[VALUE_INJECTION]

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

`DateNotPast` reads the `Clock` through `RT.Ask` and passes the snapshot to the validator. Test runtimes carry a fixed `Clock`. The same runtime also carries `ConsoleIO`, it runs `Greet`.

### [05.3]-[EFFECT_INJECTION]

Validators need the list of valid bank codes. The caller loads the codes as an effect and passes the `Seq<string>`:

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

Production composition supplies the `IO<Seq<string>>` that queries the codes. Tests supply `IO.pure`. The `IO<Seq<string>>` defers the query until `BicExists` binds it. The codes are read only when the check runs. The validator stays pure and the effect is explicit and replaceable. The host runs `BicExists` with `RunSafe()` and matches the `Fin<bool>`.

Function signatures are narrow interfaces. Injecting an effect value can replace a one-method interface, its implementation, constructor wiring, dependency-injection registration, and test fake.

## [06]-[ASYNC_AND_MULTICORE]

Distributed systems delegate computation to other processes, which raises the amount of I/O and lowers how much of a program stays pure. Performance gains come from more cores rather than faster single cores, and pure computations parallelize safely. Both trends raise the value of a small, explicit impure boundary.

## [07]-[DESIGN_CHECKLIST]

- List every non-local value a function reads and every externally visible change it makes
- Extract deterministic computation from I/O workflows
- Return all computed information rather than mutating arguments
- Replace a shared counter with generated values and `Zip`
- Treat `Map` transformations as pure
- Inject stable snapshots as values, deferred effects as `IO`, and many capabilities through a runtime `RT`
- Keep effects near application boundaries and let those boundaries call inward to pure logic
