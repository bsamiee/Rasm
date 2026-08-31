# Working with Asynchronous Computations

## `Task<T>` as an effectful container

`Task<T>` represents a computation that will deliver a `T` asynchronously. Use it when latency is significant, especially for I/O. `await` suspends the current context while the operation completes, freeing its thread for other work; synchronously waiting blocks that thread.

From a functional perspective, `Task<T>` is a container that adds asynchrony and failure handling:
- `Option<T>` may contain a `T`.
- `Func<T>` can be run to obtain a `T`.
- `Task<T>` will produce a `T` at some future time, or fault.

Lazy and asynchronous computations differ at both ends. Creating a `Func<T>` does no work, and its consumer chooses when to invoke it. Calling a task-returning operation starts the work, and its consumer does not control when the result arrives. `IO<A>` is the effect type for the workflow: `IO.liftAsync` holds the `Func<Task<A>>`, and each run calls it again. This is why fallback and retry operate on `IO<A>` and not on a started task.

For an asynchronous operation with no meaningful result, adapt the non-generic `Task` to `IO<Unit>` so it remains composable.

## Lift, map, and bind

```csharp
internal static class Codes {
    public const int ProviderDown = 3001;
    public const int UnknownAccount = 3002;
    public const int InsufficientFunds = 3003;
}

internal sealed record Flight(string Airline, decimal Price);
internal sealed record Airline(string Name, IO<Seq<Flight>> Flights);
internal sealed record ProviderDown() : Expected("provider down", Codes.ProviderDown);

internal static class Lifts {
    public static IO<Flight> Known(Flight flight) => IO.pure(flight);
    public static IO<Flight> Fetch(Func<Task<Flight>> request) => IO.liftAsync(request);
    public static IO<Flight> FetchWithToken(Func<CancellationToken, Task<Flight>> request) => IO.liftAsync(env => request(env.Token));
    public static IO<decimal> Price(IO<Flight> flight) => flight.Map(static f => f.Price);
    public static IO<int> Seats(IO<Flight> flight, Func<Flight, IO<int>> availability) => flight.Bind(availability);
    public static IO<decimal> Total(IO<Flight> outbound, IO<Flight> inbound) =>
        from o in outbound
        from i in inbound
        select o.Price + i.Price;
}
```

`IO.pure` lifts an available value. `IO.liftAsync` adapts a task-returning operation, and its `EnvIO` overload passes `env.Token` for cancellation. `Map` composes `IO<A>` with `A -> B`. `Bind` composes it with `A -> IO<B>` and flattens the result. If the source fails, the transformation is skipped and the returned effect carries the error.

`await` extracts a task's eventual value, and an `async` method wraps its returned value in a task. The LINQ query pattern gives effects the same composition: each dependent `from` is a bind step. Unlike `await`, LINQ syntax can be implemented for any monad, and LanguageExt supplies it through `Monad<M>`.

Remain inside `IO` for the whole workflow. The host runs the effect once at its boundary. Extracting a value earlier merely waits for the future to catch up.

## Failure policies: fallback, recovery, and retry

`IO<A>` captures exceptions from an asynchronous computation as `Exceptional` errors on its error channel. Expected domain invalidity is different: it is a typed `Expected` on the same channel, never a nested result type.

Keep three policies distinct:
- **Fallback** tries a lower-priority operation if the preferred effect fails: `primary | secondary`. The alternative starts only after failure, and `Catch` with an error code restricts it to one classified error.
- **Recovery** maps a final error to a normal value, usually at the workflow's end: `IfFail` on the `Fin<A>` that `RunSafe()` returns.
- **Retry** creates a fresh attempt after a transient failure and delay: `Retry` with a `Schedule`.

```csharp
internal static class Policies {
    public static IO<Flight> Fallback(IO<Flight> primary, IO<Flight> secondary) =>
        primary | secondary;
    public static IO<Flight> FallbackOnOutage(IO<Flight> primary, IO<Flight> secondary) =>
        primary.Catch(Codes.ProviderDown, _ => secondary).As();
    public static IO<Flight> Retry(IO<Flight> attempt) =>
        attempt.Retry(Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3));
}
```

`Recover` is a host operation: `RunSafe()` returns the `Fin<A>`, and `IfFail` maps its failed case to the substitute. Domain code keeps the `IO<A>` and never runs it.

Each failure waits for the next delay of the schedule and invokes the effect again. `Schedule.exponential` doubles the delay after each attempt, and `Schedule.recurs` caps the attempt count. When the schedule expires, the last error remains observable.

## Data dependency determines sequencing or parallelism

