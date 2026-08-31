# Work Smart, Not Hard with Functional Code

## The Core Move: Treat Functions as Data

A `Func` is a value that describes behavior. Once functions are stored in collections, passed into adapters, or returned from other functions, control flow can be expressed as data:

In `Func<T1, ..., TResult>`, every type except the last is a parameter type; the final type is the return type. Functions stored together therefore need compatible signatures.

- A collection of transformations applies several views to one input.
- A collection of predicates becomes a validation policy.
- An ordered collection of predicate-transform pairs becomes a decision table.
- A returned function can narrow access to one guarded operation; ordinary adapter functions can hide repetitive conversion branches.

This style removes repeated branching while leaving the variable part - the rule or transformation - visible.

## Collections of Transformations

Store functions with the same input and output types in an enumerable, then apply all of them to one value:

```csharp
IEnumerable<Func<Employee, string>> descriptors =
new Func<Employee, string>[]
{
    employee => $"First name: {employee.FirstName}",
    employee => $"Last name: {employee.LastName}",
    employee => $"Role: {employee.Role}"
};

string Describe(Employee employee) =>
    string.Join(Environment.NewLine,
        descriptors.Select(describe => describe(employee)));
```

The behavior set can be assembled dynamically, extended by adding one element, and kept separate from aggregation. Sequence-producing operations such as `Select` are deferred: the descriptor functions run when the result is enumerated. This can postpone unnecessary work, while repeated enumeration repeats the projection.

## Validation as a Predicate Set

A validation rule has the shape `T -> bool`. Collecting rules makes the policy explicit:

```csharp
public static bool IsValid<T>(
    this T value,
    params Func<T, bool>[] rules) =>
    rules.All(rule => rule(value));

public static bool IsInvalid<T>(
    this T value,
    params Func<T, bool>[] violations) =>
    violations.Any(rule => rule(value));
```

Use `All` when each predicate states what valid input must satisfy. Use `Any` when each predicate describes a violation. Both short-circuit:
- `All` stops at the first failed rule.
- `Any` stops at the first detected violation.
- An empty validity rule set returns `true`; an empty violation set returns `false`.

Short-circuiting is appropriate for a boolean answer. It is not suitable when every failure must be reported, because later rules may never run.

Keep each rule focused on one condition. The compact form works because the rules carry the variable logic while `All` or `Any` supplies the repeated branching and early return.

## Ordered Rule Tables

An `if`/`else if` ladder can be represented as ordered pairs:

```csharp
public static TOutput Match<TInput, TOutput>(
    this TInput value,
    Func<TInput, TOutput> fallback,
    params (Func<TInput, bool> When,
            Func<TInput, TOutput> Then)[] cases)
{
    var match = cases.FirstOrDefault(c => c.When(value));
    return match.When is null
        ? fallback(value)
        : match.Then(value);
}

decimal NetIncome(decimal income) => income.Match(
    x => x * 0.55m,
    (x => x <= 12_570m, x => x),
    (x => x <= 50_270m, x => x * 0.80m),
    (x => x <= 150_000m, x => x * 0.60m));
```

The first matching predicate wins, so ordering is part of the meaning. Each predicate can contain detailed criteria or delegate that decision to a named function. A fallback makes the operation defined for every input. A staged `.Match(...).DefaultMatch(...)` design must track whether a predicate matched; it cannot infer "no match" by comparing the transformed value with `default(TOutput)`, because a matching transformation may legitimately return `0`, `false`, or `null`. Passing the fallback directly, as above, avoids that ambiguity.

This is value-based predicate matching, not object-type matching. Its distinctive value is that the cases can be assembled as data; when the decision is fixed, a native switch expression is usually clearer. Tuple-free encodings such as `KeyValuePair` preserve the mechanism but add enough syntax to erase much of the benefit.

## Functions as Protective Filters

A returned function can sit between callers and an awkward API, capturing the original value in a closure while exposing only the safe operation.

### Dictionary Lookup with a Fallback

```csharp
public static Func<TKey, TValue> ToSafeDictionary<TKey, TValue>(
    this IDictionary<TKey, TValue> source,
    TValue fallback) =>
    key => source.ContainsKey(key) ? source[key] : fallback;

var actorByNumber = actors.ToDictionary(x => x.Number, x => x.Name)
                          .ToSafeDictionary("Unknown");

var name = actorByNumber(5);
```

