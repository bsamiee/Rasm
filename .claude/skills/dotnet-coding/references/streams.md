# [STREAMS]

Values that arrive over time and the process that owns a shared value, from a source's boundary reducer to the entity registry.

## [01]-[SOURCES]

Sources describe the dataflow and run no effect, one reducer at the boundary runs the effects and returns the `IO<S>` the host runs. Expected per-item failures stay `Fin<A>` values, so a rejected item leaves the stream running where a failing source ends every downstream branch. Logic across events reads the previous value from the reducer state, the source is subscribed once and holds no mutable state machine:

```csharp
internal static class Readings {
    public static IO<(Seq<Error> Fails, Seq<decimal> Succs)> Priced(Source<string> codes, Func<string, Fin<decimal>> price) =>
        codes.Map(price).Collect().Map(static outcomes => outcomes.Partition());
    public static IO<decimal> Watched(Source<decimal> balances, Func<decimal, IO<Unit>> overdrawn) =>
        balances.ReduceIO(0m, (previous, balance) =>
            when(previous >= 0m && balance < 0m, overdrawn(balance)).As().Map(Reduced.Continue(balance)));
}
```

## [02]-[AGENTS]

Rules that read current state (a balance limit) hold under concurrent commands only when one process serializes the commands for that state, since an event log replays debits validated against one snapshot into a balance past the limit. An agent is a `Conduit` reduced under `Fork`: its state is the reducer's accumulator and an immutable record, the pure transition sits outside the reducer, persistence runs inside it before the next state is adopted, and every command gets a reply on its own `Sink`, a failure included, so the caller composes an `IO<A>`. An application-wide `AtomHashMap<K, V>` maps each key to its one live inbox:

```csharp
internal sealed record Account(decimal Balance, decimal Limit);
internal sealed record Debit(decimal Amount, Sink<Fin<Account>> Reply);

internal static class Accounts {
    public static Fin<Account> Debited(Account account, decimal amount) =>
        Fault.Unless(account.Balance - amount >= -account.Limit, Reason.Range, (account.Balance, amount))
            .Map(_ => account with { Balance = account.Balance - amount });

    public static IO<Conduit<Debit, Debit>> Started(Account initial, Func<Account, IO<Unit>> persist) =>
        from inbox in IO.lift(static () => Conduit.make<Debit>())
        from _ in inbox.Reduce(initial, (account, debit) =>
                from outcome in IO.lift(Debited(account, debit.Amount)).Bind(next => persist(next).Map(next)).Try().Run().As()
                from __ in debit.Reply.Post(outcome)
                select Reduced.Continue(outcome.IfFail(account)))
            .Fork()
        select inbox;

    public static IO<Account> Charge(Conduit<Debit, Debit> inbox, decimal amount) =>
        from reply in IO.lift(static () => Conduit.make(Buffer<Fin<Account>>.Single))
        from _ in inbox.Post(new Debit(amount, reply.Sink))
        from answer in reply.Source.Take(1).Last()
        from next in IO.lift(answer)
        select next;

    public static IO<Conduit<Debit, Debit>> Resolved(
        AtomHashMap<Guid, Conduit<Debit, Debit>> live, Func<Guid, IO<Account>> load, Func<Account, IO<Unit>> persist, Guid id) =>
        IO.lift(() => live.Find(id)).Bind(found => found.Match(
            Some: static inbox => IO.pure(inbox),
            None: () =>
                from account in load(id)
                from started in Started(account, persist)
                from resolved in IO.lift(() => live.FindOrAdd(id, started))
                from _ in unless(ReferenceEquals(resolved, started), started.Complete()).As()
                select resolved));
}
```

Each agent owns the smallest independently mutable resource with an invariant, so commands for different keys run concurrently and commands for one key queue behind one owner. That order holds while every command for a key passes through this process, and ownership spanning processes belongs to an actor system. The registry is read inside the `IO` and its update function reruns under contention, so the load and the start run in the caller's `IO` before `FindOrAdd(id, started)`, and `load` raises a missing entity on its own error channel. `FindOrAdd` makes the registered agent unique, and the agent it did not return completes its inbox. `Complete()` ends an agent's loop, evicting a key removes its entry and completes its inbox in one `IO`, and an agent scoped to one block opens its inbox under `use(IO.lift(() => Conduit.make(buffer)), static inbox => inbox.Complete())`.