`Bind` is sequential because `next` needs the first effect's `A` before it can create the second effect. Independent operations use the tuple `Apply`, whose operands are already-created effects:

```csharp
internal static class Independence {
    public static IO<Flight> Best(IO<Flight> first, IO<Flight> second) => (first, second).Apply(PickCheaper).As();
    public static IO<Flight> BestForked(IO<Flight> first, IO<Flight> second) =>
        from f1 in first.Fork()
        from f2 in second.Fork()
        from a in f1.Await
        from b in f2.Await
        select PickCheaper(a, b);
    public static IO<Seq<Flight>> All(Seq<IO<Flight>> requests) => awaitAll(requests);

    private static Flight PickCheaper(Flight a, Flight b) => a.Price <= b.Price ? a : b;
}
```

`Apply` starts both operands before it waits for either. Both calls therefore overlap, and completion time is governed by the slower call rather than their sum. `Fork` starts one effect on its own thread and returns a `ForkIO<A>` whose `Await` yields the value. `awaitAll` starts every effect of a `Seq<IO<A>>` and collects the values in order. Each fork takes one dedicated thread, so a large fan-out through `Fork` is bounded by chunking the collection first. Choose `Bind` or `Apply` from dependency, not as an incidental performance switch.

## Traverse: turn many effects into one effect

```text
Map      : Tr<T> -> (T -> A<R>) -> Tr<A<R>>
Traverse : Tr<T> -> (T -> A<R>) -> A<Tr<R>>
```

`Traverse` applies a world-crossing function and reverses the order of the traversable and effect. `A` must be at least applicative. For example, parsing with `string -> Option<double>` produces `Option<Seq<double>>`: any failed parse invalidates the complete input instead of silently removing it.

The instance `Traverse` on `Seq<A>` returns `K<F, Seq<B>>`, and `.As()` recovers the concrete outer type:

```csharp
internal static partial class Traversals {
    public static Option<Seq<double>> ParseAll(Seq<string> values) =>
        values.Traverse(static s => parseDouble(s)).As();
}
```

## Monadic and applicative validation traversal

The same signature can encode different evaluation behavior:

```csharp
internal static partial class Traversals {
    public static Validation<Error, Seq<int>> ValidateAll(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.Traverse(validate).As();
    public static Validation<Error, Seq<int>> ValidateUntilFirstFailure(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.TraverseM(validate).As();
}
```

`TraverseM` stops calling validators after the first invalid value. `Traverse` calls the validator for every independent value and accumulates all errors; this is the usual default for validation.

`Traverse` over a `Seq` of validators for one value calls every validator and keeps every error, and `Map` discards the copies of the input to return the original value.

## Traversing tasks

For `Seq<A>` and `A -> IO<B>`, applicative traversal yields one `IO<Seq<B>>`, and monadic traversal stops creating later effects after a failure. `Traverse` under `IO` starts every element effect before it awaits any: effects built with `IO.liftAsync` overlap without a bound, and effects built with `IO.lift` run in order on the calling thread. `TraverseM` runs the effects one after another. `Fork` takes one thread per fork, so a large fan-out chunks the collection first. Sequential traversal can avoid starting many unnecessary operations when early failure is likely.

```csharp
internal static partial class Traversals {
    public static IO<Seq<Flight>> SearchParallel(Seq<Airline> airlines) =>
        airlines.Traverse(static airline => airline.Flights).As().Map(static groups => groups.Flatten());
    public static IO<Seq<Flight>> SearchSerial(Seq<Airline> airlines) =>
        airlines.TraverseM(static airline => airline.Flights).As().Map(static groups => groups.Flatten());
    public static IO<Seq<Flight>> SearchBestEffort(Seq<Airline> airlines) =>
        airlines.Map<K<IO, Seq<Flight>>>(static airline => airline.Flights).PartitionFallible().As().Map(static parts => parts.Succs.Flatten());
}
```

`PartitionFallible` runs every effect, does not short-circuit, and returns the `Fails` and the `Succs` inside one `IO`. `PartitionFallible` binds each effect to the previous one, so the effects run in order.

## Traverse for a zero-or-one-value structure

`Option`, `Either`, and `Validation` are traversables with one value on success and none on failure. The failure branch preserves its existing error without calling the world-crossing function; the success branch applies that function and maps the original success constructor over it.

```csharp
internal static class Layers {
    public static Option<Validation<Error, int>> Parse(Validation<Error, string> validated) =>
        validated.Traverse(static s => parseInt(s)).As();
    public static Option<Validation<Error, int>> Swap(Validation<Error, Option<int>> stacked) =>
        stacked.Traverse(static option => option).As();
}
```

