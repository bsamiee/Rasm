# Functional Error Handling

## Errors belong in the return type

An operation that can predictably fail should return both possible outcomes as data. Its signature then states the failure contract, callers can reason about it locally, and ordinary composition controls the flow. An exception, by contrast, transfers control to a handler somewhere up the call stack or escapes uncaught; understanding the next step requires tracing the surrounding call paths.

Use `Option<T>` when failure means only “no value” and no explanation is useful. Use `Fin<A>` when the caller needs failure details. `Either<L, R>` stays for two value types where neither side is an error.

```text
Fin<A> = Fail(Error) | Succ(A)

Fail = failure data, always an Error
Succ = successful result
```

The success side is the value being transformed. The failure side carries the error unchanged through the rest of a failed workflow. An `A` and an `Error` both convert to `Fin<A>` without a constructor call.

```csharp
internal static class Calculator {
    public static Fin<double> Calculate(double x, double y) =>
        y == 0 ? Error.New("y cannot be 0")
        : x != 0 && Math.Sign(x) != Math.Sign(y) ? Error.New("x / y cannot be negative")
        : Math.Sqrt(x / y);
}
```

This signature is honest: every caller knows that calculation can fail and knows the error type it must handle.

### Why a result-plus-error container is not enough

Lighter wrappers around edge calls each carry a cost:
- Returning `default` on failure is concise but swallows the exception and makes failure indistinguishable from a legitimate default result.
- Giving a reusable wrapper a logger preserves the exception, but the wrapper lacks the caller's specific context unless more information is supplied.
- Returning result-and-error metadata preserves both possibilities, but a container with both fields forces every success to carry an unused error field and every failure to carry an unused result field.
- An `OnError` operation on such a container reduces the caller's checking boilerplate and makes the error action explicit, while retaining that imperfect container shape.

A representation with mutually exclusive success and failure cases avoids the container's unused and potentially inconsistent fields.

## Core operations

`Fin<A>` applies functions only to `Succ`. `Fail` bypasses the function and preserves its error.

```csharp
internal static class Operations {
    public static Fin<string> Describe(double x, double y) =>
        Calculator.Calculate(x, y).Map(static root => string.Create(CultureInfo.InvariantCulture, $"{root}"));
    public static Fin<double> FourthRoot(double x, double y) =>
        Calculator.Calculate(x, y).Bind(static root => Calculator.Calculate(root, 1));
    public static Fin<double> Root(double value) =>
        from v in Pure(value).ToFin()
        from _ in guard(v >= 0, Error.New("value cannot be negative"))
        select Math.Sqrt(v);
}
```

- `Map` transforms a successful value with `A -> B`.
- `Bind` composes a step with `A -> Fin<B>` and flattens the result.
- `Iter` performs an action only for `Succ` and represents its completed action with `Unit`.
- `Match` handles both cases and returns an ordinary value. It exits the abstraction, so delay it until a boundary.

`Fin<A>` has no `Where`. A false predicate must produce `Fail`, but a predicate supplies only `bool` and no `Error`. Turn the predicate into a validator that constructs a specific error, then compose it with `Bind`. Inside a LINQ query the same check is a `guard` clause, as `Root` shows.

A function of shape `A -> Fin<B>` crosses from an ordinary value into an explicit outcome. `Bind` composes such functions while keeping the workflow inside `Fin`. `Match` is the downward crossing that finally interprets the outcome as something outside the abstraction.

## Fail-fast workflows

`Bind` produces a two-track pipeline. Each successful step advances on the success track. The first failure moves to the failure track, skips every later step, and reaches the final handler unchanged.

