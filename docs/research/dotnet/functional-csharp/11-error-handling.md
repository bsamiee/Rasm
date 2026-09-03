<!-- Fully integrated into dotnet-coding/SKILL.md, dotnet-coding/references/results.md, and dotnet-languageext/SKILL.md, each section carries its marker -->
# [ERROR_HANDLING]

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [01]-[ERRORS_IN_THE_RETURN_TYPE]

Operations that can predictably fail return both outcomes as data. The signature states the failure behavior, callers reason about it locally, and composition controls the flow. Exceptions, by contrast, transfer control to a handler up the call stack or escape uncaught. Understanding the next step requires tracing the surrounding call paths.

Use `Option<T>` when failure means only “no value” and no explanation is useful. Use `Fin<A>` when the caller needs failure details. `Either<L, R>` stays for two value types where neither side is an error.

```text
Fin<A> = Fail(Error) | Succ(A)

Fail = failure data, always an Error
Succ = successful result
```

The success side is the value being transformed. The failure side carries the error. Both `A` and `Error` convert to `Fin<A>` without a constructor call.

```csharp
internal static class Calculator {
    public static Fin<double> Calculate(double x, double y) =>
        y == 0 ? Error.New("y cannot be 0")
        : x != 0 && Math.Sign(x) != Math.Sign(y) ? Error.New("x / y cannot be negative")
        : Math.Sqrt(x / y);
}
```

### [01.1]-[RESULT_AND_ERROR_FIELDS]

Wrappers around boundary calls have two failure modes. Returning `default` on failure swallows the exception and makes failure indistinguishable from a valid default result. Separate result and error fields preserve both outcomes, but every success carries an unused error field and every failure an unused result field. Only mutually exclusive cases remove the invalid combinations.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [02]-[CORE_OPERATIONS]

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

- `Map` transforms a successful value with `A -> B`
- `Bind` composes a step with `A -> Fin<B>` and flattens the result
- `Iter` performs an action only for `Succ` and returns `Unit`
- `Match` handles both cases and returns a value outside `Fin`

`Fin<A>` has no `Where`. False predicates must produce `Fail`, but a predicate supplies only `bool` and no `Error`. Turn the predicate into a validator that constructs a specific error, then compose it with `Bind`. Inside a LINQ query the same check is a `guard` clause.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [03]-[FAIL_FAST_WORKFLOWS]

`Bind` produces a fail-fast pipeline. Each `Succ` passes its value to the next step. The first `Fail` skips all later steps and reaches the final handler.

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

Use `Unit` when success has no payload. The pipeline returns an explicit success value instead of relying on `void` or implicit absence.

All bound functions share the failure type `Error`. Choose the domain errors for the workflow before composing it.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [04]-[TYPED_VALIDATION]

Prefer distinct error types over strings. Strings cannot carry structured error details. Specific `Expected` records give each failure a distinct type and code and can carry additional data. Each package declares the record beside the function that returns it or the value object it protects.

```csharp
internal sealed record BookTransfer(string Bic, DateOnly Date);

internal static class Codes {
    public const int InvalidBic = 1;
    public const int TransferDateIsPast = 2;
    public const int TransferRejected = 3;
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

Each validator has the same shape: accept the request, return it on success, or return the error for the violated rule. The date validator checks that the transfer is in the future and receives the clock as an argument. The BIC validator checks the identifier's format. Returning the request in `Succ` makes it available to the next validator. `Codes` classes hold the codes of one package, and a consumer classifies an error with `Is`, `HasCode`, or `IsType<E>`, never by its message text.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [05]-[ABSTRACTION_SCOPE]

Within the core, compose with `Map` and `Bind`. Translate only in an outer adapter when the protocol, UI, or host requires another response type. Choose the result type at the input boundary and keep it through the domain. Use `Match`, `RunSafe`, and `IfFail` only at host boundaries. Every library returns its result type with its own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation.

```csharp
IActionResult Post(Request request) =>
    Workflow.Handle(request).Match<IActionResult>(
        Succ: static _ => Ok(),
        Fail: static error => BadRequest(error));