Using the identity function swaps already-stacked structures, such as `Validation<Error, Option<A>>` into `Option<Validation<Error, A>>`.

## Normalize and compose stacked effects

A bare effect nested inside another is hard to compose because each `Bind` understands only its own outer effect. `OptionT<IO, A>` is the one stack for an effectful lookup: `OptionT.lift` enters it from an `Option` or from a lifted `IO`, `OptionT.liftIO` lifts an `IO<A>` through it, and `Run()` removes one layer. Reduce unnecessary effects before building the workflow.

```csharp
internal sealed record AccountState(Guid Id, decimal Balance);
internal sealed record DebitCommand(Guid DebitedAccountId, decimal Amount);
internal sealed record Debited(AccountState NewState, string Event);
internal sealed record UnknownAccountId() : Expected("unknown account id", Codes.UnknownAccount);
internal sealed record InsufficientFunds() : Expected("insufficient funds", Codes.InsufficientFunds);
internal sealed record Runtime;

internal static class Account {
    public static Fin<Debited> Debit(AccountState account, DebitCommand command) =>
        account.Balance >= command.Amount
            ? new Debited(account with { Balance = account.Balance - command.Amount }, "debited")
            : new InsufficientFunds();
}

internal static partial class Stacks {
    public static IO<AccountState> GetAccount(Func<Guid, OptionT<IO, AccountState>> lookup, Guid id) =>
        lookup(id).Run().As().Bind(static option => IO.lift(option.ToFin(new UnknownAccountId())));
    public static OptionT<IO, decimal> Converted(Func<Guid, OptionT<IO, AccountState>> lookup, Guid id, IO<decimal> rate) =>
        from account in lookup(id)
        from factor in OptionT.liftIO<IO, decimal>(rate)
        select account.Balance * factor;
    public static IO<Unit> SaveAndPublish(Func<Task> publish) =>
        IO.liftAsync(async () => {
            await publish().ConfigureAwait(false);
            return unit;
        });
}
```

The first adapter interprets a missing account as domain invalidity: `Run()` removes the `OptionT` layer, and `ToFin` with a typed `Expected` puts the absence on the `IO` error channel. The second keeps the lookup inside the stack while it consumes a plain `IO<decimal>`. The third gives a non-generic task a composable value. The workflow now needs only `IO`.

Adapt every operation to `IO` at its boundary: a `Validation` from the admitting boundary enters through `ToFin` and `IO.lift`, and a `Fin` from a pure transition enters through `IO.lift`:

```csharp
internal static partial class Stacks {
    public static IO<AccountState> Debit(
        Func<DebitCommand, Validation<Error, DebitCommand>> validate,
        Func<Guid, OptionT<IO, AccountState>> lookup,
        Func<Task> publish,
        DebitCommand request) =>
        from command in IO.lift(validate(request).ToFin())
        from account in GetAccount(lookup, command.DebitedAccountId)
        from debit in IO.lift(Account.Debit(account, command))
        from _ in SaveAndPublish(publish)
        select debit.NewState;
}

internal static class Host {
    public static int Exit(IO<AccountState> workflow) =>
        workflow.RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => error.IsExpected ? 4 : 1);
    public static Flight Recover(IO<Flight> flight, Flight substitute) =>
        flight.RunSafe().IfFail(substitute);
    public static Task<Fin<AccountState>> ExitAsync(Eff<Runtime, AccountState> workflow, Runtime runtime) =>
        workflow.RunAsync(runtime);
}
```

At the host boundary, `RunSafe()` returns the `Fin<A>`: map an `Exceptional` error to an unexpected-error response, an `Expected` error to a client error, and a success to success. An `Eff<RT, A>` exits through `RunAsync(rt)`, which returns `Task<Fin<A>>`. If one stack dominates, encapsulate it in a dedicated type. Keep stacks shallow: `IO` already carries technical failure, so no result type wraps it for that reason.

## Operational choices

- Expose the asynchronous operation when latency requires it; a blocking twin encourages the wrong execution model.
- Keep fallback and retry lazy so work starts only when the policy selects it.
- Use monadic composition only where the next operation consumes the prior value.
- Decide whether collection failure is fail-fast (`TraverseM`), error-accumulating (`Traverse` under `Validation`), all-or-nothing (`Traverse` under `IO`), or best-effort (`PartitionFallible`) before choosing traversal.
- Normalize failure and absence at adapters, then interpret the final effect only at the application boundary.