The returned function keeps the dictionary in scope and converts an absent-key lookup into a fallback. The tradeoff is intentional interface narrowing: callers can no longer enumerate, query, or modify the value through the dictionary interface. A type default is often `null`, so an explicit domain fallback is usually clearer.

### Parse-or-Default Adapters

Move repeated parsing branches into focused conversion functions:

```csharp
public static int ToValueOrDefault(
    this object? value,
    int fallback = 0) =>
    int.TryParse(value?.ToString() ?? string.Empty, out var parsed)
        ? parsed
        : fallback;

public static string ToValueOrDefault(
    this object? value,
    string fallback = "")
{
    var text = value?.ToString();
    return string.IsNullOrWhiteSpace(text)
        ? fallback
        : text;
}
```

Call sites can then construct a settings value directly, making every default visible beside its setting. Each additional target type needs its own conversion overload. This technique deliberately collapses missing and invalid input into the same fallback; use it only when callers do not need to distinguish those cases.

## Custom Enumeration

`IEnumerable<T>` supplies an `IEnumerator<T>`. The iterator begins before the first element:
- `MoveNext()` advances and reports whether an element exists.
- `Current` reads the element at the present position.
- The enumerator must be disposed when traversal finishes.

Controlling the enumerator directly allows traversal policies that ordinary single-element predicates cannot express.

### Compare Adjacent Elements

An adjacent-pair operator follows this state transition:
1. Advance once and retain the first value as `previous`.
2. Advance again and evaluate `(previous, Current)`.
3. Stop early when the quantifier is decided.
4. Otherwise replace `previous` with `Current` and continue.

`AnyAdjacent` returns `true` when one pair matches; fewer than two elements therefore produce `false`. `AllAdjacent` follows the inverse stopping rule: it continues while pairs match, returns `false` at the first failure, and returns `true` when no pair disproves the condition. Fewer than two elements therefore produce `true`.

```csharp
public static bool AnyAdjacent<T>(
    this IEnumerable<T> source,
    Func<T, T, bool> matches)
{
    using var iterator = source.GetEnumerator();
    return iterator.MoveNext()
        && AnyAdjacent(iterator, matches, iterator.Current);
}

private static bool AnyAdjacent<T>(
    IEnumerator<T> iterator,
    Func<T, T, bool> matches,
    T previous) =>
    iterator.MoveNext()
        && (matches(previous, iterator.Current)
            || AnyAdjacent(iterator, matches, iterator.Current));
```

The first `MoveNext()` establishes `previous`; each recursive call advances once more before comparing. This is useful when a condition depends on neighboring values. For example, sort a number sequence, then test whether any adjacent pair differs by one.

### Iterate Until a State Condition

An indefinite loop can be expressed as a state transition:

```csharp
public static T AggregateUntil<T>(
    this T state,
    Func<T, bool> stop,
    Func<T, T> next) =>
    stop(state)
        ? state
        : next(state).AggregateUntil(stop, next);
```

The stopping predicate is checked before each transition. The final state is returned instead of hidden in a mutable loop variable. This is functionally useful when `next` returns a new state and all required information is carried in that state.

The abstraction has strict limits:
- `next` must eventually produce a state satisfying `stop`.
- Straight recursion consumes stack because C# does not guarantee tail-call optimization.
- Large, unbounded, or externally controlled iterations need a stack-safe implementation.
- If `next` performs user interaction, I/O, or mutation, the expression has functional shape without becoming pure.

## Choosing the Technique

- Use function collections when behaviors share a signature and vary as data.
- Use `All` or `Any` when only a short-circuiting boolean result is required.
- Use an ordered rule table for first-match decisions, always with an explicit fallback.
- Return a closure when intentionally narrowing an unsafe or noisy API to one operation.
- Control an enumerator when the condition depends on traversal state such as adjacent elements.
- Use recursive state transitions only when termination and stack depth are bounded.

Conciseness is valuable when it exposes intent. If an abstraction hides ordering, effects, missing-value semantics, or termination risk, the removed boilerplate has merely been displaced.
