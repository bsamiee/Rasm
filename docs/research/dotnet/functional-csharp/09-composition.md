<!-- Fully integrated into .claude/skills/dotnet-coding/SKILL.md, references/functions.md, and references/results.md -->
# [COMPOSITION]

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [01]-[PROGRAM_STRUCTURE]

Function composition connects functions by feeding each output into the next input. For unary functions:

```text
g : A -> B
f : B -> C
f · g : A -> C
(f · g)(x) = f(g(x))
```

Types constrain composition: the output of `g` must be assignable to the input of `f`. Nested calls express this but read in reverse execution order:

```csharp
internal sealed record Person(string FirstName, string LastName, decimal Earnings);

internal static class Email {
    public static string AbbreviateName(this Person person) => person.FirstName[..1] + person.LastName;
    public static string AppendDomain(this string name) => name + "@example.com";
    public static string Nested(Person person) => AppendDomain(AbbreviateName(person));
}
```

C# has no dedicated syntax for function composition. The Prelude function `compose` builds the same function as one reusable value, and its arguments read in execution order. `compose` needs explicit type arguments when its arguments are method groups. Instance or extension-method chaining expresses the same flow inline:

```csharp
internal static class Chaining {
    public static readonly Func<Person, string> EmailFor = compose<Person, string, string>(Email.AbbreviateName, Email.AppendDomain);

    public static string Chained(Person person) =>
        person
            .AbbreviateName()
            .AppendDomain();
}
```

Each chained method must be defined on the preceding expression's type, either as an instance method or an extension method. The methods appear in execution order. Long pipelines can describe the program at a high level.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/results.md
## [02]-[COMPOSITION_LAW]

When a value is inside a structure (`Option<T>`), `Map` must preserve ordinary composition: `option.Map(g).Map(f)` must produce the same result as `option.Map(x => f(g(x)))`. This equation is the functor composition law.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
## [03]-[DATA_FLOW]

Compositional programs are sequences of typed transformations. Track what each step does to the value and its enclosing structure:

```csharp
internal static class DataFlow {
    public static decimal AverageEarningsOfRichestQuartile(Seq<Person> population) =>
        population
            .OrderByDescending(static p => p.Earnings) // reorder, preserve elements
            .Take(population.Count / 4)                // reduce cardinality
            .Average(static p => p.Earnings);          // project and collapse to a scalar
}
```

The pipeline states the intended result. Reusable operations contain iteration, branching, and enumeration mechanics. `Average` is a terminal operation that immediately evaluates the preceding lazy sequence.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
### [03.1]-[PIPELINE_INSPECTION]

Nested lambdas in a fluent chain can obscure the first incorrect transformation. Queries with a `from` clause for each stage preserve the flow while exposing each result:

```csharp
internal static class Stages {
    public static Option<int> Inspect(int id) =>
        from initial in Some(id)
        from first in TransformationOne(initial)
        from second in TransformationTwo(first)
        from result in TransformationThree(second)
        select result;

    private static Option<int> TransformationOne(int value) => value > 0 ? Some(value * 2) : Option<int>.None;
    private static Option<int> TransformationTwo(int value) => value < 100 ? Some(value + 1) : Option<int>.None;
    private static Option<int> TransformationThree(int value) => value % 2 == 1 ? Some(value) : Option<int>.None;
}
```

Because each range variable is bound once, every stage remains available for inspection. Large intermediate values remain in scope until the containing function ends. Combine stages if a large value must be released sooner.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
### [03.2]-[COMPOSABLE_FUNCTIONS]

The following properties support function reuse and rearrangement:
- Pure: its result depends only on its arguments, with no side effects
- Chainable: an instance or extension receiver lets its result flow into the next call
- General: it performs one operation that applies to multiple use cases
- Structure-preserving: when possible, it returns the same outer structure it accepts
- Non-`void`: it returns data for the next function

The properties are guidance, not requirements. Terminal operations materialize or reduce a structure, or perform effects. They end a chain instead of continuing it.

Prefer small, general building blocks over one specific aggregate operation:

```csharp
internal static class Quartiles {
    public static Seq<Person> RichestQuartile(this Seq<Person> population) =>
        toSeq(population.OrderByDescending(static p => p.Earnings)).Take(population.Count / 4);
    public static decimal AverageEarnings(this IEnumerable<Person> population) => population.Average(static p => p.Earnings);
    public static decimal Result(Seq<Person> population) =>
        population
            .RichestQuartile()
            .AverageEarnings();
}
```

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
-->

## [04]-[WORKFLOW_OPERATORS]

Workflows are sequences of operations that produce a result. Give each step a function and use the operator that matches the step's type:

| [INDEX] | [SIGNATURE]                     | [OPERATOR]                             | [DESCRIPTION]                                               |
| :-----: | :------------------------------ | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `T -> R`                        | `Map`                                  | Transform a present value while preserving the result type  |
|  [02]   | `T -> bool`                     | `Filter` on `Option`, `guard` on `Fin` | Keep a value only when it passes a condition                |
|  [03]   | `T -> Option<R>`, `T -> Fin<R>` | `Bind`                                 | Continue with a step that can produce no value or a reason  |
|  [04]   | `T -> void`                     | `Iter`                                 | Perform the terminal effect only for a present value        |

Workflow functions need not compose directly. For example, validation returns `bool`, but the debit requires the normalized request. Higher-order operators adapt these function types. `guard` connects the `bool` result to `Fin`.

