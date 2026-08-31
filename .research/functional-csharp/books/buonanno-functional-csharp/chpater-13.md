# Working with Asynchronous Computations

## `Task<T>` as an effectful container

`Task<T>` represents a computation that will deliver a `T` asynchronously. Use it when latency is significant, especially for I/O. `await` suspends the current context while the operation completes, freeing its thread for other work; synchronously waiting blocks that thread.

From a functional perspective, `Task<T>` is a container that adds asynchrony and failure handling:

- `Option<T>` may contain a `T`.
- `Func<T>` can be run to obtain a `T`.
- `Task<T>` will produce a `T` at some future time, or fault.

Lazy and asynchronous computations differ at both ends. Creating a `Func<T>` does no work, and its consumer chooses when to invoke it. Calling a task-returning operation starts the work, and its consumer does not control when the result arrives. This is why fallback and retry operations accept `Func<Task<T>>`: each invocation must create a fresh task.

For an asynchronous operation with no meaningful result, adapt the non-generic `Task` to `Task<Unit>` so it remains composable.

## Lift, map, and bind

```csharp
public static Task<T> Async<T>(T value) =>
    Task.FromResult(value);

public static async Task<R> Map<T, R>(
    this Task<T> task, Func<T, R> transform) =>
    transform(await task);

public static async Task<R> Bind<T, R>(
    this Task<T> task, Func<T, Task<R>> next) =>
    await next(await task);
```

`Async` lifts an available value. `Map` composes `Task<T>` with `T -> R`. `Bind` composes it with `T -> Task<R>` and flattens the result. If the source faults, the transformation is skipped and the returned task carries the fault.

`await` makes the definitions direct: it extracts a task's eventual value, while an `async` method wraps its returned value in a task. The LINQ query pattern gives tasks the same composition: each dependent `from` is an await-and-bind step. Unlike `await`, LINQ syntax can be implemented for any monad.

Remain inside `Task` for the whole workflow. A host can consume `Task<IActionResult>` when it completes; extracting a value by blocking merely waits for the future to catch up.

## Failure policies: fallback, recovery, and retry

`Task<T>` already captures exceptions from an asynchronous computation. Wrapping it in `Exceptional<T>` normally duplicates the same failure effect. Expected domain invalidity is different and can justify an inner `Validation<T>`.

Keep three policies distinct:

- **Fallback** tries a lower-priority operation if the preferred task faults: `primary().OrElse(() => fallback())`. The fallback is lazy so it starts only after failure.
- **Recovery** maps a final fault to a normal value, usually at the workflow's end.
- **Retry** creates a fresh attempt after a transient failure and delay.

```csharp
public static Task<T> Recover<T>(
    this Task<T> task, Func<Exception, T> fallback) =>
    task.ContinueWith(t =>
        t.Status == TaskStatus.Faulted
            ? fallback(t.Exception)
            : t.Result);
```

A task cannot be matched synchronously because its final status is not yet available; `Recover` is effectively a map over the future faulted case. A two-branch `Map` can likewise transform both `Faulted` and `Completed` outcomes.

```csharp
public static Task<T> Retry<T>(
    int retries, int delayMillis, Func<Task<T>> start) =>
    retries == 0
        ? start()
        : start().OrElse(() =>
            from _ in Task.Delay(delayMillis)
            from value in Retry(retries - 1, delayMillis * 2, start)
            select value);
```

Each failure waits asynchronously, reduces the remaining retries, doubles the next delay, and invokes `start` again. When no retries remain, the final `start()` is returned and its failure remains observable. These combinators assume completion or fault; cancellation is outside their behavior.

## Data dependency determines sequencing or parallelism

`Bind` is sequential because `next` needs the first task's `T` before it can create the second task. Independent operations use applicative `Apply`, whose arguments are already-created tasks:

```csharp
public static async Task<R> Apply<T, R>(
    this Task<Func<T, R>> wrappedFunction, Task<T> argument) =>
    (await wrappedFunction)(await argument);

public static Task<Func<T2, R>> Apply<T1, T2, R>(
    this Task<Func<T1, T2, R>> wrappedFunction, Task<T1> argument) =>
    Apply(wrappedFunction.Map(F.Curry), argument);

Task<Flight> best = Async(PickCheaper)
    .Apply(firstAirline.BestFare(from, to, on))
    .Apply(secondAirline.BestFare(from, to, on));
```

The first `Apply` returns without waiting for its argument to finish, allowing the next task argument to be created immediately. Both calls therefore overlap, and completion time is governed by the slower call rather than their sum. Choose `Bind` or `Apply` from dependency, not as an incidental performance switch.

## Traverse: turn many effects into one effect

```text
Map      : Tr<T> -> (T -> A<R>) -> Tr<A<R>>
Traverse : Tr<T> -> (T -> A<R>) -> A<Tr<R>>
```

`Traverse` applies a world-crossing function and reverses the order of the traversable and effect. `A` must be at least applicative. For example, parsing with `string -> Option<double>` should produce `Option<IEnumerable<double>>`: any failed parse invalidates the complete input instead of silently removing it.

An `Aggregate` starts with an empty collection lifted into the target effect, then appends every transformed item inside that effect:

```csharp
public static Option<IEnumerable<R>> Traverse<T, R>(
    this IEnumerable<T> values, Func<T, Option<R>> parse) =>
    values.Aggregate(
        Some(Enumerable.Empty<R>()),
        (results, value) =>
            from items in results
            from item in parse(value)
            select items.Append(item));
```

## Monadic and applicative validation traversal

The same signature can encode different evaluation behavior:

```csharp
public static Validation<IEnumerable<R>> TraverseM<T, R>(
    this IEnumerable<T> values, Func<T, Validation<R>> validate) =>
    values.Aggregate(
        Valid(Enumerable.Empty<R>()),
        (results, value) =>
            from items in results
            from item in validate(value)
            select items.Append(item));

static Func<IEnumerable<T>, T, IEnumerable<T>> Append<T>() =>
    (items, item) => items.Append(item);

public static Validation<IEnumerable<R>> TraverseA<T, R>(
    this IEnumerable<T> values, Func<T, Validation<R>> validate) =>
    values.Aggregate(
        Valid(Enumerable.Empty<R>()),
        (results, value) => Valid(Append<R>())
            .Apply(results)
            .Apply(validate(value)));
```

`TraverseM` stops calling validators after the first invalid value. `TraverseA` calls the validator for every independent value and accumulates all errors; this is the usual default for validation.

The same operation combines many validators for one value:

```csharp
public static Validator<T> HarvestErrors<T>(
    params Validator<T>[] validators) =>
    value => validators
        .Traverse(validate => validate(value))
        .Map(_ => value);
```

On success, traversal temporarily contains one copy of the input per validator; `Map` discards those copies and returns the original value. On failure, every validation error is retained.

## Traversing tasks

For `IEnumerable<T>` and `T -> Task<R>`, applicative traversal starts independent tasks in parallel and yields one `Task<IEnumerable<R>>`. Monadic traversal starts them sequentially and stops creating later tasks after a fault. Parallel traversal is the usual default, but sequential traversal can avoid starting many unnecessary operations when early failure is likely.

```csharp
Task<IEnumerable<Flight>> search = airlines
    .Traverse(airline => airline
        .Flights(from, to, on)
        .Recover(_ => Enumerable.Empty<Flight>()))
    .Map(groups => groups.Flatten().OrderBy(flight => flight.Price));
```

Recovering each task before traversal gives best-effort aggregation: one provider contributes an empty sequence while successful results survive. Recovering only after traversal can replace only the faulted aggregate, because a single provider has already faulted the combined task.

## Traverse for a zero-or-one-value structure

`Option`, `Either`, `Validation`, and `Exceptional` can be treated as traversables with one value on success and none on failure. The failure branch preserves its existing error without calling the world-crossing function; the success branch applies that function and maps the original success constructor over it.

```csharp
public static Exceptional<Validation<R>> Traverse<T, R>(
    this Validation<T> validation,
    Func<T, Exceptional<R>> transform) =>
    validation.Match(
        Invalid: errors => Exceptional(Invalid<R>(errors)),
        Valid: value => transform(value).Map(Valid));
```

Using the identity function swaps already-stacked effects, such as `Validation<Exceptional<T>>` into `Exceptional<Validation<T>>`.

## Normalize and compose stacked effects

Stacks such as `Task<Option<T>>`, `Task<Validation<T>>`, and `Validation<Task<T>>` are hard to compose because each `Bind` understands only its own outer effect. Reduce unnecessary effects before building the workflow.

```csharp
Func<Guid, Task<Validation<AccountState>>> GetAccount = id =>
    getAccount(id)
        .Map(option => option.ToValidation(
            () => Errors.UnknownAccountId(id)));

Func<Event, Task<Unit>> SaveAndPublish = async @event =>
{
    await saveAndPublish(@event);
    return Unit();
};
```

The first adapter interprets a missing account as domain invalidity, reducing `Task<Option<T>>` to `Task<Validation<T>>`. The second gives a non-generic task a composable value. The workflow now needs only the deliberate `Task<Validation<T>>` stack.

Implement `Bind` and the LINQ query pattern for that specific stack, then adapt every operation to it:

```text
Task<Validation<T>> -> use directly
Validation<T>       -> Async(validation)
Task<T>              -> task.Map(Valid)
Validation<Task<T>> -> validation.Traverse(identity)
```

```csharp
Task<Validation<AccountState>> outcome =
    from command in Async(validate(request))
    from account in GetAccount(command.DebitedAccountId)
    from debit in Async(Account.Debit(account, command))
    from _ in SaveAndPublish(debit.Event).Map(Valid)
    select debit.NewState;
```

At the host boundary, map the outer task fault to an unexpected-error response, an inner invalid validation to a client error, and a valid state to success. A query implementation is required for each chosen stack; excessive general overloads make resolution ambiguous. If one pair of effects dominates, encapsulate it in a dedicated type. Keep stacks shallow: `Task<Exceptional<T>>` can usually become `Task<T>` because task faults already represent technical failure.

## Operational choices

- Expose the asynchronous operation when latency requires it; a blocking twin encourages the wrong execution model.
- Keep fallback and retry lazy so work starts only when the policy selects it.
- Use monadic composition only where the next operation consumes the prior value.
- Decide whether collection failure is fail-fast, error-accumulating, all-or-nothing, or best-effort before choosing traversal.
- Normalize failure and absence at adapters, then interpret the final effect only at the application boundary.
