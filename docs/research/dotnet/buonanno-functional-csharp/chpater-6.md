# Functional Error Handling

## Errors belong in the return type

An operation that can predictably fail should return both possible outcomes as data. Its signature then states the failure contract, callers can reason about it locally, and ordinary composition controls the flow. An exception, by contrast, transfers control to a handler somewhere up the call stack or escapes uncaught; understanding the next step requires tracing the surrounding call paths.

Use `Option<T>` when failure means only “no value” and no explanation is useful. Use `Either<L, R>` when the caller needs failure details:

```text
Either<L, R> = Left(L) | Right(R)

Left  = failure data
Right = successful result
```

The right side is the value being transformed. The left side carries the error unchanged through the rest of a failed workflow.

```csharp
Either<string, double> Calculate(double x, double y)
{
    if (y == 0) return Left("y cannot be 0");
    if (x != 0 && Math.Sign(x) != Math.Sign(y))
        return Left("x / y cannot be negative");

    return Right(Math.Sqrt(x / y));
}
```

This signature is honest: every caller knows that calculation can fail and knows the error type it must handle.

## Core operations

A right-biased `Either` applies functions only to `Right`. `Left` bypasses the function and preserves its error.

```csharp
public static Either<L, R2> Map<L, R, R2>(
    this Either<L, R> either,
    Func<R, R2> map)
    => either.Match<Either<L, R2>>(
        Left: error => Left(error),
        Right: value => Right(map(value)));

public static Either<L, R2> Bind<L, R, R2>(
    this Either<L, R> either,
    Func<R, Either<L, R2>> next)
    => either.Match(
        Left: error => Left(error),
        Right: next);
```

- `Map` transforms a successful value with `R -> R2`.
- `Bind` composes a step with `R -> Either<L, R2>` and flattens the result.
- `ForEach` performs an action only for `Right` and represents its completed action with `Unit`.
- `Match` handles both cases and returns an ordinary value. It exits the abstraction, so delay it until a boundary.

`Where` is not generally definable for `Either<L, R>`. A false predicate must produce `Left(L)`, but a predicate supplies only `bool` and there is no universal empty value for arbitrary `L`. Turn the predicate into a validator that constructs a specific error, then compose it with `Bind`.

A function of shape `R -> Either<L, R2>` crosses from an ordinary value into an explicit outcome. `Bind` composes such functions while keeping the workflow inside `Either`; `Match` is the downward crossing that finally interprets the outcome as something outside the abstraction.

## Fail-fast workflows

`Bind` produces a two-track pipeline. Each successful step advances on the right track; the first failure moves to the left track, skips every later step, and reaches the final handler unchanged.

```csharp
Func<Request, Either<Error, ValidRequest>> Validate;
Func<ValidRequest, Either<Error, Model>> Load;
Func<Model, Either<Error, UpdatedModel>> Update;
Func<UpdatedModel, Either<Error, Unit>> Save;

Either<Error, Unit> Handle(Request request)
    => Validate(request)
        .Bind(Load)
        .Bind(Update)
        .Bind(Save);
```

Use `Unit` when success has no meaningful payload. The pipeline still returns an explicit success value rather than relying on `void` or an implicit absence.

All bound functions must agree on the left type. Choose a stable error representation for the workflow before composing it.

## Typed business validation

Prefer distinct error types over strings. A string is too limited for structured error details, while `Exception` has the wrong meaning for expected, business-as-usual failures. Specific `Error` subtypes give each failure a domain identity and room for richer data.

```csharp
public class Error
{
    public virtual string Message { get; }
}

public sealed class InvalidBic : Error
{
    public override string Message
        => "The beneficiary BIC is invalid";
}

public sealed class TransferDateIsPast : Error
{
    public override string Message
        => "Transfer date cannot be in the past";
}

Either<Error, BookTransfer> ValidateBic(BookTransfer command)
{
    if (!BicRegex.IsMatch(command.Bic))
        return Left(new InvalidBic());
    return Right(command);
}

Either<Error, BookTransfer> Validate(BookTransfer command)
    => Right(command)
        .Bind(ValidateBic)
        .Bind(ValidateDate);
```

