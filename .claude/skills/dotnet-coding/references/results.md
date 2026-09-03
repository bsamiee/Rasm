# [RESULTS]

Worked flows for the result types: validators and their combination, `Fin` workflows and application inside an effect, translation at the host, domain unions with their folds, and the laws with their property tests. Which type a function returns and which operator joins two steps are decisions in `dotnet-coding`, and how each operation and recovery overload behaves is in `dotnet-languageext`.

## [01]-[VALIDATION]

Each validator has one shape: it accepts the request, returns it on success so the next validator receives it, and returns the error for the violated rule, and a rule that reads the clock receives it as an argument:

```csharp
internal sealed record Command(string Code, DateOnly Date);
internal sealed record InvalidCode() : Expected("code is not 8 or 11 alphanumerics", Codes.InvalidCode);
internal sealed record DateIsPast() : Expected("date is not in the future", Codes.DateIsPast);

internal static class Rules {
    public static Fin<Command> ValidCode(Command command) =>
        command.Code.Length is 8 or 11 && command.Code.All(char.IsLetterOrDigit) ? command : new InvalidCode();
    public static Fin<Command> ValidDate(Command command, DateOnly today) =>
        command.Date > today ? command : new DateIsPast();
    public static Fin<Command> Valid(Command command, DateOnly today) =>
        ValidCode(command).Bind(c => ValidDate(c, today));
}
```

`Bind` makes the rules dependent, and the date rule runs only after the code rule passes. Independent fields validate through the generated `Validate` of a smart enum or value object, each mapped to `Validation<Error, T>`, and the tuple `Apply` builds the aggregate from every result so every failed field reports:

```csharp
internal sealed record Contact(Kind Kind, Region Region, Number Number);

internal static class Contacts {
    public static Validation<Error, Kind> ValidKind(string kind) =>
        Kind.Validate(kind, provider: null, out Kind? item) is { } error ? error : item!;
    public static Validation<Error, Region> ValidRegion(string region) =>
        Region.Validate(region, provider: null, out Region item) is { } error ? error : item;
    public static Validation<Error, Number> ValidNumber(string number) =>
        Number.Validate(number, provider: null, out Number item) is { } error ? error : item;
    public static Validation<Error, Contact> Create(string kind, string region, string number) =>
        (ValidKind(kind), ValidRegion(region), ValidNumber(number))
            .Apply(static (k, r, n) => new Contact(k, r, n))
            .As();
    public static int Faults(Validation<Error, Contact> result) =>
        result.Match(Fail: static errors => errors.Count, Succ: static _ => 0);
}
```

`Kind` is a smart enum, so its `Validate` yields a nullable reference and `item!` follows the null check, and `Region` and `Number` are value objects with a struct `out` value. Three invalid inputs report 3 errors, the tuple `Apply` takes 2 to 10 independent operands with one uncurried function, and the input boundary returns the `Validation<Error, Contact>`. When a later check consumes an earlier validated value or must not run after a failure, a query binds the steps and the first failure stops the rest, at the cost of the failures from checks that never ran.

Collections of validators of shape `T -> Validation<Error, T>` fold into one validator, and the traversal selects the behavior:

```csharp
internal static class Validators {
    public static Func<T, Validation<Error, T>> FailFast<T>(Seq<Func<T, Validation<Error, T>>> rules) =>
        value => rules.TraverseM(rule => rule(value)).As().Map(_ => value);
    public static Func<T, Validation<Error, T>> Harvest<T>(Seq<Func<T, Validation<Error, T>>> rules) =>
        value => rules.Traverse(rule => rule(value)).As().Map(_ => value);
}
```

`FailFast` skips the remaining rules after the first invalid result, so cheap structural checks go before expensive database or remote checks, and `Harvest` evaluates every rule and accumulates every error for a caller that repairs all violations at once. An empty rule list returns the input as valid, on success the traversal holds one copy of the input per rule, and `Map` discards those copies.

## [02]-[WORKFLOWS]

