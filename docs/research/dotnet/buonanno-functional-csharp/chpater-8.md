# Chapter 8 - Working Effectively with Multi-Argument Functions

## Core idea

The core forms of `Map` and `Bind` accept unary functions, but real functions often need several arguments. Currying turns an n-argument function into a sequence of unary functions, allowing each argument to be supplied while the computation remains inside an effect such as `Option<T>` or `Validation<T>`.

There are two useful composition models:
- Applicative composition uses `Return` and `Apply` to combine elevated values that are computed independently; an effect-specific `Apply`, such as Validation's, can accumulate their failures.
- Monadic composition uses `Bind`, normally through LINQ query syntax, when a later computation depends on an earlier result.

This distinction controls behavior. Validation's applicative flow can combine all available failures; its monadic flow stops before later work is evaluated after a failure.

## Applying functions inside an effect

Given a curried function:

```csharp
Func<int, Func<int, int>> multiply = x => y => x * y;

Option<Func<int, int>> multiplyBy3 = Some(3).Map(multiply);
```

`Map` supplies the first argument and leaves an elevated unary function. More generally:

```text
Map   : F<T> -> (T -> R) -> F<R>
```

When `R` is itself `T2 -> R2`, mapping a curried binary function has the effective signature `F<T1> -> (T1 -> T2 -> R2) -> F<T2 -> R2>`. C# overloads can accept `Func<T1, T2, R2>` and curry it internally, so callers need not spell the curried delegate.

`Apply` supplies an elevated argument to an elevated function:

```text
Apply : A<T -> R> -> A<T> -> A<R>
```

Its implementation owns the effect-specific rules for unwrapping the function and argument, applying the function, and wrapping the result.

### `Apply` for `Option`

```csharp
public static Option<R> Apply<T, R>(
    this Option<Func<T, R>> optF,
    Option<T> optT)
    => optF.Match(
        None: () => None,
        Some: f => optT.Match(
            None: () => None,
            Some: t => Some(f(t))));
```

The result is `Some` only when both inputs are `Some`. Higher-arity overloads curry the wrapped function and delegate to this unary implementation, so one effect-specific rule serves every arity.

## Lift first, then apply

Functions are values, so `Return` can lift a function directly into an effect. Repeated `Apply` calls then supply its arguments:

```csharp
Func<int, int, int> multiply = (x, y) => x * y;

Option<int> result = Some(multiply)
    .Apply(Some(3))
    .Apply(Some(4));
// Some(12)
```

For a correctly implemented applicative, these formulations are equivalent:

```csharp
someX.Map(multiply).Apply(someY);
Some(multiply).Apply(someX).Apply(someY);
```

Lifting the function first mirrors ordinary partial application and is the more readable, intuitive form; the applicative equivalence guarantees the same result either way.

## Functor, applicative, and monad hierarchy

| Abstraction | Required operations | Capability                                                         |
| ----------- | ------------------- | ------------------------------------------------------------------ |
| Functor     | `Map`               | Transform a value without leaving its effect                       |
| Applicative | `Return`, `Apply`   | Combine independent elevated values with a multi-argument function |
| Monad       | `Return`, `Bind`    | Sequence computations whose next step can depend on a prior value  |

The capabilities form a hierarchy:

```text
Functor < Applicative < Monad < Fold
```

The stronger abstractions can define weaker operations: `Map(opt, f)` can be expressed as `Return(f).Apply(opt)`, `Apply(optF, optT)` can bind the argument and then the elevated function before applying it, and `Fold` can define `Bind`. C# cannot capture these abstractions idiomatically in a common interface, so each effect supplies the required operations directly.

Even when `Apply` can be derived from `Bind`, a dedicated implementation matters. It may be more efficient and can preserve useful semantics, such as accumulating independent validation errors, that a short-circuiting `Bind` cannot provide.

## Laws preserve refactoring safety

### Applicative equivalence

Mapping a function over the first elevated argument and then applying the rest must agree with lifting the function first and applying every argument:

```text
a.Map(f).Apply(b) == Return(f).Apply(a).Apply(b)
```

The broader applicative laws ensure that identity, composition, and function application behave inside the effect as they do for ordinary values.

### Monad laws

For monadic value `m`, plain value `t`, and world-crossing functions `f` and `g`:

```text
Right identity: m.Bind(Return) == m
Left identity:  Return(t).Bind(f) == f(t)
Associativity:  m.Bind(f).Bind(g)
             == m.Bind(x => f(x).Bind(g))
```

The identity laws require `Return` and `Bind` to wrap and unwrap without adding state changes, conditional behavior, or distortion. `Return` should perform only the minimum work required to place a value in the effect.

Associativity explains how multi-argument functions enter a monadic pipeline: the right-associated form lets the innermost function close over values produced by earlier steps. Directly nested `Bind` calls expose this mechanism but become difficult to read, so use LINQ query syntax.

## LINQ syntax for arbitrary effects

C# translates LINQ query clauses into method calls by name and signature. A custom effect does not need to implement `IEnumerable<T>`.

### Functor query pattern

