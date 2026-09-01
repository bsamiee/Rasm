# [ASYNC]

## [01]-[TASK]

`Task<T>` represents a computation that produces a `T` asynchronously. Use it for operations that must not block a thread while waiting, above all I/O. `await` suspends the current context and frees the thread; synchronous waiting blocks it. Compare the shapes:
- `Option<T>` can contain a `T`.
- `Func<T>` can be run to obtain a `T`.
- `Task<T>` will produce a `T` later, or fault.

Lazy and asynchronous computations differ in how work starts and how consumers receive results. Creating a `Func<T>` does no work, and its consumer chooses when to invoke it. Calling a task-returning operation starts the work, and its consumer does not control when the result arrives. `IO<A>` is the effect type for the workflow: `IO.liftAsync` holds the `Func<Task<A>>`, and each run calls it again. Fallback and retry operate on `IO<A>` and not on a started task.

Adapt an asynchronous operation that returns a non-generic `Task` to `IO<Unit>` to keep it composable.

## [02]-[LIFT_MAP_BIND]

```csharp
internal sealed record Flight(string Airline, decimal Price);
internal sealed record Airline(string Name, IO<Seq<Flight>> Flights);
internal sealed record ProviderDown() : Expected("provider down", 3001);

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

`IO.pure` lifts a value. `IO.liftAsync` adapts a task-returning operation, and its `EnvIO` overload passes `env.Token` for cancellation. `Map` composes `IO<A>` with `A -> B`. `Bind` composes it with `A -> IO<B>` and flattens the result. If the source fails, the transformation is skipped and the returned effect carries the error.

`await` extracts a task's value, and an `async` method wraps its returned value in a task. The LINQ query pattern gives effects the same composition: each dependent `from` is a bind step. Unlike `await`, LINQ syntax can be implemented for any monad, and LanguageExt supplies it through `Monad<M>`.

Remain inside `IO` throughout the workflow. The host runs the effect once at its boundary. Extracting a value earlier waits for the task to complete.

## [03]-[FAILURE_POLICIES]

`IO<A>` captures exceptions from an asynchronous computation as `Exceptional` errors on its error channel. An expected domain error is a typed `Expected` on the same channel, never a nested result type.

Keep three policies distinct:
- Fallback tries a lower-priority operation if the preferred effect fails: `primary | secondary`. The alternative starts only after failure, and `Catch` with an error code restricts it to one classified error.
- Recovery maps a final error to a normal value at the workflow's end: `IfFail` on the `Fin<A>` that `RunSafe()` returns.
- Retry creates a fresh attempt after a transient failure and delay: `Retry` with a `Schedule`.

```csharp
internal static class Policies {
    public static IO<Flight> Fallback(IO<Flight> primary, IO<Flight> secondary) =>
        primary | secondary;
    public static IO<Flight> FallbackOnOutage(IO<Flight> primary, IO<Flight> secondary) =>
        primary.Catch(3001, _ => secondary).As();
    public static IO<Flight> Retry(IO<Flight> attempt) =>
        attempt.Retry(Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3));
}
```

`Recover` is a host operation: `RunSafe()` returns the `Fin<A>`, and `IfFail` maps its failed case to the substitute.

Each failure waits for the next delay of the schedule and invokes the effect again. `Schedule.exponential` doubles the delay after each attempt, and `Schedule.recurs` caps the attempt count. When the schedule expires, the last error remains observable.

## [04]-[SEQUENCING_AND_PARALLELISM]

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

`Apply` starts both operands before it waits for either. Both calls overlap, and completion time is governed by the slower call rather than their sum. `Fork` starts one effect on its own thread and returns a `ForkIO<A>` whose `Await` yields the value. `awaitAll` starts every effect of a `Seq<IO<A>>` and collects the values in order. Each fork takes one dedicated thread. Chunk the collection before a large fan-out through `Fork`.

## [05]-[TRAVERSE]

```text
Map      : Tr<T> -> (T -> A<R>) -> Tr<A<R>>
Traverse : Tr<T> -> (T -> A<R>) -> A<Tr<R>>
```

`Traverse` applies an effect-returning function and reverses the order of the traversable and effect. `A` must be at least applicative. For example, parsing with `string -> Option<double>` produces `Option<Seq<double>>`: any failed parse invalidates the complete input instead of silently removing it.

The instance `Traverse` on `Seq<A>` returns `K<F, Seq<B>>`, and `.As()` recovers the concrete outer type:

```csharp
internal static partial class Traversals {
    public static Option<Seq<double>> ParseAll(Seq<string> values) =>
        values.Traverse(static s => parseDouble(s)).As();
}
```

## [06]-[VALIDATION_TRAVERSAL]

The same signature can encode different evaluation behavior:

```csharp
internal static partial class Traversals {
    public static Validation<Error, Seq<int>> ValidateAll(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.Traverse(validate).As();
    public static Validation<Error, Seq<int>> ValidateUntilFirstFailure(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.TraverseM(validate).As();
}
```

`TraverseM` stops calling validators after the first invalid value. `Traverse` calls the validator for every independent value and accumulates all errors; use it when validation must return all errors.

## [07]-[TASK_TRAVERSAL]

For `Seq<A>` and `A -> IO<B>`, applicative traversal yields one `IO<Seq<B>>`, and monadic traversal stops creating later effects after a failure. `Traverse` under `IO` overlaps the element effects; `TraverseM` runs them one after another.

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

## [08]-[OPTION_TRAVERSAL]

`Option`, `Either`, and `Validation` are traversables with one value on success and none on failure. The failure branch preserves its existing error without calling the effect-returning function; the success branch applies that function and maps the original success constructor over it.

```csharp
internal static class Layers {
    public static Option<Validation<Error, int>> Parse(Validation<Error, string> validated) =>
        validated.Traverse(static s => parseInt(s)).As();
    public static Option<Validation<Error, int>> Swap(Validation<Error, Option<int>> stacked) =>
        stacked.Traverse(static option => option).As();
}
```

Using the identity function swaps nested structures, such as `Validation<Error, Option<A>>` into `Option<Validation<Error, A>>`.

## [09]-[STACKED_EFFECTS]

An effect nested inside another cannot compose directly because each `Bind` understands only its own outer effect. `OptionT<IO, A>` is the transformer stack for a lookup that can return no value: `OptionT.lift` enters it from an `Option` or from a lifted `IO`, `OptionT.liftIO` lifts an `IO<A>` through it, and `Run()` removes one layer. Reduce unnecessary effects before building the workflow.

```csharp
internal sealed record AccountState(Guid Id, decimal Balance);
internal sealed record DebitCommand(Guid DebitedAccountId, decimal Amount);
internal sealed record Debited(AccountState NewState, string Event);
internal sealed record UnknownAccountId() : Expected("unknown account id", 3002);
internal sealed record InsufficientFunds() : Expected("insufficient funds", 3003);
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

The first adapter interprets a missing account as an expected domain error: `Run()` removes the `OptionT` layer, and `ToFin` with a typed `Expected` puts the absence on the `IO` error channel. The second keeps the lookup inside the stack while it consumes `IO<decimal>`. The workflow needs only `IO`.

Adapt every operation to `IO` at its boundary: a `Validation` from the validation boundary enters through `ToFin` and `IO.lift`, and a `Fin` from a pure transition enters through `IO.lift`:

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

At the host boundary, `RunSafe()` returns the `Fin<A>`: map an `Exceptional` error to an unexpected-error response, an `Expected` error to a client error, and a success to success. An `Eff<RT, A>` exits through `RunAsync(rt)`, which returns `Task<Fin<A>>`. If one transformer stack appears throughout the workflow, encapsulate it in a dedicated type.

## [10]-[OPERATIONAL_CHOICES]

- Expose asynchronous operations that wait on I/O; do not provide a synchronous counterpart that blocks.
- Decide whether collection failure is fail-fast (`TraverseM`), error-accumulating (`Traverse` under `Validation`), all-or-nothing (`Traverse` under `IO`), or best-effort (`PartitionFallible`) before choosing traversal.
