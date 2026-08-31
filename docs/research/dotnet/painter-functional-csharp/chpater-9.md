# Indefinite Loops

An indefinite loop advances state until a runtime condition is satisfied. Unlike a fixed collection transformation, its length is not known in advance. `Select` and `Aggregate` can consume a sequence whose producer knows when to stop, but they do not by themselves supply that stopping rule.

Model the problem with three values:

```csharp
T initialState;
Func<T, T> advance;
Func<T, bool> isFinished;
```

The important separation is between:
- state transition: produce the next state from the current state;
- termination: decide whether a state is final;
- execution: repeatedly apply the transition until termination;
- consumption: choose whether to retain only the final state or every intermediate state.

C# requires a compromise at the execution boundary. The choice is between direct recursion, an imperative loop hidden behind a functional interface, and a custom `IEnumerable<T>` that exposes successive states lazily. The latter two approaches isolate their mutable execution state; none of the approaches makes an effectful transition function pure.

## Choosing an approach

| Approach           | Strength                                    | Cost                                               | Appropriate when                                          |
| ------------------ | ------------------------------------------- | -------------------------------------------------- | --------------------------------------------------------- |
| Tail recursion     | Small, direct, expression-oriented          | Unbounded calls can grow the stack in C#           | The maximum depth is tightly bounded and remains small    |
| Loop-backed runner | Constant stack usage and little boilerplate | Contains a local mutable variable and `while` loop | Only the final state is required                          |
| Custom iterator    | Lazy states compose with LINQ               | More protocol code and internal mutation           | Intermediate states are meaningful or need transformation |

Direct recursion can keep the state progression functional, but it is unsafe for genuinely unbounded depth. The scalable alternatives confine imperative mechanics inside one mechanism while keeping the transition and stopping rule explicit. The runner is the practical trampolining compromise: a thunk is represented by a `Func` or `Action`, while a loop repeatedly invokes the supplied operation instead of recursive calls.

## Tail recursion

A tail-recursive function either returns the final value or makes its recursive call as the last operation:

```csharp
static State Run(State state, Func<State, State> advance,
    Func<State, bool> isFinished) =>
    isFinished(state)
        ? state
        : Run(advance(state), advance, isFinished);
```

This is conceptually clean: state is not mutated, and every iteration produces a replacement value. That describes the recursive structure, not necessarily `advance`; captured input, randomness, or other effects remain effects. The operational risk is that C# does not provide the tail-call optimization needed to turn this pattern into a constant-stack loop, so each recursive call can add a stack frame. A condition that takes unexpectedly many iterations can degrade performance or terminate the process with a stack overflow.

Use direct recursion only when the iteration count has a small, defensible upper bound. An iteration count that is merely usually small does not remove the stack risk.

## A contained imperative core

A generic helper can expose an expression-oriented interface while containing the necessary mutation:

```csharp
public static T IterateUntil<T>(
    this T initial,
    Func<T, T> advance,
    Func<T, bool> isFinished)
{
    var current = initial;

    while (!isFinished(current))
        current = advance(current);

    return current;
}
```

Usage keeps the domain rules visible at the call site:

```csharp
var finalState = initialState.IterateUntil(
    advance: state => ApplyNextAction(state),
    isFinished: state => state.HasExited);
```

The helper has constant stack usage and returns only the terminal state. Its local reassignment is an implementation detail; callers still supply a state-to-state function and receive one final value. One reusable loop contains the imperative compromise instead of repeating it at every call site.

The state must contain everything required by both delegates. If termination depends on the last action or the latest random outcome, those values belong in the returned state rather than in unrelated mutable flags. This helper checks the initial state before advancing, so an already-finished initial value is returned unchanged.

## Custom iteration

`IEnumerable<T>` is not an array. It is a recipe for obtaining an `IEnumerator<T>`, and the enumerator controls how values are produced. Its `MoveNext` implementation can skip, repeat, reverse, or generate values, so a custom implementation can yield an unknown number of successive states.

The protocol has four relevant responsibilities:
- `Current` holds the value a consumer reads after `MoveNext` succeeds.
- `MoveNext` performs the traversal step and reports whether `Current` should be consumed.
- `Reset` restores the starting position when supported.
- `Dispose` releases owned resources.

The wrapper creates a fresh enumerator for each enumeration. The enumerator owns the mutable cursor and stopping flag; consumers see only an `IEnumerable<T>`.

