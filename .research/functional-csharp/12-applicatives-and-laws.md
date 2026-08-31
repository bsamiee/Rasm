# Chapter 8 - Working Effectively with Multi-Argument Functions

## Core idea

The core forms of `Map` and `Bind` accept unary functions, but real functions often need several arguments. Currying turns an n-argument function into a sequence of unary functions, allowing each argument to be supplied while the computation remains inside an effect such as `Option<T>` or `Validation<Error, T>`.

There are two useful composition models:
- **Applicative composition** uses `Pure` and `Apply` to combine elevated values that are computed independently; an effect-specific `Apply`, such as Validation's, can accumulate their failures.
- **Monadic composition** uses `Bind`, normally through LINQ query syntax, when a later computation depends on an earlier result.

This distinction controls behavior. Validation's applicative flow can combine all available failures; its monadic flow stops before later work is evaluated after a failure.

## Applying functions inside an effect

Given a curried function:

```csharp
Func<int, Func<int, int>> multiply = static x => y => x * y;
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
Option<int> product = multiplyBy3.Apply(Some(4));
Option<int> viaTuple = (Some(3), Some(4)).Apply(static (x, y) => x * y).As();
```

The result is `Some` only when both inputs are `Some`. Higher-arity overloads curry the wrapped function and delegate to the unary `Apply`, so one effect-specific rule serves every arity. The tuple `Apply` takes a tuple of independent values, arity two to ten, and one uncurried function. `As()` returns the concrete `Option<int>` from the `K<Option, int>` that the trait method returns.

## Lift first, then apply

Functions are values, so `Pure` can lift a function directly into an effect. Repeated `Apply` calls then supply its arguments:

```csharp
Func<int, int, int> multiply = static (x, y) => x * y;
Option<Func<int, int, int>> lifted = Pure(multiply);
Option<int> result = lifted.Apply(Some(3)).Apply(Some(4)).As();
// Some(12)
```

For a correctly implemented applicative, these formulations are equivalent, and `fun` gives an inline lambda its delegate type:

```csharp
Option<int> mapped = fun<int, int, int>(static (x, y) => x * y).Map(Some(3)).Apply(Some(4)).As();
Option<int> applied = lifted.Apply(Some(3)).Apply(Some(4)).As();
```

Lifting the function first mirrors ordinary partial application and is the more readable, intuitive form; the applicative equivalence guarantees the same result either way.

## Functor, applicative, and monad hierarchy

| Abstraction      | Required operations | Capability                                                         |
| ---------------- | ------------------- | ------------------------------------------------------------------ |
| `Functor<F>`     | `Map`               | Transform a value without leaving its effect                       |
| `Applicative<F>` | `Pure`, `Apply`     | Combine independent elevated values with a multi-argument function |
| `Monad<M>`       | `Pure`, `Bind`      | Sequence computations whose next step can depend on a prior value  |

The capabilities form a hierarchy: `Functor < Applicative < Monad < Fold`.

The stronger abstractions can define weaker operations: `Map(opt, f)` can be expressed as `Pure(f).Apply(opt)`, `Apply(optF, optT)` can bind the argument and then the elevated function before applying it, and `Fold` can define `Bind`. The traits `Functor<F>`, `Applicative<F>`, and `Monad<M>` capture these abstractions over `K<F, A>`, and each effect implements them. LINQ query syntax comes from `Monad<M>`.

Even when `Apply` can be derived from `Bind`, a dedicated implementation matters. It may be more efficient and can preserve useful semantics, such as accumulating independent validation errors, that a short-circuiting `Bind` cannot provide.

## Laws preserve refactoring safety

### Functor laws

When a value is inside a structure such as `Option<T>`, `Map` must preserve ordinary composition:

```csharp
Option<int> option = Some(2);
Func<int, int> f = static x => x + 1;
Func<int, int> g = static x => x * 2;
Option<int> sequential = option.Map(g).Map(f);
```

must produce the same result as:

```csharp
Option<int> composed = option.Map(x => f(g(x)));
```

Two laws make `Map` trustworthy:
- Mapping the identity function changes nothing: `value.Map(x => x) == value`.
- Mapping a composition is equivalent to mapping its parts in sequence.

An implementation of `Map` should transform only the inner value. Hidden mutation, counters, or other state changes tied to the number of `Map` calls break safe refactoring. `FunctorLaw<F>.validate` checks both laws for one value and returns `Validation<Error, Unit>`.

### Applicative equivalence

Mapping a function over the first elevated argument and then applying the rest must agree with lifting the function first and applying every argument:

```text
a.Map(f).Apply(b) == Pure(f).Apply(a).Apply(b)
```

The broader applicative laws ensure that identity, composition, and function application behave inside the effect as they do for ordinary values. `ApplicativeLaw<F>.validate` checks the functor laws, then identity, composition, homomorphism, and interchange.

### Monad laws

For monadic value `m`, plain value `t`, and world-crossing functions `f` and `g`:

```text
Right identity: m.Bind(Pure) == m
Left identity:  Pure(t).Bind(f) == f(t)
Associativity:  m.Bind(f).Bind(g)
             == m.Bind(x => f(x).Bind(g))
```

The identity laws require `Pure` and `Bind` to wrap and unwrap without adding state changes, conditional behavior, or distortion. `Pure` should perform only the minimum work required to place a value in the effect. `MonadLaw<F>.validate` checks the applicative laws, the two identity laws, associativity, and the equivalence of `Monad.recur` with `Bind`:

```csharp
Validation<Error, Unit> functor = FunctorLaw<Option>.validate(Some(1));
Validation<Error, Unit> applicative = ApplicativeLaw<Option>.validate();
Validation<Error, Unit> monad = MonadLaw<Fin>.validate();
```

Checking only successful contained values is not a complete law check; `None` and failure behavior must also remain equivalent.

Associativity explains how multi-argument functions enter a monadic pipeline: the right-associated form lets the innermost function close over values produced by earlier steps. Directly nested `Bind` calls expose this mechanism but become difficult to read, so use LINQ query syntax.

## LINQ syntax for arbitrary effects

C# translates LINQ query clauses into method calls by name and signature. A custom effect does not need to implement `IEnumerable<T>`.

### Functor query pattern

A single `from` followed by `select` requires `Select`, which `Option`, `Fin`, and `Validation` supply as an alias of `Map`.

### Monad query pattern

Multiple `from` clauses require `SelectMany` in the ternary projection shape used by the compiler, and every LanguageExt monad supplies it. The same query runs over `Option` and over `Validation<Error, A>`:

```csharp
internal sealed record NotANumber() : Expected("not a number", 1200);

internal static class Queries {
    public static Validation<Error, int> ValidInt(string text) =>
        parseInt(text).ToValidation<Error>(new NotANumber());
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

The compiler translates each query into the equivalent dependency-preserving `SelectMany` chain. The ternary overload carries both values into the final projection without deeply nesting lambdas.

Other clauses are opt-in:
- `let` is expressed with `Select`, so it works once mapping is available.
- `where` requires `Where`, which `Option` supplies beside `Filter`.
- Collection-specific clauses such as `orderby` need not exist for `Option`, `Either`, or `Validation`.

## Independent validation: applicative error accumulation

Suppose three raw fields can be validated independently. Replace permissive primitive fields with specific domain types: a smart enum for the closed set of number types and a value object for each formatted string. Each error record implements `IValidationError<T>`, so the generated `Validate` returns the typed `Expected`. Each validator maps that result to `Validation<Error, T>`, and the tuple `Apply` builds the aggregate from every result:

```csharp
internal sealed record InvalidNumberType() : Expected("number type is not mobile or home", 1201), IValidationError<InvalidNumberType> {
    public static InvalidNumberType Create(string message) => new();
}

internal sealed record InvalidCountryCode() : Expected("country code is not two upper-case letters", 1202), IValidationError<InvalidCountryCode> {
    public static InvalidCountryCode Create(string message) => new();
}

internal sealed record InvalidNumber() : Expected("number is not six to twelve digits", 1203), IValidationError<InvalidNumber> {
    public static InvalidNumber Create(string message) => new();
}

[SmartEnum<string>]
[ValidationError<InvalidNumberType>]
internal sealed partial class NumberType {
    public static readonly NumberType Mobile = new("mobile");
    public static readonly NumberType Home = new("home");
}

