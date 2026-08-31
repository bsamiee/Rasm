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
Func<Person, string> emailFor =
    person => AppendDomain(AbbreviateName(person));
```

C# has no dedicated syntax for function composition. Defining a higher-order `Compose` function does not improve the right-to-left readability of nested composition, so C# usually expresses the same flow through instance or extension-method chaining:

```csharp
string email = person
    .AbbreviateName()
    .AppendDomain();
```

Each chained method must be defined on the preceding expression's type, either as an instance method or an extension method. The methods then appear in execution order, so a long pipeline can serve as a high-level description of the program.

## Composition Must Survive Elevated Values

When a value is inside a structure such as `Option<T>`, `Map` must preserve ordinary composition:

```csharp
option.Map(g).Map(f)
```

must produce the same result as:

```csharp
option.Map(x => f(g(x)))
```

Two laws make `Map` trustworthy:

- Mapping the identity function changes nothing: `value.Map(x => x) == value`.
- Mapping a composition is equivalent to mapping its parts in sequence.

An implementation of `Map` should transform only the inner value. Hidden mutation, counters, or other state changes tied to the number of `Map` calls break safe refactoring.

## Think in Data Flow

A compositional program is a sequence of typed transformations. Track what each step does to both the value and its shape:

```csharp
static decimal AverageEarningsOfRichestQuartile(List<Person> population)
    => population
        .OrderByDescending(p => p.Earnings) // reorder, preserve elements
        .Take(population.Count / 4)          // reduce cardinality
        .Select(p => p.Earnings)             // change element type
        .Average();                           // collapse sequence to scalar
```

The pipeline states what is wanted. Iteration, branching, and enumeration mechanics remain inside reusable operations. Here, `Average` is also the greedy step that evaluates the preceding lazy sequence.

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
public static IEnumerable<Person> RichestQuartile(
    this List<Person> population)
    => population
        .OrderByDescending(p => p.Earnings)
        .Take(population.Count / 4);

public static decimal AverageEarnings(
    this IEnumerable<Person> population)
    => population.Average(p => p.Earnings);

decimal result = population
    .RichestQuartile()
    .AverageEarnings();
```

Splitting one specific operation into smaller, general functions creates building blocks that can participate in other workflows while improving readability.

## Model Workflows with Functional Operators

A workflow is a meaningful sequence of operations leading to a result. Give each step a function, then use the operator that matches the step's type:

| Step shape | Operator | Meaning |
|---|---|---|
| `T -> R` | `Map` | Transform a present value while preserving optionality. |
| `T -> bool` | `Where` | Keep a value only when it passes a condition. |
| `T -> Option<R>` | `Bind` | Continue with a step that may produce no value. |
| `T -> void` | `ForEach` | Perform the terminal effect only for a present value. |

The workflow functions do not have to compose directly. For example, validation returns `bool` while booking requires the original request. The higher-order operators on `Option` provide the typed glue: workflow functions are passed to `Map`, `Where`, `Bind`, or `ForEach`, and the `Option` controls whether the value reaches the next step.

`Option<T>` can represent not only possible absence but also a gate: `Some` means a value remains eligible to continue, while `None` stops all later operations.

```csharp
public void MakeTransfer(MakeTransfer transfer)
    => Some(transfer)
        .Map(Normalize)
        .Where(validator.IsValid)
        .ForEach(Book);
```

This reads in domain order: normalize, validate, then book. Adding another transformation means defining one function and inserting one pipeline step. Control flow remains inside `Option`, so the top-level workflow does not accumulate nested conditionals.

## Keep Domain Transitions Pure

In this functional domain model, immutable state is separated from behavior. A debit rejected by the balance rule becomes `None`; a successful debit returns new state without changing the original.

```csharp
public class AccountState
{
    public decimal Balance { get; }

    public AccountState(decimal balance)
        => Balance = balance;
}

public static class Account
{
    public static Option<AccountState> Debit(
        this AccountState account,
        decimal amount)
        => account.Balance < amount
            ? None
            : Some(new AccountState(account.Balance - amount));
}
```

Unlike a mutating `void Debit` that throws for insufficient funds, this function:

- has no mutation or exception for an expected business outcome;
- exposes possible failure in its return type;
- produces a value that later steps can consume;
- leaves the original state unchanged.

## Compose the End-to-End Flow

Boundary services can expose optional reads and effectful writes while the domain transition stays pure:

```csharp
public interface IRepository<T>
{
    Option<T> Get(Guid id);
    void Save(Guid id, T value);
}

public interface ISwiftService
{
    void Wire(MakeTransfer transfer, AccountState account);
}

void Book(MakeTransfer transfer)
    => accounts
        .Get(transfer.DebitedAccountId)
        .Bind(account => account.Debit(transfer.Amount))
        .ForEach(account =>
        {
            accounts.Save(transfer.DebitedAccountId, account);
            swift.Wire(transfer, account);
        });
```

`Get` may find no account, and `Debit` may reject insufficient funds, so `Bind` connects them without producing `Option<Option<AccountState>>`. The side effects run only when both operations produce `Some` and are visibly confined to the terminal block.

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

- **`Option` discards the reason for failure.** It can short-circuit the flow, but it cannot distinguish a missing account from insufficient funds. `Either` or a related structure can carry error details without changing the compositional approach.
- **Composition does not make distributed effects atomic.** Saving a debited account and wiring funds can fail between operations. A database transaction cannot protect an external call from process failure after the call but before commit.
- **A thorough multi-system solution uses a durable task and idempotency.** Persist a representation of the combined work atomically, process it until all effects complete, and make repeat execution safe.
- **Declarative code is higher-level.** Confidence should come from tests rather than from looking at the implementation and assuming its hidden work is correct.
