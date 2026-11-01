# [RESULTS]

Covers the result-type flows, from validators to the equations results satisfy and their property tests.

## [01]-[VALIDATION]

Validators accept the request, return it on success for the next validator, and return the error for the violated rule. Rules reading the clock receive the date as an argument. `Fault.Unless` raises the package error record:

```csharp
internal sealed record Command(string Code, LocalDate Date);

internal static class Rules {
    public static Fin<Command> ValidCode(Command command) =>
        Fault.Unless(command.Code.Length is 8 or 11 && command.Code.All(char.IsLetterOrDigit), Reason.Invalid, command.Code).Map(_ => command);
    public static Fin<Command> ValidDate(Command command, LocalDate today) =>
        Fault.Unless(command.Date > today, Reason.Invalid, command.Date).Map(_ => command);
    public static Fin<Command> Valid(Command command, LocalDate today) =>
        (ValidCode(command), ValidDate(command, today)).Apply(static (_, valid) => valid).As();
}
```

Rules reading only the command run together under tuple `Apply`, and its failure holds each violated rule (`Error.Count` counts them). Value object fields join the same tuple through `X.Validated`. `Apply` takes 2 to 10 operands with one uncurried function. Checks reading an earlier result bind, the first failure stops later checks.

Collections of validators of shape `T -> Fin<T>` fold into one validator, and the traversal selects the behavior:

```csharp
internal static class Validators {
    public static Func<T, Fin<T>> FailFast<T>(Seq<Func<T, Fin<T>>> rules) =>
        value => rules.TraverseM(rule => rule(value)).As().Map(_ => value);
    public static Func<T, Fin<T>> Harvest<T>(Seq<Func<T, Fin<T>>> rules) =>
        value => rules.Traverse(rule => rule(value)).As().Map(_ => value);
}
```

`FailFast` skips the remaining rules after the first invalid result, cheap structural checks go before expensive database or remote checks, and `Harvest` evaluates every rule and accumulates every error for a caller that repairs all violations at once. Empty rule lists return the input as valid, on success the traversal holds one copy of the input per rule, and `Map` discards those copies.

## [02]-[WORKFLOWS]

`Fin<A>` applies a function only to `Succ`, and `Fail` bypasses it and keeps its error. `Map` transforms the value with `A -> B`, `Bind` composes a step with `A -> Fin<B>`. `Where` on `Fin` fails with `Errors.None`, a predicate supplies `bool` and no `Error`, a check is a validator that constructs its error and composes with `Bind`, or a `guard` clause in a query:

```csharp
internal static class Roots {
    public static Fin<double> Ratio(double x, double y) =>
        y == 0 ? new Fault("divisor", Reason.Invalid)
        : x != 0 && Math.Sign(x) != Math.Sign(y) ? new Fault("ratio", Reason.Invalid)
        : Math.Sqrt(x / y);
    public static Fin<string> Rendered(double x, double y) =>
        Ratio(x, y).Map(static root => root.ToString(CultureInfo.InvariantCulture));
    public static Fin<double> Nested(double x, double y) =>
        Ratio(x, y).Bind(static root => Ratio(root, 1));
    public static Fin<double> Root(double value) =>
        from _ in guard<Error>(value >= 0, new Fault("value", Reason.Invalid)).ToFin()
        select Math.Sqrt(value);
}
```

Both `double` and `Error` convert to `Fin<double>` without a constructor call, and the ternary chain selects the lift by its return type. Fail-fast workflows bind steps that share the failure type `Error`, each `Succ` passes its value to the next step, the first `Fail` skips every later step and reaches the final handler, and `Unit` marks a success with no value:

```csharp
internal sealed record Request(string Key, decimal Amount);
internal sealed record ValidRequest(string Key, decimal Amount);
internal sealed record Model(string Key, decimal Balance, decimal Amount);
internal sealed record Updated(string Key, decimal Balance);

internal static class Pipeline {
    public static Fin<ValidRequest> Validate(Request request) =>
        Fault.Unless(request.Amount > 0, Reason.Invalid, request.Amount).Map(_ => new ValidRequest(request.Key, request.Amount));
    public static Fin<Model> Load(Func<string, Option<(string Key, decimal Balance)>> find, ValidRequest request) =>
        find(request.Key).ToFin(new Fault(request.Key, Reason.Missing)).Map(row => new Model(row.Key, row.Balance, request.Amount));
    public static Fin<Updated> Update(Model model) =>
        Fault.Unless(model.Balance >= model.Amount, Reason.Invalid, (model.Balance, model.Amount)).Map(_ => new Updated(model.Key, model.Balance - model.Amount));
    public static Fin<Unit> Save(Updated model) =>
        Fault.Unless(model.Balance <= Limits.Reporting, Reason.Invalid, model.Balance);
    public static Fin<Unit> Handle(Func<string, Option<(string Key, decimal Balance)>> find, Request request) =>
        Validate(request)
            .Bind(valid => Load(find, valid))
            .Bind(Update)
            .Bind(Save);
}
```

