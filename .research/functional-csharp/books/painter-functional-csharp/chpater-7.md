# Chapter 7 - Functional Flow

## Put absence and failure into the value

External calls create three recurring obligations before their output can enter business logic:

- catch exceptions;
- detect a missing value;
- reject a present but unusable value.

The chapter extends `Maybe<T>` to encode all three possible outcomes:

| Case | Meaning | Payload |
|---|---|---|
| `Something<T>` | A usable value is available | `T` |
| `Nothing<T>` | No usable value exists | None |
| `Error<T>` | The attempted operation threw | The captured exception |

This is a pragmatic combination. A classical Maybe contains only `Something` and `Nothing`; success and error normally belong to an Either or Result. Combining them lets an external-resource workflow carry value, absence, and failure through one pipeline. The producer reports the outcome, while the final consumer pattern-matches it into a message or other effect.

## Bind makes the happy path conditional

The first `Bind` takes a plain transformation:

```text
Maybe<T> + (T -> U) -> Maybe<U>
```

Its intended behavior is:

| Input | Result |
|---|---|
| `Something(value)` with a non-default value | Run the function and wrap its output in `Something` |
| `Something(default)` | Return `Nothing` without running the function |
| `Nothing` | Propagate `Nothing` |
| `Error(exception)` | Propagate the exception in `Error` |
| Function throws | Capture the exception in `Error` |

That gives a railway-like flow. Only `Something` continues through the transformations; `Nothing` and `Error` bypass all later callbacks and reach the boundary unchanged in meaning.

```csharp
Maybe<string> MakeGreeting(int employeeId) =>
    new Something<int>(employeeId)
        .Bind(id => employeeRepository.GetById(id))
        .Bind(employee =>
            $"Hello {employee.Salutation} {employee.Name}");
```

The greeting is constructed only if the repository call produces a usable employee and does not throw. The caller receives a structured outcome instead of a success string mixed with error messages or companion status flags.

### The printed success guard is reversed

The prose says the callback runs for a non-default value, but several listings use `Equals(value, default(T))` as the successful guard. Copied literally, that runs the callback only for the default value and converts non-default values to `Nothing`. The guard must express *not equal to default* to implement the behavior described by the chapter.

Default-based absence is itself a policy choice. It treats `null` as absent for reference types, but also treats `0` and `false` as absent. That is valid only when those values are unusable in the domain. If they are legitimate, presence cannot be inferred from `default(T)`.

The `try` block catches exceptions thrown while executing a bind callback. It does not make arbitrary code outside the pipeline exception-free.

## Keep a pipeline inspectable

A fluent chain is concise, but stepping through nested lambdas can obscure the first bad transition. Immutable intermediates preserve the same flow while exposing each result:

```csharp
var initial = id.ToMaybe();
var first = initial.Bind(transformationOne);
var second = first.Bind(transformationTwo);
var result = second.Bind(transformationThree);
```

Because none of the variables is reassigned, every stage remains available for inspection. The tradeoff is lifetime: large intermediates remain in scope until the containing function ends. Combine stages when releasing a large value sooner matters.

## Observe a case without stopping the flow

Case-specific observers perform an action and return the same `Maybe<T>` so composition can continue:

```text
OnSomething(Action<T>)
OnNothing(Action)
OnError(Action<Exception>)
```

`OnSomething` should pass the contained value to its action. The printed listing instead passes the whole Maybe despite declaring `Action<T>`; that snippet is not copy-ready.

Because `Nothing` and `Error` propagate, an observer attached after every later bind will fire repeatedly for the same event. The chapter's one-shot design adds `UnhandledNothing` and `UnhandledError`:

```text
Something -> UnhandledNothing -> OnNothing -> Nothing
Something -> UnhandledError   -> OnError   -> Error
```

The transition away from `Something` creates an unhandled case. The first matching observer performs its action and returns the ordinary handled case; later observers stay silent. Final pattern matching can still distinguish absence from error.