```

For an optional lookup, a boundary can translate `None` to “not found” and `Some(value)` to a successful response. For `Fin`, the boundary must decide how domain failures map to the external contract.

API designs are:
- Map `Fail` and `Succ` to protocol status codes and payloads
- Always return a response with a successful transport status and a body that is a result DTO with `Succeeded` and either `Data` or `Error`, which unlike `Fin` exposes its values directly for serialization and client access

Mapping business validation to an HTTP error (400) has tradeoffs: the request can be syntactically valid yet violate a business rule, and concurrent changes can invalidate it between creation and receipt. The choice is an API-design decision.
-->

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
## [06]-[ERROR_ADAPTATION]

`MapFail` changes only the `Error`, and `BiMap` maps both sides:

```csharp
internal sealed record TransferRejected : Expected {
    public TransferRejected(Error cause) : base("transfer rejected", Codes.TransferRejected, cause) { }
}

internal static class Adapters {
    public static Fin<BookTransfer> WithContext(Fin<BookTransfer> result) =>
        result.MapFail(static error => new TransferRejected(error));
    public static Fin<string> Describe(Fin<BookTransfer> result) =>
        result.BiMap(
            Succ: static command => command.Bic,
            Fail: static error => new TransferRejected(error));
    public static Fin<BookTransfer> Recover(Fin<BookTransfer> result, BookTransfer fallback) =>
        result.Catch(Codes.InvalidBic, _ => fallback).As();
}
```

Each package translates the errors of a dependency that it reacts to into its own `Expected` record with `MapFail`, keeps the original as `Inner`, and passes every other error through unchanged. `Error.New(string, Error)` has code `0`, and `IsType`, `HasCode`, `Is`, and `Catch` do not descend into `Inner`, only the typed record stays classifiable. `Catch(code, f)` selects by code, `Catch(Error, f)` by value, and `Catch(predicate, f)` by a test on the error.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [07]-[BUSINESS_AND_TECHNICAL_FAILURES]

Use two types with `Fin<A>` to distinguish business failures from technical failures:

```text
Validation<Error, A> = Fail(Error) | Success(A)
IO<A>                = a deferred effect that fails with Error or yields A
```

- `Validation<Error, A>` represents violated business rules. Its failure type is fixed to `Error`, and `+` on `Error` combines failures into `ManyErrors`. `IsType<E>` and `IsExceptional` on `ManyErrors` test its members.
- `IO<A>` represents infrastructure, integration, or other technical work. Thrown exceptions arrive on its error channel as `Exceptional` errors.

Class-based `Result<T>` with `Success<T>` and `Failure<T>` cases, where `Failure<T>` carries an exception, is equivalent to `Fin<A>` with an `Exceptional` error. Its `Success<T>` does not prevent a `null` payload. Use `Option<T>` for absence.

Convert an exception-throwing dependency at the integration boundary. `IO.lift` captures exceptions from only that call.

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

The tuple `Apply` combines the independent validators and reports both violations together. At the outer boundary, `RunSafe` returns one `Fin<Unit>`, and `Match` separates the reachable outcomes:
- `Fail` with an `Expected` error or `ManyErrors`: expose the business errors, and log the `Inner` of a translated dependency failure
- `Fail` with an `Exceptional` error: log the technical detail and expose a generic failure
- `Succ(unit)`: return success

In the same `Match`, the host reads individual accumulated errors with `Filter<E>`, `Count`, and `Head`.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [08]-[EXCEPTION_POLICY]

Do not use exceptions for expected business outcomes. Reserve them for conditions that the workflow cannot recover from:
- Developer defects that violate a function’s required preconditions. Do not catch them as business errors.
- Configuration failures discovered during initialization that make the application unable to operate. Let them terminate initialization, apart from an outermost application handler.
- Exception-based third-party APIs. Catch narrowly and convert immediately to an explicit functional value.
-->
