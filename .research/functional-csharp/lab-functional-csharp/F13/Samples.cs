namespace Lab.F13;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        SnapshotProbe,
        LensProbe,
        SequenceProbe,
        MapProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> SnapshotProbe() {
        Circle unit = new(new Point(0, 0), 1);
        Circle scaled = Shapes.Scale(unit, 2);
        AccountState active = AccountState.Requested(new CurrencyCode("EUR")).With(AccountStatus.Active, 50m);
        AccountState frozen = Transitions.Frozen(active);
        Transaction first = new("first", 10m);
        Transaction second = new("second", -4m);
        AccountState credited = active.Add(first).Add(second);
        List<Transaction> source = [first];
        Seq<Transaction> copied = toSeq(source);
        source.Add(second);
        return Check(
            nameof(SnapshotProbe),
            ("Scaled", scaled.Radius == 2 && unit.Radius == 1),
            ("Frozen", frozen.Status == AccountStatus.Frozen && frozen.AllowedOverdraft == 50m),
            ("ActiveKept", active.Status == AccountStatus.Active && active.Transactions.IsEmpty),
            ("Newest", credited.Transactions.Head == Some(second)),
            ("Order", credited.Transactions == Seq(second, first)),
            ("Copied", copied.Count == 1 && source.Count == 2));
    }

    private static Fin<Unit> LensProbe() {
        Customer ada = new("Ada", AccountState.Requested(new CurrencyCode("EUR")));
        Customer raised = Lenses.Raise(ada, 25m);
        Customer reset = Lenses.Reset(raised);
        Customer activated = Lenses.Account.Update(static account => account.With(AccountStatus.Active), ada);
        return Check(
            nameof(LensProbe),
            ("Raised", raised.Account.AllowedOverdraft == 25m && Lenses.CustomerOverdraft.Get(raised) == 25m),
            ("Unchanged", ada.Account.AllowedOverdraft == 0m),
            ("Reset", reset.Account.AllowedOverdraft == 0m),
            ("Activated", activated.Account.Status == AccountStatus.Active && activated.Name.Equals("Ada", StringComparison.Ordinal)));
    }

    private static Fin<Unit> SequenceProbe() {
        Transaction a = new("a", 10m);
        Transaction b = new("b", -4m);
        Transaction c = new("c", 1m);
        Seq<Transaction> history = Histories.Prepend(c, Histories.Prepend(b, Seq(a)));
        Seq<Transaction> built = Seq(a, b, c);
        Option<Transaction> newest = Histories.Newest(history);
        Seq<Transaction> older = Histories.Older(history);
        Option<Transaction> none = Histories.Newest(Seq<Transaction>());
        Seq<Transaction> emptyTail = Histories.Older(Seq<Transaction>());
        decimal balance = Histories.Balance(history);
        Lst<Transaction> ledger = List(a, b, c);
        Lst<Transaction> inserted = ledger.Insert(1, c);
        Lst<Transaction> removed = ledger.RemoveAt(0);
        Lst<Transaction> corrected = Histories.Correct(ledger, 0, c);
        Option<int> hashed = HashMap(("a", 1)).Find("a");
        return Check(
            nameof(SequenceProbe),
            ("History", history == Seq(c, b, a)),
            ("Built", built.Head == Some(a)),
            ("Newest", newest == Some(c)),
            ("Older", older == Seq(b, a)),
            ("None", none.IsNone),
            ("EmptyTail", emptyTail.IsEmpty),
            ("Balance", balance == 7m && Histories.Balance(Seq<Transaction>()) == 0m),
            ("Inserted", inserted == List(a, c, b, c)),
            ("Removed", removed == List(b, c)),
            ("Corrected", corrected == List(c, b, c)),
            ("Hashed", hashed == Some(1)));
    }

    private static Fin<Unit> MapProbe() {
        AccountState requested = AccountState.Requested(new CurrencyCode("EUR"));
        AccountState active = requested.With(AccountStatus.Active);
        Map<string, AccountState> accounts = Ledgers.Open(Map<string, AccountState>(), "acc-1", requested);
        Map<string, AccountState> replaced = Ledgers.Replace(accounts, "acc-1", active);
        Option<AccountState> current = Ledgers.Current(replaced, "acc-1");
        Option<AccountState> missing = Ledgers.Current(replaced, "acc-2");
        Fin<Map<string, AccountState>> duplicate = Try.lift(() => Ledgers.Open(accounts, "acc-1", active)).Run();
        Fin<Map<string, AccountState>> absent = Try.lift(() => Ledgers.Replace(accounts, "acc-2", active)).Run();
        return Check(
            nameof(MapProbe),
            ("Opened", accounts.Find("acc-1") == Some(requested) && accounts.Count == 1),
            ("Current", current == Some(active)),
            ("Missing", missing.IsNone),
            ("Duplicate", duplicate.IsFail),
            ("Absent", absent.IsFail));
    }
}