The illustrative listing needs mechanical repair before use: derived unhandled cases must be matched before their base cases, and the successful arm must apply the supplied function and construct `Something`.

## Nested Maybe requires flattening

The plain `Bind` wraps whatever its callback returns. If the callback already returns `Maybe<U>`, the result becomes `Maybe<Maybe<U>>`:

```text
Maybe<T> + (T -> Maybe<U>) -> Maybe<Maybe<U>>
```

That breaks the next step because it receives `Maybe<U>` where it expects `U`. Forcing out `.Value` would discard the `Nothing` and `Error` paths. Composition therefore needs a flattening bind with this behavior:

```text
Maybe<T> + (T -> Maybe<U>) -> Maybe<U>
```

It runs the callback only for an outer `Something`, preserves an inner `Something`, and propagates either layer's `Nothing` or `Error`. This is the same structural role that `SelectMany` plays when it converts nested sequences into one sequence.

The chapter expresses this idea through an overload whose receiver is `Maybe<Maybe<T>>`. Its printed implementation also repeats the reversed default guard and switches on the wrong variable in places, so the semantic requirement is more reliable than the listing.

## Async creates a second container

The shown asynchronous bind accepts a callback returning a plain task result:

```text
Maybe<T> + (T -> Task<U>) -> Task<Maybe<U>>
```

On `Something`, it awaits the callback and wraps the result. On `Nothing` or `Error`, it skips the callback and returns the corresponding `Maybe` inside a completed task. Exceptions observed while awaiting become `Error`. The same reversed default guard in the listing must be corrected.

Mixed sync/async flows are clearest one stage at a time: build a `Maybe`, call `BindAsync` at the async boundary, await the resulting `Task<Maybe<U>>`, then resume ordinary binds. Otherwise a `Task<T>` can accidentally become the payload carried through later stages.

An async callback that already returns `Task<Maybe<U>>` needs a different flattening shape:

```text
Maybe<T> + (T -> Task<Maybe<U>>) -> Task<Maybe<U>>
```

The first async overload does not provide that flattening: it infers `U` as `Maybe<U>` and produces `Task<Maybe<Maybe<U>>>`. The chapter's nested-async overload starts from `Maybe<Maybe<T>>`, which is not the same case. This distinction is essential in the route-planner example.

## The monad laws constrain refactoring

Let `Return(x)` construct a successful context, and let `f` and `g` return contexts.

### Left identity

```text
Return(x).Bind(f) == f(x)
```

Wrapping a value before binding must behave like calling the context-returning function directly. The chapter demonstrates the simpler map-like case with a plain function, where the contained output should equal the direct output.

### Right identity

```text
m.Bind(Return) == m
```

Binding the successful constructor must preserve the entire context. For the map-like overload, binding the plain identity function should preserve the case and value.

The default-as-absence rule creates an important exception: `Something(default(T)).Bind(identity)` becomes `Nothing`, so right identity cannot hold for every constructible `Something<T>`. Either `Something(default)` must be impossible to construct or default values must not be reinterpreted as absence.

### Associativity

```text
m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))
```

Regrouping the same operations must not change the final context. Flattening is what prevents regrouping from leaving `Maybe<Maybe<T>>`. The chapter's examples illustrate the intended intuition, but checking only the successful contained values is not a complete law check; `Nothing` and `Error` behavior must also remain equivalent.

## Reader defers a shared environment

`Reader<TEnvironment, TValue>` stores a function that cannot run until `Run(environment)` supplies its environment:

```text
Reader<E, A> = E -> A
Run: Reader<E, A> + E -> A
```

Each bind wraps another deferred transformation. The environment type remains fixed while the value type can change. A complete workflow can therefore be constructed before a database connection or other shared input exists, then run once that input is supplied.

This avoids carrying the environment through every step in tuples and moves acquisition of an external dependency outside workflow construction. Reader is function-level dependency injection: the environment can be a connection, identifier, configuration value, or any other input needed when the deferred computation runs. The Reader itself may also be returned so further transformations can be added before `Run`.

