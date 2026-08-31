# [CURRYING_AND_PARTIAL_APPLICATION]

## [01]-[DESIGN_PRINCIPLE]

A general multi-argument function can be specialized by supplying stable inputs before runtime data. Each supplied argument returns a specialized function whose remaining inputs become available later in the application lifecycle.

```text
general function
  -> apply configuration
  -> apply operation policy
  -> apply runtime input
  -> result
```

The final consumer receives a function that accepts only its remaining inputs and does not need to know its configuration or construction process.

## [02]-[CORE_DISTINCTION]

- Currying transforms a function of `N` arguments into a chain of `N` unary functions. Each call accepts exactly one argument and returns the next function; the final call returns the result.
- Partial application supplies fewer than all of the original arguments at once and returns a function for the arguments that remain. The Prelude function `par` fixes a leading group of arguments, and the returned function can accept several arguments. Supplying every argument is full application and produces the result.

For a two-argument function:

```csharp
internal static class Shapes {
    public static readonly Func<decimal, decimal, decimal> Add = static (x, y) => x + y;
    public static readonly Func<decimal, Func<decimal, decimal>> CurriedAdd = static x => y => Add(x, y);
    public static readonly Func<decimal, decimal> Add100 = CurriedAdd(100m);
}
```

`Add100(200m)` is `300` and `Add100(900m)` is `1000`. The returned function retains each supplied value but does not invoke the original function until the remaining arguments arrive. A single general implementation can produce many reusable specializations.

## [03]-[PARTIAL_APPLICATION]

```csharp
internal static class Greetings {
    public static readonly Func<string, string, string> Greet = static (greeting, name) => $"{greeting}, {name}";
    public static readonly Func<string, string> GreetFormally = par(Greet, "Good evening");
}
```

The Prelude provides `par` for each arity and number of supplied arguments, and `lpar` fixes the second argument of a two-argument function.

### [03.1]-[IMPLEMENTATION]

Partial application avoids unnecessary unary stages when several arguments are always fixed together. `Parsing.ParseBooks` is the configurable parser used later:

```csharp
internal static class Partials {
    public static readonly Func<string, Seq<Book>> ParseLinuxComma = par(Parsing.ParseBooks, Parsing.KeepHeader, "\n", ",");
    public static readonly Func<string, string, Seq<Book>> ParseWindows = par(Parsing.ParseBooks, Parsing.SkipHeader, Environment.NewLine);
    public static readonly Func<string, Seq<Book>> ParseWindowsComma = par(ParseWindows, ",");
}
```

### [03.2]-[ARGUMENT_ORDER]

Place stable inputs first and runtime inputs last:
1. Dependencies and configuration known at the composition root
2. Policies or options that select behavior
3. The value or entity being acted upon at runtime

For example, `ConnectionIO -> SqlTemplate -> QueryParameters -> Result` and `Clock -> Command -> Validation<Error, Command>`.

This ordering makes left-to-right partial application useful. If an API puts a short-lived value before stable configuration, adapt its signature so that application setup can supply the stable values first.

## [04]-[CURRYING]

```csharp
internal static class Curried {
    public static readonly Func<string, Func<string, string>> Greet = curry(Greetings.Greet);
    public static readonly Func<string, string> GreetInformally = Greet("Hey");
    public static string Message => GreetInformally("Sam");
}
```

Currying supports specialization in stages; it adds nothing when every argument is supplied together. A function can be written directly in curried form, transformed with `curry` and then invoked successively, or specialized argument by argument with `par`. Arrow notation is right-associative and is commonly written in curried form even when the concrete C# delegate accepts several parameters; the `Func` shape determines whether successive calls are possible.

### [04.1]-[IMPLEMENTATION]

C# has no built-in automatic currying. The Prelude function `curry` transforms a function of two or more arguments. Write a function directly in curried form, such as `Shapes.CurriedAdd`, when consumers always supply one argument at a time.

```csharp
internal static class Helper {
    public static readonly Func<decimal, Func<decimal, decimal>> Add = curry(static (decimal x, decimal y) => x + y);
    public static readonly Func<decimal, decimal> Add10 = Add(10m);
    public static decimal Answer => Add10(100m); // 110
}
```

Explicit lambda parameter types can be needed because the compiler does not always infer the delegate's generic arguments at this call site. A delegate value with a declared `Func` type, such as `Greetings.Greet`, needs no annotation.

### [04.2]-[SPECIALIZED_FUNCTIONS]

```csharp
internal sealed record Book(string Title, string Author, string PublicationDate);

internal static class Parsing {
    public const bool SkipHeader = true;
    public const bool KeepHeader = false;
    public static readonly Func<bool, string, string, string, Seq<Book>> ParseBooks =
        static (skipHeader, lineBreak, fieldDelimiter, content) =>
            toSeq(content.Split(lineBreak))
                .Skip(skipHeader ? 1 : 0)
                .Map(line => line.Split(fieldDelimiter))
                .Map(static fields => new Book(fields[0], fields[1], fields[2]));
}
```