`Fin<A>` controls continuation: `Succ` keeps a value available to later operations, while `Fail` stops them and preserves the reason. In a LINQ query over `Fin`, `guard` is the validation step and a `from` clause over `Debit` is the domain step.

```csharp
internal sealed record MakeTransfer(Guid DebitedAccountId, string Bic, decimal Amount);
internal sealed record InvalidTransfer() : Expected("transfer is invalid", 901);

internal static class Workflow {
    public static Fin<AccountState> MakeTransfer(MakeTransfer transfer, AccountState account) =>
        from normalized in Pure(Normalize(transfer)).ToFin()
        from _ in guard<Error>(IsValid(normalized), new InvalidTransfer())
        from debited in account.Debit(normalized.Amount)
        select debited;
    public static MakeTransfer Normalize(MakeTransfer transfer) => transfer with { Bic = transfer.Bic.Trim().ToUpperInvariant() };
    public static bool IsValid(MakeTransfer transfer) => transfer.Amount > 0 && transfer.Bic.Length == 8;
}
```

The workflow runs in domain order: normalize, validate, then debit. Adding another transformation means defining one function and inserting one pipeline step. `Fin` handles control flow. The top-level workflow needs no nested conditionals.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
## [05]-[PURE_TRANSITIONS]

Immutable state is separate from behavior.

```csharp
internal sealed record AccountState(decimal Balance);
internal sealed record InsufficientFunds() : Expected("insufficient funds", 902);

internal static class Account {
    public static Fin<AccountState> Debit(this AccountState account, decimal amount) =>
        account.Balance < amount
            ? new InsufficientFunds()
            : new AccountState(account.Balance - amount);
}
```

`Debit`:
- Does not throw for an expected business outcome
- Exposes possible failure in its return type
- Produces a value that later steps can consume
- Leaves the original state unchanged
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
## [06]-[END_TO_END_FLOW]

Boundary services expose reads as `OptionT<IO, A>` and writes as `IO<Unit>` while the domain transition stays pure. The repository lifts its `Option` read into the transformer with `OptionT.lift`. `Require` converts the transformer to `IO<AccountState>`. `Run` unwraps the `OptionT` layer. `IO.lift` lifts the `Fin` returned by `ToFin` into `IO`, preserving `AccountNotFound` as the typed error.

```csharp
internal sealed record AccountNotFound() : Expected("account not found", 903);

internal interface IRepository<T> {
    public OptionT<IO, T> Get(Guid id);
    public IO<Unit> Save(Guid id, T value);
}
internal interface ISwiftService {
    public IO<Unit> Wire(MakeTransfer transfer, AccountState account);
}

internal sealed class MemoryAccounts : IRepository<AccountState> {
    private readonly AtomHashMap<Guid, AccountState> store = AtomHashMap<Guid, AccountState>();

    public OptionT<IO, AccountState> Get(Guid id) => OptionT.lift<IO, AccountState>(IO.lift(() => store.Find(id)));
    public IO<Unit> Save(Guid id, AccountState value) => IO.lift(() => store.AddOrUpdate(id, value));
}
internal sealed class Transfers(IRepository<AccountState> accounts, ISwiftService swift) {
    public IO<Unit> Book(MakeTransfer transfer) =>
        from account in Require(accounts.Get(transfer.DebitedAccountId))
        from debited in IO.lift(Workflow.MakeTransfer(transfer, account))
        from _ in accounts.Save(transfer.DebitedAccountId, debited)
        from __ in swift.Wire(transfer, debited)
        select unit;

    private static IO<AccountState> Require(OptionT<IO, AccountState> lookup) =>
        lookup.Run().As().Bind(static option => IO.lift(option.ToFin(new AccountNotFound())));
}
```

`Get` can find no account, and `MakeTransfer` can reject the transfer. `Book` binds them on one `IO` error channel instead of nesting result types. `Save` and `Wire` run only when both succeed, and each effect is one visible step of the query. The host runs `Book` with `RunSafe`, which returns `Fin<Unit>` and carries the typed error out of the effect.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
## [07]-[DECLARATIVE_CODE]

Expressions produce values and compose. Assignments, loops, and conditional statements direct execution and do not produce values for a pipeline. Class, method, and field declarations remain necessary but form a separate category. Using expressions shifts code from imperative instructions toward declarative descriptions.

Using expressions does not eliminate effects. It moves effects to explicit boundaries:
1. Receive external input
2. Transform and validate through expressions
3. Compute new domain state with pure functions
4. Perform required persistence or communication at the effect boundary

If a terminal step requires multiple effects, keep each one visible.

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
-->

## [08]-[LAYERING]

Do not require every layer to call only its immediate neighbor. After a low-level call performs I/O, every delegating layer becomes impure.

Let a top-level entry point compose functions from lower-level components while dependencies point downward. This structure provides:
- One overview of the business workflow
- Subworkflows for related groups of steps
- Pure mid-level validation and domain logic
- Direct testing of pure logic without mocks
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/functions.md
## [09]-[LIMITS]

- `Option` discards the reason for failure, and `Fin` keeps it. `Option` can short-circuit the flow, but it cannot distinguish a missing account from insufficient funds. `Fin` carries the typed `Error` without changing the compositional approach.
- Composition does not make distributed effects atomic. Saving a debited account and wiring funds can fail between operations. Database transactions cannot protect an external call from process failure after the call but before commit.
- One multi-system pattern uses a persisted work item and idempotency. Persist a representation of the combined work atomically, process it until all effects complete, and make repeat execution safe.
- Confidence comes from tests. Do not inspect the implementation and assume that its abstracted operations are correct.
-->
