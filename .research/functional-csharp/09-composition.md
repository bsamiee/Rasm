# Designing Programs with Function Composition

## Composition as the Program Structure

Function composition connects functions by feeding each output into the next input. For unary functions:

```text
g : A -> B
f : B -> C
f · g : A -> C
(f · g)(x) = f(g(x))
```

The types are the governing constraint: the output of `g` must be assignable to the input of `f`. Nested calls express this directly but read in reverse execution order:

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

Each chained method must be defined on the preceding expression's type, either as an instance method or an extension method. The methods then appear in execution order, so a long pipeline can serve as a high-level description of the program.

## Composition Must Survive Elevated Values

When a value is inside a structure such as `Option<T>`, `Map` must preserve ordinary composition: `option.Map(g).Map(f)` must produce the same result as `option.Map(x => f(g(x)))`. This equation is the functor composition law.

## Think in Data Flow

A compositional program is a sequence of typed transformations. Track what each step does to both the value and its shape:

```csharp
internal static class DataFlow {
    public static decimal AverageEarningsOfRichestQuartile(Seq<Person> population) =>
        population
            .OrderByDescending(static p => p.Earnings) // reorder, preserve elements
            .Take(population.Count / 4)                // reduce cardinality
            .Average(static p => p.Earnings);          // project and collapse to a scalar
}
```

The pipeline states what is wanted. Iteration, branching, and enumeration mechanics remain inside reusable operations. Here, `Average` is also the greedy step that evaluates the preceding lazy sequence.

### Keep a Pipeline Inspectable

A fluent chain is concise, but stepping through nested lambdas can obscure the first bad transition. A query whose `from` clauses name each stage preserves the same flow while exposing each result:

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

Because each range variable is bound once, every stage remains available for inspection. The tradeoff is lifetime: large intermediates remain in scope until the containing function ends. Combine stages when releasing a large value sooner matters.

### Properties of Composable Functions

A function becomes easier to reuse and rearrange when it is:
- **Pure:** its result depends only on its arguments, with no side effects.
- **Chainable:** an instance or extension receiver lets its result flow naturally into the next call.
- **General:** it performs one broadly useful operation rather than encoding one narrow use case.
- **Shape-preserving:** when possible, it returns the same outer structure it accepts.
- **Value-producing:** it returns data for the next function instead of ending in `void`; an `Action` is necessarily a terminal step.

These are design heuristics, not absolute requirements. Terminal operations must eventually collapse a shape or perform effects. The important distinction is that those operations deliberately end a pipeline.

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

Splitting one specific operation into smaller, general functions creates building blocks that can participate in other workflows while improving readability.

## Model Workflows with Functional Operators

A workflow is a meaningful sequence of operations leading to a result. Give each step a function, then use the operator that matches the step's type:

| Step shape                      | Operator                               | Meaning                                                     |
| ------------------------------- | -------------------------------------- | ----------------------------------------------------------- |
| `T -> R`                        | `Map`                                  | Transform a present value while preserving the result type. |
| `T -> bool`                     | `Filter` on `Option`, `guard` on `Fin` | Keep a value only when it passes a condition.               |
| `T -> Option<R>`, `T -> Fin<R>` | `Bind`                                 | Continue with a step that can produce no value or a reason. |
| `T -> void`                     | `Iter`                                 | Perform the terminal effect only for a present value.       |

The workflow functions do not have to compose directly. For example, validation returns `bool` while the debit requires the normalized request. The higher-order operators on `Option` and `Fin` provide the typed glue. Workflow functions are passed to `Map`, `Filter`, `Bind`, or `Iter`, and a `bool` step reaches `Fin` through `guard`. The result type controls whether the value reaches the next step.

`Fin<A>` is also a gate: `Succ` means a value remains eligible to continue, while `Fail` stops all later operations and keeps the reason. In a LINQ query over `Fin`, `guard` is the validation step and a `from` clause over `Debit` is the domain step.

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

This reads in domain order: normalize, validate, then debit. Adding another transformation means defining one function and inserting one pipeline step. Control flow remains inside `Fin`, so the top-level workflow does not accumulate nested conditionals.

## Keep Domain Transitions Pure

In this functional domain model, immutable state is separated from behavior. A debit rejected by the balance rule becomes the typed error `InsufficientFunds`. A successful debit returns new state without changing the original.

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

Unlike a mutating `void Debit` that throws for insufficient funds, this function:
- has no mutation or exception for an expected business outcome;
- exposes possible failure in its return type;
- produces a value that later steps can consume;
- leaves the original state unchanged.

## Compose the End-to-End Flow

Boundary services expose reads as `OptionT<IO, A>` and writes as `IO<Unit>` while the domain transition stays pure. The repository lifts its `Option` read into the transformer with `OptionT.lift`. `Require` is the boundary function that converts the transformer to `IO<AccountState>`. `Run` exposes one layer, and `IO.lift` folds the `Fin` from `ToFin` onto the error channel with the typed `AccountNotFound`.

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

`Get` can find no account, and `MakeTransfer` can reject the transfer, so `Book` binds them on one `IO` error channel instead of nesting result types. `Save` and `Wire` run only when both succeed, and each effect is one visible step of the query. The host runs `Book` with `RunSafe`, which returns `Fin<Unit>` and carries the typed error out of the effect.

## Expressions, Effects, and Declarative Code

Expressions produce values and therefore compose. Assignments, loops, and conditional statements direct execution and do not produce values for a pipeline. Declarations of classes, methods, and fields remain necessary and are best treated as a separate category. Favoring expressions shifts code from imperative instructions toward declarative descriptions.

This does not eliminate effects. It moves them to explicit boundaries:
1. Receive external input.
2. Transform and validate through expressions.
3. Compute new domain state with pure functions.
4. Perform required persistence or communication at the terminal edge.

If a terminal step requires multiple effects, keep each one visible.

## Layering Around a Top-Level Workflow

Do not require every layer to call only its immediate neighbor. That structure spreads impurity upward: once a low-level call performs I/O, every delegating layer becomes impure.

Instead, let a top-level entry point compose functions exposed by any lower-level component, while dependencies continue to point downward. This produces:
- a single, readable overview of the business workflow;
- optional subworkflows for meaningful groups of steps;
- pure mid-level validation and domain logic;
- effects isolated at repositories, external services, and other exit points;
- direct testing of pure logic without mocks.

## Limits and Failure Modes

- **`Option` discards the reason for failure, and `Fin` keeps it.** `Option` can short-circuit the flow, but it cannot distinguish a missing account from insufficient funds. `Fin` carries the typed `Error` without changing the compositional approach.
- **Composition does not make distributed effects atomic.** Saving a debited account and wiring funds can fail between operations. A database transaction cannot protect an external call from process failure after the call but before commit.
- **A thorough multi-system solution uses a durable task and idempotency.** Persist a representation of the combined work atomically, process it until all effects complete, and make repeat execution safe.
- **Declarative code is higher-level.** Confidence should come from tests rather than from looking at the implementation and assuming its hidden work is correct.
