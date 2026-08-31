# Structuring an Application with Functions

## The central design move

A multi-argument function can be defined at a high level of generality, then specialized by supplying stable decisions before runtime data. Each supplied argument produces a narrower function whose remaining inputs belong to a later stage of the application lifecycle.

```text
general function
  -> apply configuration
  -> apply operation policy
  -> apply runtime input
  -> result
```

This separates who chooses each input and when it becomes available. The final consumer receives a function tailored to its exact need and does not need to know the configuration or construction process behind it.

## Partial application

Partial application gives a function fewer arguments than it ultimately needs and returns a function with the supplied values captured.

```csharp
Func<string, string, string> greet =
    (greeting, name) => $"{greeting}, {name}";

public static Func<T2, R> Apply<T1, T2, R>(
    this Func<T1, T2, R> function,
    T1 first)
    => second => function(first, second);

Func<string, string> greetFormally = greet.Apply("Good evening");
```

The implementation does not change; only the point at which each input is chosen changes. The captured value remains available through the returned closure.

Overloads can support greater arities:

```csharp
public static Func<T2, T3, R> Apply<T1, T2, T3, R>(
    this Func<T1, T2, T3, R> function,
    T1 first)
    => (second, third) => function(first, second, third);
```

### Order arguments by lifecycle

Place earlier, more stable inputs first and later, operation-specific inputs last:
1. Dependencies and configuration known during application composition
2. Policies or options that select behavior
3. The value or entity being acted upon at runtime

For example:

```text
ConnectionString -> SqlTemplate -> QueryParameters -> Result
Clock -> Command -> Validation<Command>
```

This ordering makes left-to-right partial application useful. If an API puts a short-lived value before stable configuration, adapt its signature so that application setup can supply the stable values first.

## Currying is a transformation, not application

Currying converts one function that accepts several arguments into a chain of unary functions.

```text
(T1, T2, T3) -> R

becomes

T1 -> T2 -> T3 -> R
```

```csharp
public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(
    this Func<T1, T2, R> function)
    => first => second => function(first, second);

var curried = greet.Curry();
var greetInformally = curried("Hey");
var message = greetInformally("Sam");
```

The distinction matters:
- Currying supplies no values. It only changes the function's shape.
- Partial application supplies one or more values and produces a specialized function.
- A curried function makes partial application a normal function call.
- `Apply` enables partial application without first currying the function.

Currying has little value when every argument is always supplied together. Its purpose is to make staged specialization convenient. A function can be written directly in curried form, transformed with `Curry` and then invoked successively, or specialized argument by argument with `Apply`. Arrow notation is right-associative and is commonly written in curried form even when the concrete C# delegate accepts several parameters; the actual `Func` shape determines whether successive calls are possible.

## Working with C# method resolution

C# distinguishes methods, method groups, lambdas, and delegate values. A unary method often converts cleanly where a `Func<T, R>` is expected, but generic higher-order operations over multi-argument method groups can defeat type inference. Local functions behave like methods and have the same limitation.

Possible workarounds include explicit generic arguments or an explicit delegate cast, but both add noise. For functions intended to participate heavily in partial application or other higher-order operations, expose a delegate value instead:

```csharp
Func<string, string, string> Greeter =
    (greeting, name) => $"{greeting}, {name}";

Func<string, string, string> GreeterProperty =>
    (greeting, name) => $"{greeting}{Separator}{name}";

Func<string, TName, string> CreateGreeter<TName>() =>
    (greeting, name) => $"{greeting}: {name}";
```

Choose the delegate-producing form deliberately:
- A field is simple but an inline field initializer cannot depend on instance state.
- A getter-only property can create a delegate that closes over instance state.
- A factory method can also introduce generic type parameters, which fields and properties cannot.

Returning `Func` values from adapter or factory methods is often the cleanest way to cross from method-oriented APIs into a function-composition pipeline.

## Designing a specialization-friendly boundary

An existing API may expose arguments in an order that works poorly for partial application. A thin adapter can:
- put stable inputs before transient ones;
- expose semantic types instead of ambiguous primitives;
- acquire a short-lived resource only when the operation runs;
- return a `Func` so subsequent specialization benefits from delegate inference.

```csharp
public static Func<SqlTemplate, object, IEnumerable<T>> Query<T>(
    this ConnectionString connectionString)
    => (template, parameters) =>
        Connect(
            connectionString,
            connection => connection.Query<T>(template, parameters));
```

The extension-method receiver allows the connection string to be supplied before the queried type, and the call returns a `Func` for clean subsequent application. The connection string is stable and can be supplied during startup. The actual connection is lightweight and short-lived: it is opened and disposed for each query rather than captured as configuration. The template can then be supplied to create an operation-specific query, leaving only invocation-specific parameters:

```csharp
var queryEmployees = configuredConnection.Query<Employee>();
var queryById = queryEmployees.Apply(employeeByIdTemplate);
var queryByLastName = queryEmployees.Apply(employeeByLastNameTemplate);

Func<Guid, Option<Employee>> lookupEmployee =
    id => queryById(new { Id = id }).SingleOrNone();
Func<string, IEnumerable<Employee>> findEmployeesByLastName =
    name => queryByLastName(new { LastName = name });
```

Custom types such as `ConnectionString` and `SqlTemplate` make signatures intention-revealing and provide appropriate homes for extension methods that would not belong on `string`.

## Functions as dependencies

A dependency should describe exactly the behavior a consumer needs. A clock is `Func<DateTime>`; a validator is `T -> Validation<T>`; a persistence operation is `T -> Exceptional<Unit>`.

In the chapter's signatures, `Option<T>` makes lookup absence explicit, `Validation<T>` carries a valid value or validation errors, and `Exceptional<Unit>` reports either completion without a result value or an exception.

```csharp
public delegate Validation<T> Validator<T>(T value);

public static Validator<BookTransfer> DateNotPast(Func<DateTime> clock)
    => command =>
        command.Date.Date < clock().Date
            ? Invalid(TransferDateIsPast)
            : Valid(command);
```

`DateNotPast` is a function factory and a curried binary function in practice. Composition supplies the clock once; request handling supplies the command later. A test supplies a deterministic clock without constructing a fake service object.

Function dependencies preserve the useful properties normally sought through interfaces:
- The consumer is decoupled from the implementation.
- Tests can inject small deterministic functions.
- Each signature states the minimum capability required.
- Single-method interfaces and mock setup are unnecessary.

This naturally enforces interface segregation: a consumer that only saves receives only `T -> Exceptional<Unit>`, not a repository abstraction that also exposes lookup and unrelated operations. If a consumer truly needs several independent behaviors, those functions remain separate and explicit.

Objects and interfaces remain compatible with this style. Functional behavior can live behind a framework controller when that framework provides useful request and response handling there.

## The composition root

Construct specialized functions at the outermost bootstrap boundary:
1. Read stable configuration.
2. Adapt infrastructure APIs into application-shaped functions.
3. Partially apply dependencies, policies, and templates.
4. Combine small domain functions into the workflow required by the host.
5. Inject only the final narrow functions into controllers or handlers.

The framework entry point can remain thin while the behavior it invokes is supplied as narrow functions. Composition uses ordinary function application rather than requiring an inversion-of-control container.

## Folding many values into one

LINQ's `Aggregate` is a left fold. It consumes a seed and a reducer and returns one accumulated value:

```text
Aggregate : (IEnumerable<T>, Acc, (Acc, T) -> Acc) -> Acc

result = f(f(f(seed, item0), item1), item2) ...
```

The seed defines the empty-input result and may have a different type from the elements. Examples include `0` for a sum, `0` plus an incrementing reducer for a count, or an empty immutable tree plus an insertion reducer for building a tree.

The seedless overload uses the first element as the accumulator. It requires a non-empty sequence and constrains the result to the element type, so it is less general than the seeded form.

`Aggregate` is general enough to express `Map`, `Where`, and `Bind`; here its important use is reducing many validators to one validator.

## Combining validators: choose semantics first

A collection of validators can be folded into one validator:

```text
IEnumerable<T -> Validation<T>> -> T -> Validation<T>
```

There are two materially different compositions.

### Fail fast for efficiency

```csharp
public static Validator<T> FailFast<T>(
    IEnumerable<Validator<T>> validators)
    => value => validators.Aggregate(
        Valid(value),
        (result, validate) => result.Bind(_ => validate(value)));
```

`Bind` skips remaining validators after the first invalid result. An empty validator list returns the input as valid. Order cheap structural checks before expensive database or remote checks so invalid data fails before consuming costly resources.

Use this strategy when minimizing work matters more than reporting every issue, such as validation of a programmatic request.

### Harvest errors for independent checks

To report every violated rule, evaluate every validator independently, keep only invalid results, flatten their error groups, and return one combined result:

```csharp
public static Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)
    => value =>
    {
        var errors = validators.Map(validate => validate(value))
            .Bind(result => result.Match(
                Valid: _ => None,
                Invalid: errs => Some(errs)))
            .Flatten();
        return errors.Any() ? Invalid(errors) : Valid(value);
    };
```

Do not implement harvesting with monadic `Bind`: its short-circuit behavior prevents later checks from running. Error harvesting is appropriate for user-submitted forms where reporting every violated rule lets the user fix all errors before submitting again.
