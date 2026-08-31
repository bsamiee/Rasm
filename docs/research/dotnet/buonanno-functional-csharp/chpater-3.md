# Chapter 3 - Designing Function Signatures and Types

## Type design and function design are one problem

A function signature is a contract. Its input types describe every value the function accepts, and its return type describes every normal outcome callers must handle. Precise types make the contract both more informative and harder to violate.

Arrow notation keeps signatures compact:

| Meaning                     | Arrow notation                        | C# delegate                               |
| --------------------------- | ------------------------------------- | ----------------------------------------- |
| Transform a value           | `int -> string`                       | `Func<int, string>`                       |
| Produce a value             | `() -> string`                        | `Func<string>`                            |
| Consume a value for effects | `int -> ()`                           | `Action<int>`                             |
| Perform an effect           | `() -> ()`                            | `Action`                                  |
| Combine two inputs          | `(int, int) -> int`                   | `Func<int, int, int>`                     |
| Accept a function           | `(string, (IDbConnection -> R)) -> R` | `Func<string, Func<IDbConnection, R>, R>` |

`()` means no input or no returned information. Grouped inputs are treated as a tuple.

### Read behavior from the signature's shape

Generic signatures strongly constrain plausible behavior:

```csharp
(IEnumerable<T>, (T -> bool)) -> IEnumerable<T>
(IEnumerable<A>, IEnumerable<B>, ((A, B) -> C)) -> IEnumerable<C>
```

The first suggests filtering a sequence with a predicate. The second suggests combining corresponding `A` and `B` values into `C` values. By contrast, `() -> ()` reveals almost nothing about the effect performed. A signature cannot express every semantic detail - `Where` and `TakeWhile` have the same shape - so precision in both types and naming matters.

Functional design usually keeps data and logic distinct: data objects carry inputs and outputs, while functions encode behavior. A constrained type can still own the validation needed to construct it and operations, such as comparison, that protect its hidden representation.

## Make invalid inputs unrepresentable

Primitive types are often broader than the domain. An `int` used as an age admits negative and implausibly large values. Validating inside every consumer duplicates the rule and mixes validation with the consumer's actual calculation.

A custom type can narrow the domain once. The initial version of `Age` validates construction; a refined version hides its representation and owns comparison. A smart constructor then makes expected construction failure explicit:

```csharp
public struct Age
{
    private int Value { get; }

    public static Option<Age> Of(int age)
        => IsValid(age) ? Some(new Age(age)) : None;

    private Age(int value)
    {
        if (!IsValid(value))
            throw new ArgumentException($"{value} is not a valid age");

        Value = value;
    }

    private static bool IsValid(int age)
        => 0 <= age && age < 120;

    public static bool operator <(Age left, Age right)
        => left.Value < right.Value;

    public static bool operator >(Age left, Age right)
        => left.Value > right.Value;

    public static bool operator <(Age left, int right)
        => left < new Age(right);

    public static bool operator >(Age left, int right)
        => left > new Age(right);
}

Risk CalculateRiskProfile(Age age)
    => age < 60 ? Risk.Low : Risk.Medium;
```

`Age.Of` has the shape `int -> Option<Age>`. Callers cannot bypass the invariant, and consumers neither repeat validation nor inspect the underlying integer. The private constructor retains a defensive check: if code inside the type supplies an invalid value, the exception reports a programming defect rather than an expected input outcome.

### Honest functions

An honest function always honors its signature. Given a value of the declared input type, it returns a value of the declared output type; it does not return `null` or throw an exception as an intrinsic outcome that the signature fails to describe.

```text
Age -> Risk
```

This is honest when every constructible `Age` produces a `Risk`. By contrast, `int -> Risk` is dishonest if some integers cause validation exceptions. Repair the contract either by narrowing the input to a validated type or by widening the output to represent the possibility of failure.

Honesty is weaker than purity. Honesty asks whether behavior agrees with the signature; purity additionally excludes observable side effects and dependence on mutable state.

### Types as sets

Thinking of types as sets of possible values clarifies design. If `Age` has 120 values and `Gender` has two, `(Age, Gender)` has `120 * 2 = 240` possible values. A tuple or object containing both is a product type: each field adds another dimension to the space of possible states.

`Option<T>` is a union: all `Some(T)` values plus the single `None` value, so a type with `n` values yields an option with `n + 1` values. Counting possible instances helps expose types that admit states the domain does not need. Once leaf values are constrained, they can be composed into larger data objects without reintroducing invalid primitive states.

## Represent no information with `Unit`

`void` is a language special case rather than an ordinary return type. This splits delegates into `Func` and `Action` families and can force duplicate higher-order-function implementations. The same split appears between `Task<T>` and `Task`.

`Unit` is an ordinary type with exactly one value and no information. C#'s empty `ValueTuple` can represent it:

```csharp
using Unit = System.ValueTuple;

static Unit Unit() => default(Unit);

static Func<Unit> ToFunc(this Action action)
    => () => { action(); return Unit(); };

static Func<T, Unit> ToFunc<T>(this Action<T> action)
    => value => { action(value); return Unit(); };
```

An `Action` overload can adapt and delegate to the one generic implementation:

```csharp
static void Time(string operation, Action action)
    => Time<Unit>(operation, action.ToFunc());

static T Time<T>(string operation, Func<T> body)
{
    var stopwatch = Stopwatch.StartNew();
    T result = body();
    stopwatch.Stop();
    Console.WriteLine($"{operation} took {stopwatch.ElapsedMilliseconds}ms");
    return result;
}
```

Use `void` when an ordinary imperative API performs effects and returns no information. Use `Unit` when an ordinary return value enables uniform functional handling. Returning `Unit` does not make an effectful function pure; it only removes a special-case return shape.

## Represent possible absence with `Option<T>`

Framework lookup APIs demonstrate why hidden absence is unsafe: a missing key may yield `null` from one collection and throw from another, even though both indexers appear to have the shape `key -> value`.

`Option<T>` makes absence part of the result type:

```text
Option<T> = None | Some(T)
```

- `None` contains no value.
- `Some(T)` contains one non-null value.
- `Match` requires behavior for both states and returns one common result type.

```csharp
string GreetingFor(Option<string> name)
    => name.Match(
        None: () => "Dear Subscriber,",
        Some: value => $"Dear {value.ToUpper()},");
```

Changing a required `string` property to `Option<string>` is deliberately breaking: code that treats the property as a `string` stops compiling until it handles absence. This trades latent `NullReferenceException`s for explicit compile-time work.

### The implementation shape in C#

C# cannot directly define a closed union whose only cases are a non-generic `None` and a generic `Some<T>`. `None` has no useful type parameter with which to implement `Option<T>`, and an interface or abstract class cannot prevent callers from adding other cases. The chapter therefore models the cases separately, converts both into `Option<T>`, and hides a discriminator plus inner value behind `Match`:

```csharp
public static partial class F
{
    public static Option.None None => Option.None.Default;
    public static Option.Some<T> Some<T>(T value)
        => new Option.Some<T>(value);
}

namespace Option
{
    public struct None
    {
        internal static readonly None Default = new None();
    }

    public struct Some<T>
    {
        internal T Value { get; }
        internal Some(T value)
        {
            if (value == null) throw new ArgumentNullException();
            Value = value;
        }
    }
}

public struct Option<T>
{
    readonly bool isSome;
    readonly T value;

    private Option(T value)
    {
        isSome = true;
        this.value = value;
    }

    public static implicit operator Option<T>(Option.None _)
        => new Option<T>();

    public static implicit operator Option<T>(Option.Some<T> some)
        => new Option<T>(some.Value);

    public static implicit operator Option<T>(T value)
        => value == null ? F.None : F.Some(value);

    public R Match<R>(Func<R> None, Func<T, R> Some)
        => isSome ? Some(value) : None();
}
```

The default inner value in the `None` state is ignored. The essential contract is an empty case, a present case that rejects `null`, and a way to handle both cases safely. The abstraction is also commonly called `Maybe`, with cases named `Nothing` and `Just`.

## Turn partial functions into total functions

A total function is defined for every value in its declared input domain; a partial function is not. Returning `Option` totalizes a partial computation: return `Some(result)` where the computation is defined and `None` otherwise.

```csharp
static Option<int> ParseInt(string text)
    => int.TryParse(text, out var value) ? Some(value) : None;

static Option<string> Lookup(
    this NameValueCollection collection, string key)
    => collection[key];

static Option<TValue> Lookup<TKey, TValue>(
    this IDictionary<TKey, TValue> dictionary, TKey key)
    => dictionary.TryGetValue(key, out var value)
        ? Some(value)
        : None;
```

`string -> int` hides the undefined parsing cases; `string -> Option<int>` describes every outcome. Both `Lookup` adapters expose the same honest absence model over framework APIs with inconsistent failure behavior.

## Design rules

- Design the signature early; make inputs and outcomes as specific as possible.
- Prefer constrained domain types to primitives plus repeated validation.
- Keep constructors private when public construction could violate invariants.
- Use smart constructors when primitive-to-domain conversion may legitimately fail.
- Do not explicitly return `null` from functions.
- Reject unexpected `null` at public API inputs; optional parameters are the exceptional case because defaults must be compile-time constants.
- Use `Option<T>` for optional properties, parsing, lookup, and other computations that may legitimately produce no value.
- Use `Match` when a concrete result must be selected from the `None` and `Some` cases.
- Use `Unit` to adapt `Action` into `Func`-based higher-order APIs without duplicating their behavior.
