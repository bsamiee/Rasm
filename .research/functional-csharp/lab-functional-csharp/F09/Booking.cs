namespace Lab.F09;

internal sealed record AccountNotFound() : Expected("account not found", 903);

internal interface IRepository<T> {
    public OptionT<IO, T> Get(Guid id);

    public IO<Unit> Save(Guid id, T value);
}

internal interface ISwiftService {
    public IO<Unit> Wire(MakeTransfer transfer, AccountState account);
}

internal sealed class MemoryAccounts : IRepository<AccountState> {
    private readonly AtomHashMap<Guid, AccountState> store = AtomHashMap<Guid, AccountState>();

    public OptionT<IO, AccountState> Get(Guid id) => OptionT.lift<IO, AccountState>(IO.lift(() => store.Find(id)));

    public IO<Unit> Save(Guid id, AccountState value) => IO.lift(() => store.AddOrUpdate(id, value));
}

internal sealed class Transfers(IRepository<AccountState> accounts, ISwiftService swift) {
    public IO<Unit> Book(MakeTransfer transfer) =>
        from account in Require(accounts.Get(transfer.DebitedAccountId))
        from debited in IO.lift(Workflow.MakeTransfer(transfer, account))
        from _ in accounts.Save(transfer.DebitedAccountId, debited)
        from __ in swift.Wire(transfer, debited)
        select unit;

    private static IO<AccountState> Require(OptionT<IO, AccountState> lookup) =>
        lookup.Run().As().Bind(static option => IO.lift(option.ToFin(new AccountNotFound())));
}