`Fin<A>` applies a function only to `Succ`, and `Fail` bypasses it and keeps its error. `Map` transforms the value with `A -> B`, `Bind` composes a step with `A -> Fin<B>`, and `Fin` has no `Where`, because a predicate supplies only `bool` and no `Error`, so a check is a validator that constructs its error and composes with `Bind`, or a `guard` clause in a query:

```csharp
internal sealed record ZeroDivisor() : Expected("divisor is 0", Codes.ZeroDivisor);
internal sealed record NegativeRatio() : Expected("ratio is negative", Codes.NegativeRatio);
internal sealed record NegativeValue() : Expected("value is negative", Codes.NegativeValue);

internal static class Roots {
    public static Fin<double> Ratio(double x, double y) =>
        y == 0 ? new ZeroDivisor()
        : x != 0 && Math.Sign(x) != Math.Sign(y) ? new NegativeRatio()
        : Math.Sqrt(x / y);
    public static Fin<string> Rendered(double x, double y) =>
        Ratio(x, y).Map(static root => root.ToString(CultureInfo.InvariantCulture));
    public static Fin<double> Nested(double x, double y) =>
        Ratio(x, y).Bind(static root => Ratio(root, 1));
    public static Fin<double> Root(double value) =>
        from v in Pure(value).ToFin()
        from _ in guard<Error>(v >= 0, new NegativeValue())
        select Math.Sqrt(v);
}
```

Both `double` and `Error` convert to `Fin<double>` without a constructor call, and the ternary chain selects the lift by its return type. A fail-fast workflow binds steps that share the failure type `Error`, each `Succ` passes its value to the next step, the first `Fail` skips every later step and reaches the final handler, and `Unit` marks a success with no payload:

```csharp
internal sealed record Request(string Key, decimal Amount);
internal sealed record ValidRequest(string Key, decimal Amount);
internal sealed record Model(string Key, decimal Balance, decimal Amount);
internal sealed record Updated(string Key, decimal Balance);

internal static class Pipeline {
    public static Fin<ValidRequest> Validate(Request request) =>
        request.Amount > 0 ? new ValidRequest(request.Key, request.Amount) : new NotPositive();
    public static Fin<Model> Load(Func<string, Option<(string Key, decimal Balance)>> find, ValidRequest request) =>
        find(request.Key).ToFin(new NotFound()).Map(row => new Model(row.Key, row.Balance, request.Amount));
    public static Fin<Updated> Update(Model model) =>
        model.Balance >= model.Amount ? new Updated(model.Key, model.Balance - model.Amount) : new Insufficient();
    public static Fin<Unit> Save(Updated model) =>
        model.Balance <= Limits.Reporting ? unit : new OverLimit();
    public static Fin<Unit> Handle(Func<string, Option<(string Key, decimal Balance)>> find, Request request) =>
        Validate(request)
            .Bind(valid => Load(find, valid))
            .Bind(Update)
            .Bind(Save);
}
```

Choose the domain errors for the workflow before composing it, because every bound function must return the same failure type. A multi-argument function enters an effect by currying: `Map` supplies the first argument and leaves a unary function inside the effect, and `Apply` supplies each remaining argument from inside the same effect, `Some` only when every input is `Some`. Lifting the function first with `Pure`, mapping it over the first operand, and the tuple `Apply` produce the same result for a correct applicative, and lifting first mirrors partial application:

```csharp
internal static class Lifting {
    private static readonly Func<int, int, int> Multiply = static (x, y) => x * y;

    public static Option<int> Mapped(Option<int> left, Option<int> right) => Multiply.Map(left).Apply(right).As();
    public static Option<int> Lifted(Option<int> left, Option<int> right) => Pure(Multiply).Apply(left).Apply(right).As();
    public static Option<int> Tupled(Option<int> left, Option<int> right) => (left, right).Apply(Multiply).As();
}
```

The multi-argument `Map` and `Apply` overloads curry the delegate, `fun` gives an inline lambda the delegate type these overloads need, and `As()` returns the concrete type from the `K<Option, int>` the trait method returns. An `Apply` derived from `Bind` discards accumulation, so an effect with combination semantics supplies its own `Apply`.

