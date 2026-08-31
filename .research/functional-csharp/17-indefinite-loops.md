# Indefinite Loops

An indefinite loop advances state until a runtime condition is satisfied. Unlike a transformation over a fixed collection, its length is not known in advance. `Map` and `Fold` can consume a sequence whose producer knows when to stop, but they do not by themselves supply that stopping rule.

Model the problem with three values:

```text
S initialState;
Func<S, S> advance;
Func<S, bool> isFinished;
```

The separation is between:
- state transition: produce the next state from the current state;
- termination: decide whether a state is final;
- execution: repeatedly apply the transition until termination;
- consumption: choose whether to retain only the final state or every intermediate state.

## Choosing an approach

| Approach                  | Strength                                                     | Cost                                    | Appropriate when                       |
| ------------------------- | ------------------------------------------------------------ | --------------------------------------- | -------------------------------------- |
| Tail recursion            | Small, direct, expression-oriented                           | Unbounded calls can grow the stack      | The maximum depth is small and bounded |
| `Trampoline<A>`           | Pure recursion, any depth, `Bind` continues from final value | Every step is a deferred call           | The transition is pure                 |
| `Monad.recur`             | Effectful loop with constant stack usage                     | Only the final value is returned        | The transition is an effect            |
| `LanguageExt.List.unfold` | Lazy states compose with `Seq`                               | A second `unfold` reruns the transition | Intermediate states are meaningful     |

The library keeps mutable loop state inside the execution mechanism while leaving the transition and stopping rule explicit.

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

State is not mutated, and every iteration produces a replacement value. C# does not provide the tail-call optimization needed for constant stack usage. Each recursive call can add a stack frame. A condition that takes many iterations can degrade performance or terminate the process with a stack overflow.

Use direct recursion only when the iteration count has a small, known upper bound. An iteration count that is *usually* small does not remove the stack risk. `Trampoline<A>` removes it: `Trampoline.More` returns the recursive call as a deferred value, and `Run()` evaluates the deferred calls in a loop.

A recursive function needs:
1. an end condition that returns the final value
2. a recursive path that calls the same function with values closer to that condition
3. a returned value on every path

Given deltas that reach zero before they run out:

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

The function handles sequence exhaustion explicitly: `At` returns `None` when `nextIndex` is beyond the deltas, and the result is `None`. An unreachable base case loops indefinitely.

A reusable trampolined loop has this form:

```csharp
internal static class Trampolined {
    public static Trampoline<S> RunUntil<S>(S state, Func<S, bool> stop, Func<S, S> next) =>
        stop(state)
            ? Trampoline.Pure(state)
            : Trampoline.More(() => RunUntil(next(state), stop, next));
}
```

The stopping predicate is checked before each transition. The final state is returned instead of hidden in a mutable loop variable. Use this form when `next` returns a new state and the state carries all required information. `Bind` chains a second `Trampoline` onto the final state, and `Run()` still uses a constant stack. The abstraction has these limits:
- `next` must eventually produce a state satisfying `stop`.
- If `next` performs user interaction, I/O, or mutation, the expression remains impure. That transition belongs under `IO`.

## Effectful iteration with `Monad.recur`

`Monad.recur` exposes an expression-oriented interface while the library contains the mutation. The state function returns `Next.Loop` with the next state or `Next.Done` with the result:

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

The domain rules stay visible: the stopping predicate reads the state, and the transition is an `IO<int>` the caller supplies. `Monad.recur` returns `K<IO, Session>`, and `.As()` restores the `IO<Session>` at the host boundary. The host runs it with `RunSafe` and receives the final state as `Fin<Session>`.

The loop has constant stack usage and returns only the terminal state.

The state must contain everything required by both delegates. If termination depends on the last action or the latest random outcome, those values belong in the returned state rather than in unrelated mutable flags. This loop checks the initial state before advancing, so an already-finished initial value is returned unchanged.

A deep recursive `IO` remains stack-safe when `tail` wraps the recursive call as the last bind continuation after a deferred effect:

```csharp
internal static class Deep {
    public static IO<int> CountDown(IO<int> step, int remaining) =>
        remaining <= 0
            ? IO.pure(remaining)
            : step.Bind(move => tail(CountDown(step, remaining - move)));
}
```

A `tail`-recursive `IO` exits through `Run()` or `RunAsync()` only. `RunSafe()`, `Try()`, `Map`, and a later `Bind` add a mapping continuation after the tail call and fail. A host that needs a `Fin` captures with `Try.lift(io.Run).Run()`.

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

### Emitting the terminal state

`None` tells the consumer that the sequence has ended. `Step` must return `Some` for the transition that first produces the terminal state, then return `None` on the following call.

This sequence yields each state after a transition; it does not emit the initial state. It checks `HasExited` before it calls `advance`.

`Monad.recur` checks before advancing and performs zero or more transitions. `Step` emits only a state that a transition produced; an already-terminal initial state yields an empty `Seq`. If the initial state can already be terminal, decide whether enumeration is empty, emits that initial state, or advances once.

## LINQ consumption semantics

Constructing the sequence does not run the loop. Reading it does.

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

- `Last` reads until the sequence ends and returns the terminal state as an `Option`, or `None` for an empty sequence.
- `Map` defines a lazy transformation of every yielded state.
- `Fold` can retain both accumulated output and the latest state.
- `foreach` is a consumer, but mutating an outer variable reintroduces imperative state at the call site.

Short-circuiting operators change the termination behavior. `Head`, `Take`, and similar operations stop enumeration before the sequence's own end condition is reached. Use these operators only for intentional early termination, such as limiting a participant to a fixed number of actions.

The three methods in `Consumption` are alternatives, not operations to apply successively. One `Seq` keeps every state it has read. A second pass over the same `Seq` does not rerun `advance`. Each `unfold` call constructs a new producer and reruns the transition process. When `advance` reads input, randomness, or another effect, building the sequence twice performs the process twice and can produce different states. Build the `Seq` once and read every required result from it.
