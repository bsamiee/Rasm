namespace Lab.F14;

internal static class Samples {
    private static readonly Guid AccountId = new("00000000-0000-0000-0000-000000000014");

    private static readonly Seq<Event> History = [
        new Event.CreatedAccount(AccountId, "EUR"),
        new Event.DepositedCash(AccountId, 100m),
        new Event.DebitedTransfer(AccountId, 30m, "rent"),
    ];

    public static Fin<Unit> Run() =>
        RebuildSample()
            .Bind(static _ => DebitSample())
            .Bind(static _ => HandleSample())
            .Bind(static _ => QuerySample());

    private static Fin<Unit> RebuildSample() {
        Option<AccountState> rebuilt = Account.Rebuild(History);
        Option<AccountState> missing = Account.Rebuild(Seq<Event>());
        Option<AccountState> headless = Account.Rebuild(History.Tail);
        Option<AccountState> frozen = Account.Rebuild(History.Add(new Event.FrozeAccount(AccountId)));
        Option<AccountState> replayed = Account.Rebuild(History.Add(new Event.CreatedAccount(AccountId, "USD")));
        return Check(
            nameof(RebuildSample),
            ("rebuilt balance 70", rebuilt.Map(static s => s.Balance) == Some(70m)),
            ("rebuilt active", rebuilt.Map(static s => s.Status) == Some(AccountStatus.Active)),
            ("missing.IsNone", missing.IsNone),
            ("headless.IsNone", headless.IsNone),
            ("frozen status", frozen.Map(static s => s.Status) == Some(AccountStatus.Frozen)),
            ("replayed creation unchanged", replayed.Map(static s => s.Currency) == Some("EUR")));
    }

    private static Fin<Unit> DebitSample() {
        AccountState active = new(AccountStatus.Active, "EUR", 70m, 0m);
        Fin<(Event Event, AccountState State)> accepted = Account.Debit(active, new MakeTransfer(AccountId, 50m, "shop"));
        Fin<(Event Event, AccountState State)> overdrawn = Account.Debit(active, new MakeTransfer(AccountId, 500m, "shop"));
        Fin<(Event Event, AccountState State)> inactive = Account.Debit(active.WithStatus(AccountStatus.Frozen), new MakeTransfer(AccountId, 5m, "shop"));
        return Check(
            nameof(DebitSample),
            ("accepted balance 20", accepted.Exists(static r => r.State.Balance == 20m)),
            ("accepted event", accepted.Exists(static r => r.Event is Event.DebitedTransfer)),
            ("overdrawn code", overdrawn.Match(Succ: static _ => false, Fail: static e => e.HasCode(1402))),
            ("inactive type", inactive.Match(Succ: static _ => false, Fail: static e => e.IsType<AccountNotActive>())));
    }

    private static Fin<Unit> HandleSample() {
        Atom<Seq<Event>> store = Atom(History);
        Fin<(Event Event, AccountState State)> accepted = Commands.Handle(store, new MakeTransfer(AccountId, 20m, "shop")).RunSafe();
        int afterAccepted = store.Value.Count;
        Fin<(Event Event, AccountState State)> rejected = Commands.Handle(store, new MakeTransfer(AccountId, 500m, "shop")).RunSafe();
        int afterRejected = store.Value.Count;
        Fin<(Event Event, AccountState State)> malformed = Commands.Handle(store, new MakeTransfer(AccountId, -1m, "shop")).RunSafe();
        Fin<(Event Event, AccountState State)> unknown = Commands.Handle(store, new MakeTransfer(Guid.Empty, 1m, "shop")).RunSafe();
        return Check(
            nameof(HandleSample),
            ("accepted balance 50", accepted.Exists(static r => r.State.Balance == 50m)),
            ("afterAccepted == 4", afterAccepted == 4),
            ("rejected code", rejected.Match(Succ: static _ => false, Fail: static e => e.HasCode(1402))),
            ("afterRejected == 4", afterRejected == 4),
            ("malformed type", malformed.Match(Succ: static _ => false, Fail: static e => e.IsType<InvalidAmount>())),
            ("unknown type", unknown.Match(Succ: static _ => false, Fail: static e => e.IsType<AccountNotFound>())));
    }

    private static Fin<Unit> QuerySample() {
        Seq<StatementRow> rows = Queries.Rows(History);
        return Check(
            nameof(QuerySample),
            ("rows.Count == 2", rows.Count == 2),
            ("total 70", Queries.Total(rows) == 70m),
            ("first row deposit", rows.Head.Map(static r => r.Amount) == Some(100m)));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