C# translates query clauses into method calls by name and signature, and an effect needs no `IEnumerable<T>` to take part: one `from` with `select` calls `Select`, an alias of `Map`, and every further `from` calls the ternary `SelectMany` that carries earlier values into the final projection without nested lambdas, so one query shape runs over `Option` and over `Validation<Error, A>`:

```csharp
internal sealed record NotANumber() : Expected("not a number", Codes.NotANumber);

internal static class Queries {
    public static Validation<Error, int> ValidInt(string text) => parseInt(text).ToValidation<Error>(new NotANumber());
    public static Option<int> Total(string first, string second) =>
        from a in parseInt(first)
        from b in parseInt(second)
        select a + b;
    public static Validation<Error, int> Sum(string first, string second) =>
        from a in ValidInt(first)
        from b in ValidInt(second)
        select a + b;
}
```

`let` translates to `Select` and works once mapping exists, `where` needs `Where`, which `Option` supplies beside `Filter`, and collection clauses (`orderby`) need not exist for `Option`, `Either`, or `Validation`. The right-associated `Bind` lets the innermost function close over every earlier value, and the query expresses that without nesting.

## [03]-[HOST_TRANSLATION]

Validation and persistence express different effects, accumulation and a deferred side effect, so one `Bind` cannot flatten them: validation exits through `ToFin`, `IO.lift(Fin<A>)` places that result on the `IO` error channel, one query binds both, and the effect runs only for a valid command. An exception-throwing dependency converts at its integration boundary, where `IO.lift` captures the exception from only that call as an `Exceptional` error:

```csharp
internal static class Handler {
    public static Validation<Error, Command> Validated(Command command, DateOnly today) =>
        (Rules.ValidCode(command).ToValidation(), Rules.ValidDate(command, today).ToValidation())
            .Apply(static (_, valid) => valid)
            .As();
    public static IO<Unit> Handle(Command command, DateOnly today, Action<Command> insert) =>
        from valid in IO.lift(Validated(command, today).ToFin())
        from _ in IO.lift(() => insert(valid))
        select unit;
}
```

The tuple `Apply` reports both violations together, and `RunSafe` at the outer boundary returns one `Fin<Unit>`, and its `Match` separates the reachable outcomes:
- `Fail` with an `Expected` error or `ManyErrors` exposes the business errors and logs the `Inner` of a translated dependency failure, and the host reads the accumulated errors with `Filter<E>`, `Count`, and `Head`
- `Fail` with an `Exceptional` error logs the technical detail and exposes a generic failure
- `Succ(unit)` returns success

Within the core the workflow composes with `Map` and `Bind`, and only an outer adapter translates when the protocol, UI, or host requires another response type. Every library returns its result type with its own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation:

```csharp
IActionResult Post(Request request) =>
    Pipeline.Handle(find, request).Match<IActionResult>(
        Succ: static _ => Ok(),
        Fail: static error => BadRequest(error));
```

For an optional lookup the boundary translates `None` to not found and `Some(value)` to a successful response, and for `Fin` the boundary decides how domain failures map to the external contract. Two API designs exist: map `Fail` and `Succ` to protocol status codes and payloads, or always return a successful transport status with a body that is a result DTO holding `Succeeded` and either `Data` or `Error`, which unlike `Fin` exposes its values directly for serialization and client access. Mapping a business validation to an HTTP error (400) has a tradeoff, the request can be syntactically valid yet violate a business rule, and concurrent changes can invalidate it between creation and receipt, so the choice is an API-design decision.

## [04]-[UNIONS]

A discriminated union holds exactly one of several alternatives, consumers pattern-match the value to reach its case and that case's data, and components that do not care which case they hold pass the union unchanged. Cases can be unrelated alternatives that share only an API type or a collection. A lookup has 3 meaningful outcomes, found, absent, and failed, and `OptionT<IO, Item>` names each: `Some` is the found item, `None` is absence, and a lookup failure sits on the `IO` error channel:

```csharp
internal sealed record Item(int Id, string Name);

internal static class Lookups {
    public static OptionT<IO, Item> Find(Func<int, Item?> store, int id) =>
        OptionT.lift<IO, Item>(IO.lift(() => Optional(store(id))));
    public static IO<string> Describe(OptionT<IO, Item> lookup) =>
        lookup.Match(Some: static item => item.Name, None: static () => "no such item").As();
    public static IO<Unit> Notify(Action<string> transport, string address) => IO.lift(() => transport(address));
}
```

`Optional` maps the `null` of a missing row to `None`, `IO.lift` captures a thrown lookup failure as an `Exceptional` error, `OptionT.Match` returns `K<IO, B>` and `.As()` restores `IO<B>`, and the host receives the lookup failure in the `Fin` that `RunSafe()` returns, so no caller infers the outcome from `null`, a status flag, or optional metadata. `Notify` returns `IO<Unit>`, where `Unit` is completion and the error channel holds transport failures.

External input refines into typed cases in stages: the read comes in as a `Func<string>` dependency that another implementation can replace, `IO.lift` captures a read failure on the error channel, the text classifies once, and application code consumes the classified case:

```csharp
[Union]
internal abstract partial record Input {
    internal sealed record Text(string Value) : Input;
    internal sealed record Empty : Input;
    internal sealed record Number(int Value) : Input;
    internal sealed record ReadFailure(Error Error) : Input;
}

internal static class Inputs {
    public static IO<Input> Read(Func<string> read) =>
        IO.lift(() => Classify(read()))
            .Catch(static error => error.IsExceptional, static error => IO.pure<Input>(new Input.ReadFailure(error)));
    public static Input Classify(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? new Input.Empty()
            : parseInt(text).Match<Input>(Some: static value => new Input.Number(value), None: () => new Input.Text(text));
}
```

The `Catch` overload with a predicate maps the captured error to the `ReadFailure` case at the boundary, so a read failure is a case the consumer matches, code that needs a number handles `Number` directly and prompts again for the other cases, parsing and exception handling appear at no other call site, and the side-effecting read stays separate from the deterministic classification.

Union cases can carry the union type itself, and such a union models a tree the domain owns (configuration, expressions, UI hierarchies, document fragments), where wire serialization stays with `System.Text.Json` at the host boundary:

```csharp
[Union]
internal abstract partial record Node {
    internal sealed record Text(string Value) : Node;
    internal sealed record Number(decimal Value) : Node;
    internal sealed record Flag(bool Value) : Node;
    internal sealed record Nil : Node;
    internal sealed record Many(Seq<Node> Items) : Node;
    internal sealed record Keyed(Map<string, Node> Members) : Node;
}

internal sealed record NodeFold<R>(
    Func<string, R> Text,
    Func<decimal, R> Number,
    Func<bool, R> Flag,
    Func<R> Nil,
    Func<Seq<R>, R> Many,
    Func<Map<string, R>, R> Keyed);

internal static class Folds {
    public static R Fold<R>(Node node, NodeFold<R> fold) =>
        node.Switch(
            fold,
            text: static (f, x) => f.Text(x.Value),
            number: static (f, x) => f.Number(x.Value),
            flag: static (f, x) => f.Flag(x.Value),
            nil: static (f, _) => f.Nil(),
            many: static (f, x) => f.Many(x.Items.Map(child => Fold(child, f))),
            keyed: static (f, x) => f.Keyed(x.Members.Map(child => Fold(child, f))));
    public static int Count(Node node) =>
        Fold(node, new NodeFold<int>(
            Text: static _ => 1, Number: static _ => 1, Flag: static _ => 1, Nil: static () => 1,
            Many: static counts => 1 + counts.Fold(0, static (sum, child) => sum + child),
            Keyed: static counts => 1 + counts.Fold(0, static (sum, child) => sum + child)));
    public static int Depth(Node node) =>
        Fold(node, new NodeFold<int>(
            Text: static _ => 1, Number: static _ => 1, Flag: static _ => 1, Nil: static () => 1,
            Many: static depths => 1 + depths.Fold(0, Math.Max),
            Keyed: static depths => 1 + depths.Fold(0, Math.Max)));
}
```