## State carries both state and current value

The chapter's practical C# `State<TState, TValue>` stores two values eagerly:

```text
State<S, A> = (CurrentState: S, CurrentValue: A)
Bind: State<S, A> + ((S, A) -> B) -> State<S, B>
Update: State<S, A> + (S -> S) -> State<S, A>
```

Each bind receives the fixed state type and current value, computes a new value, and carries the state forward. The value's type may change between binds; the state's type remains fixed, although its value can be replaced by `Update`.

Unlike the deferred State form described conceptually at the start of the section, this C# adaptation receives its initial state immediately and carries the pair eagerly. Its purpose is explicit state threading without repeated tuple plumbing or hidden mutation.

The section prints two conflicting `Update` bodies. The body consistent with the signature, explanation, and worked arithmetic is:

```csharp
new State<S, A>(update(state.CurrentState), state.CurrentValue)
```

The later body places the old state in the state slot and the transformed state in the value slot; it does not implement the described update.

### Put Maybe inside State when both concerns are needed

Replacing `CurrentValue` with `Maybe<TValue>` keeps the state available while value transformations short-circuit on `Nothing` or `Error`:

```text
StateMaybe<S, A> = (CurrentState: S, CurrentValue: Maybe<A>)
```

Its bind preserves `CurrentState` and delegates transformation of `CurrentValue` to `Maybe.Bind`. The final consumer still pattern-matches the inner Maybe into a concrete response. A carried value may also be a function, so State can thread behavior as well as data.

## Familiar C# parallels

- `IEnumerable<T>` uses `Select` for transformation and `SelectMany` to flatten nested sequences.
- `Task<T>` carries a value through asynchronous completion; `await` extracts it, and nested asynchronous calls require corresponding flattening.
- IO, Reader, State, tuples, `Aggregate`, and dependency injection can overlap in purpose; the useful choice is the smallest structure that makes the required flow explicit.

## Route-pricing flow: intended dependencies and listing defects

The worked example intends this dependency graph:

1. Resolve the origin address.
2. Only if that succeeds, resolve the destination and retain both addresses.
3. Determine the route asynchronously.
4. Retrieve traffic advice for the route.
5. Calculate the route's base price and increase it by ten percent when advice reports roadworks.
6. Return the resulting `Maybe<decimal>` for the boundary to interpret.

Tuples carry values needed by later operations without reaching outside the flow. Every dependent callback runs only after its inputs exist; an earlier `Nothing` or `Error` skips its dependents.

The printed code does not realize that graph as written:

- the address tuple stores the destination as `Maybe<Address>` instead of flattening it to `Address`;
- the route planner returns `Task<Maybe<Route>>`, but the shown async bind does not flatten that callback shape;
- the traffic result is reduced to a boolean outside the flow, so a missing/error advice is not preserved in pricing;
- the calculated `price` is never consumed;
- the final bind runs over `route`, then treats its `Route` payload as though it were the decimal price.

A coherent implementation needs flat bind for both the second address and traffic advice, flat async bind for the route planner, and a tuple carrying route plus advice into the pricing step. Base price is then calculated from the route and adjusted from the advice within the same successful path. Any `Nothing` or `Error` from address, route, or advice remains the final outcome instead of being converted into a misleading price.

## Implementation checklist

- Keep value, absence, and captured failure as distinct cases.
- Run callbacks only for non-default `Something` values under the chosen policy.
- Correct the reversed guards before using any printed bind listing.
- Distinguish map-like bind from context-flattening bind.
- Provide separate flattening for callbacks returning `Maybe<U>` and `Task<Maybe<U>>`.
- Capture callback exceptions while propagating existing `Nothing` and `Error` cases.
- Await the async container before resuming synchronous composition.
- Decide whether observers fire at every propagated state or only at the first transition.
- Use immutable intermediates when inspectability outweighs value lifetime.
- Supply a Reader's environment only when running the completed workflow.
- Update State by replacing its state while preserving its current value.
- Pattern-match the final context at the boundary.