```csharp
internal sealed record Request(string Account, decimal Amount);
internal sealed record ValidRequest(string Account, decimal Amount);
internal sealed record Model(string Account, decimal Balance, decimal Amount);
internal sealed record UpdatedModel(string Account, decimal Balance);

internal static class Workflow {
    public static Fin<ValidRequest> Validate(Request request) =>
        request.Amount > 0 ? new ValidRequest(request.Account, request.Amount) : Error.New("amount must be positive");
    public static Fin<Model> Load(ValidRequest request) =>
        string.Equals(request.Account, "ACC-1", StringComparison.Ordinal) ? new Model(request.Account, 100m, request.Amount) : Error.New("account not found");
    public static Fin<UpdatedModel> Update(Model model) =>
        model.Balance >= model.Amount ? new UpdatedModel(model.Account, model.Balance - model.Amount) : Error.New("insufficient funds");
    public static Fin<Unit> Save(UpdatedModel model) =>
        model.Balance <= 1_000_000m ? unit : Error.New("balance exceeds the reporting limit");
    public static Fin<Unit> Handle(Request request) =>
        Validate(request)
            .Bind(Load)
            .Bind(Update)
            .Bind(Save);
}
```

Use `Unit` when success has no meaningful payload. The pipeline still returns an explicit success value rather than relying on `void` or an implicit absence.

All bound functions share the failure type `Error`. Choose the domain errors for the workflow before composing it.

## Typed business validation

Prefer distinct error types over strings. A string is too limited for structured error details, while `Exception` has the wrong meaning for expected, business-as-usual failures. Specific `Expected` records give each failure a domain identity, a code, and room for richer data.

```csharp
internal sealed record BookTransfer(string Bic, DateOnly Date);

internal static class Codes {
    public const int InvalidBic = 1;
    public const int TransferDateIsPast = 2;
}

internal sealed record InvalidBic() : Expected("The beneficiary BIC is invalid", Codes.InvalidBic);
internal sealed record TransferDateIsPast() : Expected("Transfer date cannot be in the past", Codes.TransferDateIsPast);

internal static class Transfers {
    public static Fin<BookTransfer> ValidateBic(BookTransfer command) =>
        command.Bic.Length is 8 or 11 && command.Bic.All(char.IsLetterOrDigit) ? command : new InvalidBic();
    public static Fin<BookTransfer> ValidateDate(BookTransfer command, DateOnly today) =>
        command.Date > today ? command : new TransferDateIsPast();
    public static Fin<BookTransfer> Validate(BookTransfer command, DateOnly today) =>
        ValidateBic(command).Bind(c => ValidateDate(c, today));
}
```

Each validator has the same shape: accept the request, return that request on success, or return the specific error for the violated rule. The date validator checks that the transfer is in the future and receives the clock as an argument. The BIC validator checks the identifier's format. Returning the request in `Succ` makes it available to the next validator. Because this pipeline uses `Bind`, it stops at the first invalid result. The codes live in one closed block, and a consumer classifies an error with `Is`, `HasCode`, or `IsType<E>`, never by its message text.

## Keep the abstraction inside the application core

Within the core, continue composing with `Map` and `Bind`. Translate only in an outer adapter where the protocol, UI, or host requires a concrete response. The result type is chosen where input enters and is kept through the domain. `Match`, `RunSafe`, and `IfFail` are host operations.

```csharp
IActionResult Post(Request request) =>
    Workflow.Handle(request).Match<IActionResult>(
        Succ: static _ => Ok(),
        Fail: static error => BadRequest(error));
```

For an optional lookup, a boundary can translate `None` to “not found” and `Some(value)` to a successful response. For `Fin`, the boundary must decide how domain failures map to the external contract.

Two viable API designs are:
- Map `Fail` and `Succ` to protocol status codes and payloads.
- Always return a transport-success response whose body is a result DTO with `Succeeded` plus either `Data` or `Error`. Unlike `Fin`, this DTO exposes its values directly for serialization and client access.

Mapping business validation to an HTTP error such as 400 is debatable: the request may be syntactically valid yet violate a business rule, and concurrent changes can invalidate a request between creation and receipt. The choice is an API-design decision. The invariant is that protocol details stay in the adapter and the core retains its explicit outcome type.

## Adapting error types

The usual `Map` changes only `A` and leaves the `Error` fixed. `MapFail` changes only the `Error`, and `BiMap` maps both sides at the join point:

```csharp
internal static class Adapters {
    public static Fin<BookTransfer> WithContext(Fin<BookTransfer> result) =>
        result.MapFail(static error => Error.New("transfer rejected", error));
    public static Fin<string> Describe(Fin<BookTransfer> result) =>
        result.BiMap(
            Succ: static command => command.Bic,
            Fail: static error => Error.New("transfer rejected", error));
    public static Fin<BookTransfer> Recover(Fin<BookTransfer> result, BookTransfer fallback) =>
        result.Catch(Codes.InvalidBic, _ => fallback).As();
}
```

`Error.New(string, Error)` keeps the original error as `Inner`, so context is added without losing the cause. Recovery belongs to the boundary that owns the error and uses the `Catch` overloads. `Catch(code, f)` selects by code, `Catch(Error, f)` by value, and `Catch(predicate, f)` by a test on the error. A consistent error model is clearer than repeatedly adapting incompatible representations.

## Separate business failures from technical failures

Two types beside `Fin<A>` make intent clearer and reduce generic noise:

```text
Validation<Error, A> = Fail(Error) | Success(A)
IO<A>                = a deferred effect that fails with Error or yields A
```

- `Validation<Error, A>` represents violated business rules. Its failure type is fixed to `Error`, whose `+` accumulates into `ManyErrors`, so multiple errors are carried at once. `IsType<E>` and `IsExceptional` on `ManyErrors` test its members.
- `IO<A>` represents infrastructure, integration, or other technical work. A thrown exception arrives on its error channel as an `Exceptional` error.

A class-based `Result<T>` with `Success<T>` and `Failure<T>` cases, where `Failure<T>` carries an exception, is `Fin<A>` with an `Exceptional` error under another name. Its `Success<T>` does not by itself prevent a `null` payload. Absence stays in `Option<T>`.

Convert an exception-throwing dependency immediately at the integration boundary. `IO.lift` captures the throw, so the capture scope is one call.

```csharp
internal static class Ledger {
    public static Unit Insert(BookTransfer command) =>
        string.Equals(command.Bic, "DEUTDEFFXXX", StringComparison.Ordinal) ? throw new InvalidOperationException("duplicate transfer") : unit;
}

internal static class Persistence {
    public static IO<Unit> Save(BookTransfer command) =>
        IO.lift(() => Ledger.Insert(command));
}
```

If validation returns `Validation<Error, BookTransfer>` and persistence returns `IO<Unit>`, they cannot be flattened with one `Bind` because they express different effects. Validation accumulates and exits through `ToFin`. `IO.lift(Fin<A>)` places that result on the `IO` error channel. One query then binds validation and persistence, and `Save` runs only for a valid command:

```csharp
internal static class Handler {
    public static Validation<Error, BookTransfer> ValidateCommand(BookTransfer command, DateOnly today) =>
        (Transfers.ValidateBic(command).ToValidation(), Transfers.ValidateDate(command, today).ToValidation())
            .Apply(static (_, valid) => valid)
            .As();
    public static IO<Unit> Handle(BookTransfer command, DateOnly today) =>
        from valid in IO.lift(ValidateCommand(command, today).ToFin())
        from _ in Persistence.Save(valid)
        select unit;
    public static int Exit(IO<Unit> handler) =>
        handler.RunSafe().Match(
            Succ: static _ => 200,
            Fail: static error => error.IsExceptional ? 500 : 400);
}
```

The tuple `Apply` combines the two independent validators and reports both violations together. At the outer boundary, `RunSafe` returns one `Fin<Unit>`, and `Match` separates the three reachable outcomes:
- `Fail` with an `Expected` error or `ManyErrors`: expose actionable business errors.
- `Fail` with an `Exceptional` error: log the technical detail and expose a generic failure.
- `Succ(unit)`: return success.

The host reads the accumulated leaves with `Filter<E>`, `Count`, and `Head` in the same `Match`.

This prevents infrastructure details from leaking to clients while retaining useful validation feedback.

## Exception policy

Do not use exceptions for expected business outcomes. Reserve them for conditions the normal workflow is not meant to recover from:
- Developer defects, such as violating a function’s required preconditions. These indicate broken program logic and should not be caught as business errors.
- Configuration failures discovered during initialization that make the application unable to operate. Let them terminate initialization, apart from an outermost application handler.
- Exception-based third-party APIs. Catch narrowly and convert immediately to an explicit functional value.
