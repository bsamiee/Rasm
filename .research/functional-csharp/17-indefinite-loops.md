# Indefinite Loops

An indefinite loop advances state until a runtime condition is satisfied. Unlike a fixed collection transformation, its length is not known in advance. `Map` and `Fold` can consume a sequence whose producer knows when to stop, but they do not by themselves supply that stopping rule.

Model the problem with three values:

```text
S initialState;
Func<S, S> advance;
Func<S, bool> isFinished;
```

The important separation is between:
- state transition: produce the next state from the current state;
- termination: decide whether a state is final;
- execution: repeatedly apply the transition until termination;
- consumption: choose whether to retain only the final state or every intermediate state.

The execution mechanism comes from the library. `Trampoline<A>` runs pure recursion, `Monad.recur` runs an effectful loop, and `LanguageExt.List.unfold` exposes successive states lazily. An effectful transition stays an effect and runs under `IO`.

## Choosing an approach

| Approach | Strength | Cost | Appropriate when |
| --- | --- | --- | --- |
| Tail recursion | Small, direct, expression-oriented | Unbounded calls can grow the stack in C# | The maximum depth is small and bounded |
| `Trampoline<A>` | Pure recursion at any depth, and `Bind` continues from the final value | Every step is a deferred call | The transition is pure |
| `Monad.recur` | Effectful loop with constant stack usage | Only the final value is returned | The transition is an effect |
| `LanguageExt.List.unfold` | Lazy states compose with `Seq` | A second `unfold` reruns the transition | Intermediate states are meaningful |

Direct recursion can keep the state progression functional, but it is unsafe for genuinely unbounded depth. The library forms confine imperative mechanics inside one mechanism while keeping the transition and stopping rule explicit.

## Tail recursion

A tail-recursive function either returns the final value or makes its recursive call as the last operation:

```csharp
internal static class Direct {
    public static S Run<S>(S state, Func<S, S> advance, Func<S, bool> isFinished) =>
        isFinished(state)
            ? state
            : Run(advance(state), advance, isFinished);
}
```

This is conceptually clean: state is not mutated, and every iteration produces a replacement value. That describes the recursive structure, not necessarily `advance`; captured input, randomness, or other effects remain effects. The operational risk is that C# does not provide the tail-call optimization needed to turn this pattern into a constant-stack loop, so each recursive call can add a stack frame. A condition that takes unexpectedly many iterations can degrade performance or terminate the process with a stack overflow.

Use direct recursion only when the iteration count has a small, defensible upper bound. An iteration count that is merely *usually* small does not remove the stack risk. `Trampoline<A>` removes it: `Trampoline.More` returns the recursive call as a deferred value, and `Run()` evaluates the deferred calls in a loop.

A recursive function needs:
1. an end condition that returns the final value
2. a recursive path that calls the same function with values closer to that condition
3. a returned value on every path

Each call receives a new set of argument values rather than mutating the previous call's values. A recursive iteration can model an early stop without mutable locals. For example, given deltas that reach zero before they run out:

```csharp
internal static class Positions {
    public static Trampoline<Option<int>> FirstPositionAtZero(Seq<int> deltas, int currentValue, int nextIndex) =>
        currentValue == 0
            ? Trampoline.Pure(Some(nextIndex - 1))
            : deltas.At(nextIndex).Match(
                Some: delta => Trampoline.More(() => FirstPositionAtZero(deltas, currentValue + delta, nextIndex + 1)),
                None: static () => Trampoline.Pure(Option<int>.None));
}
```

With deltas `2, -12, 9`, `FirstPositionAtZero(deltas, 10, 0).Run()` carries values `10`, `12`, then `0`, returning `Some(1)` without evaluating `9`.

The exhaustion case is total: `At` returns `None` when `nextIndex` is beyond the deltas, and the result is `None`. An unreachable base condition still loops indefinitely.

Each `Trampoline.More` returns the next call as a value instead of making it, so `Run()` evaluates the calls one after another and no frame waits on a deeper call.

The general form of the runner is a state transition until a stop condition:

```csharp
internal static class Trampolined {
    public static Trampoline<S> RunUntil<S>(S state, Func<S, bool> stop, Func<S, S> next) =>
        stop(state)
            ? Trampoline.Pure(state)
            : Trampoline.More(() => RunUntil(next(state), stop, next));
}
```

The stopping predicate is checked before each transition. The final state is returned instead of hidden in a mutable loop variable. This is functionally useful when `next` returns a new state and all required information is carried in that state. `Bind` chains a second `Trampoline` onto the final state, and `Run()` still uses a constant stack. The abstraction has strict limits:
- `next` must eventually produce a state satisfying `stop`.
- If `next` performs user interaction, I/O, or mutation, the expression has functional shape without becoming pure. That transition belongs under `IO`.

## A contained imperative core

`Monad.recur` exposes an expression-oriented interface while the library contains the necessary mutation. The state function returns `Next.Loop` with the next state or `Next.Done` with the result:

```csharp
internal sealed record Session(int Remaining, bool HasExited);

internal static class Sessions {
    public static IO<Session> Play(Session initial, IO<int> readMove) =>
        Monad.recur<IO, Session, Session>(initial, state =>
            state.HasExited
                ? IO.pure(Next.Done<Session, Session>(state))
                : readMove.Map(move => Next.Loop<Session, Session>(Apply(state, move)))).As();
    private static Session Apply(Session state, int move) {
        int remaining = state.Remaining - move;
        return new Session(remaining, remaining <= 0);
    }
}
```

