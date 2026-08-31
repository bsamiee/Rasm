namespace Lab.F09;

internal sealed class MemorySwift : ISwiftService {
    private readonly Atom<int> wired = Atom(0);

    public int Wired => wired.Value;

    public IO<Unit> Wire(MakeTransfer transfer, AccountState account) => IO.lift(() => ignore(wired.Swap(static n => n + 1)));
}

internal static class Samples {
    private static readonly Guid Ada = Guid.NewGuid();

    public static Fin<Unit> Run() =>
        CompositionSample()
            .Bind(static _ => DataFlowSample())
            .Bind(static _ => WorkflowSample())
            .Bind(static _ => BookingSample());

    private static Fin<Unit> CompositionSample() {
        Person ada = new("Ada", "Lovelace", 100m);
        Option<int> mapped = Some(2).Map(static x => x + 1).Map(static x => x * 2);
        Option<int> composed = Some(2).Map(static x => (x + 1) * 2);
        return Check(
            nameof(CompositionSample),
            ("Nested", string.Equals(Email.Nested(ada), "ALovelace@example.com", StringComparison.Ordinal)),
            ("EmailFor", string.Equals(Chaining.EmailFor(ada), "ALovelace@example.com", StringComparison.Ordinal)),
            ("Chained", string.Equals(Chaining.Chained(ada), "ALovelace@example.com", StringComparison.Ordinal)),
            ("Map composes", mapped == composed),
            ("Inspect", Stages.Inspect(3) == Some(7)),
            ("Inspect stops", Stages.Inspect(0).IsNone));
    }

    private static Fin<Unit> DataFlowSample() {
        Seq<Person> population = Seq(1m, 2m, 3m, 4m, 5m, 6m, 7m, 8m).Map(static e => new Person("P", "Q", e));
        return Check(
            nameof(DataFlowSample),
            ("Average", DataFlow.AverageEarningsOfRichestQuartile(population) == 7.5m),
            ("Quartile", population.RichestQuartile().Count == 2),
            ("Result", Quartiles.Result(population) == 7.5m));
    }

    private static Fin<Unit> WorkflowSample() {
        MakeTransfer transfer = new(Ada, " abcdefgh ", 40m);
        AccountState account = new(100m);
        Fin<AccountState> debited = Workflow.MakeTransfer(transfer, account);
        Fin<AccountState> rejected = Workflow.MakeTransfer(transfer with { Amount = 0m }, account);
        Fin<AccountState> insufficient = Workflow.MakeTransfer(transfer with { Amount = 500m }, account);
        Option<MakeTransfer> filtered = Some(Workflow.Normalize(transfer)).Filter(Workflow.IsValid);
        bool finObserved = false;
        bool optionObserved = false;
        _ = debited.Iter(_ => finObserved = true);
        _ = filtered.Iter(_ => optionObserved = true);
        return Check(
            nameof(WorkflowSample),
            ("Debited", debited == Pure(new AccountState(60m))),
            ("Rejected", rejected.Match(Succ: static _ => false, Fail: static e => e.IsType<InvalidTransfer>())),
            ("Insufficient", insufficient.Match(Succ: static _ => false, Fail: static e => e.IsType<InsufficientFunds>())),
            ("Filter", filtered.IsSome),
            ("Fin Iter", finObserved),
            ("Option Iter", optionObserved),
            ("Debit", account.Debit(30m) == Pure(new AccountState(70m))));
    }

    private static Fin<Unit> BookingSample() {
        MemoryAccounts accounts = new();
        MemorySwift swift = new();
        Transfers transfers = new(accounts, swift);
        MakeTransfer transfer = new(Ada, "abcdefgh", 40m);
        Fin<Unit> seeded = accounts.Save(Ada, new AccountState(100m)).RunSafe();
        Fin<Unit> booked = transfers.Book(transfer).RunSafe();
        Fin<Option<AccountState>> stored = accounts.Get(Ada).Run().As().RunSafe();
        Fin<Unit> missing = transfers.Book(transfer with { DebitedAccountId = Guid.NewGuid() }).RunSafe();
        Fin<Unit> broke = transfers.Book(transfer with { Amount = 500m }).RunSafe();
        return Check(
            nameof(BookingSample),
            ("Seeded", seeded.IsSucc),
            ("Booked", booked.IsSucc),
            ("Stored", stored.Exists(static o => o == Some(new AccountState(60m)))),
            ("Missing", missing.Match(Succ: static _ => false, Fail: static e => e.IsType<AccountNotFound>())),
            ("Broke", broke.Match(Succ: static _ => false, Fail: static e => e.IsType<InsufficientFunds>())),
            ("Wired once", swift.Wired == 1));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
