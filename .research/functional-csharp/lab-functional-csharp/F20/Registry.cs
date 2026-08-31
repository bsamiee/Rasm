namespace Lab.F20;

internal sealed record UnknownAccount() : Expected("no account has this id", 1002);

internal static class Registry {
    public static OptionT<IO, AccountProcess> Lookup(
        AtomHashMap<Guid, AccountProcess> processes,
        Func<Guid, OptionT<IO, AccountState>> load,
        Func<Debited, IO<Unit>> persist,
        Guid id) =>
        processes.Find(id).Match(
            Some: OptionT.Some<IO, AccountProcess>,
            None: () =>
                from state in load(id)
                from started in AccountProcess.Start(persist, state)
                from resolved in IO.lift(() => processes.FindOrAdd(id, started))
                from _ in ReferenceEquals(resolved, started) ? IO.pure(unit) : started.Inbox.Complete()
                select resolved);

    public static IO<AccountProcess> Require(OptionT<IO, AccountProcess> lookup) =>
        lookup.Run().As().Bind(static found => IO.lift(found.ToFin(new UnknownAccount())));
}