Each validator has the same shape: accept the request, return that request on success, or return the specific error for the violated rule. The date validator checks that the transfer is in the future; the BIC validator checks the identifier's format. Returning the request on the right makes it available to the next validator. Because this pipeline uses `Bind`, it stops at the first invalid result.

## Keep the abstraction inside the application core

Within the core, continue composing with `Map` and `Bind`. Translate only in an outer adapter where the protocol, UI, or host requires a concrete response.

```csharp
IActionResult BookTransfer(BookTransfer request)
    => Handle(request).Match<IActionResult>(
        Left: error => BadRequest(error),
        Right: _ => Ok());
```

For an optional lookup, a boundary can translate `None` to “not found” and `Some(value)` to a successful response. For `Either`, the boundary must decide how domain failures map to the external contract.

Two viable API designs are:
- Map `Left` and `Right` to protocol status codes and payloads.
- Always return a transport-success response whose body is a result DTO with `Succeeded` plus either `Data` or `Error`. Unlike `Either`, this DTO exposes its values directly for serialization and client access.

Mapping business validation to an HTTP error such as 400 is debatable: the request may be syntactically valid yet violate a business rule, and concurrent changes can invalidate a request between creation and receipt. The choice is an API-design decision. The invariant is that protocol details stay in the adapter and the core retains its explicit outcome type.

## Adapting error types

The usual `Map` changes only `R`; `L` remains fixed. When integrating functions with different error types, map both sides at the join point:

```csharp
public static Either<L2, R2> Map<L, L2, R, R2>(
    this Either<L, R> either,
    Func<L, L2> mapLeft,
    Func<R, R2> mapRight)
    => either.Match<Either<L2, R2>>(
        Left: error => Left(mapLeft(error)),
        Right: value => Right(mapRight(value)));
```

This operation is also called `BiMap`. It makes interoperation possible, but a consistent error model is clearer than repeatedly adapting incompatible representations.

## Separate business failures from technical failures

Specialized forms of `Either` make intent clearer and reduce generic noise:

```text
Validation<T>  = Invalid(IEnumerable<Error>) | Valid(T)
Exceptional<T> = Exception | Success(T)
```

- `Validation<T>` represents violated business rules. Its failure type is fixed to a sequence of domain errors, avoiding the repeated left-type argument and allowing multiple errors to be carried.
- `Exceptional<T>` represents infrastructure, integration, or other technical failure.

These specializations are common but not standardized; libraries may use different names and minor variations in behavior.

Convert an exception-throwing dependency immediately at the integration boundary. Keep the `try/catch` scope as small as possible.

```csharp
Exceptional<Unit> Save(BookTransfer command)
{
    try
    {
        ConnectionHelper.Connect(
            connectionString,
            connection => connection.Execute("INSERT ...", command));
        return Success(Unit());
    }
    catch (Exception exception)
    {
        return Exception(exception);
    }
}
```

If validation returns `Validation<BookTransfer>` and persistence returns `Exceptional<Unit>`, they cannot be flattened with one `Bind` because they express different effects. Map persistence over the validated value so it runs only for `Valid`, while preserving both meanings in the result:

```csharp
Validation<Exceptional<Unit>> Handle(BookTransfer command)
    => Validate(command).Map(Save);
```

The nested type makes the three reachable outcomes explicit. At the outer boundary, match the layers separately:
- `Invalid(errors)`: expose actionable business errors.
- `Valid(Exception(exception))`: log the technical detail and expose a generic failure.
- `Valid(Success(Unit))`: return success.

This prevents infrastructure details from leaking to clients while retaining useful validation feedback.

## Exception policy

Do not use exceptions for expected business outcomes. Reserve them for conditions the normal workflow is not meant to recover from:
- Developer defects, such as violating a function’s required preconditions. These indicate broken program logic and should not be caught as business errors.
- Configuration failures discovered during initialization that make the application unable to operate. Let them terminate initialization, apart from an outermost application handler.
- Exception-based third-party APIs. Catch narrowly and convert immediately to an explicit functional value.