A single `from` followed by `select` requires `Select`, which can alias `Map`:

```csharp
public static Option<R> Select<T, R>(
    this Option<T> opt,
    Func<T, R> project)
    => opt.Map(project);
```

### Monad query pattern

Multiple `from` clauses require `SelectMany`. Provide both the ordinary `Bind` shape and the ternary projection shape used by the compiler:

```csharp
public static Option<R> SelectMany<T, R>(
    this Option<T> opt,
    Func<T, Option<R>> bind)
    => opt.Bind(bind);

public static Option<RR> SelectMany<T, R, RR>(
    this Option<T> opt,
    Func<T, Option<R>> bind,
    Func<T, R, RR> project)
    => opt.Match(
        None: () => None,
        Some: t => bind(t).Match(
            None: () => None,
            Some: r => Some(project(t, r))));
```

The compiler can then translate:

```csharp
var total =
    from a in Int.Parse(first)
    from b in Int.Parse(second)
    select a + b;
```

into the equivalent dependency-preserving `SelectMany` chain. The ternary overload carries both values into the final projection without deeply nesting lambdas. Queries with three or more `from` clauses also need the ordinary `SelectMany` overload.

Other clauses are opt-in:
- `let` is expressed with `Select`, so it works once mapping is available.
- `where` requires a suitable `Where` implementation for the effect.
- Collection-specific clauses such as `orderby` need not exist for `Option`, `Either`, or `Validation`.

## Independent validation: applicative error accumulation

Suppose three raw fields can be validated independently:

Replace permissive primitive fields with specific domain types, keep the aggregate constructor private, and expose a typed factory. Smart constructors validate each raw field before it can be supplied to that factory.

```text
validNumberType  : string -> Validation<NumberType>
validCountryCode : string -> Validation<CountryCode>
validNumber      : string -> Validation<Number>
```

Lift a factory and apply every validation result:

```csharp
Validation<PhoneNumber> CreatePhoneNumber(
    string type,
    string country,
    string number)
    => Valid(PhoneNumber.Create)
        .Apply(validNumberType(type))
        .Apply(validCountryCode(country))
        .Apply(validNumber(number));
```

`Validation.Apply` combines errors when both operands are invalid:

```csharp
public static Validation<R> Apply<T, R>(
    this Validation<Func<T, R>> valF,
    Validation<T> valT)
    => valF.Match(
        Valid: f => valT.Match(
            Valid: t => Valid(f(t)),
            Invalid: errors => Invalid(errors)),
        Invalid: errorsF => valT.Match(
            Valid: _ => Invalid(errorsF),
            Invalid: errorsT => Invalid(errorsF.Concat(errorsT))));
```

Each validation result is evaluated before it is passed to `Apply`, so failures from both operands are available for accumulation.

## Dependent validation: monadic fail-fast flow

Use LINQ when each step may depend on an earlier validated value, or when later work should not run after failure. Each `from` can consume values introduced by earlier clauses while preserving the effect.

`Bind` receives a function for the next computation rather than an already evaluated result. If the current value is invalid, it can return that error without invoking the function. This is correct for dependency and fail-fast behavior, but it cannot collect failures from work that never ran.

## Property-based testing

Property-based tests state invariants over generated inputs instead of enumerating a few examples. They are well suited to verifying algebraic laws and domain invariants.

```csharp
[Property(Arbitrary = new[] { typeof(ArbitraryOption) })]
void ApplicativeEquivalenceHolds(Option<int> a, Option<int> b)
    => Assert.Equal(
        a.Map(multiply).Apply(b),
        Some(multiply).Apply(a).Apply(b));
```

FsCheck needs a custom generator for `Option<T>` that produces both `Some` and `None`. A test that lifts only generated integers checks only the `Some` path and misses half the structure.

Random sampling raises confidence but does not prove a universal law. FsCheck generates 100 cases by default; its count and ranges are configurable. A property tied to `multiply` checks that function, not every function. Properties can also capture model invariants, such as removing items from a cart never increasing its total.

## Selection guide and pitfalls

- Use `Map` for a pure unary transformation that preserves the current effect.
- Use `Return` plus `Apply` when inputs are independent and the effect has valuable combination semantics.
- Use LINQ over `Bind` when a computation consumes an earlier result or should short-circuit.
- Avoid explicit unwrapping followed by rewrapping; it duplicates effect handling and leaks representation details.
- Avoid deeply nested `Bind` calls; LINQ preserves the same semantics with clearer scope and flow.
- When lifting an inline lambda, explicitly construct or cast it as `Func<...>` because a lambda can represent either a delegate or an expression tree.
- Do not derive every `Apply` mechanically from `Bind`; doing so can discard error accumulation or other effect-specific behavior.
- Do not use applicative composition for dependent work. Its arguments are computed independently, so it cannot express that one input requires another's successful value.
- Do not use monadic validation when the requirement is to report every independent input error; short-circuiting prevents later validations from contributing failures.
- Treat laws as design constraints that enable safe refactoring. A `Some` lifting function that rejects `null` sacrifices universal left identity to preserve `Some` as genuine presence.