[ValueObject<string>]
[ValidationError<InvalidCountryCode>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal readonly partial struct CountryCode {
    static partial void ValidateFactoryArguments(ref InvalidCountryCode? validationError, ref string value) {
        if (value is not [char first, char second] || !char.IsAsciiLetterUpper(first) || !char.IsAsciiLetterUpper(second))
            validationError = new InvalidCountryCode();
    }
}

[ValueObject<string>]
[ValidationError<InvalidNumber>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal readonly partial struct Number {
    static partial void ValidateFactoryArguments(ref InvalidNumber? validationError, ref string value) {
        if (value.Length is < 6 or > 12 || !value.All(char.IsAsciiDigit))
            validationError = new InvalidNumber();
    }
}

internal sealed record PhoneNumber(NumberType Type, CountryCode Country, Number Number);

internal static class PhoneNumbers {
    public static Validation<Error, NumberType> ValidNumberType(string type) =>
        NumberType.Validate(type, provider: null, out NumberType? item) is { } error ? error : item!;
    public static Validation<Error, CountryCode> ValidCountryCode(string country) =>
        CountryCode.Validate(country, provider: null, out CountryCode item) is { } error ? error : item;
    public static Validation<Error, Number> ValidNumber(string number) =>
        Number.Validate(number, provider: null, out Number item) is { } error ? error : item;
    public static Validation<Error, PhoneNumber> CreatePhoneNumber(string type, string country, string number) =>
        (ValidNumberType(type), ValidCountryCode(country), ValidNumber(number))
            .Apply(static (t, c, n) => new PhoneNumber(t, c, n))
            .As();
}
```

`Apply` combines errors when both operands are invalid, so every failed field reports:

```csharp
Validation<Error, PhoneNumber> invalid = CreatePhoneNumber("fax", "gb", "abc");
int errorCount = invalid.Match(Fail: static e => e.Count, Succ: static _ => 0);
// 3
```

Each validation result is evaluated before it is passed to `Apply`, so failures from every operand are available for accumulation. `Error` accumulates with `+` into `ManyErrors`, and `Count`, `Head`, and `IsType<E>` read the accumulated errors. The boundary that admits the fields returns `Validation<Error, PhoneNumber>`, `ToFin` converts it at the exit, and the host matches the `Fin`.

## Dependent validation: monadic fail-fast flow

Use LINQ when each step may depend on an earlier validated value, or when later work should not run after failure. Each `from` can consume values introduced by earlier clauses while preserving the effect.

`Bind` receives a function for the next computation rather than an already evaluated result. If the current value is invalid, it can return that error without invoking the function. This is correct for dependency and fail-fast behavior, but it cannot collect failures from work that never ran.

## Combining validators: choose semantics first

A collection of validators can be folded into one validator:

```text
Seq<T -> Validation<Error, T>> -> T -> Validation<Error, T>
```

A validator is a `Func<T, Validation<Error, T>>`. There are two materially different compositions.

### Fail fast for efficiency

```csharp
internal static partial class Validators {
    public static Func<T, Validation<Error, T>> FailFast<T>(Seq<Func<T, Validation<Error, T>>> validators) =>
        value => validators.TraverseM(validate => validate(value)).As().Map(_ => value);
}
```

`TraverseM` is the monadic traversal, so it skips remaining validators after the first invalid result. An empty validator list returns the input as valid. Order cheap structural checks before expensive database or remote checks so invalid data fails before consuming costly resources.

Use this strategy when minimizing work matters more than reporting every issue, such as validation of a programmatic request.

### Harvest errors for independent checks

To report every violated rule, evaluate every validator independently and accumulate every error. The instance `Traverse` is the applicative traversal, and under `Validation` it accumulates every error:

```csharp
internal static partial class Validators {
    public static Func<T, Validation<Error, T>> HarvestErrors<T>(Seq<Func<T, Validation<Error, T>>> validators) =>
        value => validators.Traverse(validate => validate(value)).As().Map(_ => value);
}
```

On success, traversal holds one copy of the input for each validator. `Map` discards those copies and returns the original value. On failure, every validation error is retained.

Do not implement harvesting with monadic `Bind`: its short-circuit behavior prevents later checks from running. Error harvesting is appropriate for user-submitted forms where reporting every violated rule lets the user fix all errors before submitting again.

## Property-based testing

Property-based tests state invariants over generated inputs instead of enumerating a few examples. They are well suited to verifying algebraic laws and domain invariants.

```csharp
Func<int, int, int> multiply = static (x, y) => x * y;
Option<Func<int, int, int>> lifted = Pure(multiply);
Gen<Option<int>> option = Gen.OneOf(Gen.Int[-1000, 1000].Select(static x => Some(x)), Gen.Const(Option<int>.None));
Fin<Unit> equivalence = Try.lift(() => {
    option.Select(option).Sample((a, b) =>
        multiply.Map(a).Apply(b).As() == lifted.Apply(a).Apply(b).As());
    return unit;
}).Run();
```

`Gen.OneOf` builds an `Option` generator that produces both `Some` and `None`. A test that lifts only generated integers checks only the `Some` path and misses half the structure. The bounded range keeps the product inside `int`.

Random sampling raises confidence but does not prove a universal law. `Sample` throws on a counterexample, and `Try.lift` captures it into `Fin`. The case count and the ranges are configurable. A property tied to `multiply` checks that function, not every function. Properties can also capture model invariants, such as removing items from a cart never increasing its total.

## Selection guide and pitfalls

- Use `Map` for a pure unary transformation that preserves the current effect.
- Use the tuple `Apply` when inputs are independent and the effect has valuable combination semantics.
- Use LINQ over `Bind` when a computation consumes an earlier result or should short-circuit.
- Use `Traverse` to accumulate over a collection of independent checks and `TraverseM` to stop at the first failure.
- Avoid explicit unwrapping followed by rewrapping; it duplicates effect handling and leaks representation details.
- Avoid deeply nested `Bind` calls; LINQ preserves the same semantics with clearer scope and flow.
- When lifting an inline lambda, wrap it in `fun` so it has a delegate type, because a lambda can represent either a delegate or an expression tree.
- Do not derive every `Apply` mechanically from `Bind`; doing so can discard error accumulation or other effect-specific behavior.
- Do not use applicative composition for dependent work. Its arguments are computed independently, so it cannot express that one input requires another's successful value.
- Do not use monadic validation when the requirement is to report every independent input error; short-circuiting prevents later validations from contributing failures.
- Treat laws as design constraints that enable safe refactoring. `Optional` maps `null` to `None` at the null boundary, so `Some` stays genuine presence.