```csharp
public sealed class StateSequence<T> : IEnumerable<T>
{
    private readonly T _initial;
    private readonly Func<T, T> _advance;
    private readonly Func<T, bool> _isFinished;

    public StateSequence(
        T initial,
        Func<T, T> advance,
        Func<T, bool> isFinished)
    {
        _initial = initial;
        _advance = advance;
        _isFinished = isFinished;
    }

    public IEnumerator<T> GetEnumerator() =>
        new StateEnumerator<T>(_initial, _advance, _isFinished);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

### Emitting the terminal state correctly

`MoveNext` returning `false` tells the consumer not to read `Current`. Therefore, an enumerator must return `true` for the transition that first produces the terminal state, then return `false` on the following call.

```csharp
public sealed class StateEnumerator<T> : IEnumerator<T>
{
    private readonly T _initial;
    private readonly Func<T, T> _advance;
    private readonly Func<T, bool> _isFinished;
    private T _current;
    private bool _terminalWasYielded;

    public StateEnumerator(
        T initial,
        Func<T, T> advance,
        Func<T, bool> isFinished)
    {
        _initial = initial;
        _current = initial;
        _advance = advance;
        _isFinished = isFinished;
    }

    public T Current => _current;
    object IEnumerator.Current => Current!;

    public bool MoveNext()
    {
        if (_terminalWasYielded)
            return false;

        _current = _advance(_current);
        _terminalWasYielded = _isFinished(_current);
        return true;
    }

    public void Reset()
    {
        _current = _initial;
        _terminalWasYielded = false;
    }

    public void Dispose() { }
}
```

This sequence yields each state after a transition; it does not emit the initial state. Checking `_terminalWasYielded` before calling `_advance` is essential. After the terminal value has been delivered, the consumer calls `MoveNext` once more to discover the end; that call must return `false` without performing another transition.

Initial-state behavior is part of the contract. The loop-backed helper checks before advancing, so it performs zero or more transitions. The update-before-check iterator above performs one or more transitions. If the initial state may already be terminal, decide whether enumeration should be empty, emit that initial state, or intentionally advance once.

## LINQ consumption semantics

The iterator is lazy. Constructing it does not run the loop; enumeration does.

```csharp
var states = new StateSequence<State>(initial, advance, isFinished);

var final = states.Last(); // one possible consumption

var messages = states.Select(state => Describe(state)); // an alternative

var result = states.Aggregate( // another alternative
    (Messages: Enumerable.Empty<string>(), State: initial),
    (acc, state) => (
        acc.Messages.Append(Describe(state)),
        state));
```

- `Last` consumes the sequence to natural completion and returns the terminal state.
- `Select` defines a lazy transformation of every yielded state; materialization requires a terminal consumer such as `ToArray`.
- `Aggregate` can retain both accumulated output and the latest state.
- `foreach` is a simple consumer, but mutating an outer variable reintroduces imperative state at the call site.

Short-circuiting operators change the effective loop contract. `First`, `Take`, and similar operations stop enumeration before the iterator's own end condition may be reached. This is correct only when early termination is intended, such as limiting a participant to a fixed number of actions.

The three examples above are alternatives, not operations to apply successively. Multiple enumeration reruns the transition process because `GetEnumerator` creates a new enumerator. When `advance` reads input, randomness, or another effect, calling `Last` and then enumerating again for messages performs the process twice and may produce different states. Consume once and accumulate every required result in that pass.

## Design constraints and failure modes

- Make the next-state function return a replacement state. Keep mutable cursor state inside the execution mechanism.
- Keep the stopping predicate independent of the loop machinery and express it in terms of the state.
- Put every value needed by the transition or stopping predicate into the state passed between steps; do not coordinate the two through unrelated mutable flags.
- Ensure that some reachable transition can satisfy the predicate. Otherwise every approach runs forever.
- Preserve the terminal state. An off-by-one `MoveNext` implementation silently discards the most important value.
- Do not use recursion merely because it appears purer; unbounded depth is a runtime risk in C#.
- Prefer the simple loop helper when only the final result matters. A custom iterator earns its boilerplate when intermediate values or LINQ composition are part of the requirement.
- Treat iterator consumption as execution. Deferred and repeated enumeration can repeat effects and cost.

The core functional model is stable across all three choices: represent progress as successive values, make advancement and termination explicit functions, and isolate the unavoidable imperative mechanics.