Every bound function returns the same failure type, choose the workflow's errors before composing it. Lifting a multi-argument function first with `Some`, mapping it over the first operand, and the tuple `Apply` produce the same result for a correct applicative, `Some` only when every input is `Some`, and lifting first follows partial application:

```csharp
internal static class Lifting {
    private static readonly Func<int, int, int> Multiply = static (x, y) => x * y;

    public static Option<int> Mapped(Option<int> left, Option<int> right) => Multiply.Map(left).Apply(right);
    public static Option<int> Lifted(Option<int> left, Option<int> right) => Some(Multiply).Apply(left).Apply(right);
    public static Option<int> Tupled(Option<int> left, Option<int> right) => (left, right).Apply(Multiply).As();
}
```

Chained `Apply` on `Option` returns `Option<int>`, and `As()` returns the concrete type from the `K<Option, int>` the tuple `Apply` returns.

C# translates query clauses into method calls by name and signature, and an effect needs no `IEnumerable<T>` to take part: one `from` with `select` calls `Select`, an alias of `Map`, and every further `from` calls the ternary `SelectMany` that passes earlier values into the final projection without nested lambdas, one query shape runs over `Option` and over `Fin<A>`:

```csharp
internal static class Queries {
    public static Fin<int> ValidInt(string text) => parseInt(text).ToFin(Errors.ParseError("not a number"));
    public static Option<int> Total(string first, string second) =>
        from a in parseInt(first)
        from b in parseInt(second)
        select a + b;
    public static Fin<int> Sum(string first, string second) =>
        from a in ValidInt(first)
        from b in ValidInt(second)
        select a + b;
}
```

`let` translates to `Select` and works once mapping exists, `where` needs `Where`, which `Option` supplies beside `Filter`, and collection clauses (`orderby`) need not exist for `Option`, `Either`, or `Validation`. Right-associated `Bind` lets the innermost function close over every earlier value, and the query expresses that without nesting.

## [03]-[HOST_TRANSLATION]

Validation returns `Fin`, `IO.lift(Fin<A>)` places a rejection on the `IO` error channel, one query binds both, and the effect runs only for a valid command. Exception-throwing dependencies convert at their integration boundary, and `IO.lift` captures the exception from only that call as an `Exceptional` error:

```csharp
internal static class Handler {
    public static IO<Unit> Handle(Command command, LocalDate today, Action<Command> insert) =>
        from valid in IO.lift(Rules.Valid(command, today))
        from _ in IO.lift(() => insert(valid))
        select unit;
}
```

Tuple `Apply` reports both violations together, `RunSafe` at the outer boundary returns one `Fin<Unit>` for the host `Match`, and the host logs the `Inner` of a translated dependency failure beside the business errors it renders. Within the core the workflow composes with `Map` and `Bind`, and only an outer adapter translates when the protocol, UI, or host requires another response type:

```csharp
IActionResult Post(Request request) =>
    Pipeline.Handle(find, request).Match<IActionResult>(
        Succ: static _ => Ok(),
        Fail: static error => BadRequest(error));
```

For an optional lookup the boundary translates `None` to not found and `Some(value)` to a successful response, and for `Fin` the boundary maps `Fail` and `Succ` to protocol status codes and payloads.

## [04]-[UNIONS]

Discriminated unions hold exactly one of their alternatives, consumers pattern-match the value to reach its case and that case's data, and components that do not care which case they hold pass the union unchanged. Cases can be unrelated alternatives that share only an API type or a collection. Lookup outcomes are found, absent, and failed, `IO<Option<Item>>` names each: `Some` is the found item, `None` is absence, and a lookup failure sits on the `IO` error channel:

```csharp
internal sealed record Item(int Id, string Name);

internal static class Lookups {
    public static IO<Option<Item>> Find(Func<int, Item?> store, int id) =>
        IO.lift(() => Optional(store(id)));
    public static IO<string> Describe(IO<Option<Item>> lookup) =>
        lookup.Map(static found => found.Match(Some: static item => item.Name, None: static () => "no such item"));
    public static IO<Unit> Notify(Action<string> transport, string address) => IO.lift(() => transport(address));
}
```