Currying turns its four-argument shape into `bool -> string -> string -> string -> Seq<Book>`. Each stage can be saved and used to create multiple specializations:

```csharp
internal static class Families {
    public static readonly Func<bool, Func<string, Func<string, Func<string, Seq<Book>>>>> Curried = curry(Parsing.ParseBooks);
    public static readonly Func<string, Func<string, Func<string, Seq<Book>>>> ParseWithHeader = Curried(true);
    public static readonly Func<string, Func<string, Seq<Book>>> ParseWindowsWithHeader = ParseWithHeader(Environment.NewLine);
    public static readonly Func<string, Seq<Book>> ParseWindowsComma = ParseWindowsWithHeader(",");
    public static readonly Func<string, Seq<Book>> ParseWindowsPipe = ParseWindowsWithHeader("|");
}
```

Fixing a logger's `LogLevel` creates `logInfo`, `logWarning`, and `logError`. Each function needs only a message and can be passed where logging at that level is required.

### [04.3]-[PIPELINES]

Partial application can turn general operations into unary functions suitable for mapping or composition. For noncommutative operations, choose parameter order deliberately: the first parameter is fixed first, while the last one is the pipeline value. `Pipe` applies the function on its right to the value on its left.

```csharp
internal static class PipeExtensions {
    public static R Pipe<T, R>(this T value, Func<T, R> function) => function(value);
}

internal static class Temperature {
    private static readonly Func<decimal, decimal, decimal> SubtractBase = static (fixedValue, input) => input - fixedValue;
    private static readonly Func<decimal, decimal, decimal> MultiplyBase = static (fixedValue, input) => input * fixedValue;
    private static readonly Func<decimal, decimal, decimal> DivideBase = static (fixedValue, input) => input / fixedValue;
    public static decimal FahrenheitToCelsius(decimal value) => value.Pipe(par(SubtractBase, 32m)).Pipe(par(MultiplyBase, 5m)).Pipe(par(DivideBase, 9m));
}
```

## [05]-[METHOD_RESOLUTION]

C# distinguishes methods, method groups, lambdas, and delegate values. A unary method converts where a `Func<T, R>` is expected, but generic higher-order operations over multi-argument method groups can defeat type inference. Local functions behave like methods and have the same limitation.

Explicit generic arguments and delegate casts are available, but add syntax. `fun` gives a lambda its `Func` type at the call site, so the lambda can be invoked or passed without a declared local. For functions frequently used in partial application or other higher-order operations, expose a delegate value:

```csharp
internal sealed class Greeter(string separator) {
    public static readonly Func<string, string, string> Greet = static (greeting, name) => $"{greeting}, {name}";
    public Func<string, string, string> GreetProperty => (greeting, name) => $"{greeting}{separator}{name}";
    public static Func<string, TName, string> CreateGreeter<TName>() => static (greeting, name) => $"{greeting}: {name}";
    public static string GreetInformally(string name) => fun(static (string greeting, string who) => $"{greeting} {who}")("Hey", name);
}
```

Choose the delegate-producing form:
- A field initializer cannot depend on instance state.
- A getter-only property can create a delegate that closes over instance state.
- A factory method can also introduce generic type parameters, which fields and properties cannot.

Return `Func` values from adapter or factory methods to cross from method-based APIs into a function-composition pipeline.

## [06]-[API_DESIGN]

An existing API can expose arguments in an order that works poorly for partial application. An adapter can:
- expose domain-specific types instead of ambiguous primitives;
- acquire a short-lived resource only when the operation runs;
- return a `Func` so subsequent specialization benefits from delegate inference.

```csharp
internal sealed record SqlTemplate(string Text);
internal sealed record Employee(Guid Id, string LastName);

internal interface ConnectionIO {
    public Seq<T> Query<T>(SqlTemplate template, object parameters);
}

internal static class Queries {
    public static readonly SqlTemplate EmployeeById = new("select * from employee where id = @Id");
    public static Func<SqlTemplate, object, Eff<RT, Seq<T>>> Query<RT, T>() where RT : Has<Eff<RT>, ConnectionIO> =>
        static (template, parameters) => Has<Eff<RT>, RT, ConnectionIO>.ask.As().Map(connection => connection.Query<T>(template, parameters));
}
```

The runtime `RT` supplies the connection through the trait `Has<Eff<RT>, ConnectionIO>`, and the call returns a `Func` for subsequent application. The runtime record holds the capability from startup, and the implementation opens the short-lived connection when the query runs. The effect type `Eff<RT, Seq<T>>` reads the trait only when the host runs the effect, not when the query function is built. The template can then be supplied to create an operation-specific query, leaving only invocation-specific parameters:

```csharp
internal static class Lookups<RT> where RT : Has<Eff<RT>, ConnectionIO> {
    private static readonly Func<SqlTemplate, object, Eff<RT, Seq<Employee>>> QueryEmployees = Queries.Query<RT, Employee>();
    private static readonly Func<object, Eff<RT, Seq<Employee>>> QueryById = par(QueryEmployees, Queries.EmployeeById);
    public static readonly Func<Guid, Eff<RT, Option<Employee>>> LookupEmployee = static id => QueryById(new { Id = id }).Map(static rows => rows.Head);
}
```

Custom types such as `ConnectionIO` and `SqlTemplate` make signatures intention-revealing and can own extension methods that do not belong on `string`. `Seq<A>.Head` is an `Option<A>`, so lookup absence stays explicit.

## [07]-[FUNCTIONS_AS_DEPENDENCIES]

A dependency must describe the behavior a consumer needs. A clock is `Func<DateTime>`; a validator is `T -> Validation<Error, T>`; a persistence operation is `T -> IO<Unit>`.

In these signatures, `Option<T>` makes lookup absence explicit, `Validation<Error, T>` carries a valid value or accumulated errors, and `IO<Unit>` is a deferred effect that completes with no result value or fails on its error channel.

```csharp
internal sealed record TransferDateIsPast() : Expected("The transfer date is in the past", 100);
internal sealed record BookTransfer(Guid OwnerId, DateTime Date, decimal Amount);

internal static class Validators {
    public static Func<BookTransfer, Validation<Error, BookTransfer>> DateNotPast(Func<DateTime> clock) =>
        command => command.Date.Date < clock().Date ? new TransferDateIsPast() : command;
}
```

`DateNotPast` is a function factory and a curried binary function. Composition supplies the clock once; request handling supplies the command later. A test supplies a deterministic clock without constructing a fake service object. The error is a typed `Expected` record and the command lifts into `Validation<Error, BookTransfer>` by implicit conversion.

Function dependencies provide the same properties as interfaces:
- The consumer is decoupled from the implementation.
- Tests can inject deterministic functions.
- Single-method interfaces and mock setup are unnecessary.

This enforces interface segregation: a consumer that only saves receives only `T -> IO<Unit>`, not a repository abstraction that also exposes lookup and unrelated operations. If a consumer needs several independent behaviors, those functions remain separate and explicit.

Objects and interfaces remain compatible with this style. Functional behavior can live behind a framework controller that handles requests and responses.

## [08]-[COMPOSITION_ROOT]

Construct specialized functions at the composition root:
1. Read stable configuration.
2. Adapt infrastructure APIs into functions that match application needs.
3. Partially apply dependencies, policies, and templates.
4. Combine small domain functions into the workflow required by the host.
5. Inject only specialized functions with the required inputs into controllers or handlers.

The runtime record holds the configuration and implements one `Has` trait per capability. The workflow is generic over `RT` and builds an `Eff<RT, A>` from its function dependencies. The host runs the effect once with `Run(rt)`, which returns `Fin<A>`. `Eff<RT, A>.Lift(Func<Fin<A>>)` carries a `Fin` into the query, and `from _ in save(valid)` binds the `IO<Unit>` dependency.

```csharp
internal sealed record OwnerNotFound() : Expected("The transfer owner is unknown", 101);
internal sealed record Runtime(ConnectionIO Connection, Func<DateTime> Clock) : Has<Eff<Runtime>, ConnectionIO> {
    static K<Eff<Runtime>, ConnectionIO> Has<Eff<Runtime>, ConnectionIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Connection);
}

internal static class Workflow {
    public static Eff<RT, Employee> Book<RT>(
        Func<BookTransfer, Validation<Error, BookTransfer>> validate, Func<Guid, Eff<RT, Option<Employee>>> lookup,
        Func<BookTransfer, IO<Unit>> save, BookTransfer command) =>
        from valid in Eff<RT, BookTransfer>.Lift(() => validate(command).ToFin())
        from owner in lookup(valid.OwnerId)
        from found in Eff<RT, Employee>.Lift(() => owner.ToFin(new OwnerNotFound()))
        from _ in save(valid)
        select found;
}

internal static class Host {
    public static Fin<Employee> Book(Runtime runtime, Func<BookTransfer, IO<Unit>> save, BookTransfer command) =>
        Workflow.Book(Validators.DateNotPast(runtime.Clock), Lookups<Runtime>.LookupEmployee, save, command).Run(runtime);
}
```

The framework entry point can remain thin while the behavior it invokes is supplied as narrow functions. Composition uses ordinary function application rather than requiring an inversion-of-control container.

## [09]-[WHEN_TO_USE]

Use these techniques to:
- eliminate near-duplicate specialized functions;
- expose reusable intermediate configurations;
- produce unary functions that fit higher-order APIs;

Their costs are specific to C#:
- `Func` conversion and occasional explicit type annotations;
- nested delegate types that become difficult to read at higher arities.

Use them when specialized functions simplify call sites. If the helper code is larger or less readable than the duplication it removes, use ordinary functions.