The scalar cases are the leaves and `Many` and `Keyed` are the containers, the recursive payloads are ordinary case properties that leave the generator's case discovery unchanged, and the union has 6 constructors, so a fold takes one replacement per constructor: each scalar replacement receives that case's payload, `Nil` receives nothing, and the container replacements receive already-folded child results. The recursion sits in `Fold` once, the handler record travels as the `Switch` state so every arm stays `static`, `Count` replaces every leaf with 1 and adds the node to its children, and `Depth` replaces each container with one more than its deepest child. The remaining operations follow the return-type rules: member lookup passes the requested key as the state and returns `Option<Node>`, where a `Keyed` without the key and every other case answer `None`, typed extraction returns `Fin<A>` with a distinct `Expected` per wrong shape so a consumer classifies by code, and an operation that preserves a case takes the case type directly when the caller already holds one, which removes the wrong-shape error from its signature.

## [05]-[LAWS]

The abstractions form a hierarchy, `Functor < Applicative < Monad < Fold`, and each trait captures its operations over `K<F, A>`:

| [INDEX] | [ABSTRACTION]    | [OPERATIONS]    | [CAPABILITY]                                                               |
| :-----: | :--------------- | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `Functor<F>`     | `Map`           | Transforms a value without leaving its effect                              |
|  [02]   | `Applicative<F>` | `Pure`, `Apply` | Combines independent values inside an effect with a multi-argument function |
|  [03]   | `Monad<M>`       | `Pure`, `Bind`  | Sequences computations where the next step depends on a prior value        |

The stronger abstractions define the weaker operations, `Map(fa, f)` as `Pure(f).Apply(fa)`, `Apply` as a `Bind` of the argument and then the function, `Fold` defining `Bind`, and LINQ query syntax comes from `Monad<M>`, while a dedicated `Apply` stays more efficient and keeps the semantics (accumulation) that a short-circuiting `Bind` cannot give. The laws are equations an implementation must satisfy for every value, including `None` and the failure case, and an implementation that hides mutation, counters, or state tied to the number of calls breaks safe refactoring:

```text
Functor identity:    fa.Map(x => x) == fa
Functor composition: fa.Map(g).Map(f) == fa.Map(x => f(g(x)))
Applicative:         a.Map(f).Apply(b) == Pure(f).Apply(a).Apply(b), with identity, composition, homomorphism, interchange
Monad right identity: m.Bind(Pure) == m
Monad left identity:  Pure(t).Bind(f) == f(t)
Monad associativity:  m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))
```

The identity laws require `Pure` and `Bind` to wrap and unwrap without adding state changes, conditional behavior, or distortion, and associativity is why a multi-argument function enters a monadic pipeline: the right-associated form lets the innermost function close over every earlier value, and a query expresses that without nested `Bind` calls. `FunctorLaw<F>`, `ApplicativeLaw<F>`, and `MonadLaw<F>` run these checks, and their API sits in `dotnet-languageext`.

Property-based tests state invariants over generated inputs and check algebraic laws and domain invariants (removing items from a cart never increases its total), where random sampling raises confidence without proving a universal law:

```csharp
internal static class Properties {
    private static readonly Func<int, int, int> Multiply = static (x, y) => x * y;
    private static readonly Option<Func<int, int, int>> Lifted = Pure(Multiply);
    private static readonly Gen<Option<int>> Operand =
        Gen.OneOf(Gen.Int[-1000, 1000].Select(static x => Some(x)), Gen.Const(Option<int>.None));

    public static Fin<Unit> Equivalence() =>
        Try.lift(() => {
            Operand.Select(Operand).Sample((a, b) => Multiply.Map(a).Apply(b).As() == Lifted.Apply(a).Apply(b).As());
            return unit;
        }).Run();
}
```

`Gen.OneOf` builds a generator that yields both `Some` and `None`, because a test that lifts only generated integers checks only the `Some` path, the bounded range keeps the product inside `int`, `Sample` throws on a counterexample and `Try.lift` captures it into `Fin`, the case count and the ranges are configurable, and a property tied to `Multiply` checks that function and not every function.
