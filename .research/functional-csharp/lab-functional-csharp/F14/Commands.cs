namespace Lab.F14;

internal static class Commands {
    public static IO<(Event Event, AccountState State)> Handle(Atom<Seq<Event>> store, MakeTransfer command) =>
        from _ in guard<Error>(command.Amount > 0m, new InvalidAmount())
        from history in EventStore.Load(store, command.AccountId)
        from account in IO.lift(Account.Rebuild(history).ToFin(new AccountNotFound()))
        from result in IO.lift(Account.Debit(account, command))
        from __ in EventStore.Save(store, result.Event)
        select result;
}