The domain rules stay visible: the stopping predicate reads the state, and the transition is an `IO<int>` the caller supplies. `Monad.recur` returns `K<IO, Session>`, and `.As()` restores the `IO<Session>` at the concrete edge. The host runs it with `RunSafe` and receives the final state as `Fin<Session>`.

The loop has constant stack usage and returns only the terminal state. Callers still supply a state-to-state function and receive one final value. One library loop contains the imperative compromise instead of repeating it at every call site.

The state must contain everything required by both delegates. If termination depends on the last action or the latest random outcome, those values belong in the returned state rather than in unrelated mutable flags. This loop checks the initial state before advancing, so an already-finished initial value is returned unchanged.

A deep recursive `IO` keeps its shape when `tail` wraps the recursive call as the last bind continuation after a deferred effect:

```csharp
internal static class Deep {
    public static IO<int> CountDown(IO<int> step, int remaining) =>
        remaining <= 0
            ? IO.pure(remaining)
            : step.Bind(move => tail(CountDown(step, remaining - move)));
}
```

A `tail`-recursive `IO` exits through `Run()` or `RunAsync()` only. `RunSafe()`, `Try()`, `Map`, and a later `Bind` push a map into the tail and fail. A host that needs a `Fin` captures with `Try.lift(io.Run).Run()`.

Polling one effect until its value satisfies a predicate is `RepeatUntil`, and `RepeatWhile` is its complement. A `Schedule` sets the cadence between runs:

```csharp
internal static class Polling {
    public static IO<int> Drain(IO<int> step) =>
        step.RepeatUntil(Schedule.spaced(TimeSpan.FromMilliseconds(1)), static remaining => remaining <= 0);
}
```

## Custom iteration

`LanguageExt.List.unfold` is the lazy state sequence, and `Seq` is its materialized form. `unfold` takes an initial state and a `Step` that returns `Some((emitted, next))` or `None` at the terminal state. `toSeq` wraps the result as a `Seq` that reads each state on demand and keeps it.

```csharp
internal static class States {
    public static Seq<Session> Trace(Session initial, Func<Session, Session> advance) =>
        toSeq(LanguageExt.List.unfold(initial, state => Step(state, advance)));

    private static Option<(Session, Session)> Step(Session state, Func<Session, Session> advance) =>
        state.HasExited ? None : Emit(advance(state));
    private static Option<(Session, Session)> Emit(Session next) => Some((next, next));
}
```

### Emitting the terminal state correctly

`None` tells the consumer that the sequence has ended. Therefore, `Step` must return `Some` for the transition that first produces the terminal state, then return `None` on the following call.

This sequence yields each state after a transition; it does not emit the initial state. Checking `HasExited` before calling `advance` is essential. After the terminal value has been delivered, the consumer calls `Step` once more to discover the end. That call must return `None` without performing another transition.

Initial-state behavior is part of the contract. `Monad.recur` checks before advancing, so it performs zero or more transitions. `Step` emits only a state that a transition produced, so an initial state that is already terminal yields an empty `Seq`. If the initial state may already be terminal, decide whether enumeration should be empty, emit that initial state, or intentionally advance once.

## LINQ consumption semantics

The sequence is lazy. Constructing it does not run the loop. Reading it does.

```csharp
internal static class Consumption {
    public static Option<Session> Final(Seq<Session> states) => states.Last;
    public static Seq<string> Messages(Seq<Session> states) => states.Map(Describe);
    public static (Seq<string> Messages, Session State) Report(Seq<Session> states, Session initial) =>
        states.Fold(
            (Messages: Seq<string>(), State: initial),
            static (acc, state) => (acc.Messages.Add(Describe(state)), state));

    private static string Describe(Session state) =>
        string.Create(CultureInfo.InvariantCulture, $"remaining {state.Remaining}");
}
```

- `Last` reads the sequence to natural completion and returns the terminal state as an `Option`, `None` for an empty sequence.
- `Map` defines a lazy transformation of every yielded state.
- `Fold` can retain both accumulated output and the latest state.
- `foreach` is a simple consumer, but mutating an outer variable reintroduces imperative state at the call site.

Short-circuiting operators change the effective loop contract. `Head`, `Take`, and similar operations stop enumeration before the sequence's own end condition is reached. This is correct only when early termination is intended, such as limiting a participant to a fixed number of actions.

The three examples above are alternatives, not operations to apply successively. One `Seq` keeps every state it has read, so a second pass over the same `Seq` does not rerun `advance`. A second `unfold` reruns the transition process because `unfold` is a recipe, not a store. When `advance` reads input, randomness, or another effect, building the sequence twice performs the process twice and can produce different states. Build the `Seq` once and read every required result from it.

## Design constraints and failure modes

- Make the next-state function return a replacement state. Keep mutable cursor state inside the execution mechanism.
- Keep the stopping predicate independent of the loop machinery and express it in terms of the state.
- Put every value needed by the transition or stopping predicate into the state passed between steps; do not coordinate the two through unrelated mutable flags.
- Ensure that some reachable transition can satisfy the predicate. Otherwise every approach runs forever.
- Preserve the terminal state. A `Step` that returns `None` on the terminal transition silently discards the most important value.
- Do not use direct recursion merely because it appears purer; unbounded depth is a runtime risk in C#. `Trampoline<A>` keeps the recursive shape on a constant stack.
- Prefer `Monad.recur` when only the final result matters. `LanguageExt.List.unfold` earns its place when intermediate values or `Seq` composition are part of the requirement.
- Treat sequence consumption as execution. Deferred and repeated construction can repeat effects and cost.

The core functional model is stable across every choice: represent progress as successive values, make advancement and termination explicit functions, and isolate the unavoidable imperative mechanics.
