namespace Lab.F15;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        LazinessProbe,
        FallbackProbe,
        CompositionProbe,
        TryProbe,
        ReaderProbe,
        ScopeProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> LazinessProbe() {
        Atom<int> leftRuns = Atom(0);
        Atom<int> rightRuns = Atom(0);
        IO<int> left = IO.lift(() => leftRuns.Swap(static n => n + 1));
        IO<int> right = IO.lift(() => rightRuns.Swap(static n => n + 1));
        int eager = Laziness.Pick(takeLeft: true, leftRuns.Swap(static n => n + 10), rightRuns.Swap(static n => n + 10));
        int afterEager = rightRuns.Value;
        Fin<int> deferred = Laziness.Pick(takeLeft: false, left, right).RunSafe();
        Atom<int> computed = Atom(0);
        int twice = Laziness.Twice(() => computed.Swap(static n => n + 5));
        return Check(
            nameof(LazinessProbe),
            ("Eager", eager == 10 && afterEager == 10),
            ("Deferred", deferred == Pure(11) && leftRuns.Value == 10),
            ("Twice", twice == 10 && computed.Value == 5));
    }

    private static Fin<Unit> FallbackProbe() {
        Cache cache = new(HashMap((1, "cached")));
        Atom<int> reads = Atom(0);
        Database database = new(reads);
        Option<string> eager = Laziness.Eager(cache, database, 1);
        int afterEager = reads.Value;
        Option<string> deferred = Laziness.Deferred(cache, database, 1);
        int afterDeferred = reads.Value;
        Option<string> missing = Laziness.Deferred(cache, database, 2);
        int afterMissing = reads.Value;
        string named = Laziness.Named(cache, 2);
        string loaded = Laziness.Loaded(cache, database, 1);
        return Check(
            nameof(FallbackProbe),
            ("Eager", eager == Some("cached") && afterEager == 1),
            ("Deferred", deferred == Some("cached") && afterDeferred == 1),
            ("Missing", missing.IsSome && afterMissing == 2),
            ("Named", string.Equals(named, "unknown", StringComparison.Ordinal)),
            ("Loaded", string.Equals(loaded, "cached", StringComparison.Ordinal) && reads.Value == 2));
    }

    private static Fin<Unit> CompositionProbe() {
        Atom<int> reads = Atom(0);
        IO<int> doubled = Composition.Doubled(reads);
        IO<int> summed = Composition.Summed(reads);
        int beforeRun = reads.Value;
        Fin<int> doubledResult = doubled.RunSafe();
        Fin<int> summedResult = summed.RunSafe();
        return Check(
            nameof(CompositionProbe),
            ("Assembled", beforeRun == 0),
            ("Doubled", doubledResult == Pure(2)),
            ("Summed", summedResult == Pure(5) && reads.Value == 3));
    }

    private static Fin<Unit> TryProbe() {
        Try<Uri> extracted = Parsing.ExtractUri("""{"Uri": "https://example.test/orders"}""");
        Fin<Uri> found = extracted.Run();
        Fin<Uri> malformed = Parsing.ExtractUri("{").Run();
        Fin<Uri> missing = Parsing.ExtractUri("""{"Url": "x"}""").Run();
        Fin<Uri> invalid = Parsing.ExtractUri("""{"Uri": "not a uri"}""").Run();
        return Check(
            nameof(TryProbe),
            ("Found", found.Exists(static uri => string.Equals(uri.Host, "example.test", StringComparison.Ordinal))),
            ("Malformed", malformed.Match(Succ: static _ => false, Fail: static e => e.IsExceptional)),
            ("Missing", missing.IsFail),
            ("Invalid", invalid.Match(Succ: static _ => false, Fail: static e => e.IsExceptional)));
    }

    private static Fin<Unit> ReaderProbe() {
        Settings settings = new("orders", 5);
        AppSettings app = new("shop", settings);
        Atom<int> queries = Atom(0);
        ReaderT<Settings, IO, int> queried = Environments.Queried(queries);
        int beforeRun = queries.Value;
        Fin<int> ran = queried.Run(settings).As().RunSafe();
        Fin<int> fromApp = Environments.QueriedFromApp(queries).Run(app).As().RunSafe();
        return Check(
            nameof(ReaderProbe),
            ("Target", string.Equals(Environments.Target.Run(settings), "orders", StringComparison.Ordinal)),
            ("Described", string.Equals(Environments.Described.Run(settings), "orders within 5s", StringComparison.Ordinal)),
            ("Patient", string.Equals(Environments.Patient.Run(settings), "orders within 10s", StringComparison.Ordinal)),
            ("FromApp", string.Equals(Environments.FromApp.Run(app), "orders within 5s", StringComparison.Ordinal)),
            ("Assembled", beforeRun == 0),
            ("Queried", ran == Pure(5)),
            ("QueriedFromApp", fromApp == Pure(10)));
    }

    private static Fin<Unit> ScopeProbe() {
        int released = Connection.Released;
        int committed = Transaction.Committed;
        int rolledBack = Transaction.RolledBack;
        Atom<TimeSpan> elapsed = Atom(TimeSpan.Zero);
        Fin<int> logs = Scopes.DeleteOldLogs(elapsed).RunSafe();
        int afterLogs = Connection.Released;
        Fin<int> order = Scopes.DeleteOrder.RunSafe();
        int afterOrder = Connection.Released;
        int afterOrderCommitted = Transaction.Committed;
        Fin<int> rejected = Scopes.DeleteRejected.RunSafe();
        return Check(
            nameof(ScopeProbe),
            ("Logs", logs == Pure(35) && afterLogs == released + 1 && elapsed.Value > TimeSpan.Zero),
            ("Order", order == Pure(70) && afterOrder == released + 2 && afterOrderCommitted == committed + 1),
            ("Rejected", rejected.IsFail && Connection.Released == released + 3 && Transaction.RolledBack == rolledBack + 1 && Transaction.Committed == committed + 1));
    }
}