`Optional` maps the `null` of a missing row to `None`, `IO.lift` captures a thrown lookup failure as an `Exceptional` error, `Describe` is the one consumer and matches the inner `Option` once inside `Map`, and the host receives the lookup failure in the `Fin` that `RunSafe()` returns, no caller infers the outcome from `null`, a status flag, or optional metadata. `Notify` returns `IO<Unit>`, where `Unit` is completion and the error channel holds transport failures.

External input refines into typed cases in stages: the read comes in as a `Func<string>` dependency that another implementation can replace, `IO.lift` captures a read failure on the error channel, the text classifies once, and application code consumes the classified case:

```csharp
[Union]
internal abstract partial record Input {
    internal sealed record Text(string Value) : Input;
    internal sealed record Empty : Input;
    internal sealed record Number(int Value) : Input;
}

internal static class Inputs {
    public static IO<Input> Read(Func<string> read) => IO.lift(() => Classify(read()));
    public static Input Classify(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? new Input.Empty()
            : parseInt(text).Match<Input>(Some: static value => new Input.Number(value), None: () => new Input.Text(text));
}
```

Code that needs a number handles `Number` directly and prompts again for the other cases, parsing and exception handling appear at no other call site, and the side-effecting read stays separate from the deterministic classification.

Union cases can hold the union type itself, and a union of that kind models a tree (configuration, expressions, UI hierarchies, document fragments), while wire serialization stays with `System.Text.Json` at the host boundary:

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

Scalar cases are the leaves and `Many` and `Keyed` are the containers, the recursive members are ordinary case properties that leave the generator's case discovery unchanged, and a fold takes one replacement per constructor: each scalar replacement receives that case's value, `Nil` receives nothing, and the container replacements receive already-folded child results. Recursion sits in `Fold` once, the handler record passes as the `Switch` state, every arm stays `static`, `Count` replaces every leaf with 1 and adds the node to its children, and `Depth` replaces each container with one more than its deepest child. Remaining operations follow the return-type rules: member lookup passes the requested key as the state and returns `Option<Node>`, a `Keyed` without the key and every other case answer `None`, typed extraction returns `Fin<A>` with an error naming the wanted and the found case, and an operation that preserves a case takes the case type directly when the caller already holds one, which removes the wrong-shape error from its signature.

## [05]-[EQUATIONS]

Dedicated `Apply` stays more efficient than one derived from `Bind` and keeps the semantics (accumulation) that a short-circuiting `Bind` cannot give. Implementations satisfy these equations for every value, including `None` and the failure case, and an implementation that hides mutation, counters, or state tied to the number of calls breaks safe refactoring:

```text
Functor identity:    fa.Map(x => x) == fa
Functor composition: fa.Map(g).Map(f) == fa.Map(x => f(g(x)))
Applicative:         a.Map(f).Apply(b) == Pure(f).Apply(a).Apply(b), with identity, composition, homomorphism, interchange
Monad right identity: m.Bind(Pure) == m
Monad left identity:  Pure(t).Bind(f) == f(t)
Monad associativity:  m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))
```

Both identities require `Pure` and `Bind` to wrap and unwrap without adding state changes, conditional behavior, or distortion. Associativity lets a multi-argument function enter a monadic pipeline, the right-associated form lets the innermost function close over every earlier value and a query expresses that without nested `Bind` calls. `FunctorLaw<F>`, `ApplicativeLaw<F>`, and `MonadLaw<F>` run the checks.

Property-based tests check these equations and invariants over generated inputs (removing items from a cart never increases its total), random sampling raises confidence without proving an equation for every input:

```csharp
internal static class Properties {
    private static readonly Func<int, int, int> Multiply = static (x, y) => x * y;
    private static readonly Option<Func<int, int, int>> Lifted = Pure(Multiply);
    private static readonly Gen<Option<int>> Operand =
        Gen.OneOf(Gen.Int[-1000, 1000].Select(static x => Some(x)), Gen.Const(Option<int>.None));

    public static void Equivalence() =>
        Operand.Select(Operand).Sample(static (a, b) => Multiply.Map(a).Apply(b) == Lifted.Apply(a).Apply(b));
}
```

`Gen.OneOf` builds a generator that yields both `Some` and `None`, a generator of integers alone checks only the `Some` path. `Gen.Int[-1000, 1000]` keeps the product inside `int`, `Sample` throws on a counterexample, the case count and the ranges are configurable, and a property tied to `Multiply` checks that function alone.
